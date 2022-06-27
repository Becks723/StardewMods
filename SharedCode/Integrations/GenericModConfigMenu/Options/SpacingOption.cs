using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace CodeShared.Integrations.GenericModConfigMenu.Options
{
    /// <summary>添加一段行间距。</summary>
    internal class SpacingOption : BaseCustomOption
    {
        public override int Height { get; }

        public SpacingOption(int height = 40)
        {
            if (height < 0)
                height = 0;
            this.Height = height;
        }

        public override void Draw(SpriteBatch b, Vector2 drawOrigin)
        {
            //Rectangle scissor = b.GraphicsDevice.ScissorRectangle;

            //// 画一个“透明”的矩形把左侧的label遮住。
            //b.Draw(Game1.staminaRect, new Rectangle(scissor.X, (int)drawOrigin.Y, scissor.Width, this.Height), new Color(253, 188, 110));
        }
    }
}
