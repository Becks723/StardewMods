using System;
using JunimoStudio.ModIntegrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace JunimoStudio
{
    internal class GMCMJunimoStudioInteg
    {
        private readonly GMCMIntegHelper<ModConfig> _helper;

        public GMCMJunimoStudioInteg(Func<ModConfig> getConfig, Action reset, Action save, IMonitor monitor, IManifest manifest, IModRegistry modRegistry)
        {
            this._helper = new GMCMIntegHelper<ModConfig>(getConfig, reset, save, monitor, manifest, modRegistry);
        }

        public void Register()
        {
            if (!this._helper.ApiLoaded)
                return;

            this._helper
                .InitConfig(true)

                .AddLabel("常规", null)
                .AddCheckBox(
                    "启用轨道",
                    null,
                    (cfg) => cfg.EnableTracks,
                    (cfg, val) => cfg.EnableTracks = val)

                .AddLabel("钢琴卷帘窗", null)
                .AddDropDown(
                    "吸附精度",
                    null,
                    (cfg) => cfg.PianoRoll.Grid,
                    (cfg, val) => cfg.PianoRoll.Grid = val)

                .AddLabel("键位", null)
                .AddKeybinding(
                    "撤销",
                    null,
                    (cfg) => cfg.Keys.Undo,
                    (cfg, val) => cfg.Keys.Undo = val)
                 .AddKeybinding(
                    "重做",
                    null,
                    (cfg) => cfg.Keys.Redo,
                    (cfg, val) => cfg.Keys.Redo = val)
                .AddKeybinding(
                    "剪切",
                    null,
                    (cfg) => cfg.Keys.Cut,
                    (cfg, val) => cfg.Keys.Cut = val)
                .AddKeybinding(
                    "复制",
                    null,
                    (cfg) => cfg.Keys.Copy,
                    (cfg, val) => cfg.Keys.Copy = val)
                .AddKeybinding(
                    "粘贴",
                    null,
                    (cfg) => cfg.Keys.Paste,
                    (cfg, val) => cfg.Keys.Paste = val)
                .AddKeybinding(
                    "删除",
                    null,
                    (cfg) => cfg.Keys.Delete,
                    (cfg, val) => cfg.Keys.Delete = val)
                .AddKeybinding(
                    "全选",
                    null,
                    (cfg) => cfg.Keys.SelectAll,
                    (cfg, val) => cfg.Keys.SelectAll = val);
        }
    }
}
