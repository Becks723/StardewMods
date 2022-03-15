using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley;

namespace FluteBlockExtension.Framework
{
    /// <summary></summary>
    /// <remarks>原版的不发送<see cref="IKeyboardSubscriber.RecieveSpecialInput"/>给订阅者。</remarks>
    internal class KeyboardDispatcherPlus : KeyboardDispatcher
    {
        public KeyboardDispatcherPlus(GameWindow window)
            : base(window)
        {
            window.KeyDown += this.Window_KeyDown;
        }

        public new void Cleanup()
        {
            this._window.KeyDown -= this.Window_KeyDown;
            base.Cleanup();
        }

        private void Window_KeyDown(object sender, InputKeyEventArgs e)
        {
            this._keysDown.Add(e.Key);
        }
    }
}
