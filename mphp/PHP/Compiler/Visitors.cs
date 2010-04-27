using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;


namespace PHP.Compiler {


	// visitor interface
	public interface Visitor {
		void Visit(AST ast);
	}


	// build symbol table
	public class SymbolTableVisitor : Visitor {

		public CLASS_DECLARATION CD__MAIN;

		public SymbolTableVisitor() { }

		public void Visit(AST ast) {
			// start with emtpy symbol table
			//SymbolTable.reset();
			// build symbol table and at the same time check references
			foreach (Statement stmt in ast.StmtList)
				Visit(stmt);
		}

		protected void Visit(ASTNode node) {
			if (node == null)
				return;
			else if (node is INTERFACE_DECLARATION) {
				INTERFACE_DECLARATION id = (INTERFACE_DECLARATION)node;
				// process statements of interface
				SymbolTable.GetInstance().CurScope = id.Scope;
				foreach (Statement stmt in id.StmtList)
					Visit(stmt);
			}
			else if (node is CLASS_DECLARATION) {
				CLASS_DECLARATION cd = (CLASS_DECLARATION)node;
				// remember __main class
				if (cd.Name == "__MAIN")
					CD__MAIN = cd;
				// process statements of class
				SymbolTable.GetInstance().CurScope = cd.Scope;
				foreach (Statement stmt in cd.StmtList)
					Visit(stmt);
			}
			else if (node is CLASS_VARIABLE_DECLARATION) {
				CLASS_VARIABLE_DECLARATION cvd = (CLASS_VARIABLE_DECLARATION)node;
				// process all values assigned
				foreach (Expression expr in cvd.Values)
					Visit(expr);
			}
			else if (node is FUNCTION_DECLARATION) {
				FUNCTION_DECLARATION fd = (FUNCTION_DECLARATION)node;
				// process parameters and statements of function
				SymbolTable.GetInstance().CurScope = fd.Scope;
				foreach (PARAMETER_DECLARATION pd in fd.Parameters)
					Visit(pd);
				if (fd.StmtList != null)
					foreach (Statement stmt in fd.StmtList)
						Visit(stmt);
			}
			else if (node is PARAMETER_DECLARATION) {
				PARAMETER_DECLARATION pd = (PARAMETER_DECLARATION)node;
				// ensure hinted type is available
				if (pd.Type != null) {
					SymbolTableEntry cdEntry = SymbolTable.GetInstance().LookupGlobal(pd.Type, SymbolTable.CLASS);
					if (cdEntry == null) {
						Type t = SymbolTable.GetInstance().GetExternalType(pd.Type);
						if (t == null)
							Report.Error(203, pd.Type, pd.Line, pd.Column);
					}
				}
				// process default value
				Visit(pd.DefaultValue);
			}
			else if (node is GLOBAL) {
				GLOBAL g = (GLOBAL)node;
				// process expressions
				foreach (Expression expr in g.VarList)
					Visit(expr);
			}
			else if (node is STATIC_DECLARATION) {
				STATIC_DECLARATION sd = (STATIC_DECLARATION)node;
				// process expressions
				foreach (Expression expr in sd.ExprList)
					Visit(expr);
			}
			else if (node is BLOCK) {
				BLOCK b = (BLOCK)node;
				// process statements of block
				foreach (Statement stmt in b.StmtList)
					Visit(stmt);
			}
			else if (node is StatementList) {
				StatementList s = (StatementList)node;
				// process statements of block
				foreach (Statement stmt in s)
					Visit(stmt);
			}
			else if (node is TRY) {
				TRY t = (TRY)node;
				// process statements of try block
				foreach (Statement stmt in t.StmtList)
					Visit(stmt);
				// process catch blocks
				foreach (CATCH c in t.Catches)
					Visit(c);
			}
			else if (node is CATCH) {
				CATCH c = (CATCH)node;
				// ensure exception type is defined
				SymbolTableEntry cdEntry = SymbolTable.GetInstance().LookupGlobal(c.Type, SymbolTable.CLASS);
				if (cdEntry == null) {
					Type externalType = SymbolTable.GetInstance().GetExternalType(c.Type);
					if (externalType == null)
						Report.Error(203, c.Type, c.Line, c.Column);
				}
				// process statements of try block
				foreach (Statement stmt in c.StmtList)
					Visit(stmt);
			}
			else if (node is THROW) {
				THROW t = (THROW)node;
				// process throw expression
				Visit(t.Expr);
			}
			else if (node is IF) {
				// process if statement
				IF i = (IF)node;
				Visit(i.Expr);
				Visit(i.Stmt);
				// process else if statements
				foreach (ELSEIF e in i.ElseifList)
					Visit(e);
				// process else statement
				Visit(i.ElseStmt);
			}
			else if (node is ELSEIF) {
				// process if statement
				ELSEIF e = (ELSEIF)node;
				Visit(e.Expr);
				Visit(e.Stmt);
			}
			else if (node is WHILE) {
				WHILE w = (WHILE)node;
				// process while expression and statement
				Visit(w.Expr);
				Visit(w.Stmt);
			}
			else if (node is DO) {
				DO d = (DO)node;
				// process do expression and statement
				Visit(d.Stmt);
				Visit(d.Expr);
			}
			else if (node is FOR) {
				FOR f = (FOR)node;
				// process for expressions and statement
				foreach (Expression e in f.ExprList1)
					Visit(e);
				foreach (Expression e2 in f.ExprList2)
					Visit(e2);
				Visit(f.Stmt);
				foreach (Expression e3 in f.ExprList3)
					Visit(e3);
			}
			else if (node is FOREACH) {
				FOREACH f = (FOREACH)node;
				// process foreach expressions and statement
				if (f.Key != null && f.Key is FUNCTION_CALL)
					Report.Error(406, ((FUNCTION_CALL)f.Key).FunctionName, f.Key.Line, f.Key.Column);
				if (f.Value is FUNCTION_CALL)
					Report.Error(406, ((FUNCTION_CALL)f.Value).FunctionName, f.Value.Column, f.Value.Line);
				Visit(f.Stmt);
			}
			else if (node is SWITCH) {
				SWITCH s = (SWITCH)node;
				// process cases and default
				foreach (ASTNode tmpNnode in s.SwitchCaseList)
					Visit(tmpNnode);
			}
			else if (node is CASE) {
				CASE c = (CASE)node;
				// process case expression and statement
				Visit(c.Expr);
				Visit(c.Stmt);
			}
			else if (node is DEFAULT) {
				DEFAULT d = (DEFAULT)node;
				// process default statement
				Visit(d.Stmt);
			}
			else if (node is BREAK) {
				BREAK b = (BREAK)node;
				// process expression
				Visit(b.Expr);
			}
			else if (node is CONTINUE) {
				CONTINUE c = (CONTINUE)node;
				// process expression
				Visit(c.Expr);
			}
			else if (node is RETURN) {
				RETURN r = (RETURN)node;
				// process expression
				Visit(r.Expr);
			}
			else if (node is UNSET) {
				UNSET u = (UNSET)node;
				// process for unset expressions
				foreach (Expression e in u.VarList)
					Visit(e);
			}
			else if (node is ECHO) {
				ECHO e = (ECHO)node;
				// process echo expressions
				foreach (Expression e2 in e.ExprList)
					Visit(e2);
			}
			else if (node is EXPRESSION_AS_STATEMENT) {
				EXPRESSION_AS_STATEMENT eas = (EXPRESSION_AS_STATEMENT)node;
				// process expression
				Visit(eas.Expr);
			}
			else if (node is VARIABLE) {
				VARIABLE var = (VARIABLE)node;
				// process offset, if available
				if (var.Offset != null)
					Visit(var.Offset);
			}
			else if (node is OFFSET) {
				OFFSET o = (OFFSET)node;
				// process offset expression
				Visit(o.Value);
			}
			else if (node is FUNCTION_CALL) {
				FUNCTION_CALL fc = (FUNCTION_CALL)node;
				// process parameters
				foreach (Expression expr in fc.Parameters)
					Visit(expr);
			}
			else if (node is NEW) {
				NEW n = (NEW)node;
				// ensure type is available
				SymbolTableEntry cdEntry = SymbolTable.GetInstance().LookupGlobal(n.Type, SymbolTable.CLASS);
				if (cdEntry == null) {
					Type externalType = SymbolTable.GetInstance().GetExternalType(n.Type);
					if (externalType == null)
						Report.Error(203, n.Type, n.Line, n.Column);
				}
			}
			else if (node is INSTANCEOF) {
				INSTANCEOF i = (INSTANCEOF)node;
				// ensure type is available
				SymbolTableEntry cdEntry = SymbolTable.GetInstance().LookupGlobal(i.Type, SymbolTable.CLASS);
				if (cdEntry == null) {
					Type externalType = SymbolTable.GetInstance().GetExternalType(i.Type);
					if (externalType == null)
						Report.Error(203, i.Type, i.Line, i.Column);
				}
				// process expression
				Visit(i.Expr);
			}
			else if (node is ARRAY) {
				ARRAY a = (ARRAY)node;
				// process array pairs
				foreach (ARRAY_PAIR ap in a.ArrayPairList) {
					Visit(ap.Key);
					Visit(ap.Value);
				}
			}
			else if (node is PAAMAYIM_NEKUDOTAYIM) {
				PAAMAYIM_NEKUDOTAYIM pn = (PAAMAYIM_NEKUDOTAYIM)node;
				// ensure type is available
				if (pn.Type != "self" && pn.Type != "parent") {
					SymbolTableEntry cdEntry = SymbolTable.GetInstance().LookupGlobal(pn.Type, SymbolTable.CLASS);
					if (cdEntry == null) {
						Type externalType = SymbolTable.GetInstance().GetExternalType(pn.Type);
						if (externalType == null)
							Report.Error(203, pn.Type, pn.Line, pn.Column);
					}
				}
				// process expression
				Visit(pn.Expr);
			}
			else if (node is OBJECT_OPERATOR) {
				OBJECT_OPERATOR oo = (OBJECT_OPERATOR)node;
				// process left part
				Visit(oo.Expr1);
				// process right part
				Visit(oo.Expr2);
			}
			else if (node is EQUALS) {
				EQUALS e = (EQUALS)node;
				// process expressions
				Visit(e.Expr1);
				Visit(e.Expr2);
			}
			else if (node is UnaryExpression) {
				UnaryExpression ue = (UnaryExpression)node;
				// process expression
				Visit(ue.Expr);
			}
			else if (node is BinaryExpression) {
				BinaryExpression be = (BinaryExpression)node;
				// process expressions
				Visit(be.Expr1);
				Visit(be.Expr2);
			}
			else if (node is TernaryExpression) {
				TernaryExpression te = (TernaryExpression)node;
				// process expressions
				Visit(te.Expr1);
				Visit(te.Expr2);
				Visit(te.Expr3);
			}
			else if (node is Expression) {
				Expression e = (Expression)node;
				if (e is UnaryExpression)
					Visit((UnaryExpression)e);
				else if (e is BinaryExpression)
					Visit((BinaryExpression)e);
				else if (e is TernaryExpression)
					Visit((TernaryExpression)e);
			}
		}
	}


	// collects all types so they can be used before being declared
	// collects all class variables and functions so they can be used before being declared
	public class TypesVisitor : Visitor {

		public Assembly mPHPRuntime;
		public Type PHPRuntimeLang;
		public Type PHPRuntimeOperators;
		public Type PHPRuntimeConvert;
		public Type PHPRuntimeCore;
		public Type PHPArray;
		public Type PHPObject;
		public Type PHPReference;

		public Hashtable TypesAlreadyProcessed;

		public TypesVisitor() {
			mPHPRuntime = Assembly.LoadFrom("mPHPRuntime.dll");
			PHPRuntimeLang = mPHPRuntime.GetType("PHP.Runtime.Lang");
			PHPRuntimeOperators = mPHPRuntime.GetType("PHP.Runtime.Operators");
			PHPRuntimeConvert = mPHPRuntime.GetType("PHP.Runtime.Convert");
			PHPRuntimeCore = mPHPRuntime.GetType("PHP.Runtime.Core");
			PHPArray = mPHPRuntime.GetType("PHP.Array");
			PHPObject = mPHPRuntime.GetType("PHP.Object");
			PHPReference = mPHPRuntime.GetType("PHP.Reference");

			TypesAlreadyProcessed = new Hashtable();
		}

		public void Visit(AST ast) {
			// process each statement of php script recursively
			foreach (Statement stmt in ast.StmtList)
				Visit(stmt);
		}

		protected void Visit(ASTNode node) {
			if (node == null)
				return;
			else if (node is USING) {
				USING u = (USING)node;
				// add desired types
				SymbolTable.GetInstance().makeExternalTypeAvailable(u.Type, u.Alias);
			}
			else if (node is INTERFACE_DECLARATION) {
				INTERFACE_DECLARATION id = (INTERFACE_DECLARATION)node;
				// create type builder
				Type[] extends = new Type[id.Extends.Count];
				for (int i = 0; i < id.Extends.Count; i++) {
					string s = (string)id.Extends[i];
					extends[i] = (Type)TypesAlreadyProcessed[s];
				}
				TypeAttributes modifier = TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract;
				id.TypBld = PEmitter.ModBld.DefineType(id.Name, modifier, null, extends);
				TypesAlreadyProcessed.Add(id.Name, id.TypBld);
				// insert interface symbol (which is case insensitive)
				SymbolTable.GetInstance().InsertGlobal(id.Name, SymbolTable.INTERFACE, id);
			}
			else if (node is CLASS_DECLARATION) {
				CLASS_DECLARATION cd = (CLASS_DECLARATION)node;
				// ensure class doesn't extend a final class
				if (cd.Extends != null) {
					SymbolTableEntry parentCDEntry = SymbolTable.GetInstance().LookupGlobal(cd.Extends, SymbolTable.CLASS);
					CLASS_DECLARATION parentCD = (CLASS_DECLARATION)parentCDEntry.Node;
					if (parentCD.Modifier == Modifiers.FINAL)
						Report.Error(101, parentCD.Name, cd.Line, cd.Column);
				}
				// create type builder
				TypeAttributes modifier = TypeAttributes.Class;
				if (cd.Modifier == Modifiers.PUBLIC)
					modifier |= TypeAttributes.Public;
				else if (cd.Modifier == Modifiers.ABSTRACT)
					modifier |= TypeAttributes.Abstract;
				else if (cd.Modifier == Modifiers.FINAL)
					modifier |= TypeAttributes.Sealed;
				Type parent;
				if (cd.Extends == null || TypesAlreadyProcessed[cd.Extends] == null) {
					if (cd.Name == "__MAIN")
						parent = typeof(object);
					else
						parent = PHPObject;
				}
				else
					parent = (Type)TypesAlreadyProcessed[cd.Extends];
				cd.TypBld = PEmitter.ModBld.DefineType(cd.Name, modifier, parent);
				TypesAlreadyProcessed.Add(cd.Name, cd.TypBld);
				// insert class symbol (which is case insensitive)
				SymbolTable.GetInstance().InsertGlobal(cd.Name, SymbolTable.CLASS, cd);
			}
		}

	}


	// collects all class variables and functions so they can be used before being declared
	public class ClassVariableAndFunctionsVisitor : Visitor {

		public Assembly mPHPRuntime;
		public Type PHPRuntimeLang;
		public Type PHPRuntimeOperators;
		public Type PHPRuntimeConvert;
		public Type PHPRuntimeCore;
		public Type PHPArray;
		public Type PHPObject;
		public Type PHPReference;

		public CLASS_DECLARATION CD;
		public INTERFACE_DECLARATION ID;
		public FUNCTION_DECLARATION FD;
		public Hashtable TypesAlreadyProcessed;
		public MethodInfo MainMethod;

		public ClassVariableAndFunctionsVisitor() {
			mPHPRuntime = Assembly.LoadFrom("mPHPRuntime.dll");
			PHPRuntimeLang = mPHPRuntime.GetType("PHP.Runtime.Lang");
			PHPRuntimeOperators = mPHPRuntime.GetType("PHP.Runtime.Operators");
			PHPRuntimeConvert = mPHPRuntime.GetType("PHP.Runtime.Convert");
			PHPRuntimeCore = mPHPRuntime.GetType("PHP.Runtime.Core");
			PHPArray = mPHPRuntime.GetType("PHP.Array");
			PHPObject = mPHPRuntime.GetType("PHP.Object");
			PHPReference = mPHPRuntime.GetType("PHP.Reference");

			TypesAlreadyProcessed = new Hashtable();
		}

		public void Visit(AST ast) {
			// process each statement of php script recursively
			foreach (Statement stmt in ast.StmtList)
				Visit(stmt);
			// set entry point at __MAIN.__MAIN()
			if (PEmitter.Target == PEmitter.EXECUTABLE)
				PEmitter.AsmBld.SetEntryPoint(MainMethod, PEFileKinds.ConsoleApplication);
		}

		protected void Visit(ASTNode node) {
			if (node == null)
				return;
			else if (node is INTERFACE_DECLARATION) {
				ID = (INTERFACE_DECLARATION)node;
				// process each statement within interface
				SymbolTable.GetInstance().openScope();
				ID.Scope = SymbolTable.GetInstance().CurScope;
				foreach (Statement stmt in ID.StmtList)
					Visit(stmt);
				SymbolTable.GetInstance().CloseScope();
				ID = null;
			}
			else if (node is CLASS_DECLARATION) {
				CD = (CLASS_DECLARATION)node;
				// if class implemens interfaces, ensure all interface methods are implemented and ensure all interface methods are named differently
				if (CD.Implements.Count > 0) {
					// collect all methods of class and parent classes
					ArrayList methods = new ArrayList();
					foreach (Statement stmt in CD.StmtList) {
						if (stmt is FUNCTION_DECLARATION) {
							FUNCTION_DECLARATION tmpFD = (FUNCTION_DECLARATION)stmt;
							methods.Add(tmpFD.Name);
						}
					}
					CLASS_DECLARATION tmpCD = CD;
					while (tmpCD.Extends != null) {
						SymbolTableEntry parentCDEntry = SymbolTable.GetInstance().LookupGlobal(tmpCD.Extends, SymbolTable.CLASS);
						tmpCD = (CLASS_DECLARATION)parentCDEntry.Node;
						foreach (Statement stmt in tmpCD.StmtList) {
							if (stmt is FUNCTION_DECLARATION) {
								FUNCTION_DECLARATION tmpFD = (FUNCTION_DECLARATION)stmt;
								methods.Add(tmpFD.Name);
							}
						}
					}
					// iterate over all implemented interfaces
					ArrayList interfacesToProcess = new ArrayList();
					interfacesToProcess.AddRange(CD.Implements);
					ArrayList interfacesFunctions = new ArrayList();
					INTERFACE_DECLARATION tmpID = ID;
					while (interfacesToProcess.Count > 0) {
						string s = (string)(interfacesToProcess[0]);
						SymbolTableEntry interfaceEntry = SymbolTable.GetInstance().LookupGlobal(s, SymbolTable.INTERFACE);
						tmpID = (INTERFACE_DECLARATION)interfaceEntry.Node;
						foreach (Statement stmt in tmpID.StmtList)
							if (stmt is FUNCTION_DECLARATION) {
								FUNCTION_DECLARATION tmpFD = (FUNCTION_DECLARATION)stmt;
								if (!methods.Contains(tmpFD.Name))
									Report.Error(116, CD.Name, CD.Line, CD.Column);
								if (interfacesFunctions.Contains(tmpFD.Name))
									Report.Error(117, tmpFD.Name, tmpFD.Line, tmpFD.Column);
								interfacesFunctions.Add(tmpFD.Name);
							}
						interfacesToProcess.RemoveAt(0);
						if (tmpID.Extends != null)
							interfacesToProcess.AddRange(tmpID.Extends);
					}
				}
				// if this is a concrete class (not abstract)
				if (CD.Modifier != Modifiers.ABSTRACT) {
					// ensure it doesn't define abstract methods
					ArrayList concreteFunctions = new ArrayList();
					foreach (Statement stmt in CD.StmtList) {
						if (stmt is FUNCTION_DECLARATION) {
							FD = (FUNCTION_DECLARATION)stmt;
							if (FD.Modifiers.Contains(Modifiers.ABSTRACT))
								Report.Error(106, CD.Name, CD.Line, CD.Column);
							else
								concreteFunctions.Add(FD.Name);
						}
					}
					FD = null;
					// and ensure all inherited abstract methods are overridden
					ArrayList inheritedAbstractFunctions = new ArrayList();
					ArrayList inheritedConcreteFunctions = new ArrayList();
					CLASS_DECLARATION tmpCD = CD;
					while (tmpCD.Extends != null) {
						SymbolTableEntry parentCDEntry = SymbolTable.GetInstance().LookupGlobal(tmpCD.Extends, SymbolTable.CLASS);
						tmpCD = (CLASS_DECLARATION)parentCDEntry.Node;
						foreach (Statement stmt in tmpCD.StmtList) {
							if (stmt is FUNCTION_DECLARATION) {
								FD = (FUNCTION_DECLARATION)stmt;
								if (!inheritedConcreteFunctions.Contains(FD.Name)) {
									if (FD.Modifiers.Contains(Modifiers.ABSTRACT))
										inheritedAbstractFunctions.Add(FD.Name);
									else
										inheritedConcreteFunctions.Add(FD.Name);
								}
							}
						}
					}
					FD = null;
					foreach (string s in inheritedAbstractFunctions)
						if (!concreteFunctions.Contains(s))
							Report.Error(109, CD.Name, CD.Line, CD.Column);
				}
				// process each statement within class
				SymbolTable.GetInstance().openScope();
				CD.Scope = SymbolTable.GetInstance().CurScope;
				foreach (Statement stmt in CD.StmtList)
					Visit(stmt);
				SymbolTable.GetInstance().CloseScope();
				CD = null;
			}
			else if (node is CLASS_VARIABLE_DECLARATION) {
				CLASS_VARIABLE_DECLARATION cvd = (CLASS_VARIABLE_DECLARATION)node;
				// ensure no class variable is defined in an interface
				if (ID != null)
					Report.Error(114, cvd.Line, cvd.Column);
				// create field builders
				FieldAttributes modifiers = 0;
				if (cvd.Modifiers.Count == 0)
					modifiers = FieldAttributes.Public | FieldAttributes.Static;
				else if (cvd.Modifiers.Contains(Modifiers.CONST))
					modifiers = FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly;
				else if (!cvd.Modifiers.Contains(Modifiers.PUBLIC) && !cvd.Modifiers.Contains(Modifiers.PROTECTED) && !cvd.Modifiers.Contains(Modifiers.PRIVATE))
					modifiers = FieldAttributes.Public;
				else {
					ArrayList tmpModifiers = (ArrayList)cvd.Modifiers.Clone();
					if (cvd.Modifiers.Contains(Modifiers.PUBLIC)) {
						modifiers = FieldAttributes.Public;
						tmpModifiers.Remove(Modifiers.PUBLIC);
					}
					else if (cvd.Modifiers.Contains(Modifiers.PROTECTED)) {
						modifiers = FieldAttributes.Family;
						tmpModifiers.Remove(Modifiers.PROTECTED);
					}
					else if (cvd.Modifiers.Contains(Modifiers.PRIVATE)) {
						modifiers = FieldAttributes.Private;
						tmpModifiers.Remove(Modifiers.PRIVATE);
					}
					if (tmpModifiers.Contains(Modifiers.PUBLIC) || tmpModifiers.Contains(Modifiers.PROTECTED) || tmpModifiers.Contains(Modifiers.PRIVATE))
						Report.Error(105, cvd.Line, cvd.Column);
				}
				foreach (int modifier in cvd.Modifiers) {
					switch (modifier) {
						case Modifiers.STATIC: modifiers |= FieldAttributes.Static; break;
						case Modifiers.ABSTRACT: Report.Error(103, "abstract", cvd.Line, cvd.Column); break;
						case Modifiers.FINAL: Report.Error(103, "final", cvd.Line, cvd.Column); break;
					}
				}
				cvd.FieldBuilders = new ArrayList();
				for (int i = 0; i < cvd.Names.Count; i++) {
					string name = (string)cvd.Names[i];
					// remove $ at beginning of class member names
					if (name.StartsWith("$")) {
						name = name.Remove(0, 1);
						cvd.Names[i] = name;
					}
					// we're inside an interface
					if (ID != null)
						cvd.FieldBuilders.Add(ID.TypBld.DefineField(name, typeof(object), modifiers));
					// we're inside an class
					else
						cvd.FieldBuilders.Add(CD.TypBld.DefineField(name, typeof(object), modifiers));
					// insert member symbol
					SymbolTable.GetInstance().InsertLocal(name, SymbolTable.CLASS_VARIABLE, cvd);
				}
			}
			else if (node is FUNCTION_DECLARATION) {
				FD = (FUNCTION_DECLARATION)node;
				// we're inside an interface
				if (ID != null) {
					// ensure an interface function doesn't have forbidden modifiers
					if (FD.Modifiers.Contains(Modifiers.PROTECTED))
						Report.Error(111, "protected", FD.Line, FD.Column);
					else if (FD.Modifiers.Contains(Modifiers.PRIVATE))
						Report.Error(111, "private", FD.Line, FD.Column);
					else if (FD.Modifiers.Contains(Modifiers.ABSTRACT))
						Report.Error(111, "abstract", FD.Line, FD.Column);
					else if (FD.Modifiers.Contains(Modifiers.FINAL))
						Report.Error(111, "final", FD.Line, FD.Column);
					if ((FD.Name == "__construct" || FD.Name == "__constructStatic") && FD.Modifiers.Contains(Modifiers.STATIC))
						Report.Error(104, "static", FD.Line, FD.Column);
					// ensure an interface function doesn't override a function at all
					INTERFACE_DECLARATION tmpID;
					ArrayList parentIDs = ID.Extends;
					while (parentIDs.Count > 0) {
						ArrayList newParentIDs = new ArrayList();
						foreach (string s in parentIDs) {
							SymbolTableEntry parentIDEntry = SymbolTable.GetInstance().LookupGlobal(s, SymbolTable.INTERFACE);
							tmpID = (INTERFACE_DECLARATION)parentIDEntry.Node;
							SymbolTableEntry superFDEntry = tmpID.Scope.Lookup(FD.Name, SymbolTable.FUNCTION);
							if (superFDEntry != null) {
								Report.Error(110, FD.Name, FD.Line, FD.Column);
								break;
							}
							newParentIDs.AddRange(tmpID.Extends);
						}
						parentIDs = newParentIDs;
					}
					// ensure an interface function doesn't have a function body
					if (FD.StmtList != null)
						Report.Error(112, FD.Name, FD.Line, FD.Column);
					// create constructor and method builders
					MethodAttributes modifiers = MethodAttributes.Public | MethodAttributes.Abstract | MethodAttributes.Virtual;
					if (FD.Modifiers.Contains(Modifiers.STATIC) || FD.Name == "__constructStatic")
						modifiers |= MethodAttributes.Static;
					Type[] parameterTypes = new Type[FD.Parameters.Count];
					for (int i = 0; i < parameterTypes.Length; i++) {
						PARAMETER_DECLARATION pd = (PARAMETER_DECLARATION)FD.Parameters[i];
						// if a reference is desired, use PHP.Reference as type
						if (pd.ByReference)
							parameterTypes[i] = PHPReference;
						// otherwise and if no type hint is available, use object as type
						else if (pd.Type == null)
							parameterTypes[i] = typeof(object);
						// otherwise use desired type as type
						else {
							SymbolTableEntry typeEntry = SymbolTable.GetInstance().LookupGlobal(pd.Type, SymbolTable.CLASS);
							// type is defined in script
							if (typeEntry != null) {
								if (typeEntry.Node is CLASS_DECLARATION)
									parameterTypes[i] = ((CLASS_DECLARATION)typeEntry.Node).TypBld;
								else
									parameterTypes[i] = ((INTERFACE_DECLARATION)typeEntry.Node).TypBld;
							}
							// type is an external type
							else {
								Type t = SymbolTable.GetInstance().GetExternalType(pd.Type);
								if (t != null)
									parameterTypes[i] = t;
								else
									Report.Error(203, pd.Type, pd.Line, pd.Column);
							}
						}
					}
					Type returnType = typeof(object);
					if (FD.Name == "__construct" || FD.Name == "__constructStatic")
						FD.CtrBld = ID.TypBld.DefineConstructor(modifiers, CallingConventions.Standard, parameterTypes);
					else
						FD.MthBld = ID.TypBld.DefineMethod(FD.Name, modifiers, returnType, parameterTypes);
					// insert function symbol (which is case insensitive)
					SymbolTable.GetInstance().InsertLocal(FD.Name, SymbolTable.FUNCTION, FD);
					// remember scope
					SymbolTable.GetInstance().openScope();
					FD.Scope = SymbolTable.GetInstance().CurScope;
					SymbolTable.GetInstance().CloseScope();
					FD = null;
				}
				// we're inside an class
				else {
					// ensure an abstract function doesn't have a function body
					if (FD.Modifiers.Contains(Modifiers.ABSTRACT) && FD.StmtList != null)
						Report.Error(113, FD.Name, FD.Line, FD.Column);
					// ensure a concrete function does have a function body
					if (!FD.Modifiers.Contains(Modifiers.ABSTRACT) && FD.StmtList == null)
						Report.Error(115, FD.Name, FD.Line, FD.Column);
					// if there is a overridden parent function
					CLASS_DECLARATION tmpCD = CD;
					while (tmpCD.Extends != null) {
						SymbolTableEntry parentCDEntry = SymbolTable.GetInstance().LookupGlobal(tmpCD.Extends, SymbolTable.CLASS);
						tmpCD = (CLASS_DECLARATION)parentCDEntry.Node;
						SymbolTableEntry superFDEntry = tmpCD.Scope.Lookup(FD.Name, SymbolTable.FUNCTION);
						if (superFDEntry != null) {
							FUNCTION_DECLARATION superFD = (FUNCTION_DECLARATION)superFDEntry.Node;
							// ensure function doesn't override a final function
							if (superFD.Modifiers.Contains(Modifiers.FINAL)) {
								StringBuilder parameters = new StringBuilder();
								foreach (PARAMETER_DECLARATION pd in superFD.Parameters) {
									if (pd.Type != null) {
										parameters.Append(pd.Type);
										parameters.Append(" ");
									}
									parameters.Append(pd.Name);
									parameters.Append(", ");
								}
								if (parameters.Length > 0)
									parameters.Remove(parameters.Length - 2, 2);
								Report.Error(102, tmpCD.Name + "::" + superFD.Name + "(" + parameters.ToString() + ")", FD.Line, FD.Column);
							}
							// ensure an abstract function doesn't override a function at all
							else if (FD.Modifiers.Contains(Modifiers.ABSTRACT)) {
								Report.Error(107, FD.Name, FD.Line, FD.Column);
							}
							// ensure visibility is same or weaker visibility of parent function
							else {
								if (superFD.Modifiers.Contains(Modifiers.PUBLIC) && !FD.Modifiers.Contains(Modifiers.PUBLIC))
									Report.Error(108, FD.Name, FD.Line, FD.Column);
								else if (superFD.Modifiers.Contains(Modifiers.PROTECTED) && FD.Modifiers.Contains(Modifiers.PRIVATE))
									Report.Error(108, FD.Name, FD.Line, FD.Column);
							}
							break;
						}
					}
					// create constructor and method builders
					MethodAttributes modifiers = 0;
					if (FD.Modifiers.Count == 0)
						modifiers = MethodAttributes.Public;
					else if (!FD.Modifiers.Contains(Modifiers.PUBLIC) && !FD.Modifiers.Contains(Modifiers.PROTECTED) && !FD.Modifiers.Contains(Modifiers.PRIVATE))
						modifiers = MethodAttributes.Public;
					else {
						ArrayList tmpModifiers = (ArrayList)FD.Modifiers.Clone();
						if (FD.Modifiers.Contains(Modifiers.PUBLIC)) {
							modifiers = MethodAttributes.Public;
							tmpModifiers.Remove(Modifiers.PUBLIC);
						}
						else if (FD.Modifiers.Contains(Modifiers.PROTECTED)) {
							modifiers = MethodAttributes.Family;
							tmpModifiers.Remove(Modifiers.PROTECTED);
						}
						else if (FD.Modifiers.Contains(Modifiers.PRIVATE)) {
							modifiers = MethodAttributes.Private;
							tmpModifiers.Remove(Modifiers.PRIVATE);
						}
						if (tmpModifiers.Contains(Modifiers.PUBLIC) || tmpModifiers.Contains(Modifiers.PROTECTED) || tmpModifiers.Contains(Modifiers.PRIVATE))
							Report.Error(105, FD.Line, FD.Column);
					}
					foreach (int modifier in FD.Modifiers) {
						if (FD.Name == "__construct")
							switch (modifier) {
								case Modifiers.STATIC: Report.Error(104, "static", FD.Line, FD.Column); break;
								case Modifiers.ABSTRACT: Report.Error(104, "abstract", FD.Line, FD.Column); break;
								case Modifiers.FINAL: modifiers |= MethodAttributes.Final; break;
							}
						else if (FD.Name == "__constructStatic") {
							modifiers |= MethodAttributes.Static;
							switch (modifier) {
								case Modifiers.ABSTRACT: Report.Error(104, "abstract", FD.Line, FD.Column); break;
								case Modifiers.FINAL: modifiers |= MethodAttributes.Final; break;
							}
						}
						else
							switch (modifier) {
								case Modifiers.STATIC: modifiers |= MethodAttributes.Static; break;
								case Modifiers.ABSTRACT: modifiers |= MethodAttributes.Abstract; break;
								case Modifiers.FINAL: modifiers |= MethodAttributes.Final; break;
							}
					}
					Type[] parameterTypes = new Type[FD.Parameters.Count];
					for (int i = 0; i < parameterTypes.Length; i++) {
						PARAMETER_DECLARATION pd = (PARAMETER_DECLARATION)FD.Parameters[i];
						// if a reference is desired, use PHP.Reference as type
						if (pd.ByReference)
							parameterTypes[i] = PHPReference;
						// otherwise and if no type hint is available, use object as type
						else if (pd.Type == null)
							parameterTypes[i] = typeof(object);
						// otherwise use desired type as type
						else {
							SymbolTableEntry typeEntry = SymbolTable.GetInstance().LookupGlobal(pd.Type, SymbolTable.CLASS);
							// type is defined in script
							if (typeEntry != null) {
								if (typeEntry.Node is CLASS_DECLARATION)
									parameterTypes[i] = ((CLASS_DECLARATION)typeEntry.Node).TypBld;
								else
									parameterTypes[i] = ((INTERFACE_DECLARATION)typeEntry.Node).TypBld;
							}
							// type is an external type
							else {
								Type t = SymbolTable.GetInstance().GetExternalType(pd.Type);
								if (t != null)
									parameterTypes[i] = t;
								else
									Report.Error(203, pd.Type, pd.Line, pd.Column);
							}
						}
					}
					Type returnType;
					if (FD.Name == "__MAIN")
						returnType = typeof(void);
					else if (FD.ReturnByReference)
						returnType = PHPReference;
					else
						returnType = typeof(object);
					if (FD.Name == "__construct" || FD.Name == "__constructStatic")
						FD.CtrBld = CD.TypBld.DefineConstructor(modifiers, CallingConventions.Standard, parameterTypes);
					else
						FD.MthBld = CD.TypBld.DefineMethod(FD.Name, modifiers, returnType, parameterTypes);
					// at beginning of script
					if (FD.Name == "__MAIN") {
						MainMethod = FD.MthBld;
						// disable warnings, if desired
						if (!Report.WarningsEnabled) {
							mPHPRuntime = Assembly.LoadFrom("mPHPRuntime.dll");
							Type report = mPHPRuntime.GetType("PHP.Report");
							FD.MthBld.GetILGenerator().Emit(OpCodes.Ldc_I4_0);
							FD.MthBld.GetILGenerator().Emit(OpCodes.Stsfld, report.GetField("WarningsEnabled"));
						}
						// initialize local settings to en-US
						FD.MthBld.GetILGenerator().Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("Init", Type.EmptyTypes));
					}
					// insert function symbol (which is case insensitive)
					SymbolTable.GetInstance().InsertLocal(FD.Name, SymbolTable.FUNCTION, FD);
					// remember scope
					SymbolTable.GetInstance().openScope();
					FD.Scope = SymbolTable.GetInstance().CurScope;
					SymbolTable.GetInstance().CloseScope();
					FD = null;
				}
			}
		}

	}


	// transform AST to create class __MAIN and function __MAIN()
	public class MainMethodVisitor : Visitor {

		public CLASS_DECLARATION CD__MAIN;
		public FUNCTION_DECLARATION FD__MAIN;
		public StatementList ModifiedStmtList;

		public MainMethodVisitor() {
			// create public class __MAIN
			CD__MAIN = new CLASS_DECLARATION(Modifiers.PUBLIC, "__MAIN", null, new ArrayList(), new StatementList(), 0, 0);
			// create public function __MAIN()
			ArrayList modifiers = new ArrayList();
			modifiers.Add(Modifiers.PUBLIC);
			modifiers.Add(Modifiers.STATIC);
			FD__MAIN = new FUNCTION_DECLARATION(modifiers, false, "__MAIN", new ArrayList(), new StatementList(), 0, 0);
			// add function __MAIN() to class __MAIN
			CD__MAIN.StmtList.Add(FD__MAIN);
			// create modified statement list
			ModifiedStmtList = new StatementList();
		}

		public void Visit(AST ast) {
			// put class __MAIN at beginning of modified statement list
			ModifiedStmtList.Add(CD__MAIN);
			// leave top class declarations, move top function declarations to class __MAIN, move everything else to function __MAIN()
			foreach (Statement stmt in ast.StmtList)
				Visit(stmt);
			// replace AST's old statement list by modified one
			ast.StmtList = ModifiedStmtList;
		}

		protected void Visit(ASTNode node) {
			if (node == null)
				return;
			else if (node is USING)
				ModifiedStmtList.Add((Statement)node);
			else if (node is CLASS_DECLARATION)
				ModifiedStmtList.Add((Statement)node);
			else if (node is INTERFACE_DECLARATION)
				ModifiedStmtList.Add((Statement)node);
			else if (node is FUNCTION_DECLARATION) {
				FUNCTION_DECLARATION fd = (FUNCTION_DECLARATION)node;
				ArrayList modifiers = new ArrayList();
				modifiers.Add(Modifiers.PUBLIC);
				modifiers.Add(Modifiers.STATIC);
				fd.Modifiers = modifiers;
				CD__MAIN.StmtList.Add(fd);
			}
			else
				FD__MAIN.StmtList.Add((Statement)node);
		}

	}


	// reorder class declarations by inheritance
	public class InheritanceVisitor : Visitor {

		public Assembly mPHPRuntime;

		public InheritanceVisitor() {
			mPHPRuntime = Assembly.LoadFrom("mPHPRuntime.dll");
		}

		public void Visit(AST ast) {
			StatementList originalInterfaceDeclarations = new StatementList();
			StatementList originalClassDeclarations = new StatementList();
			StatementList reorderedInterfaceDeclarations = new StatementList();
			StatementList reorderedClassDeclarations = new StatementList();
			// move interface declarations from ast.StmtList to originalInterfaceDeclarations
			for (int i = 0; i < ast.StmtList.Count(); i++) {
				Statement stmt = ast.StmtList.Get(i);
				if (stmt is INTERFACE_DECLARATION) {
					ast.StmtList.Remove(stmt);
					i--;
					originalInterfaceDeclarations.Add(stmt);
				}
			}
			// move class declarations from ast.StmtList to originalClassDeclarations
			for (int i = 0; i < ast.StmtList.Count(); i++) {
				Statement stmt = ast.StmtList.Get(i);
				if (stmt is CLASS_DECLARATION) {
					ast.StmtList.Remove(stmt);
					i--;
					originalClassDeclarations.Add(stmt);
				}
			}

			// reorder interface declarations
			ArrayList parentsAvailable = new ArrayList();
			ArrayList newParents = new ArrayList();
			// start with interfaces with no parents specified and wich have an external type as parent
			for (int i = 0; i < originalInterfaceDeclarations.Count(); i++) {
				INTERFACE_DECLARATION id = (INTERFACE_DECLARATION)originalInterfaceDeclarations.Get(i);
				// no parents specified
				if (id.Extends == null) {
					originalInterfaceDeclarations.Remove(id);
					i--;
					reorderedInterfaceDeclarations.Add(id);
					newParents.Add(id.Name);
				}
				// external parents specified
				else {
					bool onlyExternalParents = true;
					foreach (string type in id.Extends) {
						Type t = mPHPRuntime.GetType(type);
						if (t == null) {
							onlyExternalParents = false;
							break;
						}
					}
					if (onlyExternalParents) {
						originalInterfaceDeclarations.Remove(id);
						i--;
						reorderedInterfaceDeclarations.Add(id);
						newParents.Add(id.Name);
					}
				}
			}
			parentsAvailable.AddRange(newParents);
			while (originalInterfaceDeclarations.Count() > 0) {
				// in each iteration, add those interface delcarations which inherit from interfaces processed in a previous iteration
				newParents = new ArrayList();
				for (int i = 0; i < originalInterfaceDeclarations.Count(); i++) {
					INTERFACE_DECLARATION id = (INTERFACE_DECLARATION)originalInterfaceDeclarations.Get(i);
					bool containsAll = true;
					foreach (string s in id.Extends)
						if (!parentsAvailable.Contains(s)) {
							containsAll = false;
							break;
						}
					if (containsAll) {
						originalInterfaceDeclarations.Remove(id);
						i--;
						reorderedInterfaceDeclarations.Add(id);
						newParents.Add(id.Name);
					}
				}
				// if there are still classes left, but no new parents were added, a parent wasn't declared or there is a cycle in inheritance
				if (originalInterfaceDeclarations.Count() > 0 && newParents.Count == 0)
					Report.Error(100);
				parentsAvailable.AddRange(newParents);
			}
			// reorder class declarations
			parentsAvailable = new ArrayList();
			newParents = new ArrayList();
			// start with classes with no parent specified and wich have an external type as parent
			for (int i = 0; i < originalClassDeclarations.Count(); i++) {
				CLASS_DECLARATION cd = (CLASS_DECLARATION)originalClassDeclarations.Get(i);
				// no parent specified
				if (cd.Extends == null) {
					originalClassDeclarations.Remove(cd);
					i--;
					reorderedClassDeclarations.Add(cd);
					newParents.Add(cd.Name);
				}
				// external parent specified
				else {
					Type t = mPHPRuntime.GetType(cd.Extends);
					if (t != null) {
						originalClassDeclarations.Remove(cd);
						i--;
						reorderedClassDeclarations.Add(cd);
						newParents.Add(cd.Name);
					}
				}
			}
			parentsAvailable.AddRange(newParents);
			while (originalClassDeclarations.Count() > 0) {
				// in each iteration, add those class delcarations which inherit from classes processed in a previous iteration
				newParents = new ArrayList();
				for (int i = 0; i < originalClassDeclarations.Count(); i++) {
					CLASS_DECLARATION cd = (CLASS_DECLARATION)originalClassDeclarations.Get(i);
					if (parentsAvailable.Contains(cd.Extends)) {
						originalClassDeclarations.Remove(cd);
						i--;
						reorderedClassDeclarations.Add(cd);
						newParents.Add(cd.Name);
					}
				}
				// if there are still classes left, but no new parents were added, a parent wasn't declared or there is a cycle in inheritance
				if (originalClassDeclarations.Count() > 0 && newParents.Count == 0)
					Report.Error(100);
				parentsAvailable.AddRange(newParents);
			}

			// add reordered interface and class declarations to ast.StmtList
			ast.StmtList.AddRange(reorderedInterfaceDeclarations);
			ast.StmtList.AddRange(reorderedClassDeclarations);
		}

	}


	// ensure every method has a return statement and cut unreachable statements after a return
	public class ReturnStatementVisitor : Visitor {

		public ReturnStatementVisitor() { }

		public void Visit(AST ast) {
			// process every statement
			foreach (Statement stmt in ast.StmtList)
				Visit(stmt);
		}

		protected void Visit(ASTNode node) {
			if (node == null)
				return;
			else if (node is CLASS_DECLARATION) {
				CLASS_DECLARATION cd = (CLASS_DECLARATION)node;
				foreach (Statement stmt in cd.StmtList)
					Visit(stmt);
			}
			else if (node is FUNCTION_DECLARATION) {
				FUNCTION_DECLARATION fd = (FUNCTION_DECLARATION)node;
				bool returnReached = false;
				StatementList newStmtList = new StatementList();
				// copy each statement to newStmtList until top return is reached
				if (fd.StmtList != null) {
					foreach (Statement stmt in fd.StmtList) {
						if (stmt is RETURN) {
							newStmtList.Add(stmt);
							returnReached = true;
							break;
						}
						else
							newStmtList.Add(stmt);
					}
					// if there was no return statement, add one
					if (!returnReached)
						newStmtList.Add(new RETURN(null, 0, 0));
					// replace statement list of function with new one
					fd.StmtList = newStmtList;
				}
			}
		}

	}


	// ensure there is no break/continue without a loop
	public class LoopVisitor : Visitor {

		public int Level;

		public LoopVisitor() {
			Level = 0;
		}

		public void Visit(AST ast) {
			// process every statement
			foreach (Statement stmt in ast.StmtList)
				Visit(stmt);
		}

		protected void Visit(ASTNode node) {
			if (node == null)
				return;
			else if (node is CLASS_DECLARATION) {
				CLASS_DECLARATION cd = (CLASS_DECLARATION)node;
				foreach (Statement stmt in cd.StmtList)
					Visit(stmt);
			}
			else if (node is FUNCTION_DECLARATION) {
				FUNCTION_DECLARATION fd = (FUNCTION_DECLARATION)node;
				if (fd.StmtList != null)
					foreach (Statement stmt in fd.StmtList)
						Visit(stmt);
			}
			else if (node is BLOCK) {
				BLOCK b = (BLOCK)node;
				foreach (Statement stmt in b.StmtList)
					Visit(stmt);
			}
			else if (node is IF) {
				IF i = (IF)node;
				Visit(i.Stmt);
			}
			else if (node is ELSEIF) {
				ELSEIF e = (ELSEIF)node;
				Visit(e.Stmt);
			}
			else if (node is WHILE) {
				WHILE w = (WHILE)node;
				Level++;
				Visit(w.Stmt);
				Level--;
			}
			else if (node is DO) {
				DO d = (DO)node;
				Level++;
				Visit(d.Stmt);
				Level--;
			}
			else if (node is FOR) {
				FOR f = (FOR)node;
				Level++;
				Visit(f.Stmt);
				Level--;
			}
			else if (node is FOREACH) {
				FOREACH f = (FOREACH)node;
				Level++;
				Visit(f.Stmt);
				Level--;
			}
			else if (node is SWITCH) {
				SWITCH s = (SWITCH)node;
				Level++;
				foreach (ASTNode node2 in s.SwitchCaseList) {
					if (node2 is CASE)
						Visit((CASE)node2);
					else if (node2 is DEFAULT)
						Visit((DEFAULT)node2);
				}
				Level--;
			}
			else if (node is CASE) {
				CASE c = (CASE)node;
				Visit(c.Stmt);
			}
			else if (node is DEFAULT) {
				DEFAULT d = (DEFAULT)node;
				Visit(d.Stmt);
			}
			else if (node is BREAK) {
				if (Level == 0)
					Report.Error(501, node.Line, node.Column);
			}
			else if (node is CONTINUE) {
				if (Level == 0)
					Report.Error(501, node.Line, node.Column);
			}
		}

	}

	// ensure every class (except __MAIN) has a constructor
	public class ConstructorVisitor : Visitor {

		public bool ConstructorFound;

		public ConstructorVisitor() { }

		public void Visit(AST ast) {
			// process every statement
			foreach (Statement stmt in ast.StmtList)
				Visit(stmt);
		}

		protected void Visit(ASTNode node) {
			if (node == null)
				return;
			else if (node is CLASS_DECLARATION) {
				CLASS_DECLARATION cd = (CLASS_DECLARATION)node;
				if (cd.Name == "__MAIN")
					return;
				ConstructorFound = false;
				foreach (Statement stmt in cd.StmtList)
					Visit(stmt);
				// if no user defined constructor is found, add one
				ArrayList modifiers, parameters;
				StatementList StmtList;
				Statement ctorDecl;
				if (!ConstructorFound) {
					modifiers = new ArrayList();
					modifiers.Add(Modifiers.PUBLIC);
					parameters = new ArrayList();
					StmtList = new StatementList();
					ctorDecl = new FUNCTION_DECLARATION(modifiers, false, "__construct", parameters, StmtList, 0, 0);
					cd.StmtList.Add(ctorDecl);
				}
				// in every case, add a static constructor
				modifiers = new ArrayList();
				modifiers.Add(Modifiers.PUBLIC);
				modifiers.Add(Modifiers.STATIC);
				parameters = new ArrayList();
				StmtList = new StatementList();
				ctorDecl = new FUNCTION_DECLARATION(modifiers, false, "__constructStatic", parameters, StmtList, 0, 0);
				cd.StmtList.Add(ctorDecl);
			}
			else if (node is FUNCTION_DECLARATION) {
				FUNCTION_DECLARATION fd = (FUNCTION_DECLARATION)node;
				if (fd.Name == "__construct")
					ConstructorFound = true;
			}
		}

	}


	// emit CIL code
	public class EmitterVisitor : Visitor {

		public Assembly mPHPRuntime;
		public Type PHPRuntimeLang;
		public Type PHPRuntimeOperators;
		public Type PHPRuntimeConvert;
		public Type PHPRuntimeCore;
		public Type PHPArray;
		public Type PHPObject;
		public Type PHPReference;


		public Hashtable predefinedFunctions;  // predefined functions and number of parameters

		public INTERFACE_DECLARATION ID;
		public CLASS_DECLARATION CD;
		public FUNCTION_DECLARATION FD;
		public CLASS_DECLARATION CD__MAIN;
		public FUNCTION_DECLARATION FD__MAIN;
		public ILGenerator IlGen;

		public bool ObjectOperatorInProgress;  // used for OBJECT_OPERATOR
		public bool ProcessingObjectThisWhenLoading;  // used for OBJECT_OPERATOR
		public bool ProcessingObjectThisWhenStoring;  // used for OBJECT_OPERATOR
		public ArrayList EnterLoopLabels, ExitLoopLabels;  // used for FOR, FOREACH, WHILE, DO and SWITCH

		public EmitterVisitor() {
			mPHPRuntime = Assembly.LoadFrom("mPHPRuntime.dll");
			PHPRuntimeLang = mPHPRuntime.GetType("PHP.Runtime.Lang");
			PHPRuntimeOperators = mPHPRuntime.GetType("PHP.Runtime.Operators");
			PHPRuntimeConvert = mPHPRuntime.GetType("PHP.Runtime.Convert");
			PHPRuntimeCore = mPHPRuntime.GetType("PHP.Runtime.Core");
			PHPArray = mPHPRuntime.GetType("PHP.Array");
			PHPObject = mPHPRuntime.GetType("PHP.Object");
			PHPReference = mPHPRuntime.GetType("PHP.Reference");
			// collect predefined functions and number of parameters
			predefinedFunctions = new Hashtable();
			predefinedFunctions["define"] = 3;
			predefinedFunctions["key"] = 1;
			predefinedFunctions["current"] = 1;
			predefinedFunctions["next"] = 1;
			predefinedFunctions["prev"] = 1;
			predefinedFunctions["each"] = 1;
			predefinedFunctions["reset"] = 1;
			predefinedFunctions["add_event"] = 3;
			predefinedFunctions["remove_event"] = 3;
			ObjectOperatorInProgress = false;
			ProcessingObjectThisWhenLoading = false;
			ProcessingObjectThisWhenStoring = false;
			EnterLoopLabels = new ArrayList();
			ExitLoopLabels = new ArrayList();
		}

		public void Visit(AST ast) {
			// process each statement of php script recursively
			foreach (Statement stmt in ast.StmtList)
				Visit(stmt);
		}

		protected void Visit(ASTNode node) {
			if (node == null)
				return;
			else if (node is INTERFACE_DECLARATION) {
				ID = (INTERFACE_DECLARATION)node;
				// process each statement of interface
				foreach (Statement stmt in ID.StmtList)
					Visit(stmt);
				// bake the interface
				ID.Typ = ID.TypBld.CreateType();
				ID = null;
			}
			else if (node is CLASS_DECLARATION) {
				CD = (CLASS_DECLARATION)node;
				// remember __main class
				if (CD.Name == "__MAIN")
					CD__MAIN = CD;
				// process each statement of class
				foreach (Statement stmt in CD.StmtList)
					Visit(stmt);
				// bake the class
				CD.Typ = CD.TypBld.CreateType();
				CD = null;
			}
			else if (node is FUNCTION_DECLARATION) {
				FD = (FUNCTION_DECLARATION)node;
				// if function is abstract or an interface function, do nothing
				if (FD.StmtList == null)
					return;
				// remember __main method
				if (FD.Name == "__MAIN")
					FD__MAIN = FD;
				// get ILGenerator and check if this is a static function or not
				bool staticFunction;
				if (FD.Name == "__construct" || FD.Name == "__constructStatic") {
					IlGen = FD.CtrBld.GetILGenerator();
					staticFunction = FD.CtrBld.IsStatic;
				}
				else {
					IlGen = FD.MthBld.GetILGenerator();
					staticFunction = FD.MthBld.IsStatic;
				}
				// in constructors, store initial class member values
				if (FD.Name == "__construct" || FD.Name == "__constructStatic") {
					CLASS_DECLARATION tmpCD = CD;
					ArrayList namesAlreadyAdded = new ArrayList();
					do { // fetch class variable declarations of current class
						ArrayList classVarDecls = tmpCD.Scope.Lookup(SymbolTable.CLASS_VARIABLE);
						foreach (SymbolTableEntry cvdEntry in classVarDecls) {
							CLASS_VARIABLE_DECLARATION cvd = (CLASS_VARIABLE_DECLARATION)cvdEntry.Node;
							for (int i = 0; i < cvd.Names.Count; i++) {
								FieldBuilder fb = (FieldBuilder)cvd.FieldBuilders[i];
								string name = (string)cvd.Names[i];
								Expression value = cvd.Values.Get(i);
								// only process names not already added
								if (namesAlreadyAdded.Contains(name))
									continue;
								bool cvdIsVisible = tmpCD == CD || !cvd.Modifiers.Contains(Modifiers.PRIVATE);
								bool cvdIsStatic = cvd.Modifiers.Contains(Modifiers.STATIC) || cvd.Modifiers.Contains(Modifiers.CONST);
								// in a static constructor, store initial static class member values
								if (FD.Name == "__constructStatic" && cvdIsVisible && cvdIsStatic) {
									if (value == null)
										IlGen.Emit(OpCodes.Ldnull);
									else
										Visit(value);
									IlGen.Emit(OpCodes.Stsfld, fb);
									namesAlreadyAdded.Add(name);
								}
								// in a local constructor, store initial local class member values
								if (FD.Name == "__construct" && cvdIsVisible && !cvdIsStatic) {
									IlGen.Emit(OpCodes.Ldarg_0);
									if (value == null)
										IlGen.Emit(OpCodes.Ldnull);
									else
										Visit(value);
									IlGen.Emit(OpCodes.Stfld, fb);
									namesAlreadyAdded.Add(name);
								}
							}
						}
						// fetch parent class declaration
						if (tmpCD.Extends == null)
							tmpCD = null;
						else {
							SymbolTableEntry parentCDEntry = SymbolTable.GetInstance().LookupGlobal(tmpCD.Extends, SymbolTable.CLASS);
							tmpCD = (CLASS_DECLARATION)parentCDEntry.Node;
						}
					} while (tmpCD != null);

				}
				// store parameters passed as local variables
				PARAMETER_DECLARATION pd;
				for (int i = 0; i < FD.Parameters.Count; i++) {
					pd = (PARAMETER_DECLARATION)FD.Parameters[i];
					// push value passed
					IlGen.Emit(OpCodes.Ldarg, i + (staticFunction ? 0 : 1));
					// if value passed is null, use default value, if available
					if (pd.DefaultValue != null) {
						Label skip = IlGen.DefineLabel();
						IlGen.Emit(OpCodes.Dup);
						IlGen.Emit(OpCodes.Brtrue, skip);
						IlGen.Emit(OpCodes.Pop);
						Visit(pd.DefaultValue);
						IlGen.MarkLabel(skip);
					}
					// check if class type hint is ok
					if (pd.Type != null) {
						IlGen.Emit(OpCodes.Dup);
						SymbolTableEntry cdEntry = SymbolTable.GetInstance().LookupGlobal(pd.Type, SymbolTable.CLASS);
						// type is defined in an referenced assembly
						if (cdEntry == null) {
							Type t = SymbolTable.GetInstance().GetExternalType(pd.Type);
							IlGen.Emit(OpCodes.Ldstr, t.AssemblyQualifiedName);
						}
						// type is defined in script
						else
							IlGen.Emit(OpCodes.Ldstr, pd.Type);
						IlGen.Emit(OpCodes.Ldc_I4, i + (staticFunction ? 1 : 2));
						IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("CheckTypeHint", new Type[] { typeof(object), typeof(string), typeof(int) }));
					}
					// store
					StoreToVariable(pd.Name);
				}
				// process statements
				if (FD.StmtList.Count() == 0)
					IlGen.Emit(OpCodes.Nop);
				else
					foreach (Statement stmt in FD.StmtList)
						Visit(stmt);
				IlGen = null;
				FD = null;
			}
			else if (node is GLOBAL) {
				GLOBAL g = (GLOBAL)node;
				foreach (VARIABLE var in g.VarList) {
					IlGen.Emit(OpCodes.Ldstr, var.Name);
					IlGen.Emit(OpCodes.Call, PHPReference.GetMethod("CreateReferenceToGlobal", new Type[] { typeof(object) }));
					IlGen.Emit(OpCodes.Ldstr, var.Name);
					IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("StoreToVariable", new Type[] { typeof(object), typeof(object) }));
				}
			}
			else if (node is STATIC_DECLARATION) {
				STATIC_DECLARATION sd = (STATIC_DECLARATION)node;
				foreach (Expression expr in sd.ExprList) {
					if (expr is VARIABLE) {
						IlGen.Emit(OpCodes.Ldnull);
						StoreToStaticVariable(((VARIABLE)expr).Name);
					}
					if (expr is EQUALS) {
						Visit(((EQUALS)expr).Expr2);
						StoreToStaticVariable(((VARIABLE)((EQUALS)expr).Expr1).Name);
					}
				}
			}
			else if (node is BLOCK) {
				BLOCK b = (BLOCK)node;
				foreach (Statement stmt in b.StmtList)
					Visit(stmt);
			}
			else if (node is StatementList) {
				StatementList s = (StatementList)node;
				// process statements of block
				foreach (Statement stmt in s)
					Visit(stmt);
			}
			else if (node is TRY) {
				TRY t = (TRY)node;
				// begin try block
				IlGen.BeginExceptionBlock();
				// process statements of try block
				foreach (Statement stmt in t.StmtList)
					Visit(stmt);
				// process catch blocks
				foreach (CATCH c in t.Catches)
					Visit(c);
				// end last catch block
				IlGen.EndExceptionBlock();
			}
			else if (node is CATCH) {
				CATCH c = (CATCH)node;
				// retrieve type of exception to be caught
				Type t;
				SymbolTableEntry cdEntry = SymbolTable.GetInstance().LookupGlobal(c.Type, SymbolTable.CLASS);
				if (cdEntry == null)
					t = SymbolTable.GetInstance().GetExternalType(c.Type);
				else {
					CLASS_DECLARATION cd = (CLASS_DECLARATION)cdEntry.Node;
					t = cd.TypBld;
				}
				// end last block (which is try or catch) and start new catch block
				IlGen.BeginCatchBlock(t);
				// make the current exception available as variable under the desired name
				StoreToVariable(c.Variable);
				// process catch statements
				foreach (Statement stmt in c.StmtList)
					Visit(stmt);
			}
			else if (node is THROW) {
				THROW t = (THROW)node;
				// process throw expression
				Visit(t.Expr);
				// ensure thrown object is an exception
				IlGen.Emit(OpCodes.Call, PHPRuntimeConvert.GetMethod("ToException", new Type[] { typeof(object) }));
				IlGen.Emit(OpCodes.Throw);
			}
			else if (node is IF) {
				IF i = (IF)node;
				Label nextCheck = IlGen.DefineLabel();
				Label endIf = IlGen.DefineLabel();
				// process if statement
				Visit(i.Expr);
				IlGen.Emit(OpCodes.Call, PHPRuntimeConvert.GetMethod("ToBool", new Type[] { typeof(object) }));
				IlGen.Emit(OpCodes.Brfalse, nextCheck);
				Visit(i.Stmt);
				IlGen.Emit(OpCodes.Br, endIf);
				IlGen.MarkLabel(nextCheck);
				// process elseif statements
				foreach (ELSEIF e in i.ElseifList) {
					//Visit(e);
					ELSEIF ei = (ELSEIF)node;
					Label nextCheck2 = IlGen.DefineLabel();
					// process else statement
					Visit(ei.Expr);
					IlGen.Emit(OpCodes.Call, PHPRuntimeConvert.GetMethod("ToBool", new Type[] { typeof(object) }));
					IlGen.Emit(OpCodes.Brfalse, nextCheck2);
					Visit(ei.Stmt);
					IlGen.Emit(OpCodes.Br, endIf);
					IlGen.MarkLabel(nextCheck2);
				}
				// process else statement
				Visit(i.ElseStmt);
				// done
				IlGen.MarkLabel(endIf);
				endIf = IlGen.DefineLabel();
			}
			else if (node is WHILE) {
				WHILE w = (WHILE)node;
				Label enterLoop = IlGen.DefineLabel();
				Label exitLoop = IlGen.DefineLabel();
				EnterLoopLabels.Add(enterLoop);
				ExitLoopLabels.Add(exitLoop);
				IlGen.MarkLabel(enterLoop);
				Visit(w.Expr);
				IlGen.Emit(OpCodes.Call, PHPRuntimeConvert.GetMethod("ToBool", new Type[] { typeof(object) }));
				IlGen.Emit(OpCodes.Brfalse, exitLoop);
				Visit(w.Stmt);
				IlGen.Emit(OpCodes.Br, enterLoop);
				IlGen.MarkLabel(exitLoop);
				EnterLoopLabels.RemoveAt(EnterLoopLabels.Count - 1);
				ExitLoopLabels.RemoveAt(ExitLoopLabels.Count - 1);
			}
			else if (node is DO) {
				DO d = (DO)node;
				Label enterLoop = IlGen.DefineLabel();
				Label exitLoop = IlGen.DefineLabel();
				EnterLoopLabels.Add(enterLoop);
				ExitLoopLabels.Add(exitLoop);
				IlGen.MarkLabel(enterLoop);
				Visit(d.Stmt);
				Visit(d.Expr);
				IlGen.Emit(OpCodes.Call, PHPRuntimeConvert.GetMethod("ToBool", new Type[] { typeof(object) }));
				IlGen.Emit(OpCodes.Brfalse, exitLoop);
				IlGen.Emit(OpCodes.Br, enterLoop);
				IlGen.MarkLabel(exitLoop);
				EnterLoopLabels.RemoveAt(EnterLoopLabels.Count - 1);
				ExitLoopLabels.RemoveAt(ExitLoopLabels.Count - 1);
			}
			else if (node is FOR) {
				FOR f = (FOR)node;
				Label enterLoop = IlGen.DefineLabel();
				Label exitLoop = IlGen.DefineLabel();
				EnterLoopLabels.Add(enterLoop);
				ExitLoopLabels.Add(exitLoop);
				foreach (Expression expr in f.ExprList1) {
					Visit(expr);
					// pop to treat for expression(s) 1 as statement(s)
					IlGen.Emit(OpCodes.Pop);
				}
				IlGen.MarkLabel(enterLoop);
				IEnumerator ienum = f.ExprList2.GetEnumerator();
				bool hasMore = ienum.MoveNext();
				while (hasMore) {
					Visit((Expression)ienum.Current);
					hasMore = ienum.MoveNext();
					if (hasMore)
						// if more than one expression 2, pop all except last one to only decide on last one for jumping
						IlGen.Emit(OpCodes.Pop);
				}
				// if for expression 2 is empty, the loop should run indefinitely
				if (f.ExprList2.Count() > 0) {
					IlGen.Emit(OpCodes.Call, PHPRuntimeConvert.GetMethod("ToBool", new Type[] { typeof(object) }));
					IlGen.Emit(OpCodes.Brfalse, exitLoop);
				}
				Visit(f.Stmt);
				foreach (Expression expr in f.ExprList3) {
					Visit(expr);
					// pop to treat for expression(s) 3 as statement(s)
					IlGen.Emit(OpCodes.Pop);
				}
				IlGen.Emit(OpCodes.Br, enterLoop);
				IlGen.MarkLabel(exitLoop);
				EnterLoopLabels.RemoveAt(EnterLoopLabels.Count - 1);
				ExitLoopLabels.RemoveAt(ExitLoopLabels.Count - 1);
			}
			else if (node is FOREACH) {
				FOREACH f = (FOREACH)node;
				if (f.Key != null && f.Key is FUNCTION_CALL)
					Report.Error(408, f.Key.Line, f.Key.Column);
				if (f.Value is FUNCTION_CALL)
					Report.Error(408, f.Value.Line, f.Value.Column);
				Label enterLoop = IlGen.DefineLabel();
				Label exitLoop = IlGen.DefineLabel();
				EnterLoopLabels.Add(enterLoop);
				ExitLoopLabels.Add(exitLoop);
				Label warn = IlGen.DefineLabel();
				// push array reference
				Visit(f.Array);
				// if type is not array, throw exception
				IlGen.Emit(OpCodes.Dup);
				IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("EnsureArray", new Type[] { typeof(object) }));
				// if array is not referenced, clone to work on a copy
				if (!(f.Value is REFERENCE))
					IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Clone", new Type[] { typeof(object) }));
				// declare key and value variable
				string valueName;
				if (f.Value is VARIABLE)
					valueName = ((VARIABLE)f.Value).Name;
				else
					valueName = ((VARIABLE)((REFERENCE)f.Value).Expr).Name;
				if (f.Key != null) {
					IlGen.Emit(OpCodes.Ldnull);
					StoreToVariable(((VARIABLE)f.Key).Name);
				}
				IlGen.Emit(OpCodes.Ldnull);
				StoreToVariable(valueName);
				// reset public array pointer
				IlGen.Emit(OpCodes.Dup);
				IlGen.Emit(OpCodes.Callvirt, PHPArray.GetMethod("Reset", Type.EmptyTypes));
				IlGen.Emit(OpCodes.Pop);
				// enter loop
				IlGen.MarkLabel(enterLoop);
				// check if public array pointer is inside the array
				IlGen.Emit(OpCodes.Dup);
				IlGen.Emit(OpCodes.Callvirt, PHPArray.GetMethod("CurrentIsValid", Type.EmptyTypes));
				// if not, exit loop
				IlGen.Emit(OpCodes.Brfalse, exitLoop);
				// if key variable declared, push current key of array and store to key variable
				if (f.Key != null) {
					IlGen.Emit(OpCodes.Dup);
					IlGen.Emit(OpCodes.Callvirt, PHPArray.GetMethod("Key", Type.EmptyTypes));
					StoreToVariable(((VARIABLE)f.Key).Name);
				}
				// push current value of array and store to value variable
				IlGen.Emit(OpCodes.Dup);
				IlGen.Emit(OpCodes.Callvirt, PHPArray.GetMethod("Current", Type.EmptyTypes));
				StoreToVariable(valueName);
				// process foreach statement
				Visit(f.Stmt);
				// load value variable and store as current value of array
				IlGen.Emit(OpCodes.Dup);
				IlGen.Emit(OpCodes.Dup);
				IlGen.Emit(OpCodes.Callvirt, PHPArray.GetMethod("Key", Type.EmptyTypes));
				LoadFromVariable(valueName);
				IlGen.Emit(OpCodes.Callvirt, PHPArray.GetMethod("Append", new Type[] { typeof(object), typeof(object) }));
				// advance public array pointer
				IlGen.Emit(OpCodes.Dup);
				IlGen.Emit(OpCodes.Callvirt, PHPArray.GetMethod("Next", Type.EmptyTypes));
				IlGen.Emit(OpCodes.Pop);
				// and reenter loop
				IlGen.Emit(OpCodes.Br, enterLoop);
				// pop array reference
				IlGen.MarkLabel(exitLoop);
				IlGen.Emit(OpCodes.Pop);
				EnterLoopLabels.RemoveAt(EnterLoopLabels.Count - 1);
				ExitLoopLabels.RemoveAt(ExitLoopLabels.Count - 1);
			}
			else if (node is SWITCH) {
				SWITCH s = (SWITCH)node;
				Label enterLoop = IlGen.DefineLabel();
				Label exitLoop = IlGen.DefineLabel();
				EnterLoopLabels.Add(enterLoop);
				ExitLoopLabels.Add(exitLoop);
				// push switch expression and enter loop
				Visit(s.Expr);
				IlGen.MarkLabel(enterLoop);
				// process case and default statements
				foreach (ASTNode node2 in s.SwitchCaseList) {
					if (node2 is CASE)
						Visit((CASE)node2);
					else if (node2 is DEFAULT)
						Visit((DEFAULT)node2);
				}
				// done
				IlGen.MarkLabel(exitLoop);
				IlGen.Emit(OpCodes.Pop);
				IlGen.Emit(OpCodes.Ldc_I4_0);
				IlGen.Emit(OpCodes.Stsfld, PHPRuntimeCore.GetField("SwitchInProgress"));
				EnterLoopLabels.RemoveAt(EnterLoopLabels.Count - 1);
				ExitLoopLabels.RemoveAt(ExitLoopLabels.Count - 1);
			}
			else if (node is CASE) {
				CASE c = (CASE)node;
				Label processCase = IlGen.DefineLabel();
				Label nextCase = IlGen.DefineLabel();
				// if statements already processing, process this one as well
				IlGen.Emit(OpCodes.Ldsfld, PHPRuntimeCore.GetField("SwitchInProgress"));
				IlGen.Emit(OpCodes.Brtrue, processCase);
				// else if case expression doesn't equal switch expression, jump to next case
				IlGen.Emit(OpCodes.Dup);
				Visit(c.Expr);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("IsEqual", new Type[] { typeof(object), typeof(object) }));
				IlGen.Emit(OpCodes.Call, PHPRuntimeConvert.GetMethod("ToBool", new Type[] { typeof(object) }));
				IlGen.Emit(OpCodes.Brfalse, nextCase);
				// else start processing switch statements
				IlGen.Emit(OpCodes.Ldc_I4_1);
				IlGen.Emit(OpCodes.Stsfld, PHPRuntimeCore.GetField("SwitchInProgress"));
				// process case statement
				IlGen.MarkLabel(processCase);
				Visit(c.Stmt);
				// mark start of next case
				IlGen.MarkLabel(nextCase);
			}
			else if (node is DEFAULT) {
				DEFAULT d = (DEFAULT)node;
				// process default statement
				IlGen.Emit(OpCodes.Ldc_I4_1);
				IlGen.Emit(OpCodes.Stsfld, PHPRuntimeCore.GetField("SwitchInProgress"));
				Visit(d.Stmt);
			}
			else if (node is BREAK) {
				BREAK b = (BREAK)node;
				// if no number of levels to jump was performed jump out of current loop
				if (b.Expr == null) {
					Label exitLoop = (Label)ExitLoopLabels[ExitLoopLabels.Count - 1];
					IlGen.Emit(OpCodes.Br, exitLoop);
				}
				// otherwise jump out the number of levels desired
				else {
					// create jump table for each level
					Label[] jumpTable = new Label[ExitLoopLabels.Count + 1];
					jumpTable[0] = (Label)ExitLoopLabels[ExitLoopLabels.Count - 1];
					for (int i = 1; i <= ExitLoopLabels.Count; i++)
						jumpTable[i] = (Label)ExitLoopLabels[ExitLoopLabels.Count - i];
					// retrieve number of levels desired
					Visit(b.Expr);
					IlGen.Emit(OpCodes.Call, PHPRuntimeConvert.GetMethod("ToInt", new Type[] { typeof(object) }));
					// if a negative number of levels is provided, replace by 0
					Label skipReplace = IlGen.DefineLabel();
					IlGen.Emit(OpCodes.Dup);
					IlGen.Emit(OpCodes.Ldc_I4_0);
					IlGen.Emit(OpCodes.Bgt, skipReplace);
					IlGen.Emit(OpCodes.Pop);
					IlGen.Emit(OpCodes.Ldc_I4_0);
					IlGen.MarkLabel(skipReplace);
					// jump to the label desired
					IlGen.Emit(OpCodes.Switch, jumpTable);
					// if no jump was performed an invalid jump was requested
					IlGen.Emit(OpCodes.Ldc_I4, 504);
					IlGen.Emit(OpCodes.Call, mPHPRuntime.GetType("PHP.Report").GetMethod("Error", new Type[] { typeof(int) }));
				}
			}
			else if (node is CONTINUE) {
				CONTINUE c = (CONTINUE)node;
				// if no number of levels to jump was performed jump out of current loop
				if (c.Expr == null) {
					Label enterLoop = (Label)EnterLoopLabels[EnterLoopLabels.Count - 1];
					IlGen.Emit(OpCodes.Br, enterLoop);
				}
				// otherwise jump out the number of levels desired
				else {
					// create jump table for each level
					Label[] jumpTable = new Label[EnterLoopLabels.Count + 1];
					jumpTable[0] = (Label)EnterLoopLabels[EnterLoopLabels.Count - 1];
					for (int i = 1; i <= EnterLoopLabels.Count; i++)
						jumpTable[i] = (Label)EnterLoopLabels[EnterLoopLabels.Count - i];
					// retrieve number of levels desired
					Visit(c.Expr);
					IlGen.Emit(OpCodes.Call, PHPRuntimeConvert.GetMethod("ToInt", new Type[] { typeof(object) }));
					// if a negative number of levels is provided, replace by 0
					Label skipReplace = IlGen.DefineLabel();
					IlGen.Emit(OpCodes.Dup);
					IlGen.Emit(OpCodes.Ldc_I4_0);
					IlGen.Emit(OpCodes.Bgt, skipReplace);
					IlGen.Emit(OpCodes.Pop);
					IlGen.Emit(OpCodes.Ldc_I4_0);
					IlGen.MarkLabel(skipReplace);
					// jump to the label desired
					IlGen.Emit(OpCodes.Switch, jumpTable);
					// if no jump was performed an invalid jump was requested
					IlGen.Emit(OpCodes.Ldc_I4, 504);
					IlGen.Emit(OpCodes.Call, mPHPRuntime.GetType("PHP.Report").GetMethod("Error", new Type[] { typeof(int) }));
				}
			}
			else if (node is RETURN) {
				RETURN r = (RETURN)node;
				if (FD.Name != "__MAIN" && FD.Name != "__construct" && FD.Name != "__constructStatic") {
					// if no return value specified, use null as return value; otherwise use return value specified
					if (r.Expr == null)
						IlGen.Emit(OpCodes.Ldnull);
					else {
						// ensure a reference is returned if one is required
						if (FD.ReturnByReference)
							Visit(new REFERENCE(r.Expr, r.Line, r.Column));
						// if no reference should be returned, dereference
						else {
							Visit(r.Expr);
							IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("DeReference", new Type[] { typeof(object) }));
						}
					}
				}
				IlGen.Emit(OpCodes.Ret);
			}
			else if (node is UNSET) {
				UNSET u = (UNSET)node;
				// unset variables
				for (int i = 0; i < u.VarList.Count(); i++) {
					if (u.VarList.Get(i) is FUNCTION_CALL)
						Report.Error(408, u.VarList.Get(i).Line, u.VarList.Get(i).Column);
					VARIABLE var = (VARIABLE)u.VarList.Get(i);
					// regular variable, so unset
					if (var.Offset == null) {
						IlGen.Emit(OpCodes.Ldstr, var.Name);
						IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("UnsetVariable", new Type[] { typeof(object) }));
					}
					// array item, so remove from array
					else if (var.Offset.Kind == OFFSET.SQUARE) {
						LoadFromVariable(var.Name);
						// convert to Array (in case the variable was unset)
						IlGen.Emit(OpCodes.Call, PHPRuntimeConvert.GetMethod("ToArray", new Type[] { typeof(object) }));
						if (var.Offset.Value == null)
							IlGen.Emit(OpCodes.Ldnull);
						else
							Visit(var.Offset);
						IlGen.Emit(OpCodes.Callvirt, PHPArray.GetMethod("Remove", new Type[] { typeof(object) }));
					}
				}
			}
			else if (node is ECHO) {
				ECHO e = (ECHO)node;
				foreach (Expression e2 in e.ExprList) {
					Visit(e2);
					IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Echo", new Type[] { typeof(object) }));
				}
			}
			else if (node is EXPRESSION_AS_STATEMENT) {
				EXPRESSION_AS_STATEMENT eas = (EXPRESSION_AS_STATEMENT)node;
				Visit(eas.Expr);
				IlGen.Emit(OpCodes.Pop);
			}
			else if (node is VARIABLE) {
				VARIABLE var = (VARIABLE)node;
				// get desired variable value
				LoadFromVariable(var.Name);
				// process offset, if available
				if (var.Offset != null) {
					// this is an array
					if (var.Offset.Kind == OFFSET.SQUARE) {
						if (var.Offset.Value == null)
							IlGen.Emit(OpCodes.Ldnull);
						else
							Visit(var.Offset);
						IlGen.Emit(OpCodes.Ldc_I4, OFFSET.SQUARE);
						IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("Offset", new Type[] { typeof(object), typeof(object), typeof(int) }));
					}
				}
			}
			else if (node is REFERENCE) {
				REFERENCE r = (REFERENCE)node;
				// process enclosed variable
				if (r.Expr is VARIABLE) {
					VARIABLE var = (VARIABLE)r.Expr;
					// if there is no offset, just reference to that variable
					if (var.Offset == null) {
						IlGen.Emit(OpCodes.Ldstr, var.Name);
						IlGen.Emit(OpCodes.Call, PHPReference.GetMethod("CreateReferenceToLocal", new Type[] { typeof(object) }));
					}
					// if there is an array offset, load array and store to specified place of that array
					else if (var.Offset.Kind == OFFSET.SQUARE) {
						LoadFromVariable(var.Name);
						// if array loaded is null, create a new one and store
						Label skip = IlGen.DefineLabel();
						IlGen.Emit(OpCodes.Brtrue, skip);
						IlGen.Emit(OpCodes.Newobj, PHPArray.GetConstructor(Type.EmptyTypes));
						StoreToVariable(var.Name);
						IlGen.MarkLabel(skip);
						LoadFromVariable(var.Name);
						// if no offset available, append new key with null value and reference to that key
						if (var.Offset.Value == null) {
							IlGen.Emit(OpCodes.Ldnull);
							IlGen.Emit(OpCodes.Call, PHPReference.GetMethod("CreateReference", new Type[] { typeof(object), typeof(object) }));
						}
						// otherwise reference to the key desired
						else {
							Visit(var.Offset);
							IlGen.Emit(OpCodes.Call, PHPReference.GetMethod("CreateReference", new Type[] { typeof(object), typeof(object) }));
						}
					}
				}
				// process enclosed function call
				else if (r.Expr is FUNCTION_CALL) {
					FUNCTION_CALL fc = (FUNCTION_CALL)r.Expr;
					// look in global scope as calls to instance or static methods in user defined classes are handled by object operator or paamayim nekudotayim
					SymbolTableEntry entry = CD__MAIN.Scope.Lookup(fc.FunctionName, SymbolTable.FUNCTION);
					FUNCTION_DECLARATION fd = (FUNCTION_DECLARATION)entry.Node;
					if (!fd.ReturnByReference)
						Report.Error(305, fd.Name, fc.Line, fc.Column);
					// get returned reference
					Visit(fc);
				}
				// process enclosed pyymayim nedudotayim
				else if (r.Expr is PAAMAYIM_NEKUDOTAYIM) {
					PAAMAYIM_NEKUDOTAYIM pn = (PAAMAYIM_NEKUDOTAYIM)r.Expr;
					LoadFromPaamayimNekudotayimAsReference(pn);
				}
				// process enclosed object operator
				else if (r.Expr is OBJECT_OPERATOR) {
					OBJECT_OPERATOR oo = (OBJECT_OPERATOR)r.Expr;
					LoadFromObjectOperatorAsReference(oo);
				}
				// other expressions cannot be referenced
				else
					Report.Error(306, r.Line, r.Column);
			}
			else if (node is CONSTANT) {
				CONSTANT c = (CONSTANT)node;
				// check if a null is desired
				if (c.Name.ToLower() == "null")
					IlGen.Emit(OpCodes.Ldnull);
				else {
					// check if a function pointer is desired
					SymbolTableEntry tmpFDEntry = SymbolTable.GetInstance().Lookup(c.Name, SymbolTable.FUNCTION);
					if (tmpFDEntry != null) {
						FUNCTION_DECLARATION tmpFD = (FUNCTION_DECLARATION)tmpFDEntry.Node;
						IlGen.Emit(OpCodes.Ldftn, tmpFD.MthBld);
						IlGen.Emit(OpCodes.Box, typeof(IntPtr));
					}
					// push constant value
					else {
						IlGen.Emit(OpCodes.Ldstr, c.Name);
						IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("GetConstant", new Type[] { typeof(object) }));
					}
				}
			}
			else if (node is OFFSET) {
				OFFSET o = (OFFSET)node;
				// process offset expression
				Visit(o.Value);
			}
			else if (node is FUNCTION_CALL) {
				FUNCTION_CALL fc = (FUNCTION_CALL)node;
				// check if a predefined function is invoked
				string functionName = fc.FunctionName.ToLower();
				if (predefinedFunctions.ContainsKey(functionName)) {
					int numberOfParameters = (int)predefinedFunctions[functionName];
					PushParameters(fc.Parameters, numberOfParameters);
					Type[] parameterTypes = new Type[numberOfParameters];
					for (int i = 0; i < numberOfParameters; i++)
						parameterTypes[i] = typeof(object);
					IlGen.Emit(OpCodes.Call, PHPRuntimeLang.GetMethod(functionName, parameterTypes));
				}
				// otherwise call user defined function
				else {
					// look in global scope as calls to instance or static methods in user defined classes are handled by object operator or paamayim nekudotayim
					SymbolTableEntry entry = CD__MAIN.Scope.Lookup(fc.FunctionName, SymbolTable.FUNCTION);
					if (entry == null)
						Report.Error(212, fc.FunctionName, fc.Line, fc.Column);
					FUNCTION_DECLARATION fd = (FUNCTION_DECLARATION)entry.Node;
					// pass parameters (only as many as needed)
					int parametersPassedActually = (int)Math.Min(fd.Parameters.Count, fc.Parameters.Count());
					for (int i = 0; i < parametersPassedActually; i++) {
						PARAMETER_DECLARATION pd = (PARAMETER_DECLARATION)fd.Parameters[i];
						Expression expr = (Expression)fc.Parameters.Get(i);
						// ensure a reference is passed, if a reference is required
						if (pd.ByReference && !(expr is REFERENCE))
							expr = new REFERENCE(expr, expr.Line, expr.Column);
						// process parameter
						Visit(expr);
					}
					// if less parameters actually passed then necessary, pass null references instead
					for (int i = parametersPassedActually; i < fd.Parameters.Count; i++) {
						PARAMETER_DECLARATION pd = (PARAMETER_DECLARATION)fd.Parameters[i];
						if (pd.DefaultValue == null)
							Report.Warn(300, System.Convert.ToString(i + 1), fc.Line, fc.Column);
						IlGen.Emit(OpCodes.Ldnull);
					}
					// add function call to call trace
					IlGen.Emit(OpCodes.Ldstr, CD.Name + "->" + fc.FunctionName);
					IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("AddFunctionCallToTrace", new Type[] { typeof(string) }));
					// if an object operator in progress, call instance function
					if (ObjectOperatorInProgress)
						IlGen.Emit(OpCodes.Call, fd.MthBld);
					// else call static function
					else
						IlGen.Emit(OpCodes.Call, fd.MthBld);
					// remove function call from call trace
					IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("RemoveFunctionCallFromTrace", Type.EmptyTypes));
				}
			}
			else if (node is NEW) {
				NEW n = (NEW)node;
				SymbolTableEntry cdEntry = SymbolTable.GetInstance().LookupGlobal(n.Type, SymbolTable.CLASS);
				// type is defined in an referenced assembly
				if (cdEntry == null) {
					Type t = SymbolTable.GetInstance().GetExternalType(n.Type);
					IlGen.Emit(OpCodes.Ldstr, t.AssemblyQualifiedName);
					IlGen.Emit(OpCodes.Newobj, typeof(ArrayList).GetConstructor(Type.EmptyTypes));
					foreach (Expression expr in n.CtorArgs) {
						IlGen.Emit(OpCodes.Dup);
						Visit(expr);
						IlGen.Emit(OpCodes.Call, typeof(ArrayList).GetMethod("Add", new Type[] { typeof(object) }));
						IlGen.Emit(OpCodes.Pop);
					}
					IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("InvokeExternalConstructor", new Type[] { typeof(string), typeof(ArrayList) }));
				}
				// type is defined in script
				else {
					CLASS_DECLARATION cd = (CLASS_DECLARATION)cdEntry.Node;
					// ensure no instance of an abstract class is created
					if (cd.Modifier == Modifiers.ABSTRACT)
						Report.Error(409, cd.Name, n.Line, n.Column);
					SymbolTableEntry ctorEntry = cd.Scope.Lookup("__construct", SymbolTable.FUNCTION);
					FUNCTION_DECLARATION ctorDecl = (FUNCTION_DECLARATION)ctorEntry.Node;
					// pass parameters (only as many as needed)
					int parametersPassedActually = (int)Math.Min(ctorDecl.Parameters.Count, n.CtorArgs.Count());
					for (int i = 0; i < parametersPassedActually; i++) {
						PARAMETER_DECLARATION pd = (PARAMETER_DECLARATION)ctorDecl.Parameters[i];
						Expression expr = (Expression)n.CtorArgs.Get(i);
						// ensure a reference is passed, if a reference is required
						if (pd.ByReference && !(expr is REFERENCE))
							expr = new REFERENCE(expr, expr.Line, expr.Column);
						// process parameter
						Visit(expr);
					}
					// if less parameters actually passed then necessary, pass null references instead
					for (int i = parametersPassedActually; i < ctorDecl.Parameters.Count; i++) {
						PARAMETER_DECLARATION pd = (PARAMETER_DECLARATION)ctorDecl.Parameters[i];
						if (pd.DefaultValue == null)
							Report.Warn(300, System.Convert.ToString(i + 1), n.Line, n.Column);
						IlGen.Emit(OpCodes.Ldnull);
					}
					// add constructor call to call trace
					IlGen.Emit(OpCodes.Ldstr, n.Type + "->__construct");
					IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("AddFunctionCallToTrace", new Type[] { typeof(string) }));
					// call constructor
					IlGen.Emit(OpCodes.Newobj, ctorDecl.CtrBld);
					// remove constructor call from call trace
					IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("RemoveFunctionCallFromTrace", Type.EmptyTypes));
				}
			}
			else if (node is INSTANCEOF) {
				INSTANCEOF i = (INSTANCEOF)node;
				Visit(i.Expr);
				IlGen.Emit(OpCodes.Ldstr, i.Type);
				IlGen.Emit(OpCodes.Ldc_I4_0);
				IlGen.Emit(OpCodes.Ldc_I4_1);
				IlGen.Emit(OpCodes.Call, typeof(Type).GetMethod("GetType", new Type[] { typeof(string), typeof(bool), typeof(bool) }));
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Instanceof", new Type[] { typeof(object), typeof(Type) }));
			}
			else if (node is ARRAY) {
				ARRAY a = (ARRAY)node;
				// create new empty array
				IlGen.Emit(OpCodes.Newobj, PHPArray.GetConstructor(Type.EmptyTypes));
				// process array pairs
				foreach (ARRAY_PAIR ap in a.ArrayPairList) {
					// duplicate reference to array (in order not to loose it after append)
					IlGen.Emit(OpCodes.Dup);
					// process key
					if (ap.Key == null)
						IlGen.Emit(OpCodes.Ldnull);
					else
						Visit(ap.Key);
					// process value
					Visit(ap.Value);
					IlGen.Emit(OpCodes.Callvirt, PHPArray.GetMethod("Append", new Type[] { typeof(object), typeof(object) }));
				}
			}
			else if (node is INC) {
				INC i = (INC)node;
				if (i.Expr is FUNCTION_CALL)
					Report.Error(408, i.Expr.Line, i.Expr.Column);
				LoadFromVariable(((VARIABLE)i.Expr).Name);
				if (i.Kind == 1)
					IlGen.Emit(OpCodes.Dup);
				IlGen.Emit(OpCodes.Ldc_I4_1);
				IlGen.Emit(OpCodes.Box, typeof(int));
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Plus", new Type[] { typeof(object), typeof(object) }));
				if (i.Kind == 0)
					IlGen.Emit(OpCodes.Dup);
				StoreToVariable(((VARIABLE)i.Expr).Name);
			}
			else if (node is DEC) {
				DEC d = (DEC)node;
				if (d.Expr is FUNCTION_CALL)
					Report.Error(408, d.Expr.Line, d.Expr.Column);
				LoadFromVariable(((VARIABLE)d.Expr).Name);
				if (d.Kind == 1)
					IlGen.Emit(OpCodes.Dup);
				IlGen.Emit(OpCodes.Ldc_I4_1);
				IlGen.Emit(OpCodes.Box, typeof(int));
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Minus", new Type[] { typeof(object), typeof(object) }));
				if (d.Kind == 0)
					IlGen.Emit(OpCodes.Dup);
				StoreToVariable(((VARIABLE)d.Expr).Name);
			}
			else if (node is BOOLEAN_NOT) {
				BOOLEAN_NOT bn = (BOOLEAN_NOT)node;
				Visit(bn.Expr);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("BooleanNot", new Type[] { typeof(object) }));
			}
			else if (node is NOT) {
				NOT n = (NOT)node;
				Visit(n.Expr);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Not", new Type[] { typeof(object) }));
			}
			else if (node is EXIT) {
				EXIT e = (EXIT)node;
				if (e.Expr == null)
					IlGen.Emit(OpCodes.Ldnull);
				else
					Visit(e.Expr);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Exit", new Type[] { typeof(object) }));
			}
			else if (node is PRINT) {
				PRINT p = (PRINT)node;
				Visit(p.Expr);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Print", new Type[] { typeof(object) }));
			}
			else if (node is BOOL_CAST) {
				BOOL_CAST bc = (BOOL_CAST)node;
				Visit(bc.Expr);
				IlGen.Emit(OpCodes.Call, PHPRuntimeConvert.GetMethod("ToBool", new Type[] { typeof(object) }));
			}
			else if (node is INT_CAST) {
				INT_CAST ic = (INT_CAST)node;
				Visit(ic.Expr);
				IlGen.Emit(OpCodes.Call, PHPRuntimeConvert.GetMethod("ToInt", new Type[] { typeof(object) }));
			}
			else if (node is DOUBLE_CAST) {
				DOUBLE_CAST dc = (DOUBLE_CAST)node;
				Visit(dc.Expr);
				IlGen.Emit(OpCodes.Call, PHPRuntimeConvert.GetMethod("ToDouble", new Type[] { typeof(object) }));
			}
			else if (node is STRING_CAST) {
				STRING_CAST sc = (STRING_CAST)node;
				Visit(sc.Expr);
				IlGen.Emit(OpCodes.Call, PHPRuntimeConvert.GetMethod("ToString", new Type[] { typeof(object) }));
			}
			else if (node is ARRAY_CAST) {
				ARRAY_CAST ac = (ARRAY_CAST)node;
				Visit(ac.Expr);
				IlGen.Emit(OpCodes.Call, PHPRuntimeConvert.GetMethod("ToArray", new Type[] { typeof(object) }));
			}
			else if (node is OBJECT_CAST) {
				OBJECT_CAST oc = (OBJECT_CAST)node;
				Visit(oc.Expr);
				IlGen.Emit(OpCodes.Call, PHPRuntimeConvert.GetMethod("ToObject", new Type[] { typeof(object) }));
			}
			else if (node is CLONE) {
				CLONE c = (CLONE)node;
				Visit(c.Expr);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Clone", new Type[] { typeof(object) }));
			}
			else if (node is PAAMAYIM_NEKUDOTAYIM) {
				PAAMAYIM_NEKUDOTAYIM pn = (PAAMAYIM_NEKUDOTAYIM)node;
				LoadFromPaamayimNekudotayim(pn);
			}
			else if (node is OBJECT_OPERATOR) {
				OBJECT_OPERATOR oo = (OBJECT_OPERATOR)node;
				LoadFromObjectOperator(oo);
			}
			else if (node is EQUALS) {
				EQUALS e = (EQUALS)node;
				if (e.Expr1 is FUNCTION_CALL)
					Report.Error(408, e.Expr1.Line, e.Expr1.Column);
				// push assigned value as result of this equals expression
				Visit(e.Expr2);
				// if no reference is desired, dereference in case a reference is evaluated
				if (!(e.Expr2 is REFERENCE))
					IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("DeReference", new Type[] { typeof(object) }));
				// store to an object operator expression
				if (e.Expr1 is OBJECT_OPERATOR)
					StoreToObjectOperator((OBJECT_OPERATOR)e.Expr1, e.Expr2);
				// store to an object operator expression
				else if (e.Expr1 is PAAMAYIM_NEKUDOTAYIM)
					StoreToPaamayimNekudotayim((PAAMAYIM_NEKUDOTAYIM)e.Expr1, e.Expr2);
				// store to a variable expression
				else if (e.Expr1 is VARIABLE) {
					VARIABLE var = (VARIABLE)e.Expr1;
					// if there is no offset, just store to that variable
					if (var.Offset == null) {
						IlGen.Emit(OpCodes.Dup);
						StoreToVariable(var.Name);
					}
					// if there is an array offset, load array and store to specified place of that array
					else if (var.Offset.Kind == OFFSET.SQUARE) {
						LoadFromVariable(var.Name);
						// if array loaded is null, create a new one and store
						Label skip = IlGen.DefineLabel();
						IlGen.Emit(OpCodes.Brtrue, skip);
						IlGen.Emit(OpCodes.Newobj, PHPArray.GetConstructor(Type.EmptyTypes));
						StoreToVariable(var.Name);
						IlGen.MarkLabel(skip);
						LoadFromVariable(var.Name);
						// convert to Array (in case the variable was unset)
						IlGen.Emit(OpCodes.Call, PHPRuntimeConvert.GetMethod("ToArray", new Type[] { typeof(object) }));
						// if no offset available, append without user defined key
						if (var.Offset.Value == null) {
							Visit(e.Expr2);
							IlGen.Emit(OpCodes.Callvirt, PHPArray.GetMethod("Append", new Type[] { typeof(object) }));
						}
						// otherwise use user defined key
						else {
							Visit(var.Offset.Value);
							Visit(e.Expr2);
							IlGen.Emit(OpCodes.Callvirt, PHPArray.GetMethod("Append", new Type[] { typeof(object), typeof(object) }));
						}
					}
				}
			}
			else if (node is PLUS_EQUAL) {
				PLUS_EQUAL pe = (PLUS_EQUAL)node;
				// treat as if it was an EQUALS node
				Visit(new EQUALS(pe.Expr1, new PLUS(pe.Expr1, pe.Expr2, pe.Line, pe.Column), pe.Line, pe.Column));
			}
			else if (node is MINUS_EQUAL) {
				MINUS_EQUAL me = (MINUS_EQUAL)node;
				// treat as if it was an EQUALS node
				Visit(new EQUALS(me.Expr1, new MINUS(me.Expr1, me.Expr2, me.Line, me.Column), me.Line, me.Column));
			}
			else if (node is MUL_EQUAL) {
				MUL_EQUAL me = (MUL_EQUAL)node;
				// treat as if it was an EQUALS node
				Visit(new EQUALS(me.Expr1, new TIMES(me.Expr1, me.Expr2, me.Line, me.Column), me.Line, me.Column));
			}
			else if (node is DIV_EQUAL) {
				DIV_EQUAL de = (DIV_EQUAL)node;
				// treat as if it was an EQUALS node
				Visit(new EQUALS(de.Expr1, new DIV(de.Expr1, de.Expr2, de.Line, de.Column), de.Line, de.Column));
			}
			else if (node is CONCAT_EQUAL) {
				CONCAT_EQUAL ce = (CONCAT_EQUAL)node;
				// treat as if it was an EQUALS node
				Visit(new EQUALS(ce.Expr1, new CONCAT(ce.Expr1, ce.Expr2, ce.Line, ce.Column), ce.Line, ce.Column));
			}
			else if (node is MOD_EQUAL) {
				MOD_EQUAL me = (MOD_EQUAL)node;
				// treat as if it was an EQUALS node
				Visit(new EQUALS(me.Expr1, new MOD(me.Expr1, me.Expr2, me.Line, me.Column), me.Line, me.Column));
			}
			else if (node is AND_EQUAL) {
				AND_EQUAL ae = (AND_EQUAL)node;
				// treat as if it was an EQUALS node
				Visit(new EQUALS(ae.Expr1, new AND(ae.Expr1, ae.Expr2, ae.Line, ae.Column), ae.Line, ae.Column));
			}
			else if (node is OR_EQUAL) {
				OR_EQUAL oe = (OR_EQUAL)node;
				// treat as if it was an EQUALS node
				Visit(new EQUALS(oe.Expr1, new OR(oe.Expr1, oe.Expr2, oe.Line, oe.Column), oe.Line, oe.Column));
			}
			else if (node is XOR_EQUAL) {
				XOR_EQUAL xe = (XOR_EQUAL)node;
				// treat as if it was an EQUALS node
				Visit(new EQUALS(xe.Expr1, new XOR(xe.Expr1, xe.Expr2, xe.Line, xe.Column), xe.Line, xe.Column));
			}
			else if (node is SL_EQUAL) {
				SL_EQUAL se = (SL_EQUAL)node;
				// treat as if it was an EQUALS node
				Visit(new EQUALS(se.Expr1, new SL(se.Expr1, se.Expr2, se.Line, se.Column), se.Line, se.Column));
			}
			else if (node is SR_EQUAL) {
				SR_EQUAL se = (SR_EQUAL)node;
				// treat as if it was an EQUALS node
				Visit(new EQUALS(se.Expr1, new SR(se.Expr1, se.Expr2, se.Line, se.Column), se.Line, se.Column));
			}
			else if (node is BOOLEAN_AND) {
				BOOLEAN_AND ba = (BOOLEAN_AND)node;
				Visit(ba.Expr1);
				Visit(ba.Expr2);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("BooleanAnd", new Type[] { typeof(object), typeof(object) }));
			}
			else if (node is BOOLEAN_OR) {
				BOOLEAN_OR bo = (BOOLEAN_OR)node;
				Visit(bo.Expr1);
				Visit(bo.Expr2);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("BooleanOr", new Type[] { typeof(object), typeof(object) }));
			}
			else if (node is LOGICAL_AND) {
				LOGICAL_AND la = (LOGICAL_AND)node;
				Visit(la.Expr1);
				Visit(la.Expr2);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("LogicalAnd", new Type[] { typeof(object), typeof(object) }));
			}
			else if (node is LOGICAL_OR) {
				LOGICAL_OR lo = (LOGICAL_OR)node;
				Visit(lo.Expr1);
				Visit(lo.Expr2);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("LogicalOr", new Type[] { typeof(object), typeof(object) }));
			}
			else if (node is LOGICAL_XOR) {
				LOGICAL_XOR lx = (LOGICAL_XOR)node;
				Visit(lx.Expr1);
				Visit(lx.Expr2);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("LogicalXor", new Type[] { typeof(object), typeof(object) }));
			}
			else if (node is CONCAT) {
				CONCAT c = (CONCAT)node;
				Visit(c.Expr1);
				Visit(c.Expr2);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Concat", new Type[] { typeof(object), typeof(object) }));
			}
			else if (node is PLUS) {
				PLUS p = (PLUS)node;
				Visit(p.Expr1);
				Visit(p.Expr2);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Plus", new Type[] { typeof(object), typeof(object) }));
			}
			else if (node is MINUS) {
				MINUS m = (MINUS)node;
				Visit(m.Expr1);
				Visit(m.Expr2);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Minus", new Type[] { typeof(object), typeof(object) }));
			}
			else if (node is TIMES) {
				TIMES t = (TIMES)node;
				Visit(t.Expr1);
				Visit(t.Expr2);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Times", new Type[] { typeof(object), typeof(object) }));
			}
			else if (node is DIV) {
				DIV d = (DIV)node;
				Visit(d.Expr1);
				Visit(d.Expr2);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Div", new Type[] { typeof(object), typeof(object) }));
			}
			else if (node is MOD) {
				MOD m = (MOD)node;
				Visit(m.Expr1);
				Visit(m.Expr2);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Mod", new Type[] { typeof(object), typeof(object) }));
			}
			else if (node is AND) {
				AND a = (AND)node;
				Visit(a.Expr1);
				Visit(a.Expr2);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("And", new Type[] { typeof(object), typeof(object) }));
			}
			else if (node is OR) {
				OR o = (OR)node;
				Visit(o.Expr1);
				Visit(o.Expr2);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Or", new Type[] { typeof(object), typeof(object) }));
			}
			else if (node is XOR) {
				XOR x = (XOR)node;
				Visit(x.Expr1);
				Visit(x.Expr2);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Xor", new Type[] { typeof(object), typeof(object) }));
			}
			else if (node is SL) {
				SL s = (SL)node;
				Visit(s.Expr1);
				Visit(s.Expr2);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Sl", new Type[] { typeof(object), typeof(object) }));
			}
			else if (node is SR) {
				SL s = (SL)node;
				Visit(s.Expr1);
				Visit(s.Expr2);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Sr", new Type[] { typeof(object), typeof(object) }));
			}
			else if (node is IS_EQUAL) {
				IS_EQUAL ie = (IS_EQUAL)node;
				Visit(ie.Expr1);
				Visit(ie.Expr2);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("IsEqual", new Type[] { typeof(object), typeof(object) }));
			}
			else if (node is IS_NOT_EQUAL) {
				IS_NOT_EQUAL ine = (IS_NOT_EQUAL)node;
				Visit(ine.Expr1);
				Visit(ine.Expr2);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("IsNotEqual", new Type[] { typeof(object), typeof(object) }));
			}
			else if (node is IS_IDENTICAL) {
				IS_IDENTICAL ii = (IS_IDENTICAL)node;
				Visit(ii.Expr1);
				Visit(ii.Expr2);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("IsIdentical", new Type[] { typeof(object), typeof(object) }));
			}
			else if (node is IS_NOT_IDENTICAL) {
				IS_NOT_IDENTICAL ini = (IS_NOT_IDENTICAL)node;
				Visit(ini.Expr1);
				Visit(ini.Expr2);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("IsNotIdentical", new Type[] { typeof(object), typeof(object) }));
			}
			else if (node is LOWER) {
				LOWER l = (LOWER)node;
				Visit(l.Expr1);
				Visit(l.Expr2);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Lower", new Type[] { typeof(object), typeof(object) }));
			}
			else if (node is IS_LOWER_OR_EQUAL) {
				IS_LOWER_OR_EQUAL iloe = (IS_LOWER_OR_EQUAL)node;
				Visit(iloe.Expr1);
				Visit(iloe.Expr2);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("IsLowerOrEqual", new Type[] { typeof(object), typeof(object) }));
			}
			else if (node is GREATER) {
				GREATER g = (GREATER)node;
				Visit(g.Expr1);
				Visit(g.Expr2);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Greater", new Type[] { typeof(object), typeof(object) }));
			}
			else if (node is IS_GREATER_OR_EQUAL) {
				IS_GREATER_OR_EQUAL igoe = (IS_GREATER_OR_EQUAL)node;
				Visit(igoe.Expr1);
				Visit(igoe.Expr2);
				IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("IsGreaterOrEqual", new Type[] { typeof(object), typeof(object) }));
			}
			else if (node is IF_EXPR) {
				IF_EXPR ie = (IF_EXPR)node;
				Label falseBranch = IlGen.DefineLabel();
				Label mergeBranches = IlGen.DefineLabel();
				Visit(ie.Expr1);
				IlGen.Emit(OpCodes.Call, PHPRuntimeConvert.GetMethod("ToBool", new Type[] { typeof(object) }));
				IlGen.Emit(OpCodes.Brfalse, falseBranch);
				Visit(ie.Expr2);
				IlGen.Emit(OpCodes.Br, mergeBranches);
				IlGen.MarkLabel(falseBranch);
				Visit(ie.Expr3);
				IlGen.MarkLabel(mergeBranches);
			}
			else if (node is MAGIC_CONSTANT) {
				MAGIC_CONSTANT mc = (MAGIC_CONSTANT)node;
				if (mc.Kind == MAGIC_CONSTANT.LINE) {
					IlGen.Emit(OpCodes.Ldc_I4, mc.Line);
					IlGen.Emit(OpCodes.Box, typeof(int));
				}
				else if (mc.Kind == MAGIC_CONSTANT.FILE)
					IlGen.Emit(OpCodes.Ldstr, PEmitter.OutputFile.FullName);
				else if (mc.Kind == MAGIC_CONSTANT.CLASS)
					IlGen.Emit(OpCodes.Ldstr, CD.Name);
				else if (mc.Kind == MAGIC_CONSTANT.METHOD) {
					if (CD == CD__MAIN)
						IlGen.Emit(OpCodes.Ldstr, FD.Name);
					else
						IlGen.Emit(OpCodes.Ldstr, CD.Name + "::" + FD.Name);
				}
				else if (mc.Kind == MAGIC_CONSTANT.FUNCTION)
					IlGen.Emit(OpCodes.Ldstr, FD.Name);
			}
			else if (node is LNUMBER_SCALAR) {
				LNUMBER_SCALAR ls = (LNUMBER_SCALAR)node;
				IlGen.Emit(OpCodes.Ldc_I4, ls.Value);
				IlGen.Emit(OpCodes.Box, typeof(int));
			}
			else if (node is DNUMBER_SCALAR) {
				DNUMBER_SCALAR ds = (DNUMBER_SCALAR)node;
				IlGen.Emit(OpCodes.Ldc_R8, ds.Value);
				IlGen.Emit(OpCodes.Box, typeof(double));
			}
			else if (node is STRING_SCALAR) {
				STRING_SCALAR ss = (STRING_SCALAR)node;
				if (ss.Value.ToLower() == "true") {
					IlGen.Emit(OpCodes.Ldc_I4_1);
					IlGen.Emit(OpCodes.Box, typeof(bool));
				}
				else if (ss.Value.ToLower() == "false") {
					IlGen.Emit(OpCodes.Ldc_I4_0);
					IlGen.Emit(OpCodes.Box, typeof(bool));
				}
				else
					IlGen.Emit(OpCodes.Ldstr, ss.Value);
			}
			else if (node is SINGLE_QUOTES) {
				SINGLE_QUOTES sq = (SINGLE_QUOTES)node;
				StringBuilder result = new StringBuilder();
				foreach (object o in sq.EncapsList) {
					if (o is string)
						result.Append((string)o);
					else if (o is VARIABLE) {
						result.Append('$');
						result.Append(((VARIABLE)o).Name);
					}
				}
				IlGen.Emit(OpCodes.Ldstr, result.ToString());
			}
			else if (node is DOUBLE_QUOTES) {
				DOUBLE_QUOTES dq = (DOUBLE_QUOTES)node;
				StringBuilder output = new StringBuilder();
				bool concat = false;
				foreach (object o in dq.EncapsList) {
					if (o is string)
						output.Append((string)o);
					else {
						// push substring between last variable and current one and concat, if necessary
						if (output.Length > 0) {
							IlGen.Emit(OpCodes.Ldstr, output.ToString());
							if (concat)
								IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Concat", new Type[] { typeof(object), typeof(object) }));
							concat = true;
							output = new StringBuilder();
						}
						// push variable value
						if (o is VARIABLE)
							Visit((VARIABLE)o);
						// or push object operator result
						else if (o is OBJECT_OPERATOR)
							Visit((OBJECT_OPERATOR)o);
						// or push null
						else
							IlGen.Emit(OpCodes.Ldnull);
						// concat, if necessary
						if (concat)
							IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Concat", new Type[] { typeof(object), typeof(object) }));
						concat = true;
					}
				}
				// push substring after last variable and concat, if necessary
				if (output.Length > 0) {
					IlGen.Emit(OpCodes.Ldstr, output.ToString());
					if (concat)
						IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Concat", new Type[] { typeof(object), typeof(object) }));
					output = null;
				}
			}
			else if (node is HEREDOC) {
				HEREDOC h = (HEREDOC)node;
				StringBuilder output = new StringBuilder();
				bool concat = false;
				foreach (object o in h.EncapsList) {
					if (o is string)
						output.Append((string)o);
					else {
						// push substring between last variable and current one and concat, if necessary
						if (output.Length > 0) {
							IlGen.Emit(OpCodes.Ldstr, output.ToString());
							if (concat)
								IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Concat", new Type[] { typeof(object), typeof(object) }));
							concat = true;
							output = new StringBuilder();
						}
						// push variable value
						if (o is VARIABLE)
							Visit((VARIABLE)o);
						// or push object operator result
						else if (o is OBJECT_OPERATOR)
							Visit((OBJECT_OPERATOR)o);
						// or push null
						else
							IlGen.Emit(OpCodes.Ldnull);
						// concat, if necessary
						if (concat)
							IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Concat", new Type[] { typeof(object), typeof(object) }));
						concat = true;
					}
				}
				// push substring after last variable and concat, if necessary
				if (output.Length > 0) {
					IlGen.Emit(OpCodes.Ldstr, output.ToString());
					if (concat)
						IlGen.Emit(OpCodes.Call, PHPRuntimeOperators.GetMethod("Concat", new Type[] { typeof(object), typeof(object) }));
					output = null;
				}
			}
			else if (node is UnaryExpression) {
				UnaryExpression ue = (UnaryExpression)node;
				// process expression
				Visit(ue.Expr);
			}
			else if (node is BinaryExpression) {
				BinaryExpression be = (BinaryExpression)node;
				// process expressions
				Visit(be.Expr1);
				Visit(be.Expr2);
			}
			else if (node is TernaryExpression) {
				TernaryExpression te = (TernaryExpression)node;
				// process expressions
				Visit(te.Expr1);
				Visit(te.Expr2);
				Visit(te.Expr3);
			}
			else if (node is Expression) {
				Expression e = (Expression)node;
				if (e is VARIABLE)
					Visit((VARIABLE)e);
				else if (e is FUNCTION_CALL)
					Visit((FUNCTION_CALL)e);
				else if (e is ARRAY)
					Visit((ARRAY)e);
				else if (e is UnaryExpression)
					Visit((UnaryExpression)e);
				else if (e is BinaryExpression)
					Visit((BinaryExpression)e);
				else if (e is TernaryExpression)
					Visit((TernaryExpression)e);
			}
		}

		public void PushParameters(ExpressionList parameters, int number) {
			int parametersPushed = 0;
			// push parameters available, until desired number is reached
			while (parametersPushed < number && parametersPushed < parameters.Count())
				Visit(parameters.Get(parametersPushed++));
			// if not enough parameters were available, fill up with nulls
			while (parametersPushed < number) {
				IlGen.Emit(OpCodes.Ldnull);
				parametersPushed++;
			}
		}

		public void LoadFromVariable(string variableName) {
			IlGen.Emit(OpCodes.Ldstr, variableName);
			IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("LoadFromVariable", new Type[] { typeof(object) }));
		}

		public void StoreToVariable(string variableName) {
			IlGen.Emit(OpCodes.Ldstr, variableName);
			IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("StoreToVariable", new Type[] { typeof(object), typeof(object) }));
		}

		public void StoreToStaticVariable(string variableName) {
			IlGen.Emit(OpCodes.Ldstr, variableName);
			IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("StoreToStaticVariable", new Type[] { typeof(object), typeof(object) }));
		}

		public void StoreToObjectOperator(OBJECT_OPERATOR oo, Expression value) {
			// warn if left part is a function call
			if (oo.Expr1 is FUNCTION_CALL) {
				FUNCTION_CALL fc = (FUNCTION_CALL)oo.Expr1;
				Report.Warn(404, fc.FunctionName, fc.Line, fc.Column);
				if (ObjectOperatorInProgress) {
					IlGen.Emit(OpCodes.Pop);
					ObjectOperatorInProgress = false;
				}
				return;
			}
			// process left part
			ObjectOperatorInProgress = true;
			// local variable
			if (oo.Expr1 is VARIABLE) {
				VARIABLE var = (VARIABLE)oo.Expr1;
				// handle $this->
				if (var.Name == "$this") {
					ProcessingObjectThisWhenStoring = true;
					// if this is a static context, load object from which this static context was called
					if (FD.Modifiers.Contains(Modifiers.STATIC)) {
						Report.Warn(502, var.Line, var.Column);
						IlGen.Emit(OpCodes.Ldsfld, PHPRuntimeCore.GetField("ThisForStaticContext"));
					}
					// otherwise load current object
					else
						IlGen.Emit(OpCodes.Ldarg_0);
					if (var.Offset != null)
						Report.Warn(401, CD.Name, var.Offset.Line, var.Offset.Column);
				}
				// some variable
				else if (var.Name.StartsWith("$")) {
					ProcessingObjectThisWhenStoring = false;
					Visit(var);
				}
				// class member
				else {
					// if processing a class member of $this (of a non-static context), load directly
					if (ProcessingObjectThisWhenStoring && !FD.Modifiers.Contains(Modifiers.STATIC))
						LoadFromClassMemberOfThis(var.Name, var.Line, var.Column);
					// else load at runtime by reflection
					else
						LoadFromClassMemberOfAnotherObject(var.Name);
					ProcessingObjectThisWhenStoring = false;
					// process offset, if available
					if (var.Offset != null) {
						// this is an array
						if (var.Offset.Kind == OFFSET.SQUARE) {
							if (var.Offset.Value == null)
								IlGen.Emit(OpCodes.Ldnull);
							else
								Visit(var.Offset);
							IlGen.Emit(OpCodes.Ldc_I4, OFFSET.SQUARE);
							IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("Offset", new Type[] { typeof(object), typeof(object), typeof(int) }));
						}
					}
				}
			}
			// paamayim nekudotayim
			else if (oo.Expr1 is PAAMAYIM_NEKUDOTAYIM) {
				PAAMAYIM_NEKUDOTAYIM pn = (PAAMAYIM_NEKUDOTAYIM)oo.Expr1;
				LoadFromPaamayimNekudotayim(pn);
			}
			// process right part
			if (oo.Expr2 is OBJECT_OPERATOR)
				StoreToObjectOperator((OBJECT_OPERATOR)oo.Expr2, value);
			else if (oo.Expr2 is VARIABLE) {
				VARIABLE var2 = (VARIABLE)oo.Expr2;
				// no offset available, so store to class member
				if (var2.Offset == null) {
					Visit(value);
					// if processing a class member of $this, store directly
					if (ProcessingObjectThisWhenStoring)
						StoreToClassMemberOfThis(var2.Name, var2.Line, var2.Column);
					// else store at runtime by reflection
					else
						StoreToClassMemberOfAnotherObject(var2.Name);
				}
				// array offset available, so store to specified place
				else if (var2.Offset.Kind == OFFSET.SQUARE) {
					IlGen.Emit(OpCodes.Dup);
					IlGen.Emit(OpCodes.Dup);
					// if processing a class member of $this, load directly
					if (ProcessingObjectThisWhenStoring)
						LoadFromClassMemberOfThis(var2.Name, var2.Line, var2.Column);
					// else load at runtime by reflection
					else
						LoadFromClassMemberOfAnotherObject(var2.Name);
					// if array loaded is null, create a new one and store
					Label skip = IlGen.DefineLabel();
					Label join = IlGen.DefineLabel();
					IlGen.Emit(OpCodes.Brtrue, skip);
					IlGen.Emit(OpCodes.Newobj, PHPArray.GetConstructor(Type.EmptyTypes));
					// if processing a class member of $this, store directly
					if (ProcessingObjectThisWhenStoring)
						StoreToClassMemberOfThis(var2.Name, var2.Line, var2.Column);
					// else store at runtime by reflection
					else
						StoreToClassMemberOfAnotherObject(var2.Name);
					IlGen.Emit(OpCodes.Br, join);
					IlGen.MarkLabel(skip);
					IlGen.Emit(OpCodes.Pop);
					IlGen.MarkLabel(join);
					// if processing a class member of $this, load directly
					if (ProcessingObjectThisWhenStoring)
						LoadFromClassMemberOfThis(var2.Name, var2.Line, var2.Column);
					// else load at runtime by reflection
					else
						LoadFromClassMemberOfAnotherObject(var2.Name);
					// convert to Array (in case the variable was unset)
					IlGen.Emit(OpCodes.Callvirt, PHPRuntimeConvert.GetMethod("ToArray", new Type[] { typeof(object) }));
					// if no offset available, append without user defined key
					if (var2.Offset.Value == null) {
						Visit(value);
						IlGen.Emit(OpCodes.Callvirt, PHPArray.GetMethod("Append", new Type[] { typeof(object) }));
					}
					// otherwise use user defined key
					else {
						Visit(var2.Offset.Value);
						Visit(value);
						IlGen.Emit(OpCodes.Callvirt, PHPArray.GetMethod("Append", new Type[] { typeof(object), typeof(object) }));
					}
				}
				ObjectOperatorInProgress = false;
			}
			else if (oo.Expr2 is FUNCTION_CALL) {
				FUNCTION_CALL fc = (FUNCTION_CALL)oo.Expr2;
				Report.Warn(403, fc.FunctionName, fc.Line, fc.Column);
				if (ObjectOperatorInProgress) {
					IlGen.Emit(OpCodes.Pop);
					ObjectOperatorInProgress = false;
				}
				return;
			}
		}

		public void LoadFromObjectOperator(OBJECT_OPERATOR oo) {
			// warn if left part is a function call
			if (oo.Expr1 is FUNCTION_CALL) {
				FUNCTION_CALL fc = (FUNCTION_CALL)oo.Expr1;
				Report.Warn(404, fc.FunctionName, fc.Line, fc.Column);
				if (ObjectOperatorInProgress) {
					IlGen.Emit(OpCodes.Pop);
					ObjectOperatorInProgress = false;
				}
				IlGen.Emit(OpCodes.Ldnull);
				return;
			}
			// process left part
			ObjectOperatorInProgress = true;
			// local variable
			if (oo.Expr1 is VARIABLE) {
				VARIABLE var = (VARIABLE)oo.Expr1;
				// handle $this->
				if (var.Name == "$this") {
					ProcessingObjectThisWhenLoading = true;
					// if this is a static context, load object from which this static context was called
					if (FD.Modifiers.Contains(Modifiers.STATIC)) {
						Report.Warn(502, var.Line, var.Column);
						IlGen.Emit(OpCodes.Ldsfld, PHPRuntimeCore.GetField("ThisForStaticContext"));
					}
					// otherwise load current object
					else
						IlGen.Emit(OpCodes.Ldarg_0);
					if (var.Offset != null)
						Report.Warn(401, CD.Name, var.Offset.Line, var.Offset.Column);
				}
				// some variable
				else if (var.Name.StartsWith("$")) {
					ProcessingObjectThisWhenLoading = false;
					Visit(var);
				}
				// class member
				else {
					// if processing a class member of $this, load directly
					if (ProcessingObjectThisWhenLoading)
						LoadFromClassMemberOfThis(var.Name, var.Line, var.Column);
					// else load at runtime by reflection
					else
						LoadFromClassMemberOfAnotherObject(var.Name);
					ProcessingObjectThisWhenLoading = false;
					// process offset, if available
					if (var.Offset != null) {
						// this is an array
						if (var.Offset.Kind == OFFSET.SQUARE) {
							if (var.Offset.Value == null)
								IlGen.Emit(OpCodes.Ldnull);
							else
								Visit(var.Offset);
							IlGen.Emit(OpCodes.Ldc_I4, OFFSET.SQUARE);
							IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("Offset", new Type[] { typeof(object), typeof(object), typeof(int) }));
						}
					}
				}
			}
			// paamayim nekudotayim
			else if (oo.Expr1 is PAAMAYIM_NEKUDOTAYIM) {
				PAAMAYIM_NEKUDOTAYIM pn = (PAAMAYIM_NEKUDOTAYIM)oo.Expr1;
				LoadFromPaamayimNekudotayim(pn);
			}
			// process right part
			if (oo.Expr2 is OBJECT_OPERATOR)
				LoadFromObjectOperator((OBJECT_OPERATOR)oo.Expr2);
			else if (oo.Expr2 is VARIABLE) {
				VARIABLE var2 = (VARIABLE)oo.Expr2;
				// if processing a class member of $this (of a non-static context), load directly
				if (ProcessingObjectThisWhenLoading && !FD.Modifiers.Contains(Modifiers.STATIC))
					LoadFromClassMemberOfThis(var2.Name, var2.Line, var2.Column);
				// else load at runtime by reflection
				else
					LoadFromClassMemberOfAnotherObject(var2.Name);
				// process offset, if available
				if (var2.Offset != null) {
					// this is an array
					if (var2.Offset.Kind == OFFSET.SQUARE) {
						if (var2.Offset.Value == null)
							IlGen.Emit(OpCodes.Ldnull);
						else
							Visit(var2.Offset);
						IlGen.Emit(OpCodes.Ldc_I4, OFFSET.SQUARE);
						IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("Offset", new Type[] { typeof(object), typeof(object), typeof(int) }));
					}
				}
				ObjectOperatorInProgress = false;
			}
			else if (oo.Expr2 is FUNCTION_CALL) {
				FUNCTION_CALL fc = (FUNCTION_CALL)oo.Expr2;
				// if processing a class member of $this, invoke directly
				if (ProcessingObjectThisWhenLoading)
					InvokeFunctionOfThis(fc.FunctionName, fc.Line, fc.Column);
				// else invoke at runtime by reflection
				else {
					IlGen.Emit(OpCodes.Newobj, typeof(ArrayList).GetConstructor(Type.EmptyTypes));
					foreach (Expression expr in fc.Parameters) {
						IlGen.Emit(OpCodes.Dup);
						Visit(expr);
						IlGen.Emit(OpCodes.Call, typeof(ArrayList).GetMethod("Add", new Type[] { typeof(object) }));
						IlGen.Emit(OpCodes.Pop);
					}
					InvokeFunctionOfAnotherObject(fc.FunctionName);
				}
				ObjectOperatorInProgress = false;
			}
		}

		public void LoadFromObjectOperatorAsReference(OBJECT_OPERATOR oo) {
			// warn if left part is a function call
			if (oo.Expr1 is FUNCTION_CALL) {
				FUNCTION_CALL fc = (FUNCTION_CALL)oo.Expr1;
				Report.Warn(404, fc.FunctionName, fc.Line, fc.Column);
				if (ObjectOperatorInProgress) {
					IlGen.Emit(OpCodes.Pop);
					ObjectOperatorInProgress = false;
				}
				IlGen.Emit(OpCodes.Ldnull);
				return;
			}
			// process left part
			ObjectOperatorInProgress = true;
			// local variable
			if (oo.Expr1 is VARIABLE) {
				VARIABLE var = (VARIABLE)oo.Expr1;
				// handle $this->
				if (var.Name == "$this") {
					ProcessingObjectThisWhenLoading = true;
					// if this is a static context, load object from which this static context was called
					if (FD.Modifiers.Contains(Modifiers.STATIC)) {
						Report.Warn(502, var.Line, var.Column);
						IlGen.Emit(OpCodes.Ldsfld, PHPRuntimeCore.GetField("ThisForStaticContext"));
					}
					// otherwise load current object
					else
						IlGen.Emit(OpCodes.Ldarg_0);
					if (var.Offset != null)
						Report.Warn(401, CD.Name, var.Offset.Line, var.Offset.Column);
				}
				// some variable
				else if (var.Name.StartsWith("$")) {
					ProcessingObjectThisWhenLoading = false;
					Visit(var);
				}
				// class member
				else {
					// if processing a class member of $this, load directly
					if (ProcessingObjectThisWhenLoading)
						LoadFromClassMemberOfThis(var.Name, var.Line, var.Column);
					// else load at runtime by reflection
					else
						LoadFromClassMemberOfAnotherObject(var.Name);
					ProcessingObjectThisWhenLoading = false;
					// process offset, if available
					if (var.Offset != null) {
						// this is an array
						if (var.Offset.Kind == OFFSET.SQUARE) {
							if (var.Offset.Value == null)
								IlGen.Emit(OpCodes.Ldnull);
							else
								Visit(var.Offset);
							IlGen.Emit(OpCodes.Ldc_I4, OFFSET.SQUARE);
							IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("Offset", new Type[] { typeof(object), typeof(object), typeof(int) }));
						}
					}
				}
			}
			// paamayim nekudotayim
			else if (oo.Expr1 is PAAMAYIM_NEKUDOTAYIM) {
				PAAMAYIM_NEKUDOTAYIM pn = (PAAMAYIM_NEKUDOTAYIM)oo.Expr1;
				LoadFromPaamayimNekudotayim(pn);
			}
			// process right part
			if (oo.Expr2 is OBJECT_OPERATOR)
				LoadFromObjectOperatorAsReference((OBJECT_OPERATOR)oo.Expr2);
			else if (oo.Expr2 is VARIABLE) {
				VARIABLE var = (VARIABLE)oo.Expr2;
				// if there is no offset, just reference to that variable
				if (var.Offset == null) {
					IlGen.Emit(OpCodes.Ldstr, var.Name);
					IlGen.Emit(OpCodes.Call, PHPReference.GetMethod("CreateReference", new Type[] { typeof(object), typeof(object) }));
				}
				// if there is an array offset, load array and store to specified place of that array
				else if (var.Offset.Kind == OFFSET.SQUARE) {
					// if array loaded is null, create a new one and store
					if (ProcessingObjectThisWhenLoading) {
						IlGen.Emit(OpCodes.Dup);
						LoadFromClassMemberOfThis(var.Name, var.Line, var.Column);
						Label skip = IlGen.DefineLabel();
						IlGen.Emit(OpCodes.Brtrue, skip);
						IlGen.Emit(OpCodes.Dup);
						IlGen.Emit(OpCodes.Newobj, PHPArray.GetConstructor(Type.EmptyTypes));
						StoreToClassMemberOfThis(var.Name, var.Line, var.Column);
						IlGen.MarkLabel(skip);
						LoadFromClassMemberOfThis(var.Name, var.Line, var.Column);
					}
					else {
						IlGen.Emit(OpCodes.Dup);
						LoadFromClassMemberOfAnotherObject(var.Name);
						Label skip = IlGen.DefineLabel();
						IlGen.Emit(OpCodes.Brtrue, skip);
						IlGen.Emit(OpCodes.Dup);
						IlGen.Emit(OpCodes.Newobj, PHPArray.GetConstructor(Type.EmptyTypes));
						StoreToClassMemberOfAnotherObject(var.Name);
						IlGen.MarkLabel(skip);
						LoadFromClassMemberOfAnotherObject(var.Name);
					}
					// if no offset available, append new key with null value and reference to that key
					if (var.Offset.Value == null) {
						IlGen.Emit(OpCodes.Ldnull);
						IlGen.Emit(OpCodes.Call, PHPReference.GetMethod("CreateReference", new Type[] { typeof(object), typeof(object) }));
					}
					// otherwise reference to the key desired
					else {
						Visit(var.Offset);
						IlGen.Emit(OpCodes.Call, PHPReference.GetMethod("CreateReference", new Type[] { typeof(object), typeof(object) }));
					}
				}
				ObjectOperatorInProgress = false;
			}
			else if (oo.Expr2 is FUNCTION_CALL) {
				FUNCTION_CALL fc = (FUNCTION_CALL)oo.Expr2;
				// if processing a class member of $this, invoke directly
				if (ProcessingObjectThisWhenLoading) {
					SymbolTableEntry cdEntry = SymbolTable.GetInstance().Lookup(CD.Name, SymbolTable.CLASS);
					if (cdEntry == null)
						Report.Error(203, CD.Name, oo.Line, oo.Column);
					CLASS_DECLARATION cd = (CLASS_DECLARATION)cdEntry.Node;
					SymbolTableEntry fdEntry = cd.Scope.Lookup(fc.FunctionName, SymbolTable.FUNCTION);
					if (fdEntry == null)
						Report.Error(212, fc.FunctionName, fc.Line, fc.Column);
					FUNCTION_DECLARATION fd = (FUNCTION_DECLARATION)fdEntry.Node;
					if (!fd.ReturnByReference)
						Report.Error(305, fd.Name, fc.Line, fc.Column);
					// get returned reference
					InvokeFunctionOfThis(fc.FunctionName, fc.Line, fc.Column);
				}
				// else invoke at runtime by reflection
				else {
					// work on parameters
					IlGen.Emit(OpCodes.Newobj, typeof(ArrayList).GetConstructor(Type.EmptyTypes));
					foreach (Expression expr in fc.Parameters) {
						IlGen.Emit(OpCodes.Dup);
						Visit(expr);
						IlGen.Emit(OpCodes.Call, typeof(ArrayList).GetMethod("Add", new Type[] { typeof(object) }));
						IlGen.Emit(OpCodes.Pop);
					}
					// get returned reference
					InvokeFunctionOfAnotherObject(fc.FunctionName);
					// ensure a reference has been returned (this cannot be ensured at compile time)
					IlGen.Emit(OpCodes.Dup);
					IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("EnsureReference", new Type[] { typeof(object) }));
				}
				ObjectOperatorInProgress = false;
			}
		}

		public void StoreToPaamayimNekudotayim(PAAMAYIM_NEKUDOTAYIM pn, Expression value) {
			// handle self:: and parent::
			if (pn.Type == "self")
				pn.Type = CD.Name;
			else if (pn.Type == "parent") {
				if (CD.Extends == null)
					Report.Error(503, pn.Line, pn.Column);
				pn.Type = CD.Extends;
			}
			// process right part
			if (pn.Expr is VARIABLE) {
				VARIABLE var = (VARIABLE)pn.Expr;
				string classMemberName = (var.Name.StartsWith("$")) ? var.Name.Remove(0, 1) : var.Name;
				// no offset available, so store to class member
				if (var.Offset == null) {
					Visit(value);
					// store
					StoreToStaticClassMember(pn.Type, classMemberName, var.Line, var.Column);
				}
				// array offset available, so store to specified place
				else if (var.Offset.Kind == OFFSET.SQUARE) {
					// load
					LoadFromStaticClassMember(pn.Type, classMemberName, var.Line, var.Column);
					// if array loaded is null, create a new one and store
					Label skip = IlGen.DefineLabel();
					IlGen.Emit(OpCodes.Brtrue, skip);
					IlGen.Emit(OpCodes.Newobj, PHPArray.GetConstructor(Type.EmptyTypes));
					// store
					StoreToStaticClassMember(pn.Type, classMemberName, var.Line, var.Column);
					IlGen.MarkLabel(skip);
					// load
					LoadFromStaticClassMember(pn.Type, classMemberName, var.Line, var.Column);
					// convert to Array (in case the variable was unset)
					IlGen.Emit(OpCodes.Callvirt, PHPRuntimeConvert.GetMethod("ToArray", new Type[] { typeof(object) }));
					// if no offset available, append without user defined key
					if (var.Offset.Value == null) {
						Visit(value);
						IlGen.Emit(OpCodes.Callvirt, PHPArray.GetMethod("Append", new Type[] { typeof(object) }));
					}
					// otherwise use user defined key
					else {
						Visit(var.Offset.Value);
						Visit(value);
						IlGen.Emit(OpCodes.Callvirt, PHPArray.GetMethod("Append", new Type[] { typeof(object), typeof(object) }));
					}
				}
			}
			else if (pn.Expr is FUNCTION_CALL) {
				Report.Error(408, pn.Line, pn.Column);
				return;
			}
		}

		public void LoadFromPaamayimNekudotayim(PAAMAYIM_NEKUDOTAYIM pn) {
			// handle self:: and parent::
			if (pn.Type == "self")
				pn.Type = CD.Name;
			else if (pn.Type == "parent") {
				if (CD.Extends == null)
					Report.Error(503, pn.Line, pn.Column);
				pn.Type = CD.Extends;
			}
			if (pn.Expr is VARIABLE) {
				VARIABLE var = (VARIABLE)pn.Expr;
				string classMemberName = (var.Name.StartsWith("$")) ? var.Name.Remove(0, 1) : var.Name;
				LoadFromStaticClassMember(pn.Type, classMemberName, var.Line, var.Column);
				// process offset, if available
				if (var.Offset != null) {
					// this is an array
					if (var.Offset.Kind == OFFSET.SQUARE) {
						if (var.Offset.Value == null)
							IlGen.Emit(OpCodes.Ldnull);
						else
							Visit(var.Offset);
						IlGen.Emit(OpCodes.Ldc_I4, OFFSET.SQUARE);
						IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("Offset", new Type[] { typeof(object), typeof(object), typeof(int) }));
					}
				}
			}
			else if (pn.Expr is FUNCTION_CALL) {
				FUNCTION_CALL fc = (FUNCTION_CALL)pn.Expr;
				// is this a non-static context?
				// if so, push current object to handle $this in static function
				if (!FD.Modifiers.Contains(Modifiers.STATIC))
					IlGen.Emit(OpCodes.Ldarg_0);
				// if not, push null
				else
					IlGen.Emit(OpCodes.Ldnull);
				IlGen.Emit(OpCodes.Stsfld, PHPRuntimeCore.GetField("ThisForStaticContext"));
				// invoke
				InvokeStaticFunction(pn.Type, fc.FunctionName, fc.Parameters, fc.Line, fc.Column);
			}
		}

		public void LoadFromPaamayimNekudotayimAsReference(PAAMAYIM_NEKUDOTAYIM pn) {
			// handle self:: and parent::
			if (pn.Type == "self")
				pn.Type = CD.Name;
			else if (pn.Type == "parent") {
				if (CD.Extends == null)
					Report.Error(503, pn.Line, pn.Column);
				pn.Type = CD.Extends;
			}
			if (pn.Expr is VARIABLE) {
				VARIABLE var = (VARIABLE)pn.Expr;
				string classMemberName = (var.Name.StartsWith("$")) ? var.Name.Remove(0, 1) : var.Name;
				// if there is no offset, just reference to that variable
				if (var.Offset == null) {
					Type externalType = SymbolTable.GetInstance().GetExternalType(pn.Type);
					if (externalType == null)
						IlGen.Emit(OpCodes.Ldstr, pn.Type);
					else
						IlGen.Emit(OpCodes.Ldstr, externalType.AssemblyQualifiedName);
					IlGen.Emit(OpCodes.Call, typeof(Type).GetMethod("GetType", new Type[] { typeof(string) }));
					IlGen.Emit(OpCodes.Ldstr, classMemberName);
					IlGen.Emit(OpCodes.Call, PHPReference.GetMethod("CreateReference", new Type[] { typeof(object), typeof(object) }));
				}
				// if there is an array offset, load array and store to specified place of that array
				else if (var.Offset.Kind == OFFSET.SQUARE) {
					LoadFromStaticClassMember(pn.Type, classMemberName, var.Line, var.Column);
					// if array loaded is null, create a new one and store
					Label skip = IlGen.DefineLabel();
					IlGen.Emit(OpCodes.Brtrue, skip);
					IlGen.Emit(OpCodes.Newobj, PHPArray.GetConstructor(Type.EmptyTypes));
					StoreToStaticClassMember(pn.Type, classMemberName, var.Line, var.Column);
					IlGen.MarkLabel(skip);
					LoadFromStaticClassMember(pn.Type, classMemberName, pn.Line, pn.Column);
					// if no offset available, append new key with null value and reference to that key
					if (var.Offset.Value == null) {
						IlGen.Emit(OpCodes.Ldnull);
						IlGen.Emit(OpCodes.Call, PHPReference.GetMethod("CreateReference", new Type[] { typeof(object), typeof(object) }));
					}
					// otherwise reference to the key desired
					else {
						Visit(var.Offset);
						IlGen.Emit(OpCodes.Call, PHPReference.GetMethod("CreateReference", new Type[] { typeof(object), typeof(object) }));
					}
				}
			}
			else if (pn.Expr is FUNCTION_CALL) {
				FUNCTION_CALL fc = (FUNCTION_CALL)pn.Expr;
				// is this a non-static context?
				// if so, push current object to handle $this in static function
				if (!FD.Modifiers.Contains(Modifiers.STATIC))
					IlGen.Emit(OpCodes.Ldarg_0);
				// if not, push null
				else
					IlGen.Emit(OpCodes.Ldnull);
				IlGen.Emit(OpCodes.Stsfld, PHPRuntimeCore.GetField("ThisForStaticContext"));
				// look in global scope as calls to instance or static methods in user defined classes are handled by object operator or paamayim nekudotayim
				SymbolTableEntry cdEntry = SymbolTable.GetInstance().LookupGlobal(pn.Type, SymbolTable.CLASS);
				if (cdEntry == null)
					Report.Error(203, pn.Type, pn.Line, pn.Column);
				CLASS_DECLARATION cd = (CLASS_DECLARATION)cdEntry.Node;
				SymbolTableEntry fdEntry = cd.Scope.Lookup(fc.FunctionName, SymbolTable.FUNCTION);
				FUNCTION_DECLARATION fd = (FUNCTION_DECLARATION)fdEntry.Node;
				if (!fd.ReturnByReference)
					Report.Error(305, fd.Name, fc.Line, fc.Column);
				// get returned reference
				InvokeStaticFunction(pn.Type, fc.FunctionName, fc.Parameters, fc.Line, fc.Column);
			}
		}

		public void StoreToClassMemberOfThis(string classMemberName, int line, int column) {
			CLASS_DECLARATION tmpCD = CD;
			SymbolTableEntry cvdEntry = tmpCD.Scope.Lookup(classMemberName, SymbolTable.CLASS_VARIABLE);
			while (cvdEntry == null) {
				if (tmpCD.Extends == null)
					break;
				SymbolTableEntry parentCDEntry = SymbolTable.GetInstance().LookupGlobal(tmpCD.Extends, SymbolTable.CLASS);
				tmpCD = (CLASS_DECLARATION)parentCDEntry.Node;
				cvdEntry = tmpCD.Scope.Lookup(classMemberName, SymbolTable.CLASS_VARIABLE);
				if (cvdEntry != null && ((CLASS_VARIABLE_DECLARATION)cvdEntry.Node).Modifiers.Contains(Modifiers.PRIVATE))
					Report.Error(206, classMemberName, line, column);
			}
			if (cvdEntry == null)
				Report.Error(205, classMemberName, line, column);
			CLASS_VARIABLE_DECLARATION cvd = (CLASS_VARIABLE_DECLARATION)cvdEntry.Node;
			int index = cvd.Names.IndexOf(classMemberName);
			FieldBuilder fb = (FieldBuilder)cvd.FieldBuilders[index];
			IlGen.Emit(OpCodes.Stfld, fb);
		}

		public void LoadFromClassMemberOfThis(string classMemberName, int line, int column) {
			CLASS_DECLARATION tmpCD = CD;
			SymbolTableEntry cvdEntry = tmpCD.Scope.Lookup(classMemberName, SymbolTable.CLASS_VARIABLE);
			while (cvdEntry == null) {
				if (tmpCD.Extends == null)
					break;
				SymbolTableEntry parentCDEntry = SymbolTable.GetInstance().LookupGlobal(tmpCD.Extends, SymbolTable.CLASS);
				tmpCD = (CLASS_DECLARATION)parentCDEntry.Node;
				cvdEntry = tmpCD.Scope.Lookup(classMemberName, SymbolTable.CLASS_VARIABLE);
				if (cvdEntry != null && ((CLASS_VARIABLE_DECLARATION)cvdEntry.Node).Modifiers.Contains(Modifiers.PRIVATE))
					Report.Error(206, classMemberName, line, column);
			}
			if (cvdEntry == null)
				Report.Error(205, classMemberName, line, column);
			CLASS_VARIABLE_DECLARATION cvd = (CLASS_VARIABLE_DECLARATION)cvdEntry.Node;
			int index = cvd.Names.IndexOf(classMemberName);
			FieldBuilder fb = (FieldBuilder)cvd.FieldBuilders[index];
			IlGen.Emit(OpCodes.Ldfld, fb);
		}

		public void InvokeFunctionOfThis(string functionName, int line, int column) {
			CLASS_DECLARATION tmpCD = CD;
			SymbolTableEntry fdEntry = tmpCD.Scope.Lookup(functionName, SymbolTable.FUNCTION);
			while (fdEntry == null) {
				if (tmpCD.Extends == null)
					break;
				SymbolTableEntry parentCDEntry = SymbolTable.GetInstance().LookupGlobal(tmpCD.Extends, SymbolTable.CLASS);
				tmpCD = (CLASS_DECLARATION)parentCDEntry.Node;
				fdEntry = tmpCD.Scope.Lookup(functionName, SymbolTable.FUNCTION);
				if (fdEntry != null && ((FUNCTION_DECLARATION)fdEntry.Node).Modifiers.Contains(Modifiers.PRIVATE))
					Report.Error(213, functionName, line, column);
			}
			if (fdEntry == null)
				Report.Error(212, functionName, line, column);
			FUNCTION_DECLARATION fd = (FUNCTION_DECLARATION)fdEntry.Node;
			MethodBuilder mb = fd.MthBld;
			// add function call to call trace
			IlGen.Emit(OpCodes.Ldstr, CD.Name + "->" + fd.Name);
			IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("AddFunctionCallToTrace", new Type[] { typeof(string) }));
			IlGen.Emit(OpCodes.Call, mb);
			// remove function call from call trace
			IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("RemoveFunctionCallFromTrace", Type.EmptyTypes));
		}

		public void StoreToClassMemberOfAnotherObject(string classMemberName) {
			IlGen.Emit(OpCodes.Ldstr, classMemberName);
			IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("StoreToClassMember", new Type[] { typeof(object), typeof(object), typeof(object) }));
		}

		public void LoadFromClassMemberOfAnotherObject(string classMemberName) {
			IlGen.Emit(OpCodes.Ldstr, classMemberName);
			IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("LoadFromClassMember", new Type[] { typeof(object), typeof(object) }));
		}

		public void InvokeFunctionOfAnotherObject(string functionName) {
			IlGen.Emit(OpCodes.Ldstr, functionName);
			IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("InvokeFunction", new Type[] { typeof(object), typeof(ArrayList), typeof(object) }));
		}

		public void StoreToStaticClassMember(string className, string classMemberName, int line, int column) {
			SymbolTableEntry cdEntry = SymbolTable.GetInstance().LookupGlobal(className, SymbolTable.CLASS);
			// type is defined in an referenced assembly
			if (cdEntry == null) {
				Type t = SymbolTable.GetInstance().GetExternalType(className);
				// type is enum, so get desired enum value
				if (t.IsSubclassOf(typeof(Enum)))
					Report.Error(403, line, column);
				// type is an object or value type, so get desired field
				else {
					FieldInfo fi = t.GetField(classMemberName);
					PropertyInfo pi = t.GetProperty(classMemberName);
					if (fi != null) {
						if (!fi.IsStatic)
							Report.Error(208, classMemberName, line, column);
						IlGen.Emit(OpCodes.Ldstr, fi.FieldType.AssemblyQualifiedName);
						IlGen.Emit(OpCodes.Call, PHPRuntimeConvert.GetMethod("FitTypeForExternalUse", new Type[] { typeof(object), typeof(Type) }));
						IlGen.Emit(OpCodes.Stsfld, fi);
						if (fi.FieldType.IsValueType)
							IlGen.Emit(OpCodes.Unbox, fi.FieldType);
					}
					else if (pi != null) {
						MethodInfo setter = pi.GetSetMethod();
						if (!pi.CanWrite || setter == null)
							Report.Error(227, classMemberName, line, column);
						if (!pi.GetSetMethod().IsStatic)
							Report.Error(208, classMemberName, line, column);
						IlGen.Emit(OpCodes.Ldstr, pi.PropertyType.AssemblyQualifiedName);
						IlGen.Emit(OpCodes.Call, PHPRuntimeConvert.GetMethod("FitTypeForExternalUse", new Type[] { typeof(object), typeof(Type) }));
						if (pi.PropertyType.IsValueType)
							IlGen.Emit(OpCodes.Unbox, pi.PropertyType);
						IlGen.Emit(OpCodes.Call, setter);
					}
					else
						Report.Error(209, classMemberName, line, column);
				}
			}
			// type is defined in script
			else {
				CLASS_DECLARATION cd = (CLASS_DECLARATION)cdEntry.Node;
				CLASS_DECLARATION tmpCD = cd;
				SymbolTableEntry cvdEntry = tmpCD.Scope.Lookup(classMemberName, SymbolTable.CLASS_VARIABLE);
				while (cvdEntry == null) {
					if (tmpCD.Extends == null)
						break;
					SymbolTableEntry parentCDEntry = SymbolTable.GetInstance().LookupGlobal(tmpCD.Extends, SymbolTable.CLASS);
					tmpCD = (CLASS_DECLARATION)parentCDEntry.Node;
					cvdEntry = tmpCD.Scope.Lookup(classMemberName, SymbolTable.CLASS_VARIABLE);
					if (cvdEntry != null && !((CLASS_VARIABLE_DECLARATION)cvdEntry.Node).Modifiers.Contains(Modifiers.STATIC) && !((CLASS_VARIABLE_DECLARATION)cvdEntry.Node).Modifiers.Contains(Modifiers.CONST))
						Report.Error(208, classMemberName, line, column);
					if (cvdEntry != null && ((CLASS_VARIABLE_DECLARATION)cvdEntry.Node).Modifiers.Contains(Modifiers.PRIVATE))
						Report.Error(210, classMemberName, line, column);
				}
				if (cvdEntry == null)
					Report.Error(209, classMemberName, line, column);
				CLASS_VARIABLE_DECLARATION cvd = (CLASS_VARIABLE_DECLARATION)cvdEntry.Node;
				if (!cvd.Modifiers.Contains(Modifiers.STATIC) && !cvd.Modifiers.Contains(Modifiers.CONST))
					Report.Error(208, classMemberName, line, column);
				int index = cvd.Names.IndexOf(classMemberName);
				FieldBuilder fb = (FieldBuilder)cvd.FieldBuilders[index];
				IlGen.Emit(OpCodes.Stsfld, fb);
			}
		}

		public void LoadFromStaticClassMember(string className, string classMemberName, int line, int column) {
			SymbolTableEntry cdEntry = SymbolTable.GetInstance().LookupGlobal(className, SymbolTable.CLASS);
			// type is defined in an referenced assembly
			if (cdEntry == null) {
				Type t = SymbolTable.GetInstance().GetExternalType(className);
				// type is enum, so get desired enum value
				if (t.IsSubclassOf(typeof(Enum))) {
					IlGen.Emit(OpCodes.Ldstr, t.AssemblyQualifiedName);
					IlGen.Emit(OpCodes.Ldstr, classMemberName);
					IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("LoadFromExternalEnumeration", new Type[] { typeof(string), typeof(string) }));
				}
				// type is an object or value type, so get desired field
				else {
					FieldInfo fi = t.GetField(classMemberName);
					PropertyInfo pi = t.GetProperty(classMemberName);
					if (fi != null) {
						IlGen.Emit(OpCodes.Ldsfld, fi);
						if (fi.FieldType.IsValueType)
							IlGen.Emit(OpCodes.Box, fi.FieldType);
						IlGen.Emit(OpCodes.Call, PHPRuntimeConvert.GetMethod("FitTypeForInternalUse", new Type[] { typeof(object) }));
					}
					else if (pi != null) {
						MethodInfo getter = pi.GetGetMethod();
						if (!pi.CanRead || getter == null)
							Report.Error(226, classMemberName, line, column);
						if (!pi.GetGetMethod().IsStatic)
							Report.Error(208, classMemberName, line, column);
						IlGen.Emit(OpCodes.Ldstr, pi.PropertyType.AssemblyQualifiedName);
						IlGen.Emit(OpCodes.Call, PHPRuntimeConvert.GetMethod("FitTypeForExternalUse", new Type[] { typeof(object), typeof(Type) }));
						if (pi.PropertyType.IsValueType)
							IlGen.Emit(OpCodes.Unbox, pi.PropertyType);
						IlGen.Emit(OpCodes.Call, getter);
					}
					else
						Report.Error(209, classMemberName, line, column);
				}
			}
			// type is defined in script
			else {
				CLASS_DECLARATION cd = (CLASS_DECLARATION)cdEntry.Node;
				CLASS_DECLARATION tmpCD = cd;
				SymbolTableEntry cvdEntry = tmpCD.Scope.Lookup(classMemberName, SymbolTable.CLASS_VARIABLE);
				while (cvdEntry == null) {
					if (tmpCD.Extends == null)
						break;
					SymbolTableEntry parentCDEntry = SymbolTable.GetInstance().LookupGlobal(tmpCD.Extends, SymbolTable.CLASS);
					tmpCD = (CLASS_DECLARATION)parentCDEntry.Node;
					cvdEntry = tmpCD.Scope.Lookup(classMemberName, SymbolTable.CLASS_VARIABLE);
					if (cvdEntry != null && !((CLASS_VARIABLE_DECLARATION)cvdEntry.Node).Modifiers.Contains(Modifiers.STATIC) && !((CLASS_VARIABLE_DECLARATION)cvdEntry.Node).Modifiers.Contains(Modifiers.CONST))
						Report.Error(208, classMemberName, line, column);
					if (cvdEntry != null && ((CLASS_VARIABLE_DECLARATION)cvdEntry.Node).Modifiers.Contains(Modifiers.PRIVATE))
						Report.Error(210, classMemberName, line, column);
				}
				if (cvdEntry == null)
					Report.Error(209, classMemberName, line, column);
				CLASS_VARIABLE_DECLARATION cvd = (CLASS_VARIABLE_DECLARATION)cvdEntry.Node;
				if (!cvd.Modifiers.Contains(Modifiers.STATIC) && !cvd.Modifiers.Contains(Modifiers.CONST))
					Report.Error(208, classMemberName, line, column);
				int index = cvd.Names.IndexOf(classMemberName);
				FieldBuilder fb = (FieldBuilder)cvd.FieldBuilders[index];
				IlGen.Emit(OpCodes.Ldsfld, fb);
			}
		}

		public void InvokeStaticFunction(string className, string functionName, ExpressionList parameters, int line, int column) {
			SymbolTableEntry cdEntry = SymbolTable.GetInstance().LookupGlobal(className, SymbolTable.CLASS);
			// type is defined in an referenced assembly
			if (cdEntry == null) {
				Type t = SymbolTable.GetInstance().GetExternalType(className);
				IlGen.Emit(OpCodes.Ldstr, t.AssemblyQualifiedName);
				IlGen.Emit(OpCodes.Newobj, typeof(ArrayList).GetConstructor(Type.EmptyTypes));
				foreach (Expression expr in parameters) {
					IlGen.Emit(OpCodes.Dup);
					Visit(expr);
					IlGen.Emit(OpCodes.Call, typeof(ArrayList).GetMethod("Add", new Type[] { typeof(object) }));
					IlGen.Emit(OpCodes.Pop);
				}
				IlGen.Emit(OpCodes.Ldstr, functionName);
				IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("InvokeExternalStaticFunction", new Type[] { typeof(string), typeof(ArrayList), typeof(object) }));
			}
			// type is defined in script
			else {
				CLASS_DECLARATION cd = (CLASS_DECLARATION)cdEntry.Node;
				CLASS_DECLARATION tmpCD = cd;
				SymbolTableEntry fdEntry = tmpCD.Scope.Lookup(functionName, SymbolTable.FUNCTION);
				while (fdEntry == null) {
					if (tmpCD.Extends == null)
						break;
					SymbolTableEntry parentCDEntry = SymbolTable.GetInstance().LookupGlobal(tmpCD.Extends, SymbolTable.CLASS);
					tmpCD = (CLASS_DECLARATION)parentCDEntry.Node;
					fdEntry = tmpCD.Scope.Lookup(functionName, SymbolTable.FUNCTION);
					if (fdEntry != null && !((FUNCTION_DECLARATION)fdEntry.Node).Modifiers.Contains(Modifiers.STATIC)) {
						Report.Error(215, functionName, line, column);
						IlGen.Emit(OpCodes.Ldnull);
						return;
					}
					if (fdEntry != null && ((FUNCTION_DECLARATION)fdEntry.Node).Modifiers.Contains(Modifiers.PRIVATE)) {
						Report.Error(217, functionName, line, column);
						IlGen.Emit(OpCodes.Ldnull);
						return;
					}
				}
				if (fdEntry == null) {
					Report.Error(216, functionName, line, column);
					IlGen.Emit(OpCodes.Ldnull);
					return;
				}
				FUNCTION_DECLARATION fd = (FUNCTION_DECLARATION)fdEntry.Node;
				if (!fd.Modifiers.Contains(Modifiers.STATIC)) {
					Report.Error(215, functionName, line, column);
					IlGen.Emit(OpCodes.Ldnull);
					return;
				}
				// pass parameters (only as many as needed)
				int parametersPassedActually = (int)Math.Min(fd.Parameters.Count, parameters.Count());
				for (int i = 0; i < parametersPassedActually; i++) {
					PARAMETER_DECLARATION pd = (PARAMETER_DECLARATION)fd.Parameters[i];
					Expression expr = (Expression)parameters.Get(i);
					// ensure a reference is passed, if a reference is required
					if (pd.ByReference && !(expr is REFERENCE))
						expr = new REFERENCE(expr, expr.Line, expr.Column);
					// process parameter
					Visit(expr);
				}
				// if less parameters actually passed then necessary, pass nulls instead
				for (int i = parametersPassedActually; i < fd.Parameters.Count; i++) {
					PARAMETER_DECLARATION pd = (PARAMETER_DECLARATION)fd.Parameters[i];
					if (pd.DefaultValue == null)
						Report.Warn(300, System.Convert.ToString(i + 1), line, column);
					IlGen.Emit(OpCodes.Ldnull);
				}
				// add function call to call trace
				IlGen.Emit(OpCodes.Ldstr, CD.Name + "->" + fd.Name);
				IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("AddFunctionCallToTrace", new Type[] { typeof(string) }));
				// call
				MethodBuilder mb = fd.MthBld;
				IlGen.Emit(OpCodes.Call, mb);
				// remove function call from call trace
				IlGen.Emit(OpCodes.Call, PHPRuntimeCore.GetMethod("RemoveFunctionCallFromTrace", Type.EmptyTypes));
			}
		}

	}


}