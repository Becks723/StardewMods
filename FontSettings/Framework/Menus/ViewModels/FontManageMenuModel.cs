using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using FontSettings.Framework.Models;
using StardewValleyUI.Mvvm;

namespace FontSettings.Framework.Menus.ViewModels
{
    internal class FontManageMenuModel : MenuModelBase
    {
        private readonly SearchManager _searchManager;

        #region SearchFolders Property
        private ObservableCollection<SearchFolderViewModel> _searchFolders = new();
        public ObservableCollection<SearchFolderViewModel> SearchFolders
        {
            get => this._searchFolders;
            set => this.SetField(ref this._searchFolders, value);
        }
        #endregion

        #region RecursiveScan Property
        public bool RecursiveScan
        {
            get
            {
                if (this.SelectedFolderIndex != -1)
                    return this.SearchFolders[this.SelectedFolderIndex].RecursiveScan;
                return true;
            }
            set
            {
                if (this.SelectedFolderIndex != -1)
                    this.SearchFolders[this.SelectedFolderIndex].RecursiveScan = value;

                this.RaisePropertyChanged(nameof(this.RecursiveScan));
            }
        }
        #endregion

        #region LogDetails Property
        public bool LogDetails
        {
            get
            {
                if (this.SelectedFolderIndex != -1)
                    return this.SearchFolders[this.SelectedFolderIndex].LogDetails;
                return false;
            }
            set
            {
                if (this.SelectedFolderIndex != -1)
                    this.SearchFolders[this.SelectedFolderIndex].LogDetails = value;

                this.RaisePropertyChanged(nameof(this.LogDetails));
            }
        }
        #endregion

        #region IgnoredFiles Property
        private ObservableCollection<string> _ignoredFiles = new();
        public ObservableCollection<string> IgnoredFiles
        {
            get => this._ignoredFiles;
            set => this.SetField(ref this._ignoredFiles, value);
        }
        #endregion

        #region SelectedFolderIndex Property
        private int _selectedFolderIndex = -1;
        public int SelectedFolderIndex
        {
            get => this._selectedFolderIndex;
            set
            {
                this.SetField(ref this._selectedFolderIndex, value);

                this.RaisePropertyChanged(nameof(this.RecursiveScan));
                this.RaisePropertyChanged(nameof(this.LogDetails));
                this.RaisePropertyChanged(nameof(this.CanDeleteFolder));
                this.RaisePropertyChanged(nameof(this.CanEditFolder));
                this.RaisePropertyChanged(nameof(this.IsEditingFolder));
            }
        }
        #endregion

        #region CanNewFolder Property
        public bool CanNewFolder => true;
        #endregion

        #region CanDeleteFolder Property
        public bool CanDeleteFolder
        {
            get
            {
                return this.SelectedFolderIndex >= this._searchManager.GetReservedCount();
            }
        }
        #endregion

        #region CanEditFolder Property
        public bool CanEditFolder => this.SelectedFolderIndex >= this._searchManager.GetReservedCount();
        #endregion

        #region IsEditingFolder Property
        public bool IsEditingFolder
        {
            get
            {
                if (this.SelectedFolderIndex == -1)
                    return false;
                return this.SearchFolders[this.SelectedFolderIndex].IsEditing;
            }
        }
        #endregion

        public ICommand NewFolderCommand { get; }

        public ICommand DeleteFolderCommand { get; }

        public ICommand EditFolderCommand { get; }

        public ICommand ConfirmEditFolderCommand { get; }

        public ICommand OkCommand { get; }

        public ICommand CancelCommand { get; }

        public FontManageMenuModel(SearchManager searchManager)
        {
            this._searchManager = searchManager;

            this.NewFolderCommand = new DelegateCommand(this.NewFolder);
            this.DeleteFolderCommand = new DelegateCommand(this.DeleteFolder);
            this.EditFolderCommand = new DelegateCommand(this.EditFolder);
            this.ConfirmEditFolderCommand = new DelegateCommand(this.ConfirmEditFolder);
            this.OkCommand = new DelegateCommand<IOverlayMenu>(this.Ok);
            this.CancelCommand = new DelegateCommand<IOverlayMenu>(this.Cancel);

            this.SearchFolders.CollectionChanged += (sender, e) =>
            {
                this.RaisePropertyChanged(nameof(this.CanNewFolder));
            };

            // fill in search settings.
            {
                var search = this._searchManager.DisplaySearchSettings();
                foreach (var folder in search.Folders)
                {
                    this.SearchFolders.Add(new SearchFolderViewModel
                    {
                        Enabled = folder.Enabled,
                        Path = folder.Path,
                        RecursiveScan = folder.RecursiveScan,
                        LogDetails = folder.LogDetails,
                    });
                }
                this.IgnoredFiles = new ObservableCollection<string>(search.IgnoredFiles);
            }
        }

        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(sender, e);

            if (e.PropertyName == nameof(this.SelectedFolderIndex))
            {
                for (int i = 0; i < this.SearchFolders.Count; i++)
                {
                    this.SearchFolders[i].IsSelected = (i == this.SelectedFolderIndex);
                    if (i != this.SelectedFolderIndex)
                    {
                        this.SearchFolders[i].IsEditing = false;
                    }
                }
                this.RaisePropertyChanged(nameof(this.IsEditingFolder));
            }
        }

        private void NewFolder()
        {
            Debug.Assert(this.CanNewFolder);

            var newFolder = new SearchFolderViewModel()
            {
                Enabled = true,
                IsSelected = true,
                IsEditing = true
            };
            this.SearchFolders.Add(newFolder);
            this.SelectedFolderIndex = this.SearchFolders.Count - 1;
        }

        private void DeleteFolder()
        {
            Debug.Assert(this.CanDeleteFolder);

            this.SearchFolders.RemoveAt(this.SelectedFolderIndex);

            if (this.SearchFolders.Count > 0)
                this.SelectedFolderIndex = Math.Clamp(this.SelectedFolderIndex, 0, this.SearchFolders.Count - 1);  // clamp selectedIndex
            else
                this.SelectedFolderIndex = -1;
        }

        private void EditFolder()
        {
            if (this.TryCorrectSelectedFolderIndex())
                return;
            if (!this.CanEditFolder)
                return;

            this.SearchFolders[this.SelectedFolderIndex].IsEditing = true;
            this.RaisePropertyChanged(nameof(this.IsEditingFolder));
        }

        private void ConfirmEditFolder()
        {
            if (this.TryCorrectSelectedFolderIndex())
                return;

            this.SearchFolders[this.SelectedFolderIndex].IsEditing = false;
            this.RaisePropertyChanged(nameof(this.IsEditingFolder));
        }

        private void Ok(IOverlayMenu menu)
        {
            ManageSettings settings = this.GenerateSettings();
            _ = Task.Run(() => this._searchManager.SaveSearchSettings(settings.Search));

            menu?.Close(settings);
        }

        private void Cancel(IOverlayMenu menu)
        {
            menu?.Close(null);
        }

        private ManageSettings GenerateSettings()
        {
            var search = new SearchSettings();
            foreach (var folder in this.SearchFolders)
            {
                search.Folders.Add(new SearchFolderSettings
                {
                    Enabled = folder.Enabled,
                    Path = folder.Path,
                    RecursiveScan = folder.RecursiveScan,
                    LogDetails = folder.LogDetails,
                });
            }
            search.IgnoredFiles = new HashSet<string>(this.IgnoredFiles);

            var settings = new ManageSettings();
            settings.Search = search;
            return settings;
        }

        /// <summary>If <see cref="SelectedFolderIndex"/> is not valid, correct it to -1 (unselected), return true. If valid, return false.</summary>
        private bool TryCorrectSelectedFolderIndex()
        {
            if (this.SelectedFolderIndex < 0 || this.SelectedFolderIndex > this.SearchFolders.Count - 1)
            {
                this.SelectedFolderIndex = -1;
                return true;
            }
            return false;
        }
    }
}
