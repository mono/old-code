/*
Copyright (C) 2001-2002 arton
Copyright (C) 2005      Jaen Saul
*/

using System;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace NETRuby
{
    public class RRegexp : RBasic, ICloneable
    {
        public RRegexp(NetRuby rb, RMetaObject meta) :
            base(rb, meta)
        {
            regex = null;
        }
        Regex regex;

        public string Source
        {
            get { Check(); return regex.ToString(); }
        }
        public bool IsCaseFold
        {
            get { Check(); return (regex.Options & RegexOptions.IgnoreCase) != 0; }
        }
        internal Regex Regex
        {
            get { return regex; }
        }
        public int Search(string s, int pos, bool rev, bool stringTainted)
        {
            if (pos > s.Length) return -1;

            RThread th = ruby.GetCurrentContext();
            Check();
            if (ruby.cRegexp.IsRecompileNeed)
                checkPreparation();
            Regex re = regex;
            if (rev)
            {
                re = new Regex(re.ToString(), re.Options | RegexOptions.RightToLeft);
            }
            Match m = re.Match(s, pos);
            if (m.Success == false)
            {
                th.BackRef = null;
                return -1;
            }
            RMatchData mt = (RMatchData)th.BackRef;
            if (mt == null || mt.Test(RRegexpClass.MatchBusy))
            {
                mt = new RMatchData(ruby, s, m);
            }
            else
            {
/*            
                if (th.safeLevel >= 3)
                    mt.Taint();
                else
                    mt.Untaint();
*/                    
                mt.SetData(s, m);
            }
            th.BackRef = mt;
            mt.Infect(this);
            if (stringTainted) mt.Taint();
            return m.Index;
        }
        public override object Inspect()
        {
            Check();
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("/{0}/", regex.ToString());
            RegexOptions opt = regex.Options;
            if ((opt & RegexOptions.Multiline) != 0)
                sb.Append("m");
            if ((opt & RegexOptions.IgnoreCase) != 0)
                sb.Append("i");
            if ((opt & RegexOptions.IgnorePatternWhitespace) != 0)
                sb.Append("x");
            string s = sb.ToString();
            if (IsTainted)
                return new RString(ruby, s, true);
            return s;
        }
        public object Clone()
        {
            return MemberwiseClone();
        }

        internal void Check()
        {
            if (regex == null)
                throw new eTypeError("uninitialized Regexp");
        }

        internal void Initialize(RRegexp r, RegexOptions fl)
        {
            regex = r.regex;
            if (regex.Options != fl)
            {
                regex = new Regex(regex.ToString(), fl);
            }
        }
        internal void Initialize(string s, RegexOptions fl)
        {
            regex = new Regex(s, fl);
        }

        public override bool Equals(object o)
        {
            if (base.Equals(o)) return true;
            if (o is RRegexp == false) return false;
            RRegexp re = (RRegexp)o;
            Check();
            re.Check();
            if (re.regex.ToString() != regex.ToString() || re.regex.Options != regex.Options)
                return false;
            return true;
        }
        public override int GetHashCode() // for voiding CS0659 warning.
        {
            return base.GetHashCode();
        }
        void checkPreparation()
        {
            bool needRecompile = false;
            Check();
            RegexOptions options = regex.Options;
            bool globalSetting = ruby.cRegexp.IsIgnoreCase;
            if (globalSetting && (options & RegexOptions.IgnoreCase) == 0)
            {
                options |= RegexOptions.IgnoreCase;
                needRecompile = true;
            }
            if (!globalSetting && (options & RegexOptions.IgnoreCase) != 0)
            {
                options &= ~RegexOptions.IgnoreCase;
                needRecompile = true;
            }
            if (needRecompile)
            {
                regex = new Regex(regex.ToString(), options);
            }
        }
    }

    public class RMatchData : RData
    {
        internal RMatchData(NetRuby rb, string s, Match m) :
            base(rb, rb.cMatch)
        {
            SetData(s, m);
        }
        internal void SetData(string s, Match m)
        {
            match = m;
            target = s;
        }
        public Match Match
        {
            get { return match; }
        }
        public override RString ToRString()
        {
            string s = this[0];
            RString r = new RString(ruby, (s == null) ? String.Empty : s);
            if (IsTainted)
                r.Taint();
            return r;
        }
        public override RArray ToArray()
        {
            bool b = IsTainted;
            RArray a = new RArray(ruby, true);
            GroupCollection g = match.Groups;
            for (int i = 0; i < g.Count; i++)
            {
                string s = g[i].Value;
                if (b)
                    a.Add(new RString(ruby, s, b));
                else
                    a.Add(s);
            }
            if (b)
                a.Taint();
            return a;
        }
        public string this[int index]
        {
            get {
                if (match == null) return null;
                if (index == 0)
                    return match.ToString();
                GroupCollection col = match.Groups;
                if (index >= col.Count) return null;
                if (index < 0)
                {
                    index += col.Count;
                    if (index <= 0) return null;
                }
                return col[index].Value;
            }
        }
        public string Pre
        {
            get { return (match == null) ? null : target.Substring(0, match.Index); }
        }
        public string Post
        {
            get { return (match == null) ? null : target.Substring(match.Index + match.Length); }
        }
        public string Last
        {
            get {
                if (match == null) return null;
                GroupCollection col = match.Groups;
                if (col.Count == 0) return null;
                return col[col.Count - 1].Value;
            }
        }
        public int Count
        {
            get { return (match == null) ? 0 : match.Groups.Count; }
        }
        public string Sub(string str, string src)
        {
            StringBuilder val = new StringBuilder();
            int p = 0;
            int no;
            int i;
            for (i = 0; i < str.Length;)
            {
                int ix = i;
                char c = str[i++];
                if (c != '\\') continue;
                val.Append(str, p, ix - p);
                c = str[i++];
                p = i;
                switch (c)
                {
                case '0': case '1': case '2': case '3': case '4':
                case '5': case '6': case '7': case '8': case '9':
                    no = c - '0';
                    break;
                case '&':
                    no = 0;
                    break;
                case '`':
                    val.Append(src, 0, match.Index);
                    continue;
                case '\'':
                    val.Append(src, match.Length, src.Length - match.Length);
                    continue;
                case '+':
                    no = match.Groups.Count - 1;
                    while (match.Groups[no].Index == -1 && no > 0) no--;
                    if (no == 0) continue;
                    break;
                case '\\':
                    val.Append('\\');
                    continue;
                default:
                    val.Append(str, i - 2, 2);
                    continue;
                }
                if (no >= 0)
                {
                    if (no >= match.Groups.Count) continue;
                    if (match.Groups[no].Index == -1) continue;
                    val.Append(src, match.Groups[no].Index, match.Groups[no].Length);
                }
            }
            if (p < str.Length)
            {
                val.Append(str, p, str.Length - p);
            }
            return val.ToString();
        }
        
        void CheckOffset(int i)
        {
            if (i < 0 || i >= match.Groups.Count)
                throw new eIndexError(String.Format("index {0} out of matches", i));
        }
        public RArray Offset(int i)
        {
            CheckOffset(i);
            ArrayList ar = new ArrayList();
            Group g = match.Groups[i];
            if (g.Index < 0)
            {
                ar.Add(null);
                ar.Add(null);
            }
            else
            {
                ar.Add(g.Index);
                ar.Add(g.Index + g.Length);
            }
            return new RArray(ruby, ar);
        }
        public int Begin(int i)
        {
            CheckOffset(i);
            Group g = match.Groups[i];
            return g.Index;
        }
        public int End(int i)
        {
            CheckOffset(i);
            Group g = match.Groups[i];
            return g.Index + g.Length;
        }
        Match match;
        string target;
    }
    
    public class RRegexpClass : RClass
    {
        private RRegexpClass(NetRuby rb) :
            base(rb, "Regexp", rb.cObject)
        {
            ignoreCase = false;
            recompileNeed = false;
        }
        public bool IsIgnoreCase
        {
            get { return ignoreCase; }
            set {
                if (value != ignoreCase)
                {
                    ignoreCase = value;
                    recompileNeed = true;
                }
            }
        }
        internal bool IsRecompileNeed
        {
            get { return recompileNeed; }
        }
        
        bool ignoreCase;
        bool recompileNeed;
        
        internal const FL MatchBusy = FL.USER2;
        internal const FL CaseState = FL.USER0;

        internal static void matchBusy(object match)
        {
            if (match is RBasic)
            {
                ((RBasic)match).Set(MatchBusy);
            }
        }

        static private object matchGetter(uint id, GlobalEntry gb, NetRuby rb)
        {
            RThread th = rb.GetCurrentContext();
            object o = th.BackRef;
            matchBusy(o);
            return o;
        }
        static private void matchSetter(object val, uint id, GlobalEntry gb, NetRuby rb)
        {
            rb.CheckType(val, typeof(RMatchData));
            RThread th = rb.GetCurrentContext();
            th.BackRef = val;
        }
        static private object lastMatchGetter(uint id, GlobalEntry gb, NetRuby rb)
        {
            RThread th = rb.GetCurrentContext();
            RMatchData m = (RMatchData)th.BackRef;
            if (m == null) return null;
            string s = m[0];
            if (m.IsTainted)
                return new RString(rb, s, true);
            return s;
        }
        static private object preMatchGetter(uint id, GlobalEntry gb, NetRuby rb)
        {
            RThread th = rb.GetCurrentContext();
            RMatchData m = (RMatchData)th.BackRef;
            if (m == null) return null;
            string s = m.Pre;
            if (m.IsTainted)
                return new RString(rb, s, true);
            return s;
        }
        static private object postMatchGetter(uint id, GlobalEntry gb, NetRuby rb)
        {
            RThread th = rb.GetCurrentContext();
            RMatchData m = (RMatchData)th.BackRef;
            if (m == null) return null;
            string s = m.Post;
            if (m.IsTainted)
                return new RString(rb, s, true);
            return s;
        }
        static private object lastParenGetter(uint id, GlobalEntry gb, NetRuby rb)
        {
            RThread th = rb.GetCurrentContext();
            RMatchData m = (RMatchData)th.BackRef;
            if (m == null) return null;
            string s = m.Last;
            if (m.IsTainted)
                return new RString(rb, s, true);
            return s;
        }
        static private object iCaseGetter(uint id, GlobalEntry gb, NetRuby rb)
        {
            return rb.cRegexp.ignoreCase;
        }
        static private void  iCaseSetter(object val, uint id, GlobalEntry gb, NetRuby rb)
        {
            rb.cRegexp.IsIgnoreCase = RBasic.RTest(val);
        }
        static private object kCodeGetter(uint id, GlobalEntry gb, NetRuby rb)
        {
            return "none";
        }
        static private void kCodeSetter(object val, uint id, GlobalEntry gb, NetRuby rb)
        {
        }

        static internal object s_new(RBasic r, params object[] args)
        {
            RRegexp re = new RRegexp(r.ruby, (RMetaObject)r);
            r.ruby.CallInit(re, args);
            return re;
        }
        static internal object s_quote(RBasic r, params object[] args)
        {
            if (args.Length < 1 || args.Length > 2)
                throw new eArgError("wront # of argument");
            string p = RString.ToString(r.ruby, args[0]);
            return Regex.Escape(p);
        }
        static internal object initialize(RBasic r, params object[] args)
        {
            if (args.Length < 1 || args.Length > 3)
                throw new eArgError("wrong # of argument");
            RegexOptions flag = RegexOptions.None;
            if (args.Length >= 2)
            {
                if (args[1] is int || args[1] is RegexOptions) flag = (RegexOptions)(int)args[1];
                else if (args[1] is RFixnum) flag = (RegexOptions)((RFixnum)args[1]).ToInt();
                else if (RBasic.RTest(args[1])) flag = RegexOptions.IgnoreCase;
            }
            if (args.Length == 3)
            {
                // ignore char set
            }
            NetRuby ruby = r.ruby;
            RRegexp self = (RRegexp)r;
            if (args[0] is RRegexp)
            {
                RRegexp re = (RRegexp)args[0];
                re.Check();
                self.Initialize(re, flag);
            }
            else
            {
                string p = RString.ToString(ruby, args[0]);
                self.Initialize(p, flag);
            }
            return self;
        }
        static internal object match(RBasic r, params object[] args)
        {
            if (args[0] == null) return null;
            NetRuby rb = r.ruby;
            string s = RString.ToString(rb, args[0]);
            bool infect = false;
            if (args[0] is RBasic && ((RBasic)args[0]).IsTainted)
                infect = true;
            int start = ((RRegexp)r).Search(s, 0, false, infect);
            if (start < 0)
                return null;
            return start;
        }
        static internal object match2(RBasic r, params object[] args)
        {
            throw new NotSupportedException("Deprecated Method");
        }
        static private object s_lastmatch(RBasic r, params object[] args)
        {
            RThread th = r.ruby.GetCurrentContext();
            RMatchData m = (RMatchData)th.BackRef;
            if (m == null) return null;
            string s = m[0];
            if (m.IsTainted)
                return new RString(r.ruby, s, true);
            return s;
        }
        static internal object source(RBasic r, params object[] args)
        {
            string s = ((RRegexp)r).Source;
            if (r.IsTainted)
                return new RString(r.ruby, s, true);
            return s;
        }
        static internal object kcode_m(RBasic r, params object[] args)
        {
            return "none";
        }
        static internal object casefold_p(RBasic r, params object[] args)
        {
            return ((RRegexp)r).IsCaseFold;
        }
        static internal object match_m(RBasic r, params object[] args)
        {
            object o = match(r, args);
            if (o == null) return null;
            RThread th = r.ruby.GetCurrentContext();
            o = th.BackRef;
            matchBusy(o);
            return o;
        }
        static internal object match_size(RBasic r, params object[] args)
        {
            return ((RMatchData)r).Count;
        }
        static internal object match_offset(RBasic r, params object[] args)
        {
            NetRuby ruby = r.ruby;
            int i = RInteger.ToInt(r.ruby, args[0]);
            return ((RMatchData)r).Offset(i);
        }
        static internal object match_begin(RBasic r, params object[] args)
        {
            NetRuby ruby = r.ruby;
            int i = RInteger.ToInt(r.ruby, args[0]);
            return ((RMatchData)r).Begin(i);
        }
        static internal object match_end(RBasic r, params object[] args)
        {
            NetRuby ruby = r.ruby;
            int i = RInteger.ToInt(r.ruby, args[0]);
            return ((RMatchData)r).End(i);
        }
        static internal object match_to_s(RBasic r, params object[] args)
        {
            return ((RMatchData)r).ToRString();
        }
        static internal object match_string(RBasic r, params object[] args)
        {
            RString rs = ((RMatchData)r).ToRString();
            rs.Freeze();
            return rs;
        }
        static internal object match_aref(RBasic r, params object[] args)
        {
            object[] argv = new object[2];
            NetRuby rb = r.ruby;
            rb.ScanArgs(args, "11", argv);
            if (argv[1] != null)
                return ((RMatchData)r).ToArray().ARef(args);
            return ((RMatchData)r)[RInteger.ToInt(rb, args[0])];
        }
        static internal object match_to_a(RBasic r, params object[] args)
        {
            return ((RMatchData)r).ToArray();
        }
        static internal object match_pre(RBasic r, params object[] args)
        {
            return ((RMatchData)r).Pre;
        }
        static internal object match_post(RBasic r, params object[] args)
        {
            return ((RMatchData)r).Post;
        }
        static internal void Init(NetRuby rb)
        {
            rb.eRegexpError = rb.DefineClass("RegexpError", rb.eStandardError);

            rb.DefineVirtualVariable("$~",
                                  new GlobalEntry.Getter(matchGetter),
                                  new GlobalEntry.Setter(matchSetter));
            rb.DefineVirtualVariable("$&",
                                     new GlobalEntry.Getter(lastMatchGetter), null);
            rb.DefineVirtualVariable("$`",
                                     new GlobalEntry.Getter(preMatchGetter), null);
            rb.DefineVirtualVariable("$'",
                                     new GlobalEntry.Getter(postMatchGetter), null);
            rb.DefineVirtualVariable("$+",
                                     new GlobalEntry.Getter(lastParenGetter), null);
            rb.DefineVirtualVariable("$=",
                                  new GlobalEntry.Getter(iCaseGetter),
                                  new GlobalEntry.Setter(iCaseSetter));
            rb.DefineVirtualVariable("$KCODE",
                                  new GlobalEntry.Getter(kCodeGetter),
                                  new GlobalEntry.Setter(kCodeSetter));
            rb.DefineVirtualVariable("$-K",
                                  new GlobalEntry.Getter(kCodeGetter),
                                  new GlobalEntry.Setter(kCodeSetter));

            RRegexpClass reg = new RRegexpClass(rb);
            reg.DefineClass("Regexp", rb.cObject);
            rb.cRegexp = reg;

            reg.DefineSingletonMethod("new", new RMethod(s_new), -1);
            reg.DefineSingletonMethod("compile", new RMethod(s_new), -1);
            reg.DefineSingletonMethod("quote", new RMethod(s_quote), -1);
            reg.DefineSingletonMethod("escape", new RMethod(s_quote), -1);
            reg.DefineSingletonMethod("last_match", new RMethod(s_lastmatch), 0);
        
            reg.DefineMethod("initialize", new RMethod(initialize), -1);
            reg.DefineMethod("=~", new RMethod(match), 1);
            reg.DefineMethod("===", new RMethod(match), 1);
            reg.DefineMethod("~", new RMethod(match2), 0);
            reg.DefineMethod("match", new RMethod(match_m), 1);
            reg.DefineMethod("source", new RMethod(source), 0);
            reg.DefineMethod("casefold?", new RMethod(casefold_p), 0);
            reg.DefineMethod("kcode", new RMethod(kcode_m), 0);

            reg.DefineConst("IGNORECASE", RegexOptions.IgnoreCase);
            reg.DefineConst("EXTENDED", RegexOptions.IgnorePatternWhitespace);
            reg.DefineConst("MULTILINE", RegexOptions.Multiline);

            RClass md = rb.DefineClass("MatchData", rb.cObject);
            rb.DefineGlobalConst("MatchingData", md);
            rb.cMatch = md;
            rb.ClassOf(md).UndefMethod("new");
            md.DefineMethod("size", new RMethod(match_size), 0);
            md.DefineMethod("length", new RMethod(match_size), 0);
            md.DefineMethod("offset", new RMethod(match_offset), 1);
            md.DefineMethod("begin", new RMethod(match_begin), 1);
            md.DefineMethod("end", new RMethod(match_end), 1);
            md.DefineMethod("to_a", new RMethod(match_to_a), 0);
            md.DefineMethod("[]", new RMethod(match_aref), -1);
            md.DefineMethod("pre_match", new RMethod(match_pre), 0);
            md.DefineMethod("post_match", new RMethod(match_post), 0);
            md.DefineMethod("to_s", new RMethod(match_to_s), 0);
            md.DefineMethod("string", new RMethod(match_string), 0);
        }
    }
}
