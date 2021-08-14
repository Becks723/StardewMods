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
//    internal class WindBlock : Block, ISaveElement, ICustomObject
//    {
//        public WindBlock() : base()
//        {
//            Texture = Texture_Wind;
//            Name = ModEntry._i18n.Get("Wind_Name");
//            Menu = new SetSoundMenu(Labels_Wind);
//        }

//        public override string getDescription()
//        {
//            return ModEntry._i18n.Get("Wind_Description");
//        }
//        public override Item getOne()
//        {
//            return new WindBlock();
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
//            WindBlock w = new WindBlock();
//            return (ICustomObject)SetAdditionalDataToInstance(additionalSaveData, w, Labels_Wind);
//        }

//    }
//}