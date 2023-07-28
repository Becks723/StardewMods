using System;
using CodeShared.Integrations.GMCMOptions;
using StardewModdingAPI;

namespace CodeShared.Integrations.GenericModConfigMenu
{
    internal abstract class GenericModConfigMenuIntegrationBase : ModIntegrationBase<IGenericModConfigMenuApi>
    {
        private readonly Action _reset;
        private readonly Action _save;
        private readonly IManifest _manifest;
        private readonly bool _isGMCMOptionsRequired;

        private readonly GMCMOptionsIntegrationObject _gmcmOptions;

        protected GenericModConfigMenuIntegrationBase(Action reset, Action save, IModRegistry modRegistry, IMonitor monitor, IManifest manifest, bool isGMCMOptionsRequired = false)
            : base(modID: "spacechase0.GenericModConfigMenu", modRegistry: modRegistry, monitor: monitor)
        {
            this._reset = reset;
            this._save = save;
            this._manifest = manifest;
            this._isGMCMOptionsRequired = isGMCMOptionsRequired;

            this._gmcmOptions = new GMCMOptionsIntegrationObject(modRegistry, monitor);
        }

        protected override void IntegrateOverride(IGenericModConfigMenuApi api)
        {
            this._gmcmOptions.Integrate();
            if (this._gmcmOptions.Api == null && this._isGMCMOptionsRequired)  // TODO: pull up, 放到最基类
                throw new Exception("GMCM Options required.");

            this.IntegrateOverride(
                new GenericModConfigMenuFluentHelper(api, this._manifest, this._reset, this._save, this._gmcmOptions.Api)
            );
        }

        protected abstract void IntegrateOverride(GenericModConfigMenuFluentHelper helper);
    }
}
