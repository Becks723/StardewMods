using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace FluteBlockExtension.Framework.Models
{
    /// <summary>A collection of Sound-Floor pairs.</summary>
    internal class SoundFloorMap : ICollection<SoundFloorMapItem>
    {
        private readonly List<SoundFloorMapItem> _items = new();

        public int Count => this._items.Count;

        public bool IsReadOnly => ((ICollection<SoundFloorMapItem>)this._items).IsReadOnly;

        public void Add(SoundFloorMapItem item)
        {
            this._items.Add(item);
        }

        public bool Remove(SoundFloorMapItem item)
        {
            return this._items.Remove(item);
        }

        public bool Rename(string name, string newName)
        {
            foreach (SoundFloorMapItem item in this)
            {
                if (item.Sound.Name == name)
                {
                    item.Sound.Name = newName;
                    return Game1.soundBank.RenameCue(name, newName);
                }
            }

            return false;
        }

        public SoundData? FindSound(FloorData floor)
        {
            foreach (SoundFloorMapItem item in this)
            {
                if (item.Floor == floor)
                {
                    return item.Sound;
                }
            }
            return null;
        }

        public IEnumerator<SoundFloorMapItem> GetEnumerator()
        {
            return this._items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Clear()
        {
            this._items.Clear();
        }

        public bool Contains(SoundFloorMapItem item)
        {
            return this._items.Contains(item);
        }

        public void CopyTo(SoundFloorMapItem[] array, int arrayIndex)
        {
            this._items.CopyTo(array, arrayIndex);
        }
    }
}
