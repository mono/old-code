// DocVersionatorPlus.cs
//
// Utility to add <since> elements to a documentation catalog
// for API elements added between two versions of an assembly.
//
// Author:  Mike Kestner  <mkestner@novell.com>
//
// Derived heavily from the monodocer source code by Joshua Tauberer.

using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Reflection;
using System.Xml;

using Mono.GetOptions;

[assembly: AssemblyTitle("DocVersionatorPlus - The Mono Documentation Versioning Tool")]
[assembly: AssemblyCopyright("Copyright (c) 2004 Joshua Tauberer <tauberer@for.net>\nCopyright (c) 2005 Novell, Inc.\nreleased under the GNU GPL.")]
[assembly: AssemblyDescription("A tool for versioning API elements within Mono XML documentation files.")]

namespace DocVersionatorPlus {

public class Application {
	
	class Opts : Options {
		[Option("The root directory of an assembly's documentation files.")]
		public string path = null;

		[Option("The base assembly to version against.  API elements exposed by this assembly will not be annotated in the docs.  If this option is not provided all elements in the assembly argument are annotated.  Specify a path to an assembly.")]
		public string base_assembly = null;

		[Option("The assembly to version.  Specify a path to an assembly.")]
		public string assembly = null;

		[Option("The version name to add to the documentation.")]
		public string version = null;
	}

	static Assembly LoadAssembly (string path)
	{
		Assembly assembly = null;
		try {
			assembly = Assembly.LoadFile (path);
		} catch (System.IO.FileNotFoundException e) { 
			Console.WriteLine ("Unable to load assembly path " + path);
		}

		return assembly;
	}

	public static int Main (string[] args) 
	{
		Opts opts = new Opts();

		if (args.Length == 0) {
			opts.DoHelp ();
			return 1;
		}
		
		opts.ProcessArgs (args);
		Assembly assm = opts.assembly == null ? null : LoadAssembly (opts.assembly);

		if (opts.path == null || !Directory.Exists (opts.path) || assm == null) {
			Console.WriteLine ("Valid path and assembly options are required.");
			opts.DoHelp ();
			return 1;
		}

		string version = opts.version == null ? assm.GetName ().Version.ToString () : opts.version;
		AssemblyInfo base_info = opts.base_assembly == null ? null : new AssemblyInfo (LoadAssembly (opts.base_assembly));
		AssemblyInfo info = new AssemblyInfo (assm);

		Versionator v = new Versionator (base_info, info, opts.path, version);
		v.Versionate ();

		return 0;
	}
}

public class Versionator {

	AssemblyInfo base_info;
	AssemblyInfo info;
	string path;
	string version;

	public Versionator (AssemblyInfo base_info, AssemblyInfo info, string path, string version)
	{
		this.base_info = base_info;
		this.info = info;
		this.path = path;
		this.version = version;
	}

	public void Versionate ()
	{
		foreach (TypeInfo ti in info) {
			if (base_info.HasType (ti.Fullname)) {
				TypeInfo base_type = base_info [ti.Fullname];
				ArrayList new_members = new ArrayList ();
				foreach (string sig in ti.MemberSignatures)
					if (!base_type.HasMember (sig))
						new_members.Add (sig);
				if (new_members.Count > 0)
					Annotate (ti, (string[]) new_members.ToArray (typeof (string)));
			} else
				Annotate (ti);
		}
	}

	void Annotate (TypeInfo ti)
	{
		string typefile = Path.Combine (path, ti.FilePath);
		XmlDocument doc = LoadTypeDoc (typefile);
		if (doc == null)
			return;

		XmlElement docs_elem = doc.DocumentElement ["Docs"] as XmlElement;
		if (docs_elem == null) {
			Console.WriteLine ("Missing Docs element in " + ti.Fullname);
			return;
		}

		UpdateVersion (docs_elem);
		doc.Save (typefile);
	}

	void Annotate (TypeInfo ti, string[] member_sigs)
	{
		string typefile = Path.Combine (path, ti.FilePath);
		XmlDocument doc = LoadTypeDoc (typefile);
		if (doc == null)
			return;

		foreach (string sig in member_sigs) {
			XmlNode sig_node = doc.DocumentElement.SelectSingleNode ("Members/Member/MemberSignature[@Value='" + sig + "']");
			if (sig_node == null) {
				Console.WriteLine ("No signature for type " + ti.Fullname + " matches " + sig);
				continue;
			}

			XmlElement docs_elem = (sig_node.ParentNode as XmlElement) ["Docs"] as XmlElement;
			if (docs_elem == null) {
				Console.WriteLine ("No Docs node found for type signature " + sig + " on type " + ti.Fullname);
				continue;
			}

			UpdateVersion (docs_elem);
		}

		doc.Save (typefile);
	}

	XmlDocument LoadTypeDoc (string path)
	{
		XmlDocument result = new XmlDocument();
		try {
			using (Stream s = File.Open (path, FileMode.Open))
				result.Load (s);
		} catch (Exception e) {
			Console.WriteLine ("Error loading " + path + ": " + e.Message, e);
			return null;
		}

		return result;
	}

	void UpdateVersion (XmlElement docs_elem)
	{
		XmlElement version_elem = docs_elem ["since"];
		if (version_elem == null) {
			version_elem = docs_elem.OwnerDocument.CreateElement ("since");
			docs_elem.AppendChild (version_elem);
		}
		version_elem.SetAttribute ("version", version);
	}
}


public class AssemblyInfo : IEnumerable {
	
	Hashtable types = new Hashtable ();

	bool TypeIsVisible (TypeAttributes ta) {
		switch (ta & TypeAttributes.VisibilityMask) {
		case TypeAttributes.Public:
		case TypeAttributes.NestedPublic:
		case TypeAttributes.NestedFamily:
		case TypeAttributes.NestedFamORAssem:
				return true;

		default:
				return false;
		}
	}

	public AssemblyInfo (Assembly assembly) 
	{
		if (assembly == null)
			return;

		foreach (Type type in assembly.GetTypes()) {
			if (type.Namespace == null || !TypeIsVisible (type.Attributes))
				continue;

			TypeInfo ti = new TypeInfo (type);
			types [ti.Fullname] = ti;
		}
	}

	public TypeInfo this [string full_name] {
		get {
			return types [full_name] as TypeInfo;
		}
	}

	public IEnumerator GetEnumerator ()
	{
		return types.Values.GetEnumerator ();
	}

	public bool HasType (string full_name)
	{
		return types.Contains (full_name);
	}
}

public class TypeInfo {

	private static string GetTypeFileName(Type type) {
		int start = 0;
		if (type.Namespace != null && type.Namespace != "")
			start = type.Namespace.Length + 1;
		return type.FullName.Substring(start);
	}

	string file_path;
	string full_name;
	Hashtable members = new Hashtable ();

	public TypeInfo (Type type)
	{
		file_path = type.Namespace + Path.DirectorySeparatorChar + GetTypeFileName (type) + ".xml";
		full_name = type.FullName;
		AddMembers (type);
	}

	public string FilePath {
		get {
			return file_path;
		}
	}

	public string Fullname {
		get {
			return full_name;
		}
	}

	public bool HasMember (string signature)
	{
		return members.Contains (signature);
	}

	public string[] MemberSignatures {
		get {
			string[] result = new string [members.Keys.Count];
			int i = 0;
			foreach (string key in members.Keys)
				result [i++] = key;
			return result;
		}
	}

	void AddMembers (Type type) 
	{
		if (IsDelegate (type))
			return;

		foreach (MemberInfo m in type.GetMembers(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly)) {
			if (m is Type || !IsNew (m)) continue;
				
			string sig = MakeMemberSignature (m);
			if (sig != null) 
				members [sig] = sig;
		}
	}
	
	bool IsNew (MemberInfo m) 
	{
		if (m is MethodInfo && !IsNew((MethodInfo)m)) return false;
		if (m is PropertyInfo && !IsNew(((PropertyInfo)m).GetGetMethod())) return false;
		if (m is PropertyInfo && !IsNew(((PropertyInfo)m).GetSetMethod())) return false;
		if (m is EventInfo && !IsNew(((EventInfo)m).GetAddMethod())) return false;
		if (m is EventInfo && !IsNew(((EventInfo)m).GetRaiseMethod())) return false;
		if (m is EventInfo && !IsNew(((EventInfo)m).GetRemoveMethod())) return false;
		return true;
	}
	
	bool IsNew (MethodInfo m) 
	{
		if (m == null) return true;
		MethodInfo b = m.GetBaseDefinition();
		if (b == null || b == m) return true;
		return false;
	}
	
	private static bool GetFieldConstValue(FieldInfo field, out string value) {
		value = null;
		if (field.DeclaringType.IsEnum) return false;
		if (field.IsLiteral || (field.IsStatic && field.IsInitOnly)) {
			object val = field.GetValue(null);
			if (val == null) value = "null";
			else if (val is Enum) value = val.ToString();
			else if (val is IFormattable) {
				value = ((IFormattable)val).ToString();
				if (val is string)
					value = "\"" + value + "\"";
			}
			if (value != null && value != "")
				return true;
		}
		return false;
	}
	
	bool IsDelegate(Type type) 
	{
		return typeof(System.Delegate).IsAssignableFrom (type) && !type.IsAbstract;
	}
	
	static string GetFieldVisibility (FieldInfo field) {
		if (field.IsPublic) return "public";
		if (field.IsFamily) return "protected";
		return null;
	}

	static string MakeFieldSignature (FieldInfo field) {
		if (field.DeclaringType.IsEnum && field.Name == "value__")
			return null; // This member of enums aren't documented.
		
		string visibility = GetFieldVisibility (field);
		if (visibility == null) return null;
		if (field.DeclaringType.IsEnum) return field.Name;
		
		string type = ConvertCTSName (field.FieldType.FullName);
		
		string modifiers = String.Empty;
		if (field.IsStatic && !field.IsLiteral) modifiers += " static";
		if (field.IsInitOnly) modifiers += " readonly";
		if (field.IsLiteral) modifiers += " const";
		
		string fieldValue;
		if (GetFieldConstValue(field, out fieldValue))
			fieldValue = " = " + fieldValue;
		else
			fieldValue = "";

		return String.Format ("{0}{1} {2} {3}{4};",
						visibility, modifiers, type, field.Name, fieldValue);
	}

	static string GetMethodVisibility (MethodBase method) {
		if (method.IsPublic) return "public";
		if (method.IsFamily) return "protected";
		return null;
	}

	static string GetMethodParameters (ParameterInfo[] pi) {
		if (pi.Length == 0) return "";
		
		StringBuilder sb = new StringBuilder ();

		int i = 0;
		foreach (ParameterInfo parameter in pi) {
			if (i != 0) sb.Append (", ");
			if (parameter.ParameterType.IsByRef) {
				if (parameter.IsOut) sb.Append("out ");
				else sb.Append("ref ");
			}
			string param = parameter.ParameterType.FullName;
			if (parameter.ParameterType.IsByRef)
				param = param.Substring (0, param.Length - 1);
			param = ConvertCTSName(param);			
			sb.Append (param);
			sb.Append (" ");
			sb.Append (parameter.Name);
			i++;
		}

		return sb.ToString();
	}

	static string MakeMethodSignature (MethodInfo method) {
		string visibility = GetMethodVisibility (method);
		if (visibility == null)
			return null;
		
		if (method.IsSpecialName && (method.Name.StartsWith ("get_") || method.Name.StartsWith ("set_") || method.Name.StartsWith ("add_") || method.Name.StartsWith ("remove_")))
			return null;

		string modifiers = String.Empty;
		if (method.IsStatic) modifiers += " static";
		if (method.IsVirtual && !method.IsAbstract) {
			if ((method.Attributes & MethodAttributes.NewSlot) != 0) modifiers += " virtual";
			else modifiers += " override";
		}
		if (method.IsAbstract && !method.DeclaringType.IsInterface) modifiers += " abstract";
		if (method.IsFinal) modifiers += " sealed";
		if (modifiers == " virtual sealed") modifiers = "";

		// Special signature for destructors.
		if (method.Name == "Finalize" && method.GetParameters().Length == 0)
			return "~" + method.DeclaringType.Name + " ();";	

		string return_type = ConvertCTSName (method.ReturnType.FullName);
		string parameters = GetMethodParameters (method.GetParameters());

		string method_name = method.Name;
		
		// operators, default accessors need name rewriting

		return String.Format ("{0}{1} {2} {3} ({4});",
						visibility, modifiers, return_type, method_name, parameters);
	}

	static string MakeConstructorSignature (ConstructorInfo constructor) {
		string visibility = GetMethodVisibility (constructor);
		if (visibility == null)
			return null;

		string name = constructor.DeclaringType.Name;
		string parameters = GetMethodParameters (constructor.GetParameters());

		return String.Format ("{0} {1} ({2});",
						visibility, name, parameters);
	}


	static string MakePropertySignature (PropertyInfo property) {
		// Check accessibility of get and set, since they can be different these days.
		string get_visible = null, set_visible = null;
		MethodBase get_method = property.GetGetMethod (true);
		MethodBase set_method = property.GetSetMethod (true);
		if (get_method != null) get_visible = GetMethodVisibility(get_method);
		if (set_method != null) set_visible = GetMethodVisibility(set_method);
		if (get_visible == null && set_visible == null) return null; // neither are visible
		
		// Pick an accessor to use for static/virtual/override/etc. checks.
		MethodBase method = property.GetSetMethod (true);
		if (method == null)
			method = property.GetGetMethod (true);
	
		string modifiers = String.Empty;
		if (method.IsStatic) modifiers += " static";
		if (method.IsVirtual && !method.IsAbstract) {
				if ((method.Attributes & MethodAttributes.NewSlot) != 0) modifiers += " virtual";
				else modifiers += " override";
		}
		if (method.IsAbstract && !method.DeclaringType.IsInterface) modifiers += " abstract";
		if (method.IsFinal) modifiers += " sealed";
		if (modifiers == " virtual sealed") modifiers = "";
	
		string name = property.Name;
	
		string type_name = property.PropertyType.FullName;
		type_name = ConvertCTSName (type_name);
		
		string parameters = GetMethodParameters (property.GetIndexParameters());
		if (parameters != "") parameters = "[" + parameters + "]";		
		
		string visibility;
		if (get_visible != null && (set_visible == null || (set_visible != null && get_visible == set_visible)))
			visibility = get_visible;
		else if (set_visible != null && get_visible == null)
			visibility = set_visible;
		else
			visibility = "public"; // if they are different, but both externally accessible, the greater access must be public, I think
		
		string accessors = "{";
		if (set_visible != null) {
			if (set_visible != visibility)
				accessors += " " + set_visible;
			accessors += " set;";
		}
		if (get_visible != null) {
			if (get_visible != visibility)
				accessors += " " + get_visible;
			accessors += " get;";
		}
		accessors += " }";
	
		return String.Format ("{0}{1} {2} {3}{4} {5};",
						visibility, modifiers, type_name, name, parameters, accessors);
	}
		
	static string MakeEventSignature (EventInfo ev) {
		MethodInfo add = ev.GetAddMethod ();

		string visibility = GetMethodVisibility(add);
		if (visibility == null)
			return null;

		string modifiers = String.Empty;
		if (add.IsStatic) modifiers += " static";
		if (add.IsVirtual && !add.IsAbstract) {
			if ((add.Attributes & MethodAttributes.NewSlot) != 0) modifiers += " virtual";
			else modifiers += " override";
		}
		if (add.IsAbstract && !ev.DeclaringType.IsInterface) modifiers += " abstract";
		if (add.IsFinal) modifiers += " sealed";
		if (modifiers == " virtual sealed") modifiers = "";
		
		string name = ev.Name;
		string type = ConvertCTSName(ev.EventHandlerType.FullName);

		return String.Format ("{0}{1} event {2} {3};",
						visibility, modifiers, type, name);
	}
	
	static string MakeMemberSignature(MemberInfo mi) {
		if (mi is ConstructorInfo) return MakeConstructorSignature((ConstructorInfo)mi);
		if (mi is MethodInfo) return MakeMethodSignature((MethodInfo)mi);
		if (mi is PropertyInfo) return MakePropertySignature((PropertyInfo)mi);
		if (mi is FieldInfo) return MakeFieldSignature((FieldInfo)mi);
		if (mi is EventInfo) return MakeEventSignature((EventInfo)mi);
		throw new ArgumentException(mi.ToString());
	}

	// Converts a fully .NET qualified type name into a C#-looking one
	static string ConvertCTSName (string type) {
		if (type.EndsWith ("[]"))
			return ConvertCTSName(type.Substring(0, type.Length - 2).TrimEnd()) + "[]";

		if (type.EndsWith ("&"))
			return ConvertCTSName(type.Substring(0, type.Length - 1).TrimEnd()) + "&";

		if (type.EndsWith ("*"))
			return ConvertCTSName(type.Substring(0, type.Length - 1).TrimEnd()) + "*";

		if (!type.StartsWith ("System."))
				return type;
		
		switch (type) {
		case "System.Byte": return "byte";
		case "System.SByte": return "sbyte";
		case "System.Int16": return "short";
		case "System.Int32": return "int";
		case "System.Int64": return "long";

		case "System.UInt16": return "ushort";
		case "System.UInt32": return "uint";
		case "System.UInt64": return "ulong";

		case "System.Single":  return "float";
		case "System.Double":  return "double";
		case "System.Decimal": return "decimal";
		case "System.Boolean": return "bool";
		case "System.Char":    return "char";
		case "System.Void":    return "void";
		case "System.String":  return "string";
		case "System.Object":  return "object";
		}
		
		// Types in the system namespace just get their type name returned.
		if (type.StartsWith("System.")) {
			string sysname = type.Substring(7);
			if (sysname.IndexOf(".") == -1)
				return sysname;
		}

		return type;
	}
}

}
