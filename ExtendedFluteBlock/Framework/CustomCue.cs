using Microsoft.Xna.Framework.Audio;
using StardewValley;

namespace FluteBlockExtension.Framework
{
    /// <summary><see cref="ICue"/> implementation for <see cref="SoundType.CustomCue"/>.</summary>
    internal class CustomCue : ICue
    {
        private readonly ICue _cue;

        public CustomCue(ICue cue)
        {
            this._cue = cue;
        }

        public bool IsStopped => this._cue.IsStopped;

        public bool IsStopping => this._cue.IsStopping;

        public bool IsPlaying => this._cue.IsPlaying;

        public bool IsPaused => this._cue.IsPaused;

        public string Name => this._cue.Name;

        public float Pitch
        {
            get => this._cue.Pitch;
            set
            {
                CueDefinition cue = Game1.soundBank.GetCueDefinition(this.Name);
                foreach (XactSoundBankSound sound in cue.sounds)
                    sound.pitch = value;
            }
        }

        public float Volume
        {
            get => this._cue.Volume;
            set => this._cue.Volume = value;
        }

        public bool IsPitchBeingControlledByRPC => this._cue.IsPitchBeingControlledByRPC;

        public void Dispose()
        {
            this._cue.Dispose();
        }

        public float GetVariable(string var)
        {
            return this._cue.GetVariable(var);
        }

        public void Pause()
        {
            this._cue.Pause();
        }

        public void Play()
        {
            this._cue.Play();
        }

        public void Resume()
        {
            this._cue.Resume();
        }

        public void SetVariable(string var, int val)
        {
            this._cue.SetVariable(var, val);
        }

        public void SetVariable(string var, float val)
        {
            this._cue.SetVariable(var, val);
        }

        public void Stop(AudioStopOptions options)
        {
            this._cue.Stop(options);
        }
    }
}