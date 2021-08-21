//using Microsoft.Xna.Framework.Audio;
//using StardewValley;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace JunimoStudio
//{
//    public class IdentifiableSoundBank : ISoundBank
//    {
//        private readonly IAudioEngine _parent;
//        private readonly SoundBank _soundBank;
//        private readonly string _path;

//        public bool IsInUse
//        {
//            get { return _soundBank.IsInUse; }
//        }

//        public bool IsDisposed
//        {
//            get { return _soundBank.IsDisposed; }
//        }

//        public IdentifiableSoundBank(IAudioEngine audioEngine, string soundBankPath)
//        {
//            _parent = audioEngine;
//            _path = soundBankPath;
//            _soundBank = new SoundBank(audioEngine.Engine, soundBankPath);
//        }

//        public void Dispose()
//        {
//            _soundBank.Dispose();
//        }

//        public ICue GetCue(string name)
//        {
//            return new CueWrapper(_soundBank.GetCue(name));
//        }

//        public void PlayCue(string name)
//        {
//            _soundBank.PlayCue(name);
//        }

//        public void PlayCue(string name, AudioListener listener, AudioEmitter emitter)
//        {
//            _soundBank.PlayCue(name, listener, emitter);
//        }

//        public string GetName()
//        {
//            return new FileInfo(_path).Name;
//        }

//        public IAudioEngine GetParent()
//        {
//            return _parent;
//        }
//    }
//}
