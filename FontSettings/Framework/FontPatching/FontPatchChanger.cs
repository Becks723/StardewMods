using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework.FontPatching
{
    internal class FontPatchChanger : IAsyncGameFontChanger, IDisposable
    {
        private readonly int _maxStateCapacity = Enum.GetValues<GameFontType>().Length;

        private readonly IDictionary<FontContext, InvalidateState> _invalidateStateLookups = new Dictionary<FontContext, InvalidateState>();

        private readonly MainFontPatcher _mainFontPatcher;

        private readonly int _mainThreadId;

        public FontPatchChanger(MainFontPatcher mainFontPatcher)
        {
            this._mainFontPatcher = mainFontPatcher;
            this._mainFontPatcher.Invalidated += this.OnFontInvalidated;
            this._mainFontPatcher.InvalidateFailed += this.OnFontInvalidatedFailed;
            this._mainThreadId = Environment.CurrentManagedThreadId;
        }

        public async Task<IGameFontChangeResult> ChangeGameFontAsync(FontConfig font, FontContext context)
        {
            Exception? exception = await this._mainFontPatcher.PendPatchAsync(font, context);

            if (exception == null)
            {
                int threadId = Environment.CurrentManagedThreadId;
                bool isCurrentMainThread = (threadId == this._mainThreadId);

                var state = this.EnsureStateLookup(context);
                state.IsMainThread = isCurrentMainThread;

                if (isCurrentMainThread)
                {
                    // 立即刷新字体。
                    this._mainFontPatcher.InvalidateGameFont(context);

                    // 拿取报错信息
                    exception = state.Exception;
                    state.Exception = null;

                    // 没有报错，成功
                    if (exception == null)
                        return this.SuccessResult();
                }

                else
                {
                    // 通知主线程刷新字体，不立即。
                    this._mainFontPatcher.PendInvalidate(context);

                    try
                    {
                        // 阻塞当前后台线程，等待主线程刷新完字体。
                        state.ResetEvent.Wait();

                        // 主线程刷新完了
                        // 拿取报错信息
                        exception = state.Exception;
                        state.Exception = null;

                        // 没有报错，成功
                        if (exception == null)
                            return this.SuccessResult();
                    }
                    finally
                    {
                        state.ResetEvent.Reset();
                    }
                }
            }

            return this.ErrorResult(exception, this.GetErrorMessageRecursively);
        }

        public void Dispose()
        {
            lock (this._invalidateStateLookups)
            {
                foreach (var state in this._invalidateStateLookups.Values)
                    state.ResetEvent.Dispose();

                this._invalidateStateLookups.Clear();
            }
        }

        private void OnFontInvalidated(object sender, InvalidatedEventArgs e)
        {
            // 主线程成功刷新字体，通知后台线程继续。
            this.OnInvalidated(e.Context, null);
        }

        private void OnFontInvalidatedFailed(object sender, InvalidateFailedEventArgs e)
        {
            // 主线程刷新字体失败，通知后台线程继续，并发送报错信息。
            this.OnInvalidated(e.Context, e.Exception);
        }

        private InvalidateState EnsureStateLookup(FontContext context)
        {
            InvalidateState state;
            lock (this._invalidateStateLookups)
            {
                if (!this._invalidateStateLookups.TryGetValue(context, out state))
                    state = this._invalidateStateLookups[context] = new InvalidateState();
            }

            return state;
        }

        private void OnInvalidated(FontContext context, Exception? exception)
        {
            lock (this._invalidateStateLookups)
            {
                if (this._invalidateStateLookups.TryGetValue(context, out var state))
                {
                    state.Exception = exception;
                    if (!state.IsMainThread)
                        state.ResetEvent.Set();
                }
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

        private class InvalidateState
        {
            public readonly ManualResetEventSlim ResetEvent = new(false);
            public bool IsMainThread;
            public Exception? Exception;
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
