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
        public static bool IsFluteBlock(this SObject obj)
        {
            return obj.name == FluteBlockName && obj.ParentSheetIndex == FluteBlockSheetIndex;
        }

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

        public static int GetExtraPitch(this SObject obj)
        {
            return int.Parse(GetExtraPitchStr(obj));
        }

        public static void SetExtraPitch(this SObject obj, int value)
        {
            obj.modData[FluteBlockModData_ExtraPitch] = value.ToString();
        }
    }
}