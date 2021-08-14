using System;
using Microsoft.Xna.Framework.Audio;
using StardewValley;

namespace JunimoStudio
{
    internal class AudioEngineWrapper : IAudioEngine, IDisposable
    {
        public AudioEngineWrapper(AudioEngine engine)
        {
            this.Engine = engine;
        }

        public bool IsDisposed
        {
            get
            {
                return this.Engine.IsDisposed;
            }
        }

        public void Dispose()
        {
            this.Engine.Dispose();
        }

        public IAudioCategory GetCategory(string name)
        {
            return new AudioCategoryWrapper(this.Engine.GetCategory(name));
        }

        public void Update()
        {
            this.Engine.Update();
        }

        public AudioEngine Engine { get; }
    }
}
