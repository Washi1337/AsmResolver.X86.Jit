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

            var ptr = emitter.CreateExecutableMemorySegment(out long size);
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
            if (!Kernel32.VirtualFree(Address, new IntPtr(Size), Kernel32.FreeType.DECOMMIT))
                throw new Win32Exception();
            Address = IntPtr.Zero;
            Size = 0;
            Delegate = default(TDelegate);
        }
    }
}
