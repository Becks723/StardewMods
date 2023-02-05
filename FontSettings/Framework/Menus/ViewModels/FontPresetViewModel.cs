using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;
using FontSettings.Framework.Preset;
using StardewValleyUI.Mvvm;

namespace FontSettings.Framework.Menus.ViewModels
{
    internal class FontPresetViewModel : MenuModelBase
    {
        private readonly IFontPresetManager _presetManager;
        private readonly GameFontType _fontType;
        private readonly FontSettingsMenuPresetContextModel _stagedValues;

        /// <summary>获取当前上下文所有可用的预设。第一个是 无预设 ，即null。</summary>
        private FontPreset[] _presets;
        private FontPreset[] Presets
        {
            get => this._presets;
            set => this.SetField(ref this._presets, value);
        }

        private FontPreset _currentPresetPrivate;
        private FontPreset CurrentPresetPrivate
        {
            get => this._currentPresetPrivate;
            set
            {
                this.SetField(ref this._currentPresetPrivate, value);

                this.RaisePropertyChanged(nameof(this.CurrentPresetName));
                this.RaisePropertyChanged(nameof(this.CurrentPreset));
            }
        }

        private bool NoPresetSelected => this.CurrentPresetPrivate == null;

        public string CurrentPresetName => this.CurrentPresetPrivate is IPresetWithName withName ? withName.Name
                                                                                                  : string.Empty;

        public FontConfig CurrentPreset => this.CurrentPresetPrivate?.Settings;

        public event EventHandler PresetChanged;

        public FontPresetViewModel(IFontPresetManager presetManager, GameFontType fontType, FontSettingsMenuPresetContextModel stagedValues)
        {
            this._presetManager = presetManager;
            this._fontType = fontType;
            this._stagedValues = stagedValues;

            this.RegisterCallbackToStageValues();

            this.UpdatePresets();

            // 填入之前记录的值。
            int index = this._stagedValues.PresetIndex;
            this.CurrentPresetPrivate = this.Presets.Length > index
                ? this.Presets[index]
                : null;
        }

        private void RegisterCallbackToStageValues()
        {
            this.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(this.Presets):
                    case nameof(this.CurrentPresetPrivate):
                        this._stagedValues.PresetIndex = Array.IndexOf(this.Presets, this.CurrentPresetPrivate);
                        break;
                }
            };
        }

        public void MoveToPreviousPreset()
        {
            this.CurrentPresetPrivate = GetPreviousItem(this.Presets, this.CurrentPresetPrivate);

            this.RaisePresetChanged(EventArgs.Empty);
        }

        public void MoveToNextPreset()
        {
            this.CurrentPresetPrivate = GetNextItem(this.Presets, this.CurrentPresetPrivate);

            this.RaisePresetChanged(EventArgs.Empty);
        }

        public bool CanSaveAsNewPreset()
        {
            return true;
        }

        public bool CanSavePreset()
        {
            var currentPreset = this.CurrentPresetPrivate;

            return currentPreset != null                                  // 无选中预设时不可。
                && !this._presetManager.IsReadOnlyPreset(currentPreset);  // 只读的预设不可编辑。
        }

        public bool CanDeletePreset()
        {
            var currentPreset = this.CurrentPresetPrivate;

            return currentPreset != null                                  // 无选中预设时不可。
                && !this._presetManager.IsReadOnlyPreset(currentPreset);  // 只读的预设不可删除。
        }

        public void DeleteCurrentPreset()
        {
            if (this.CurrentPresetPrivate == null) return;

            var next = GetNextItem(this.Presets, this.CurrentPresetPrivate);

            this._presetManager.UpdatePreset(this.CurrentPresetName, null);

            this.CurrentPresetPrivate = next;

            this.RaisePresetChanged(EventArgs.Empty);
        }

        public void SaveCurrentPreset(FontConfig settings)
        {
            if (this.NoPresetSelected)
                return;

            if (settings is null)
                throw new ArgumentNullException(nameof(settings));

            this._presetManager.UpdatePreset(this.CurrentPresetName,
                new FontPresetWithName(FontHelpers.GetCurrentLanguage(), this._fontType, settings, this.CurrentPresetName));   // TODO: 封装preset实例创建过程
        }

        public void SaveCurrentAsNewPreset(string presetName, FontConfig settings)
        {
            if (settings is null)
                throw new ArgumentNullException(nameof(settings));

            this._presetManager.UpdatePreset(presetName,
                new FontPresetWithName(FontHelpers.GetCurrentLanguage(), this._fontType, settings, presetName));   // TODO: 封装preset实例创建过程

            this.UpdatePresets();
        }

        protected virtual void RaisePresetChanged(EventArgs e)
        {
            PresetChanged?.Invoke(this, e);
        }

        private void UpdatePresets()
        {
            this.Presets = new FontPreset[] { null }.Concat(
                this._presetManager.GetPresets(FontHelpers.GetCurrentLanguage(), this._fontType)
                )
                .ToArray();
        }

        private static T? GetPreviousItem<T>(T?[] array, T? item, Func<T?, T?, bool> comparer = null)
        {
            comparer ??= (t1, t2) => ReferenceEquals(t1, t2);

            int index = Array.FindIndex(array, x => comparer(item, x));
            if (index == -1)
                if (item == null)
                    if (array.Length > 0)
                        return array[array.Length - 1];
                    else
                        throw new ArgumentOutOfRangeException(nameof(array), "数组长度为零。");
                else
                    throw new KeyNotFoundException();

            int prevIndex = index - 1;
            if (prevIndex < 0)
                prevIndex = array.Length - 1;

            return array[prevIndex];
        }

        private static T? GetNextItem<T>(T?[] array, T? item, Func<T?, T?, bool> comparer = null)
        {
            comparer ??= (t1, t2) => ReferenceEquals(t1, t2);

            int index = Array.FindIndex(array, x => comparer(item, x));
            if (index == -1)
                if (item == null)
                    if (array.Length > 0)
                        return array[0];
                    else
                        throw new ArgumentOutOfRangeException(nameof(array), "数组长度为零。");
                else
                    throw new KeyNotFoundException();

            int nextIndex = index + 1;
            if (nextIndex > array.Length - 1)
                nextIndex = 0;

            return array[nextIndex];
        }

        private record Preset(string Name, FontConfig Config);
    }
}
