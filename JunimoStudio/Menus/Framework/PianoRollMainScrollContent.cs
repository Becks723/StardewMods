using System;
using System.Collections.Generic;
using JunimoStudio.Menus.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using JunimoStudio.Core.ComponentModel;
using _Line = JunimoStudio.Menus.Controls.Shapes.Line;
using _Rectangle = JunimoStudio.Menus.Controls.Shapes.Rectangle;
using JunimoStudio.Core;
using System.Linq;
using GuiLabs.Undo;
using JunimoStudio.Actions;
using StardewModdingAPI;
using System.ComponentModel;
using JConstants = JunimoStudio.Core.Constants;


namespace JunimoStudio.Menus.Framework
{
    internal class PianoRollMainScrollContent : ScrollContentBase
    {
        private readonly IMonitor _monitor;

        private readonly float _noteHeight;

        private readonly float _tickLength;

        private readonly float _keyboardWidth;

        private readonly int _totalPitches = JConstants.MaxNoteNumber + 1;

        private readonly PianoRollVerticalSeperatorsHelper _verticalSeperatorsHelper;

        /// <summary>Horizonal lines seperating notes.</summary>
        private readonly List<_Line> _noteSeperators = new List<_Line>();

        private readonly List<_Rectangle> _noteRects = new List<_Rectangle>();

        /// <summary>A note board that contains all notes.</summary>
        private readonly NoteBoard _noteBoard;

        private readonly ModConfig _config;

        private readonly ITimeBasedObject _timeSettings;

        private readonly INoteCollection _notes;

        private readonly ActionManager _actionManager;

        private readonly ICursorRenderer _cursorRenderer;

        protected override Rectangle ScissorRectangle => new Rectangle(
                (int)this.Position.X + 16 + (int)this._keyboardWidth,
                (int)this.Position.Y + 16,
                this.ViewportWidth,
                this.ViewportHeight);

        public override int ViewportWidth => this.Owner.Width - 32 - (int)this._keyboardWidth;

        public override int ViewportHeight => this.Owner.Height - 32;

        public override int ExtentWidth => (int)(TimeLengthHelper.GetBarLength(this._timeSettings, this._tickLength) * 2);

        public override int ExtentHeight => (int)(this._totalPitches * this._noteHeight);

        public PianoRollMainScrollContent(IMonitor monitor, ScrollViewer owner, float tickLength, float noteHeight, float keyboardWidth,
            ModConfig config, ITimeBasedObject timeSettings, INoteCollection notes, ActionManager actionManager, ICursorRenderer cursorRenderer)
            : base(owner)
        {
            this._monitor = monitor;
            this._config = config;
            this._timeSettings = timeSettings;
            this._notes = notes;
            this._actionManager = actionManager;
            this._cursorRenderer = cursorRenderer;
            this._noteHeight = noteHeight;
            this._tickLength = tickLength;
            this._keyboardWidth = keyboardWidth;
            this._verticalSeperatorsHelper = new PianoRollVerticalSeperatorsHelper(
                new Rectangle(this.ScissorRectangle.X, this.ScissorRectangle.Y, this.ExtentWidth, this.ExtentHeight), this._config, this._timeSettings, this._tickLength);

            config.PianoRoll.PropertyChanged += this.OnConfigChanged;
            timeSettings.PropertyChanged += this.OnConfigChanged;

            this.InitLayout();

            this._noteBoard = new NoteBoard(this._monitor, this.ScissorRectangle, this, this._config, this._timeSettings, this._notes, this._tickLength, this._noteHeight, this._actionManager, this._cursorRenderer);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            this._verticalSeperatorsHelper.Update(gameTime);
            this._noteBoard.Update(gameTime);
        }

        protected override void DrawScrollContent(SpriteBatch b)
        {
            foreach (_Rectangle rect in this._noteRects)
                rect.Draw(b);
            foreach (_Line line in this._noteSeperators)
                line.Draw(b);
            this._verticalSeperatorsHelper.Draw(b);
            this._noteBoard.Draw(b);
        }

        protected override void DrawNonScrollContent(SpriteBatch b)
        { }

        private void OnConfigChanged(object sender, PropertyChangedEventArgs e)
        {
            string[] affectLayout = new string[]
            {
                nameof(this._timeSettings.TicksPerQuarterNote),
                nameof(this._timeSettings.TimeSignature.Numerator),
                nameof(this._timeSettings.TimeSignature.Denominator),
                nameof(this._config.PianoRoll.Grid)
            };

            if (affectLayout.Any(a => e.PropertyName == a))
            {
                this.InvalidateLayout();
            }
        }

        /// <summary>Invalid and redo layout.</summary>
        private void InvalidateLayout()
        {
            this._noteSeperators.Clear();
            this._noteRects.Clear();
            this.InitLayout();
            this.Owner.InvalidateScrollInfo();
        }

        private void InitLayout()
        {
            // init background layout.

            // 横向的分割线。
            for (int n = 0; n < this._totalPitches + 1; n++)
            {
                _Line line = new _Line();
                line.Horizontal = true;
                line.LocalPosition
                    = new Vector2(this.ScissorRectangle.X, this.ScissorRectangle.Y + this.ExtentHeight - (n + 1) * this._noteHeight);
                line.Length = this.ExtentWidth;
                line.Thickness = 1;
                line.Color = Color.Brown;
                this._noteSeperators.Add(line);
            }

            // 横向的长条。
            for (int n = 0; n < this._totalPitches; n++)
            {
                _Rectangle r = new _Rectangle();
                r.LocalPosition
                    = new Vector2(this.ScissorRectangle.X, this.ScissorRectangle.Y + this.ExtentHeight - (n + 1) * this._noteHeight);
                r.Size = new Vector2(this.ExtentWidth, this._noteHeight);

                // 白键。
                if (n % 12 == 0 || n % 12 == 2 || n % 12 == 4 || n % 12 == 6 || n % 12 == 7 || n % 12 == 9 || n % 12 == 11)
                {
                    r.Stroke = r.Fill = /* Color.Yellow;*/new Color(248, 205, 154); // 白键淡一点
                }

                // 黑键。
                else
                {
                    r.Stroke = r.Fill =/*Color.YellowGreen;*/new Color(253, 188, 110); // 黑键深一点
                }

                this._noteRects.Add(r);
            }
        }

        internal Rectangle NoteAvailableBounds => this.ScissorRectangle;

        /// <summary>
        /// Gets the absolute ticks on the <see cref="NoteBoard"/> according to an absolute x position on screen.
        /// </summary>
        /// <param name="x">Absolute x on screen.</param>
        /// <param name="forceScrollIfOutOfBounds">Whether force this to scroll when the given <paramref name="x"/> is out of available bounds.</param>
        /// <returns>Ticks got.</returns>
        internal int GetNoteTicksAtXPos(int x, bool forceScrollIfOutOfBounds = true)
        {
            Rectangle availableBounds = this.ScissorRectangle;

            // out of bounds, scroll to fit.
            if (x < availableBounds.Left)
            {
                if (!forceScrollIfOutOfBounds)
                    x = availableBounds.X;
                else
                {
                    float pointX = MathHelper.Clamp(x - availableBounds.X + this.HorizontalOffset, 0, this.ExtentWidth);
                    this.MakeVisible(new Vector2(pointX, this.VerticalOffset));
                }
            }
            else if (x > availableBounds.Right)
            {
                if (!forceScrollIfOutOfBounds)
                    x = availableBounds.Right;
                else
                {
                    float pointX = MathHelper.Clamp(x - availableBounds.X + this.HorizontalOffset, 0, this.ExtentWidth);
                    this.MakeVisible(new Vector2(pointX, this.VerticalOffset));
                }
            }

            int localX = x + this.HorizontalOffset - availableBounds.X;
            int ticks = (int)(localX / this._tickLength);
            return ticks;
        }

        /// <summary>
        /// Gets the absolute ticks on the <see cref="NoteBoard"/> according to an absolute x position on screen.
        /// </summary>
        /// <param name="y">Absolute y on screen.</param>
        /// <param name="forceScrollIfOutOfBounds">Whether force this to scroll when the given <paramref name="y"/> is out of available bounds.</param>
        /// <returns>Ticks got.</returns>
        internal int GetNotePitchAtYPos(int y, bool forceScrollIfOutOfBounds = true)
        {
            Rectangle availableBounds = this.ScissorRectangle;

            // out of bounds, scroll to fit.
            if (y < availableBounds.Top)
            {
                if (!forceScrollIfOutOfBounds)
                    y = availableBounds.Top;
                else
                {
                    float pointY = MathHelper.Clamp(y - availableBounds.Y + this.VerticalOffset, 0, this.ExtentHeight);
                    this.MakeVisible(new Vector2(this.HorizontalOffset, pointY));
                }
            }
            else if (y > availableBounds.Bottom)
            {
                if (!forceScrollIfOutOfBounds)
                    y = availableBounds.Bottom;
                else
                {
                    float pointY = MathHelper.Clamp(y - availableBounds.Y + this.VerticalOffset, 0, this.ExtentHeight);
                    this.MakeVisible(new Vector2(this.HorizontalOffset, pointY));
                }
            }

            int localY = y + this.VerticalOffset - availableBounds.Y;
            int pitch = JConstants.MaxNoteNumber - (int)(localY / this._noteHeight);
            pitch = Math.Max(pitch, JConstants.MinNoteNumber);
            pitch = Math.Min(pitch, JConstants.MaxNoteNumber);
            return pitch;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">The source collection to search.</param>
        /// <param name="x">Absolute x on screen.</param>
        /// <param name="y">Absolute y on screen.</param>
        internal bool TryFindNoteAtPos(INoteCollection source, int x, int y, out INote note)
        {
            Rectangle availableBounds = this.ScissorRectangle;

            // out of bounds.
            if (!availableBounds.Contains(x, y))
            {
                note = null;
                return false;
            }

            int ticks = this.GetNoteTicksAtXPos(x, false);
            int pitch = this.GetNotePitchAtYPos(y, false);

            Func<INote, bool> match = n =>
               (n.Start <= ticks && ticks <= n.Start + n.Duration)
               && n.Number == pitch;
            note = source.LastOrDefault(match); // use last() to select the upper note if two or more notes are overlapped.
            return (note != null);
        }

        internal DisplayNote FindExistingDisplayNote(INote note)
        {
            return this._noteBoard.FindExistingDisplayNote(note);
        }
    }
}
