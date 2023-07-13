using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;
using FontSettings.Framework.Models.Builder;
using FontSettings.Framework.Preset;
using StardewModdingAPI;

namespace FontSettings.Framework
{
    internal class FontPresetBuilder
    {
        private FontPresetDecorator _preset;

        public FontPreset Build()
        {
            return this._preset;
        }

        public FontPresetBuilder Clear()
        {
            this._preset = null;
            return this;
        }

        public FontPresetBuilder BasicPreset(FontPreset preset)
        {
            this._preset = new FontPresetBasic(preset);
            return this;
        }

        public FontPresetBuilder BasicPreset(FontContext fontContext, FontConfig settings)
        {
            return this.BasicPreset(new FontPreset(fontContext, settings));
        }

        public FontPresetBuilder WithName(string? name)
        {
            this._preset = new FontPresetWithName(this._preset, name);
            return this;
        }

        public FontPresetBuilder WithName(Func<string?> name)
        {
            this._preset = new FontPresetWithName(this._preset, name);
            return this;
        }


        public FontPresetBuilder WithDescription(string? description)
        {
            this._preset = new FontPresetWithDescription(this._preset, description);
            return this;
        }

        public FontPresetBuilder WithDescription(Func<string?> description)
        {
            this._preset = new FontPresetWithDescription(this._preset, description);
            return this;
        }

        public FontPresetBuilder WithKey<TKey>(TKey key)
        {
            this._preset = new FontPresetWithKey<TKey>(this._preset, key);
            return this;
        }

        public FontPresetBuilder FromContentPack(IContentPack sContentPack)
        {
            this._preset = new FontPresetFromContentPack(this._preset, sContentPack);
            return this;
        }
    }
}
