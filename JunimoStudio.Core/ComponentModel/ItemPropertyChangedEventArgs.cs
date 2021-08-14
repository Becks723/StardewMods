using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio.Core.ComponentModel
{
    /// <summary>Provides data for a <see cref="IObservableCollection{T}.CollectionItemChanged"/> event.</summary>
    public class ItemPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        /// <summary>
        /// Gets the index of item whose property is changed.
        /// </summary>
        public int Index { get; }

        public ItemPropertyChangedEventArgs(int index, string propertyName)
            : base(propertyName)
        {
            Index = index;
        }
    }
}
