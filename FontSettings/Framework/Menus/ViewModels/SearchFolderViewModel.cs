using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValleyUI.Mvvm;

namespace FontSettings.Framework.Menus.ViewModels
{
    internal class SearchFolderViewModel : MenuModelBase
    {
        #region IsSelected Property
        private bool _isSelected;
        public bool IsSelected
        {
            get => this._isSelected;
            set => this.SetField(ref this._isSelected, value);
        }
        #endregion

        #region IsEditing Property
        private bool _isEditing;
        public bool IsEditing
        {
            get => this._isEditing;
            set => this.SetField(ref this._isEditing, value);
        }
        #endregion

        #region Enabled Property
        private bool _enabled;
        public bool Enabled
        {
            get => this._enabled;
            set => this.SetField(ref this._enabled, value);
        }
        #endregion

        #region Path Property
        private string _path;
        public string Path
        {
            get => this._path;
            set => this.SetField(ref this._path, value);
        }
        #endregion

        #region LogDetails Property
        private bool _logDetails;
        public bool LogDetails
        {
            get => this._logDetails;
            set => this.SetField(ref this._logDetails, value);
        }
        #endregion

        #region RecursiveScan Property
        private bool _recursiveScan;
        public bool RecursiveScan
        {
            get => this._recursiveScan;
            set => this.SetField(ref this._recursiveScan, value);
        }
        #endregion

        public SearchFolderViewModel()
        {
            this.RecursiveScan = true;
        }
    }
}
