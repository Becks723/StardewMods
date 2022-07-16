using StardewValley;
using StardewValleyUI.Controls;

namespace FontSettings.Framework.Menus
{
    internal class OKButton : TextureButton
    {
        public override int Width => this.SourceRectangle.Value.Width;

        public override int Height => this.SourceRectangle.Value.Height;

        public OKButton()
        {
            this.Texture = Game1.mouseCursors;
            this.SourceRectangle = Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46);
        }
    }
}
