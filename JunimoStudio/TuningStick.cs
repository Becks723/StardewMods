using JunimoStudio.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Linq;
using System.Xml.Serialization;
using SObject = StardewValley.Object;
using JConstants = JunimoStudio.Core.Constants;

namespace JunimoStudio
{
    [XmlType("Mods_JunimoStudio.TuningStick")]
    public class TuningStick : Tool
    {
        private readonly Texture2D _texture;

        public TuningStick()
            : base(I18n.TuningStick_Name, 0, 0, 0, false)
        {
            this._texture = Textures.TuningStick;
            this.InstantUse = true;
        }

        public override Item getOne()
        {
            return new TuningStick();
        }

        public override bool canBeTrashed()
        {
            return true;
        }

        protected override string loadDisplayName()
        {
            return I18n.TuningStick_Name;
        }

        protected override string loadDescription()
        {
            return I18n.TuningStick_Description;
        }

        public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
        {
            Vector2 tile = new Vector2(x / 64, y / 64);
            location.objects.TryGetValue(tile, out SObject obj);

            if (obj is NoteBlock noteBlock)
                this.LowerBySemitone(noteBlock);

            who.CanMove = true;
        }

        public override void drawInMenu(SpriteBatch b, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            b.Draw(
                this._texture, 
                location + new Vector2(32f, 32f), 
                new Rectangle(0, 0, 16, 16), 
                color * transparency, 0f, 
                new Vector2(8f, 8f), 
                4f * scaleSize, 
                SpriteEffects.None, 
                layerDepth);
        }

        public void RaiseBySemitone(NoteBlock noteBlock)
        {
            this.TuningCore(noteBlock, 1);
        }

        public void LowerBySemitone(NoteBlock noteBlock)
        {
            this.TuningCore(noteBlock, -1);
        }

        private void TuningCore(NoteBlock noteBlock, int semitones)
        {
            noteBlock.StopImmediately();
            noteBlock.EnsureOneNote();
            INote noteToTune =
                (from note in noteBlock.ChannelManager.Channels.First().Notes
                 orderby note.Start
                 select note).First();

            // 保证值在范围以内。
            int minPitch = JConstants.MinNoteNumber;
            int maxPitch = JConstants.MaxNoteNumber;

            int result = noteToTune.Number + semitones;
            result = Math.Min(result, maxPitch);
            result = Math.Max(minPitch, result);

            // 如果到最低了，赋最高；如果到最高了，赋最低。
            if (result == minPitch)
                result = maxPitch;
            else if (result == maxPitch)
                result = minPitch;

            noteToTune.Number = result;
        }
    }
}
