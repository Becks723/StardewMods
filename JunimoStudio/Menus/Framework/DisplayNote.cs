using System;
using System.ComponentModel;
using JunimoStudio.Menus.Controls;
using JunimoStudio.Core;
using JunimoStudio.Core.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using _Rectangle = JunimoStudio.Menus.Controls.Shapes.Rectangle;
using StardewValley;

namespace JunimoStudio.Menus.Framework
{
    /// <summary>在UI中显示的音符。</summary>
    internal class DisplayNote : Element
    {
        private readonly float _moveResizeSizeRatio = 2f;

        private readonly float _maxResizeSize = 20f;

        private readonly float _minResizeSize = 5f;

        /// <summary>底层的音符。</summary>
        private readonly INote _coreNote;

        private readonly ModConfig _config;

        private readonly ITimeBasedObject _timeSettings;

        /// <summary>调整音符的位置。</summary>
        private readonly Func<long, int, Vector2> _positioning;

        /// <summary>调整音符的宽高。</summary>
        private readonly Func<int, Vector2> _sizing;

        /// <summary>用来渲染音符外观。</summary>
        private readonly _Rectangle _rect;

        public override int Width => (int)this.Size.X;

        public override int Height => (int)this.Size.Y;

        public Vector2 Size { get; set; }

        /// <summary>底层的音符。</summary>
        public INote Core => this._coreNote;

        public Rectangle MoveBounds
        {
            get
            {
                Rectangle resizeBounds = this.ResizeBounds;

                return new Rectangle(
                    (int)this.Position.X,
                    (int)this.Position.Y,
                    Math.Max(this.Width, resizeBounds.X - (int)this.Position.X),
                    this.Height);
            }
        }

        public Rectangle ResizeBounds
        {
            get
            {
                if (this.Width / this._moveResizeSizeRatio > this._maxResizeSize)
                {
                    return new Rectangle(
                        (int)this.Position.X + this.Width - (int)this._maxResizeSize + 5,
                        (int)this.Position.Y,
                        (int)this._maxResizeSize + 5,
                        this.Height);
                }
                else if (this.Width / this._moveResizeSizeRatio < this._minResizeSize)
                {
                    return new Rectangle(
                        (int)this.Position.X + (int)(this._minResizeSize * this._moveResizeSizeRatio),
                        (int)this.Position.Y,
                        (int)this._minResizeSize,
                        this.Height);
                }
                else
                {
                    float resizeSize = this.Width / this._moveResizeSizeRatio;
                    return new Rectangle(
                        (int)this.Position.X + this.Width - (int)resizeSize + 3,
                        (int)this.Position.Y,
                        (int)resizeSize + 3,
                        this.Height);
                }
            }
        }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="note">The underlying note.</param>
        /// <param name="config">A config containing time settings.</param>
        /// <param name="positioning">A function for setting <see cref="LocalPosition"/>.</param>
        /// <param name="sizing">A function for setting <see cref="Size"/>.</param>
        public DisplayNote(INote note, ModConfig config, ITimeBasedObject timeSettings, Controls.Container parent,
            Func<long, int, Vector2> positioning, Func<int, Vector2> sizing)
        {
            this._coreNote = note;
            this._config = config;
            this._timeSettings = timeSettings;
            this._positioning = positioning;
            this._sizing = sizing;

            parent.AddChild(this);
            this._rect = new _Rectangle();
            this.InvalidateBoth();
            this._rect.Fill = Color.LightGreen;
            this._rect.Stroke = Color.LawnGreen;
            this._rect.StrokeThickness = 1;

            // register callbacks when config changes.
            // Since config and note are both ITimeBasedObject, so simply set corresponding properties of the ITimeBasedObject interface.
            timeSettings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(timeSettings.Bpm))
                    this._coreNote.Bpm = timeSettings.Bpm;
                else if (e.PropertyName == nameof(timeSettings.TicksPerQuarterNote))
                    this._coreNote.TicksPerQuarterNote = timeSettings.TicksPerQuarterNote;
                else if (e.PropertyName == nameof(timeSettings.TimeSignature.Numerator))
                    this._coreNote.TimeSignature.Numerator = timeSettings.TimeSignature.Numerator;
                else if (e.PropertyName == nameof(timeSettings.TimeSignature.Denominator))
                    this._coreNote.TimeSignature.Denominator = timeSettings.TimeSignature.Denominator;
            };

            note.PropertyChanged += this.OnNotePropertyChanged;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch b)
        {
            this._rect.Draw(b);
        }

        /// <summary>
        /// If note has a notification ability, this method is for register callbacks when note properties changes.
        /// Else, see contents in <see cref="Update(bool)"/> method instead.
        /// </summary>
        private void OnNotePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(INote.TimeSignature.Denominator):
                case nameof(INote.TimeSignature.Numerator):
                    this.InvalidateBoth();
                    break;
                case nameof(INote.Start):
                case nameof(INote.Number):
                    this.InvalidatePosition();
                    break;
                case nameof(INote.Duration):
                    this.InvalidateSize();
                    break;
            }
        }

        private void InvalidatePosition()
        {
            this.LocalPosition = this._positioning(this._coreNote.Start, this._coreNote.Number);
            this._rect.LocalPosition = this.Position;
        }

        private void InvalidateSize()
        {
            this.Size = this._sizing(this._coreNote.Duration);
            this._rect.Size = this.Size;
        }

        private void InvalidateBoth()
        {
            this.InvalidatePosition();
            this.InvalidateSize();
        }
    }
}
