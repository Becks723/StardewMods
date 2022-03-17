using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeShared.Utils;
using FluteBlockExtension.Framework;
using FluteBlockExtension.Framework.Integrations;
using FluteBlockExtension.Framework.Models;
using FluteBlockExtension.Framework.Patchers;
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

        private SoundsConfig _soundsConfig;

        private readonly string _soundsKey = "sounds";

        private readonly SoundManager _soundManager = new SoundManager();

        private readonly List<ProblemFluteBlock> _problemFluteBlocks = new();

        private List<ProblemFluteBlock>.Enumerator _problemFluteBlocksIterator;

        public static string ModID { get; private set; }

        public override void Entry(IModHelper helper)
        {
            // init translation.
            I18n.Init(helper.Translation);

            // init misc data.
            ModID = this.ModManifest.UniqueID;
            FluteBlockModData_ExtraPitch = $"{ModID}/extraPitch";
            SoundsConfig.DefaultSoundsFolderPath = Path.Combine(helper.DirectoryPath, "assets", "sounds");

            // read mod config.
            this._config = helper.ReadConfig<ModConfig>();
            this._config.UpdatePitches();

            // read sounds config.
            this.ReadSoundsConfig();

            // init Harmony.
            var harmony = new HarmonyLib.Harmony(ModID);

            // fix soundeffect duration error.
            new SoundEffectZeroDurationFix(harmony, this.Monitor).ApplyFix();

            // customize keyboardDispatcher.
            Game1.keyboardDispatcher.Cleanup();
            Game1.keyboardDispatcher = new KeyboardDispatcherPlus(Game1.game1.Window);

            // patch.
            MainPatcher.Prepare(
                harmony,
                this._config,
                this.Monitor,
                new SoundFloorMapper(this._soundsConfig.SoundFloorPairs, this._soundManager),
                this._soundManager
            );
            MainPatcher.Patch();


            // load sounds.
            this._soundManager.LoadSounds(this._soundsConfig);

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.Saving += this.OnSaving;
            helper.Events.World.ObjectListChanged += this.OnObjectListChanged;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            new GMCMIntegration(
                config: () => this._config,
                soundsConfig: () => this._soundsConfig,
                reset: this.ResetConfig,
                save: this.SaveConfig,
                modRegistry: this.Helper.ModRegistry,
                monitor: this.Monitor,
                manifest: this.ModManifest
            ).Integrate();

            new SaveAnywhereIntegration(
                beforeSave: this.OnSaving,
                modRegistry: this.Helper.ModRegistry,
                monitor: this.Monitor
            ).Integrate();
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            var fluteBlocks = from loc in GameHelper.GetLocations()
                              from pair in loc.objects.Pairs
                              where pair.Value.IsFluteBlock()
                              select new { Tile = pair.Key, FluteBlock = pair.Value, Location = loc };
            foreach (var obj in fluteBlocks)
            {
                SObject fluteBlock = obj.FluteBlock;

                if (!fluteBlock.modData.ContainsKey(FluteBlockModData_ExtraPitch))
                {
                    fluteBlock.SetExtraPitch(0);
                    this.Monitor.Log($"Detected flute block missing 'extraPitch' key. Added.");
                    return;
                }

                // try merge the two pitches on loading.
                // check if problem, add problem ones to to-fix list.
                if (!fluteBlock.TryMergePitch(out int? gamePitch, out int? extraPitch))
                {
                    this._problemFluteBlocks.Add(new ProblemFluteBlock(fluteBlock, obj.Tile, obj.Location));
                    this.Monitor.Log($"Detected flute block mismatched pitch. gamePitch: {gamePitch.Value}; extraPitch: {extraPitch.Value};");
                }
            }

            // open fix menu for first problem flute block.
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

        private void OnSaving(object sender, SavingEventArgs e)
        {
            this.OnSaving();
        }

        private void OnSaving()
        {
            var fluteBlocks = from loc in GameHelper.GetLocations()
                              from obj in loc.objects.Values
                              where obj.IsFluteBlock()
                              select obj;
            foreach (SObject fluteBlock in fluteBlocks)
            {
                fluteBlock.VerifyPitchForSave();
            }
        }

        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            foreach (var pair in e.Added)
            {
                SObject obj = pair.Value;
                if (obj.IsFluteBlock())
                {
                    obj.SetPitch(this._config.MinAccessiblePitch);
                    this.Monitor.Log($"A flute block is placed. Set its pitch to {obj.GetPitch()}.");
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
            this._config = new ModConfig();
            this._config.UpdatePitches();
            this._soundsConfig = new SoundsConfig();
        }

        private void SaveConfig()
        {
            this._config.UpdatePitches();
            this.Helper.WriteConfig(this._config);
            this.Helper.Data.WriteGlobalData(this._soundsKey, this._soundsConfig);
        }

        private void ReadSoundsConfig()
        {
            var helper = this.Helper;
            this._soundsConfig = helper.Data.ReadGlobalData<SoundsConfig>(this._soundsKey);

            // if sounds file not exists, create a new.
            if (this._soundsConfig is null)
            {
                this._soundsConfig = new SoundsConfig();
                helper.Data.WriteGlobalData(this._soundsKey, this._soundsConfig);
            }

            // recover built-in sounds if lost.
            var pairs = this._soundsConfig.SoundFloorPairs;
            var builtInSounds = SoundsConfig.BuiltInSoundFloorPairs.Select(p => p.Sound).ToArray();
            var currentSounds = pairs.Select(p => p.Sound).ToArray();

            foreach (var sound in builtInSounds)
            {
                if (!currentSounds.Contains(sound))
                {
                    pairs.Add(new SoundFloorMapItem() { Sound = sound, Floor = FloorData.Empty });
                }
            }
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
                    fluteBlock.preservedParentSheetIndex.Value = fluteBlock.GetExtraPitch() > 0 ? 2300 : 0;
                    break;
            }

            fluteBlock.TryMergePitch(out _, out _);
        }
    }
}
