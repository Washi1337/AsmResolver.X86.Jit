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
        public X86Emitter Test(X86Register registerA, X86Register registerB)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Test,
                OpCode = X86OpCodes.Test_RegOrMem1632_Reg1632,
                Operand1 = new X86Operand(registerA),
                Operand2 = new X86Operand(registerB)
            };
            return Append(instruction);
        }

        public X86Emitter Jmp(X86Label label)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Jmp,
                OpCode = X86OpCodes.Jmp_Rel8,
                Operand1 = new X86Operand(label),
            };
            return Append(instruction);
        }

        public X86Emitter Je(X86Label label)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Je,
                OpCode = X86OpCodes.Je_Rel8,
                Operand1 = new X86Operand(label),
            };
            return Append(instruction);
        }

        public X86Emitter Jne(X86Label label)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Jne,
                OpCode = X86OpCodes.Jne_Rel8,
                Operand1 = new X86Operand(label),
            };
            return Append(instruction);
        }

        public X86Emitter Jle(X86Label label)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Jle,
                OpCode = X86OpCodes.Jle_Rel8,
                Operand1 = new X86Operand(label),
            };
            return Append(instruction);
        }

        public X86Emitter Jg(X86Label label)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Jg,
                OpCode = X86OpCodes.Jg_Rel8,
                Operand1 = new X86Operand(label),
            };
            return Append(instruction);
        }

        public X86Emitter Jge(X86Label label)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Jge,
                OpCode = X86OpCodes.Jge_Rel8,
                Operand1 = new X86Operand(label),
            };
            return Append(instruction);
        }

        public X86Emitter Jl(X86Label label)
        {
            var instruction = new X86Instruction()
            {
                Mnemonic = X86Mnemonic.Jl,
                OpCode = X86OpCodes.Jl_Rel8,
                Operand1 = new X86Operand(label),
            };
            return Append(instruction);
        }
    }
}
