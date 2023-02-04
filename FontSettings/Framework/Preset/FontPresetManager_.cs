using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

#nullable enable

namespace FontSettings.Framework.Preset
{
    internal class FontPresetManager_ : IFontPresetManager
    {
        private readonly IDictionary<string, FontPresetReal> _keyedPresets = new Dictionary<string, FontPresetReal>();

        private readonly IList<FontPresetReal> _presets = new List<FontPresetReal>();

        private readonly IList<FontPresetReal> _readonlyPresets = new List<FontPresetReal>();

        public event EventHandler<PresetUpdatedEventArgs>? PresetUpdated;

        public FontPresetManager_(IEnumerable<FontPresetReal> presets)
        {
            foreach (var preset in presets)
            {
                if (preset is IPresetWithKey<string> withKey)
                    this._keyedPresets[withKey.Key] = preset;

                this._presets.Add(preset);
            }
        }

        public IEnumerable<FontPresetReal> GetPresets(LanguageInfo language, GameFontType fontType)
        {
            return from preset in this._presets
                   where preset.Language == language && preset.FontType == fontType
                   select preset;
        }

        public bool IsReadOnlyPreset(FontPresetReal preset)
        {
            return !this._keyedPresets.Values.Contains(preset);
        }

        public bool IsValidPresetName(string? name, out InvalidPresetNameTypes? invalidType)
        {
            invalidType = null;
            if (string.IsNullOrWhiteSpace(name))
                invalidType = InvalidPresetNameTypes.EmptyName;

            else if (this.ContainsInvalidChar(name))
                invalidType = InvalidPresetNameTypes.ContainsInvalidChar;

            else if (this.DuplicatePresetName(name))
                invalidType = InvalidPresetNameTypes.DuplicatedName;

            return invalidType == null;
        }

        void IFontPresetManager.UpdatePreset(string name, FontPresetReal? preset)
        {
            if (!this._keyedPresets.ContainsKey(name))
            {
                if (preset != null)
                {
                    this._keyedPresets.Add(name, preset);
                    this.RaisePresetUpdated(name, preset);
                }
            }
            else
            {
                if (preset != null)
                {
                    this._keyedPresets[name] = preset;
                    this.RaisePresetUpdated(name, this._keyedPresets[name]);
                }
                else
                {
                    this._keyedPresets.Remove(name);
                    this.RaisePresetUpdated(name, null);
                }
            }
        }

        private bool ContainsInvalidChar(string name)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in name)
            {
                if (invalidChars.Contains(c))
                    return true;
            }

            return false;
        }

        private bool DuplicatePresetName(string name)
        {
            return this._keyedPresets.ContainsKey(name);
        }

        private void RaisePresetUpdated(string name, FontPresetReal? preset)
        {
            this.RaisePresetUpdated(
                new PresetUpdatedEventArgs(name, preset));
        }

        protected virtual void RaisePresetUpdated(PresetUpdatedEventArgs e)
        {
            PresetUpdated?.Invoke(this, e);
        }
    }

    internal class PresetUpdatedEventArgs : EventArgs
    {
        public string Name { get; }

        public FontPresetReal? Preset { get; }

        public PresetUpdatedEventArgs(string name, FontPresetReal? preset)
        {
            this.Name = name;
            this.Preset = preset;
        }
    }
}
