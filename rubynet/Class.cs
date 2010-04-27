/*
Copyright (C) 1993-2000 Yukihiro Matsumoto
Copyright (C) 2001-2002 arton
Copyright (C) 2005      Jaen Saul
*/

using System;
using System.Collections;
using System.Reflection;
using System.Security;

namespace NETRuby
{
    public abstract class RMetaObject : RObjectBase
    {
        internal RMetaObject(NetRuby rb, string name, RMetaObject sp, RMetaObject meta)
            : base(rb, meta)
        {
            super = sp;
            m_tbl = new st_table();
            if (name != null)
            {
                uint id = rb.intern(name);
                IVarSet("__classid__", Symbol.ID2SYM(id));
            }
        }
        internal RMetaObject(RMetaObject o) :
            base(o)
        {
            if (o.m_tbl != null)
            {
                m_tbl = (st_table)o.m_tbl.Clone();
            }
        }
        internal RMetaObject super;
        internal st_table m_tbl;

        public override string ToString()
        {
            return ClassPath;
        }

        public void Attribute(uint id, bool read, bool write, bool ex)
        {
            string s = ruby.id2name(id);
            if (s == null)
                throw new eArgError("argument needs to be symbol or string");
            Attribute(s, read, write, ex);
        }
        public void Attribute(string name, bool read, bool write, bool ex)
        {
            NOEX noex = NOEX.PUBLIC;
            RThread th = ruby.GetCurrentContext();
            if (ex)
            {
                if (th.ScopeTest(Scope.ScopeMode.Private))
                {
                    noex = NOEX.PRIVATE;
                    ruby.warning((th.ScopeMode == Scope.ScopeMode.ModFunc) ?
                                 "attribute accessor as module_function" :
                                 "private attribute?");
                }
                else if (th.ScopeTest(Scope.ScopeMode.Protected))
                {
                    noex = NOEX.PROTECTED;
                }
            }
            uint id = ruby.ToID(name);
            uint attriv = ruby.intern("@" + name);
            if (read)
            {
                ////addMethod(id, new RNIVar(th, attriv), noex);
                ruby.Funcall(this, "method_added", Symbol.ID2SYM(id));
            }
            if (write)
            {
                id = ruby.intern(name + "=");
                ////addMethod(id, new RNAttrSet(th, attriv), noex);
                ruby.Funcall(this, "method_added", Symbol.ID2SYM(id));
            }
        }
        public void DefineConst(string name, object val)
        {
            uint id = ruby.intern(name);
            ////if (this == ruby.cObject) ruby.Secure(4);
            if (Parser.is_const_id(id) == false)
                throw new eNameError("wrong constant name " + name);
            ConstSet(id, val);
        }
        public void ConstSet(uint id, object val)
        {
            valSet(id, val, true);
        }
        
        public object ConstGet(uint id)
        {
            bool fretry = false;
            RMetaObject klass = this;
        retry:
            while (klass != null)
            {
                object o;
                if (klass.iv_tbl != null && klass.iv_tbl.lookup(id, out o))
                {
                    return o;
                }
                if (klass == ruby.cObject && ruby.TopConstGet(id, out o))
                {
                    return o;
                }
                klass = klass.super;
            }
            if (fretry == false && this is RModule)
            {
                fretry = true;
                klass = ruby.cObject;
                goto retry;
            }
            if (this != ruby.cObject)
            {
                string s = String.Format("uninitialized constant {0} at {1}",
                                         ruby.id2name(id), ClassPath);
#if _DEBUG
                System.Console.Error.WriteLine(s);
#endif        
                throw new eNameError(s);
            }
            throw new eNameError("uninitialized constant " + ruby.id2name(id));
        }

        public object ConstGetAt(uint id)
        {
            object o;
            if (iv_tbl != null && iv_tbl.lookup(id, out o))
            {
                return o;
            }
            if (this == ruby.cObject && ruby.TopConstGet(id, out o))
            {
                return o;
            }
            throw new eNameError(String.Format("uninitialized constant {0}::{1}",
                                               ClassPath, ruby.id2name(id)));
        }

        public bool IsConstDefinedAt(uint id)
        {
            if (iv_tbl != null && iv_tbl.ContainsKey(id))
                return true;
            if (this == ruby.cObject)
                return IsConstDefined(id);
            return false;
        }
        public bool IsConstDefined(uint id)
        {
            RMetaObject kclass = this;

            while (kclass != null)
            {
                if (kclass.iv_tbl != null && kclass.iv_tbl.ContainsKey(id))
                {
                    return true;
                }
                kclass = kclass.super;
            }
            if (this is RModule)
            {
                return ruby.cObject.IsConstDefined(id);
            }
            if (ruby.class_tbl.ContainsKey(id)) return true;
            ////return ruby.IsAutoloadDefined(id);
            return false;
        }
        public object RemoveConst(uint id)
        {
            if (Parser.is_const_id(id) == false)
                throw new eNameError("wrong constant name " + ruby.id2name(id));
/*                
            if (IsTainted == false && ruby.SafeLevel >= 4)
                throw new SecurityException("Insecure: can't remove constant");
*/                
            if (IsFrozen) ruby.ErrorFrozen("class/module");
            object o;
            if (iv_tbl != null && iv_tbl.delete(id, out o)) return o;
            if (IsConstDefinedAt(id))
                throw new eNameError(String.Format("cannot remove {0}::{1}",
                                                   Name, ruby.id2name(id)));
            throw new eNameError(String.Format("constant {0}::{1} not defined",
                                               Name, ruby.id2name(id)));
        }
        public void CVarDeclare(uint id, object val)
        {
            try
            {
                CVarSet(id, val);
            }
            catch (eNameError)
            {
                valSet(id, val, false);
            }
        }
        public object CVarGet(uint id)
        {
            RMetaObject tmp = this;
            object result = null;
            while (tmp != null)
            {
                if (tmp.iv_tbl != null && tmp.iv_tbl.lookup(id, out result))
                    return result;
                tmp = tmp.super;
            }
            throw new eNameError(String.Format("uninitialized class variable {0} in {1}",
                                 ruby.id2name(id), ClassName));
        }
        public void CVarSet(uint id, object val)
        {
            RMetaObject tmp = this;
            while (tmp != null)
            {
                lock (tmp.iv_tbl.SyncRoot)
                {
                    if (tmp.iv_tbl != null && tmp.iv_tbl.ContainsKey(id))
                    {
                        tmp.CVarCheck("class variable");
                        tmp.iv_tbl[id] = val;
                        return;
                    }
                }
                tmp = tmp.super;
            }
            throw new eNameError("uninitialized class variable " + ruby.id2name(id) + " in " + ClassName);
        }
        void valSet(uint id, object val, bool isconst)
        {
            string dest = (isconst) ? "constant" : "class variable";
            CVarCheck(dest);
            lock (this)
            {
                if (iv_tbl == null)
                {
                    iv_tbl = new st_table();
                }
                else if (isconst)
                {
                    if (iv_tbl.ContainsKey(id) ||
                        (this == ruby.cObject && ruby.class_tbl.ContainsKey(id)))
                    {
                        ruby.warn(String.Format("already initialized {0} {1}",
                                                dest, ruby.id2name(id)));
                    }
                }
                iv_tbl[id] = val;
            }
        }
        protected void CVarCheck(string dest)
        {
/*        
            if (IsTainted == false && ruby.SafeLevel >= 4)
            {
                throw new SecurityException("Insecure: can't set " + dest);
            }
*/            
            if (IsFrozen) ruby.ErrorFrozen("class/module");
        }

        public virtual string Name // class2name
        {
            get { return ClassPath; }
        }
        public string ModuleName
        {
            get {
                string s = ClassPath;
                if (s != null) return s;
                return String.Empty;
            }
        }
        public virtual RMetaObject ClassReal
        {
            get {
                RMetaObject cl;
                if (this is RIncClass)
                {
                    cl = klass;
                }
                else
                {
                    cl = this;
                }
                while (cl is RIncClass || cl is RSingletonClass)
                {
                    cl = cl.super;
                }
                return cl;
            }
        }
        public string ClassPath
        {
            get {
                string s = ClassName;
                if (s != null) return s;
                s = "Class";
                if (this is RModule) s = "Module";
                return String.Format("#<{0} 0lx{1:x8}>", s, GetHashCode());
            }
        }
        public string ClassName
        {
            get {
                object path = null;
                uint cp = ruby.intern("__classpath__");
                RMetaObject klass = ClassReal;
                if (klass == null) klass = ruby.cObject;
                lock (klass)
                {
                    if (klass.iv_tbl != null &&
                        klass.iv_tbl.lookup(cp, out path) == false)
                    {
                        uint cid = ruby.intern("__classid__");
                        if (klass.iv_tbl.lookup(cid, out path))
                        {
                            path = ruby.id2name(Symbol.SYM2ID((uint)path));
                            klass.iv_tbl[cp] = path;
                            klass.iv_tbl.Remove(cid);
                        }
                    }
                }
                if (path == null)
                {
                    path = klass.FindClassPath();
                    if (path == null) return String.Empty;
                    return (string)path;
                }
                if (path is string == false)
                {
                    ruby.bug("class path is not set properly");
                }
                return (string)path;
            }
        }

        internal void SetClassPath(RMetaObject under, string name)
        {
            string str = name;
            if (under != ruby.cObject)
            {
                str = under.ClassPath;
                str += "::" + name;
            }
            IVarSet("__classpath__", str);
        }
        private class fc_result
        {
            internal fc_result(uint key, RObjectBase kl, RObjectBase value, fc_result pv)
            {
                name = key;
                path = null;
                klass = kl;
                track = value;
                prev = pv;
            }
            internal bool end(RObjectBase o) { return (o == track); }
            internal bool lookup(uint id, out string val)
            {
                val = null;
                object o;
                if (track.iv_tbl != null && track.iv_tbl.lookup(id, out o))
                {
                    val = (string)o;
                    return true;
                }
                return false;
            }
            internal uint name;
            internal RObjectBase klass;
            internal string path;
            RObjectBase track;
            internal fc_result prev;
        }
        private string fc_path(fc_result fc, uint name)
        {
            string path = ruby.id2name(name);
            string tmp;
            uint cp = ruby.intern("__classpath__");
            while (fc != null)
            {
                if (fc.end(ruby.cObject)) break;
                if (fc.lookup(cp, out tmp))
                {
                    return tmp + "::" + path;
                }
                tmp = ruby.id2name(fc.name);
                path = tmp + "::" + path;
                fc = fc.prev;
            }
            return path;
        }
        private bool fc_i(uint key, object value, fc_result res)
        {
            if (Parser.is_const_id(key)) return false;
            if (value is RModule || value is RClass)
            {
                RMetaObject va = (RMetaObject)value;
                if (va == res.klass)
                {
                    res.path = fc_path(res, key);
                    return true;
                }
                if (va.iv_tbl == null) return false;
                fc_result list = res;
                while (list != null)
                {
                    if (list.end(va)) return false;
                    list = list.prev;
                }
                fc_result arg = new fc_result(key, res.klass, va, res);
                lock (va.iv_tbl.SyncRoot)
                {
                    foreach (DictionaryEntry ent in va.iv_tbl)
                    {
                        if (fc_i((uint)ent.Key, ent.Value, arg))
                        {
                            res.path = arg.path;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        internal object FindClassPath()
        {
            fc_result arg = new fc_result(0, this, ruby.cObject, null);
            if (ruby.cObject.iv_tbl != null)
            {
                lock (ruby.cObject.iv_tbl.SyncRoot)
                {
                    foreach (DictionaryEntry entry in ruby.cObject.iv_tbl)
                    {
                        if (fc_i((uint)entry.Key, entry.Value, arg)) break;
                    }
                }
            }
            if (arg.path == null)
            {
                lock (ruby.class_tbl.SyncRoot)
                {
                    foreach (DictionaryEntry entry in ruby.class_tbl)
                    {
                        if(fc_i((uint)entry.Key, entry.Value, arg)) break;
                    }
                }
            }
            if (arg.path != null)
            {
                lock (this)
                {
                    if (iv_tbl == null) iv_tbl = new st_table();
                    iv_tbl[ruby.intern("__classpath__")] = arg.path;
                }
            }
            return (string)arg.path;
        }

        public static void ExtendObject(RBasic klass, RMetaObject module)
        {
            IncludeModule(SingletonClass(klass, klass.ruby), module);
        }
        internal static object extend_object(RBasic r, params object[] args)
        {
            ExtendObject((RMetaObject)args[0], (RMetaObject)r);
            return r;
        }
        public static void IncludeModule(RMetaObject klass, RMetaObject module)
        {
            bool changed = false;
            if (module == null) return;
            if (module == klass) return;
            // call Check_Type if need

            // what is the syncroot ?
            RMetaObject c = klass;
            while (module != null)
            {
                for (RMetaObject r = klass.super; r != null; r = r.super)
                {
                    if (r is RIncClass &&
                        r.m_tbl == module.m_tbl)
                    {
                        c = r;
                        goto skip;
                    }
                }
                c.super = new RIncClass(module, c.super);
                c = c.super;
                changed = true;
            skip:
                module = module.super;
            }
            ////if (changed) klass.ruby.ClearCache();
        }

        private class InsMethods
        {
            internal virtual string Inspect(NetRuby ruby, uint key, RNode body)
            {
                if ((body.noex & (NOEX.PRIVATE | NOEX.PROTECTED)) == 0)
                {
                    if (body.body != null)
                        return ruby.id2name(key);
                }
                return null;
            }
        }
        private class InsMethodsProtected : InsMethods
        {
            internal override string Inspect(NetRuby ruby, uint key, RNode body)
            {
                if (body.body != null && (body.noex & (NOEX.PROTECTED)) != 0)
                {
                    return ruby.id2name(key);
                }
                return null;
            }
        }
        private class InsMethodsPrivate : InsMethods
        {
            internal override string Inspect(NetRuby ruby, uint key, RNode body)
            {
                if (body.body != null && (body.noex & (NOEX.PRIVATE)) != 0)
                {
                    return ruby.id2name(key);
                }
                return null;
            }
        }
        public override RArray SingletonMethods
        {
            get {
                ArrayList ary = new ArrayList();
                RMetaObject klass = this;
                InsMethods ins = new InsMethods();
                while (klass != null && klass.Test(FL.SINGLETON))
                {
                    lock (klass.m_tbl.SyncRoot)
                    {
                        foreach (DictionaryEntry ent in klass.m_tbl)
                        {
                            string s = ins.Inspect(ruby, (uint)ent.Key, (RNode)ent.Value);
                            if (s != null && ary.Contains(s) == false)
                            {
                                ary.Add(s);
                            }
                        }
                    }
                    klass = klass.super;
                }
                return new RArray(ruby, ary);
            }
        }
        public virtual RArray ClassInstanceMethods(object[] argv)
        {
            object[] inherited_too = new object[1];
            ruby.ScanArgs(argv, "01", inherited_too);
            return MethodList(RTest(inherited_too[0]), new InsMethods());
        }
        public virtual RArray ClassProtectedInstanceMethods(object[] argv)
        {
            object[] inherited_too = new object[1];
            ruby.ScanArgs(argv, "01", inherited_too);
            return MethodList(RTest(inherited_too[0]), new InsMethodsProtected());
        }
        public virtual RArray ClassPrivateInstanceMethods(object[] argv)
        {
            object[] inherited_too = new object[1];
            ruby.ScanArgs(argv, "01", inherited_too);
            return MethodList(RTest(inherited_too[0]), new InsMethodsPrivate());
        
        }
        private RArray MethodList(bool inherited_too, InsMethods ins)
        {
            ArrayList ary = new ArrayList();
            lock (this)
            {
                for (RMetaObject klass = this; klass != null; klass = klass.super)
                {
                    foreach (DictionaryEntry entry in klass.m_tbl)
                    {
                        ////string s = ins.Inspect(ruby, (uint)entry.Key, (RNode)entry.Value);
                        RCMethod m = (RCMethod)entry.Value;
                        string s = m.Name;
                        if (s != null && ary.Contains(s) == false)
                        {
                            ary.Add(s);
                        }
                    }
                    if (inherited_too == false) break;
                }
            }
            return new RArray(ruby, ary);
        }
        public object AppendFeatures(object o)
        {
            
            if (o is RMetaObject == false)
            {
                ruby.CheckType(o, typeof(RClass));
            }
            
            IncludeModule((RMetaObject)o, this);
            return this;
        }
        public object Include(object[] args)
        {
            uint id = ruby.intern("append_features");
            for (int i = 0; i < args.Length; i++)
            {
                ruby.CheckType(args[i], typeof(RModule));
                ruby.Funcall(args[i], id, this);
            }
            return this;
        }
        private void ExportMethod(uint name, NOEX noex)
        {
/*        
            if (this == ruby.cObject)
            {
                ruby.Secure(4);
            }
            RMetaObject origin;
            RNode body = SearchMethod(name, out origin);
            if (body == null && this is RModule)
            {
                body = ruby.cObject.SearchMethod(name, out origin);
            }
            if (body == null)
            {
                printUndef(name);
            }
            if (body.noex != noex)
            {
                if (this == origin)
                {
                    body.noex = noex;
                }
                else
                {
                    ruby.ClearCache(name);
                    addMethod(name, new RNZSuper(ruby.GetCurrentContext(), ruby), noex);
                }
            }
*/                        
        }
        private void SetMethodVisibility(object[] args, NOEX ex)
        {
            ////ruby.SecureVisibility(this);
            for (int i = 0; i < args.Length; i++)
            {
                ExportMethod(ruby.ToID(args[i]), ex);
            }
        }
        public object Public(params object[] args)
        {
            ////ruby.SecureVisibility(this);
            RThread th = ruby.GetCurrentContext();
            if (args == null || args.Length == 0)
            {
                th.ScopeSet(Scope.ScopeMode.Public);
            }
            else
            {
                SetMethodVisibility(args, NOEX.PUBLIC);
            }
            return this;
        }
        public object Protected(params object[] args)
        {
            ////ruby.SecureVisibility(this);
            RThread th = ruby.GetCurrentContext();
            if (args == null || args.Length == 0)
            {
                th.ScopeSet(Scope.ScopeMode.Protected);
            }
            else
            {
                SetMethodVisibility(args, NOEX.PROTECTED);
            }
            return this;
        }
        public object Private(params object[] args)
        {
            ////ruby.SecureVisibility(this);
            RThread th = ruby.GetCurrentContext();
            if (args == null || args.Length == 0)
            {
                th.ScopeSet(Scope.ScopeMode.Private);
            }
            else
            {
                SetMethodVisibility(args, NOEX.PRIVATE);
            }
            return this;
        }
        public RMetaObject ModuleFunction(params object[] argv)
        {
/*        
            if (this is RModule == false)
                throw new eTypeError("module_function must be called for modules");
            ////ruby.SecureVisibility(this);
            RThread th = ruby.GetCurrentContext();
            if (argv == null || argv.Length == 0)
            {
                th.ScopeSet(Scope.ScopeMode.ModFunc);
                return this;
            }
            SetMethodVisibility(argv, NOEX.PRIVATE);
            for (int i = 0; i < argv.Length; i++)
            {
                uint id = ruby.ToID(argv[i]);
                RMetaObject origin;
                RNode body = SearchMethod(id, out origin);
                if (body == null || body.body == null)
                {
                    ruby.bug(String.Format("undefined method `{0}'; can't happen",
                                           ruby.id2name(id)));
                }
                SingletonClass(this, ruby).addMethod(id, body.body, NOEX.PUBLIC);
                ruby.Funcall(this, "singleton_method_added", Symbol.ID2SYM(id));
            }
*/            
            return this;
        }
        
        public bool Eqq(object o)
        {
            return ruby.InstanceOf(o).IsKindOf(this);
        }
        private RMetaObject ModCheck(object o)
        {
            if (o is RMetaObject == false)
            {
                throw new eTypeError("<=> requires Class or Module (" +
                                     ruby.ClassOf(o).ClassName + " given)");
            }
            return (RMetaObject)o;
        }
        public int CompareTo(object o)
        {
            if (this == o) return 0;
            RMetaObject r = ModCheck(o);
            if (le(r)) return -1;
            return 1;
        }
        public bool le(object o)
        {
            RMetaObject arg = ModCheck(o);
            RMetaObject mod = this;
            while (mod != null)
            {
                if (mod.m_tbl == arg.m_tbl)
                    return true;
                mod = mod.super;
            }
            return false;
        }
        public bool lt(object o)
        {
            if (this == o) return false;
            return le(o);
        }
        public bool ge(object o)
        {
            RMetaObject arg = ModCheck(o);
            return arg.le(this);
        }
        public bool gt(object o)
        {
            if (this == o) return false;
            return ge(o);
        }
        public RArray IncludedModules()
        {
            ArrayList a = new ArrayList();
            for (RMetaObject p = super; p != null; p = p.super)
            {
                if (p is RIncClass) a.Add(p.klass);
            }
            return new RArray(ruby, a);
        }

        public void DefineModuleFunction(string name, MethodInfo mi)
        {
            DefinePrivateMethod(name, mi);
            DefineSingletonMethod(name, mi);
        }
        public void DefineModuleFunction(string name, RMethod rm, int argc)
        {
            DefinePrivateMethod(name, rm, argc);
            DefineSingletonMethod(name, rm, argc);
        }
        public void DefinePrivateMethod(string name, MethodInfo mi)
        {
            addMethod(name, mi, NOEX.PRIVATE | NOEX.CFUNC);
        }
        public void DefinePrivateMethod(string name, RMethod rm, int argc)
        {
            addMethod(name, rm, argc, NOEX.PRIVATE | NOEX.CFUNC);
        }
        public void DefineProtectedMethod(string name, MethodInfo mi)
        {
            addMethod(name, mi, NOEX.PROTECTED | NOEX.CFUNC);
        }
        public void DefineProtectedMethod(string name, RMethod rm, int argc)
        {
            addMethod(name, rm, argc, NOEX.PROTECTED | NOEX.CFUNC);
        }
        public void DefineMethod(string name, MethodInfo mi)
        {
            addMethod(name, mi,
              ((name == "initialize") ? NOEX.PRIVATE : NOEX.PUBLIC) | NOEX.CFUNC);
        }
        
        public void DefineMethod(string name, RCMethod rm)
        {
            m_tbl[name] = rm;
        }
        
        public void DefineMethod(string name, RMethod rm, int argc)
        {
            AddMethodCheck();
            NOEX accs = ((name == "initialize") ? NOEX.PRIVATE : NOEX.PUBLIC) | NOEX.CFUNC;
            addMethod(name, rm, argc, accs);
        }
/*
        protected void addMethod(string name, RMethod rm, int argc, NOEX noex)
        {
            RNode func = new RNRFunc(ruby, rm, argc);
            RNode body = new RNMethod(func, noex);
#if INIT_DEBUG
            System.Console.WriteLine("AddMethod for " + ToString() + ", " + name + "(" + ruby.intern(name).ToString() + ")");
#endif
            lock (m_tbl.SyncRoot)
            {
                m_tbl[ruby.intern(name)] = body;
            }
        }
*/

        protected void addMethod(string name, RMethod rm, int argc, NOEX noex)
        {
            //Console.WriteLine("addMethod " + rm.Method.DeclaringType.Name + "." + rm.Method.Name);
            m_tbl[name] = new RDelegateMethod(ruby, name, rm);
        }

        public static RBasic ConvertToRuby(NetRuby ruby, object v)
        {
            if(v is RBasic) {
                return (RBasic)v;
            } else if (v is int) {
                return new RFixnum(ruby, (int)v);
            } else if (v is bool) {
                if((bool)v)
                    return new RTrue(ruby);
                else
                    return new RFalse(ruby);
            } else if (v is string) {
                return new RString(ruby, (string)v);
            } else if (v == null) {
                return ruby.oNil;
            } else {
                throw new Exception("Illegal type to convert to Ruby object: " + v.GetType().Name);
            }
        }
        
        class RDelegateMethod : RCMethod
        {
            NetRuby ruby;
            RMethod method;

            internal RDelegateMethod(NetRuby r, string n, RMethod m)
            {
                method = m;
                ruby = r;
                name = n;
            }

            public override RBasic Call(RThread th, RBasic self, RBasic[] args, RCBlock block)
            {
                th.PushLegacyBlock(block);
                RBasic ret = RClass.ConvertToRuby(ruby, method(self, (object[])args));
                th.PopLegacyBlock();
                return ret;
            }
            
            public override string ToString()
            {
                return "<DelegateMethod: " + method.Method.DeclaringType.Name + "." + method.Method.Name + ">";
            }
        }
        
        class RBuiltinMethod : RCMethod
        {
            NetRuby ruby;
            MethodInfo method_info;

            internal RBuiltinMethod(NetRuby r, MethodInfo mi)
            {
                ruby = r;
                method_info = mi;
            }

            public override string ToString()
            {
                return "<BuiltinMethod: " + method_info.DeclaringType.Name + "." + method_info.Name + ">";
            }

            public override RBasic Call(RThread th, RBasic self, RBasic[] args, RCBlock block)
            {
                th.PushLegacyBlock(block);
                /*
                Console.WriteLine("Invoke " + method_info.Name + " self=" + self.GetType().Name + ":" + self.ToString());
                Console.WriteLine("mi_type=" + method_info.DeclaringType.Name);
                foreach(ParameterInfo p in method_info.GetParameters()) {
                    Console.WriteLine("mparam: " + p.ParameterType.Name);
                }
                foreach(RBasic r in args) {
                    Console.WriteLine("realparam: " + r.GetType().Name);
                }
                */

                // return (RBasic)method_info.Invoke(null, new object[] { self, args });
                ParameterInfo[] pi = method_info.GetParameters();
                RBasic ret;
                
                if(pi.Length > 0 && pi[0].ParameterType == typeof(object[])) {                        
                    ret = RClass.ConvertToRuby(ruby, method_info.Invoke(self, new object[] { args }));
                } else {
                    object[] ca = new object[pi.Length];
                    for(int i = 0; i < pi.Length; i++) {
                        if(pi[i].ParameterType == typeof(int)) {
                            ca[i] = args[i].ToInteger();
                        } else {
                            ca[i] = args[i];
                        }
                    }
                
                    ret = RClass.ConvertToRuby(ruby, method_info.Invoke(self, ca));
                }
                th.PopLegacyBlock();
                return ret;
            }
        }
        
        protected void addMethod(string name, MethodInfo mi, NOEX noex)
        {
            if(!mi.IsStatic) {
                m_tbl[name] = new RBuiltinMethod(ruby, mi);
            }
            /*
            Console.WriteLine("adding " + mi.DeclaringType.Name + "." + mi.Name + " static=" + mi.IsStatic.ToString()
                    + " ret=" + mi.ReturnType.Name);
            
            foreach(ParameterInfo pi in mi.GetParameters()) {
                Console.WriteLine("param " + pi.ParameterType.Name + " " + pi.Name); 
            }
            */
        }

        public void RemoveMethod(uint id)
        {
            RThread th = ruby.GetCurrentContext();
/*            
            if (this == ruby.cObject) th.Secure(4);
            if (th.safeLevel >= 4 && IsTainted == false)
                throw new SecurityException("Insecure: can't remove method");
*/                
            if (IsFrozen) ruby.ErrorFrozen("class/module");
            lock (m_tbl.SyncRoot)
            {
                if (m_tbl.ContainsKey(id) == false)
                    throw new eNameError(String.Format("method `{0}' not defined in {1}",
                                                       ruby.id2name(id), Name));
                m_tbl.Remove(id);
            }
            ///ruby.ClearCache(id);
        }
        public void UndefMethod(string name)
        {
            AddMethodCheck();
            lock (m_tbl.SyncRoot)
            {
                                /*
                m_tbl[ruby.intern(name)] = new RNMethod(null, NOEX.UNDEF);
                                */
            }
#if INIT_DEBUG
            System.Console.WriteLine("Undef for " + ToString() + ", " + name);
#endif
        }
        public void DefineAlias(string name, string def)
        {
            DefineAlias(ruby.intern(name), ruby.intern(def));
        }
        
        public void DefineAlias(uint name, uint defid)
        {
/*                        
            if (IsFrozen) ruby.ErrorFrozen("class/module");
            if (name == defid) return;
            if (this == ruby.cObject)
            {
                ruby.Secure(4);
            }
            RMetaObject origin;
            RNode org = SearchMethod(defid, out origin);
            if (org == null || org.body == null)
            {
                if (this is RModule)
                {
                    org = ruby.cObject.SearchMethod(defid, out origin);
                }
            }
            if (org == null || org.body == null)
            {
                printUndef(ruby.id2name(defid));
            }
            RNode body = org.body;
            org.cnt++;
            if (body is RNFBody)
            {
                defid = body.mid;
                origin = body.orig;
                body = body.head;
            }
            ruby.ClearCache(name);
            lock (m_tbl.SyncRoot)
            {
                m_tbl[name] = new RNMethod(new RNFBody(body, defid, origin), org.noex);
            }
*/                        
        }
        protected void printUndef(uint id)
        {
            printUndef(ruby.id2name(id));
        }
        protected void printUndef(string name)
        {
            eNameError ex = new eNameError("undefned method '" + name +
                                           "' for " + ((this is RModule) ? "module" : "class") + " '" + ToString());
            throw ex;
        }
        
        internal object SearchMethod(string id, out RMetaObject origin)
        {
            origin = null;
            RMetaObject klass = this;
            object o;
            while (klass.m_tbl.lookup(id, out o) == false)
            {
                klass = klass.super;
                if (klass == null) return null;
            }
            origin = klass;
            return o;
        }
        
        private void AddMethodCheck()
        {
/*        
            if (ruby.SafeLevel >= 4 && (this == ruby.cObject || IsTainted == false))
            {
                throw new SecurityException("Insecure: can't define method");
            }
*/            
            if (IsFrozen) ruby.ErrorFrozen("class/moodule");
        }
/*        
        internal void addMethod(uint mid, RNode defn, NOEX accs)
        {
            AddMethodCheck();
            RNode body = new RNMethod(defn, accs);
            lock (m_tbl.SyncRoot)
            {
                m_tbl[mid] = body;
            }
        }
        private void addMethod(string name, MethodInfo mi, NOEX accs)
        {
            if (mi == null)
            {
                ruby.warn("method '{0}' is missing", name);
            }
            AddMethodCheck();
            RNode func = new RNCFunc(mi, ruby);
            RNode body = new RNMethod(func, accs);
#if INIT_DEBUG
            System.Console.WriteLine("AddMethod for " + ToString() + ", " + name + "(" + ruby.intern(name).ToString() + ")");
#endif
            lock (m_tbl.SyncRoot)
            {
                m_tbl[ruby.intern(name)] = body;
            }
        }        
        internal RNode GetMethodBody(ref uint id, out RMetaObject klass, out NOEX noex)
        {
            noex = NOEX.PUBLIC;
            klass = null;
        
            RMetaObject org;
            RNode body = SearchMethod(id, out org);
            if (body == null || body.body == null)
            {
                ruby.SetCache(this, id);
                return null;
            }
            ruby.SetCache(this, id, body, org);
            noex = body.noex;
            body = body.body;
            if (body is RNFBody)
            {
                klass = body.orig;
                id = body.mid;
                body = body.head;
            }
            else
            {
                klass = org;
            }
            return body;
        }
*/        
        public RArray Constants
        {
            get {
                RArray ary = new RArray(ruby, true);
                for (RMetaObject mod = this; mod != null; mod = mod.super)
                {
                    if (mod.iv_tbl == null) continue;

                    lock (mod.iv_tbl.SyncRoot)
                    {
                        foreach (DictionaryEntry ent in mod.iv_tbl)
                        {
                            if (Parser.is_const_id((uint)ent.Key))
                            {
                                string s = ruby.id2name((uint)ent.Key);
                                if (ary.Contains(s) == false)
                                    ary.Add(s);
                            }
                        }
                    }
                    if (mod == ruby.cObject)
                    {
                        lock (ruby.class_tbl.SyncRoot)
                        {
                            foreach (DictionaryEntry ent in ruby.class_tbl)
                            {
                                if (Parser.is_const_id((uint)ent.Key))
                                {
                                    string s = ruby.id2name((uint)ent.Key);
                                    if (ary.Contains(s) == false)
                                        ary.Add(s);
                                }
                            }
                        }
                        // autoload
                    }
                }
                return ary;
            }
        }
        
        internal object constants(RBasic r, params object[] args)
        {
            return ((RMetaObject)r).Constants;
        }
        internal object const_get(RBasic r, params object[] args)
        {
            uint id = ruby.ToID(args[0]);
            if (Parser.is_const_id(id) == false)
                throw new eNameError("wrong constant name " + ruby.id2name(id));
            return ConstGet(id);
        }
        internal object const_set(RBasic r, params object[] args)
        {
            uint id = ruby.ToID(args[0]);
            if (Parser.is_const_id(id) == false)
                throw new eNameError("wrong constant name " + ruby.id2name(id));
            ConstSet(id, args[1]);
            return args[1];
        }
        internal object is_const_defined(RBasic r, params object[] args)
        {
            uint id = ruby.ToID(args[0]);
            if (Parser.is_const_id(id) == false)
                throw new eNameError("wrong constant name " + ruby.id2name(id));
            return IsConstDefinedAt(id);
        }
        internal object remove_const(RBasic r, params object[] args)
        {
            uint id = ruby.ToID(args[0]);
            return RemoveConst(id);
        }
        internal static object remove_method(RBasic r, params object[] args)
        {
            ((RMetaObject)r).RemoveMethod(r.ruby.ToID(args[0]));
            return r;
        }
        internal static object undef_method(RBasic r, params object[] args)
        {
            ((RMetaObject)r).UndefMethod(args[0].ToString());
            return r;
        }
        internal static object alias_method(RBasic r, params object[] args)
        {
            NetRuby rb = r.ruby;
            ((RMetaObject)r).DefineAlias(rb.ToID(args[0]), rb.ToID(args[1]));
            return r;
        }
        internal static object define_method(RBasic r, params object[] args)
        {
/*        
            NetRuby ruby = r.ruby;
            uint id;
            object body;
            if (args.Length == 1)
            {
                id = ruby.ToID(args[0]);
                body = ruby.Lambda();
            }
            else if (args.Length == 2)
            {
                id = ruby.ToID(args[0]);
                body = ruby.InstanceOf(args[1]);
        ruby.bug("define_method(with 2 args) is not yet implemented");

                //if (body.IsKindOf(ruby.cMethod))

            }
            else
            {
                throw new eArgError(String.Format("worng # of arguments({0} for 1)",
                                                  args.Length));
            }
        ruby.bug("Module::define_method is not yet implemented");
*/        
            return null;
        }
    }

    public class RModule : RMetaObject, ICloneable
    {
        public RModule(NetRuby rb)
            : base(rb, null, null, rb.cModule)
        {
        }
        public RModule(NetRuby rb, string name)
            : base(rb, name, null, rb.cModule)
        {
            if (name != null)
            {
                uint id = rb.intern(name);
                rb.class_tbl.Add(id, this);
            }
        }
        public RModule(NetRuby rb, string name, RMetaObject spr)
            : base(rb, name, spr, rb.cModule)
        {
            if (name != null)
            {
                uint id = rb.intern(name);
                rb.class_tbl.Add(id, this);
            }
        }

        internal RModule(RModule o) :
            base(o)
        {
        }

        public override object Clone()
        {
            return new RModule(this);
        }
    }
    
    public class RClass : RMetaObject, ICloneable
    {
        public RClass(NetRuby rb, string name, RMetaObject spr)
            : base(rb, name, spr, rb.cClass)
        {
        }
        public RClass(NetRuby rb, RMetaObject spr)
            : base(rb, null, spr, rb.cClass)
        {
        }
        internal RClass(RClass o) :
            base(o)
        {
        }

        public override object Clone()
        {
            return new RClass(this);
        }

        public virtual RBasic NewInstance(object[] argv)
        {
            return NewInstance(argv, this);
        }
        static public RBasic NewInstance(object[] argv, RMetaObject meta)
        {
            if (meta is RSingletonClass)
            {
                throw new eTypeError("can't create instance of virtual class");
            }
            NetRuby ruby = meta.ruby;
            RObject obj = new RObject(ruby, meta);
            ruby.CallInit(obj, argv);
            return obj;
        }

        internal RClass DefineClass(string name, RMetaObject spr)
        {
            klass = new RSingletonClass(spr.klass);
            klass.AttachSingleton(this);
            ruby.Funcall(spr, "inherited", this);
            lock (ruby.class_tbl.SyncRoot)
            {
                ruby.class_tbl[ruby.intern(name)] = this;
            }
            return this;
        }
        
        static public RClass ClassNew(NetRuby ruby, RMetaObject spr, object[] o)
        {
            if (spr == null)
            {
                spr = ruby.cObject;
            }
            else if (spr is RSingletonClass)
            {
                throw new eTypeError("can't make subclass of virtual class");
            }
            RClass klass = new RClass(ruby, spr);
            klass.klass = new RSingletonClass(spr.klass);
            klass.klass.AttachSingleton(klass);
            ruby.CallInit(klass, o);
            ruby.Funcall(spr, "inherited", new object[1] { klass });
            return klass;
        }
        public RMetaObject Superclass
        {
            get {
                RMetaObject spr = super;
                while (spr is RIncClass)
                {
                    spr = spr.super;
                }
                if (spr == null)
                    return null;
                return spr;
            }
        }
        internal static object Inherited(RBasic rb, params object[] o)
        {
            throw new eTypeError("can't make subclass of Class");
        }
    }

    public class RSingletonClass : RClass
    {
        internal RSingletonClass(RMetaObject o)
            : base(o.ruby, o)
        {
        }
        public override bool IsSingleton
        {
            get { return true; }
        }
        internal override void AttachSingleton(RObjectBase obj)
        {
            if (iv_tbl == null)
            {
                iv_tbl = new st_table();
            }
            iv_tbl[ruby.intern("__attached__")] = obj;
        }
    }

    public class RIncClass : RClass
    {
        internal RIncClass(RMetaObject module, RMetaObject sp) :
            base(module.ruby, sp)
        {
            if (module.iv_tbl == null)
            {
                module.iv_tbl = new st_table();
            }
            iv_tbl = module.iv_tbl;
            m_tbl = module.m_tbl;
            if (module is RIncClass)
            {
                klass = module.klass;
            }
            else
            {
                klass = module;
            }
        }
    }
}
// vim:et:sts=4:sw=4
