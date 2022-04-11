using System;
using System.Linq;
using CodeShared.Integrations.GenericModConfigMenu.Options;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace CodeShared.Integrations.GenericModConfigMenu
{
    internal class GenericModConfigMenuFluentHelper
    {
        private readonly IGenericModConfigMenuApi _api;
        private readonly IManifest _manifest;
        private readonly Action _reset;
        private readonly Action _save;

        public GenericModConfigMenuFluentHelper(IGenericModConfigMenuApi api, IManifest manifest, Action reset, Action save)
        {
            this._api = api ?? throw new ArgumentNullException(nameof(api));
            this._manifest = manifest;
            this._reset = reset;
            this._save = save;
        }

        public GenericModConfigMenuFluentHelper Register(bool titleScreenOnly = false)
        {
            this._api.Register(this._manifest, this._reset, this._save, titleScreenOnly);
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

        public GenericModConfigMenuFluentHelper AddDropDown(Func<string> name, Func<string> get, Action<string> set, string[] choices, Func<string, string> formattedChoices = null, Func<string> tooltip = null, Action<string> fieldChanged = null)
        {
            string fieldID = this.WhenChange(fieldChanged);

            this._api.AddTextOption(
                mod: this._manifest,
                getValue: get,
                setValue: set,
                name: name,
                tooltip: tooltip,
                allowedValues: choices,
                formatAllowedValue: formattedChoices,
                fieldId: fieldID
            );

            return this;
        }

        /// <summary>
        /// Add a drop down for a set of integer choices.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="get">Gets the integer value.</param>
        /// <param name="set">Sets a new integer value.</param>
        /// <param name="choices"></param>
        /// <param name="tooltip"></param>
        /// <param name="fieldChanged"></param>
        /// <returns></returns>
        public GenericModConfigMenuFluentHelper AddDropDown(Func<string> name, Func<int> get, Action<int> set, int[] choices, Func<string> tooltip = null, Action<int> fieldChanged = null)
        {
            this.AddDropDown(
                name: name,
                get: () => get().ToString(),
                set: val => set(int.Parse(val)),
                choices: choices.Select(i => i.ToString()).ToArray(),
                tooltip: tooltip,
                fieldChanged: val => fieldChanged?.Invoke(int.Parse(val))
            );
            return this;
        }

        /// <summary>Add a drop down for an Enum type.</summary>
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
                fieldChanged: val => fieldChanged?.Invoke(Enum.Parse<TEnum>(val))
            );
            return this;
        }

        public GenericModConfigMenuFluentHelper AddCheckbox(Func<string> name, Func<bool> get, Action<bool> set, Func<string> tooltip = null, Action<bool> fieldChanged = null)
        {
            string fieldID = this.WhenChange(fieldChanged);

            this._api.AddBoolOption(
                mod: this._manifest,
                getValue: get,
                setValue: set,
                name: name,
                tooltip: tooltip,
                fieldId: fieldID
            );

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

        public GenericModConfigMenuFluentHelper AddCustom(Func<string> name, Action<SpriteBatch, Vector2> draw, Func<string> tooltip = null, Action beforeMenuOpened = null, Action beforeSave = null, Action afterSave = null, Action beforeReset = null, Action afterReset = null, Action beforeMenuClosed = null, Func<int> height = null, string fieldId = null)
        {
            this._api.AddComplexOption(
                mod: this._manifest,
                name: name,
                draw: draw,
                tooltip: tooltip,
                beforeMenuOpened: beforeMenuOpened,
                beforeSave: beforeSave,
                afterSave: afterSave,
                beforeReset: beforeReset,
                afterReset: afterReset,
                beforeMenuClosed: beforeMenuClosed,
                height: height,
                fieldId: fieldId
            );
            return this;
        }

        public GenericModConfigMenuFluentHelper AddCustom(Func<string> name, Func<string> tooltip, BaseCustomOption option)
        {
            return this.AddCustom(
                name: name,
                tooltip: tooltip,
                draw: option.Draw,
                beforeMenuOpened: option.OnMenuOpening,
                beforeMenuClosed: option.OnMenuClosing,
                beforeSave: option.OnSaving,
                afterSave: option.OnSaved,
                beforeReset: option.OnReseting,
                afterReset: option.OnReset,
                height: () => option.Height
            );
        }

        public GenericModConfigMenuFluentHelper AddFilePathPicker(Func<string> name, Func<string> tooltip, Func<string> getPath, Action<string> setPath, Func<string> browseButtonText = null)
        {
            return this.AddCustom(
                name: name,
                tooltip: tooltip,
                option: new FilePathPicker(getPath, setPath) { BrowseButtonText = browseButtonText }
            );
        }

        /// <summary>Subscribe to field change event.</summary>
        /// <returns>The unique field ID.</returns>
        private string WhenChange<TField>(Action<TField> fieldChange)
        {
            string fieldID = null;
            if (fieldChange != null)
            {
                fieldID = Guid.NewGuid().ToString("N");

                this._api.OnFieldChanged(this._manifest, (id, val) =>
                {
                    if (id == fieldID)
                        fieldChange((TField)val);
                });
            }

            return fieldID;
        }
    }
}