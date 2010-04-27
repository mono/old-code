/*
Copyright (C) 1993-2000 Yukihiro Matsumoto
Copyright (C) 2001-2002 arton
Copyright (C) 2005      Jaen Saul
*/

using System;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Globalization;

namespace NETRuby
{
    public class RTime : RData, ICloneable, IComparable
    {
        internal RTime(NetRuby rb) :
            base(rb, rb.cTime)
        {
            utc = false;
            time = DateTime.Now;
        }
        internal RTime(NetRuby rb, RMetaObject meta) :
            base(rb, meta)
        {
            utc = false;
            time = DateTime.Now;
        }
        internal RTime(NetRuby rb, long ftime) :
            base(rb, rb.cTime)
        {
            utc = false;
            time = DateTime.FromFileTime(ftime);
        }
        internal RTime(NetRuby rb, DateTime tm) :
            base(rb, rb.cTime)
        {
            utc = false;
            time = tm;
        }

        public int CompareTo(object o)
        {
            if (o is RTime == false)
            {
                long l = RInteger.ToLong(ruby, o);
                return ToLong().CompareTo(l);
            }
            return time.CompareTo(((RTime)o).time);
        }

        public override bool Eql(object o)
        {
            if (o is RTime == false) return false;
            RTime t = (RTime)o;
            return (time == t.time);
        }
        
        public override int GetHashCode() // for voiding CS0659 warning.
        {
            return base.GetHashCode();
        }
        public override bool Equals(object o)
        {
            if (o is RTime == false) return false;
            return o.GetHashCode() == GetHashCode();
        }

        public long ToLong() // time_t
        {
            long l = time.ToFileTime();
            l -= RTimeClass.Epoch;
            l /= 10000000;
            return l;
        }
        public double ToDouble()
        {
            return (double)time.ToFileTime();
        }
        public override RInteger ToInteger()
        {
            long l = ToLong();
            if (l <= Int32.MaxValue)
                return new RFixnum(ruby, (int)l);
            return new RBignum(ruby, l);
        }
        public override RFloat ToFloat()
        {
            long l = time.ToFileTime();
            l -= RTimeClass.Epoch;
            return new RFloat(ruby, l / 10000000.0);
        }
        public override string ToString()
        {
            if (utc)
                return time.ToUniversalTime().ToString("ddd MMM dd HH:mm:ss \\G\\M\\T yyyy",
                                                       USFormat);
            else
                return time.ToString("ddd MMM dd HH:mm:ss \\L\\M\\T yyyy", USFormat);
        }
        
        internal RTime CopyFormat(RTime t)
        {
            utc = t.utc;
            return this;
        }
        public DateTime ToDateTime()
        {
            return time;
        }
        public object Clone()
        {
            return MemberwiseClone();
        }

        DateTime time;
        bool utc;
        static IFormatProvider USFormat = CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat;
    }
    
    public class RTimeClass : RClass
    {
        private RTimeClass(NetRuby rb) :
            base(rb, "Time", rb.cObject)
        {
        }

        static internal object s_now(RBasic r, params object[] args)
        {
            return new RTime(r.ruby, (RMetaObject)r);
        }
        static internal object s_new(RBasic r, params object[] args)
        {
            object o = s_now(r, args);
            r.ruby.CallInit(o, args);
            return o;
        }
        static internal object s_at(RBasic r, params object[] args)
        {
            NetRuby rb = r.ruby;
            object[] argv = new object[2];
            long tmv = 0;
            long usec = 0;
            RTime result = null;
            int cnt = rb.ScanArgs(args, "11", argv);
            if (argv[0] is RTime)
            {
                DateTime tm = ((RTime)argv[0]).ToDateTime();
                if (cnt == 2)
                {
                    usec = RInteger.ToLong(rb, argv[1]);
                    usec *= 10;
                    result = new RTime(rb, tm.ToFileTime() + usec);
                }
                else
                {
                    result = new RTime(rb, tm);
                }
                result.CopyFormat((RTime)argv[0]);
            }
            else
            {
                if (cnt == 2)
                {
                    tmv = RInteger.ToLong(rb, argv[0]);
                    usec = RInteger.ToLong(rb, argv[1]);
                    usec *= 10;
                }
                else
                {
                    tmv = RInteger.ToLong(rb, argv[0]);
                }
                // Adjust Unix Epoch to .NET
                tmv *= 1000;  // mill second
                tmv *= 10000; // 100-nanosecond
                tmv += epoch;
                tmv += usec;
                result = new RTime(rb, tmv);
            }
            return result;
        }
        static readonly long epoch = (new DateTime(1970, 1, 1, 0, 0, 0)).ToFileTime();
        public static long Epoch
        {
            get { return epoch; }
        }
        static internal object s_mkutc(RBasic r, params object[] args)
        {
            return null;
        }
        static internal object s_mktime(RBasic r, params object[] args)
        {
            return null;
        }
        static internal object s_times(RBasic r, params object[] args)
        {
            return null;
        }
        static internal object to_i(RBasic r, params object[] args)
        {
            return ((RTime)r).ToInteger();
        }
        static internal object to_f(RBasic r, params object[] args)
        {
            return ((RTime)r).ToFloat();
        }
        static internal object cmp(RBasic r, params object[] args)
        {
            return ((IComparable)r).CompareTo(args[0]);
        }
        static internal object plus(RBasic r, params object[] args)
        {
            NetRuby rb = r.ruby;
            if (args[0] is RTime)
            {
                throw new eTypeError("time + time?");
            }
            long l = RInteger.ToLong(rb, args[0]);
            l += ((RTime)r).ToLong();
            RTime result = new RTime(rb, l * 10000000 + RTimeClass.Epoch);
            return result.CopyFormat((RTime)r);
        }
        static internal object minus(RBasic r, params object[] args)
        {
            NetRuby rb = r.ruby;
            if (args[0] is RTime)
            {
                double d = ((RTime)args[0]).ToDouble();
                return new RFloat(rb, (((RTime)r).ToDouble() - d) / 10000000);
            }

            long l = ((RTime)r).ToLong() - RInteger.ToLong(rb, args[0]);
            RTime result = new RTime(rb, l * 10000000 + RTimeClass.Epoch);
            return result.CopyFormat((RTime)r);
        }
        internal static void Init(NetRuby rb)
        {
            RTimeClass t = new RTimeClass(rb);
            t.DefineClass("Time", rb.cObject);
            rb.cTime = t;

            t.DefineSingletonMethod("now", new RMethod(s_now), 0);
            t.DefineSingletonMethod("new", new RMethod(s_new), -1);
            t.DefineSingletonMethod("at", new RMethod(s_at), -1);
            t.DefineSingletonMethod("utc", new RMethod(s_mkutc), -1);
            t.DefineSingletonMethod("gm", new RMethod(s_mkutc), -1);
            t.DefineSingletonMethod("local", new RMethod(s_mktime), -1);
            t.DefineSingletonMethod("mktime", new RMethod(s_mktime), -1);

            t.DefineSingletonMethod("times", new RMethod(s_times), 0);

            t.DefineMethod("to_i", new RMethod(to_i), 0);
            t.DefineMethod("to_f", new RMethod(to_f), 0);
                t.DefineMethod("<=>", new RMethod(cmp), 1);

                t.DefineMethod("+", new RMethod(plus), 1);
                t.DefineMethod("-", new RMethod(minus), 1);
        }
    }
}
