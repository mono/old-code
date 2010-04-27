// JCodeGeneratorIntf.cs: JANET code generation interface
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

using JANET.Compiler;


// Namespace for classes associated with generating output code.
namespace JANET.Compiler {


// Base interface for a chunk of generated code
public interface CodeFrag
	{
	// Return the location of the first or primary source token associated
	// with this fragment.
	SrcLoc loc { get; }
	} // CodeFrag


// Interface for a code fragment which yields a value (lvalue or rvalue).
public interface ExprFrag : CodeFrag
	{
	}


// Interface for a code fragment which does not yield a value.
public interface StmtFrag : CodeFrag
	{
	// Append the given fragment to this fragment, so that they execute
	// sequentially.  (NOTE: once this has been done, the "frag" parameter
	// is linked into this fragment, and should no longer be manipulated
	// independently.)
	void Append(StmtFrag frag);
	}


// This interface represents an object used by a CodeGenerator to keep
// track of code generation within a particular function.  A separate
// CGFuncInfo object should be created for each function in the program,
// including the main function.
public interface CGFuncInfo
	{
	} // CGFuncInfo


// This interface represents an object used by a CodeGenerator to keep
// track of a loop statement and manage "break" and "continue" statements
// inside it.  It is also used to track switch statements, and any labeled
// statement.
public interface LoopInfo
	{
	}


// This interface represents an object used by a CodeGenerator to keep
// track of a with statement and manage variable references inside it.
// Also used to manage the variable defined by a "catch" clause.
public interface WithInfo
	{
	}


// This interface represents an object which is used to keep track of
// the entries in an ObjectLiteral expression.
public interface ObjectLiteralInfo
	{
	// Add a new property to the literal object.
	void AddProp(SrcLoc loc, string id, ExprFrag value);
	}


// This interface represents an object which is used to keep track of
// the entries in an ArrayLiteral expression.
public interface ArrayLiteralInfo
	{
	// Add a new entry to the literal array.  The value parameter can
	// be null, in which case we store null in the array.
	// 
	// HACK snewman 8/13/01: actually, I don't think this action for
	// a null value is correct.  Need to review the ECMA spec for array
	// literals (I tried once, but I got a headache).
	void AddEntry(ExprFrag value);
	}


// This interface represents an object which is used to keep track of
// the entries in a switch statement.
public interface SwitchInfo
	{
	// Add a new clause to the switch statement.  caseValue should be
	// the expression after the keyword "case", or null if this is the
	// default clause.  caseStmt should be the statements for this
	// clause, or null if none (i.e. if this clause simply falls through
	// to the next clause or the end of the switch).
	void AddCase(ExprFrag caseValue, StmtFrag caseStmt);
	}


// Interface for the main code generation library.
public interface ICodeGenerator
	{
	// Create a CGFuncInfo object for the given function.
	CGFuncInfo NewFuncInfo(FunctionInfo func);
	
	// Generate prefix code for the program.  This should be called before
	// any other emit calls.
	void EmitProgramPrefix();
	
	// Generate suffix code for the program.  This should be called after
	// any other emit calls.
	void EmitProgramSuffix();
	
	// Generate code for the main function, given the function's body.
	void EmitMainFunction(StmtFrag body);
	
	// Generate code for the given function (other than the main function),
	// given the function's FunctionInfo object and its body.
	void EmitFunction(FunctionInfo info, StmtFrag body);
	
	// Return a new, empty StmtFrag object.
	StmtFrag NewStmtFrag(SrcLoc loc);
	
	// Return a new LoopInfo object for an iteratiion statement with the
	// given label set.  The labels parameter can be null if there were
	// no labels.
	LoopInfo NewLoopInfo(CGFuncInfo cgFuncInfo, SrcLoc loc, StringCollection labels);
	
	// Return a new LoopInfo object for a labeled non-iteration statement
	// with the given label set.  isSwitch should be true if the statement
	// was a switch statement; in this case, if the statement had no labels,
	// the labels parameter can be null.
	LoopInfo NewLabeledStmtInfo( CGFuncInfo cgFuncInfo, SrcLoc loc,
								 StringCollection labels, bool isSwitch );
	
	// Return a new WithInfo object.
	WithInfo NewWithInfo(CGFuncInfo cgFuncInfo, SrcLoc loc);
	
	// Return a new, empty ObjectLiteralInfo object.
	ObjectLiteralInfo NewObjectLiteralInfo();
	
	// Return a new, empty ArrayLiteralInfo object.
	ArrayLiteralInfo NewArrayLiteralInfo();
	
	// Return a new, empty SwitchInfo object.
	SwitchInfo NewSwitchInfo();
	
	// Return a statement which evaluates the given expression.
	StmtFrag ExpressionStmt(ExprFrag expr);
	
	// Return an if/then or if/then/else statement.  If there was no else
	// clause, elseClause will be null.
	StmtFrag IfThenElse( SrcLoc loc, ExprFrag condition, StmtFrag ifClause,
						 StmtFrag elseClause );
	
	// Return a do...while statement.
	StmtFrag DoWhile(LoopInfo loopInfo, StmtFrag body, ExprFrag condition);
	
	// Return a while...do statement.
	StmtFrag WhileDo(LoopInfo loopInfo, ExprFrag condition, StmtFrag body);
	
	// Return a for statement.  init is the loop initializer, cond is the
	// loop control expression, and step is the loop increment expression.
	// Any or all of init, cond, and step can be null.
	StmtFrag For(LoopInfo loopInfo, StmtFrag init, ExprFrag cond, ExprFrag step, StmtFrag body);
	
	// Return a for/in statement.
	StmtFrag ForIn(LoopInfo loopInfo, StmtFrag init, ExprFrag lhs, ExprFrag rhs, StmtFrag body);
	
	// Return a continue statement.  label will be null if no label was
	// specified in the statement.  enclosingLoops should be a stack of
	// LoopInfo objects for all enclosing loops and labeled statements.
	StmtFrag Continue(SrcLoc loc, string label, Stack enclosingLoops);
	
	// Return a break statement.  label will be null if no label was
	// specified in the statement.  enclosingLoops should be a stack of
	// LoopInfo objects for all enclosing loops and labeled statements.
	StmtFrag Break(SrcLoc loc, string label, Stack enclosingLoops);
	
	// Return a statement which associated the specified LoopInfo with
	// an enclosing statement.  This should be called for switch statements,
	// and for any labeled statement which is not a loop statement.
	StmtFrag LabeledStmt(LoopInfo loopInfo, StmtFrag stmt);
	
	// Return a return statement.  value will be null if no value was
	// specified in the statement.
	StmtFrag Return(SrcLoc loc, ExprFrag value);
	
	// Return a throw statement.
	StmtFrag Throw(SrcLoc loc, ExprFrag value);
	
	// Return a with statement.
	StmtFrag With(WithInfo withInfo, ExprFrag value, StmtFrag body);
	
	// Return a switch statement.
	StmtFrag Switch(SrcLoc loc, ExprFrag switchValue, SwitchInfo switchInfo);
	
	// Return a try...catch, try...finally, or try...catch...finally
	// statement.  If there was no "catch" clause, then catchVar, catchWithInfo,
	// and catchBody will be null.  If there was no "finally" clause, then
	// finallyBody will be null.  These parameters can't all be null (i.e.
	// at least one of the "catch" and "finally" clauses must have been
	// present).
	StmtFrag TryCatchFinally( SrcLoc loc,
							  StmtFrag tryBody, string catchVar,
							  WithInfo catchWithInfo, StmtFrag catchBody,
							  StmtFrag finallyBody );
	
	// Return a ?: expression.
	ExprFrag ConditionalExpr( SrcLoc loc, ExprFrag condition,
							  ExprFrag p1, ExprFrag p2 );
	
	// Return an expression built from a binary operator.
	ExprFrag BinaryExpr( SrcLoc loc, string op, ExprFrag x,
						 ExprFrag y );
	
	// Return an expression built from an unary prefix or suffix operator.
	ExprFrag UnaryExpr(SrcLoc loc, string op, bool isSuffix, ExprFrag x);
	
	// Return an expression for an array dereference.
	ExprFrag ArrayReference(SrcLoc loc, ExprFrag arrayExpr, ExprFrag index);
	
	// Return an expression for a field dereference.
	ExprFrag FieldReference(SrcLoc loc, ExprFrag objectExpr, string id);
	
	// Return an expression for a function call.  args is an array of
	// ExprFrags.
	ExprFrag Call(SrcLoc loc, ExprFrag functionExpr, ArrayList args);
	
	// Return an expression for a constructor call (a "new" expression).
	// args is an array of ExprFrags, or null if no parameter list was
	// provided.
	ExprFrag Construct(SrcLoc loc, ExprFrag functionExpr, ArrayList args);
	
	// Return an expression for the keyword "this".
	ExprFrag ThisExpr(SrcLoc loc, FunctionInfo info);
	
	// Return an expression for the keyword "null".
	ExprFrag NullExpr(SrcLoc loc);
	
	// Return a BooleanLiteral expression.
	ExprFrag BoolLiteralExpr(SrcLoc loc, bool b);
	
	// Return an expression for the given numeric literal.
	ExprFrag NumLiteralExpr(SrcLoc loc, double d);
	
	// Return a StringLiteral expression.
	ExprFrag StringLiteralExpr(SrcLoc loc, string s);
	
	// Return an expression to reference the given identifier.
	// enclosingWiths should be a stack of WithInfo objects, giving
	// all with scopes in the current function that enclose the identifier
	// reference.
	ExprFrag IdentifierExpr( SrcLoc loc, string id, FunctionInfo function,
							 Stack enclosingWiths );
	
	// Return an expression to reference the identifier "arguments".
	// enclosingWiths should be a stack of WithInfo objects, giving
	// all with scopes in the current function that enclose the identifier
	// reference.
	ExprFrag ArgumentsExpr( SrcLoc loc, FunctionInfo function,
							Stack enclosingWiths );
	
	// Return a FunctionExpr.
	ExprFrag FunctionExpr(SrcLoc loc, FunctionInfo childInfo);
	
	// Return an ObjectLiteral expression.
	ExprFrag ObjectLiteralExpr(SrcLoc loc, ObjectLiteralInfo litInfo);
	
	// Return an ArrayLiteral expression.
	ExprFrag ArrayLiteralExpr(SrcLoc loc, ArrayLiteralInfo arrayInfo);
	} // ICodeGenerator


} // namespace JANET.Compiler
