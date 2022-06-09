namespace FontSettings.Framework
{
    internal enum GameFontType
    {
        SmallFont,
        DialogueFont,
        SpriteText,
    }

    internal static class GameFontTypeExtensions
    {
        public static string LocalizedName(this GameFontType fontType)
        {
            return fontType switch
            {
                GameFontType.SmallFont => I18n.GameFontType_SmallFont(),
                GameFontType.DialogueFont => I18n.GameFontType_DialogueFont(),
                GameFontType.SpriteText => I18n.GameFontType_SpriteText(),
                _ => throw new System.NotSupportedException()
            };
        }
    }
}
