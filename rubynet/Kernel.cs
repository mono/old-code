/*
Copyright (C) 1993-2000 Yukihiro Matsumoto
Copyright (C) 2000      Network Applied Communication Laboratory, Inc.
Copyright (C) 2000      Information-technology Promotion Agency, Japan
Copyright (C) 2001-2002 arton
Copyright (C) 2005      Jaen Saul
*/

using System;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;

namespace NETRuby
{
    public class RKernel : RModule
    {
        internal RKernel(NetRuby rb, string name) :
            base(rb, name)
        {
        
        }

        public object IsInstanceOf(RBasic r, params object[] args)
        {
            object o = args[0];
            if (o == null)
            {
                return (r == ruby.oNil) ? true : false;
            }
            else if (o is bool)
            {
                return ((bool)o) ? r == ruby.oTrue : r == ruby.oFalse;
            }
            else if (o is RMetaObject == false)
            {
                throw new eTypeError("class or module required");
            }
            return r.Class == o;
        }

        public object IsKindOf(RBasic obj, params object[] o)
        {
            RMetaObject cl = ruby.ClassOf(obj);
            object c = o[0];
            if (c is RMetaObject == false)
            {
                throw new eTypeError("class or module required");
            }
            while (cl != null)
            {
                if (cl == c || cl.m_tbl == ((RMetaObject)c).m_tbl)
                    return true;
                cl = cl.super;
            }
            return false;
        }

        public object ruby_array(RBasic r, params object[] o)
        {
            RBasic x = ruby.InstanceOf(o[0]);
            object val;
            uint to_ary = ruby.intern("to_ary");
            if (x.RespondTo(to_ary, false))
                val = ruby.Funcall(x, to_ary, null);
            else
                val = ruby.Funcall(x, ruby.intern("to_a"), null);
            if (val is RArray == false)
                throw new eTypeError("`to_a' did not return Array");
            return (RArray)val;
        }
        public object ruby_string(RBasic r, params object[] o)
        {
            RBasic x = ruby.InstanceOf(o[0]);
            return (RString)ruby.ConvertType(x, typeof(RString), "String", "to_s");
        }
        public object ruby_integer(RBasic r, params object[] o)
        {
            RBasic x = ruby.InstanceOf(o[0]);
            return (RInteger)ruby.ConvertType(x, typeof(RInteger), "Integer", "to_int");
        }
        public object ruby_float(RBasic r, params object[] o)
        {
            if (o[0] == null || o[0] == ruby.oNil) return new RFloat(ruby, 0.0);
        
            RBasic x = ruby.InstanceOf(o[0]);
            return (RFloat)ruby.ConvertType(x, typeof(RFloat), "Float", "to_f");
        }
        public object ruby_new(RBasic r, params object[] o)
        {
            return ((RClass)r).NewInstance(o);
        }
        public object ruby_superclass(RBasic r, params object[] o)
        {
            return ((RClass)r).Superclass;
        }
        public object ruby_s_new(RBasic r, params object[] o)
        {
            object[] args = new object[1];
            if (ruby.ScanArgs(o, "01", args) == 0)
            {
                args[0] = ruby.cObject;
            }
            RMetaObject spr = (RMetaObject)args[0];
            return RClass.ClassNew(ruby, spr, o);
        }
        public object extend(RBasic r, params object[] o)
        {
            if (o == null || o.Length == 0)
                throw new eArgError("wrong # of arguments(0 for 1)");
            for (int i = 0; i < o.Length; i++)
                ruby.CheckType(o[i], typeof(RModule));
            uint id = ruby.intern("extend_object");
            for (int i = 0; i < o.Length; i++)
            {
                ruby.Funcall(o[i], id, r);
            }
            return r;
        }
        public object attr(RBasic r, params object[] o)
        {
            object[] argv = new object[2];
            ruby.ScanArgs(o, "11", argv);
            uint id = ruby.ToID(argv[0]);
            ((RMetaObject)r).Attribute(ruby.ToID(argv[0]), true, RBasic.RTest(argv[1]), true);
            return null;
        }
        public object attr_reader(RBasic r, params object[] o)
        {
            for (int i = 0; i < o.Length; i++)
            {
                ((RMetaObject)r).Attribute(ruby.ToID(o[i]), true, false, true);
            }
            return null;
        }
        public object attr_writer(RBasic r, params object[] o)
        {
            for (int i = 0; i < o.Length; i++)
            {
                ((RMetaObject)r).Attribute(ruby.ToID(o[i]), false, true, true);
            }
            return null;
        }
        public object attr_accessor(RBasic r, params object[] o)
        {
            for (int i = 0; i < o.Length; i++)
            {
                ((RMetaObject)r).Attribute(ruby.ToID(o[i]), true, true, true);
            }
            return null;
        }

        enum PFlag {
            NONE = 0,
            SHARP = 1,
            MINUS = 2,
            PLUS = 4,
            ZERO = 8,
            SPACE = 16,
            WIDTH = 32,
            PREC = 64,
        }
        public object ruby_sleep(RBasic r, params object[] o)
        {
            long beg = DateTime.Now.Ticks;
            if (o.Length == 0)
            {
                Thread.Sleep(Timeout.Infinite);
            }
            else if (o.Length == 1)
            {
                int slp = 0;
                if (o[0] is int)
                {
                    slp = (int)o[0] * 1000;
                }
                Thread.Sleep(slp);
            }
            else
            {
                throw new ArgumentException("wrong # of arguments");
            }
            long end = DateTime.Now.Ticks - beg;
            end /= 1000;
            return (int)end;
        }

        internal object initialize(RBasic r, params object[] o)
        {
            return null;
        }
        internal object s_new(RBasic r, params object[] o)
        {
            RModule mod = new RModule(ruby, null, (RMetaObject)r);
            ruby.CallInit(r, o);
            return mod;
        }
        private static int getarg(object[] args, int index, out object arg)
        {
            if (index >= args.Length)
                throw new ArgumentException("too few argument.");
            arg = args[index++];
            return index;
        }
        private static int getaster(string fmt, ref int i, ref int nextarg, object[] args)
        {
            int t = i++;
            int n = 0;
            for (; i < fmt.Length && Char.IsDigit(fmt[i]); i++)
            {
                n = 10 * n + (int)Char.GetNumericValue(fmt[i]);
            }
            if (i >= fmt.Length)
                throw new ArgumentException("malformed format string - %%*[0-9]");
            object temp;
            if (fmt[i] == '$')
            {
                int curarg = nextarg;
                nextarg = n;
                nextarg = getarg(args, nextarg, out temp);
                nextarg = curarg;
            }
            else
            {
                nextarg = getarg(args, nextarg, out temp);
                i = t;
            }
            return Convert.ToInt32(temp);
        }

        private static string remove_sign_bits(string str, int bas)
        {
            StringBuilder sb = new StringBuilder(str);
            int i = 0;
            if (bas == 16)
            {
            x_retry:
                switch (sb[i])
                {
                case 'c': case 'C':
                    sb[i] = '4';
                    break;
                case 'd': case 'D':
                    sb[i] = '5';
                    break;
                case 'e': case 'E':
                    sb[i] = '2';
                    break;
                case 'f': case 'F':
                    if (sb[i + 1] > '8') {
                        i++;
                        goto x_retry;
                    }
                    sb[i] = '1';
                    break;
                case '1':
                case '3':
                case '7':
                    if (sb[i + 1] > '8') {
                        i++;
                        goto x_retry;
                    }
                    break;
                }
                switch (sb[i]) {
                case '1': sb[i] = 'f'; break;
                case '2': sb[i] = 'e'; break;
                case '3': sb[i] = 'f'; break;
                case '4': sb[i] = 'c'; break;
                case '5': sb[i] = 'd'; break;
                case '6': sb[i] = 'e'; break;
                case '7': sb[i] = 'f'; break;
                }
            }
            else if (bas == 8) {
            o_retry:
                switch (sb[i]) {
                case '6':
                    sb[i] = '2';
                    break;
                case '7':
                    if (sb[i + 1] > '3') {
                        i++;
                        goto o_retry;
                    }
                    sb[i] = '1';
                    break;
                case '1':
                case '3':
                    if (sb[i + 1] > '3') {
                        i++;
                        goto o_retry;
                    }
                    break;
                }
                switch (sb[i]) {
                case '1': sb[i] = '7'; break;
                case '2': sb[i] = '6'; break;
                case '3': sb[i] = '7'; break;
                }
            }
            else if (bas == 2) {
                while (i < sb.Length && sb[i] == '1') i++;
                i--;
            }
            int i2 = 0;
            while (i < sb.Length)
                sb[i2++] = sb[i++];
            sb.Length = i2;

            return sb.ToString();
        }

        internal static object sprintf(RBasic r, params object[] args)
        {
            NetRuby ruby = r.ruby;
        
            PFlag flags = PFlag.NONE;
            bool tainted = false;
            object ofmt;
            int nextarg = getarg(args, 0, out ofmt);
            if (ofmt is RBasic)
            {
                tainted = ((RBasic)ofmt).IsTainted;
            }
            string fmt = ofmt.ToString();
            string result = String.Empty;
            int width, prec;
            for (int i = 0; i < fmt.Length; i++)
            {
                int n, ix;
                for (ix = i; ix < fmt.Length && fmt[ix] != '%'; ix++) ;
                result += fmt.Substring(i, ix - i);
                if (ix >= fmt.Length) break;
                i = ix + 1;
                width = prec = -1;
            retry:
                switch (fmt[i]) {
                case ' ':
                    flags |= PFlag.SPACE;
                    i++;
                    goto retry;
                case '#':
                    flags |= PFlag.SHARP;
                    i++;
                    goto retry;
                case '+':
                    flags |= PFlag.PLUS;
                    i++;
                    goto retry;
                case '-':
                    flags |= PFlag.MINUS;
                    i++;
                    goto retry;
                case '0':
                    flags |= PFlag.ZERO;
                    i++;
                    goto retry;
                case '1': case '2': case '3': case '4':
                case '5': case '6': case '7': case '8': case '9':
                    n = 0;
                    for (; i < fmt.Length && Char.IsDigit(fmt[i]); i++)
                    {
                        n = 10 * n + (int)Char.GetNumericValue(fmt[i]);
                    }
                    if (i >= fmt.Length)
                    {
                        throw new ArgumentException("malformed format string - %%[0-9]");
                    }
                    if (fmt[i] == '$')
                    {
                        nextarg = n;
                        i++;
                        goto retry;
                    }
                    width = n;
                    flags |= PFlag.WIDTH;
                    goto retry;
                case '*':
                    if ((flags & PFlag.WIDTH) != 0)
                    {
                        throw new ArgumentException("width given twice");
                    }
                    flags |= PFlag.WIDTH;
                    width = getaster(fmt, ref i, ref nextarg, args);
                    if (width < 0)
                    {
                        flags |= PFlag.MINUS;
                        width = -width;
                    }
                    i++;
                    goto retry;
                case '.':
                    if ((flags & PFlag.PREC) != 0)
                    {
                        throw new ArgumentException("precision given twice");
                    }
                    flags |= PFlag.PREC;
                    prec = 0;
                    i++;
                    if (fmt[i] == '*')
                    {
                        prec = getaster(fmt, ref i, ref nextarg, args);
                        if (prec < 0) {        /* ignore negative precision */
                            flags &= ~PFlag.PREC;
                        }
                        i++;
                        goto retry;
                    }
                    for (; i < fmt.Length && Char.IsDigit(fmt[i]); i++)
                    {
                        prec = 10 * prec + (int)Char.GetNumericValue(fmt[i]);
                    }
                    if (i >= fmt.Length)
                    {
                        throw new ArgumentException("malformed format string - %%.[0-9]");
                    }
                    goto retry;
                case '\n':
                    i--;
                    goto case '%';
                case '\0':
                case '%':
                    if (flags != PFlag.NONE)
                    {
                        throw new ArgumentException("illegal format character - %%");
                    }
                    result += '%';
                    break;
                case 'c': {
                    object val;
                    nextarg = getarg(args, nextarg, out val);
                    if ((flags & PFlag.MINUS) == 0)
                    {
                        if (width > 0) result = result.PadRight(result.Length + width);
                    }
                    char c = (char)Convert.ToInt32(val);
                    result += c;
                    result = result.PadRight(result.Length + width);
                    break;
                }
                case 's': {
                    object arg;
                    nextarg = getarg(args, nextarg, out arg);
                    RString rs = RString.AsRString(ruby, arg);
                    if (rs.IsTainted) tainted = true;
                    int len = rs.Length;
                    if ((flags & PFlag.PREC) != 0)
                    {
                        if (prec < len)
                        {
                            len = prec;
                        }
                    }
                    if ((flags & PFlag.WIDTH) != 0)
                    {
                        if (width > len)
                        {
                            width -= len;
                            if ((flags & PFlag.MINUS) == 0)
                            {
                                if (width > 0) result = result.PadRight(result.Length + width);
                            }
                            result += rs.ToString();
                            if ((flags & PFlag.MINUS) != 0)
                            {
                                if (width > 0) result = result.PadRight(result.Length + width);
                            }
                            break;
                        }
                    }
                    result += rs.ToString().Substring(0, len);
                    break;
                }
                case 'd':
                case 'i':
                case 'o':
                case 'x':
                case 'X':
                case 'b':
                case 'u': {
                    char sc = '\0';
                    char ch = fmt[i];
                    object val;
                    nextarg = getarg(args, nextarg, out val);
                    bool sign = false;
                    bool bignum = false;
                    long num = 0;
                    string prefix = String.Empty;
                    string s = String.Empty;
                    switch (ch)
                    {
                    case 'd':
                    case 'i':
                        sign = true; break;
                    case 'o':
                    case 'x':
                    case 'X':
                    case 'b':
                    case 'u':
                    default:
                        if ((flags & (PFlag.PLUS|PFlag.SPACE)) != 0) sign = true;
                        break;
                    }
                    if ((flags & PFlag.SHARP) != 0)
                    {
                        if (fmt[i] == 'o') prefix = "0";
                        else if (fmt[i] == 'x') prefix = "0x";
                        else if (fmt[i] == 'X') prefix = "0X";
                        else if (fmt[i] == 'b') prefix = "0b";
                        if (prefix.Length > 0)
                        {
                            width -= prefix.Length;
                        }
                    }
                bin_retry:
                    if (val is RFloat)
                    {
                        val = ((RFloat)val).ToInteger();
                        goto bin_retry;
                    }
                    else if (val is double)
                    {
                        val = RFloat.ToInteger(ruby, (double)val);
                        goto bin_retry;
                    }
                    else if (val is string)
                    {
                        val = RInteger.StringToInteger(ruby, (string)val, 0);
                        goto bin_retry;
                    }
                    else if (val is RString)
                    {
                        val = ((RString)val).ToInteger();
                        goto bin_retry;
                    }
                    else if (val is int)
                    {
                        num = (long)(int)val;
                    }
                    else if (val is long)
                    {
                        num = (long)val;
                    }
                    else if (val is uint)
                    {
                        num = (long)(uint)val;
                    }
                    else if (val is RBignum)
                    {
                        bignum = true;
                    }
                    else
                    {
                        num = RInteger.ToLong(ruby, val);
                    }
                    int bas = 0;
                    if (ch == 'u' || ch == 'd' || ch == 'i') bas = 10;
                    else if (ch == 'x' || ch == 'X') bas = 16;
                    else if (ch == 'o') bas = 8;
                    else if (ch == 'b') bas = 2;
                    if (!bignum)
                    {
                        if (sign)
                        {
                            if (ch == 'i') ch = 'd'; /* %d and %i are identical */
                            if (num < 0)
                            {
                                num = -num;
                                sc = '-';
                                width--;
                            }
                            else if ((flags & PFlag.PLUS) != 0)
                            {
                                sc = '+';
                                width--;
                            }
                            else if ((flags & PFlag.SPACE) != 0)
                            {
                                sc = ' ';
                                width--;
                            }
                            s = Convert.ToString(num, bas);
                            goto format_integer;
                        }
                        else
                        {
                            s = Convert.ToString(num, bas);
                            goto format_integer;
                        }
                    }
                    // bignum
                    RBignum big = (RBignum)val;
                    if (sign) {
                        s = big.ToRString(bas).ToString();
                        if (s[0] == '-') {
                            s = s.Substring(1);
                            sc = '-';
                            width--;
                        }
                        else if ((flags & PFlag.PLUS) != 0) {
                            sc = '+';
                            width--;
                        }
                        else if ((flags & PFlag.SPACE) != 0) {
                            sc = ' ';
                            width--;
                        }
                        goto format_integer;
                    }
                    if (big.Sign == false)
                    {
                        big = (RBignum)big.Clone();
                        big.TwoComp();
                    }
                    s = big.ToRString(bas).ToString();
                    if (s[0] == '-') {
                        s = remove_sign_bits(s.Substring(1), bas);
                        StringBuilder sb = new StringBuilder(s.Length + 3);
                        sb.Append("..");
                        switch (bas)
                        {
                        case 16:
                            if (s[0] != 'f') sb.Append('f'); break;
                        case 8:
                            if (s[0] != '7') sb.Append('7'); break;
                        }
                        sb.Append(s);
                        s = sb.ToString();
                    }

                format_integer:
                    int pos = -1;
                    int len = s.Length;

                    if (ch == 'X')
                    {
                        s = s.ToUpper();
                    }
                    if (prec < len)
                        prec = len;
                    width -= prec;
                    if ((flags & (PFlag.ZERO|PFlag.MINUS)) == 0 && s[0] != '.')
                    {
                        if (width > 0) s = s.PadLeft(s.Length + width);
                    }
                    if (sc != '\0') result += sc;
                    if (prefix.Length > 0)
                    {
                        result += prefix;
                        if (pos != 0) pos += prefix.Length;
                    }
                    if ((flags & PFlag.MINUS) == 0)
                    {
                        char c = ' ';

                        if (s[0] == '.')
                        {
                            c = '.';
                            if ((flags & PFlag.PREC) != 0 && prec > len)
                            {
                                pos = result.Length;
                            }
                            else
                            {
                                pos = result.Length + 2;
                            }
                        }
                        else if ((flags & PFlag.ZERO) != 0) c = '0';
                        if (width > 0)
                        {
                            result = result.PadRight(result.Length + width, c);
                        }
                    }
                    if (len < prec)
                    {
                        if (width > 0) result = result.PadRight(result.Length + (prec - len), (s[0]=='.'?'.':'0'));
                    }
                    result += s;
                    if (width > 0)
                    {
                        result = result.PadRight(result.Length + width);
                    }
                    break; }

                case 'f':
                case 'g':
                case 'G':
                case 'e':
                case 'E': {
                    object val;
                    nextarg = getarg(args, nextarg, out val);
                    double fval = 0.0;

                    if (val is RString || val is string)
                    {
                        fval = Convert.ToDouble(((RString)val).ToString());
                    }
                    else if (val is int || val is long || val is uint)
                    {
                        fval = Convert.ToDouble(val);
                    }
                    else if (val is RFloat)
                    {
                        fval = ((RFloat)val).Double;
                    }
                    else if (val is double)
                    {
                        fval = (double)val;
                    }

                    string buf;
                    if (fmt[i] != 'e' && fmt[i] != 'E')
                    {
                        buf = new String(fmt[i], 1);
                        if ((flags & PFlag.PREC) != 0)
                            buf += prec.ToString();
                    }
                    else
                    {
                        buf = "#";
                        if ((flags & PFlag.SHARP) != 0)
                            buf += ".0";
                        buf += fmt[i];
                        if ((flags & PFlag.PLUS) != 0)
                            buf += '+';
                        if ((flags & PFlag.PREC) != 0)
                            buf += new String('0', prec);
                        else
                            buf += '0';
                    }
                    buf = fval.ToString(buf);
                    if ((flags & PFlag.WIDTH) != 0)
                    {
                    }
                    result += buf;
                    break; }
                default:
                    throw new ArgumentException(String.Format("malformed format string - %{0}", fmt[i]));
                }
                flags = PFlag.NONE;
            }
            if (tainted)
                return new RString(ruby, result, true);
            return result;
        }

        internal void Init(NetRuby rb)
        {
            BindingFlags bf = BindingFlags.InvokeMethod
                | BindingFlags.Static | BindingFlags.Public
                | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy
                | BindingFlags.Instance;

            RMethod rmDummy = new RMethod(ruby_dummy);
            rb.cObject.DefinePrivateMethod("initialize", rmDummy, 0);
            rb.cClass.DefinePrivateMethod("inherited", rmDummy, 1);

            DefineMethod("nil?", new RMethod(ruby_isnil), 0);
            DefineMethod("==", new RMethod(ruby_equals), 1);
            DefineAlias("equal?", "==");
            DefineAlias("===", "==");
            DefineMethod("=~", new RMethod(ruby_false), 1);
            DefineMethod("eql?", new RMethod(ruby_eql), 1);

            Type obj = GetType();

            RMethod rm = new RMethod(ruby_id);
            DefineMethod("hash", rm, 0);
            DefineMethod("id", rm, 0);
            DefineMethod("__id__", rm, 0);
            rm = new RMethod(ruby_class);
            DefineMethod("type", rm, 0);
            DefineMethod("class", rm, 0);
        
            DefineMethod("clone", new RMethod(ruby_clone), 0);
            DefineMethod("dup", new RMethod(ruby_dup), 0);
                   DefineMethod("taint", new RMethod(ruby_taint), 0);
            DefineMethod("tainted?", new RMethod(ruby_istainted), 0);
        
            DefineMethod("Untaint", new RMethod(ruby_untaint), 0);
            DefineMethod("freeze", new RMethod(ruby_freeze), 0);
            DefineMethod("frozen?", new RMethod(ruby_isfrozen), 0);
        
            DefineMethod("to_a", new RMethod(ruby_to_a), 0);
            DefineMethod("to_s", new RMethod(ruby_to_s), 0);
            DefineMethod("inspect", new RMethod(ruby_inspect), 0);
            rm = new RMethod(ruby_methods);
            DefineMethod("methods", rm, 0);
            DefineMethod("public_methods", rm, 0);
            DefineMethod("singleton_methods", new RMethod(ruby_singleton_methods), 0);
            DefineMethod("protected_methods", new RMethod(ruby_protected_methods), 0);
            DefineMethod("private_methods", new RMethod(ruby_private_methods), 0);
            DefineMethod("instance_variables", new RMethod(ruby_instance_variables), 0);
            DefinePrivateMethod("remove_instance_variable",
                                new RMethod(ruby_remove_instance_variable), 1);
            DefineMethod("instance_of?", new RMethod(IsInstanceOf), 1);
            rm = new RMethod(IsKindOf);
            DefineMethod("kind_of?", rm, 1);
            DefineMethod("is_a?", rm, 1);

            rb.DefineGlobalFunction("singleton_method_added", rmDummy, 1);
            rb.DefineGlobalFunction("sprintf", new RMethod(sprintf), -1);
            rb.DefineGlobalFunction("format",  new RMethod(sprintf), -1);

            rb.DefineGlobalFunction("Integer", new RMethod(ruby_integer), 1);
            rb.DefineGlobalFunction("Float", new RMethod(ruby_float), 1);
            rb.DefineGlobalFunction("String", new RMethod(ruby_string), 1);
            rb.DefineGlobalFunction("Array", new RMethod(ruby_array), 1);
        
            rb.DefineGlobalFunction("sleep", new RMethod(ruby_sleep), -1);
        
            rb.oNil = new RNil(rb);
            rb.oNil.Init(rb);

            Symbol.Init(rb);

            obj = typeof(RMetaObject);
                rb.cModule.DefineMethod("===", obj.GetMethod("Eqq"));
            rb.cModule.DefineMethod("<=>", obj.GetMethod("CompareTo"));
            rb.cModule.DefineMethod("<", obj.GetMethod("lt"));
            rb.cModule.DefineMethod("<=", obj.GetMethod("le"));
            rb.cModule.DefineMethod(">", obj.GetMethod("gt"));
            rb.cModule.DefineMethod(">=", obj.GetMethod("ge"));
        
            rb.cModule.DefineMethod("included_modules", obj.GetMethod("IncludedModules"));
            rb.cModule.DefineMethod("name", obj.GetMethod("get_ModuleName"));

            rb.cModule.DefineMethod("attr", new RMethod(attr), -1);
            rb.cModule.DefineMethod("attr_reader", new RMethod(attr_reader), -1);
            rb.cModule.DefineMethod("attr_writer", new RMethod(attr_writer), -1);
            rb.cModule.DefineMethod("attr_accessor", new RMethod(attr_accessor), -1);

            rb.cModule.DefineSingletonMethod("new", new RMethod(s_new), 0);
            rb.cModule.DefineMethod("initialize", new RMethod(initialize), -1);
        
            rb.cModule.DefineMethod("instance_methods", obj.GetMethod("ClassInstanceMethods", bf));
            rb.cModule.DefineMethod("public_instance_methods", obj.GetMethod("ClassInstanceMethods", bf));
            rb.cModule.DefineMethod("protected_instance_methods", obj.GetMethod("ClassProtectedInstanceMethods", bf));
            rb.cModule.DefineMethod("private_instance_methods", obj.GetMethod("ClassPrivateInstanceMethods", bf));

            rb.cModule.DefineMethod("constants", new RMethod(rb.cModule.constants), 0);
            rb.cModule.DefineMethod("const_get", new RMethod(rb.cModule.const_get), 1);
            rb.cModule.DefineMethod("const_set", new RMethod(rb.cModule.const_set), 2);
            rb.cModule.DefineMethod("const_defined?", new RMethod(rb.cModule.is_const_defined), 1);
            rb.cModule.DefineMethod("remove_const", new RMethod(rb.cModule.remove_const), 1);
            rb.cModule.DefineMethod("method_added", rmDummy, 1);

            obj = rb.cClass.GetType();
            rb.cClass.DefineMethod("new", new RMethod(ruby_new), -1);
            rb.cClass.DefineMethod("superclass", new RMethod(ruby_superclass), 0);
            rb.cClass.DefineSingletonMethod("new", new RMethod(ruby_s_new), -1);
            rb.cClass.UndefMethod("extend_object");
            rb.cClass.UndefMethod("append_feartures");
            rb.cClass.DefineSingletonMethod("inherited", new RMethod(RClass.Inherited), 1);
        
            rb.cData = rb.DefineClass("Data", rb.cObject);
            rb.ClassOf(rb.cData).UndefMethod("new");

            rb.topSelf = new RMainObject(rb);

            rb.oTrue = new RTrue(rb);
            rb.oTrue.Init(rb);
            rb.oFalse = new RFalse(rb);
            rb.oFalse.Init(rb);
        }

        internal void evalInit()
        {
            DefineMethod("respond_to?", new RMethod(ruby_respond_to), -1);

            DefineMethod("send", new RMethod(ruby_send), -1);
            DefineMethod("__send__", new RMethod(ruby_send), -1);
            DefineMethod("instance_eval", new RMethod(instance_eval), -1);
        }
    }
}

