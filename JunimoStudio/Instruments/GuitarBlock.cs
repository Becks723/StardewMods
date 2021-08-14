//using Microsoft.Xna.Framework;
//using JunimoStudio.UI;
//using PyTK.CustomElementHandler;
//using StardewValley;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace JunimoStudio.Instruments
//{
//    class GuitarBlock : Block, ICustomObject
//    {
//        public GuitarBlock() : base()
//        {
//            this.Texture = Texture_Guitar;
//            this.Name = ModEntry._i18n.Get("Guitar_Name");
//            this.Menu = new SetSoundMenu(Labels_Guitar);
//        }
//        public override string getDescription()
//        {
//            return ModEntry._i18n.Get("Guitar_Description");
//        }
//        public override Item getOne()
//        {
//            return new GuitarBlock();
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
//            GuitarBlock g = new GuitarBlock();
//            return (ICustomObject)SetAdditionalDataToInstance(additionalSaveData, g, Labels_Guitar);
//        }
//    }
//}
