using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontInfo.OpenType
{
    internal class OpenTypeCommonReader : IDisposable
    {
        private readonly byte[] _buffer;

        private bool _isDisposed;

        private readonly Stream _stream;

        public OpenTypeCommonReader(Stream stream)
            : this(stream, 16)
        {
        }

        public OpenTypeCommonReader(Stream stream, int bufferSize)
        {
            this._stream = stream;
            this._buffer = new byte[bufferSize];
        }

        public byte ReadUInt8() => this.InternalReadByte();

        public sbyte ReadInt8() => (sbyte)this.InternalReadByte();

        public ushort ReadUInt16() => BinaryPrimitives.ReadUInt16BigEndian(this.InternalRead(2));

        public short ReadInt16() => BinaryPrimitives.ReadInt16BigEndian(this.InternalRead(2));

        public int ReadUInt24() => this.ReadUInt8() << 16 | this.ReadUInt16();

        public uint ReadUInt32() => BinaryPrimitives.ReadUInt32BigEndian(this.InternalRead(4));

        public int ReadInt32() => BinaryPrimitives.ReadInt32BigEndian(this.InternalRead(4));

        public short ReadFWORD() => this.ReadInt16();

        public ushort ReadUFWORD() => this.ReadUInt16();

        public string ReadTag() => Encoding.UTF8.GetString(this.InternalRead(4));

        public ushort ReadOffset16() => this.ReadUInt16();

        public uint ReadOffset32() => this.ReadUInt32();

        public byte[] ReadBytes(int numBytes)
        {
            byte[] bytes = new byte[numBytes];
            int read = this._stream.Read(bytes, 0, bytes.Length);

            if (read != numBytes)
            {
                byte[] copy = new byte[read];
                Buffer.BlockCopy(bytes, 0, copy, 0, copy.Length);
                return copy;
            }

            return bytes;
        }

        public long Position => this._stream.Position;

        public void Seek(long offset, SeekOrigin seekOrigin)
        {
            this._stream.Seek(offset, seekOrigin);
        }

        private ReadOnlySpan<byte> InternalRead(int bytesNum)
        {
            Span<byte> span = this._buffer.AsSpan(0, bytesNum);
            this._stream.Read(span);

            return span;
        }

        private byte InternalReadByte()
        {
            int b = this._stream.ReadByte();
            if (b == -1)
                throw new InvalidOperationException();  // TODO: 说明

            return (byte)b;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._isDisposed)
            {
                if (disposing)
                    this._stream.Dispose();
                this._isDisposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
