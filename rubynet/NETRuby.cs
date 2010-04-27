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
using System.Threading;
using System.Security;

[assembly: AssemblyTitle("NETRuby")]
[assembly: AssemblyVersion("0.8.*")]

namespace NETRuby
{
    public class st_table : Hashtable, ICloneable
    {
        public st_table()
        {
        }
        public st_table(Hashtable h) : base(h)
        {
        }
        public bool lookup(string name, out uint id)
        {
            object o = this[name];
            if (o != null)
            {
                id = (uint)o;
                return true;
            }
            id = 0;
            return false;
        }
        public bool lookup(string name, out object o)
        {
            o = this[name];
            return (o != null) ? true : false;
        }

        public bool lookup(uint id, out string name)
        {
            object o = this[id];
            if (o != null)
            {
                name = (string)o;
                return true;
            }
            name = null;
            return false;
        }
        public bool lookup(uint id, out object o)
        {
            o = this[id];
            return (o != null) ? true : false;
        }
        public bool lookup(object key, out st_table o)
        {
            o = (st_table)this[key];
            return (o != null) ? true : false;
        }
        public bool delete(uint id, out object o)
        {
            lock (SyncRoot)
            {
                bool ret = lookup(id, out o);
                if (ret)
                {
                    Remove(id);
                }
                return ret;
            }
        }
        public override object Clone()
        {
            lock(SyncRoot)
            {
                return new st_table((Hashtable)base.Clone());
            }
        }
    }

    internal class trace_var
    {
        public trace_var()
        {
            removed = false;
            data = null;
            next = null;
        }
        internal bool removed;
        internal delegate void func();
        internal object data;
        internal trace_var next;
    }

    // 
    // hold all global variables in a NetRuby instance
    public class GlobalEntry
    {
        public GlobalEntry(uint i)
        {
            id = i;
            data = null;
            block_trace = false;
            trace = null;
            getMethod = new Getter(undef_getter);
            setMethod = new Setter(undef_setter);
        }
        static private object undef_getter(uint i, GlobalEntry gb, NetRuby rb)
        {
            rb.warning("global variable `" + rb.id2name(i) + "' not initialized");
            return null;
        }
        static private void undef_setter(object val, uint i, GlobalEntry gb, NetRuby rb)
        {
            gb.getMethod = new Getter(val_getter);
            gb.setMethod = new Setter(val_setter);
            gb.data = val;
        }
        static private object val_getter(uint i, GlobalEntry gb, NetRuby rb)
        {
            return gb.data;
        }
        static private void val_setter(object val, uint i, GlobalEntry gb, NetRuby rb)
        {
            gb.data = val;
        }
        static public void ReadonlySetter(object val, uint i, GlobalEntry gb, NetRuby rb)
        {
            throw new eNameError("can't set variable " + rb.id2name(i));
        }
        
        internal uint id;
        internal object data;
        public delegate object Getter(uint id, GlobalEntry gb, NetRuby rb);
        public delegate void Setter(object val, uint id, GlobalEntry gb, NetRuby rb);
        internal void ReplaceGetter(Getter gt)
        {
            if (gt == null)
                getMethod = new Getter(val_getter);
            else
                getMethod = gt;
        }
        internal void ReplaceSetter(Setter st)
        {
            if (st == null)
                setMethod = new Setter(val_setter);
            else
                setMethod = st;
        }
        public object GetValue(NetRuby rb)
        {
            return getMethod(id, this, rb);
        }
        public void SetValue(object val, NetRuby rb)
        {
            setMethod(val, id, this, rb);
        }
        private event Getter getMethod;
        private event Setter setMethod;
        internal bool block_trace;
        internal trace_var trace;
    }

    internal class LocalVars : ICloneable
    {
        internal LocalVars(LocalVars p, int ct, int dl)
        {
            if (ct > 0)
                tbl = new uint[ct + 2];
            else
                tbl = null;
            prev = p;
            dlev = dl;
        }
        internal uint[] tbl;
        internal int dlev;
        internal LocalVars prev;
        internal int Length
        {
            get { return (tbl != null) ? tbl.Length : 0; }
        }
        public object Clone()
        {
            LocalVars o;
            lock (this)
            {
                o = (LocalVars)MemberwiseClone();
                o.tbl = (uint[])o.tbl.Clone();
            }
            if (o.prev != null)
            {
                o.prev = (LocalVars)o.prev.Clone();
            }
            return o;
        }
        internal int Count(uint id)
        {
            if (id == 0) return Length;
            if (tbl != null)
            {
                int cnt = Array.IndexOf(tbl, id);
                if (cnt >= 0) return cnt;
            }
            return Append(id);
        }
        internal int Append(uint id)
        {
            int idx = 2;
            lock (this)
            {
                if (tbl == null) {
                    tbl = new uint[3];
                    tbl[0] = (uint)'_';
                    tbl[1] = (uint)'~';
                    if (id == '_') return 0;
                    if (id == '~') return 1;
                }
                else
                {
                    idx = Length;
                    uint[] t = new uint[idx + 1];
                    Array.Copy(tbl, t, idx);
                    tbl = t;
                }
                tbl[idx] = id;
            }
            return idx;
        }
        internal void Init(int cnt, uint[] local, bool dlv)
        {
            lock (this)
            {
                if (cnt > 0) {
                    tbl = new uint[cnt + 2];
                    Array.Copy(local, tbl, cnt);
                }
                else {
                    tbl = null;
                }
            }
            dlev = (dlv) ? 1 : 0;
        }
    }
    
    public class RVarmap : RBasic
    {
        public RVarmap(NetRuby rb, uint i, object v, RVarmap prev) :
            base(rb, null)
        {
            id = i;
            val = v;
            next = prev;
        }
        internal new uint id;
        internal object val;
        internal RVarmap next;
    }
/*
    struct CacheEntry
    {
        internal uint mid;        // method id
        internal uint mid0;
        internal RMetaObject klass;
        internal RMetaObject origin;
        internal RNode method;
        internal NOEX noex;
        internal const int SIZE = 0x800;
        internal const int MASK = 0x7ff;
        static internal int EXPR1(object o, uint mid)
        {
            return ((int)(((uint)o.GetHashCode()) ^ mid) & MASK);
        }
        internal void SetEmtptyInfo(RMetaObject o, uint id)
        {
            klass = origin = o;
            mid = mid0 = id;
            noex = NOEX.PUBLIC;
            method = null;
        }
        internal void SetInfo(RMetaObject o, uint id, RNode body, RMetaObject org)
        {
            klass = o;
            noex = body.noex;
            body = body.body;
            if (body is RNFBody)
            {
                mid = id;
                origin = body.orig;
                mid0 = body.mid;
                method = body.head;
            }
            else
            {
                origin = org;
                mid = mid0 = id;
                method = body;
            }
        }
    }
*/    
    internal enum CALLSTAT
    {
        PUBLIC = 0,
        PRIVATE = 1,
        PROTECTED = 2,
        VCALL = 4,
    }

    public class RBuiltinType
    {
        public readonly Type Type;
        public readonly string Name;
        static public RBuiltinType[] Table
        {
            get { return tbl; }
        }
        private RBuiltinType(Type t, string n)
        {
            Type = t;
            Name = n;
        }
        static RBuiltinType[] tbl = new RBuiltinType[] {
            new RBuiltinType(typeof(RNil), "nil"),
            new RBuiltinType(typeof(RObject), "Object"),
            new RBuiltinType(typeof(RClass), "Class"),
            new RBuiltinType(typeof(RIncClass), "iClass"),
            new RBuiltinType(typeof(RModule), "Module"),
            new RBuiltinType(typeof(RFloat), "Float"),
            new RBuiltinType(typeof(RString), "String"),
            new RBuiltinType(typeof(RRegexp), "Regexp"),
            new RBuiltinType(typeof(RArray), "Array"),
            new RBuiltinType(typeof(RFixnum), "Fixnum"),
            new RBuiltinType(typeof(RBignum), "Bignum"),
            new RBuiltinType(typeof(RTrue), "true"),
            new RBuiltinType(typeof(RFalse), "false"),
            new RBuiltinType(typeof(Symbol), "Symbol"),
            new RBuiltinType(typeof(RData), "Data"),
            new RBuiltinType(typeof(RVarmap), "Varmap"),
            new RBuiltinType(typeof(Scope), "Scope"),
            new RBuiltinType(typeof(RNode), "Node"),
            new RBuiltinType(typeof(QUndef), "undef"),
        };
    }
    
    public class NetRuby : IDisposable
    {
        public NetRuby()
        {
            disposed = false;
            sym_tbl = new st_table();
            sym_rev_tbl = new st_table();
            class_tbl = new st_table();
            global_tbl = new st_table();
            //generic_iv_tbl = null;
            last_id = Token.LAST_TOKEN;
            ////cache = new CacheEntry[CacheEntry.SIZE];
            threads = new Hashtable();
            threadMain = new RThread(this);
            InitEnvironment(threadMain);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;
        ~NetRuby()
        {
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
        /*
            if (!disposed)
            {
                RThread th = GetCurrentContext();
                th.PushTag(Tag.TAG.PROT_NONE);
                if (endProcs != null)
                {
                    while (endProcs.Count > 0)
                    {
                        try
                        {
                            EndProcData ed = (EndProcData)endProcs.Pop();
                            ed.Exec();
                        }
                        catch (Exception ex)
                        {
                            th.errorHandle(ex);
                        }
                    }
                }
                if (ephemeralEndProcs != null)
                {
                    while (ephemeralEndProcs.Count > 0)
                    {
                        try
                        {
                            EndProcData ed = (EndProcData)ephemeralEndProcs.Pop();
                            ed.Exec();
                        }
                        catch (Exception ex)
                        {
                            th.errorHandle(ex);
                        }
                    }
                }
                th.PopTag(false);
                disposed = true;
            }
        */            
        }
/*        
        internal object[] CallArgs(RNode nd, object self)
        {
            RThread th = GetCurrentContext();
            Block tempBlock = th.block;
            if ((ITER)th.iter.Peek() == ITER.PRE)
            {
                th.block = th.block.prev;
            }
            th.iter.Push(ITER.NOT);
            object[] ret = nd.SetupArgs(this, self, nd.args);
            th.block = tempBlock;
            th.iter.Pop();
            return ret;
        }
        internal object[] CallArgs(RNCall nd, object self, out object receiver)
        {
            RThread th = GetCurrentContext();
            Block tempBlock = th.block;
            if ((ITER)th.iter.Peek() == ITER.PRE)
            {
                th.block = th.block.prev;
            }
            th.iter.Push(ITER.NOT);
            object[] ret = nd.SetupArgs(this, self, out receiver);
            th.block = tempBlock;
            th.iter.Pop();
            return ret;
        }
        internal object CallIter(RNode nd, object self)
        {
            RThread th = GetCurrentContext();
            Block tempBlock = th.block;
            if ((ITER)th.iter.Peek() == ITER.PRE)
            {
                th.block = th.block.prev;
            }
            th.iter.Push(ITER.NOT);
            object ret = Eval(self, nd.iter);
            th.block = tempBlock;
            th.iter.Pop();
            return ret;
        }
*/        
        //
        private void InitEnvironment(RThread th)
        {
            ////Scope.ScopeMode oldvmode = th.PushScope();
            ////th.PushTag(Tag.TAG.PROT_NONE);
            /*
            try
            {
            */
                missing = intern("method_missing");
                idEq = intern("==");
                idEql = intern("eql?");

                cObject = new RClass(this, "Object", null);
                class_tbl[intern("Object")] = cObject;
                cModule = new RModule(this, "Module", cObject);
                cClass = new RClass(this, "Class", cModule);
                class_tbl[intern("Class")] = cClass;
                cObject.klass = new RSingletonClass(cClass);
                cObject.klass.AttachSingleton(cObject);
                cModule.klass = new RSingletonClass(cObject.klass);
                cModule.klass.AttachSingleton(cModule);
                cClass.klass = new RSingletonClass(cModule.klass);
                cClass.klass.AttachSingleton(cClass);
                mKernel = new RKernel(this, "Kernel");
                RMetaObject.IncludeModule(cObject, mKernel);
                mKernel.Init(this);
                REnumerable.Init(this);
                evalInit();
                RString.Init(this);
                RException.Init(this);
                RThreadClass.Init(this);
                RThreadGroupClass.Init(this);
                RNumeric.Init(this);
                RInteger.Init(this);
                RFixnum.Init(this);
                RFloat.Init(this);
                RBignum.Init(this);
                RArray.Init(this);
                RHashClass.Init(this);
                RRegexpClass.Init(this);
                RIOClass.Init(this);
                RProcClass.Init(this);
                RTimeClass.Init(this);
                loader = Loader.Init(this);
                ////RDotNetClass.Init(this);

                versionInit();
        
                threadMain.Init(this);

                th.rClass = cObject;
                /*
                topCRef = new RNCRef(cObject);
                th.cRef = topCRef;
                th.frame.self = topSelf;
                th.frame.cBase = topCRef;
                */

                progInit();
            /*
            }
            catch (eTagJump)
            {
                       errorPrint(th);
            }
            */
            /*
            catch (Exception e)
            {
                errorPrint(th, e);
            }
            */
            ////th.PopTag(true);
            ////th.PopScope(oldvmode);
        }

        public virtual void Init()
        {
        }
        
        private void evalInit()
        {
            BindingFlags bf = BindingFlags.InvokeMethod
                | BindingFlags.Static | BindingFlags.Public
                | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy
                | BindingFlags.Instance;
            missing = intern("method_missing");
            initialize = intern("initialize");

            DefineVirtualVariable("$@",
                                  new GlobalEntry.Getter(errAtGetter),
                                  new GlobalEntry.Setter(errAtSetter));
            DefineVariable("$!", null,
                           new GlobalEntry.Getter(errInfoGetter),
                           new GlobalEntry.Setter(errInfoSetter));

            /*
            DefineGlobalFunction("eval", new RBasic.RMethod(ruby_eval), -1);
            DefineGlobalFunction("iterator?", new RBasic.RMethod(ruby_block_given_p), 0);
            DefineGlobalFunction("block_given?", new RBasic.RMethod(ruby_block_given_p), 0);
            DefineGlobalFunction("method_missing", new RBasic.RMethod(RBasic.ruby_missing), -1);
            DefineGlobalFunction("loop", new RBasic.RMethod(ruby_loop), 0);
            */
            
            DefineGlobalFunction("raise", new RBasic.RMethod(ruby_raise), -1);
            DefineGlobalFunction("fail", new RBasic.RMethod(ruby_raise), -1);

            ////DefineGlobalFunction("caller", new RBasic.RMethod(ruby_caller), -1);
        
            DefineGlobalFunction("exit", new RBasic.RMethod(ruby_exit), -1);
            DefineGlobalFunction("abort", new RBasic.RMethod(ruby_abort), -1);
            ////DefineGlobalFunction("at_exit", new RBasic.RMethod(ruby_at_exit), 0);

            //catch, throw
            //globals, locals

            mKernel.evalInit();

            Type obj = cModule.GetType();
            cModule.DefinePrivateMethod("append_features", obj.GetMethod("AppendFeatures", bf));
            cModule.DefinePrivateMethod("extend_object", new RBasic.RMethod(RMetaObject.extend_object), 1);
            cModule.DefinePrivateMethod("include", obj.GetMethod("Include", bf));
            cModule.DefinePrivateMethod("public", obj.GetMethod("Public", bf));
            cModule.DefinePrivateMethod("protected", obj.GetMethod("Protected", bf));
            cModule.DefinePrivateMethod("private", obj.GetMethod("Private", bf));
            cModule.DefinePrivateMethod("module_function", obj.GetMethod("ModuleFunction", bf));
            cClass.UndefMethod("module_function");

            cModule.DefinePrivateMethod("remove_method", new RBasic.RMethod(RMetaObject.remove_method), 1);
            cModule.DefinePrivateMethod("undef_method", new RBasic.RMethod(RMetaObject.undef_method), 1);
            cModule.DefinePrivateMethod("alias_method", new RBasic.RMethod(RMetaObject.alias_method), 2);
            cModule.DefinePrivateMethod("define_method", new RBasic.RMethod(RMetaObject.define_method), -1);
        
            obj = topSelf.GetType();
            topSelf.DefineSingletonMethod("include", obj.GetMethod("Include", bf));
            topSelf.DefineSingletonMethod("public", obj.GetMethod("Public", bf));
            topSelf.DefineSingletonMethod("private", obj.GetMethod("Private", bf));

            mKernel.DefineMethod("extend", new RBasic.RMethod(mKernel.extend), -1);
        
            ////DefineVariable("$SAFE", null, new GlobalEntry.Getter(safeGetter), new GlobalEntry.Setter(safeSetter));
        }

        private void versionInit()
        {
            Assembly asm = Assembly.GetAssembly(GetType());
            FileVersionInfo finfo = FileVersionInfo.GetVersionInfo(
                                           asm.Location);
            RString s = new RString(this, finfo.FileVersion);
            s.Freeze();
            DefineGlobalConst("RUBY_VERSION", s);
            s = new RString(this, "CommonLanguageRuntime");
            s.Freeze();
            DefineGlobalConst("RUBY_PLATFORM", s);
        }
        
        private void progInit()
        {
            DefineVirtualVariable("$VERBOSE", new GlobalEntry.Getter(verboseGetter), new GlobalEntry.Setter(verboseSetter));
            DefineVariable("$-v", null, new GlobalEntry.Getter(verboseGetter), new GlobalEntry.Setter(verboseSetter));
            DefineVariable("$-w", null, new GlobalEntry.Getter(verboseGetter), new GlobalEntry.Setter(verboseSetter));
            DefineVariable("$DEBUG", null, new GlobalEntry.Getter(debugGetter), new GlobalEntry.Setter(debugSetter));
            DefineVariable("$-d", null, new GlobalEntry.Getter(debugGetter), new GlobalEntry.Setter(debugSetter));
            DefineReadonlyVariable("$-p", null, new GlobalEntry.Getter(printGetter));
            DefineReadonlyVariable("$-l", null, new GlobalEntry.Getter(lineGetter));

            progName = new RString(this, String.Empty, true);
#if RUBY_ARG0
            DefineVariable("$0", progName, null, new GlobalEntry.Setter(SetArg0));
#else
            DefineVariable("$0", progName);
#endif
            argv = new RArray(this, true);
            DefineReadonlyVariable("$*", argv, new GlobalEntry.Getter(argvGetter));
            DefineGlobalConst("ARGV", argv);
            DefineReadonlyVariable("$-a", argv, new GlobalEntry.Getter(splitGetter));

            DefineVirtualVariable("$_", new GlobalEntry.Getter(lastLineGetter),new GlobalEntry.Setter(lastLineSetter));
        }

        /*
        // returns false if prog termination is required.
        public bool Options(string[] args)
        {

            bool f = false;
            bool created;
            RThread th = GetCurrentContext(out created);
        
            th.PushTag(Tag.TAG.PROT_NONE);
            try
            {
                f = ProcessOptions(th, args);
            }
            catch (eTagJump)
            {
                       errorPrint(th);
            }
            catch (Exception e)
            {
                errorPrint(th, e);
            }
            th.PopTag(false);
            // Keep ThreadContext because EvalTree was not yet evaluated.
            return f;
        }
        
        internal bool ProcessOptions(RThread th, string[] args)
        {
            bool result = SetOptions(args, false);
            if (doCheck && th.CompileFailed == false)
            {
                System.Console.Out.WriteLine("Syntax OK");
                result = false;
            }
            if (doLoop)
            {
            }
            return result;
        }
        
        internal bool SetOptions(string[] args, bool reent)
        {
            if (args.Length <= 0) return true;
            bool vbose = false;
            bool dosearch = false;
            bool version = false;
            bool copyright = false;
            string e_script = null;
            int idx = 0;
            for (; idx < args.Length; idx++)
            {
                if (args[idx][0] != '-') break;
                for (string s = args[idx].Substring(1); s.Length > 0; s = s.Substring(1))
                {
                    switch (s[0])
                    {
                    case 'a':
                        doSplit = true;
                        break;
                    case 'p':
                        doPrint = true;
                        goto case 'n';
                    case 'n':
                        doLoop = true;
                        break;
                    case 'd':
                        debug = true;
                        verbose = true;
                        break;
                    case 'y':
                        yydebug = true;
                        break;
                    case 'v':
                        if (vbose)
                            break;
                        ShowVersion();
                        vbose = true;
                        goto case 'w';
                    case 'w':
                        verbose = true;
                        break;
                    case 'c':
                        doCheck = true;
                        break;
                    case 's':
                        warn("NETRuby doesn't support -s");
                        // not support sflag
                        break;
                    case 'h':
                        ShowUsage();
                        return false;
                    case 'l':
                        warn("NETRuby doesn't support -l");
                        // doLine = true;
                        break;
                    case 'S':
                        warn("NETRuby doesn't support -S");
                        break;
                    case 'e':
                        if (s.Length == 1)
                        {
                            idx++;
                            if (idx >= args.Length)
                            {
                                s = "";
                            }
                            else
                            {
                                s = args[idx];
                            }
                        }
                        else
                        {
                            s = s.Substring(1);
                        }
                        if (s.Length == 0)
                        {
                            System.Console.Error.WriteLine(args[idx - 1] + ": no code specified for -e");
                            return false;
                        }
                        if (e_script == null)
                        {
                            e_script = s;
                            script = "-e";
                        }
                        else
                        {
                            e_script += s + "\n";
                        }
                        goto next_argv;
                    case 'r':
                        warn("NETRuby doesn't support -r");
                        goto next_argv;
                    case 'i':        // inplace mode
                        warn("NETRRuby doesn't support -i");
                        goto next_argv;
                    case 'C':
                    case 'X':
                        warn("NETRuby doesn't support -C -X");
                        break;
                    case 'F':
                        if (s.Length > 1)
                            fSep = s.Substring(1);
                        goto next_argv;
                    case 'K':
                        if (s.Length > 1)
                        {
                            SetKCode(s[1]);
                            s = s.Substring(1);
                        }
                        break;
                    case 'T':
                    {
                        int v = 1;
                        if (s.Length > 1)
                        {
                            v = Convert.ToInt32(s.Substring(1));
                        }
                        SafeLevel = v;
                        goto next_argv;
                    }
                    case 'I':
                        if (s.Length > 1)
                        {
                            loader.IncPush(s.Substring(1));
                        }
                        goto next_argv;
                    case '0':
                        break;
                    case '-':
                        if (s.Length <= 1)
                        {
                            idx++;
                            goto switch_end;
                        }
                        s = s.Substring(1);
                        if (s == "copyright")
                        {
                            copyright = true;
                        }
                        else if (s == "debug")
                        {
                            debug = true;
                            verbose = true;
                        }
                        else if (s == "version")
                        {
                            version = true;
                        }
                        else if (s == "verbose")
                        {
                            vbose = true;
                            verbose = true;
                        }
                        else if (s == "yydebug")
                        {
                            yydebug = true;
                        }
                        else if (s == "help")
                        {
                            ShowUsage();
                            return false;
                        }
                        else
                        {
                            System.Console.Error.WriteLine(args[idx] + ": invalid option -" + s[0] + "  (-h will show valid options)\n");
                            return false;
                        }
                        goto next_argv;
                    }
                }
            next_argv:
                ;
            }
        switch_end:
            if (reent) return true;
            if (version)
            {
                ShowVersion();
                return false;
            }
            if (copyright)
            {
                ShowCopyright();
            }

            if (e_script == null && idx >= args.Length)
            {
                if (vbose) return false;
                script = "-";
            }
            else
            {
                if (e_script == null)
                {
                    script = args[idx];
                }
                if (script.Length <= 0)
                {
                    script = "-";
                }
                if (e_script == null)
                {
                    idx++;
                }
            }
            RubyScript(script);
            SetArgv(args, idx);
            //processSFlag();
            //initLoadPath();
            RThread th = GetCurrentContext();
            th.file = AppDomain.CurrentDomain.FriendlyName;
            th.evalTree = null;
            if (e_script != null)
            {
                th.evalTree = CompileFile("-e", new StringReader(e_script), 1, th);
            }
            else
            {
                LoadFile(script, true);
            }
            //processSFlag();
            return true;
        }
        */
        private void ShowVersion()
        {
            Assembly asm = Assembly.GetAssembly(GetType());
            FileVersionInfo finfo = FileVersionInfo.GetVersionInfo(
                                           asm.Location);
            string version = finfo.FileVersion;
            string date = String.Empty;
            string platform = String.Empty;
            System.Console.Out.WriteLine("NETRuby {0} ({1}) [{2}]",
                                           version, date, platform);
        }
        private void ShowCopyright()
        {
            System.Console.Out.WriteLine("ruby - Copyright (C) 1993-2002 Yukihiro Matsumoto");
            System.Console.Out.WriteLine("NETRuby - Copyright (C) 2002 arton");
        }
        private void ShowUsage()
        {
        }
        /*
        public void SetKCode(char c)
        {
        }
        
        public void RubyScript(string name)
        {
            if (name != null)
            {
                progName.SetData(name);
                SourceFile = name;
            }
        }
        public RException ErrorInfo
        {
            get { return GetCurrentContext().errInfo; }
        }
        internal void SetArgv(string[] args, int start)
        {
            argv.Clear();
            for (int i = start; i < args.Length; i++)
            {
                argv.Add(args[i]);
            }
        }
*/
/*
        public bool Require(string path)
        {
            return loader.Require(path);
        }
*/
        
        public void Load(object fobj, bool wrap)
        {
/*        
            RThread th = GetCurrentContext();
            RMetaObject cls = th.rClass;
            RObjectBase self = topSelf;

            if (wrap && th.safeLevel >= 4)
            {
                CheckType(fobj, typeof(RString));
            }
            else
            {
                CheckSafeString(th, fobj);
            }
            string fname = loader.FindFile(fobj.ToString());
            if (fname == null)
                throw new eLoadError("No such file to load -- " + fobj.ToString());
            th.errInfo = null;
            RVarmap old = th.dyna_vars;
            th.dyna_vars = null;
            RModule old_wrapper = th.wrapper;
            RNCRef saved_cRef = th.cRef;
            th.cRef = topCRef;
            if (wrap == false)
            {
                th.Secure(4);
                th.rClass = cObject;
                th.wrapper = null;
            }
            else
            {
                th.wrapper = new RModule(this);
                th.rClass = th.wrapper;
                self = (RObjectBase)topSelf.Clone();
                RMetaObject.ExtendObject(self, th.rClass);
                th.cRef = new RNCRef(th.wrapper, th.cRef);
            }
            Frame frm = th.PushFrame(false);
            frm.lastFunc = 0;
            frm.lastClass = null;
            frm.self = self;
            frm.cBase = new RNCRef(th.rClass);
            Scope.ScopeMode vm = th.PushScope();
            th.ScopeSet(Scope.ScopeMode.Private);
            th.PushTag(Tag.TAG.PROT_NONE);
            Tag.TAG state = Tag.TAG.EMPTY;
            try
            {
                th.EnterEval();
                LoadFile(fname);
                th.LeaveEval();
                RNode node = th.evalTree;
                if (th.CompileFailed == false)
                {
                    EvalNode(self, node, th);
                }
            }
            catch (eTagJump e)
            {
#if EXCEP_TRACE
                Console.WriteLine(e.StackTrace);
#endif        
                state = e.state;
            }
            catch (Exception ex)
            {
#if EXCEP_TRACE
                Console.WriteLine(ex.StackTrace);
#endif        
                th.errInfo = new RException(this, ex);
                state = Tag.TAG.RAISE;
            }
            th.PopTag(true);
            th.cRef = saved_cRef;
            th.PopScope(vm);
            th.PopFrame();
            th.rClass = cls;
            th.dyna_vars = old;
            th.wrapper = old_wrapper;
            if (th.CompileFailed)
            {
                th.ClearCompileError();
                throw new eTagJump(th.errInfo);
            }
            if (state != Tag.TAG.EMPTY)
            {
                JumpTagButLocalJump(state & Tag.TAG.MASK);
            }
            if (th.errInfo != null)
            {
                throw new eTagJump(th.errInfo);
            }
*/            
        }
/*        
        public void LoadFile(string fname)
        {
            LoadFile(fname, false);
        }
        private void LoadFile(string fname, bool script)
        {
            TextReader tr;
            int line_start = 1;
            if (fname == "-")
            {
                tr = System.Console.In;
            }
            else
            {
                try
                {
                    tr = new StreamReader(fname, Encoding.Default);
                }
                catch (FileNotFoundException)
                {
                    tr = null;
                    ruby_raise(null, eLoadError, "No such file to load -- " + fname);
                }
                catch (Exception e)
                {
                    Exception en = new eTagJump(Tag.TAG.RAISE, e, e.Message + " -- " + fname);
                    throw en;
                }
            }
            if (script)
            {
                int c = tr.Peek();
                if (c == '#')
                {
                    string ln = tr.ReadLine();
                    line_start++;
                    if (ln != null && ln.Length > 2 && ln[1] == '!')
                    {
                        int r = ln.IndexOf("ruby");
                        if (r < 0)
                        {
                            // execute non ruby program
                            Process.GetCurrentProcess().Kill();
                        }

                        ln = ln.Substring(r + 4);
                        if ((r = ln.IndexOf(" -")) >= 0)
                        {
                            ln = ln.Substring(1);
                            string[] argst = ln.Split(null);
                            int n = 0;
                            foreach (string x in argst)
                            {
                                if (x.Length > 1) n++;
                            }
                            string[] args = new string[n];
                            n = 0;
                            foreach (string x in argst)
                            {
                                if (x.Length > 1)
                                {
                                    args[n++] = x;
                                }
                            }
                            SetOptions(args, true);
                        }
                    }
                }
            }
            bool thCreated;
            RThread th = GetCurrentContext(out thCreated);
            th.evalTree = CompileFile(fname, tr, line_start, th);
            if (script && __end__seen)
            {
                DefineGlobalConst("DATA", tr);
            }
            else if (fname != "-")
            {
                tr.Close();
            }
        }
 
        private string MoreSwitches(string p)
        {
            string result = null;
            for (int i = 0; i < p.Length; i++)
            {
                if (Char.IsWhiteSpace(p[i]))
                {
                    p = p.Substring(0, i);
                    result = p.Substring(i);
                }
            }
            SetOptions(new string[] {p}, true);
            return (result == null) ? "" : result.TrimStart(null);
        }
*/               
        internal RNode CompileFile(string fname, TextReader tr, int start, RThread th)
        {
            th.ClearCompileError();
            Parser parser = new Parser(this, th, th.IsInEval);
            try
            {
                yyParser.Scanner scanner =
                    new yyParser.Scanner(parser, tr, fname, start, this, th);
                RNode node = (RNode)parser.Parse(scanner);
                /*
                if (doPrint)
                {
                    ParserAppendPrint(th, node);
                }
                */
                return node;
            }
            catch (IOException oe)
            {
                Console.Error.WriteLine(oe.Message);
            }
            return null;
        }
/*        
        private void ParserAppendPrint(RThread th, RNode node)
        {
            uint id = intern("$_");
            node = RNode.block_append(th,
                        node,
                        new RNFCall(null, intern("print"),
                        new RNArray(null, new RNGVar(id, global_entry(id)))));

        }
*/        
/*        
        public void Run()
        {
            bool thCreated;
            RThread th = GetCurrentContext(out thCreated);
            if (th.CompileFailed) return;
        
            Tag.TAG state = Tag.TAG.EMPTY;
            th.PushTag(Tag.TAG.PROT_NONE);
            th.PushIter(ITER.NOT);
            try
            {
                EvalNode(topSelf, th.evalTree, th);
            }
            catch (eTagJump tj)
            {
#if _DEBUG
                System.Console.WriteLine(tj.StackTrace);
#endif        
                state = tj.state;
                if (th.CurrentTag.tag != Tag.TAG.EMPTY)
                    errorPrint(th);
            }
            catch (Exception ex)
            {
#if _DEBUG
                System.Console.WriteLine(ex.StackTrace);
#endif        
                state = Tag.TAG.RAISE;
                if (th.CurrentTag.tag != Tag.TAG.EMPTY)
                    errorPrint(th, ex);
            }
            th.PopIter();
            th.PopTag(false);

            if (thCreated) ClearContext(th);
        }
*/
        internal void errorPrint(RThread th)
        {
        
            if (th.errInfo == null) return;
            ////th.PushTag(Tag.TAG.PROT_NONE);
            RArray errat;
            try
            {
                errat = th.errInfo.Backtrace;
            }
            catch
            {
                errat = null;
            }
            ////th.PopTag(false);
        
            if (errat == null)
            {
                if (th.file != null)
                    System.Console.Error.Write(String.Format("{0}:{1}",
                                     th.file, th.line));
                else
                    System.Console.Error.Write(String.Format("{0}", th.line));
            }
            else if (errat.Count == 0)
            {
                th.errorPos();
            }
            else
            {
                if (errat[0] == null)
                    th.errorPos();
                else
                    System.Console.Error.Write(errat[0].ToString());
            }
            string einfo = String.Empty;
            RMetaObject eClass = ClassOf(th.errInfo);
            ////th.PushTag(Tag.TAG.PROT_NONE);
            try
            {
                einfo = RString.AsString(this, th.errInfo);
            }
            catch
            {
            }
            ////Tag tag2 = th.PopTag(true);
            if (eClass == eRuntimeError && einfo.Length == 0)
            {
                System.Console.Error.WriteLine(": unhandled exception");
            }
            else
            {
                string epath = eClass.ClassPath;
                if (einfo.Length == 0)
                {
                    System.Console.Error.WriteLine(": " + epath);
                }
                else
                {
                    if (epath[0] == '#') epath = null;
                    System.Console.Error.Write(": " + einfo);
                    if (epath != null)
                    {
                        System.Console.Error.WriteLine(" (" + epath + ")");
                    }
                }
            }
            printBacktrace(errat);           
        }
        internal void errorPrint(RThread th, Exception errinfo)
        {
        
            if (errinfo is eTagJump)
            {
                errorPrint(th);
                return;
            }
            th.errorPos();

            string tp = errinfo.GetType().ToString();
            int n = tp.LastIndexOf('.');
            if (n >= 0) tp = tp.Substring(n + 1);
            if (tp[0] == 'e') tp = tp.Substring(1);
            System.Console.Error.WriteLine(String.Format(": {0}: ({1})",
                                                         errinfo.Message, tp));
            RArray ra = Backtrace(-1);
            printBacktrace(ra);
#if EXCEP_TRACE
            warning(errinfo.StackTrace);
#endif
        }
        
        void printBacktrace(RArray errat)
        {
            if (errat != null)
            {
                for (int i = 1; i < errat.Count; i++)
                {
                    if (errat[i] is string || errat[i] is RString)
                    {
                        System.Console.Error.WriteLine(String.Format("\tfrom {0}", errat[i]));
                    }
                    if (i == 8 && errat.Count > 18)
                    {
                        System.Console.Error.WriteLine(String.Format("\t ... {0} levels...",
                                                                     errat.Count - 8 - 5));
                        i = errat.Count - 5;
                    }
                }
            }
        }
        
        ////internal Frame topFrame;
        internal RObjectBase topSelf;
        private int exitStatus = 0;
        public RObjectBase TraceFunc;
        internal Loader loader;
        public RClass cObject;
        public RModule cModule;
        public RKernel mKernel;
        public REnumerable mEnumerable;
        public RClass cClass;
        public RClass cNilClass;
        public RClass cFalseClass;
        public RClass cTrueClass;
        public RClass cData;
        public RClass cSymbol;
        public RProcClass cProc;
        public RThreadClass cThread;
        public RThreadGroupClass cThreadGroup;
        public RTimeClass cTime;
        ////public RDotNetClass cDotNet;
        //
        public RClass cString;
        public RClass cArray;
        //
        public RClass cNumeric;
        public RClass cInteger;
        public RClass cFixnum;
        public RClass cBignum;
        public RClass cFloat;
        public RHashClass cHash;
        public RIOClass cIO;
        public RRegexpClass cRegexp;
        public RClass cMatch;
        //
        public RClass eException;
        public RClass eSystemExit;
        public RClass eFatal;
        public RClass eInterrupt;
        public RClass eSignal;
        public RClass eStandardError;
        public RClass eTypeError;
        public RClass eArgError;
        public RClass eIndexError;
        public RClass eRangeError;
        public RClass eScriptError;
        public RClass eSyntaxError;
        public RClass eNameError;
        public RClass eLoadError;
        public RClass eNotImpError;
        public RClass eRuntimeError;
        public RClass eSecurityError;
        public RClass eNoMemError;
        public RClass eThreadError;
        public RClass eRegexpError;
        //
        private uint last_id;
        internal uint idEq;
        internal uint idEql;
        private uint missing;
        private uint initialize;
        private uint inspect_key = 0;
        //
        public RNil oNil;
        public RTrue oTrue;
        public RFalse oFalse;
        internal RFixnum oFixZero;
        internal RFixnum oFixnum;
        internal RFloat oFloat;
        internal RString oString;
        internal Symbol oSymbol;
        //
        ////internal RNCRef topCRef;
        //
        internal bool verbose = true;
        internal bool debug = true;
        internal bool yydebug = false;
        internal bool doLoop = false;
        internal bool doPrint = false;
        internal bool doLine = false;
        internal bool doSplit = false;
        internal bool doCheck = false;
        public bool __end__seen = false;
        internal string fSep = null;
        internal string outputFS = null;
        internal string recSep = "\n";
        internal string defaultRecSep = "\n";
        public string SourceFile
        {
            get { return GetCurrentContext().file; }
            set { GetCurrentContext().file = value; }
        }
        public int SourceLine
        {
            get { return GetCurrentContext().line; }
            set { GetCurrentContext().line = value; }
        }
        internal RString progName = null;
        internal RArray argv;
        private string script = null;
        internal Hashtable threads;
        internal RThread threadMain;

        internal bool IsBlockGiven
        {
            get { return GetCurrentContext().IsBlockGiven(); }
        }

        internal object Yield(object o)
        {
            RThread th = GetCurrentContext();
            RCBlock b = th.GetLegacyBlock();
            if(b == null) {
                throw new Exception("No block given!");
            }
            return b.Call(th, new RBasic[] { RClass.ConvertToRuby(this, o) }, null);
        }
        
        /*
        internal bool IsBlockGiven
        {
            get { return (GetCurrentContext().frame.iter != 0); }
        }
        internal RProc Lambda()
        {
            return new RProc(this);
        }

        struct evalParam
        {
            internal evalParam(RBasic o, string s, string f, int l)
            {
                self = o;
                src = s;
                file = f;
                line = l;
            }
            public RBasic self;
            public string src;
            public string file;
            public int line;
        }
        delegate object ExecMethod(RThread th, object arg);
        private object EvalI(RThread th, object o)
        {
            evalParam param = (evalParam)o;
            return Eval(param.self, param.src, null, param.file, param.line);
        }
        private object Eval(RThread th, RMetaObject under, RBasic self, object src, string file, int line)
        {
            if (src is string == false)
            {
                if (th.safeLevel >= 4)
                {
                    CheckType(src, typeof(RString));
                }
                else
                {
                    CheckSafeString(th, src);
                }
            }
            evalParam param = new evalParam(self, src.ToString(), file, line);
            return ExecUnder(th, under, new ExecMethod(EvalI), param);
        }
        private object YieldI(RThread th, object o)
        {
            RBasic self = (RBasic)o;
            th.block.frame.cBase = th.frame.cBase;
            return Yield(self, self, th.rClass, false);
        }
        private object Yield(RThread th, RMetaObject under, RBasic self)
        {
            return ExecUnder(th, under, new ExecMethod(YieldI), self);
        }
        object ExecUnder(RThread th, RMetaObject under, ExecMethod func, object arg)
        {
            object val = null;
            RMetaObject cls = th.PushClass(under);
            th.PushFrame(false);
            th.CopyPrevFrame();
            RNode cbase = th.frame.cBase;
            if (cbase.clss != under)
            {
                th.frame.cBase = new RNCRef(under, cbase);
            }
            RNCRef old_cRef = th.cRef;
            Scope.ScopeMode vm = th.PushScope();
            th.PushTag(Tag.TAG.PROT_NONE);

            Tag.TAG state = Tag.TAG.EMPTY;
            try
            {
                val = func(th, arg);
            }
            catch (eTagJump ex)
            {
                state = ex.state;
            }
            catch (Exception ex)
            {
                state = Tag.TAG.RAISE;
                th.errInfo = new RException(this, ex);
            }
            th.PopTag(true);
            th.cRef = old_cRef;
            th.PopScope(vm);
            th.PopFrame();
            th.PopClass(cls);
            if (state != Tag.TAG.EMPTY)
                th.TagJump(state);
            return val;
        }
        
        internal object Yield(object o)
        {
            return Yield(o, null, null, false);
        }
        internal object Yield(object val, object self, object klass, bool acheck)
        {
            RThread th = GetCurrentContext();

            string file = th.file;
            int line = th.line;
            Tag.TAG state = Tag.TAG.EMPTY;
            object result = null;
            if (th.IsBlockGiven == false)
            {
                throw new eLocalJumpError("no block given");
            }
            RVarmap old = th.dyna_vars;
            th.dyna_vars = null;
            RMetaObject cls = th.rClass;
            Frame _frame = (Frame)th.block.frame.Clone();
            _frame.prev = th.frame;
            th.frame = _frame;
            RNCRef old_cRef = th.cRef;
            th.cRef = _frame.cBase;
            RModule old_wrapper = th.wrapper;
            th.wrapper = th.block.wrapper;
            Scope old_scope = th.scope;
            th.scope = th.block.scope;
            Block blk = th.block;
            th.block = th.block.prev;
            if (blk.DScope)
            {
                // put place holder for dynamic (in-block) local variables
                th.dyna_vars = th.new_dvar(0, null, blk.dyna_vars);
            }
            else
            {
                // FOR does not introduce new scope
                th.dyna_vars = blk.dyna_vars;
            }
            th.rClass = (klass == null) ? blk.klass : (RMetaObject)klass;
            if (klass == null) self = blk.self;
            RNode node = blk.body;

            if (blk.var != null)
            {
                th.PushTag(Tag.TAG.PROT_NONE);
                state = Tag.TAG.EMPTY;
                try
                {

                    if (blk.var is RNBlockNoArg) // original = (NODE*)1
                    {
                        if (acheck && (val is QUndef == false) &&
                            val is RArray && ((RArray)val).Count != 0)
                        {
                            throw new ArgumentException("wrong # of arguments ("
                                                        + ((RArray)val).Count.ToString()
                                                        + " for 0)");
                        }
                    }
                    else
                    {
                        if (blk.var is RNMAsgn == false)
                        {
                            // argument adjust for proc_call etc
                            if (acheck && (val is QUndef == false))
                            {
                                if (val is RArray && ((RArray)val).Count == 1)
                                {
                                    val = ((RArray)val)[0];
                                }
                                else if (val is object[] && ((object[])val).Length == 1)
                                {
                                    val = ((object[])val)[0];
                                }
                            }
                        }
                        Assign(self, blk.var, val, acheck);
                    }
                }
                catch (eTagJump e)
                {
                    state = e.state;
                }
                catch (Exception e)
                {
                    state = Tag.TAG.RAISE;
                    th.errInfo = new RException(this, e);
                }
                th.PopTag(true);
                if (state != Tag.TAG.EMPTY) goto pop_state;
            }
            else
            {
                // argument adjust for proc_call etc.
                if (acheck && (val is QUndef == false))
                {
                    if (val is RArray && ((RArray)val).Count == 1)
                    {
                        val = ((RArray)val)[0];
                    }
                    else if (val is object[] && ((object[])val).Length == 1)
                    {
                        val = ((object[])val)[0];
                    }
                }
            }

            th.PushIter(blk.iter);
            th.PushTag(Tag.TAG.PROT_NONE);
        redo:
            try
            {
                if (node == null)
                {
                    result = null;
                }
                else if (node is RNCFunc)
                {
                    bug("CFunc is not for Iterator block in NETRuby");
                }
                else if (node is RNIFunc)
                {
                    result = ((RNIFunc)node).Call(val, self);
                }
                else
                {
                    result = Eval(self, node);
                }
            }
            catch (eTagJump e)
            {
                state = e.state;
            }
            catch (Exception ex)
            {
                state = Tag.TAG.RAISE;
                th.errInfo = new RException(this, ex);
            }
            switch (state)
            {
            case Tag.TAG.REDO:
                state = Tag.TAG.EMPTY;
                goto redo;
            case Tag.TAG.NEXT:
                state = Tag.TAG.EMPTY;
                result = null;
                break;
            case Tag.TAG.BREAK:
            case Tag.TAG.RETURN:
//                state |= (serial++ << 8);
//                state |= 0x10;
                blk.DStatus = state;
                break;
            default:
                break;
            }

            th.PopTag(true);
        pop_state:
            th.PopIter();
            th.rClass = cls;
            th.dyna_vars = old;
            th.block = blk;
            th.frame = th.frame.prev;
            th.cRef = old_cRef;
            th.wrapper = old_wrapper;
            th.file = file;
            th.line = line;

            if (th.scope.DontRecycle)
            {
                old_scope.Dup();
            }
            th.scope = old_scope;
            if (state != Tag.TAG.EMPTY)
            {
                if (blk.IsTag == false)
                {
                    switch (state & Tag.TAG.MASK)
                    {
                    case Tag.TAG.BREAK:
                    case Tag.TAG.RETURN:
                        JumpTagButLocalJump(state & Tag.TAG.MASK);
                        break;
                    }
                }
                th.TagJump(state);
            }
            return result;
        }

        internal void JumpTagButLocalJump(Tag.TAG state)
        {
            switch (state) {
            case Tag.TAG.EMPTY:
                break;
            case Tag.TAG.RETURN:
                throw new eLocalJumpError("unexpected return");
            case Tag.TAG.NEXT:
                throw new eLocalJumpError("unexpected next");
            case Tag.TAG.BREAK:
                throw new eLocalJumpError("unexpected break");
            case Tag.TAG.REDO:
                throw new eLocalJumpError("unexpected redo");
            case Tag.TAG.RETRY:
                throw new eLocalJumpError("retry outside of rescue clause");
            default:
                //                JUMP_TAG(state);
                break;
            }
        }
*/
        internal int ScanArgs(object[] argv, string fmt, object[] args)
        {
            int i = 0;
            int dst = 0;
            int idx = 0;
            if (fmt[idx] == '*') goto rest_arg;
            if (Char.IsDigit(fmt[idx]))
            {
                int n = (int)Char.GetNumericValue(fmt[idx]);
                if (n > argv.Length)
                    throw new ArgumentException(
                        String.Format("wrong # of arguments ({0} for {1})", argv.Length, n));
                for (i = 0; i < n; i++)
                {
                    args[dst++] = argv[i];
                }
                idx++;
            }
            if (idx < fmt.Length && Char.IsDigit(fmt[idx]))
            {
                int n = i + (int)Char.GetNumericValue(fmt[idx]);
                for (; i < n; i++)
                {
                    args[dst++] = (argv.Length > i) ? argv[i] : null;
                }
                idx++;
            }
        rest_arg:
            int rs = i;
            if (idx < fmt.Length && fmt[idx] == '*')
            {
                object[] rest = new object[argv.Length - i];
                args[dst] = rest;
                for (; i < argv.Length; i++)
                {
                    rest[i - rs] = argv[i];
                }
                dst++;
                idx++;
            }
            if (idx < fmt.Length && fmt[idx] == '&')
            {
                /*            
                if (IsBlockGiven)
                {
                    args[dst] = Lambda();
                }
                else
                {
                */                
                    args[dst] = null;
                /*                    
                }
                */                
            }
            if (argv.Length > i)
            {
                throw new ArgumentException(
                    String.Format("wrong # of arguments({0} for {1})", argv.Length, i));
            }
            return argv.Length;

        
        }
/*        
        
        public void Secure(int level)
        {
            RThread th = GetCurrentContext();
            th.Secure(level);
        }
        public int SafeLevel
        {
            get {
                RThread th = GetCurrentContext();
                return (th == null) ? 0 : th.safeLevel; }
            set {
                RThread th = GetCurrentContext();
                th.safeLevel = value; }
        }
        public void CheckSafeString(object x)
        {
            RThread th = GetCurrentContext();
            CheckSafeString(th, x);
        }
        internal void CheckSafeString(RThread th, object x)
        {
            if (x is RBasic)
            {
                RBasic r = (RBasic)x;
                if (th.safeLevel > 0 && r.IsTainted)
                {
                    if (th.frame.lastFunc != 0)
                        throw new SecurityException("Insecure operation - " + id2name(th.frame.lastFunc));
                    else
                        throw new SecurityException("Insecure operation: -r");
                }
                th.Secure(4);
                if (r is RString) return;
            }
            if (x is string == false)
                throw new eTypeError("wrong argument type " + ClassOf(InstanceOf(x)).Name + " (expected String)");
        }
*/
        
        public void CheckType(object x, Type t)
        {
            if (x is QUndef)
            {
                bug("undef leaked to the Ruby space");
            }
            if (t.IsInstanceOfType(x) == false)
            {
                foreach (RBuiltinType ent in RBuiltinType.Table)
                {
                    string etype;
                    if (ent.Type == t)
                    {
                        if (x == null)
                        {
                            etype = "nil";
                        }
                        else if (x is int)
                        {
                            etype = "Fixnum";
                        }
                        else if (RBasic.IsSpecialConstType(x))
                        {
                            etype = RString.AsString(this, x);
                        }
                        else
                        {
                            etype = ClassOf(x).ClassName;
                        }
                        throw new eTypeError("wrong argument type " + etype +
                                             " (expected " + ent.Name + ")");
                    }
                }
                bug("unknown type " + x.GetType().Name);
            }
        }
/*        
        internal void SecureVisibility(RBasic self)
        {
            RThread th = GetCurrentContext();
            if (th.safeLevel >= 4 && self.IsTainted == false)
            {
                throw new SecurityException("Insecure: can't change method visibility");
            }
        }
*/        
        public void ErrorFrozen(string what)
        {
            throw new eTypeError("can't modify frozen " + what);
        }
        static private object errAtGetter(uint id, GlobalEntry gb, NetRuby rb)
        {
            RThread th = rb.GetCurrentContext();
            if (th.errInfo != null)
            {
                return th.errInfo.Backtrace;
            }
            return null;
        }
        static private void errAtSetter(object val, uint id, GlobalEntry gb, NetRuby rb)
        {
            RThread th = rb.GetCurrentContext();
            if (th.errInfo == null)
            {
                throw new ArgumentException("$! not set");
            }
            RException.exc_set_backtrace(th.errInfo, val);
        }
        static private void errInfoSetter(object val, uint id, GlobalEntry gb, NetRuby rb)
        {
            RThread th = rb.GetCurrentContext();
            if (val != null)
            {
                RBasic b = rb.InstanceOf(val);
                if (b.IsKindOf(rb.eException) == false)
                    throw new eTypeError("assigning non-exception to $!");
            }
            th.errInfo = (RException)val;
        }
        static private object errInfoGetter(uint i, GlobalEntry gb, NetRuby rb)
        {
            RThread th = rb.GetCurrentContext();
            return th.errInfo;
        }
/*        
        static private object safeGetter(uint i, GlobalEntry gb, NetRuby rb)
        {
            return rb.GetCurrentContext().safeLevel;
        }
        static private void safeSetter(object val, uint i, GlobalEntry gb, NetRuby rb)
        {
            int level = RInteger.ToInt(rb, val);
            RThread th = rb.GetCurrentContext();
            if (level < th.safeLevel)
                throw new SecurityException(String.Format("tried to downgrade safe level from {0} to {1}", th.safeLevel, level));
            th.safeLevel = level;
        }
*/        
        static private object verboseGetter(uint i, GlobalEntry gb, NetRuby rb)
        {
            return rb.verbose;
        }
        static private void verboseSetter(object val, uint i, GlobalEntry gb, NetRuby rb)
        {
            if (val == null) rb.verbose = false;
            else if (val is bool) rb.verbose = (bool)val;
            else rb.verbose = true;
        }
        static private object debugGetter(uint i, GlobalEntry gb, NetRuby rb)
        {
            return rb.debug;
        }
        static private void debugSetter(object val, uint i, GlobalEntry gb, NetRuby rb)
        {
            if (val == null) rb.debug = false;
            else if (val is bool) rb.debug = (bool)val;
            else rb.debug = true;
        }
        
        static internal object lastLineGetter(uint i, GlobalEntry gb, NetRuby rb)
        {
            /*
            RThread th = rb.GetCurrentContext();
            if (th.scope.local_vars != null)
            {
                return th.scope.local_vars[0];
            }
            */
            return null;
        }
        static private void lastLineSetter(object val, uint i, GlobalEntry gb, NetRuby rb)
        {
            /*
            RThread th = rb.GetCurrentContext();
            if (th.scope.local_vars != null)
            {
                th.scope.local_vars[0] = val.ToString();
            }
            else
            {
            }
            */
        }
        
        static private object printGetter(uint i, GlobalEntry gb, NetRuby rb)
        {
            return rb.doPrint;
        }
        static private object splitGetter(uint i, GlobalEntry gb, NetRuby rb)
        {
            return rb.doSplit;
        }
        static private object lineGetter(uint i, GlobalEntry gb, NetRuby rb)
        {
            return rb.doLine;
        }
        static private object argvGetter(uint i, GlobalEntry gb, NetRuby rb)
        {
            return rb.argv;
        }
#if RUBY_ARG0
        static private void SetArg0(object val, uint i, GlobalEntry gb, NetRuby rb)
        {
        }
#endif        
        internal struct OPTBL {
            internal OPTBL(uint t, string s)
            {
                token = t;
                name = s;
            }
            internal uint token;
            internal string name;
        }
        internal static OPTBL[] op_tbl = new OPTBL[]{
            new OPTBL(Token.tDOT2,        ".."),
            new OPTBL(Token.tDOT3,        "..."),
            new OPTBL('+',        "+"),
            new OPTBL('-',        "-"),
            new OPTBL('+',        "+(binary)"),
            new OPTBL('-',        "-(binary)"),
            new OPTBL('*',        "*"),
            new OPTBL('/',        "/"),
            new OPTBL('%',        "%"),
            new OPTBL(Token.tPOW,        "**"),
            new OPTBL(Token.tUPLUS,        "+@"),
            new OPTBL(Token.tUMINUS,        "-@"),
            new OPTBL(Token.tUPLUS,        "+(unary)"),
            new OPTBL(Token.tUMINUS,        "-(unary)"),
            new OPTBL('|',        "|"),
            new OPTBL('^',        "^"),
            new OPTBL('&',        "&"),
            new OPTBL(Token.tCMP,        "<=>"),
            new OPTBL('>',        ">"),
            new OPTBL(Token.tGEQ,        ">="),
            new OPTBL('<',        "<"),
            new OPTBL(Token.tLEQ,        "<="),
            new OPTBL(Token.tEQ,        "=="),
            new OPTBL(Token.tEQQ,        "==="),
            new OPTBL(Token.tNEQ,        "!="),
            new OPTBL(Token.tMATCH,        "=~"),
            new OPTBL(Token.tNMATCH,        "!~"),
            new OPTBL('!',        "!"),
            new OPTBL('~',        "~"),
            new OPTBL('!',        "!(unary)"),
            new OPTBL('~',        "~(unary)"),
            new OPTBL('!',        "!@"),
            new OPTBL('~',        "~@"),
            new OPTBL(Token.tAREF,        "[]"),
            new OPTBL(Token.tASET,        "[]="),
            new OPTBL(Token.tLSHFT,        "<<"),
            new OPTBL(Token.tRSHFT,        ">>"),
            new OPTBL(Token.tCOLON2,        "::"),
            new OPTBL(Token.tCOLON3,        "::"),
            new OPTBL('`',        "`"),
        };
        internal st_table sym_tbl;
        internal st_table sym_rev_tbl;
        internal st_table global_tbl;
        internal st_table class_tbl;
        internal st_table autoload_tbl;
        ////internal st_table generic_iv_tbl;
        /*
        internal CacheEntry[] cache;
        */
/*
        internal Stack endProcs;
        internal Stack ephemeralEndProcs;
        internal class EndProcData
        {
            internal delegate void EndMethod(object o);
            private EndProcData(EndMethod fnc, object dat)
            {
                method = fnc;
                data = dat;
            }
            internal void Exec()
            {
                method(data);
            }
            private event EndMethod method;
            private object data;
            
            internal static void SetEndProc(NetRuby ruby, EndMethod mtd, object dt, bool ephemeral)
            {
                Stack stk;
                if (ephemeral) {
                    if (ruby.ephemeralEndProcs == null)
                        ruby.ephemeralEndProcs = new Stack();
                    stk = ruby.ephemeralEndProcs;
                }
                else
                {
                    if (ruby.endProcs == null)
                        ruby.endProcs = new Stack();
                    stk = ruby.endProcs;
                }
                lock (stk.SyncRoot)
                {
                    stk.Push(new EndProcData(mtd, dt));
                }
            }
        }
*/        
/*
        internal void ClearCache()
        {
            lock (cache.SyncRoot)
            {
                for (int i = 0; i < cache.Length; i++)
                {
                    cache[i].mid = 0;
                }
            }
        }
        internal void ClearCache(uint id)
        {
            lock (cache.SyncRoot)
            {
                for (int i = 0; i < cache.Length; i++)
                {
                    if (cache[i].mid == id)
                        cache[i].mid = 0;
                }
            }
        }
        internal void SetCache(RMetaObject o, uint id)
        {
            lock (cache.SyncRoot)
            {
                int idx = CacheEntry.EXPR1(o, id);
                cache[idx].SetEmtptyInfo(o, id);
            }
        }
        internal void SetCache(RMetaObject o, uint id, RNode body, RMetaObject org)
        {
            lock (cache.SyncRoot)
            {
                int idx = CacheEntry.EXPR1(o, id);
                cache[idx].SetInfo(o, id, body, org);
            }
        }
*/        
        //
        internal uint intern(string name)
        {
            uint id;
            if (sym_tbl.lookup(name, out id) == true)
                return id;
            id = 0;
            switch (name[0])
            {
            case '$':
                id |= (uint)Parser.ID.GLOBAL;
                break;
            case '@':
                if (name[1] == '@')
                    id |= (uint)Parser.ID.CLASS;
                else
                    id |= (uint)Parser.ID.INSTANCE;
                break;
            default:
                if (name[0] != '_' && !Char.IsLetter(name[0]))
                {
                    int i;
                    for (i = 0; i < NetRuby.op_tbl.Length; i++)
                    {
                        if (NetRuby.op_tbl[i].name == name)
                        {
                            id = NetRuby.op_tbl[i].token;
                            goto id_regist;
                        }
                    }
                }

                int last = name.Length - 1;
                if (name[last] == '=')
                {
                    string buf = name.Substring(0, last);
                    id = intern(buf);
                    if (id > (uint)Token.LAST_TOKEN && !Parser.is_attrset_id(id))
                    {
                        id = Parser.id_attrset(id);
                        goto id_regist;
                    }
                    id = (uint)Parser.ID.ATTRSET;
                }
                else if (Char.IsUpper(name[0]))
                {
                    id = (uint)Parser.ID.CONST;
                }
                else
                {
                    id = (uint)Parser.ID.LOCAL;
                }
                break;
            }
            lock (this)
            {
                id |= ++last_id << (int)Parser.ID.SCOPE_SHIFT;
            }
        id_regist:
            lock (sym_tbl.SyncRoot)
            {
                sym_tbl[name] = id;
                sym_rev_tbl[id] = name;
            }
            return id;
        }
        internal string id2name(uint id)
        {
            if (id < Token.LAST_TOKEN) {
                int i = 0;

                for (i=0; i < op_tbl.Length; i++) {
                    if (op_tbl[i].token == id)
                        return op_tbl[i].name;
                }
            }
            string name = null;

            if (sym_rev_tbl.lookup(id, out name))
                return name;

            if (Parser.is_attrset_id(id)) {
                uint id2 = Parser.make_local_id(id);

            again:
                name = id2name(id2);
                if (name != null) {
                    string buf = name + "=";
                    intern(name + "=");
                    return id2name(id);
                }
                if (Parser.is_local_id(id2)) {
                    id2 = Parser.make_const_id(id);
                    goto again;
                }
            }
            return null;
        }

        internal uint global_id(string s)
        {
            if (s[0] == '$') return intern(s);
            return intern("$" + s);
        }
        internal GlobalEntry global_entry(uint id)
        {
            object o;
            GlobalEntry ret = null;
            lock (global_tbl.SyncRoot)
            {
                if (global_tbl.lookup(id, out o))
                    ret = (GlobalEntry)o;
                else
                {
                    ret = new GlobalEntry(id);
                    global_tbl.Add(id, ret);
                }
            }
            return ret;
        }
        /*
        internal void CloneGenericIVar(RBasic clone, RBasic obj)
        {
            if (generic_iv_tbl == null) return;
            lock (generic_iv_tbl.SyncRoot)
            {
                if (generic_iv_tbl.ContainsKey(obj))
                {
                    st_table tbl = (st_table)generic_iv_tbl[obj];
                    generic_iv_tbl[clone] = tbl.Clone();
                }
            }
        }
        internal bool IsGenericIVarDefined(RBasic obj, uint id)
        {
            if (generic_iv_tbl == null) return false;
            st_table tbl = null;
            if (generic_iv_tbl.lookup(obj, out tbl) == false) return false;
            object val;
            return tbl.lookup(id, out val);
        }
        internal object GenericIVarGet(RBasic obj, uint id)
        {
            if (generic_iv_tbl == null) return null;
            st_table tbl = null;
            if (generic_iv_tbl.lookup(obj, out tbl) == false) return null;
            object val;
            if (tbl.lookup(id, out val)) return val;
            return null;
        }
        internal void GenericIVarSet(RBasic obj, uint id, object val)
        {
            if (generic_iv_tbl == null)
            {
                lock (this)
                {
                    if (generic_iv_tbl == null)
                    {
                        generic_iv_tbl = new st_table();
                    }
                }
            }
            st_table tbl = null;
            lock (generic_iv_tbl.SyncRoot)
            {
                if (generic_iv_tbl.lookup(obj, out tbl) == false)
                {
                    tbl = new st_table();
                    generic_iv_tbl[obj] = tbl;
                }
                tbl[id] = val;
            }
        }
        internal object GenericIVarRemove(RBasic obj, uint id)
        {
            if (generic_iv_tbl == null) return null;
            st_table tbl;
            if (generic_iv_tbl.lookup(obj, out tbl) == false) return null;
            object val = tbl[id];
            lock (tbl.SyncRoot)
            {
                tbl.Remove(id);
                if (tbl.Count == 0)
                {
                    generic_iv_tbl.Remove(obj);
                }
            }
            return val;
        }
        */
        public void DefineVariable(string name, object obj, GlobalEntry.Getter gtr, GlobalEntry.Setter str)
        {
            GlobalEntry ent = global_entry(global_id(name));
            lock (ent)
            {
                ent.data = obj;
                ent.ReplaceGetter(gtr);
                ent.ReplaceSetter(str);
            }
        }
        internal void DefineVirtualVariable(string name, GlobalEntry.Getter gtr, GlobalEntry.Setter str)
        {
            DefineVariable(name, null, gtr, str);
        }
        internal void DefineVariable(string name, object obj)
        {
            DefineVariable(name, obj, null, null);
        }
        internal void DefineReadonlyVariable(string name, object obj, GlobalEntry.Getter gtr)
        {
            GlobalEntry ent = global_entry(global_id(name));
            lock (ent)
            {
                ent.data = obj;
                ent.ReplaceGetter(gtr);
                ent.ReplaceSetter(new GlobalEntry.Setter(GlobalEntry.ReadonlySetter));
            }
        }
        internal void DefineGlobalConst(string name, object val)
        {
            DefineConst(cObject, name, val);
        }
        internal object GVarSet(GlobalEntry entry, object o)
        {
            RThread th = GetCurrentContext();
            /*if (th.safeLevel >= 4)
            {
                throw new SecurityException("Insecure: can't change global variable value");
            }*/
            lock (entry)
            {
                entry.SetValue(o, this);
            }
    // trace
            return o;
        }
        internal object GVarGet(GlobalEntry entry)
        {
            return entry.GetValue(this);
        }
        public void DefineConst(RMetaObject obj, string name, object val)
        {
            uint id = intern(name);
            if (Parser.is_const_id(id) == false)
            {
                throw new eNameError("wrong constant name " + name);
            }
            ////if (obj == cObject) Secure(4);
            obj.ConstSet(id, val);
        }
        public bool TopConstGet(uint id, out object val)
        {
            if (class_tbl.lookup(id, out val)) return true;
/*            
            if (autoload_tbl != null && autoload_tbl.ContainsKey(id))
            {
                AutoLoad(id);
                val = cObject.ConstGet(id);
                return true;
            }
*/            
            return false;
        }
        internal GlobalEntry Global(uint v)
        {
            return global_entry(v);
        }
/*
        internal uint[] Locals
        {
            get { return GetCurrentContext().Locals; }
        }
        public void AutoLoad(uint id)
        {
            ; // to be filled
        }
        public bool IsAutoloadDefined(uint id)
        {
            return (autoload_tbl != null && autoload_tbl.ContainsKey(id));
        }
*/
        public string Inspect(Object o)
        {
            object x = Funcall(o, "inspect", null);
            return x.ToString();
        }
        
        public object ProtectInspect(RBasic obj, RBasic.InspectMethod method, object[] arg)
        {
            if (inspect_key == 0)
                inspect_key = intern("__inspect_key__");
            RThread th = GetCurrentContext();
            ArrayList inspect_tbl = (ArrayList)th.LocalARef(inspect_key);
            if (inspect_tbl == null)
            {
                inspect_tbl = new ArrayList();
                th.LocalASet(inspect_key, inspect_tbl);
            }
            int id = InstanceOf(obj).id;
            if (inspect_tbl.Contains(id))
            {
                return method(obj, arg);
            }
            return inspect_call(inspect_tbl, id, obj, method, arg);
        }
        private object inspect_call(ArrayList inspect_tbl, int id, RBasic obj, RBasic.InspectMethod method, object[] arg)
        {
            inspect_tbl.Add(id);
            object result = null;
            try
            {
                result = method(obj, arg);
            }
            finally
            {
                inspect_tbl.Remove(id);
            }
            return result;
        }
        
        internal bool IsInspecting(object obj) // for avoiding eternal loop by inspect
        {
            if (inspect_key == 0)
                inspect_key = intern("__inspect_key__");

            ArrayList inspect_tbl = (ArrayList)GetCurrentContext().LocalARef(inspect_key);
            if (inspect_tbl == null) return false;
            return inspect_tbl.Contains(InstanceOf(obj).id);
        }
       
        internal RArray ThreadList
        {
            get {
                RArray ary = new RArray(this, true);
                lock (threads.SyncRoot)
                {
                    foreach (DictionaryEntry ent in threads)
                    {
                        RThread r = (RThread)ent.Value;
                        if (r.IsAlive) ary.Add(r);
                    }
                }
                ary.Add(threadMain);
                return ary;
            }
        }
        internal RThread GetCurrentContext()
        {
            Thread t = Thread.CurrentThread;
            RThread th;
            lock (threads.SyncRoot)  // Indexer will insert a new element if not found.
            {
                th = (RThread)threads[t];
            }
            return (th == null) ? threadMain : th;
        }

        internal RThread GetCurrentContext(out bool created)
        {
            created = false;
            Thread t = Thread.CurrentThread;
            if (t == threadMain.Owner) return threadMain;
            RThread th;
            lock (threads.SyncRoot)  // Indexer will insert a new element if not found.
            {
                th = (RThread)threads[t];
            }
            if (th == null)
            {
                lock (threadMain)
                {
                    th = new RThread(this, threadMain);
                }
                threads[t] = th;
                created = true;
            }
            return th;
        }
        internal void AddContext(RThread th)
        {
            lock (threads.SyncRoot)
            {
                threads[th.thread] = th;
            }
        }
        internal void ClearContext(RThread th)
        {
            lock (threads.SyncRoot)
            {
                threads.Remove(th.Owner);
            }
        }
        
        //
        public RModule DefineModule(string name)
        {
            uint id = intern(name);
            if (cObject.IsConstDefined(id))
            {
                object o = cObject.ConstGet(id);
                if (o is RModule) return (RModule)o;
                throw new eTypeError(name + " is not a module");
            }
            RModule module = new RModule(this, name);
            return module;
        }
        /*
        private void CallEndProc(object o)
        {
            RThread th = GetCurrentContext();
            th.PushIter(ITER.NOT);
            Frame frm = th.PushFrame(false);
            frm.self = frm.prev.self;
            frm.lastFunc = 0;
            frm.lastClass = null;
            try
            {
                ((RProc)o).Call();
            }
            catch (Exception e)
            {
                th.errorHandle(e);
            }
            th.PopFrame();
            th.PopIter();
        }
        internal void End()
        {
            RThread th = GetCurrentContext();
            Frame frm = th.PushFrame(false);
            frm.args = null;
            EndProcData.SetEndProc(this, new EndProcData.EndMethod(CallEndProc), Lambda(),
                                   th.wrapper != null);
            th.PopFrame();
        }
        object ruby_at_exit(RBasic r, params object[] args)
        {
            NetRuby ruby = r.ruby;
            object o = ruby.Lambda();
            EndProcData.SetEndProc(ruby, new EndProcData.EndMethod(ruby.CallEndProc), o,
                                   ruby.GetCurrentContext().wrapper != null);
            return o;
        }
*/
/*        
        internal object ScopedFuncall(RMetaObject klass, object recver, uint id, object[] argv, RNScope n)
        {
            RThread th = GetCurrentContext();
#if LOCAL_TRACE
            System.Console.WriteLine("ScopedFuncall");
            for (int h = 0; h < argv.Length; h++)
            {
                System.Console.WriteLine("arg = " + ((argv[h]==null)?"null":argv[h].ToString()));
            }
#endif
            object result = null;
            object[] local_vars = null;
            RNCRef savedCRef = null;
            Scope.ScopeMode vm = th.PushScope();
            if (n.rval != null)
            {
                savedCRef = th.cRef;
                th.cRef = (RNCRef)n.rval;
                th.frame.cBase = th.cRef;
            }
            if (n.tbl != null)
            {
                local_vars = new object[n.tbl.Length + 1];
                local_vars[0] = n;
                th.scope.InitLocal(n.tbl, local_vars);
            }
            RNode body = n.next;
            RNode b2 = body;
#if LOCAL_TRACE
            System.Console.WriteLine("local_vars = " +
                             ((local_vars == null)?"null":local_vars.Length.ToString()));
#endif
            RVarmap old = th.dyna_vars;
            th.dyna_vars = null;

            th.PushTag(Tag.TAG.PROT_FUNC);
            Tag.TAG state = Tag.TAG.EMPTY;
            try
            {
                RNode node = null;
                if (body is RNArgs)
                {
                    node = body;
                    body = null;
                }
                else if (body is RNBlock)
                {
                    node = body.head;
                    body = body.next;
                }
                int i = 0;
                if (node != null)
                {
                    if (node is RNArgs == false)
                    {
                        bug("no argumenet-node");
                    }
                    i = node.cnt;
                    if (i > argv.Length)
                    {
                        throw new ArgumentException(
                            String.Format("wrong # of arguments({0} for {1})", argv.Length, i));
                    }
                    if (node.rest == -1)
                    {
                        int opt = i;
                        RNode optnode = node.opt;
                        while (optnode != null)
                        {
                            opt++;
                            optnode = optnode.next;
                        }
                        if (opt < argv.Length)
                        {
                            throw new ArgumentException(
                                String.Format("wrong # of arguments({0} for {1})",
                                              argv.Length, opt));
                        }
                        // index offset is 2, for $_ and $~
                        th.frame.args = local_vars;
                    }
                }

                if (local_vars != null)
                {
                    if (i > 0)
                    {
                        // +2 for $_ and $~
                        Array.Copy(argv, 0, local_vars, 2, i);
                    }
#if LOCAL_TRACE
                    for (int h = 0; h < i; h++)
                    {
                        System.Console.WriteLine("local = " +
                                 ((local_vars[2+h]==null)?"null":local_vars[2+h].ToString()));
                    }
#endif        
                    int argc = argv.Length - i;
                    if (node.opt != null)
                    {
                        RNode opt = node.opt;
                        while (opt != null && argc > 0)
                        {
                            Assign(recver, opt.head, argv[i], true);
                            argc--;
                            i++;
                            opt = opt.next;
                        }
                        if (opt != null)
                        {
                            th.file = opt.file;
                            th.line = opt.Line;
                            Eval(recver, opt);
                        }
                    }
                    if (node.rest >= 0)
                    {
                        ArrayList a = new ArrayList();
                        if (argc > 0)
                        {
                            for (int ix = argc; ix < argv.Length; ix++)
                                a.Add(argv[ix]);
                        }
                        th.scope.local_vars[node.rest] = new RArray(this, a);
                    }
                }
                if (TraceFunc != null)
                {
                }
                result = Eval(recver, body);
            }
            catch (eTagJump e)
            {
                Tag etag = th.CurrentTag;
                if (e.state == Tag.TAG.RETURN)
                {
                    result = etag.Result;
                    state = Tag.TAG.EMPTY;
                }
            }
             catch (Exception e)
            {
                th.errInfo = new RException(this, e);
                state = Tag.TAG.RAISE;
            }

            th.PopTag(true);

            th.dyna_vars = old;
            th.PopScope(vm);
            th.cRef = savedCRef;
            if (TraceFunc != null)
            {
            }
            switch (state)
            {
            case Tag.TAG.EMPTY:
                ;
                break;
            case Tag.TAG.RETRY:
                goto default;
            default:
                break;
            }
            return result;
        }

        internal void Assign(object self, RNode lhs, object val, bool check)
        {
            if (val is QUndef)
            {
                val = null;
            }
            if (val is string)
            {
                // create assignable object
                val = new RString(this, (string)val);
            }
            lhs.Assign(this, self, val, check);
        }

        public delegate object IterationProc(RBasic ob);
        public delegate object BlockProc(object it, object data, object self);
        public object Iterate(IterationProc itProc, RBasic dat1, BlockProc blProc, object data)
        {
            Tag.TAG state;
            object result = null;
            RThread th = GetCurrentContext();
            RNode node = new RNIFunc(th, blProc, data);
            RObjectBase self = topSelf;
        iter_retry:
            th.PushIter(ITER.PRE);
            Block _block = new Block(th, null, node, self);
            th.block = _block;
            th.PushTag(Tag.TAG.PROT_NONE);
            state = Tag.TAG.EMPTY;
            try
            {
                result = itProc(dat1);
            }
            catch (eTagJump e)
            {
                state = e.state;
                if (_block.CheckTag(e.state))
                {
                    state &= Tag.TAG.MASK;
                    if (state == Tag.TAG.RETURN)
                    {
                        Tag etag = th.CurrentTag;
                        result = etag.Result;
                    }
                }
            }
            catch (Exception e)
            {
                th.errInfo = new RException(this, e);
                state = Tag.TAG.RAISE;
            }
            th.PopTag(true);
            th.block = _block.prev;
            th.PopIter();
            switch (state)
            {
            case Tag.TAG.EMPTY:
                break;
            case Tag.TAG.RETRY:
                goto iter_retry;
            case Tag.TAG.BREAK:
                result = null;
                break;
            case Tag.TAG.RETURN:
                Tag etag = (Tag)th.protTag.Peek();
                etag.Result = result;
                goto default;
            default:
                th.TagJump(state);
                break;
            }
            return result;
        }

        internal object Iteration(RNode node, object self)
        {
            RThread th = GetCurrentContext();
            object result = null;
            for (;;)
            {
                th.PushTag(Tag.TAG.PROT_FUNC);
                Block _block = new Block(th, node.var, node.body, self);
                th.block = _block;

                Tag.TAG state = Tag.TAG.EMPTY;
                try
                {
                    th.PushIter(ITER.PRE);
                    if (node is RNIter)
                    {
                        result = Eval(self, node.iter);
                    }
                    else
                    {
                        string file = th.file;
                        int line = th.line;
                        _block.DScope = false;
                        object recv = CallIter(node, self);
                        th.file = file;
                        th.line = line;
                        result = Call(ClassOf(recv), recv, "Each", null, 0);
                    }
                    th.PopIter();
                }
                catch (eTagJump e)
                {
                    state = e.state;
                    if (_block.CheckTag(e.state))
                    {
                        state &= Tag.TAG.MASK;
                        if (state == Tag.TAG.RETURN)
                        {
                            Tag etag = th.CurrentTag;
                            result = etag.Result;
                        }
                    }
                    errorPrint(th);
                }
                catch (Exception ex)
                {
                    state = Tag.TAG.RAISE;
                    errorPrint(th, ex);
                }
                th.PopTag(false);
                if (_block.IsDynamic)
                {
                    _block.Orphan = true;
                }
                th.block = _block.prev;
                switch (state)
                {
                case Tag.TAG.EMPTY:
                    goto exit;
                case Tag.TAG.RETRY:
                    continue;
                case Tag.TAG.BREAK:
                    result = null;
                    goto exit;
                case Tag.TAG.RETURN: {
                    Tag etag = (Tag)th.protTag.Peek();
                    etag.Result = result;
                    goto default;
                }
                default:
                    th.TagJump(state);
                    break;
                }
            }
        exit:
            return result;
        }

        internal void ReturnCheck()
        {
            RThread th = GetCurrentContext();
            foreach (Tag tt in th.protTag)
            {
#if EXCEP_TRACE
                System.Console.WriteLine("TAG:" + tt.tag.ToString());
#endif
                if (tt == null) break;
                if (tt.tag == Tag.TAG.PROT_FUNC) break;
                if (tt.tag == Tag.TAG.PROT_THREAD)
                {
                    throw new eThreadError("return from within thread "
                                           + Thread.CurrentThread.ToString());
                }
            }
        }
        internal void SetReturnValue(object o)
        {
            RThread th = GetCurrentContext();
            th.CurrentTag.Result = o;
        }

        internal object ConditionalLoop(RNode node, object self, bool cond)
        {
            RThread th = GetCurrentContext();
            Tag.TAG state = Tag.TAG.EMPTY;
            th.PushTag(Tag.TAG.PROT_NONE);
            bool redo = false;
            bool next = false;
            for (;;)
            {
                try
                {
                    if (redo == false && next == false)
                    {
                        th.line = node.Line;
                        if (node.state && RBasic.RTest(Eval(self, node.cond)) != cond)
                        {
                            goto loop_out;
                        }
                    }
                    do
                    {
                        if (next == false)
                        {
                            Eval(self, node.body);
                        }
                        redo = next = false;
                    }
                    while (RBasic.RTest(Eval(self, node.cond)) == cond);
                    break;
                }
                catch (eTagJump e)
                {
                    state = e.state;
                }
                catch (Exception e)
                {
                    th.errInfo = new RException(this, e);
                    state = Tag.TAG.RAISE;
                }
                switch (state)
                {
                case Tag.TAG.REDO:
                    state = Tag.TAG.EMPTY;
                    next = false;
                    redo = true;
                    break;
                case Tag.TAG.NEXT:
                    state = Tag.TAG.EMPTY;
                    next = true;
                    redo = false;
                    break;
                case Tag.TAG.BREAK:
                    state = Tag.TAG.EMPTY;
                    goto loop_out;
                default:
                    goto loop_out;
                }
            }
        loop_out:
            th.PopTag(true);
            if (state != Tag.TAG.EMPTY)
            {
                th.TagJump(state);
            }
            return null;
        }
        
        internal object ModuleSetup(RMetaObject module, RNode n)
        {
            RThread th = GetCurrentContext();

            object result = null;
            string file = th.file;
            int line = th.line;
            Frame frm = th.DupFrame();
            RMetaObject cls = th.rClass;
            th.rClass = module;
            Scope.ScopeMode oldvmode = th.PushScope();
            RVarmap old = th.dyna_vars;
            th.dyna_vars = null;

            if (n.tbl != null)
            {
                object[] tbl = new object[n.tbl.Length + 1];
                tbl[0] = n;
                th.scope.InitLocal(n.tbl, tbl);
            }
            else
            {
                th.scope.local_tbl = null;
                th.scope.local_vars = null;
            }
            th.cRef = new RNCRef(module, th.cRef);
            frm.cBase = th.cRef;
            th.PushTag(Tag.TAG.PROT_NONE);
            int state = 0;
            try
            {
                if (TraceFunc != null)
                {
                }
                result = Eval(th.rClass, n.next);
            }
            catch (eTagJump tj)
            {
                //errat = null;
                state = (int)tj.state;
                errorPrint(th);
            }
            catch (Exception ex)
            {
                state = (int)Tag.TAG.RAISE;
                errorPrint(th, ex);
            }
            th.PopTag(true);

            th.cRef = (RNCRef)th.cRef.next;
            th.dyna_vars = old;
            th.PopScope(oldvmode);
            th.rClass = cls;
            th.frame = frm.tmp;

            if (TraceFunc != null)
            {
            }
            return result;
        }
*/        
        public RClass DefineClass(string name, RMetaObject super)
        {
            uint id = intern(name);
            if (cObject.IsConstDefined(id))
            {
                object o = cObject.ConstGet(id);
                if (o is RClass == false)
                {
                    throw new eTypeError(name + " is not a class");
                }
                RClass r = (RClass)o;
                if (r.super.ClassReal != super)
                {
                    throw new eNameError(name + "# is already defined");
                }
                return r;
            }
            if (super == null) super = cObject;
            RClass klass = new RClass(this, name, super);
            klass.klass = new RSingletonClass(super.klass);
            klass.klass.AttachSingleton(klass);
            Funcall(super, "inherited", klass);
            lock (class_tbl.SyncRoot)
            {
                class_tbl[id] = klass;
            }
            return klass;
        }
        internal void AddConstClass(RMetaObject obj, string name)
        {
            obj.klass = new RSingletonClass(cObject.klass);
            obj.klass.AttachSingleton(obj);
            class_tbl[intern(name)] = obj;
        }
        internal void DefineGlobalFunction(string name, MethodInfo mi)
        {
            mKernel.DefineModuleFunction(name, mi);
        }
        internal void DefineGlobalFunction(string name, RBasic.RMethod rm, int argc)
        {
            mKernel.DefineModuleFunction(name, rm, argc);
        }

        public void CallInit(object obj, object[] args)
        {
            RThread th = GetCurrentContext();
            ////th.iter.Push((IsBlockGiven) ? ITER.PRE : ITER.NOT);
            Funcall(obj, initialize, args);
            ////th.iter.Pop();
        }
        
        public object Funcall(object obj, string name, params object[] args)
        {
            return Call(ClassOf(obj), obj, name, args, 1);
        }
        public object Funcall(object obj, uint name, params object[] args)
        {
            return Call(ClassOf(obj), obj, name, args, 1);
        }
/*        
        public object FuncallRetry(object obj, params object[] args)
        {
            RThread th = GetCurrentContext();
            return Call(ClassOf(obj), obj, th.frame.lastFunc, args, 1);
        }
*/        
        internal void CheckMissingParam(object[] args)
        {
            if (args.Length == 0 || Symbol.IsSymbol(args[0]) == false)
            {
                throw new ArgumentException("no id given");
            }
        }
        internal object Missing(object obj, object[] args)
        {
            CheckMissingParam(args);
            RThread th = GetCurrentContext();
            CALLSTAT stat = th.lastCallStat;
            th.lastCallStat = CALLSTAT.PUBLIC;
        
            uint id = Symbol.SYM2ID((uint)args[0]);
            string format = null;
            string desc = "";
            int fmtcnt = 1;
            obj = InstanceOf(obj);
            if (obj == oNil)
            {
                format = "undefined method `{0}' for nil";
            }
            else if (obj is RBool)
            {
                if (obj == oTrue)
                    format = "undefined method `{0}' for true";
                else
                    format = "undefined method `{0}' for false";
            }
            else
            {
                if (obj is RBasic)
                    desc = ((RBasic)obj).Inspect().ToString();
                else
                    desc = obj.ToString();
        
                if ((stat & CALLSTAT.PRIVATE) == CALLSTAT.PRIVATE)
                    format = "private method `{0}' for {1}{2}{3}";
                else if ((stat & CALLSTAT.PROTECTED) == CALLSTAT.PROTECTED)
                    format = "protected method `{0}' for {1}{2}{3}";
                else if ((stat & CALLSTAT.VCALL) == CALLSTAT.VCALL)
                {
                    string mname = id2name(id);
                    if (('a' <= mname[0] && mname[0] <= 'z') || mname[0] == '_')
                    {
                        format = "undefined local variable or method `{0}' for {1}{2}{3}";
                    }
                }
                if (format == null)
                    format = "undefined method `{0}' for {1}{2}{3}";
                fmtcnt = 4;
            }

            ////th.PushFrame(true);
            string msg;
            if (fmtcnt == 1)
            {
                msg = String.Format(format, id2name(id));
            }
            else
            {
                msg = String.Format(format, id2name(id),
                                    desc, desc[0] == '#' ? "" : ":",
                                    desc[0] == '#' ? "" : ClassOf(obj).Name);
            }
                throw new eNameError(msg);
            // these line cause warning CS0162, so comment out it.
            // PopFrame();
            // return null;
        }
        internal object Undefined(object obj, uint mid, object[] args, CALLSTAT callstat)
        {
            RThread th = GetCurrentContext();
            th.lastCallStat = callstat;
            if (mid == missing)
            {
                ////th.PushFrame(false);
                Missing(obj, args);
                ////th.PopFrame();
            }
            object[] newargs = new object[args.Length + 1];
            Array.Copy(args, 0, newargs, 1, args.Length);
            newargs[0] = Symbol.ID2SYM(mid);
            return Funcall(obj, missing, newargs);
        }
        
        public RMetaObject CVarSingleton(object obj)
        {
            if (obj is RModule || obj is RClass) return (RMetaObject)obj;
            return ClassOf(obj);
        }
        public RMetaObject ClassOf(object obj)
        {
            if (obj == null || obj == oNil) return cNilClass;

            if (obj is bool)
            {
                return ((bool)obj) ? cTrueClass : cFalseClass;
            }
            if (obj is RBool)
            {
                return (obj == oTrue) ? cTrueClass : cFalseClass;
            }
            if (obj is int)
            {
                return cFixnum;
            }
            if (obj is double)
            {
                return cFloat;
            }
            if (obj is string)
            {
                return cString;
            }
            if (Symbol.IsSymbol(obj))
            {
                return cSymbol;
            }
            if (obj is RBasic)
            {
                return ((RBasic)obj).klass;
            }
            if (obj is ArrayList)
            {
                bug("ArrayList is no!");
            }
            ////Type tp = obj.GetType();
            ////return cDotNet.AddFrameworkClass(tp);
            return null;
        }
        public RBasic InstanceOf(object o)
        {
            if (o == null)
                return oNil;
            else if (o is int)
                return new RFixnum(this, (int)o);
            else if (o is double)
                return new RFloat(this, (double)o);
            else if (o is string)
                return new RString(this, (string)o);
            else if (Symbol.IsSymbol(o))
                return new Symbol(this, (uint)o);
            else if (o is bool)
            {
                return ((bool)o) ? (RBool)oTrue : (RBool)oFalse;
            }
            else if (o is RBasic)
                return (RBasic)o;

            /*
            Type tp = o.GetType();
            string cname = tp.ToString().Replace('.', '_');
            
            warning("Implicitly create DotNet class({0}) for `{1}'", cname, o.ToString());
            RFrmworkClass fc = (RFrmworkClass)cDotNet.ConstGet(intern(cname));
            if (fc == null)
                bug(cname + " not found");
            return fc.WrapObject(o);
            */
            return null;
        }
        public object CheckConvertType(object o, Type type, string tname, string method)
        {
            if (type.IsInstanceOfType(o)) return o;
            object result = null;
            try
            {
                result = Funcall(o, method, null);
            }
            catch
            {
                ;
            }
            if (type == typeof(RInteger))
            {
                if (result is int)
                    return new RFixnum(this, (int)result);
                else if (result is long)
                    return new RBignum(this, (long)result);
            }
            if (result != null && type.IsInstanceOfType(result) == false)
            {
                eTypeError ex = new eTypeError(String.Format("{0}#{1} should return {2}",
                                                             ClassOf(o).ClassName,
                                                             method, tname));
                throw ex;
            }
            return result;
        }
        public object ConvertType(object o, Type type, string tname, string method)
        {
            if (type.IsInstanceOfType(o)) return o;
            object result = null;
            try
            {
                result = Funcall(o, method, null);
            }
            catch (Exception e)
            {
                string s;
                if (o == null) s = "nil";
                else if (o is bool)
                    s = ((bool)o) ? "true" : "false";
                else
                    s = ClassOf(o).ClassName;
                eTypeError ex = new eTypeError(String.Format("failed to convert {0} into {1}",
                                                             s, tname), e);
                throw ex;
            }
            if (result is int
                && (type == typeof(RInteger) || type == typeof(RFixnum)))
            {
                return new RFixnum(this, (int)result);
            }
            if (result is long
                && (type == typeof(RInteger) || type == typeof(RBignum)))
            {
                return new RBignum(this, (long)result);
            }
            if (type.IsInstanceOfType(result) == false)
            {
                eTypeError ex = new eTypeError(String.Format("{0}#{1} should return {2}",
                                                             ClassOf(o).ClassName,
                                                             method, tname));
                throw ex;
            }
            return result;
        }
        
        public bool IsKindOf(object obj, RMetaObject klass)
        {
            RMetaObject o = ClassOf(obj);
            if (o is RModule || o is RClass)
            {
                while (o != null)
                {
                    if (o == klass || o.m_tbl == klass.m_tbl) return true;
                    o = o.super;
                }
                return false;
            }
            throw new eTypeError("class or module required");
        }
/*
        static private object[] dummyargs = new object[0];

        internal object Call0(RMetaObject klass, object recver, uint id, object[] args, RNode body, bool nosuper)
        {
            RThread th = GetCurrentContext();

            if (args == null) args = dummyargs;
            object result = null;
            ITER itr;
            switch (th.Iter)
            {
            case ITER.PRE:
                itr = ITER.CUR;
                break;
            case ITER.CUR:
            default:
                itr = ITER.NOT;
                break;
            }
            th.PushIter(itr);
            Frame frm = th.PushFrame(false);
            frm.lastFunc = id;
            frm.lastClass = (nosuper) ? null : klass;
            frm.self = recver;
            frm.args = args;

            result = body.Funcall(klass, recver, id, args);

            th.PopFrame();
            th.PopIter();
            return result;
        }
*/        
        internal object Call(RMetaObject klass, object recver, string name, object[] args, int scope)
        {
            return Call(klass, recver, intern(name), args, scope);
        }
        internal object Call(RMetaObject klass, object recver, uint id, object[] args, int scope)
        {
            // XXX
            string s = id2name(id);
            //Console.WriteLine("Call " + s);
            int len = args != null ? args.Length : 0;
            RBasic[] a = new RBasic[len];
            for(int i = 0; i < len; i++) {
                RBasic x = null;
                if(args[i] != null) x = args[i] as RBasic;
                if(x == null) x = oNil;
                a[i] = x;
            }
            return ((RBasic)recver).BasicSend(s, a, null);
        /*
            RThread th = GetCurrentContext();
            if (klass == null)
            {
                throw new NotImplementedException("method call on terminated object");
            }
            NOEX noex;
            int idx = CacheEntry.EXPR1(klass, id);
            RNode body;
            if (cache[idx].mid == id && cache[idx].klass == klass)
            {
                if (cache[idx].method == null)
                {
                    return Undefined(recver, id, args, (scope == 2) ? CALLSTAT.VCALL : CALLSTAT.PUBLIC);
                }
                klass = cache[idx].origin;
                id = cache[idx].mid0;
                noex = cache[idx].noex;
                body = cache[idx].method;
            }
            if ((body = klass.GetMethodBody(ref id, out klass, out noex)) == null)
            {
                if (scope == 3)
                {
                    throw new eNameError("super: no superclass method `" + id2name(id) + "'");
                }
                return Undefined(recver, id, args, (scope == 2) ? CALLSTAT.VCALL : 0);
            }
            if (id != missing)
            {
                if ((noex & NOEX.PRIVATE) != 0 && scope == 0)
                {
                    return Undefined(recver, id, args, CALLSTAT.PRIVATE);
                }
                if ((noex & NOEX.PROTECTED) != 0)
                {
                    RMetaObject defClass = klass;
                    while (defClass is RIncClass)
                    {
                        defClass = defClass.klass;
                    }
                    if (IsKindOf(th.frame.self, defClass) == false)
                    {
                        return Undefined(recver, id, args, CALLSTAT.PROTECTED);
                    }
                }
            }
            return Call0(klass, recver, id, args, body, ((noex & NOEX.UNDEF) != 0));
*/
        }
        internal object Call(RProc proc, object[] args)
        {
/*                    
            RThread th = GetCurrentContext();

            Block data = proc.block;
            int safe = th.safeLevel;
            RModule old_wrapper = th.wrapper;
        
            bool orphan = data.IsOrphan(th);
            th.wrapper = data.wrapper;
            Block old_block = th.block;
            Block _block = (Block)data.Clone();
            th.block = _block;

            th.PushIter(ITER.CUR);
            th.frame.iter = ITER.CUR;

            object result = null;
            th.PushTag(Tag.TAG.PROT_NONE);
            Tag.TAG state = Tag.TAG.EMPTY;
            try
            {
                th.safeLevel = proc.SafeLevel;
                result = Yield(args, null, null, true);
            }
            catch (eTagJump ej)
            {
                state = ej.state;
                if (th.block.CheckTag(ej.state))
                {
                    state &= Tag.TAG.MASK;
                }
            }
            catch (Exception e)
            {
                th.errInfo = new RException(this, e);
                state = Tag.TAG.RAISE;
            }
            th.PopTag(true);
            th.PopIter();

            th.block = old_block;
            th.wrapper = old_wrapper;
            th.safeLevel = safe;

            if (state != Tag.TAG.EMPTY)
            {
                switch (state)
                {
                case Tag.TAG.BREAK:
                    break;
                case Tag.TAG.RETRY:
                    throw new eLocalJumpError("retry from proc-closure");
                case Tag.TAG.RETURN:
                    if (orphan)
                    {
                        throw new eLocalJumpError("return from proc-closure");
                    }
                    goto default;
                default:
                    th.TagJump(state);
                    break;
                }
            }
            return result;
*/                    
            return null; //PH
        }
        internal uint ToID(object nm)
        {
            if (nm is string)
            {
                return intern((string)nm);
            }
            else if (nm is RString)
            {
                return intern(nm.ToString());
            }
            else if (Symbol.IsSymbol(nm))
            {
                return Symbol.SYM2ID((uint)nm);
            }
            else if (nm is int || nm is long || nm is uint)
            {
                if (id2name((uint)nm) == null)
                {
                    throw new ArgumentException(nm.ToString() + " is not a symbol");
                }
                return (uint)nm;
            }
            throw new eTypeError(Funcall(nm, "inspect", null).ToString() + " is not a symbol");
        }
        public bool RespondTo(object obj, uint id)
        {
            return IsMethodBound(ClassOf(obj), id, false);
        }
        public bool Equal(object obj1, object obj2)
        {
            if (obj1 == obj2) return true;
            object o = Call(ClassOf(obj1), obj1, idEq, new object[1] {obj2}, 1);
            return RBasic.RTest(o);
        }
        public bool Eql(object obj1, object obj2)
        {
            object o = Call(ClassOf(obj1), obj1, idEql, new object[1] {obj2}, 1);
            return RBasic.RTest(o);
        }
        // ex means accesible check
        internal bool IsMethodBound(RMetaObject klass, uint id, bool ex)
        {
        /*        
            int idx = CacheEntry.EXPR1(klass, id);
            if (cache[idx].mid == id && cache[idx].klass == klass)
            {
                if (ex && (cache[idx].noex & NOEX.PRIVATE) != 0)
                {
                    return false;
                }
                if (cache[idx].method == null)
                {
                    return false;
                }
                return true;
            }
            NOEX noex;
            if (klass.GetMethodBody(ref id, out klass, out noex) != null)
            {
                if (ex && (noex & NOEX.PRIVATE) != 0)
                    return false;
                return true;
            }
            */                        
            return false;
        }
        //
        //
        internal void bug(string s)
        {
            warn(s);

            Assembly asm = Assembly.GetAssembly(GetType());
            FileVersionInfo finfo = FileVersionInfo.GetVersionInfo(
                                           asm.Location);
            string version = finfo.FileVersion;
            string date = String.Empty;
            string platform = String.Empty;
            System.Console.Error.WriteLine("NETRuby {0} ({1}) [{2}]",
                                           version, date, platform);
            Process.GetCurrentProcess().Kill();
        }
        internal void warn(string s, object arg)
        {
            warn(string.Format(s, arg));
        }
        internal void warn(string s)
        {
            warnPrint(s);
        }
        internal void warning(string s, object arg)
        {
            warning(string.Format(s, arg));
        }
        internal void warning(string s, params object[] args)
        {
            warning(string.Format(s, args));
        }
        internal void warning(string s)
        {
            if (verbose)
            {
                warnPrint(s);
            }
        }
        internal void warnPrint(string s)
        {
            RThread th = GetCurrentContext();
            StringBuilder sb = new StringBuilder(s.Length + 128);
            if (th.file != null)
            {
                if (th.line == 0)
                    sb.AppendFormat("{0}: ", th.file);
                else
                    sb.AppendFormat("{0}:{1}: ", th.file, th.line);
            }
            sb.AppendFormat("warning: {0}", s);
            System.Console.Error.WriteLine(sb.ToString());
        }
        
        private void compileError(string s)
        {
            RThread th = GetCurrentContext();
            th.ClearCompileError();
            StringBuilder str = new StringBuilder("compile error");
            if (s != null)
            {
                str.AppendFormat(" in {0}", s);
            }
            str.Append('\n');
            if (th.errInfo != null)
            {
                str.Append(th.errInfo.ToString());
            }
            throw new eTagJump(new RException(this, str.ToString(), eSyntaxError));
        }
/*
        internal object SpecificEval(RMetaObject klass, RBasic self, object[] argv)
        {
            RThread th = GetCurrentContext();
            if (th.IsBlockGiven)
            {
                if (argv.Length > 0)
                {
                    throw new eArgError(String.Format("wrong # of arguments ({0} for 0)",
                                                      argv.Length));
                }
                return Yield(th, klass, self);
            }
            else
            {
                string file = "(eval)";
                int line = 1;
                int argc = argv.Length;
                if (argc == 0)
                    throw new eArgError("block not supplied");
                CheckSafeString(th, argv[0]);
                if (argc > 3)
                    throw new eArgError(String.Format("wrong # of arguments : {0}(src) or {1}(..)",
                                                      id2name(th.frame.lastFunc),
                                                      id2name(th.frame.lastFunc)));
                if (argc > 1) file = argv[1].ToString();
                if (argc > 2) line = RInteger.ToInt(this, argv[2]);

                return Eval(th, klass, self, argv[0], file, line);
            }
        }
        private object EvalNode(object self, RNode node, RThread th)
        {
            RNode beg = th.evalTreeBegin;
            th.evalTreeBegin = null;
            if (beg != null)
            {
                Eval(self, beg);
            }
            if (node == null) return null;

            return Eval(self, node);
        }

        internal object ruby_eval(RBasic r, params object[] args)
        {
            string file = "(eval)";
            string src = String.Empty;
            int line = 1;
            object[] argv = new object[4];
            ScanArgs(args, "13", argv);
            if (args.Length >= 3)
            {
                if (argv[2] is string)
                {
                    file = (string)argv[2];
                }
                else
                {
                    CheckType(argv[2], typeof(RString));
                    file = ((RString)argv[2]).ToString();
                }
            }
            if (args.Length >= 4)
            {
                line = RInteger.ToInt(this, argv[3]);
            }
            Scope scope = null;
            if (argv[1] != null)
            {
                CheckType(argv[1], typeof(Scope));
                scope = (Scope)argv[1];
            }
            RThread th = GetCurrentContext();
            if (argv[0] is string)
            {
                src = (string)argv[0];
            }
            else
            {
                if (th.safeLevel >= 4)
                {
                    CheckType(argv[0], typeof(RString));
                    if (scope != null && scope.IsTainted == false)
                        throw new SecurityException("Insecure: can't modify trusted binding");
                }
                else
                {
                    CheckSafeString(th, argv[0]);
                }
                src = argv[0].ToString();
            }
            if (scope == null && th.frame.prev != null)
            {
                lock (th)
                {
                    Frame pv = th.frame;
                    Frame fm = th.PushFrame(true);
                    fm.prev = pv;
                }
                object val = Eval(r, src, scope, file, line);
                th.PopFrame();
                return val;
            }
            return Eval(r, src, scope, file, line);
        }
*/
/*        
        internal object ruby_block_given_p(RBasic r, params object[] args)
        {
            RThread th = GetCurrentContext();
            return th.IsPriorBlockGiven;
        }
        internal object ruby_loop(RBasic r, params object[] args)
        {
            for (;;)
            {
                Yield(null, null, null, false);
            }
        }
*/        
        internal object ruby_raise(RBasic r, params object[] args)
        {
            RBasic msg = null;
            switch (args.Length)
            {
            case 0:
                break;
            case 1:
                if (args[0] == null) break;
                if (args[0] is string || args[0] is RString)
                {
                    msg = new RException(this, args[0].ToString(), eRuntimeError);
                    break;
                }
                msg = (RBasic)Funcall(args[0], "exception");
                break;
            case 2:
            case 3:
                msg = (RBasic)Funcall(args[0], "exception", args[1]);
                break;
            default:
                throw new eArgError("wrong # of arguments");
            }
            if (args.Length > 0)
            {
                if (msg.IsKindOf(eException) == false)
                    throw new eTypeError("exception object expected");
                RException.exc_set_backtrace(msg, (args.Length > 2) ? args[2] : null);
            }
            /*
            RThread th = GetCurrentContext();
            if (th.frame != topFrame)
            {
                th.PushFrame(true);
            }
            */
            throw new eTagJump((RException)msg);
        }
        /*
        internal object ruby_caller(RBasic r, params object[] args)
        {
            int level = 1;
            if (args.Length >= 1)
                level = RInteger.ToInt(this, args[0]);
            if (level < 0)
                throw new eArgError(String.Format("negative level({0})", level));
            return Backtrace(level);
        }
        */
        internal object ruby_exit(RBasic r, params object[] args)
        {
            ////Secure(4);
            int stat = 0;
            if (args.Length > 0)
                stat = RInteger.ToInt(this, args[0]);
            Exit(1);
            return null;
        }

        //
        public void Exit(int stat)
        {
            RThread th = GetCurrentContext();
/*            
            if (th.protTag.Count > 1)
            {
                exitStatus = stat;
                RException ex = new RException(this, eSystemExit);
                ex.IVarSet("status", stat);
                throw new eTagJump(ex);
            }
*/            
            Dispose();
            Environment.Exit(stat);
        }
        internal object ruby_abort(RBasic r, params object[] args)
        {
            RThread th = GetCurrentContext();
            ////th.Secure(4);
            errorPrint(th);
            Exit(1);
            return null;
        }
        public RArray Backtrace(int lev)
        {
/*        
            RThread th = GetCurrentContext();
            Frame frm = th.frame;
            string buf;
*/            
            RArray ary = new RArray(this, true);
/*            
            if (lev < 0)
            {
                if (frm.lastFunc != 0)
                {
                    buf = String.Format("{0}:{1}:in `{2}'",
                                        th.file, th.line, id2name(frm.lastFunc));
                }
                else if (th.line == 0)
                {
                    buf = th.file;
                }
                else
                {
                    buf = String.Format("{0}:{1}", th.file, th.line);
                }
                ary.Add(buf);
            }
            else
            {
                while (lev-- > 0)
                {
                    frm = frm.prev;
                    if (frm == null)
                    {
                        ary = null;
                        break;
                    }
                }
            }
            while (frm != null && frm.file != null)
            {
                if (frm.prev != null && frm.prev.lastFunc != 0)
                {
                    buf = String.Format("{0}:{1}:in `{2}'",
                                        frm.file, frm.line, id2name(frm.prev.lastFunc));
                }
                else
                {
                    buf = String.Format("{0}:{1}", frm.file, frm.line);
                }
                ary.Add(buf);
                frm = frm.prev;
            }
        */
            return ary;        
        }
/*        
        internal object Eval(object self, RNode n)
        {
#if EVAL_DEBUG        
            System.Console.WriteLine("Entering Eval");
#endif        
            object o = null;
            for (RNode node = n; node != null; )
            {
#if EVAL_DEBUG
                System.Console.WriteLine("Node:" + node.GetType());
                RNode prev = node;
#endif

                node = node.Eval(this, self, out o);

#if EVAL_DEBUG        
                System.Console.WriteLine("Leaving Eval(" + prev.ToString() + "), result=" + ((o == null) ? "null" : o.ToString()));
#endif
            }
            return o;
        }
        internal object Eval(object self, string src, RBasic scope, string file, int line)
        {
            RThread th = GetCurrentContext();

            object result = null;
            string filesave = th.file;
            int linesave = th.line;
            ITER itr = th.frame.iter;
            Frame frm = null;
            Scope old_scope = null;
            Block old_block = null;
            RVarmap old_dyna_vars = null;
            Scope.ScopeMode old_vmode = th.ScopeMode;
            RNCRef old_cref = null;
            RModule old_wrapper = null;
            Block data = null;
            if (scope != null)
            {
                if (scope is RProc == false)
                    throw new eTypeError(String.Format("wrong argument type {0} (expected Proc/Binding)",
                                                       ClassOf(scope).Name));
                data = ((RProc)scope).block;
                frm = data.frame;
                frm.tmp = th.frame;
                th.frame = frm;
                old_scope = th.scope;
                old_block = th.block;
                th.block = data.prev;
                old_dyna_vars = th.dyna_vars;
                th.dyna_vars = data.dyna_vars;
                old_vmode = th.ScopeMode;
                old_cref = th.cRef;
                th.cRef = frm.cBase;
                old_wrapper = th.wrapper;
                th.wrapper = data.wrapper;
                if ((file == null || file == String.Empty || (line == 1 && file == "(eval)"))
                    && data.body != null && data.body.File != null)
                {
                    file = data.body.File;
                    line = data.body.Line;
                }
                self = data.self;
                th.frame.iter = data.iter;
            }
            else
            {
                if (th.frame.prev != null)
                {
                    th.frame.iter = th.frame.prev.iter;
                }

            }
            if (file == null)
            {
                file = th.file;
                line = th.line;
            }

            RMetaObject cls = th.rClass;
            th.rClass = th.cBase;

            th.EnterEval();
            if (th.rClass is RIncClass)
                th.rClass = th.rClass.klass;
            Tag.TAG state = Tag.TAG.EMPTY;
            th.PushTag(Tag.TAG.PROT_NONE);
            try
            {
                RNode node = CompileFile(file, new StringReader(src), line, th);
                if (th.CompileFailed)
                {
                    compileError(null);
                }

                result = EvalNode(self, node, th);
            }
            catch (eTagJump tj)
            {
#if _DEBUG
                System.Console.WriteLine(tj.Message);
                System.Console.WriteLine(tj.StackTrace);
#endif
                state = tj.state;
            }
            catch (Exception e)
            {
#if _DEBUG
                System.Console.WriteLine(e.Message);
                System.Console.WriteLine(e.StackTrace);
#endif
                state = Tag.TAG.RAISE;
                th.errInfo = new RException(this, e);
            }
            th.PopTag(true);
            th.rClass = cls;
            th.LeaveEval();
        
            if (th.scope != null)
            {
                th.wrapper = old_wrapper;
                th.cRef = old_cref;
                th.frame = frm.tmp;
                th.scope = old_scope;
                th.block = old_block;
                th.dyna_vars = old_dyna_vars;
                th.ScopeSet(old_vmode);
            }
            else
            {
                th.frame.iter = itr;
            }
            th.file = filesave;
            th.line = linesave;

            if (state != Tag.TAG.EMPTY)
            {
                if (state == Tag.TAG.RAISE)
                {
                    RArray errat = null;
                    string msg = th.errInfo.ToString();
                    string err = msg;
                    if (file == "(eval)")
                    {
                        if (th.line > 1)
                        {
                            errat = th.errInfo.Backtrace;
                            if (errat != null)
                            {
                                err = String.Format("{0}: {1}", errat[0], msg);
                            }
                        }
                        ruby_raise((RException)RException.exc_new(ClassOf(th.errInfo), err));
                    }
                    ruby_raise(th.errInfo);
                }
                th.TagJump(state);
            }
            return result;
        }
        
        public object EvalString(string str)
        {
            bool newThread;
            RThread th = GetCurrentContext(out newThread);

            string oldsrc = th.file;
            th.file = "(eval)";
            object o = Eval(topSelf, str, null, null, 0);
            th.file = oldsrc;
            if (newThread)
            {
                ClearContext(th);
            }
            if (th.errInfo != null)
            {
#if _DEBUG
                if (th.errInfo.InnerException != null)
                    System.Console.Error.WriteLine(th.errInfo.InnerException.StackTrace);
#endif
                throw new NetRubyException(th.errInfo);
            }
            return o;
        }
*/        
        void Start(string filename, bool print)
        {
            StreamReader sr = new StreamReader(File.OpenRead(filename));
            
            RNode n = CompileFile(filename, sr, 0, GetCurrentContext());
           
            if(print) {
                BlockPrinter p = new BlockPrinter(this, Console.Out);
                p.Write(n);
                Console.WriteLine("");
            } else {
                RThread th = GetCurrentContext();
                EmitContext ec = new EmitContext(this, "ruby_program", "ruby.out.dll");
                RCMethod m = ec.Compile(n);
                th.PushClassScope(topSelf.klass);
                m.Call(th, topSelf, new RBasic[] {}, null);
            }
            // Flush the console
            Console.WriteLine("");
        }

        public static void Main(string[] args)
        {
            int i = 0;
            string filename = null;
            bool print = false;
            
            try {
                if(args[i] == "-p") {
                    print = true;
                    i++;
                }

                filename = args[i];
            } catch(IndexOutOfRangeException e) {
                Console.WriteLine("Usage:");
                Console.WriteLine("netruby.exe [-p] <filename>");
                return;
            }
        
            NetRuby rb = new NetRuby();
            rb.Init();
            rb.Start(filename, print);
            
            /*
            if (rb.Options(args))
            {
                rb.Run();
            }
            */
        }
    }
}

// vim:et:sts=4:sw=4
