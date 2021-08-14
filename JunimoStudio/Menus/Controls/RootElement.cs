using Microsoft.Xna.Framework;

namespace JunimoStudio.Menus.Controls
{
    public class RootElement : Container
    {
        //public bool Obscured { get; set; } = false;

        public override int Width => 0;
        public override int Height => 0;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            foreach (var child in Children)
                child.Update(gameTime);
        }

        internal override RootElement GetRootImpl()
        {
            return this;
        }
    }
}
