using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace JunimoStudio.ModIntegrations.GenericModConfigMenu
{
    public static class IGenericModConfigMenuApiEx
    {
        /// <summary>
        /// 最简单的文本框。
        /// </summary>
        /// <param name="options"></param>
        /// <remarks>
        /// get和set中参数<see cref="object"/>可支持的类型有：
        /// <see cref="bool"/>,
        /// <see cref="int"/>,
        /// <see cref="float"/>,
        /// <see cref="string"/>,
        /// <see cref="SButton"/>,
        /// <see cref="KeybindList"/>。<br/>
        /// 其中<see cref="bool"/>显示成复选框。
        /// </remarks>
        public static void RegisterSimpleOption<T>(this IGenericModConfigMenuApi gmcmApi, IManifest mod, string optionName, string optionDesc, Func<T> optionGet, Action<T> optionSet)
        {
            if (typeof(T) == typeof(bool))
            {
                gmcmApi.RegisterSimpleOption(mod, optionName, optionDesc, () => (bool)(object)optionGet(), (b) => optionSet((T)(object)b));
            }
            else if (typeof(T) == typeof(int))
            {
                gmcmApi.RegisterSimpleOption(mod, optionName, optionDesc, () => (int)(object)optionGet(), (i) => optionSet((T)(object)i));
            }
            else if (typeof(T) == typeof(float))
            {
                gmcmApi.RegisterSimpleOption(mod, optionName, optionDesc, () => (float)(object)optionGet(), (f) => optionSet((T)(object)f));
            }
            else if (typeof(T) == typeof(string))
            {
                gmcmApi.RegisterSimpleOption(mod, optionName, optionDesc, () => (string)(object)optionGet(), (s) => optionSet((T)(object)s));
            }
            else if (typeof(T) == typeof(SButton))
            {
                gmcmApi.RegisterSimpleOption(mod, optionName, optionDesc, () => (SButton)(object)optionGet(), (s) => optionSet((T)(object)s));
            }
            else if (typeof(T) == typeof(KeybindList))
            {
                gmcmApi.RegisterSimpleOption(mod, optionName, optionDesc, () => (KeybindList)(object)optionGet(), (k) => optionSet((T)(object)k));
            }
        }

    }
}
