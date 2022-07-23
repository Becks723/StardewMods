using System;
using StardewModdingAPI;

namespace CodeShared.Integrations.GenericModConfigMenu
{
    internal abstract class GenericModConfigMenuIntegrationBase : ModIntegration<IGenericModConfigMenuApi>
    {
        private Action _reset;
        private Action _save;
        private IManifest _manifest;

        protected GenericModConfigMenuIntegrationBase(Action reset, Action save, IModRegistry modRegistry, IMonitor monitor, IManifest manifest)
            : this(modRegistry: modRegistry, monitor: monitor)
        {
            this._reset = reset;
            this._save = save;
            this._manifest = manifest;
        }

        protected GenericModConfigMenuIntegrationBase(IModRegistry modRegistry, IMonitor monitor)
            : base(modID: "spacechase0.GenericModConfigMenu", modRegistry: modRegistry, monitor: monitor)
        {
        }

        protected override void IntegrateOverride(IGenericModConfigMenuApi api)
        {
            this.IntegrateOverride(
                new GenericModConfigMenuFluentHelper(api, this._manifest, this._reset, this._save)
            );
        }

        protected abstract void IntegrateOverride(GenericModConfigMenuFluentHelper helper);

        protected void InitFields(Action reset, Action save, IManifest manifest)
        {
            this._reset = reset;
            this._save = save;
            this._manifest = manifest;
        }
    }
}
