using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework.FontPatching
{
    internal class FontPatchChanger : IGameFontChanger, IAsyncGameFontChanger
    {
        private readonly IFontPatchResolver _resolver;
        private readonly IFontPatchInvalidator _invalidator;

        public FontPatchChanger(IFontPatchResolver resolver, IFontPatchInvalidator invalidator)
        {
            this._resolver = resolver;
            this._invalidator = invalidator;
        }

        public IGameFontChangeResult ChangeGameFont(FontConfig font)
        {
            Exception? exception;

            var result = this._resolver.Resolve(font);
            if (result.IsSuccess)
            {
                exception = null;

                IFontPatch patch = result.GetData();
                this._invalidator.Patch = patch;

                this._invalidator.InvalidateAndPropagate();
                // TODO: 这里怎么才能知道有没有改成功呢？
                // 1) InvalidateAndPropagate的返回值改成bool？
            }
            else
            {
                exception = result.GetError();
            }

            if (exception == null)
                return this.SuccessResult();
            else
                return this.ErrorResult(exception, this.GetErrorMessageRecursively);
        }

        public async Task<IGameFontChangeResult> ChangeGameFontAsync(FontConfig font)
        {
            Exception? exception;

            var result = await this._resolver.ResolveAsync(font);
            if (result.IsSuccess)
            {
                exception = null;

                IFontPatch patch = result.GetData();
                this._invalidator.Patch = patch;

                this._invalidator.InvalidateAndPropagate();
                // TODO: 这里怎么才能知道有没有改成功呢？
                // 1) InvalidateAndPropagate的返回值改成bool？
            }
            else
            {
                exception = result.GetError();
            }

            if (exception == null)
                return this.SuccessResult();
            else
                return this.ErrorResult(exception, this.GetErrorMessageRecursively);
        }


        private string GetErrorMessageRecursively(Exception exception)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder
                .AppendLine(exception.Message)
                .AppendLine(exception.StackTrace);

            if (exception.InnerException != null)
            {
                stringBuilder
                    .AppendLine("Inner Exception:")
                    .AppendLine(this.GetErrorMessageRecursively(exception.InnerException));
            }

            return stringBuilder.ToString();
        }

        private IGameFontChangeResult SuccessResult() => new ChangeResult(true, null);

        private IGameFontChangeResult ErrorResult(string errorMessage) => new ChangeResult(false, errorMessage);

        private IGameFontChangeResult ErrorResult(Exception exception, Func<Exception, string> getErrorMessage) => this.ErrorResult(getErrorMessage(exception));

        private record ChangeResult(bool IsSuccess, string? ErrorMessage) : IGameFontChangeResult
        {
            bool IGameFontChangeResult.IsSuccessful => this.IsSuccess;

            string? IGameFontChangeResult.GetErrorMessage() => this.ErrorMessage;
        }
    }
}
