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
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace FontSettings.Framework.FontPatching
{
    internal class MainFontPatcher
    {
        private readonly IFontConfigManager _fontConfigManager;
        private readonly FontPatchResolverFactory _resolverFactory;
        private readonly IFontPatchInvalidator _invalidator;
        private readonly IMonitor _monitor;

        private bool _bypassFontPatch;

        private readonly IDictionary<FontContext, IFontPatch?> _pendingPatchSlots = new Dictionary<FontContext, IFontPatch?>();
        private readonly IList<FontContext> _pendingInvalidates = new List<FontContext>();

        public event EventHandler<FontPixelZoomOverrideEventArgs> FontPixelZoomOverride;

        /// <summary>Raised after a game font is successfully invalidated.</summary>
        public event EventHandler<InvalidatedEventArgs> Invalidated;

        /// <summary>Raised after a game font failed to invalidate.</summary>
        public event EventHandler<InvalidateFailedEventArgs> InvalidateFailed;

        public MainFontPatcher(IFontConfigManager fontConfigManager, FontPatchResolverFactory resolverFactory,
            IFontPatchInvalidator invalidator, IMonitor monitor)
        {
            this._fontConfigManager = fontConfigManager;
            this._resolverFactory = resolverFactory;
            this._invalidator = invalidator;
            this._monitor = monitor;
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
                if (e.NameWithoutLocale.IsEquivalentTo(FontHelpers.GetFontFileAssetName()))
                {
                    this.PatchBmFontFile(e);
                }

                else if (this.IsPatchingBmFont(e)
                    && this.IsBmFontPage(e, out string pageKey))
                {
                    this.PatchBmFontPage(e, pageKey);
                }
            }
        }

        public void OnAssetReady(AssetReadyEventArgs e)
        {
        }

        public void OnUpdateTicking(UpdateTickingEventArgs e)
        {
            lock (this._pendingInvalidates)
            {
                foreach (FontContext context in this._pendingInvalidates.Distinct().ToArray())
                    this.InvalidateGameFont(context);

                this._pendingInvalidates.Clear();
            }
        }

        public void PauseFontPatch()
        {
            this._bypassFontPatch = true;
        }

        public void ResumeFontPatch()
        {
            this._bypassFontPatch = false;
        }

        public async Task PatchAsync(FontContext context)
        {
            Exception? exception = await this.PendPatchAsync(context);
            if (exception == null)
            {
                this.PendInvalidate(context);
            }
            else
            {
                if (exception is not KeyNotFoundException)
                {
                    // TODO
                }
            }
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

        public void PendInvalidate(FontContext context)
        {
            lock (this._pendingInvalidates)
            {
                this._pendingInvalidates.Add(context);

                this._monitor.Log($"To invalidate added: {context.Language},{context.FontType}. Count: {this._pendingInvalidates.Count}");
            }
        }

        /// <summary>Sync one, not thread safe.</summary>
        public void InvalidateGameFont(FontContext context)
        {
            try
            {
                this._invalidator.InvalidateAndPropagate(context);

#if DEBUG
                // throw new Exception("Test Exception");
#endif
            }
            catch (Exception ex)
            {
                this._monitor.Log($"Error when invalidating font {context.Language},{context.FontType}. {ex}");
                InvalidateFailed?.Invoke(this, 
                    new InvalidateFailedEventArgs(context, ex));
                return;
            }

            Invalidated?.Invoke(this, new InvalidatedEventArgs(context));
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
                    if (this._pendingPatchSlots.TryGetValue(context, out IFontPatch? patch)
                        && patch != null)
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

        private void PatchBmFontFile(AssetRequestedEventArgs e)
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

        private void PatchBmFontPage(AssetRequestedEventArgs e, string pageKey)
        {
            var bmFontPatch = this._bmFontPatch;

            var loader = bmFontPatch.PageLoaders[pageKey];
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

        private bool IsPatchingBmFont(AssetRequestedEventArgs e)
        {
            return this._bmFontPatch != null;
        }

        private bool IsBmFontPage(AssetRequestedEventArgs e, out string pageKey)
        {
            var bmFontPatch = this._bmFontPatch;

            if (bmFontPatch.PageLoaders != null)
            {
                var key = bmFontPatch.PageLoaders.Keys
                    .Where(key => e.NameWithoutLocale.IsEquivalentTo(key))
                    .FirstOrDefault();
                if (key != null)
                {
                    pageKey = key;
                    return true;
                }
            }

            pageKey = null;
            return false;
        }

        private void LoadAsset(AssetRequestedEventArgs e, IFontLoader loader)
        {
            e.LoadFrom(() => loader.Load(), (AssetLoadPriority)loader.Priority);
        }

        private void EditAsset(AssetRequestedEventArgs e, IFontEditor editor)
        {
            AssetEditPriority priority = (AssetEditPriority)editor.Priority;

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

    internal class InvalidatedEventArgs : EventArgs
    {
        public FontContext Context { get; }
        public InvalidatedEventArgs(FontContext context)
        {
            this.Context = context;
        }
    }

    internal class InvalidateFailedEventArgs : EventArgs
    {
        public FontContext Context { get; }
        public Exception Exception { get; }
        public InvalidateFailedEventArgs(FontContext context, Exception exception)
        {
            this.Context = context;
            this.Exception = exception;
        }
    }
}
