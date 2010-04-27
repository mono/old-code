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
    public class RArray : RBasic, ICloneable, IList, IEnumerable
    {
        public RArray(NetRuby rb, ArrayList a) :
            base(rb, rb.cArray)
        {
            ptr = a;
        }
        public RArray(NetRuby rb, ArrayList a, bool clone) :
            base(rb, rb.cArray)
        {
            if (clone)
            {
                // but it creates only a shallow copy.
                ptr = (ArrayList)a.Clone();
            }
            else
            {
                ptr = a;
            }
        }
        public RArray(NetRuby rb, ICollection col) :
            base(rb, rb.cArray)
        {
            ptr = new ArrayList(col);
        }
        public RArray(NetRuby rb, bool newobj) :
            base(rb, rb.cArray)
        {
            ptr = (newobj) ? new ArrayList() : null;
        }
        public RArray(NetRuby rb, bool newobj, RMetaObject spr) :
            base(rb, spr)
        {
            ptr = (newobj) ? new ArrayList() : null;
        }
        public RArray(NetRuby rb, int capa) :
            base(rb, rb.cArray)
        {
            ptr = new ArrayList(capa);
        }

        public ArrayList ArrayList
        {
            get { return ptr; }
        }

        public bool IsFixedSize
        {
            get { return false; }
        }
        public bool IsReadOnly
        {
            get { return IsFrozen; }
        }
        
        public override RArray ToArray()
        {
            return this;
        }

        public object[] Array
        {
            get { return ptr.ToArray(); }
        }
        
        public object Clone()
        {
            RArray ra = new RArray(ruby, (ArrayList)ptr.Clone());
            if (Test(FL.TAINT)) ra.Set(FL.TAINT);
            return ra;
        }
        
        public override string ToString()
        {
            if (ptr == null || ptr.Count <= 0) return String.Empty;
            return Join(ruby.outputFS).ToString();
        }

        public override RString ToRString()
        {
            if (ptr.Count <= 0) return new RString(ruby, String.Empty, false);
            return Join(ruby.outputFS);
        }

        public override object Inspect()
        {
            if (ptr.Count == 0) return "[]";
            if (ruby.IsInspecting(this)) return "[...]";
            return ruby.ProtectInspect(this,
                                  new InspectMethod(inspect_ary),
                                  new object[2] {this, 0});
        }

        public void Clear()
        {
            ptr.Clear();
        }
        public RArray Clear2()
        {
            ptr.Clear();
            return this;
        }

        static public RArray AssocNew(NetRuby ruby, RBasic car, RBasic cdr)
        {
            ArrayList ar = new ArrayList();
            ar.Add(car);
            ar.Add(cdr);
            return new RArray(ruby, ar);
        }
        static public RArray AssocNew(NetRuby ruby, object car, object cdr)
        {
            ArrayList ar = new ArrayList();
            ar.Add(car);
            ar.Add(cdr);
            return new RArray(ruby, ar);
        }
        
        public RArray Fill(object[] argv)
        {
            object[] args = new object[3];
            ruby.ScanArgs(argv, "12", args);
            int beg = 0;
            int len = ptr.Count;
            switch (argv.Length)
            {
            case 1:
                break;
            case 2:
                //range
                goto case 3;
            case 3:
                beg = (args[1] == null) ? 0 : (int)args[1];
                if (beg < 0)
                {
                    beg = ptr.Count + beg;
                    if (beg < 0) beg = 0;
                }
                len = (args[2] == null) ? ptr.Count - beg : (int)args[2];
                break;
            }
            CheckModify();
            int end = beg + len;
            if (end > ptr.Count)
            {
                ptr.AddRange(new object[end - ptr.Count]);
            }
            for (int i = beg; i < len; i++)
            {
                ptr[i] = args[0];
            }
            return this;
        }

        public bool Contains(object o)
        {
            return ptr.Contains(o);
        }

        public RArray ToRArrayWith(object x)
        {
            if (x is ArrayList)
                return new RArray(ruby, (ArrayList)x);
            return (RArray)ruby.ConvertType(x, typeof(RArray), "Array", "to_ary");
        }
        
        public RArray Concat(object o)
        {
            RArray ary = ToRArrayWith(o);
            ptr.AddRange(ary.ptr);
            return this;
        }
        
        public RString JoinMethod(object[] argv)
        {
            object[] args = new object[1];
            ruby.ScanArgs(argv, "01", args);
            return Join((args[0] == null) ? ruby.outputFS : args[0]);
        }
        
        public RString Join(object sep)
        {
            if (ptr.Count <= 0) return new RString(ruby, String.Empty, false);
            bool taint = IsTainted;
            if (sep is RBasic && ((RBasic)sep).IsTainted)
            {
                taint = true;
            }
            string ssep = String.Empty;
            if (sep != null)
            {
                if (sep is string || sep is RString)
                {
                    ssep = sep.ToString();
                }
                else
                {
                    ssep = RString.StringToRString(ruby, sep).ToString();
                }
            }
            string result = String.Empty;
            string tmp;
            for (int i = 0; i < ptr.Count; i++)
            {
                object o = ptr[i];
                if (o is RBasic && ((RBasic)o).IsTainted) taint = true;
        
                if (o is string || o is RString)
                {
                    tmp = o.ToString();
                }
                else if (o is RArray)
                {
                    if (ruby.IsInspecting(o))
                    {
                        tmp = "[...]";
                    }
                    else
                    {
                        tmp = ruby.ProtectInspect(this,
                                  new InspectMethod(inspect_join),
                                  new object[2] {o, sep}).ToString();
                    }
                }
                else
                {
                    tmp = RString.AsString(ruby, o);
                }
                if (i > 0)
                {
                    result += ssep;
                }
                result += tmp;
            }
            return new RString(ruby, result, taint);
        }

        public RArray Reverse()
        {
            RArray ary = (RArray)Clone();
            return ary.ReverseAt();
        }
        public RArray ReverseAt()
        {
            CheckModify();
            ptr.Reverse();
            return this;
        }
        public RArray Sort()
        {
            RArray ary = (RArray)Clone();
            return ary.SortAt();
        }
        public RArray SortAt()
        {
            CheckModify();
            ptr.Sort();
            return this;
        }
        public RArray Collect()
        {
            if (ruby.IsBlockGiven == false)
                return (RArray)Clone();
            RArray collect = new RArray(ruby, ptr.Count);
            lock (ptr.SyncRoot)
            {
                foreach (object o in ptr)
                {
                    collect.Add(ruby.Yield(o));
                }
            }
            return collect;
        }
        public RArray CollectAt()
        {
            CheckModify();
            lock (ptr.SyncRoot)
            {
                for (int i = 0; i < ptr.Count; i++)
                {
                    ptr[i] = ruby.Yield(ptr[i]);
                }
            }
            return this;
        }
        private object inspect_ary(RBasic ary, object[] args)
        {
            bool taint = false;
            StringBuilder result = new StringBuilder("[");
            for (int i = 0; i < ptr.Count; i++)
            {
                object o = ruby.Inspect(ptr[i]);
                if (o is RBasic && ((RBasic)o).IsTainted) taint = true;
                if (i > 0)
                    result.AppendFormat(", {0}", (o == null) ? "nil" : o.ToString());
                else
                    result.Append(o.ToString());
            }
            result.Append("]");
            if (IsTainted || taint)
                return new RString(ruby, result.ToString(), true);
            else
                return result.ToString();
        }
        private object inspect_join(RBasic ary, object[] args)
        {
            RString rs = ((RArray)args[0]).Join(args[1]);
            return rs;
        }

        public RArray Initialize(object[] argv)
        {
            object[] args = new object[2];
            if (ruby.ScanArgs(argv, "02", args) == 0)
            {
                ptr = new ArrayList();
                return this;
            }
            CheckModify();
            int len = (int)args[0];
            if (len < 0)
            {
                throw new ArgumentException("negative array size");
            }
            if (len > ptr.Capacity)
            {
                ptr = ArrayList.Repeat(args[1], len);
            }
            return this;
        }

        private void CheckModify()
        {
            if (IsFrozen) ruby.ErrorFrozen("array");
/*            
            // Sort is done by .NET Framework
            if (IsTainted == false && ruby.SafeLevel >= 4)
                throw new SecurityException("Insecure: can't modify array");
*/                
        }
        
        internal static object s_new(RBasic r, params object[] args)
        {
            NetRuby rb = r.ruby;
            RArray a = new RArray(rb, false, (RMetaObject)r);
            rb.CallInit(a, args);
            if (a.ptr == null) a.ptr = new ArrayList();
            return a;
        }

        internal static RArray Create(NetRuby rb, object[] args)
        {
            RArray a = new RArray(rb, args);
            return a;
        }

        public object this[int index]
        {
            get {
                if (ptr.Count == 0) return null;
                if (index < 0)
                {
                    index += ptr.Count;
                }
                if (index < 0 || ptr.Count <= index)
                {
                    return null;
                }
                return ptr[index];
            }
            set {
                if (index >= ptr.Count)
                {
                    ptr.Insert(index, value);
                }
                else
                {
                    ptr[index] = value;
                }
            }
        }
        public object First
        {
            get { return (ptr.Count == 0) ? null : ptr[0]; }
        }
        public object Last
        {
            get { return (ptr.Count == 0) ? null : ptr[ptr.Count - 1]; }
        }
        internal object ARef(object[] argv)
        {
            int beg, len;
            object[] args = new object[2];
            if (ruby.ScanArgs(argv, "11", args) == 2)
            {
                beg = RInteger.ToInt(ruby, args[0]);
                len = RInteger.ToInt(ruby, args[1]);
                if (beg < 0)
                    beg += ptr.Count;
                return Subseq(beg, len);
            }
            int offset;
            if (args[0] is int)
                offset = (int)args[0];
            else if (args[0] is RBignum)
                throw new eIndexError("index too big");
            else
  // Range object check
                offset = RInteger.ToInt(ruby, args[0]);
            if (offset < 0)
                offset = ptr.Count + offset;
            if (offset < 0 || ptr.Count <= offset)
                return null;
            return ptr[offset];
        }
        internal object Subseq(int beg, int len)
        {
            if (beg > ptr.Count) return null;
            if (beg < 0 || len < 0) return null;
            if (beg + len > ptr.Count)
            {
                len = ptr.Count - beg;
            }
            if (len < 0) len = 0;
            if (len == 0) return new RArray(ruby, true);
            RArray ary2 = new RArray(ruby, ptr.GetRange(beg, len));
            ary2.klass = Class;
            return ary2;
        }
        internal void Replace(int beg, int len, object rpl)
        {
            if (len < 0) throw new ArgumentOutOfRangeException("negative length " + len.ToString());
            if (beg < 0)
                beg += ptr.Count;
            if (beg < 0)
            {
                beg -= ptr.Count;
                throw new ArgumentOutOfRangeException("index " + beg.ToString() + " out of array");
            }
            if (beg + len > ptr.Count)
            {
                len = ptr.Count - beg;
            }
            ArrayList ary2;
            if (rpl == null)
            {
                ary2 = new ArrayList();
            }
            else if (rpl is RArray == false)
            {
                ary2 = new ArrayList();
                ary2.Add(rpl);
            }
            else
            {
                ary2 = ((RArray)rpl).ArrayList;
            }
            CheckModify();
            if (beg >= ptr.Count)
            {
                if (beg > ptr.Count)
                    ptr.AddRange(new object[beg - ptr.Count]);
                ptr.AddRange(ary2);
            }
            else
            {
                if (beg + len > ptr.Count)
                {
                    len = ptr.Count - beg;
                }
                ptr.RemoveRange(beg, len);
                ptr.InsertRange(beg, ary2);
            }
#if ARRAY_DEBUG
            System.Console.WriteLine("replace");
            foreach (object x in ptr)
            {
                System.Console.WriteLine(" - " + ((x == null) ? "null" : x.ToString()));
            }
            System.Console.WriteLine("done");
#endif
        }
        internal object ASet(object[] argv)
        {
            if (argv.Length == 3)
            {
                Replace((int)argv[0], (int)argv[1], argv[2]);
                return argv[2];
            }
            if (argv.Length != 2)
            {
                throw new ArgumentException("wrong # of argments(" + argv.Length.ToString() + " for 2)");
            }
  // Range object check
            CheckModify();
            int idx = RInteger.ToInt(ruby, argv[0]);
            if (idx < 0)
            {
                idx += ptr.Count;
                if (idx < 0)
                    throw new eIndexError(String.Format("index {0} out of array",
                                                        idx - ptr.Count));
            }
            if (idx >= ptr.Count)
            {
                ptr.Insert(idx, argv[1]);
            }
            else
            {
                ptr[idx] = argv[1];
            }
            return argv[1];
        }
        internal object At(int pos)
        {
            return this[pos];
        }
        internal object Push(params object[] argv)
        {
            if (argv.Length == 0)
            {
                throw new ArgumentException("wrong # of arguments(at least 1)");
            }
            CheckModify();
            ptr.AddRange(argv);
            return this;
        }
        public int Add(object o)
        {
            return ptr.Add(o);
        }
        internal object Pop()
        {
            CheckModify();
            if (ptr.Count == 0) return null;
            object result = ptr[ptr.Count - 1];
            ptr.Remove(ptr.Count - 1);
            return result;
        }
        internal object Shift()
        {
            CheckModify();
            if (ptr.Count == 0) return null;
            object result = ptr[0];
            ptr.RemoveAt(0);
            return result;
        }
        internal object Unshift(params object[] args)
        {
            if (args == null || args.Length == 0)
                throw new eArgError("wrong # of arguments(at least 1)");
        
            CheckModify();
            ptr.InsertRange(0, args);
            return this;
        }
        internal RArray Each()
        {
            foreach (object o in ptr)
            {
                ruby.Yield(o);
            }
            return this;
        }
        internal RArray EachIndex()
        {
            for (int i = 0; i < ptr.Count; i++)
            {
                ruby.Yield(i);
            }
            return this;
        }
        internal RArray ReverseEach()
        {
            for (int i = ptr.Count - 1; i >= 0; i--)
            {
                ruby.Yield(ptr[i]);
            }
            return this;            
        }
        internal bool ArrayEqual(object o)
        {
            if (this == o) return true;
            if (o is RArray == false) return false;
            RArray a = (RArray)o;
            if (ptr == a.ptr) return true;
            if (ptr.Count != a.ptr.Count) return false;
            for (int i = 0; i < ptr.Count; i++)
            {
                if (ruby.Equal(ptr[i], a.ptr[i]) == false) return false;
            }
            return true;
        }
        internal bool ArrayEql(object o)
        {
            if (o is RArray == false) return false;
            RArray a = (RArray)o;
            if (ptr.Count != a.ptr.Count) return false;
            for (int i = 0; i < ptr.Count; i++)
            {
                if (ruby.Eql(ptr[i], a.ptr[i])== false) return false;
            }
            return true;
        }
        public IEnumerator GetEnumerator()
        {
            return ptr.GetEnumerator();
        }
        public int Count
        {
            get { return ptr.Count; }
        }
        public bool IsSynchronized
        {
            get { return ptr.IsSynchronized; }
        }
        public object SyncRoot
        {
            get { return ptr.SyncRoot; }
        }
        public void CopyTo(Array array, int index)
        {
            ptr.CopyTo(array, index);
        }
        public bool IsEmpty
        {
            get { return (ptr.Count == 0); }
        }
        public int IndexOf(object o)
        {
            for (int i = 0; i < ptr.Count; i++)
            {
                if (ruby.Equal(ptr[i], o)) return i;
            }
            return -1;
        }
        public void Insert(int index, object value)
        {
            ptr.Insert(index, value);
        }
        public void Remove(object o)
        {
            int i = IndexOf(o);
            if (i >= 0) RemoveAt(i);
        }
        public void RemoveAt(int i)
        {
            if (i < 0 || i >= ptr.Count) throw new ArgumentOutOfRangeException();
            try
            {
                DeleteAt(i);
            }
            catch (SecurityException)
            {
                throw new NotSupportedException();
            }
            catch (eTypeError)
            {
                throw new NotSupportedException();
            }
        }
        internal object Delete(object item)
        {
            int pos = ptr.Count;
            Remove(item);
            if (ptr.Count == pos)
            {
                if (ruby.IsBlockGiven)
                {
                    ruby.Yield(item);
                }
                return null;
            }
            return item;
        }
        internal object DeleteAt(int pos)
        {
            CheckModify();
            if (pos >= ptr.Count) return null;
            if (pos < 0) pos += ptr.Count;
            if (pos < 0) return null;
            object del = ptr[pos];
            ptr.RemoveAt(pos);
            return del;
        }
        public object Index(object o)
        {
            int i = IndexOf(o);
            if (i < 0)
                return null;
            return i;
        }
        public object RIndex(object o)
        {
            for (int i = ptr.Count - 1; i >= 0; i--)
            {
                if (ruby.Equal(ptr[i], o)) return i;
            }
            return null;
        }
        ArrayList ptr;

        static internal void Init(NetRuby rb)
        {
            BindingFlags bf = BindingFlags.InvokeMethod
                | BindingFlags.Static | BindingFlags.Public
                | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy
                | BindingFlags.Instance;
            RClass ary = rb.DefineClass("Array", rb.cObject);
            RMetaObject.IncludeModule(ary, rb.mEnumerable);
            rb.cArray = ary;
            Type obj = typeof(RArray);
            ary.DefineSingletonMethod("new", new RMethod(s_new), -1);
            ary.DefineSingletonMethod("[]", obj.GetMethod("Create", bf));
        
            ary.DefineMethod("initialize", obj.GetMethod("Initialize", bf));

            ary.DefineMethod("to_ary", obj.GetMethod("ToArray", bf));
            ary.DefineMethod("==", obj.GetMethod("ArrayEqual", bf));
            ary.DefineMethod("eql?", obj.GetMethod("ArrayEql", bf));

            ary.DefineMethod("[]", obj.GetMethod("ARef", bf));
            ary.DefineMethod("[]=", obj.GetMethod("ASet", bf));
            ary.DefineMethod("at", obj.GetMethod("At", bf));
            ary.DefineMethod("first", obj.GetMethod("get_First", bf));
            ary.DefineMethod("last", obj.GetMethod("get_Last", bf));
            ary.DefineMethod("concat", obj.GetMethod("Concat", bf));
            ary.DefineMethod("<<", obj.GetMethod("Push", bf));
            ary.DefineMethod("push", obj.GetMethod("Push", bf));
            ary.DefineMethod("pop", obj.GetMethod("Pop", bf));
            ary.DefineMethod("shift", obj.GetMethod("Shift", bf));
            ary.DefineMethod("unshift", obj.GetMethod("Unshift", bf));
            ary.DefineMethod("each", obj.GetMethod("Each", bf));
            ary.DefineMethod("each_index", obj.GetMethod("EachIndex", bf));
            ary.DefineMethod("reverse_each", obj.GetMethod("ReverseEach", bf));
            ary.DefineMethod("length", obj.GetMethod("get_Count", bf));
            ary.DefineAlias("size", "length");
            ary.DefineMethod("empty?", obj.GetMethod("get_IsEmpty", bf));
            ary.DefineMethod("index", obj.GetMethod("Index", bf));
            ary.DefineMethod("rindex", obj.GetMethod("RIndex", bf));
        
            ary.DefineMethod("clone", obj.GetMethod("Clone", bf));
            ary.DefineMethod("join", obj.GetMethod("JoinMethod", bf));

            ary.DefineMethod("reverse", obj.GetMethod("Reverse", bf));
            ary.DefineMethod("reverse!", obj.GetMethod("ReverseAt", bf));
            ary.DefineMethod("sort", obj.GetMethod("Sort", bf));
            ary.DefineMethod("sort!", obj.GetMethod("SortAt", bf));
            ary.DefineMethod("collect", obj.GetMethod("Collect", bf));
            ary.DefineMethod("collect!", obj.GetMethod("CollectAt", bf));

            ary.DefineMethod("delete", obj.GetMethod("Delete", bf));
            ary.DefineMethod("delete_at", obj.GetMethod("DeleteAt", bf));
        
            ary.DefineMethod("clear", obj.GetMethod("Clear2", bf));
            ary.DefineMethod("fill", obj.GetMethod("Fill", bf));
            ary.DefineMethod("include", obj.GetMethod("Contains", bf));
        }
    }
}
