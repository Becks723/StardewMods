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
    internal class PatchSolution1
    {
        public void Patch(Harmony harmony)
        {
            new Solution1().Patch(harmony);
        }

        private class Solution1
        {
            /// <remarks>
            ///                           patch targets               |             patch type            |                 patch summary
            ///              --------------------------------------------------------------------------------------------------------------------------------------------
            ///                     Object::farmerAdjacentAction      |    prefix                         |    1. re-evalute shakeTimer; 2. clamp value to [0, 2400] in IslandSouthEast::OnFlutePlayed;
            ///  |                  Object::checkForAction            |    transpiler                     |    1. re-evalute preservedParentSheetIndex; 2. re-evalute shakeTimer;
            ///  |  from            Cue::SetVariable                  |    transpiler                     |    skip [MinValue, MaxValue] clamp to handle special case for "flute"
            ///  |  high-level      Cue::UpdateRpcCurves              |    transpiler or simply prefix    |    handle special case for "flute"
            ///  |  to              SoundEffectInstance.Pitch::set    |    transpiler                     |    skip new pitch range checks.
            ///  |  low-level       SoundEffectInstance::PlatformPlay |    transpiler                     |    replace '2f' with larger num (max 10f) (5th para in FAudio.FAudio_CreateSourceVoice, see https://github.com/FNA-XNA/FAudio/blob/ac6a4e46b1d55c4cd380d1328d835808f5379ca0/include/FAudio.h#L608)     
            ///  V                                                                                             Note: FAudio range: [1f / 1024f, 1024f]
            ///                                                                                                    Corresponding MG range: [-10f, 10f]
            /// 
            ///  y-coord: Relative value of pitch
            ///        every 100 represents one semitone;
            ///        so every 1200 represents one octave.
            ///
            ///                 ^
            ///                 |
            ///            1200 |- --  --  --  --  --  --  --  --  --  --  ---
            ///                 |                                           /|
            ///                 |                                        /   
            ///                 |                                     /      |
            ///                 |                                   /        
            ///                 |                                /           |
            ///                 |                              /             
            ///                 |                           /                |
            ///                 |                        /               
            ///    -------------+----------------------+---------------------+--------------------------------------------------------------->
            ///               0 |                    /                      2400                             x-coord: In-game XACT Cue variable 'Pitch' 
            ///                 |                 /    1200                                                                          (Default: 1200;  
            ///                 |               /                                                                                      Min: 0;
            ///                 |            /                                                                                         Max: 2400)
            ///                 |         /          
            ///                 |       /            
            ///                 |    /             
            ///                 | /               
            ///           -1200 |-                     
            ///                 |
            ///                 
            /// Goal of the patch is to make flute block pitch range widen to that of an 88-key piano.
            /// 
            ///                                     |    Default    |     Min     |    Max
            /// -------------------------------------------------------------------------------------------
            ///             Flute block             |     0 (C5)    |    0 (C5)   |  2400 (C7) (Acutally 2300 B6, since game code did '% 2400')
            /// (Object::preservedParentSheetIndex) |               |             |
            ///                                     |               |             |
            ///             88-key Piano            |     0 (C5)    |  -3900 (A1) |  4800 (C9)
            ///                                     |               |             |
            /// 
            /// </remarks>
            public void Patch(Harmony harmony)
            {
                harmony.Patch(
                    original: AccessTools.Method(typeof(SObject), nameof(SObject.farmerAdjacentAction)),
                    prefix: new HarmonyMethod(typeof(Solution1), nameof(SObject_farmerAdjacentAction_Prefix))
                );
                harmony.Patch(
                    original: AccessTools.Method(typeof(SObject), nameof(SObject.checkForAction)),
                    prefix: new HarmonyMethod(typeof(Solution1), nameof(SObject_checkForAction_Prefix)),
                    postfix: new HarmonyMethod(typeof(Solution1), nameof(SObject_checkForAction_Postfix))
                );
                harmony.Patch(
                    original: AccessTools.Method(typeof(Cue), nameof(Cue.SetVariable)),
                    //prefix: new HarmonyMethod(typeof(Solution1), nameof(Cue_SetVariable_Prefix)),
                    //postfix: new HarmonyMethod(typeof(Solution1), nameof(Cue_SetVariable_Postfix)),
                    transpiler: new HarmonyMethod(typeof(Solution1), nameof(Cue_SetVariable_Transpiler))
                );
                harmony.Patch(
                    original: AccessTools.Method(typeof(Cue), "UpdateRpcCurves"),
                    prefix: new HarmonyMethod(typeof(Solution1), nameof(Cue_UpdateRpcCurves_Prefix)),
                    postfix: new HarmonyMethod(typeof(Solution1), nameof(Cue_UpdateRpcCurves_Postfix)),
                    transpiler: new HarmonyMethod(typeof(Solution1), nameof(Cue_UpdateRpcCurves_Transpiler))
                );
                harmony.Patch(
                    original: AccessTools.Method(typeof(SoundEffectInstance), "set_Pitch"),
                    prefix: new HarmonyMethod(typeof(Solution1), nameof(SoundEffectInstance_Pitch_Prefix)),
                    postfix: new HarmonyMethod(typeof(Solution1), nameof(SoundEffectInstance_Pitch_Postfix)),
                    transpiler: new HarmonyMethod(typeof(Solution1), nameof(SoundEffectInstance_Pitch_Transpiler))
                );
                harmony.Patch(
                    original: AccessTools.Method(typeof(SoundEffectInstance), "PlatformPlay"),
                    transpiler: new HarmonyMethod(typeof(Solution1), nameof(SoundEffectInstance_PlatformPlay_Transpiler))
                );
            }

            private static void SObject_checkForAction_Postfix(SObject __instance, bool justCheckingForActivity)
            {
                if (__instance.name is FluteBlockName && !justCheckingForActivity)
                {
                    _monitor.VerboseLog($"{nameof(SObject_checkForAction_Postfix)} called. __instance.preservedParentSheetIndex: {__instance.preservedParentSheetIndex.Value}.");
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

                        if (who?.currentLocation is IslandSouthEast)  // extra pitch is not available at island SE
                        {
                            _monitor.VerboseLog($"{nameof(SObject_checkForAction_Prefix)}: Detected farmer at {nameof(IslandSouthEast)}.");
                            __instance.preservedParentSheetIndex.Value = (__instance.preservedParentSheetIndex.Value + 100) % 2400;
                        }
                        else
                        {
                            int pitch = __instance.preservedParentSheetIndex.Value;
                            _monitor.VerboseLog($"{nameof(SObject_checkForAction_Prefix)}: Before calling {nameof(CalculateNewPitch)}. preservedParentSheetIndex.Value: {pitch}.");
                            __instance.preservedParentSheetIndex.Value = CalculateNewPitch(pitch);
                            _monitor.VerboseLog($"{nameof(SObject_checkForAction_Prefix)}: After calling {nameof(CalculateNewPitch)}. preservedParentSheetIndex.Value: {__instance.preservedParentSheetIndex.Value}.");
                        }

                        int realShakeTimer =
                            CalculateShakeTimer((__instance.preservedParentSheetIndex.Value - 1200) / 100);  // *** added
                        __instance.shakeTimer = realShakeTimer;                                                          // *** edited
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
                            __instance.internalSound.Play();
                        }
                        __instance.scale.Y = 1.3f;
                        __instance.shakeTimer = realShakeTimer;                                               // *** edited

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
                        int realShakeTimer = 200;                                                               // *** added
                        if (Game1.soundBank != null)
                        {
                            realShakeTimer = CalculateShakeTimer(
                                (__instance.preservedParentSheetIndex.Value - 1200) / 100);         // *** added

                            __instance.internalSound = Game1.soundBank.GetCue("flute");
                            __instance.internalSound.SetVariable("Pitch", __instance.preservedParentSheetIndex.Value);
                            __instance.internalSound.Play();
                        }
                        __instance.scale.Y = 1.3f;
                        __instance.shakeTimer = realShakeTimer;                                                 // *** edited
                        __instance.lastNoteBlockSoundTime = (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;

                        if (location is IslandSouthEast ise)
                            ise.OnFlutePlayed(Math.Clamp(__instance.preservedParentSheetIndex.Value, 0, 2400));

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

            private static void SoundEffectInstance_Pitch_Prefix(SoundEffectInstance __instance, float value)
            {
                //if (value < -1f || value > 1f)
                //{
                //    _monitor.Log($"SoundEffectInstance::set_Pitch prefix: Potential value out of range. value: {value}f");
                //}
            }

            private static void SoundEffectInstance_Pitch_Postfix(SoundEffectInstance __instance, float value)
            {
                //if (value < -1f || value > 1f)
                //{
                //    _monitor.Log($"SoundEffectInstance::set_Pitch postfix: value: {value}f; final value: {__instance.Pitch};");
                //}
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

            private static IEnumerable<CodeInstruction> Cue_UpdateRpcCurves_Transpiler(IEnumerable<CodeInstruction> oldInstructions, ILGenerator gen)
            {
                // current solution: add guard codes before original method.
                // code to add:
                //  if (_currentXactSound.trackIndex == 112)                            // 112 is "flute"'s unique offset in raw soundbank. (See https://stardewvalleywiki.com/Modding:Audio#Sound)
                //  {
                //  	RpcCurve rpcCurve = this._engine.RpcCurves[3];                  // this '3' is special for flute block, RPC curves: pitch, two rpc points: 1. Position = 0, Value = -1200; 2. Position = 2400, Value = 1200;
                //  	float varValue = this._variables[rpcCurve.Variable].Value;      // 'rpcCurve.Variable' here is 9. (See 'Variables Settings' form at https://github.com/Pathoschild/SMAPI/issues/467#issuecomment-519736572)
                //  	                                                                // 'varValue' is super than [0, 2400], to extend octaves.
                //  	float rate = (1200f - -1200) / (2400f - 0);                     // 1f
                //  	float rpcPitch = varValue * rate - 1200;                        // calculate final pitch.
                //  	rpcPitch /= 1200f;  
                //  	_rpcPitch = rpcPitch;                                           // no clamp here.
                //  	_UpdateSoundParameters();
                //  	return 1f;                                                      // since no volume here, just return 1f. (See original code logic)
                //  }

                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Cue), "_currentXactSound"));
                yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(XactSoundBankSound), nameof(XactSoundBankSound.trackIndex)));
                yield return new CodeInstruction(OpCodes.Ldc_I4_S, 112);
                yield return new CodeInstruction(OpCodes.Ceq);
                Label label_notSpecialCase = gen.DefineLabel();
                yield return new CodeInstruction(OpCodes.Brfalse_S, label_notSpecialCase);

                /* float varValue = this._variables[9].Value; */
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Cue), "_variables"));
                yield return new CodeInstruction(OpCodes.Ldc_I4, 9);
                Type typeofRpcVariable = FindMGType("RpcVariable");
                yield return new CodeInstruction(OpCodes.Ldelema, typeofRpcVariable);
                yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeofRpcVariable, "Value"));
                LocalBuilder local_varValue = gen.DeclareLocal(typeof(float));
                yield return new CodeInstruction(OpCodes.Stloc, local_varValue);

                /* float rate = (1200f - -1200) / (2400f - 0); */
                yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
                LocalBuilder local_rate = gen.DeclareLocal(typeof(float));
                yield return new CodeInstruction(OpCodes.Stloc, local_rate);

                /* float rpcPitch = varValue * rate - 1200; */
                yield return new CodeInstruction(OpCodes.Ldloc, local_varValue);
                yield return new CodeInstruction(OpCodes.Ldloc, local_rate);
                yield return new CodeInstruction(OpCodes.Mul);
                yield return new CodeInstruction(OpCodes.Ldc_R4, 1200f);
                yield return new CodeInstruction(OpCodes.Sub);
                LocalBuilder local_rpcPitch = gen.DeclareLocal(typeof(float));
                yield return new CodeInstruction(OpCodes.Stloc_S, local_rpcPitch);

                /* rpcPitch /= 1200f; */
                yield return new CodeInstruction(OpCodes.Ldloc_S, local_rpcPitch);
                yield return new CodeInstruction(OpCodes.Ldc_R4, 1200f);
                yield return new CodeInstruction(OpCodes.Div);
                yield return new CodeInstruction(OpCodes.Stloc_S, local_rpcPitch);

                /* _rpcPitch = rpcPitch; */
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldloc_S, local_rpcPitch);
                yield return new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(Cue), "_rpcPitch"));

                /* _UpdateSoundParameters(); */
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Cue), "_UpdateSoundParameters"));

                /* return 1f; */
                yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
                yield return new CodeInstruction(OpCodes.Ret);

                var instructions = oldInstructions.ToArray();
                for (int i = 0; i < instructions.Length; i++)
                {
                    CodeInstruction instr = instructions[i];

                    if (i == 0)
                    {
                        instr.labels.Add(label_notSpecialCase);
                    }

                    yield return instr;
                }
            }

            private static void Cue_UpdateRpcCurves_Prefix(Cue __instance, XactSoundBankSound ____currentXactSound)
            {
                //if (____currentXactSound.trackIndex == 112)
                //{
                //    _monitor.Log($"Detected patched Cue.UpdateRpcCurves called.");
                //}
            }

            private static void Cue_UpdateRpcCurves_Postfix(Cue __instance, XactSoundBankSound ____currentXactSound, float ____rpcPitch)
            {
                //if (____currentXactSound.trackIndex == 112)
                //{
                //    _monitor.Log($"_rpcPitch: {____rpcPitch}f");
                //}
            }

            private static void Cue_SetVariable_Prefix(Cue __instance, string name, float value)
            {
                //if (__instance.Name is "flute" && name is "Pitch")
                //{
                //    _monitor.Log($"Detect patched {nameof(Cue)}.{nameof(Cue.SetVariable)} called. value: {value}f");
                //}
            }

            private static void Cue_SetVariable_Postfix(Cue __instance, string name, float value)
            {
                //if (__instance.Name is "flute" && name is "Pitch")
                //{
                //    var audioEngine = Game1.audioEngine;
                //}
            }

            private static IEnumerable<CodeInstruction> Cue_SetVariable_Transpiler(IEnumerable<CodeInstruction> oldInstructions, ILGenerator gen)
            {
                // insert logic before the last line in original code:
                //
                //  the last line: 'this._variables[num].SetValue(value);'
                //  the inserted code: 
                // 
                //  if (this.Name is "flute")
                //  {
                //      this._variables[num].Value = value;
                //      return;
                //  }

                List<CodeInstruction> result = new();
                var instructions = oldInstructions.ToArray();
                for (int i = 0; i < instructions.Length; i++)
                {
                    //bool insertPoint = i >= 2
                    //    && instructions[i - 2].opcode == OpCodes.Newobj
                    //    && instructions[i - 2].operand is ConstructorInfo ctor
                    //    && ctor.DeclaringType == typeof(IndexOutOfRangeException);
                    //if (insertPoint)
                    //{
                    //    result.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    //    result.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Cue), "_name")));
                    //    result.Add(new CodeInstruction(OpCodes.Ldstr, "flute"));
                    //    result.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(string), "op_Equality")));
                    //    Label label_notFluteCase = gen.DefineLabel();
                    //    result.Add(new CodeInstruction(OpCodes.Brfalse_S, label_notFluteCase));
                    //    instructions[i].labels.Add(label_notFluteCase);

                    //    result.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    //    result.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Cue), "_variables")));
                    //    result.Add(new CodeInstruction(OpCodes.Ldc_I4, 9));
                    //    result.Add(new CodeInstruction(OpCodes.Ldelema, GetType("RpcVariable")));
                    //    result.Add(new CodeInstruction(OpCodes.Ldarg_2));
                    //    result.Add(new CodeInstruction(OpCodes.Stfld, AccessTools.Field(GetType("RpcVariable"), "Value")));
                    //    result.Add(new CodeInstruction(OpCodes.Ret));
                    //}
                    if (instructions[i].opcode == OpCodes.Call && instructions[i].operand is MethodInfo { Name: "SetValue" })
                    {
                        instructions[i] = new CodeInstruction(OpCodes.Stfld, AccessTools.Field(FindMGType("RpcVariable"), "Value"));
                    }
                    result.Add(instructions[i]);
                }

                return result;
            }

        }

    }
}
