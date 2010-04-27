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

namespace NETRuby
{
    public class RHash : RBasic, ICloneable, IEnumerable, ICollection
    {
        public RHash(NetRuby rb) :
            base(rb, rb.cHash)
        {
            hash = new Hashtable();
            ifnone = null;
        }
        public RHash(NetRuby rb, RMetaObject meta) :
            base(rb, meta)
        {
            hash = new Hashtable();
            ifnone = null;
        }
        public RHash(NetRuby rb, RHash org, RMetaObject meta) :
            base(rb, meta)
        {
            hash = (Hashtable)org.hash.Clone();
            ifnone = null;
        }
        object ifnone;
        Hashtable hash;
        public object this[object index]
        {
            get { return hash[index]; }
            set { hash[index] = value; }
        }
        public object Default
        {
            get { return ifnone; }
            set { Modify(); ifnone = value; }
        }
        public object Index(object val)
        {
            lock (hash.SyncRoot)
            {
                foreach (DictionaryEntry ent in hash)
                {
                    if (val.Equals(ent.Value)) return ent.Key;
                }
            }
            return null;
        }
        public RArray Indices(object[] args)
        {
            RArray ary = new RArray(ruby, true);
            for (int i = 0; i < args.Length; i++)
            {
                ary.Add(hash[args[i]]);
            }
            return ary;
        }
        public object Clone()
        {
            RHash newhash = (RHash)MemberwiseClone();
            newhash.hash = (Hashtable)hash.Clone();
            return newhash;
        }
        public override RArray ToArray()
        {
            ArrayList a = new ArrayList();
            lock (hash.SyncRoot)
            {
                foreach (DictionaryEntry ent in hash)
                {
                    a.Add(RArray.AssocNew(ruby, ent.Key, ent.Value));
                }
            }
            return new RArray(ruby, a);
        }
        public override string ToString()
        {
            return ToArray().ToString();
        }
        public override object Inspect()
        {
            if (hash.Count == 0) return "{}";
            if (ruby.IsInspecting(this)) return "{...}";
            return ruby.ProtectInspect(this,
                                  new InspectMethod(inspect_hash),
                                  new object[2] {this, 0});
        }
        private object inspect_hash(RBasic h, object[] args)
        {
            bool tainted = false;
            bool start = false;
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            lock (hash.SyncRoot)
            {
                foreach (DictionaryEntry ent in hash)
                {
                    if (start == false)
                    {
                        start = true;
                    }
                    else
                    {
                        sb.Append(", ");
                    }
                    sb.AppendFormat("{0}=>{1}", ruby.Inspect(ent.Key), ruby.Inspect(ent.Value));
                    if (ent.Key is RBasic && ((RBasic)ent.Key).IsTainted)
                        tainted = true;
                    if (ent.Value is RBasic && ((RBasic)ent.Value).IsTainted)
                        tainted = true;
                }
            }
            sb.Append("}");
            if (IsTainted || tainted)
                return new RString(ruby, sb.ToString(), true);
            return sb.ToString();
        }
        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o is RHash == false) return false;
            RHash ot = (RHash)o;
            Hashtable oh = ot.hash;
            if (hash.Count != oh.Count) return false;
            lock (hash.SyncRoot)
            {
                foreach (DictionaryEntry ent in hash)
                {
                    if (oh.ContainsKey(ent.Key) == false) return false;
                    if (oh[ent.Key].Equals(ent.Value) == false) return false;
                }
            }
            return true;
        }
        public override int GetHashCode() // for voiding CS0659 warning.
        {
            return base.GetHashCode();
        }
        public IEnumerator GetEnumerator()
        {
            return hash.GetEnumerator();
        }
        public int Count
        {
            get { return hash.Count; }
        }
        public bool IsSynchronized
        {
            get { return hash.IsSynchronized; }
        }
        public object SyncRoot
        {
            get { return hash.SyncRoot; }
        }
        public void CopyTo(Array array, int index)
        {
            hash.CopyTo(array, index);
        }
        public bool IsEmpty
        {
            get { return hash.Count == 0; }
        }
        public RArray Sort()
        {
            return ToArray().SortAt();
        }
        public RArray Keys
        {
            get {
                ArrayList ary = new ArrayList();
                lock (hash)
                {
                    foreach (DictionaryEntry ent in hash)
                    {
                        ary.Add(ent.Key);
                    }
                }
                return new RArray(ruby, ary);
            }
        }
        public RArray Values
        {
            get {
                ArrayList ary = new ArrayList();
                lock (hash)
                {
                    foreach (DictionaryEntry ent in hash)
                    {
                        ary.Add(ent.Value);
                    }
                }
                return new RArray(ruby, ary);
            }
        }
        public bool ContainsKey(object o)
        {
            return hash.ContainsKey(o);
        }
        public bool ContainsValue(object o)
        {
            return hash.ContainsValue(o);
        }
        private void Modify()
        {
            if (IsFrozen) ruby.ErrorFrozen("hash");
/*            
            if (IsTainted == false && ruby.SafeLevel >= 4)
            {
                throw new SecurityException("Insecure: can't modify hash");
            }
*/            
        }
        internal RHash Initialize(object[] argv)
        {
            Modify();
            if (argv.Length >= 1)
                ifnone = argv[0];
            return this;
        }
    }

    public class RHashClass : RClass
    {
        private RHashClass(NetRuby rb) :
            base(rb, "Hash", rb.cObject)
        {
        }

        static internal object s_new(RBasic r, params object[] args)
        {
            RHash hash = new RHash(r.ruby, (RMetaObject)r);
            r.ruby.CallInit(hash, args);
            return hash;
        }
        static internal object s_create(RBasic r, params object[] args)
        {
            RHash hash;
            if (args.Length == 1 && args[0] is RHash)
            {
                hash = new RHash(r.ruby, ((RHash)args[0]), (RMetaObject)r);
                return hash;
            }
            if (args.Length % 2 != 0)
            {
                throw new eArgError("odd number args for Hash");
            }
            hash = new RHash(r.ruby, (RMetaObject)r);
            for (int i = 0; i < args.Length; i += 2)
            {
                hash[args[i]] = args[i + 1];
            }
            return hash;
        }
        static internal object initialize(RBasic r, params object[] args)
        {
            return ((RHash)r).Initialize(args);
        }
        static internal object s_noop(RBasic r, params object[] args)
        {
            return r;
        }
        static internal object aref(RBasic r, params object[] args)
        {
            RHash h = (RHash)r;
            return h[args[0]];
        }
        static internal object aset(RBasic r, params object[] args)
        {
            RHash h = (RHash)r;
            h[args[0]] = args[1];
            return args[1];
        }
        static internal object defaultv(RBasic r, params object[] args)
        {
            return ((RHash)r).Default;
        }
        static internal object set_default(RBasic r, params object[] args)
        {
            ((RHash)r).Default = args[0];
            return args[0];
        }
        static internal object index(RBasic r, params object[] args)
        {
            return ((RHash)r).Index(args[0]);
        }
        static internal object indices(RBasic r, params object[] args)
        {
            return ((RHash)r).Indices(args);
        }
        static internal object size(RBasic r, params object[] args)
        {
            return ((RHash)r).Count;
        }
        static internal object empty_p(RBasic r, params object[] args)
        {
            return ((RHash)r).IsEmpty;
        }
        static internal object each_pair(RBasic r, params object[] args)
        {
/*        
            NetRuby rb = r.ruby;
            lock (((RHash)r).SyncRoot)
            {
                foreach (DictionaryEntry ent in ((RHash)r))
                {
                    rb.Yield(RArray.AssocNew(rb, ent.Key, ent.Value));
                }
            }
            return r;
*/
            return null; //PH
        }
        static internal object each_key(RBasic r, params object[] args)
        {
/*        
            NetRuby rb = r.ruby;
            lock (((RHash)r).SyncRoot)
            {
                foreach (DictionaryEntry ent in ((RHash)r))
                {
                    rb.Yield(ent.Key);
                }
            }
            return r;
*/
            return null; //PH
        }
        static internal object each_value(RBasic r, params object[] args)
        {
/*        
            NetRuby rb = r.ruby;
            lock (((RHash)r).SyncRoot)
            {
                foreach (DictionaryEntry ent in ((RHash)r))
                {
                    rb.Yield(ent.Value);
                }
            }
            return r;
*/
            return null; //PH
        }
        static internal object sort(RBasic r, params object[] args)
        {
            return ((RHash)r).Sort();
        }
        static internal object keys(RBasic r, params object[] args)
        {
            return ((RHash)r).Keys;
        }
        static internal object values(RBasic r, params object[] args)
        {
            return ((RHash)r).Values;
        }
        static internal object contains_value(RBasic r, params object[] args)
        {
            return ((RHash)r).ContainsValue(args[0]);
        }
        static internal object contains_key(RBasic r, params object[] args)
        {
            return ((RHash)r).ContainsKey(args[0]);
        }
        
        
        static internal void Init(NetRuby rb)
        {
            RHashClass hash = new RHashClass(rb);
            hash.DefineClass("Hash", rb.cObject);
            IncludeModule(hash, rb.mEnumerable);
            rb.cHash = hash;
            hash.DefineSingletonMethod("new", new RMethod(s_new), -1);
            hash.DefineSingletonMethod("[]", new RMethod(s_create), -1);
        
            hash.DefineMethod("rehash", new RMethod(s_noop), 0);
            hash.DefineMethod("to_hash", new RMethod(s_noop), 0);

            hash.DefineMethod("[]", new RMethod(aref), 1);
            hash.DefineMethod("[]=", new RMethod(aset), 2);
            hash.DefineMethod("store", new RMethod(aset), 2);
            hash.DefineMethod("default", new RMethod(defaultv), 0);
            hash.DefineMethod("default=", new RMethod(set_default), 1);
            hash.DefineMethod("index", new RMethod(index), 1);
            hash.DefineMethod("indexes", new RMethod(indices), -1);
            hash.DefineMethod("indices", new RMethod(indices), -1);
            hash.DefineMethod("size", new RMethod(size), 0);
            hash.DefineMethod("length", new RMethod(size), 0);
            hash.DefineMethod("empty?", new RMethod(empty_p), 0);

            hash.DefineMethod("each", new RMethod(each_pair), 0);
            hash.DefineMethod("each_value", new RMethod(each_value), 0);
            hash.DefineMethod("each_key", new RMethod(each_key), 0);
            hash.DefineMethod("each_pair", new RMethod(each_pair), 0);
            hash.DefineMethod("sort", new RMethod(sort), 0);

            hash.DefineMethod("keys", new RMethod(keys), 0);
            hash.DefineMethod("values", new RMethod(values), 0);

            hash.DefineMethod("include?", new RMethod(contains_key), 1);
            hash.DefineMethod("member?", new RMethod(contains_key), 1);
            hash.DefineMethod("has_key?", new RMethod(contains_key), 1);
            hash.DefineMethod("key?", new RMethod(contains_key), 1);
            hash.DefineMethod("has_value?", new RMethod(contains_value), 1);
            hash.DefineMethod("value?", new RMethod(contains_value), 1);
        
        }
    }
    
}
