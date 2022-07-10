using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontInfomation
{
    internal class TTCHeader
    {
        public string TtcTag { get; }

        public ushort MajorVersion { get; }

        public ushort MinorVersion { get; }

        public uint NumFonts { get; }

        public uint[] TableDirectoryOffsets { get; }

        public uint? DsigTag { get; }

        public uint? DsigLength { get; }

        public uint? DsigOffset { get; }

        public TTCHeader(OpenTypeCommonReader reader)
        {
            this.TtcTag = reader.ReadTag();
            this.MajorVersion = reader.ReadUInt16();
            this.MinorVersion = reader.ReadUInt16();
            this.NumFonts = reader.ReadUInt32();
            this.TableDirectoryOffsets = new uint[this.NumFonts];
            for (int i = 0; i < this.NumFonts; i++)
                this.TableDirectoryOffsets[i] = reader.ReadUInt32();

            if (this.MajorVersion == 2)
            {
                this.DsigTag = reader.ReadUInt32();
                this.DsigLength = reader.ReadUInt32();
                this.DsigOffset = reader.ReadUInt32();
            }
        }
    }
}
