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
        public abstract IResult<IFontPatch, Exception> Resolve(FontConfig config, FontContext context);
        public virtual Task<IResult<IFontPatch, Exception>> ResolveAsync(FontConfig config, FontContext context)
            => Task.Run(() => this.Resolve(config, context));

        protected FontPatchFactory PatchFactory { get; } = new();

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

        protected IResult<IFontPatch, Exception> SuccessResult(IFontPatch patch) => new Result(true, patch, null);

        protected IResult<IFontPatch, Exception> ErrorResult(Exception exception) => new Result(false, null, exception);

        private record Result(bool IsSuccess, IFontPatch Patch, Exception Exception) : IResult<IFontPatch, Exception>
        {
            public IFontPatch GetData() => this.Patch;

            public Exception GetError() => this.Exception;
        }
    }
}
