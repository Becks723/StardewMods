using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Preset;
using StardewModdingAPI;

namespace FontSettings.Framework.Models
{
    internal class FontPresetModelForContentPack : FontPresetModel, IPresetFromContentPack, IPresetWithName, IPresetWithDescription
    {
        private readonly Func<string> _name;
        private readonly Func<string> _description;

        public IContentPack SContentPack { get; }

        public string Name => this._name();

        public string Description => this._description();

        public FontPresetModelForContentPack(FontPresetModel basedOn,
            IContentPack sContentPack, Func<string> name, Func<string> description)
            : base(basedOn.Context, basedOn.Settings)
        {
            this.SContentPack = sContentPack;
            this._name = name ?? throw new ArgumentNullException(nameof(name));
            this._description = description ?? throw new ArgumentNullException(nameof(description));
        }
    }
}
