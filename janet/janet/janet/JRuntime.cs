// JRuntime.cs: JANET runtime support functions
//
// Author: Steve Newman (steve@snewman.net)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Bitcraft, Inc.

// HACK snewman 7/26/01: currently, many of the functions in this file are
// a more or less direct transcription of the ECMAScript specification.
// Look for places to simplify and optimize.


#define TRACE

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

using JANET.Runtime;


namespace JANET.Runtime {

// This class represents a function's activation object.
public class JActivationObject : JObject
	{
	// Construct a JActivationObject.  The first parameter is the array
	// of arguments passed to the function (not including "this"), and
	// the remaining parameters are the names of the function's format
	// parameters.  We populate the object with the actual values of
	// the parameters.
	public JActivationObject(object[] args, params string[] paramNames)
	 : base(null, null, "Activation")
		{
		for (int i=0; i<paramNames.Length; i++)
			if (i < args.Length)
				Put(paramNames[i], args[i]);
			else
				Put(paramNames[i], JUndefinedObject.instance);
		
		} // JActivationObject constructor
	
	} // JActivationObject


// This class is used to cache information about native CLI types so that
// we can quickly resolve method or property references from script code
// to native objects.
// 
// HACK snewman 9/17/01: need to review this entire class to see if we're
// properly extracting data from the Type object.  For example, I'm not
// sure that the way we handle non-public declarations is correct.  And
// I may need to handle name collisions, e.g. between a method and a property.
// 
// HACK snewman 8/21/01: need massive performance optimization
// here (e.g. caching), and anywhere else we muck around with
// Type objects.
internal class CachedTypeInfo
	{
	// Construct a CachedTypeInfo object for the given type.  We record either
	// static or instance members, depending on the useStatic parameter.
	internal CachedTypeInfo(Type targetType, bool useStatic)
		{
		targetType_ = targetType;
		useStatic_  = useStatic;
		
		members = new System.Collections.Hashtable();
		constructors = null;
		
		MemberInfo[] typeMembers = targetType.GetMembers(
				BindingFlags.Public |
				((useStatic) ? BindingFlags.Static : BindingFlags.Instance) );
		foreach (MemberInfo curMember in typeMembers)
			{
			// HACK snewman 9/17/01: handle EventInfo and Type objects as well.
			// I should probably be looking at curMember.MemberType.
			
			if (curMember is FieldInfo)
				members[curMember.Name] = curMember;
			else if (curMember is PropertyInfo)
				members[curMember.Name] = curMember;
			else if (curMember is MethodBase)
				{
				object curMethods = null;
				if (curMember is ConstructorInfo)
					curMethods = constructors;
				else if (members.ContainsKey(curMember.Name))
					curMethods = members[curMember.Name];
				
				curMethods = AddMethodToList(curMethods, (MethodBase)curMember);
			
				if (curMember is ConstructorInfo)
					constructors = curMethods;
				else
					members[curMember.Name] = curMethods;
				}
			
			}
		
		} // CachedTypeInfo constructor
	
	
	// Return a CachedTypeInfo object for the given type.
	internal static CachedTypeInfo GetInfoForType(Type targetType, bool useStatic)
		{
		System.Collections.Hashtable hashTable = (useStatic) ? staticCache
															 : instanceCache;
		if (hashTable.ContainsKey(targetType))
			return (CachedTypeInfo) hashTable[targetType];
		
		CachedTypeInfo info = new CachedTypeInfo(targetType, useStatic);
		hashTable[targetType] = info;
		return info;
		} // GetInfoForType
	
	
	// Invoke the method of our type that best matches the given argument
	// list, and return its result.  If there is no matching method, or the
	// method match is ambiguous, throw an exception.
	//
	// This method may alter the args array (performing type conversions on
	// arguments as needed to match the method parameters).
	internal object InvokeMethod(object target, string methName, object[] args)
		{
		if (!members.ContainsKey(methName))
			throw new ReferenceError("attempt to call an undefined method of a non-JavaScript object");
		
		object methInfo = members[methName];
		if (methInfo is MethodInfo)
			return InvokeSpecificMethod((MethodInfo)methInfo, target, args);
		else if (methInfo is MethodBase[])
			{
			MethodBase match = ResolveOverload((MethodBase[])methInfo, args);
			if (match == null)
				throw new ReferenceError("call parameters do not match any method declaration");
			
			return InvokeSpecificMethod((MethodInfo)match, target, args);
			}
		else
			throw new ReferenceError("attempt to call a non-method");
		
		} // InvokeMethod
	
	
	// Invoke the constructor for our type that best matches the given
	// argument list, and return its result.  If there is no matching
	// constructor, or the constructor match is ambiguous, throw an
	// exception.
	//
	// This method may alter the args array (performing type conversions on
	// arguments as needed to match the constructor parameters).
	internal object InvokeConstructor(object[] args)
		{
		if (constructors is ConstructorInfo)
			return InvokeSpecificConstructor((ConstructorInfo)constructors, args);
		else if (constructors != null)
			{
			MethodBase match = ResolveOverload((MethodBase[])constructors, args);
			if (match == null)
				throw new ReferenceError("parameters do not match any constructor declaration");
			
			return InvokeSpecificConstructor((ConstructorInfo)match, args);
			}
		else
			throw new ReferenceError("attempt to construct a class with no public constructors");
		
		} // InvokeConstructor
	
	
	// Return the method which best matches the given argument list.  If no
	// match can be found, return null.
	private MethodBase ResolveOverload(MethodBase[] methods, object[] args)
		{
		int argCount = (args == null) ? 0 : args.Length;
		
		// HACK snewman 9/18/01: implement a proper overload-resolution
		// mechanism, and detect ambiguous calls.
		
		for (int phase=1; phase<=2; phase++)
			foreach (MethodBase curMethod in methods)
				{
				int paramCount = curMethod.GetParameters().Length;
				if ( argCount == paramCount ||
					 (argCount >= paramCount-1 && MethodIsStretchable(curMethod)) )
					if (CanMatchCall( curMethod, curMethod.GetParameters(),
									  args, phase==2 ))
						return curMethod;
				}
		
		return null;
		} // ResolveOverload
	
	
	// Return the field or property value of the given name from the given
	// object.  If there is no such field or (parameterless) property, return
	// undefined.
	internal bool HasFieldOrProperty(string name)
		{
		if (!members.ContainsKey(name))
			return false;
		
		object memberInfo = members[name];
		if ( memberInfo is FieldInfo || memberInfo is PropertyInfo ||
			 memberInfo is MethodInfo )
			return true;
		
		return false;
		} // HasFieldOrProperty
	
	
	// Return the field or property value of the given name from the given
	// object.  If there is no such field or (parameterless) property, return
	// undefined.
	internal object GetFieldOrProperty(object target, string name)
		{
		if (!members.ContainsKey(name))
			{
			UInt32 arrayIndex;
			if ( !useStatic_ && ((JArrayObject)target).IsArrayIndex(name, out arrayIndex) &&
				  targetType_.IsArray )
				{
				return InvokeMethod(target, "GetValue", new object[]{arrayIndex});
				}
			
			return JUndefinedObject.instance;
			}
		
		object memberInfo = members[name];
		if (memberInfo is FieldInfo)
			return ((FieldInfo)memberInfo).GetValue(target);
		else if (memberInfo is PropertyInfo)
			return ((PropertyInfo)memberInfo).GetValue(target, null);
		else
			{
			// HACK snewman 9/19/01: return a function object for methods; return
			// undefined otherwise.
			throw new ReferenceError("attempt to read an object member that is not a field or property");
			}
		
		} // GetFieldOrProperty
	
	
	// Set the field or property value of the given name from in given
	// object.  If there is no such field or (parameterless) property, throw
	// an exception.
	internal void SetFieldOrProperty(object target, string name, object value)
		{
		if (!members.ContainsKey(name))
			throw new ReferenceError("attempt to write undefined property of a non-JavaScript object");
		
		object memberInfo = members[name];
		if (memberInfo is FieldInfo)
			((FieldInfo)memberInfo).SetValue(target, value);
		else if (memberInfo is PropertyInfo)
			((PropertyInfo)memberInfo).SetValue(target, value, null);
		else
			throw new ReferenceError("attempt to write to a member that is not a field or property in a non-JavaScript object");
		
		} // SetFieldOrProperty
	
	
	// Invoke the given method of the given object, with the given arguments, and
	// return the function result.
	//
	// This method may alter the args array (performing type conversions on
	// arguments as needed to match the method parameters).
	private object InvokeSpecificMethod(MethodInfo method, object target, object[] args)
		{
		ConvertParameters(method, method.GetParameters(), ref args);
		return method.Invoke(target, args);
		} // InvokeSpecificMethod
	
	
	// Invoke the given constructor, with the given arguments, and return
	// the created object.
	//
	// This method may alter the args array (performing type conversions on
	// arguments as needed to match the constructor parameters).
	private object InvokeSpecificConstructor( ConstructorInfo constructor,
											  object[] args )
		{
		ConvertParameters(constructor, constructor.GetParameters(), ref args);
		return constructor.Invoke(args);
		} // InvokeSpecificConstructor
	
	
	// Return true if the given argument list matches the given formal
	// parameter list.  If allowConversion is true, then we allow conversions
	// to primitive types such as string or double.
	private bool CanMatchCall( MethodBase method, ParameterInfo[] parameters,
										object[] args, bool allowConversion )
		{
		int paramCount = parameters.Length;
		bool methodIsStretchable = MethodIsStretchable(method);
		
		if (args == null)
			{
			Trace.Assert( paramCount == 0 ||
						     (paramCount == 1 && methodIsStretchable) );
			return true;
			}
		
		Trace.Assert( args.Length == paramCount ||
					     (args.Length >= paramCount-1 && methodIsStretchable) );
		
		for (int i=0; i<paramCount; i++)
			{
			if (methodIsStretchable && i == paramCount-1)
				{
				// HACK snewman 10/3/01: need to test all parameters against
				// the entry type of the stretchable parameter.  However, I
				// shouldn't implement this until MethodIsStretchable has been
				// implemented correctly.
				break;
				}
			
			object arg = args[i];
			if (arg == null)
				{
				// HACK snewman 9/24/01: currently we allow null to match any
				// formal parameter; need a more precise test.
				continue;
				}
			
			ParameterInfo p = parameters[i];
			Type pType = p.ParameterType;
			
			if (pType.IsAssignableFrom(arg.GetType()))
				continue;
			
			if (!allowConversion)
				return false;
			
			// HACK snewman 9/22/01: should support all numeric types here.
			// Also, need to check more carefully to see if the conversions
			// can actually be performed.
			
			if ( pType == typeof(Int32)  ||
			     pType == typeof(UInt32) ||
			     pType == typeof(double) ||
			     pType == typeof(bool)   ||
			     pType == typeof(string) )
				return true;
			
			return false;
			}
		
		return true;
		} // CanMatchCall
	
	
	// Change the type of any entries in the args array as needed to match
	// the type declarations of the params array.
	private void ConvertParameters( MethodBase method,
											  ParameterInfo[] parameters,
											  ref object[] args )
		{
		int paramCount = parameters.Length;
		bool methodIsStretchable = MethodIsStretchable(method);
		
		if (args == null)
			{
			Trace.Assert( paramCount == 0 ||
						  (paramCount == 1 && methodIsStretchable) );
			return;
			}
		
		Trace.Assert( args.Length == paramCount ||
					  (args.Length >= paramCount-1 && methodIsStretchable) );
		
		for (int i=0; i<paramCount; i++)
			{
			if (methodIsStretchable && i == paramCount-1)
				{
				// HACK snewman 10/3/01: need to convert all parameters to
				// the entry type of the stretchable parameter.  Also, need
				// to implement the array creation for types other than object[].
				
				int extraArgsCount = args.Length - (paramCount-1);
				object[] extraArgs = new object[extraArgsCount];
				Array.Copy(args, paramCount-1, extraArgs, 0, extraArgsCount);
				
				object[] newArgs = new object[paramCount];
				Array.Copy(args, newArgs, paramCount-1);
				newArgs[paramCount-1] = extraArgs;
				args = newArgs;
				
				break;
				}
			
			object arg = args[i];
			if (arg == null)
				continue;
			
			ParameterInfo p = parameters[i];
			Type pType = p.ParameterType;
			
			if (pType.IsAssignableFrom(arg.GetType()))
				continue;
			
			// HACK snewman 9/22/01: should support all numeric types here.
			// Also, should check for out-of-range errors when downcasting
			// numeric values.
			
			if (pType == typeof(Int32))
				args[i] = (Int32) JConvert.ToNumber(arg);
			else if (pType == typeof(UInt32))
				args[i] = (UInt32) JConvert.ToNumber(arg);
			else if (pType == typeof(double))
				args[i] = JConvert.ToNumber(arg);
			else if (pType == typeof(bool))
				args[i] = JConvert.ToBoolean(arg);
			else if (pType == typeof(string))
				args[i] = JConvert.ToString(arg);
			}
		
		} // ConvertParameters
	
	
	// This function appends a MethodBase object to a list of methods, and
	// returns the concatenated list.
	// 
	// On entry, list can be null (representing an empty list), a MethodBase
	// object (representing a list of length 1), or a MethodBase[] (representing
	// a list of length >1).  We return a MethodBase object or a MethodBase[].
	private static object AddMethodToList(object list, MethodBase method)
		{
		if (list == null)
			return method;
		else if (list is MethodBase)
			{
			MethodBase[] newList = new MethodBase[2];
			newList[0] = (MethodBase)list;
			newList[1] = method;
			return newList;
			}
		else
			{
			MethodBase[] oldList = (MethodBase[]) list;
			MethodBase[] newList = new MethodBase[oldList.Length + 1];
			Array.Copy(oldList, newList, oldList.Length);
			newList[oldList.Length] = method;
			return newList;
			}
		
		} // AddMethodToList

	
	// Return true if the given method's parameter list ends with a
	// "params" parameter, i.e. one that can accomodate any number of
	// arguments (zero or more).
	private static bool MethodIsStretchable(MethodBase method)
		{
		// HACK snewman 10/3/01: the current implementation is a complete
		// hack.  How can I correctly determine this status?  Where is the
		// "params" indication stored in the metadata?
		
		if (method.Name == "GetValue")
			return false;
		
		ParameterInfo[] parameters = method.GetParameters();
		if (parameters.Length <= 0)
			return false;
		
		ParameterInfo param = parameters[parameters.Length - 1];
		Type paramType = param.ParameterType;
		string paramTypeName = paramType.Name;
		string paramTypeFullName = paramType.FullName;
		
		// Console.WriteLine( "MethodIsStretchable: name=\"{0}\", fullName=\"{1}\"",
		// 				   paramTypeName, paramTypeFullName );
		
		return (paramType.IsArray && paramTypeFullName == "System.Object[]");
		} // MethodIsStretchable
	
	
	Type targetType_; // The type for which we record information.
	bool useStatic_;  // True if we record static members, false if instance members.
	
	// This hash table stores the accessible members of our target class,
	// excluding constructors.  It maps strings (the member name) to one
	// of the following values:
	// 
	//    FieldInfo (for fields)
	//
	//	  PropertyInfo (for properties)
	//	
	//	  MethodInfo (for non-overloaded methods)
	// 
	//    MethodBase[] (for overloaded methods)
	internal System.Collections.Hashtable members;
	
	// This holds the accessible constructors of our target class.  If
	// there are no accessible constructors, it is null.  If there is one
	// accessible constructor, it is the ConstructorInfo object.  If there
	// is more than one accessible constructor, it is a MethodBase[].
	internal object constructors;
	
	// These tables map Type to CachedTypeInfo object.  staticCache holds
	// CachedTypeInfo objects with useStatic=true, and instanceCache holds
	// objects with useStatic=false.
	private static System.Collections.Hashtable staticCache = new System.Collections.Hashtable();
	private static System.Collections.Hashtable instanceCache = new System.Collections.Hashtable();
	} // CachedTypeInfo


// This class contains support functions used by generated code.
public class Support
	{
	// Get the value of an object property.
	public static object GetProperty(object lhs, object propName)
		{
		lhs = JConvert.ToObject(lhs);
		string nameStr = JConvert.ToString(propName);
		
		return GetPropertyCore(lhs, nameStr);
		} // GetProperty (object propname)
	
	
	// Get the value of an object property.
	public static object GetProperty(object lhs, string propName)
		{
		lhs = JConvert.ToObject(lhs);
		
		return GetPropertyCore(lhs, propName);
		} // GetProperty (string propname)
	
	
	// Core code for both versions of GetProperty.  Assumes that we've
	// already called ToObject for lhs.
	private static object GetPropertyCore(object lhs, string propName)
		{
		JObject lhsObj = lhs as JObject;
		if (lhsObj != null)
			return lhsObj.Get(propName);
		else if (lhs == null)
			throw new ReferenceError("attempt to get a property of the null object");
		else
			{
			// HACK snewman 8/21/01: extend this to allow reading methods
			// as well as fields.  Review this code in general, and read the
			// detailed class library documentation.  Handle exceptions.
			// This also applies to all the places we muck around with Type
			// objects.
			
			// Any changes here should be reflected in HasProperty (below).
			
			CachedTypeInfo info = GetInfoForObject(ref lhs);
			return info.GetFieldOrProperty(lhs, propName);
			}
		
		} // GetPropertyCore
	
	
	// Return true if the given object has a property of the given name.
	internal static bool HasProperty(object lhs, string propName)
		{
		lhs = JConvert.ToObject(lhs);
		
		JObject lhsObj = lhs as JObject;
		if (lhsObj != null)
			return lhsObj.HasProperty(propName);
		else if (lhs == null)
			throw new ReferenceError("attempt to get a property of the null object");
		else
			{
			// HACK snewman 8/29/01: this code is copied from GetPropertyCore
			// (above), which is incomplete.  Need to track changes there.
			
			CachedTypeInfo info = GetInfoForObject(ref lhs);
			return info.HasFieldOrProperty(propName);
			}
		
		} // HasProperty
	
	
	// Set the value of an object property, and return the assigned value.
	public static object AssignProperty(object lhs, object propName, object value)
		{
		lhs = JConvert.ToObject(lhs);
		string nameStr = JConvert.ToString(propName);
		
		return AssignPropertyCore(lhs, nameStr, value);
		} // AssignProperty (object propname)
	
	
	// Set the value of an object property, and return the assigned value.
	public static object AssignProperty(object lhs, string propName, object value)
		{
		lhs = JConvert.ToObject(lhs);
		
		return AssignPropertyCore(lhs, propName, value);
		} // AssignProperty (string propname)
	
	
	// Core code for both versions of AssignProperty.  Assumes that we've
	// already called ToObject for lhs.
	private static object AssignPropertyCore( object lhs, string propName,
											  object value )
		{
		// HACK snewman 8/20/01: support objects other than JObjects.
		JObject lhsObj = lhs as JObject;
		if (lhsObj != null)
			{
			lhsObj.Put(propName, value);
			return value;
			}
		else if (lhs == null)
			throw new ReferenceError("attempt to set a property of the null object");
		else
			{
			CachedTypeInfo info = GetInfoForObject(ref lhs);
			info.SetFieldOrProperty(lhs, propName,value);
			return value;
			}
		
		} // AssignPropertyCore
	
	
	// Delete an object property.
	public static bool DeleteProperty(object lhs, string propName)
		{
		lhs = JConvert.ToObject(lhs);
		
		// HACK snewman 8/20/01: support objects other than JObjects.
		JObject lhsObj = lhs as JObject;
		return lhsObj.Delete(propName);
		} // DeleteProperty
	
	
	// Implement "typeof" for an object property.
	public static string TypeofProperty(object lhs, string propName)
		{
		if (lhs == null)
			return "undefined";
		
		lhs = JConvert.ToObject(lhs);
		
		// HACK snewman 8/20/01: support objects other than JObjects.
		JObject lhsObj = lhs as JObject;
		object value = lhsObj.Get(propName);
		
		return JObject.Typeof(value);
		} // TypeofProperty
	
	
	// Execute a "new" expression
	public static object New(object constructor, params object[] args)
		{
		JFunctionObject cFunc = constructor as JFunctionObject;
		if (cFunc == null)
			throw new TypeError("operator new invoked on a non-function");
		
		JObject newObj = new JObject( JObject.ObjectPrototype,
									  cFunc,
									  "Object" );
		return cFunc.Construct(newObj, args);
		} // New
	
	
	// Execute a nonmethod call expression
	public static object Call(object function, params object[] args)
		{
		JFunctionObject func = function as JFunctionObject;
		if (func == null)
			throw new TypeError("call invoked on a non-function");
		
		return func.Call(null, args);
		} // Call
	
	
	// Execute a method call expression
	//
	// This method may alter the args array (performing type conversions on
	// arguments as needed to match the method parameters).
	public static object CallMethod(object target, string methName, params object[] args)
		{
		target = JConvert.ToObject(target);
		
		JObject targetObj = target as JObject;
		if (targetObj != null)
			{
			object funcObj = targetObj.Get(methName);
			
			JFunctionObject func = funcObj as JFunctionObject;
			if (func == null)
				throw new TypeError("call invoked on a non-function");
			
			// ECMA version 3 section 11.2.3: if the target is an activation
			// object, replace it with null.
			if (target is JActivationObject)
				target = null;
			
			return func.Call(targetObj, args);
			}
		else if (target == null)
			throw new ReferenceError("attempt to call a method of the null object");
		else
			{
			CachedTypeInfo info = GetInfoForObject(ref target);
			return info.InvokeMethod(target, methName, args);
			}
		
		} // CallMethod (string methName)
	
	
	// Execute a method call expression
	//
	// This method may alter the args array (performing type conversions on
	// arguments as needed to match the method parameters).
	public static object CallMethod(object target, object methName, params object[] args)
		{
		return CallMethod(target, JConvert.ToString(methName), args);
		} // CallMethod (object methName)
	
	
	// Convert the given value to a boolean.  If the value cannot be
	// converted, throw a TypeError.
	public static bool BoolTest(object value)
		{
		return JConvert.ToBoolean(value);
		} // BoolTest
	
	
	// Create a new object from a literal expression.  There should be
	// an even number of arguments, consisting alternately of property
	// names and property values for the object.
	public static JObject LiteralObject(params object[] args)
		{
		JObject obj = new JObject(JObject.ObjectPrototype, null, "Object");
		
		for (int i=0; i<args.Length; i += 2)
			obj.Put((string)(args[i]), args[i+1]);
		
		return obj;
		} // LiteralObject
	
	
	// Create a new array from a literal expression.
	public static object LiteralArray(params object[] args)
		{
		// HACK snewman 8/28/01: properly implement missing elements in
		// the original array literal (commas with no values in between
		// them).
		
		return JArrayObject.ArrayConstructor(null, args);
		
		// This code has been replaced by the call to ArrayConstructor.
		// 
		// JObject obj = new JObject(JObject.ObjectPrototype, null, "Object");
		// 
		// for (int i=0; i<args.Length; i++)
		// 	obj.Put(JConvert.ToString(i), args[i]);
		// 
		// return obj;
		} // LiteralArray
	
	
	// Read an identifier value, searching one or more "with" scopes.
	// We first search for id in each object in the withs array, starting
	// with the last entry in the array.  If any object in the array has
	// a property of the given name, we return the property value.  Otherwise,
	// if lhs has a property of that name, we return its value.  If no
	// object had a property of the given name, we return undefined.
	public static object WithGet(JObject lhs, string id, params object[] withs)
		{
		for (int i=0; i<withs.Length; i++)
			{
			object value = GetProperty(withs[i], id);
			if (!(value is JUndefinedObject))
				return value;
			}
		
		return lhs.Get(id);
		} // WithGet
	
	
	// Set an identifier value, searching one or more "with" scopes.
	// We first search for id in each object in the withs array, starting
	// with the last entry in the array.  If any object in the array has
	// a property of the given name, we update that property value.  Otherwise,
	// we call lhs.Put(id, value).
	public static object WithPut( JObject lhs, string id, object value,
								  params object[] withs )
		{
		for (int i=0; i<withs.Length; i++)
			if (HasProperty(withs[i], id))
				return AssignProperty(withs[i], id, value);
		
		return lhs.Put(id, value);
		} // WithPut
	
	
	// Increment or decrement an identifier value, searching one or more "with"
	// scopes.  We identify the scope as for WithPut.
	// 
	// If isIncrement is true, then we increment the value, otherwise we
	// decrement it.  If isSuffix is true, then we return the original
	// value, otherwise we return the modified value.
	public static object WithIncDec( JObject lhs, string id,
									 bool isIncrement, bool isSuffix,
									 params object[] withs )
		{
		for (int i=0; i<withs.Length; i++)
			if (HasProperty(withs[i], id))
				if (isSuffix)
					return Op.PreIncDecProperty(withs[i], id, isIncrement);
				else
					return Op.PostIncDecProperty(withs[i], id, isIncrement);
		
		if (isSuffix)
			return Op.PreIncDecProperty(lhs, id, isIncrement);
		else
			return Op.PostIncDecProperty(lhs, id, isIncrement);
		
		} // WithIncDec
	
	
	// Delete an identifier value, searching one or more "with"
	// scopes.  We identify the scope as for WithPut.
	public static object WithDelete( JObject lhs, string id,
									 params object[] withs )
		{
		for (int i=0; i<withs.Length; i++)
			if (HasProperty(withs[i], id))
				return DeleteProperty(withs[i], id);
		
		return DeleteProperty(lhs, id);
		} // WithDelete
	
	
	// Compute "typeof" on an identifier value, searching one or more "with"
	// scopes.  We identify the scope as for WithPut.
	public static object WithTypeof( JObject lhs, string id,
									 params object[] withs )
		{
		for (int i=0; i<withs.Length; i++)
			if (HasProperty(withs[i], id))
				return TypeofProperty(withs[i], id);
		
		return TypeofProperty(lhs, id);
		} // WithTypeof
	
	
	// Execute a call expression where the function to be called is specified
	// by an identifier inside one or more with scopes.  We determine the
	// actual function or method as for WithPut.
	//
	// This method may alter the args array (performing type conversions on
	// arguments as needed to match the method parameters).
	public static object WithCall( JObject lhs, string id, object[] withs,
								   params object[] args )
		{
		for (int i=0; i<withs.Length; i++)
			if (HasProperty(withs[i], id))
				return CallMethod(withs[i], id, args);
		
		return Call(lhs.Get(id), args);
		} // WithCall
	
	
	// This class is used to wrap an object thrown by JavaScript code in
	// an Exception.
	private class WrappedException : Exception
		{
		internal WrappedException(object obj) { this.obj = obj; }
		
		internal object obj; // The object that we wrap
		} // WrappedException
	
	
	// Given an arbitrary object, return a WrappedException object that
	// encapsulates it.  This is used to reconcile the JavaScript behavior
	// of allowing any value to be thrown, with the CLR restriction that
	// only instances of System.Exception and its subclasses can be thrown.
	public static Exception WrapException(object obj)
		{
		Exception e = obj as Exception;
		if (e != null)
			return e;
		else
			return new WrappedException(obj);
		
		} // WrapException
	
	
	// Create a JObject with one property.  The property's name
	// is "id" and its value is the given exception object.
	public static JObject CreateCatchScope(Exception exception, string id)
		{
		object unwrapped;
		if (exception is WrappedException)
			unwrapped = ((WrappedException)exception).obj;
		else
			unwrapped = exception;
		
		JObject obj = new JObject(null, null, "CatchScope");
		obj.Put(id, unwrapped);
		return obj;
		} // CreateCatchScope
	
	
	// HACK snewman 8/22/01: need to clean up and organize these global
	// functions.
	
	// Implementation for the global writeln function.
	private static object writeln_(object this_, params object[] args)
		{
		if (args.Length == 0)
			Console.WriteLine("");
		else
			{
			for (int i=0; i<args.Length-1; i++)
				Console.Write(JConvert.ToString(args[i]));
			
			Console.WriteLine(JConvert.ToString(args[args.Length-1]));
			}
		
		return null;
		} // writeln_
	
	
	static int evalIndex = 0;
	
	
	// This delegate is used to encapsulate Compiler.CompileAndLoadForEval.
	// It's necessary to allow JRuntime to invoke JCompiler without an
	// explicit dependency.
	public delegate Type EvalHook( TextReader input,
								   string progClassName,
								   string inputFileLabel,
								   string fileNameBase,
								   JObject globals );
	
	public static EvalHook evalHook; // Refers to Compiler.CompileAndLoadForEval.
	
	// Implementation for the global eval function.
	public static object eval_(object this_, params object[] args)
		{
		// HACK snewman 8/26/01: this is a complete hack, need to replace
		// it with a proper implementation of "eval".
		
		string src = JConvert.ToString(args[0]);
		
		StringReader reader = new StringReader(src);
		string fileNameBase = String.Format("eval_temp{0}", ++evalIndex);
		Type program = evalHook( reader, "EvalProgram", "eval",
								 fileNameBase, (JObject)this_ );
		program.GetMethod("Init").Invoke(null, new object[]{this_});
		program.GetMethod("GlobalCode").Invoke(null, null);
		return null;
		} // eval_
	
	
	private static String AdjustTypeName(String typeName)
		{
		// HACK snewman 4/18/02: Type.GetType, for some reason, isn't
		// resolving the Windows.Forms assembly without all this junk on
		// the end of the string.
		if (typeName.EndsWith("System.Windows.Forms"))
			typeName += ", Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		if (typeName.EndsWith("System.Drawing"))
			typeName += ", Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
		
		return typeName;
		} // AdjustTypeName
	
	
	// Implementation for the global GetType function.
	private static object GetType_(object this_, params object[] args)
		{
		string typeName = JConvert.ToString(args[0]);
		typeName = AdjustTypeName(typeName);
		
		Type type = Type.GetType(typeName);
		if (type == null)
			throw new Exception(String.Format( "undefined type name \"{0}\"",
											   typeName ));
		
		return new TypeWrapper(type);
		} // GetType_
	
	
	// Implementation for the global CreateTypeInstance function.
	private static object CreateTypeInstance_(object this_, params object[] args)
		{
		string typeName = JConvert.ToString(args[0]);
		typeName = AdjustTypeName(typeName);
		
		Type theType = Type.GetType(typeName);
		CachedTypeInfo info = CachedTypeInfo.GetInfoForType(theType, false);
		
		if (args.Length == 1)
			return info.InvokeConstructor(null);
		else
			{
			object[] trimmedArgs = new object[args.Length-1];
			Array.Copy(args, 1, trimmedArgs, 0, trimmedArgs.Length);
			return info.InvokeConstructor(trimmedArgs);
			}
		
		} // CreateTypeInstance_
	
	
	// Add a property to the given object for each built-in identifier
	// that is defined in the global scope.
	public static void DefineBuiltinGlobals(JObject globals)
		{
		// HACK snewman 8/6/01: need to finish the ECMAScript spec.  Make
		// sure that the "length" property is defined for all functions
		// created here.
		
		// HACK snewman 10/3/01: document the "extra" functions we define
		// that aren't in ECMAScript, such as writeln, GetType, and
		// CreateTypeInstance.
		globals.Put("writeln", new JFunctionObject(new JFunctionObject.JFunctionImp(writeln_), null));
		globals.Put("eval",    new JFunctionObject(new JFunctionObject.JFunctionImp(eval_   ), null));
		globals.Put("GetType", new JFunctionObject(new JFunctionObject.JFunctionImp(GetType_), null));
		globals.Put("CreateTypeInstance", new JFunctionObject(new JFunctionObject.JFunctionImp(CreateTypeInstance_), null));
		
		// HACK snewman 10/3/01: all of these properties should be dontEnum,
		// dontDelete.
		globals.Put("NaN",       Double.NaN);
		globals.Put("Infinity",  Double.PositiveInfinity);
		globals.Put("undefined", JUndefinedObject.instance);
		RegisterBuiltins(typeof(GlobalMethods), globals);
		
		
		globals.Put("Object", JObject.ObjectConstructorObj);
		RegisterBuiltins(typeof(ObjectPrototypeMethods), JObject.ObjectPrototype);
		
		globals.Put("Boolean", JBooleanObject.BooleanConstructorObj);
		RegisterBuiltins(typeof(BooleanPrototypeMethods), JBooleanObject.BooleanPrototype);
		
		globals.Put("Number", JNumberObject.NumberConstructorObj);
		RegisterBuiltins(typeof(NumberPrototypeMethods), JNumberObject.NumberPrototype);
		
		globals.Put("Array", JArrayObject.ArrayConstructorObj);
		RegisterBuiltins(typeof(ArrayPrototypeMethods), JArrayObject.ArrayPrototype);
		
		globals.Put("String", JStringObject.StringConstructorObj);
		RegisterBuiltins(typeof(StringConstructorMethods), JStringObject.StringConstructorObj);
		RegisterBuiltins(typeof(StringPrototypeMethods), JStringObject.StringPrototype);
		
		JObject mathObj = new JObject(JObject.ObjectPrototype, null, "Math");
		globals.Put("Math", mathObj);
		RegisterBuiltins(typeof(MathMethods), mathObj);
		
		// HACK snewman 10/3/01: all of these properties should be dontEnum,
		// dontDelete, readOnly.
		mathObj.Put("E",       Math.E        );
		mathObj.Put("LN10",    Math.Log(10)  );
		mathObj.Put("LN2",     Math.Log(2)   );
		mathObj.Put("LOG2E",   1/Math.Log(2) );
		mathObj.Put("LOG10E",  1/Math.Log(10));
		mathObj.Put("PI",      Math.PI       );
		mathObj.Put("SQRT1_2", Math.Sqrt(0.5));
		mathObj.Put("SQRT2",   Math.Sqrt(2)  );
		} // DefineBuiltinGlobals
	
	
	// For each public static method of the specified type, create a
	// function in the given prototype object.
	private static void RegisterBuiltins(Type impType, JObject proto)
		{
		CachedTypeInfo typeInfo = CachedTypeInfo.GetInfoForType(impType, true);
		
		IDictionaryEnumerator enumerator = typeInfo.members.GetEnumerator();
		while (enumerator.MoveNext())
			if (enumerator.Value is MethodInfo)
				{
				// Non-overloaded method.  Register a function glue
				// object.
				JFunctionObject func = new JFunctionStaticGlue(
					typeInfo, (string) enumerator.Key, enumerator.Value );
				
				proto.Put((string)enumerator.Key, func);
				}
			else if (enumerator.Value is MethodBase[])
				{
				// Overloaded method.  Register a function glue
				// object.
				JFunctionObject func = new JFunctionStaticGlue(
					typeInfo, (string) enumerator.Key, enumerator.Value );
				
				proto.Put((string)enumerator.Key, func);
				}
		
		} // RegisterBuiltins
	
	
	// Return the CachedTypeInfo for the given object's type.  If obj is a
	// TypeWrapper, then we return a CachedTypeInfo for the static members
	// of the wrapped type, and set obj to null (as appropriate for accessing
	// static members).
	private static CachedTypeInfo GetInfoForObject(ref object obj)
		{
		Type objType;
		bool useStatic;
		if (obj is TypeWrapper)
			{
			objType = ((TypeWrapper)obj).type;
			obj = null;
			useStatic = true;
			}
		else
			{
			objType = obj.GetType();
			useStatic = false;
			}
		
		return CachedTypeInfo.GetInfoForType(objType, useStatic);
		} // GetInfoForObject
	
	
	// This subclass of JFunctionObject provides glue to a static
	// function of a C# class.  Used for builtin functions.
	private class JFunctionStaticGlue : JFunctionObject
		{
		// Construct a JFunctionStaticGlue.  methodInfo should be
		// a MethodInfo or a MethodBase[].
		internal JFunctionStaticGlue( CachedTypeInfo typeInfo, string methodName,
									  object methodInfo )
		  : base(null, null)
			{
			this.typeInfo   = typeInfo;
			this.methodName = methodName;
			this.methodInfo = methodInfo;
			} // JFunctionStaticGlue constructor
		
		
		// Perform the [[Call]] operation on this object.
		public override object Call(JObject this_, params object[] args)
			{
			object[] augmentedArgs = new object[args.Length+1];
			augmentedArgs[0] = this_;
			Array.Copy(args, 0, augmentedArgs, 1, args.Length);
			
			return typeInfo.InvokeMethod(null, methodName, augmentedArgs);
			} // Call


		private CachedTypeInfo typeInfo;
		private string methodName;
		
		// The MethodInfo or MethodBase[] indicating which method we
		// invoke.
		private object methodInfo;
		} // JFunctionStaticGlue
	
	
	} // Support


// This class is used to encapsulate a System.Type object so that JavaScript
// programs can access static fields and methods of the type, instead of
// accessing fields of the System.Type object.
public class TypeWrapper
	{
	public TypeWrapper(Type type)
		{
		this.type = type;
		}
	
	public Type type;
	} // TypeWrapper


// This class contains functions to implement the ECMAScript operators.
public class Op
	{
	// Implement the "+" operator
	public static object Plus(object x, object y)
		{
		x = JConvert.ToPrimitive(x);
		y = JConvert.ToPrimitive(y);
		if (x is string || y is string)
			return JConvert.ToString(x) + JConvert.ToString(y);
		else
			return JConvert.ToNumber(x) + JConvert.ToNumber(y);
		
		} // Plus
	
	
	// Implement the "-" operator
	public static double Minus(object x, object y)
		{
		return JConvert.ToNumber(x) - JConvert.ToNumber(y);
		} // Minus
	
	
	// Implement the "*" operator
	public static double Mul(object x, object y)
		{
		return JConvert.ToNumber(x) * JConvert.ToNumber(y);
		} // Mul
	
	
	// Implement the "/" operator
	public static double Div(object x, object y)
		{
		return JConvert.ToNumber(x) / JConvert.ToNumber(y);
		} // Div
	
	
	// Implement the "%" operator
	public static double Mod(object x, object y)
		{
		return JConvert.ToNumber(x) % JConvert.ToNumber(y);
		} // Mod
	
	
	// Implement the "<<" (shift left) operator
	public static Int32 ShiftLeft(object x, object y)
		{
		return JConvert.ToInt32(x) << (Int32)(JConvert.ToUInt32(y) & 0x1F);
		} // ShiftLeft
	
	
	// Implement the ">>" (signed shift right) operator
	public static Int32 ShiftRightSigned(object x, object y)
		{
		return JConvert.ToInt32(x) >> (Int32)(JConvert.ToUInt32(y) & 0x1F);
		} // ShiftRightSigned
	
	
	// Implement the ">>>" (unsigned shift right) operator
	public static UInt32 ShiftRightUnsigned(object x, object y)
		{
		return JConvert.ToUInt32(x) >> (Int32)(JConvert.ToUInt32(y) & 0x1F);
		} // ShiftRightUnsigned
	
	
	// Implement the "&" (bitwise AND) operator
	public static Int32 BitAnd(object x, object y)
		{
		return JConvert.ToInt32(x) & JConvert.ToInt32(y);
		} // BitAnd
	
	
	// Implement the "^" (bitwise XOR) operator
	public static Int32 BitXor(object x, object y)
		{
		return JConvert.ToInt32(x) ^ JConvert.ToInt32(y);
		} // BitXor
	
	
	// Implement the "|" (bitwise OR) operator
	public static Int32 BitOr(object x, object y)
		{
		return JConvert.ToInt32(x) | JConvert.ToInt32(y);
		} // BitOr
	
	
	// Implement the "instanceof" operator
	public static object opInstanceof(object x, object y)
		{
		// HACK snewman 8/15/01: implement instanceof (ECMA 11.8.6)
		throw new Exception("instanceof not yet implemented");
		} // opInstanceof
	
	
	// Implement the "in" operator
	public static object opIn(object x, object y)
		{
		return Support.HasProperty(y, JConvert.ToString(x));
		} // opIn
	
	
	// Implement the "==" operator
	// 
	// HACK snewman 8/6/01: review the C# spec in detail, to verify that
	// we implement the correct ECMAScript behavior in all corner cases.
	public static bool EQ(object x, object y)
		{
		// Deal with the cases where at least one operand is null or undefined
		if (x == null)
			{
			if (y == null || y is JUndefinedObject)
				return true;
			else
				return false;
			}
		else if (y == null)
			{
			if (x is JUndefinedObject)
				return true;
			else
				return false;
			}
		else if (x is JUndefinedObject)
			{
			if (y is JUndefinedObject)
				return true;
			else
				return false;
			}
		else if (y is JUndefinedObject)
			{
			return false;
			}
		
		// Deal with the cases where at least one operand is a number.
		double xd, yd;
		if (PrimSupport.AsNumber(x, out xd))
			{
			if (PrimSupport.AsNumber(y, out yd))
				return (xd == yd);
			else if (y is string)
				return (xd == JConvert.ToNumber(y));
			else if (y is bool)
				return EQ(x, (bool)y ? 1 : 0);
			else
				return EQ(x, JConvert.ToPrimitive(y)); // OPTIMIZATION snewman 8/6/01:
													   // this could bypass the checks
													   // for primitive types, since
													   // we know y isn't primitive.
													   // Ditto for other calls to
													   // ToPrimitive in this method.
			
			}
		else if (PrimSupport.AsNumber(y, out yd))
			{
			if (x is string)
				return (JConvert.ToNumber(x) == yd);
			else if (x is bool)
				return EQ(((bool)x ? 1 : 0), y);
			else
				return EQ(JConvert.ToPrimitive(x), y);
			}
		
		// Deal with the cases where at least one operand is a string.
		if (x is string)
			{
			if (y is string)
				return ((string)x == (string)y);
			else if (y is bool)
				return EQ(x, ((bool)y ? 1 : 0));
			else
				return EQ(x, JConvert.ToPrimitive(y));
			}
		else if (y is string)
			{
			if (x is bool)
				return EQ(((bool)x ? 1 : 0), y);
			else
				return EQ(JConvert.ToPrimitive(x), y);
			}
		
		// Deal with the cases where at least one operand is a boolean.
		if (x is bool)
			{
			if (y is bool)
				return ((bool)x == (bool)y);
			else
				return EQ(((bool)x ? 1 : 0), y);
			}
		else if (y is bool)
			return EQ(x, ((bool)y ? 1 : 0));
		
		// Both operands must be objects.
		return (x == y); // HACK snewman 8/6/01: this needs to be a strict object-
						 // identity test.
		} // EQ
	
	
	// Implement the "!=" operator
	public static bool NE(object x, object y)
		{
		return !EQ(x, y);
		} // NE
	
	
	// Implement the "===" (strict equality) operator
	public static bool StrictEQ(object x, object y)
		{
		// Deal with the cases where at least one operand is null or undefined
		if (x == null)
			return (y == null);
		else if (y == null)
			return false;
		else if (x is JUndefinedObject)
			return (y is JUndefinedObject);
		else if (y is JUndefinedObject)
			return false;
		
		// Deal with the cases where at least one operand is a number.
		double xd, yd;
		if (PrimSupport.AsNumber(x, out xd))
			return PrimSupport.AsNumber(y, out yd) && (xd == yd);
		else if (PrimSupport.AsNumber(y, out yd))
			return false;
		
		// Deal with the cases where at least one operand is a string.
		if (x is string)
			return (y is string) && ((string)x == (string)y);
		else if (y is string)
			return false;
		
		// Deal with the cases where at least one operand is a boolean.
		if (x is bool)
			return (y is bool) && ((bool)x == (bool)y);
		else if (y is bool)
			return false;
		
		// Both operands must be objects.
		return (x == y); // HACK snewman 8/6/01: this needs to be a strict object-
						 // identity test.
		} // StrictEQ
	
	
	// Implement the "!==" (strict inequality) operator
	public static bool StrictNE(object x, object y)
		{
		return !StrictEQ(x, y);
		} // StrictNE
	
	
	// Implement the ">" operator
	public static bool GT(object x, object y)
		{
		return CompareCore(y, x, false);
		} // GT
	
	
	// Implement the "<" operator
	public static bool LT(object x, object y)
		{
		return CompareCore(x, y, false);
		} // LT
	
	
	// Implement the ">=" operator
	public static bool GE(object x, object y)
		{
		return !CompareCore(x, y, true);
		} // GE
	
	
	// Implement the "<=" operator
	public static bool LE(object x, object y)
		{
		return !CompareCore(y, x, true);
		} // LE
	
	
	// Return true if x < y.  If the comparison is undefined, return defValue.
	// This implements the "Abstract Relational Comparison Algorithm" in
	// section 11.8.5 of ECMAScript version 3.
	// 
	// HACK snewman 8/6/01: review the C# spec in detail, to verify that
	// we implement the correct ECMAScript behavior in all corner cases.
	private static bool CompareCore(object x, object y, bool defValue)
		{
		x = JConvert.ToPrimitive(x, JObject.ValueHintType.Number);
		y = JConvert.ToPrimitive(y, JObject.ValueHintType.Number);
		
		if (x is string && y is string)
			return String.Compare((string)x, (string)y) < 0;
		
		double xd = JConvert.ToNumber(x);
		double yd = JConvert.ToNumber(y);
		if (Double.IsNaN(xd) || Double.IsNaN(yd))
			return defValue;
		else
			return xd < yd;
		} // CompareCore
	
	
	// Implement the "," operator
	public static object Comma(object x, object y)
		{
		return y;
		} // Comma
	
	
	// Implement the "void" operator.
	public static object Void(object x)
		{
		return JUndefinedObject.instance;
		} // Void
	
	
	// Implement the unary "+" operator.
	public static double UnaryPlus(object x)
		{
		return JConvert.ToNumber(x);
		} // UnaryPlus
	
	
	// Implement the unary "-" operator.
	public static double UnaryMinus(object x)
		{
		return -JConvert.ToNumber(x);
		} // UnaryMinus
	
	
	// Implement the "~" operator.
	public static Int32 BitwiseNOT(object x)
		{
		return ~JConvert.ToInt32(x);
		} // BitwiseNOT
	
	
	// Implement the "!" operator.
	public static bool LogicalNOT(object x)
		{
		return !JConvert.ToBoolean(x);
		} // LogicalNOT
	
	
	// Increment or decrement an object's property, and return the original
	// value.  Implements the preincrement and predecrement operators for
	// an object property.
	public static object PreIncDecProperty( object lhs, string propname,
											bool isIncrement )
		{
		lhs = JConvert.ToObject(lhs);
		
		// HACK snewman 8/20/01: support objects other than JObjects.
		JObject lhsObj = lhs as JObject;
		object value = lhsObj.Get(propname);
		double d = JConvert.ToNumber(value) + ((isIncrement) ? 1 : -1);
		lhsObj.Put(propname, d);
		return d;
		} // PreIncDecProperty
	
	
	// Increment or decrement an object's property, and return the updated
	// value.  Implements the postincrement and postdecrement operators for
	// an object property.
	public static object PostIncDecProperty( object lhs, string propname,
										     bool isIncrement )
		{
		lhs = JConvert.ToObject(lhs);
		
		// HACK snewman 8/20/01: support objects other than JObjects.
		JObject lhsObj = lhs as JObject;
		object value = lhsObj.Get(propname);
		double d = JConvert.ToNumber(value);
		lhsObj.Put(propname, (isIncrement) ? d+1 : d-1);
		return d;
		} // PostIncDecProperty
	
	} // Op


// This class defines the methods of the standard Object prototype.
public class ObjectPrototypeMethods
	{
	public static string toString(JObject _this)
		{
		return "[object " + _this.className + "]";
		} // toString
	
	
	public static string toLocaleString(JObject _this)
		{
		return toString(_this);
		} // toLocaleString
	
	
	public static JObject valueOf(JObject _this)
		{
		return _this;
		} // valueOf
	
	
	public static bool hasOwnProperty(JObject _this, object v)
		{
		string vStr = JConvert.ToString(v);
		return _this.HasOwnProperty(vStr);
		} // hasOwnProperty
	
	
	public static bool isPrototypeOf(JObject _this, object v)
		{
		JObject vObj = v as JObject;
		if (vObj == null)
			return false;
		
		while (true)
			{
			vObj = vObj.proto;
			if (vObj == null)
				return false;
			
			if (vObj == _this)
				return true;
			
			}
		
		} // isPrototypeOf
	
	
	public static bool propertyIsEnumerable(JObject _this, object v)
		{
		string vStr = JConvert.ToString(v);
		
		JProperty prop;
		return _this.LookupProperty(vStr, out prop) &&
			   (prop.attributes & JProperty.AttrFlags.dontEnum) == 0;
		} // propertyIsEnumerable
	
	
	} // ObjectPrototypeMethods


// This class defines the methods of the standard Boolean prototype.
public class BooleanPrototypeMethods
	{
	public static string toString(JBooleanObject _this)
		{
		return (_this.value) ? "true" : "false";
		} // toString (no explicit radix)
	
	
	} // BooleanPrototypeMethods


// This class defines the methods of the standard Number prototype.
public class NumberPrototypeMethods
	{
	public static string toString(JNumberObject _this, int radix)
		{
		if (radix == 10)
			return _this.ToString();
		else if (radix >= 2 && radix <= 36)
			{
			// HACK snewman 10/17/01: implement non-base-10 radixes, at least
			// for integer values.
			throw new Exception("Number.toString doesn't yet implement radixes other than 10");
			}
		else
			throw new ParameterError("Number.toString radix must be in the range 2...36");
		
		} // toString
	
	
	// HACK snewman 10/17/01: implement toFixed, toExponential, and
	// toPrecision.
	
	// Glue for "toString" with no explicit radix.
	public static string toString(JNumberObject _this)
		{
		return toString(_this, 10);
		} // toString (no explicit radix)
	
	
	} // NumberPrototypeMethods


// This class defines the methods of the standard Array prototype.
// 
// HACK snewman 10/3/01: many of these methods are written in a
// very generic fashion, and could be optimized in the case where
// the object in question is a non-sparse JArrayObject.
public class ArrayPrototypeMethods
	{
	// The result of calling this function is the same as if the built-in
	// "join" method were invoked for this object with no argument.
	public static string toString(JArrayObject _this)
		{
		return join(_this);
		} // toString
	
	
	// The elements of the array are converted to strings using their
	// toLocaleString methods, and these strings are then concatenated,
	// separated by occurrences of a separator string that has been derived in
	// an implementation-defined locale-specific way.  The result of calling
	// this function is intended to be analogous to the result of toString,
	// except that the result of this function is intended to be locale-specific.
	public static string toLocaleString(JArrayObject _this)
		{
		return joinCore(_this, ",", true);
		} // toLocaleString
	
	
	// When the concat method is called with zero or more arguments item1,
	// item2, etc., it returns an array containing the array elements of
	// the object followed by the array elements of each argument in order.
	public static JArrayObject concat(object _this, params object[] args)
		{
		JArrayObject newArray = new JArrayObject();
		
		AppendToArray(newArray, _this);
		foreach (object obj in args)
			AppendToArray(newArray, obj);
		
		return newArray;
		
		// HACK snewman 10/2/01: the spec states that "the length property of
		// the concat method is 1".  Need to implement this.
		} // concat
	
	
	// Glue for "join" with no explicit separator
	public static string join(object _this)
		{
		return join(_this, ",");
		} // join
	
	
	// The elements of the array are converted to strings, and these strings
	// are then concatenated, separated by occurrences of the separator.  If
	// no separator is provided, a single comma is used as the separator.
	public static string join(object _this, string separator)
		{
		return joinCore(_this, separator, false);
		
		// HACK snewman 10/2/01: the spec states that "the length property of
		// the join method is 1".  Need to implement this.
		} // join
	
	
	// The last element of the array is removed from the array and returned.
	public static object pop(object _this)
		{
		uint length = JConvert.ToUInt32(Support.GetProperty(_this, "length"));
		if (length == 0)
			{
			Support.AssignProperty(_this, "length", 0);
			return JUndefinedObject.instance;
			}
		
		string newLenStr = JArrayObject.ArrayIndexToString(length-1);
		object popped = Support.GetProperty(_this, newLenStr);
		Support.DeleteProperty(_this, newLenStr);
		Support.AssignProperty(_this, "length", length-1);
		return popped;
		} // pop
	
	
	// The arguments are appended to the end of the array, in the order in
	// which they appear.  The new length the array is returned as the result
	// of the call.
	public static uint push(object _this, params object[] args)
		{
		uint n = JConvert.ToUInt32(Support.GetProperty(_this, "length"));
		foreach (object curArg in args)
			Support.AssignProperty( _this,
									JArrayObject.ArrayIndexToString(n++),
									curArg );
		
		Support.AssignProperty(_this, "length", n);
		return n;

		// HACK snewman 10/2/01: the spec states that "the length property of
		// the join method is 1".  Need to implement this.
		} // push
	
	
	// The elements of the array are rearranged so as to reverse their order.
	// The object is returned as the result of the call.
	public static object reverse(object _this)
		{
		uint length = JConvert.ToUInt32(Support.GetProperty(_this, "length"));
		uint lenDiv2 = length >> 1;
		
		for (uint k = 0; k < lenDiv2; k++)
			{
			uint kPrime = length - k - 1;
			
			string kString      = JArrayObject.ArrayIndexToString(k     );
			string kPrimeString = JArrayObject.ArrayIndexToString(kPrime);
			
			object entryK      = Support.GetProperty(_this, kString);
			object entryKPrime = Support.GetProperty(_this, kPrimeString);
			
			if (entryKPrime is JUndefinedObject)
				{
				if (entryK is JUndefinedObject)
					{
					Support.DeleteProperty(_this, kString);
					Support.DeleteProperty(_this, kPrimeString);
					}
				else
					{
					Support.DeleteProperty(_this, kString);
					Support.AssignProperty(_this, kPrimeString, entryK);
					}
				
				}
			else
				{
				if (entryK is JUndefinedObject)
					{
					Support.AssignProperty(_this, kString, entryKPrime);
					Support.DeleteProperty(_this, kPrimeString);
					}
				else
					{
					Support.AssignProperty(_this, kString, entryKPrime);
					Support.AssignProperty(_this, kPrimeString, entryK);
					}
				
				}
			
			} // k loop
		
		return _this;
		} // reverse
	
	
	// The first element of the array is removed from the array and returned.
	public static object shift(object _this)
		{
		uint length = JConvert.ToUInt32(Support.GetProperty(_this, "length"));
		if (length == 0)
			{
			Support.AssignProperty(_this, "length", 0);
			return JUndefinedObject.instance;
			}
		
		object shifted = Support.GetProperty(_this, "0");
		
		for (uint k=1; k < length; k++)
			{
			string kString   = JArrayObject.ArrayIndexToString(k);
			string kM1String = JArrayObject.ArrayIndexToString(k-1);
			
			object entryK = Support.GetProperty(_this, kString);
			if (entryK is JUndefinedObject)
				Support.DeleteProperty(_this, kM1String);
			else
				Support.AssignProperty(_this, kM1String, entryK);
			
			}
		
		string newLenStr = JArrayObject.ArrayIndexToString(length-1);
		Support.DeleteProperty(_this, newLenStr);
		Support.AssignProperty(_this, "length", length-1);
		
		return shifted;
		} // shift
	
	
	// Glue for "slice" with no "end" parameter
	public static JArrayObject slice(object _this, int start)
		{
		return slice( _this, start,
					  JConvert.ToInt32(Support.GetProperty(_this, "length")) );
		} // join
	
	
	// The slice method takes two arguments, start and end, and returns an array
	// containing the elements of the array from element start up to, but not
	// including, element end (or through the end of the array if end is undefined).
	// If start is negative, it is treated as (length+start) where length is the
	// length of the array.  If end is negative, it is treated as (length+end)
	// where length is the length of the array.
	public static JArrayObject slice(object _this, int start, int end)
		{
		JArrayObject newArray = new JArrayObject();
		
		uint length = JConvert.ToUInt32(Support.GetProperty(_this, "length"));
		
		int adjustedStart = (start < 0) ? Math.Max((int)(length+start), 0)
										: Math.Min(start, (int)length);
		int adjustedEnd   = (end < 0) ? Math.Max((int)(length+end), 0)
									  : Math.Min(end, (int)length);
		
		uint n = 0;
		for (int k = adjustedStart; k < adjustedEnd; k++)
			{
			string kString = JArrayObject.ArrayIndexToString((uint)k);
			object entryK = Support.GetProperty(_this, kString);
			if (entryK is JUndefinedObject)
				newArray.IncrementLength();
			else
				newArray.Add(entryK);
			
			n++;
			}
		
		return newArray;
		
		// HACK snewman 10/2/01: the spec states that "the length property of
		// the join method is 2".  Need to implement this.
		} // slice
	
	
	// Glue for "sort" with no "comparefn" parameter
	public static object sort(object _this)
		{
		return sort(_this, null);
		} // sort
	
	
	// HACK snewman 10/3/01: implement the "sort" method.  See the description
	// on pp. 94-95 (PDF pages #106-107) of the ECMAScript 3 spec.
	public static object sort(object _this, JFunctionObject comparefn)
		{
		throw new Exception("Array.sort not yet implemented");
		} // sort
	
	
	// When the splice method is called with two or more arguments start,
	// deleteCount and (optionally) item1, item2, etc., the deleteCount
	// elements of the array starting at array index start are replaced by
	// the arguments item1, item2, etc.
	// 
	// NOTE: we return a new array containing the deleted elements.  This
	// is not explicitly mentioned in the description in the spec, but it
	// is part of the algorithm contained in the spec.
	public static JArrayObject splice( object _this, int start, int deleteCount,
									   params object[] args )
		{
		JArrayObject newArray = new JArrayObject();
		uint length = JConvert.ToUInt32(Support.GetProperty(_this, "length"));
		uint argCount = (uint)args.Length;
		
		uint adjustedStart = (start < 0) ? Math.Max((uint)((int)length+start), 0)
										 : Math.Min((uint)start, length);
		
		uint adjustedDel = Math.Min( (uint)Math.Max(deleteCount,0),
									 (uint)(length-adjustedStart) );
		uint newLength = length - adjustedDel + argCount;
		
		for (int k=0; k < adjustedDel; k++)
			{
			object entry = Support.GetProperty(_this,
				JArrayObject.ArrayIndexToString((uint)(adjustedStart+k)) );
			
			if (entry is JUndefinedObject)
				newArray.IncrementLength();
			else
				newArray.Add(entry);
			}
		
		if (argCount < adjustedDel)
			{
			for (uint k = adjustedStart; k < length - adjustedDel; k++)
				{
				object tempEntry = Support.GetProperty( _this,
							JArrayObject.ArrayIndexToString(k+adjustedDel) );
				string kArgString = JArrayObject.ArrayIndexToString(k+argCount);
				if (tempEntry is JUndefinedObject)
					Support.DeleteProperty(_this, kArgString);
				else
					Support.AssignProperty(_this, kArgString, tempEntry);
				
				}
			
			for (uint k = length; k > newLength; k--)
				Support.DeleteProperty( _this,
										JArrayObject.ArrayIndexToString(k-1) );
			
			}
		else if (argCount > adjustedDel)
			{
			for (uint k = length - adjustedDel; k > adjustedStart; k--)
				{
				object tempEntry = Support.GetProperty( _this,
							JArrayObject.ArrayIndexToString(k+adjustedDel-1) );
				string kArgString = JArrayObject.ArrayIndexToString(k+argCount-1);
				if (tempEntry is JUndefinedObject)
					Support.DeleteProperty(_this, kArgString);
				else
					Support.AssignProperty(_this, kArgString, tempEntry);
				}
			
			}
		
		uint argIndex = adjustedStart;
		foreach (object curArg in args)
			{
			Support.AssignProperty( _this,
									JArrayObject.ArrayIndexToString(argIndex),
									curArg );
			argIndex++;
			}
		
		Support.AssignProperty(_this, "length", newLength);
		return newArray;
		
		// HACK snewman 10/2/01: the spec states that "the length property of
		// the splice method is 2".  Need to implement this.
		} // splice
	

	// The arguments are prepended to the start of the array, such that their
	// order within the array is the same as the order in which they appear
	// in the argument list.
	public static uint unshift(object _this, params object[] args)
		{
		uint length = JConvert.ToUInt32(Support.GetProperty(_this, "length"));
		uint argCount = (uint) args.Length;
		
		for (uint k=length; k > 0; k--)
			{
			object tempEntry = Support.GetProperty( _this,
						JArrayObject.ArrayIndexToString(k-1) );
			string kArgString = JArrayObject.ArrayIndexToString(k+argCount-1);
			if (tempEntry is JUndefinedObject)
				Support.DeleteProperty(_this, kArgString);
			else
				Support.AssignProperty(_this, kArgString, tempEntry);
			
			}
		
		uint argIndex = 0;
		foreach (object curArg in args)
			{
			Support.AssignProperty( _this,
									JArrayObject.ArrayIndexToString(argIndex),
									curArg );
			argIndex++;
			}
		
		Support.AssignProperty(_this, "length", length+argCount);
		return length+argCount;
		} // unshift
	
	
	// This function implements both join (useLocaleString=false) and
	// toLocaleString (useLocaleString=true).
	private static string joinCore(object _this, string separator, bool useLocaleString)
		{
		uint length = JConvert.ToUInt32(Support.GetProperty(_this, "length"));
		if (length == 0)
			return "";
		
		StringBuilder result = new StringBuilder();
		
		for (uint k=0; k<length; k++)
			{
			if (k > 0)
				result.Append(separator);
			
			object entry = Support.GetProperty(_this, JArrayObject.ArrayIndexToString(k));
			if (entry != null && !(entry is JUndefinedObject))
				{
				string entryStr;
				if (useLocaleString)
					entryStr = JConvert.ToString(Support.CallMethod(entry, "toLocaleString", new Object[0]));
				else
					entryStr = JConvert.ToString(entry);
				
				result.Append(entryStr);
				}
			
			}
		
		return result.ToString();
		} // toLocaleString
	
	
	// If obj is a JArrayObject, append its contents to array.  Otherwise,
	// append obj to array.
	private static void AppendToArray(JArrayObject array, object obj)
		{
		if (obj is JArrayObject)
			{
			JArrayObject objAsArray = (JArrayObject)obj;
			uint arrayLen = objAsArray.ArrayLength;
			
			for (uint i=0; i<arrayLen; i++)
				{
				object curEntry = objAsArray.Get(JArrayObject.ArrayIndexToString(i));
				if (!(curEntry is JUndefinedObject))
					array.Add(curEntry);
				else
					array.IncrementLength();
				
				}
			
			}
		else
			array.Add(obj);
		
		} // AppendToArray
	
	
	} // ArrayPrototypeMethods


// This class defines the methods of the standard String constructor.
public class StringConstructorMethods
	{
	public static string fromCharCode(object _this, params object[] args)
		{
		StringBuilder builder = new StringBuilder();
		foreach (object curArg in args)
			{
			UInt16 curCharCode = JConvert.ToUInt16(curArg);
			builder.Append((char)curCharCode);
			}
		
		return builder.ToString();
		
		// HACK snewman 10/2/01: the spec states that "the length property of
		// the fromCharCode method is 1".  Need to implement this.
		} // fromCharCode
	
	
	} // StringConstructorMethods


// This class defines the methods of the standard String prototype.
public class StringPrototypeMethods
	{
	public static string toString(JStringObject _this)
		{
		return _this.value;
		} // toString
	
	
	public static string valueOf(JStringObject _this)
		{
		return _this.value;
		} // valueOf
	
	
	// Returns a string containing the character at position pos in the string
	// resulting from converting this object to a string.  If there is no
	// character at that position, the result is the empty string. The result is
	// a string value, not a String object.
	// 
	// If pos is a value of Number type that is an integer, then the result of
	// x.charAt(pos) is equal to the result of x.substring(pos, pos+1).
	public static string charAt(string _this, int pos)
		{
		if (pos < 0 || pos >= _this.Length)
			return "";
		
		return _this[pos].ToString();
		} // charAt
	
	
	// Returns a number (a nonnegative integer less than 2^16) representing the
	// code point value of the character at position pos in the string
	// resulting from converting this object to a string.  If there is no
	// character at that position, the result is NaN.
	public static double charCodeAt(string _this, int pos)
		{
		if (pos < 0 || pos >= _this.Length)
			return Double.NaN;
		
		return (double) _this[pos];
		} // charCodeAt
	
	
	// When the concat method is called with zero or more arguments string1,
	// string2, etc., it returns a string consisting of the characters of
	// this object (converted to a string) followed by the characters of
	// each of string1, string2, etc. (where each argument is converted to
	// a string). The result is a string value, not a String object.
	public static string concat(string _this, params object[] args)
		{
		StringBuilder builder = new StringBuilder();
		builder.Append(_this);
		
		foreach (object curArg in args)
			builder.Append(JConvert.ToString(curArg));
		
		return builder.ToString();
		
		// HACK snewman 10/2/01: the spec states that "the length property of
		// the concat method is 1".  Need to implement this.
		} // concat
	
	
	// If searchString appears as a substring of the result of converting this
	// object to a string, at one or more positions that are greater than or
	// equal to position, then the index of the smallest such position is
	// returned; otherwise, -1 is returned.  If position is undefined, 0 is
	// assumed, so as to search all of the string.
	public static int indexOf(string _this, string searchString, int position)
		{
		int len = _this.Length;
		int searchLen = searchString.Length;
		position = Math.Min(Math.Max(position, 0), len);
		if (position >= len)
			return -1;
		
		return _this.IndexOf(searchString, position);

		// HACK snewman 10/2/01: the spec states that "the length property of
		// the indexOf method is 1".  Need to implement this.
		} // indexOf
	
	
	// Overload of indexOf where the position parameter is not defined.
	public static int indexOf(string _this, string searchString)
		{
		return indexOf(_this, searchString, 0);
		} // indexOf (implicit position)
	
	
	// If searchString appears as a substring of the result of converting this
	// object to a string at one or more positions that are smaller than or
	// equal to position, then the index of the greatest such position is
	// returned; otherwise, -1 is returned.  If position is undefined, the
	// length of the string value is assumed, so as to search all of the string.
	public static int lastIndexOf(string _this, string searchString, int position)
		{
		// HACK snewman 10/17/01: if position is NaN or +infinity, we're
		// supposed to start at the end of the string.  Not sure what will
		// actually happen, given that we're letting the C# glue do the
		// parameter conversion.
		
		int len = _this.Length;
		int searchLen = searchString.Length;
		position = Math.Min(Math.Max(position, 0), len);
		
		return _this.LastIndexOf(searchString, position);

		// HACK snewman 10/2/01: the spec states that "the length property of
		// the lastIndexOf method is 1".  Need to implement this.
		} // lastIndexOf
	
	
	// Overload of lastIndexOf where the position parameter is not defined.
	public static int lastIndexOf(string _this, string searchString)
		{
		return lastIndexOf(_this, searchString, 0x7FFFFFFF);
		} // lastIndexOf (implicit position)
	
	
	// Return a numerical value indicating the sort order of two strings.
	public static int localeCompare(string _this, string that)
		{
		return String.Compare(_this, that);
		} // localeCompare
	
	
	// Return _this, with all instances of searchValue replaced by replaceValue.
	public static string replace( string _this, string searchValue,
								  string replaceValue )
		{
		// HACK snewman 10/17/01: the ECMAScript spec states that replaceValue
		// can be a function instead of a string.  Need to implement this.
		
		// HACK snewman 10/17/01: implement $ escapes as described in the spec.
		
		return _this.Replace(searchValue, replaceValue);
		} // replace
	
	
	// The slice method takes two arguments, start and end, and returns a
	// substring of the result of converting this object to a string, starting
	// from character position start and running to, but not including,
	// character position end (or through the end of the string if end is
	// undefined). If start is negative, it is treated as (sourceLength+start)
	// where sourceLength is the length of the string.  If end is negative, it
	// is treated as (sourceLength+end) where sourceLength is the length of
	// the string.
	public static string slice(string _this, int start, int end)
		{
		int len = _this.Length;
		start = (start < 0) ? Math.Max(len+start, 0) : Math.Min(start, len);
		end   = (end   < 0) ? Math.Max(len+end,   0) : Math.Min(end,   len);
		int spliceLen = Math.Max(end-start, 0);
		
		return _this.Substring(start, spliceLen);
		
		// HACK snewman 10/2/01: the spec states that "the length property of
		// the slice method is 2".  Need to implement this.
		} // slice
	
	
	// Overload of slice where the end parameter is not defined.
	public static string slice(string _this, int start)
		{
		return slice(_this, start, _this.Length);
		} // slice (implicit end)
	
	
	// Returns an Array object into which substrings of the result of converting
	// this object to a string have been stored.  The substrings are determined
	// by searching from left to right for occurrences of separator; these
	// occurrences are not part of any substring in the returned array, but
	// serve to divide up the string value.  The value of separator may be a
	// string of any length or it may be a RegExp object.
	public static JArrayObject split(string _this, string separator, uint limit)
		{
		// HACK snewman 10/17/01: add support for RegExpr separators.
		
		JArrayObject A = new JArrayObject();
		if (limit == 0)
			return A;
		
		int len = _this.Length;
		int matchEnd;
		if (len == 0)
			{
			matchEnd = SplitMatch(separator, _this, 0);
			if (matchEnd < 0)
				A.Add(_this);
			return A;
			}
		
		// Look for split positions.
		int fragmentStart = 0;
		while (true)
			{
			bool foundFrag = false;
			for (int searchPos = fragmentStart+1; searchPos < len; searchPos++)
				{
				matchEnd = SplitMatch(separator, _this, searchPos);
				if (matchEnd >= 0)
					{
					A.Add(_this.Substring(fragmentStart, searchPos-fragmentStart));
					
					if (A.ArrayLength >= limit)
						return A;
					
					fragmentStart = matchEnd;
					foundFrag = true;
					break;
					}
				
				}
			
			if (!foundFrag)
				break;
			
			} // while (true)

		A.Add(_this.Substring(fragmentStart, len-fragmentStart));
		return A;

		// HACK snewman 10/2/01: the spec states that "the length property of
		// the split method is 2".  Need to implement this.
		} // split
	
	
	// Perform the SplitMatch operation specified in the ECMAScript spec (in
	// the algorithm for String.split).  For non-regexp patterns (as we
	// assume), this essentially a simple string comparison.
	private static int SplitMatch(string R, string S, int searchPos)
		{
		int r = R.Length;
		int s = S.Length;
		if (searchPos+r > s)
			return -1;
		
		if (String.Compare(S, searchPos, R, 0, r) != 0)
			return -1;
		
		return searchPos+r;
		} // SplitMatch
	
	
	// Overload of split where the limit parameter is not specified.
	public static JArrayObject split(string _this, string separator)
		{
		return split(_this, separator, 0xFFFFFFFF);
		} // split (implicit limit)
	
	
	// Overload of split where the separator and limit parameters are not specified.
	public static JArrayObject split(string _this)
		{
		JArrayObject A = new JArrayObject();
		A.Add(_this);
		return A;
		} // split (implicit separator, limit)
	
	
	// The substring method takes two arguments, start and end, and returns a
	// substring of the result of converting this object to a string, starting
	// from character position start and running to, but not including, character
	// position end of the string (or through the end of the string if end is
	// undefined).
	public static string substring(string _this, int start, int end)
		{
		int srcLen = _this.Length;
		start = Math.Min(Math.Max(start, 0), srcLen);
		end   = Math.Min(Math.Max(end,   0), srcLen);
		
		if (start > end)
			{
			int temp = start;
			start = end;
			end = temp;
			}
		
		return _this.Substring(start, end-start);
		
		// HACK snewman 10/2/01: the spec states that "the length property of
		// the substring method is 2".  Need to implement this.
		} // substring
	
	
	// Overload of substring where the end parameter is not specified.
	public static string substring(string _this, int start)
		{
		return substring(_this, start, _this.Length);
		} // substring (implicit end)
	
	
	// If this object is not already a string, it is converted to a string.
	// The characters in that string are converted one by one to lower case.
	// The result is a string value, not a String object.
	public static string toLowerCase(string _this)
		{
		return _this.ToLower();
		} // toLowerCase
	
	
	// This function works exactly the same as toLowerCase except that its
	// result is intended to yield the correct result for the host environment's
	// current locale, rather than a locale-independent result.
	public static string toLocaleLowerCase(string _this)
		{
		return _this.ToLower();
		} // toLocaleLowerCase
	
	
	// If this object is not already a string, it is converted to a string.
	// The characters in that string are converted one by one to upper case.
	// The result is a string value, not a String object.
	public static string toUpperCase(string _this)
		{
		return _this.ToUpper();
		} // toUpperCase
	
	
	// This function works exactly the same as toUpperCase except that its
	// result is intended to yield the correct result for the host environment's
	// current locale, rather than a locale-independent result.
	public static string toLocaleUpperCase(string _this)
		{
		return _this.ToUpper();
		} // toLocaleUpperCase
	
	
	} // StringPrototypeMethods


// This class defines the methods of the builtin Math object.
// 
// HACK snewman 10/3/01: these are generally implemented as straight
// call-throughs to the .NET Math class.  Technically, I ought review
// to ensure that these meet the ECMAScript specification, including all
// corner cases.  For example, the current implementation of Round returns
// +0 in some cases where ECMAScript specifies -0.
public class MathMethods
	{
	public static double abs(object _this, double x)
		{
		return Math.Abs(x);
		} // abs
	
	
	public static double acos(object _this, double x)
		{
		return Math.Acos(x);
		} // acos
	
	
	public static double asin(object _this, double x)
		{
		return Math.Asin(x);
		} // asin
	
	
	public static double atan(object _this, double x)
		{
		return Math.Atan(x);
		} // atan
	
	
	public static double atan2(object _this, double y, double x)
		{
		return Math.Atan2(y,x);
		} // atan2
	
	
	public static double ceil(object _this, double x)
		{
		return Math.Ceiling(x);
		} // ceil
	
	
	public static double cos(object _this, double x)
		{
		return Math.Cos(x);
		} // cos
	
	
	public static double exp(object _this, double x)
		{
		return Math.Exp(x);
		} // exp
	
	
	public static double floor(object _this, double x)
		{
		return Math.Floor(x);
		} // floor
	
	
	public static double log(object _this, double x)
		{
		return Math.Log(x);
		} // log
	
	
	public static double max(object _this, params object[] args)
		{
		double x = Double.NegativeInfinity;
		foreach (object curArg in args)
			x = Math.Max(x, JConvert.ToNumber(curArg));
		
		return x;
		
		// HACK snewman 10/2/01: the spec states that "the length property of
		// the max method is 2".  Need to implement this.
		} // max
	
	
	public static double min(object _this, params object[] args)
		{
		double x = Double.PositiveInfinity;
		foreach (object curArg in args)
			x = Math.Min(x, JConvert.ToNumber(curArg));
		
		return x;
		
		// HACK snewman 10/2/01: the spec states that "the length property of
		// the min method is 2".  Need to implement this.
		} // min
	
	
	public static double pow(object _this, double x, double y)
		{
		return Math.Pow(x,y);
		} // pow
	
	
	private static Random rGen = null;
	
	public static double rand(object _this)
		{
		if (rGen == null)
			rGen = new Random();
		
		return rGen.NextDouble();
		} // rand
	
	
	public static double round(object _this, double x)
		{
		return Math.Floor(x+0.5);
		} // round
	
	
	public static double sin(object _this, double x)
		{
		return Math.Sin(x);
		} // sin
	
	
	public static double sqrt(object _this, double x)
		{
		return Math.Sqrt(x);
		} // sqrt
	
	
	public static double tan(object _this, double x)
		{
		return Math.Tan(x);
		} // tan
	
	
	} // MathMethods


// This class defines builtin methods of the builtin global object.
public class GlobalMethods
	{
	public static bool isNaN(object _this, double x)
		{
		return Double.IsNaN(x);
		} // isNaN
	
	
	public static bool isFinite(object _this, double x)
		{
		return !(Double.IsNaN(x) || Double.IsInfinity(x));
		} // isFinite
	
	
	} // GlobalMethods



// This interface is inherited by all compiled JANET programs.
// public interface IJANETProgram
// 	{
// 	JObject GetGlobals();
// 	
// 	void Main();
// 	} // IJANETProgram

} // namespace JANET.Runtime
