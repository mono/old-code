/*
Copyright (C) 2001-2002 arton
Copyright (C) 2005      Jaen Saul
*/

using System;
using System.Collections;
using System.Reflection;
using System.Security;

namespace NETRuby
{
    public class RChar : RBasic
    {
        internal RChar(NetRuby rb, char c) :
            base(rb, rb.cObject)
        {
            cVal = c;
        }
        char cVal;
        public char Char
        {
            get { return cVal; }
        }
    }
    public abstract class RNumeric : RBasic, ICloneable
    {
        internal RNumeric(NetRuby rb, RMetaObject meta) :
            base(rb, meta)
        {
        }
        public override bool Eql(object o)
        {
            if (GetType() != ruby.InstanceOf(o).GetType())
                return false;
            return Equals(o);
        }
        public abstract long ToLong();

        private RNumeric Instanciate(object o)
        {
            if (o is int)
                return new RFixnum(ruby, (int)o);
            if (o is long)
                return new RBignum(ruby, (long)o);
            if (o is double)
                return new RFloat(ruby, (double)o);
            return (RNumeric)o;
        }
        public virtual RArray Coerce(object o)
        {
            if (ruby.ClassOf(o) == Class)
                return RArray.AssocNew(ruby, Instanciate(o), this);
            return RArray.AssocNew(ruby, RFloat.Float(ruby, o), ToFloat());
        }

        protected void DoCoerce(object o, out RNumeric x, out RNumeric y)
        {
            x = y = null;
            try
            {
                RArray a = (RArray)ruby.Funcall(o, "coerce", this);
                if (a == null || a.Count != 2)
                    throw new eTypeError("coerce must return [x, y]");
                x = (RNumeric)a[0];
                y = (RNumeric)a[1];
            }
            catch
            {
                throw new eTypeError(String.Format("{0} can't be coerced into {1}",
                                                   IsSpecialConstType(o) ?
                                                       ruby.Inspect(o) : ruby.ClassOf(o).Name,
                                                   ruby.ClassOf(this).Name));
            }
        }
        public object CoerceBin(object o)
        {
            RNumeric x;
            RNumeric y;
            DoCoerce(o, out x, out y);
            ////return ruby.FuncallRetry(x, y);
            return null; //PH
        }
        
        public virtual object Clone()
        {
            return this;
        }

        public abstract RNumeric Plus(object o);
        public abstract RNumeric Minus(object o);
        public abstract RNumeric Multiply(object o);
        public abstract RNumeric Divide(object o);
        public abstract RNumeric Modulo(object o);
        public abstract RNumeric Remainder(object o);

        public abstract RArray DivMod(object o);
        public abstract bool Gt(object o);
        public abstract bool GE(object o);
        public abstract bool Lt(object o);
        public abstract bool LE(object o);
        static public object num_coerce(RBasic r, params object[] args)
        {
            return ((RNumeric)r).Coerce(args[0]);
        }
        static public object num_plus(RBasic r, params object[] args)
        {
            return ((RNumeric)r).Plus(args[0]);
        }
        static public object num_minus(RBasic r, params object[] args)
        {
            return ((RNumeric)r).Minus(args[0]);
        }
        static public object num_mul(RBasic r, params object[] args)
        {
            return ((RNumeric)r).Multiply(args[0]);
        }
        static public object num_div(RBasic r, params object[] args)
        {
            return ((RNumeric)r).Divide(args[0]);
        }
        static public object num_mod(RBasic r, params object[] args)
        {
            return ((RNumeric)r).Modulo(args[0]);
        }
        static public object num_remainder(RBasic r, params object[] args)
        {
            return ((RNumeric)r).Remainder(args[0]);
        }
        static public object num_divmod(RBasic r, params object[] args)
        {
            return ((RNumeric)r).DivMod(args[0]);
        }
        static public object num_equal(RBasic r, params object[] args)
        {
            return r.Equals(args[0]);
        }
        static public object num_gt(RBasic r, params object[] args)
        {
            return ((RNumeric)r).Gt(args[0]);
        }
        static public object num_ge(RBasic r, params object[] args)
        {
            return ((RNumeric)r).GE(args[0]);
        }
        static public object num_lt(RBasic r, params object[] args)
        {
            return ((RNumeric)r).Lt(args[0]);
        }
        static public object num_le(RBasic r, params object[] args)
        {
            return ((RNumeric)r).LE(args[0]);
        }
        static public object num_uplus(RBasic r, params object[] args)
        {
            return r;
        }
        static public object num_to_i(RBasic r, params object[] args)
        {
            return r.ToInteger();
        }
        static public object num_uminus(RBasic r, params object[] args)
        {
            RNumeric x;
            RNumeric y;
            ((RNumeric)r).ruby.oFixZero.DoCoerce(r, out x, out y);
            return r.ruby.Funcall(x, (uint)'-', y);
        }
        static public object num_cmp(RBasic r, params object[] args)
        {
            try
            {
                IComparable c = (IComparable)r;
                return c.CompareTo(args[0]);
            }
            catch
            {
                return ((RNumeric)r).CoerceBin(args[0]);
            }
        }
        
        static internal void Init(NetRuby ruby)
        {
            RClass num = ruby.DefineClass("Numeric", ruby.cObject);
            ruby.cNumeric = num;
            num.DefineMethod("coerce", new RMethod(num_coerce), 1);

            num.DefineMethod("to_i", new RMethod(num_to_i), 0);
            num.DefineMethod("truncate", new RMethod(num_to_i), 0);
        
            num.DefineMethod("+@", new RMethod(num_uplus), 0);
            num.DefineMethod("-@", new RMethod(num_uminus), 0);
            num.DefineMethod("===", new RMethod(num_equal), 1);
        
            num.DefineMethod("+", new RMethod(num_plus), 1);
            num.DefineMethod("-", new RMethod(num_minus), 1);
            num.DefineMethod("*", new RMethod(num_mul), 1);
            num.DefineMethod("/", new RMethod(num_div), 1);
            num.DefineMethod("%", new RMethod(num_mod), 1);
            num.DefineMethod("modulo", new RMethod(num_mod), 1);
            num.DefineMethod("divmod", new RMethod(num_divmod), 1);
            num.DefineMethod("remainder", new RMethod(num_remainder), 1);

            num.DefineMethod("<=>", new RMethod(num_cmp), 1);
            num.DefineMethod(">", new RMethod(num_gt), 1);
            num.DefineMethod(">=", new RMethod(num_ge), 1);
            num.DefineMethod("<", new RMethod(num_lt), 1);
            num.DefineMethod("<=", new RMethod(num_le), 1);
        }
    }
    
    public class RFloat : RNumeric, IComparable
    {
        internal RFloat(NetRuby rb)
            : base (rb, rb.cFloat)
        {
        }
        public RFloat(NetRuby rb, double d)
            : base(rb, rb.cFloat)
        {
            dVal = d;
        }

        // as rb_Float
        static public RFloat Float(NetRuby rb, object o)
        {
            if (o == null || o == rb.oNil)
                return new RFloat(rb, 0.0);
            if (o is int)
                return new RFloat(rb, (double)(int)o);
            if (o is long)
                return new RFloat(rb, (double)(long)o);
            if (o is double)
                return new RFloat(rb, (double)o);
            if (o is RFloat)
                return (RFloat)o;
            if (o is RBignum)
                return new RFloat(rb, ((RBignum)o).Big2Dbl());
            string s = null;
            if (o is string)
                s = (string)o;
            else if (o is RString)
                s = ((RString)o).ToString();
            if (s != null)
            {
                s = s.Trim().Replace("_", "");
                try
                {
                    return new RFloat(rb, Convert.ToDouble(s));
                }
                catch
                {
                    throw new eArgError("Invalid valud for Float: \"" + s + "\"");
                }
            }
            return (RFloat)rb.ConvertType(o, typeof(RFloat), "Float", "to_f");
        }
        
        internal RFloat SetData(double d)
        {
            dVal = d;
            return this;
        }
        public double Double
        {
            get { return dVal; }
        }

        static internal RInteger ToInteger(NetRuby ruby, double d)
        {
            if (d <= Int32.MaxValue && d >= Int32.MinValue)
            {
                return new RFixnum(ruby, (int)d);
            }
            else if (d <= Int64.MaxValue && d >= Int64.MinValue)
            {
                return new RBignum(ruby, (long)d);
            }
            return RBignum.Dbl2Big(ruby, d);
        }
        
        public override RInteger ToInteger()
        {
            double d = dVal;
            if (d > 0.0) d = Math.Floor(d);
            else if (d < 0.0) d = Math.Ceiling(d);
            return ToInteger(ruby, d);
        }

        public override string ToString()
        {
            string s = dVal.ToString();
            return (s.IndexOf('.') < 0) ? s + ".0" : s;
        }

        public override long ToLong()
        {
            if (dVal <= (double)Int64.MaxValue
                && dVal >= (double)Int64.MinValue)
            {
                return Convert.ToInt64(dVal);
            }
            throw new eRangeError(String.Format("float {0} out of range of integer", dVal));
        }
        public override RFloat ToFloat()
        {
            return this;
        }
        public int CompareTo(object o)
        {
            double y;
            if (convd(o, out y) == false)
                throw new ArgumentException("object is not a Float");
            if (dVal == y) return 0;
            if (dVal > y) return 1;
            if (dVal < y) return -1;
            throw new eFloatDomainError("comparing NaN");
        }
        private bool convd(object o, out double y)
        {
            if (o is int)
                y = (int)o;
            else if (o is long)
                y = (long)o;
            else if (o is RFixnum)
                y = ((RFixnum)o).ToLong();
            else if (o is double)
                y = (double)o;
            else if (o is RFloat)
                y = ((RFloat)o).dVal;
            else if (o is RBignum)
                y = ((RBignum)o).Big2Dbl();
            else
            {
                y = 0.0;
                return false;
            }
            return true;
        }
        public override bool Gt(object o)
        {
            double y;
            if (convd(o, out y) == false)
                return (bool)CoerceBin(o);
            return (dVal > y);
        }
        public override bool GE(object o)
        {
            double y;
            if (convd(o, out y) == false)
                return (bool)CoerceBin(o);
            return dVal >= y;
        }
        public override bool Lt(object o)
        {
            double y;
            if (convd(o, out y) == false)
                return (bool)CoerceBin(o);
            return dVal < y;
        }
        public override bool LE(object o)
        {
            double y;
            if (convd(o, out y) == false)
                return (bool)CoerceBin(o);
            return dVal <= y;
        }

        public override bool Equals(object o)
        {
            if (o is double)
                return (dVal == (double)o);
            if (o is float)
                return (dVal == (float)o);
            if (o is RFloat)
                return (dVal == ((RFloat)o).dVal);
            return false;
        }
        public override int GetHashCode() // for voiding CS0659 warning.
        {
            return base.GetHashCode();
        }

        double dVal;

        public RInteger Round {
            get {
                double f = dVal;
                if (f > 0.0) f = Math.Floor(f + 0.5);
                if (f < 0.0) f = Math.Ceiling(f - 0.5);
                return ToInteger(ruby, f);
            }
        }
        public RInteger Ceil {
            get { return ToInteger(ruby, Math.Ceiling(dVal)); }
        }
        public RInteger Floor {
            get { return ToInteger(ruby, Math.Floor(dVal)); }
        }
        public bool IsNaN {
            get { return Double.IsNaN(dVal); }
        }
        public bool IsInfinite {
            get { return Double.IsInfinity(dVal); }
        }
        public bool IsFinite {
            get { return !Double.IsInfinity(dVal); }
        }
        
        public override RNumeric Plus(object o)
        {
            if (o is int)
                return new RFloat(ruby, dVal + (int)o);
            if (o is long)
                return new RFloat(ruby, dVal + (long)o);
            if (o is RFixnum)
                return new RFloat(ruby, dVal + ((RFixnum)o).ToLong());
            if (o is double)
                return new RFloat(ruby, dVal + (double)o);
            if (o is RFloat)
                return new RFloat(ruby, dVal + ((RFloat)o).dVal);
            if (o is RBignum)
                return new RFloat(ruby, dVal + ((RBignum)o).Big2Dbl());
            return (RNumeric)CoerceBin(o);
        }
        public override RNumeric Minus(object o)
        {
            if (o is int)
                return new RFloat(ruby, dVal - (int)o);
            if (o is long)
                return new RFloat(ruby, dVal - (long)o);
            if (o is RFixnum)
                return new RFloat(ruby, dVal - ((RFixnum)o).ToLong());
            if (o is double)
                return new RFloat(ruby, dVal - (double)o);
            if (o is RFloat)
                return new RFloat(ruby, dVal - ((RFloat)o).dVal);
            if (o is RBignum)
                return new RFloat(ruby, dVal - ((RBignum)o).Big2Dbl());
            return (RNumeric)CoerceBin(o);
        }
        public override RNumeric Multiply(object o)
        {
            if (o is int)
                return new RFloat(ruby, dVal * (int)o);
            if (o is long)
                return new RFloat(ruby, dVal * (long)o);
            if (o is RFixnum)
                return new RFloat(ruby, dVal * ((RFixnum)o).ToLong());
            if (o is double)
                return new RFloat(ruby, dVal * (double)o);
            if (o is RFloat)
                return new RFloat(ruby, dVal * ((RFloat)o).dVal);
            if (o is RBignum)
                return new RFloat(ruby, dVal * ((RBignum)o).Big2Dbl());
            return (RNumeric)CoerceBin(o);
        }
        public override RNumeric Divide(object o)
        {
            double y;
            if (o is int)
                y = (int)o;
            else if (o is long)
                y = (long)o;
            else if (o is RFixnum)
                y = ((RFixnum)o).ToLong();
            else if (o is double)
                y = (double)o;
            else if (o is RFloat)
                y = ((RFloat)o).dVal;
            else if (o is RBignum)
                y = ((RBignum)o).Big2Dbl();
            else
                return (RNumeric)CoerceBin(o);
            if (y == 0.0)
                throw new DivideByZeroException("divided by 0");
            return new RFloat(ruby, dVal / y);
        }
        public override RNumeric Remainder(object o)
        {
            double y;
            if (o is int)
                y = (int)o;
            else if (o is long)
                y = (long)o;
            else if (o is RFixnum)
                y = ((RFixnum)o).ToLong();
            else if (o is double)
                y = (double)o;
            else if (o is RFloat)
                y = ((RFloat)o).dVal;
            else if (o is RBignum)
                y = ((RBignum)o).Big2Dbl();
            else
                return (RNumeric)CoerceBin(o);
            if (y == 0.0)
                throw new DivideByZeroException("divided by 0");
            return new RFloat(ruby, dVal % y);
        }
        public override RNumeric Modulo(object o)
        {
            double y;
            if (o is int)
                y = (int)o;
            else if (o is long)
                y = (long)o;
            else if (o is RFixnum)
                y = ((RFixnum)o).ToLong();
            else if (o is double)
                y = (double)o;
            else if (o is RFloat)
                y = ((RFloat)o).dVal;
            else if (o is RBignum)
                y = ((RBignum)o).Big2Dbl();
            else
                return (RNumeric)CoerceBin(o);
            if (y == 0.0)
                throw new DivideByZeroException("divided by 0");
            double z = dVal % y;
            if (z != 0.0)
            {
                if (dVal < 0.0 && y > 0.0 || dVal > 0.0 && y < 0.0)
                    z = z + y;
            }
            return new RFloat(ruby, z);
        }
        public override RArray DivMod(object o)
        {
            double y;
            if (o is int)
                y = (int)o;
            else if (o is long)
                y = (long)o;
            else if (o is RFixnum)
                y = ((RFixnum)o).ToLong();
            else if (o is double)
                y = (double)o;
            else if (o is RFloat)
                y = ((RFloat)o).dVal;
            else if (o is RBignum)
                y = ((RBignum)o).Big2Dbl();
            else
                return (RArray)CoerceBin(o);
            if (y == 0.0)
                throw new DivideByZeroException("divided by 0");
            return RArray.AssocNew(ruby, new RFloat(ruby, dVal / y), Modulo(y));
        }
        static internal new void Init(NetRuby rb)
        {
            BindingFlags bf = BindingFlags.InvokeMethod
                | BindingFlags.Static | BindingFlags.Public
                | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy
                | BindingFlags.Instance;
            RClass flo = rb.DefineClass("Float", rb.cNumeric);
            rb.cFloat = flo;
            rb.oFloat = new RFloat(rb, 0.0);
            Type obj = typeof(RFloat);

            rb.ClassOf(flo).UndefMethod("new");

            flo.DefineMethod("floor", obj.GetMethod("get_Floor", bf));
            flo.DefineMethod("ceil", obj.GetMethod("get_Ceil", bf));
            flo.DefineMethod("round", obj.GetMethod("get_Round", bf));
            flo.DefineMethod("nan?", obj.GetMethod("get_IsNaN", bf));
            flo.DefineMethod("infinite?", obj.GetMethod("get_IsInfinite", bf));
            flo.DefineMethod("finite?", obj.GetMethod("get_IsFinite", bf));
        }
    }

    public abstract class RInteger : RNumeric
    {
        internal RInteger(NetRuby rb, RMetaObject sp)
            : base (rb, sp)
        {
        }

        protected const uint BITSPERDIG = 32;
        protected const ulong BIGRAD = (ulong)1 << (int)BITSPERDIG;
        protected const uint DIGSPERLONG = 1;
        protected static ulong BIGUP(ulong x)
        {
            return (x << (int)BITSPERDIG);
        }
        protected static uint BIGDN(ulong x)
        {
            return (uint)(x >> (int)BITSPERDIG);
        }
        protected static int BIGDN(long x)
        {
            return (int)(x >> (int)BITSPERDIG);
        }
        protected static uint BIGDN(uint x)
        {
            return (x >> (int)BITSPERDIG);
        }
        protected static uint BIGLO(uint x)
        {
            return (x & (uint)(BIGRAD - 1));
        }
        protected static uint BIGLO(long x)
        {
            return (uint)(x & (uint)(BIGRAD - 1));
        }
        protected static uint BIGLO(ulong x)
        {
            return (uint)(x & (BIGRAD - 1));
        }

        public abstract RInteger Normalize { get; }

        public static int ToInt(NetRuby ruby, object o)
        {
            long l = ToLong(ruby, o);
            if (l < Int32.MinValue || l > Int32.MaxValue)
                throw new eRangeError(String.Format("integer {0} too big to convert to `int'",
                                                    l));
            return (int)l;
        }
        public static long ToLong(NetRuby ruby, object o)
        {
            if (o == null)
            {
                throw new eTypeError("no implicit conversion from nil");
            }
            if (o is int) return (long)(int)o;
            if (o is uint) return (long)(uint)o;
            if (o is long) return (long)o;
            if (o is RNumeric)
                return ((RNumeric)o).ToLong();
            if (o is string || o is RString)
                throw new eTypeError("no implicit conversion from string");
            if (o is bool || o is RBool)
                throw new eTypeError("no implicit conversion from boolean");
            RFixnum f = (RFixnum)ruby.ConvertType(o, typeof(RFixnum), "Integer", "to_int");
            return (long)f.ToLong();
        }

        internal static object StringToInteger(NetRuby ruby, string astr, int radix)
        {
            RBignum z = null;
            bool badcheck = (radix == 0) ? true : false;
            string str = astr.Trim().ToUpper().Replace("_", "");
            bool sign = true;
            int idx = 0;
            if (astr.Length == 1)
            {
                return Convert.ToInt32(str);
            }
            if (str[0] == '+')
            {
                idx++;
            }
            else if (str[0] == '-')
            {
                idx++;
                sign = false;
            }
            if (str[idx] == '+' || str[idx] == '-')
            {
                if (badcheck) goto bad;
                return 0;
            }
            if (radix == 0)
            {
                if (str[idx] == '0' && (idx + 1) < str.Length)
                {
                    char c = str[idx + 1];
                    if (c == 'X')
                    {
                        radix = 16;
                    }
                    else if (c == 'B')
                    {
                        radix = 2;
                    }
                    else
                    {
                        radix = 8;
                    }
                }
                else
                {
                    radix = 10;
                }
            }
            int len = str.Length;
            if (radix == 8)
            {
                while (idx < str.Length && str[idx] == '0') idx++;
                if (idx == str.Length) return 0;
                str = str.Substring(idx);
                len = 3 * str.Length;
            }
            else
            {
                if (radix == 16 && str[idx] == '0' && str[idx + 1] == 'X')
                    idx += 2;
                else if (radix == 2 && str[idx] == '0' && str[idx + 1] == 'B')
                    idx += 2;
                while (idx < str.Length && str[idx] == '0') idx++;
                if (idx == str.Length)
                {
                    if (badcheck) goto bad;
                    return 0;
                }
                str = str.Substring(idx);
                len = 4 * str.Length;
            }
            uint x;
            if (len <= 32)
            {
                try
                {
                    x = Convert.ToUInt32(str, radix);
                    if (x > (uint)Int32.MaxValue)
                    {
                        return new RBignum(ruby, x, sign);
                    }
                    else
                    {
                        int xx = (int)x;
                        return (sign) ? xx : -xx;
                    }
                }
                catch (OverflowException)
                {
                    ;
                }
                catch
                {
                    goto bad;
                }
            }

            len = (int)(len / BITSPERDIG) + 1;
            uint[] zds = new uint[len];
            int blen = 1;
            for (idx = 0; idx < str.Length; idx++)
            {
                char c = str[idx];
                switch (c)
                {
                case '8':
                case '9':
                    if (radix == 8)
                    {
                        c = (char)radix;
                        break;
                    }
                    goto case '0';
                case '7': case '6': case '5': case '4':
                case '3': case '2': case '1': case '0':
                    c = (char)(c - '0');
                    break;
                case 'A': case 'B': case 'C': case 'D': case 'E': case 'F':
                    if (radix != 16) c = (char)radix;
                    else c = (char)(c - 'A' + 10);
                    break;
                default:
                    c = (char)radix;
                    break;
                }
                if (c >= radix) break;
                int i = 0;
                ulong num = c;
                for (;;)
                {
                    while (i < blen)
                    {
                        num += (ulong)zds[i] * (ulong)radix;
                        zds[i++] = BIGLO(num);
                        num = BIGDN(num);
                    }
                    if (num != 0)
                    {
                        blen++;
                        continue;
                    }
                    break;
                }
            }
            z = new RBignum(ruby, zds, sign);
            return z;
        
        bad:
            throw new eArgError("Invalid value for Integer: \"" + astr + "\"");
        }

        public virtual RInteger UpTo(object o)
        {
/*        
            object[] up = new object[] { 1 };
            object[] to = new object[] { o };
            RInteger r = this;
            for (;;)
            {
                if (RTest(ruby.Funcall(r, (uint)'>', to))) break;
                ruby.Yield(r);
                r = (RInteger)ruby.Funcall(r, (uint)'+', up);
            }
            return this;
*/
            return null; //PH
        }
        public virtual RInteger DownTo(object o)
        {
/*        
            object[] down = new object[] { 1 };
            object[] to = new object[] { o };
            RInteger r = this;
            for (;;)
            {
                if (RTest(ruby.Funcall(r, (uint)'<', to))) break;
                ruby.Yield(r);
                r = (RInteger)ruby.Funcall(r, (uint)'-', down);
            }
            return this;
*/
            return null; //PH
        }
        public virtual RInteger Step(object to, object step)
        {
/*        
            RInteger i = this;
            object[] tox = new object[] { to };
            long l = ToLong(ruby, step);
            uint cmp;
            if (l == 0)
            {
                throw new eArgError("step cannot be 0");
            }
            object stepx = new object[] { l };
            if (RTest(ruby.Funcall(step, '>', new object[] {0})))
            {
                cmp = (uint)'>';
            }
            else
            {
                cmp = (uint)'<';
            }
            for (;;)
            {
                if (RTest(ruby.Funcall(i, cmp, tox))) break;
                ruby.Yield(i);
                i = (RInteger)ruby.Funcall(i, (uint)'+', stepx);
            }
            return this;            
*/
            return null; //PH
        }
        internal static object int_upto(RBasic r, params object[] args)
        {
            return ((RInteger)r).UpTo(args[0]);
        }
        internal static object int_downto(RBasic r, params object[] args)
        {
            return ((RInteger)r).DownTo(args[0]);
        }
        internal static object int_step(RBasic r, params object[] args)
        {
            return ((RInteger)r).Step(args[0], args[1]);
        }
        
        internal static new void Init(NetRuby rb)
        {
            RClass it = rb.DefineClass("Integer", rb.cNumeric);
            rb.cInteger = it;
            rb.ClassOf(it).UndefMethod("new");

            it.DefineMethod("upto", new RMethod(int_upto), 1);
            it.DefineMethod("downto", new RMethod(int_downto), 1);
            it.DefineMethod("step", new RMethod(int_step), 2);

            it.DefineMethod("to_int", new RMethod(num_to_i), 0);
            it.DefineMethod("floor", new RMethod(num_to_i), 0);
            it.DefineMethod("ceil", new RMethod(num_to_i), 0);
            it.DefineMethod("round", new RMethod(num_to_i), 0);

        }
    }
    
    public class RFixnum : RInteger, IComparable, ICloneable
    {
        internal RFixnum(NetRuby rb)
            : base (rb, rb.cFixnum)
        {
            iVal = 0;
        }
        public RFixnum(NetRuby rb, int i)
            : base (rb, rb.cFixnum)
        {
            iVal = i;
        }
        public override object Clone()
        {
            return MemberwiseClone();
        }
        internal RFixnum SetData(int n)
        {
            iVal = n;
            return this;
        }
        internal int GetData()
        {
            return iVal;
        }
        public override RInteger Normalize
        {
            get { return this; }
        }
        
        public int CompareTo(object o)
        {
            int y;
            if (o is int)
                y = (int)o;
            else if (o is RFixnum)
                y = ((RFixnum)o).iVal;
            else
            {
                throw new ArgumentException("object is not a Integer");
            }
            if (iVal == y) return 0;
            if (iVal > y) return 1;
            return -1;
        }
        public override bool Equals(object o)
        {
            if (o is int)
                return iVal == (int)o;
            if (o is long)
                return iVal == (long)o;
            if (o is double)
                return iVal == (double)o;
            if (o is float)
                return iVal == (float)o;
            if (o is RFloat)
                return iVal == ((RFloat)o).Double;
            if (o is RFixnum)
                return ((RFixnum)o).Equals(iVal);
            return ruby.Equal(o, this);
        }
        public override int GetHashCode() // for voiding CS0659 warning.
        {
            return base.GetHashCode();
        }
        public override string ToString()
        {
            return iVal.ToString();
        }
        public override RInteger ToInteger()
        {
            return (RInteger)Clone();
        }
        public int ToInt()
        {
            return iVal;
        }
        public override long ToLong()
        {
            return (long)iVal;
        }
        public override RChar ToChar()
        {
            return new RChar(ruby, (char)iVal);
        }
        public override RFloat ToFloat()
        {
            return new RFloat(ruby, Convert.ToDouble(iVal));
        }
        public override int id
        {
            get {
                int n = iVal;
                n &= 0x7fffffff;
                return (n << 1) | 1;
            }
        }
        int iVal;

        private bool convi(object o, out int y)
        {
            if (o is int)
                y = (int)o;
            else if (o is RFixnum)
                y = ((RFixnum)o).iVal;
            else
            {
                y = 0;
                return false;
            }
            return true;
        }
        public override bool Gt(object o)
        {
            int y;
            if (convi(o, out y) == false)
                return (bool)CoerceBin(o);
            return (iVal > y);
        }
        public override bool GE(object o)
        {
            int y;
            if (convi(o, out y) == false)
                return (bool)CoerceBin(o);
            return (iVal >= y);
        }
        public override bool Lt(object o)
        {
            int y;
            if (convi(o, out y) == false)
                return (bool)CoerceBin(o);
            return (iVal < y);
        }
        public override bool LE(object o)
        {
            int y;
            if (convi(o, out y) == false)
                return (bool)CoerceBin(o);
            return (iVal <= y);
        }
        public override RNumeric Plus(object o)
        {
            long l;
            if (o is int)
                l = (int)o;
            else if (o is RFixnum)
                l = ((RFixnum)o).iVal;
            else
            {
                if (o is double)
                    return new RFloat(ruby, (double)o + iVal);
                if (o is RFloat)
                    return new RFloat(ruby, ((RFloat)o).Double + iVal);
                return (RNumeric)CoerceBin(o);
            }
            l += (long)iVal;
            if (l > Int32.MaxValue || l < Int32.MinValue)
            {
                return new RBignum(ruby, l);
            }
            return new RFixnum(ruby, (int)l);
        }
        public override RNumeric Minus(object o)
        {
            long l;
            if (o is int)
                l = (int)o;
            else if (o is RFixnum)
                l = ((RFixnum)o).iVal;
            else
            {
                if (o is long)
                    return new RBignum(ruby, (long)iVal - (long)o);
                if (o is double)
                    return new RFloat(ruby, (double)iVal - (double)o);
                if (o is RFloat)
                    return new RFloat(ruby, (double)iVal - ((RFloat)o).Double);
                return (RNumeric)CoerceBin(o);
            }
            l = (long)iVal - l;
            if (l > Int32.MaxValue || l < Int32.MinValue)
            {
                return new RBignum(ruby, l);
            }
            return new RFixnum(ruby, (int)l);
        }
        public override RNumeric Multiply(object o)
        {
            long l;
            if (o is int)
                l = (int)o;
            else if (o is RFixnum)
                l = ((RFixnum)o).iVal;
            else
            {
                if (o is double)
                    return new RFloat(ruby, (double)iVal * (double)o);
                if (o is RFloat)
                    return new RFloat(ruby, (double)iVal * ((RFloat)o).Double);
                return (RNumeric)CoerceBin(o);
            }
            l *= iVal;
            if (l > Int32.MaxValue || l < Int32.MinValue)
            {
                return new RBignum(ruby, l);
            }
            return new RFixnum(ruby, (int)l);
        }
        public override RNumeric Divide(object o)
        {
            int y;
            if (o is int) y = (int)o;
            else if (o is RFixnum) y = (int)((RFixnum)o).iVal;
            else
            {
                if (o is long)
                {
                    long l = (long)o;
                    if (l == 0) throw new DivideByZeroException("divided by 0");
                    return new RBignum(ruby, (long)iVal % l);
                }
                if (o is double)
                {
                    double d = (double)o;
                    if (d == 0.0) throw new DivideByZeroException("divided by 0");
                    return new RFloat(ruby, (double)iVal % d);
                }
                if (o is RFloat)
                {
                    double d = ((RFloat)o).Double;
                    if (d == 0.0) throw new DivideByZeroException("divided by 0");
                    return new RFloat(ruby, (double)iVal % d);
                }
                return (RNumeric)CoerceBin(o);
            }
            if (y == 0)        throw new DivideByZeroException("divided by 0");
            int mod;
            int div = divmod(y, out mod);
            return new RFixnum(ruby, div);
        }
        public override RNumeric Remainder(object o)
        {
            int y;
            if (o is int) y = (int)o;
            else if (o is RFixnum) y = (int)((RFixnum)o).iVal;
            else
            {
                if (o is long)
                {
                    long l = (long)o;
                    if (l == 0) throw new DivideByZeroException("divided by 0");
                    return new RBignum(ruby, (long)iVal % l);
                }
                if (o is double)
                {
                    double d = (double)o;
                    if (d == 0.0) throw new DivideByZeroException("divided by 0");
                    return new RFloat(ruby, (double)iVal % d);
                }
                if (o is RFloat)
                {
                    double d = ((RFloat)o).Double;
                    if (d == 0.0) throw new DivideByZeroException("divided by 0");
                    return new RFloat(ruby, (double)iVal % d);
                }
                return (RNumeric)CoerceBin(o);
            }
            if (y == 0)        throw new DivideByZeroException("divided by 0");
            return new RFixnum(ruby, iVal % y);
        }
        public override RNumeric Modulo(object o)
        {
            int y;
            if (o is int) y = (int)o;
            else if (o is RFixnum) y = (int)((RFixnum)o).iVal;
            else
            {
                return (RNumeric)CoerceBin(o);
            }
            int mod;
            int d = divmod(y, out mod);
            return new RFixnum(ruby, mod);
        }
        private int divmod(int y, out int mod)
        {
            int div;
            mod = 0;
            if (y == 0)
                throw new DivideByZeroException("divided by 0");
            if (y < 0)
            {
                if (iVal < 0)
                    div = -iVal / -y;
                else
                    div = -(iVal / -y);
            }
            else
            {
                if (iVal < 0)
                    div = -(-iVal / y);
                else
                    div = iVal / y;
            }
            mod = iVal - div * y;
            if (mod < 0 && y > 0 || mod > 0 && y < 0)
            {
                mod += y;
                div -= 1;
            }
            return div;
        }
        public override RArray DivMod(object o)
        {
            int y;
            if (o is int) y = (int)o;
            else if (o is RFixnum) y = (int)((RFixnum)o).iVal;
            else
            {
                return (RArray)CoerceBin(o);
            }
            int mod;
            int d = divmod(y, out mod);
            return RArray.AssocNew(ruby, new RFixnum(ruby, d), new RFixnum(ruby, mod));
        }

        static internal object fix_equal(RBasic r, params object[] args)
        {
            return ((RFixnum)r).Equals(args[0]);
        }
        
        static internal new void Init(NetRuby rb)
        {
            RClass fix = rb.DefineClass("Fixnum", rb.cInteger);
            rb.cFixnum = fix;
            RFixnum o = new RFixnum(rb);
            rb.oFixnum = o;
            rb.oFixZero = new RFixnum(rb, 0);
            fix.DefineMethod("to_f", new RMethod(o.ruby_to_f), 0);

            fix.DefineMethod("to_c", new RMethod(o.ruby_to_c), 0);
        
            fix.DefineMethod("==", new RMethod(fix_equal), 1);
        }
    }
}
