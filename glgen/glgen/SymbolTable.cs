// GtkSharp.Generation.SymbolTable.cs - The Symbol Table Class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2002 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.Collections;

	public class SymbolTable {
		
		private static Hashtable alias = new Hashtable ();
		private static Hashtable complex_types = new Hashtable ();
		private static Hashtable simple_types;
		private static Hashtable dlls;
		
		static SymbolTable ()
		{
			simple_types = new Hashtable ();
			simple_types.Add ("void", "void");
			simple_types.Add ("void *", "IntPtr");
			simple_types.Add ("const void *", "IntPtr");
			simple_types.Add ("unsigned int", "uint");
			simple_types.Add ("const unsigned int *", "uint[]");
			simple_types.Add ("unsigned int *", "int[]");
			simple_types.Add ("unsigned char", "byte");
			simple_types.Add ("const unsigned char *", "byte[]");
			simple_types.Add ("unsigned char *", "byte[]");
			simple_types.Add ("signed char", "sbyte");
			simple_types.Add ("const signed char *", "sbyte[]");
			simple_types.Add ("char", "char");
			simple_types.Add ("double", "double");
			simple_types.Add ("double *", "double[]");
			simple_types.Add ("float", "float");
			simple_types.Add ("int", "int");
			simple_types.Add ("short", "short");
			simple_types.Add ("const short *", "short[]");
			simple_types.Add ("unsigned short", "ushort");
			simple_types.Add ("unsigned short *", "ushort[]");
			simple_types.Add ("const unsigned short *", "ushort[]");
			simple_types.Add ("float *", "float[]");
			simple_types.Add ("const float *", "float[]");
			simple_types.Add ("int *", "int[]");
			simple_types.Add ("const int *", "int[]");
			simple_types.Add ("const double *", "double[]");
			
			dlls = new Hashtable();
			dlls.Add("gl", "GL");
			dlls.Add("glut", "glut");

			// Here are the real exceptions to the rule...
			AddAlias ("GLvoid*", "void *");
		}
		
		public static void AddAlias (string name, string type)
		{
			type = type.TrimEnd(' ', '\t');
			alias [name] = type;
		}

		public static void AddType (IGeneratable gen)
		{
			complex_types [gen.Name] = gen;
		}

		public static int Count {
			get
			{
				return complex_types.Count;
			}
		}

		public static IEnumerable Generatables
		{
			get
			{
				return complex_types.Values;
			}
		}
		
		private static string Trim(string type)
		{
			string trim_type = type.TrimEnd('*');
			return trim_type;
		}

		private static string DeAlias (string type)
		{
			while (alias.ContainsKey(type))
				type = (string) alias[type];
			return type;
		}

		public static string GetCSType(string c_type)
		{
			string da_c_type = DeAlias(c_type);

			// Ok, in this case, we check to see if we
			// could dealias c_type.  If we couldn't, let's
			// try converting the OGL type to something
			// else, and see if we can get a winner...
			
			if (da_c_type == c_type) {

				// First: Split up the string, and clear out so
				// we can "rebuild" the c_type.
				string[] c_args = da_c_type.Split(null);
				string edit_arg;

				da_c_type = "";

				foreach (string arg in c_args) {
					if (arg.StartsWith("GL")) {
						edit_arg = DeAlias(arg);
					} else {
						edit_arg = arg;
					}
					da_c_type = String.Concat(da_c_type, edit_arg);
					da_c_type = String.Concat(da_c_type, " ");
				}

				da_c_type = da_c_type.TrimEnd(' ');
			}

			c_type = DeAlias(da_c_type);

			if (simple_types.ContainsKey(c_type)) {
				return (string) simple_types[c_type];
			} else {
				Console.Error.WriteLine("err: " + c_type);
				return "_ERROR_";
			}
		}
		
		public static string GetDllName(string ns)
		{
			return (string) dlls[ns];
		}
		
		public static bool IsFunc(string c_type)
		{
			c_type = Trim(c_type);
			c_type = DeAlias(c_type);
			if (complex_types.ContainsKey(c_type)) {
				IGeneratable gen = (IGeneratable) complex_types[c_type];
				if (gen is FuncGen) {
					return true;
				}
			}
			return false;
		}
		
		public static bool IsConst(string c_type)
		{
			c_type = Trim(c_type);
			c_type = DeAlias(c_type);
			if (complex_types.ContainsKey(c_type)) {
				IGeneratable gen = (IGeneratable) complex_types[c_type];
				if (gen is ConstGen) {
					return true;
				}
			}
			return false;
		}
	}
}

