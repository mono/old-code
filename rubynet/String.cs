/*
Copyright (C) 2001-2002 arton
Copyright (C) 2005      Jaen Saul
*/

using System;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Reflection;
using System.Security;
using System.Text.RegularExpressions;

namespace NETRuby
{
    public class RString : RBasic, ICloneable, IComparable
    {
        // this class has no org ptr,
        // because CLR manages all string, so we don't need to care.
        public RString(NetRuby rb, string s) :
            base(rb, rb.cString)
        {
            ptr = s;
        }
        public RString(RString rs) :
            base(rs)
        {
            ptr = rs.ptr;
        }
        public RString(NetRuby rb, string s, bool taint) :
            base(rb, rb.cString)
        {
            ptr = s;
            if (taint)
                flags |= FL.TAINT;
        }

        public RString Initialize(object str2)
        {
            if (this == str2) return this;
            if (str2 is string)
            {
                ptr = (string)str2;
            }
            else if (str2 is RString)
            {
                ptr = ((RString)str2).ptr;
                if (((RString)str2).IsTainted) Set(FL.TAINT);
            }
            else
            {
                ptr = obj_to_s(str2);
                if (str2 is RBasic && ((RBasic)str2).IsTainted) Set(FL.TAINT);
            }
            return this;
        }
        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o is string)
                return ptr == (string)o;
            RString r;
            if (o is RString)
            {
                r = (RString)o;
            }
            else
            {
                r = (RString)ruby.CheckConvertType(o, typeof(RString), "String", "to_str");
                if (r == null) return false;
            }
            return ptr == r.ptr;
        }
        public override int GetHashCode() // for voiding CS0659 warning.
        {
            return base.GetHashCode();
        }
        public int CompareTo(object str2)
        {
            string s = null;
            if (str2 is string) s = (string)str2;
            else if (str2 is RString) s = ((RString)str2).ptr;
            else throw new ArgumentException("object must be the same type");
            return ptr.CompareTo(s);
        }
        public int CompareMethod(object str2)
        {
            string s = null;
            if (str2 is string) s = (string)str2;
            else if (str2 is RString) s = ((RString)str2).ptr;
            else s = StringToRString(ruby, str2).ptr;
            return CompareTo(s);
        }
        public static string AsString(NetRuby ruby, object x)
        {
            if (x == null)
                return String.Empty;
            if (x is string)
                return (string)x;
            if (x is RString)
                return x.ToString();
            object o = ruby.Funcall(x, "to_s", null);
            return o.ToString();
        }
        public override RInteger ToInteger()
        {
            object o = RInteger.StringToInteger(ruby, ptr, 0);
            if (o is int)
                return new RFixnum(ruby, (int)o);
            else if (o is long)
                return new RBignum(ruby, (long)o);
            return (RInteger)o;
        }
        public override RFloat ToFloat()
        {
            string s = ptr.Trim().Replace("_", "");
            return new RFloat(ruby, Convert.ToDouble(s.Trim()));
        }
        // create new instance(INFECTED) RString
        public static RString AsRString(NetRuby ruby, object x)
        {
            if (x == null)
                return new RString(ruby, String.Empty);
            if (x is string)
                return new RString(ruby, (string)x, false);
            if (x is RString)
                return (RString)x;
            object o = ruby.Funcall(x, "to_s", null);
            if (o is string)
                return new RString(ruby, (string)o);
            if (o is RString)
            {
                RString r = (RString)o;
                if (x is RBasic && ((RBasic)x).IsTainted)
                    r.Set(FL.TAINT);
                return r;
            }
            else if (o is RBasic)
                return ((RBasic)o).ToRString();
            else
                return new RString(ruby, o.ToString());
        }
        public static string ToString(NetRuby ruby, object s)
        {
            if (s is string) return (string)s;
            if (s is RString) return s.ToString();
            return StringToRString(ruby, s).ToString();
        }
        public static RString StringToRString(NetRuby ruby, object s)
        {
            return (RString)ruby.ConvertType(s, typeof(RString), "String", "to_str");
        }
        public override string ToString()
        {
            return ptr;
        }
        public override RString ToRString()
        {
            return this;
        }
        internal static RString New(NetRuby rb, object[] args)
        {
            RString rs = new RString(rb, String.Empty);
            rb.CallInit(rs, args);
            return rs;
        }
        internal RString SetData(string s)
        {
            ptr = s;
            return this;
        }
        public object Clone()
        {
            RString rs = Dup();
            // CLONESETUP
            return rs;
        }
        
        internal RString Dup(object o)
        {
            // any object to string
            return null;
        }
        public RString Dup()
        {
            RString rs = new RString(this);
            return rs;
        }
        
        internal object Plus(object o)
        {
            string s = ToString(ruby, o);
            string result = ptr + s;
            if (IsTainted)
            {
                RString n = new RString(ruby, result, true);
                return n;
            }
            return result;
        }

        public object ToInteger(int radix)
        {
            bool sign = true;
            bool badcheck = (radix == 0) ? true : false;
            string s = ptr.Trim();
            if (s[0] == '+')
            {
                s = s.Substring(1);
            }
            else if (s[0] == '-')
            {
                s = s.Substring(1);
                sign = false;
            }
            if (s[0] == '+' || s[0] == '-')
            {
                if (badcheck)
                    throw new eArgError("invalid value for Integer: \"" + ptr + "\"");
                return 0;
            }
            if (radix == 0)
            {
                if (s[0] == '0')
                {
                    if (s[1] == 'x' || s[1] == 'X')
                    {
                        radix = 16;
                    }
                    else if (s[1] == 'b' || s[1] == 'B')
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
            int pos = 0;
            if (radix == 8)
            {
                while (s[pos] == '0') pos++;
                if (pos == s.Length) return 0;
                while (s[pos] == '_') pos++;
                s = s.Substring(pos);
            }
            else
            {
                if (radix == 16 && s[0] == '0' && (s[1] == 'x' || s[1] == 'X'))
                {
                    s = s.Substring(2);
                }
                if (radix == 2 && s[0] == '0' && (s[1] == 'b' || s[1] == 'B'))
                {
                    s = s.Substring(2);
                }
                while (s[pos] == '0') pos++;
                if (Char.IsWhiteSpace(s[pos]))
                {
                    if (badcheck)
                        throw new eArgError("invalid value for Integer: \"" + ptr + "\"");
                    return 0;
                }
                if (pos == s.Length) pos--;
                s = s.Substring(pos);
            }
            s.Replace("_", "");
            try
            {
                int result = Convert.ToInt32(ptr, radix);
                return (sign) ? result : -result;
            }
            catch (ArgumentException)
            {
                if (badcheck)
                    throw new eArgError("invalid value for Integer: \"" + ptr + "\"");
                return 0;
            }
            catch (OverflowException)
            {
                ;
            }
            return "bignum";
        }

        private struct EscTbl
        {
            internal EscTbl(char c, string s)
            {
                code = c;
                result = s;
            }
            internal char code;
            internal string result;
        }
        static EscTbl[] esctbl = new EscTbl[] {
            new EscTbl('\n', "\\n"),
            new EscTbl('\r', "\\r"),
            new EscTbl('\t', "\\t"),
            new EscTbl('\f', "\\f"),
            new EscTbl('\v', "\\v"),
            new EscTbl('\a', "\\a"),
            new EscTbl('\b', "\\b"), // .NET (not ruby)
            new EscTbl('\u001b', "\\e"),
        };
        
        public override object Inspect()
        {
            string result = "\"";
            foreach (char c in ptr)
            {
                bool esc;
                if (c == '"' || c == '\\')
                {
                    result += "\\" + c;
                }
                else
                {
                    UnicodeCategory uc = Char.GetUnicodeCategory(c);
                    if (uc == UnicodeCategory.Control ||
                        uc == UnicodeCategory.NonSpacingMark ||
                        uc == UnicodeCategory.OtherNotAssigned ||
                        uc == UnicodeCategory.Surrogate)
                    {
                        esc = false;
                        for (int i = 0; i < esctbl.Length; i++)
                        {
                            if (esctbl[i].code == c)
                            {
                                esc = true;
                                result += esctbl[i].result;
                                break;
                            }
                        }
                        if (esc == false)
                        {
                            result += String.Format("\\u{0:x4}", c);
                        }
                    }
                    else
                    {
                        result += c;
                    }
                }
            }
            result += "\"";
            if (IsTainted)
            {
                RString rs = new RString(ruby, result, true);
                return rs;
            }
            return result;
        }

        public int Length
        {
            get { return ptr.Length; }
        }
        public bool IsEmpty
        {
            get { return (ptr == null || ptr.Length == 0); }
        }
        
        public RString UpCase()
        {
            RString rs = Dup();
            return rs.UpCaseBang();
        }

        public RString DownCase()
        {
            RString rs = Dup();
            return rs.DownCaseBang();
        }

        public RString Capitalize()
        {
            RString rs = Dup();
            return rs.CapitalizeBang();
        }

        public RString SwapCase()
        {
            RString rs = Dup();
            return rs.SwapCaseBang();
        }

        public RString UpCaseBang()
        {
            ptr = ptr.ToUpper();
            return this;
        }

        public RString DownCaseBang()
        {
            ptr = ptr.ToLower();
            return this;
        }

        public RString CapitalizeBang()
        {
            StringBuilder sb = new StringBuilder(ptr.ToLower());
            if (sb.Length > 0)
                sb[0] = Char.ToUpper(sb[0]);
            ptr = sb.ToString();
            return this;
        }

        public RString SwapCaseBang()
        {
            StringBuilder sb = new StringBuilder(ptr);
            for (int i = 0; i < sb.Length; i++)
            {
                if (Char.IsUpper(sb[i]))
                    sb[i] = Char.ToLower(sb[i]);
                else if (Char.IsLower(sb[i]))
                    sb[i] = Char.ToUpper(sb[i]);
            }
            ptr = sb.ToString();
            return this;
        }

        public RString Sub(params object[] args)
        {
            RString s = Dup();
            s.SubAt(args);
            return s;
        }
        private RRegexp GetPat(object pat)
        {
            if (pat is RRegexp)
            {
                return (RRegexp)pat;
            }
            if (pat is string || pat is RString)
            {
                return (RRegexp)RRegexpClass.s_new(ruby.cRegexp, pat);
            }
            ruby.CheckType(pat, typeof(RRegexp));
            return null;
        }
        public RString SubAt(params object[] args)
        {
            bool iter = false;
            RString repl = null;
            bool tainted = false;
/*            
            if (args != null && args.Length == 1 && ruby.IsBlockGiven)
            {
                iter = true;
            }            
            else
            */
            if (args != null && args.Length == 2)
            {
                repl = StringToRString(ruby, args[1]);
                tainted = repl.IsTainted;
            }
            else
            {
                throw new eArgError(String.Format("wrong # of arguments({0} for 2)",
                                                  (args == null) ? 0 : args.Length));
            }
            RRegexp pat = GetPat(args[0]);
            if (pat.Search(ptr, 0, false, IsTainted) >= 0)
            {
                RThread th = ruby.GetCurrentContext();
                RMatchData match = (RMatchData)th.BackRef;
                int beg = match.Begin(0);
                int len = match.End(0) - beg;
                if (iter)
                {
/*                
                    RRegexpClass.matchBusy(match);
                    repl = RString.AsRString(ruby, ruby.Yield(match[0]));
                    th.BackRef = match;        // rescue from yield.
*/                    
                }
                else
                {
                    repl = new RString(ruby, match.Sub(repl.ToString(), ptr));
                }
                if (repl.IsTainted) tainted = true;
                StringBuilder sb = new StringBuilder(ptr);
                sb.Remove(beg, len);
                sb.Insert(beg, repl.ptr, 1);
                ptr = sb.ToString();
                if (tainted) Taint();
                return this;
            }
            return null;
        }
        public RString Gsub(params object[] args)
        {
            RString s = Dup();
            RString x = s.GsubAt(args);
            return (x == null) ? s : x;
        }
        public RString GsubAt(params object[] args)
        {
            bool iter = false;
            RString repl = null;
            bool tainted = false;
/*            
            if (args != null && args.Length == 1 && ruby.IsBlockGiven)
            {
                iter = true;
            }            
            else
            */
            if (args != null && args.Length == 2)
            {
                repl = StringToRString(ruby, args[1]);
                tainted = repl.IsTainted;
            }
            else
            {
                throw new eArgError(String.Format("wrong # of arguments({0} for 2)",
                                                  (args == null) ? 0 : args.Length));
            }
            RRegexp pat = GetPat(args[0]);
            int beg = pat.Search(ptr, 0, false, IsTainted);
            if (beg < 0) return null;

            StringBuilder sb = new StringBuilder();
            RThread th = ruby.GetCurrentContext();
            int offset = 0;
            RMatchData match = null;
            while (beg >= 0)
            {
                string val;
                match = (RMatchData)th.BackRef;
                beg = match.Begin(0);
                int len = match.End(0) - beg;
                /*
                if (iter)
                {
                
                    RRegexpClass.matchBusy(match);
                    repl = RString.AsRString(ruby, ruby.Yield(match[0]));
                    th.BackRef = match;        // rescue from yield.
                    if (repl.IsTainted) tainted = true;
                    val = repl.ToString();
                
                }
                else
                */
                {
                    val = match.Sub(repl.ToString(), ptr);
                }
                if (beg > offset)
                {
                    sb.Append(ptr, offset, beg - offset);
                }
                sb.Append(val);
                if (len == 0)
                {
                    if (ptr.Length > match.End(0))
                    {
                        sb.Append(ptr, match.End(0), 1);
                    }
                    offset = beg + 1;
                }
                else
                {
                    offset = beg + len;
                }
                if (offset > ptr.Length) break;
                beg = pat.Search(ptr, offset, false, IsTainted);
            }
            if (ptr.Length > offset)
            {
                sb.Append(ptr, offset, ptr.Length - offset);
            }
            th.BackRef = match;        // rescue from yield.
            ptr = sb.ToString();
            if (tainted) Taint();
            return this;
        }
        
        public object Dump()
        {
            return Inspect();
        }

        internal object str_equal(RBasic obj, params object[] o)
        {
            return (o[0] == obj) ? true : false;
        }
        
        private string ptr;

        static internal void Init(NetRuby rb)
        {
            BindingFlags bf = BindingFlags.InvokeMethod
                | BindingFlags.Static | BindingFlags.Public
                | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy
                | BindingFlags.Instance;
            RClass str = rb.DefineClass("String", rb.cObject);
            rb.cString = str;
            rb.oString = new RString(rb, String.Empty);
#if MIXIN
            RClass.IncludeModule(str, rb.mComparable);
            RClass.IncludeModule(str, rb.mEnumerable);
#endif
            Type obj = typeof(RString);
            str.DefineSingletonMethod("new", obj.GetMethod("New", bf));
            str.DefineMethod("initialize", obj.GetMethod("Initialize", bf));
            str.DefineMethod("clone", obj.GetMethod("Clone", bf));
            str.DefineMethod("dup", obj.GetMethod("Dup", new Type[0]));
            str.DefineMethod("<=>", obj.GetMethod("CompareMethod", bf));
            str.DefineMethod("eql?", obj.GetMethod("Equals", bf & (~BindingFlags.Static)));
            str.DefineMethod("equal?", new RMethod(rb.oString.str_equal), 1);
            str.DefineMethod("+", obj.GetMethod("Plus", bf));
            str.DefineMethod("length", obj.GetMethod("get_Length", bf));
            str.DefineMethod("size", obj.GetMethod("get_Length", bf));
            str.DefineMethod("empty?", obj.GetMethod("get_IsEmpty", bf));
        
            str.DefineMethod("to_i", obj.GetMethod("ToInteger", new Type[0]));
            str.DefineMethod("to_f", obj.GetMethod("ToFloat", bf));
            MethodInfo mi = obj.GetMethod("ToRString", bf);
            str.DefineMethod("to_s", mi);
            str.DefineMethod("to_str", mi);
            str.DefineMethod("inspect", obj.GetMethod("Inspect", bf));
            str.DefineMethod("dump", obj.GetMethod("Dump", bf));

            str.DefineMethod("upcase", obj.GetMethod("UpCase", bf));
            str.DefineMethod("downcase", obj.GetMethod("DownCase", bf));
            str.DefineMethod("capitalize", obj.GetMethod("Capitalize", bf));
            str.DefineMethod("swapcase", obj.GetMethod("SwapCase", bf));

            str.DefineMethod("upcase!", obj.GetMethod("UpCaseBang", bf));
            str.DefineMethod("downcase!", obj.GetMethod("DownCaseBang", bf));
            str.DefineMethod("capitalize!", obj.GetMethod("CapitalizeBang", bf));
            str.DefineMethod("swapcase!", obj.GetMethod("SwapCaseBang", bf));

            str.DefineMethod("sub", obj.GetMethod("Sub", bf));
            str.DefineMethod("gsub", obj.GetMethod("Gsub", bf));

            str.DefineMethod("sub!", obj.GetMethod("SubAt", bf));
            str.DefineMethod("gsub!", obj.GetMethod("GsubAt", bf));
        }
    }

}
