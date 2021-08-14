using System;
using System.Collections.Specialized;

namespace JunimoStudio.Core.ComponentModel
{
    /// <summary>Based on <see cref="INotifyCollectionChanged"/> functions, notifies listeners of collection item changes.</summary>
    public interface INotifyCollectionItemChanged : INotifyCollectionChanged
    {
        /// <summary>
        /// Raised after any property of any item in collection is changed.
        /// </summary>
        event EventHandler<ItemPropertyChangedEventArgs> CollectionItemChanged;
    }
}
