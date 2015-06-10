using AsmResolver.X86;

namespace AsmResolver.X86.Jit
{
    public class X86Label
    {
        public string Name
        {
            get;
            set;
        }

        public X86Instruction Instruction
        {
            get;
            set;
        }
    }
}