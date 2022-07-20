using PhoneTravelingCart.Framework;
using PhoneTravelingCart.Framework.Patchers;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace PhoneTravelingCart
{
    internal class ModEntry : Mod
    {
        private ModConfig _config;

        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            this._config = helper.ReadConfig<ModConfig>();

            Harmony harmony = new Harmony(this.ModManifest.UniqueID);
            {
                new Game1Patcher().Patch(harmony, this.Monitor);
                new GameLocationPatcher(this._config).Patch(harmony, this.Monitor);
            }

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            new GMCMIntrgration(
                config: this._config,
                reset: this.ResetConfig,
                save: this.SaveConfig,
                modRegistry: this.Helper.ModRegistry,
                monitor: this.Monitor,
                manifest: this.ModManifest)
                .Integrate();
        }

        private void ResetConfig()
        {
            this._config.ResetToDefault();
            this.Helper.WriteConfig(this._config);
        }

        private void SaveConfig()
        {
            this.Helper.WriteConfig(this._config);
        }
    }
}
