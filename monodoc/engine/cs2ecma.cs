//
// cs2ecma.cs - C# documentation to ECMA source converter
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Mono.CSharp
{
	class Driver
	{
		public static void Main (string [] args)
		{
			if (args.Length < 4) {
				Console.WriteLine ("usage: cs2ecma c#-xmlfile assemblyfile outputfile library-type(arbitrary name)");
				return;
			}
			bool debug = args.Length > 4 && args [4] == "--debug";
			try {
				XmlTextReader input = new XmlTextReader (args [0]);
				Assembly asm = Assembly.LoadFile (Path.GetFullPath (args [1]));
				XmlTextWriter output = new XmlTextWriter (args [2], Encoding.UTF8);
				output.Formatting = Formatting.Indented;
				output.IndentChar = '\t';
				output.Indentation = 1;

				EcmaDocumentGenerator.Generate (input, asm,
					output, args [3]);
			} catch (Exception ex) {
				Console.WriteLine ("An error occured: " + ex.Message);
				if (debug)
					throw;
			}
		}
	}

	public class EcmaDocumentGenerator
	{
		#region Static Members

		public static void Generate (XmlReader input, Assembly asm, XmlWriter output, string libraryType)
		{
			new EcmaDocumentGenerator (
				input, asm, output, libraryType).Run ();
		}

		static readonly Type [] emptyParams = new Type [0];

		const BindingFlags bindAll =
			(BindingFlags.Public | 
			BindingFlags.NonPublic | 
			BindingFlags.Instance | 
			BindingFlags.Static);

		static readonly ICodeGenerator cscodegen = new Microsoft.CSharp.CSharpCodeProvider ().CreateGenerator ();

		#endregion

		#region Mapping Types

		class TypeMap
		{
			public TypeMap (Type type, XmlElement elem)
			{
				this.Type = type;
				this.Element = elem;
			}

			public readonly Type Type;
			public readonly XmlElement Element; // possibly null
			public readonly ArrayList Members = new ArrayList ();
		}

		class MemberMap
		{
			public MemberMap (MemberInfo mi, XmlElement elem)
			{
				this.Member = mi;
				this.Element = elem;
			}

			public readonly MemberInfo Member;
			public readonly XmlElement Element;
		}

		#endregion

		private EcmaDocumentGenerator (
			XmlReader input, 
			Assembly asm,
			XmlWriter output,
			string libraryType)
		{
			this.input = input;
			this.asm = asm;
			this.output = output;
			this.libraryType = libraryType;

			// Load referenced assemblies
			AssemblyName [] anames = asm.GetReferencedAssemblies ();
			references = new Assembly [anames.Length];
			for (int i = 0; i < anames.Length; i++) {
				Assembly a = Assembly.Load (anames [i]);
				if (a == null)
					throw new EcmaDocumentGeneratorException ("ERROR: referenced assembly could not be loaded: ");
				references [i] = a;
			}
		}

		XmlReader input;
		Assembly asm;
		Assembly [] references;
		XmlWriter output;
		string libraryType;

		StringBuilder tmpBuilder = new StringBuilder ();

		void Run ()
		{
			// LAMESPEC: ECMA document have DTD, but no public identifier.

			output.WriteStartElement ("Libraries");
			output.WriteStartElement ("Types");
			output.WriteAttributeString ("Library", libraryType);

			ProcessInput ();

			output.WriteEndElement (); // Types
			output.WriteEndElement (); // Libraries

			output.Close ();
		}

		void ProcessInput ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.Load (input);
			Hashtable types = new Hashtable ();
			ArrayList members = new ArrayList ();
			CollectTypesAndMembers (doc, types, members);

			MapMembers (doc, types, members);

			GenerateDocument (doc, types);
		}

		private void CollectTypesAndMembers (XmlDocument doc,
			Hashtable types, ArrayList members)
		{
			foreach (XmlElement member in
				doc.SelectNodes ("/doc/members/member")) {
				string raw = member.GetAttribute ("name");
				if (raw.Length < 2) {
					Console.WriteLine ("WARNING: invalid name string: " + raw);
					continue;
				}
				string name = raw.Substring (2);
				if (raw [0] == 'T') {
					Type t = asm.GetType (name);
					if (t == null) {
						int lastPeriod =  name.LastIndexOf ('.');
						if (lastPeriod > 0) {
							name = name.Substring (0, lastPeriod) + '+' + name.Substring (lastPeriod + 1);
							t = asm.GetType (name);
						}
					}
					if (t == null) {
						Console.WriteLine ("WARNING: documented type not found: " + raw);
						continue;
					}

					types.Add (t, new TypeMap (t, member));
				}
				else
					members.Add (member);
			}
		}

		private void MapMembers (XmlDocument doc,
			Hashtable types, ArrayList members)
		{
			foreach (XmlElement member in members) {
				string raw = member.GetAttribute ("name");
				string sig = raw.Substring (2);
				int brace = sig.IndexOf ('(');
				string ident = brace > 0 ? sig.Substring (0, brace) : sig;
				int lastPeriod = ident.LastIndexOf ('.');
				if (lastPeriod < 0) {
					Console.WriteLine ("WARNING: A documented member must contain full declaring type name: " + ident);
					continue;
				}
				string name = ident.Substring (lastPeriod + 1);
				string tname = ident.Substring (0, lastPeriod);
				Type t = asm.GetType (tname);
				if (t == null) {
					Console.WriteLine ("WARNING: The type specified in a documented member was not found: " + tname);
					continue;
				}
				MemberInfo [] mis = null;
				switch (name) {
				case "#ctor":
					mis = t.GetConstructors ();
					break;
				default:
					mis = t.GetMember (name, bindAll);
					break;
				}
				MemberInfo mi = null;
				if (mis.Length > 1) {
					if (raw [0] == 'E')
						mi = t.GetEvent (name);
					else {
						Type [] parameters = brace < 0 ? emptyParams : GetParameterTypes (sig.Substring (brace));
						ParameterInfo [] pinfos = null;
						if (parameters == null) {
							Console.WriteLine ("WARNING: specified parameters could not be loaded: " + raw);
							continue;
						}
						foreach (MemberInfo im in mis) {
							PropertyInfo pi = im as PropertyInfo;
							bool matches = true;
							if (pi != null)
								pinfos = pi.GetIndexParameters ();
							MethodBase mb = im as MethodBase;
							if (mb != null)
								pinfos = mb.GetParameters ();
							if (pinfos.Length != parameters.Length)
								continue;
							for (int i = 0; i < pinfos.Length; i++)
								if (pinfos [i].ParameterType != parameters [i])
									matches = false;
							if (!matches)
								continue;
							mi = im;
							break;
						}
					}
				}
				else if (mis.Length == 1)
					mi = mis [0];
				if (mi == null) {
					Console.WriteLine ("WARNING: incorrect number of members: {0} matched to {1} item(s).", sig, mis.Length);
					continue;
				}
				TypeMap map = (TypeMap) types [t];
				if (map == null) {
					// Possibly type comment is either
					// missing, invalid, and so on.
					map = new TypeMap (t, null);
					types [t] = map;
				}
				map.Members.Add (new MemberMap (mi, member));
			}
		}


		private void GenerateDocument (XmlDocument doc, Hashtable types)
		{
			foreach (DictionaryEntry de in types) {
				Type t = (Type) de.Key;
				TypeMap map = (TypeMap) de.Value;
				XmlElement type = map.Element;
				output.WriteStartElement ("Type");
				output.WriteAttributeString ("Name", t.Name);
				output.WriteAttributeString ("FullName",
					t.FullName.Replace ('+', '.'));
				output.WriteAttributeString ("FullNameSP",
					t.FullName.Replace ('.', '_')
						.Replace ('+', '_'));

				WriteTypeSignatures (t);

				output.WriteElementString ("MemberOfLibrary",
					libraryType);

				WriteAssemblyInfo (t);

				// Cannot get ThreadSafetyStatement from csdoc

				output.WriteStartElement ("Docs");
				if (map.Element != null)
					map.Element.WriteContentTo (output);
				output.WriteEndElement (); // Docs

				output.WriteStartElement ("Base");
				if (t.BaseType != null)
					output.WriteElementString (
						"BaseTypeName",
						t.BaseType.FullName);
				else
					output.WriteElementString (
						"BaseTypeName",
						"System.Object");
				output.WriteEndElement (); // Base

				output.WriteStartElement ("Interfaces");
				if (!t.IsEnum) {
					foreach (Type i in t.GetInterfaces ()) {
						output.WriteStartElement ("Interface");
						output.WriteElementString (
							"InterfaceName",
							i.FullName);
						// FIXME: find out what is expected here.
						output.WriteElementString (
							"Excluded",
							"0");
						output.WriteEndElement (); // Interface
					}
				}
				output.WriteEndElement (); // Interfaces

				WriteMembers (map);

				// FIXME: find out what is expected here.
				output.WriteElementString ("TypeExcluded",
					"0");

				output.WriteEndElement (); // Type
			}
		}

		Type [] GetParameterTypes (string sig)
		{
			string [] names = sig.Substring (1, sig.Length -2)
				.Split (',');
			Type [] types = new Type [names.Length];
			for (int i = 0; i < types.Length; i++) {
				Type t = asm.GetType (names [i]);
				if (t == null) {
					foreach (Assembly rasm in references) {
						t = rasm.GetType (names [i]);
						if (t != null)
							break;
					}
				}
				if (t == null)
					return null;
				types [i] = t;
			}
			return types;
		}

		void WriteTypeSignatures (Type t)
		{
			output.WriteStartElement ("TypeSignature");

			// FIXME: ILASM signature

			// C#
			output.WriteAttributeString ("Language", "C#");
			output.WriteStartAttribute ("Value", "");
			if (t.IsNestedPublic)
				output.WriteString ("public ");
			else if (t.IsNestedFamily)
				output.WriteString ("protected ");
			else if (t.IsNestedAssembly)
				output.WriteString ("internal ");
			else if (t.IsNestedFamORAssem)
				output.WriteString ("internal protected ");

			if (t.IsSealed)
				output.WriteString ("sealed ");
			else if (t.IsAbstract)
				output.WriteString ("abstract ");

			if (t.IsClass)
				output.WriteString ("class ");
			else if (t.IsEnum)
				output.WriteString ("enum ");
			else if (t.IsInterface)
				output.WriteString ("interface ");

			output.WriteString (t.Name);

			bool comma = false;
			if (t.BaseType != null) {
				output.WriteString (" : ");
				output.WriteString (t.Namespace == t.BaseType.Namespace ? t.BaseType.Name : t.BaseType.FullName);
				comma = true;
			}
			foreach (Type iface in t.GetInterfaces ()) {
				output.WriteString (comma ? ", " : " : ");
				comma = true;
				output.WriteString (iface.Namespace == t.Namespace ? iface.Name : iface.FullName);
			}

			output.WriteEndElement (); // TypeSignature
		}

		void WriteAssemblyInfo (Type t)
		{
			AssemblyName an = asm.GetName ();
			output.WriteStartElement ("AssemblyInfo");
			output.WriteElementString ("AssemblyName", an.Name);
			if (an.KeyPair != null) {
				byte [] keydata = an.KeyPair.PublicKey;
				tmpBuilder.Length = 0;
				tmpBuilder.Append ('[');
				for (int i = 0; i < keydata.Length; i++) {
					tmpBuilder.Append (keydata [i].ToString ("x"));
				}
				tmpBuilder.Append (']');
				output.WriteElementString ("AssemblyPublicKey",
					tmpBuilder.ToString ());
			}
			else
				output.WriteElementString ("AssemblyPublicKey", 
					"[00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ]");
			output.WriteElementString ("AssemblyVersion",
				an.Version.ToString ());
			string cultureName = an.CultureInfo != null ?
				an.CultureInfo.Name : "";
			output.WriteElementString ("AssemblyCulture",
				 cultureName.Length > 0 ? cultureName : "none");

			WriteAttributes (t, asm.GetCustomAttributes (typeof (Attribute), true));

			output.WriteEndElement (); // AssemblyInfo
		}

		void WriteAttributes (Type t, object [] atts)
		{
			output.WriteStartElement ("Attributes");
			foreach (object aobj in atts) {
				output.WriteStartElement ("Attribute");
				Type atype = aobj.GetType ();
				output.WriteElementString ("AttributeName",
					t.Namespace == atype.Namespace ?
					atype.Name : atype.FullName);
				// FIXME: fill constructor args (but how?)
				output.WriteEndElement (); // Attribute
			}
			output.WriteEndElement (); // Attributes
		}

		void WriteMemberSignatures (Type t, MemberInfo mi)
		{
			// FIXME: ILASM Signature

			// C# Signature
			output.WriteStartElement ("MemberSignature");
			output.WriteAttributeString ("Language", "C#");
			output.WriteStartAttribute ("Value", "");

			if (mi.MemberType == MemberTypes.Constructor)
				WriteConstructorSignatureCS ((ConstructorInfo) mi);
			else if (mi.MemberType == MemberTypes.Event)
				WriteEventSignatureCS ((EventInfo) mi);
			else if (mi.MemberType == MemberTypes.Field)
				WriteFieldSignatureCS ((FieldInfo) mi);
			else if (mi.MemberType == MemberTypes.Method)
				WriteMethodSignatureCS ((MethodInfo) mi);
			else if (mi.MemberType == MemberTypes.Property)
				WritePropertySignatureCS ((PropertyInfo) mi);
			output.WriteEndElement (); // MemberSignature
		}

		void WriteConstructorSignatureCS (ConstructorInfo mi)
		{
			WriteMethodAttributesCS (mi.Attributes, false);
		}

		void WriteEventSignatureCS (EventInfo mi)
		{
			WriteMethodAttributesCS (
				mi.GetAddMethod ().Attributes, false);
			output.WriteString (CSharpName (mi.EventHandlerType));
			output.WriteString (" ");
			output.WriteString (mi.Name);
			output.WriteString (";");
		}

		void WriteFieldSignatureCS (FieldInfo mi)
		{
			if (mi.IsPublic)
				output.WriteString ("public ");
			else if (mi.IsFamily)
				output.WriteString ("protected ");
			else if (mi.IsFamilyOrAssembly)
				output.WriteString ("internal protected ");
			else if (mi.IsAssembly)
				output.WriteString ("internal ");
			else if (mi.IsPrivate)
				output.WriteString ("private ");
			if (mi.IsStatic)
				output.WriteString ("static ");
			if (mi.IsInitOnly)
				output.WriteString ("readonly ");

			output.WriteString (CSharpName (mi.FieldType));
			output.WriteString (" ");
			output.WriteString (mi.Name);
			output.WriteString (";");
		}

		void WriteMethodSignatureCS (MethodInfo mi)
		{
			string op = OperatorNameCS (mi.Name);
			WriteMethodAttributesCS (mi.Attributes, op != null);
			output.WriteString (CSharpName (mi.ReturnType));
			output.WriteString (" ");
			output.WriteString (op != null ?
				"operator " + op : mi.Name);
			output.WriteString ("(");
			WriteParametersCS (mi.GetParameters ());
			output.WriteString (");");
		}

		void WriteParametersCS (ParameterInfo [] plist)
		{
			for (int i = 0; i < plist.Length; i++) {
				if (i > 0)
					output.WriteString (",");
				output.WriteString (CSharpName (
					plist [i].ParameterType));
				output.WriteString (" ");
				output.WriteString (plist [i].Name);
			}
		}

		void WritePropertySignatureCS (PropertyInfo mi)
		{
			MethodInfo method = mi.CanRead ?
				mi.GetGetMethod (true) : mi.GetSetMethod (true);
			WriteMethodAttributesCS (method.Attributes, false);
			output.WriteString (CSharpName (mi.PropertyType));
			output.WriteString (" ");
			output.WriteString (mi.Name);
			ParameterInfo [] plist = mi.GetIndexParameters ();
			if (plist.Length > 0) {
				output.WriteString (" [");
				WriteParametersCS (plist);
				output.WriteString ("]");
			}
			output.WriteString (" { ");
			if (mi.CanRead)
				output.WriteString ("get; ");
			if (mi.CanWrite)
				output.WriteString ("set; ");
			output.WriteString ("}");
		}

		void WriteMethodAttributesCS (MethodAttributes a, bool isOperator)
		{
			if ((a & MethodAttributes.Public) != 0)
				output.WriteString ("public ");
			else if ((a & MethodAttributes.FamORAssem) != 0)
				output.WriteString ("internal protected ");
			else if ((a & MethodAttributes.Assembly) != 0)
				output.WriteString ("internal ");
			else if ((a & MethodAttributes.Family) != 0)
				output.WriteString ("protected ");
			else if ((a & MethodAttributes.Private) != 0)
				output.WriteString ("private ");

			if ((a & MethodAttributes.Static) != 0)
				output.WriteString ("static ");
			if ((a & MethodAttributes.UnmanagedExport) != 0)
				output.WriteString ("extern ");

			if (!isOperator) {
				if ((a & MethodAttributes.Abstract) != 0)
					output.WriteString ("abstract ");
				else if ((a & MethodAttributes.HideBySig) != 0)
					output.WriteString ("override ");
				else if ((a & MethodAttributes.Virtual) != 0)
					output.WriteString ("virtual ");
				else if ((a & MethodAttributes.Final) != 0)
					output.WriteString ("sealed ");
			}
		}

		string OperatorNameCS (string name)
		{
			switch (name) {
			case "op_Addition":
				return "+";
			case "op_BitWiseAnd":
				return "&";
			case "op_BitWiseOr":
				return "|";
			case "op_Decrement":
				return "--";
			case "op_Division":
				return "/";
			case "op_Equality":
				return "==";
			case "op_ExclusiveOr":
				return "^";
			case "op_False":
				return "false";
			case "op_GreaterThan":
				return ">";
			case "op_Increment":
				return "++";
			case "op_Inequality":
				return "!=";
			case "op_LessThan":
				return "<";
			case "op_LogicalNot":
				return "!";
			case "op_Modulus":
				return "%";
			case "op_Multiply":
				return "*";
			case "op_OnesComplement":
				return "~";
			case "op_Subtraction":
				return "-";
			case "op_True":
				return "true";
			case "op_UnaryNegation":
				return "-";
			case "op_UnaryPlus":
				return "+";

			// They don't have specific name, so just return as is
			case "op_Implicit":
				return "op_Implicit";
			case "op_Explicit":
				return "op_Explicit";
			}
			return null;
		}

		string CSharpName (Type t)
		{
			// FIXME: Mhm...should check if assembly is mscorlib?
			// but some users might want to treat another assembly
			// as mscorlib (e.g. "corlib.dll").
			switch (t.FullName) {
			case "System.Int32":
				return "int";
			case "System.UInt32":
				return "uint";
			case "System.Short32":
				return "short";
			case "System.UShort32":
				return "ushort";
			case "System.Long32":
				return "long";
			case "System.ULong32":
				return "ulong";
			case "System.Single":
				return "float";
			case "System.Double":
				return "double";
			case "System.Char":
				return "char";
			case "System.Decimal":
				return "decimal";
			case "System.Byte":
				return "byte";
			case "System.SByte":
				return "sbyte";
			case "System.Object":
				return "object";
			case "System.Boolean":
				return "bool";
			case "System.String":
				return "string";
			case "System.Void":
				return "void";
			}
			return t.FullName;
		}

		void WriteMembers (TypeMap map)
		{
			Type t = map.Type;
			output.WriteStartElement ("Members");
			foreach (MemberMap mm in map.Members) {
				MemberInfo mi = mm.Member;
				output.WriteStartElement ("Member");
				output.WriteAttributeString ("MemberName", mi.Name);
				WriteMemberSignatures (t, mi);

				// MemberType
				string memberType = null;
				if (mi is ConstructorInfo)
					memberType = "Constructor";
				else if (mi is MethodInfo)
					memberType = "Method";
				else if (mi is PropertyInfo)
					memberType = "Property";
				else if (mi is FieldInfo)
					memberType = "Field";
				output.WriteElementString ("MemberType",
					memberType);

				WriteAttributes (map.Type, mi.GetCustomAttributes (true));

				// ReturnType, Parameters
				Type retType = null;
				ParameterInfo [] plist = null;
				if (mi is FieldInfo)
					retType = ((FieldInfo) mi).FieldType;
				else if (mi is PropertyInfo) {
					retType = ((PropertyInfo) mi).PropertyType;
					plist = ((PropertyInfo) mi).GetIndexParameters ();
				}
				else if (mi is MethodInfo)
					retType = ((MethodInfo) mi).ReturnType;

				if (mi is MethodBase)
					plist = ((MethodBase) mi).GetParameters ();
				output.WriteStartElement ("ReturnValue");
				if (retType != null) {
					output.WriteElementString ("ReturnType",
						retType.Namespace == t.Namespace ?
						retType.Name : retType.FullName);
				}
				output.WriteEndElement (); // ReturnValue
				output.WriteStartElement ("Parameters");
				if (plist != null) {
					foreach (ParameterInfo p in plist) {
						output.WriteStartElement ("Parameter");
						output.WriteAttributeString ("Name", p.Name);
						Type pt = p.ParameterType;
						output.WriteAttributeString ("Type", pt.Namespace == t.Namespace ? pt.Name : pt.FullName);
						output.WriteEndElement (); // Parameter
					}
				}
				output.WriteEndElement ();

				// Docs
				output.WriteStartElement ("Docs");
				if (mm.Element != null)
					mm.Element.WriteContentTo (output);
				output.WriteEndElement ();

				// FIXME: find out what is expected here.
				output.WriteElementString ("Excluded", "0");

				output.WriteEndElement (); // Member
			}
			output.WriteEndElement (); // Members
		}
	}

	public class EcmaDocumentGeneratorException : Exception
	{
		public EcmaDocumentGeneratorException (string msg)
			: base (msg)
		{
		}
	}

}
