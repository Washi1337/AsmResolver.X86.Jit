using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.X86;
using AsmResolver.X86.Jit;

namespace AsmResolver.X86.Jit
{
    public class X86Procedure<TDelegate> : IDisposable
    {
        public static X86Procedure<TDelegate> FromEmitter(X86Emitter emitter)
        {
            if (!typeof (TDelegate).IsSubclassOf(typeof (Delegate)))
                throw new ArgumentException("Type must be a subclass of System.Delegate.");

            var protection = NativeMethods.MemoryProtection.READWRITE;
            var size = emitter.GetSize();

            var ptr = NativeMethods.VirtualAlloc(IntPtr.Zero,
                new IntPtr(size),
                NativeMethods.AllocationType.COMMIT,
                protection);

            if (ptr == IntPtr.Zero)
                throw new Win32Exception();

            var writer = new UnsafeMemoryWriter(ptr, size);
            var assembler = new X86Assembler(writer);
            emitter.WriteTo(assembler, writer.Position);

            if (!NativeMethods.VirtualProtect(ptr, (uint)size, NativeMethods.MemoryProtection.EXECUTE_READ, out protection))
                throw new Win32Exception();

            return new X86Procedure<TDelegate>(ptr, size);
        }

        private X86Procedure(IntPtr address, long size)
        {
            Address = address;
            Size = size;
            Delegate = Marshal.GetDelegateForFunctionPointer<TDelegate>(Address);
        }

        public IntPtr Address
        {
            get;
            private set;
        }

        public long Size
        {
            get;
            private set;
        }

        public TDelegate Delegate
        {
            get;
            private set;
        }
        
        public void Dispose()
        {
            if (!NativeMethods.VirtualFree(Address, new IntPtr(Size), NativeMethods.FreeType.DECOMMIT))
                throw new Win32Exception();
            Address = IntPtr.Zero;
            Size = 0;
            Delegate = default(TDelegate);
        }
    }
}
