using JunimoStudio.Menus.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JunimoStudio.Menus.Framework
{
    /// <summary>A simple base class for helping implement <see cref="ScrollContent"/>.</summary>
    internal abstract class ScrollContentBase : ScrollContent
    {
        protected RasterizerState _rasterizerState;

        protected Matrix _matrix;

        /// <summary>A scissored rectangle that represents the scrolling area, i.e., viewport.</summary>
        protected virtual Rectangle ScissorRectangle => Owner.Bounds;

        public ScrollContentBase(ScrollViewer owner)
            : base()
        {
            Owner = owner;
            Parent = owner;
            LocalPosition = Vector2.Zero;
            CanHorizontallyScroll = CanVerticallyScroll = true;

            _rasterizerState = new RasterizerState { ScissorTestEnable = true };
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _matrix = Matrix.CreateTranslation(new Vector3(-HorizontalOffset, -VerticalOffset, 0));
        }

        public override void Draw(SpriteBatch b)
        {
            // cache.
            SpriteSortMode oldSsm = (SpriteSortMode)typeof(SpriteBatch)
                .GetField("spriteSortMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(b);
            BlendState oldBlend = (BlendState)typeof(SpriteBatch)
                .GetField("blendState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(b);
            SamplerState oldSampler = (SamplerState)typeof(SpriteBatch)
                .GetField("samplerState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(b);
            DepthStencilState oldDepth = (DepthStencilState)typeof(SpriteBatch)
                .GetField("depthStencilState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(b);
            RasterizerState oldRasterizer = (RasterizerState)typeof(SpriteBatch)
                .GetField("rasterizerState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(b);
            Effect oldEffect = (Effect)typeof(SpriteBatch)
                .GetField("customEffect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(b);
            Matrix oldMatrix = (Matrix)typeof(SpriteBatch)
                .GetField("transformMatrix", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(b);
            Rectangle oldScissorRect = b.GraphicsDevice.ScissorRectangle;

            b.End();

            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, _rasterizerState, null, _matrix);
            b.GraphicsDevice.ScissorRectangle = ScissorRectangle;
            DrawScrollContent(b);
            b.End();

            b.Begin(oldSsm, oldBlend, oldSampler, oldDepth, oldRasterizer, oldEffect, oldMatrix);
            b.GraphicsDevice.ScissorRectangle = oldScissorRect;
            DrawNonScrollContent(b);
        }

        public override void MakeVisible(Element element, Rectangle rect)
        {
        }

        protected abstract void DrawScrollContent(SpriteBatch b);

        protected abstract void DrawNonScrollContent(SpriteBatch b);
    }
}
