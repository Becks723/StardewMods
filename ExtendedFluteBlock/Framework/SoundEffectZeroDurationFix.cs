using System;
using System.Runtime.InteropServices;
using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;
using static FAudio;

namespace FluteBlockExtension.Framework
{
    /// <summary>Fix an error that a SoundEffect's Duration property is always 0. This error is specific in SDV's custom Monogame impl.</summary>
    internal class SoundEffectZeroDurationFix
    {
        private readonly Harmony _harmony;
        private readonly IMonitor _monitor;

        public SoundEffectZeroDurationFix(Harmony harmony, IMonitor monitor)
        {
            this._harmony = harmony;
            this._monitor = monitor;
        }

        public void ApplyFix()
        {
            var harmony = this._harmony;
            harmony.Patch(
                original: AccessTools.Method(typeof(SoundEffect), "PlatformLoadAudioStream"),
                postfix: new HarmonyMethod(typeof(SoundEffectZeroDurationFix), nameof(SoundEffect_PlatformLoadAudioStream_Postfix))
            );
        }

        private static void SoundEffect_PlatformLoadAudioStream_Postfix(ref TimeSpan duration, IntPtr ___formatPtr, FAudioBuffer ___handle)
        {
            FAudioWaveFormatEx fAudioWaveFormatEx = Marshal.PtrToStructure<FAudioWaveFormatEx>(___formatPtr);

            // add '1.0 *' make ulong calculation to double calculation.
            duration = TimeSpan.FromSeconds((double)(1.0 * (ulong)___handle.AudioBytes / (ulong)((long)((int)fAudioWaveFormatEx.nChannels * Math.Max((int)(fAudioWaveFormatEx.wBitsPerSample / 8), 1))) / (ulong)fAudioWaveFormatEx.nSamplesPerSec));
        }
    }
}
