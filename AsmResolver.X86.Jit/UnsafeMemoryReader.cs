using System;
using System.IO;
using System.Runtime.InteropServices;
using AsmResolver;

namespace AsmResolver.X86.Jit
{
    public unsafe class UnsafeMemoryReader : IBinaryStreamReader
    {
        private readonly byte* _startPtr;
        private byte* _ptr;

        public UnsafeMemoryReader(IntPtr ptr, long size)
            : this((byte*)ptr, size)
        {
        }

        public UnsafeMemoryReader(byte* ptr, long size)
        {
            _startPtr = _ptr = ptr;
            Length = size;
        }

        public long StartPosition
        {
            get { return (long)_startPtr; }
        }

        public long Position
        {
            get { return (long)_ptr; }
            set
            {
                if (value < (long)_startPtr || value >= (long)(_startPtr + Length))
                    throw new EndOfStreamException();
                _ptr = (byte*)value;
            }
        }

        public long Length
        {
            get;
            private set;
        }

        private void AssertCanReadBytes(int count)
        {
            if (Position - StartPosition + count >  Length)
                throw new EndOfStreamException();
        }

        public IBinaryStreamReader CreateSubReader(long address, int size)
        {
            throw new NotImplementedException();
        }

        public byte[] ReadBytesUntil(byte value)
        {
            var start = _ptr;
            while (*_ptr != value)
            {
                AssertCanReadBytes(1);
                _ptr++;
            }
            var buffer = new byte[_ptr - start];
            Marshal.Copy(new IntPtr(start), buffer, 0, buffer.Length);
            return buffer;
        }

        public byte[] ReadBytes(int count)
        {
            AssertCanReadBytes(count);
            var buffer = new byte[count];
            Marshal.Copy(new IntPtr(_ptr), buffer, 0, count);
            _ptr += count;
            return buffer;
        }

        public byte ReadByte()
        {
            AssertCanReadBytes(1);
            return *_ptr++;
        }

        public ushort ReadUInt16()
        {
            AssertCanReadBytes(2);
            var value = *(ushort*)_ptr;
            _ptr += 2;
            return value;
        }

        public uint ReadUInt32()
        {
            AssertCanReadBytes(4);
            var value = *(uint*)_ptr;
            _ptr += 4;
            return value;
        }

        public ulong ReadUInt64()
        {
            AssertCanReadBytes(8);
            var value = *(ulong*)_ptr;
            _ptr += 8;
            return value;
        }

        public sbyte ReadSByte()
        {
            AssertCanReadBytes(1);
            var value = *(sbyte*)_ptr;
            _ptr++;
            return value;
        }

        public short ReadInt16()
        {
            AssertCanReadBytes(2);
            var value = *(short*)_ptr;
            _ptr += 2;
            return value;
        }

        public int ReadInt32()
        {
            AssertCanReadBytes(4);
            var value = *(int*)_ptr;
            _ptr += 4;
            return value;
        }

        public long ReadInt64()
        {
            AssertCanReadBytes(8);
            var value = *(long*)_ptr;
            _ptr += 8;
            return value;
        }

        public float ReadSingle()
        {
            AssertCanReadBytes(4);
            var value = *(float*)_ptr;
            _ptr += 3;
            return value;
        }

        public double ReadDouble()
        {
            AssertCanReadBytes(8);
            var value = *(double*)_ptr;
            _ptr += 8;
            return value;
        }
    }
}