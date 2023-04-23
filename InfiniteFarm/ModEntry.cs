using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using xTile;
using xTile.Dimensions;
using xTile.Format;
using xTile.Layers;
using xTile.Tiles;

namespace InfiniteFarm
{
    internal class ModEntry : Mod
    {
        private Lazy<Map> _infiniteFarmMap;

        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);

            this._infiniteFarmMap = new Lazy<Map>(() => this.LoadInfiniteFarmMap(helper));

            Map map = Game1.content.Load<Map>("Maps/Farm");
            helper.Events.Content.AssetRequested += this.OnAssetRequested;

            //Map map = helper.ModContent.Load<Map>("assets/farm-infinite.tmx");
            //Map map = Game1.content.Load<Map>("Maps/Farm");
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Maps/Farm"))
            {
                e.LoadFrom(() => this._infiniteFarmMap.Value, AssetLoadPriority.Exclusive);
                //e.LoadFromModFile<Map>("assets/farm-infinite.tmx", AssetLoadPriority.Exclusive);
            }
        }

        private Map LoadInfiniteFarmMap(IModHelper helper)
        {
            Map map = helper.ModContent.Load<Map>("assets/farm-infinite.tmx");
            map.AddTileSheet(new TileSheet("t", map, "Maps/spring_outdoorsTileSheet", new Size(100), new Size(16)));

            Layer backLayer = map.GetLayer("Back");
            if (backLayer == null)
                map.AddLayer(backLayer = new Layer("Back", map, new Size(100), new Size(16)));

            foreach (Layer layer in map.Layers.ToArray())
            {
                layer.LayerSize = new Size(100);

                if (layer == backLayer)
                    for (int y = 0; y < 100; y++)
                    {
                        for (int x = 0; x < 100; x++)
                        {
                            layer.Tiles[x, y] = new StaticTile(
                                layer: layer,
                                tileSheet: map.GetTileSheet("t"),
                                blendMode: BlendMode.Alpha,
                                tileIndex: 587);
                        }
                    }
            }

            return map;
        }
    }
}
