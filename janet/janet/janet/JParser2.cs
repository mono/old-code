// JParser2.cs: JANET parser, second phase (code generation)
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

// Second pass parser: scans the program and generates code, using the
// information gathered during the first phase.
public class Phase2Parser : JParserBase
	{
	public Phase2Parser( TokenizerBase tokenStream,
						 ICodeGenerator gen )
	 : base(tokenStream)
		{
		this.gen = gen;
		
		loops = new Stack();
		withs = new Stack();
		} // Phase2Parser constructor
	
	
	// Parse a program from the input stream, given the FunctionInfo
	// object for the program's root scope.
	public void ParseProgram(FunctionInfo info)
		{
		gen.EmitProgramPrefix();
		
		CGFuncInfo cgFuncInfo = gen.NewFuncInfo(info);
		
		curCGFuncInfo = cgFuncInfo;
		gen.EmitMainFunction(ParseSourceElements(info, cgFuncInfo));
		
		Trace.Assert(curCGFuncInfo == cgFuncInfo);
		Trace.Assert(loops.Count == 0);
		Trace.Assert(withs.Count == 0);
		
		curCGFuncInfo = null;
		
		gen.EmitProgramSuffix();
		} // ParseProgram
	
	
	// Parse a SourceElements (i.e. the main program, or a function body)
	// and return the generated code.
	private StmtFrag ParseSourceElements( FunctionInfo info,
										  CGFuncInfo cgFuncInfo )
		{
		SrcLoc loc;
		loc.lineNum     = 1;
		loc.colNum      = 1;
		loc.absPosition = 0;
		loc.len         = 1;
		
		StmtFrag frag = gen.NewStmtFrag(loc);
		while (!tok.atEnd && !tok.PeekOp("}"))
			if (tok.TryMatchKeyword("function"))
				ParseFunction(info.GetNextChild(), false);
			else
				frag.Append(ParseStatement(info));
		
		return frag;
		} // ParseSourceElements
	
	
	// Parse a FunctionDeclaration or FunctionExpression.  The "function"
	// keyword should already have been matched.
	private void ParseFunction(FunctionInfo info, bool isExpression)
		{
		// Save the current loop and with stacks.  OPTIMIZATION: don't need to
		// do this if the stack was empty.
		Stack savedLoops = loops;
		loops = new Stack();
		
		Stack savedWiths = withs;
		withs = new Stack();
		
		CGFuncInfo savedCGFuncInfo = curCGFuncInfo;
		CGFuncInfo cgFuncInfo = gen.NewFuncInfo(info);
		curCGFuncInfo = cgFuncInfo;
		
		string functionName = null;
		
		if (isExpression)
			tok.TryMatchID(out functionName);
		else
			functionName = tok.MatchID();
		
		// Scan to the end of the parameter list.  We don't bother parsing
		// it in detail, as this was done during the first phase.
		while (!tok.TryMatchOp(")"))
			tok.Match();
		
		tok.MatchOp("{");
		StmtFrag body = ParseSourceElements(info, cgFuncInfo);
		tok.MatchOp("}");
		
		gen.EmitFunction(info, body);
		
		Trace.Assert(loops.Count == 0);
		Trace.Assert(withs.Count == 0);
		Trace.Assert(curCGFuncInfo == cgFuncInfo);
		loops = savedLoops;
		withs = savedWiths;
		curCGFuncInfo = savedCGFuncInfo;
		} // ParseFunction
	
	
	// Parse a Block.  The stream should be positioned at the "{" token.
	private StmtFrag ParseBlock(FunctionInfo info)
		{
		StmtFrag frag = gen.NewStmtFrag(tok.Prev().loc);
		
		tok.MatchOp("{");
		while (!tok.TryMatchOp("}"))
			frag.Append(ParseStatement(info));
		
		return frag;
		} // ParseBlock
	
	
	// This method is called when we are about to parse a Statement.  If
	// the statement has any labels, return an array of strings giving
	// the labels (in program order).  Otherwise return null.
	private StringCollection MatchLabelSet()
		{
		StringCollection labels = null;
		
		while ( tok.Peek() != null && tok.Peek().type == Token.Type.id &&
				tok.Peek2() != null && tok.Peek2().rawText == ":" )
			{
			String id = tok.MatchID();
			tok.MatchOp(":");
			
			if (labels == null)
				labels = new StringCollection();
			
			labels.Add(id);
			}
			
		return labels;
		} // MatchLabelSet
	
	
	// Parse a for statement.  The caller should already have matched
	// the "for" keyword.
	private StmtFrag ParseForStatement(FunctionInfo info, StringCollection labels)
		{
		LoopInfo loopInfo = gen.NewLoopInfo(curCGFuncInfo, tok.Prev().loc, labels);
		loops.Push(loopInfo);
		
		// HACK snewman 8/13/01: need to review this section against the
		// specific semantics for "for" statements, and especially for/in
		// statements, in section 12.6 of the ECMA spec.
		
		tok.MatchOp("(");
		
		bool isIn;			  // True for for/in, false otherwise
		StmtFrag init=null;   // Initializer; used for both types of loop
		ExprFrag cond=null;   // Condition; only used for non-in loops
		ExprFrag update=null; // Update step; only used for non-in loops
		ExprFrag inLHS=null;  // LHS expression where in loops put prop names
		ExprFrag inRHS=null;  // RHS object that in loops iterate over
		if (tok.TryMatchKeyword("var"))
			{
			string varName;
			SrcLoc varLoc;
			init = ParseVariableDeclaration(info, false, out varName, out varLoc);
			
			isIn = tok.TryMatchKeyword("in");
			if (isIn)
				inLHS = gen.IdentifierExpr( varLoc, varName, info,
											CloneStack(withs) );
			else
				{
				while (tok.TryMatchOp(","))
					init.Append(ParseVariableDeclaration( info, false, out varName,
														  out varLoc ));
				}
			
			}
		else
			{
			if (!tok.PeekOp(";"))
				{
				ExprFrag initExpr = ParseExpression(info, false, 99);
				isIn = tok.TryMatchKeyword("in");
				
				if (isIn)
					inLHS = initExpr;
				else
					init = gen.ExpressionStmt(initExpr);
				
				}
			else
				isIn = false;
			
			}
		
		if (isIn)
			inRHS = ParseExpression(info, true);
		else
			{
			tok.MatchOp(";");
			if (!tok.PeekOp(";"))
				cond = ParseExpression(info, true);
			
			tok.MatchOp(";");
			if (!tok.PeekOp(")"))
				update = ParseExpression(info, true);
			}
		
		tok.MatchOp(")");
		
		StmtFrag body = ParseStatement(info);
		
		LoopInfo temp = (LoopInfo) loops.Pop();
		Trace.Assert(temp == loopInfo);
		
		if (isIn)
			return gen.ForIn(loopInfo, init, inLHS, inRHS, body);
		else
			return gen.For(loopInfo, init, cond, update, body);
		
		} // ParseForStatement
	
	
	// Parse a Statement.
	private StmtFrag ParseStatement(FunctionInfo info)
		{
		StringCollection labels = MatchLabelSet();
		
		if (tok.TryMatchKeyword("do"))
			{
			LoopInfo loopInfo = gen.NewLoopInfo( curCGFuncInfo, tok.Prev().loc,
												 labels );
			loops.Push(loopInfo);
			
			StmtFrag body = ParseStatement(info);
			tok.MatchKeyword("while");
			tok.MatchOp("(");
			ExprFrag condition = ParseExpression(info, true);
			tok.MatchOp(")");
			tok.MatchOp(";");
			
			LoopInfo temp = (LoopInfo) loops.Pop();
			Trace.Assert(temp == loopInfo);
			
			return gen.DoWhile(loopInfo, body, condition);
			}
		else if (tok.TryMatchKeyword("while"))
			{
			LoopInfo loopInfo = gen.NewLoopInfo( curCGFuncInfo, tok.Prev().loc,
												 labels );
			loops.Push(loopInfo);
			
			tok.MatchOp("(");
			ExprFrag condition = ParseExpression(info, true);
			tok.MatchOp(")");
			StmtFrag body = ParseStatement(info);
			
			LoopInfo temp = (LoopInfo) loops.Pop();
			Trace.Assert(temp == loopInfo);
			
			return gen.WhileDo(loopInfo, condition, body);
			}
		else if (tok.TryMatchKeyword("for"))
			return ParseForStatement(info, labels);
		else
			{
			bool isSwitch = tok.PeekKeyword("switch");
			LoopInfo labelInfo = null;
			if (labels != null || isSwitch)
				{
				labelInfo = gen.NewLabeledStmtInfo( curCGFuncInfo, tok.Prev().loc,
													labels, isSwitch );
				loops.Push(labelInfo);
				}
			
			StmtFrag stmt;
			if (tok.PeekOp("{"))
				stmt = ParseBlock(info);
			else if (tok.TryMatchKeyword("var"))
				{
				stmt = gen.NewStmtFrag(tok.Prev().loc);
				
				do
					{
					string varName;
					SrcLoc varLoc;
					stmt.Append(ParseVariableDeclaration( info, true, out varName,
														  out varLoc ));
					}
				while (tok.TryMatchOp(","));
				tok.MatchOp(";");
				}
			else if (tok.TryMatchOp(";"))
				{
				// EmptyStatement
				stmt = gen.NewStmtFrag(tok.Prev().loc);
				}
			else if (tok.TryMatchKeyword("if"))
				{
				SrcLoc ifLoc = tok.Prev().loc;
				
				tok.MatchOp("(");
				ExprFrag condition = ParseExpression(info, true);
				tok.MatchOp(")");
				
				StmtFrag ifClause = ParseStatement(info);
				StmtFrag elseClause = null;
				if (tok.TryMatchKeyword("else"))
					elseClause = ParseStatement(info);
				
				stmt = gen.IfThenElse(ifLoc, condition, ifClause, elseClause);
				}
			else if (tok.TryMatchKeyword("continue"))
				{
				SrcLoc continueLoc = tok.Prev().loc;
				
				// HACK snewman 8/7/01: the grammar (ContinueStatement) specifies
				// "no LineTerminator here".
				string id;
				tok.TryMatchID(out id);
				tok.MatchOp(";");
				
				stmt = gen.Continue(continueLoc, id, CloneStack(loops));
				}
			else if (tok.TryMatchKeyword("break"))
				{
				SrcLoc breakLoc = tok.Prev().loc;
				
				// HACK snewman 8/7/01: the grammar (BreakStatement) specifies
				// "no LineTerminator here".
				string id;
				tok.TryMatchID(out id);
				tok.MatchOp(";");
				
				stmt = gen.Break(breakLoc, id, CloneStack(loops));
				}
			else if (tok.TryMatchKeyword("return"))
				{
				SrcLoc returnLoc = tok.Prev().loc;
				
				// HACK snewman 8/7/01: the grammar (ReturnStatement) specifies
				// "no LineTerminator here".
				if (tok.TryMatchOp(";"))
					stmt = gen.Return(returnLoc, null);
				else
					{
					ExprFrag value = ParseExpression(info, true);
					tok.MatchOp(";");
					stmt = gen.Return(returnLoc, value);
					}
				
				}
			else if (tok.TryMatchKeyword("with"))
				{
				SrcLoc withLoc = tok.Prev().loc;
				
				tok.MatchOp("(");
				ExprFrag value = ParseExpression(info, true);
				tok.MatchOp(")");
				
				WithInfo withInfo = gen.NewWithInfo(curCGFuncInfo, withLoc);
				withs.Push(withInfo);
				
				StmtFrag body = ParseStatement(info);
				
				WithInfo temp = (WithInfo) withs.Pop();
				Trace.Assert(temp == withInfo);
				
				stmt = gen.With(withInfo, value, body);
				}
			else if (tok.TryMatchKeyword("switch"))
				{
				SrcLoc switchLoc = tok.Prev().loc;
				SwitchInfo switchInfo = gen.NewSwitchInfo();
				
				tok.MatchOp("(");
				ExprFrag switchValue = ParseExpression(info, true);
				tok.MatchOp(")");
				
				tok.MatchOp("{");
				while (!tok.TryMatchOp("}"))
					{
					ExprFrag caseValue;
					if (tok.TryMatchKeyword("default"))
						caseValue = null;
					else
						{
						tok.MatchKeyword("case");
						caseValue = ParseExpression(info, true);
						}
					
					StmtFrag clauseStmt = null;
					tok.MatchOp(":");
					while ( !tok.PeekOp("}") && !tok.PeekKeyword("case") &&
							!tok.PeekKeyword("default") )
						{
						StmtFrag tempStmt = ParseStatement(info);
						if (clauseStmt == null)
							clauseStmt = tempStmt;
						else
							clauseStmt.Append(tempStmt);
						}
					
					switchInfo.AddCase(caseValue, clauseStmt);
					}
				
				stmt = gen.Switch(switchLoc, switchValue, switchInfo);
				}
			else if (tok.TryMatchKeyword("throw"))
				{
				SrcLoc throwLoc = tok.Prev().loc;
				
				// HACK snewman 8/7/01: the grammar (ThrowStatement) specifies
				// "no LineTerminator here".
				ExprFrag value = ParseExpression(info, true);
				tok.MatchOp(";");
				
				stmt = gen.Throw(throwLoc, value);
				}
			else if (tok.TryMatchKeyword("try"))
				{
				SrcLoc tryLoc = tok.Prev().loc;
				StmtFrag tryBody = ParseBlock(info);
				String catchVar        = null;
				WithInfo catchWithInfo = null;
				StmtFrag catchBody     = null;
				StmtFrag finallyBody   = null;
				
				if (tok.TryMatchKeyword("catch"))
					{
					SrcLoc catchLoc = tok.Prev().loc;
					
					tok.MatchOp("(");
					catchVar = tok.MatchID();
					tok.MatchOp(")");
					
					catchWithInfo = gen.NewWithInfo(curCGFuncInfo, catchLoc);
					withs.Push(catchWithInfo);
					
					catchBody = ParseBlock(info);
					
					WithInfo temp = (WithInfo) withs.Pop();
					Trace.Assert(temp == catchWithInfo);
					}
				
				if (tok.TryMatchKeyword("finally"))
					finallyBody = ParseBlock(info);
				
				stmt = gen.TryCatchFinally( tryLoc, tryBody, catchVar,
											catchWithInfo, catchBody, finallyBody );
				}
			else
				{
				ExprFrag expr = ParseExpression(info, true);
				tok.MatchOp(";");
				stmt = gen.ExpressionStmt(expr);
				}
			
			if (labelInfo != null)
				{
				LoopInfo temp2 = (LoopInfo) loops.Pop();
				Trace.Assert(temp2 == labelInfo);
				
				stmt = gen.LabeledStmt(labelInfo, stmt);
				}
			
			return stmt;
			}
		
		} // ParseStatement
	
	
	// Parse a VariableDeclaration.  See ParseExpression for an explanation
	// of allowIN.  We set varName to the name of the declared variable.
	private StmtFrag ParseVariableDeclaration( FunctionInfo info, bool allowIN,
											   out string varName,
											   out SrcLoc varLoc )
		{
		varName = tok.MatchID();
		varLoc = tok.Prev().loc;
		if (tok.TryMatchOp("="))
			{
			SrcLoc opLoc = tok.Prev().loc;
			ExprFrag lhs = gen.IdentifierExpr( varLoc, varName, info,
											   CloneStack(withs) );
			ExprFrag rhs = ParseExpression(info, allowIN, assignmentPrec);
			ExprFrag assignment = gen.BinaryExpr(opLoc, "=", lhs, rhs);
			return gen.ExpressionStmt(assignment);
			}
		else
			return gen.NewStmtFrag(tok.Prev().loc);
		
		} // ParseVariableDeclaration
	
	
	// Parse an Expression.
	// 
	// If allowIN is false, then we don't look for "in" operators.  (This
	// implements the "NoIn" variant of the grammar, e.g. ExpressionNoIn
	// as opposed to Expression.)
	private ExprFrag ParseExpression(FunctionInfo info, bool allowIN)
		{
		return ParseExpression(info, allowIN, 99);
		} // ParseExpression
	
	
	// Parse an Expression.
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
	private ExprFrag ParseExpression( FunctionInfo info, bool allowIN,
								      int maxPrecedence )
		{
		// Note that we use a simpler grammar than ECMAScript, because
		// we don't need to catch assignment to non-lvalue in the parser
		// (it will have been caught in phase 1).
		
		// Parse any prefix operators.
		Stack stackedOps = new Stack();
		Stack stackedOpsLocs = new Stack();
		OperatorInfo matchedOp;
		while (TryMatchOperator(true, -1, out matchedOp, true))
			{
			stackedOps.Push(matchedOp);
			stackedOpsLocs.Push(tok.Prev().loc);
			}
		
		ExprFrag expr = ParseLeftHandSideExpression(info, true);
		
		if (stackedOps.Count > 0)
			{
			while (TryMatchOperator(allowIN, unarySuffixPrec, out matchedOp, true))
				{
				Trace.Assert(matchedOp.opType == OperatorInfo.Types.unarySuffix);
				expr = gen.UnaryExpr( tok.Prev().loc, matchedOp.text,
									  matchedOp.opType == OperatorInfo.Types.unarySuffix,
									  expr );
				}
			
			while (stackedOps.Count > 0)
				{
				OperatorInfo curOp = (OperatorInfo)stackedOps.Pop();
				expr = gen.UnaryExpr( (SrcLoc)stackedOpsLocs.Pop(),
									  curOp.text,
									  curOp.opType == OperatorInfo.Types.unarySuffix,
									  expr );
				}
			
			} // if (stackedOps.Count > 0)
		
		while (TryMatchOperator(allowIN, maxPrecedence, out matchedOp, true))
			{
			SrcLoc opLoc = tok.Prev().loc;
			if (matchedOp.text == "?")
				{
				ExprFrag p1 = ParseExpression(info, allowIN, assignmentPrec);
				tok.MatchOp(":");
				ExprFrag p2 = ParseExpression(info, allowIN, assignmentPrec);
				expr = gen.ConditionalExpr(opLoc, expr, p1, p2);
				}
			else if (matchedOp.opType == OperatorInfo.Types.assignment)
				expr = gen.BinaryExpr( opLoc, matchedOp.text, expr,
									   ParseExpression(info, allowIN, matchedOp.precedence) );
			else if (matchedOp.opType == OperatorInfo.Types.binary)
				expr = gen.BinaryExpr( opLoc, matchedOp.text, expr,
									   ParseExpression(info, allowIN, matchedOp.precedence-1) );
			else
				{
				Trace.Assert(matchedOp.opType == OperatorInfo.Types.unarySuffix);
				expr = gen.UnaryExpr( opLoc, matchedOp.text,
									  matchedOp.opType == OperatorInfo.Types.unarySuffix,
									  expr );
				}
			
			}
		
		return expr;
		} // ParseExpression
	
	
	// Parse a LeftHandSideExpression.
	// 
	// If checkForCallExpr is false, then we don't check for CallExpressions.
	// This is used when parsing the function in a "new" expression, to
	// avoid parsing arguments to "new" as part of the expression that
	// specifies the function.
	private ExprFrag ParseLeftHandSideExpression( FunctionInfo info,
												  bool checkForCallExpr )
		{
		ExprFrag expr;
		if (tok.TryMatchKeyword("new"))
			{
			SrcLoc newLoc = tok.Prev().loc;
			
			ExprFrag constructor = ParseLeftHandSideExpression(info, false);
			ArrayList args = null;
			if (tok.TryMatchOp("("))
				args = MatchOptionalArgumentList(info);
			
			expr = gen.Construct(newLoc, constructor, args);
			}
		else if (tok.TryMatchKeyword("function"))
			{
			SrcLoc functionLoc = tok.Prev().loc;
			FunctionInfo childInfo = info.GetNextChild();
			ParseFunction(childInfo, true);
			expr = gen.FunctionExpr(functionLoc, childInfo);
			}
		else
			expr = ParsePrimaryExpression(info);
		
		while (true)
			if (checkForCallExpr && tok.TryMatchOp("("))
				{
				SrcLoc opLoc = tok.Prev().loc;
				ArrayList args = MatchOptionalArgumentList(info);
				expr = gen.Call(opLoc, expr, args);
				}
			else if (tok.TryMatchOp("["))
				{
				SrcLoc opLoc = tok.Prev().loc;
				expr = gen.ArrayReference(opLoc, expr, this.ParseExpression(info, true));
				tok.MatchOp("]");
				}
			else if (tok.TryMatchOp("."))
				{
				SrcLoc opLoc = tok.Prev().loc;
				expr = gen.FieldReference(opLoc, expr, tok.MatchID());
				}
			else
				return expr;
		
		} // ParseLeftHandSideExpression
	
	
	// Parse a PrimaryExpression.
	private ExprFrag ParsePrimaryExpression(FunctionInfo info)
		{
		string id;
		double d;
		string s;
		
		if (tok.TryMatchKeyword("this"))
			return gen.ThisExpr(tok.Prev().loc, info);
		else if (tok.TryMatchKeyword("null"))
			return gen.NullExpr(tok.Prev().loc);
		else if (tok.TryMatchKeyword("true"))
			return gen.BoolLiteralExpr(tok.Prev().loc, true);
		else if (tok.TryMatchKeyword("false"))
			return gen.BoolLiteralExpr(tok.Prev().loc, false);
		else if (tok.TryMatchOp("("))
			{
			ExprFrag expr = ParseExpression(info, true);
			tok.MatchOp(")");
			return expr;
			}
		else if (tok.TryMatchOp("{"))
			{
			SrcLoc startLoc = tok.Prev().loc;
			ObjectLiteralInfo litInfo = gen.NewObjectLiteralInfo();
			
			if (!tok.PeekOp("}"))
				{
				do
					{
					double tempD;
					if (tok.TryMatchNumLit(out tempD))
						{
						// HACK snewman 7/26/01: format according to the ECMA spec.
						id = System.Convert.ToString(tempD);
						}
					else if (tok.TryMatchStringLit(out id))
						{}
					else
						id = tok.MatchID();
					
					SrcLoc idLoc = tok.Prev().loc;
					tok.MatchOp(":");
					ExprFrag value = ParseExpression(info, true, assignmentPrec);
					
					litInfo.AddProp(idLoc, id, value);
					}
				while (tok.TryMatchOp(","));
				}
			
			tok.MatchOp("}");
			
			return gen.ObjectLiteralExpr(startLoc, litInfo);
			}
		else if (tok.TryMatchOp("["))
			{
			SrcLoc startLoc = tok.Prev().loc;
			ArrayLiteralInfo arrayInfo = gen.NewArrayLiteralInfo();
			
			bool anyCommas = false;
			bool requireComma = false;
			while (!tok.PeekOp("]"))
				if (tok.TryMatchOp(","))
					{
					anyCommas = true;
					if (!requireComma)
						arrayInfo.AddEntry(null);
					else
						requireComma = false;
					}
				else if (!requireComma)
					{
					ExprFrag value = ParseExpression(info, true, assignmentPrec);
					arrayInfo.AddEntry(value);
					requireComma = true;
					}
				else
					break;
			
			// If the list ends with a comma, then add one more null entry
			// to the end.
			if (anyCommas && !requireComma)
				arrayInfo.AddEntry(null);
			
			tok.MatchOp("]");
			return gen.ArrayLiteralExpr(startLoc, arrayInfo);
			}
		else if (tok.TryMatchID(out id))
			{
			SrcLoc idLoc = tok.Prev().loc;
			if (id == "eval")
				return gen.IdentifierExpr(idLoc, id, info, CloneStack(withs));
			else if (id == "arguments")
				return gen.ArgumentsExpr(idLoc, info, CloneStack(withs));
			else
				return gen.IdentifierExpr(idLoc, id, info, CloneStack(withs));
			
			}
		else if (tok.TryMatchNumLit(out d))
			return gen.NumLiteralExpr(tok.Prev().loc, d);
		else if (tok.TryMatchStringLit(out s))
			return gen.StringLiteralExpr(tok.Prev().loc, s);
		else
			{
			// One of the if clauses should fire; if not, the phase 1
			// parser would have reported an error.
			Trace.Assert(false);
			return null;
			}
		
		} // ParsePrimaryExpression
	
	
	// Match an ArgumentList (if any) and the trailing ')'.  Return
	// an array of ExprFrag objects.
	private ArrayList MatchOptionalArgumentList(FunctionInfo info)
		{
		ArrayList argList = new ArrayList();
		
		if (!tok.PeekOp(")"))
			{
			do
				argList.Add(ParseExpression(info, true, assignmentPrec));
			while (tok.TryMatchOp(","));
			}
		
		tok.MatchOp(")");
		return argList;
		} // MatchOptionalArgumentList
	
	
	// Return a new Stack with the same entries as the given stack.
	// 
	// This is used when passing our "loops" or "withs" stack to
	// the code generator.  If we didn't make a clone, then the stack
	// passed to the code generator would be altered the next time we
	// push or pop an entry on our stack.
	// 
	// OPTIMIZATION snewman 8/29/01: it's pretty wasteful to clone the
	// stack on each use; we should implement a simple cache so as to only
	// make a new clone after the stack has been changed.
	private Stack CloneStack(Stack stack)
		{
		if (stack.Count == 0)
			return emptyStack;
		else
			return (Stack)(stack.Clone());
		
		} // CloneStack
	
	
	private ICodeGenerator gen;   // The factory object we use to generate code.
	private Stack          loops; // The LoopInfo objects for each loop in the
								  // current function that encloses the current
								  // parse location.
	private Stack          withs; // The WithInfo objects for each with statement
								  // in the current function that encloses the
								  // current parse location.
	private CGFuncInfo     curCGFuncInfo; // CGFuncInfo for the function we're
								  // currently parsing.
	
	private static Stack emptyStack = new Stack(); // A permanently empty
								  // stack used in CloneStack.
	} // Phase2Parser

} // namespace JANET.Compiler