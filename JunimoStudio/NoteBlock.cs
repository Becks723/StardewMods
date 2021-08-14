using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using GuiLabs.Undo;
using JunimoStudio.Core;
using JunimoStudio.Core.Plugins.Instruments;
using JunimoStudio.Menus;
using JunimoStudio.Menus.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace JunimoStudio
{
    //internal class NoteBlock : SObject
    //{
    //    private readonly INoteBlock _noteBlockImpl;
    //    private readonly Func<NoteBlock> _creator;

    //    public NoteBlock(INoteBlock noteBlockImpl, Func<NoteBlock> creator)
    //    {
    //        _noteBlockImpl = noteBlockImpl ?? throw new ArgumentNullException(nameof(noteBlockImpl));
    //        _creator = creator ?? throw new ArgumentNullException(nameof(creator));
    //    }

    //    public override void farmerAdjacentAction(GameLocation location)
    //    {
    //        _noteBlockImpl.ChannelManager.StartPlayback();
    //    }

    //    public override Item getOne()
    //    {
    //        return _creator.Invoke();
    //    }
    //}

    //internal class NoteBlock : SObject
    //{
    //    public const string NameForSaving = "JunimoStudio.NoteBlock";

    //    private readonly IChannelManager _channelManager;

    //    public NoteBlock(IChannelManager channelManager)
    //    {
    //        _channelManager = channelManager;
    //        //GetModData = () =>
    //        //{
    //        //    return new ModDataDictionary
    //        //    {
    //        //        { "Becks723.JunimoStudio" , JsonConvert.SerializeObject(_channelManager) }
    //        //    };
    //        //};
    //    }

    //    public override string DisplayName
    //    {
    //        get => I18n.NoteBlockDisplayName;
    //    }

    //    public override string getDescription()
    //    {
    //        return "";
    //    }

    //    public void CleanForSaving()
    //    {
    //        JsonConvert.SerializeObject(_channelManager);
    //    }

    //    public Func<Dictionary<string, string>> GetModData { get; private set; }
    //}

    [XmlType("Mods_JunimoStudio.NoteBlock")]
    public class NoteBlock : SObject
    {
        private readonly Texture2D _texture;

        private IPlayback _playBack;

        [XmlElement("channelManager")]
        public readonly NetRef<NetChannelManager> netChannelManager = new NetRef<NetChannelManager>();

        public override string DisplayName { get; set; }

        internal IChannelManager ChannelManager => this.netChannelManager.Value.Core;

        private static IMonitor _monitor;

        private static ModConfig _config;

        private static ActionManager _actionManager;

        internal static SaveConfig saveConfig;

        private bool _suppressSync = false;

        public NoteBlock()
        {
            // 这个无参的构造器是Xml反序列化时自动调用的。
            this.initNetFields();

            this._texture = Textures.NoteBlock;
        }

        public NoteBlock(Vector2 tile)
            : this()
        {
            this.name = I18n.NoteBlock_Name;
            this.DisplayName = I18n.NoteBlock_Name;
            this.Type = "Crafting";
            this.Price = 100;
            this.Edibility = -300;
            this.bigCraftable.Value = false;
            this.CanBeGrabbed = true;
            this.CanBeSetDown = true;
            this.isHoedirt.Value = false;
            this.IsSpawnedObject = false;
            this.Stack = 1;
            this.TileLocation = tile;
            this.boundingBox.Value = new Rectangle((int)tile.X * 64, (int)tile.Y * 64, 64, 64);

            this.netChannelManager.Value = new NetChannelManager();
            if (saveConfig != null)
                saveConfig.Time.PropertyChanged += this.TimeSettingsChangedCallback;
        }

        public void TimeSettingsChangedCallback(object sender, PropertyChangedEventArgs e)
        {
            // 当时间设置变化时，
            // 1. 更新音符中对应的时间设置。

            // 该音符块中所有的音符。
            var notes = this.ChannelManager.Channels.SelectMany(c => c.Notes);

            // 时间设置。
            ITimeBasedObject timeSettings = sender as ITimeBasedObject;

            if (e.PropertyName == nameof(ITimeBasedObject.TicksPerQuarterNote))
            {
                foreach (INote note in notes)
                    note.TicksPerQuarterNote = timeSettings.TicksPerQuarterNote;
            }
            else if (e.PropertyName == nameof(ITimeBasedObject.Bpm))
            {
                foreach (INote note in notes)
                    note.Bpm = timeSettings.Bpm;
            }
            else if (e.PropertyName == nameof(ITimeBasedObject.TimeSignature.Denominator))
            {
                foreach (INote note in notes)
                    note.TimeSignature.Denominator = timeSettings.TimeSignature.Denominator;
            }
            else if (e.PropertyName == nameof(ITimeBasedObject.TimeSignature.Numerator))
            {
                foreach (INote note in notes)
                    note.TimeSignature.Numerator = timeSettings.TimeSignature.Numerator;
            }
        }

        public override Item getOne()
        {
            return new NoteBlock(Vector2.Zero);
        }

        public override void farmerAdjacentAction(GameLocation location)
        {
            this.scale.Y = 1.3f;
            this.shakeTimer = 200;
            this._playBack ??= new ChannelPlayBack(saveConfig.Time, this.ChannelManager.Channels);
            this._playBack.StartPlayback();
        }

        public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
        {
            if (justCheckingForActivity)
                return true;

            if (who.CurrentTool is TuningStick tuning)
            {
                tuning.RaiseBySemitone(this);
            }
            else
            {
                Game1.activeClickableMenu = new ChannelRackMenu(
                    _monitor, this.ChannelManager, _config, saveConfig.Time, _actionManager, new MenuCursorRenderer(Textures.ExpandedCursors));
            }

            return true;
        }

        public override bool performToolAction(Tool t, GameLocation location)
        {
            if (t is not MeleeWeapon && t.isHeavyHitter())
            {
                location.playSound("hammer");
                location.debris.Add(
                    new Debris(new NoteBlock(Vector2.One), this.TileLocation * 64f + new Vector2(32f, 32f)));
                location.objects.Remove(this.TileLocation);
            }

            if (t is TuningStick tuning)
            {
                tuning.LowerBySemitone(this);
            }
            return false;
        }

        public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        {
            bool result = base.placementAction(location, x, y, who);
            this.boundingBox.Value = new Rectangle((int)this.TileLocation.X * 64, (int)this.TileLocation.Y * 64, 64, 64);
            return result;
        }

        public override string getDescription()
        {
            return I18n.NoteBlock_Description;
        }

        public override void drawInMenu(SpriteBatch b, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            bool shouldDrawStackNumber =
                ((drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive)
                && scaleSize > 0.3
                && this.Stack != int.MaxValue;
            if (drawShadow)
            {
                b.Draw(Game1.shadowTexture, location + new Vector2(32f, 48f), Game1.shadowTexture.Bounds, color * 0.5f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0001f);
            }
            b.Draw(this._texture, location + new Vector2((int)(32f * scaleSize), (int)(32f * scaleSize)), null, color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth);
            if (shouldDrawStackNumber)
            {
                Utility.drawTinyDigits(this.Stack, b, location + new Vector2(64 - Utility.getWidthOfTinyDigitString(this.Stack, 3f * scaleSize) + 3f * scaleSize, 64f - 18f * scaleSize + 1f), 3f * scaleSize, 1f, color);
            }
        }

        public override void draw(SpriteBatch b, int x, int y, float alpha = 1)
        {
            b.Draw(
                Game1.shadowTexture,
                Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32f, y * 64 + 53f)),
                Game1.shadowTexture.Bounds,
                Color.White, 0f,
                new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y),
                4f, SpriteEffects.None,
                this.getBoundingBox(new Vector2(x, y)).Bottom / 15000f);

            var shakeOffset = (this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0;
            b.Draw(
                this._texture,
                Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + shakeOffset, y * 64 + 32 + shakeOffset)),
                null,
                Color.White * alpha, 0f, new Vector2(8f, 8f), (this.scale.Y > 1f) ? this.getScale().Y : 4f,
                this.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                this.getBoundingBox(new Vector2(x, y)).Bottom / 10000f);
        }

        public override void drawWhenHeld(SpriteBatch b, Vector2 objectPosition, Farmer f)
        {
            b.Draw(
                this._texture,
                objectPosition,
                null,
                Color.White, 0f,
                Vector2.Zero,
                4f, SpriteEffects.None,
                Math.Max(0f, (f.getStandingY() + 3) / 10000f));
        }

        public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
        {
            if (!this._suppressSync)
                this.netChannelManager.Value.Update(time);
            base.updateWhenCurrentLocation(time, environment);
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            this.NetFields.AddFields(this.netChannelManager);
        }

        internal static void Init(IMonitor monitor, ModConfig config, ActionManager actionManager)
        {
            _monitor = monitor;
            _config = config;
            _actionManager = actionManager;
        }

        internal void EnsureOneNote()
        {
            var mgr = this.ChannelManager;
            IChannel targetChannel =
                mgr.Channels.Any()
                ? mgr.Channels.First()
                : mgr.AddChannel<MidiOutPlugin>();

            if (!targetChannel.Notes.Any())
                targetChannel.Notes.Add(
                    60, 0, saveConfig.Time.TicksPerQuarterNote);
        }

        internal void StopImmediately()
        {
            this._playBack?.Stop();
        }

        internal void Restore()
        {
            this._suppressSync = true;
            this.netChannelManager.Value?.RestoreCoreObject();
            this._suppressSync = false;
        }
    }
}
