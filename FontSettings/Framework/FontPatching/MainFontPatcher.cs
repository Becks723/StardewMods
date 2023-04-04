using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using FontSettings.Framework.FontPatching.Editors;
using FontSettings.Framework.FontPatching.Loaders;
using FontSettings.Framework.FontPatching.Replacers;
using FontSettings.Framework.Models;
using StardewModdingAPI.Events;
using StardewValley;

namespace FontSettings.Framework.FontPatching
{
    internal class MainFontPatcher
    {
        private readonly FontConfigManager _fontConfigManager;
        private readonly FontPatchResolverFactory _resolverFactory;
        private readonly IFontPatchInvalidator _invalidator;

        private bool _bypassFontPatch;

        private readonly IDictionary<FontContext, IFontPatch?> _pendingPatchSlots = new Dictionary<FontContext, IFontPatch?>();

        public event EventHandler<FontPixelZoomOverrideEventArgs> FontPixelZoomOverride;

        public MainFontPatcher(FontConfigManager fontConfigManager, FontPatchResolverFactory resolverFactory,
            IFontPatchInvalidator invalidator)
        {
            this._fontConfigManager = fontConfigManager;
            this._resolverFactory = resolverFactory;
            this._invalidator = invalidator;
        }

        public void OnAssetRequested(AssetRequestedEventArgs e)
        {
            if (this._bypassFontPatch)
                return;

            if (e.NameWithoutLocale.IsEquivalentTo("Fonts/SmallFont"))
            {
                this.PatchCommonFont(e, GameFontType.SmallFont);
            }

            else if (e.NameWithoutLocale.IsEquivalentTo("Fonts/SpriteFont1"))
            {
                this.PatchCommonFont(e, GameFontType.DialogueFont);
            }

            else
            {
                this.PatchBmFont(e);
            }
        }

        public void OnAssetReady(AssetReadyEventArgs e)
        {
        }

        public void PauseFontPatch()
        {
            this._bypassFontPatch = true;
        }

        public void ResumeFontPatch()
        {
            this._bypassFontPatch = false;
        }

        public async Task<Exception?> PendPatchAsync(FontContext context)
        {
            var language = context.Language;
            var fontType = context.FontType;
            if (this._fontConfigManager.TryGetFontConfig(language, fontType, out var config))
            {
                return await this.PendPatchAsync(config, context);
            }

            return new KeyNotFoundException();  // not found saved config. 
        }

        public async Task<Exception?> PendPatchAsync(FontConfig fontConfig, FontContext context)
        {
            var resolver = this.GetResolver(context.FontType);
            var result = await resolver.ResolveAsync(fontConfig, context);
            if (result.IsSuccess)
            {
                this.UpdatePendingPatch(context, result.GetData());
                return null;
            }
            else
            {
                return result.GetError();
            }
        }

        /// <summary>Sync one, not thread safe.</summary>
        public void InvalidateGameFont(FontContext context)
        {
            this._invalidator.InvalidateAndPropagate(context);
        }

        public async Task InvalidateGameFontAsync(FontContext context)
        {
            await Task.Run(() => this._invalidator.InvalidateAndPropagate(context));  // here assumes `_invalidator.InvalidateAndPropagate` thread safe
        }

        /// <summary>Thread safe.</summary>
        public void UpdatePendingPatch(FontContext context, IFontPatch patch)
        {
            lock (this._pendingPatchSlots)
            {
                this._pendingPatchSlots[context] = patch;
            }
        }

        private void PatchCommonFont(AssetRequestedEventArgs e, GameFontType fontType)
        {
            var patch = this.ResolvePatch(fontType);
            if (patch != null)
            {
                this.PatchCommonFontCore(e, patch);
            }
        }

        private IFontPatch? ResolvePatch(GameFontType fontType)
        {
            var language = FontHelpers.GetCurrentLanguage();
            var context = new FontContext(language, fontType);

            // patch data is prepared in advance.
            lock (this._pendingPatchSlots)
            {
                try
                {
                    if (this._pendingPatchSlots.TryGetValue(context, out IFontPatch? patch))
                        return patch;
                }
                finally
                {
                    this._pendingPatchSlots[context] = null;
                }
            }

            // we need resolve manually.
            if (this._fontConfigManager.TryGetFontConfig(language, fontType, out var config))
            {
                var resolver = this.GetResolver(fontType);
                var result = resolver.Resolve(config, context);
                if (result.IsSuccess)
                {
                    return result.GetData();
                }
                else
                {
                    Exception exception = result.GetError();
                    throw exception;  // TODO
                }
            }

            return null;
        }

        private void PatchCommonFontCore(AssetRequestedEventArgs e, IFontPatch patch)
        {
            if (patch.Loader != null)
            {
                this.LoadAsset(e, patch.Loader);
            }

            if (patch.Editor != null)
            {
                this.EditAsset(e, patch.Editor);
            }
        }

        private IBmFontPatch _bmFontPatch;
        private void PatchBmFont(AssetRequestedEventArgs e)
        {
            string fontFileName = FontHelpers.GetFontFileAssetName();
            if (e.NameWithoutLocale.IsEquivalentTo(fontFileName))
            {
                this.PatchFontFile(e);
            }

            else if (this._bmFontPatch != null)
            {
                this.PatchFontPages(e);
            }
        }

        private void PatchFontFile(AssetRequestedEventArgs e)
        {
            var bmFontPatch = this.ResolvePatch(GameFontType.SpriteText) as IBmFontPatch;
            if (bmFontPatch != null)
            {
                this.PatchCommonFontCore(e, bmFontPatch);

                if (bmFontPatch.PageLoaders == null)
                    this.RaiseFontPixelZoomOverride(bmFontPatch.FontPixelZoom);
            }

            this._bmFontPatch = bmFontPatch;
        }

        private void PatchFontPages(AssetRequestedEventArgs e)
        {
            var bmFontPatch = this._bmFontPatch;

            if (bmFontPatch.PageLoaders != null)
            {
                var pairs = bmFontPatch.PageLoaders
                    .Where(pair => e.NameWithoutLocale.IsEquivalentTo(pair.Key));
                if (pairs.Any())
                {
                    var pair = pairs.First();

                    string pageKey = pair.Key;
                    var loader = pair.Value;
                    if (loader != null)
                        this.LoadAsset(e, loader);

                    bmFontPatch.PageLoaders.Remove(pageKey);
                    if (bmFontPatch.PageLoaders.Count == 0)
                    {
                        this._bmFontPatch = null;

                        // 设置缩放，放在最后。
                        this.RaiseFontPixelZoomOverride(bmFontPatch.FontPixelZoom);
                    }
                }
            }
        }

        private void LoadAsset(AssetRequestedEventArgs e, IFontLoader loader, AssetLoadPriority priority = AssetLoadPriority.Exclusive)
        {
            e.LoadFrom(() => loader.Load(), priority);
        }

        private void EditAsset(AssetRequestedEventArgs e, IFontEditor editor, AssetEditPriority priority = AssetEditPriority.Default)
        {
            if (editor is IFontReplacer replacer)
                e.Edit(asset => asset.ReplaceWith(replacer.Replacement), priority);
            else
                e.Edit(asset => editor.Edit(asset.Data), priority);
        }

        private IFontPatchResolver GetResolver(GameFontType fontType)
            => this._resolverFactory.CreateResolver(fontType);

        private void RaiseFontPixelZoomOverride(float pixelZoom)
        {
            this.RaiseFontPixelZoomOverride(
                new FontPixelZoomOverrideEventArgs(true, pixelZoom));
        }

        protected virtual void RaiseFontPixelZoomOverride(FontPixelZoomOverrideEventArgs e)
        {
            FontPixelZoomOverride?.Invoke(this, e);
        }
    }

    internal class FontPixelZoomOverrideEventArgs : EventArgs
    {
        public bool NeedsOverride { get; }

        public float PixelZoom { get; }

        public FontPixelZoomOverrideEventArgs(bool needsOverride, float pixelZoom)
        {
            this.NeedsOverride = needsOverride;
            this.PixelZoom = pixelZoom;
        }
    }
}
