using System;
using CodeShared.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace PhoneTravelingCart.Framework
{
    internal class GMCMIntrgration : GenericModConfigMenuIntegrationBase
    {
        private ModConfig Config { get; }

        public GMCMIntrgration(ModConfig config, Action reset, Action save, IModRegistry modRegistry, IMonitor monitor, IManifest manifest)
            : base(reset, save, modRegistry, monitor, manifest)
        {
            this.Config = config;
        }

        protected override void IntegrateOverride(GenericModConfigMenuFluentHelper helper)
        {
            helper
                .Register()
                .AddCheckbox(
                    name: I18n.Config_RemotePurchase,
                    tooltip: I18n.Config_RemotePurchase_Description,
                    get: () => this.Config.RemotePurchase,
                    set: val => this.Config.RemotePurchase = val
                );
        }
    }
}
