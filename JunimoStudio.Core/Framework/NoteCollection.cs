using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio.Core.Framework
{
    internal class NoteCollection : INoteCollection
    {
        protected readonly IList<INote> _notes;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public NoteCollection()
        {
            this._notes = new List<INote>();
        }

        public void Add(INote note)
        {
            if (note == null)
                throw new ArgumentNullException(nameof(note));

            this._notes.Add(note);
            this.OnCollectionChanged(new(NotifyCollectionChangedAction.Add, note));
        }

        public INote Add(int pitch, long start, int duration)
        {
            INote note = new Note();
            note.Number = pitch;
            note.Start = start;
            note.Duration = duration;
            this.Add(note);
            return note;
        }

        public bool Remove(INote note)
        {
            bool removed = this._notes.Remove(note);
            if (removed)
                this.OnCollectionChanged(new(NotifyCollectionChangedAction.Remove, note));
            return removed;
        }

        public IEnumerator<INote> GetEnumerator()
        {
            return this._notes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

    }
}
