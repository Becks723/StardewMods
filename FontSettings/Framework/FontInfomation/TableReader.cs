using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontInfomation
{
    internal class TableReader
    {
        private readonly OpenTypeCommonReader _reader;
        private readonly ReadOnlyDictionary<string, TableRecord> _tables;

        public TableReader(OpenTypeCommonReader reader, ReadOnlyDictionary<string, TableRecord> tables)
        {
            this._reader = reader;
            this._tables = tables;
        }

        public NameTable ReadNameTable()
        {
            TableRecord nameTableRecord = this._tables["name"];

            long savedPos = this._reader.Position;
            this._reader.Seek(nameTableRecord.Offset, System.IO.SeekOrigin.Begin);
            NameTable result = NameTable.Read(this._reader);
            this._reader.Seek(savedPos, System.IO.SeekOrigin.Begin);
            return result;
        }
    }
}
