using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System;
using Microsoft.Xna.Framework;
using StardewValley.Locations;
using JunimoStudio.Patches;
using System.Threading;
using System.Globalization;
using JunimoStudio.Menus;
using JunimoStudio.Core;
using JunimoStudio.Core.Plugins.Instruments;
using GuiLabs.Undo;
using HarmonyLib;
using JunimoStudio.ModIntegrations.SaveAnywhere;
using JunimoStudio.ModIntegrations.SpaceCore;
using JunimoStudio.ModIntegrations.GenericModConfigMenu;
using StardewModdingAPI.Utilities;

namespace JunimoStudio
{
    internal class ModEntry : Mod
    {
        private readonly string SpaceCoreUniqueId = "spacechase0.SpaceCore";

        private readonly string SaveAnywhereUniqueId = "Omegasis.SaveAnywhere";

        private readonly string SaveConfigKey = "save-config";

        private readonly ActionManager _actionManager = new ActionManager();

        private ModConfig _config;

        private SaveConfig _saveConfig;

        private IChannelManager _testChannelManager;

        private readonly int BuffUniqueID = 58012398;

        private Texture2D _background;

        public override void Entry(IModHelper helper)
        {
            this._config = helper.ReadConfig<ModConfig>();
            I18n.Init(helper.Translation);
            Textures.LoadAll(helper.Content);
            NoteBlock.Init(this.Monitor, this._config, this._actionManager);

            this._testChannelManager = Factory.ChannelManager();
            this._testChannelManager.AddChannel("1", new MidiOutPlugin());
            this._testChannelManager.AddChannel("2", new MidiOutPlugin());

            this._actionManager.CollectionChanged += (_, _) =>
            {
            };

            Harmony harmony = new Harmony(this.ModManifest.UniqueID);
            GameLocationPatcher.Patch(harmony, this.Monitor, () => this._config.EnableTracks);

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.SaveCreated += this.OnSaveCreated;
            helper.Events.GameLoop.Saving += this.OnSaving;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            //helper.Events.Display.Rendered += (s, e) => OnRendered(Config, Game1.currentLocation, Game1.spriteBatch);
        }

        private void OnSaveCreated(object sender, SaveCreatedEventArgs e)
        {
            this._saveConfig = new SaveConfig();
            TrackManager.Init(this.Monitor, this._saveConfig);
            TrackManager.Register(new TrackInfo(Game1.getFarm(), new Rectangle(0, 0, 80, 65), true));
            NoteBlock.saveConfig = this._saveConfig;
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            this._saveConfig = this.Helper.Data.ReadSaveData<SaveConfig>(this.SaveConfigKey) ?? new SaveConfig();
            TrackManager.Init(this.Monitor, this._saveConfig);
            NoteBlock.saveConfig = this._saveConfig;

            foreach (GameLocation location in Utilities.GetLocations())
                foreach (NoteBlock noteBlock in location.objects.Values.OfType<NoteBlock>())
                    noteBlock.Restore();
        }

        private void OnSaving(object sender, EventArgs e)
        {
            this.Helper.Data.WriteSaveData<SaveConfig>(this.SaveConfigKey, this._saveConfig);
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var spaceApi = this.Helper.ModRegistry.GetApi<ISpaceCoreApi>(this.SpaceCoreUniqueId);
            if (spaceApi != null)
            {
                spaceApi.RegisterSerializerType(typeof(NoteBlock));
                spaceApi.RegisterSerializerType(typeof(TuningStick));
            }

            var saveAnywhereApi = this.Helper.ModRegistry.GetApi<ISaveAnywhereAPI>(this.SaveAnywhereUniqueId);
            if (saveAnywhereApi != null)
            {
                saveAnywhereApi.BeforeSave += this.OnSaving;
            }

            new GMCMJunimoStudioInteg(
                () => this._config,
                () =>
                {
                    this.Helper.WriteConfig(new ModConfig());
                    this._config = this.Helper.ReadConfig<ModConfig>();
                },
                () =>
                {
                    this.Helper.WriteConfig(this._config);
                    this._config = this.Helper.ReadConfig<ModConfig>();
                },
                this.Monitor,
                this.ModManifest,
                this.Helper.ModRegistry)
                .Register();
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            ShopMenu shop = e.NewMenu as ShopMenu;
            if (this.IsFurnitureCatalogue(shop))
            {
                var itemsForSale = new ISalable[]
                {
                    new NoteBlock(Vector2.Zero),
                    new TuningStick(),
                };
                foreach (ISalable item in itemsForSale)
                {
                    shop.itemPriceAndStock.Add(item, new int[2] { 0, int.MaxValue });
                    shop.forSale.Add(item);
                }
                return;
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            //if (!Context.IsPlayerFree || Game1.currentMinigame != null)
            //    return;
            //IChannelManager cm = Factory.ChannelManager();
            //cm.AddChannel("1", new MidiOutPlugin());
            //cm.AddChannel("2", new MidiOutPlugin());

            //if (e.Button == SButton.NumPad0)
            //{
            //    _testChannelManager = Factory.ChannelManager();
            //    _testChannelManager.AddChannel("1", new MidiOutPlugin());
            //    _testChannelManager.AddChannel("2", new MidiOutPlugin());
            //    Game1.soundBank.PlayCue("bigSelect");
            //}
            //else if (e.Button == SButton.NumPad1)
            //{
            //    Game1.activeClickableMenu = new PianoRollMenu(Monitor, "2", cm, _config,TimeSettings, _actionManager);
            //}
            //else if (e.Button == SButton.NumPad2)
            //{
            //    Game1.activeClickableMenu = new ChannelRackMenu(Monitor, _testChannelManager, _config, TimeSettings, _actionManager);
            //}
            //else if (e.Button == SButton.NumPad4)
            //{
            //}
            //else if (e.Button == SButton.NumPad5)
            //{
            //}
            //else if (e.Button == SButton.NumPad6)
            //{
            //}
            //else if (e.Button == SButton.NumPad7)
            //{
            //}
            //else if (e.Button == SButton.Left)
            //{
            //    if (_config.TicksPerQuarterNote >= 48)
            //        _config.TicksPerQuarterNote -= 24;
            //}
            //else if (e.Button == SButton.Right)
            //{
            //    if (_config.TicksPerQuarterNote <= 120)
            //        _config.TicksPerQuarterNote += 24;
            //}
            //else if (e.Button == SButton.Up)
            //{
            //    if (_config.TimeSignature.Denominator < 16)
            //        _config.TimeSignature.Denominator *= 2;
            //}
            //else if (e.Button == SButton.Down)
            //{
            //    if (_config.TimeSignature.Denominator > 2)
            //        _config.TimeSignature.Denominator /= 2;
            //}

            //else if (e.Button == SButton.OemMinus)
            //{
            //    INoteCollection notes = _testChannelManager.Channels["1"].Notes;
            //    int r = new Random().Next(0, notes.Count() - 1);
            //    _testChannelManager.Channels["1"].Notes.Remove(notes.ToArray()[r]);
            //    Game1.soundBank.PlayCue("coin");
            //}
            //else if (e.Button == SButton.OemPlus)
            //{
            //    _testChannelManager.Channels["1"].Notes.Add(pitch: new Random().Next(0, 131),
            //      start: new Random().Next(0, 1000),
            //       duration: new Random().Next(50, 480));
            //    Game1.soundBank.PlayCue("bigDeSelect");
            //}
            //else if (e.Button == SButton.H)
            //{
            //    foreach (IAction action in _actionManager.EnumUndoableActions())
            //    {
            //        if (action is INamedAction named)
            //            Monitor.Log($"{named.Name}", LogLevel.Debug);
            //        else
            //            Monitor.Log($"{action.GetType().Name}", LogLevel.Debug);
            //    }
            //    Monitor.Log("------------------------------------------------------", LogLevel.Debug);
            //}
        }

        private bool IsFurnitureCatalogue(ShopMenu shop)
        {
            return
                shop != null
                && shop.portraitPerson == null
                && shop.forSale.All(salable => salable is Furniture);
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            //    // ignore in cutscenes
            //    if (Game1.eventUp || !Context.IsWorldReady)
            //        return;

            //    // ignore if walking
            //    bool running = Game1.player.running;
            //    bool runEnabled = running || Game1.options.autoRun != Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), Game1.options.runButton); // auto-run enabled and not holding walk button, or disabled and holding run button
            //    if (!runEnabled)
            //        return;

            //    // add or update buff
            //    int buffId = this.BuffUniqueID;
            //    Buff buff = Game1.buffsDisplay.otherBuffs.FirstOrDefault(p => p.which == buffId);
            //    if (buff == null)
            //    {
            //        Game1.buffsDisplay.addOtherBuff(
            //            buff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, "Junimo Studio", "Junimo Studio") { which = buffId }
            //        );
            //    }
            //    buff.millisecondsDuration = 50;
        }

        private void OnRendered(ModConfig config, GameLocation location, SpriteBatch b)
        {
            //    //ignore when game isn't loaded.
            //    if (Game1.currentMinigame != null || Game1.eventUp || !Context.IsWorldReady)
            //        return;

            //    //ignore when sreenshot mode enabled.
            //    if (!Game1.displayHUD)
            //        return;

            //    bool isCave = location is MineShaft || location is FarmCave;
            //    IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), 5, isCave ? 100 : 70, this.Helper.Translation.LocaleEnum.Equals(LocalizedContentManager.LanguageCode.zh) ? 150 : 200, 85, Color.White, 1f, true);
            //    Utility.drawTextWithShadow(b, this.GetDisplayedPlayerTempo(config), Game1.smallFont, new Vector2(20, isCave ? 115 : 85), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
        }

        //private string GetDisplayedPlayerTempo(ModConfig config)
        //{
        //    return this.Helper.Translation.Get("Rendered_PlayerTempo", new { addedSpeed = (config.playerAbsoluteTempo >= 5 ? "+" : "-") + float.Parse(Math.Abs(config.playerAbsoluteTempo - 5).ToString("#0.00")).ToString() });
        //}
    }
}
