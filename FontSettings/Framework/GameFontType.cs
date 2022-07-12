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

        public static GameFontType Previous(this GameFontType fontType, bool skipSpriteText = false)
        {
            if (!skipSpriteText)
                return fontType switch
                {
                    GameFontType.SmallFont => GameFontType.SpriteText,
                    GameFontType.DialogueFont => GameFontType.SmallFont,
                    GameFontType.SpriteText => GameFontType.DialogueFont,
                    _ => throw new System.NotSupportedException()
                };
            else
                return fontType switch
                {
                    GameFontType.SmallFont => GameFontType.DialogueFont,
                    GameFontType.DialogueFont => GameFontType.SmallFont,
                    GameFontType.SpriteText => GameFontType.SmallFont,
                    _ => throw new System.NotSupportedException()
                };
        }

        public static GameFontType Next(this GameFontType fontType, bool skipSpriteText = false)
        {
            if (!skipSpriteText)
                return fontType switch
                {
                    GameFontType.SmallFont => GameFontType.DialogueFont,
                    GameFontType.DialogueFont => GameFontType.SpriteText,
                    GameFontType.SpriteText => GameFontType.SmallFont,
                    _ => throw new System.NotSupportedException()
                };
            else
                return fontType switch
                {
                    GameFontType.SmallFont => GameFontType.DialogueFont,
                    GameFontType.DialogueFont => GameFontType.SmallFont,
                    GameFontType.SpriteText => GameFontType.SmallFont,
                    _ => throw new System.NotSupportedException()
                };
        }
    }
}
