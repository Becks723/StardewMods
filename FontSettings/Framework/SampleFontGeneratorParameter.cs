namespace FontSettings.Framework
{
    internal record SampleFontGeneratorParameter(
        bool Enabled,
        string FontFilePath,
        float FontSize,
        float Spacing,
        float LineSpacing,
        string? SampleText,
        GameFontType FontType,
        LanguageInfo Language,
        float PixelZoom,

        int FontIndex = 0,
        float CharOffsetX = 0,
        float CharOffsetY = 0)
        
        : FontGeneratorParameter(FontFilePath, FontSize, Spacing, LineSpacing, null, FontIndex, CharOffsetX, CharOffsetY);
}
