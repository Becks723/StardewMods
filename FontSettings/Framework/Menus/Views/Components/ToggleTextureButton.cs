using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValleyUI;
using StardewValleyUI.Controls;

namespace FontSettings.Framework.Menus.Views.Components
{
    internal class ToggleTextureButton : TextureButton
    {
        private readonly bool _togglesTexture;

        private readonly Texture2D _onTexture;
        private readonly Rectangle? _onSourceRectangle;
        private readonly float _onScale;

        private readonly Texture2D _offTexture;
        private readonly Rectangle? _offSourceRectangle;
        private readonly float _offScale;

        private static readonly UIPropertyInfo IsToggledProperty
            = new UIPropertyInfo(nameof(IsToggled), typeof(bool), typeof(ToggleTextureButton), false, OnToggled);
        public bool IsToggled
        {
            get { return this.GetValue<bool>(IsToggledProperty); }
            set { this.SetValue(IsToggledProperty, value); }
        }

        private static void OnToggled(object sender, UIPropertyChangedEventArgs e)
        {
            var button = (ToggleTextureButton)sender;
            bool isToggled = (bool)e.NewValue;

            if (button._togglesTexture)
                if (isToggled)
                {
                    button.Texture = button._onTexture;
                    button.SourceRectangle = button._onSourceRectangle;
                    button.Scale = button._onScale;
                }
                else
                {
                    button.Texture = button._offTexture;
                    button.SourceRectangle = button._offSourceRectangle;
                    button.Scale = button._offScale;
                }
        }

        public ToggleTextureButton()
            : base()
        {
            this._togglesTexture = false;
        }

        public ToggleTextureButton(Texture2D texture, Rectangle? sourceRectangle, float scale = 1f)
            : base(texture, sourceRectangle, scale)
        {
            this._togglesTexture = false;
        }

        public ToggleTextureButton(Texture2D onTexture, Rectangle? onSourceRectangle,
            Texture2D offTexture, Rectangle? offSourceRectangle, float onScale = 1f, float offScale = 1f)
            : base(offTexture, offSourceRectangle, offScale)
        {
            this._togglesTexture = true;

            this._onTexture = onTexture;
            this._onSourceRectangle = onSourceRectangle;
            this._offTexture = offTexture;
            this._offSourceRectangle = offSourceRectangle;
            this._onScale = onScale;
            this._offScale = offScale;
        }

        protected override void RaiseClick(EventArgs e)
        {
            base.RaiseClick(e);

            this.IsToggled = !this.IsToggled;
        }
    }
}
