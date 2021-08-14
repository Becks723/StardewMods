using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio.Core.ComponentModel
{
    /// <summary>Notifies clients that a property value has changed, and provides both property value both before and after changing.</summary>
    public interface INotifyPropertyValueChanged
    {
        event EventHandler<PropertyValueChangedEventArgs> PropertyValueChanged;
    }
}
