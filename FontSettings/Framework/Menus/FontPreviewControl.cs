using System;
using CodeShared;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValleyUI;
using StardewValleyUI.Controls;

namespace FontSettings.Framework.Menus
{
    internal class FontPreviewControl : Container
    {
        private bool _merged = false;
        private CombinedPreviewFontBlock _combinedPreviewBlock;

        public Orientation Orientation { get; set; } = Orientation.Horizontal;

        public int Gap { get; set; } = 0;

        public int CellPreviewWidth
        {
            get => this.VanillaBlock.BorderWidth;
            set => this.VanillaBlock.BorderWidth = this.CurrentBlock.BorderWidth = value;
        }

        public int CellPreviewHeight
        {
            get => this.VanillaBlock.BorderHeight;
            set => this.VanillaBlock.BorderHeight = this.CurrentBlock.BorderHeight = value;
        }

        private PreviewFontBlock VanillaBlock { get; } = new();

        private PreviewFontBlock CurrentBlock { get; } = new();

        public FontExampleLabel VanillaLabel => this._merged ? this._combinedPreviewBlock.VanillaLabel : this.VanillaBlock.Label;

        public FontExampleLabel CurrentLabel => this._merged ? this._combinedPreviewBlock.CurrentLabel : this.CurrentBlock.Label;

        public override int Width => this.Orientation is Orientation.Horizontal
            ? this.VanillaBlock.Width + this.Gap + this.CurrentBlock.Width
            : Math.Max(this.VanillaBlock.Width, this.CurrentBlock.Width);

        public override int Height => this.Orientation is Orientation.Vertical
            ? this.VanillaBlock.Height + this.Gap + this.CurrentBlock.Height
            : Math.Max(this.VanillaBlock.Height, this.CurrentBlock.Height);

        public FontPreviewControl()
        {
            this.VanillaBlock = new PreviewFontBlock();
            this.VanillaBlock.Label = new FontExampleLabel();
            this.CurrentBlock = new PreviewFontBlock();
            this.CurrentBlock.Label = new FontExampleLabel();

            this.AddChild(this.VanillaBlock);
            this.AddChild(this.CurrentBlock);

            this._combinedPreviewBlock = new CombinedPreviewFontBlock(this.VanillaBlock.Label, this.CurrentBlock.Label, this);
        }

        public void MergePreviews()
        {
            this._merged = true;

            this.RemoveChild(this.VanillaBlock);
            this.RemoveChild(this.CurrentBlock);

            this._combinedPreviewBlock = new CombinedPreviewFontBlock(this.VanillaBlock.Label, this.CurrentBlock.Label, this);
            this.VanillaBlock.Label = null;
            this.CurrentBlock.Label = null;

            this.AddChild(this._combinedPreviewBlock);
        }

        public void SeperatePreviews()
        {
            this._merged = false;

            this.RemoveChild(this._combinedPreviewBlock);

            this.VanillaBlock.Label = this._combinedPreviewBlock.VanillaLabel;
            this.CurrentBlock.Label = this._combinedPreviewBlock.CurrentLabel;

            this.AddChild(this.VanillaBlock);
            this.AddChild(this.CurrentBlock);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Orientation is Orientation.Horizontal)
            {
                this.VanillaBlock.LocalPosition = new Vector2(0, 0);
                this.CurrentBlock.LocalPosition = new Vector2(this.VanillaBlock.Width + this.Gap, 0);
            }
            else
            {
                this.VanillaBlock.LocalPosition = new Vector2(0, 0);
                this.CurrentBlock.LocalPosition = new Vector2(0, this.VanillaBlock.Height + this.Gap);
            }
        }

        private class PreviewFontBlock : Container
        {
            private readonly TextureBox _backgroundBox;
            private readonly Thickness _backgroundBorderThickness;

            public int BorderWidth
            {
                get => this.SettableWidth;
                set => this.SettableWidth = value;
            }

            public int BorderHeight
            {
                get => this.SettableHeight;
                set => this.SettableHeight = value;
            }

            private FontExampleLabel _label;
            public FontExampleLabel Label
            {
                get => this._label;
                set
                {
                    if (this._label != null)
                        this.RemoveChild(this._label);

                    this._label = value;

                    if (this._label != null)
                        this.AddChild(this._label);
                }
            }

            public PreviewFontBlock()
            {
                this._backgroundBox = new TextureBox()
                {
                    Kind = TextureBoxes.Patterns,
                    Scale = Game1.pixelZoom,
                    DrawShadow = false,
                };
                this.AddChild(this._backgroundBox);

                this._backgroundBorderThickness = new Thickness(3 * Game1.pixelZoom);
            }

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);

                this._backgroundBox.SettableWidth = this.Width;
                this._backgroundBox.SettableHeight = this.Height;

                if (this.Label != null)
                    this.Label.LocalPosition = new Vector2(
                        this.BorderWidth / 2 - this.Label.Width / 2,
                        this.BorderHeight / 2 - this.Label.Height / 2);
            }

            public override void Draw(SpriteBatch b)
            {
                this._backgroundBox.Draw(b);

                Rectangle scissor = new Rectangle(
                    (int)(this.Bounds.X + this._backgroundBorderThickness.Left),
                    (int)(this.Bounds.Y + this._backgroundBorderThickness.Top),
                    (int)(this.Bounds.Width - this._backgroundBorderThickness.Left - this._backgroundBorderThickness.Right),
                    (int)(this.Bounds.Height - this._backgroundBorderThickness.Top - this._backgroundBorderThickness.Bottom));
                b.InNewScissoredState(scissor, Vector2.Zero, () =>
                {
                    this.Label?.Draw(b);
                });
            }
        }

        private class CombinedPreviewFontBlock : PreviewFontBlock
        {
            private readonly FontExampleLabel _vanillaLabel;
            private readonly FontExampleLabel _currentLabel;
            private readonly FontPreviewControl _preview;

            public override int Width => this._preview.Width;

            public override int Height => this._preview.Height;

            internal FontExampleLabel VanillaLabel => this._vanillaLabel;

            internal FontExampleLabel CurrentLabel => this._currentLabel;

            public CombinedPreviewFontBlock(FontExampleLabel vanillaLabel, FontExampleLabel currentLabel, FontPreviewControl preview)
            {
                this._vanillaLabel = vanillaLabel ?? throw new ArgumentNullException(nameof(vanillaLabel));
                this._currentLabel = currentLabel ?? throw new ArgumentNullException(nameof(currentLabel));
                this._preview = preview ?? throw new ArgumentNullException(nameof(preview));
            }

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);

                // 居中合并的文本。
                int combineWidth = Math.Max(this.VanillaLabel.Width, this._currentLabel.Width);
                int combineHeight = Math.Max(this.VanillaLabel.Height, this._currentLabel.Height);
                this.VanillaLabel.LocalPosition = this._currentLabel.LocalPosition
                    = this.Position + new Vector2(this.Width / 2 - combineWidth / 2, this.Height / 2 - combineHeight / 2);
            }

            public override void Draw(SpriteBatch b)
            {
                base.Draw(b);

                b.InNewScissoredState(this.Bounds, Vector2.Zero, () =>
                {
                    this._vanillaLabel.Draw(b);
                    this._currentLabel.Draw(b);
                });
            }
        }
    }
}
