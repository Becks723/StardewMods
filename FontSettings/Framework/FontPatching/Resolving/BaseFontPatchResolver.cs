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
        public abstract IResult<IFontPatch, Exception> Resolve(FontConfig_ config);
        public abstract Task<IResult<IFontPatch, Exception>> ResolveAsync(FontConfig_ config);

        protected FontPatchFactory PatchFactory { get; } = new();

        protected IResult<IFontPatch, Exception> SuccessResult(IFontPatch patch) => new Result(true, patch, null);

        protected IResult<IFontPatch, Exception> ErrorResult(Exception exception) => new Result(false, null, exception);

        private record Result(bool IsSuccess, IFontPatch Patch, Exception Exception) : IResult<IFontPatch, Exception>
        {
            public IFontPatch GetData() => this.Patch;

            public Exception GetError() => this.Exception;
        }
    }
}
