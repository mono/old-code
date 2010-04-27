using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Reflection;


namespace PHP.Runtime {


	public class Convert {

		public static Boolean ToBool(object o) {
			Core.DeReference(ref o);
			if (o == null)
				return false;
			if (o is bool)
				return (bool)o;
			if (o is int) {
				if ((int)o == 0)
					return false;
				else
					return true;
			}
			if (o is double) {
				if ((double)o == 0)
					return false;
				else
					return true;
			}
			if (o is string) {
				if ((string)o == "" || (string)o == "0")
					return false;
				else
					return true;
			}
			if (o is Array) {
				if (((Array)o).Keys.Count == 0)
					return false;
				else
					return true;
			}
			return true;
		}

		public static Int32 ToInt(object o) {
			Core.DeReference(ref o);
			if (o == null)
				return 0;
			if (o is bool) {
				if ((bool)o)
					return 1;
				else
					return 0;
			}
			if (o is int)
				return (int)o;
			if (o is double)
				return (int)Math.Floor((double)o);
			if (o is string) {
				string toDoubleShortenedString = ShortenToDoubleString((string)o);
				double d = System.Convert.ToDouble(toDoubleShortenedString);
				return (int)Math.Floor(d);
			}
			return ToInt(ToBool(o));
		}

		public static Double ToDouble(object o) {
			Core.DeReference(ref o);
			if (o == null)
				return 0;
			if (o is bool) {
				if ((bool)o)
					return 1;
				else
					return 0;
			}
			if (o is int)
				return (int)o;
			if (o is double)
				return (double)o;
			if (o is string) {
				string toDoubleShortenedString = ShortenToDoubleString((string)o);
				double d = System.Convert.ToDouble(toDoubleShortenedString);
				return d;
			}
			return ToDouble(ToInt(o));
		}

		public static string ToString(object o) {
			Core.DeReference(ref o);
			if (o == null)
				return "";
			if (o is bool) {
				if ((bool)o)
					return "1";
				else
					return "";
			}
			if (o is int)
				return ((int)o).ToString();
			if (o is double)
				return ((double)o).ToString();
			if (o is string)
				return (string)o;
			if (o is Array)
				return "Array";
			if (o is Object)
				return "Object id #" + ((Object)o).__Id;
			return o.ToString();
		}

		public static Array ToArray(object o) {
			Core.DeReference(ref o);
			if (o == null)
				return new Array();
			if (o is bool || o is int || o is double || o is string) {
				Array a = new Array();
				a.Append(o);
				return a;
			}
			if (o is Array)
				return (Array)o;
			Array result = new Array();
			foreach (FieldInfo f in o.GetType().GetFields()) {
				// don't use the internal fields __Id and __MaxId
				if (f.Name != "__Id" && f.Name != "__MaxId") {
					object value = f.GetValue(o);
					result.Append(f.Name, value);
				}
			}
			return result;
		}

		public static object ToObject(object o) {
			Core.DeReference(ref o);
			if (o == null || o is ValueType)
				return new StdClass(o);
			return o;
		}

		public static System.Exception ToException(object o) {
			Core.DeReference(ref o);
			if (o == null || !(o is System.Exception))
				Report.Error(123, o.GetType().FullName);
			return (System.Exception)o;
		}

		public static object FitTypeForExternalUse(object o, string type) {
			Type t = Type.GetType(type);
			return FitTypeForExternalUse(o, t);
		}
		
		public static object FitTypeForExternalUse(object o, Type t) {
			Core.DeReference(ref o);
			// if type of object is desired type or subtype, fine
			if (o.GetType() == t || o.GetType().IsSubclassOf(t))
				return o;
			// try to convert an number desired number type
			else if (o is int || o is double) {
				if (t == typeof(int))
					return System.Convert.ToInt32(o);
				else if (t == typeof(uint))
					return System.Convert.ToUInt32(o);
				else if (t == typeof(long))
					return System.Convert.ToInt64(o);
				else if (t == typeof(ulong))
					return System.Convert.ToUInt64(o);
				else if (t == typeof(short))
					return System.Convert.ToInt16(o);
				else if (t == typeof(ushort))
					return System.Convert.ToUInt16(o);
				else if (t == typeof(byte))
					return System.Convert.ToByte(o);
				else if (t == typeof(sbyte))
					return System.Convert.ToSByte(o);
				else if (t == typeof(bool))
					return System.Convert.ToBoolean(o);
				else if (t == typeof(float))
					return System.Convert.ToSingle(o);
				else if (t == typeof(double))
					return System.Convert.ToDouble(o);
				else if (t == typeof(decimal))
					return System.Convert.ToDecimal(o);
				else
					return null;
			}
			// try to convert a string to the desired string type
			else if (o is string) {
				if (t == typeof(char))
					return System.Convert.ToChar((string)o);
				else
					return null;
			}
			// try to convert an Array to the desired array type
			else if (o is Array) {
				if (t == typeof(System.Array) || t.IsArray) {
					Array arr = (Array)o;
					System.Array result = System.Array.CreateInstance(t.GetElementType(), arr.Values.Count);
					for (int i = 0; i < arr.Values.Count; i++)
						result.SetValue(FitTypeForExternalUse(arr.Values[i], t.GetElementType()), i);
					return result;
				}
				else
					return null;
			}
			else
				return null;
		}

		public static object FitTypeForInternalUse(object o) {
			Core.DeReference(ref o);
			// leave a null unchanged
			if (o == null)
				return null;
			// try to convert an integer type other than int to int
			if (o is uint || o is long || o is ulong || o is short || o is ushort || o is byte || o is sbyte || o is bool)
				return System.Convert.ToInt32(o);
			// try to convert a floating point type other than double to double
			else if (o is float || o is decimal)
				return System.Convert.ToDouble(o);
			// try to convert a string type other than string to string
			else if (o is char)
				return System.Convert.ToString(o);
			// try to convert an array type to Array
			else if (o.GetType().IsArray) {
				System.Array arr = (System.Array)o;
				ArrayList keys = new ArrayList();
				ArrayList values = new ArrayList();
				for (int i = 0; i < arr.GetLength(0); i++) {
					keys.Add(i);
					object value = arr.GetValue(i);
					// resursively fit type of current value
					values.Add(FitTypeForInternalUse(value));
				}
				return new Array(keys, values);
			}
			// otherwise leave type unchanged
			else
				return o;
		}

		private static string ShortenToDoubleString(string s) {
			int i = 0;
			// optional sign
			if (i < s.Length) {
				if (s[i] == '+' || s[i] == '-')
					i++;
			}
			else
				return "0";
			// if sign was the only input, return 0
			if (i == s.Length)
				return "0";
			// optional digits
			while (i < s.Length && s[i] >= '0' && s[i] <= '9')
				i++;
			// optional dot
			if (i < s.Length) {
				if (s[i] == '.')
					i++;
			}
			else
				return s.Substring(0, i);
			// optional digits
			while (i < s.Length && s[i] >= '0' && s[i] <= '9')
				i++;
			// optional e
			bool eFound = false;
			if (i < s.Length) {
				if (s[i] == 'e' || s[i] == 'E') {
					i++;
					eFound = true;
				}
			}
			else
				return s.Substring(0, i);
			// optional digits
			bool digitsFoundAfterE = false;
			while (i < s.Length && s[i] >= '0' && s[i] <= '9') {
				i++;
				digitsFoundAfterE = true;
			}
			// cut e if not followed by digits
			if (eFound && !digitsFoundAfterE)
				i--;
			// done
			return s.Substring(0, i);
		}

	}


}