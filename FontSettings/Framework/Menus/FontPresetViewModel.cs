using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.Menus
{
    internal class FontPresetViewModel
    {
        private readonly FontPresetManager _presetManager;
        private readonly FontPresetFontType _targetFontType;

        public FontPreset? CurrentPreset { get; set; }

        public event EventHandler PresetChanged;

        public FontPresetViewModel(FontPresetManager presetManager, FontPresetFontType targetFontType)
        {
            this._presetManager = presetManager;
            this._targetFontType = targetFontType;
        }

        public void MoveToPreviousPreset()
        {
            var presets = this.GetAvailablePresets();
            this.CurrentPreset = GetPreviousItem(presets, this.CurrentPreset, ComparePresets);

            this.RaisePresetChanged(EventArgs.Empty);
        }

        public void MoveToNextPreset()
        {
            var presets = this.GetAvailablePresets();
            this.CurrentPreset = GetNextItem(presets, this.CurrentPreset, ComparePresets);

            this.RaisePresetChanged(EventArgs.Empty);
        }

        public void DeleteCurrentPreset()
        {
            if (this.CurrentPreset == null) return;

            var presets = this.GetAvailablePresets();
            FontPreset? next = GetNextItem(presets, this.CurrentPreset,
                ComparePresets);
            this._presetManager.RemovePreset(this.CurrentPreset.Name);

            this.CurrentPreset = next;

            this.RaisePresetChanged(EventArgs.Empty);
        }

        public void SaveCurrentPreset(string fontFileName, int fontIndex, float fontSize, float spacing, int lineSpacing, float offsetX, float offsetY)
        {
            this._presetManager.EditPreset(this.CurrentPreset, fontFileName, fontIndex, fontSize, spacing, lineSpacing, offsetX, offsetY);
        }

        public void SaveCurrentAsNewPreset(string presetName, string fontFileName, int fontIndex, float fontSize, float spacing, int lineSpacing, float offsetX, float offsetY)
        {
            FontPreset newPreset = new FontPreset
            {
                Name = presetName,
                Requires = new FontPresetPrecondition
                {
                    FontFileName = fontFileName,
                },
                FontIndex = fontIndex,
                FontSize = fontSize,
                Spacing = spacing,
                LineSpacing = lineSpacing,
                CharOffsetX = offsetX,
                CharOffsetY = offsetY,
                FontType = _targetFontType,
                Lang = StardewValley.LocalizedContentManager.CurrentLanguageCode,
                Locale = FontHelpers.GetCurrentLocale()
            };
            this._presetManager.AddPreset(newPreset);
        }

        protected virtual void RaisePresetChanged(EventArgs e)
        {
            PresetChanged?.Invoke(this, e);
        }

        /// <summary>获取当前上下文所有可用的预设。第一个是 无预设 ，即null。</summary>
        private FontPreset?[] GetAvailablePresets()
        {
            var result = new List<FontPreset>();
            result.Add(null);
            result.AddRange(this._presetManager.GetAllUnder(this._targetFontType, FontHelpers.GetCurrentLanguage()));

            return result.ToArray();
        }

        private static bool ComparePresets(FontPreset? x, FontPreset? y)
        {
            return new FontPresetComparer().Equals(x, y);
        }

        private static T? GetPreviousItem<T>(T?[] array, T? item, Func<T?, T?, bool> comparer)
        {
            int index = Array.FindIndex(array, x => comparer(item, x));
            if (index == -1)
            {
                if (item == null)
                {
                    if (array.Length > 0)
                        return array[array.Length - 1];
                    else
                        throw new ArgumentOutOfRangeException(nameof(array), "数组长度为零。");
                }
                else
                    throw new KeyNotFoundException();
            }

            int prevIndex = index - 1;
            if (prevIndex < 0)
                prevIndex = array.Length - 1;

            return array[prevIndex];
        }

        private static T? GetNextItem<T>(T?[] array, T? item, Func<T?, T?, bool> comparer)
        {
            int index = Array.FindIndex(array, x => comparer(item, x));
            if (index == -1)
            {
                if (item == null)
                {
                    if (array.Length > 0)
                        return array[0];
                    else
                        throw new ArgumentOutOfRangeException(nameof(array), "数组长度为零。");
                }
                else
                    throw new KeyNotFoundException();
            }

            int nextIndex = index + 1;
            if (nextIndex > array.Length - 1)
                nextIndex = 0;

            return array[nextIndex];
        }
    }
}
