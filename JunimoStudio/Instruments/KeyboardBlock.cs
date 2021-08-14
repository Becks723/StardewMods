//using System.Collections.Generic;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using StardewValley;
//using StardewModdingAPI;
//using System;
//using StardewValley.Objects;
//using PyTK.CustomElementHandler;
//using JunimoStudio.UI;

//namespace JunimoStudio.Instruments
//{
//    internal class KeyboardBlock : Block, ISaveElement, ICustomObject
//    {
//        public KeyboardBlock() : base()
//        {
//            this.Texture = Texture_Keyboard;
//            this.Name = ModEntry._i18n.Get("Keyboard_Name");
//            this.Menu = new SetSoundMenu(Labels_Keyboard);
//        }
//        public override string getDescription()
//        {
//            return ModEntry._i18n.Get("Keyboard_Description");
//        }
//        public override Item getOne()
//        {
//            return new KeyboardBlock();
//        }
//        public override object getReplacement()
//        {
//            return base.getReplacement();
//        }
//        public override Dictionary<string, string> getAdditionalSaveData()
//        {
//            return base.getAdditionalSaveData();
//        }
//        public override void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
//        {
//            base.rebuild(additionalSaveData, replacement);
//        }
//        public ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
//        {
//            return (ICustomObject)SetAdditionalDataToInstance(additionalSaveData, new KeyboardBlock(), Labels_Keyboard);
//        }
//    }
//}
