using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Utilities;

namespace FontSettings.Framework.Menus
{
    internal class NewPresetMenuModel
    {
        private readonly FontPresetManager _presetManager;

        public event EventHandler Accepted;

        public bool IsFinished { get; set; }

        public string Name { get; set; }

        public bool CanOk { get; private set; }

        public string InvalidNameMessage { get; private set; }

        public NewPresetMenuModel(FontPresetManager presetManager)
        {
            this._presetManager = presetManager;
        }

        public void OnOk()
        {
            if (this.CanOk)
            {
                this.IsFinished = true;
                Accepted?.Invoke(this, EventArgs.Empty);
            }
        }

        public void OnCancel()
        {
            this.IsFinished = true;
        }

        public void CheckNameValid()
        {
            this.CanOk = this._presetManager.IsValidPresetName(this.Name, out string message);
            this.InvalidNameMessage = message;
        }
    }
}
