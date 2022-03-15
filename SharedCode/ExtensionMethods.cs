using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CodeShared
{
    internal static class ExtensionMethods
    {
        public static void InNewScissoredState(this SpriteBatch b, Rectangle scissorRectangle, Vector2 offset, Action action)
        {
            TField? GetField<TField>(string fieldName, BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance)
            {
                FieldInfo? field = typeof(SpriteBatch).GetField(fieldName, flags);
                if (field != null)
                    return (TField)field.GetValue(b);
                else
                {
                    // TODO: error handling.
                    return default;
                }
            }

            var spriteSortMode = GetField<SpriteSortMode>("_sortMode");
            var blendState = GetField<BlendState>("_blendState");
            var samplerState = GetField<SamplerState>("_samplerState");
            var depthStencilState = GetField<DepthStencilState>("_depthStencilState");
            var rasterizerState = GetField<RasterizerState>("_rasterizerState");
            var effect = GetField<Effect>("_effect");
            var matrix = GetField<SpriteEffect>("_spriteEffect")?.TransformMatrix;
            var scissorRect = b.GraphicsDevice.ScissorRectangle;

            b.End();
            b.Begin(spriteSortMode, blendState, samplerState, depthStencilState, StardewValley.Utility.ScissorEnabled, effect, Matrix.CreateTranslation(-offset.X, -offset.Y, 0));
            b.GraphicsDevice.ScissorRectangle = scissorRectangle;
            
            action?.Invoke();

            b.End();
            b.Begin(spriteSortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, matrix);
            b.GraphicsDevice.ScissorRectangle = scissorRect;
        }
    }
}
