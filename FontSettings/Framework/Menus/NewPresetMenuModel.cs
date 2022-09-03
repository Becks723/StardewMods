using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Utilities;

namespace FontSettings.Framework.Menus
{
    internal class NewPresetMenuModel : ViewModel
    {
        private readonly FontPresetManager _presetManager;

        public event EventHandler Accepted;

        private bool _isFinished;
        public bool IsFinished
        {
            get => this._isFinished;
            private set => this.SetField(ref this._isFinished, value);
        }

        public string Name { get; set; }

        private bool _canOk;
        public bool CanOk
        {
            get => this._canOk;
            private set => this.SetField(ref this._canOk, value);
        }

        private string _invalidNameMessage;
        public string InvalidNameMessage
        {
            get => this._invalidNameMessage;
            private set => this.SetField(ref this._invalidNameMessage, value);
        }

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
