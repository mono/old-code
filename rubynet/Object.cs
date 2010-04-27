/*
Copyright (C) 1993-2000 Yukihiro Matsumoto
Copyright (C) 2000      Network Applied Communication Laboratory, Inc.
Copyright (C) 2000      Information-technology Promotion Agency, Japan
Copyright (C) 2001-2002 arton
Copyright (C) 2005      Jaen Saul
*/

using System;
using System.Collections;
using System.Reflection;
using System.Security;

namespace NETRuby
{
    public class RBasic
    {
        /*
        internal RBasic(NetRuby rb)
        {
        }
        */
        internal RBasic(NetRuby rb, RMetaObject meta)
        {
            klass = meta;
            ruby = rb;
            /*
            if (rb.SafeLevel >= 3)
            {
                flags |= FL.TAINT;
            }
            */
            GetHashCode();        // define ID
        }

        internal RBasic(RBasic o)
        {
            ruby = o.ruby;
            if (o.klass is RSingletonClass)
            {
                RMetaObject org = o.klass;
                RMetaObject meta = (RMetaObject)org.Clone();
                if (meta.m_tbl == null) meta.m_tbl = new st_table();
                else meta.m_tbl.Clear();
/*
                foreach (DictionaryEntry ent in org.m_tbl)
                {
                    RNMethod m = (RNMethod)ent.Value;
                    meta.m_tbl.Add(ent.Key, new RNMethod(m));
                }
*/
                klass = meta;
            }
            else
            {
                klass = o.klass;
            }
            flags = o.flags;
/*            
            if (o.Test(FL.EXIVAR))
            {
                ruby.CloneGenericIVar(this, o);
            }
*/            
            GetHashCode();        // define ID
        }

        public enum FL
        {
            EMPTY = 0,
            USHIFT = 11,
            USER0 = (1<<(USHIFT+0)),
            USER1 = (1<<(USHIFT+1)),
            USER2 = (1<<(USHIFT+2)),
            USER3 = (1<<(USHIFT+3)),
            USER4 = (1<<(USHIFT+4)),
            USER5 = (1<<(USHIFT+5)),
            USER6 = (1<<(USHIFT+6)),
            USER7 = (1<<(USHIFT+7)),
            UMASK = (0xff<<USHIFT),
            SINGLETON = USER0,
            FINALIZE = (1<<7),
            TAINT = (1<<8),
            EXIVAR = (1<<9),
            FREEZE = (1<<10),
        }

        public const int Qfalse = 0;
        public const int Qtrue = 2;
        public const int Qnil = 4;

        protected FL flags;
        internal RMetaObject klass;
        public NetRuby ruby;

        public override string ToString()
        {
            string s = ruby.ClassOf(this).Name;
            s = String.Format("#<{0}:0x{1:x8}>", s, GetHashCode());
            return s;
        }
        public virtual RString ToRString()
        {
            RString rs = new RString(ruby, ToString(), IsTainted);
            return rs;
        }
        public virtual RArray ToArray()
        {
            ArrayList a = new ArrayList();
            a.Add(this);
            return new RArray(ruby, a);
        }
        public virtual RInteger ToInteger()
        {
            return null;
        }
        public virtual RFloat ToFloat()
        {
            return null;
        }
        public virtual RChar ToChar()
        {
            return new RChar(ruby, '?');
        }
        public virtual RMetaObject Class
        {
            get { return ruby.ClassOf(this).ClassReal; }
        }
        public virtual bool IsNil
        {
            get { return false; }
        }
        public virtual bool IsTrue()
        {
            return true;
        }

        public virtual RBasic Not()
        {
            return ruby.oFalse;
        }
        public virtual object And(object o)
        {
            return null;
        }
        public virtual object Or(object o)
        {
            return null;
        }
        public virtual object Xor(object o)
        {
            return null;
        }

        public RBasic Freeze()
        {
            flags |= FL.FREEZE;
            return this;
        }
        public bool IsFrozen
        {
            get { return (flags & FL.FREEZE) != 0; }
        }

        internal void FrozenClassCheck()
        {
            if (IsFrozen)
            {
                string desc = "something(?!)";
                if (IsSingleton)
                {
                    desc = "object";
                }
                else
                {
                    if (this is RModule || this is RIncClass)
                    {
                        desc = "module";
                    }
                    else if (this is RClass)
                    {
                        desc = "class";
                    }
                }
                ruby.ErrorFrozen(desc);
            }
        }

        public bool IsTainted
        {
            get { return (flags & FL.TAINT) != 0; }
        }
        public virtual bool IsSingleton
        {
            get { return false; }
        }
        public RBasic Taint()
        {
            ////ruby.Secure(4);
            if ((flags & FL.TAINT) == 0)
            {
                if ((flags & FL.FREEZE) != 0)
                {
                    ruby.ErrorFrozen("object");
                }
                flags |= FL.TAINT;
            }
            return this;
        }

        public RBasic Untaint()
        {
            ////ruby.Secure(3);
            if ((flags & FL.TAINT) != 0)
            {
                if ((flags & FL.FREEZE) != 0)
                {
                    ruby.ErrorFrozen("object");
                }
                flags &= ~FL.FREEZE;
            }
            return this;
        }
        internal void Set(FL f)
        {
            flags |= f;
        }
        internal void Unset(FL f)
        {
            flags &= ~f;
        }
        public bool Test(FL f)
        {
            return ((flags & f) == 0) ? false : true;
        }
        public void Infect(RBasic o)
        {
            if (o.IsSpecialConst == false && IsSpecialConst == false)
            {
                flags |= (o.flags & FL.TAINT);
            }
        }
        public static bool RTest(object o)
        {
            if (o == null || (o is bool && (bool)o == false))
                return false;
            return true;
        }

        public virtual object Inspect()
        {
            return ruby.Funcall(this, "to_s", null);
        }
        public delegate object InspectMethod(RBasic r, object[] arg);
        public virtual bool RespondTo(object[] argv)
        {
            object[] args = new object[2];
            ruby.ScanArgs(argv, "11", args);
            uint id = ruby.ToID(args[0]);
            return RespondTo(id, !RTest(args[1]));
        }
        public virtual bool RespondTo(uint id, bool priv)
        {
            if (ruby.IsMethodBound(klass, id, !priv))
            {
                return true;
            }
            return false;
        }
        
        public virtual object Send(string name, params object[] argv)
        {
            ////RThread th = ruby.GetCurrentContext();
            ////th.PushIter((th.IsBlockGiven) ? ITER.PRE : ITER.NOT);
            object obj = ruby.Call(klass, this, name, argv, 1);
            ////th.PopIter();
            return obj;
        }
        
        public virtual RBasic BasicSend(string name, RBasic[] args, RCBlock block)
        {
            RMetaObject origin;
            RCMethod m = (RCMethod)klass.SearchMethod(name, out origin);
            /*
            Console.WriteLine("sending");
            Console.WriteLine("self " + this.ToString());
            Console.WriteLine("send " + name + " found " + m.ToString());
            foreach(RBasic ro in args) {
                Console.WriteLine("arg " + ro.ToString());
            }
            */
            if(m == null) {
                Console.WriteLine("Available methods:");
                
                foreach(object o in klass.ClassInstanceMethods(new object[] {ruby.oTrue})) {
                    Console.WriteLine(o.ToString());
                }
                
                throw new Exception("Method not found: " + name);
            }
            return m.Call(ruby.GetCurrentContext(), this, args, block);
        }
        
        public virtual object InstanceEval(params object[] argv)
        {
            RMetaObject klass;
            if (IsSpecialConst)
                klass = null;
            else
                klass = SingletonClass(this, ruby);
        
            ////return ruby.SpecificEval(klass, this, argv);
            return null; //PH
        }
        protected object InspectObj(string str)
        {
            string s = "#" + str.Substring(1) + ">";
            if (IsTainted)
            {
                return new RString(ruby, s, true);
            }
            return s;
        }

        // for inspect (use RString.AsString for general purpose)
        protected string obj_to_s(object x)
        {
            if (x == null)
                return "nil";
            if (x is string)
                return(string)x;
            if (x is int || x is long)
                return x.ToString();
            object r = ruby.Funcall(x, "to_s", null);
            if (r is RString)
                return r.ToString();
            return (string)r;
        }
        
        public virtual bool IsSpecialConst
        {
            get { return false; }
        }
        public static bool IsSpecialConstType(object o)
        {
            if (o is RBasic) return ((RBasic)o).IsSpecialConst;
            if (o == null) return true;  // nil
            if (o is bool) return true; // bool
            if (o is int || o is long || o is double) return true;
            return false;
        }
        public bool IsKindOf(object c)
        {
            RMetaObject cl = ruby.ClassOf(this);
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

        public override bool Equals(object o)
        {
            if (base.Equals(o)) return true;
            return false;
        }
        public override int GetHashCode() // for voiding CS0659 warning.
        {
            return base.GetHashCode();
        }
        public virtual bool Eql(object o)
        {
            if (GetType() != o.GetType()) return false;
            return ruby.Eql(o, this);
        }

        public bool IsIVarDefined(string name)
        {
            return IsIVarDefined(ruby.intern(name));
        }
        public virtual bool IsIVarDefined(uint id)
        {
/*        
            if (Test(FL.EXIVAR) || IsSpecialConst)
            {
                return ruby.IsGenericIVarDefined(this, id);
            }
*/            
            return false;
        }
        public object IVarGet(string name)
        {
            return IVarGet(ruby.intern(name));
        }
        public virtual object IVarGet(uint iid)
        {
/*        
            if (Test(FL.EXIVAR) || IsSpecialConst)
            {
                return ruby.GenericIVarGet(this, iid);
            }
*/            
            if (ruby.verbose)
            {
                ruby.warning(String.Format("instance variable {0} not initialized",
                                           ruby.id2name(iid)));
            }
            return null;
        }
        public virtual object IVarSet(uint uid, object val)
        {
            IVarCheck();
/*            
            ruby.GenericIVarSet(this, uid, val);
            Set(FL.EXIVAR);
*/            
            return val;
        }
        public virtual object IVarSet(string name, object val)
        {
            return IVarSet(ruby.intern(name), val);
        }
        
        protected void IVarCheck()
        {
/*            
            if (IsTainted == false && ruby.SafeLevel >= 4)
            {
                throw new SecurityException("Insecure: can't modify instance variable");
            }
*/            
            if (IsFrozen) ruby.ErrorFrozen("object");
        }

        public virtual RArray InstanceVariables
        {
            get {
                ArrayList ary = new ArrayList();
/*                
                if (ruby.generic_iv_tbl != null)
                {
                    if (Test(FL.EXIVAR) || IsSpecialConst)
                    {
                        st_table tbl;
                        if (ruby.generic_iv_tbl.lookup(this, out tbl))
                        {
                            foreach (DictionaryEntry ent in tbl)
                            {
                                if (Parser.is_instance_id((uint)ent.Key))
                                {
                                    ary.Add(ruby.id2name((uint)ent.Key));
                                }
                            }
                        }
                    }
                }
*/                
                return new RArray(ruby, ary);
            }
        }
        public virtual object RemoveInstanceVariable(uint id)
        {
            object val = null;
/*            
            if (IsTainted && ruby.SafeLevel >= 4)
                throw new SecurityException("Insecure: can't modify instance variable");
*/                
            if (IsFrozen) ruby.ErrorFrozen("object");
            if (Parser.is_instance_id(id) == false)
            {
                throw new eNameError("`" + ruby.id2name(id) + "' is not an instance variable");
            }
/*            
            if (Test(FL.EXIVAR) || IsSpecialConst)
            {
                return ruby.GenericIVarRemove(this, id);
            }
*/            
            return val;
        }
        public virtual int id
        {
            get { return GetHashCode() << 1; }
        }
        public virtual RArray Methods
        {
            get { return ruby.ClassOf(this).ClassInstanceMethods(new object[1] {true}); }
        }
        public virtual RArray SingletonMethods
        {
            get { return ruby.ClassOf(this).SingletonMethods; }
        }
        public virtual RArray ProtectedMethods
        {
            get { return ruby.ClassOf(this).ClassProtectedInstanceMethods(new object[1] {true}); }
        }
        public virtual RArray PrivateMethods
        {
            get { return ruby.ClassOf(this).ClassPrivateInstanceMethods(new object[1] { true}); }
        }
        
        static internal RClass SingletonClass(object obj, NetRuby ruby)
        {
            if (obj is int || obj is long || obj is double || Symbol.IsSymbol(obj))
            {
                throw new eTypeError("can't define singleton");
            }
            if (RBasic.IsSpecialConstType(obj))
            {
                if (obj == null)
                {
                    if (ruby.cNilClass.IsSingleton == false)
                    {
                        ruby.cNilClass = new RSingletonClass(ruby.cNilClass);
                        return ruby.cNilClass;
                    }
                }
                bool b = (bool)obj;
                RClass o = (b) ? ruby.cTrueClass : ruby.cFalseClass;
                if (o.IsSingleton == false)
                {
                    if (b)
                        o = ruby.cTrueClass = new RSingletonClass(o);
                    else
                        o = ruby.cFalseClass = new RSingletonClass(o);
                }
                return o;
            }
            if (obj is RBasic == false)
            {
                ruby.bug("Unknown object " + obj.ToString());
            }
            RBasic bas = (RBasic)obj;
            RClass klass = null;
            if (bas is RSingletonClass)
            {
                klass = (RClass)bas.klass;
            }
            else
            {
                klass = new RSingletonClass(bas.klass);
                bas.klass = klass;
                klass.AttachSingleton((RObjectBase)obj);
            }
            if (bas.Test(FL.TAINT))
            {
                klass.Set(FL.TAINT);
            }
            else
            {
                klass.Unset(FL.TAINT);
            }
            if (bas.Test(FL.FREEZE))
            {
                klass.Set(FL.FREEZE);
            }
        
            return klass;
        }
        public virtual object Missing(params object[] args)
        {
            return ruby.Missing(this, args);
        }

        //
        // Internal Methods for interpreter
        //
        public delegate object RMethod(RBasic rcver, params object[] args);
        protected object ruby_dummy(RBasic r, params object[] args)
        {
            return null;
        }
        static internal object ruby_missing(RBasic r, params object[] args)
        {
            return r.Missing(args);
        }
        protected object ruby_isnil(RBasic r, params object[] args)
        {
            return r.IsNil;
        }
        internal object ruby_equals(RBasic r, params object[] args)
        {
            return r.Equals(ruby.InstanceOf(args[0]));
        }
        internal object ruby_eql(RBasic r, params object[] args)
        {
            return r.Eql(ruby.InstanceOf(args[0]));
        }
        static protected object ruby_compare(RBasic r, params object[] args)
        {
            return ((IComparable)r).CompareTo(args[0]);
        }
        protected object ruby_false(RBasic r, params object[] args)
        {
            return false;
        }
        protected object ruby_true(RBasic r, params object[] args)
        {
            return true;
        }
        protected object ruby_id(RBasic r, params object[] args)
        {
            return r.id;
        }
        protected object ruby_class(RBasic r, params object[] args)
        {
            return r.Class;
        }
        protected object ruby_and(RBasic r, params object[] args)
        {
            return r.And(ruby.InstanceOf(args[0]));
        }
        protected object ruby_or(RBasic r, params object[] args)
        {
            return r.Or(ruby.InstanceOf(args[0]));
        }
        protected object ruby_xor(RBasic r, params object[] args)
        {
            return r.Xor(ruby.InstanceOf(args[0]));
        }
        protected object ruby_inspect(RBasic r, params object[] args)
        {
            return r.Inspect();
        }
        protected object ruby_respond_to(RBasic r, params object[] args)
        {
            return r.RespondTo(args);
        }
        protected object ruby_send(RBasic r, params object[] args)
        {
            if (args.Length < 1)
                throw new eArgError("no method name given");
            object[] argv = new object[args.Length - 1];
            Array.Copy(args, 1, argv, 0, args.Length - 1);
            return r.Send(args[0].ToString(), argv);
        }
        protected object instance_eval(RBasic r, params object[] args)
        {
            return r.InstanceEval(args);
        }
        protected object ruby_methods(RBasic r, params object[] args)
        {
            return r.Methods;
        }
        protected object ruby_singleton_methods(RBasic r, params object[] args)
        {
            return r.SingletonMethods;
        }
        protected object ruby_protected_methods(RBasic r, params object[] args)
        {
            return r.ProtectedMethods;
        }
        protected object ruby_private_methods(RBasic r, params object[] args)
        {
            return r.PrivateMethods;
        }
        protected object ruby_instance_variables(RBasic r, params object[] args)
        {
            return r.InstanceVariables;
        }
        protected object ruby_remove_instance_variable(RBasic r, params object[] args)
        {
            return r.RemoveInstanceVariable(ruby.ToID(args[0]));
        }
        protected object ruby_to_a(RBasic r, params object[] args)
        {
            return r.ToArray();
        }
        protected object ruby_to_s(RBasic r, params object[] args)
        {
            return r.ToRString();
        }
        protected object ruby_to_f(RBasic r, params object[] args)
        {
            return r.ToFloat();
        }
        protected object ruby_to_c(RBasic r, params object[] args)
        {
            return r.ToChar();
        }
        protected object ruby_clone(RBasic r, params object[] args)
        {
            try
            {
                return ((ICloneable)r).Clone();
            }
#if _DEBUG
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
                System.Console.WriteLine(e.StackTrace);
#else
            catch
            {
#endif
                throw new eTypeError("can't clone " + ruby.ClassOf(r).Name);
            }
        }
        protected object ruby_dup(RBasic r, params object[] args)
        {
            RBasic dup = (RBasic)ruby.Funcall(r, "clone", null);
            if (dup.GetType() != r.GetType())
            {
                throw new eTypeError("duplicated object must be same type");
            }
            if (dup.IsSpecialConst == false)
            {
                dup.klass = r.klass;
                dup.flags = r.flags;
                dup.Infect(r);
            }
            return dup;
        }
        protected object ruby_taint(RBasic r, params object[] args)
        {
            return r.Taint();
        }
        protected object ruby_istainted(RBasic r, params object[] args)
        {
            return r.IsTainted;
        }
        protected object ruby_untaint(RBasic r, params object[] args)
        {
            return r.Untaint();
        }
        protected object ruby_freeze(RBasic r, params object[] args)
        {
            return r.Freeze();
        }
        protected object ruby_isfrozen(RBasic r, params object[] args)
        {
            return r.IsFrozen;
        }
    }

    public class RData : RBasic
    {
        internal RData(NetRuby rb, RMetaObject meta) :
            base(rb, meta)
        {
        }
    }
    
    public class RObjectBase : RBasic, ICloneable
    {
        internal RObjectBase(NetRuby rb, RMetaObject meta)
            : base(rb, meta)
        {
            iv_tbl = null;
        }
        internal RObjectBase(RObjectBase o)
            : base(o)
        {
            klass.AttachSingleton(this);
            if (o.iv_tbl != null)
            {
                iv_tbl = (st_table)o.iv_tbl.Clone();
            }
        }
        
        internal st_table iv_tbl;

        public override bool IsIVarDefined(uint iid)
        {
            object val = null;
            if (iv_tbl != null && iv_tbl.lookup(iid, out val))
            {
                return true;
            }
            return false;
        }
        public override object IVarGet(uint iid)
        {
            object val = null;
            if (iv_tbl != null && iv_tbl.lookup(iid, out val))
            {
                return val;
            }
            if (ruby.verbose)
            {
                ruby.warning(String.Format("instance variable {0} not initialized",
                                           ruby.id2name(iid)));
            }
            return null;
        }
        public override object IVarSet(uint uid, object val)
        {
            IVarCheck();
            if (iv_tbl == null) iv_tbl = new st_table();
            iv_tbl[uid] = val;
            return val;
        }

        public override RArray InstanceVariables
        {
            get {
                ArrayList ary = new ArrayList();
                if (iv_tbl != null)
                {
                    foreach (DictionaryEntry ent in iv_tbl)
                    {
                        if (Parser.is_instance_id((uint)ent.Key))
                        {
                            ary.Add(ruby.id2name((uint)ent.Key));
                        }
                    }
                }
                return new RArray(ruby, ary);
            }
        }

        public override object RemoveInstanceVariable(uint id)
        {
            object val = null;
/*            
            if (IsTainted && ruby.SafeLevel >= 4)
                throw new SecurityException("Insecure: can't modify instance variable");
*/                
            if (IsFrozen) ruby.ErrorFrozen("object");
            if (Parser.is_instance_id(id) == false)
            {
                throw new eNameError("`" + ruby.id2name(id) + "' is not an instance variable");
            }
            if (iv_tbl != null)
            {
                val = iv_tbl[id];
                iv_tbl.Remove(id);
            }
            return val;
        }

        internal bool valGet(uint id, out object val)
        {
            val = null;
            if (iv_tbl != null && iv_tbl.lookup(id, out val))
            {
                return true;
            }
            return false;
        }
        
        public void DefineSingletonMethod(string name, MethodInfo mi)
        {
            SingletonClass(this, ruby).DefineMethod(name, mi);
        }
        public void DefineSingletonMethod(string name, RMethod rm, int argc)
        {
            SingletonClass(this, ruby).DefineMethod(name, rm, argc);
        }

        internal virtual void AttachSingleton(RObjectBase obj)
        {
            // overriden by RSingletonClass
        }

        public virtual object Clone()
        {
            ruby.bug("invalid clone call");
            return null;
        }

    }
        
    public class RObject : RObjectBase, ICloneable
    {
        internal RObject(NetRuby rb, RMetaObject meta)
            : base(rb, meta)
        {
        }
        internal RObject(RObject o)
            : base(o)
        {
        }

        public override object Inspect()
        {
            if (iv_tbl != null && iv_tbl.Count > 0)
            {
                string s = ruby.ClassOf(this).Name;
                if (ruby.IsInspecting(this))
                {
                    return String.Format("#<{0}:0x{1:x8} ...>", s, id);
                }
                return InspectObj(String.Format("-<{0}:0x{1:x8}", s, id));
            }
            return base.Inspect();
        }

        public override object Clone()
        {
            RObject o = new RObject(this);
            return o;
        }
    }

    internal class RMainObject : RObject // for top level object only
    {
        internal RMainObject(NetRuby rb)
            : base(rb, rb.cObject)
        {
        }
        public override string ToString()
        {
            return "main";
        }
        public object Include(object[] args)
        {
            ////ruby.Secure(4);
            return ruby.cObject.Include(args);
        }
        public object Public(object[] args)
        {
            return ruby.cObject.Public(args);
        }
        public object Private(object[] args)
        {
            return ruby.cObject.Private(args);
        }
    }
}

// vim:et:sts=4:sw=4
