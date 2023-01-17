namespace FontSettings.Framework
{
    internal interface IGameFontChangeResult
    {
        bool IsSuccessful { get; }

        /// <summary></summary>
        /// <returns>null if <see cref="IsSuccessful"/> is true.</returns>
        string? GetErrorMessage();
    }
}
