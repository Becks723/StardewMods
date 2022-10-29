using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValleyUI.Controls;

namespace FontSettings.Framework.Menus
{
    internal class RefreshButton : TextureButton
    {
        private static readonly Lazy<Texture2D> _refreshTexture = new(LoadRefreshTexture);

        private bool _isAnimating;

        private double _angle;

        /// <summary>动画一次全程经过的时间，单位为毫秒。</summary>
        public int AnimationDuration { get; set; } = 500;

        /// <summary>动画一次全程转过的角度，如360表示360°，转一圈。</summary>
        public int RotationAngle { get; set; } = 180;

        public RefreshButton(float scale)
            : base(_refreshTexture.Value, null, scale)
        {
        }

        protected override void UpdateOverride(GameTime gameTime)
        {
            if (this._isAnimating)
            {
                this._angle += this.GetAngleDeltaPerUpdate();
                if (this._angle > this.RotationAngle)
                {
                    this._angle = 0;
                    this._isAnimating = false;
                }
            }
        }

        protected override void DrawOverride(SpriteBatch b)
        {
            if (this.Texture != null)
            {
                int texturewidth = this.SourceRectangle.HasValue ? this.SourceRectangle.Value.Width : this.Texture.Width;
                int textureHeight = this.SourceRectangle.HasValue ? this.SourceRectangle.Value.Height : this.Texture.Height;
                b.Draw(this.Texture, this.Position + new Vector2(texturewidth / 2, textureHeight / 2) * this.Scale, this.SourceRectangle, Color.White * (this.GreyedOut ? 0.33f : 1f), this.CalculateRotationByAngle(this._angle), new Vector2(texturewidth / 2, textureHeight / 2), this.CurrentScale, SpriteEffects.None, 0f);
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            if (!this._isAnimating)
            {
                this._isAnimating = true;
                this._angle = 0;
            }
        }

        private double GetAngleDeltaPerUpdate()
        {
            const int fpsRate = 60;
            double count = this.AnimationDuration / 1000.0 * fpsRate;
            return this.RotationAngle / count;
        }

        private float CalculateRotationByAngle(double angle)
        {
            double unit = Math.PI / 180;
            return (float)(unit * angle);
        }

        private static Texture2D LoadRefreshTexture()
        {
            return Textures.Refresh;
        }
    }
}
