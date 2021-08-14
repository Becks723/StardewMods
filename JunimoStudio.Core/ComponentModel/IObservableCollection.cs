using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio.Core.ComponentModel
{
    /// <summary>
    /// Represents a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed,
    /// or any property of any existing item changes.
    /// </summary>
    public interface IObservableCollection<T> : ICollection<T>, INotifyPropertyChanged, INotifyCollectionItemChanged
         where T : INotifyPropertyChanged
    {
    }
}
