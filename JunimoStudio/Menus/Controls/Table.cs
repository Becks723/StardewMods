using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace JunimoStudio.Menus.Controls
{
    internal class Table : Container
    {
        private readonly List<Element[]> Rows = new();

        private Vector2 SizeImpl;
        public Vector2 Size
        {
            get => SizeImpl;
            set
            {
                SizeImpl = new Vector2(value.X, (int)value.Y / RowHeight * RowHeight);
                UpdateScrollbar();
            }
        }

        public const int RowPadding = 16;
        private int RowHeightImpl;
        public int RowHeight
        {
            get => RowHeightImpl;
            set
            {
                RowHeightImpl = value + RowPadding;
                UpdateScrollbar();
            }
        }

        public int RowCount => Rows.Count;

        public Scrollbar Scrollbar { get; }

        public Table()
        {
            Scrollbar = new Scrollbar {
                LocalPosition = new Vector2(0, 0)
            };
            AddChild(Scrollbar);
        }

        public void AddRow(Element[] elements)
        {
            Rows.Add(elements);
            foreach (var child in elements)
                AddChild(child);
            UpdateScrollbar();
        }

        private void UpdateScrollbar()
        {
            Scrollbar.LocalPosition = new Vector2(Size.X + 48, Scrollbar.LocalPosition.Y);
            Scrollbar.RequestLength = (int)Size.Y;
            Scrollbar.Rows = Rows.Count;
            Scrollbar.FrameSize = (int)(Size.Y / RowHeight);
        }

        public override int Width => (int)Size.X;
        public override int Height => (int)Size.Y;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            //if (hidden) return;

            int ir = 0;
            foreach (var row in Rows)
            {
                foreach (var element in row)
                {
                    element.LocalPosition = new Vector2(element.LocalPosition.X, ir * RowHeight - Scrollbar.TopRow * RowHeight);
                    if (!(element is Label) && // Labels must update anyway to get rid of hovertext on scrollwheel
                            (element.Position.Y < Position.Y || element.Position.Y + RowHeight - RowPadding > Position.Y + Size.Y))
                        continue;
                    element.Update(gameTime);
                }
                ++ir;
            }
            Scrollbar.Update(gameTime);
        }

        //public void ForceUpdateEvenHidden(bool hidden = false)
        //{
        //    int ir = 0;
        //    foreach (var row in Rows)
        //    {
        //        foreach (var element in row)
        //        {
        //            element.LocalPosition = new Vector2(element.LocalPosition.X, ir * RowHeight - Scrollbar.ScrollPercent * Rows.Count * RowHeight);
        //            element.Update(hidden || element.Position.Y < Position.Y || element.Position.Y + RowHeight - RowPadding > Position.Y + Size.Y);
        //        }
        //        ++ir;
        //    }
        //    Scrollbar.Update(hidden);
        //}

        public override void Draw(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, (int)Position.X - 32, (int)Position.Y - 32, (int)Size.X + 64, (int)Size.Y + 64, Color.White);

            foreach (var row in Rows)
                foreach (var element in row)
                {
                    if (element.Position.Y < Position.Y || element.Position.Y + RowHeight - RowPadding > Position.Y + Size.Y)
                        continue;
                    if (element == RenderLast)
                        continue;
                    element.Draw(b);
                }

            RenderLast?.Draw(b);

            Scrollbar.Draw(b);
        }
    }
}
