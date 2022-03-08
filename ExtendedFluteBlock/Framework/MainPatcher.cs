using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using static FluteBlockExtension.Framework.Constants;
using SObject = StardewValley.Object;

namespace FluteBlockExtension.Framework
{
    internal static class MainPatcher
    {
        /// <summary>对应<see cref="ModConfig.MinAccessiblePitch"/></summary>
        public static int MinPitch = MIN_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE;

        /// <summary>对应<see cref="ModConfig.MaxAccessiblePitch"/></summary>
        public static int MaxPitch = MAX_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE;

        private static IMonitor _monitor;

        private static Harmony _harmony;

        private static bool _patched;

        private static readonly Solution2 _corePatcher = new();

        /// <summary>Call this before call patch.</summary>
        public static void Prepare(Harmony harmony, IMonitor monitor)
        {
            _harmony = harmony;
            _monitor = monitor;
        }

        /// <summary>Toggle patch on.</summary>
        public static void Patch()
        {
            if (_patched) return;

            if (_harmony != null)
            {
                _corePatcher.Patch(_harmony);
                _patched = true;
            }
        }

        /// <summary>Toggle patch off.</summary>
        public static void Unpatch()
        {
            if (!_patched) return;

            if (_harmony != null)
            {
                _corePatcher.Unpatch(_harmony);
                _patched = false;
            }
        }

        /// <summary>Core patch solution.</summary>
        private class Solution2
        {
            public void Patch(Harmony harmony)
            {
                // always patch
                harmony.Patch(
                    original: AccessTools.Method(typeof(SObject), nameof(SObject.checkForAction)),
                    prefix: new HarmonyMethod(typeof(Solution2), nameof(SObject_checkForAction_Prefix_Always))
                );

                // change flute block play logic when player walks by.
                harmony.Patch(
                    original: AccessTools.Method(typeof(SObject), nameof(SObject.farmerAdjacentAction)),
                    prefix: new HarmonyMethod(typeof(Solution2), nameof(SObject_farmerAdjacentAction_Prefix))
                );
                // change flute block play and pitching logic when player right clicks.
                harmony.Patch(
                    original: AccessTools.Method(typeof(SObject), nameof(SObject.checkForAction)),
                    prefix: new HarmonyMethod(typeof(Solution2), nameof(SObject_checkForAction_Prefix))
                );
                // bypass throw exception when assigning Pitch with value out of range.
                harmony.Patch(
                    original: AccessTools.Method(typeof(SoundEffectInstance), "set_Pitch"),
                    transpiler: new HarmonyMethod(typeof(Solution2), nameof(SoundEffectInstance_Pitch_Transpiler))
                );
                // unlock the FAudio max frequency limited by game.
                harmony.Patch(
                    original: AccessTools.Method(typeof(SoundEffectInstance), "PlatformPlay"),
                    transpiler: new HarmonyMethod(typeof(Solution2), nameof(SoundEffectInstance_PlatformPlay_Transpiler))
                );
            }

            public void Unpatch(Harmony harmony)
            {
                harmony.Unpatch(
                    original: AccessTools.Method(typeof(SObject), nameof(SObject.farmerAdjacentAction)),
                    patch: AccessTools.Method(typeof(Solution2), nameof(SObject_farmerAdjacentAction_Prefix))
                );
                harmony.Unpatch(
                    original: AccessTools.Method(typeof(SObject), nameof(SObject.checkForAction)),
                    patch: AccessTools.Method(typeof(Solution2), nameof(SObject_checkForAction_Prefix))
                );
                harmony.Unpatch(
                    original: AccessTools.Method(typeof(SoundEffectInstance), "set_Pitch"),
                    patch: AccessTools.Method(typeof(Solution2), nameof(SoundEffectInstance_Pitch_Transpiler))
                );
                harmony.Unpatch(
                    original: AccessTools.Method(typeof(SoundEffectInstance), "PlatformPlay"),
                    patch: AccessTools.Method(typeof(Solution2), nameof(SoundEffectInstance_PlatformPlay_Transpiler))
                );
            }

            private static void SObject_checkForAction_Prefix_Always(SObject __instance, Farmer who, bool justCheckingForActivity)
            {
                try
                {
                    if (__instance.isTemporarilyInvisible)
                        return;

                    if (__instance.name is FluteBlockName)
                    {
                        if (justCheckingForActivity)
                            return;

                        // 如果在扩展音域功能关闭的前提下，玩家又右键调音了。那么有可能的情况是：
                        // 如果音高比原版音域低，如-1000，此时preservedParentSheetIndex是-1000，
                        // 按照原版计算公式（x = (x + 100) % 2400）可得，新preservedParentSheetIndex为-900，甚至不在范围内。
                        // 我们要让新值为0，因此就有了下面的检查代码。
                        if (!_patched)
                        {
                            // 检查旧值，让它不小于-100，这样调音后就变成0。
                            if (__instance.preservedParentSheetIndex.Value < -100)
                            {
                                __instance.preservedParentSheetIndex.Value = -100;
                                _monitor.Log($"{nameof(SObject_checkForAction_Prefix_Always)}: Fixed a pitch mismatch when mod off. {SuffixSObjectInfo(__instance, who.currentLocation)}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    AssertPatchFailure(ex);
                }
            }

            private static bool SObject_checkForAction_Prefix(SObject __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
            {
                try
                {
                    if (__instance.isTemporarilyInvisible)
                    {
                        __result = true;
                        return false;
                    }

                    if (__instance.IsFluteBlock())
                    {
                        if (justCheckingForActivity)
                        {
                            __result = true;
                            return false;
                        }

                        int oldPitch = __instance.preservedParentSheetIndex.Value;
                        _monitor.Log($"{nameof(SObject_checkForAction_Prefix)}: {SuffixSObjectInfo(__instance, who.currentLocation)}");
                        _monitor.Log($"{nameof(SObject_checkForAction_Prefix)}: Before tuning, {nameof(__instance.preservedParentSheetIndex)}: {oldPitch}.");

                        if (who.currentLocation is IslandSouthEast)  // extra pitch is not available at island South-east
                        {
                            _monitor.Log($"{nameof(SObject_checkForAction_Prefix)}: Detected farmer at {nameof(IslandSouthEast)}.");
                            __instance.preservedParentSheetIndex.Value = (oldPitch + 100) % 2400;
                        }
                        else
                        {
                            __instance.preservedParentSheetIndex.Value = CalculateNextPitch(oldPitch);
                        }

                        int newPitch = __instance.preservedParentSheetIndex.Value;
                        _monitor.Log($"{nameof(SObject_checkForAction_Prefix)}: After tuning, {nameof(__instance.preservedParentSheetIndex)}: {newPitch}.");

                        if (__instance.internalSound != null)
                        {
                            __instance.internalSound.Stop(AudioStopOptions.Immediate);
                            __instance.internalSound = Game1.soundBank.GetCue("flute");
                        }
                        else
                        {
                            __instance.internalSound = Game1.soundBank.GetCue("flute");
                        }

                        var (gamePitch, extraPitch) = __instance.SeperatePitch();
                        __instance.internalSound.SetVariable("Pitch", gamePitch);
                        __instance.internalSound.Pitch = extraPitch / 1200f;

                        __instance.internalSound.Play();
                        __instance.scale.Y = 1.3f;
                        __instance.shakeTimer = CalculateShakeTimer(
                            (newPitch - 1200) / 100
                        );

                        __result = true;
                        return false;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    AssertPatchFailure(ex);
                    return true;
                }
            }

            private static bool SObject_farmerAdjacentAction_Prefix(SObject __instance, GameLocation location)
            {
                try
                {
                    if (__instance.isTemporarilyInvisible)
                        return false;

                    if (__instance.IsFluteBlock() && (__instance.internalSound == null || ((int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds - __instance.lastNoteBlockSoundTime >= 1000 && !__instance.internalSound.IsPlaying)) && !Game1.dialogueUp)
                    {
                        __instance.internalSound = Game1.soundBank.GetCue("flute");

                        var (gamePitch, extraPitch) = __instance.SeperatePitch();
                        __instance.internalSound.SetVariable("Pitch", gamePitch);
                        __instance.internalSound.Pitch = extraPitch / 1200f;

                        __instance.internalSound.Play();
                        __instance.scale.Y = 1.3f;
                        __instance.shakeTimer = CalculateShakeTimer(
                            (__instance.preservedParentSheetIndex.Value - 1200) / 100
                        );
                        __instance.lastNoteBlockSoundTime = (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;

                        if (location is IslandSouthEast ise)
                            ise.OnFlutePlayed(gamePitch);

                        return false;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    AssertPatchFailure(ex);
                    return true;
                }
            }

            private static IEnumerable<CodeInstruction> SoundEffectInstance_Pitch_Transpiler(IEnumerable<CodeInstruction> oldInstructions)
            {
                // original:
                //  set
                //  {
                //      if (!this._isXAct && (value < -1f || value > 1f))
                //      {
                //          throw new ArgumentOutOfRangeException();
                //      }
                //      this._pitch = value;
                //      this.PlatformSetPitch(value);
                //  }
                // ----------------------------------------------------------------
                // after:
                //  set
                //  {
                //      this._pitch = value;
                //      this.PlatformSetPitch(value);
                //  }

                var result = new List<CodeInstruction>();

                bool adding = false;
                var instructions = oldInstructions.ToArray();
                for (int i = 0; i < instructions.Length; i++)
                {
                    if (adding)
                    {
                        result.Add(instructions[i]);
                        continue;
                    }

                    if (instructions[i + 2].opcode == OpCodes.Stfld &&
                        instructions[i + 2].operand is FieldInfo { Name: "_pitch" })
                    {
                        adding = true;
                        result.Add(instructions[i]);
                    }
                }

                return result;
            }

            private static IEnumerable<CodeInstruction> SoundEffectInstance_PlatformPlay_Transpiler(IEnumerable<CodeInstruction> oldInstructions)
            {
                var instructions = oldInstructions.ToArray();
                for (int i = 0; i < instructions.Length; i++)
                {
                    if (i >= 2 &&
                        instructions[i - 2].operand is FieldInfo { Name: "formatPtr" })
                    {
                        instructions[i].operand = 8f;       // C6 ~ C9  3 octaves  2^3
                                                            // now SoundEffectInstance::Pitch ranges from [-10, 3], no exception thrown if outofrange
                    }

                    yield return instructions[i];
                }
            }

            /// <summary>Get the next pitch when player right clicks on a flute block.</summary>
            private static int CalculateNextPitch(int currentPitch)
            {
                int max = MaxPitch;
                int min = MinPitch;

                // a 100 is a semitone.
                currentPitch += 100;

                // clamp.
                if (currentPitch > max || currentPitch < min)
                    currentPitch = min;

                //_monitor.Log($"Tuning to {currentPitch}.");

                return currentPitch;
            }

            /// <summary>Get the vibration duration (in milliseconds) of flute block after it plays a sound.</summary>
            /// <param name="deltaSemitone">Delta semitone. (Base pitch: C6)</param>
            private static int CalculateShakeTimer(int deltaSemitone)
            {
                SoundEffect flute = Game1.waveBank.GetSoundEffect(FluteTrackIndex);
                double baseDuration = flute.Duration.TotalMilliseconds;
                double octaves = deltaSemitone / 12.0;
                return (int)(baseDuration * Math.Pow(2, -octaves));
            }

            private static readonly Dictionary<string, Type> _runtimeTypes = new();
            private static Type FindMGType(string name)
            {
                if (_runtimeTypes.TryGetValue(name, out Type type))
                    return type;

                type = typeof(Cue).Assembly.GetTypes().Where(t => t.Name == name).FirstOrDefault();
                if (type == null)
                    throw new ArgumentException(nameof(name));
                _runtimeTypes[name] = type;
                return type;
            }

            private static string SuffixSObjectInfo(SObject obj, GameLocation? location)
            {
                return $"(At {obj.TileLocation}, {location?.Name ?? "Unknown location"})";
            }

            private static void AssertPatchFailure(Exception ex, [CallerMemberName] string methodName = "")
            {
                _monitor.Log($"Failed to patch {nameof(MainPatcher)}.{methodName}: {ex.Message}.\nStack trace: {ex.StackTrace}", LogLevel.Error);
            }
        }
    }
}