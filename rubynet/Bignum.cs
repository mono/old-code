/*
Copyright (C) 1993-2001 Yukihiro Matsumoto
Copyright (C) 2001-2002 arton
Copyright (C) 2005      Jaen Saul
*/

using System;
using System.Collections;
using System.Reflection;
using System.Security;
using System.Text;

namespace NETRuby
{
    public class RBignum : RInteger, IComparable, ICloneable
    {
        
        internal RBignum(NetRuby rb, int ln, bool sgn)
            : base(rb, rb.cBignum)
        {
            sign = sgn;
            len = ln;
            digits = new uint[ln];
            for (int i = 0; i < ln; i++) digits[i] = 0;
        }

        internal RBignum(NetRuby rb, uint u, bool sgn)
            : base(rb, rb.cBignum)
        {
            sign = sgn;
            len = 1;
            digits = new uint[1];
            digits[0] = u;
        }

        internal RBignum(NetRuby rb, long l)
            : base(rb, rb.cBignum)
        {
            sign = (l < 0) ? false : true;
            len = 2;
            digits = new uint[2];
            digits[0] = BIGLO(l);
            digits[1] = (uint)BIGDN(l);
        }

        internal RBignum(NetRuby rb, uint[] us, bool sgn)
            : base(rb, rb.cBignum)
        {
            sign = sgn;

            len = us.Length;
            digits = us;
            // normalize
            int i = len;
            while (i-- != 0 && digits[i] == 0);
            len = ++i;
        }

        private bool sign;
        private int len;
        private uint[] digits;

        public ulong Big2Ulong()
        {
            if (len > 8)
                throw new eRangeError("bignum too big to convert into `ulong'");
            ulong num = 0;
            int i = len;
            while (i-- > 0)
            {
                num = BIGUP(num);
                num += digits[i];
            }
            return num;
        }
        
        public override RFloat ToFloat()
        {
            return new RFloat(ruby, Big2Dbl());
        }

        public override long ToLong()
        {
            ulong num = Big2Ulong();
            if (num > (ulong)Int64.MaxValue)
                throw new eRangeError("bignum too big to convert into `long'");
            long l = (long)num;
            return (sign) ? l : -l;
        }
        
        public override object Clone()
        {
            RBignum big = new RBignum(ruby, len, sign);
            Array.Copy(digits, big.digits, len);
            return big;
        }

        public void TwoComp()
        {
            int i = len;
            while (i-- > 0) digits[i] = ~digits[i];
            i = 0;
            ulong num = 1;
            do
            {
                num += digits[i];
                digits[i++] = BIGLO(num);
                num = BIGDN(num);
            }
            while (i < len);
            if (digits[0] == 1 || digits[0] == 0)
            {
                for (i = 1; i < len; i++)
                {
                    if (digits[i] != 0) return;
                }
                uint[] ns = new uint[len + 1];
                for (i = 0; i < len; i++)
                    ns[i] = digits[i];
                ns[len] = 1;
                len++;
                digits = ns;
            }
        }
        public double Big2Dbl()
        {
            double d = 0.0;
            int i = len;
            while (i-- != 0)
            {
                d = digits[i] + BIGRAD * d;
            }
            return (sign) ? d : -d;
        }

        public static RBignum Dbl2Big(NetRuby ruby, double d)
        {
            if (Double.IsInfinity(d))
            {
                throw new eFloatDomainError((d < 0) ? "-Infinity" : "Infinity");
            }
            if (Double.IsNaN(d))
            {
                throw new eFloatDomainError("NaN");
            }
            int i = 0;
            double u = (d < 0) ? -d : d;
            while (u <= Int32.MaxValue == false || 0 != (int)u)
            {
                u /= (double)BIGRAD;
                i++;
            }
            uint[] dg = new uint[i];
            while (i-- != 0)
            {
                u *= BIGRAD;
                dg[i] = (uint)u;
                u -= dg[i];
            }
            return new RBignum(ruby, dg, (d >= 0));
        }
        
        public static RBignum Int2Big(NetRuby ruby, int n)
        {
            bool neg = false;
            if (n < 0)
            {
                n = -n;
                neg = true;
            }
            RBignum big = Uint2Big(ruby, (uint)n);
            if (neg)
            {
                big.sign = false;
            }
            return big;
        }
        
        public static RBignum Uint2Big(NetRuby ruby, uint n)
        {
            RBignum big = new RBignum(ruby, 1, true);
            big.digits[0] = n;
            if (n == 0) big.len = 0;
            return big;
        }

        public static RBignum Long2Big(NetRuby ruby, long n)
        {
            bool neg = false;
            if (n < 0)
            {
                n = -n;
                neg = true;
            }
            RBignum big = Ulong2Big(ruby, (ulong)n);
            if (neg)
            {
                big.sign = false;
            }
            return big;
        }
        
        public static RBignum Ulong2Big(NetRuby ruby, ulong n)
        {
            ulong num = (ulong)n;
            RBignum big = new RBignum(ruby, 2, true);
            big.digits[0] = (uint)n;
            big.digits[1] = (uint)(n >> 32);
            return big;
        }

        static private RBignum to_big(NetRuby ruby, object o)
        {
            RBignum y = null;
            if (o is RFixnum)
                o = ((RFixnum)o).GetData();
            if (o is RBignum)
                y = (RBignum)o;
            if (o is int)
                y = Int2Big(ruby, (int)o);
            else if (o is long)
                y = Long2Big(ruby, (long)o);
            else if (o is double)
                y = Dbl2Big(ruby, (double)o);
            else if (o is RFloat)
                y = Dbl2Big(ruby, ((RFloat)o).Double);
            return y;
        }
        
        public bool Eq(object o)
        {
            RBignum y = null;
            if (o is RFixnum)
                o = ((RFixnum)o).GetData();
            if (o is int)
                y = Int2Big(ruby, (int)o);
            else if (o is long)
                y = Long2Big(ruby, (long)o);
            else if (o is double)
                return (Big2Dbl() == (double)o);
            else if (o is RFloat)
                return (Big2Dbl() == ((RFloat)o).Double);
            else if (o is RBignum == false)
                return false;
            else
                y = (RBignum)o;
            if (sign != y.sign) return false;
            if (len != y.len) return false;
            for (int i = 0; i < len; i++)
            {
                if (digits[i] != y.digits[i]) return false;
            }
            return true;
        }

        public override string ToString()
        {
            return ToRString(10).ToString();
        }

        public override RString ToRString()
        {
            return ToRString(10);
        }
        public override RInteger ToInteger()
        {
            return this;
        }

        private static readonly string hexmap = "0123456789abcdef";
        public RString ToRString(int radix)
        {
            if (len == 0) return new RString(ruby, "0");
            int j = 0;
            int hbase = 0;
            switch (radix)
            {
            case 10:
                j = (32 * len * 241)/800 + 2;
                hbase = 10000;
                break;
            case 16:
                j = (32 * len) / 4 + 2;
                hbase = 0x10000;
                break;
            case 8:
                j = (32 * len) + 2;
                hbase = 0x1000;
                break;
            case 2:
                j = (32 * len) + 2;
                hbase = 0x10;
                break;
            default:
                throw new eArgError("bignum cannot treat base " + base.ToString());
            }
            uint[] t = new uint[len];
            Array.Copy(digits, t, len);
            StringBuilder ss = new StringBuilder(j);
            ss.Length = j;
            int i = len;
            while (i != 0 && j != 0)
            {
                int k = i;
                ulong num = 0;
                while (k-- != 0)
                {
                    num = BIGUP(num) + t[k];
                    t[k] = (uint)(num / (uint)hbase);
                    num %= (uint)hbase;
                }
                if (t[i - 1] == 0) i--;
                k = 4;
                while (k-- != 0)
                {
                    int c = (char)(num % (uint)radix);
                    ss[--j] = hexmap[c];
                    num /= (uint)radix;
                    if (i == 0 && num == 0) break;
                }
            }
            while (ss[j] == '0') j++;
            if (sign == false)
            {
                ss[--j] = '-';
            }
            int ln = ss.Length - j;
            string s = ss.ToString().Substring(j, ln);
            return new RString(ruby, s);
        }
        
        public int CompareTo(object o)
        {
            RBignum r = to_big(ruby, o);
            if (r == null)
            {
                throw new ArgumentException("object is not a Bignum");
            }
            if (sign && !r.sign) return 1;
            if (!sign && r.sign) return -1;
            if (len < r.len)
                return (sign) ? -1 : 1;
            if (len > r.len)
                return (sign) ? 1 : -1;
            int xlen = len;
            while ((xlen-- > 0) && digits[xlen] == r.digits[xlen]);
            if (xlen < 0) return 0;
            return (digits[xlen] > r.digits[xlen]) ? (sign ? 1 : -1) : (sign ? -1 : 1);
        }
        public override bool Gt(object o)
        {
            RBignum r = to_big(ruby, o);
            if (r == null)
            {
                return (bool)CoerceBin(o);
            }
            int i = CompareTo(r);
            return i > 0;
        }
        public override bool GE(object o)
        {
            RBignum r = to_big(ruby, o);
            if (r == null)
            {
                return (bool)CoerceBin(o);
            }
            int i = CompareTo(r);
            return i >= 0;
        }
        public override bool Lt(object o)
        {
            RBignum r = to_big(ruby, o);
            if (r == null)
            {
                return (bool)CoerceBin(o);
            }
            int i = CompareTo(r);
            return i < 0;
        }
        public override bool LE(object o)
        {
            RBignum r = to_big(ruby, o);
            if (r == null)
            {
                return (bool)CoerceBin(o);
            }
            int i = CompareTo(r);
            return i <= 0;
        }
        
        public RInteger Negate()
        {
            RBignum z = (RBignum)Clone();
            if (sign == false) z.TwoComp();
            for (int i = 0; i < len; i++)
            {
                z.digits[i] = ~z.digits[i];
            }
            if (sign) z.TwoComp();
            z.sign = !z.sign;
            return z.Normalize;
        }
        public override RInteger Normalize
        {
            get {
                int i = len;
                while (i-- != 0 && digits[i] == 0);
                len = ++i;
                if (len <= 1 && (int)digits[0] <= Int32.MaxValue)
                {
                    return new RFixnum(ruby, (sign ? (int)digits[0] : -(int)digits[0]));
                }
                return this;
            }
        }
        public override RArray Coerce(object y)
        {
            if (y is int || y is long || y is RFixnum)
                return RArray.AssocNew(ruby, to_big(ruby, y), this);
            throw new eTypeError("Can't create " + ruby.ClassOf(y).Name + " to Bignum");
        }
        public override RNumeric Plus(object o)
        {
            return (this + to_big(ruby, o)).Normalize;
        }
        public override RNumeric Minus(object o)
        {
            return (this - to_big(ruby, o)).Normalize;
        }
        public override RNumeric Multiply(object o)
        {
            return (this * to_big(ruby, o)).Normalize;
        }
        public override RNumeric Divide(object o)
        {
            RBignum z;
            RBignum y = divmod(o, out z);
            return y.Normalize;
        }
        public override RNumeric Modulo(object o)
        {
            RBignum z;
            divmod(o, out z);
            return z.Normalize;
        }
        public override RNumeric Remainder(object o)
        {
            return (this % to_big(ruby, o)).Normalize;
        }
        public override RArray DivMod(object o)
        {
            RBignum z;
            RBignum y = divmod(o, out z);
            return RArray.AssocNew(ruby, y.Normalize, z.Normalize);
        }
        private RBignum divmod(object o, out RBignum mod)
        {
            mod = null;
            RBignum y = to_big(ruby, o);
            RBignum dv = divrem(y, out mod);
            if (sign != y.sign && mod.len > 0)
            {
                dv = dv - to_big(ruby, 1);
                mod = mod + y;
            }
            return dv;
        }

        public override bool Equals(object o)
        {
            return base.Equals(o);
        }
        public bool Sign
        {
            get { return sign; }
        }
        static internal object ruby_neg(RBasic r, params object[] args)
        {
            return ((RBignum)r).Negate();
        }
        static internal object ruby_eq(RBasic r, params object[] args)
        {
            return ((RBignum)r).Eq(args[0]);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public static bool operator ==(RBignum b1, RBignum b2)
        {
            return b1.Eq(b2);
        }
        public static bool operator !=(RBignum b1, RBignum b2)
        {
            return !b1.Eq(b2);
        }
        public static RBignum operator +(RBignum b1, RBignum b2)
        {
            return b1.add(b2, true);
        }
        public static RBignum operator -(RBignum b1, RBignum b2)
        {
            return b1.add(b2, false);
        }
        public static RBignum operator *(RBignum x, RBignum y)
        {
            int j = x.len + y.len + 1;
            uint[] z = new uint[j];
            while (j-- != 0) z[j] = 0;
            for (int i = 0; i < x.len; i++)
            {
                ulong dd = x.digits[i];
                if (dd == 0) continue;
                ulong n = 0;
                for (j = 0; j < y.len; j++)
                {
                    ulong ee = n + dd * y.digits[j];
                    n = (ulong)z[i + j] + ee;
                    if (ee != 0) z[i + j] = BIGLO(n);
                    n = BIGDN(n);
                }
                if (n != 0)
                {
                    z[i + j] = (uint)n;
                }
            }
            return new RBignum(x.ruby, z, x.sign == y.sign);
        }
        public static RBignum operator /(RBignum x, RBignum y)
        {
            RBignum mod;
            return x.divrem(y, out mod);
        }
        public static RBignum operator %(RBignum x, RBignum y)
        {
            RBignum mod;
            RBignum div = x.divrem(y, out mod);
            return mod;
        }
        public static bool operator >(RBignum x, RBignum y)
        {
            return x.Gt(y);
        }
        public static bool operator >=(RBignum x, RBignum y)
        {
            return x.GE(y);
        }
        public static bool operator <(RBignum x, RBignum y)
        {
            return x.Lt(y);
        }
        public static bool operator <=(RBignum x, RBignum y)
        {
            return x.LE(y);
        }

        private RBignum sub(RBignum y)
        {
            uint[] xx = digits;
            uint[] yy = y.digits;
            int xlen = len;
            int ylen = y.len;
            bool sig = true;
            int i;
            if (len < y.len)
            {
                xx = y.digits;
                yy = digits;
                sig = false;
                xlen = y.len;
                ylen = len;
            }
            else if (len == y.len)
            {
                for (i = len; i > 0; )
                {
                    i--;
                    if (xx[i] > yy[i])
                        break;
                    if (xx[i] < yy[i])
                    {
                        xx = y.digits;
                        yy = digits;
                        xlen = y.len;
                        ylen = len;
                        sig = false;
                    }
                }
            }
            uint[] z = new uint[xlen];
            long num = 0;
            for (i = 0; i < ylen; i++)
            {
                num += (long)xx[i] - yy[i];
                z[i] = BIGLO(num);
                num = BIGDN(num);
            }
            while (num != 0 && i < xlen)
            {
                num += xx[i];
                z[i++] = BIGLO(num);
                num = BIGDN(num);
            }
            while (i < xlen)
            {
                z[i] = xx[i];
                i++;
            }
            return new RBignum(ruby, z, sig);
        }
        private RBignum add(RBignum y, bool s)
        {
            bool sig = (s == y.sign);
            if (sign != sig)
            {
                if (sig) return y.sub(this);
                return sub(y);
            }
            uint[] xx = digits;
            uint[] yy = y.digits;
            int xlen = len;
            int ylen = y.len;
            int nl = len;
            if (nl > y.len)
            {
                nl++;
                xx = y.digits;
                yy = digits;
                xlen = y.len;
                ylen = len;
            }
            else
            {
                nl = y.len + 1;
            }
            uint[] z = new uint[nl];
            ulong num = 0;
            int i;
            for (i = 0; i < xlen; i++)
            {
                num += (ulong)xx[i] + (ulong)yy[i];
                z[i] = BIGLO(num);
                num = BIGDN(num);
            }
            while (num != 0 && i < ylen)
            {
                num += yy[i];
                z[i++] = BIGLO(num);
                num = BIGDN(num);
            }
            while (i < ylen)
            {
                z[i] = yy[i];
                i++;
            }
            z[i] = (uint)num;
            return new RBignum(ruby, z, sig);
        }

        private RBignum divrem(RBignum y, out RBignum mod)
        {
            mod = null;
            uint dd;
            int ny = y.len;
            int nx = len;
            int i;
            ulong t2;
            if (ny == 0 && y.digits[0] == 0)
                throw new DivideByZeroException("divided by 0");
            if (nx < ny || nx == ny && digits[nx - 1] < y.digits[ny - 1])
            {
                mod = this;
                return new RBignum(ruby, (uint)0, true);
            }
            if (ny == 1)
            {
                dd = y.digits[0];
                RBignum z = (RBignum)Clone();
                i = nx;
                t2 = 0;
                while (i-- != 0)
                {
                    t2 = (ulong)BIGUP(t2) + z.digits[i];
                    z.digits[i] = (uint)(t2 / dd);
                    t2 %= dd;
                }
                z.sign = (sign == y.sign);
                mod = Uint2Big(ruby, (uint)t2);
                mod.sign = sign;
                return z;
            }
        
            uint[] zz = new uint[(nx == ny) ? nx + 2 : nx + 1];
            if (nx == ny) zz[nx + 1] = 0;
            while (y.digits[ny - 1] == 0) ny--;
        
            dd = 0;
            uint q = y.digits[ny - 1];
            int j = 0;
            uint[] ys = y.digits;
            while ((q & ((uint)1 << (int)(BITSPERDIG-1))) == 0)
            {
                q <<= 1;
                dd++;
            }
            if (dd != 0)
            {
                RBignum yy = (RBignum)y.Clone();
                j = 0;
                t2 = 0;
                while (j < ny)
                {
                    t2 += ((ulong)y.digits[j]) << (int)dd;
                    yy.digits[j++] = BIGLO(t2);
                    t2 = BIGDN(t2);
                }
                ys = yy.digits;
                j = 0;
                t2 = 0;
                while (j < nx)
                {
                    t2 += ((ulong)digits[j]) << (int)dd;
                    zz[j++] = BIGLO(t2);
                    t2 = BIGDN(t2);
                }
                zz[j] = (uint)t2;
            }
            else
            {
                zz[nx] = 0;
                j = nx;
                while (j-- != 0) zz[j] = digits[j];
            }

            j = (nx == ny) ? nx + 1 : nx;
            do
            {
                if (zz[j] == ys[ny - 1]) q = (uint)(BIGRAD - 1);
                else q = (uint)((BIGUP(zz[j]) + zz[j - 1])/ys[ny - 1]);
                if (q != 0)
                {
                    i = 0;
                    long num = 0;
                    t2 = 0;
                    do
                    {
                        t2 += (ulong)ys[i] * q;
                        ulong ee = (ulong)(num - BIGLO(t2));
                        num = (long)(zz[j - ny + i] + ee);
                        if (ee != 0) zz[j - ny + i] = BIGLO(num);
                        num = BIGDN(num);
                        t2 = BIGDN(t2);
                    }
                    while (++i < ny);
                    num += (long)(zz[j - ny + i] - t2);
                    while (num != 0)
                    {
                        i = 0;
                        num = 0;
                        q--;
                        do
                        {
                            ulong ee = (ulong)(num + ys[i]);
                            num = (long)((ulong)zz[j - ny + i] + ee);
                            if (ee != 0) zz[j - ny + i] = BIGLO(num);
                            num = BIGDN(num);
                        }
                        while (++i < ny);
                        num--;
                    }
                }
                zz[j] = q;
            }
            while (--j >= ny);
            RBignum div = new RBignum(ruby, zz, sign == y.sign);
            mod = (RBignum)div.Clone();
            j = (nx == ny ? nx + 2 : nx + 1) - ny;
            for (i = 0; i < j; i++) div.digits[i] = div.digits[i + ny];
            div.len = i;
        
            while (ny-- != 0 && mod.digits[ny] == 0);
            ny++;
            if (dd != 0)
            {
                t2 = 0;
                i = ny;
                while (i-- != 0)
                {
                    t2 = (t2 | mod.digits[i]) >> (int)dd;
                    q = mod.digits[i];
                    mod.digits[i] = BIGLO(t2);
                    t2 = BIGUP(q);
                }
                mod.len = ny;
                mod.sign = sign;
            }
            return div;
        }
        
        static internal new void Init(NetRuby rb)
        {
            RClass big = rb.DefineClass("Bignum", rb.cInteger);
            rb.cBignum = big;

            big.DefineMethod("~", new RMethod(ruby_neg), 0);
        
            RMethod rm = new RMethod(ruby_eq);
            big.DefineMethod("==", rm, 1);
            big.DefineMethod("===", rm, 1);
            big.DefineMethod("eql?", rm, 1);
        }
    }
}
