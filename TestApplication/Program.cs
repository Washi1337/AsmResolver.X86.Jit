using System;
using System.Runtime.InteropServices;
using System.Text;
using AsmResolver.X86;
using AsmResolver.X86.Jit;

namespace TestApplication
{
    internal class Program
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NativeMethodSignature(IntPtr text, int count);

        private static unsafe void Main(string[] args)
        {
            using (var procedure = BuildNativeProcedure())
            {
                DisassembleAndPrint(procedure.Address, procedure.Size);
                var nativeMethod = procedure.Delegate;
                
                Console.Write("Format:");
                var formatBytes = Encoding.ASCII.GetBytes(Console.ReadLine() + "\r\n");

                Console.Write("Count:");
                int count = int.Parse(Console.ReadLine());

                fixed (byte* formatPtr = formatBytes)
                {
                    nativeMethod(new IntPtr(formatPtr), count);
                }
            }
            Console.ReadKey();
        }
        
        private static X86Procedure<NativeMethodSignature> BuildNativeProcedure()
        {
            // C signature: __cdecl void PrintRepeat(const char* format, unsigned int count)
            // Description: Prints a string a specific amount of times sequentially.
            //
            // the x86 code is equivalent to the following loop in C:
            // for (int i = 0; i < count; i++)
            //     printf(format, i);

            var emitter = new X86Emitter();

            var conditionStart = new X86Label();
            var loopStart = new X86Label();

            const X86Register eax = X86Register.Eax;
            const X86Register ecx = X86Register.Ecx;
            const X86Register ebp = X86Register.Ebp;
            const X86Register esp = X86Register.Esp;
            const X86OperandUsage dword = X86OperandUsage.DwordPointer;

            const int varCounter = -8;
            const int paramFormat = +8;
            const int paramCount = +12;

            emitter
                .Push(ebp)                                 // Function prologue.
                .Mov(ebp, esp)
                
                .Push(ecx)                                 // Save ECX
                .Sub(esp, 4)                               // Allocate space for counter var.
                
                .Mov(dword, ebp, varCounter, 0u)           // int counter = 0;
                .Jmp(conditionStart)                       // goto conditionStart;

                .MarkLabel(loopStart)                      // loopStart:
                .Push(dword, ebp, varCounter)              // printf(format, counter);
                .Push(dword, ebp, paramFormat)
                .Call("msvcrt.dll", "printf")
                .Add(esp, 8)

                .Mov(eax, dword, ebp, varCounter)          // counter++;
                .Inc(eax)
                .Mov(dword, ebp, varCounter, eax)

                .MarkLabel(conditionStart)                 // conditionStart:
                .Mov(ecx, dword, ebp, varCounter)          // if (counter <= count) goto loopStart;
                .Cmp(ecx, dword, ebp, paramCount)
                .Jle(loopStart)

                .Add(esp, 4)                               // Free counter variable. 
                .Pop(ecx)                                  // Restore ECX
                
                .Mov(esp, ebp)                             // Function epilogue.
                .Pop(ebp)
                .Retn();

            return X86Procedure<NativeMethodSignature>.FromEmitter(emitter);
        }
        
        private static void DisassembleAndPrint(IntPtr ptr, long size)
        {
            var formatter = new FasmX86Formatter();
            var reader = new UnsafeMemoryReader(ptr, size);
            var disassembler = new X86Disassembler(reader);

            while (reader.Position - reader.StartPosition < reader.Length)
            {
                var start = reader.Position;
                var instruction = disassembler.ReadNextInstruction();
                var end = reader.Position;

                var buffer = new byte[end - start];
                Marshal.Copy(new IntPtr(start), buffer, 0, buffer.Length);
                Console.WriteLine("{0:X8} |   {1, -25} |   {2}", instruction.Offset, BitConverter.ToString(buffer).Replace('-', ' '), formatter.FormatInstruction(instruction));
            }
        }
    }
}
