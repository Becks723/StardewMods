using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Xna.Framework.Audio;
using StardewValley;
using static FluteBlockExtension.Framework.Constants;
using SObject = StardewValley.Object;

namespace FluteBlockExtension.Framework
{
    internal static class ExtensionMethods
    {
        /// <summary>Whether this <see cref="SObject"/> instance is a flute block.</summary>
        public static bool IsFluteBlock(this SObject obj)
        {
            // In game decompiled source, only name is checked. So keep same. ParentSheetIndex is dismissed.
            return obj.name == FluteBlockName /*&& obj.ParentSheetIndex == FluteBlockSheetIndex*/;
        }

        /// <summary>Gets string form of flute block's custom 'extraPitch' field.</summary>
        public static string GetExtraPitchStr(this SObject fluteBlock)
        {
            if (fluteBlock.modData.TryGetValue(FluteBlockModData_ExtraPitch, out string extraPitchStr))
            {
                return extraPitchStr;
            }
            else
            {
                fluteBlock.SetExtraPitch(0);
                return 0.ToString();
            }
        }

        /// <summary>Gets the value of flute block's custom 'extraPitch' field.</summary>
        public static int GetExtraPitch(this SObject fluteBlock)
        {
            return int.Parse(GetExtraPitchStr(fluteBlock));
        }

        /// <summary>Sets flute block's custom 'extraPitch' field.</summary>
        public static void SetExtraPitch(this SObject fluteBlock, int value)
        {
            fluteBlock.modData[FluteBlockModData_ExtraPitch] = value.ToString();
        }

        /// <summary>Gets the flute block's pitch in game.</summary>
        public static int GetPitch(this SObject fluteBlock)
        {
            return fluteBlock.preservedParentSheetIndex.Value;
        }

        /// <summary>Sets the flute block's pitch in game.</summary>
        /// <param name="value">The new pitch.</param>
        public static void SetPitch(this SObject fluteBlock, int value)
        {
            fluteBlock.preservedParentSheetIndex.Value = value;
        }

        /// <summary>Seperate the two pitch parts, game pitch & extra pitch.</summary>
        public static (int gamePitch, int extraPitch) SeperatePitch(this SObject fluteBlock)
        {
            int pitch = GetPitch(fluteBlock);
            return pitch switch
            {
                > 2300 => (2300, pitch - 2300),
                < 0 => (0, pitch),
                _ => (pitch, 0),
            };
        }

        /// <summary>When saving, seperate two pitch parts.</summary>
        public static void VerifyPitchForSave(this SObject fluteBlock)
        {
            var (gamePitch, extraPitch) = SeperatePitch(fluteBlock);

            fluteBlock.preservedParentSheetIndex.Value = gamePitch;
            fluteBlock.SetExtraPitch(extraPitch);
        }

        /// <summary>When loading a save, merge two pitch parts.</summary>
        /// <returns>Whether successfully merged.</returns>
        public static bool TryMergePitch(this SObject fluteBlock,
            [NotNullWhen(false)] out int? gamePitch,
            [NotNullWhen(false)] out int? extraPitch)
        {
            gamePitch = fluteBlock.preservedParentSheetIndex.Value;
            extraPitch = GetExtraPitch(fluteBlock);

            if (extraPitch != 0 &&
                gamePitch != 0 && gamePitch != 2300)
            {
                return false;
            }

            fluteBlock.preservedParentSheetIndex.Value = gamePitch.Value + extraPitch.Value;
            gamePitch = extraPitch = null;
            return true;
        }


        /// <summary>Remove a cue by name.</summary>
        /// <param name="name">The unique cue name.</param>
        /// <returns>true if removed successfully, otherwise false.</returns>
        public static bool RemoveCue(this ISoundBank soundBank, string name)
        {
            var cues = GetSoundBankCues(soundBank);

            return cues?.Remove(name) ?? false;
        }

        /// <summary>Rename a cue.</summary>
        /// <param name="name">The unique cue name.</param>
        /// <param name="newName">The new cue name.</param>
        /// <returns>true if found and renamed successfully, otherwise false.</returns>
        public static bool RenameCue(this ISoundBank soundBank, string name, string newName)
        {
            var cues = GetSoundBankCues(soundBank);
            if (cues != null)
            {
                if (cues.TryGetValue(name, out CueDefinition cue))
                {
                    return false;
                }

                cues.Remove(name);
                cues[newName] = cue;
                return true;
            }

            return false;
        }

        /// <summary>List all cues as <see cref="CueDefinition"/> in a soundbank.</summary>
        public static IEnumerable<CueDefinition> GetAllCues(this ISoundBank soundBank)
        {
            var cues = GetSoundBankCues(soundBank);
            return cues.Values;
        }

        private static Dictionary<string, CueDefinition> GetSoundBankCues(this ISoundBank soundBank)
        {
            return object.ReferenceEquals(soundBank, Game1.soundBank)
                ? _gameSoundBankCues.Value
                : ReflectSoundBankCues(soundBank);
        }

        private static readonly Lazy<Dictionary<string, CueDefinition>> _gameSoundBankCues = new(() => ReflectSoundBankCues(Game1.soundBank));
        private static Dictionary<string, CueDefinition> ReflectSoundBankCues(ISoundBank soundBank)
        {
            if (soundBank is SoundBankWrapper wrapper)
            {
                SoundBank wrappedBank = wrapper.GetType().GetField("soundBank", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(wrapper) as SoundBank;
                if (wrappedBank != null)
                {
                    return wrappedBank.GetType().GetField("_cues", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(wrappedBank) as Dictionary<string, CueDefinition>;
                }
            }

            return null;
        }
    }
}