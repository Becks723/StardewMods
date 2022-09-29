using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValleyUI;
using StardewValleyUI.Controls;

namespace FontSettings.Framework.Menus
{
    internal class ToggleTextureButton : TextureButton
    {
        private static readonly UIPropertyInfo IsToggledProperty
            = new UIPropertyInfo(nameof(IsToggled), typeof(bool), typeof(ToggleTextureButton), false);
        public bool IsToggled
        {
            get { return GetValue<bool>(IsToggledProperty); }
            set { SetValue(IsToggledProperty, value); }
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
