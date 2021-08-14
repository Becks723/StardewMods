using Microsoft.Xna.Framework.Audio;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio
{
    internal class IdentifiableAudioEngine : IAudioEngine
    {
        private readonly string _path;

        public bool IsDisposed
        {
            get { return Engine.IsDisposed; }
        }

        public AudioEngine Engine { get; }

        public IdentifiableAudioEngine(string audioEnginePath)
        {
            _path = audioEnginePath;
            Engine = new AudioEngine(audioEnginePath);
        }

        public void Dispose()
        {
            Engine.Dispose();
        }

        public IAudioCategory GetCategory(string name)
        {
            return new AudioCategoryWrapper(Engine.GetCategory(name));
        }

        public void Update()
        {
            Engine.Update();
        }

        public string GetName()
        {
            return new FileInfo(_path).Name;
        }
    }
}
