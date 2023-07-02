using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;

namespace FontSettings.Framework
{
    internal class DataAdditionalLanguagesWatcher
    {
        public event EventHandler<List<ModLanguage>> Updated;
        public void OnAssetReady(AssetReadyEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/AdditionalLanguages"))
            {
                this.RaiseUpdated(
                    Game1.content.Load<List<ModLanguage>>("Data/AdditionalLanguages"));
            }
        }

        private void RaiseUpdated(List<ModLanguage> value)
        {
            Updated?.Invoke(this, value);
        }
    }
}
