// JGenCSharp.cs: JANET code generation implementation for C# output
//
// Author: Steve Newman (steve@snewman.net)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Bitcraft, Inc.


#define TRACE

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;

using JANET.Printer;
using JANET.Compiler;


namespace JANET.Compiler {

// This class implements the CGFuncInfo interface for generating C#
// code.  It stores the information we need to generate code within a
// single function.
internal class CSharpGenFuncInfo : CGFuncInfo
	{
	internal CSharpGenFuncInfo(FunctionInfo func) { this.func = func; }
	
	internal FunctionInfo func;
	
	internal int loopCount = 0; // The number of CSharpLoopInfo objects
							    // created so far in this function.
	internal int withCount = 0; // The number of CSharpWithInfo objects
							    // created so far in this function.
	} // CSharpGenFuncInfo


internal abstract class CSharpExprFrag : ExprFrag
	{
	internal CSharpExprFrag(SrcLoc loc)
		{
		loc_ = loc;
		} // CSharpExprFrag constructor
	
	public SrcLoc loc { get { return loc_;  } }
	internal SrcLoc loc_;
	
	// Return a code string which evaluates this expression as a value.
	internal abstract string GenerateRHS();
	
	// Return a code string which assigns the given expression to this value
	// (using this value as the left hand side of the assignment).
	internal abstract string GenerateAssignment(string rhsCode);
	
	// Return a code string which performs a pre- or post-increment or
	// -decrement on this value.
	internal abstract string GenerateIncDec(bool isIncrement, bool isSuffix);
	
	// Return a code string which implements the "delete" operator on this
	// value.
	internal abstract string GenerateDelete();
	
	// Return a code string which implements the "typeof" operator on this
	// value.
	internal abstract string GenerateTypeof();
	
	// Return a code string which implements a function or constructor call
	// on this value.  args is an array of ExprFrags, or null if no parameter
	// list was provided.
	internal abstract string GenerateCall(ArrayList args, bool isConstruct);
	} // CSharpExprFrag


// This class represents an ExprFrag which can't be used as the left
// side of an assignment.
internal class CSharpRHSExpr : CSharpExprFrag
	{
	internal CSharpRHSExpr(SrcLoc loc, string code) : base(loc)
		{
		code_ = code;
		} // CSharpRHSExpr constructor
	
	
	// Return a code string which evaluates this expression as a value.
	internal override string GenerateRHS() { return code_; }
	
	
	// Return a code string which assigns the given expression to this value
	// (using this value as the left hand side of the assignment).
	internal override string GenerateAssignment(string rhsCode)
		{
		throw new ParseError( "GenerateAssignment called for CSharpRHSExpr",
							  loc );
		} // GenerateAssignment
	
	
	// Return a code string which performs a pre- or post-increment or
	// -decrement on this value.
	internal override string GenerateIncDec(bool isIncrement, bool isSuffix)
		{
		throw new ParseError( "GenerateIncDec called for CSharpRHSExpr",
							  loc );
		} // GenerateIncDec
	
	
	// Return a code string which implements the "delete" operator on this
	// value.
	internal override string GenerateDelete()
		{
		// operator delete applied to a non-reference returns true (ECMA
		// version 3 section 11.4.1).
		return "true";
		} // GenerateDelete
	
	
	// Return a code string which implements the "typeof" operator on this
	// value.
	internal override string GenerateTypeof()
		{
		return String.Format("JObject.Typeof({0})", code_);
		} // GenerateTypeof
	
	
	// Return a code string which implements a function or constructor call
	// on this value.  args is an array of ExprFrags, or null if no parameter
	// list was provided.
	internal override string GenerateCall(ArrayList args, bool isConstruct)
		{
		StringBuilder expr = new StringBuilder();
		expr.Append((isConstruct) ? "Support.New(" : "Support.Call(");
		expr.Append(code_);
		
		return AppendArgs(expr, args);
		} // GenerateCall
	
	
	// Append code for the given ExprFrags to expr, add a closing paren,
	// and return the resulting string.  This is common code for various
	// GenerateCall implementations.
	internal static string AppendArgs(StringBuilder expr, ArrayList args)
		{
		if (args != null)
			foreach (object curArg in args)
				{
				expr.Append(", ");
				expr.Append(((CSharpExprFrag)curArg).GenerateRHS());
				}
		
		expr.Append(")");
		return expr.ToString();
		} // AppendArgs
	
	
	private string code_;
	} // CSharpRHSExpr


// This class represents an ExprFrag which comes from an array element
// reference.
internal class CSharpArrayRef : CSharpExprFrag
	{
	internal CSharpArrayRef(SrcLoc loc, string arrayCode, string indexCode)
	  : base(loc)
		{
		this.arrayCode = arrayCode;
		this.indexCode = indexCode;
		} // CSharpArrayRef constructor
	
	
	// Return a code string which evaluates this expression as a value.
	internal override string GenerateRHS()
		{
		return String.Format("Support.GetProperty({0}, {1})", arrayCode, indexCode);
		} // GenerateRHS
	
	
	// Return a code string which assigns the given expression to this value
	// (using this value as the left hand side of the assignment).
	internal override string GenerateAssignment(string rhsCode)
		{
		return String.Format( "Support.AssignProperty({0}, {1}, {2})",
							  arrayCode, indexCode, rhsCode );
		} // GenerateAssignment
	
	
	// Return a code string which performs a pre- or post-increment or
	// -decrement on this value.
	internal override string GenerateIncDec(bool isIncrement, bool isSuffix)
		{
		string opName = (isSuffix) ? "PostIncDecProperty" : "PreIncDecProperty";
		return String.Format( "Op.{0}({1}, JConvert.ToString({2}), {3})",
							  opName, arrayCode, indexCode,
							  (isIncrement) ? "true" : "false" );
		} // GenerateIncDec
	
	
	// Return a code string which implements the "delete" operator on this
	// value.
	internal override string GenerateDelete()
		{
		return String.Format( "Support.DeleteProperty({0}, JConvert.ToString({1}))",
							  arrayCode, indexCode );
		} // GenerateDelete
	
	
	// Return a code string which implements the "typeof" operator on this
	// value.
	internal override string GenerateTypeof()
		{
		return String.Format( "Support.TypeofProperty({0}, JConvert.ToString({1}))",
							  arrayCode, indexCode );
		} // GenerateTypeof
	
	
	// Return a code string which implements a function or constructor call
	// on this value.  args is an array of ExprFrags, or null if no parameter
	// list was provided.
	internal override string GenerateCall(ArrayList args, bool isConstruct)
		{
		StringBuilder expr = new StringBuilder();
		
		if (isConstruct)
			{
			expr.Append("Support.New(");
			expr.Append(GenerateRHS());
			}
		else
			{
			expr.Append("Support.CallMethod(");
			expr.Append(arrayCode);
			expr.Append(", JConvert.ToString(");
			expr.Append(indexCode);
			}
		
		return CSharpRHSExpr.AppendArgs(expr, args);
		} // GenerateCall
	
	
	private string arrayCode; // Code to generate the array object
	private string indexCode; // Code to generate the element index
	} // CSharpArrayRef


// This class represents an ExprFrag which comes from an object field
// reference.
internal class CSharpFieldRef : CSharpExprFrag
	{
	internal CSharpFieldRef(SrcLoc loc, string objectCode, string id)
	  : base(loc)
		{
		this.objectCode = objectCode;
		this.id = id;
		} // CSharpFieldRef constructor
	
	
	// Return a code string which evaluates this expression as a value.
	internal override string GenerateRHS()
		{
		return String.Format("Support.GetProperty({0}, \"{1}\")", objectCode, id);
		} // GenerateRHS
	
	
	// Return a code string which assigns the given expression to this value
	// (using this value as the left hand side of the assignment).
	internal override string GenerateAssignment(string rhsCode)
		{
		return String.Format( "Support.AssignProperty({0}, \"{1}\", {2})",
							  objectCode, id, rhsCode );
		} // GenerateAssignment
	
	
	// Return a code string which performs a pre- or post-increment or
	// -decrement on this value.
	internal override string GenerateIncDec(bool isIncrement, bool isSuffix)
		{
		string opName = (isSuffix) ? "PostIncDecProperty" : "PreIncDecProperty";
		return String.Format( "Op.{0}({1}, \"{2}\", {3})",
							  opName, objectCode, id,
							  (isIncrement) ? "true" : "false" );
		} // GenerateIncDec
	
	
	// Return a code string which implements the "delete" operator on this
	// value.
	internal override string GenerateDelete()
		{
		return String.Format( "Support.DeleteProperty({0}, \"{1}\")",
							  objectCode, id );
		} // GenerateDelete
	
	
	// Return a code string which implements the "typeof" operator on this
	// value.
	internal override string GenerateTypeof()
		{
		return String.Format( "Support.TypeofProperty({0}, \"{1}\")",
							  objectCode, id );
		} // GenerateTypeof
	
	
	// Return a code string which implements a function or constructor call
	// on this value.  args is an array of ExprFrags, or null if no parameter
	// list was provided.
	internal override string GenerateCall(ArrayList args, bool isConstruct)
		{
		StringBuilder expr = new StringBuilder();
		
		if (isConstruct)
			{
			expr.Append("Support.New(");
			expr.Append(GenerateRHS());
			}
		else
			expr.Append(String.Format( "Support.CallMethod({0}, \"{1}\"",
									   objectCode, id ));
		
		return CSharpRHSExpr.AppendArgs(expr, args);
		} // GenerateCall
	
	
	private string objectCode; // Code to generate the target object
	private string id;         // Name of the field being referenced
	} // CSharpFieldRef


// This class represents an ExprFrag which comes from a variable
// reference.
// 
// HACK snewman 8/20/01: this entire class needs to respect enclosingWiths.
internal class CSharpVariableRef : CSharpExprFrag
	{
	// Construct a CSharpVariableRef for a reference at the given location,
	// to the given variable, in the given function, and in the given stack
	// of WithInfo objects.  function should be null for global variable
	// references.
	internal CSharpVariableRef( SrcLoc loc, string id, FunctionInfo function,
							    Stack enclosingWiths )
	  : base(loc)
		{
		this.id = id;
		this.function = function;
		this.enclosingWiths = enclosingWiths;
		} // CSharpVariableRef constructor
	
	
	// Return a code string which evaluates this expression as a value.
	internal override string GenerateRHS()
		{
		if (enclosingWiths.Count > 0)
			return String.Format("Support.WithGet({0}, \"{1}\"", ObjName(), id) +
				   ScopesAndCloseParen();
		else
			return String.Format("{0}.Get(\"{1}\")", ObjName(), id);
		
		} // GenerateRHS
	
	
	// Return a code string which assigns the given expression to this value
	// (using this value as the left hand side of the assignment).
	internal override string GenerateAssignment(string rhsCode)
		{
		if (enclosingWiths.Count > 0)
			return String.Format( "Support.WithPut({0}, \"{1}\", {2}",
								  ObjName(), id, rhsCode) +
				   ScopesAndCloseParen();
		else
			return String.Format( "{0}.Put(\"{1}\", {2})",
								  ObjName(), id, rhsCode );
		} // GenerateAssignment
	
	
	// Return a code string which performs a pre- or post-increment or
	// -decrement on this value.
	internal override string GenerateIncDec(bool isIncrement, bool isSuffix)
		{
		if (enclosingWiths.Count > 0)
			return String.Format( "Support.WithIncDec({0}, \"{1}\", {2}, {3}",
								  ObjName(), id,
								  (isIncrement) ? "true" : "false",
								  (isSuffix) ? "true" : "false" ) +
				   ScopesAndCloseParen();
		else
			{
			string opName = (isSuffix) ? "PostIncDecProperty" : "PreIncDecProperty";
			return String.Format( "Op.{0}({1}, \"{2}\", {3})",
								  opName, ObjName(), id,
								  (isIncrement) ? "true" : "false" );
			}
		
		} // GenerateIncDec
	
	
	// Return a code string which implements the "delete" operator on this
	// value.
	internal override string GenerateDelete()
		{
		if (enclosingWiths.Count > 0)
			return String.Format( "Support.WithDelete({0}, \"{1}\"",
								  ObjName(), id ) +
				   ScopesAndCloseParen();
		else
			return String.Format( "Support.DeleteProperty({0}, \"{1}\")",
								  ObjName(), id );
		} // GenerateDelete
	
	
	// Return a code string which implements the "typeof" operator on this
	// value.
	internal override string GenerateTypeof()
		{
		if (enclosingWiths.Count > 0)
			return String.Format( "Support.WithTypeof({0}, \"{1}\"",
								  ObjName(), id ) +
				   ScopesAndCloseParen();
		else
			return String.Format( "Support.TypeofProperty({0}, \"{1}\")",
								  ObjName(), id );
		} // GenerateTypeof
	
	
	// Return a code string which implements a function or constructor call
	// on this value.  args is an array of ExprFrags, or null if no parameter
	// list was provided.
	internal override string GenerateCall(ArrayList args, bool isConstruct)
		{
		// HACK snewman 8/26/01: this eval thing is a complete hack, which
		// eventually needs to be replaced by a proper implementation.
		if (id == "eval")
			{
			StringBuilder expr = new StringBuilder();
			expr.Append("Support.eval_(globals");
			return CSharpRHSExpr.AppendArgs(expr, args);
			}
		else
			{
			if (enclosingWiths.Count > 0 && !isConstruct)
				{
				StringBuilder expr = new StringBuilder();
				expr.Append(String.Format( "Support.WithCall({0}, \"{1}\"",
							 			   ObjName(), id ));
				
				expr.Append(", new object[] {");
				bool firstPass = true;
				foreach (CSharpWithInfo curInfo in enclosingWiths)
					{
					if (firstPass)
						firstPass = false;
					else
						expr.Append(", ");
					
					expr.Append("withTemp_");
					expr.Append(System.Convert.ToString(curInfo.index+1));
					}
				expr.Append("}");
				
				return CSharpRHSExpr.AppendArgs(expr, args);
				}
			else
				{
				StringBuilder expr = new StringBuilder();
				expr.Append((isConstruct) ? "Support.New(" : "Support.Call(");
				expr.Append(GenerateRHS());
				return CSharpRHSExpr.AppendArgs(expr, args);
				}
			
			}
		
		} // GenerateCall
	
	
	// Return the name of the activation object we search.
	private string ObjName()
		{
		return (function == null) ? "globals" : "activation";
		} // ObjName
	
	
	// Return a string giving a C# expression for each enclosing "with" object,
	// with each expression preceeded by a comma, and a close parenthesis on
	// the end.  For example, if enclosingWiths has two entries, we would
	// return a string similar to the following:
	// 
	// 		, withTemp_3, withTemp_5)
	private string ScopesAndCloseParen()
		{
		StringBuilder builder = new StringBuilder();
		foreach (CSharpWithInfo curInfo in enclosingWiths)
			{
			builder.Append(", withTemp_");
			builder.Append(System.Convert.ToString(curInfo.index+1));
			}
		
		builder.Append(")");
		return builder.ToString();
		} // ScopesAndCloseParen
	
	
	private string id;  	       // Name of the variable being referenced
	private FunctionInfo function; // Function containing the variable, or
								   // null for global variable references.
	private Stack enclosingWiths;  // Enclosing WithInfo objects.
	} // CSharpVariableRef


// This class is used to store one line of text in a StmtFrag.
internal class StatementLine
	{
	internal string text;		 // Text of this line.  Can be one of the
								 // special values $$indent$$ or $$outdent$$,
								 // which causes subsequent lines to be indented
								 // or outdented.
	internal StatementLine next; // chain pointer for the linked list of
								 // StatementLines in our StmtFrag.
	} // StatementLine


internal class CSharpStmtFrag : StmtFrag
	{
	internal CSharpStmtFrag(SrcLoc loc)
		{
		loc_  = loc;
		code_ = null;
		last_ = null;
		used_ = false;
		} // CSharpStmtFrag constructor
	
	public SrcLoc loc { get { return loc_;  } }
	
	public StatementLine code { get { return code_;  } }
	
	// Append the given fragment to this fragment, so that they execute
	// sequentially.  (NOTE: once this has been done, the "frag" parameter
	// is linked into this fragment, and should no longer be manipulated
	// independently.)
	public void Append(StmtFrag frag)
		{
		CSharpStmtFrag csFrag = (CSharpStmtFrag)frag;
		
		Trace.Assert(!used_);
		Trace.Assert(!csFrag.used_);
		
		if (csFrag.code_ == null)
			return;
		
		if (code_ == null)
			{
			code_ = csFrag.code_;
			last_ = csFrag.last_;
			}
		else
			{
			last_.next = csFrag.code_;
			last_ = csFrag.last_;
			}
		
		csFrag.used_ = true;
		} // Append (StmtFrag)
	
	
	// Append the given C# statement to this fragment.
	internal void Append(string code)
		{
		StatementLine newLine = new StatementLine();
		newLine.text = code;
		newLine.next = null;
		
		if (code_ == null)
			code_ = newLine;
		else
			last_.next = newLine;
		
		last_ = newLine;
		} // Append (string)
	
	
	// Append the given C# statement to this fragment, applying String.Format
	// with the given parameters.
	internal void Append(string format, params object[] p)
		{
		Append(String.Format(format, p));
		} // Append (string + parameters)
	
	
	// Append the given fragment, indented and enclosed in braces.
	internal void AppendIndentedBody(StmtFrag body)
		{
		Indent();
		Append("{");
		Append(body);
		Append("}");
		Outdent();
		}
	
	
	internal void Indent() { Append("$$indent$$"); }
	
	internal void Outdent() { Append("$$outdent$$"); }
	
	private SrcLoc loc_;
	private StatementLine code_; // Header to our linked list of StatementLines,
								 // or null if none.
	private StatementLine last_; // Last entry in our linked list of StatementLines,
								 // or null if none.
	private bool used_;		     // True if this fragment has been "used up",
								 // by appending it into another fragment.
	} // CSharpStmtFrag


internal class CSharpLoopInfo : LoopInfo
	{
	internal CSharpLoopInfo( CSharpGenFuncInfo cgInfo, SrcLoc loc,
							 StringCollection labels, bool isLoop,
							 bool isSwitch )
		{
		this.loc      = loc;
		this.labels   = labels;
		this.isLoop   = isLoop;
		this.isSwitch = isSwitch;
		this.index    = cgInfo.loopCount++;
		} // CSharpLoopInfo constructor
	
	internal SrcLoc           loc;      // Statement location
	internal StringCollection labels;   // Labels for this statement
	internal bool             isLoop;   // True if the labeled statement was a loop,
							            // false if it was just a labeled statement.
	internal bool			  isSwitch; // True if the labeled statement was a
										// switch.
	internal int              index;    // 0-based index of this CSharpWithInfo
							            // object within its function.
	} // CSharpLoopInfo


internal class CSharpWithInfo : WithInfo
	{
	internal CSharpWithInfo(CSharpGenFuncInfo cgInfo, SrcLoc loc)
		{
		this.loc = loc;
		this.index = cgInfo.withCount++;
		} // CSharpWithInfo constructor
	
	internal SrcLoc loc;   // Location of the "with" statement
	internal int    index; // 0-based index of this CSharpWithInfo object
						   // within its function.
	} // CSharpWithInfo


internal class CSharpObjectLiteral : ObjectLiteralInfo
	{
	internal CSharpObjectLiteral() {}
	
	public void AddProp(SrcLoc loc, string id, ExprFrag value)
		{
		Entry newEntry = new Entry();
		newEntry.loc   = loc;
		newEntry.id    = id;
		newEntry.value = value;
		entries.Add(newEntry);
		}
	
	internal class Entry
		{
		internal SrcLoc loc;
		internal string id;
		internal ExprFrag value;
		}
	
	internal ArrayList entries = new ArrayList(); // Array of Entry objects
	} // CSharpObjectLiteral


internal class CSharpArrayLiteral : ArrayLiteralInfo
	{
	internal CSharpArrayLiteral() {}
	
	public void AddEntry(ExprFrag value)
		{
		entries.Add(value);
		}
	
	internal ArrayList entries = new ArrayList(); // Array of ExprFrag objects; null
												  // for null slots in the array.
	} // CSharpArrayLiteral


internal class CSharpSwitchInfo : SwitchInfo
	{
	internal CSharpSwitchInfo() {}
	
	public void AddCase(ExprFrag caseValue, StmtFrag caseStmt)
		{
		Entry newEntry = new Entry();
		newEntry.caseValue = caseValue;
		newEntry.caseStmt  = caseStmt;
		entries.Add(newEntry);
		}
	
	private class Entry
		{
		internal ExprFrag caseValue; // Value for this case, or null for "default:".
		internal StmtFrag caseStmt;  // Body for this case, or null for an empty
									 // (fallthrough) body.
		}
	
	private ArrayList entries = new ArrayList(); // Array of Entry objects
	} // CSharpSwitchInfo


// This class implements the ICodeGenerator interface for generating C#
// code.
public class CSharpGenerator : ICodeGenerator
	{
	// Construct a CSharpGenerator.  Parameters:
	// 
	// 	rootInfo				Info object for the program's root function.
	//	pp						Object where we write the generated program.
	//	progClassName			C# class name to use for the generated program.
	//  forEvalCode				True if the code being compiled came from a
	//							call to eval().
	public CSharpGenerator( FunctionInfo rootInfo, PrettyPrinter pp,
							String progClassName, bool forEvalCode )
		{
		this.rootInfo      = rootInfo;
		this.pp            = pp;
		this.progClassName = progClassName;
		this.forEvalCode   = forEvalCode;
		} // CSharpGenerator constructor
	
	
	// Create a CGFuncInfo object for the given function.
	public CGFuncInfo NewFuncInfo(FunctionInfo func)
		{
		return new CSharpGenFuncInfo(func);
		} // NewFuncInfo
	
	
	// Generate prefix code for the program.  This should be called before
	// any other emit calls.
	public void EmitProgramPrefix()
		{
		pp.Line("// Standard prelude (debugging version)");
		pp.Line("#define TRACE");
		pp.Line("using System;");
		pp.Line("using System.Diagnostics;");
		pp.Line("using JANET.Runtime;");
		pp.Line();
		pp.Line("class " + progClassName + " {");
		pp.Line();
		pp.Line("// Object which holds global-scope variables and functions");
		pp.Line("public static JObject globals;");
		} // EmitProgramPrefix
	
	
	// Generate suffix code for the program.  This should be called after
	// any other emit calls.
	public void EmitProgramSuffix()
		{
		if (forEvalCode)
			{
			// Output an initialization function that takes the globals object
			// as a parameter.  This is used when executing the program in
			// another program's context (i.e. for "eval").
			pp.Line();
			pp.Line("// Initializer for \"eval\" usage");
			pp.Line("public static void Init(JObject theGlobals)");
			pp.Indent();
			pp.Line("{");
			
			pp.Line("globals = theGlobals;");
			
			for ( FunctionInfo child = rootInfo.firstChild; child != null;
				  child = child.nextSib )
				if (child.nameInParent != null)
				{
				// HACK snewman 10/3/01: set the length property for the
				// function we create here.
				string funcName = child.nameInParent;
				pp.Line("JFunctionObject.JFunctionImp {0}_delegate = new JFunctionObject.JFunctionImp({0}_);", funcName);
				pp.Line("globals.Put(\"{0}\", new JFunctionObject({0}_delegate, {0}_delegate));", funcName);
				}
			
			foreach (string varName in rootInfo.locals)
				pp.Line("globals.Put(\"{0}\", JUndefinedObject.instance);", varName);
			
			pp.Line("} // Init");
			pp.Outdent();
			}
		else
			{
			// Output a constructor for the program class.
			pp.Line();
			pp.Line("// Constructor: initializes the global variables");
			pp.Line("static " + progClassName + "()");
			pp.Indent();
			pp.Line("{");
			
			pp.Line("globals = new JObject(JObject.ObjectPrototype, null, \"Object\");");
			pp.Line("Support.DefineBuiltinGlobals(globals);");
			
			for ( FunctionInfo child = rootInfo.firstChild; child != null;
				  child = child.nextSib )
				if (child.nameInParent != null)
				{
				// HACK snewman 10/3/01: set the length property for the
				// function we create here.
				string funcName = child.nameInParent;
				pp.Line("JFunctionObject.JFunctionImp {0}_delegate = new JFunctionObject.JFunctionImp({0}_);", funcName);
				pp.Line("globals.Put(\"{0}\", new JFunctionObject({0}_delegate, {0}_delegate));", funcName);
				}
			
			foreach (string varName in rootInfo.locals)
				pp.Line("globals.Put(\"{0}\", JUndefinedObject.instance);", varName);
			
			pp.Line("} // " + progClassName);
			pp.Outdent();
			}
		
		
		// Output a program entry point.
		// 
		// HACK snewman 8/21/01: eliminate this, or put it under
		// control of a compiler parameter.
		pp.Line();
		pp.Line("public static void Main()");
		pp.Line("{");
		pp.Indent();
		// pp.Line("{0} prog = new {0}();", progClassName);
		// pp.Line("prog.Main();");
		pp.Line("{0}.GlobalCode();", progClassName);
		pp.Outdent();
		pp.Line("} // Main");
		
		// Implement the IJANETProgram interface.
		pp.Line();
		pp.Line("public static JObject GetGlobals() { return globals; }");
		
		// Terminate the program class.
		pp.Line();
		pp.Line("} // " + progClassName);
		} // EmitProgramSuffix
	
	
	// Generate code for the main function, given the function's body.
	public void EmitMainFunction(StmtFrag body)
		{
		pp.Line();
		pp.Line("// Main program code");
		pp.Line("public static void GlobalCode()");
		pp.Indent();
		pp.Line("{");
		
		EmitStatements(body);
		
		pp.Line("} // GlobalCode");
		pp.Outdent();
		} // EmitMainFunction
	
	
	// Generate code for the given function (other than the main function),
	// given the function's FunctionInfo object and its body.
	public void EmitFunction(FunctionInfo info, StmtFrag body)
		{
		// HACK snewman 8/15/01: review the way locals are handled here, to
		// make sure we're declaring them in precise accordance with the spec.
		
		// HACK snewman 8/15/01: add support for nested functions
		if (info.firstChild != null)
			throw new ParseError( "EmitFunction: nested functions not yet implemented",
								  body.loc );
		
		string functionLabel, functionName;
		if (info.nameInParent == null)
			{
			functionLabel = "Anonymous function";
			functionName = "anon_";
			}
		else
			{
			functionLabel = "Function \"" + info.nameInParent + "\"";
			functionName = info.nameInParent + "_";
			}
		
		functionName = MakeUniqueMethodName(functionName);
		
		pp.Line();
		pp.Line("// {0}", functionLabel);
		
		pp.Line("public static object {0}(object this_, params object[] args)", functionName);
		pp.Indent();
		pp.Line("{");
		
		pp.Text("JActivationObject activation = new JActivationObject(args");
		foreach (string paramName in info.paramNames)
			pp.Text(", \"{0}\"", paramName);
		pp.EndLine(");");
		
		foreach (string varName in info.locals)
			pp.Line("activation.Put(\"{0}\", JUndefinedObject.instance);", varName);
		
		EmitStatements(body);
		
		pp.Line("return JUndefinedObject.instance;");
		
		pp.Line("{0} // {1}", "}", functionName);
		pp.Outdent();
		} // EmitFunction
	
	
	// Generate code for the given StmtFrag.  Don't bother enclosing the code
	// in braces (the caller is assumed to have done that).
	private void EmitStatements(StmtFrag statements)
		{
		for ( StatementLine code = ((CSharpStmtFrag)statements).code;
			  code != null;
			  code = code.next )
			if (code.text == "$$indent$$")
				pp.Indent();
			else if (code.text == "$$outdent$$")
				pp.Outdent();
			else
				pp.Line(code.text);
		
		} // EmitStatements
	
	
	// Return a new, empty StmtFrag object.
	public StmtFrag NewStmtFrag(SrcLoc loc)
		{
		return new CSharpStmtFrag(loc);
		} // NewStmtFrag
	
	
	// Return a new LoopInfo object for an iteratiion statement with the
	// given label set.  The labels parameter can be null if there were no
	// labels.
	public LoopInfo NewLoopInfo( CGFuncInfo cgFuncInfo, SrcLoc loc,
								 StringCollection labels )
		{
		return new CSharpLoopInfo( (CSharpGenFuncInfo)cgFuncInfo, loc, labels,
								   true, false );
		} // NewLoopInfo
	
	
	// Return a new LoopInfo object for a labeled non-iteration statement
	// with the given label set.  isSwitch should be true if the statement
	// was a switch statement; in this case, if the statement had no labels,
	// the labels parameter can be null.
	public LoopInfo NewLabeledStmtInfo( CGFuncInfo cgFuncInfo, SrcLoc loc,
										StringCollection labels,
										bool isSwitch )
		{
		Trace.Assert(labels != null || isSwitch);
		return new CSharpLoopInfo( (CSharpGenFuncInfo)cgFuncInfo, loc, labels,
								   false, isSwitch );
		} // NewLabeledStmtInfo
	
	
	// Return a new WithInfo object.
	public WithInfo NewWithInfo(CGFuncInfo cgFuncInfo, SrcLoc loc)
		{
		return new CSharpWithInfo((CSharpGenFuncInfo)cgFuncInfo, loc);
		} // NewWithInfo
	
	
	// Return a new, empty ObjectLiteralInfo object.
	public ObjectLiteralInfo NewObjectLiteralInfo()
		{
		return new CSharpObjectLiteral();
		} // NewObjectLiteralInfo
	
	
	// Return a new, empty ArrayLiteralInfo object.
	public ArrayLiteralInfo NewArrayLiteralInfo()
		{
		return new CSharpArrayLiteral();
		} // NewArrayLiteralInfo
	
	
	// Return a new, empty SwitchInfo object.
	public SwitchInfo NewSwitchInfo()
		{
		return new CSharpSwitchInfo();
		} // NewSwitchInfo
	
	
	// Return a statement which evaluates the given expression.
	public StmtFrag ExpressionStmt(ExprFrag expr)
		{
		CSharpStmtFrag frag = new CSharpStmtFrag(expr.loc);
		frag.Append(((CSharpExprFrag)expr).GenerateRHS() + ";");
		return frag;
		} // ExpressionStmt
	
	
	// Return an if/then or if/then/else statement.  If there was no else
	// clause, elseClause will be null.
	public StmtFrag IfThenElse( SrcLoc loc, ExprFrag condition,
								StmtFrag ifClause, StmtFrag elseClause )
		{
		CSharpStmtFrag frag = new CSharpStmtFrag(loc);
		
		CSharpExprFrag csCondition = (CSharpExprFrag)condition;
		frag.Append("if (Support.BoolTest(" + csCondition.GenerateRHS() + "))");
		frag.AppendIndentedBody(ifClause);
		
		if (elseClause != null)
			{
			frag.Append("else");
			frag.AppendIndentedBody(elseClause);
			}
		
		return frag;
		} // IfThenElse
	
	
	// Return a do...while statement.
	public StmtFrag DoWhile( LoopInfo loopInfo, StmtFrag body,
							 ExprFrag condition )
		{
		CSharpLoopInfo csLoopInfo = (CSharpLoopInfo)loopInfo;
		CSharpStmtFrag frag = new CSharpStmtFrag(csLoopInfo.loc);
		
		CSharpExprFrag csCondition = (CSharpExprFrag)condition;
		frag.Append("do");
		((CSharpStmtFrag)body).Append("continueTarget_{0}:", csLoopInfo.index + 1);
		frag.AppendIndentedBody(body);
		frag.Append("while (Support.BoolTest(" + csCondition.GenerateRHS() + "))");
		frag.Append("breakTarget_{0}: {1}", csLoopInfo.index + 1, "{}");
		
		return frag;
		} // DoWhile
	
	
	// Return a while...do statement.
	public StmtFrag WhileDo( LoopInfo loopInfo, ExprFrag condition,
							 StmtFrag body )
		{
		CSharpLoopInfo csLoopInfo = (CSharpLoopInfo)loopInfo;
		CSharpStmtFrag frag = new CSharpStmtFrag(csLoopInfo.loc);
		
		CSharpExprFrag csCondition = (CSharpExprFrag)condition;
		frag.Append("while (Support.BoolTest(" + csCondition.GenerateRHS() + "))");
		((CSharpStmtFrag)body).Append("continueTarget_{0}:", csLoopInfo.index + 1);
		frag.AppendIndentedBody(body);
		frag.Append("breakTarget_{0}: {1}", csLoopInfo.index + 1, "{}");
		
		return frag;
		} // WhileDo
	
	
	// Return a for statement.  init is the loop initializer, cond is the
	// loop control expression, and step is the loop increment expression.
	// Any or all of init, cond, and step can be null.
	public StmtFrag For( LoopInfo loopInfo, StmtFrag init,
						 ExprFrag cond, ExprFrag step, StmtFrag body )
		{
		CSharpLoopInfo csLoopInfo = (CSharpLoopInfo)loopInfo;
		CSharpStmtFrag frag = new CSharpStmtFrag(csLoopInfo.loc);
		
		frag.Append(init);
		
		CSharpExprFrag csCondition = (CSharpExprFrag)cond;
		frag.Append("while (Support.BoolTest(" + csCondition.GenerateRHS() + "))");
		((CSharpStmtFrag)body).Append("continueTarget_{0}:", csLoopInfo.index + 1);
		((CSharpStmtFrag)body).Append(ExpressionStmt(step));
		frag.AppendIndentedBody(body);
		frag.Append("breakTarget_{0}: {1}", csLoopInfo.index + 1, "{}");
		
		return frag;
		} // For
	
	
	// Return a for/in statement.
	public StmtFrag ForIn( LoopInfo loopInfo, StmtFrag init,
						   ExprFrag lhs, ExprFrag rhs, StmtFrag body )
		{
		CSharpLoopInfo csLoopInfo = (CSharpLoopInfo)loopInfo;
		
		// HACK snewman 8/15/01: implement for/in statements.
		throw new ParseError( "For/In statements not yet implemented",
							  csLoopInfo.loc );
		} // ForIn
	
	
	// Return a continue statement.  label will be null if no label was
	// specified in the statement.  enclosingLoops should be a stack of
	// LoopInfo objects for all enclosing loops and labeled statements.
	public StmtFrag Continue(SrcLoc loc, string label, Stack enclosingLoops)
		{
		CSharpLoopInfo targetLoop = null;
		foreach (object curLoop in enclosingLoops)
			{
			CSharpLoopInfo curCSLoop = (CSharpLoopInfo)curLoop;
			if (curCSLoop.isLoop)
				if (label == null || curCSLoop.labels.Contains(label))
					{
					targetLoop = curCSLoop;
					break;
					}
			
			} // curLoop loop
		
		if (targetLoop == null)
			throw new ParseError( "can't find enclosing loop to match continue label \"" + label + "\"",
								  loc );
		
		CSharpStmtFrag frag = new CSharpStmtFrag(loc);
		frag.Append(String.Format("goto continueTarget_{0};", targetLoop.index + 1));
		return frag;
		} // Continue
	
	
	// Return a break statement.  label will be null if no label was
	// specified in the statement.  enclosingLoops should be a stack of
	// LoopInfo objects for all enclosing loops and labeled statements.
	public StmtFrag Break(SrcLoc loc, string label, Stack enclosingLoops)
		{
		CSharpLoopInfo targetLoop = null;
		foreach (object curLoop in enclosingLoops)
			{
			CSharpLoopInfo curCSLoop = (CSharpLoopInfo)curLoop;
			if ( (label == null && (curCSLoop.isLoop || curCSLoop.isSwitch)) ||
				 curCSLoop.labels.Contains(label) )
				{
				targetLoop = curCSLoop;
				break;
				}
			
			} // curLoop loop
		
		if (targetLoop == null)
			throw new ParseError( "can't find enclosing loop to match break label \"" + label + "\"",
								  loc );
		
		CSharpStmtFrag frag = new CSharpStmtFrag(loc);
		frag.Append(String.Format("goto breakTarget_{0};", targetLoop.index + 1));
		return frag;
		} // Break
	
	
	// Return a statement which associated the specified LoopInfo with
	// an enclosing statement.  This should be called for switch statements,
	// and for any labeled statement which is not a loop statement.
	public StmtFrag LabeledStmt(LoopInfo loopInfo, StmtFrag stmt)
		{
		CSharpLoopInfo csLoopInfo = (CSharpLoopInfo)loopInfo;
		CSharpStmtFrag frag = new CSharpStmtFrag(csLoopInfo.loc);
		
		frag.Append(stmt);
		frag.Append("breakTarget_{0}: {1}", csLoopInfo.index + 1, "{}");
		
		return frag;
		} // LabeledStmt
	
	
	// Return a return statement.  value will be null if no value was
	// specified in the statement.
	public StmtFrag Return(SrcLoc loc, ExprFrag value)
		{
		CSharpStmtFrag frag = new CSharpStmtFrag(loc);
		
		if (value != null)
			frag.Append("return " + ((CSharpExprFrag)value).GenerateRHS() + ";");
		else
			frag.Append("return;");
		
		return frag;
		} // Return
	
	
	// Return a throw statement.
	public StmtFrag Throw(SrcLoc loc, ExprFrag value)
		{
		CSharpStmtFrag frag = new CSharpStmtFrag(loc);
		frag.Append("throw Support.WrapException(" + ((CSharpExprFrag)value).GenerateRHS() + ");");
		return frag;
		} // Throw
	
	
	// Return a with statement.
	public StmtFrag With(WithInfo withInfo, ExprFrag value, StmtFrag body)
		{
		CSharpWithInfo csWithInfo = (CSharpWithInfo)withInfo;
		CSharpStmtFrag frag = new CSharpStmtFrag(csWithInfo.loc);
		
		frag.Append( "object withTemp_{0} = ({1});",
					 csWithInfo.index+1, ((CSharpExprFrag)value).GenerateRHS() );
		frag.Append(body);
		
		return frag;
		} // With
	
	
	// Return a switch statement.
	public StmtFrag Switch( SrcLoc loc, ExprFrag switchValue,
							SwitchInfo switchInfo )
		{
		// HACK snewman 8/15/01: implement switch statements.
		throw new ParseError( "Switch statements not yet implemented",
							  loc );
		} // Switch
	
	
	// Return a try...catch, try...finally, or try...catch...finally
	// statement.  If there was no "catch" clause, then catchVar, catchWithInfo,
	// and catchBody will be null.  If there was no "finally" clause, then
	// finallyBody will be null.  These parameters can't all be null (i.e.
	// at least one of the "catch" and "finally" clauses must have been
	// present).
	public StmtFrag TryCatchFinally( SrcLoc loc,
									 StmtFrag tryBody, string catchVar,
									 WithInfo catchWithInfo,
									 StmtFrag catchBody,
									 StmtFrag finallyBody )
		{
		CSharpStmtFrag frag = new CSharpStmtFrag(loc);
		
		frag.Append("try");
		frag.AppendIndentedBody(tryBody);
		
		if (catchBody != null)
			{
			CSharpWithInfo csWithInfo = (CSharpWithInfo)catchWithInfo;
			
			frag.Append("catch (Exception catchTemp_{0})", csWithInfo.index+1);
			frag.Indent();
			frag.Append("{");
			
			frag.Append( "JObject withTemp_{0} = Support.CreateCatchScope(catchTemp_{0}, \"{1}\");",
						 csWithInfo.index+1, catchVar );
			
			frag.Append(catchBody);
			
			frag.Append("}");
			frag.Outdent();
			}
		
		if (finallyBody != null)
			{
			frag.Append("finally");
			frag.AppendIndentedBody(finallyBody);
			}
		
		return frag;
		} // TryCatchFinally
	
	
	// Return a ?: expression.
	public ExprFrag ConditionalExpr( SrcLoc loc, ExprFrag condition,
									 ExprFrag p1, ExprFrag p2 )
		{
		string condCode = ((CSharpExprFrag)condition).GenerateRHS();
		string p1Code   = ((CSharpExprFrag)p1       ).GenerateRHS();
		string p2Code   = ((CSharpExprFrag)p2       ).GenerateRHS();
		
		string finalCode = String.Format( "Support.BoolTest({0}) ? ({1}) : ({2})",
										  condCode, p1Code, p2Code );
		return new CSharpRHSExpr(loc, finalCode);
		} // ConditionalExpr
	
	
	// binaryOpTable maps binary operators to the name of the method
	// in the JANET.Runtime.Op class that implements that operator.
	// prefixOpTable and suffixOpTable serve a similar purpose for
	// prefix unary and suffix unary operators.
	static Hashtable binaryOpTable, prefixOpTable, suffixOpTable;
	
	static CSharpGenerator()
		{
		binaryOpTable = new Hashtable();
		
		binaryOpTable.Add("*",          "Mul");
		binaryOpTable.Add("/",          "Div");
		binaryOpTable.Add("%",          "Mod");
		binaryOpTable.Add("+",          "Plus");
		binaryOpTable.Add("-",          "Minus");
		binaryOpTable.Add("<<",         "ShiftLeft");
		binaryOpTable.Add(">>",         "ShiftRightSigned");
		binaryOpTable.Add(">>>",        "ShiftRightUnsigned");
		binaryOpTable.Add("<",          "LT");
		binaryOpTable.Add(">",          "GT");
		binaryOpTable.Add("<=",         "LE");
		binaryOpTable.Add(">=",         "GE");
		binaryOpTable.Add("instanceof", "opInstanceof");
		binaryOpTable.Add("in",         "opIn");
		binaryOpTable.Add("==",         "EQ");
		binaryOpTable.Add("!=",         "NE");
		binaryOpTable.Add("===",        "StrictEQ");
		binaryOpTable.Add("!==",        "StrictNE");
		binaryOpTable.Add("&",          "BitAnd");
		binaryOpTable.Add("^",          "BitXor");
		binaryOpTable.Add("|",          "BitOr");
		
		binaryOpTable.Add("*=",         "$$compound$$");
		binaryOpTable.Add("/=",         "$$compound$$");
		binaryOpTable.Add("%=",         "$$compound$$");
		binaryOpTable.Add("+=",         "$$compound$$");
		binaryOpTable.Add("-=",         "$$compound$$");
		binaryOpTable.Add("<<=",        "$$compound$$");
		binaryOpTable.Add(">>=",        "$$compound$$");
		binaryOpTable.Add(">>>=",       "$$compound$$");
		binaryOpTable.Add("&=",         "$$compound$$");
		binaryOpTable.Add("^=",         "$$compound$$");
		binaryOpTable.Add("|=",         "$$compound$$");
		
		binaryOpTable.Add(",",          "Comma");
		
		prefixOpTable = new Hashtable();
		prefixOpTable.Add("void",   "Void");
		prefixOpTable.Add("+",      "UnaryPlus");
		prefixOpTable.Add("-",      "UnaryMinus");
		prefixOpTable.Add("~",      "BitwiseNOT");
		prefixOpTable.Add("!",      "LogicalNOT");
		
		suffixOpTable = new Hashtable();
		}
	
	
	// Return an expression built from a binary operator.
	public ExprFrag BinaryExpr(SrcLoc loc, string op, ExprFrag x, ExprFrag y)
		{
		// HACK snewman 8/15/01: && and || shouldn't return hard-coded
		// true or false, they should always return one of the two
		// operands.  (This is difficult to fix in C#, but will be easy
		// in the direct-to-CIL implementation.)
		
		string finalCode;
		CSharpExprFrag xExpr = (CSharpExprFrag)x;
		CSharpExprFrag yExpr = (CSharpExprFrag)y;
		
		if (op == "=")
			finalCode = xExpr.GenerateAssignment(yExpr.GenerateRHS());
		else
			{
			string xCode = xExpr.GenerateRHS();
			string yCode = yExpr.GenerateRHS();
			
			if (op == "&&")
				finalCode = String.Format( "Support.BoolTest({0}) ? ({1}) : false",
											xCode, yCode );
			else if (op == "||")
				finalCode = String.Format( "Support.BoolTest({0}) ? true : ({1})",
											xCode, yCode );
			else
				{
				string opName = (string)(binaryOpTable[op]);
				if (opName == "$$compound$$")
					{
					// HACK snewman 8/16/01: compound assignment operators are
					// a bit of a hassle to map down to C# code, so for now we're
					// just splitting them into a standard operator followed by
					// a standard assignment.  This is not really kosher, because
					// the side effects of the LHS are performed twice, and the
					// order of evaluatation is twisted.
					ExprFrag rhs = BinaryExpr(loc, op.Substring(0, op.Length-1), x, y);
					return BinaryExpr(loc, "=", x, rhs);
					}
				
				finalCode = String.Format("Op.{0}({1}, {2})", opName, xCode, yCode);
				}
			
			}
		
		return new CSharpRHSExpr(loc, finalCode);
		} // BinaryExpr
	
	
	// Return an expression built from an unary prefix or suffix operator.
	public ExprFrag UnaryExpr(SrcLoc loc, string op, bool isSuffix, ExprFrag x)
		{
		string finalCode;
		if (op == "delete")
			finalCode = ((CSharpExprFrag)x).GenerateDelete();
		else if (op == "++" || op == "--")
			finalCode = ((CSharpExprFrag)x).GenerateIncDec(op == "++", isSuffix);
		else if (op == "typeof")
			finalCode = ((CSharpExprFrag)x).GenerateTypeof();
		else
			{
			string xCode = ((CSharpExprFrag)x).GenerateRHS();
			string opName = (isSuffix) ? (string)(suffixOpTable[op])
									   : (string)(prefixOpTable[op]);
			finalCode = String.Format("Op.{0}({1})", opName, xCode);
			}
		
		return new CSharpRHSExpr(loc, finalCode);
		} // UnaryExpr
	
	
	// Return an expression for an array dereference.
	public ExprFrag ArrayReference(SrcLoc loc, ExprFrag arrayExpr, ExprFrag index)
		{
		string arrayCode = ((CSharpExprFrag)arrayExpr).GenerateRHS();
		string indexCode = ((CSharpExprFrag)index).GenerateRHS();
		return new CSharpArrayRef(loc, arrayCode, indexCode);
		} // ArrayReference
	
	
	// Return an expression for a field dereference.
	public ExprFrag FieldReference(SrcLoc loc, ExprFrag objectExpr, string id)
		{
		string objectCode = ((CSharpExprFrag)objectExpr).GenerateRHS();
		return new CSharpFieldRef(loc, objectCode, id);
		} // FieldReference
	
	
	// Return an expression for a function call.  args is an array of
	// ExprFrags.
	public ExprFrag Call(SrcLoc loc, ExprFrag functionExpr, ArrayList args)
		{
		String code = ((CSharpExprFrag)functionExpr).GenerateCall(args, false);
		return new CSharpRHSExpr(loc, code);
		} // Call
	
	
	// Return an expression for a constructor call (a "new" expression).
	// args is an array of ExprFrags, or null if no parameter list was
	// provided.
	public ExprFrag Construct(SrcLoc loc, ExprFrag functionExpr, ArrayList args)
		{
		String code = ((CSharpExprFrag)functionExpr).GenerateCall(args, true);
		return new CSharpRHSExpr(loc, code);
		} // Construct
	
	
	// Return an expression for the keyword "this".
	public ExprFrag ThisExpr(SrcLoc loc, FunctionInfo info)
		{
		if (info.isRoot)
			return new CSharpRHSExpr(loc, "globals");
		else
			return new CSharpRHSExpr(loc, "this_");
		
		} // ThisExpr
	
	
	// Return an expression for the keyword "null".
	public ExprFrag NullExpr(SrcLoc loc)
		{
		return new CSharpRHSExpr(loc, "null");
		} // NullExpr
	
	
	// Return a BooleanLiteral expression.
	public ExprFrag BoolLiteralExpr(SrcLoc loc, bool b)
		{
		return new CSharpRHSExpr(loc, (b) ? "true" : "false");
		} // BoolLiteralExpr
	
	
	// Return an expression for the given numeric literal.
	public ExprFrag NumLiteralExpr(SrcLoc loc, double d)
		{
		return new CSharpRHSExpr(loc, System.Convert.ToString(d));
		} // NumLiteralExpr
	
	
	// Return a StringLiteral expression.
	public ExprFrag StringLiteralExpr(SrcLoc loc, string s)
		{
		StringBuilder escapedBuilder = new StringBuilder();
		escapedBuilder.Append("@\"");
		
		foreach (char c in s)
			if (c == '"')
				escapedBuilder.Append("\"\"");
			else
				escapedBuilder.Append(c);
		
		escapedBuilder.Append("\"");
		
		return new CSharpRHSExpr(loc, escapedBuilder.ToString());
		} // StringLiteralExpr
	
	
	// Return an expression to reference the given identifier.
	// enclosingWiths should be a stack of WithInfo objects, giving
	// all with scopes in the current function that enclose the identifier
	// reference.
	// 
	// HACK snewman 8/13/01: this method, and ArgumentsExpr below, need a
	// way to search "with" statements in enclosing scopes (for function
	// expressions and expr code).
	public ExprFrag IdentifierExpr( SrcLoc loc, string id,
									FunctionInfo function,
									Stack enclosingWiths )
		{
		// HACK snewman 8/20/01: extend this to support function scopes
		// where not all identifiers are known at compile time (because
		// they invoke "eval").
		
		// Determine which enclosing scope, if any, defines the identifier.
		FunctionInfo bindingScope = function;
		while (bindingScope != null && !bindingScope.IDBoundInThisScope(id))
			bindingScope = bindingScope.parent;
		
		// HACK snewman 8/20/01: add support for locals that are implemented
		// directly as C# locals.  I think this needs to be added in some
		// other places as well, such as EmitFunction.
		if (bindingScope == null || bindingScope.isRoot)
			return new CSharpVariableRef(loc, id, null, enclosingWiths);
		else if (bindingScope == function)
			return new CSharpVariableRef(loc, id, function, enclosingWiths);
		else
			{
			// HACK snewman 8/20/01: add support for referencing a local
			// variable of an enclosing scope.
			throw new ParseError( "Access to parent function variables not yet implemented",
								  loc );
			}
		
		} // IdentifierExpr
	
	
	// Return an expression to reference the identifier "arguments".
	// enclosingWiths should be a stack of WithInfo objects, giving
	// all with scopes in the current function that enclose the identifier
	// reference.
	public ExprFrag ArgumentsExpr( SrcLoc loc, FunctionInfo function,
								   Stack enclosingWiths )
		{
		// HACK snewman 8/20/01: implement this.  Support "with"
		// scopes... we may be able to just fall through into
		// IdentifierExpr.
		throw new ParseError( "\"arguments\" not yet implemented",
							  loc );
		} // ArgumentsExpr
	
	
	// Return a FunctionExpr.
	public ExprFrag FunctionExpr(SrcLoc loc, FunctionInfo childInfo)
		{
		// HACK snewman 8/15/01: implement function expressions.
		throw new ParseError( "Function expressions not yet implemented",
							  loc );
		} // FunctionExpr
	
	
	// Return an ObjectLiteral expression.
	public ExprFrag ObjectLiteralExpr(SrcLoc loc, ObjectLiteralInfo litInfo)
		{
		StringBuilder builder = new StringBuilder();
		builder.Append("Support.LiteralObject(");
		
		CSharpObjectLiteral csInfo = (CSharpObjectLiteral)litInfo;
		bool first = true;
		foreach (CSharpObjectLiteral.Entry entry in csInfo.entries)
			{
			if (first)
				first = false;
			else
				builder.Append(", ");
			
			builder.Append("\"");
			builder.Append(entry.id);
			builder.Append("\", ");
			builder.Append(((CSharpExprFrag)entry.value).GenerateRHS());
			}
		
		builder.Append(")");
		
		return new CSharpRHSExpr(loc, builder.ToString());
		} // ObjectLiteralExpr
	
	
	// Return an ArrayLiteral expression.
	public ExprFrag ArrayLiteralExpr(SrcLoc loc, ArrayLiteralInfo arrayInfo)
		{
		// HACK snewman 8/28/01: properly implement missing elements in
		// the original array literal (commas with no values in between
		// them).  Properly implement the array "length" property.
		
		StringBuilder builder = new StringBuilder();
		builder.Append("Support.LiteralArray(");
		
		CSharpArrayLiteral csInfo = (CSharpArrayLiteral)arrayInfo;
		bool first = true;
		foreach (CSharpExprFrag entry in csInfo.entries)
			{
			if (first)
				first = false;
			else
				builder.Append(", ");
			
			builder.Append(entry.GenerateRHS());
			}
		
		builder.Append(")");
		
		return new CSharpRHSExpr(loc, builder.ToString());
		} // ArrayLiteralInfo
	
	
	// Return a unique method name in the program object.  We either
	// We either return the input string, or the input string with a
	// number and "_" appended.
	private string MakeUniqueMethodName(string baseName)
		{
		int i=1;
		string candidate = baseName;
		while (methodNamesUsed.ContainsKey(candidate))
			{
			i++;
			candidate = baseName + System.Convert.ToString(i) + "_";
			}
		
		methodNamesUsed[candidate] = null;
		return candidate;
		} // MakeUniqueMethodName
	
	
	FunctionInfo rootInfo; // FunctionInfo for the root function of
						   // the program we're generating code for.
	string progClassName;  // C# class name to use for the generated program.
	bool forEvalCode;	   // True if we are compiling eval code, false for
						   // normal source code.
	
	PrettyPrinter  pp;	   // Object to which we send our code output.
	
	// This table holds all names that we've used for methods in the
	// program class.
	private Hashtable methodNamesUsed = new Hashtable();
	} // CSharpGenerator


} // namespace JANET.Compiler
