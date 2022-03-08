using System;
using CodeShared.Integrations.SaveAnywhere;
using StardewModdingAPI;

namespace FluteBlockExtension.Framework.Integrations
{
    internal class SaveAnywhereIntegration : SaveAnywhereIntegrationBase
    {
        private readonly Action _beforeSave;

        public SaveAnywhereIntegration(Action beforeSave!!, IModRegistry modRegistry, IMonitor monitor)
            : base(modRegistry, monitor)
        {
            this._beforeSave = beforeSave;
        }

        protected override void IntegrateOverride(ISaveAnywhereAPI api)
        {
            api.BeforeSave += this.BeforeSave;
        }

        private void BeforeSave(object sender, EventArgs e)
        {
            this._beforeSave();
        }
    }
}
