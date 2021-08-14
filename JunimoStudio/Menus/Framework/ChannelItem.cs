using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JunimoStudio.Core;
using JunimoStudio.Menus.Controls;
using Microsoft.Xna.Framework;
using StardewValley;

namespace JunimoStudio.Menus.Framework
{
    internal class ChannelItem : Container
    {
        private readonly IChannel _channel;

        private readonly Button2 _instrumentBtn;

        /// <summary>右边的钢琴卷帘缩略图。</summary>
        private readonly Button2 _pianoRollThumb;

        public ChannelItem(IChannel channel, Rectangle local, Action<string> openPianoRoll)
        {
            this._channel = channel;
            this.LocalPosition = new Vector2(local.X, local.Y);
            this.Width = local.Width;
            this.Height = local.Height;

            this._instrumentBtn = new Button2();
            this._instrumentBtn.LocalPosition = new Vector2(40, 40);
            this._instrumentBtn.Size = new Vector2(100, 60);
            this._instrumentBtn.Callback = (btn) => { Game1.soundBank.PlayCue("flute"); };

            this._pianoRollThumb = new Button2();
            this._pianoRollThumb.LocalPosition = new Vector2(
                this._instrumentBtn.LocalPosition.X + this._instrumentBtn.Width + 40,
                this._instrumentBtn.LocalPosition.Y);
            this._pianoRollThumb.Size = new Vector2(200, this._instrumentBtn.Height);
            this._pianoRollThumb.Callback = (btn) =>
            {
                openPianoRoll?.Invoke(channel.Name);
            };

            this.AddChild(this._instrumentBtn);
            this.AddChild(this._pianoRollThumb);
        }

        public override int Width { get; }

        public override int Height { get; }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            foreach (Element child in this.Children)
            {
                child.Update(gameTime);
            }
        }
    }
}
