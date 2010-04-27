using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Reflection;
using PHP.Runtime;


namespace PHP {


	public class Reference {

		internal object ReferencedScope;
		internal object ReferencedName;

		internal Reference(object referencedScope, object referencedName) {
			ReferencedScope = referencedScope;
			ReferencedName = referencedName;
		}

		public static Reference CreateReference(object referencedScope, object referencedName) {
			// handle referencing to a new array element
			if (referencedScope is Array && referencedName == null) {
				Array arr = (Array)referencedScope;
				arr.Append(null);
				referencedName = arr.MaxKey;
			}
			return new Reference(referencedScope, referencedName);
		}

		public static Reference CreateReferenceToLocal(object referencedName) {
			return new Reference(Core.FunctionCallTraceAsString(), referencedName);
		}

		public static Reference CreateReferenceToGlobal(object referencedName) {
			return new Reference("__MAIN->__MAIN", referencedName);
		}

		public object GetValue() {
			object o = Core.LoadFromCell(ReferencedScope, ReferencedName);
			while (o is Reference) {
				Reference r = (Reference)o;
				o = Core.LoadFromCell(r.ReferencedScope, r.ReferencedName);
			}
			return o;
		}

		public void SetValue(object value) {
			Core.StoreToCell(ReferencedScope, value, ReferencedName);
		}

	}


	public class Array {
        internal ArrayList Keys;
		internal ArrayList Values;
		internal int MaxKey;
		internal int InternalArrayPointer;
        public Array() : this(new ArrayList(), new ArrayList()) { }
        public Array(ArrayList keys, ArrayList values) : base() {
            Keys = keys;
            Values = values;
            // calculate maximum key
            MaxKey = -1;
            foreach (object key in keys) {
                if (key is int) {
                    int keyValue = (int)key;
                    if (keyValue > MaxKey)
                        MaxKey = keyValue;
                }
            }
			InternalArrayPointer = 0;
        }
		public void Append(object value) {
			Append(null, value);
		}
		public void Append(object key, object value) {
			// store to heap (storing to array is handled there)
			Core.StoreToCell(this, value, key);
		}
		public void Remove(object key) {
			// remove to heap (removing from array is handled there)
			Core.RemoveCell(this, key);
		}
		public object Get(object key) {
			if (key is bool)
				key = Runtime.Convert.ToInt((bool)key);
			else if (key is double)
				key = Runtime.Convert.ToInt((double)key);
			else if (key is string) {
					key = (string)key;
					if (IsStandardInteger((string)key))
						key = System.Convert.ToInt32((string)key);
			}
			else if (key == null)
				key = Runtime.Convert.ToString(key);
			// other data types are not allowed for keys, so ignore them
			else if (!(key is int)) {
				Report.Warn(402);
				return null;
			}
			// return desired value
			int index = Keys.IndexOf(key);
			if (index > -1) {
				object result = Values[index];
				// this is a workaround as Reference is not yet implemented as value type
				if (result is Reference) {
					Reference r = (Reference)result;
					return new Reference(r.ReferencedScope, r.ReferencedName);
				}
				else
					return result;
			}
			return null;
		}
		internal static bool IsStandardInteger(string s) {
			if (s.Length == 0)
				return false;
			if (s == "0")
				return true;
			if (s[0] < '1' || s[0] > '9')
				return false;
			for (int i = 1; i < s.Length; i++)
				if (s[0] < '0' || s[0] > '9')
					return false;
			return true;
		}
		public object Key() {
			if ((bool)CurrentIsValid())
				return Keys[InternalArrayPointer];
			else
				return false;
		}
		public object Current() {
			if ((bool)CurrentIsValid())
				return Values[InternalArrayPointer];
			else
				return false;
		}
		public object Next() {
			InternalArrayPointer++;
			return Current();
		}
		public object Prev() {
			InternalArrayPointer--;
			return Current();
		}
		public object Each() {
			ArrayList keys = new ArrayList();
			keys.Add(1);
			keys.Add("value");
			keys.Add(0);
			keys.Add("key");
			ArrayList values = new ArrayList();
			values.Add(Current());
			values.Add(Current());
			values.Add(Key());
			values.Add(Key());
			InternalArrayPointer++;
			return new Array(keys, values);
		}
		public object Reset() {
			InternalArrayPointer = 0;
			return Current();
		}
		public object CurrentIsValid() {
			return InternalArrayPointer >= 0 && InternalArrayPointer < Keys.Count;
		}

		public override sealed bool Equals(object o) {
			if (o is Array) {
				Array a = (Array)o;
				if (Keys.Count != a.Keys.Count)
					return false;
				for (int i = 0; i < Keys.Count; i++) {
					object key = Keys[i];
					object value = Values[i];
					int i2 = a.Keys.IndexOf(key);
					if (i2 == -1)
						return false;
					if (!key.Equals(a.Keys[i2]))
						return false;
					if (value == null ^ a.Values[i2] == null)
						return false;
					if (!value.Equals(a.Values[i2]))
						return false;
				}
				return true;
			}
			else
				return Equals(Runtime.Convert.ToArray(o));
		}
		public override sealed int GetHashCode() {
			return Keys.GetHashCode() ^ Values.GetHashCode();
		}
		public override sealed string ToString() {
			return "Array";
		}
	}


	public abstract class Object {
		internal static int __MaxId = 0;
		internal int __Id;
		internal Object() : base() {
			__Id = ++__MaxId;
		}
		public override sealed bool Equals(object o) {
			if (o is Object) {
				Array result = new Array();
				// first check if both Objects have the same amount of fields
				if (this.GetType().GetFields().Length != o.GetType().GetFields().Length)
					return false;
				// then check equality of fields
				foreach (FieldInfo f in this.GetType().GetFields()) {
					// don't use the internal fields __Id and __MaxId
					if (f.Name != "__Id" && f.Name != "__MaxId") {
						// check if o has the same field
						if (o.GetType().GetField(f.Name) == null)
							return false;
						// if so, check if values are the same
						if (!f.GetValue(this).Equals(f.GetValue(o)))
							return false;
					}
				}
				return true;
			}
			else
				return Equals(Runtime.Convert.ToObject(o));
		}
		public override sealed int GetHashCode() {
			return __Id;
		}
		public override sealed string ToString() {
            return "Object id #" + __Id;
        }
    }

	public class StdClass : PHP.Object {
		public object Scalar;
		public StdClass(object scalar) : base() {
			Scalar = scalar;
		}
	}


}