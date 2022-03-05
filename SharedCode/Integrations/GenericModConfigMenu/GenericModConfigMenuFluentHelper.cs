using System;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace CodeShared.Integrations.GenericModConfigMenu
{
    internal class GenericModConfigMenuFluentHelper
    {
        private readonly IGenericModConfigMenuApi _api;
        private readonly IManifest _manifest;

        public GenericModConfigMenuFluentHelper(IGenericModConfigMenuApi api, IManifest manifest)
        {
            this._api = api ?? throw new ArgumentNullException(nameof(api));
            this._manifest = manifest;
        }

        public GenericModConfigMenuFluentHelper Register(Action reset, Action save, bool titleScreenOnly = false)
        {
            this._api.Register(this._manifest, reset, save, titleScreenOnly);
            return this;
        }

        public GenericModConfigMenuFluentHelper Unregister()
        {
            this._api.Unregister(this._manifest);
            return this;
        }

        public GenericModConfigMenuFluentHelper SetTitleScreenOnlyForNextOptions(bool titleScreenOnly)
        {
            this._api.SetTitleScreenOnlyForNextOptions(this._manifest, titleScreenOnly);
            return this;
        }

        public GenericModConfigMenuFluentHelper AddPage(string pageId, Func<string> pageTitle = null)
        {
            this._api.AddPage(this._manifest, pageId, pageTitle);
            return this;
        }

        public GenericModConfigMenuFluentHelper AddPageLink(string pageId, Func<string> text, Func<string> tooltip = null)
        {
            this._api.AddPageLink(this._manifest, pageId, text, tooltip);
            return this;
        }

        public GenericModConfigMenuFluentHelper AddSectionTitle(Func<string> name, Func<string> tooltip = null)
        {
            this._api.AddSectionTitle(this._manifest, name, tooltip);
            return this;
        }

        public GenericModConfigMenuFluentHelper AddKeyBindList(Func<string> name, Func<KeybindList> get, Action<KeybindList> set, Func<string> tooltip = null)
        {
            this._api.AddKeybindList(
                mod: this._manifest,
                getValue: get,
                setValue: set,
                name: name,
                tooltip: tooltip
            );
            return this;
        }

        public GenericModConfigMenuFluentHelper AddTextBox(Func<string> name, Func<string> get, Action<string> set, Func<string> tooltip = null)
        {
            this._api.AddTextOption(
                mod: this._manifest,
                getValue: get,
                setValue: set,
                name: name,
                tooltip: tooltip,
                allowedValues: null,
                formatAllowedValue: null,
                fieldId: null
            );
            return this;
        }

        public GenericModConfigMenuFluentHelper AddDropDown(Func<string> name, Func<string> get, Action<string> set, string[] choices, Func<string, string> formattedChoices = null, Func<string> tooltip = null, Action<object> fieldChanged = null)
        {
            string fieldId = null;
            if (fieldChanged != null)
                fieldId = Guid.NewGuid().ToString();
            this._api.AddTextOption(
                mod: this._manifest,
                getValue: get,
                setValue: set,
                name: name,
                tooltip: tooltip,
                allowedValues: choices,
                formatAllowedValue: formattedChoices,
                fieldId: fieldId
            );
            if (fieldChanged != null)
                this._api.OnFieldChanged(this._manifest, (id, val) =>
                {
                    if (id == fieldId)
                        fieldChanged(val);
                });
            return this;
        }

        /// <summary>
        /// Add a drop down, 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="getIndex">Gets the index of value from mod config.</param>
        /// <param name="setIndex">Sets the index of a new value in mod config.</param>
        /// <param name="choices"></param>
        /// <param name="tooltip"></param>
        /// <param name="fieldChanged"></param>
        /// <returns></returns>
        public GenericModConfigMenuFluentHelper AddDropDown(Func<string> name, Func<int> getIndex, Action<int> setIndex, string[] choices, Func<string> tooltip = null, Action<int> fieldChanged = null)
        {
            this.AddDropDown(
                name: name,
                get: () => choices[getIndex()],
                set: val => setIndex(Array.IndexOf(choices, val)),
                choices: choices,
                tooltip: tooltip,
                fieldChanged: val => fieldChanged?.Invoke(Array.IndexOf(choices, val))
            );
            return this;
        }

        /// <summary>
        /// Add a drop down for a set of integer choices.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="get">Gets the integer value from mod config.</param>
        /// <param name="set">Sets a new integer value in mod config.</param>
        /// <param name="choices"></param>
        /// <param name="tooltip"></param>
        /// <param name="fieldChanged"></param>
        /// <returns></returns>
        public GenericModConfigMenuFluentHelper AddDropDown(Func<string> name, Func<int> get, Action<int> set, int[] choices, Func<string> tooltip = null)
        {
            this.AddDropDown(
                name: name,
                get: () => get().ToString(),
                set: val => set(int.Parse(val)),
                choices: choices.Select(i => i.ToString()).ToArray(),
                tooltip: tooltip,
                fieldChanged: null
            );
            return this;
        }

        public GenericModConfigMenuFluentHelper AddDropDown<TEnum>(Func<string> name, Func<TEnum> get, Action<TEnum> set, Func<TEnum, string> displayText = null, Func<string> tooltip = null, Action<TEnum> fieldChanged = null)
            where TEnum : struct, Enum
        {
            Func<string, string> formattedChoices = displayText is null
                ? null
                : str => displayText(Enum.Parse<TEnum>(str));
            this.AddDropDown(
                name: name,
                get: () => Enum.GetName(get()),
                set: val => set(Enum.Parse<TEnum>(val)),
                choices: Enum.GetNames<TEnum>(),
                tooltip: tooltip,
                formattedChoices: formattedChoices,
                fieldChanged: val => fieldChanged?.Invoke((TEnum)val)
            );
            return this;
        }

        public GenericModConfigMenuFluentHelper AddCheckbox(Func<string> name, Func<bool> get, Action<bool> set, Func<string> tooltip = null, Action<bool> fieldChanged = null)
        {
            string fieldId = null;
            if (fieldChanged != null)
                fieldId = Guid.NewGuid().ToString();
            this._api.AddBoolOption(
                mod: this._manifest,
                getValue: get,
                setValue: set,
                name: name,
                tooltip: tooltip,
                fieldId: fieldId
            );
            if (fieldChanged != null)
                this._api.OnFieldChanged(this._manifest, (id, val) =>
                {
                    if (id == fieldId)
                        fieldChanged((bool)val);
                });
            return this;
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <typeparam name="TEnum">Must marked with <see cref="FlagsAttribute"/>.</typeparam>
        ///// <param name="get"></param>
        ///// <param name="addFlag"></param>
        ///// <param name="removeFlag"></param>
        ///// <param name="tooltip"></param>
        ///// <param name="fieldChanged"></param>
        ///// <returns></returns>
        ///// <exception cref="ArgumentException"><typeparamref name="TEnum"/> doesn't have a <see cref="FlagsAttribute"/>.</exception>
        //public GenericModConfigMenuFluentHelper AddCheckboxes<TEnum>(Func<TEnum> get, Action<TEnum> addFlag, Action<TEnum> removeFlag, params EnumFlagLabel<TEnum>[] flagLabels)
        //    where TEnum : struct, Enum
        //{
        //    //if (!typeof(TEnum).IsDefined(typeof(FlagsAttribute), inherit: false))
        //    //    throw new ArgumentException(); // TODO: add ex message.

        //    //foreach (TEnum current in Enum.GetValues<TEnum>())
        //    //{
        //    //    string name, tooltip;
        //    //    var label = flagLabels.Where(label => label.Flag.Equals(current)).First();
        //    //    if (label != null)
        //    //    {
        //    //        name = label.Name;
        //    //        tooltip = label.Tooltip;
        //    //    }
        //    //    else
        //    //    {
        //    //        name = current.ToString();
        //    //        tooltip = null;
        //    //    }

        //    //    this.AddCheckbox
        //    //      (
        //    //        name: name,
        //    //        get: () => get().Has(current),
        //    //        set: check =>
        //    //        {
        //    //            if (check)
        //    //                addFlag(current);
        //    //            else
        //    //                removeFlag(current);
        //    //        },
        //    //        tooltip: tooltip,
        //    //        fieldChanged: null
        //    //      );
        //    //}
        //    throw new NotImplementedException();
        //    return this;
        //}

        public GenericModConfigMenuFluentHelper AddSlider(Func<string> name, Func<int> get, Action<int> set, int? min, int? max, int? interval = null, Func<string> tooltip = null)
        {
            this._api.AddNumberOption(
                mod: this._manifest,
                getValue: get,
                setValue: set,
                name: name,
                tooltip: tooltip,
                min: min,
                max: max,
                interval: interval,
                fieldId: null
            );
            return this;
        }

        public GenericModConfigMenuFluentHelper AddSlider(Func<string> name, Func<float> get, Action<float> set, float? min, float? max, float? interval = null, Func<string> tooltip = null)
        {
            this._api.AddNumberOption(
                mod: this._manifest,
                getValue: get,
                setValue: set,
                name: name,
                tooltip: tooltip,
                min: min,
                max: max,
                interval: interval,
                fieldId: null
            );
            return this;
        }
    }
}
