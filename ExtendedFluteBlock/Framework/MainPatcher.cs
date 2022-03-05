using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;
using SObject = StardewValley.Object;
using static FluteBlockExtension.Framework.Constants;
using StardewValley;
using StardewValley.Locations;

namespace FluteBlockExtension.Framework
{
    internal static class MainPatcher
    {
        public static int MinPitch = MIN_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE;

        public static int MaxPitch = MAX_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE;

        private static IMonitor _monitor;

        private static Harmony _harmony;

        private static bool _enablePatch;

        private static readonly Solution2 _corePatcher = new();

        public static void Prepare(Harmony harmony, IMonitor monitor)
        {
            _harmony = harmony;
            _monitor = monitor;
        }

        public static void Patch()
        {
            if (_enablePatch) return;

            if (_harmony != null)
            {
                _corePatcher.Patch(_harmony);
                _enablePatch = true;
            }
        }

        public static void Unpatch()
        {
            if (!_enablePatch) return;

            if (_harmony != null)
            {
                _corePatcher.Unpatch(_harmony);
                _enablePatch = false;
            }
        }

        private class Solution2
        {
            public void Patch(Harmony harmony)
            {
                // always patch
                harmony.Patch(
                    original: AccessTools.Method(typeof(SObject), nameof(SObject.checkForAction)),
                    prefix: new HarmonyMethod(typeof(Solution2), nameof(SObject_checkForAction_Prefix_Always))
                );

                harmony.Patch(
                    original: AccessTools.Method(typeof(SObject), nameof(SObject.farmerAdjacentAction)),
                    prefix: new HarmonyMethod(typeof(Solution2), nameof(SObject_farmerAdjacentAction_Prefix))
                );
                harmony.Patch(
                    original: AccessTools.Method(typeof(SObject), nameof(SObject.checkForAction)),
                    prefix: new HarmonyMethod(typeof(Solution2), nameof(SObject_checkForAction_Prefix))
                );
                harmony.Patch(
                    original: AccessTools.Method(typeof(SoundEffectInstance), "set_Pitch"),
                    transpiler: new HarmonyMethod(typeof(Solution2), nameof(SoundEffectInstance_Pitch_Transpiler))
                );
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

                        if (!_enablePatch)
                        {
                            int extraPitch = __instance.GetExtraPitch();
                            if (extraPitch < 0)
                            {
                                __instance.SetExtraPitch(0);
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

                    if (__instance.name is FluteBlockName)
                    {
                        if (justCheckingForActivity)
                        {
                            __result = true;
                            return false;
                        }

                        _monitor.Log($"{nameof(SObject_checkForAction_Prefix)}: {SuffixSObjectInfo(__instance, who?.currentLocation)}");
                        _monitor.Log($"{nameof(SObject_checkForAction_Prefix)}: Before tuning, {nameof(__instance.preservedParentSheetIndex)}: {__instance.preservedParentSheetIndex.Value}; extraPitch: {__instance.modData[FluteBlockModData_ExtraPitch]}.");

                        int extraPitch = 0;
                        int newPitch = 1200;
                        if (who?.currentLocation is IslandSouthEast)  // extra pitch is not available at island SE
                        {
                            _monitor.Log($"{nameof(SObject_checkForAction_Prefix)}: Detected farmer at {nameof(IslandSouthEast)}.");
                            newPitch = __instance.preservedParentSheetIndex.Value = (__instance.preservedParentSheetIndex.Value + 100) % 2400;
                        }
                        else
                        {
                            newPitch = CalculateNewPitch(
                                __instance.preservedParentSheetIndex.Value + __instance.GetExtraPitch()
                            );
                            if (newPitch > 2300)
                            {
                                __instance.preservedParentSheetIndex.Value = 2300;
                                extraPitch = newPitch - 2300;
                            }
                            else if (newPitch < 0)
                            {
                                __instance.preservedParentSheetIndex.Value = 0;
                                extraPitch = newPitch;
                            }
                            else
                            {
                                __instance.preservedParentSheetIndex.Value = newPitch;
                            }
                        }
                        __instance.SetExtraPitch(extraPitch);

                        _monitor.Log($"{nameof(SObject_checkForAction_Prefix)}: After tuning, {nameof(__instance.preservedParentSheetIndex)}: {__instance.preservedParentSheetIndex.Value}; extraPitch: {__instance.modData[FluteBlockModData_ExtraPitch]}.");

                        int realShakeTimer =
                            CalculateShakeTimer((newPitch - 1200) / 100
                        );
                        __instance.shakeTimer = realShakeTimer;
                        if (Game1.soundBank != null)
                        {
                            if (__instance.internalSound != null)
                            {
                                __instance.internalSound.Stop(AudioStopOptions.Immediate);
                                __instance.internalSound = Game1.soundBank.GetCue("flute");
                            }
                            else
                            {
                                __instance.internalSound = Game1.soundBank.GetCue("flute");
                            }
                            __instance.internalSound.SetVariable("Pitch", __instance.preservedParentSheetIndex.Value);
                            __instance.internalSound.Pitch = extraPitch / 1200f;
                            __instance.internalSound.Play();
                        }
                        __instance.scale.Y = 1.3f;
                        __instance.shakeTimer = realShakeTimer;

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

                    if (__instance.name == FluteBlockName && (__instance.internalSound == null || ((int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds - __instance.lastNoteBlockSoundTime >= 1000 && !__instance.internalSound.IsPlaying)) && !Game1.dialogueUp)
                    {
                        int realShakeTimer = 200;
                        int preservedParentSheetIndex = __instance.preservedParentSheetIndex.Value;
                        int extraPitch = 0;

                        if (Game1.soundBank != null)
                        {
                            int realPitch;
                            if (preservedParentSheetIndex > 0 && preservedParentSheetIndex < 2300)      // 正常
                            {
                                __instance.SetExtraPitch(0);
                                realPitch = preservedParentSheetIndex;
                            }
                            else        //  0 or 2300
                            {
                                extraPitch = __instance.GetExtraPitch();
                                realPitch = preservedParentSheetIndex + extraPitch;
                            }

                            realShakeTimer = CalculateShakeTimer(
                                (realPitch - 1200) / 100
                            );

                            __instance.internalSound = Game1.soundBank.GetCue("flute");
                            __instance.internalSound.SetVariable("Pitch", preservedParentSheetIndex);
                            __instance.internalSound.Pitch = extraPitch / 1200f;
                            __instance.internalSound.Play();
                        }
                        __instance.scale.Y = 1.3f;
                        __instance.shakeTimer = realShakeTimer;
                        __instance.lastNoteBlockSoundTime = (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;

                        if (location is IslandSouthEast ise)
                            ise.OnFlutePlayed(preservedParentSheetIndex);

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

            private static int CalculateNewPitch(int currentPitch)
            {
                int max = MaxPitch;
                int min = MinPitch;

                //// 超出边界，通常是用户修改了音高范围后又对某个在边界之外的笛块调音。
                //if (currentPitch < min || currentPitch > max)
                //    return min;

                currentPitch += 100;
                if (currentPitch > max || currentPitch < min)
                    currentPitch = min;
                //_monitor.Log($"Tuning to {currentPitch}.");
                return currentPitch;
            }

            private static int CalculateShakeTimer(int deltaSemitone)
            {
                SoundEffect flute = Game1.waveBank.GetSoundEffect(FluteBlockTrackIndex);
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