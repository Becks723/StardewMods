using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontChangers
{
    internal class GameFontChangeResult : IGameFontChangeResult
    {
        private readonly string? _errorMessage;

        public bool IsSuccessful { get; }

        public GameFontChangeResult(bool isSuccessful, Exception? exception)
        {
            this.IsSuccessful = isSuccessful;
            this._errorMessage = this.ParseException(exception);
        }

        public string? GetErrorMessage()
        {
            return this._errorMessage;
        }

        private string? ParseException(Exception? exception)
        {
            if (exception == null)
                return null;
            return $"{exception.Message}\n{exception.StackTrace}";
        }
    }
}
