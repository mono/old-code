//
//  Authors
//    - Kangaroo, Geoff Norton
//    - Urs C. Muff, Quark Inc., <umuff@quark.com>
//
//  Copyright (c) 2004 Quark Inc.  All rights reserved.
//
// $Id$
//

using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace CocoaSharp {
	[XmlRoot("mappings")]
	public class Mappings {
		[XmlElement("property")]
		public PropertyMapping[] Properties;
		[XmlElement("method")]
		public MethodMapping[] Methods;
	}

	public class MappingInfo {
		[XmlAttribute("name")]
		public string Name;
		[XmlAttribute("returntype")]
		public string ReturnType;
		[XmlAttribute("noverbose")]
		public bool NoVerbose;
	}

	public class PropertyMapping : MappingInfo, IComparable {
		[XmlAttribute("get")]
		public string GetSelector;
		[XmlAttribute("set")]
		public string SetSelector;
		[XmlAttribute("getsignature")]
		public string GetSignature;
		[XmlAttribute("setsignature")]
		public string SetSignature;

		public int CompareTo(object obj) {
			if(obj is PropertyMapping) {
				PropertyMapping p = (PropertyMapping)obj;
				if (GetSelector != null && p.GetSelector != null) return GetSelector.CompareTo(p.GetSelector);
				if (SetSelector != null && p.SetSelector != null) return SetSelector.CompareTo(p.SetSelector);
				return GetSelector != null ? 1 : -1;
			}

			throw new ArgumentException("object is not a PropertyMapping");    
		}
	}

	public class MethodMapping : MappingInfo, IComparable {
		[XmlAttribute("selector")]
		public string Selector;
		[XmlAttribute("signature")]
		public string Signature;

		public int CompareTo(object obj) {
			if(obj is MethodMapping) {
				MethodMapping m = (MethodMapping)obj;
				return Selector.CompareTo(m.Selector);
			}

			throw new ArgumentException("object is not a MethodMapping");    
		}
	}
		
	[XmlRoot("conversions")]
	public class TypeConversions {
		[XmlArray("type")]
		public NativeData[] Conversions;
		[XmlArray("regex")]
		public NativeData[] Regexs;
		[XmlArray("replace")]
		public ReplaceData[] Replaces;
	}

	public class NativeData {
		[XmlAttribute("native")]
		public string Native;
		[XmlAttribute("api")]
		public string Api;
		[XmlAttribute("glue")]
		public string Glue;
		[XmlAttribute("gluearg")]
		public string GlueArg;
		[XmlAttribute("format")]
		public string Format;
	}

	public class ReplaceData {
		[XmlAttribute("type")]
		public string Type;
		[XmlAttribute("regex")] 
		public string Regex;
		[XmlAttribute("old")]
		public string ToReplace;
		[XmlAttribute("new")]
		public string ReplaceWith;
	}

	public class Method {
		public Method(string name,string selector,string types,TypeUsage returnType, ParameterInfo[] parameters) {
			this.name = name;
			this.selector = selector;
			System.Diagnostics.Debug.Assert(!this.Selector.StartsWith("-"));
			System.Diagnostics.Debug.Assert(this.Selector.Length > 0);
			this.types = types;

			this.returnType = returnType;
			this.parameters = parameters;
		}
		public Method(string className, bool isClassMethod, string name, string selector, TypeUsage returnType, ParameterInfo[] parameters, string declaration) 
			: this (name, selector, null, returnType, parameters) {
			this.declaration = declaration;
#if !WINDOWS
			this.types = ObjCClassInspector.GetSignature(className,FullSelector (isClassMethod));
#else
			this.types = returnType.TypeStr + "@0:4";
			foreach (ParameterInfo p in this.parameters)
				this.types += p.Type.TypeStr;
#endif
		}

		public static ICollection MergeMethods(ICollection headerMethods, ICollection machoMethods) {
			IDictionary header = new Hashtable();
			foreach (Method m in headerMethods)
				header[m.Selector] = m;
			foreach (Method m in machoMethods)
				if (header.Contains(m.Selector))
					((Method)header[m.Selector]).Merge(m);
				else
					header[m.Selector] = m;
			return header.Values;
		}
		public void Merge(Method machoMethod) {
			bool match = this.returnType.Merge(machoMethod.ReturnType);
			int ndx = 0;
			foreach (ParameterInfo p in this.parameters)
				if (!p.Merge(machoMethod.parameters[ndx++]))
					match = false;
#if DEBUG
			if (!match) {
				Console.WriteLine("DEBUG: objC	 method = " + this.AsDebugString());
				Console.WriteLine("DEBUG: machO method = " + machoMethod.AsDebugString());
			}
#endif
			this.types = machoMethod.types;
		}
		internal string AsDebugString() {
			return this.Name + ", selector: " + this.Selector + " types=" + this.Types + ", decl=" + this.declaration;
		}

		// -- Public Properties --
		public string Name { get { return name; } }
		public string Selector { get { return selector; } }
		public string FullSelector(bool isClassMethod) { return (isClassMethod ? "+" : "-") + Selector; } 
		public string Types { get { return types; } }
		public TypeUsage ReturnType { get { return returnType; } }
		public ParameterInfo[] Parameters { get { return parameters; } }
		public MappingInfo GetMapping(bool isClassMethod) { return (MappingInfo)NameMappings[FullSelector(isClassMethod)]; }
		public bool IsVerbose(bool isClassMethod) {
            MappingInfo info = GetMapping(isClassMethod);
            return info == null ? true : !info.NoVerbose;
		}
		public bool IsConstructor {
			get {
				return
					Name.StartsWith("init") 
					&& ReturnType.Type.OCType == OCType.id && Parameters.Length > 0;
			}
		}
		public string CSConstructorSignature {
			get {
				if (!IsConstructor)
					return null;

				ArrayList argTypes = new ArrayList();
				for(int i = 0; i < Parameters.Length; ++i) 
					argTypes.Add(StripComments(Parameters[i].Type.ApiType));

				return string.Join(",",(string[])argTypes.ToArray(typeof(string)));
			}
		}

		// -- Members --
		private string name, selector, types, declaration;
		private TypeUsage returnType;
		private ParameterInfo[] parameters;
		private bool mCSAPIDone;

		private static TypeConversions sConversions;
		private static Mappings sNameMappings;
		private static IDictionary Conversions;
		private static IDictionary NameMappings;

		#region -- Static Constructor --
		static Method() {
			XmlSerializer _ser = new XmlSerializer(typeof(TypeConversions));
			XmlTextReader _xtr = new XmlTextReader(Path.Combine(Configuration.XmlPath,"typeconversion.xml"));
			sConversions = (TypeConversions)_ser.Deserialize(_xtr);
			_xtr.Close();
			Conversions = new Hashtable();
			foreach (NativeData nd in sConversions.Conversions)
				Conversions[nd.Native] = nd;

			if (File.Exists(Path.Combine(Configuration.XmlPath,"mapping.xml"))) {
				_ser = new XmlSerializer(typeof(Mappings));
				_xtr = new XmlTextReader(Path.Combine(Configuration.XmlPath,"mapping.xml"));
				sNameMappings = (Mappings)_ser.Deserialize(_xtr);
				_xtr.Close();
			}
			else
				sNameMappings = new Mappings();
			
			NameMappings = new Hashtable();
			if(sNameMappings.Properties != null)
				foreach (PropertyMapping map in sNameMappings.Properties) {
					if(map.GetSelector != null)
						NameMappings[map.GetSelector] = map;
					if(map.SetSelector != null)
						NameMappings[map.SetSelector] = map;
				}
			if(sNameMappings.Methods != null)
				foreach (MethodMapping map in sNameMappings.Methods) {
					System.Diagnostics.Debug.Assert(map.Selector.Length > 1);
					NameMappings[map.Selector] = map;
				}
		} 
		#endregion

		// -- Methods --
		public void SetCSAPIDone() {
			mCSAPIDone = true;
		}
		public void ClearCSAPIDone() {
			mCSAPIDone = false;
		}

		#region -- C# Public API --
		public bool IsGetMethod(string type) {
			if (OCType.@void == ReturnType.Type.OCType)
				return false;
			if (Parameters.Length > 0)
				return false;
			mCSAPIDone = true;
			return true;
		}
		
		public Method GetGetMethod(bool isClassMethod,IDictionary methods, out string propName) {
			propName = Name.Substring(3);
			string sel = Selector;
    		sel = sel.Substring(3,sel.Length-4);

			Method get = (Method)methods[sel.Substring(0,1).ToLower() + sel.Substring(1)];
			
			if (get == null)
				get = (Method)methods["is" + propName];
			if (get == null)
				get = (Method)methods["get" + propName];
			if (get == null)
				get = (Method)methods[propName];
			
			propName = MakeCSMethodName(isClassMethod,propName);
			return get;
		}
		
		public Method GetSetMethod(bool isClassMethod, IDictionary methods, out string propName) {
			propName = MakeCSMethodName(isClassMethod, Name);
			string sel = Selector;
			sel = sel.Substring(0,1).ToUpper() + sel.Substring(1,sel.Length-1);

			Method set = (Method)methods["set" + sel + ":"];
			
			return set;
		}

		private void GenerateProperty(bool isClassMethod,string className,System.IO.TextWriter w, Method get, Method set, string propName, bool isProtocol) {
			bool hasGet = get != null;
			bool hasSet = set != null;
			string t = hasGet ? get.ReturnType.ApiType : set.Parameters[0].Type.ApiType;

            if(hasSet)
				w.WriteLine("        // setSelector: {0}", set.Selector);
			if (hasGet)
				w.WriteLine("        // getSelector: {0}", get.Selector);

            w.Write("        {0}{1}{2} {3} {{",
				isProtocol ? string.Empty : "public ",
				isClassMethod ? "static " : string.Empty, t, propName);
			
			if (!isProtocol)
				w.WriteLine();
            
            if (hasGet) {
				if (isProtocol)
					w.Write(" get;");
				else {
					w.WriteLine("            get {{ {0}; }}", ReturnExpression(
						get.ReturnType, 
						string.Format("ObjCMessaging.objc_msgSend({0},{2},typeof({1}))", 
				            isClassMethod ? className + "_classPtr" : "Raw",
				            get.ReturnType.GlueType, 
				            "\"" + get.Selector + "\"")));
				}
				get.SetCSAPIDone();
			}

			if (hasSet) {
				if (isProtocol)
					w.Write(" set;");
				else {
					w.WriteLine("            set {{ ObjCMessaging.objc_msgSend({0},{1},typeof(void),typeof({2}),{3}); }}", 
						isClassMethod ? className + "_classPtr" : "Raw",
						"\"" + set.Selector + "\"",
						set.Parameters[0].Type.GlueType,
						ArgumentExpression(set.Parameters[0].Type,"value"));
				}
				set.SetCSAPIDone();
			}
			if (isProtocol)
				w.WriteLine(" }");
			else
				w.WriteLine("        }");
			// Check to see if this selector is in our map
			if (hasGet && !NameMappings.Contains(get.FullSelector(isClassMethod)))
				NameMappings[get.FullSelector(isClassMethod)] = GeneratePropertyMapping(isClassMethod, className, propName, get, set);
			if (hasSet && !NameMappings.Contains(set.FullSelector(isClassMethod)))
				NameMappings[set.FullSelector(isClassMethod)] = GeneratePropertyMapping(isClassMethod, className, propName, get, set);
		}

		private void GenerateProperty(bool isClassMethod,string className,System.IO.TextWriter w, PropertyMapping propMap,IDictionary methods, bool isProtocol) {
			Method getMethod = propMap.GetSelector != null ? (Method)methods[propMap.GetSelector.Substring(1)] : null;
			Method setMethod = propMap.SetSelector != null ? (Method)methods[propMap.SetSelector.Substring(1)] : null;

			GenerateProperty(isClassMethod, className, w, getMethod, setMethod, propMap.Name, isProtocol);
		}

		private string ParametersString() {
			string paramsStr = string.Empty;
			foreach (ParameterInfo p in Parameters) {
				if (paramsStr != string.Empty)
					paramsStr += ",";
				paramsStr += p.Type.ApiType + " " + p.Name;
			}
			return paramsStr;
		}

		private string GlueArgumentsString(bool isClassMethod, string className) {
			string glueArgsStr = isClassMethod ? className + "_classPtr" : "Raw";
			glueArgsStr += ",\"" + Selector + "\"";
			glueArgsStr += ",typeof(" + ReturnType.GlueType + ")";
			foreach (ParameterInfo p in Parameters) {
				string pType = "typeof(" + p.Type.GlueType + ")";
				if (p.Type.GlueType == "System.ValueType")
					pType = ArgumentExpression(p.Type,p.Name) + ".GetType()";
				glueArgsStr += ",";
				glueArgsStr += pType;
				glueArgsStr += ",";
				glueArgsStr += ArgumentExpression(p.Type,p.Name);
			}
			return glueArgsStr;
		}

		public void GenerateMethod(bool isClassMethod,string className,System.IO.TextWriter w,string methodName, bool isProtocol) {
			w.WriteLine("        // {0}", Selector);
			w.WriteLine("        {0}{1}{2} {3} ({4}) {5}", 
				isProtocol ? string.Empty : "public ",
				isClassMethod ? "static " : string.Empty, 
				ReturnType.ApiType, MakeCSMethodName(isClassMethod,methodName), ParametersString(),
				isProtocol ? ";" : "{");
			if (!isProtocol) {
				w.WriteLine("            {0};",ReturnExpression(ReturnType,
					string.Format("{0}.{1}({2})", "ObjCMessaging", "objc_msgSend", GlueArgumentsString(isClassMethod, className))));
				w.WriteLine("        }");
			}
			
			// Check to see if this selector is in our map
			if(!NameMappings.Contains(FullSelector(isClassMethod)))
				NameMappings[FullSelector(isClassMethod)] = GenerateMethodMapping(isClassMethod,className);
		}

		public void CSAPIMethod(bool isClassMethod,string className,IDictionary methods,bool propOnly,System.IO.TextWriter w, Overrides _o) {
			if (mCSAPIDone)
				return;

			// Check to see if we're overridden
			if(_o != null && _o.Methods != null)
				foreach(MethodOverride _mo in _o.Methods) 
					if(_mo.Selector == FullSelector(isClassMethod)) {
						w.WriteLine("        //{0} is overridden", Selector);
						w.WriteLine(_mo.Method);
						mCSAPIDone = true;
						// Check to see if this selector is in our map
						if(!NameMappings.Contains(FullSelector(isClassMethod)))
							NameMappings[FullSelector(isClassMethod)] = GenerateMethodMapping(isClassMethod,className);
						return;
					}
			GenerateCSMethod(isClassMethod,className,methods,propOnly,w,false);
		}
		
		private static PropertyMapping GeneratePropertyMapping(bool isClassMethod,string name,string propName, Method get, Method set) {
			PropertyMapping pm = new PropertyMapping();
			pm.Name = propName;
			if(get != null) {
				pm.GetSelector = get.FullSelector(isClassMethod);
				pm.GetSignature = get.Types;
			}
			if(set != null) {
				pm.SetSelector = set.FullSelector(isClassMethod);
				pm.SetSignature = set.Types;
			}
			return pm;
		}

		private MethodMapping GenerateMethodMapping(bool isClassMethod,string className) {
			MethodMapping mm = new MethodMapping();
			mm.Name = Name;
			mm.Selector = FullSelector(isClassMethod);
			mm.Signature = Types;
			return mm;
		}

		public static void SaveMapping() {
			IDictionary pMaps = new Hashtable();
			ArrayList mMaps = new ArrayList();
			foreach(object val in NameMappings.Values) {
				if(val is PropertyMapping) {
					PropertyMapping p = (PropertyMapping)val;
					if (pMaps.Contains(p.Name)) {
						PropertyMapping o = (PropertyMapping)pMaps[p.Name];
						if (o.GetSelector == null)
							o.GetSelector = p.GetSelector;
						else if (p.GetSelector != null && o.GetSelector != p.GetSelector)
							Console.WriteLine("Warning: conflicting get selectors " + o.GetSelector + " != " + p.GetSelector);
						if (o.SetSelector == null)
							o.SetSelector = p.SetSelector;
						else if (p.SetSelector != null && o.SetSelector != p.SetSelector)
							Console.WriteLine("Warning: conflicting set selectors " + o.SetSelector + " != " + p.SetSelector);
					}
					else
						pMaps[p.Name] = val;
				}
				if(val is MethodMapping)
					mMaps.Add(val);
			}

			Mappings toOutput = new Mappings();
			mMaps.Sort();
			toOutput.Methods = (MethodMapping[])mMaps.ToArray(typeof(MethodMapping));
			mMaps = new ArrayList(pMaps.Values);
			mMaps.Sort();
			toOutput.Properties = (PropertyMapping[])mMaps.ToArray(typeof(PropertyMapping));

			if (!File.Exists (Path.Combine (Configuration.XmlPath, "mapping.xml")) || (File.GetAttributes(Path.Combine(Configuration.XmlPath,"mapping.xml")) & FileAttributes.ReadOnly) == 0) {
				XmlSerializer _ser = new XmlSerializer(typeof(Mappings));
				StreamWriter _sw = new StreamWriter(Path.Combine(Configuration.XmlPath,"mapping.xml"));
				_ser.Serialize(_sw, toOutput);
				_sw.Close();
			}
		}

		private static string ReturnExpression(TypeUsage type,string expression) {
			if(type.Type.OCType == OCType.SEL)
				return string.Format("return NSString.FromSEL((IntPtr){0}).ToString()", expression);
			if(type.Type.GlueType == "System.String" && type.Type.OCType == OCType.char_ptr)
				return string.Format("return Marshal.PtrToStringAnsi((IntPtr){0})", expression);
			if(type.Type.NeedConversion)
				return string.Format("return ({0})NSObject.NS2Net((IntPtr){1})", type.ApiType, expression);
			if (type.Type.OCType == OCType.@void)
				return expression;
			return "return (" + type.Type.ApiType + ")" + expression;
		}

		private static string ArgumentExpression(TypeUsage type,string expression) {
			if(type.Type.OCType == OCType.SEL)
				return string.Format("NSString.NSSelector({0})", expression);
			if(type.Type.NeedConversion)
				return string.Format("NSObject.Net2NS({0})", expression);
			return expression;
		}

		public void CSConstructor(string className,TextWriter w) {
			if (!IsConstructor)
				return;

			//BuildArgs(className);
			ArrayList args = new ArrayList();
			for(int i = 0; i < Parameters.Length; ++i) 
				args.Add(Parameters[i].Name);

			w.WriteLine("        public {0}({1}) : this() {{", className, ParametersString());
			w.WriteLine("            {0}({1});", Name, string.Join(", ", (string[])args.ToArray(typeof(string))));
			w.WriteLine("        }");
		}
		#endregion

		private void GenerateCSMethod(bool isClassMethod,string className,IDictionary methods,bool propOnly,System.IO.TextWriter w, bool isProtocol) {
			//BuildArgs(className);
			bool isVoid = ReturnType.Type.OCType == OCType.@void;
			
			if(NameMappings.Contains(FullSelector(isClassMethod))) {
				object _mapping = NameMappings[FullSelector(isClassMethod)];
				if (_mapping is PropertyMapping) {
					PropertyMapping _p = (PropertyMapping)_mapping;
					if (isVoid && _p.GetSelector == FullSelector(isClassMethod)) {
						if (!propOnly)
							GenerateMethod(isClassMethod, className,w,_p.Name + "_",isProtocol);
						return;
					}
					GenerateProperty(isClassMethod, className, w, _p, methods, isProtocol);
					return;
				}

				if (propOnly)
					return;

				MethodMapping mm = (MethodMapping)_mapping;
				GenerateMethod(isClassMethod, className, w, mm.Name, isProtocol);
				return;
			}

			if (isVoid && Parameters.Length == 1 && Name.StartsWith("set")) {
//				Console.WriteLine("INFO: New name mapping for selector: " + Selector);
				string propName;
				Method get = GetGetMethod(isClassMethod, methods, out propName);
				GenerateProperty(isClassMethod, className, w, get, this, propName, isProtocol);
				return;
			}
			
			if (propOnly)
				return;

//			Console.WriteLine("INFO: New name mapping for selector: " + Selector);
			if (!isVoid && Parameters.Length == 0) {
				string _propName;
				Method set = GetSetMethod(isClassMethod, methods, out _propName);
				GenerateProperty(isClassMethod, className, w, this, set, _propName, isProtocol); 
				return;
			}

			GenerateMethod(isClassMethod, className, w, Name, isProtocol);
		}

		#region -- C# Interface --
		public void CSInterfaceMethod(bool isClassMethod,string className,IDictionary methods,bool propOnly,System.IO.TextWriter w) {
			if (isClassMethod || mCSAPIDone)
				return;

			GenerateCSMethod(isClassMethod,className,methods,propOnly,w,true);
		}
		#endregion

		#region -- Private Functions --
		static public string MakeCSMethodName(bool isClassMethod,string name) {
			if (isClassMethod)
				name = name.Substring(0,1).ToUpper() + name.Substring(1);
			else {
				int pos = 1;
				name = name.Substring(0,1).ToLower() + name.Substring(1);
				while (pos < name.Length-1 && name[pos] == char.ToUpper(name[pos])) {
					name = name.Substring(0,pos+1).ToLower() + name.Substring(pos+1);
					++pos;
				}
				if (pos > 1 && pos < name.Length-1)
					name = name.Substring(0,pos-1) + name.Substring(pos-1,1).ToUpper() + name.Substring(pos);
			}

			return TranslateKeywords (name);
		}

		public static string TranslateKeywords (string name) {
			switch (name) {
				case "new": case "override": case "virtual": case "typeof":
				case "is": case "as": case "delegate": case "this":
				case "base": case "lock": case "object": case "string":
				case "int": case "short": case "long": case "bool":
				case "void": case "char": case "static": case "class":
				case "interface": case "struct": case "enum": case "null":
				case "private": case "public": case "protected":
				case "internal": case "if": case "else": case "switch":
				case "for": case "foreach": case "while": case "do":
				case "case": case "return": case "default":
				case "continue": case "break": case "event": case "checked":
				case "unsafe":
					return name + "_";
			}
			return name;
		}

		public static string StripComments(string str) {
			int ndx = str.IndexOf("/*");
			while (ndx >= 0) {
				int ndx2 = str.IndexOf("*/",ndx,str.Length-ndx);
				if (ndx2 >= ndx) {
					str = str.Remove(ndx,ndx2-ndx+2);
					ndx = str.IndexOf("/*");
				}
				else
					ndx = -1;
			}
			return str.Trim();
		}

		public static string ConvertTypeGlue(string type,bool arg) {
			type = type.Replace("const ",string.Empty);
			{
				NativeData nd = (NativeData)Conversions[type];
				if(nd != null && nd.Glue != null)
					return arg ? (nd.GlueArg != null ? nd.GlueArg : nd.Glue) : nd.Glue;
			}

			if (sConversions.Regexs != null)
				foreach (NativeData nd in sConversions.Regexs)
					if(new Regex(nd.Native).IsMatch(type) && nd.Glue != null)
						return arg ? (nd.GlueArg != null ? nd.GlueArg : nd.Glue) : nd.Glue;

			if (sConversions.Replaces != null)
				foreach (ReplaceData rd in sConversions.Replaces)
					if(rd.Type == "glue")
						if(new Regex(rd.Regex).IsMatch(type))
							return type.Replace(rd.ToReplace, rd.ReplaceWith).Trim();
			
			return type;
		}

		public static string ConvertType(string type,bool arg) {
			type = type.Replace("const ",string.Empty);
			{
				NativeData nd = (NativeData)Conversions[type];
				if(nd != null && nd.Api != null)
					return nd.Api;
			}

			if (sConversions.Regexs != null)
				foreach (NativeData nd in sConversions.Regexs)
					if(new Regex(nd.Native).IsMatch(type) && nd.Api != null) {
						if (nd.Api == "{detect}") {
							string cls = type.Substring(0,type.Length-1).Replace(" ",string.Empty);
							if (cls.StartsWith("NS") && !cls.EndsWith("*"))
								return cls;
							else if (ObjCClassInspector.IsObjCClass(cls))
								return cls;
							return "IntPtr /*(" + type + ")*/";
						}
						return nd.Api;
					}

			if (sConversions.Replaces != null)
				foreach (ReplaceData rd in sConversions.Replaces)
					if(rd.Type == "api")
						if(new Regex(rd.Regex).IsMatch(type))
							return type.Replace(rd.ToReplace, rd.ReplaceWith).Trim();

			return type;
		}
		#endregion
	}

	public class ParameterInfo {
		public ParameterInfo(string name, TypeUsage type) { this.name = name; this.type = type; }

		public bool Merge(ParameterInfo macho) {
			return this.type.Merge(macho.Type);
		}

		// -- Public Properties --
		public TypeUsage Type { get { return type; } }
		public string Name { get { return Method.TranslateKeywords (name); } }

		// -- Members --
		private string name;
		private TypeUsage type;
	}
}

//
// $Log: Method.cs,v $
// Revision 1.7  2004/09/21 04:28:54  urs
// Shut up generator
// Add namespace to generator.xml
// Search for framework
// Fix path issues
// Fix static methods
//
// Revision 1.6  2004/09/20 22:31:18  gnorton
// Generator v3 now generators Foundation in a compilable glueless state.
//
// Revision 1.5  2004/09/20 20:18:23  gnorton
// More refactoring; Foundation almost gens properly now.
//
// Revision 1.4  2004/09/20 16:42:52  gnorton
// More generator refactoring.  Start using the MachOGen for our classes.
//
// Revision 1.3  2004/09/11 00:41:22  urs
// Move Output to gen-out
//
// Revision 1.2  2004/09/09 03:32:22  urs
// Convert methods from mach-o to out format
//
// Revision 1.1  2004/09/09 01:16:03  urs
// 1st draft of out module of 2nd generation generator
//
//
