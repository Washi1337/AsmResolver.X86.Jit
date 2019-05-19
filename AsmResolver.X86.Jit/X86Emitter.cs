using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.X86;

namespace AsmResolver.X86.Jit
{
    public partial class X86Emitter
    {
        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern UIntPtr GetProcAddress(UIntPtr hModule, string procName);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        static extern UIntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

        private readonly List<X86Instruction> _instructions = new List<X86Instruction>();
        private readonly Queue<X86Label> _queuedLabels = new Queue<X86Label>();

        public IntPtr CreateExecutableMemorySegment(out long size)
        {
            var protection = Kernel32.MemoryProtection.READWRITE;
            size = GetSize();

            var ptr = Kernel32.VirtualAlloc(IntPtr.Zero,
                new IntPtr(size),
                Kernel32.AllocationType.COMMIT,
                protection);

            if (ptr == IntPtr.Zero)
                throw new Win32Exception();

            var writer = new UnsafeMemoryWriter(ptr, size);
            var assembler = new X86Assembler(writer);
            WriteTo(assembler, writer.Position);

            if (!Kernel32.VirtualProtect(ptr, (uint) size, Kernel32.MemoryProtection.EXECUTE_READ, out protection))
                throw new Win32Exception();
            return ptr;
        }
        
        public X86Emitter Nop()
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Nop,
                OpCode = X86OpCodes.Nop,
            };
            return Append(instruction);
        }

        public X86Emitter Pushad()
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Pushad,
                OpCode = X86OpCodes.Pushad,
            };
            return Append(instruction);
        }

        public X86Emitter Popad()
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Popad,
                OpCode = X86OpCodes.Popad,
            };
            return Append(instruction);
        }

        public X86Emitter Push(uint address)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Push,
                OpCode = X86OpCodes.Push_Imm1632,
                Operand1 = new X86Operand(address),
            };
            return Append(instruction);
        }

        public X86Emitter Push(X86Register register)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Push,
                OpCode = X86OpCodes.SingleByteOpCodes[X86OpCodes.Push_Eax.Op1 + ((int)register & 7)],
                Operand1 = new X86Operand(register),
            };
            return Append(instruction);
        }

        public X86Emitter Push(X86OperandUsage usage, X86Register register, int offset)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Push,
                OpCode = X86OpCodes.FF,
                Operand1 = new X86Operand(usage, register, offset),
            };
            return Append(instruction);
        }

        public X86Emitter Pop(X86Register register)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Pop,
                OpCode = X86OpCodes.SingleByteOpCodes[X86OpCodes.Pop_Eax.Op1 + ((int)register & 7)],
                Operand1 = new X86Operand(register),
            };
            return Append(instruction);
        }

        public X86Emitter Mov(X86Register registerA, X86Register registerB)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Mov,
                OpCode = X86OpCodes.Mov_Reg1632_RegOrMem1632,
                Operand1 = new X86Operand(registerA),
                Operand2 = new X86Operand(registerB),
            };
            return Append(instruction);
        }

        public X86Emitter Mov(X86OperandUsage usageA, X86Register registerA, X86Register registerB)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Mov,
                OpCode = X86OpCodes.Mov_Reg1632_RegOrMem1632,
                Operand1 = new X86Operand(usageA, registerA),
                Operand2 = new X86Operand(registerB),
            };
            return Append(instruction);
        }

        public X86Emitter Mov(X86OperandUsage usageA, X86Register registerA, int offset, X86Register registerB)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Mov,
                OpCode = X86OpCodes.Mov_RegOrMem1632_Reg1632,
                Operand1 = new X86Operand(usageA, registerA, offset),
                Operand2 = new X86Operand(registerB),
            };
            return Append(instruction);
        }

        public X86Emitter Mov(X86OperandUsage usageA, X86Register registerA, X86Register offsetReg, int offsetScalar, int offset, X86Register registerB)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Mov,
                OpCode = X86OpCodes.Mov_RegOrMem1632_Reg1632,
                Operand1 = new X86Operand(usageA, registerA, new X86ScaledIndex(offsetReg, offsetScalar), offset, X86OffsetType.Short),
                Operand2 = new X86Operand(registerB),
            };
            return Append(instruction);
        }

        public X86Emitter Mov(X86OperandUsage usageA, X86Register registerA, int offset, uint value)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Mov,
                OpCode = X86OpCodes.Mov_RegOrMem1632_Imm1632,
                Operand1 = new X86Operand(usageA, registerA, offset),
                Operand2 = new X86Operand(value),
            };
            return Append(instruction);
        }

        public X86Emitter Mov(X86Register registerA, X86OperandUsage usageB, X86Register registerB)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Mov,
                OpCode = X86OpCodes.Mov_Reg1632_RegOrMem1632,
                Operand1 = new X86Operand(registerA),
                Operand2 = new X86Operand(usageB, registerB),
            };
            return Append(instruction);
        }

        public X86Emitter Mov(X86Register registerA, X86OperandUsage usageB, X86Register registerB, int offset)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Mov,
                OpCode = X86OpCodes.Mov_Reg1632_RegOrMem1632,
                Operand1 = new X86Operand(registerA),
                Operand2 = new X86Operand(usageB, registerB, offset),
            };
            return Append(instruction);
        }


        public X86Emitter Mov(X86Register registerA, X86OperandUsage usageB, X86Register registerB, X86Register offsetReg, int offsetScalar, int offset)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Mov,
                OpCode = X86OpCodes.Mov_Reg1632_RegOrMem1632,
                Operand1 = new X86Operand(registerA),
                Operand2 = new X86Operand(usageB, registerB, new X86ScaledIndex(offsetReg, offsetScalar), offset, X86OffsetType.Short),
            };
            return Append(instruction);
        }

        public X86Emitter Mov(X86Register register, uint value)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Mov,
                OpCode = register == X86Register.Eax
                    ? X86OpCodes.Mov_Eax_Imm1632
                    : X86OpCodes.Mov_RegOrMem1632_Imm1632,
                Operand1 = new X86Operand(register),
                Operand2 = new X86Operand(value),
            };

            return Append(instruction);
        }

        public X86Emitter Call(X86Label label)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Call,
                OpCode = X86OpCodes.Call_Rel1632,
                Operand1 = new X86Operand(label),
            };

            return Append(instruction);
        }

        public X86Emitter Call(string library, string method)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Call,
                OpCode = X86OpCodes.Call_Rel1632,
                Operand1 = new X86Operand(GetProcAddress(LoadLibrary(library), method).ToUInt32()),
            };

            return Append(instruction);
        }

        public X86Emitter Retn()
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Retn,
                OpCode = X86OpCodes.Retn,
            };
            return Append(instruction);
        }

        public X86Emitter Retn(ushort value)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Retn,
                OpCode = X86OpCodes.Retn_Imm16,
                Operand1 = new X86Operand(value),
            };
            return Append(instruction);
        }
        
        public X86Emitter MarkLabel(X86Label label)
        {
            _queuedLabels.Enqueue(label);
            return this;
        }

        public X86Emitter Append(X86Instruction instruction)
        {
            while (_queuedLabels.Count > 0)
                _queuedLabels.Dequeue().Instruction = instruction;

            _instructions.Add(instruction);
            return this;
        }

        private X86Emitter Append(X86OpCode opcode, X86Mnemonic mnemonic, object operand1, object operand2)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = mnemonic,
                OpCode = opcode,
                Operand1 = new X86Operand(operand1),
                Operand2 = new X86Operand(operand2),
            };
            return Append(instruction);
        }

        public long GetSize()
        {
            ComputeOffsets(0);
            var lastInstruction = _instructions[_instructions.Count - 1];
            return lastInstruction.Offset + lastInstruction.ComputeSize();
        }

        private void ComputeOffsets(long baseAddress)
        {
            long currentOffset = baseAddress;
            foreach (var instruction in _instructions)
            {
                instruction.Offset = currentOffset;
                currentOffset += instruction.ComputeSize();
            }
        }

        public void WriteTo(X86Assembler assembler, long baseAddress)
        {
            ComputeOffsets(baseAddress);
            foreach (var instruction in _instructions)
            {
                if (instruction.Operand1 != null)
                {
                    var label = instruction.Operand1.Value as X86Label;
                    if (label != null)
                        instruction.Operand1.Value = label.Instruction.Offset;
                }

                assembler.Write(instruction);
            }
        }
    }
}
