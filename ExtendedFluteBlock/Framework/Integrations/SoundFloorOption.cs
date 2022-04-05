using System;
using System.Collections.Generic;
using System.Linq;
using CodeShared.Integrations.GenericModConfigMenu.Options;
using FluteBlockExtension.Framework.Menus;
using FluteBlockExtension.Framework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using Button = StardewValley.Controls.Button2;

namespace FluteBlockExtension.Framework.Integrations
{
    /// <summary>A custom GMCM option for editing sound-floor mapping.</summary>
    internal class SoundFloorOption : BaseCustomOption
    {
        private SoundFloorEditor _soundFloorEditor;

        private readonly Lazy<Texture2D> _spannerIcon = new(LoadSpannerTexture);

        private readonly Button _button;

        private readonly Func<SoundFloorMap> _map;

        private readonly Func<SoundsConfig> _config;

        /// <summary>Gets the GMCM config menu.</summary>
        private IClickableMenu ConfigMenu
        {
            get { return Game1.activeClickableMenu is TitleMenu ? TitleMenu.subMenu : Game1.activeClickableMenu; }
        }

        public override int Height => 50;

        public SoundFloorOption(Func<SoundsConfig> config)
        {
            this._config = config;
            this._map = () => config().SoundFloorPairs;

            this._button = new()
            {
                SettableWidth = 80,
                SettableHeight = 50,
                Content = this._spannerIcon.Value,
                Scale = 4f
            };
            this._button.Click += this.ButtonClicked;
        }

        public override void Draw(SpriteBatch b, Vector2 drawOrigin)
        {
            this._button.LocalPosition = drawOrigin;
            this._button.Update(default);
            this._button.Draw(b);
        }

        public override void OnMenuOpening()
        {
            // 每次打开重新初始化，不能仅在构造函数中初始化。
            // 这是由于该GMCM选项仅在游戏打开时注册（GameLaunched），那时本地化还没加载，导致SpriteFont还都是英文，其他语言会乱码。
            this._soundFloorEditor?.Dispose();
            this._soundFloorEditor = new SoundFloorEditor(this._map);
        }

        public override void OnSaving()
        {
            this._soundFloorEditor.OnSaving();

            // load new sounds.
            SoundManager.LoadSounds(this._config());
        }

        private void ButtonClicked(object sender, EventArgs e)
        {
            this._soundFloorEditor.ConfigMenu = this.ConfigMenu;

            if (Game1.activeClickableMenu is TitleMenu)
                TitleMenu.subMenu = this._soundFloorEditor;
            else
                Game1.activeClickableMenu = this._soundFloorEditor;
        }

        private static Texture2D LoadSpannerTexture()
        {
            // See (48, 208, 16, 16) in LooseSprites/Cursors2.
            Texture2D result = new Texture2D(Game1.graphics.GraphicsDevice, 8, 8);

            Dictionary<Color, (int x, int y)[]> colors = new()
            {
#pragma warning disable format
                { new Color(130, 102, 101), new[] { (2, 0), (3, 0), (3, 1), (4, 1), (0, 2), (3, 2), (4, 2), (1, 3), (2, 3), (3, 3), (4, 3), (4, 4), (5, 4), (5, 5), (6, 5), (6, 6), (7, 6), (7, 7) } },
                { new Color(104, 72,  71 ), new[] { (0, 3), (1, 4), (2, 4), (3, 4), (4, 5), (5, 6), (6, 7) } }
#pragma warning restore format
            };

            Color[] data = new Color[8 * 8];
            for (int j = 0; j < 8; j++)
            {
                for (int i = 0; i < 8; i++)
                {
                    Color c = Color.Transparent;
                    foreach (var kvp in colors)
                        if (kvp.Value.Any(xy => xy.x == 7 - i && xy.y == j))
                            c = kvp.Key;

                    data[i + j * 8] = c;
                }
            }

            result.SetData(data);
            return result;
        }
    }
}