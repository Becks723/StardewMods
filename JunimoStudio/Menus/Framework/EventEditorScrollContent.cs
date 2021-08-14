using System.Collections.Generic;
using System.Linq;
using JunimoStudio.Menus.Controls;
using JunimoStudio.Core.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using _Line = JunimoStudio.Menus.Controls.Shapes.Line;
using StardewValley.Menus;
using System.ComponentModel;
using JunimoStudio.Core;

namespace JunimoStudio.Menus.Framework
{
    internal class EventEditorScrollContent : ScrollContentBase
    {
        private readonly PianoRollMainScrollContent _main;

        /// <summary>The length of a tick.</summary>
        private readonly float _tickLength;

        /// <summary>The width of the standing keyboard.</summary>
        private readonly float _keyboardWidth;

        private readonly PianoRollVerticalSeperatorsHelper _verticalSeperatorsHelper;

        private readonly ModConfig _config;

        private readonly ITimeBasedObject _timeSettings;

        private readonly List<_Line> _horizontalSeperators = new List<_Line>();

        public override int ExtentWidth => _main.ExtentWidth;

        public override int ExtentHeight => Owner.Height - 32;

        public override int ViewportWidth => Owner.Width - 32 - (int)_keyboardWidth;

        public override int ViewportHeight => Owner.Height - 32;

        protected override Rectangle ScissorRectangle
            => new Rectangle((int)Position.X + 16 + (int)_keyboardWidth, (int)Position.Y + 16, ViewportWidth, ViewportHeight);

        public EventEditorScrollContent(ScrollViewer owner, PianoRollMainScrollContent main, float tickLength, float keyboardWidth, ModConfig config, ITimeBasedObject timeSettings)
            : base(owner)
        {
            _main = main;
            _tickLength = tickLength;
            _keyboardWidth = keyboardWidth;
            _config = config;
            _timeSettings = timeSettings;
            _verticalSeperatorsHelper = new PianoRollVerticalSeperatorsHelper(
                new Rectangle(ScissorRectangle.X, ScissorRectangle.Y, ExtentWidth, ExtentHeight), _config, _timeSettings, _tickLength);

            config.PianoRoll.PropertyChanged += OnConfigChanged;
            timeSettings.PropertyChanged += OnConfigChanged;

            InitLayout();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _verticalSeperatorsHelper.Update(gameTime);
        }

        protected override void DrawNonScrollContent(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, (int)Position.X, (int)Position.Y, (int)_keyboardWidth + 16, Owner.Height, Color.White);
        }

        protected override void DrawScrollContent(SpriteBatch b)
        {
            foreach (_Line line in _horizontalSeperators)
            {
                line.Draw(b);
            }

            _verticalSeperatorsHelper.Draw(b);
        }

        private void OnConfigChanged(object sender, PropertyChangedEventArgs e)
        {
            string[] affectLayout = new string[]
            {
                nameof(_timeSettings.TicksPerQuarterNote),
                nameof(_timeSettings.TimeSignature.Numerator),
                nameof(_timeSettings.TimeSignature.Denominator),
                nameof(_config.PianoRoll.Grid)
            };

            if (affectLayout.Any(a => e.PropertyName == a))
            {
                InvalidateLayout();
            }
        }

        private void InvalidateLayout()
        {
            _horizontalSeperators.Clear();
            InitLayout();
            Owner.InvalidateScrollInfo();
        }

        private void InitLayout()
        {
            float interval = ExtentHeight / 8f;
            for (int i = 0; i < 8; i++)
            {
                _Line line = new _Line();
                line.Horizontal = true;
                line.LocalPosition = new Vector2(ScissorRectangle.X, ScissorRectangle.Y + i * interval);
                line.Length = ExtentWidth;
                line.Thickness = 1;
                line.Color = Color.SandyBrown;
                _horizontalSeperators.Add(line);
            }
        }
    }
}
