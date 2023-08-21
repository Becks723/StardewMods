using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework.FontPatching.Resolving
{
    internal abstract class BaseFontPatchResolver : IFontPatchResolver
    {
        private readonly Func<PatchModeInfo> _patchModeInfo;

        public abstract IResult<IFontPatch, Exception> Resolve(FontConfig config, FontContext context);
        public virtual Task<IResult<IFontPatch, Exception>> ResolveAsync(FontConfig config, FontContext context)
            => Task.Run(() => this.Resolve(config, context));

        protected FontPatchFactory PatchFactory { get; } = new();

        protected BaseFontPatchResolver(Func<PatchModeInfo> patchModeInfo)
        {
            this._patchModeInfo = patchModeInfo;
        }

        protected (bool loadOrReplace, int loadPriority, int editPriority) GetPatchDetailsForCompat(FontContext context)
        {
            int loadPriority = int.MaxValue;
            int editPriority = 0;

            bool loadOrReplace = !context.Language.IsModLanguage();  // 自定义语言下replace。
            // TODO: 改成别的模组load时我们replace，这样既包括了自定义语言模组，还有一些替换字体的UI美化。
            // TODO: 如果别的模组是replace怎么办？a. 允许用户修改我们edit的优先级。b. ……
            // TODO: 能否从冲突的模组下手，需要研究cp的api。

            return new(loadOrReplace, loadPriority, editPriority);
        }

        protected PatchModeInfo GetPatchModeInfo(FontContext context)
        {
            PatchModeInfo info = this._patchModeInfo();
            bool loadOrReplace = info.LoadOrReplace;
            int loadPriority = info.LoadPriority;
            int editPriority = info.EditPriority;

            if (context.Language.IsModLanguage())  // 自定义语言下replace。
                loadOrReplace = false;

            return new PatchModeInfo(loadOrReplace, loadPriority, editPriority);
        }

        protected IResult<IFontPatch, Exception> SuccessResult(IFontPatch patch) => ResultFactory.SuccessResult(patch);

        protected IResult<IFontPatch, Exception> ErrorResult(Exception exception) => ResultFactory.ErrorResult<IFontPatch>(exception);
    }
}
