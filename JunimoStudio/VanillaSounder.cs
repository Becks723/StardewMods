using Microsoft.Xna.Framework.Audio;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio
{
    internal class VanillaSounder : ISounder
    {
        private readonly XnaAudioObject[] _audios;
        private readonly Dictionary<string, ICue> _toPlayList = new Dictionary<string, ICue>();

        public VanillaSounder(XnaAudioObject[] audios)
        {
            _audios = audios;
        }

        public void Play()
        {
            foreach (ICue cue in _toPlayList.Values)
            {
                if (!cue.IsPlaying)
                    cue.Play();
            }
        }

        public void StopAll()
        {
            var list = _toPlayList;
            foreach (ICue cue in list.Values.Where(c => !c.IsStopped && !c.IsStopping))
            {
                cue.Stop(AudioStopOptions.AsAuthored);
            }
            list.Clear();
        }

        public void Register(string key)
        {
            var list = _toPlayList;
            list ??= new();

            if (TryFindCue(key, out ICue cue))
            {
                if (list.ContainsKey(key))
                {
                    list[key] = cue;
                }
                else
                {
                    list.Add(key, cue);
                }
            }
        }

        public void Register(string[] keys)
        {
            var list = _toPlayList;
            list ??= new();

            foreach (string key in keys)
                if (TryFindCue(key, out ICue cue))
                {
                    if (list.ContainsKey(key))
                    {
                        list[key] = cue;
                    }
                    else
                    {
                        list.Add(key, cue);
                    }
                }
        }

        public void Stop(string key)
        {
            var list = _toPlayList;
            if (!list.ContainsKey(key))
                return;

            ICue cueToStop = list[key];
            if (!cueToStop.IsStopped && !cueToStop.IsStopping)
            {
                cueToStop.Stop(AudioStopOptions.AsAuthored);
                list.Remove(key);
            }
        }

        public void Stop(string[] keys)
        {
            foreach (string key in keys)
                Stop(key);
        }

        public void Unregister(string key)
        {
            _toPlayList.Remove(key);
        }

        public void Unregister(string[] keys)
        {
            foreach (string key in keys)
            {
                Unregister(key);
            }
        }

        public void SetVolume(string key, float value)
        {
            throw new NotImplementedException();
        }

        public void SetVelocity(string key, float value)
        {
            throw new NotImplementedException();
        }

        public void SetPan(string key, float value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Try to find a <see cref="ICue"/> <paramref name="cue"/> from <see cref="_audios"/>, according to its <paramref name="uniqueKey"/>.
        /// </summary>
        /// <param name="uniqueKey">
        /// Key pointing to the cue.
        /// <para>Syntax: "&lt;audioName&gt;.&lt;soundBankName&gt;.&lt;cueName&gt;"</para>
        /// <para> - audioName: <see cref="XnaAudioObject.Name"/>.</para>
        /// <para> - soundBankName: <see cref="ISoundBank.GetName"/>.</para>
        /// <para> - cueName: parameter for <see cref="ISoundBank.GetCue(string)"/>.</para>
        /// <para>-----------------------------------------------------------------------------</para>
        /// <para>Syntax: "&lt;audioName&gt;.&lt;cueName&gt;"</para>
        /// <para>- This syntax only works when all items in <see cref="_audios"/> are <see cref="BasicXnaAudioObject"/>.</para>
        /// </param>
        /// <param name="cue"></param>
        /// <returns>Whether a correctly matched cue is found.</returns>
        private bool TryFindCue(string uniqueKey, out ICue cue)
        {
            string audioName = null, soundBankName = null, cueName = null;

            var subs = uniqueKey.Split('.');

            if (_audios.All(a => a is BasicXnaAudioObject))
            {
                audioName = subs[0];
                cueName = subs[1];
            }
            else
            {
                audioName = subs[0];
                soundBankName = subs[1];
                cueName = subs[2];
            }

            if (TryFindAudioObject(audioName, out XnaAudioObject audioObj))
                if (audioObj.TryFindSoundBank(soundBankName, out ISoundBank soundBank))
                    try { cue = soundBank.GetCue(cueName); return true; }
                    catch { cue = new DummyCue(); return false; }

            { cue = new DummyCue(); return false; }
        }

        private bool TryFindAudioObject(string name, out XnaAudioObject audioObject)
        {
            foreach (XnaAudioObject audioObj in _audios)
                if (audioObj.Name == name)
                { audioObject = audioObj; return true; }

            { audioObject = null; return false; }
        }

    }
}
