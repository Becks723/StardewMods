//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Audio;
//using PyTK.CustomElementHandler;
//using StardewModdingAPI;
//using StardewValley;

//namespace JunimoStudio.Instruments
//{
//    class CustomBlock : Block, ISaveElement, ICustomObject
//    {
//        internal static List<SoundEffectInstance> soundeffectInstances = new List<SoundEffectInstance>();
//        public static int soundeffectsCount;
//        public static string soundeffectsPath;
//        public CustomBlock() : base()
//        {
//            Texture = Texture_Custom;
//            Name = ModEntry._i18n.Get("Custom_Name");
//        }
//        public override string getDescription()
//        {
//            return ModEntry._i18n.Get("Custom_Description");
//        }
//        public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
//        {
//            if (justCheckingForActivity)
//                return true;
//            if (soundeffectInstances.Count == soundeffectsCount && soundeffectsCount > 0)
//            {
//                preservedParentSheetIndex.Value = (preservedParentSheetIndex.Value + 1) % soundeffectsCount;
//                shakeTimer = 200;
//                foreach (var soundeffectInstance in soundeffectInstances)
//                    soundeffectInstance.Stop();
//                soundeffectInstances[preservedParentSheetIndex.Value].Play();
//                scale.Y = 1.3f;
//                shakeTimer = 200;
//            }
//            return true;
//        }
//        public override void farmerAdjacentAction(GameLocation location)
//        {
//            if (soundeffectInstances.Count == soundeffectsCount && soundeffectsCount > 0 && Game1.currentGameTime.TotalGameTime.TotalMilliseconds - lastNoteBlockSoundTime >= 1000 && !Game1.dialogueUp)
//            {
//                soundeffectInstances[preservedParentSheetIndex.Value].Play();
//                scale.Y = 1.3f;
//                shakeTimer = 200;
//                lastNoteBlockSoundTime = (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
//            }
//        }
//        public override Item getOne()
//        {
//            return new CustomBlock();
//        }
//        public override object getReplacement()
//        {
//            return base.getReplacement();
//        }
//        public override Dictionary<string, string> getAdditionalSaveData()
//        {
//            Dictionary<string, string> saveData = new Dictionary<string, string>();
//            saveData.Add("tileLocation", TileLocation.ToString());
//            saveData.Add("stack", Stack.ToString());
//            saveData.Add("preservedParentSheetIndex", preservedParentSheetIndex.Value.ToString());
//            return saveData;
//        }
//        public override void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
//        {
//            base.rebuild(additionalSaveData, replacement);
//        }
//        public ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
//        {
//            CustomBlock c = new CustomBlock();
//            if (additionalSaveData.TryGetValue("tileLocation", out string data))
//                c.TileLocation = Utilities.StringToVector2(data);
//            else
//                c.TileLocation = Vector2.Zero;
//            if (additionalSaveData.TryGetValue("stack", out data))
//                c.Stack = Convert.ToInt32(data);
//            else
//                c.Stack = 1;
//            if (additionalSaveData.TryGetValue("preservedParentSheetIndex", out data))
//                c.preservedParentSheetIndex.Value = Convert.ToInt32(data);
//            else
//                c.preservedParentSheetIndex.Value = 0;
//            return c;
//        }

//    }
//}
