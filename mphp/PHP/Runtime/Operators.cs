using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Reflection;


namespace PHP.Runtime {


	public class Operators {

		public static TextWriter stdOut = Console.Out;
		public static TextWriter nowhere = new StringWriter();

		public static void Echo(object o) {
			Core.DeReference(ref o);
			if (o == null) { /* do nothing */ }
			else if (o is bool) {
				if ((bool)o)
					Console.Write(1);
			}
			else if (o is Array)
				Console.Write("Array");
			else if (o is Object)
				Console.Write("Object id #" + ((Object)o).__Id);
			else
				Console.Write(o);
		}

		public static object Instanceof(object o, Type t) {
			Core.DeReference(ref o);
			return t.IsInstanceOfType(o);
		}

		public static object BooleanNot(object o) {
			Core.DeReference(ref o);
			return !Convert.ToBool(o);
		}

		public static object Not(object o) {
			Core.DeReference(ref o);
			if (o is string) {
				StringBuilder result = new StringBuilder();
				string s = Convert.ToString(o);
				// process on ASCII value of characters
				for (int i = 0; i < s.Length; i++)
					result.Append((char)(~s[i]));
				// done
				return result.ToString();
			}
			else {
				int i = Convert.ToInt(o);
				return ~i;
			}
		}

		public static object Exit(object o) {
			Core.DeReference(ref o);
			int exitCode = 0;
			if (o is int)
				exitCode = (int)o;
			Environment.Exit(exitCode);
			return null;
		}

		public static object Print(object o) {
			Core.DeReference(ref o);
			Echo(o);
			return 1;
		}

		public static object BooleanAnd(object o1, object o2) {
			Core.DeReference(ref o1, ref o2);
			bool b1 = Convert.ToBool(o1);
			bool b2 = Convert.ToBool(o2);
			return b1 && b2;
		}

		public static object BooleanOr(object o1, object o2) {
			Core.DeReference(ref o1, ref o2);
			bool b1 = Convert.ToBool(o1);
			bool b2 = Convert.ToBool(o2);
			return b1 || b2;
		}

		public static object LogicalAnd(object o1, object o2) {
			Core.DeReference(ref o1, ref o2);
			bool b1 = Convert.ToBool(o1);
			bool b2 = Convert.ToBool(o2);
			return b1 && b2;
		}

		public static object LogicalOr(object o1, object o2) {
			Core.DeReference(ref o1, ref o2);
			bool b1 = Convert.ToBool(o1);
			bool b2 = Convert.ToBool(o2);
			return b1 || b2;
		}

		public static object LogicalXor(object o1, object o2) {
			Core.DeReference(ref o1, ref o2);
			bool b1 = Convert.ToBool(o1);
			bool b2 = Convert.ToBool(o2);
			return b1 ^ b2;
		}

		public static object Concat(object o1, object o2) {
			Core.DeReference(ref o1, ref o2);
			string s1 = Convert.ToString(o1);
			string s2 = Convert.ToString(o2);
			return s1 + s2;
		}

		public static object Plus(object o1, object o2) {
			Core.DeReference(ref o1, ref o2);
			if (o1 is Array && !(o2 is Array) || o2 is Array && !(o1 is Array))
				throw Report.Exception(500, "+");
			// if one operand is an array, perform array union
			if (o1 is Array || o2 is Array) {
				Array a1 = (Array)o1;
				Array a2 = (Array)o2;
				Array result = new Array();
				result.Keys.AddRange(a1.Keys);
				result.Values.AddRange(a1.Values);
				// only add a key (and its object) if it wasn't in a1
				for (int i = 0; i < a2.Keys.Count; i++) {
					object key = a2.Keys[i];
					object value = a2.Values[i];
					if (!result.Keys.Contains(key)) {
						result.Keys.Add(key);
						result.Values.Add(value);
					}
				}
				return result;
			}
			// else perform regular additions
			else {
				double f1 = Convert.ToDouble(o1);
				double f2 = Convert.ToDouble(o2);
				double result = f1 + f2;
				if (result % 1 == 0)
					return (int)result;
				else
					return result;
			}
		}

		public static object Minus(object o1, object o2) {
			Core.DeReference(ref o1, ref o2);
			if (o1 is Array || o2 is Array)
				throw Report.Exception(500, "-");
			double f1 = Convert.ToDouble(o1);
			double f2 = Convert.ToDouble(o2);
			double result = f1 - f2;
			if (result % 1 == 0)
				return (int)result;
			else
				return result;
		}

		public static object Times(object o1, object o2) {
			Core.DeReference(ref o1, ref o2);
			if (o1 is Array || o2 is Array)
				throw Report.Exception(500, "*");
			double f1 = Convert.ToDouble(o1);
			double f2 = Convert.ToDouble(o2);
			double result = f1 * f2;
			if (result % 1 == 0)
				return (int)result;
			else
				return result;
		}

		public static object Div(object o1, object o2) {
			Core.DeReference(ref o1, ref o2);
			if (o1 is Array || o2 is Array)
				throw Report.Exception(500, "/");
			double f1 = Convert.ToDouble(o1);
			double f2 = Convert.ToDouble(o2);
			double result = f1 / f2;
			if (result % 1 == 0)
				return (int)result;
			else
				return result;
		}

		public static object Mod(object o1, object o2) {
			Core.DeReference(ref o1, ref o2);
			double f1 = Convert.ToDouble(o1);
			double f2 = Convert.ToDouble(o2);
			double result = f1 % f2;
			if (result % 1 == 0)
				return (int)result;
			else
				return result;
		}

		public static object And(object o1, object o2) {
			Core.DeReference(ref o1, ref o2);
			if (o1 is String && o2 is String) {
				StringBuilder result = new StringBuilder();
				string s1 = Convert.ToString(o1);
				string s2 = Convert.ToString(o2);
				// cut strings to same length
				if (s1.Length > s2.Length)
					s1 = s1.Substring(0, s2.Length);
				else if (s2.Length > s1.Length)
					s2 = s2.Substring(0, s1.Length);
				// process on ASCII value of characters
				for (int i = 0; i < s1.Length; i++)
					result.Append((char)(s1[i] & s2[i]));
				// done
				return result.ToString();
			}
			else {
				int i1 = Convert.ToInt(o1);
				int i2 = Convert.ToInt(o2);
				return i1 & i2;
			}
		}

		public static object Or(object o1, object o2) {
			Core.DeReference(ref o1, ref o2);
			if (o1 is String && o2 is String) {
				StringBuilder result = new StringBuilder();
				string s1 = Convert.ToString(o1);
				string s2 = Convert.ToString(o2);
				// cut strings to same length
				if (s1.Length > s2.Length)
					s1 = s1.Substring(0, s2.Length);
				else if (s2.Length > s1.Length)
					s2 = s2.Substring(0, s1.Length);
				// process on ASCII value of characters
				for (int i = 0; i < s1.Length; i++)
					result.Append((char)(s1[i] | s2[i]));
				// done
				return result.ToString();
			}
			else {
				int i1 = Convert.ToInt(o1);
				int i2 = Convert.ToInt(o2);
				return i1 | i2;
			}
		}

		public static object Xor(object o1, object o2) {
			Core.DeReference(ref o1, ref o2);
			if (o1 is string && o2 is string) {
				StringBuilder result = new StringBuilder();
				string s1 = Convert.ToString(o1);
				string s2 = Convert.ToString(o2);
				// cut strings to same length
				if (s1.Length > s2.Length)
					s1 = s1.Substring(0, s2.Length);
				else if (s2.Length > s1.Length)
					s2 = s2.Substring(0, s1.Length);
				// process on ASCII value of characters
				for (int i = 0; i < s1.Length; i++)
					result.Append((char)(s1[i] ^ s2[i]));
				// done
				return result.ToString();
			}
			else {
				int i1 = Convert.ToInt(o1);
				int i2 = Convert.ToInt(o2);
				return i1 ^ i2;
			}
		}

		public static object Sl(object o1, object o2) {
			Core.DeReference(ref o1, ref o2);
			if (o1 is String && o2 is String) {
				StringBuilder result = new StringBuilder();
				string s1 = Convert.ToString(o1);
				string s2 = Convert.ToString(o2);
				// cut strings to same length
				if (s1.Length > s2.Length)
					s1 = s1.Substring(0, s2.Length);
				else if (s2.Length > s1.Length)
					s2 = s2.Substring(0, s1.Length);
				// process on ASCII value of characters
				for (int i = 0; i < s1.Length; i++)
					result.Append((char)(s1[i] << s2[i]));
				// done
				return result.ToString();
			}
			else {
				int i1 = Convert.ToInt(o1);
				int i2 = Convert.ToInt(o2);
				return i1 << i2;
			}
		}

		public static object Sr(object o1, object o2) {
			Core.DeReference(ref o1, ref o2);
			if (o1 is string && o2 is string) {
				StringBuilder result = new StringBuilder();
				string s1 = Convert.ToString(o1);
				string s2 = Convert.ToString(o2);
				// cut strings to same length
				if (s1.Length > s2.Length)
					s1 = s1.Substring(0, s2.Length);
				else if (s2.Length > s1.Length)
					s2 = s2.Substring(0, s1.Length);
				// process on ASCII value of characters
				for (int i = 0; i < s1.Length; i++)
					result.Append((char)(s1[i] >> s2[i]));
				// done
				return result.ToString();
			}
			else {
				int i1 = Convert.ToInt(o1);
				int i2 = Convert.ToInt(o2);
				return i1 >> i2;
			}
		}

		public static object IsEqual(object o1, object o2) {
			Core.DeReference(ref o1, ref o2);
			if (o1 == null)
				return o2 == null;
			if (o2 == null)
				return o1 == null;
			// if both operands are arrays, check equality on array pairs
			if (o1 is Array && o2 is Array)
				return o1.Equals(o2);
			// if both operands are objects, check identity on instances
			if (o1 is Object && o2 is Object)
				return o1.Equals(o2);
			// otherwise perform regular equality check
			if (o1 is bool) {
				bool b1 = (bool)o1;
				bool b2 = Convert.ToBool(o2);
				return b1 == b2;
			}
			if (o2 is bool) {
				bool b1 = Convert.ToBool(o1);
				bool b2 = (bool)o2;
				return b1 == b2;
			}
			if (o1 is int) {
				int i1 = (int)o1;
				int i2 = Convert.ToInt(o2);
				return i1 == i2;
			}
			if (o2 is int) {
				int i1 = Convert.ToInt(o1);
				int i2 = (int)o2;
				return i1 == i2;
			}
			if (o1 is double) {
				double i1 = (double)o1;
				double i2 = Convert.ToDouble(o2);
				return i1 == i2;
			}
			if (o2 is double) {
				double i1 = Convert.ToDouble(o1);
				double i2 = (double)o2;
				return i1 == i2;
			}
			if (o1 is string) {
				string i1 = (string)o1;
				string i2 = Convert.ToString(o2);
				return i1 == i2;
			}
			if (o2 is string) {
				string i1 = Convert.ToString(o1);
				string i2 = (string)o2;
				return i1 == i2;
			}
			return false;
		}

		public static object IsNotEqual(object o1, object o2) {
			Core.DeReference(ref o1, ref o2);
			bool isEqual = (bool)IsEqual(o1, o2);
			return !isEqual;
		}

		public static object IsIdentical(object o1, object o2) {
			Core.DeReference(ref o1, ref o2);
			// if both operands are arrays, check identity on array pairs
			if (o1 is Array && o2 is Array) {
				Array a1 = (Array)o1;
				Array a2 = (Array)o2;
				if (a1.Keys.Count != a2.Keys.Count)
					return false;
				for (int i = 0; i < a1.Keys.Count; i++) {
					if (!(bool)IsIdentical(a1.Keys[i], a2.Keys[i]))
						return false;
					if (!(bool)IsIdentical(a1.Values[i], a2.Values[i]))
						return false;
				}
				return true;
			}
			// if both operands are objects, check identity on instances
			else if (o1 is Object && o2 is Object) {
				int id1 = ((Object)o1).__Id;
				int id2 = ((Object)o2).__Id;
				return id1 == id2;
			}
			// otherwise perform regular identity check
			else {
				if (o1.GetType() == o2.GetType())
					return IsEqual(o1, o2);
				else
					return false;
			}
		}

		public static object IsNotIdentical(object o1, object o2) {
			Core.DeReference(ref o1, ref o2);
			bool isIdentical = (bool)IsIdentical(o1, o2);
			return !isIdentical;
		}

		public static object Lower(object o1, object o2) {
			Core.DeReference(ref o1, ref o2);
			double f1 = Convert.ToDouble(o1);
			double f2 = Convert.ToDouble(o2);
			return f1 < f2;
		}

		public static object IsLowerOrEqual(object o1, object o2) {
			Core.DeReference(ref o1, ref o2);
			double f1 = Convert.ToDouble(o1);
			double f2 = Convert.ToDouble(o2);
			return f1 <= f2;
		}

		public static object Greater(object o1, object o2) {
			Core.DeReference(ref o1, ref o2);
			double f1 = Convert.ToDouble(o1);
			double f2 = Convert.ToDouble(o2);
			return f1 > f2;
		}

		public static object IsGreaterOrEqual(object o1, object o2) {
			Core.DeReference(ref o1, ref o2);
			double f1 = Convert.ToDouble(o1);
			double f2 = Convert.ToDouble(o2);
			return f1 >= f2;
		}

		public static object Clone(object o) {
			Core.DeReference(ref o);
			if (o is bool || o is int || o is double || o is string)
				return o;
			if (o is Array) {
				Array a = (Array)o;
				object clonedKey;
				object clonedValue;
				ArrayList clonedKeys = new ArrayList();
				ArrayList clonedValues = new ArrayList();
				for (int i = 0; i < a.Keys.Count; i++) {
					clonedKey = Clone(a.Keys[i]);
					clonedValue = Clone(a.Values[i]);
					clonedKeys.Add(clonedKey);
					clonedValues.Add(clonedValue);
				}
				return new Array(clonedKeys, clonedValues);
			}
			if (o is Object) {
				if (o == null)
					return null;
				// find constructor of object to be cloned
				ConstructorInfo ctor = null;
				foreach (ConstructorInfo ci in o.GetType().GetConstructors()) {
					if (!ci.IsStatic) {
						ctor = ci;
						break;
					}
				}
				int parameterCount = ctor.GetParameters().Length;
				object[] parameters = new object[parameterCount];
				for (int i = 0; i < parameters.Length; i++)
					parameters[i] = null;
				// set the standard out to nowhere to avoid output when calling the constructor right now
				Console.SetOut(nowhere);
				// create new instance
				Object result = (Object)Activator.CreateInstance(o.GetType(), parameters);
				// set field values
				foreach (FieldInfo f in o.GetType().GetFields()) {
					// don't use the internal fields __Id and __placeOnHeap
					if (f.Name != "__Id" && f.Name != "__placeOnHeap")
						f.SetValue(result, f.GetValue(o));
				}
				// if a __clone function is available, invoke
				MethodInfo clone = o.GetType().GetMethod("__clone", Type.EmptyTypes);
				if (clone != null)
					clone.Invoke(result, null);
				// reset the standard output
				Console.SetOut(stdOut);
				return result;
			}
			if (o is System.ICloneable)
				return ((ICloneable)o).Clone();
			return null;
		}

	}


}