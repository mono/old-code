// JObjects.cs: runtime classes which represent script-created objects
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
using System.Diagnostics;
using System.Collections;

using JANET.Runtime;


namespace JANET.Runtime {

// This struct encapsulates a single property within a script object.
public struct JProperty
	{
	public enum AttrFlags : int { none=0, readOnly=1, dontEnum=2, dontDelete=4 }
	
	public AttrFlags attributes;
	public string    name;
	public object    value;
	}


// This is the base class for all objects defined or created by script code.
// It implements the basic ECMAScript object features such as prototypes and
// runtime-defined ("expando") properties.
public class JObject
	{
	public  JObject proto;       // Prototype from which we inherit, or null.
	public  object  constructor; // Object from which we were constructed, or null.
	public  string  className;   // A description of this object's type.
	protected System.Collections.Hashtable props; // Properties of this object.  Maps string to JProperty.
	
	
	// Object prototype.
	private static JObject objectPrototype = null;
	public static JObject ObjectPrototype { get
		{
		if (objectPrototype == null)
			{
			objectPrototype = new JObject(null, null, "Object");
			objectPrototype.constructor = ObjectConstructorObj;
			}
		
		// Note that most methods of this class are added by
		// Support.DefineBuiltinGlobals.
		
		return objectPrototype;
		} } // ObjectPrototype
	
	
	// Object constructor.
	private static JFunctionObject objectConstructorObj;
	public static JFunctionObject ObjectConstructorObj { get
		{
		if (objectConstructorObj == null)
			{
			objectConstructorObj = new JFunctionObject(
				new JFunctionObject.JFunctionImp(ObjectConstructorFunction),
				new JFunctionObject.JFunctionImp(ObjectConstructor),
				new object[0] );
			objectConstructorObj.instanceProto = ObjectPrototype;
			objectConstructorObj.Put("length", 1);
			
			// HACK snewman 10/3/01: also need to set these three
			// attributes for the Array and String constructors.
			// Probably for all built-in constructors.  Need to
			// check the spec and see if this is true for user-defined
			// constructors as well.
			JProperty prop;
			prop.name = "prototype";
			prop.attributes = JProperty.AttrFlags.dontEnum   |
							  JProperty.AttrFlags.dontDelete |
							  JProperty.AttrFlags.readOnly;
			prop.value = ObjectPrototype;
			objectConstructorObj.StoreProperty("prototype", prop);
			}
		
		return objectConstructorObj;
		} } // ObjectConstructorObj
	
	
	// Implementation for calling the Object constructor as a function.
	// "When Object is called as a function rather than as a constructor, it
	// performs a type conversion."
	private static object ObjectConstructorFunction(object this_, params object[] args)
		{
		if (args.Length == 0 || args[0] == null || args[0] is JUndefinedObject)
			return ObjectConstructor(null, args);
		
		return JConvert.ToObject(args[0]);
		} // ObjectConstructorFunction
	
	
	// Implementation for calling the Object constructor as a constructor.
	// "When Object is called as part of a new expression, it is a
	// constructor that may create an object."
	public static object ObjectConstructor(object this_, params object[] args)
		{
		if (args.Length == 0 || args[0] == null || args[0] is JUndefinedObject)
			return new JObject(ObjectPrototype, ObjectConstructorObj, "Object");
		else
			{
			object v = args[0];
			if (v is bool || IsNumber(v) || v is string)
				return JConvert.ToObject(v);
			else
				return v;
			
			}
		
		} // ObjectConstructor
	
	
	// Construct a JObject with the given prototype, constructor link, and
	// class name.
	public JObject(JObject proto, object constructor, string className)
		{
		this.proto       = proto;
		this.constructor = constructor;
		this.className   = className;
		this.props       = new System.Collections.Hashtable();
		} // JObject constructor
	
	
	// Object manipulation methods.  Names are taken from the ECMAScript spec.
	
	// Return the value of our property with the given name.  If we have no
	// such property, search the prototype chain, and then return undefined.
	public virtual object Get(string propName)
		{
		JProperty prop;
		if (LookupProperty(propName, out prop))
			return prop.value;
		else if (proto != null)
			return proto.Get(propName);
		else
			return JUndefinedObject.instance;
		
		} // Get (string propName)
	
	
	public virtual object Get(object propName)
		{
		return Get(JConvert.ToString(propName));
		} // Get (object propName)
	
	
	// Store the given value in a property of this object, updating any
	// existing property or else creating a new one.  If the object (or
	// a prototype) has an existing property with the readOnly flag set,
	// we do nothing.
	// 
	// We return the value that was assigned.
	public virtual object Put(string propName, object value)
		{
		if (CanPut(propName))
			{
			JProperty prop;
			if (!LookupProperty(propName, out prop))
				{
				prop.name = propName;
				prop.attributes = JProperty.AttrFlags.none;
				}
			
			prop.value = value;
			StoreProperty(propName, prop);
			}
		
		return value;
		} // Put
	
	
	// Return true if this object, or a prototype, has a property of the
	// given name.
	public virtual bool HasProperty(string propName)
		{
		JProperty prop;
		if (LookupProperty(propName, out prop))
			return true;
		else if (proto != null)
			return proto.HasProperty(propName);
		else
			return false;
		
		} // HasProperty
	
	
	// Remove any property of the given name from this object, and return
	// true.  If the object has a property of this name with the dontDelete
	// attribute set, then we leave the property alone and return false.
	// 
	// This method does not search the prototype chain.
	public virtual bool Delete(string propName)
		{
		JProperty prop;
		if (!LookupProperty(propName, out prop))
			return true;
		
		if ((prop.attributes & JProperty.AttrFlags.dontDelete) != 0)
			return false;
		else
			{
			RemoveProperty(propName);
			return true;
			}
		
		} // Delete
	
	
	// Return true if it's legal to create or update a property of the given
	// name in this object.  We only return false when the object (or a
	// prototype) has an existing property with the readOnly flag set.
	internal virtual bool CanPut(string propName)
		{
		JProperty prop;
		if (LookupProperty(propName, out prop))
			return (prop.attributes & JProperty.AttrFlags.readOnly) == 0;
		else if (proto != null)
			return proto.CanPut(propName);
		else
			return true;
		
		} // CanPut
	
	
	// Perform the [[Construct]] operation on this object.
	// 
	// The JObject implementation simply throws TypeError, per the ECMAScript
	// spec.  JFunctionObject overrides it.
	public virtual object Construct(JObject this_, params object[] args)
		{
		throw new TypeError("[[Construct]] invoked for a non-function object");
		} // Construct
	
	
	// Perform the [[Call]] operation on this object.
	// 
	// The JObject implementation simply throws TypeError, per the ECMAScript
	// spec.  JFunctionObject overrides it.
	public virtual object Call(JObject this_, params object[] args)
		{
		throw new TypeError("[[call]] invoked for a non-function object");
		} // Call
	
	
	// Return true if x is a numeric value.
	public static bool IsNumber(object x)
		{
		// NOTE: this implementation should be kept in synch with
		// PrimSupport.AsNumber.
		
		// HACK snewman 8/7/01: flesh this out to check for all numeric types.
		return x is double || x is Int32 || x is UInt32;
		} // IsNumber
	
	
	// Return true if the given value is a primitive value (number, string,
	// etc.).
	public static bool IsPrimitive(object value)
		{
		return ( value == null             ||
				 value is JUndefinedObject ||
				 value is bool             ||
				 value is string           ||
				 JObject.IsNumber(value) );
		} // IsPrimitive
	
	
	// Implement the "typeof" operator.
	// 
	// Note that when typeof is applied to a reference, and the object
	// being referenced is null, typeof is supposed to return "undefined".
	// The caller must test for this case before resolving the reference
	// and invoking us.
	public static string Typeof(object x)
		{
		if (x == null)
			return ("object");
		else if (x is JUndefinedObject)
			return "undefined";
		else if (x is bool)
			return "boolean";
		else if (IsNumber(x))
			return "number";
		else if (x is string)
			return "string";
		else if (x is JFunctionObject)
			return "function";
		else
			return "object";
		
		} // Typeof
	
	
	public enum ValueHintType { String, Number, None };
	
	// This method implements the [[DefaultValue]] operation in the ECMAScript
	// specification.
	// 
	// HACK snewman 8/6/01: might factor this into three separate methods,
	// and might specialize it for various subclasses of JObject.
	public virtual object DefaultValue(ValueHintType hint)
		{
		// HACK snewman 8/7/01: move this into JDate
		// 
		// if (hint == ValueHintType.None && (this is JDate))
		// 	hint = ValueHintType.String;
		
		object result;
		if (hint == ValueHintType.String)
			{
			if ( CallMethodIfPresent("toString", out result) &&
				 IsPrimitive(result) )
				return result;
			else if ( CallMethodIfPresent("valueOf", out result) &&
				 IsPrimitive(result) )
				return result;
			
			throw new TypeError("DefaultValue called for an object with no valueOf or toString method");
			}
		else if (hint == ValueHintType.Number || hint == ValueHintType.None)
			{
			if ( CallMethodIfPresent("valueOf", out result) &&
				 IsPrimitive(result) )
				return result;
			else if ( CallMethodIfPresent("toString", out result) &&
				 IsPrimitive(result) )
				return result;
			
			throw new TypeError("DefaultValue called for an object with no valueOf or toString method");
			}
		else
			{
			Trace.Assert(false, "unknown ValueHintType");
			return null;
			}
		
		} // DefaultValue
	
	
	// If this object has a method of the given name, call it, store the
	// return value in the result parameter, and return true.  Otherwise
	// set result to null and return false.
	internal virtual bool CallMethodIfPresent( string name, out object result,
											   params object[] args )
		{
		result = null;
		
		object method = Get(name);
		if (method == null)
			return false;
		
		JObject methodObj = method as JFunctionObject;
		if (methodObj == null)
			return false;
		
		result = methodObj.Call(this, args);
		return true;
		} // CallMethodIfPresent
	
	
	// Return true if this object has a property of the given name,
	// ignoring prototype properties.
	public virtual bool HasOwnProperty(string propName)
		{
		JProperty prop;
		return LookupProperty(propName, out prop);
		} // HasOwnProperty
	
	
	// If this object has a property of the given name, fill in prop
	// and return true.  Otherwise, return false (and prop is in an
	// undefined state).
	public virtual bool LookupProperty(string propName, out JProperty prop)
		{
		if (props.ContainsKey(propName))
			{
			prop = (JProperty) (props[propName]);
			return true;
			}
		else
			{
			// Fill in prop to silence a compiler warning for uninitialized "out"
			prop.attributes = JProperty.AttrFlags.none;
			prop.name       = null;
			prop.value      = null;
			return false;
			}
		
		} // LookupProperty
	
	
	// Store the given property under the given name, overwriting any
	// existing property of the same name.
 	protected virtual void StoreProperty(string propName, JProperty prop)
		{
		Trace.Assert(prop.name == propName);
		props[propName] = prop;
		} // StoreProperty
	
	
	// Remove any property of the given name.  It is illegal to call
	// this method if the object has a property of that name with the
	// dontDelete attribute set.
	protected virtual void RemoveProperty(string propName)
		{
		props.Remove(propName);
		} // RemoveProperty
	
	
	} // JObject


// This is the class for the "Undefined" type.
public class JUndefinedObject : JObject
	{
	// Return the one global instance of this type.
	public static JUndefinedObject instance { get {
		if (instance_ == null) instance_ = new JUndefinedObject();
		return instance_;
		}}
	
	private JUndefinedObject() : base(null, null, "Undefined") {}
	
	private static JUndefinedObject instance_; // The one global instance of this
											   // class; null until first use.
	} // JUndefinedObject


// This is the class for the "Boolean" type.
public sealed class JBooleanObject : JObject
	{
	public bool value;
	
	public JBooleanObject(bool value)
	 : base(BooleanPrototype, BooleanConstructorObj, "Boolean")
		{
		this.value = value;
		} // JBooleanObject constructor
	
	
	// Only used when creating booleanPrototype.
	private JBooleanObject(JObject prototype, bool value)
	 : base(prototype, null, "Boolean")
		{
		this.value = value;
		} // JBooleanObject constructor
	
	
	// Boolean prototype.
	private static JObject booleanPrototype;
	public static JObject BooleanPrototype { get
		{
		if (booleanPrototype == null)
			{
			booleanPrototype = new JBooleanObject(JObject.ObjectPrototype, false);
			booleanPrototype.constructor = BooleanConstructorObj;
			}
		
		// Note that most methods of this class are added by
		// Support.DefineBuiltinGlobals.
		
		return booleanPrototype;
		} } // BooleanPrototype
	
	
	// Boolean constructor.
	private static JFunctionObject booleanConstructorObj;
	public static JFunctionObject BooleanConstructorObj { get
		{
		if (booleanConstructorObj == null)
			{
			booleanConstructorObj = new JFunctionObject(
				new JFunctionObject.JFunctionImp(BooleanConstructorFunction),
				new JFunctionObject.JFunctionImp(BooleanConstructor),
				new object[0] );
			booleanConstructorObj.instanceProto = BooleanPrototype;
			booleanConstructorObj.Put("length", 1);
			}
		
		return booleanConstructorObj;
		} } // BooleanConstructorObj
	
	
	// Implementation for calling the Boolean constructor as a function.
	// "When Boolean is called as a function rather than as a constructor,
	// it performs a type conversion."
	private static object BooleanConstructorFunction(object this_, params object[] args)
		{
		if (args.Length == 0)
			return 0;
		else
			return JConvert.ToBoolean(args[0]);
		
		} // BooleanConstructorFunction
	
	
	// Implementation for calling the Boolean constructor as a constructor.
	// "When Boolean is called as part of a new expression, it is a constructor:
	// it initialises the newly created object."
	public static object BooleanConstructor(object this_, params object[] args)
		{
		bool boolValue;
		if (args.Length == 0)
			boolValue = false;
		else
			boolValue = JConvert.ToBoolean(args[0]);
		
		return new JBooleanObject(boolValue);
		} // BooleanConstructor
	
	} // JBooleanObject


// This is the class for the "Number" type.
public sealed class JNumberObject : JObject
	{
	public double value;
	
	public JNumberObject(double value)
	 : base(NumberPrototype, NumberConstructorObj, "Number")
		{
		this.value = value;
		} // JNumberObject constructor
	
	
	// Only used when creating numberPrototype.
	private JNumberObject(JObject prototype, double value)
	 : base(prototype, null, "Number")
		{
		this.value = value;
		} // JNumberObject constructor
	
	
	// Number prototype.
	private static JObject numberPrototype;
	public static JObject NumberPrototype { get
		{
		if (numberPrototype == null)
			{
			numberPrototype = new JNumberObject(JObject.ObjectPrototype, 0);
			numberPrototype.constructor = NumberConstructorObj;
			}
		
		// Note that most methods of this class are added by
		// Support.DefineBuiltinGlobals.
		
		return numberPrototype;
		} } // NumberPrototype
	
	
	// Number constructor.
	private static JFunctionObject numberConstructorObj;
	public static JFunctionObject NumberConstructorObj { get
		{
		if (numberConstructorObj == null)
			{
			numberConstructorObj = new JFunctionObject(
				new JFunctionObject.JFunctionImp(NumberConstructorFunction),
				new JFunctionObject.JFunctionImp(NumberConstructor),
				new object[0] );
			numberConstructorObj.instanceProto = NumberPrototype;
			numberConstructorObj.Put("length", 1);
			numberConstructorObj.Put("MAX_VALUE",         Double.MaxValue);
			numberConstructorObj.Put("MIN_VALUE",         Double.MinValue);
			numberConstructorObj.Put("NaN",               Double.NaN);
			numberConstructorObj.Put("NEGATIVE_INFINITY", Double.NegativeInfinity);
			numberConstructorObj.Put("POSITIVE_INFINITY", Double.PositiveInfinity);
			}
		
		return numberConstructorObj;
		} } // NumberConstructorObj
	
	
	// Implementation for calling the Number constructor as a function.
	// "When Number is called as a function rather than as a constructor,
	// it performs a type conversion."
	private static object NumberConstructorFunction(object this_, params object[] args)
		{
		if (args.Length == 0)
			return 0;
		else
			return JConvert.ToNumber(args[0]);
		
		} // NumberConstructorFunction
	
	
	// Implementation for calling the Number constructor as a constructor.
	// "When Number is called as part of a new expression, it is a constructor:
	// it initialises the newly created object."
	public static object NumberConstructor(object this_, params object[] args)
		{
		double numValue;
		if (args.Length == 0)
			numValue = 0;
		else
			numValue = JConvert.ToNumber(args[0]);
		
		return new JNumberObject(numValue);
		} // NumberConstructor
	
	} // JNumberObject


// This is the class for the "String" type.
public sealed class JStringObject : JObject
	{
	public string value;
	
	public JStringObject(string value)
	 : base(StringPrototype, StringConstructorObj, "String")
		{
		this.value = value;
		} // JStringObject constructor
	
	
	// Only used when creating stringPrototype.
	private JStringObject(JObject prototype, string value)
	 : base(prototype, null, "String")
		{
		this.value = value;
		} // JStringObject constructor
	
	
	// String prototype.
	private static JObject stringPrototype;
	public static JObject StringPrototype { get
		{
		if (stringPrototype == null)
			{
			stringPrototype = new JStringObject(JObject.ObjectPrototype, "");
			stringPrototype.constructor = StringConstructorObj;
			}
		
		// Note that most methods of this class are added by
		// Support.DefineBuiltinGlobals.
		
		return stringPrototype;
		} } // StringPrototype
	
	
	// String constructor.
	private static JFunctionObject stringConstructorObj;
	public static JFunctionObject StringConstructorObj { get
		{
		if (stringConstructorObj == null)
			{
			stringConstructorObj = new JFunctionObject(
				new JFunctionObject.JFunctionImp(StringConstructorFunction),
				new JFunctionObject.JFunctionImp(StringConstructor),
				new object[0] );
			stringConstructorObj.instanceProto = StringPrototype;
			stringConstructorObj.Put("length", 1);
			}
		
		return stringConstructorObj;
		} } // StringConstructorObj
	
	
	// Implementation for calling the String constructor as a function.
	// "When String is called as a function rather than as a constructor,
	// it performs a type conversion."
	private static object StringConstructorFunction(object this_, params object[] args)
		{
		if (args.Length == 0)
			return "";
		else
			return JConvert.ToString(args[0]);
		
		} // StringConstructorFunction
	
	
	// Implementation for calling the String constructor as a constructor.
	// "When String is called as part of a new expression, it is a constructor:
	// it initialises the newly created object."
	public static object StringConstructor(object this_, params object[] args)
		{
		string strValue;
		if (args.Length == 0)
			strValue = "";
		else
			strValue = JConvert.ToString(args[0]);
		
		return new JStringObject(strValue);
		} // StringConstructor
	
	
	// If this object has a property of the given name, fill in prop
	// and return true.  Otherwise, return false (and prop is in an
	// undefined state).
	public override bool LookupProperty(string propName, out JProperty prop)
		{
		if (propName == "length")
			{
			prop.attributes = JProperty.AttrFlags.readOnly |
							  JProperty.AttrFlags.dontEnum |
							  JProperty.AttrFlags.dontDelete;
			prop.name       = propName;
			prop.value      = value.Length;
			return true;
			}
		else
			return base.LookupProperty(propName, out prop);
		
		} // LookupProperty
	
	
	} // JStringObject


// This is the class for the "Array" type.
public sealed class JArrayObject : JObject
	{
	public JArrayObject() : base(ArrayPrototype, ArrayConstructorObj, "Array") {}
	private JArrayObject(JObject prototype) : base(prototype, ArrayConstructorObj, "Array") {}
	
	
	// Array prototype.
	private static JObject arrayPrototype;
	public static JObject ArrayPrototype { get
		{
		if (arrayPrototype == null)
			arrayPrototype = new JArrayObject(JObject.ObjectPrototype);
		
		// Note that most methods of this class are added by
		// Support.DefineBuiltinGlobals.
		
		return arrayPrototype;
		} } // ArrayPrototype
	
	
	// Array constructor.
	private static JFunctionObject arrayConstructorObj;
	public static JFunctionObject ArrayConstructorObj { get
		{
		if (arrayConstructorObj == null)
			{
			arrayConstructorObj = new JFunctionObject(
				new JFunctionObject.JFunctionImp(ArrayConstructorFunction),
				new JFunctionObject.JFunctionImp(ArrayConstructor),
				new object[0] );
			arrayConstructorObj.instanceProto = ArrayPrototype;
			arrayConstructorObj.Put("length", 1);
			}
		
		return arrayConstructorObj;
		} } // ArrayConstructorObj
	
	
	// Implementation for calling the Array constructor as a function.
	private static object ArrayConstructorFunction(object this_, params object[] args)
		{
		return ArrayConstructor(null, args);
		} // ArrayConstructorFunction
	
	
	// Implementation for calling the Array constructor as a constructor.
	public static object ArrayConstructor(object this_, params object[] args)
		{
		JArrayObject arrayObj = new JArrayObject();
		
		if (args.Length == 1 && JObject.Typeof(args[0]) == "number")
			{
			// Treat the one argument as a length, and create an array of
			// that length.
			UInt32 newLength = JConvert.ToUInt32(args[0]);
			if (newLength != JConvert.ToNumber(args[0]))
				throw new RangeError("array length in constructor is outside the UInt32 range");
			
			arrayObj.arrayLength = newLength;
			}
		else
			{
			// Create an array with the entries specified in the args array.
			foreach (object curArg in args)
				arrayObj.entries.Add(curArg);
			
			arrayObj.arrayLength = (uint) args.Length;
			}
		
		return arrayObj;
		} // ArrayConstructor
	
	
	// If this object has a property of the given name, fill in prop
	// and return true.  Otherwise, return false (and prop is in an
	// undefined state).
	public override bool LookupProperty(string propName, out JProperty prop)
		{
		UInt32 arrayIndex;
		if (propName == "length")
			{
			prop.attributes = JProperty.AttrFlags.dontEnum |
							  JProperty.AttrFlags.dontDelete;
			prop.name       = propName;
			prop.value      = arrayLength;
			return true;
			}
		else if (IsArrayIndex(propName, out arrayIndex) && arrayIndex < entries.Count)
			{
			prop.attributes = JProperty.AttrFlags.none;
			prop.name       = propName;
			prop.value      = entries[(int)arrayIndex];
			return true;
			}
		else
			return base.LookupProperty(propName, out prop);
		
		} // LookupProperty
	
	
	// Store the given property under the given name, overwriting any
	// existing property of the same name.
 	protected override void StoreProperty(string propName, JProperty prop)
		{
		Trace.Assert(prop.name == propName);
		
		UInt32 arrayIndex;
		if (propName == "length")
			{
			UInt32 newLength = JConvert.ToUInt32(prop.value);
			if (newLength != JConvert.ToNumber(prop.value))
				throw new RangeError("array length set to a value outside the UInt32 range");
			
			if (entries.Count > newLength)
				entries.RemoveRange((int)newLength, (int)(entries.Count - newLength));
			
			if (newLength < arrayLength)
				for (uint i=newLength; i<=arrayLength && sparsePropertyCount > 0; i++)
				{
				string iAsString = ArrayIndexToString(i);
				if (props.ContainsKey(iAsString))
					{
					sparsePropertyCount--;
					props.Remove(iAsString);
					}
				
				}
			
			arrayLength = newLength;
			}
		else if (IsArrayIndex(propName, out arrayIndex))
			{
			Trace.Assert(prop.attributes == JProperty.AttrFlags.none);
			
			if (arrayIndex < entries.Count)
				{
				// This index is already in our entries array; update it.
				entries[(int)arrayIndex] = prop.value;
				}
			else if (arrayIndex == entries.Count)
				{
				// This index lands just after our entries array.  Extend
				// the entries array to include it.
				entries.Add(prop.value);
				if (arrayIndex >= arrayLength)
					arrayLength = arrayIndex + 1;
				
				// Then, if there were any further adjacent entries being
				// stored in the hash table, move them to the entries array.
				while (sparsePropertyCount > 0)
					{
					string indexAsString = ArrayIndexToString((uint)(entries.Count));
					if (props.ContainsKey(indexAsString))
						{
						JProperty tempProp = (JProperty) (props[indexAsString]);
						entries.Add(tempProp.value);
						sparsePropertyCount--;
						props.Remove(indexAsString);
						}
					}
				
				}
			else
				{
				if (arrayIndex >= arrayLength)
					arrayLength = arrayIndex + 1;
				
				if (!props.ContainsKey(propName))
					sparsePropertyCount++;
				
				base.StoreProperty(propName, prop);
				}
			
			}
		else
			base.StoreProperty(propName, prop);
		
		} // StoreProperty
	
	
	// Remove any property of the given name.  It is illegal to call
	// this method if the object has a property of that name with the
	// dontDelete attribute set.
	protected override void RemoveProperty(string propName)
		{
		UInt32 arrayIndex;
		if (IsArrayIndex(propName, out arrayIndex))
			{
			if (arrayIndex < entries.Count)
				{
				// The index is in our entries array.  Truncate the
				// entries array just before this index, and move any
				// "orphaned" entries to the properties array.
				for (uint i = arrayIndex+1; i < entries.Count; i++)
					{
					string indexAsString = ArrayIndexToString(i);
					JProperty tempProp;
					tempProp.attributes = JProperty.AttrFlags.none;
					tempProp.name       = indexAsString;
					tempProp.value      = entries[(int)i];
					props[indexAsString] = tempProp;
					
					sparsePropertyCount++;
					}
				
				entries.RemoveRange((int)arrayIndex, (int)(entries.Count - arrayIndex));
				}
			else
				{
				// The index is not in our entries array, so it must be
				// in the props table.
				if (props.ContainsKey(propName))
					sparsePropertyCount--;
				
				base.RemoveProperty(propName);
				}
			
			// If this was the highest-indexed entry in the array,
			// then trim arrayLength to just after the previous entry.
			// This requires nontrivial logic in the case of sparse
			// arrays.
			// 
			// HACK snewman 10/3/01: need to review this code to see if it
			// will work if arrayLength has been explicitly assigned to be larger
			// than the actual array length.
			// 
			// HACK snewman 10/3/01: on further inspection, I don't see where
			// the ECMAScript specification calls for this behavior.  For now,
			// I'm commenting it out.
			// 
			// if (arrayLength == arrayIndex + 1)
			// 	if (sparsePropertyCount == 0)
			// 		arrayLength = entries.Count;
			// 	else
			// 		while ( arrayLength > entries.Count &&
			// 				!props.ContainsKey(ArrayIndexToString(arrayLength-1)) )
			// 			arrayLength--;
			}
		else
			base.RemoveProperty(propName);
		
		} // RemoveProperty
	
	
	// Append the given value to the array.
	public void Add(object value)
		{
		if (sparsePropertyCount == 0)
			{
			entries.Add(value);
			arrayLength = (uint)entries.Count;
			}
		else
			{
			string indexAsString = ArrayIndexToString(arrayLength);
			Trace.Assert(!props.ContainsKey(indexAsString));
			
			JProperty property;
			property.attributes = JProperty.AttrFlags.none;
			property.name       = indexAsString;
			property.value      = value;
			base.StoreProperty(indexAsString, property);
			
			sparsePropertyCount++;
			arrayLength++;
			}
		
		} // Add
	
	
	// Increment the array's length property.
	public void IncrementLength()
		{
		arrayLength++;
		} // IncrementLength
	
	
	// If the given string is an ECMAScript numerical array index, fill in
	// arrayIndex and return true.  Otherwise set arrayIndex to 0 and return
	// false.
	public bool IsArrayIndex(string propName, out UInt32 arrayIndex)
		{
		arrayIndex = 0;
		
		// HACK snewman 10/3/01: need to implement the ECMAScript spec more
		// precisely.
		if (propName == "0")
			return true;
		
		foreach (char c in propName)
			if (c < '0' || c > '9')
				return false;
		
		double d = JConvert.ToNumber(propName);
		if (d > 0 && d <= 0xFFFFFFFE)
			{
			arrayIndex = (UInt32)d;
			return true;
			}
		
		return false;
		} // IsArrayIndex
	
	
	// Return the string form of the given numerical array index.
	public static string ArrayIndexToString(UInt32 arrayIndex)
		{
		return arrayIndex.ToString();
		} // ArrayIndexToString
	
	
	/* The entries array stores the properties "0", "1", ... "n-1", where n
	 * is as large as possible without exceeding the number of contiguously
	 * defined properties for this object.  Properties which are stored in the
	 * entries array are not also stored in JObject.props.
	 * 
	 * HACK snewman 10/3/01: this assumes that array entries can never have any
	 * attributes, such as ReadOnly or DontDelete.  If that were incorrect, the
	 * entries array would have to be adapted to store each property's
	 * attributes as well as its value.
	 * 
	 * sparsePropertyCount stores the number of properties in the object whose
	 * name is an array index as defined by ECMAScript (i.e. a name which is a
	 * simple integer in the range [0, 2^32-2]) but which is not stored in the
	 * entries array.  In other words, entries holds the contiguous properties
	 * starting at 0, and sparsePropertyCount counts the number of
	 * noncontiguous properties.
	 * 
	 * arrayLength stores the value defined by ECMAScript for the length
	 * property, i.e. 1 more than the largest array index defined in the
	 * object, or 0 if there are no array indexes.  (arrayLength can be
	 * larger than described here if it has been explicitly assigned to by
	 * script code.)
	 * 
	 * As properties are added or removed from the array object, we maintain
	 * the invariant that the entries array holds exactly the contiguous
	 * properties starting at 0.  When a new property is added at index
	 * entries.Count, the entries array is expanded to contain that property,
	 * and any further adjacent propeties.  When a property is removed at an
	 * index less than entries.Cength, the entries array is truncated to
	 * exclude that property and any subsequent properties.  This may require
	 * moving properties between the entries array and JObject.props.  The
	 * sparsePropertyCount field allows us to detect the common case
	 * where there are no noncontiguous array entries, and thus avoid
	 * consulting the JObject property table in many cases.
	 */
	private ArrayList entries             = new ArrayList();
	private UInt32    sparsePropertyCount = 0;
	private UInt32    arrayLength         = 0;
	
	public UInt32 ArrayLength { get { return arrayLength; }}
	} // JArrayObject


// This class is used to represent all compiled JavaScript functions.
public class JFunctionObject : JObject
	{
	// Delegate type used to represent the [[Call]] or [[Construct]]
	// action for a function.
	public delegate object JFunctionImp(object this_, params object[] args);
	
	
	// HACK snewman 8/28/01: define a prototype for function objects.
	// Be careful to avoid infinite recursion when creating the initial
	// object graph (constructing the Object prototype creates function
	// objects).
	private static JObject functionPrototype = null;
	
	
	// Construct a JFunctionObject with the given implementation and
	// scope chain.  callImp and/or constructImp can be null if the
	// corresponding operation is undefined.
	public JFunctionObject( JFunctionImp callImp, JFunctionImp constructImp,
							object[] scopes )
	  : base(functionPrototype, null, "Function")
		{
		this.scopes        = scopes;
		this.callImp       = callImp;
		this.constructImp  = constructImp;
		this.instanceProto = null;
		} // JFunctionObject constructor
	
	
	// Like the previous constructor, but allows the caller to explicitly
	// supply the prototype object for objects created using this function.
	public JFunctionObject( JFunctionImp callImp, JFunctionImp constructImp,
							object[] scopes, JObject instanceProto )
	  : this(callImp, constructImp, scopes)
		{
		this.instanceProto = instanceProto;
		} // JFunctionObject constructor
	
	
	// Construct a JFunctionObject with the given implementation and
	// no scope chain.  (This is typically used when defining built-in
	// functions that are implemented in C#.)
	public JFunctionObject(JFunctionImp callImp, JFunctionImp constructImp)
	  : this(callImp, constructImp, new object[0]) {}
	
	
	// If this object has a property of the given name, fill in prop
	// and return true.  Otherwise, return false (and prop is in an
	// undefined state).
	// 
	// We override this to define the "prototype" property on first
	// use.
	public override bool LookupProperty(string propName, out JProperty prop)
		{
		bool result = base.LookupProperty(propName, out prop);
		if (!result && propName == "prototype")
			{
			prop.attributes = JProperty.AttrFlags.none;
			prop.name       = propName;
			prop.value      = InstancePrototype;
			StoreProperty(propName, prop);
			return true;
			}
		
		return result;
		} // LookupProperty
	
	
	// Perform the [[Construct]] operation on this object.
	public override object Construct(JObject this_, params object[] args)
		{
		if (constructImp != null)
			{
			JObject newObj = new JObject(InstancePrototype, this, "Object");
			object temp = constructImp(newObj, args);
			if (JObject.Typeof(temp) == "object")
				return temp;
			else
				return newObj;
			
			}
		else
			throw new TypeError("Construct called for an object with no [[Construct]] operation");
		
		} // Construct
	
	
	// Perform the [[Call]] operation on this object.
	public override object Call(JObject this_, params object[] args)
		{
		if (callImp != null)
			return callImp(this_, args);
		else
			throw new TypeError("Call called for an object with no [[Call]] operation");
		
		} // Call
	
	
	// Perform the [[HasInstance]] operation on this object.
	public bool HasInstance(object value)
		{
		JObject jObject = value as JObject;
		if (jObject == null)
			return false;
		
		if (instanceProto == null)
			return false;
		
		JObject protoObj = InstancePrototype;
		
		while (true)
			{
			jObject = jObject.proto;
			if (jObject == null)
				return false;
			
			if (jObject == protoObj)
				return true;
			
			}
		
		} // HasInstance
	
	
	private object[] scopes; // Scope chain for this function
	private JFunctionImp callImp;      // Implementation of [[Call]] for this
									   // function, or null if none.
	private JFunctionImp constructImp; // Implementation of [[Construct] for
									   // this function, or null if none.
	
	internal JObject instanceProto;    // Prototype object for instances
									   // of this function.  null until
									   // first reference (so, if this
									   // function is never used as a
									   // constructor, and its "prototype"
									   // property is never explicitly
									   // referenced, instanceProto will
									   // never be allocated).
	
	// Return the prototype for instances of this function, allocating it
	// if necessary.
	private JObject InstancePrototype
		{
		get
			{
			if (instanceProto == null)
				{
				instanceProto = new JObject(JObject.ObjectPrototype, null, "Object");
				instanceProto.Put("constructor", this);
				}
			
			return instanceProto;
			}
		
		} // InstancePrototype property
	
	} // JFunctionObject


// This class provides type conversion operations.
public class JConvert
	{
	// HACK snewman 7/26/01: ensure that the type analysis in these methods is
	// complete, e.g. that "value is double" will fire for all numeric values.
	// Might need to add additional tests for 64-bit int, for example.
	
	// Convert the given value to a primitive type.
	public static object ToPrimitive(object value, JObject.ValueHintType hint)
		{
		// If the value is already a JavaScript primitive type, just return it.
		if (JObject.IsPrimitive(value))
			return value;
		
		// Otherwise, convert it to a primitive.
		JObject asObj = value as JObject;
		if (asObj != null)
			return asObj.DefaultValue(hint);
		else
			throw new TypeError("attempt to convert non-JavaScript object to primitive");
		
		} // ToPrimitive
	
	
	public static object ToPrimitive(object value)
		{ return ToPrimitive(value, JObject.ValueHintType.None); }
	
	
	// Convert the given value to a boolean value.
	public static bool ToBoolean(object value)
		{
		double d;
		
		if (value == null || value is JUndefinedObject)
			return false;
		else if (value is bool)
			return (bool)value;
		else if (PrimSupport.AsNumber(value, out d))
			return (d != 0 && !Double.IsNaN(d));
		else if (value is string)
			return ((string)value).Length > 0;
		else
			return true;
		
		} // ToBoolean
	
	
	// Convert the given value to a numeric value.
	public static double ToNumber(object value)
		{
		double d;
		if (PrimSupport.AsNumber(value, out d))
			return d;
		else if (value == null)
			return 0;
		else if (value is JUndefinedObject)
			return Double.NaN;
		else if (value is bool)
			return (bool)value ? 1 : 0;
		else if (value is string)
			{
			// HACK snewman 7/26/01: parse according to grammar in 9.3.1 on p. 43
			return System.Convert.ToDouble((string)value);
			}
		
		JObject asObj = value as JObject;
		if (asObj != null)
			return ToNumber(asObj.DefaultValue(JObject.ValueHintType.Number));
		else
			throw new TypeError("attempt to convert non-JavaScript object to number");
		
		} // ToNumber
	
	
	// Convert the given value to an integer value (or infinity).
	public static double ToInteger(object value)
		{
		double number = ToNumber(value);
		if (Double.IsNaN(number))
			return 0.0;
		else if (number == 0 || Double.IsInfinity(number))
			return number;
		else
			return (number < 0) ? Math.Ceiling(number) : Math.Floor(number);
		
		} // ToInteger
	
	
	// Convert the given value to an integer value in the signed 32-bit range.
	public static Int32 ToInt32(object value)
		{
		return (Int32)(ToUInt32(value));
		} // ToInt32
	
	
	// Convert the given value to an integer value in the unsigned 32-bit range.
	public static UInt32 ToUInt32(object value)
		{
		double number = ToNumber(value);
		if (number == 0 || Double.IsInfinity(number) || Double.IsNaN(number))
			return 0;
		else
			{
			number = (number < 0) ? Math.Ceiling(number) : Math.Floor(number);
			
			// HACK snewman 7/26/01: review/test this code
			double x = Math.IEEERemainder(number, (1<<31)*2.0); 
			if (x < 0)
				return (UInt32)(x + (1<<31) * 2.0);
			else
				return (UInt32)x;
			
			}
		
		} // ToUInt32
	
	
	// Convert the given value to an integer value in the unsigned 16-bit range.
	public static UInt16 ToUInt16(object value)
		{
		UInt32 x = ToUInt32(value);
		return (UInt16)(x & 0xFFFF);
		} // ToUInt16
	
	
	// Convert the given value to a string value.
	public static string ToString(object value)
		{
		double d;
		if (value is string)
			return (string)value;
		else if (PrimSupport.AsNumber(value, out d))
			{
			// HACK snewman 7/26/01: format according to grammar in 9.8.1 on p. 47
			return System.Convert.ToString(d);
			}
		else if (value == null)
			return "null";
		else if (value is JUndefinedObject)
			return "undefined";
		else if (value is bool)
			return (bool)value ? "true" : "false";
		
		JObject asObj = value as JObject;
		if (asObj != null)
			return ToString(asObj.DefaultValue(JObject.ValueHintType.String));
		else
			return value.ToString();
		
		} // ToString
	
	
	// Convert the given value to a string value.
	public static object ToObject(object value)
		{
		double d;
		if (value is string)
			return new JStringObject((string)value);
		else if (PrimSupport.AsNumber(value, out d))
			return new JNumberObject(d);
		else if (value is bool)
			return new JBooleanObject((bool)value);
		else if (value == null)
			throw new TypeError("attempt to convert null to an object");
		else if (value is JUndefinedObject)
			throw new TypeError("attempt to convert undefined to an object");
		else
			return value;
		
		} // ToObject
	
	} // JConvert


} // namespace JANET.Runtime