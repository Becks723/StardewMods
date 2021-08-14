using System;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace JunimoStudio.ModIntegrations.GenericModConfigMenu
{
    public class GMCMIntegHelper<TConfig> : GenericIntegHelper<IGenericModConfigMenuApi>
        where TConfig : class, new()
    {
        private static readonly string GMCMName = "Generic Mod Config Menu";

        private static readonly string GMCMUniqueId = "spacechase0.GenericModConfigMenu";

        private readonly IManifest _manifest;

        private readonly Func<TConfig> _config;

        private readonly Action _reset;

        private readonly Action _save;

        public GMCMIntegHelper(Func<TConfig> getConfig, Action reset, Action save, IMonitor monitor, IManifest manifest, IModRegistry modRegistry)
            : base(GMCMName, GMCMUniqueId, monitor, modRegistry)
        {
            this._config = getConfig;
            this._reset = reset;
            this._save = save;
            this._manifest = manifest;
        }

        public GMCMIntegHelper<TConfig> InitConfig(bool inGame = true)
        {
            this.ErrorIfApiNotLoaded();

            this._api.RegisterModConfig(this._manifest, this._reset, this._save);
            if (inGame)
            {
                this._api.SetDefaultIngameOptinValue(this._manifest, true);
            }
            return this;
        }

        public GMCMIntegHelper<TConfig> AddLabel(string name, string description)
        {
            this.ErrorIfApiNotLoaded();

            this._api.RegisterLabel(this._manifest, name, description);
            return this;
        }

        /// <summary>见拓展方法<see cref="IGenericModConfigMenuApiEx.RegisterSimpleOption{T}(IGenericModConfigMenuApi, IManifest, string, string, Func{T}, Action{T})"/>。</summary>
        public GMCMIntegHelper<TConfig> AddSimpleOptions<T>(params (string name, string description, Func<TConfig, T> get, Action<TConfig, T> set)[] options)
        {
            this.ErrorIfApiNotLoaded();

            foreach (var (name, description, get, set) in options)
            {
                this._api.RegisterSimpleOption<T>(
                    this._manifest,
                    name,
                    description,
                    () => get(this._config()),
                    (val) => set(this._config(), val));
            }

            return this;
        }

        public GMCMIntegHelper<TConfig> AddCheckBox(string name, string description, Func<TConfig, bool> get, Action<TConfig, bool> set)
        {
            return this.AddSimpleOptions((name, description, get, set));
        }

        public GMCMIntegHelper<TConfig> AddTextBox(string name, string description, Func<TConfig, int> get, Action<TConfig, int> set)
        {
            return this.AddSimpleOptions((name, description, get, set));
        }

        public GMCMIntegHelper<TConfig> AddTextBox(string name, string description, Func<TConfig, float> get, Action<TConfig, float> set)
        {
            return this.AddSimpleOptions((name, description, get, set));
        }

        public GMCMIntegHelper<TConfig> AddTextBox(string name, string description, Func<TConfig, string> get, Action<TConfig, string> set)
        {
            return this.AddSimpleOptions((name, description, get, set));
        }

        public GMCMIntegHelper<TConfig> AddKeybinding(string name, string description, Func<TConfig, SButton> get, Action<TConfig, SButton> set)
        {
            return this.AddSimpleOptions((name, description, get, set));
        }

        public GMCMIntegHelper<TConfig> AddKeybinding(string name, string description, Func<TConfig, KeybindList> get, Action<TConfig, KeybindList> set)
        {
            return this.AddSimpleOptions((name, description, get, set));
        }

        public GMCMIntegHelper<TConfig> AddDropDown(string name, string description, Func<TConfig, string> get, Action<TConfig, string> set, string[] choices)
        {
            this.ErrorIfApiNotLoaded();

            this._api.RegisterChoiceOption(
                this._manifest,
                name,
                description,
                () => get(this._config()),
                (val) => set(this._config(), val),
                choices);
            return this;
        }

        public GMCMIntegHelper<TConfig> AddDropDown<TEnum>(string name, string description, Func<TConfig, TEnum> get, Action<TConfig, TEnum> set)
            where TEnum : Enum
        {
            return this.AddDropDown(
                name,
                description,
                (cfg) => get(cfg).ToString(),
                (cfg, val) => set(cfg, (TEnum)Enum.Parse(typeof(TEnum), val)),
                choices: Enum.GetNames(typeof(TEnum)));
        }

    }
}
