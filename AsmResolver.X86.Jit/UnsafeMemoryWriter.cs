using System;
using System.IO;
using System.Runtime.InteropServices;
using AsmResolver;

namespace AsmResolver.X86.Jit
{
    public unsafe class UnsafeMemoryWriter : IBinaryStreamWriter
    {
        private readonly byte* _startPtr;
        private byte* _ptr;

        public UnsafeMemoryWriter(IntPtr ptr, long length)
            : this((byte*)ptr, length)
        {
        }

        public UnsafeMemoryWriter(byte* ptr, long length)
        {
            _startPtr = _ptr = ptr;
            Length = length;
        }

        public long Position
        {
            get { return (long)_ptr; }
            set
            {
                if (value < (long)_startPtr || value >= (long)(_startPtr + Length))
                    throw new OutOfMemoryException();
                _ptr = (byte*)value;
            }
        }

        public long Length
        {
            get;
            private set;
        }

        private void AssertCanWriteBytes(int count)
        {
            if (Position - (long)_startPtr + count > Length)
                throw new OutOfMemoryException();
        }

        public void WriteBytes(byte[] buffer, int count)
        {
            AssertCanWriteBytes(count);
            Marshal.Copy(buffer, 0, new IntPtr(_ptr), count);
            _ptr += count;
        }

        public void WriteByte(byte value)
        {
            AssertCanWriteBytes(1);
            *_ptr = value;
            _ptr++;
        }

        public void WriteUInt16(ushort value)
        {
            AssertCanWriteBytes(2);
            *(ushort*)_ptr = value;
            _ptr += 2;
        }

        public void WriteUInt32(uint value)
        {
            AssertCanWriteBytes(4);
            *(uint*)_ptr = value;
            _ptr += 4;
        }

        public void WriteUInt64(ulong value)
        {
            AssertCanWriteBytes(8);
            *(ulong*)_ptr = value;
            _ptr += 8;
        }

        public void WriteSByte(sbyte value)
        {
            AssertCanWriteBytes(1);
            *(sbyte*)_ptr = value;
            _ptr++;
        }

        public void WriteInt16(short value)
        {
            AssertCanWriteBytes(2);
            *(short*)_ptr = value;
            _ptr += 2;
        }

        public void WriteInt32(int value)
        {
            AssertCanWriteBytes(4);
            *(int*)_ptr = value;
            _ptr += 4;
        }

        public void WriteInt64(long value)
        {
            AssertCanWriteBytes(8);
            *(long*)_ptr = value;
            _ptr += 8;
        }

        public void WriteSingle(float value)
        {
            AssertCanWriteBytes(4);
            *(float*)_ptr = value;
            _ptr += 4;
        }

        public void WriteDouble(double value)
        {
            AssertCanWriteBytes(8);
            *(double*)_ptr = value;
            _ptr += 8;
        }
    }
}