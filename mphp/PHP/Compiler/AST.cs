using System;
using System.Collections;
using System.Reflection.Emit;


namespace PHP.Compiler {


	public class AST {
		public StatementList StmtList;
		public AST(StatementList StmtList) {
			this.StmtList = StmtList;
		}
	}

	public abstract class ASTNode {
		public int Line;
		public int Column;
		public ASTNode(int line, int column) {
			Line = line;
			Column = column;
		}
	}

	public abstract class Statement : ASTNode {
		public Statement(int line, int column)
			: base(line, column) { }
	}

	public class USING : Statement {
		public string Type;
		public string Alias;
		public USING(string type, string alias, int line, int column)
			: base(line, column) {
			Type = type;
			Alias = alias;
		}
	}

	public class INTERFACE_DECLARATION : Statement {
		public int Modifier;
		public string Name;
		public ArrayList Extends;
		public StatementList StmtList;
		public SymbolTableScope Scope;
		public TypeBuilder TypBld;
		public Type Typ;
		public INTERFACE_DECLARATION(string name, ArrayList extends, StatementList stmtList, int line, int column)
			: base(line, column) {
			Name = name;
			Extends = extends;
			StmtList = stmtList;
		}
	}

	public class CLASS_DECLARATION : Statement {
		public int Modifier;
		public string Name;
		public string Extends;
		public ArrayList Implements;
		public StatementList StmtList;
		public SymbolTableScope Scope;
		public TypeBuilder TypBld;
		public Type Typ;
		public CLASS_DECLARATION(int modifier, string name, string extends, ArrayList implements, StatementList stmtList, int line, int column)
			: base(line, column) {
			Modifier = modifier;
			Name = name;
			Extends = extends;
			Implements = implements;
			StmtList = stmtList;
		}
	}

	public class CLASS_VARIABLE_DECLARATION : Statement {
		public ArrayList Modifiers;
		public ArrayList Names;
		public ExpressionList Values;
		public ArrayList FieldBuilders;
		public CLASS_VARIABLE_DECLARATION(ArrayList modifiers, ArrayList names, ExpressionList values, int line, int column)
			: base(line, column) {
			Modifiers = modifiers;
			Names = names;
			Values = values;
		}
	}

	public class FUNCTION_DECLARATION : Statement {
		public ArrayList Modifiers;
		public bool ReturnByReference;
		public string Name;
		public ArrayList Parameters;
		public StatementList StmtList;
		public SymbolTableScope Scope;
		public ConstructorBuilder CtrBld;
		public MethodBuilder MthBld;
		public FUNCTION_DECLARATION(ArrayList modifiers, bool returnByReference, string name, ArrayList parameters, StatementList stmtList, int line, int column)
			: base(line, column) {
			Modifiers = modifiers;
			ReturnByReference = returnByReference;
			Name = name;
			Parameters = parameters;
			StmtList = stmtList;
		}
	}

	public class PARAMETER_DECLARATION : ASTNode {
		public string Type;
		public bool ByReference;
		public string Name;
		public Expression DefaultValue;
		public PARAMETER_DECLARATION(string type, bool byReference, string name, Expression defaultValue, int line, int column)
			: base(line, column) {
			Type = type;
			ByReference = byReference;
			Name = name;
			DefaultValue = defaultValue;
		}
	}

	public class GLOBAL : Statement {
		public ExpressionList VarList;
		public GLOBAL(ExpressionList varList, int line, int column)
			: base(line, column) {
			VarList = varList;
		}
	}

	public class STATIC_DECLARATION : Statement {
		public ExpressionList ExprList;
		public STATIC_DECLARATION(ExpressionList exprList, int line, int column)
			: base(line, column) {
			ExprList = exprList;
		}
	}

	public class ECHO : Statement {
		public ExpressionList ExprList;
		public ECHO(ExpressionList exprList, int line, int column)
			: base(line, column) {
			ExprList = exprList;
		}
	}

	public class BLOCK : Statement {
		public StatementList StmtList;
		public BLOCK(StatementList stmtList, int line, int column)
			: base(line, column) {
			StmtList = stmtList;
		}
	}

	public class IF : Statement {
		public Expression Expr;
		public Statement Stmt;
		public ArrayList ElseifList;
		public Statement ElseStmt;
		public IF(Expression expr, Statement stmt, ArrayList elseifList, Statement elseStmt, int line, int column)
			: base(line, column) {
			Expr = expr;
			Stmt = stmt;
			ElseifList = elseifList;
			ElseStmt = elseStmt;
		}
	}

	public class ELSEIF : ASTNode {
		public Expression Expr;
		public Statement Stmt;
		public ELSEIF(Expression expr, Statement stmt, int line, int column)
			: base(line, column) {
			Expr = expr;
			Stmt = stmt;
		}
	}

	public class WHILE : Statement {
		public Expression Expr;
		public Statement Stmt;
		public WHILE(Expression expr, Statement stmt, int line, int column)
			: base(line, column) {
			Expr = expr;
			Stmt = stmt;
		}
	}

	public class DO : Statement {
		public Statement Stmt;
		public Expression Expr;
		public DO(Statement stmt, Expression expr, int line, int column)
			: base(line, column) {
			Stmt = stmt;
			Expr = expr;
		}
	}

	public class FOR : Statement {
		public ExpressionList ExprList1;
		public ExpressionList ExprList2;
		public ExpressionList ExprList3;
		public Statement Stmt;
		public FOR(ExpressionList exprList1, ExpressionList exprList2, ExpressionList exprList3, Statement stmt, int line, int column)
			: base(line, column) {
			ExprList1 = exprList1;
			ExprList2 = exprList2;
			ExprList3 = exprList3;
			Stmt = stmt;
		}
	}

	public class FOREACH : Statement {
		public Expression Array;
		public Expression Key;
		public Expression Value;
		public Statement Stmt;
		public FOREACH(Expression array, Expression key, Expression value, Statement stmt, int line, int column)
			: base(line, column) {
			Array = array;
			Key = key;
			Value = value;
			Stmt = stmt;
		}
	}

	public class SWITCH : Statement {
		public Expression Expr;
		public ArrayList SwitchCaseList;
		public SWITCH(Expression expr, ArrayList switchCaseList, int line, int column)
			: base(line, column) {
			Expr = expr;
			SwitchCaseList = switchCaseList;
		}
	}

	public class CASE : ASTNode {
		public Expression Expr;
		public Statement Stmt;
		public CASE(Expression expr, Statement stmt, int line, int column)
			: base(line, column) {
			Expr = expr;
			Stmt = stmt;
		}
	}

	public class DEFAULT : ASTNode {
		public Statement Stmt;
		public DEFAULT(Statement stmt, int line, int column)
			: base(line, column) {
			Stmt = stmt;
		}
	}

	public class BREAK : Statement {
		public Expression Expr;
		public BREAK(Expression expr, int line, int column)
			: base(line, column) {
			Expr = expr;
		}
	}

	public class CONTINUE : Statement {
		public Expression Expr;
		public CONTINUE(Expression expr, int line, int column)
			: base(line, column) {
			Expr = expr;
		}
	}

	public class RETURN : Statement {
		public Expression Expr;
		public RETURN(Expression expr, int line, int column)
			: base(line, column) {
			Expr = expr;
		}
	}

	public class UNSET : Statement {
		public ExpressionList VarList;
		public UNSET(ExpressionList varList, int line, int column)
			: base(line, column) {
			VarList = varList;
		}
	}

	public class TRY : Statement {
		public StatementList StmtList;
		public ArrayList Catches;
		public TRY(StatementList stmtList, ArrayList catches, int line, int column)
			: base(line, column) {
			StmtList = stmtList;
			Catches = catches;
		}
	}

	public class CATCH : ASTNode {
		public string Type;
		public string Variable;
		public StatementList StmtList;
		public CATCH(string type, string variable, StatementList stmtList, int line, int column)
			: base(line, column) {
			Type = type;
			Variable = variable;
			StmtList = stmtList;
		}
	}

	public class THROW : Statement {
		public Expression Expr;
		public THROW(Expression expr, int line, int column)
			: base(line, column) {
			Expr = expr;
		}
	}

	public class EXPRESSION_AS_STATEMENT : Statement {
		public Expression Expr;
		public EXPRESSION_AS_STATEMENT(Expression expr, int line, int column)
			: base(line, column) {
			Expr = expr;
		}
	}

	public abstract class Expression : ASTNode {
		public Expression(int line, int column) : base(line, column) { }
	}

	public abstract class UnaryExpression : Expression {
		public Expression Expr;
		public UnaryExpression(Expression expr, int line, int column)
			: base(line, column) {
			Expr = expr;
		}
	}

	public abstract class BinaryExpression : Expression {
		public Expression Expr1;
		public Expression Expr2;
		public BinaryExpression(Expression expr1, Expression expr2, int line, int column)
			: base(line, column) {
			Expr1 = expr1;
			Expr2 = expr2;
		}
	}

	public abstract class TernaryExpression : Expression {
		public Expression Expr1;
		public Expression Expr2;
		public Expression Expr3;
		public TernaryExpression(Expression expr1, Expression expr2, Expression expr3, int line, int column)
			: base(line, column) {
			Expr1 = expr1;
			Expr2 = expr2;
			Expr3 = expr3;
		}
	}

	public class VARIABLE : Expression {
		public string Name;
		public OFFSET Offset;
		public VARIABLE(string name, int line, int column) : this(name, null, line, column) { }
		public VARIABLE(string name, OFFSET offset, int line, int column)
			: base(line, column) {
			Name = name;
			Offset = offset;
		}
	}

	public class REFERENCE : UnaryExpression {
		public REFERENCE(Expression expr, int line, int column)
			: base(expr, line, column) {
		}
	}

	public class FUNCTION_CALL : Expression {
		public string FunctionName;
		public ExpressionList Parameters;
		public FUNCTION_CALL(string functionName, ExpressionList parameters, int line, int column)
			: base(line, column) {
			FunctionName = functionName;
			Parameters = parameters;
		}
	}

	public class NEW : Expression {
		public string Type;
		public ExpressionList CtorArgs;
		public NEW(string type, ExpressionList ctorArgs, int line, int column)
			: base(line, column) {
			Type = type;
			CtorArgs = ctorArgs;
		}
	}

	public class INSTANCEOF : Expression {
		public Expression Expr;
		public string Type;
		public INSTANCEOF(Expression expr, string type, int line, int column)
			: base(line, column) {
			Expr = expr;
			Type = type;
		}
	}

	public class ARRAY : Expression {
		public ArrayList ArrayPairList;
		public ARRAY(ArrayList arrayPairList, int line, int column)
			: base(line, column) {
			ArrayPairList = arrayPairList;
		}
	}

	public class ARRAY_PAIR : ASTNode {
		public Expression Key;
		public Expression Value;
		public ARRAY_PAIR(Expression key, Expression value, int line, int column)
			: base(line, column) {
			Key = key;
			Value = value;
		}
	}

	public class OFFSET : ASTNode {
		public const int SQUARE = 0;
		public const int CURLY = 1;
		public int Kind;
		public Expression Value;
		public OFFSET(int kind, Expression value, int line, int column)
			: base(line, column) {
			Kind = kind;
			Value = value;
		}
	}

	public class MAGIC_CONSTANT : Expression {
		public const int LINE = 0;
		public const int FILE = 1;
		public const int CLASS = 2;
		public const int METHOD = 3;
		public const int FUNCTION = 4;
		public int Kind;
		public MAGIC_CONSTANT(int kind, int line, int column) : base(line, column) {
			Kind = kind;
		}
	}

	public class INC : UnaryExpression {
		public int Kind;
		public INC(Expression expr, int kind, int line, int column)
			: base(expr, line, column) {
			Kind = kind;
		}
	}

	public class DEC : UnaryExpression {
		public int Kind;
		public DEC(Expression expr, int kind, int line, int column) : base(expr, line, column) {
			Kind = kind;
		}
	}

	public class BOOLEAN_NOT : UnaryExpression {
		public BOOLEAN_NOT(Expression expr, int line, int column) : base(expr, line, column) { }
	}

	public class NOT : UnaryExpression {
		public NOT(Expression expr, int line, int column) : base(expr, line, column) { }
	}

	public class EXIT : UnaryExpression {
		public EXIT(Expression expr, int line, int column) : base(expr, line, column) { }
	}

	public class PRINT : UnaryExpression {
		public PRINT(Expression expr, int line, int column) : base(expr, line, column) { }
	}

	public class BOOL_CAST : UnaryExpression {
		public BOOL_CAST(Expression expr, int line, int column) : base(expr, line, column) { }
	}

	public class INT_CAST : UnaryExpression {
		public INT_CAST(Expression expr, int line, int column) : base(expr, line, column) { }
	}

	public class DOUBLE_CAST : UnaryExpression {
		public DOUBLE_CAST(Expression expr, int line, int column) : base(expr, line, column) { }
	}

	public class STRING_CAST : UnaryExpression {
		public STRING_CAST(Expression expr, int line, int column) : base(expr, line, column) { }
	}

	public class ARRAY_CAST : UnaryExpression {
		public ARRAY_CAST(Expression expr, int line, int column) : base(expr, line, column) { }
	}

	public class OBJECT_CAST : UnaryExpression {
		public OBJECT_CAST(Expression expr, int line, int column) : base(expr, line, column) { }
	}

	public class CLONE : UnaryExpression {
		public CLONE(Expression expr, int line, int column) : base(expr, line, column) { }
	}

	public class PAAMAYIM_NEKUDOTAYIM : UnaryExpression {
		public string Type;
		public PAAMAYIM_NEKUDOTAYIM(string type, Expression expr, int line, int column) : base(expr, line, column) {
			Type = type;
		}
	}

	public class OBJECT_OPERATOR : BinaryExpression {
		public OBJECT_OPERATOR(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class EQUALS : BinaryExpression {
		public EQUALS(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class PLUS_EQUAL : BinaryExpression {
		public PLUS_EQUAL(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class MINUS_EQUAL : BinaryExpression {
		public MINUS_EQUAL(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class MUL_EQUAL : BinaryExpression {
		public MUL_EQUAL(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class DIV_EQUAL : BinaryExpression {
		public DIV_EQUAL(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class CONCAT_EQUAL : BinaryExpression {
		public CONCAT_EQUAL(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class MOD_EQUAL : BinaryExpression {
		public MOD_EQUAL(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class AND_EQUAL : BinaryExpression {
		public AND_EQUAL(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class OR_EQUAL : BinaryExpression {
		public OR_EQUAL(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class XOR_EQUAL : BinaryExpression {
		public XOR_EQUAL(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class SL_EQUAL : BinaryExpression {
		public SL_EQUAL(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class SR_EQUAL : BinaryExpression {
		public SR_EQUAL(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class BOOLEAN_OR : BinaryExpression {
		public BOOLEAN_OR(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class BOOLEAN_AND : BinaryExpression {
		public BOOLEAN_AND(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class LOGICAL_OR : BinaryExpression {
		public LOGICAL_OR(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class LOGICAL_AND : BinaryExpression {
		public LOGICAL_AND(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class LOGICAL_XOR : BinaryExpression {
		public LOGICAL_XOR(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class CONCAT : BinaryExpression {
		public CONCAT(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class PLUS : BinaryExpression {
		public PLUS(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class MINUS : BinaryExpression {
		public MINUS(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class TIMES : BinaryExpression {
		public TIMES(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class DIV : BinaryExpression {
		public DIV(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class MOD : BinaryExpression {
		public MOD(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class AND : BinaryExpression {
		public AND(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class OR : BinaryExpression {
		public OR(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class XOR : BinaryExpression {
		public XOR(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class SL : BinaryExpression {
		public SL(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class SR : BinaryExpression {
		public SR(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class IS_EQUAL : BinaryExpression {
		public IS_EQUAL(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class IS_NOT_EQUAL : BinaryExpression {
		public IS_NOT_EQUAL(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class IS_IDENTICAL : BinaryExpression {
		public IS_IDENTICAL(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class IS_NOT_IDENTICAL : BinaryExpression {
		public IS_NOT_IDENTICAL(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class LOWER : BinaryExpression {
		public LOWER(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class IS_LOWER_OR_EQUAL : BinaryExpression {
		public IS_LOWER_OR_EQUAL(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class GREATER : BinaryExpression {
		public GREATER(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class IS_GREATER_OR_EQUAL : BinaryExpression {
		public IS_GREATER_OR_EQUAL(Expression expr1, Expression expr2, int line, int column) : base(expr1, expr2, line, column) { }
	}

	public class IF_EXPR : TernaryExpression {
		public IF_EXPR(Expression expr1, Expression expr2, Expression expr3, int line, int column) : base(expr1, expr2, expr3, line, column) { }
	}

	public abstract class SCALAR : Expression {
		public SCALAR(int line, int column) : base(line, column) { }
	}

	public class LNUMBER_SCALAR : SCALAR {
		public int Value;
		public LNUMBER_SCALAR(int value, int line, int column)
			: base(line, column) {
			Value = value;
		}
	}

	public class DNUMBER_SCALAR : SCALAR {
		public double Value;
		public DNUMBER_SCALAR(double value, int line, int column)
			: base(line, column) {
			Value = value;
		}
	}

	public class STRING_SCALAR : SCALAR {
		public string Value;
		public STRING_SCALAR(string value, int line, int column)
			: base(line, column) {
			Value = value;
		}
	}

	public class SINGLE_QUOTES : SCALAR {
		public ArrayList EncapsList;
		public SINGLE_QUOTES(ArrayList encapsList, int line, int column)
			: base(line, column) {
			EncapsList = encapsList;
		}
	}

	public class DOUBLE_QUOTES : SCALAR {
		public ArrayList EncapsList;
		public DOUBLE_QUOTES(ArrayList encapsList, int line, int column)
			: base(line, column) {
			EncapsList = encapsList;
		}
	}

	public class HEREDOC : SCALAR {
		public ArrayList EncapsList;
		public HEREDOC(ArrayList encapsList, int line, int column)
			: base(line, column) {
			EncapsList = encapsList;
		}
	}

	public class CONSTANT : SCALAR {
		public string Name;
		public CONSTANT(string name, int line, int column)
			: base(line, column) {
			Name = name;
		}
	}


}