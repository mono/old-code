using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Reflection;


namespace PHP.Runtime {


	public class Core {

		// used for switch statement
		public static bool SwitchInProgress;
		// used for calls of a static function from a non-static context
		public static object ThisForStaticContext;

		internal static ArrayList FunctionCallTrace;

		internal static Hashtable VariablePool;
		internal static Hashtable ReferencedCells;

		internal static Hashtable CaseSensitiveConstantPool;
		internal static Hashtable CaseInsensitiveConstantPool;

		static Core() {
			SwitchInProgress = false;
			ThisForStaticContext = null;
			FunctionCallTrace = new ArrayList();
			FunctionCallTrace.Add("__MAIN->__MAIN");
			VariablePool = new Hashtable();
			ReferencedCells = new Hashtable();
			CaseSensitiveConstantPool = new Hashtable();
			CaseInsensitiveConstantPool = new Hashtable();
		}

		public static void Init() {
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
		}

		internal static Cell GetReferenceTo(object scope, object name) {
			return (Cell)ReferencedCells[new Cell(scope, name)];
		}

		public static void StoreToVariable(object value, object variableName) {
			// if a static variable is available, store there
			if (LoadFromStaticVariable(variableName) != null)
				StoreToStaticVariable(value, variableName);
			// otherwise store as a local variable in current scope
			else
				StoreToLocalVariable(value, variableName);
		}

		public static void StoreToLocalVariable(object value, object variableName) {
			string scope = FunctionCallTraceAsString();
			StoreToCell(scope, value, variableName);
		}

		public static void StoreToStaticVariable(object value, object variableName) {
			// if variable is available in global scope, store there
			string scope = "__MAIN->__MAIN";
			if (LoadFromVariablePool(scope, variableName) != null)
				StoreToCell(scope, value, variableName);
			// otherwise remove the local version of the variable and store to static variable
			else {
				scope = "__static=>" + (string)FunctionCallTrace[FunctionCallTrace.Count - 1];
				object localVar = LoadFromLocalVariable(variableName);
				if (localVar != null)
					UnsetVariable(variableName);
				StoreToCell(scope, value, variableName);
			}
		}

		internal static void StoreToCell(object scope, object value, object name) {
			StoreToCell(scope, value, name, true);
		}

		internal static void StoreToCell(object scope, object value, object name, bool adjustReferencingCell) {
			// invalid type
			if (scope == null)
				throw Report.Exception(403);
			// it is about a variable
			if (scope is string) {
				string variableName = Convert.ToString(name);
				Hashtable variablePoolOfScope = (Hashtable)VariablePool[scope];
				if (variablePoolOfScope == null) {
					variablePoolOfScope = new Hashtable();
					VariablePool[scope] = variablePoolOfScope;
				}
				// remember old value
				object oldValue = variablePoolOfScope[variableName];
				// if a reference is to be stored...
				if (value is Reference) {
					Reference r = (Reference)value;
					Cell c;
					// ...adjust referencing cell
					if (adjustReferencingCell) {
						c = GetReferenceTo(scope, variableName);
						if (c != null)
							StoreToCell(c.Scope, oldValue, c.Name, false);
					} 
					// ...store and ensure new reference is at beginning of reference line)
					c = GetReferenceTo(r.ReferencedScope, r.ReferencedName);
					while (c != null) {
						r.ReferencedScope = c.Scope;
						r.ReferencedName = c.Name;
						c = GetReferenceTo(c.Scope, c.Name);
					}
					variablePoolOfScope[variableName] = r;
					// ...remember that the desired cell references the target of the reference
					ReferencedCells[new Cell(r.ReferencedScope, r.ReferencedName)] = new Cell(scope, variableName);
				}
				// if a real value (no reference) is to be stored, store
				else {
					if (oldValue is Reference) {
						Reference r = (Reference)oldValue;
						StoreToCell(r.ReferencedScope, value, r.ReferencedName);
					}
					else
						variablePoolOfScope[variableName] = value;
				}
			}
			// it is about an array element
			else if (scope is Array) {
				Array arr = (Array)scope;
				object key = name;
				if (key == null)
					key = ++arr.MaxKey;
				else if (key is bool || key is double)
					key = Convert.ToInt(key);
				else if (key is string && Array.IsStandardInteger((string)key))
					key = System.Convert.ToInt32((string)key);
				// other data types are not allowed for keys, so ignore them
				else if (!(key is int))
					throw Report.Exception(402, key.GetType().FullName);
				// append key and value to array
				int index = arr.Keys.IndexOf(key);
				if (index > -1) {
					// remember old value
					object oldValue = arr.Values[index];
					// if a reference is to be stored...
					if (value is Reference) {
						Reference r = (Reference)value;
						Cell c;
						// ...adjust referencing cell
						if (adjustReferencingCell) {
							c = GetReferenceTo(arr, key);
							if (c != null)
								StoreToCell(c.Scope, oldValue, c.Name, false);
						}
						// ...store and ensure new reference is at beginning of reference line)
						c = GetReferenceTo(r.ReferencedScope, r.ReferencedName);
						while (c != null) {
							r.ReferencedScope = c.Scope;
							r.ReferencedName = c.Name;
							c = GetReferenceTo(c.Scope, c.Name);
						}
						arr.Values[index] = r;
						// ...remember that the desired cell references the target of the reference
						ReferencedCells[new Cell(r.ReferencedScope, r.ReferencedName)] = new Cell(arr, key);
					}
					// if a real value (no reference) is to be stored, store
					else {
						if (oldValue is Reference) {
							Reference r = (Reference)oldValue;
							StoreToCell(r.ReferencedScope, value, r.ReferencedName);
						}
						else
							arr.Values[index] = value;
					}
				}
				else {
					arr.Keys.Add(key);
					arr.Values.Add(value);
				}
				if ((int)key > arr.MaxKey)
					arr.MaxKey = (int)key;
			}
			// it is about a static class member
			else if (scope is Type) {
				Type t = (Type)scope;
				string memberName = Convert.ToString(name);
				// check if field exisits
				FieldInfo fi = t.GetField(memberName);
				if (fi != null && !fi.IsStatic)
					throw Report.Exception(208, memberName);
				PropertyInfo pi = null;
				if (fi == null) {
					pi = t.GetProperty(memberName);
					if (pi != null && (pi.GetGetMethod() == null || !pi.GetGetMethod().IsStatic))
						throw Report.Exception(208, memberName);
					if (pi != null && !pi.CanRead)
						throw Report.Exception(226, memberName);
				}
				if (fi == null && pi == null)
					throw Report.Exception(209, memberName);
				// remember old value
				object oldValue;
				if (fi != null)
					oldValue = fi.GetValue(null);
				else
					oldValue = pi.GetValue(null, null);
				// if a reference is to be stored...
				if (value is Reference) {
					Reference r = (Reference)value;
					Cell c;
					// ...adjust referencing cell
					if (adjustReferencingCell) {
						c = GetReferenceTo(t, memberName);
						if (c != null)
							StoreToCell(c.Scope, oldValue, c.Name, false);
					}
					// ...store and ensure new reference is at beginning of reference line)
					c = GetReferenceTo(r.ReferencedScope, r.ReferencedName);
					while (c != null) {
						r.ReferencedScope = c.Scope;
						r.ReferencedName = c.Name;
						c = GetReferenceTo(c.Scope, c.Name);
					}
					if (fi != null)
						fi.SetValue(null, r);
					else
						pi.SetValue(null, r, null);
					// ...remember that the desired cell references the target of the reference
					ReferencedCells[new Cell(r.ReferencedScope, r.ReferencedName)] = new Cell(t, memberName);
				}
				// if a real value (no reference) is to be stored, store
				else {
					if (oldValue is Reference) {
						Reference r = (Reference)oldValue;
						StoreToCell(r.ReferencedScope, value, r.ReferencedName);
					}
					else if (fi != null) {
						// convert primitive type in case value is stored to an object of external type
						if (value.GetType() == typeof(int) || value.GetType() == typeof(double) || value.GetType() == typeof(string) || value is Array)
							value = Convert.FitTypeForExternalUse(value, fi.FieldType);
						fi.SetValue(null, value);
					}
					else {
						// convert primitive type in case value is stored to an object of external type
						if (value.GetType() == typeof(int) || value.GetType() == typeof(double) || value.GetType() == typeof(string) || value is Array)
							value = Convert.FitTypeForExternalUse(value, pi.PropertyType);
						pi.SetValue(null, value, null);
					}
				}
			}
			// it is about an object member
			else if (!(scope is Enum)) {
				object o = scope;
				string memberName = Convert.ToString(name);
				// check if field exisits
				FieldInfo fi = o.GetType().GetField(memberName);
				PropertyInfo pi = o.GetType().GetProperty(memberName);
				if (fi == null && pi != null && !pi.CanWrite)
					throw Report.Exception(227, memberName);
				if (fi == null && pi == null)
					throw Report.Exception(205, memberName);
				// remember old value
				object oldValue;
				if (fi != null)
					oldValue = fi.GetValue(o);
				else
					oldValue = pi.GetValue(o, null);
				// if a reference is to be stored...
				if (value is Reference) {
					Reference r = (Reference)value;
					Cell c;
					// ...adjust referencing cell
					if (adjustReferencingCell) {
						c = GetReferenceTo(o, memberName);
						if (c != null)
							StoreToCell(c.Scope, oldValue, c.Name, false);
					}
					// ...store and ensure new reference is at beginning of reference line)
					c = GetReferenceTo(r.ReferencedScope, r.ReferencedName);
					while (c != null) {
						r.ReferencedScope = c.Scope;
						r.ReferencedName = c.Name;
						c = GetReferenceTo(c.Scope, c.Name);
					}
					if (fi != null)
						fi.SetValue(o, r);
					else
						pi.SetValue(o, r, null);
					// ...remember that the desired cell references the target of the reference
					ReferencedCells[new Cell(r.ReferencedScope, r.ReferencedName)] = new Cell(o, memberName);
				}
				// if a real value (no reference) is to be stored, store
				else {
					bool externalClassMember = !(o is PHP.Object || o is Type);
					if (oldValue is Reference) {
						Reference r = (Reference)oldValue;
						StoreToCell(r.ReferencedScope, value, r.ReferencedName);
					}
					else if (fi != null) {
						// convert primitive type in case value is stored to an object of external type
						if (externalClassMember)
							if (value.GetType() == typeof(int) || value.GetType() == typeof(double) || value.GetType() == typeof(string) || value is Array)
								value = Convert.FitTypeForExternalUse(value, fi.FieldType);
						fi.SetValue(o, value);
					}
					else {
						// convert primitive type in case value is stored to an object of external type
						if (externalClassMember)
							if (value.GetType() == typeof(int) || value.GetType() == typeof(double) || value.GetType() == typeof(string) || value is Array)
								value = Convert.FitTypeForExternalUse(value, pi.PropertyType);
						pi.SetValue(o, value, null);
					}
				}
			}
			// invalid type
			else
				throw Report.Exception(403);
		}

		internal static void RemoveCell(object scope, object name) {
			// it is about a variable
			if (scope is string) {
				Hashtable variablePoolOfScope = (Hashtable)VariablePool[scope];
				if (variablePoolOfScope == null)
					return;
				string variableName = Convert.ToString(name);
				// remember old value
				object oldValue = variablePoolOfScope[variableName];
				// adjust referencing cell and remove
				Cell c = GetReferenceTo(scope, variableName);
				if (c == null)
					ReferencedCells.Remove(new Cell(scope, name));
				else
					StoreToCell(c.Scope, oldValue, c.Name, false);
				variablePoolOfScope.Remove(variableName);
			}
			// it is about an array
			else if (scope is Array) {
				Array arr = (Array)scope;
				object key = name;
				if (key == null)
					return;
				else if (key is bool || key is double)
					key = Convert.ToInt(key);
				else if (key is string && Array.IsStandardInteger((string)key))
					key = System.Convert.ToInt32((string)key);
				// other data types are not allowed for keys, so ignore them
				else if (!(key is int))
					throw Report.Exception(402, key.GetType().FullName);
				int index = arr.Keys.IndexOf(key);
				if (index > -1) {
					// remember old value
					object oldValue = arr.Values[index];
					// adjust referencing cell and remove
					Cell c = GetReferenceTo(arr, key);
					if (c != null)
						StoreToCell(c.Scope, oldValue, c.Name, false);
					arr.Keys.RemoveAt(index);
					arr.Values.RemoveAt(index);
				}
			}
			// invalid type
			else {
				string nameString = Convert.ToString(name);
				throw Report.Exception(410, nameString);
			}
		}

		public static object LoadFromVariable(object variableName) {
			string scope = FunctionCallTraceAsString();
			// if a local variable is available, take it
			object result = LoadFromLocalVariable(variableName);
			// otherwise look for a static variable
			if (result == null)
				result = LoadFromStaticVariable(variableName);
			// anything found?
			if (result == null) {
				string variableNameString = Convert.ToString(variableName);
				throw Report.Exception(218, variableNameString);
			}
			return result;
		}

		internal static object LoadFromLocalVariable(object variableName) {
			string scope = FunctionCallTraceAsString();
			return LoadFromVariablePool(scope, variableName);
		}

		internal static object LoadFromStaticVariable(object variableName) {
			string scope = "__static=>" + (string)FunctionCallTrace[FunctionCallTrace.Count - 1];
			return LoadFromVariablePool(scope, variableName);
		}

		internal static object LoadFromVariablePool(object scope, object variableName) {
			// use scope delivered
			Hashtable variablePoolOfScope = (Hashtable)VariablePool[scope];
			if (variablePoolOfScope == null)
				return null;
			// fetch desired entry
			string variableNameString = Convert.ToString(variableName);
			object result = variablePoolOfScope[variableNameString];
			// this is a workaround as Reference is not yet implemented as value type
			if (result is Reference) {
				Reference r = (Reference)result;
				return new Reference(r.ReferencedScope, r.ReferencedName);
			}
			else
				return result;
		}

		internal static object LoadFromCell(object scope, object name) {
			// invalid type
			if (scope == null)
				throw Report.Exception(403);
			// it is about a variable
			if (scope is string)
				return LoadFromVariablePool(scope, name);
			// it is about an array
			else if (scope is Array)
				return ((Array)scope).Get(name);
			// it is about static class member
			else if (scope is Type)
				return LoadFromClassMember(scope, name);
			// it is about an object member
			else if (!(scope is Enum))
				return LoadFromClassMember(scope, name);
			// invalid type
			else
				throw Report.Exception(404);
		}

		public static void AddFunctionCallToTrace(string functionCall) {
			// add function call to trace
			FunctionCallTrace.Add(functionCall);
			// unset local variables defined that are left from a prior call to the function in the same scope
			string scope = FunctionCallTraceAsString();
			Hashtable variablePoolOfScope = (Hashtable)VariablePool[scope];
			if (variablePoolOfScope == null)
				return;
			foreach (object variableName in new ArrayList(variablePoolOfScope.Keys))
				RemoveCell(scope, variableName);
		}

		public static void RemoveFunctionCallFromTrace() {
			// remove function call from trace
			FunctionCallTrace.RemoveAt(FunctionCallTrace.Count - 1);
			// local variables cannot be unset right now as a returned variable might be referenced
		}

		public static string FunctionCallTraceAsString() {
			StringBuilder result = new StringBuilder();
			for (int i = 0; i < FunctionCallTrace.Count; i++) {
				string functionCall = (string)FunctionCallTrace[i];
				result.Append(functionCall);
				result.Append("=>");
			}
			if (result.Length > 0)
				result.Remove(result.Length - 2, 2);
			return result.ToString();
		}

		public static void UnsetVariable(object variableName) {
			// use current scope
			string scope = FunctionCallTraceAsString();
			// remove from variable pool
			RemoveCell(scope, variableName);
		}

		public static bool DefineConstant(object name, object value, object caseInsensitive) {
			string nameString = Convert.ToString(name);
			// ensure value is scalar
			bool valueIsScalar = value is bool || value is int || value is double || value is string;
			if (!valueIsScalar)
				throw Report.Exception(400, nameString);
			// add to constant pool
			// as case insensitive
			if (Convert.ToBool(caseInsensitive)) {
				if (CaseInsensitiveConstantPool[nameString.ToLower()] == null) {
					CaseInsensitiveConstantPool[nameString.ToLower()] = value;
					return true;
				}
				else
					throw Report.Exception(219, nameString);
			}
			// as case sensitive
			else {
				if (CaseSensitiveConstantPool[nameString] == null) {
					CaseSensitiveConstantPool[nameString] = value;
					return true;
				}
				else
					throw Report.Exception(219, nameString);
			}
		}

		public static object GetConstant(object name) {
			string nameString = Convert.ToString(name);
			// if the desired constant exists as case sensitive, take it
			if (CaseSensitiveConstantPool[nameString] != null)
				return CaseSensitiveConstantPool[nameString];
			// else if is exists as case insensitive, take it
			else if (CaseInsensitiveConstantPool[nameString.ToLower()] != null)
				return CaseInsensitiveConstantPool[nameString.ToLower()];
			// nothing found
			else
				return null;
		}

		public static void StoreToClassMember(object o, object value, object classMemberName) {
			// store to cell (storing to class member is handled there)
			StoreToCell(o, value, classMemberName);
		}

		public static object LoadFromClassMember(object o, object classMemberName) {
			// invalid type
			if (o == null)
				throw Report.Exception(404);
			string classMemberNameString = Convert.ToString(classMemberName);
			// load
			if (o.GetType() == typeof(Enum)) {
				string[] names = Enum.GetNames(o.GetType());
				for (int i = 0; i < names.Length; i++)
					if (names[i] == classMemberNameString) {
						System.Array values = Enum.GetValues(o.GetType());
						return values.GetValue(i);
					}
				throw Report.Exception(205, classMemberNameString);
			}
			FieldInfo fi;
			PropertyInfo pi;
			if (o is Type) {
				Type t = (Type)o;
				fi = t.GetField(classMemberNameString);
				pi = t.GetProperty(classMemberNameString);
			}
			else {
				fi = o.GetType().GetField(classMemberNameString);
				pi = o.GetType().GetProperty(classMemberNameString);
			}
			// if no such field available
			if (fi == null && pi != null && !pi.CanRead)
				throw Report.Exception(226, classMemberNameString);
			if (fi == null && pi == null) {
				if (o is Type)
					throw Report.Exception(209, ((Type)o).FullName + "::" + classMemberNameString);
				else
					throw Report.Exception(205, o.GetType().FullName + "->" + classMemberNameString);
			}
			else {
				object result;
				if (fi != null)
					result = fi.GetValue(o);
				else
					result = pi.GetValue(o, null);
				// this is a workaround as Reference is not yet implemented as value type
				if (result is Reference) {
					Reference r = (Reference)result;
					return new Reference(r.ReferencedScope, r.ReferencedName);
				}
				// convert external types to match the internal ones
				else
					return Convert.FitTypeForInternalUse(result);
			}
		}

		public static object LoadFromExternalEnumeration(string enumName, string fieldName) {
			Type t = Type.GetType(enumName);
			// invalid type
			if (t == null)
				throw Report.Exception(223, t.FullName + "::" + fieldName);
			System.Array names = Enum.GetNames(t);
			System.Array values = Enum.GetValues(t);
			for (int i = 0; i < names.Length; i++) {
				string name = (string)names.GetValue(i);
				if (name == fieldName)
					return values.GetValue(i);
			}
			throw Report.Exception(223, t.FullName + "::" + fieldName);
		}

		public static object InvokeFunction(object o, ArrayList parametersSupplied, object functionName) {
			// invalid type
			if (o == null)
				throw Report.Exception(405);
			// if object is an external one, handle differently
			if (!(o is PHP.Object))
				return InvokeExternalFunction(o, parametersSupplied, functionName);
			// object was defined in php script
			string functionNameString = Convert.ToString(functionName);
			// invoke
			MethodInfo mi = o.GetType().GetMethod(functionNameString);
			if (mi == null)
				throw Report.Exception(212, functionNameString);
			else {
				// pass parameters (only as many as needed)
				int parametersNeeded = mi.GetParameters().Length;
				object[] parametersPassed = new object[parametersNeeded];
				int i = 0;
				for (; i < Math.Min(parametersNeeded, parametersSupplied.Count); i++) {
					// ensure references are passed whereever they are required
					Type typeRequired = mi.GetParameters()[i].ParameterType;
					Type typeSupplied;
					if (parametersSupplied[i] == null)
						typeSupplied = typeof(object);
					else
						typeSupplied = parametersSupplied[i].GetType();
					if (typeRequired == typeof(Reference) && typeSupplied != typeof(Reference))
						throw Report.Exception(301, i.ToString());
					parametersPassed[i] = parametersSupplied[i];
				}
				// if less parameters actually passed then necessary, pass nulls instead
				for (; i < parametersNeeded; i++)
					parametersPassed[i] = null;
				// add function call to trace
				AddFunctionCallToTrace(o.GetType().Name + "->" + functionNameString);
				// invoke
				object result = mi.Invoke(o, parametersPassed);
				// remove function call to trace
				RemoveFunctionCallFromTrace();
				// push return value
				return result;
			}
		}

		public static object InvokeExternalStaticFunction(string type, ArrayList parametersSupplied, object functionName) {
			// invalid type
			if (type == null || type == "")
				throw Report.Exception(405);
			Type t = Type.GetType(type);
			string functionNameString = Convert.ToString(functionName);
			if (t == null)
				return null;
			// determine types of supplied parameters
			Type[] typesSupplied = Type.EmptyTypes;
			if (parametersSupplied.Count != 0)
				typesSupplied = new Type[parametersSupplied.Count];
			for (int i = 0; i < parametersSupplied.Count; i++) {
				object parameterSupplied = parametersSupplied[i];
				if (parameterSupplied == null)
					typesSupplied[i] = null;
				else
					typesSupplied[i] = parameterSupplied.GetType();
			}
			// if such a static method doesn't exist in current type, proceed with next assembly
			// determine methods possible
			ArrayList methodsPossible = new ArrayList();
			ArrayList methodsPossibleAfterTypeFit = new ArrayList();
			foreach (MethodInfo tmpMi in t.GetMethods()) {
				// ensure name is correct
				if (tmpMi.Name != functionNameString)
					continue;
				// ensure method is static
				if (!tmpMi.IsStatic)
					continue;
				// ensure amount of parameters is correct
				ParameterInfo[] tmpPis = tmpMi.GetParameters();
				if (tmpPis.Length != typesSupplied.Length)
					continue;
				// ensure types fit
				bool typesOk = true;
				bool primitiveTypesOkAfterFit = true;
				for (int i = 0; i < tmpPis.Length; i++) {
					ParameterInfo pi = (ParameterInfo)tmpPis[i];
					Type typeExpected = pi.ParameterType;
					Type typeSupplied = typesSupplied[i];
					// check if type is ok
					bool typeOk = typeSupplied == null || typeSupplied == typeExpected || typeSupplied.IsSubclassOf(typeExpected);
					if (!typeOk) {
						typesOk = false;
						// check if primitive type can be made fit
						if (Convert.FitTypeForExternalUse(parametersSupplied[i], typeExpected) == null) {
							primitiveTypesOkAfterFit = false;
							break;
						}
					}
				}
				if (typesOk)
					methodsPossible.Add(tmpMi);
				else if (primitiveTypesOkAfterFit)
					methodsPossibleAfterTypeFit.Add(tmpMi);
			}
			// determine methods desired
			MethodInfo mi;
			if (methodsPossible.Count == 0) {
				if (methodsPossibleAfterTypeFit.Count == 0)
					throw Report.Exception(216, functionNameString);
				else if (methodsPossibleAfterTypeFit.Count == 1)
					mi = (MethodInfo)methodsPossibleAfterTypeFit[0];
				else
					throw Report.Exception(119);
			}
			else if (methodsPossible.Count == 1)
				mi = (MethodInfo)methodsPossible[0];
			// if more than one method is possible, warn about ambigious call
			else
				throw Report.Exception(119);
			// check each primitive parameter
			ParameterInfo[] pis = mi.GetParameters();
			for (int i = 0; i < pis.Length; i++) {
				ParameterInfo pi = (ParameterInfo)pis[i];
				Type typeSupplied = typesSupplied[i];
				// convert primitive type
				if (typeSupplied == typeof(int) || typeSupplied == typeof(double) || typeSupplied == typeof(string) || typeSupplied == typeof(Array)) {
					Type typeExpected = pi.ParameterType;
					parametersSupplied[i] = Convert.FitTypeForExternalUse(parametersSupplied[i], typeExpected);
				}
			}
			// add function call to trace
			AddFunctionCallToTrace(t.Name + "->" + functionNameString);
			// invoke
			object result = mi.Invoke(null, parametersSupplied.ToArray());
			// fit parameter for internal use
			result = Convert.FitTypeForInternalUse(result);
			// remove function call to trace
			RemoveFunctionCallFromTrace();
			// push return value
			return result;
		}

		public static object InvokeExternalFunction(object o, ArrayList parametersSupplied, object functionName) {
			// invalid type
			if (o == null)
				throw Report.Exception(405);
			Type t = o.GetType();
			string functionNameString = Convert.ToString(functionName);
			if (t == null)
				return null;
			// determine types of supplied parameters
			Type[] typesSupplied = Type.EmptyTypes;
			if (parametersSupplied.Count != 0)
				typesSupplied = new Type[parametersSupplied.Count];
			for (int i = 0; i < parametersSupplied.Count; i++) {
				object parameterSupplied = parametersSupplied[i];
				if (parameterSupplied == null)
					typesSupplied[i] = null;
				else
					typesSupplied[i] = parameterSupplied.GetType();
			}
			// determine methods possible
			ArrayList methodsPossible = new ArrayList();
			ArrayList methodsPossibleAfterTypeFit = new ArrayList();
			foreach (MethodInfo tmpMi in t.GetMethods()) {
				// ensure name is correct
				if (tmpMi.Name != functionNameString)
					continue;
				// ensure amount of parameters is correct
				ParameterInfo[] tmpPis = tmpMi.GetParameters();
				if (tmpPis.Length != typesSupplied.Length)
					continue;
				// ensure types fit
				bool typesOk = true;
				bool primitiveTypesOkAfterFit = true;
				for (int i = 0; i < tmpPis.Length; i++) {
					ParameterInfo pi = (ParameterInfo)tmpPis[i];
					Type typeExpected = pi.ParameterType;
					Type typeSupplied = typesSupplied[i];
					// check if type is ok
					bool typeOk = typeSupplied == null || typeSupplied == typeExpected || typeSupplied.IsSubclassOf(typeExpected);
					if (!typeOk) {
						typesOk = false;
						// check if primitive type can be made fit
						if (Convert.FitTypeForExternalUse(parametersSupplied[i], typeExpected) == null) {
							primitiveTypesOkAfterFit = false;
							break;
						}
					}
				}
				if (typesOk)
					methodsPossible.Add(tmpMi);
				else if (primitiveTypesOkAfterFit)
					methodsPossibleAfterTypeFit.Add(tmpMi);
			}
			// determine methods desired
			MethodInfo mi;
			if (methodsPossible.Count == 0) {
				if (methodsPossibleAfterTypeFit.Count == 0)
					throw Report.Exception(212, functionNameString);
				else if (methodsPossibleAfterTypeFit.Count == 1)
					mi = (MethodInfo)methodsPossibleAfterTypeFit[0];
				else
					throw Report.Exception(119);
			}
			else if (methodsPossible.Count == 1)
				mi = (MethodInfo)methodsPossible[0];
			// if more than one method is possible, warn about ambigious call
			else
				throw Report.Exception(119);
			// check each primitive parameter
			ParameterInfo[] pis = mi.GetParameters();
			for (int i = 0; i < pis.Length; i++) {
				ParameterInfo pi = (ParameterInfo)pis[i];
				Type typeSupplied = typesSupplied[i];
				// convert primitive type
				if (typeSupplied == typeof(int) || typeSupplied == typeof(double) || typeSupplied == typeof(string) || typeSupplied == typeof(Array)) {
					Type typeExpected = pi.ParameterType;
					parametersSupplied[i] = Convert.FitTypeForExternalUse(parametersSupplied[i], typeExpected);
				}
			}
			// add function call to trace
			AddFunctionCallToTrace(t.Name + "->" + functionNameString);
			// invoke
			object result = mi.Invoke(o, parametersSupplied.ToArray());
			// fit parameter for internal use
			result = Convert.FitTypeForInternalUse(result);
			// remove function call to trace
			RemoveFunctionCallFromTrace();
			// push return value
			return result;
		}

		public static object InvokeExternalConstructor(string type, ArrayList parametersSupplied) {
			// invalid type
			if (type == null || type == "")
				throw Report.Exception(405);
			Type t = Type.GetType(type);
			if (t == null)
				return null;
			// determine types of supplied parameters
			Type[] typesSupplied = Type.EmptyTypes;
			if (parametersSupplied.Count != 0)
				typesSupplied = new Type[parametersSupplied.Count];
			for (int i = 0; i < parametersSupplied.Count; i++) {
				object parameterSupplied = parametersSupplied[i];
				if (parameterSupplied == null)
					typesSupplied[i] = null;
				else
					typesSupplied[i] = parameterSupplied.GetType();
			}
			// determine constructors possible
			ArrayList constructorsPossible = new ArrayList();
			ArrayList constructorsPossibleAfterTypeFit = new ArrayList();
			foreach (ConstructorInfo tmpCi in t.GetConstructors()) {
				// ensure amount of parameters is correct
				ParameterInfo[] tmpPis = tmpCi.GetParameters();
				if (tmpPis.Length != typesSupplied.Length)
					continue;
				// ensure types fit
				bool typesOk = true;
				bool primitiveTypesOkAfterFit = true;
				for (int i = 0; i < tmpPis.Length; i++) {
					ParameterInfo pi = (ParameterInfo)tmpPis[i];
					Type typeExpected = pi.ParameterType;
					Type typeSupplied = typesSupplied[i];
					// check if type is ok
					bool typeOk = typeSupplied == null || typeSupplied == typeExpected || typeSupplied.IsSubclassOf(typeExpected);
					if (!typeOk) {
						typesOk = false;
						// check if primitive type can be made fit
						if (Convert.FitTypeForExternalUse(parametersSupplied[i], typeExpected) == null) {
							primitiveTypesOkAfterFit = false;
							break;
						}
					}
				}
				if (typesOk)
					constructorsPossible.Add(tmpCi);
				else if (primitiveTypesOkAfterFit)
					constructorsPossibleAfterTypeFit.Add(tmpCi);
			}
			// determine constructor desired
			ConstructorInfo ci;
			if (constructorsPossible.Count == 0) {
				if (constructorsPossibleAfterTypeFit.Count == 0)
					throw Report.Exception(221, t.FullName);
				else if (constructorsPossibleAfterTypeFit.Count == 1)
					ci = (ConstructorInfo)constructorsPossibleAfterTypeFit[0];
				else
					throw Report.Exception(118);
			}
			else if (constructorsPossible.Count == 1)
				ci = (ConstructorInfo)constructorsPossible[0];
			// if more than one constructor is possible, warn about ambigious call
			else
				throw Report.Exception(118);
			// check each primitive parameter
			ParameterInfo[] pis = ci.GetParameters();
			for (int i = 0; i < pis.Length; i++) {
				ParameterInfo pi = (ParameterInfo)pis[i];
				Type typeSupplied = typesSupplied[i];
				// convert primitive type
				if (typeSupplied == typeof(int) || typeSupplied == typeof(double) || typeSupplied == typeof(string) || typeSupplied == typeof(Array)) {
					Type typeExpected = pi.ParameterType;
					parametersSupplied[i] = Convert.FitTypeForExternalUse(parametersSupplied[i], typeExpected);
				}
			}
			// add function call to trace
			AddFunctionCallToTrace(t.Name + "->" + "__construct");
			// invoke
			object result = ci.Invoke(parametersSupplied.ToArray());
			// remove function call to trace
			RemoveFunctionCallFromTrace();
			// push return value
			return result;
		}

		public static void CheckTypeHint(object o, string type, int paramIndex) {
			Core.DeReference(ref o);
			Type t = Type.GetType(type);
			if (t == null)
				throw Report.Exception(203);
			bool typeHintOk = (bool)Operators.Instanceof(o, t);
			if (!typeHintOk)
				throw Report.Exception(302, System.Convert.ToString(paramIndex));
		}

		public static object Offset(object o1, object o2, int kind) {
			Core.DeReference(ref o1, ref o2);
			// if empty offset passed, return null
			if (o2 == null)
				return null;
			// else process offset
			if (kind == 0 /* OFFSET.SQUARE */) {
				if (o1 is Array) {
					Array a = (Array)o1;
					return a.Get(o2);
				}
				// ignore offset
				else {
					if (o1 != null)
						throw Report.Exception(401, o1.GetType().FullName);
					return o1;
				}
			}
			else
				return null;
		}

		public void EnsureReference(object o) {
			if (!(o is Reference))
				throw Report.Exception(122, o.GetType().FullName);
		}

		public void EnsureArray(object o) {
			if (!(o is Array))
				throw Report.Exception(407, o.GetType().FullName);
		}

		public static object DeReference(object o) {
			if (o != null && o is Reference)
				return ((Reference)o).GetValue();
			else
				return o;
		}

		public static void DeReference(ref object o) {
			if (o != null && o is Reference)
				o = ((Reference)o).GetValue();
		}

		public static void DeReference(ref object o1, ref object o2) {
			if (o1 != null && o1 is Reference)
				o1 = ((Reference)o1).GetValue();
			if (o2 != null && o2 is Reference)
				o2 = ((Reference)o2).GetValue();
		}

	}


	internal class Cell {

		internal object Scope;
		internal object Name;

		internal Cell(object scope, object name) {
			Scope = scope;
			Name = name;
		}

		public override bool Equals(object o) {
			if (o is Cell) {
				Cell c = (Cell)o;
				return Scope == c.Scope && Name == c.Name;
			}
			else
				return base.Equals(o);
		}

		public override int GetHashCode() {
			return Scope.GetHashCode() ^ Name.GetHashCode();
		}	}


}