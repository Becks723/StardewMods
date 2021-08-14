//using System.Collections.Generic;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using StardewValley;
//using System;
//using PyTK.CustomElementHandler;
//using StardewValley.Objects;
//using JunimoStudio.UI;

//namespace JunimoStudio.Instruments
//{
//    internal class StringBlock : Block, ISaveElement, ICustomObject
//    {
//        public StringBlock() : base()
//        {
//            Texture = Texture_String;
//            Name = ModEntry._i18n.Get("String_Name");
//            Menu = new SetSoundMenu(Labels_String);
//        }

//        public override string getDescription()
//        {
//            return ModEntry._i18n.Get("String_Description");
//        }
//        public override Item getOne()
//        {
//            return new StringBlock();
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
//            StringBlock s = new StringBlock();
//            return (ICustomObject)SetAdditionalDataToInstance(additionalSaveData, s, Labels_String);
//        }

//    }
//}