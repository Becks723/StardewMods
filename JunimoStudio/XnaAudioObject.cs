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
//    public class XnaAudioObject 
//    {
//        private readonly IMonitor _monitor;
//        protected readonly IAudioEngine _audioEngine;
//        protected readonly ISoundBank[] _soundBanks;
//        protected readonly WaveBank[] _waveBanks;

//        public string Name { get; }

//        public XnaAudioObject(IMonitor monitor, string name, string settingsFilePath, string[] waveBankPaths, string[] soundBankPaths)
//            : this(monitor, settingsFilePath, waveBankPaths, soundBankPaths)
//        {
//            Name = name;
//        }

//        public XnaAudioObject(IMonitor monitor, string settingsFilePath, string[] waveBankPaths, string[] soundBankPaths)
//        {
//            _monitor = monitor;
//            try
//            {
//                _audioEngine = new IdentifiableAudioEngine(settingsFilePath);
//            }
//            catch (Exception e)
//            {
//                monitor.Log(e.Message, LogLevel.Error);
//                _audioEngine = new DummyAudioEngine();
//            }
//            Name = _audioEngine.GetName();
//            _waveBanks = new WaveBank[waveBankPaths.Length];
//            _soundBanks = new ISoundBank[soundBankPaths.Length];

//            try
//            {

//                var engine = _audioEngine.Engine;
//                for (int i = 0; i < waveBankPaths.Length; i++)
//                {
//                    _waveBanks[i] = new WaveBank(engine, waveBankPaths[i]);
//                }
//                for (int i = 0; i < soundBankPaths.Length; i++)
//                {
//                    _soundBanks[i] = new IdentifiableSoundBank(_audioEngine, soundBankPaths[i]);
//                }
//            }
//            catch (Exception e)
//            {
//                monitor.Log(e.Message, LogLevel.Error);
//                var dummy = new DummySoundBank();
//                for (int i = 0; i < soundBankPaths.Length; i++)
//                    _soundBanks[i] = dummy;
//            }
//        }

//        public XnaAudioObject(IMonitor monitor, string name, IAudioEngine audioEngine, WaveBank[] waveBanks, ISoundBank[] soundBanks)
//        {
//            _monitor = monitor;
//            Name = name;
//            _audioEngine = audioEngine;
//            _waveBanks = waveBanks;
//            _soundBanks = soundBanks;
//        }

//        internal virtual bool TryFindSoundBank(string soundBankName, out ISoundBank soundBank)
//        {
//            foreach (ISoundBank sb in _soundBanks)
//            {
//                if (sb.GetName() == soundBankName)
//                {
//                    soundBank = sb;
//                    return true;
//                }
//            }

//            soundBank = null;
//            return false;
//        }
//    }
//}
