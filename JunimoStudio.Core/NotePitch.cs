using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio.Core
{
    public struct NotePitch
    {
        public Scale Scale { get; private set; }

        public int Group { get; private set; }

        public NotePitch(Scale scale, int group)
        {
            Scale = scale;
            if (group < 0 || group > 10) throw new ArgumentOutOfRangeException(nameof(group));
            Group = group;
        }

        public NotePitch Previous
        {
            get
            {
                var scale = Scale;
                var group = Group;
                if (scale == Scale.C)
                    group--;
                if (group == -1)
                    group = 10;

                return new NotePitch(GetPrevious(scale), group);
            }
        }

        public NotePitch Next
        {
            get
            {
                var scale = Scale;
                var group = Group;
                if (scale == Scale.B)
                    group++;
                if (group == 11)
                    group = 0;

                return new NotePitch(GetNext(scale), group);
            }
        }

        public bool HigherThan(NotePitch value)
        {
            if (value == this)
                return false;
            if (value.Group != Group)
                return value.Group < Group;
            return value.Scale < Scale;
        }

        public bool LowerThan(NotePitch value)
        {
            if (value == this)
                return false;
            if (value.Group != Group)
                return value.Group > Group;
            return value.Scale > Scale;
        }


        public override string ToString()
        {
            string scale = Scale.ToString().Replace("sharp", "#");
            return $"{scale}{Group}";
        }

        public static bool operator ==(NotePitch value1, NotePitch value2)
        {
            return value1.Scale == value2.Scale && value1.Group == value2.Group;
        }

        public static bool operator !=(NotePitch value1, NotePitch value2)
        {
            return !(value1 == value2);
        }

        public static bool operator <(NotePitch value1, NotePitch value2)
        {
            return value1.LowerThan(value2);
        }

        public static bool operator >(NotePitch value1, NotePitch value2)
        {
            return value1.HigherThan(value2);
        }

        public static bool operator <=(NotePitch value1, NotePitch value2)
        {
            return value1 == value2 || value1.LowerThan(value2);
        }

        public static bool operator >=(NotePitch value1, NotePitch value2)
        {
            return value1 == value2 || value1.HigherThan(value2);
        }

        public static NotePitch operator ++(NotePitch value)
        {
            return value.Next;
        }

        public static NotePitch operator --(NotePitch value)
        {
            return value.Previous;
        }

        private static Scale GetNext(Scale value)
        {
            if (value == Scale.C)
                return Scale.Csharp;
            else if (value == Scale.Csharp)
                return Scale.D;
            else if (value == Scale.D)
                return Scale.Dsharp;
            else if (value == Scale.Dsharp)
                return Scale.E;
            else if (value == Scale.E)
                return Scale.F;
            else if (value == Scale.F)
                return Scale.Fsharp;
            else if (value == Scale.Fsharp)
                return Scale.G;
            else if (value == Scale.G)
                return Scale.Gsharp;
            else if (value == Scale.Gsharp)
                return Scale.A;
            else if (value == Scale.A)
                return Scale.Asharp;
            else if (value == Scale.Asharp)
                return Scale.B;
            else
                return Scale.C;

            //return (from Scale sca in Enum.GetValues(typeof(Scale))
            //        where sca > value
            //        orderby sca
            //        select sca).DefaultIfEmpty().First();
        }

        private static Scale GetPrevious(Scale value)
        {
            if (value == Scale.C)
                return Scale.B;
            else if (value == Scale.Csharp)
                return Scale.C;
            else if (value == Scale.D)
                return Scale.Csharp;
            else if (value == Scale.Dsharp)
                return Scale.D;
            else if (value == Scale.E)
                return Scale.Dsharp;
            else if (value == Scale.F)
                return Scale.E;
            else if (value == Scale.Fsharp)
                return Scale.F;
            else if (value == Scale.G)
                return Scale.Fsharp;
            else if (value == Scale.Gsharp)
                return Scale.G;
            else if (value == Scale.A)
                return Scale.Gsharp;
            else if (value == Scale.Asharp)
                return Scale.A;
            else
                return Scale.Asharp;

            //return (from Scale sca in Enum.GetValues(typeof(Scale))
            //        where sca < value
            //        orderby sca
            //        select sca).DefaultIfEmpty().Last();
        }

        #region All Pitches
        public static NotePitch C0 { get; } = new(Scale.C, 0);

        public static NotePitch Csharp0 { get; } = new(Scale.Csharp, 0);

        public static NotePitch D0 { get; } = new(Scale.D, 0);

        public static NotePitch Dsharp0 { get; } = new(Scale.Dsharp, 0);

        public static NotePitch E0 { get; } = new(Scale.E, 0);

        public static NotePitch F0 { get; } = new(Scale.F, 0);

        public static NotePitch Fsharp0 { get; } = new(Scale.Fsharp, 0);

        public static NotePitch G0 { get; } = new(Scale.G, 0);

        public static NotePitch Gsharp0 { get; } = new(Scale.Gsharp, 0);

        public static NotePitch A0 { get; } = new(Scale.A, 0);

        public static NotePitch Asharp0 { get; } = new(Scale.Asharp, 0);

        public static NotePitch B0 { get; } = new(Scale.B, 0);

        public static NotePitch C1 { get; } = new(Scale.C, 1);

        public static NotePitch Csharp1 { get; } = new(Scale.Csharp, 1);

        public static NotePitch D1 { get; } = new(Scale.D, 1);

        public static NotePitch Dsharp1 { get; } = new(Scale.Dsharp, 1);

        public static NotePitch E1 { get; } = new(Scale.E, 1);

        public static NotePitch F1 { get; } = new(Scale.F, 1);

        public static NotePitch Fsharp1 { get; } = new(Scale.Fsharp, 1);

        public static NotePitch G1 { get; } = new(Scale.G, 1);

        public static NotePitch Gsharp1 { get; } = new(Scale.Gsharp, 1);

        public static NotePitch A1 { get; } = new(Scale.A, 1);

        public static NotePitch Asharp1 { get; } = new(Scale.Asharp, 1);

        public static NotePitch B1 { get; } = new(Scale.B, 1);

        public static NotePitch C2 { get; } = new(Scale.C, 2);

        public static NotePitch Csharp2 { get; } = new(Scale.Csharp, 2);

        public static NotePitch D2 { get; } = new(Scale.D, 2);

        public static NotePitch Dsharp2 { get; } = new(Scale.Dsharp, 2);

        public static NotePitch E2 { get; } = new(Scale.E, 2);

        public static NotePitch F2 { get; } = new(Scale.F, 2);

        public static NotePitch Fsharp2 { get; } = new(Scale.Fsharp, 2);

        public static NotePitch G2 { get; } = new(Scale.G, 2);

        public static NotePitch Gsharp2 { get; } = new(Scale.Gsharp, 2);

        public static NotePitch A2 { get; } = new(Scale.A, 2);

        public static NotePitch Asharp2 { get; } = new(Scale.Asharp, 2);

        public static NotePitch B2 { get; } = new(Scale.B, 2);

        public static NotePitch C3 { get; } = new(Scale.C, 3);

        public static NotePitch Csharp3 { get; } = new(Scale.Csharp, 3);

        public static NotePitch D3 { get; } = new(Scale.D, 3);

        public static NotePitch Dsharp3 { get; } = new(Scale.Dsharp, 3);

        public static NotePitch E3 { get; } = new(Scale.E, 3);

        public static NotePitch F3 { get; } = new(Scale.F, 3);

        public static NotePitch Fsharp3 { get; } = new(Scale.Fsharp, 3);

        public static NotePitch G3 { get; } = new(Scale.G, 3);

        public static NotePitch Gsharp3 { get; } = new(Scale.Gsharp, 3);

        public static NotePitch A3 { get; } = new(Scale.A, 3);

        public static NotePitch Asharp3 { get; } = new(Scale.Asharp, 3);

        public static NotePitch B3 { get; } = new(Scale.B, 3);

        public static NotePitch C4 { get; } = new(Scale.C, 4);

        public static NotePitch Csharp4 { get; } = new(Scale.Csharp, 4);

        public static NotePitch D4 { get; } = new(Scale.D, 4);

        public static NotePitch Dsharp4 { get; } = new(Scale.Dsharp, 4);

        public static NotePitch E4 { get; } = new(Scale.E, 4);

        public static NotePitch F4 { get; } = new(Scale.F, 4);

        public static NotePitch Fsharp4 { get; } = new(Scale.Fsharp, 4);

        public static NotePitch G4 { get; } = new(Scale.G, 4);

        public static NotePitch Gsharp4 { get; } = new(Scale.Gsharp, 4);

        public static NotePitch A4 { get; } = new(Scale.A, 4);

        public static NotePitch Asharp4 { get; } = new(Scale.Asharp, 4);

        public static NotePitch B4 { get; } = new(Scale.B, 4);

        public static NotePitch C5 { get; } = new(Scale.C, 5);

        public static NotePitch Csharp5 { get; } = new(Scale.Csharp, 5);

        public static NotePitch D5 { get; } = new(Scale.D, 5);

        public static NotePitch Dsharp5 { get; } = new(Scale.Dsharp, 5);

        public static NotePitch E5 { get; } = new(Scale.E, 5);

        public static NotePitch F5 { get; } = new(Scale.F, 5);

        public static NotePitch Fsharp5 { get; } = new(Scale.Fsharp, 5);

        public static NotePitch G5 { get; } = new(Scale.G, 5);

        public static NotePitch Gsharp5 { get; } = new(Scale.Gsharp, 5);

        public static NotePitch A5 { get; } = new(Scale.A, 5);

        public static NotePitch Asharp5 { get; } = new(Scale.Asharp, 5);

        public static NotePitch B5 { get; } = new(Scale.B, 5);

        public static NotePitch C6 { get; } = new(Scale.C, 6);

        public static NotePitch Csharp6 { get; } = new(Scale.Csharp, 6);

        public static NotePitch D6 { get; } = new(Scale.D, 6);

        public static NotePitch Dsharp6 { get; } = new(Scale.Dsharp, 6);

        public static NotePitch E6 { get; } = new(Scale.E, 6);

        public static NotePitch F6 { get; } = new(Scale.F, 6);

        public static NotePitch Fsharp6 { get; } = new(Scale.Fsharp, 6);

        public static NotePitch G6 { get; } = new(Scale.G, 6);

        public static NotePitch Gsharp6 { get; } = new(Scale.Gsharp, 6);

        public static NotePitch A6 { get; } = new(Scale.A, 6);

        public static NotePitch Asharp6 { get; } = new(Scale.Asharp, 6);

        public static NotePitch B6 { get; } = new(Scale.B, 6);

        public static NotePitch C7 { get; } = new(Scale.C, 7);

        public static NotePitch Csharp7 { get; } = new(Scale.Csharp, 7);

        public static NotePitch D7 { get; } = new(Scale.D, 7);

        public static NotePitch Dsharp7 { get; } = new(Scale.Dsharp, 7);

        public static NotePitch E7 { get; } = new(Scale.E, 7);

        public static NotePitch F7 { get; } = new(Scale.F, 7);

        public static NotePitch Fsharp7 { get; } = new(Scale.Fsharp, 7);

        public static NotePitch G7 { get; } = new(Scale.G, 7);

        public static NotePitch Gsharp7 { get; } = new(Scale.Gsharp, 7);

        public static NotePitch A7 { get; } = new(Scale.A, 7);

        public static NotePitch Asharp7 { get; } = new(Scale.Asharp, 7);

        public static NotePitch B7 { get; } = new(Scale.B, 7);

        public static NotePitch C8 { get; } = new(Scale.C, 8);

        public static NotePitch Csharp8 { get; } = new(Scale.Csharp, 8);

        public static NotePitch D8 { get; } = new(Scale.D, 8);

        public static NotePitch Dsharp8 { get; } = new(Scale.Dsharp, 8);

        public static NotePitch E8 { get; } = new(Scale.E, 8);

        public static NotePitch F8 { get; } = new(Scale.F, 8);

        public static NotePitch Fsharp8 { get; } = new(Scale.Fsharp, 8);

        public static NotePitch G8 { get; } = new(Scale.G, 8);

        public static NotePitch Gsharp8 { get; } = new(Scale.Gsharp, 8);

        public static NotePitch A8 { get; } = new(Scale.A, 8);

        public static NotePitch Asharp8 { get; } = new(Scale.Asharp, 8);

        public static NotePitch B8 { get; } = new(Scale.B, 8);

        public static NotePitch C9 { get; } = new(Scale.C, 9);

        public static NotePitch Csharp9 { get; } = new(Scale.Csharp, 9);

        public static NotePitch D9 { get; } = new(Scale.D, 9);

        public static NotePitch Dsharp9 { get; } = new(Scale.Dsharp, 9);

        public static NotePitch E9 { get; } = new(Scale.E, 9);

        public static NotePitch F9 { get; } = new(Scale.F, 9);

        public static NotePitch Fsharp9 { get; } = new(Scale.Fsharp, 9);

        public static NotePitch G9 { get; } = new(Scale.G, 9);

        public static NotePitch Gsharp9 { get; } = new(Scale.Gsharp, 9);

        public static NotePitch A9 { get; } = new(Scale.A, 9);

        public static NotePitch Asharp9 { get; } = new(Scale.Asharp, 9);

        public static NotePitch B9 { get; } = new(Scale.B, 9);

        public static NotePitch C10 { get; } = new(Scale.C, 10);

        public static NotePitch Csharp10 { get; } = new(Scale.Csharp, 10);

        public static NotePitch D10 { get; } = new(Scale.D, 10);

        public static NotePitch Dsharp10 { get; } = new(Scale.Dsharp, 10);

        public static NotePitch E10 { get; } = new(Scale.E, 10);

        public static NotePitch F10 { get; } = new(Scale.F, 10);

        public static NotePitch Fsharp10 { get; } = new(Scale.Fsharp, 10);

        public static NotePitch G10 { get; } = new(Scale.G, 10);

        public static NotePitch Gsharp10 { get; } = new(Scale.Gsharp, 10);

        public static NotePitch A10 { get; } = new(Scale.A, 10);

        public static NotePitch Asharp10 { get; } = new(Scale.Asharp, 10);

        public static NotePitch B10 { get; } = new(Scale.B, 10);
        #endregion
    }
}
