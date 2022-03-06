using System.Collections.Generic;
using System.Linq;
using CodeShared.Utils;
using FluteBlockExtension.Framework;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using static FluteBlockExtension.Framework.Constants;
using SObject = StardewValley.Object;

namespace FluteBlockExtension
{
    /// <summary></summary>
    /// <remarks>All notes referred to in this project have same middle C: C5</remarks>
    internal class ModEntry : Mod
    {
        private ModConfig _config;

        private readonly List<ProblemFluteBlock> _problemFluteBlocks = new();

        private List<ProblemFluteBlock>.Enumerator _problemFluteBlocksIterator;

        public override void Entry(IModHelper helper)
        {
            // init translation.
            I18n.Init(helper.Translation);

            // init custom data key.
            FluteBlockModData_ExtraPitch = string.Format("{0}/extraPitch", this.ModManifest.UniqueID);

            // init patcher.
            MainPatcher.Prepare(new HarmonyLib.Harmony(this.ModManifest.UniqueID), this.Monitor);

            // read mod config.
            this._config = helper.ReadConfig<ModConfig>();
            this._config.UpdatePitches();

            // 第一次生成配置文件，手动开/关patch
            if (this._config.EnableMod)
            {
                MainPatcher.Patch();
            }
            else
            {
                MainPatcher.Unpatch();
            }

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.World.ObjectListChanged += this.World_ObjectListChanged;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            new GMCMIntegration(
                config: this._config,
                reset: this.ResetConfig,
                save: this.SaveConfig,
                modRegistry: this.Helper.ModRegistry,
                monitor: this.Monitor,
                manifest: this.ModManifest
            ).Integrate();
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            var fluteBlocks = from loc in GameHelper.GetLocations()
                              from pair in loc.objects.Pairs
                              where pair.Value.name is FluteBlockName
                              select new { Tile = pair.Key, FluteBlock = pair.Value, Location = loc };
            foreach (var block in fluteBlocks)
            {
                SObject obj = block.FluteBlock;
                if (!obj.modData.TryGetValue(FluteBlockModData_ExtraPitch, out string extraPitchStr))
                {
                    obj.SetExtraPitch(0);
                    this.Monitor.Log($"Detected flute block missing 'extraPitch' key. Added.");
                }
                else
                {
                    int extraPitch = int.Parse(extraPitchStr);

                    // check if problem, add problem ones to to-fix list.
                    if (extraPitch != 0 &&
                        obj.preservedParentSheetIndex.Value != 0 && obj.preservedParentSheetIndex.Value != 2300)
                    {
                        this._problemFluteBlocks.Add(new ProblemFluteBlock(obj, block.Tile, block.Location));
                        this.Monitor.Log($"Detected flute block mismatched pitch. {nameof(obj.preservedParentSheetIndex)}: {obj.preservedParentSheetIndex}; extraPitch: {obj.modData[FluteBlockModData_ExtraPitch]};");
                    }
                }
            }

            if (this._problemFluteBlocks.Count > 0)
            {
                this._problemFluteBlocksIterator = this._problemFluteBlocks.GetEnumerator();
                this._problemFluteBlocksIterator.MoveNext();
                var cur = this._problemFluteBlocksIterator.Current;
                var fixMenu = new FixConflictMenu(cur);
                fixMenu.OptionSelected += this.FixMenu_OptionSelected;
                Game1.playSound("bigSelect");
                Game1.activeClickableMenu = fixMenu;
            }
        }

        private void World_ObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            foreach (var pair in e.Added)
            {
                SObject obj = pair.Value;
                if (obj.IsFluteBlock())
                {
                    obj.SetPitch(this._config.MinAccessiblePitch);

                    this.Monitor.Log($"A flute block is placed. Set its {nameof(obj.preservedParentSheetIndex)} to {obj.preservedParentSheetIndex}; extraPitch to {obj.GetExtraPitchStr()};");
                }
            }

            foreach (var pair in e.Removed)
            {
                SObject obj = pair.Value;
                if (obj.IsFluteBlock())
                {
                    obj.modData.Remove(FluteBlockModData_ExtraPitch);

                    this.Monitor.Log($"A flute block is removed. Delete its 'extraPitch' field.");
                }
            }
        }

        private void ResetConfig()
        {
            this._config = new ModConfig() { EnableMod = true };  // init property to run setter logic.
            this._config.UpdatePitches();
            this.SaveConfig();
        }

        private void SaveConfig()
        {
            this._config.UpdatePitches();
            this.Helper.WriteConfig(this._config);
        }

        private void FixMenu_OptionSelected(object sender, FixOptionSelectedEventArgs e)
        {
            (sender as FixConflictMenu).OptionSelected -= this.FixMenu_OptionSelected;

            this.FixConflict(e.FluteBlock, e.Option);
            if (e.Always)
            {
                while (this._problemFluteBlocksIterator.MoveNext())
                {
                    this.FixConflict(this._problemFluteBlocksIterator.Current.Core, e.Option);
                }
                this._problemFluteBlocks.Clear();
                return;
            }

            if (this._problemFluteBlocksIterator.MoveNext())
            {
                var cur = this._problemFluteBlocksIterator.Current;
                var fixMenu = new FixConflictMenu(cur);
                fixMenu.OptionSelected += this.FixMenu_OptionSelected;
                Game1.playSound("bigSelect");
                Game1.activeClickableMenu = fixMenu;
            }
            else
            {
                this._problemFluteBlocks.Clear();
            }
        }

        private void FixConflict(SObject fluteBlock, FixOption option)
        {
            switch (option)
            {
                case FixOption.ApplyGamePitch:
                    fluteBlock.SetExtraPitch(0);
                    break;

                case FixOption.ApplyExtraPitch:
                    int extraPitch = fluteBlock.GetExtraPitch();
                    fluteBlock.preservedParentSheetIndex.Value = extraPitch > 0 ? 2300 : 0;
                    break;
            }
        }
    }
}
