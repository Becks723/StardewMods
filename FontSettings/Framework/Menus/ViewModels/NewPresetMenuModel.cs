using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using StardewModdingAPI.Utilities;
using StardewValleyUI.Mvvm;

namespace FontSettings.Framework.Menus.ViewModels
{
    internal class NewPresetMenuModel : MenuModelBase
    {
        private readonly IFontPresetManager _presetManager;

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

        public ICommand OkCommand { get; }

        public ICommand CancelCommand { get; }

        public NewPresetMenuModel(IFontPresetManager presetManager)
        {
            this._presetManager = presetManager;

            this.OkCommand = new DelegateCommand<IOverlayMenu>(this.Ok);
            this.CancelCommand = new DelegateCommand<IOverlayMenu>(this.Cancel);
        }

        private void Ok(IOverlayMenu overlay)
        {
            overlay?.Close(this.Name);
        }

        private void Cancel(IOverlayMenu overlay)
        {
            overlay?.Close(null);
        }

        public void CheckNameValid()
        {
            this.CanOk = this._presetManager.IsValidPresetName(this.Name, out InvalidPresetNameTypes? invalidType);
            this.InvalidNameMessage = invalidType.HasValue
                ? invalidType.Value.GetMessage()
                : null;
        }
    }
}
