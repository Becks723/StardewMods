using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using K4os.Compression.LZ4;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework.Exporting
{
    internal enum CompressAlgorithm
    {
        None = 0x00,         // don't compress
        Lz4 = 0x40,          // MonoGame implementation
        XMemCompress = 0x80  // XNA 4.0 spec
    }

    internal class ObjectWriterResolveEventArgs : EventArgs
    {
        public Type ObjectType { get; }

        public void SetWriter(IObjectWriter writer)
        {

        }
    }

    internal class XnbWriter : BinaryWriter
    {
        private static readonly Dictionary<Type, IObjectWriter> _writerPool = new();

        private readonly Dictionary<Type, IObjectWriter> _objectWriters = new();

        private readonly XnbPlatform _xnbPlatform;
        private readonly GameFramework _gameFramework;
        private readonly GraphicsProfile _graphicsProfile;
        private readonly bool _isCompressed;

        private readonly Stream _mainStream;

        public XnbWriter(Stream stream, XnbPlatform xnbPlatform, GameFramework gameFramework, GraphicsProfile graphicsProfile, bool isCompressed)
        {
            this._xnbPlatform = xnbPlatform;
            this._gameFramework = gameFramework;
            this._graphicsProfile = graphicsProfile;
            this._isCompressed = isCompressed;
            this._mainStream = stream;

            this.OutStream = this._mainStream;
        }

        public XnbWriter AddObjectWriter(IObjectWriter objectWriter)
        {
            lock (this._objectWriters)
            {
                Type type = objectWriter.Type;
                if (!this._objectWriters.ContainsKey(type))
                    this._objectWriters[type] = objectWriter;
            }

            return this;
        }

        public void WritePrimaryObject<TObject>(TObject value)
        {
            CompressAlgorithm compressAlgorithm =
                SelectCompressAlgorithm(this._isCompressed, this._gameFramework);
            this.Write('X');
            this.Write('N');
            this.Write('B');
            this.Write(TargetPlatform(this._xnbPlatform));
            this.Write(XnbFormatVersion());

            byte profileFlag = (byte)(this._graphicsProfile == GraphicsProfile.HiDef ? 0x01 : 0);
            byte compressFlag = (byte)compressAlgorithm;

            using MemoryStream primaryObjStream = new MemoryStream();
            this.OutStream = primaryObjStream;
            {
                this.WriteObject(value);
            }

            using MemoryStream dataStream = new MemoryStream();
            this.OutStream = dataStream;
            {
                lock (this._objectWriters)
                {
                    this.Write7BitEncodedInt(this._objectWriters.Count);
                    foreach (IObjectWriter writer in this._objectWriters.Values)
                    {
                        this.Write(writer.TypeReaderName);
                        this.Write(writer.TypeReaderVersionNumber);
                    }
                }

                this.Write7BitEncodedInt(0);

                primaryObjStream.Position = 0;
                primaryObjStream.CopyTo(dataStream);
            }

            this.OutStream = this._mainStream;

            if (TryCompressData(dataStream, compressAlgorithm,
                out int totalSize, out int decompressedDataSize, out byte[] compressedArray, out int compressedLength))
            {
                this.Write((byte)(profileFlag | compressFlag));
                this.Write(totalSize);
                this.Write(decompressedDataSize);
                this.Write(compressedArray, 0, compressedLength);
            }
            else
            {
                this.Write((byte)profileFlag);
                this.Write(totalSize);
                this.Write(dataStream.GetBuffer(), 0, (int)dataStream.Length);
            }
        }

        public void WriteObject<TObject>(TObject obj)
        {
            IObjectWriter writer;
            int typeId;
            Type objType = typeof(TObject);

            lock (this._objectWriters)
            {
                if (!this._objectWriters.TryGetValue(objType, out writer))
                    this._objectWriters[objType] = writer = this.GetObjectWriter(objType);
                var array = this._objectWriters.Values.ToArray();
                typeId = Array.IndexOf(array, writer) + 1;
            }

            // write typeId
            if (!objType.IsValueType)
            {
                if (obj == null)
                    this.Write7BitEncodedInt(0);
                else
                    this.Write7BitEncodedInt(typeId);
            }

            // write value
            writer.Write(this, obj);
        }

        protected virtual IObjectWriter GetObjectWriter(Type type)
        {
            IObjectWriter writer;

            Type[] writerTypes = typeof(IObjectWriter).Assembly.GetTypes()
                .Where(t => t.IsAssignableTo(typeof(IObjectWriter)))
                .Select(t => new { type = t, attr = t.GetCustomAttribute<ObjectWriterAttribute>() })
                .Where(x => x.attr?.ObjectType == type
                    || (type.IsGenericType && x.attr?.ObjectType == type.GetGenericTypeDefinition()))
                .Select(x => x.type)
                .ToArray();

            for (int i = 0; i < writerTypes.Length; i++)
            {
                Type writerType = writerTypes[i];

                if (writerType.IsGenericTypeDefinition)
                {
                    Type[] genericParameters = type.GenericTypeArguments;
                    writerType = writerType.MakeGenericType(genericParameters);
                }

                try
                {
                    return (IObjectWriter)Activator.CreateInstance(writerType);
                }
                catch (Exception) { }
            }

            throw new NotImplementedException($"Cannot find type reader for '{type}' type.");
        }

        private static char TargetPlatform(XnbPlatform platform)
        {
            switch (platform)
            {
                case XnbPlatform.Windows: return 'w';
                case XnbPlatform.DesktopGL: return 'd';
                case XnbPlatform.Android: return 'a';
                default:
                    throw new NotSupportedException();
            }
        }

        private static byte XnbFormatVersion()
        {
            return 5;
        }

        private static CompressAlgorithm SelectCompressAlgorithm(bool isCompressed, GameFramework gameFramework)
        {
            if (!isCompressed)
                return CompressAlgorithm.None;

            switch (gameFramework)
            {
                case GameFramework.Xna:
                    return CompressAlgorithm.XMemCompress;  // XNA只识别0x80？
                case GameFramework.Monogame:
                    return CompressAlgorithm.Lz4;
                default:
                    throw new NotSupportedException();
            }
        }

        private static bool TryCompressData(MemoryStream dataStream, CompressAlgorithm algorithm,
            out int totalSize, out int decompressedDataSize, out byte[] compressedData, out int compressedDataLength)
        {
            switch (algorithm)
            {
                case CompressAlgorithm.None:
                    goto failed;

                case CompressAlgorithm.Lz4:
                    int srcLength = (int)dataStream.Length;
                    int maxOutput = LZ4Codec.MaximumOutputSize(srcLength);
                    byte[] destArray = new byte[maxOutput];
                    byte[] srcArray = dataStream.GetBuffer();
                    int count = LZ4Codec.Encode(srcArray, 0, srcLength, destArray, 0, destArray.Length);
                    if (count < 0)
                        goto failed;
                    else
                    {
                        totalSize = 6 + 4 + 4 + count;
                        decompressedDataSize = srcLength;
                        compressedData = destArray;
                        compressedDataLength = count;
                        return true;
                    }

                case CompressAlgorithm.XMemCompress:
                    throw new NotSupportedException($"Not implement '{algorithm}' yet");  // TODO:实现

                default:
                    throw new NotSupportedException();
            }

        failed:
            totalSize = 6 + 4 + (int)dataStream.Length;
            decompressedDataSize = 0;  // not needed
            compressedData = null;  // not needed
            compressedDataLength = 0;  // not needed
            return false;
        }
    }
}
