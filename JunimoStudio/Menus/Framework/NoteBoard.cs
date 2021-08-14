using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using JunimoStudio.Menus.Controls;
using JunimoStudio.Core;
using Microsoft.Xna.Framework;
using JunimoStudio.Input;
using JunimoStudio.Input.Gestures;
using GuiLabs.Undo;
using StardewValley;
using JunimoStudio.Core.Utilities;
using StardewModdingAPI;
using JunimoStudio.Actions.PianoRollActions;
using JunimoStudio.Menus.Framework.Functions.MouseFunctions;
using JConstants = JunimoStudio.Core.Constants;

namespace JunimoStudio.Menus.Framework
{
    /// <summary>A board covers the piano roll where all display notes live. It provides basic gestures for editing notes.</summary>
    internal class NoteBoard : Container
    {
        private readonly IMonitor _monitor;

        private readonly ModConfig _config;

        private readonly ITimeBasedObject _timeSettings;

        private readonly INoteCollection _notes;

        private readonly List<DisplayNote> _displayNotes = new List<DisplayNote>();

        private readonly float _tickLength;

        private readonly float _noteHeight;

        private readonly PianoRollMainScrollContent _pianoRoll;

        private readonly ICursorRenderer _cursorRenderer;

        private Cursors _currentCursor;

        private int _noteDurationCached;

        private readonly ActionManager _actionManager;

        private readonly PianoRollAddNoteFunction _addNoteFunction;

        private readonly PianoRollDeleteNoteFunction _deleteNoteFunction;

        private readonly PianoRollMoveNoteFunction _moveNoteFunction;

        private readonly PianoRollResizeNoteFunction _resizeNoteFunction;

        private MouseDoubleClickGesture _openNotePropertiesPopUp;

        public Point Offset => new Point(this._pianoRoll.HorizontalOffset, this._pianoRoll.VerticalOffset);

        public int ExtentWidth => this._pianoRoll.ExtentWidth;

        public int ExtentHeight => this._pianoRoll.ExtentHeight;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bounds">An absolute bounds of the noteboard. (abs x, abs y, extent width, extent height)</param>
        /// <param name="pianoRoll">Used to set parent <see cref="Container"/> of the noteboard.</param>
        /// <param name="config"></param>
        /// <param name="notes"></param>
        /// <param name="tickLength"></param>
        /// <param name="noteHeight"></param>
        /// <param name="actionManager"></param>
        public NoteBoard(IMonitor monitor, Rectangle bounds, PianoRollMainScrollContent pianoRoll, ModConfig config, ITimeBasedObject timeSettings, INoteCollection notes, float tickLength, float noteHeight, ActionManager actionManager, ICursorRenderer cursorRenderer)
        {
            this._monitor = monitor;
            this.LocalPosition = new Vector2(bounds.X, bounds.Y);
            this.Width = bounds.Width;
            this.Height = bounds.Height;
            this._pianoRoll = pianoRoll;

            this._config = config;
            this._timeSettings = timeSettings;
            this._notes = notes;
            this._tickLength = tickLength;
            this._noteHeight = noteHeight;
            this._actionManager = actionManager;
            this._cursorRenderer = cursorRenderer;

            this._noteDurationCached = timeSettings.TicksPerQuarterNote;
            this._openNotePropertiesPopUp = new MouseDoubleClickGesture(MouseButton.Left);
            this._openNotePropertiesPopUp.DoubleClicked += (s, e) =>
            {

            };

            notes.CollectionChanged += this.OnNoteCollectionChanged;

            // init existing notes.
            foreach (INote note in notes)
            {
                this._displayNotes.Add(new DisplayNote(note, config, this._timeSettings, this, this.GetNotePosition, this.GetNoteSize));
            }

            this._addNoteFunction = new PianoRollAddNoteFunction(this._actionManager, this._pianoRoll, this._timeSettings, this._notes, this._displayNotes);
            this._deleteNoteFunction = new PianoRollDeleteNoteFunction(this._actionManager, this._pianoRoll, this._notes, this._displayNotes);
            this._moveNoteFunction = new PianoRollMoveNoteFunction(this._actionManager, this._pianoRoll, this._displayNotes, () => this._addNoteFunction.Adding);
            this._resizeNoteFunction = new PianoRollResizeNoteFunction(this._actionManager, this._pianoRoll, this._displayNotes);
        }

        public override int Width { get; }

        public override int Height { get; }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var editors = this.GetType()
                 .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                 .Where(f => typeof(PianoRollBaseMouseFunction).IsAssignableFrom(f.FieldType))
                 .Select(f => (PianoRollBaseMouseFunction)f.GetValue(this));
            foreach (var ed in editors)
                ed?.Update(gameTime);

            //if (_moveNoteEditor.Moving || _resizeNoteEditor.Resizing)
            //    _addNoteEditor.SuppressUpdate();
            //else
            //    _addNoteEditor.Update(gameTime);

            //_deleteNoteEditor.Update(gameTime);

            //if (_addNoteEditor.Adding || _resizeNoteEditor.Resizing)
            //    _moveNoteEditor.SuppressUpdate();
            //else
            //    _moveNoteEditor.Update(gameTime);

            //if (_addNoteEditor.Adding || _moveNoteEditor.Moving)
            //    _resizeNoteEditor.SuppressUpdate();
            //else
            //    _resizeNoteEditor.Update(gameTime);


            //Point mousePos = Game1.getMousePosition();

            //if ((_moveNoteEditor.CanMove(mousePos) || _moveNoteEditor.Moving) && !_resizeNoteEditor.Resizing)
            //{
            //    _cursorRenderer.Current = Cursors.Move;
            //}
            //else if ((_resizeNoteEditor.CanResize(mousePos) || _resizeNoteEditor.Resizing) && !_moveNoteEditor.Moving)
            //{
            //    _cursorRenderer.Current = Cursors.Size3;
            //}
            //else
            //{
            //    _cursorRenderer.Current = Cursors.Arrow;
            //}
            Point mousePos = Game1.getMousePosition();

            this._currentCursor = this.CursorTypeAtPos(mousePos);
            this._cursorRenderer.Current = this._currentCursor;

            foreach (DisplayNote note in this._displayNotes)
                note.Update(gameTime);
        }

        private void OnNoteCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // note added.
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    INote note = (INote)item;
                    DisplayNote displayNote
                        = new DisplayNote(note, this._config, this._timeSettings, this, this.GetNotePosition, this.GetNoteSize);
                    this._displayNotes.Add(displayNote);
                }
            }

            // note removed.
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in e.OldItems)
                {
                    INote note = (INote)item;
                    DisplayNote toRemove
                        = this.FindExistingDisplayNote(note);
                    this.RemoveChild(toRemove);
                    this._displayNotes.Remove(toRemove);
                }
            }
        }

        /// <summary>Helper to get a note position in a <see cref="NoteBoard"/>.</summary>
        private Vector2 GetNotePosition(long startTime, int pitch)
        {
            if (pitch < JConstants.MinNoteNumber || pitch > JConstants.MaxNoteNumber)
                throw new ArgumentOutOfRangeException(nameof(pitch));

            float x, y;
            x = startTime * this._tickLength;
            y = (JConstants.MaxNoteNumber - pitch) * this._noteHeight;
            return new Vector2(x, y);
        }

        /// <summary>Helper to get a note width and height in a <see cref="NoteBoard"/>.</summary>
        private Vector2 GetNoteSize(int duration)
        {
            return new Vector2(duration * this._tickLength, this._noteHeight);
        }

        private bool IsWithinViewport(Rectangle rect)
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">The source collection to search.</param>
        /// <param name="x">Absolute x on screen.</param>
        /// <param name="y">Absolute y on screen.</param>
        private bool TryFindNoteAtPos(INoteCollection source, int x, int y, out INote note)
        {
            // out of bounds.
            if (!this.Bounds.Contains(x, y))
            {
                note = null;
                return false;
            }

            int ticksAtPos = this.GetTicksByXPos(x);
            int pitch = this.GetPitchByYPos(y);

            Func<INote, bool> match = n =>
               (n.Start <= ticksAtPos && ticksAtPos <= n.Start + n.Duration)
               && n.Number == pitch;
            note = source.LastOrDefault(match); // use last() to select the upper note if two or more notes are overlapped.
            return (note != null);
        }

        /// <summary>
        /// Gets the absolute ticks on the <see cref="NoteBoard"/> according to an absolute x position on screen.
        /// </summary>
        /// <param name="x">Absolute x on screen.</param>
        /// <returns>Ticks got.</returns>
        private int GetTicksByXPos(int x)
        {
            // out of bounds, scroll to fit.
            if (x < this.Bounds.Left)
            {
                if (this.Offset.X == 0)
                    return 0;

                this._pianoRoll.MakeVisible(new Vector2(x, this.Position.Y + this.Offset.Y));
            }
            if (x > this.Bounds.Right)
            {
                if (this.Offset.X == this.ExtentWidth - this.Width)
                    return TimeConvert.FromBarsToTicks(2, this._timeSettings);

                this._pianoRoll.MakeVisible(new Vector2(x, this.Position.Y + this.Offset.Y));
            }

            int localX = x + this.Offset.X - (int)this.Position.X;
            int ticks = (int)(localX / this._tickLength);
            return ticks;
        }

        ///// <summary>
        ///// Gets the absolute ticks on the <see cref="NoteBoard"/> after snapping to grid, according to an absolute x position on screen.
        ///// </summary>
        ///// <param name="x">Absolute x on screen.</param>
        ///// <param name="ticks"></param>
        ///// <returns></returns>
        //private bool TryGetAlignedTicksByXPos(int x, out int ticks)
        //{
        //    if (!GetTicksByXPos(x, out ticks)) // here we get the ticks that is accurate.
        //        return false;

        //    // we need to get ticks of the grid that is exactly ahead of the accurate ticks.
        //    GridResolution grid = _config.PianoRoll.Grid;
        //    int tpq = _config.TicksPerQuarterNote;
        //    return true;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="y">Absolute y on screen.</param>
        /// <returns>Pitch according to mouse y. See <see cref="INote.Number"/>.</returns>
        private int GetPitchByYPos(int y)
        {
            int minPitch = JConstants.MinNoteNumber;
            int maxPitch = JConstants.MaxNoteNumber;

            // out of bounds, scroll to fit.
            if (y < this.Bounds.Top)
            {
                if (this.Offset.Y == 0)
                    return maxPitch;

                this._pianoRoll.MakeVisible(new Vector2(this.Position.X + this.Offset.X, y));
            }
            if (y > this.Bounds.Bottom)
            {
                if (this.Offset.Y == this.ExtentHeight - this.Height)
                    return minPitch;

                this._pianoRoll.MakeVisible(new Vector2(this.Position.X + this.Offset.X, y));
            }

            int localY = y + this.Offset.Y - (int)this.Position.Y;
            int pitch = maxPitch - (int)(localY / this._noteHeight);
            pitch = Math.Max(pitch, minPitch);
            pitch = Math.Min(pitch, maxPitch);
            return pitch;
        }

        internal DisplayNote FindExistingDisplayNote(INote note)
        {
            return this._displayNotes.FirstOrDefault(n => object.ReferenceEquals(n.Core, note));
        }

        private Cursors CursorTypeAtPos(Point mousePos)
        {
            if (this._addNoteFunction.Adding || this._moveNoteFunction.Moving)
            {
                return Cursors.Move;
            }
            else if (this._resizeNoteFunction.Resizing)
            {
                return Cursors.Size3;
            }

            Point extentMousePos = new Point(
                mousePos.X + this._pianoRoll.HorizontalOffset,
                mousePos.Y + this._pianoRoll.VerticalOffset);

            // arrow.
            // 1. out of note bounds.
            // 2. not hover on any note.
            if (!this._pianoRoll.NoteAvailableBounds.Contains(mousePos)
                || this._displayNotes.All(n => !n.MoveBounds.Contains(extentMousePos) && !n.ResizeBounds.Contains(extentMousePos)))
            {
                return Cursors.Arrow;
            }

            // move.
            // 1. within any note's move bounds, but out of its resize bounds.
            else if (this._displayNotes.Any(n => n.MoveBounds.Contains(extentMousePos) && !n.ResizeBounds.Contains(extentMousePos)))
            {
                return Cursors.Move;
            }

            // sizeWE.
            // 1. within any note's resize bounds.
            else if (this._displayNotes.Any(n => n.ResizeBounds.Contains(extentMousePos)))
            {
                return Cursors.Size3;
            }

            return Cursors.Arrow;
        }
    }
}
