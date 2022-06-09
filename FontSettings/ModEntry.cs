using System;
using System.Linq;
using FontSettings.Framework;
using FontSettings.Framework.Menus;
using FontSettings.Framework.Patchers;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace FontSettings
{
    internal class ModEntry : Mod
    {
        private ModConfig _config;

        private RuntimeFontManager _fontManager;

        private GameFontChanger _fontChanger;

        internal static Harmony Harmony { get; private set; }

        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            Log.Init(this.Monitor);
            this._config = helper.ReadConfig<ModConfig>();
            this._fontManager = new(helper.ModContent);
            this._fontChanger = new(this._fontManager);

            BmFontGenerator.Initialize(helper);
            CharsFileManager.Initialize(System.IO.Path.Combine(helper.DirectoryPath, "Chars"));

            Harmony = new Harmony(this.ModManifest.UniqueID);
            {
                new Game1Patcher(this._config, this._fontManager, this._fontChanger)
                    .Patch(Harmony, this.Monitor);

                new SpriteTextPatcher(this._config, this._fontManager, this._fontChanger)
                    .Patch(Harmony, this.Monitor);

                OptionsPageWithFont.Initialize(this._config, this._fontManager, this._fontChanger, (cfg) => helper.WriteConfig(cfg));
                OptionsPageWithFont.Patch(Harmony, this.Monitor);
            }
            LocalizedContentManager.OnLanguageChange += this.OnLanguageChanged;
        }

        private void OnLanguageChanged(LocalizedContentManager.LanguageCode code)
        {
            foreach (GameFontType type in Enum.GetValues<GameFontType>())
            {
                if (!this._config.Fonts.Any(f => (int)f.Lang == (int)code && f.InGameType == type))
                    this._config.Fonts.Add(new FontConfig()
                    {
                        Lang = (LanguageCode)(int)code,
                        InGameType = type,
                        FontSize = 12f,
                        Spacing = 0,
                        LineSpacing = 12
                    });
            }
            this.Helper.WriteConfig(this._config);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this._fontManager.Dispose();
        }
    }
}
