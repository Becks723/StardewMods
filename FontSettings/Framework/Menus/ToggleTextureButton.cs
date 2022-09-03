using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValleyUI.Controls;

namespace FontSettings.Framework.Menus
{
    internal class ToggleTextureButton : TextureButton
    {
        private bool _isToggled;
        public bool IsToggled
        {
            get => this._isToggled;
            set => this.SetField(ref this._isToggled, value);
        }

        public ToggleTextureButton(Texture2D texture, Rectangle? sourceRectangle, float scale = 1f)
            : base(texture, sourceRectangle, scale)
        {
        }

        protected override void RaiseClick(EventArgs e)
        {
            base.RaiseClick(e);

            this.IsToggled = !this.IsToggled;
        }
    }
}
