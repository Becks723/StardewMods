using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Locations;

namespace JunimoStudio
{
    public static class Utilities
    {
        //public static List<ICue> temporaryCues = new List<ICue>();
        //public static double WhenToPlayOrStop(int whichBpm, double whichDelayTimeOrDuration)
        //{
        //    return 60000 / whichBpm * whichDelayTimeOrDuration * 4;
        //}
        //public static bool AllCuesStopped(List<ICue> cues)
        //{
        //    bool allStopping = true;
        //    for (int i = 0; i < cues.Count; i++)
        //        allStopping = (cues[i].IsStopping || cues[i].IsStopped) && allStopping;
        //    return allStopping;
        //}
        //public static bool ContainsStopAtNext(InstrumentBase block)
        //{
        //    for (int i = 0; i < block.Menu.TotalTabs; i++)
        //    {
        //        if (block.Menu.StopAtNext[i])
        //            return true;
        //    }
        //    return false;
        //}
        //public static void DoStopAtNextFunction(InstrumentBase block, List<ICue> cues)
        //{
        //    for (int i = 0; i < block.Menu.TotalTabs; i++)
        //    {
        //        if (block.Menu.StopAtNext[i])
        //        {
        //            temporaryCues.Add(cues[i]);
        //            temporaryCues[temporaryCues.IndexOf(cues[i])].Play();
        //        }
        //        else
        //            cues[i].Play();
        //    }
        //}
        //public static bool ContainsDelayed(InstrumentBase block)
        //{
        //    for (int i = 0; i < block.Menu.TotalTabs; i++)
        //    {
        //        if (block.Menu.IsDelayed[i])
        //            return true;
        //    }
        //    return false;
        //}
        //public static void DoDelayedFunction(InstrumentBase block, List<ICue> cues)
        //{
        //    for (int i = 0; i < block.Menu.TotalTabs; i++)
        //    {
        //        if (!block.Menu.IsDelayed[i] && !block.Menu.StopAtNext[i])
        //            cues[i].Play();
        //        else
        //            continue;
        //    }
        //}
        //public static int GetDelayedCount(InstrumentBase block)
        //{
        //    int count = 0;
        //    for (int i = 0; i < block.Menu.TotalTabs; i++)
        //    {
        //        if (block.Menu.IsDelayed[i])
        //            count++;
        //    }
        //    return count;
        //}
        //public static bool AllDelayed(InstrumentBase block)
        //{
        //    int count = 0;
        //    for (int i = 0; i < block.Menu.TotalTabs; i++)
        //    {
        //        if (block.Menu.IsDelayed[i])
        //            count++;
        //    }
        //    return count == block.Menu.TotalTabs ? true : false;
        //}
        //public static int GetSameDelayTimeCountForDelayed(InstrumentBase block)
        //{
        //    int count = 1;
        //    double delayTime = 0;
        //    for (int i = 0; i < block.Menu.TotalTabs; i++)
        //    {
        //        if (block.Menu.IsDelayed[i])
        //        {
        //            if (delayTime == block.Menu.DelayTime[i])
        //                count++;
        //            delayTime = block.Menu.DelayTime[i];
        //        }
        //    }
        //    return count;
        //}
        //public static Rectangle GetRandomSourceRectForNotes()
        //{
        //    int[] x = { 0, 6, 12, 18 };
        //    int[] y = { 0, 9 };
        //    return new Rectangle(x[Game1.random.Next(4)], y[Game1.random.Next(2)], 6, 9);
        //}
        //public static Vector2 StringToVector2(string str)
        //{
        //    float x = Convert.ToSingle(str.Substring(3, str.IndexOf(" ") - 3));
        //    float y = Convert.ToSingle(str.Substring(str.IndexOf(" ") + 3, str.IndexOf("}") - 3 - str.IndexOf(" ")));
        //    return new Vector2(x, y);
        //}
        private static int GetCharCount(string input, char which)
        {
            int n = 0;
            foreach (var letter in input.ToCharArray())
            {
                if (letter == which)
                    n++;
            }
            return n;
        }
        public static double CheckDoubleForamt(string input)
        {
            foreach (var letter in input.ToCharArray())
            {
                if (letter != '0' && letter != '1' && letter != '2' && letter != '3' && letter != '4' && letter != '5' && letter != '6' && letter != '7' && letter != '8' && letter != '9' && letter != '0' && letter != '.' && letter != '/')
                    return 0;
            }
            if (GetCharCount(input, '.') > 1)
                return 0;
            if (input.Contains("/"))
            {
                if (GetCharCount(input, '/') == 1 && input.IndexOf('/') > 0)
                {
                    string numerator = input.Substring(0, input.IndexOf('/'));
                    string denominator = input.Split('/')[1];
                    double d = Convert.ToDouble(numerator) / Convert.ToDouble(denominator);
                    if (d > 2)
                        d = 2;
                    return d;
                }
                else
                    return 0;
            }
            else
            {
                double d = Convert.ToDouble(input);
                if (d > 2)
                    d = 2;
                return d;
            }
        }

        // <summary>
        // Get all game locations.
        // </summary>
        public static IEnumerable<GameLocation> GetLocations()
        {
            return Game1.locations
                .Concat(
                    from location in Game1.locations.OfType<BuildableGameLocation>()
                    from building in location.buildings
                    where building.indoors.Value != null
                    select building.indoors.Value
                );
        }

        /// <summary>
        /// https://github.com/spacechase0/SpaceShared/blob/f044f715785611a5acf3a1f5523cab807eec115d/Util.cs#L53
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="min"></param>
        /// <param name="t"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static T Clamp<T>(T min, T t, T max)
        {
            if (Comparer<T>.Default.Compare(min, t) > 0)
                return min;
            if (Comparer<T>.Default.Compare(max, t) < 0)
                return max;
            return t;
        }

        /// <summary>
        /// https://github.com/spacechase0/SpaceShared/blob/f044f715785611a5acf3a1f5523cab807eec115d/Util.cs#L62
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static T Adjust<T>(T value, T interval)
        {
            if (value is float vFloat && interval is float iFloat)
                value = (T)(object)(float)((decimal)vFloat - ((decimal)vFloat % (decimal)iFloat));

            if (value is int vInt && interval is int iInt)
                value = (T)(object)(vInt - vInt % iInt);

            return value;
        }
    }
}
