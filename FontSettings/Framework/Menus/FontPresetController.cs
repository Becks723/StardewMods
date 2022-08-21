using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.FontInfomation;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace FontSettings.Framework.Menus
{
    internal class PresetChangedEventArgs : EventArgs
    {
        public GameFontType FontType { get; }

        public PresetChangedEventArgs(GameFontType fontType)
        {
            this.FontType = fontType;
        }
    }

    internal class FontPresetController
    {
        private readonly IDictionary<FontPresetFontType, FontPresetCellController> _controllers = new Dictionary<FontPresetFontType, FontPresetCellController>();
        private readonly FontPresetManager _presetManager;
        private readonly Func<IEnumerable<FontModel>> _getAllFonts;

        public event EventHandler<PresetChangedEventArgs> PresetChanged;

        public bool IsCurrentPresetValid { get; private set; }

        public string MessageWhenPresetIsInvalid { get; private set; }

        public bool IsNamingNewPreset { get; set; }

        public FontPresetController(FontPresetManager presetManager, Func<IEnumerable<FontModel>> getAllFonts)
        {
            this._presetManager = presetManager;
            this._getAllFonts = getAllFonts;

            foreach (FontPresetFontType fontType in new[] { FontPresetFontType.Small, FontPresetFontType.Medium, FontPresetFontType.Dialogue })
            {
                var controller = new FontPresetCellController(presetManager, fontType);
                controller.PresetChanged += (_, _) =>
                {
                    this.RaisePresetChanged(fontType switch
                    {
                        FontPresetFontType.Small => GameFontType.SmallFont,
                        FontPresetFontType.Medium => GameFontType.DialogueFont,
                        FontPresetFontType.Dialogue => GameFontType.SpriteText
                    });
                };

                this._controllers.Add(fontType, controller);
            }
            PresetChanged += this.OnPresetChanged;
        }

        public FontPreset? CurrentPreset(GameFontType fontType)
        {
            return this.Controller(fontType).CurrentPreset;
        }

        public void SwitchToPreviousPreset(GameFontType fontType)
        {
            this.Controller(fontType).SwitchToPreviousPreset();
        }

        public void SwitchToNextPreset(GameFontType fontType)
        {
            this.Controller(fontType).SwitchToNextPreset();
        }

        public void SaveCurrentPreset(GameFontType fontType,
            string fontFileName, int fontIndex, float fontSize, float spacing, int lineSpacing, float offsetX, float offsetY)
        {
            this.Controller(fontType).SaveCurrentPreset(
                fontFileName, fontIndex, fontSize, spacing, lineSpacing, offsetX, offsetY);
        }

        public void SaveCurrentAsNewPreset(GameFontType fontType,
            string presetName, string fontFileName, int fontIndex, float fontSize, float spacing, int lineSpacing, float offsetX, float offsetY)
        {
            this.Controller(fontType).SaveCurrentAsNewPreset(
                presetName, fontFileName, fontIndex, fontSize, spacing, lineSpacing, offsetX, offsetY);
        }

        public void DeleteCurrentPreset(GameFontType fontType)
        {
            this.Controller(fontType).DeleteCurrentPreset();
        }

        public bool CanUseNewButton(GameFontType fontType)
        {
            return true;
        }

        public bool CanUseSaveButton(GameFontType fontType)
        {
            FontPreset? preset = this.CurrentPreset(fontType);

            return preset != null;                                // 无选中预设时不可。
        }

        public bool CanUseDeleteButton(GameFontType fontType)
        {
            FontPreset? preset = this.CurrentPreset(fontType);

            return preset != null                                 // 无选中预设时不可。
                && !this._presetManager.IsBuiltInPreset(preset);  // 内置的预设不可删除。
        }

        public bool MeetsRequirement(FontPreset preset, IEnumerable<FontModel> availableFonts, out FontModel match)
        {
            match = null;
            if (availableFonts == null) return false;

            bool keepOriginal;
            string specifiedName = null;
            string specifiedExtension = null;
            if (string.IsNullOrWhiteSpace(preset.Requires.FontFileName))
                keepOriginal = true;
            else
            {
                keepOriginal = false;
                string specifiedFile = PathUtilities.NormalizePath(preset.Requires.FontFileName);
                specifiedName = Path.GetFileNameWithoutExtension(specifiedFile);
                specifiedExtension = Path.GetExtension(specifiedFile);
            }

            foreach (FontModel font in availableFonts)
            {
                if (keepOriginal)
                {
                    if (font.FullPath == null)
                    {
                        match = font;
                        return true;
                    }
                }
                else
                {
                    string name = Path.GetFileNameWithoutExtension(font.FullPath);
                    string extension = Path.GetExtension(font.FullPath);
                    if (name == specifiedName
                        && extension.Equals(specifiedExtension, StringComparison.OrdinalIgnoreCase)
                        && preset.FontIndex == font.FontIndex)
                    {
                        match = font;
                        return true;
                    }
                }
            }

            return false;
        }

        private void OnPresetChanged(object sender, PresetChangedEventArgs e)
        {
            FontPreset? newPreset = this.CurrentPreset(e.FontType);
            if (newPreset == null)
                this.IsCurrentPresetValid = true;
            else
                this.IsCurrentPresetValid = this.MeetsRequirement(newPreset, this._getAllFonts(), out _);

            if (!this.IsCurrentPresetValid)
            {
                this.MessageWhenPresetIsInvalid = $"预设不可用：需要安装字体文件：{newPreset.Requires.FontFileName}。";
                Game1.addHUDMessage(new HUDMessage(this.MessageWhenPresetIsInvalid, HUDMessage.error_type));
            }
        }

        private FontPresetCellController Controller(GameFontType fontType)
        {
            var key = fontType switch
            {
                GameFontType.SmallFont => FontPresetFontType.Small,
                GameFontType.DialogueFont => FontPresetFontType.Medium,
                GameFontType.SpriteText => FontPresetFontType.Dialogue,
                _ => throw new NotSupportedException(),
            };

            return this._controllers[key];
        }

        private void RaisePresetChanged(GameFontType fontType)
        {
            this.RaisePresetChanged(
                new PresetChangedEventArgs(fontType));
        }

        protected virtual void RaisePresetChanged(PresetChangedEventArgs e)
        {
            PresetChanged?.Invoke(this, e);
        }
    }
}
