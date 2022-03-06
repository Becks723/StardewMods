using System;
using StardewModdingAPI;

namespace CodeShared.Integrations.GenericModConfigMenu
{
    internal abstract class GenericModConfigMenuIntegrationBase : ModIntegration<IGenericModConfigMenuApi>
    {
        private readonly Action _reset;
        private readonly Action _save;
        private readonly IManifest _manifest;

        protected GenericModConfigMenuIntegrationBase(Action reset, Action save, IModRegistry modRegistry, IMonitor monitor, IManifest manifest)
            : base(modID: "spacechase0.GenericModConfigMenu", modRegistry: modRegistry, monitor: monitor)
        {
            this._reset = reset;
            this._save = save;
            this._manifest = manifest;
        }

        protected override void IntegrateOverride(IGenericModConfigMenuApi api)
        {
            this.IntegrateOverride(
                new GenericModConfigMenuFluentHelper(api, this._manifest, this._reset, this._save)
            );
        }

        protected abstract void IntegrateOverride(GenericModConfigMenuFluentHelper helper);
    }
}
