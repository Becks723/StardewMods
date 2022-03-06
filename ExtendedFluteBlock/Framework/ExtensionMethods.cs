using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SObject = StardewValley.Object;
using static FluteBlockExtension.Framework.Constants;

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
        public static string GetExtraPitchStr(this SObject obj)
        {
            if (obj.modData.TryGetValue(FluteBlockModData_ExtraPitch, out string extraPitchStr))
            {
                return extraPitchStr;
            }
            else
            {
                throw new KeyNotFoundException();
            }
        }

        /// <summary>Gets the value of flute block's custom 'extraPitch' field.</summary>
        public static int GetExtraPitch(this SObject obj)
        {
            return int.Parse(GetExtraPitchStr(obj));
        }

        /// <summary>Sets flute block's custom 'extraPitch' field.</summary>
        public static void SetExtraPitch(this SObject obj, int value)
        {
            obj.modData[FluteBlockModData_ExtraPitch] = value.ToString();
        }

        /// <summary>Gets the flute block's pitch.</summary>
        public static int GetPitch(this SObject obj)
        {
            return obj.preservedParentSheetIndex.Value + GetExtraPitch(obj);
        }

        /// <summary>Sets the flute block's pitch.</summary>
        /// <param name="value">The new pitch.</param>
        public static void SetPitch(this SObject obj, int value)
        {
            switch (value)
            {
                case > 2300:
                    obj.preservedParentSheetIndex.Value = 2300;
                    obj.SetExtraPitch(value - 2300);
                    break;

                case < 0:
                    obj.preservedParentSheetIndex.Value = 0;
                    obj.SetExtraPitch(value);
                    break;

                default:
                    obj.preservedParentSheetIndex.Value = value;
                    obj.SetExtraPitch(0);
                    break;
            }
        }
    }
}