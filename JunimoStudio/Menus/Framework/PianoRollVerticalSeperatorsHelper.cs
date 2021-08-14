using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JunimoStudio.Core;
using JunimoStudio.Core.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using _Line = JunimoStudio.Menus.Controls.Shapes.Line;


namespace JunimoStudio.Menus.Framework
{
    /// <summary>A helper class for drawing vertical lines (i.e., bar and beat seperators) on piano roll.</summary>
    internal class PianoRollVerticalSeperatorsHelper
    {
        /// <summary>The length of a tick.</summary>
        private readonly float _tickLength;

        private readonly ModConfig _config;

        private readonly ITimeBasedObject _timeSettings;

        private readonly Rectangle _bounds;

        private readonly Color _barSeperatorPaint;

        private readonly Color _beatSeperatorPaint;

        /// <summary>Vertical lines seperating bars.</summary>
        private readonly List<_Line> _barSeperators = new List<_Line>();

        /// <summary>Vertical lines seperating beats.</summary>
        private readonly List<_Line> _beatSeperators = new List<_Line>();

        /// <summary>Vertical lines seperating half beats.</summary>
        private readonly List<_Line> _halfBeatSeperators = new List<_Line>();

        /// <summary>Vertical lines seperating 1/3 beats.</summary>
        private readonly List<_Line> _oneThirdBeatSeperators = new List<_Line>();

        /// <summary>Vertical lines seperating 1/4 beats.</summary>
        private readonly List<_Line> _quarterBeatSeperators = new List<_Line>();

        /// <summary>Vertical lines seperating 1/6 beats.</summary>
        private readonly List<_Line> _oneSixthBeatSeperators = new List<_Line>();

        public PianoRollVerticalSeperatorsHelper(Rectangle bounds, ModConfig config, ITimeBasedObject timeSettings, float tickLength)
        {
            _bounds = bounds;
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _timeSettings = timeSettings;
            _tickLength = tickLength;

            _barSeperatorPaint = Color.Black;
            _beatSeperatorPaint = Color.Brown;

            config.PianoRoll.PropertyChanged += OnConfigChanged;
            timeSettings.PropertyChanged += OnConfigChanged;

            Init();
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(SpriteBatch b)
        {
            foreach (_Line line in _barSeperators)
                line.Draw(b);
            foreach (_Line line in _beatSeperators)
                line.Draw(b);
            foreach (_Line line in _halfBeatSeperators)
                line.Draw(b);
            foreach (_Line line in _oneThirdBeatSeperators)
                line.Draw(b);
            foreach (_Line line in _quarterBeatSeperators)
                line.Draw(b);
            foreach (_Line line in _oneSixthBeatSeperators)
                line.Draw(b);
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
            _barSeperators.Clear();
            _beatSeperators.Clear();
            _halfBeatSeperators.Clear();
            _oneThirdBeatSeperators.Clear();
            _quarterBeatSeperators.Clear();
            _oneSixthBeatSeperators.Clear();
            Init();
        }

        private void Init()
        {
            // we need to know piano roll's grid resolution setting to decide whether to draw more accurate time scale.
            GridResolution grid = _config.PianoRoll.Grid;

            // init vertical seperators between every two bars.
            {
                for (int bar = 0; bar < 3; bar++)
                {
                    _Line line = new _Line();
                    line.LocalPosition = new Vector2(
                        _bounds.X + TimeLengthHelper.GetBarLength(_timeSettings, _tickLength) * bar,
                        _bounds.Y);
                    line.Horizontal = false;
                    line.Length = _bounds.Height;
                    line.Thickness = 3;
                    line.Color = _barSeperatorPaint;
                    _barSeperators.Add(line);
                }
            }

            if (grid == GridResolution.Bar)
                return;

            // init vertical seperators between every two beats.
            {
                int count = (_barSeperators.Count - 1) * _timeSettings.TimeSignature.Numerator;
                for (int beat = 0; beat < count; beat++)
                {
                    if (beat % _timeSettings.TimeSignature.Numerator == 0)
                        continue;
                    _Line line = new _Line();
                    line.LocalPosition = new Vector2(
                        _bounds.X + TimeLengthHelper.GetBeatLength(_timeSettings, _tickLength) * beat,
                        _bounds.Y);
                    line.Horizontal = false;
                    line.Length = _bounds.Height;
                    line.Thickness = 2;
                    line.Color = _beatSeperatorPaint;
                    _beatSeperators.Add(line);
                }
            }

            if (grid == GridResolution.Beat)
                return;

            // init vertical seperators between every two half-beats.
            {
                if (grid != GridResolution.OneThirdBeat)
                {
                    int count = (_barSeperators.Count - 1) * _timeSettings.TimeSignature.Numerator * 2;
                    for (int half = 0; half < count; half++)
                    {
                        if (half % 2 == 0)
                            continue;
                        _Line line = new _Line();
                        line.LocalPosition = new Vector2(
                            _bounds.X + (float)TimeLengthHelper.GetBeatLength(_timeSettings, _tickLength) / 2 * half,
                            _bounds.Y);
                        line.Horizontal = false;
                        line.Length = _bounds.Height;
                        line.Thickness = 1;
                        line.Color = _beatSeperatorPaint;
                        _halfBeatSeperators.Add(line);
                    }
                }
            }

            if (grid == GridResolution.HalfBeat)
                return;

            // init vertical seperators between every two 1/3 beats.
            {
                if (grid != GridResolution.HalfBeat
                    && grid != GridResolution.QuarterBeat)
                {
                    int count = (_barSeperators.Count - 1) * _timeSettings.TimeSignature.Numerator * 3;
                    for (int oneThrid = 0; oneThrid < count; oneThrid++)
                    {
                        if (oneThrid % 3 == 0)
                            continue;
                        _Line line = new _Line();
                        line.LocalPosition = new Vector2(
                            _bounds.X + (float)TimeLengthHelper.GetBeatLength(_timeSettings, _tickLength) / 3 * oneThrid,
                            _bounds.Y);
                        line.Horizontal = false;
                        line.Length = _bounds.Height;
                        line.Thickness = 1;
                        line.Color = _beatSeperatorPaint;
                        _oneThirdBeatSeperators.Add(line);
                    }
                }
            }

            if (grid == GridResolution.OneThirdBeat)
                return;

            // init vertical seperators between every two 1/4 beats.
            {
                if (grid != GridResolution.OneSixthBeat)
                {
                    int count = (_barSeperators.Count - 1) * _timeSettings.TimeSignature.Numerator * 4;
                    for (int quarter = 0; quarter < count; quarter++)
                    {
                        if (quarter % 4 == 0)
                            continue;
                        _Line line = new _Line();
                        line.LocalPosition = new Vector2(
                            _bounds.X + (float)TimeLengthHelper.GetBeatLength(_timeSettings, _tickLength) / 4 * quarter,
                            _bounds.Y);
                        line.Horizontal = false;
                        line.Length = _bounds.Height;
                        line.Thickness = 1;
                        line.Color = _beatSeperatorPaint;
                        _quarterBeatSeperators.Add(line);
                    }
                }
            }

            if (grid == GridResolution.QuarterBeat)
                return;

            // init vertical seperators between every two 1/6 beats.
            {
                int count = (_barSeperators.Count - 1) * _timeSettings.TimeSignature.Numerator * 6;
                for (int oneSixth = 0; oneSixth < count; oneSixth++)
                {
                    if (oneSixth % 6 == 0)
                        continue;
                    _Line line = new _Line();
                    line.LocalPosition = new Vector2(
                        _bounds.X + (float)TimeLengthHelper.GetBeatLength(_timeSettings, _tickLength) / 6 * oneSixth,
                        _bounds.Y);
                    line.Horizontal = false;
                    line.Length = _bounds.Height;
                    line.Thickness = 1;
                    line.Color = _beatSeperatorPaint;
                    _oneSixthBeatSeperators.Add(line);
                }
            }

            if (grid == GridResolution.OneSixthBeat)
                return;

        }
    }
}
