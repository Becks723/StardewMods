using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValleyUI;
using StardewValleyUI.Controls;
using StardewValleyUI.Data;

namespace FluteBlockExtension.Framework.Menus
{
    internal class SortedComboBox : ComboBox
    {
        private readonly ScrollBar2 _scrollBar;

        private readonly List<ClickableComponent> _capitals = new();

        private readonly List<ClickableLabelAdapter> _capitalAdapters = new();

        private static readonly char[] Initials = GetInitials();

        public SortedComboBox()
            : base()
        {
            this._scrollBar = new()
            {
                Orientation = Orientation.Vertical,
                BarLength = this.VisibleRows * this.RowHeight,
                Minimum = 0,
            };
            this._scrollBar.LocalPosition = this.Position + new Vector2(this.Width - 48, this.RowHeight);
            this._scrollBar.ScrollChanged += this.OnScrollChanged;

            this.InitializeBindings();
        }

        /// <summary>Update choices. Resort choices. Update capitals if necessary.</summary>
        public void UpdateChoices(string[] choices = null)
        {
            choices ??= this.Choices as string[];
            object[] copy = new object[choices.Length];
            Array.Copy(choices, copy, choices.Length);
            Array.Sort(copy);  // must sort before assign in order to update combobox's display texts.
            this.Choices = copy;

            // labels.
            this._capitals.Clear();
            this._capitalAdapters.Clear();
            foreach (char initial in Initials)
            {
                if ((initial is '#' && this.Choices.Any(choice => Regex.IsMatch((choice as string).Substring(0, 1), @"[^A-Za-z]")))                     //    '#' && no a-z
                 || (initial != '#' && this.Choices.Any(choice => (choice as string).StartsWith(initial) || (choice as string).StartsWith(char.ToLower(initial)))))    // || not '#' && has a-z
                {
                    ClickableComponent label = new(Rectangle.Empty, initial.ToString(), initial.ToString());
                    this._capitals.Add(label);
                    this._capitalAdapters.Add(new(label));
                }
            }

            float height = this.RowHeight * this.VisibleRows;
            float perHeight = height / this._capitals.Count;
            float curHeight = Game1.dialogueFont.MeasureString(this._capitals[0].label).Y;
            float scale = perHeight / curHeight;
            foreach (var label in this._capitalAdapters)
                label.Scale = scale;

            float y = this.Position.Y + this.RowHeight;
            foreach (ClickableComponent label in this._capitals)
            {
                Vector2 size = Game1.dialogueFont.MeasureString(label.label) * scale;
                label.bounds = new Rectangle((int)(this.Position.X - size.X - 4), (int)y, (int)size.X, (int)size.Y);
                y += size.Y;
            }
        }

        public void ReceiveLeftClick(int x, int y, bool playSound = true)
        {
            foreach (ClickableComponent label in this._capitals)
            {
                if (label.bounds.Contains(x, y))
                {
                    this.NavigateTo(label.label[0]);
                }
            }
        }

        public void PerformHoverAction(int x, int y)
        {
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            this._scrollBar.BarLength = this.VisibleRows * this.RowHeight;
            if (this.IsExpanded)
            {
                this._scrollBar.Update(gameTime);
                foreach (var label in this._capitalAdapters)
                    label.Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch b)
        {
            if (this.IsExpanded)
            {
                this._scrollBar.Draw(b);
                foreach (var label in this._capitalAdapters)
                    label.Draw(b);
            }

            // base draw at last to avoid cover text.
            base.Draw(b);
        }

        protected override IEnumerable GetVisualChildren()
        {
            foreach (var item in base.GetVisualChildren())
                yield return item;
            foreach (var label in this._capitalAdapters)
                yield return label;
            yield return this._scrollBar;
        }

        protected override void OnPositionChanged(Vector2 oldPosition, Vector2 newPosition)
        {
            base.OnPositionChanged(oldPosition, newPosition);

            // scroll bar pos.
            this._scrollBar.LocalPosition = this.Position + new Vector2(this.Width - 48, this.RowHeight);

            // labels' pos.
            float height = this.RowHeight * this.VisibleRows;
            float perHeight = height / this._capitals.Count;
            float curHeight = Game1.dialogueFont.MeasureString(this._capitals[0].label).Y;
            float scale = perHeight / curHeight;
            foreach (var label in this._capitalAdapters)
                label.Scale = scale;

            float y = this.Position.Y + this.RowHeight;
            foreach (ClickableComponent label in this._capitals)
            {
                Vector2 size = Game1.dialogueFont.MeasureString(label.label) * scale;
                label.bounds = new Rectangle((int)(this.Position.X - size.X - 4), (int)y, (int)size.X, (int)size.Y);
                y += size.Y;
            }
        }

        protected override void OnWidthChanged(int oldWidth, int newWidth)
        {
            base.OnWidthChanged(oldWidth, newWidth);

            // scroll bar pos.
            this._scrollBar.LocalPosition = this.Position + new Vector2(this.Width - 48, this.RowHeight);
        }

        protected override void InitializeBindings(IBindingContext context)
        {
            context
                .AddBinding(() => this.RowHeight * this._indexOffset, () => this._scrollBar.Value, BindingMode.OneWay)
                .AddBinding(() => this.RowHeight * this.VisibleRows, () => this._scrollBar.Viewport, BindingMode.OneWay)
                .AddBinding(() => this.RowHeight * (this.Choices.Length > this.MaxDisplayRows ? this.Choices.Length - this.MaxDisplayRows : this.Choices.Length), () => this._scrollBar.Maximum, BindingMode.OneWay);
        }

        protected override bool AutoInitBindings => false;

        protected override bool CanCollapse(Point mousePos)
        {
            return !this._scrollBar.Bounds.Contains(mousePos)
                && !this._capitals.Any(label => label.bounds.Contains(mousePos));
        }

        private void OnScrollChanged(object sender, ScrollBarValueChangedEventArgs e)
        {
            this._indexOffset = (int)(e.NewValue / this.RowHeight);
            this._indexOffset = Math.Clamp(this._indexOffset, 0, this.Choices.Length > this.MaxDisplayRows ? this.Choices.Length - this.MaxDisplayRows : this.Choices.Length);
        }

        private void NavigateTo(char initial)
        {
            int index = this.GetStartingIndex(initial);
            this._indexOffset = Math.Clamp(index, 0, this.Choices.Length > this.MaxDisplayRows ? this.Choices.Length - this.MaxDisplayRows : this.Choices.Length);
        }

        private int GetStartingIndex(char initial)
        {
            if (initial is not '#')
            {
                return Array.FindIndex(this.Choices, choice => (choice as string).StartsWith(initial)
                                                                        || (choice as string).StartsWith(char.ToLower(initial)));
            }
            else
            {
                return Array.FindIndex(this.Choices, choice => Regex.IsMatch((choice as string).Substring(0, 1), @"[^A-Za-z]"));
            }
        }

        private static char[] GetInitials()
        {
            return "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray()
                .Append('#').ToArray();
        }

        private class Comparer : IComparer<char>, IComparer
        {
            public int Compare(char x, char y)
            {
                throw new NotImplementedException();
            }

            public int Compare(object x, object y)
            {
                throw new NotImplementedException();
            }
        }
    }
}