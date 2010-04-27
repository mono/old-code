using System;
using System.Collections;
using System.Reflection;

using Gdk;

namespace Monotalk.Browser
{
	public class TypeAliases
	{
		private Hashtable knownTypes = new Hashtable ();
		public static bool FullNames = false;

		public string this [Type type] {
			set {
				knownTypes.Add (type, value);
			}
			get {
				if (type.IsArray) {
					if (knownTypes.Contains (type.GetElementType ()))
						return (string) knownTypes [type.GetElementType ()] + "[]";
				} else
					if (knownTypes.Contains (type))
						return (string) knownTypes [type];

				return FullNames ? type.FullName : type.Name;
			}
		}
	}

	public abstract class MemberRecord
	{
		private static readonly Pixbuf icon = new Pixbuf (null, "transparent.png");
		public static TypeAliases Alias = new TypeAliases ();

		protected MemberInfo mi;

		static MemberRecord ()
		{
			Alias [typeof (object)] = "object";
			Alias [typeof (string)] = "string";
			Alias [Type.GetType ("System.String&")] = "string";
			Alias [typeof (sbyte)] = "sbyte";
			Alias [typeof (byte)] = "byte";
			Alias [typeof (short)] = "short";
			Alias [typeof (ushort)] = "ushort";
			Alias [typeof (int)] = "int";
			Alias [typeof (uint)] = "uint";
			Alias [typeof (long)] = "long";
			Alias [typeof (ulong)] = "ulong";
			Alias [typeof (char)] = "char";
			Alias [typeof (float)] = "float";
			Alias [typeof (double)] = "double";
			Alias [typeof (bool)] = "bool";
			Alias [typeof (decimal)] = "decimal";
			Alias [typeof (void)] = "void";
		}

		public MemberRecord (MemberInfo info)
		{
			mi = info;
		}

		public MemberInfo MemberInfo {
			get {
				return mi;
			}
		}

		public abstract string Label {
			get;
		}

		public virtual string SourceKey {
			get {
				return "";
			}
		}

		protected string EscapedName (string name)
		{
			return name.Replace ("<", "&lt;").Replace (">", "&gt;");
		}

		public virtual Pixbuf Icon  {
			get {
				return icon;
			}
		}

		public virtual string Name {
			get {
				return mi.Name;
			}
		}
		
		protected string ParametersLabel (ParameterInfo[] piArray)
		{
			string parameters = "";
			bool first = true;

			foreach (ParameterInfo pi in piArray) {
				if (!first)
					parameters = parameters + ", ";

				if (pi.IsOut)
					parameters += "<span color=\"orange\">out</span> ";

				string paramTypeName = Alias [pi.ParameterType];
				parameters += "<span color=\"blue\">" + paramTypeName + "</span> " + pi.Name;;
				first = false;
			}

			return parameters;
		}

		protected string ParametersKey (ParameterInfo[] piArray)
		{
			string parameters = "";
			bool first = true;

			foreach (ParameterInfo pi in piArray) {
				if (!first)
					parameters = parameters + ", ";

				//if (pi.IsOut)
				//parameters += "out ";

				string paramTypeName = Alias [pi.ParameterType];
				parameters += paramTypeName + " " + pi.Name;;
				first = false;
			}

			return parameters;
		}

		protected string MethodLabel (string name)
		{
			return "<b>" + name + "</b> (" + ParametersLabel (((MethodBase) mi).GetParameters ()) + ")";
		}

		protected string MethodKey (string name)
		{
			return name + " (" + ParametersKey (((MethodBase) mi).GetParameters ()) + ")";
		}

		public MemberInfo Info {
			get {
				return mi;
			}
		}
	}

	public class MethodRecord : MemberRecord
	{
		private static readonly Pixbuf iconMethod = new Pixbuf (null, "method.png");

		public MethodRecord (MethodInfo mi) : base (mi) {}

		public override string Label {
			get {
				return MethodLabel (mi.Name) + ", <span color=\"blue\">" + Alias [((MethodInfo) mi).ReturnType] + "</span>";
			}
		}

		public override string SourceKey {
			get {
				return MethodKey (mi.Name) + ", " + Alias [((MethodInfo) mi).ReturnType];
			}
		}

		public override Pixbuf Icon {
			get {
				return iconMethod;
			}
		}
	}

	public class ConstructorRecord : MemberRecord
	{
		private static readonly Pixbuf iconConstructor = new Pixbuf (null, "constructor.png");

		public ConstructorRecord (ConstructorInfo mi) : base (mi) {}

		public override string Label {
			get {
				return MethodLabel (mi.DeclaringType.Name);
			}
		}

		public override string SourceKey {
			get {
				return MethodKey (mi.Name);
			}
		}

		public override Pixbuf Icon {
			get {
				return iconConstructor;
			}
		}

		public override string Name {
			get {
				return mi.DeclaringType.Name;
			}
		}
	}

	public class EventRecord : MemberRecord
	{
		private static readonly Pixbuf iconEvent = new Pixbuf (null, "event.png");

		public EventRecord (EventInfo mi) : base (mi) {}

		public override string Label {
			get {
				return "<b>" + mi.Name + "</b> (" + ParametersLabel (((EventInfo) mi).GetAddMethod (true).GetParameters ()) + ")";
			}
		}

		public override Pixbuf Icon {
			get {
				return iconEvent;
			}
		}
	}

	public class FieldRecord : MemberRecord
	{
		private static readonly Pixbuf iconField = new Pixbuf (null, "field.png");

		public FieldRecord (FieldInfo mi) : base (mi) {}

		public override Pixbuf Icon {
			get {
				return iconField;
			}
		}

		public override string SourceKey {
			get {
				return mi.Name;
			}
		}

		public override string Label {
			get {
				return "<b>" + mi.Name + "</b>, <span color=\"blue\">" + Alias [((FieldInfo) mi).FieldType] + "</span>";
			}
		}
	}

	public class PropertyRecord : MemberRecord
	{
		private static readonly Pixbuf iconRO = new Pixbuf (null, "prop-read-only.png");
		private static readonly Pixbuf iconWO = new Pixbuf (null, "prop-write-only.png");
		private static readonly Pixbuf iconRW = new Pixbuf (null, "prop-read-write.png");

		public PropertyRecord (PropertyInfo mi) : base (mi) {}

		public override Pixbuf Icon {
			get {
				PropertyInfo pi = (PropertyInfo) mi;

				if (pi.CanRead) {
					if (pi.CanWrite)
						return iconRW;
					return iconRO;
				}
				return iconWO;
			}
		}

		public override string SourceKey {
			get {
				return Alias [((PropertyInfo) mi).PropertyType] + " " + mi.Name;
			}
		}

		public override string Label {
			get {
				return "<b>" + mi.Name + "</b>, <span color=\"blue\">" + Alias [((PropertyInfo) mi).PropertyType] + "</span>";
			}
		}
	}

	public class TypeRecord : MemberRecord
	{
		private static readonly Pixbuf iconClass = new Pixbuf (null, "class.png");
		private static readonly Pixbuf iconSealed = new Pixbuf (null, "sealed.png");
		private static readonly Pixbuf iconAbstract = new Pixbuf (null, "abstract.png");
		private static readonly Pixbuf iconEnum = new Pixbuf (null, "enum.png");
		private static readonly Pixbuf iconInterface = new Pixbuf (null, "interface.png");

		public TypeRecord (Type type) : base (type) {}

		public override Pixbuf Icon {
			get {
				Type type = mi as Type;

				if (type.IsEnum)
					return iconEnum;
				else if (type.IsInterface)
					return iconInterface;
				else if (type.IsAbstract)
					return iconAbstract;
				else if (type.IsSealed)
					return iconSealed;
				return iconClass;
			}
		}

		public override string Label {
			get {
				return "<b>" + EscapedName (mi.Name) + "</b>";
			}
		}
	}
}
