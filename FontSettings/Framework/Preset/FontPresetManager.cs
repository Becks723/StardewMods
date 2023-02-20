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
    internal class FontPresetManager : IFontPresetManager
    {
        private readonly IDictionary<string, FontPreset> _keyedPresets = new Dictionary<string, FontPreset>();

        private readonly IList<FontPreset> _presets = new List<FontPreset>();

        private readonly IList<FontPreset> _readonlyPresets = new List<FontPreset>();

        public event EventHandler<PresetUpdatedEventArgs>? PresetUpdated;

        public FontPresetManager()
        {
        }

        public FontPresetManager(IEnumerable<FontPreset> presets)
        {
            this.AddPresets(presets);
        }

        public IEnumerable<FontPreset> GetPresets(LanguageInfo language, GameFontType fontType)
        {
            return from preset in this.GetAllPresets()
                   where preset.Language == language && preset.FontType == fontType
                   select preset;
        }

        public bool IsReadOnlyPreset(FontPreset preset)
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

        /// <summary>Won't raise <see cref="PresetUpdated"/>.</summary>
        public void AddPresets(IEnumerable<FontPreset> presets)
        {
            foreach (var preset in presets)
            {
                if (preset.Supports<IPresetWithKey<string>>())
                {
                    var withKey = preset.GetInstance<IPresetWithKey<string>>();
                    this._keyedPresets[withKey.Key] = preset;
                }
            }
        }

        void IFontPresetManager.UpdatePreset(string name, FontPreset? preset)
        {
            this.UpdatePreset(name, preset, raisePresetUpdated: true);
        }

        private void UpdatePreset(string name, FontPreset? preset, bool raisePresetUpdated)
        {
            if (!this._keyedPresets.ContainsKey(name))
            {
                if (preset != null)
                {
                    this._keyedPresets.Add(name, preset);

                    if (raisePresetUpdated)
                        this.RaisePresetUpdated(name, preset);
                }
            }
            else
            {
                if (preset != null)
                {
                    this._keyedPresets[name] = preset;

                    if (raisePresetUpdated)
                        this.RaisePresetUpdated(name, this._keyedPresets[name]);
                }
                else
                {
                    this._keyedPresets.Remove(name);

                    if (raisePresetUpdated)
                        this.RaisePresetUpdated(name, null);
                }
            }
        }

        private IEnumerable<FontPreset> GetAllPresets()
        {
            foreach (var preset in this._keyedPresets.Values)
                yield return preset;
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

        private void RaisePresetUpdated(string name, FontPreset? preset)
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

        public FontPreset? Preset { get; }

        public PresetUpdatedEventArgs(string name, FontPreset? preset)
        {
            this.Name = name;
            this.Preset = preset;
        }
    }
}
