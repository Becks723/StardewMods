using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework.FontPatching
{
    internal class FontPatchChanger : IAsyncGameFontChanger
    {
        private readonly MainFontPatcher _mainFontPatcher;

        public FontPatchChanger(MainFontPatcher mainFontPatcher)
        {
            this._mainFontPatcher = mainFontPatcher;
        }

        public async Task<IGameFontChangeResult> ChangeGameFontAsync(FontConfig font, FontContext context)
        {
            Exception? exception = await this._mainFontPatcher.PendPatchAsync(font, context);

            if (exception == null)
            {
                this._mainFontPatcher.InvalidateGameFont(context);
                // TODO: 这里怎么才能知道有没有改成功呢？
                // 1) InvalidateAndPropagate的返回值改成bool？

                return this.SuccessResult();
            }
            else
            {
                return this.ErrorResult(exception, this.GetErrorMessageRecursively);
            }
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
