//using Microsoft.Xna.Framework.Audio;
//using StardewModdingAPI;
//using StardewValley;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace JunimoStudio
//{
//    /// <summary>
//    /// A simple xna audio object with only one soundbank and one wavebank.
//    /// </summary>
//    internal class BasicXnaAudioObject : XnaAudioObject
//    {
//        public BasicXnaAudioObject(IMonitor monitor, string settingsFilePath, string waveBankPath, string soundBankPath)
//            : base(monitor, settingsFilePath, new string[] { waveBankPath }, new string[] { soundBankPath })
//        {
//        }

//        public BasicXnaAudioObject(IMonitor monitor, string name, string settingsFilePath, string waveBankPath, string soundBankPath)
//            : base(monitor, name, settingsFilePath, new string[] { waveBankPath }, new string[] { soundBankPath })
//        {
//        }

//        public BasicXnaAudioObject(IMonitor monitor, string name, IAudioEngine audioEngine, WaveBank waveBank, ISoundBank soundBank)
//            : base(monitor, name, audioEngine, new WaveBank[] { waveBank }, new ISoundBank[] { soundBank })
//        {
//        }

//        internal override bool TryFindSoundBank(string soundBankName, out ISoundBank soundBank)
//        {
//            soundBank = _soundBanks.FirstOrDefault();
//            return true;
//        }
//    }
//}
