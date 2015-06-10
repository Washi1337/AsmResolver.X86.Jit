using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.X86;

namespace AsmResolver.X86.Jit
{
    public partial class X86Emitter
    {
        public X86Emitter Inc(X86Register register)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Inc,
                OpCode = X86OpCodes.SingleByteOpCodes[X86OpCodes.Inc_Eax.Op1 + ((int)register & 7)],
                Operand1 = new X86Operand(register),
            };
            return Append(instruction);
        }

        public X86Emitter Dec(X86Register register)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Dec,
                OpCode = X86OpCodes.SingleByteOpCodes[X86OpCodes.Dec_Eax.Op1 + ((int)register & 7)],
                Operand1 = new X86Operand(register),
            };
            return Append(instruction);
        }

        public X86Emitter Add(X86Register registerA, X86Register registerB)
        {
            return Append(X86OpCodes.Add_Reg1632_RegOrMem1632, X86Mnemonic.Add, registerA, registerB);
        }

        public X86Emitter Add(X86Register register, uint value)
        {
            return Arithmetic(X86Mnemonic.Add, register, value);
        }

        public X86Emitter Sub(X86Register registerA, X86Register registerB)
        {
            return Append(X86OpCodes.Sub_Reg1632_RegOrMem1632, X86Mnemonic.Sub, registerA, registerB);
        }

        public X86Emitter Sub(X86Register register, uint value)
        {
            return Arithmetic(X86Mnemonic.Sub, register, value);
        }

        public X86Emitter Mul(X86Register register)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Mul,
                OpCode = X86OpCodes.F7,
                Operand1 = new X86Operand(register),
            };
            return Append(instruction);
        }

        public X86Emitter Div(X86Register register)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Div,
                OpCode = X86OpCodes.F7,
                Operand1 = new X86Operand(register),
            };
            return Append(instruction);
        }

        public X86Emitter Cmp(X86Register registerA, X86Register registerB)
        {
            return Append(X86OpCodes.Cmp_Reg1632_RegOrMem1632, X86Mnemonic.Cmp, registerA, registerB);
        }

        public X86Emitter Cmp(X86Register registerA, X86OperandUsage usageB, X86Register registerB, int offset)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Cmp,
                OpCode = X86OpCodes.Cmp_Reg1632_RegOrMem1632,
                Operand1 = new X86Operand(registerA),
                Operand2 = new X86Operand(usageB, registerB, offset)
            };
            return Append(instruction);
        }

        public X86Emitter Cmp(X86Register register, uint value)
        {
            return Arithmetic(X86Mnemonic.Cmp, register, value);
        }

        public X86Emitter Or(X86Register registerA, X86Register registerB)
        {
            return Append(X86OpCodes.Or_Reg1632_RegOrMem1632, X86Mnemonic.Or, registerA, registerB);
        }

        public X86Emitter Or(X86Register register, uint value)
        {
            return Arithmetic(X86Mnemonic.Or, register, value);
        }

        public X86Emitter Xor(X86Register registerA, X86Register registerB)
        {
            return Append(X86OpCodes.Xor_Reg1632_RegOrMem1632, X86Mnemonic.Xor, registerA, registerB);
        }

        public X86Emitter Xor(X86Register register, uint value)
        {
            return Arithmetic(X86Mnemonic.Xor, register, value);
        }

        public X86Emitter And(X86Register registerA, X86Register registerB)
        {
            return Append(X86OpCodes.And_Reg1632_RegOrMem1632, X86Mnemonic.And, registerA, registerB);
        }

        public X86Emitter And(X86Register register, uint value)
        {
            return Arithmetic(X86Mnemonic.And, register, value);
        }

        public X86Emitter Rol(X86Register register, uint value)
        {
            return Bitshift(X86Mnemonic.Rol, register, value);
        }

        public X86Emitter Ror(X86Register register, uint value)
        {
            return Bitshift(X86Mnemonic.Ror, register, value);
        }

        public X86Emitter Shl(X86Register register, uint value)
        {
            return Bitshift(X86Mnemonic.Shl, register, value);
        }

        public X86Emitter Shr(X86Register register, uint value)
        {
            return Bitshift(X86Mnemonic.Shr, register, value);
        }

        private X86Emitter Bitshift(X86Mnemonic mnemonic, X86Register register, uint value)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = mnemonic,
                OpCode = value == 1
                    ? X86OpCodes.BitShift_RegOrMem1632_1
                    : X86OpCodes.BitShift_RegOrMem1632_Imm8,
                Operand1 = new X86Operand(register),
                Operand2 = new X86Operand(value),
            };

            return Append(instruction);
        }

        private X86Emitter Arithmetic(X86Mnemonic mnemonic, X86Register register, uint value)
        {
            return RegImm8Or32(X86OpCodes.Arithmetic_RegOrMem32_Imm8, X86OpCodes.Arithmetic_RegOrMem32_Imm1632, mnemonic,
                register, value);
        }

        private X86Emitter RegImm8Or32(X86OpCode shortForm, X86OpCode longForm, X86Mnemonic mnemonic,
            X86Register register, uint value)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = mnemonic,
                OpCode =
                    value <= byte.MaxValue
                        ? shortForm
                        : longForm,
                Operand1 = new X86Operand(register),
                Operand2 = new X86Operand(value)
            };
            return Append(instruction);
        }
    }
}
