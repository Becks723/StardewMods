using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private ObservableCollection<FontPreset> _presets;
        private ObservableCollection<FontPreset> Presets
        {
            get => this._presets;
            set
            {
                if (this._presets != null)
                    this._presets.CollectionChanged -= this.OnPresetsCollectionChanged;

                this.SetField(ref this._presets, value);

                if (this._presets != null)
                    this._presets.CollectionChanged += this.OnPresetsCollectionChanged;
            }
        }

        private FontPreset _currentPresetPrivate;
        private FontPreset CurrentPresetPrivate
        {
            get => this._currentPresetPrivate;
            set
            {
                this.SetField(ref this._currentPresetPrivate, value);

                this.RaisePropertyChanged(nameof(this.CurrentPresetName));
                this.RaisePropertyChanged(nameof(this.CurrentPresetSettings));

                this.RaisePropertyChanged(nameof(this.CanSaveCurrentPreset));
            }
        }

        private bool NoPresetSelected => this.CurrentPresetPrivate == null;

        /// <summary>Unique key of current preset. If current preset is null or doesn't have a key, returns null.</summary>
        public string? CurrentPresetName  // TODO: 更名成 CurrentPresetKeyOrNull
        {
            get
            {
                if (this.NoPresetSelected)
                    return null;

                if (this.CurrentPresetPrivate.TryGetInstance(out IPresetWithKey<string> withKey))
                    return withKey.Key;
                else
                    return null;
            }
        }

        public string? CurrentPresetNameOrNull
        {
            get
            {
                if (this.NoPresetSelected)
                    return null;

                if (this.CurrentPresetPrivate.TryGetInstance(out IPresetWithName withName))
                    return withName.Name;
                else
                    return null;
            }
        }

        public string? CurrentPresetAuthorOrNull
        {
            get
            {
                if (this.NoPresetSelected)
                    return null;

                if (this.CurrentPresetPrivate.TryGetInstance(out IPresetFromContentPack fromContentPack))
                    return fromContentPack.SContentPack.Manifest.Author;
                else
                    return null;
            }
        }

        public FontConfig CurrentPresetSettings => this.CurrentPresetPrivate?.Settings;

        public bool CanSaveCurrentPreset
        {
            get
            {
                var currentPreset = this.CurrentPresetPrivate;

                return currentPreset != null                                        // 无选中预设时不可。
                    && !this._presetManager.IsReadOnlyPreset(currentPreset)         // 只读的预设不可编辑。
                    && !new FontConfigValueComparer().Equals(
                        currentPreset?.Settings,                                    // 仅在改动时有必要保存。
                        this.CurrentFontConfigRealTime);
            }
        }

        private FontConfig _currentFontConfigRealTime;
        public FontConfig CurrentFontConfigRealTime
        {
            get => this._currentFontConfigRealTime;
            set
            {
                this.SetField(ref this._currentFontConfigRealTime, value);

                this.RaisePropertyChanged(nameof(this.CanSaveCurrentPreset));
            }
        }

        public event EventHandler PresetChanged;

        public FontPresetViewModel(IFontPresetManager presetManager, GameFontType fontType, FontSettingsMenuPresetContextModel stagedValues)
        {
            this._presetManager = presetManager;
            this._fontType = fontType;
            this._stagedValues = stagedValues;

            this.RegisterCallbackToStageValues();

            this.Presets = new ObservableCollection<FontPreset>(this.EnumerateAvailablePresets());

            // 填入之前记录的值。
            this.ApplyStagedValues();
        }

        private void RegisterCallbackToStageValues()
        {
            this.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(this.Presets):
                    case nameof(this.CurrentPresetPrivate):
                        this._stagedValues.PresetIndex = Math.Max(0, this.Presets.IndexOf(this.CurrentPresetPrivate));
                        break;
                }
            };
        }

        private void ApplyStagedValues()
        {
            int index = Math.Clamp(this._stagedValues.PresetIndex, 0, this.Presets.Count - 1);
            this.CurrentPresetPrivate = this.Presets[index];
        }

        private void OnPresetsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this._stagedValues.PresetIndex = Math.Max(0, this.Presets.IndexOf(this.CurrentPresetPrivate));
        }

        public void MoveToPreviousPreset()
        {
            this.CurrentPresetPrivate = GetPreviousItem(this.Presets.ToArray(), this.CurrentPresetPrivate);

            this.RaisePresetChanged(EventArgs.Empty);
        }

        public void MoveToNextPreset()
        {
            this.CurrentPresetPrivate = GetNextItem(this.Presets.ToArray(), this.CurrentPresetPrivate);

            this.RaisePresetChanged(EventArgs.Empty);
        }

        public bool CanSaveAsNewPreset()
        {
            return true;
        }

        public bool CanDeletePreset()
        {
            var currentPreset = this.CurrentPresetPrivate;

            return currentPreset != null                                  // 无选中预设时不可。
                && !this._presetManager.IsReadOnlyPreset(currentPreset);  // 只读的预设不可删除。
        }

        public void DeleteCurrentPreset()
        {
            if (this.CurrentPresetPrivate == null
                || this.CurrentPresetName == null)
                return;

            var next = GetNextItem(this.Presets.ToArray(), this.CurrentPresetPrivate);

            this._presetManager.UpdatePreset(this.CurrentPresetName, null);
            this.Presets.Remove(this.CurrentPresetPrivate);

            this.CurrentPresetPrivate = next;

            this.RaisePresetChanged(EventArgs.Empty);
        }

        public void SaveCurrentPreset(FontConfig settings)
        {
            if (this.NoPresetSelected
                || this.CurrentPresetName == null)
                return;

            if (settings is null)
                throw new ArgumentNullException(nameof(settings));

            // 获取保存后的预设。
            FontPreset newPreset = this.CreateNewPreset(this.CurrentPresetName, settings);

            // 更新数据库。
            this._presetManager.UpdatePreset(this.CurrentPresetName, newPreset);

            // 更新自己的相关属性。
            int index = this.Presets.IndexOf(this.CurrentPresetPrivate);
            this.Presets[index] = newPreset;
            this.CurrentPresetPrivate = newPreset;

            this.RaisePresetChanged(EventArgs.Empty);
        }

        public void SaveCurrentAsNewPreset(string presetName, FontConfig settings)
        {
            if (settings is null)
                throw new ArgumentNullException(nameof(settings));

            FontPreset newPreset = this.CreateNewPreset(presetName, settings);

            this._presetManager.UpdatePreset(presetName, newPreset);

            this.Presets.Add(newPreset);
        }

        public bool TryGetCurrentPresetIfSupportsDetailedInfo(out FontPreset preset)
        {
            preset = null;

            if (this.NoPresetSelected)
                return false;

            var currentPreset = this.CurrentPresetPrivate;
            if (currentPreset.Supports<IPresetFromContentPack>())
            {
                preset = currentPreset;
                return true;
            }
            return false;
        }

        protected virtual void RaisePresetChanged(EventArgs e)
        {
            PresetChanged?.Invoke(this, e);
        }

        private IEnumerable<FontPreset> EnumerateAvailablePresets()
        {
            /* default *not selected* preset */
            yield return null;

            /* presets from database */
            var presets = this._presetManager.GetPresets(FontHelpers.GetCurrentLanguage(), this._fontType);  // TODO: 排序
            foreach (FontPreset preset in presets)
                yield return preset;
        }

        private FontPreset CreateNewPreset(string presetName, FontConfig settings)
        {
            return new FontPresetBuilder()
                .BasicPreset(new FontContext(FontHelpers.GetCurrentLanguage(), this._fontType), settings)
                .WithKey(presetName)
                .WithName(presetName)
                .Build();
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
    }
}
