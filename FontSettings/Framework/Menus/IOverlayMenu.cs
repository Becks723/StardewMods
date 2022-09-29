using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.Menus
{
    internal class OverlayMenuClosedEventArgs : EventArgs
    {
        public object Parameter { get; }

        public OverlayMenuClosedEventArgs(object parameter)
        {
            this.Parameter = parameter;
        }
    }

    internal interface IOverlayMenu
    {
        public event EventHandler<OverlayMenuClosedEventArgs> Closed;

        void Open();

        void Close(object? parameter);
    }
}
