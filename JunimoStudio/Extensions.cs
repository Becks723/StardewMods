using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SObject = StardewValley.Object;

namespace JunimoStudio
{
    public static class Extensions
    {
        public static string GetName(this IAudioEngine audioEngine)
        {
            if (audioEngine is IdentifiableAudioEngine iae)
            {
                return iae.GetName();
            }
            else
            {
                return null;
            }
        }

        public static string GetName(this ISoundBank soundBank)
        {
            if (soundBank is IdentifiableSoundBank isb)
            {
                return isb.GetName();
            }
            else
            {
                return null;
            }
        }

        public static IAudioEngine GetParent(this ISoundBank soundBank)
        {
            if (soundBank is IdentifiableSoundBank isb)
            {
                return isb.GetParent();
            }
            else
            {
                return null;
            }
        }

        public static bool TryFindObject(this GameLocation location, Vector2 key, out SObject result)
        {
            var all = location.objects;

            if (all.ContainsKey(key))
            {
                result = all[key];
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        //public static byte ToByte(this NotePitch pitch)
        //{
        //    var group = pitch.Group;
        //    var start = group * 12;
        //    switch (pitch.Scale)
        //    {
        //        case Scale.C:
        //            return (byte)start;
        //        case Scale.Csharp:
        //            return (byte)(start + 1);
        //        case Scale.D:
        //            return (byte)(start + 2);
        //        case Scale.Dsharp:
        //            return (byte)(start + 3);
        //        case Scale.E:
        //            return (byte)(start + 4);
        //        case Scale.F:
        //            return (byte)(start + 5);
        //        case Scale.Fsharp:
        //            return (byte)(start + 6);
        //        case Scale.G:
        //            return (byte)(start + 7);
        //        case Scale.Gsharp:
        //            return (byte)(start + 8);
        //        case Scale.A:
        //            return (byte)(start + 9);
        //        case Scale.Asharp:
        //            return (byte)(start + 10);
        //        case Scale.B:
        //            return (byte)(start + 11);
        //        default:
        //            throw new InvalidEnumArgumentException();
        //    }

        //}
    }
}
