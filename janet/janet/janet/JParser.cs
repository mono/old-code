// JParser.cs: JANET parser
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

// Namespace for classes associated with compiling ECMAScript source code.
namespace JANET.Compiler {

// This class encapsulates information about an entire program.
public class ProgramInfo
	{
	public ProgramInfo()
		{
		potentialGlobals = new Hashtable();
		}
	
	
	// Note the given id as being a potential global variable name.
	public void PotentialGlobal(string id)
		{
		potentialGlobals[id] = null;
		} // PotentialGlobal
	
	
	private Hashtable potentialGlobals; // All identifiers used in the
									    // program which are not provably
										// bound in a non-global scope.
	} // ProgramInfo


// This class encapsulates information about a function and its nested
// functions.
public class FunctionInfo
	{
	public ProgramInfo  program        { get { return program_;          }}
	public FunctionInfo parent         { get { return parent_;           }}
	public FunctionInfo firstChild     { get { return firstChild_;       }}
	public FunctionInfo nextSib        { get { return nextSib_;          }}
	public string       nameInParent   { get { return nameInParent_;     }}
	public bool         callsEval      { get { return callsEval_;        }}
	public bool         usesArgs       { get { return usesArgs_;         }}
	public bool			isRoot         { get { return (parent_ == null); }}
	
	public StringCollection paramNames { get { return params_;           }}
	public StringCollection locals     { get { return locals_;           }}
	
	
	// Construct a FunctionInfo object.  parent should be the info object
	// for the enclosing function, or null if we are the root program.
	// nameInParent should be the function name, or null for functions that
	// are defined in an expression (even if the function is named in the
	// expression, since such names don't create an entry in the parent's
	// scope).  nameInParent should also be null for the root function.
	// 
	// If parent is not null, we add ourselves to its children list.
	public FunctionInfo(ProgramInfo program, FunctionInfo parent, string nameInParent)
		{
		program_    = program;
		parent_     = parent;
		firstChild_ = null;
		lastChild_  = null;
		nextSib_    = null;
		childScan_  = null;
		
		nameInParent_ = nameInParent;
		
		callsEval_ = false;
		usesArgs_  = false;
		
		params_ = new StringCollection();
		locals_ = new StringCollection();
		
		// Register ourselves in our parent scope.
		if (parent != null)
			{
			if (parent.firstChild_ == null)
				{
				Trace.Assert(parent.lastChild_ == null);
				parent.firstChild_ = this;
				parent.childScan_  = this;
				parent.lastChild_  = this;
				}
			else
				{
				Trace.Assert(parent.lastChild_ != null);
				parent.lastChild_.nextSib_ = this;
				parent.lastChild_ = this;
				}
			
			}
		
		} // FunctionInfo constructor
	
	
	// Return true if the given identifier is bound in this scope.
	public bool IDBoundInThisScope(string id)
		{
		if (params_.Contains(id) || locals_.Contains(id))
			return true;
		
		for ( FunctionInfo childScan = firstChild_; childScan != null;
			  childScan = childScan.nextSib_ )
			if (childScan.nameInParent_ == id)
				return true;
		
		return false;
		} // IDBoundInThisScope
	
	
	// Return true if the given identifier is bound in this scope or any
	// containing scope, other than the global scope.
	public bool IDHasNonglobalBinding(string id)
		{
		if (parent_ == null)
			return false; // we are the global scope
		
		if (IDBoundInThisScope(id))
			return true;
		
		return parent.IDHasNonglobalBinding(id);
		} // IDHasNonglobalBinding
	
	
	// This should only be called during phase 2 parsing.  Calls to this
	// function return each of our (immediate) child functions in turn.
	public FunctionInfo GetNextChild()
		{
		FunctionInfo result = childScan_;
		childScan_ = childScan_.nextSib;
		return result;
		} // GetNextChild
	
	
	// Add an entry to the list of formal parameter names for this function.
	public void AddParamName(string id)
		{
		params_.Add(id);
		} // AddParamName
	
	
	// Add an entry to the list of local variable names for this function.
	public void AddLocalName(string id)
		{
		locals_.Add(id);
		} // AddLocalName
	
	
	// Note that this function invokes "eval".
	public void CallsEval()
		{
		callsEval_ = true;
		} // CallsEval
	
	
	// Note that this function references "arguments".
	public void UsesArgs()
		{
		usesArgs_ = true;
		} // UsesArgs
	
	
	private ProgramInfo  program_;    // Our enclosing program object.
	private FunctionInfo parent_;     // Our enclosing function, or null if we
								      // are a root program.
	private FunctionInfo firstChild_; // Header to a linked list of child functions.
	private FunctionInfo lastChild_;  // Last child function, or null if none.
	private FunctionInfo nextSib_;    // Next entry in the linked list of children
								      // for our parent.
	
	private FunctionInfo childScan_;  // During phase 1 parsing, this is equal
									  // to firstChild_.  During phase 2 parsing,
									  // it points to the first child function
									  // which we have not yet begun parsing.
									  // Used to extract the appropriate
									  // FunctionInfo when we begin parsing a
									  // child function.
	
	private string nameInParent_;     // Name under which we are declared in our
								      // parent.  Null for the root program or for
								      // a function which appears within an
								      // expression (even if the function is named
								      // in the expression, since such names don't
								      // create an entry in the parent's scope).
	private bool callsEval_;		  // True if this function invokes "eval".
	private bool usesArgs_;		      // True if this function references the
								      // "arguments" variable.
	private StringCollection params_; // Formal parameter names
	private StringCollection locals_; // Local variable names
	} // FunctionInfo


// HACK snewman 8/7/01: add support for implied semicolons to all parser phases

// Base class for parsing ECMAScript programs.  Contains utilities to
// do the grunt work of parsing.  Subclasses use these utilities to
// quickly implement a recursive descent parser.
public class JParserBase
	{
	protected JParserBase(TokenizerBase tokenStream)
		{
		this.tok = tokenStream;
		} // JParserBase constructor
	
	
	// Throw a ParseError at the location of the last matched token.
	protected void ReportError(string msg)
		{
		throw new ParseError(msg, tok.Prev().loc);
		} // ReportError
	
	
	protected struct OperatorInfo
		{
		public enum Types { unaryPrefix, unarySuffix, binary, assignment, dummy };
		
		public string text;       // The raw text of the operator
		public int    precedence; // Operator precedence (lower values bind tighter).
		public Types  opType;	  // What sort of operator this is.
		
		public OperatorInfo(string text, int precedence, Types opType)
			{ this.text = text; this.precedence = precedence; this.opType = opType; }
		}
	
	
	// This table lists all postfix unary, binary and trinary operators.
	private static OperatorInfo[] binaryOpTable =
		{
		new OperatorInfo( "++",         1,  OperatorInfo.Types.unarySuffix ),
		new OperatorInfo( "--",         1,  OperatorInfo.Types.unarySuffix ),
		
		new OperatorInfo( "*",          2,  OperatorInfo.Types.binary ),
		new OperatorInfo( "/",          2,  OperatorInfo.Types.binary ),
		new OperatorInfo( "%",          2,  OperatorInfo.Types.binary ),
		
		new OperatorInfo( "+",          3,  OperatorInfo.Types.binary ),
		new OperatorInfo( "-",          3,  OperatorInfo.Types.binary ),
		
		new OperatorInfo( "<<",         4,  OperatorInfo.Types.binary ),
		new OperatorInfo( ">>",         4,  OperatorInfo.Types.binary ),
		new OperatorInfo( ">>>",        4,  OperatorInfo.Types.binary ),
		
		new OperatorInfo( "<",          5,  OperatorInfo.Types.binary ),
		new OperatorInfo( ">",          5,  OperatorInfo.Types.binary ),
		new OperatorInfo( "<=",         5,  OperatorInfo.Types.binary ),
		new OperatorInfo( ">=",         5,  OperatorInfo.Types.binary ),
		new OperatorInfo( "instanceof", 5,  OperatorInfo.Types.binary ),
		new OperatorInfo( "in",         5,  OperatorInfo.Types.binary ),
		
		new OperatorInfo( "==",         6,  OperatorInfo.Types.binary ),
		new OperatorInfo( "!=",         6,  OperatorInfo.Types.binary ),
		new OperatorInfo( "===",        6,  OperatorInfo.Types.binary ),
		new OperatorInfo( "!==",        6,  OperatorInfo.Types.binary ),
		
		new OperatorInfo( "&",          7,  OperatorInfo.Types.binary ),
		
		new OperatorInfo( "^",          8,  OperatorInfo.Types.binary ),
		
		new OperatorInfo( "|",          9,  OperatorInfo.Types.binary ),
		
		new OperatorInfo( "&&",        10,  OperatorInfo.Types.binary ),
		
		new OperatorInfo( "||",        11,  OperatorInfo.Types.binary ),
		
		new OperatorInfo( "?",         12,  OperatorInfo.Types.binary ),
		
		new OperatorInfo( "=",		   13,  OperatorInfo.Types.assignment ),
		new OperatorInfo( "*=",		   13,  OperatorInfo.Types.assignment ),
		new OperatorInfo( "/=",		   13,  OperatorInfo.Types.assignment ),
		new OperatorInfo( "%=",		   13,  OperatorInfo.Types.assignment ),
		new OperatorInfo( "+=",		   13,  OperatorInfo.Types.assignment ),
		new OperatorInfo( "-=",		   13,  OperatorInfo.Types.assignment ),
		new OperatorInfo( "<<=",	   13,  OperatorInfo.Types.assignment ),
		new OperatorInfo( ">>=",	   13,  OperatorInfo.Types.assignment ),
		new OperatorInfo( ">>>=",	   13,  OperatorInfo.Types.assignment ),
		new OperatorInfo( "&=",		   13,  OperatorInfo.Types.assignment ),
		new OperatorInfo( "^=",		   13,  OperatorInfo.Types.assignment ),
		new OperatorInfo( "|=",		   13,  OperatorInfo.Types.assignment ),
		
		new OperatorInfo( ",",		   14,  OperatorInfo.Types.binary ),
		
		new OperatorInfo( "",        9999,  OperatorInfo.Types.dummy )
		};
	
	protected const int unarySuffixPrec = 1;
	
	protected static OperatorInfo opAssignInfo =
		new OperatorInfo( "=",		   13,  OperatorInfo.Types.assignment );
	
	// Constant for the precendence of the assignment operator.
	protected const int assignmentPrec = 13;
	
	// This table lists all prefix unary operators.
	private static OperatorInfo[] unaryOpTable =
		{
		new OperatorInfo( "delete",     0,  OperatorInfo.Types.unaryPrefix ),
		new OperatorInfo( "void",       0,  OperatorInfo.Types.unaryPrefix ),
		new OperatorInfo( "typeof",     0,  OperatorInfo.Types.unaryPrefix ),
		new OperatorInfo( "++",         0,  OperatorInfo.Types.unaryPrefix ),
		new OperatorInfo( "--",         0,  OperatorInfo.Types.unaryPrefix ),
		new OperatorInfo( "+",          0,  OperatorInfo.Types.unaryPrefix ),
		new OperatorInfo( "-",          0,  OperatorInfo.Types.unaryPrefix ),
		new OperatorInfo( "~",          0,  OperatorInfo.Types.unaryPrefix ),
		new OperatorInfo( "!",          0,  OperatorInfo.Types.unaryPrefix ),
		
		new OperatorInfo( "",        9999,  OperatorInfo.Types.dummy )
		};
	
	
	// If the next token is an operator that appears in our table of
	// suffix and multiple-parameter operators, fill in matchedOp and return
	// true.  Otherwise return false.
	// 
	// If allowIN is false, then we ignore (don't match) the "in" operator.
	// 
	// We ignore (don't match) any operator whose precedence is greater
	// than maxPrecedence.
	// 
	// If maxPrecedence is negative, then we instead match prefix unary operators.
	// In this case, allowIn is ignored, and the specific value of maxPrecedence
	// is unimportant.
	// 
	// If consumeToken is true, then we consume the operator token.  Otherwise,
	// or if no operator is found, we do not consume any token.
	protected bool TryMatchOperator( bool allowIN, int maxPrecedence,
								     out OperatorInfo matchedOp,
									 bool consumeToken )
		{
		OperatorInfo[] table = (maxPrecedence == -1) ? unaryOpTable : binaryOpTable;
		
		int dummyOpIndex = table.Length - 1;
		
		Token peek = tok.Peek();
		if ( peek == null ||
			 (peek.type != Token.Type.op && peek.type != Token.Type.keyword) )
			{
			matchedOp = table[dummyOpIndex];
			return false;
			}
		
		// PERFORMANCE snewman 8/8/01: this could easily be optimized, by
		// using a hash table instead of a linear search.
		
		for (int i=0; i<dummyOpIndex; i++)
			{
			if (maxPrecedence >= 0 && table[i].precedence > maxPrecedence)
				break;
			
			if (table[i].text == peek.rawText)
				if (maxPrecedence < 0 || allowIN || table[i].text != "in")
					{
					matchedOp = table[i];
					if (consumeToken)
						tok.Match();
					
					return true;
					}
			
			}
		
		matchedOp = table[dummyOpIndex];
		return false;
		} // TryMatchOperator
	
	
	protected TokenizerBase tok; // The token stream that we are parsing.
	} // JParserBase


// First pass parser: scans the program to determine the function structure,
// identifier declarations and usage, and other summary information.
public class Phase1Parser : JParserBase
	{
	public Phase1Parser(TokenizerBase tokenStream) : base(tokenStream) {}
	
	
	// Parse a program from the input stream, filling in FunctionInfo.
	public void ParseProgram(FunctionInfo info)
		{
		ParseSourceElements(info);
		} // ParseProgram
	
	
	// Parse a SourceElements (i.e. the main program, or a function
	// body), and fill in FunctionInfo.
	private void ParseSourceElements(FunctionInfo info)
		{
		while (!tok.atEnd && !tok.PeekOp("}"))
			if (tok.TryMatchKeyword("function"))
				ParseFunction(info, false);
			else
				ParseStatement(info);
		
		} // ParseSourceElements
	
	
	// Parse a FunctionDeclaration or FunctionExpression.  The "function"
	// keyword should already have been matched.
	private void ParseFunction(FunctionInfo parent, bool isExpression)
		{
		string functionName = null;
		
		if (isExpression)
			tok.TryMatchID(out functionName);
		else
			functionName = tok.MatchID();
		
		FunctionInfo info = new FunctionInfo( parent.program, parent,
											  (isExpression) ? null : functionName );
		
		tok.MatchOp("(");
		if (!tok.PeekOp(")"))
			{
			do
				info.AddParamName(tok.MatchID());
			while (tok.TryMatchOp(","));
			}
		
		tok.MatchOp(")");
		tok.MatchOp("{");
		ParseSourceElements(info);
		tok.MatchOp("}");
		} // ParseFunction
	
	
	// Parse a Block.  The stream should be positioned at the "{" token.
	private void ParseBlock(FunctionInfo info)
		{
		tok.MatchOp("{");
		while (!tok.TryMatchOp("}"))
			ParseStatement(info);
		
		} // ParseBlock
	
	
	// Parse a Statement, filling in info with any declarations.
	private void ParseStatement(FunctionInfo info)
		{
		if (tok.PeekOp("{"))
			ParseBlock(info);
		else if (tok.TryMatchKeyword("var"))
			{
			do
				ParseVariableDeclaration(info, true);
			while (tok.TryMatchOp(","));
			tok.MatchOp(";");
			}
		else if (tok.TryMatchOp(";"))
			{
			// EmptyStatement
			}
		else if (tok.TryMatchKeyword("if"))
			{
			tok.MatchOp("(");
			ParseExpression(info, true);
			tok.MatchOp(")");
			
			ParseStatement(info);
			if (tok.TryMatchKeyword("else"))
				ParseStatement(info);
			
			}
		else if (tok.TryMatchKeyword("do"))
			{
			ParseStatement(info);
			tok.MatchKeyword("while");
			tok.MatchOp("(");
			ParseExpression(info, true);
			tok.MatchOp(")");
			tok.MatchOp(";");
			}
		else if (tok.TryMatchKeyword("while"))
			{
			tok.MatchOp("(");
			ParseExpression(info, true);
			tok.MatchOp(")");
			ParseStatement(info);
			}
		else if (tok.TryMatchKeyword("for"))
			{
			tok.MatchOp("(");
			
			bool isIn;
			bool isLHS = true;
			if (tok.TryMatchKeyword("var"))
				{
				ParseVariableDeclaration(info, false);
				
				isIn = tok.TryMatchKeyword("in");
				if (!isIn)
					while (tok.TryMatchOp(","))
						ParseVariableDeclaration(info, false);
				
				}
			else
				{
				if (!tok.PeekOp(";"))
					ParseExpression(info, false, 99, out isLHS);
				
				isIn = tok.TryMatchKeyword("in");
				}
			
			if (isIn)
				{
				if (!isLHS)
					ReportError("illegal expression in left side of for/in");
				ParseExpression(info, true);
				}
			else
				{
				tok.MatchOp(";");
				if (!tok.PeekOp(";"))
					ParseExpression(info, true);
				
				tok.MatchOp(";");
				if (!tok.PeekOp(")"))
					ParseExpression(info, true);
				}
			
			tok.MatchOp(")");
			ParseStatement(info);
			}
		else if (tok.TryMatchKeyword("continue"))
			{
			// HACK snewman 8/7/01: the grammar (ContinueStatement) specifies
			// "no LineTerminator here".
			string id;
			tok.TryMatchID(out id);
			tok.MatchOp(";");
			}
		else if (tok.TryMatchKeyword("break"))
			{
			// HACK snewman 8/7/01: the grammar (BreakStatement) specifies
			// "no LineTerminator here".
			string id;
			tok.TryMatchID(out id);
			tok.MatchOp(";");
			}
		else if (tok.TryMatchKeyword("return"))
			{
			// HACK snewman 8/7/01: the grammar (ReturnStatement) specifies
			// "no LineTerminator here".
			if (!tok.TryMatchOp(";"))
				{
				ParseExpression(info, true);
				tok.MatchOp(";");
				}
			
			}
		else if (tok.TryMatchKeyword("with"))
			{
			tok.MatchOp("(");
			ParseExpression(info, true);
			tok.MatchOp(")");
			ParseStatement(info);
			}
		else if (tok.TryMatchKeyword("switch"))
			{
			tok.MatchOp("(");
			ParseExpression(info, true);
			tok.MatchOp(")");
			
			tok.MatchOp("{");
			bool seenDefault = false;
			while (!tok.TryMatchOp("}"))
				{
				if (tok.TryMatchKeyword("default"))
					{
					if (seenDefault)
						{
						// HACK snewman 8/7/01: provide the location of the first clause in
						// the error message.
						ReportError("duplicate default clauses in switch");
						}
					else
						seenDefault = true;
					
					}
				else
					{
					tok.MatchKeyword("case");
					ParseExpression(info, true);
					}
				
				tok.MatchOp(":");
				while ( !tok.PeekOp("}") && !tok.PeekKeyword("case") &&
					    !tok.PeekKeyword("default") )
					ParseStatement(info);
				
				}
			
			}
		else if (tok.TryMatchKeyword("throw"))
			{
			// HACK snewman 8/7/01: the grammar (ThrowStatement) specifies
			// "no LineTerminator here".
			ParseExpression(info, true);
			tok.MatchOp(";");
			}
		else if (tok.TryMatchKeyword("try"))
			{
			ParseBlock(info);
			if (tok.TryMatchKeyword("catch"))
				{
				tok.MatchOp("(");
				string catchVar = tok.MatchID();
				
				// HACK snewman 8/9/01: catchVar is bound in all code inside
				// the catch block; we should note this, so that references
				// to it are not interpreted as potential globals.
				
				tok.MatchOp(")");
				ParseBlock(info);
				
				if (tok.TryMatchKeyword("finally"))
					ParseBlock(info);
				}
			else if (tok.TryMatchKeyword("finally"))
				ParseBlock(info);
			else
				ReportError("try block must be followed by \"catch\" or \"finally\" (or both)");
			
			}
		else if ( tok.Peek() != null && tok.Peek().type == Token.Type.id &&
				  tok.Peek2() != null && tok.Peek2().rawText == ":" )
			{
			// LabeledStatement
			tok.MatchID();
			tok.MatchOp(":");
			ParseStatement(info);
			}
		else if (tok.TryMatchKeyword("function"))
			ReportError("function declarations not allowed within a statement list");
		else
			{
			ParseExpression(info, true);
			tok.MatchOp(";");
			}
		
		} // ParseStatement
	
	
	// Parse a VariableDeclaration, filling in info with any declarations.
	// See ParseExpression for an explanation of allowIN.
	private void ParseVariableDeclaration(FunctionInfo info, bool allowIN)
		{
		info.AddLocalName(tok.MatchID());
		if (tok.TryMatchOp("="))
			ParseExpression(info, allowIN, assignmentPrec);
		
		} // ParseVariableDeclaration
	
	
	// Parse an Expression, filling in info with any declarations.
	// 
	// If allowIN is false, then we don't look for "in" operators.  (This
	// implements the "NoIn" variant of the grammar, e.g. ExpressionNoIn
	// as opposed to Expression.)
	private void ParseExpression(FunctionInfo info, bool allowIN)
		{
		ParseExpression(info, allowIN, 99);
		} // ParseExpression
	
	
	// Simplified version of ParseExpression (no isLHS parameter).
	private void ParseExpression( FunctionInfo info, bool allowIN,
								 int maxPrecedence )
		{
		bool isLHS; // Unused result
		ParseExpression(info, allowIN, maxPrecedence, out isLHS);
		} // ParseExpression
	
	
	// Parse an Expression, filling in info with any declarations.
	// 
	// If allowIN is false, then we don't look for "in" operators.  (This
	// implements the "NoIn" variant of the grammar, e.g. ExpressionNoIn
	// as opposed to Expression.)
	// 
	// This method can also be used to parse "smaller" nonterminals such
	// as AssignmentExpression, ConditionalExpression, and so on down to
	// MultiplicativeExpression.  This is controlled by the maxPrecedence
	// parameter: we ignore any operators with a looser (numerically
	// greater) precedence than maxPrecedence.
	// 
	// We set isLHS to true if this was a simple LeftHandSideExpression,
	// false if it included any operators.
	void ParseExpression( FunctionInfo info, bool allowIN,
						  int maxPrecedence, out bool isLHS )
		{
		// Note that we don't strictly follow the ECMAScript grammar here.
		// The grammar is designed to prevent assignment to a non-lvalue.
		// This is a hassle to implement in the parser, and leads to confusing
		// error messages.  Instead, we allow any expression (except an
		// assignment expression) to parse as the left-hand side of an
		// assignment, but then we report an error if the parsed LHS is not
		// a valid LeftHandSideExpression.
		
		isLHS = true;
		
		// Parse any prefix operators.
		OperatorInfo matchedOp;
		while (TryMatchOperator(true, -1, out matchedOp, true))
			isLHS = false;
		
		ParseLeftHandSideExpression(info, true);
		
		while (TryMatchOperator(allowIN, maxPrecedence, out matchedOp, true))
			{
			if (matchedOp.text == "?")
				{
				ParseExpression(info, allowIN, assignmentPrec);
				tok.MatchOp(":");
				ParseExpression(info, allowIN, assignmentPrec);
				}
			else if (matchedOp.opType == OperatorInfo.Types.assignment)
				{
				if (!isLHS)
					ReportError("illegal expression in left side of assignment");
				ParseExpression( info, allowIN, matchedOp.precedence,
								 out isLHS );
				}
			else if (matchedOp.opType == OperatorInfo.Types.binary)
				ParseExpression( info, allowIN, matchedOp.precedence-1,
								 out isLHS );
			
			isLHS = false;
			}
		
		} // ParseExpression
	
	
	// Parse a LeftHandSideExpression, filling in info with any declarations.
	// 
	// If checkForCallExpr is false, then we don't check for CallExpressions.
	// This is used when parsing the function in a "new" expression, to
	// avoid parsing arguments to "new" as part of the expression that
	// specifies the function.
	void ParseLeftHandSideExpression(FunctionInfo info, bool checkForCallExpr)
		{
		if (tok.TryMatchKeyword("new"))
			{
			ParseLeftHandSideExpression(info, false);
			if (tok.TryMatchOp("("))
				MatchOptionalArgumentList(info);
			
			}
		else if (tok.TryMatchKeyword("function"))
			ParseFunction(info, true);
		else
			ParsePrimaryExpression(info);
		
		while (true)
			if (checkForCallExpr && tok.TryMatchOp("("))
				MatchOptionalArgumentList(info);
			else if (tok.TryMatchOp("["))
				{
				this.ParseExpression(info, true);
				tok.MatchOp("]");
				}
			else if (tok.TryMatchOp("."))
				tok.MatchID();
			else
				break;
		
		} // ParseLeftHandSideExpression
	
	
	// Parse a PrimaryExpression, filling in info with any declarations.
	void ParsePrimaryExpression(FunctionInfo info)
		{
		string id;
		double d;
		string s;
		
		if (tok.TryMatchKeyword("this"))
			{}
		else if (tok.TryMatchKeyword("null"))
			{}
		else if (tok.TryMatchKeyword("true"))
			{}
		else if (tok.TryMatchKeyword("false"))
			{}
		else if (tok.TryMatchOp("("))
			{
			ParseExpression(info, true);
			tok.MatchOp(")");
			}
		else if (tok.TryMatchOp("{"))
			{
			if (!tok.PeekOp("}"))
				{
				do
					{
					if (tok.TryMatchNumLit(out d))
						{}
					else if (tok.TryMatchStringLit(out s))
						{}
					else
						tok.MatchID();
					
					tok.MatchOp(":");
					ParseExpression(info, true, assignmentPrec);
					}
				while (tok.TryMatchOp(","));
				}
			
			tok.MatchOp("}");
			}
		else if (tok.TryMatchOp("["))
			{
			bool requireComma = false;
			while (!tok.PeekOp("]"))
				if (tok.TryMatchOp(","))
					requireComma = false;
				else if (!requireComma)
					{
					ParseExpression(info, true, assignmentPrec);
					requireComma = true;
					}
				else
					break;
			
			tok.MatchOp("]");
			}
		else if (tok.TryMatchID(out id))
			{
			if (id == "eval")
				info.CallsEval();
			else if (id == "arguments")
				info.UsesArgs();
			
			// If the id is not bound in any enclosing scope, then list it
			// as a potential global.  Note that this is conservative -- it
			// may actually be bound by a declaration we haven't parsed yet.
			// 
			// HACK snewman 8/9/01: as currently implemented, this will not
			// filter out identifiers that are bound by an enclosing "catch"
			// clause.
			if (!info.IDHasNonglobalBinding(id))
				info.program.PotentialGlobal(id);
			
			}
		else if (tok.TryMatchNumLit(out d))
			{}
		else if (tok.TryMatchStringLit(out s))
			{}
		else
			{
			tok.Match();
			ReportError("bad token when an expression was expected");
			}
		
		} // ParsePrimaryExpression
	
	
	// Match an ArgumentList (if any) and the trailing ')'.  Fill in info
	// with any declarations.
	void MatchOptionalArgumentList(FunctionInfo info)
		{
		if (!tok.PeekOp(")"))
			{
			do
				ParseExpression(info, true, assignmentPrec);
			while (tok.TryMatchOp(","));
			}
		
		tok.MatchOp(")");
		} // MatchOptionalArgumentList
	
	} // Phase1Parser

} // namespace JANET.Compiler
