using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace FontSettings.Framework.Menus
{
    internal class OptionsFontSelector : OptionsSelector<GameFontType>
    {
        public GameFontType CurrentFont => this.SelectedChoice;

        public OptionsFontSelector(bool skipSpriteText, GameFontType defaultSelectedChoice = GameFontType.SmallFont)
            : base(
                  type => GetPrev(type, skipSpriteText),
                  type => GetNext(type, skipSpriteText),
                  defaultSelectedChoice)
        {
            this.Choices = Enum.GetValues<GameFontType>();
            this.DisplayChoiceParser = font => this.ParseFontName(font);
        }

        private string ParseFontName(GameFontType fontType)
        {
            return fontType switch
            {
                GameFontType.SmallFont => GameFontType.SmallFont.LocalizedName(),
                GameFontType.DialogueFont => GameFontType.DialogueFont.LocalizedName(),
                GameFontType.SpriteText => GameFontType.SpriteText.LocalizedName(),
                _ => throw new NotSupportedException()
            };
        }

        private static GameFontType GetPrev(GameFontType current)
        {
            return current switch
            {
                GameFontType.SpriteText => GameFontType.DialogueFont,
                GameFontType.DialogueFont => GameFontType.SmallFont,
                GameFontType.SmallFont => GameFontType.SpriteText,
                _ => throw new NotSupportedException()
            };
        }

        private static GameFontType GetNext(GameFontType current)
        {
            return current switch
            {
                GameFontType.SpriteText => GameFontType.SmallFont,
                GameFontType.DialogueFont => GameFontType.SpriteText,
                GameFontType.SmallFont => GameFontType.DialogueFont,
                _ => throw new NotSupportedException()
            };
        }

        private static GameFontType GetPrev(GameFontType current, bool skipSpriteText)
        {
            if (skipSpriteText)
                return current switch
                {
                    GameFontType.DialogueFont => GameFontType.SmallFont,
                    GameFontType.SmallFont => GameFontType.DialogueFont,
                    GameFontType.SpriteText => GameFontType.SmallFont,
                    _ => throw new NotSupportedException()
                };
            else
                return GetPrev(current);
        }

        private static GameFontType GetNext(GameFontType current, bool skipSpriteText)
        {
            if (skipSpriteText)
                return current switch
                {
                    GameFontType.DialogueFont => GameFontType.SmallFont,
                    GameFontType.SmallFont => GameFontType.DialogueFont,
                    GameFontType.SpriteText => GameFontType.SmallFont,
                    _ => throw new NotSupportedException()
                };
            else
                return GetNext(current);
        }

        protected override Vector2 MeasureString(string text)
        {
            return new Vector2(
                SpriteText.getWidthOfString(text), 
                SpriteText.getHeightOfString(text)
            );
        }

        protected override void DrawString(SpriteBatch b, string text, Vector2 position, Color color)
        {
            SpriteText.drawString(b, text, (int)position.X, (int)position.Y);
        }
    }
}
