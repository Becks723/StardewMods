using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.FontScanning;
using FontSettings.Framework.Models;
using SGamePlatform = StardewModdingAPI.GamePlatform;
using SConstants = StardewModdingAPI.Constants;
using FontSettings.Framework.FontScanning.Scanners;
using FontSettings.Framework.DataAccess;
using System.IO;

namespace FontSettings.Framework
{
    internal class SearchManager
    {
        private static class Locations
        {
            public const string ModFolder = "assets/fonts";

            public const string WINDOWS_SystemRoot = @"%SYSTEMROOT%\Fonts";  // C:\Windows
            public const string WINDOWS_AppData = @"%APPDATA%\Microsoft\Windows\Fonts";  // Roaming
            public const string WINDOWS_LocalAppData = @"%LOCALAPPDATA%\Microsoft\Windows\Fonts";  // Local
            public const int WINDOWS_Count = 3;

            public const string MACOS_User = "%HOME%/Library/Fonts/";
            public const string MACOS_Local = "/Library/Fonts/";
            public const string MACOS_System = "/System/Library/Fonts/";
            public const string MACOS_Network = "/Network/Library/Fonts/";
            public const int MACOS_Count = 4;

            public const string LINUX_Home = "%HOME%/.fonts/";
            public const string LINUX_Local = "/usr/local/share/fonts";
            public const string LINUX_Share = "/usr/share/fonts";
            public const int LINUX_Count = 3;
        }

        private readonly ICollection<IFontFileScanner> _scanners;
        private readonly string _modDirectory;
        private readonly SearchSettingsRepository _repo;

        private readonly SGamePlatform _platform = SConstants.TargetPlatform;

        private readonly BasicFontFileScanner _modFolderScanner;

        private readonly BasicFontFileScanner _windowsRootScanner;
        private readonly BasicFontFileScanner _windowsAppDataScanner;
        private readonly BasicFontFileScanner _windowsLocalScanner;

        private readonly BasicFontFileScanner _macOSUserScanner;
        private readonly BasicFontFileScanner _macOSLocalScanner;
        private readonly BasicFontFileScanner _macOSSystemScanner;
        private readonly BasicFontFileScanner _macOSNetworkScanner;

        private readonly BasicFontFileScanner _linuxHomeScanner;
        private readonly BasicFontFileScanner _linuxLocalScanner;
        private readonly BasicFontFileScanner _linuxShareScanner;

        public SearchManager(ICollection<IFontFileScanner> scanners, string modDirectory, SearchSettingsRepository repo)
        {
            this._scanners = scanners;
            this._modDirectory = modDirectory;
            this._repo = repo;

            this._modFolderScanner = new BasicFontFileScanner(Path.Combine(modDirectory, Locations.ModFolder), null);
            switch (this._platform)
            {
                case SGamePlatform.Linux:
                    this._linuxHomeScanner = new BasicFontFileScanner(Environment.ExpandEnvironmentVariables("%HOME%/.fonts/"), null);
                    this._linuxLocalScanner = new BasicFontFileScanner(Environment.ExpandEnvironmentVariables("/usr/local/share/fonts"), null);
                    this._linuxShareScanner = new BasicFontFileScanner(Environment.ExpandEnvironmentVariables("/usr/share/fonts"), null);
                    break;

                case SGamePlatform.Mac:
                    this._macOSUserScanner = new BasicFontFileScanner(Environment.ExpandEnvironmentVariables("%HOME%/Library/Fonts/"), null);
                    this._macOSLocalScanner = new BasicFontFileScanner(Environment.ExpandEnvironmentVariables("/Library/Fonts/"), null);
                    this._macOSSystemScanner = new BasicFontFileScanner(Environment.ExpandEnvironmentVariables("/System/Library/Fonts/"), null);
                    this._macOSNetworkScanner = new BasicFontFileScanner(Environment.ExpandEnvironmentVariables("/Network/Library/Fonts/"), null);
                    break;

                case SGamePlatform.Windows:
                    this._windowsRootScanner = new BasicFontFileScanner(Environment.ExpandEnvironmentVariables(@"%SYSTEMROOT%\Fonts"), null);
                    this._windowsAppDataScanner = new BasicFontFileScanner(Environment.ExpandEnvironmentVariables(@"%APPDATA%\Microsoft\Windows\Fonts"), null);
                    this._windowsLocalScanner = new BasicFontFileScanner(Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Microsoft\Windows\Fonts"), null);
                    break;
            }
        }

        public SearchSettings DisplaySearchSettings()
        {
            SearchSettings search = this._repo.ReadData();

            SearchSettings result = new SearchSettings();
            result.IgnoredFiles = search.IgnoredFiles;

            // mod folder.
            result.Folders.Add(DisplayFolder(search.Folders[0], Locations.ModFolder));

            // platform specific.
            switch (this._platform)
            {
                case SGamePlatform.Linux:
                    result.Folders.Add(DisplayFolder(search.Folders[1], Environment.ExpandEnvironmentVariables(Locations.LINUX_Home)));
                    result.Folders.Add(DisplayFolder(search.Folders[2], Environment.ExpandEnvironmentVariables(Locations.LINUX_Local)));
                    result.Folders.Add(DisplayFolder(search.Folders[3], Environment.ExpandEnvironmentVariables(Locations.LINUX_Share)));
                    break;

                case SGamePlatform.Mac:
                    result.Folders.Add(DisplayFolder(search.Folders[1], Environment.ExpandEnvironmentVariables(Locations.MACOS_User)));
                    result.Folders.Add(DisplayFolder(search.Folders[2], Environment.ExpandEnvironmentVariables(Locations.MACOS_Local)));
                    result.Folders.Add(DisplayFolder(search.Folders[3], Environment.ExpandEnvironmentVariables(Locations.MACOS_System)));
                    result.Folders.Add(DisplayFolder(search.Folders[4], Environment.ExpandEnvironmentVariables(Locations.MACOS_Network)));
                    break;

                case SGamePlatform.Windows:
                    result.Folders.Add(DisplayFolder(search.Folders[1], Environment.ExpandEnvironmentVariables(Locations.WINDOWS_SystemRoot)));
                    result.Folders.Add(DisplayFolder(search.Folders[2], Environment.ExpandEnvironmentVariables(Locations.WINDOWS_AppData)));
                    result.Folders.Add(DisplayFolder(search.Folders[3], Environment.ExpandEnvironmentVariables(Locations.WINDOWS_LocalAppData)));
                    break;
            }

            // custom.
            for (int i = 10; i < search.Folders.Count; i++)
                result.Folders.Add(search.Folders[i]);

            return result;

            SearchFolderSettings DisplayFolder(SearchFolderSettings basedOn, string displayPath)
            {
                basedOn.Path = displayPath;
                return basedOn;
            }
        }

        public void SaveSearchSettings(SearchSettings? search)
        {
            if (search == null)
            {
                this._repo.ClearData();
                return;
            }

            int reserved = this.GetReservedCount();
            if (reserved > 10)
                throw new ArgumentException($"reserved folders must be <=10. Current: {reserved}");

            var result = new SearchSettings(search);

            for (int i = 0; i < Math.Max(10, result.Folders.Count); i++)
            {
                string resStr = "RESERVED" + (i + 1).ToString("00");

                if (i < reserved)
                    result.Folders[i].Path = resStr;
                else if (i < 10)
                    result.Folders.Insert(i, new SearchFolderSettings { Path = resStr });
            }

            this._repo.WriteData(result);
        }

        public void ApplySearchSettings()
        {
            SearchSettings search = this._repo.ReadData();

            // if first time, need initialize.
            if (search.Folders.Count == 0)
            {                
                // mod folder.
                search.Folders.Add(new SearchFolderSettings { Path = Path.Combine(this._modDirectory, Locations.ModFolder) });

                // platform speific.
                switch (this._platform)
                {
                    case SGamePlatform.Linux:
                        search.Folders.Add(new SearchFolderSettings { Path = Environment.ExpandEnvironmentVariables(Locations.LINUX_Home) });
                        search.Folders.Add(new SearchFolderSettings { Path = Environment.ExpandEnvironmentVariables(Locations.LINUX_Local) });
                        search.Folders.Add(new SearchFolderSettings { Path = Environment.ExpandEnvironmentVariables(Locations.LINUX_Share) });
                        break;

                    case SGamePlatform.Mac:
                        search.Folders.Add(new SearchFolderSettings { Path = Environment.ExpandEnvironmentVariables(Locations.MACOS_User) });
                        search.Folders.Add(new SearchFolderSettings { Path = Environment.ExpandEnvironmentVariables(Locations.MACOS_Local) });
                        search.Folders.Add(new SearchFolderSettings { Path = Environment.ExpandEnvironmentVariables(Locations.MACOS_System) });
                        search.Folders.Add(new SearchFolderSettings { Path = Environment.ExpandEnvironmentVariables(Locations.MACOS_Network) });
                        break;

                    case SGamePlatform.Windows:
                        search.Folders.Add(new SearchFolderSettings { Path = Environment.ExpandEnvironmentVariables(Locations.WINDOWS_SystemRoot) });
                        search.Folders.Add(new SearchFolderSettings { Path = Environment.ExpandEnvironmentVariables(Locations.WINDOWS_AppData) });
                        search.Folders.Add(new SearchFolderSettings { Path = Environment.ExpandEnvironmentVariables(Locations.WINDOWS_LocalAppData) });
                        break;
                }

                // then save.
                this.SaveSearchSettings(search);
            }

            this.ApplySearchSettings(search);
        }

        public void ApplySearchSettings(SearchSettings search)
        {
            if (search == null)
                throw new ArgumentNullException(nameof(search));

            this._scanners.Clear();

            // mod folder.
            this.ApplyScanner(this._modFolderScanner, search, search.Folders[0]);

            // platform specific.
            switch (this._platform)
            {
                case SGamePlatform.Linux:
                    this.ApplyScanner(this._linuxHomeScanner, search, search.Folders[1]);
                    this.ApplyScanner(this._linuxLocalScanner, search, search.Folders[2]);
                    this.ApplyScanner(this._linuxShareScanner, search, search.Folders[3]);
                    break;

                case SGamePlatform.Mac:
                    this.ApplyScanner(this._macOSUserScanner, search, search.Folders[1]);
                    this.ApplyScanner(this._macOSLocalScanner, search, search.Folders[2]);
                    this.ApplyScanner(this._macOSSystemScanner, search, search.Folders[3]);
                    this.ApplyScanner(this._macOSNetworkScanner, search, search.Folders[4]);
                    break;

                case SGamePlatform.Windows:
                    this.ApplyScanner(this._windowsRootScanner, search, search.Folders[1]);
                    this.ApplyScanner(this._windowsAppDataScanner, search, search.Folders[2]);
                    this.ApplyScanner(this._windowsLocalScanner, search, search.Folders[3]);
                    break;
            }

            // custom.
            for (int i = this.GetReservedCount(); i < search.Folders.Count; i++)
                this.ApplyScanner(new BasicFontFileScanner(null, null), search, search.Folders[i], overridePath: true);
        }

        public int GetReservedCount()
        {
            int reserved = 1;
            switch (this._platform)
            {
                case SGamePlatform.Linux: reserved += Locations.LINUX_Count; break;
                case SGamePlatform.Mac: reserved += Locations.MACOS_Count; break;
                case SGamePlatform.Windows: reserved += Locations.WINDOWS_Count; break;
            }
            return reserved;
        }

        private void ApplyScanner(BasicFontFileScanner scanner, SearchSettings search, SearchFolderSettings folder, bool overridePath = false)
        {
            if (folder.Enabled)
            {
                if (overridePath)
                    scanner.BaseDirectory = folder.Path;
                scanner.ScanSettings = this.ParseSettings(search, folder);
                this._scanners.Add(scanner);
            }
        }

        private ScanSettings ParseSettings(SearchSettings search, SearchFolderSettings folder)
        {
            if (search == null)
                throw new ArgumentNullException(nameof(search));
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));

            ScanSettings scan = new ScanSettings();
            {
                scan.LogDetails = folder.LogDetails;
                scan.RecursiveScan = folder.RecursiveScan;
                scan.Extensions.Clear();
                foreach (var item in folder.Extensions)
                    scan.Extensions.Add(item);
                foreach (var item in search.IgnoredFiles)
                    scan.IgnoredFiles.Add(item);
            }
            return scan;
        }
    }
}
