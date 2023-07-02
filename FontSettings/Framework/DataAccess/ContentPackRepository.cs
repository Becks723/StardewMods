using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.DataAccess.Models;
using FontSettings.Framework.DataAccess.Parsing;
using FontSettings.Framework.Models;
using Newtonsoft.Json;
using StardewModdingAPI;

namespace FontSettings.Framework.DataAccess
{
    internal class ContentPackRepository
    {
        private readonly IContentPackHelper _contentPackHelper;
        private readonly IMonitor _monitor;
        private readonly IEnumerable<IContentPack> _fakeContentPacks;

        public ContentPackRepository(IContentPackHelper contentPackHelper, IMonitor monitor, IEnumerable<IContentPack>? fakeContentPacks = null)
        {
            this._contentPackHelper = contentPackHelper;
            this._monitor = monitor;
            this._fakeContentPacks = fakeContentPacks ?? Array.Empty<IContentPack>();
        }

        public IEnumerable<FontPresetModel> ReadContentPacks(FontContext context)
            => this.ReadAllContentPacks().Where(preset => preset.Context == context);

        public IEnumerable<FontPresetModel> ReadContentPacks(LanguageInfo language)
            => this.ReadAllContentPacks().Where(preset => preset.Context.Language == language);

        public IEnumerable<FontPresetModel> ReadContentPacks(IEnumerable<LanguageInfo> languages)
            => this.ReadAllContentPacks().Where(preset => languages.Contains(preset.Context.Language));

        public IEnumerable<FontPresetModel> ReadAllContentPacks()
        {
            foreach (IContentPack pack in this._contentPackHelper.GetOwned().Concat(this._fakeContentPacks))
            {
                this._monitor.Log($"Reading content pack: {pack.Manifest.Name} {pack.Manifest.Version} from {pack.DirectoryPath}");

                const string contentFile = "content.json";
                if (pack.HasFile(contentFile))
                {
                    ContentPackParser parser = new ContentPackParser();
                    FontContentPack[] rawContentPacks;
                    try
                    {
                        try
                        {
                            rawContentPacks = pack.ReadJsonFile<FontContentPack[]>(contentFile);
                        }
                        catch (JsonReaderException)
                        {
                            rawContentPacks = new[] { pack.ReadJsonFile<FontContentPack>(contentFile) };
                        }
                    }
                    catch (Exception ex)
                    {
                        this._monitor.Log($"Error reading content pack: {ex}", LogLevel.Error);
                        continue;
                    }

                    foreach (FontContentPack rawContentPack in rawContentPacks)
                    {
                        IEnumerable<FontPresetModel> presets;
                        try
                        {
                            presets = parser.Parse(rawContentPack, pack).ToArray();
                        }
                        catch (Exception ex)
                        {
                            this._monitor.Log($"Error reading content pack: {ex}", LogLevel.Error);  // TODO: 标明具体的名字（可能需要加一个name属性？）
                            presets = null;
                        }

                        if (presets != null)
                            foreach (FontPresetModel preset in presets)
                                yield return preset;
                    }
                }
                else
                {
                    this._monitor.Log($"'{contentFile}' not found. Skipped");
                    continue;
                }
            }
        }
    }
}
