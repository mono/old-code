using System;
using System.Collections;
using System.IO;
using System.Reflection;


namespace PHP.Compiler {


	public class SymbolTable {

		private static SymbolTable SymTab;
		public SymbolTableScope CurScope;
		public ArrayList ExternalAssemblies;
		public Hashtable ExternalNamespaces;
		public Hashtable ExternalTypesAvailable;

		public const int RESERVED_WORD = 0;
		public const int CLASS = 1;
		public const int CLASS_VARIABLE = 2;
		public const int INTERFACE = 3;
		public const int FUNCTION = 4;

		public static SymbolTable GetInstance() {
			if (SymTab == null)
				SymTab = new SymbolTable();
			return SymTab;
		}

		private SymbolTable() {
			CurScope = new SymbolTableScope(null);
			ExternalAssemblies = new ArrayList();
			ExternalTypesAvailable = new Hashtable();
			// collect namespaces of common assemblies
			ExternalNamespaces = new Hashtable();
			ArrayList assemblies = new ArrayList();
			assemblies.AddRange(System.AppDomain.CurrentDomain.GetAssemblies());
			assemblies.Add(Assembly.LoadFrom("mPHPRuntime.dll"));
			foreach (Assembly ass in assemblies) {
				if (ass.GetName().Name != "mPHP")
					try {
						// the following if clause is a workaround as Mono returns null for ass.GetTypes() on the dymanic assembly to be created
						if (ass.GetTypes() != null)
							foreach (Type t in ass.GetTypes()) {
								string name_space = (t.Namespace == null) ? "__root" : t.Namespace;
								ArrayList typesOfNamespace = (ArrayList)ExternalNamespaces[name_space];
								if (typesOfNamespace == null) {
									typesOfNamespace = new ArrayList();
									ExternalNamespaces[name_space] = typesOfNamespace;
								}
								typesOfNamespace.Add(t);
							}
					}
					catch (ReflectionTypeLoadException) {
						// when using .NET Framework ass.GetTypes() throws a ReflectionTypeLoadException when trying to get the types of the dynamic module to be created
						continue;
					}
			}
			// insert reserved words
			InsertGlobal("__MAIN", RESERVED_WORD);
			InsertGlobal("and", RESERVED_WORD);
			InsertGlobal("or", RESERVED_WORD);
			InsertGlobal("xor", RESERVED_WORD);
			InsertGlobal("__FILE__", RESERVED_WORD);
			InsertGlobal("exception", RESERVED_WORD);
			InsertGlobal("__LINE__", RESERVED_WORD);
			InsertGlobal("array", RESERVED_WORD);
			InsertGlobal("as", RESERVED_WORD);
			InsertGlobal("break", RESERVED_WORD);
			InsertGlobal("case", RESERVED_WORD);
			InsertGlobal("class", RESERVED_WORD);
			InsertGlobal("const", RESERVED_WORD);
			InsertGlobal("continue", RESERVED_WORD);
			InsertGlobal("declare", RESERVED_WORD);
			InsertGlobal("default", RESERVED_WORD);
			InsertGlobal("die", RESERVED_WORD);
			InsertGlobal("do", RESERVED_WORD);
			InsertGlobal("echo", RESERVED_WORD);
			InsertGlobal("else", RESERVED_WORD);
			InsertGlobal("elseif", RESERVED_WORD);
			InsertGlobal("empty", RESERVED_WORD);
			InsertGlobal("enddeclare", RESERVED_WORD);
			InsertGlobal("endfor", RESERVED_WORD);
			InsertGlobal("endforeach", RESERVED_WORD);
			InsertGlobal("endif", RESERVED_WORD);
			InsertGlobal("endswitch", RESERVED_WORD);
			InsertGlobal("endwhile", RESERVED_WORD);
			InsertGlobal("eval", RESERVED_WORD);
			InsertGlobal("exit", RESERVED_WORD);
			InsertGlobal("for", RESERVED_WORD);
			InsertGlobal("foreach", RESERVED_WORD);
			InsertGlobal("function", RESERVED_WORD);
			InsertGlobal("global", RESERVED_WORD);
			InsertGlobal("if", RESERVED_WORD);
			InsertGlobal("include", RESERVED_WORD);
			InsertGlobal("include_once", RESERVED_WORD);
			InsertGlobal("isset", RESERVED_WORD);
			InsertGlobal("list", RESERVED_WORD);
			InsertGlobal("new", RESERVED_WORD);
			InsertGlobal("print", RESERVED_WORD);
			InsertGlobal("require", RESERVED_WORD);
			InsertGlobal("require_once", RESERVED_WORD);
			InsertGlobal("return", RESERVED_WORD);
			InsertGlobal("static", RESERVED_WORD);
			InsertGlobal("switch", RESERVED_WORD);
			InsertGlobal("unset", RESERVED_WORD);
			InsertGlobal("use", RESERVED_WORD);
			InsertGlobal("var", RESERVED_WORD);
			InsertGlobal("while", RESERVED_WORD);
			InsertGlobal("__FUNCTION__", RESERVED_WORD);
			InsertGlobal("__CLASS__", RESERVED_WORD);
			InsertGlobal("__METHOD__", RESERVED_WORD);
			InsertGlobal("final", RESERVED_WORD);
			InsertGlobal("php_user_filter", RESERVED_WORD);
			InsertGlobal("interface", RESERVED_WORD);
			InsertGlobal("implements", RESERVED_WORD);
			InsertGlobal("extends", RESERVED_WORD);
			InsertGlobal("public", RESERVED_WORD);
			InsertGlobal("private", RESERVED_WORD);
			InsertGlobal("protected", RESERVED_WORD);
			InsertGlobal("abstract", RESERVED_WORD);
			InsertGlobal("clone", RESERVED_WORD);
			InsertGlobal("try", RESERVED_WORD);
			InsertGlobal("catch", RESERVED_WORD);
			InsertGlobal("throw", RESERVED_WORD);
		}

		public void AddExternalAssembly(string filename) {
			try {
				Assembly ass = Assembly.LoadFrom(filename);
				ExternalAssemblies.Add(ass);
				// add types to external namespaces
				foreach (Type t in ass.GetTypes()) {
					string nameSpace = (t.Namespace == null) ? "__root" : t.Namespace;
					ArrayList typesOfNamespace = (ArrayList)ExternalNamespaces[nameSpace];
					if (typesOfNamespace == null) {
						typesOfNamespace = new ArrayList();
						ExternalNamespaces[nameSpace] = typesOfNamespace;
					}
					typesOfNamespace.Add(t);
				}
			}
			catch (FileNotFoundException) {
				Report.Error(006, filename);
			}
			catch (BadImageFormatException) {
				Report.Error(007, filename);
			}
		}

		public void makeExternalTypeAvailable(string type, string alias) {
			string dotNetType = type.Replace("::", ".");
			// if a namespace was specified...
			if (ExternalNamespaces.ContainsKey(dotNetType)) {
				// ...make sure no alias was passed
				if (alias != null)
					Report.Error(120, type);
				// ...make each type of namespace available
				ArrayList typesOfNamespace = (ArrayList)ExternalNamespaces[dotNetType];
				foreach (Type t in typesOfNamespace)
					AddExternalType(t, t.Name);
				// done
				return;
			}
			// otherwise...
			else {
				// determine namespace
				string nameSpace;
				if (dotNetType.LastIndexOf('.') == -1)
					nameSpace = "__root";
				else
					nameSpace = dotNetType.Substring(0, dotNetType.LastIndexOf('.'));
				// determine full type name
				// ...search for type in given namespace
				ArrayList typesOfNamespace = (ArrayList)ExternalNamespaces[nameSpace];
				if (typesOfNamespace == null)
					Report.Error(203, dotNetType);
				// ...make type available
				foreach (Type tmpT in typesOfNamespace) {
					if (tmpT.FullName == dotNetType) {
						// if no alias is specified, use simple type name as alias
						if (alias == null || alias == "")
							AddExternalType(tmpT, tmpT.Name);
						// otherwise use alias specified
						else
							AddExternalType(tmpT, alias);
						return;
					}
				}
				// type not found
				Report.Error(203, type);
			}
		}

		private void AddExternalType(Type t, string alias) {
			ArrayList types = (ArrayList)ExternalTypesAvailable[alias];
			if (types == null) {
				types = new ArrayList();
				ExternalTypesAvailable[alias] = types;
			}
			types.Add(t);
		}

		public Type GetExternalType(string type) {
			// check if an appropriate alias is available
			ArrayList types = (ArrayList)ExternalTypesAvailable[type];
			if (types != null) {
				if (types.Count > 1)
					Report.Error(121, type);
				return (Type)types[0];
			}
			// otherwise check for full type
			string dotNetType = type.Replace("::", ".");
			string nameSpace;
			if (dotNetType.LastIndexOf('.') == -1)
				nameSpace = "__root";
			else
				nameSpace = dotNetType.Substring(0, dotNetType.LastIndexOf('.'));
			ArrayList typesOfNamespace = (ArrayList)ExternalNamespaces[nameSpace];
			if (typesOfNamespace == null)
				return null;
			foreach (Type tmpT in typesOfNamespace)
				if (tmpT.FullName == dotNetType)
					return tmpT;
			// nothing found
			return null;
		}

		public void openScope() {
			SymbolTableScope new_scope = new SymbolTableScope(CurScope);
			CurScope = new_scope;
		}

		public void CloseScope() {
			CurScope = CurScope.Parent;
		}

		public void InsertLocal(string name, int kind) {
			InsertLocal(name, kind, null);
		}

		public void InsertLocal(string name, int kind, ASTNode node) {
			CurScope.insert(name, kind, node);
		}

		public void InsertGlobal(string name, int kind) {
			InsertGlobal(name, kind, null);
		}

		public void InsertGlobal(string name, int kind, ASTNode node) {
			GetTopScope().insert(name, kind, node);
		}

		public SymbolTableEntry Lookup(string name, int kind) {
			return CurScope.Lookup(name, kind);
		}

		public SymbolTableEntry LookupGlobal(string name, int kind) {
			return GetTopScope().Lookup(name, kind);
		}

		public static void reset() {
			SymTab = new SymbolTable();
		}

		private SymbolTableScope GetTopScope() {
			SymbolTableScope tmp_scope = CurScope;
			while (tmp_scope.Parent != null)
				tmp_scope = tmp_scope.Parent;
			return tmp_scope;
		}

	}


	public class SymbolTableScope {

		public SymbolTableScope Parent;
		public Hashtable Entries;

		public ArrayList ClassMembers;

		public SymbolTableScope(SymbolTableScope parent) {
			Parent = parent;
			Entries = new Hashtable();
			ClassMembers = new ArrayList();
		}

		public void insert(string name, int kind) {
			insert(name, kind, null);
		}

		public void insert(string name, int kind, ASTNode node) {
			// treat inferface, classe and function names as case insensitive
			if (kind == SymbolTable.INTERFACE || kind == SymbolTable.CLASS || kind == SymbolTable.FUNCTION)
				name = name.ToLower();
			SymbolTableEntry entry = new SymbolTableEntry(name, kind, node);
			// a new class member?
			if (kind == SymbolTable.CLASS_VARIABLE) {
				if (ClassMembers.Contains(name))
					Report.Error(306, name);
				ClassMembers.Add(name);
			}
			// no symbol with this name exists, so add it
			if (Entries[name] == null) {
				Hashtable value = new Hashtable();
				value[kind] = entry;
				Entries[name] = value;
			}
			// a symbol with this name already exists
			else {
				Hashtable value = (Hashtable)Entries[name];
				// but with another kind, so add it
				if (value[kind] == null)
					value[kind] = entry;
				// with kind reserved word, class, interface, class variable or function, so report error
				else
					switch (kind) {
						case SymbolTable.RESERVED_WORD: Report.Error(201, name, node.Line, node.Column); break;
						case SymbolTable.CLASS: Report.Error(202, name, node.Line, node.Column); break;
						case SymbolTable.INTERFACE: Report.Error(220, name, node.Line, node.Column); break;
						case SymbolTable.CLASS_VARIABLE: Report.Error(204, name, node.Line, node.Column); break;
						case SymbolTable.FUNCTION: Report.Error(211, name, node.Line, node.Column); break;
					}
			}

		}

		public SymbolTableEntry Lookup(string name, int kind) {
			// treat inferface, classe and function names as case insensitive
			if (kind == SymbolTable.INTERFACE || kind == SymbolTable.CLASS || kind == SymbolTable.FUNCTION)
				name = name.ToLower();
			Hashtable entry = (Hashtable)Entries[name];
			// no entry with this name and kind found in current scope
			if (entry == null || entry[kind] == null) {
				// no parent scope, so we are at top scope
				if (Parent == null)
					return null;
				// parent scope available, so lookup there
				else
					return Parent.Lookup(name, kind);
			}
			// entry found with matching kind
			else
				return (SymbolTableEntry)entry[kind];
		}

		public ArrayList Lookup(int kind) {
			ArrayList result = new ArrayList();
			// check all entries
			foreach (Hashtable entry in Entries.Values) {
				// if entry contains specified kind, add to result
				if (entry[kind] != null)
					result.Add((SymbolTableEntry)entry[kind]);
			}
			// done
			return result;
		}

	}


	public class SymbolTableEntry {

		public string Name;
		public int Kind;
		public ASTNode Node;

		public SymbolTableEntry(string name, int kind, ASTNode node) {
			Name = name;
			Kind = kind;
			Node = node;
		}

	}


}