using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio
{
    internal class TrackManager
    {
        private static IMonitor _monitor;

        private static IList<TrackInfo> _tracks;

        public static void Init(IMonitor monitor, SaveConfig saveConfig)
        {
            _monitor = monitor;
            _tracks = saveConfig.Tracks;
        }

        public static void Register(TrackInfo track)
        {
            // 轨道两两之间不能有重叠。
            // 这个Intersects跟Rectangle.Intersects不同的是，Rectangle那个不算边界，这个只要边界重合就算。
            static bool Intersects(Rectangle value1, Rectangle value2)
            {
                return
                    value1.X <= value2.X + value2.Width
                    && value2.X <= value1.X + value1.Width
                    && value1.Y <= value2.Y + value2.Height
                    && value2.Y <= value1.Y + value1.Height;
            }

            var sameLocationTracks = _tracks.Where(t => t.Location == track.Location);
            if (sameLocationTracks == null ||
                !sameLocationTracks.Any(t => Intersects(t.TileBounds, track.TileBounds)))
            {
                _tracks.Add(track);
                _monitor.Log($"New track created.");
            }
        }

        public static void Unregister(int index)
        {
            _tracks.RemoveAt(index);
        }

        public static bool IsTrackHere(GameLocation location, Vector2 tilePosition, out bool horizontal)
        {
            TrackInfo track =
                 (from t in _tracks
                  where t.Location == location.NameOrUniqueName
                  && t.TileBounds.Contains((int)tilePosition.X, (int)tilePosition.Y)
                  select t)
                 .FirstOrDefault();

            if (track == null)
            {
                horizontal = true;
                return false;
            }
            else
            {
                horizontal = track.Horizontal;
                return true;
            }
        }

        public static void LogAllTracks()
        {
            for (int i = 0; i < _tracks.Count; i++)
            {
                TrackInfo current = _tracks[i];
                _monitor.Log($"{i} {current}\n");
            }
        }
    }
}
