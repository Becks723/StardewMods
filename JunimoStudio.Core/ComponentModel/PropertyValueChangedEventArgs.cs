using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio.Core.ComponentModel
{
    /// <summary>
    /// Derived from <see cref="ProgressChangedEventArgs"/> with two additional properties,
    /// that represent value before and after changing.
    /// </summary>
    public class PropertyValueChangedEventArgs : PropertyChangedEventArgs
    {
        /// <summary>
        /// Gets the property value before changing.
        /// </summary>
        public object Before { get; }

        /// <summary>
        /// Gets the property value after changing.
        /// </summary>
        public object After { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValueChangedEventArgs"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        /// <param name="before">The property value before changing.</param>
        /// <param name="after">The property value after changing.</param>
        public PropertyValueChangedEventArgs(string propertyName, object before, object after)
            : base(propertyName)
        {
            Before = before;
            After = after;
        }
    }
}
