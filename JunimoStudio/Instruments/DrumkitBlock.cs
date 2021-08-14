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
//    class DrumkitBlock : Block, ICustomObject
//    {
//        public DrumkitBlock() : base()
//        {
//            this.Texture = Texture_Drumkit;
//            this.Name = ModEntry._i18n.Get("Drumkit_Name");
//            this.Menu = new SetSoundMenu(Labels_Drumkit);
//        }
//        public override string getDescription()
//        {
//            return ModEntry._i18n.Get("Drumkit_Description");
//        }
//        public override Item getOne()
//        {
//            return new DrumkitBlock();
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
//            DrumkitBlock d = new DrumkitBlock();
//            return (ICustomObject)SetAdditionalDataToInstance(additionalSaveData, d, Labels_Drumkit);
//        }

//    }
//}
