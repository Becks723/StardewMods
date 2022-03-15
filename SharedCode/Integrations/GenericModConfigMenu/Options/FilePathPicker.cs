using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace CodeShared.Integrations.GenericModConfigMenu.Options
{
    internal class FilePathPicker : BaseCustomOption
    {
        private readonly Textbox _textbox = new Textbox();

        private readonly Button _browseButton = new Button();

        private readonly Func<string> _getPath;
        private readonly Action<string> _setPath;
        private readonly Func<string> _getDefaultPath;

        public FilePathPicker(Func<string> getPath!!, Action<string> setPath!!, Func<string> getDefaultPath = null)
        {
            this._textbox.Width = 192 * 2;
            this._browseButton.Size = new Vector2(100, this._textbox.Height + 8);
            this._getPath = getPath;
            this._setPath = setPath;
            this._getDefaultPath = getDefaultPath;
            this._browseButton.Callback += this.BrowseFolder;
        }

        public override int Height
        {
            get { return this._browseButton.Height; }
        }

        public override void Draw(SpriteBatch b, Vector2 drawOrigin)
        {
            this._textbox.LocalPosition = drawOrigin;
            this._browseButton.LocalPosition = new Vector2(drawOrigin.X + this._textbox.Width + 40f, drawOrigin.Y - 4);

            this._textbox.Update(default);
            this._textbox.Draw(b);
            this._browseButton.Update(default);
            this._browseButton.Draw(b);
        }

        public override void OnSaving()
        {
            this._setPath(this._textbox.String);
        }

        public override void OnReseting()
        {
            var defaultPath = this._getDefaultPath;
            if (defaultPath != null)
                this._textbox.String = defaultPath();
        }

        public override void OnReset()
        {
            this._textbox.String = this._getPath();
        }

        public override void OnMenuOpening()
        {
            this._textbox.String = this._getPath();
        }

        private void BrowseFolder(object sender, EventArgs e)
        {

        }
    }
}
