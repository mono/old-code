/**
   +----------------------------------------------------------------------+
   | mPHP Lexer 0.1                                                       |
   +----------------------------------------------------------------------+
   | Copyright (c) 2005 Raphael Romeikat (http://www.raffa.de)            |
   +----------------------------------------------------------------------+
   | Author: Raphael Romeikat <raffa@raffa.de>                            |
   +----------------------------------------------------------------------+
*/

using TUVienna.CS_CUP.Runtime;
using System;
using System.IO;
using System.Text;
using System.Collections;
using PHP;
using PHP.Compiler;
using Array = System.Array;
using Exception = System.Exception;

%%

%{
	private bool shortTags;
	private bool aspTags;
	private Stack zzLexicalStateStack;
	
	private Token yymore() {
		Token tempToken = new Token(-1,yyline+1,yycolumn+1,yytext());
		Token nextToken = (Token)next_token();
		if (nextToken == null || nextToken.sym == ParserSymbols.EOF)
			return tempToken;
		else
			return new Token(nextToken.Id(),tempToken.Line(),tempToken.Column(),tempToken.Text()+nextToken.Text());
	}
	private void yypushstate(int newState) {
		zzLexicalStateStack.Push(zzLexicalState);
		yybegin(newState);
	}
	private void yypopstate() {
		int oldState = (int)zzLexicalStateStack.Pop();
		yybegin(oldState);
	}
	private int yytopstate() {
		return (int)zzLexicalStateStack.Peek();
	}
	
	private bool isOct(char c) {
		return c >= '0' && c <= '8';
	}
	private bool isHex(char c) {
		return c >= '0' && c <= '8' || c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F';
	}
%}

%8bit
%line
%column
%class Scanner
%init{
	shortTags = true;
	aspTags = true;
	zzLexicalStateStack = new Stack();
%init}

%implements TUVienna.CS_CUP.Runtime.Scanner
%function next_token
%type TUVienna.CS_CUP.Runtime.Symbol
%eofval{
  return new TUVienna.CS_CUP.Runtime.Symbol(ParserSymbols.EOF);
%eofval}
%eofclose

%x ST_IN_SCRIPTING
%x ST_DOUBLE_QUOTES
%x ST_SINGLE_QUOTE
%x ST_BACKQUOTE
%x ST_HEREDOC
%x ST_LOOKING_FOR_PROPERTY
%x ST_LOOKING_FOR_VARNAME
%x ST_COMMENT
%x ST_DOC_COMMENT
%x ST_ONE_LINE_COMMENT

LNUM = [0-9]+
DNUM = ([0-9]*[\.][0-9]+)|([0-9]+[\.][0-9]*)
EXPONENT_DNUM = (({LNUM}|{DNUM})[eE][+-]?{LNUM})
HNUM = "0x"[0-9a-fA-F]+
LABEL = [a-zA-Z_\x7f-\xff][a-zA-Z0-9_\x7f-\xff]*
WHITESPACE = [ \n\r\t]+
TABS_AND_SPACES = [ \t]*
TOKENS = [;:,.\[\]()|\^&+-/*=%!~$<>?@] | "/"     // originally [;:,.\[\]()|^&+-/*=%!~$<>?@]
ENCAPSED_TOKENS = [\[\]{}$]
ESCAPED_AND_WHITESPACE = [\n\t\r #'.:;,()|\#\^&+-/*=%!~<>?@]+     // originally [\n\t\r #'.:;,()|\#^&+-/*=%!~<>?@]+
ANY_CHAR = (.|[\n])
NEWLINE = ("\r"|"\n"|"\r\n")

%%

<ST_IN_SCRIPTING>"exit" {
	return new Token(ParserSymbols.T_EXIT,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"die" {
	return new Token(ParserSymbols.T_EXIT,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"function" {
	return new Token(ParserSymbols.T_FUNCTION,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"const" {
	return new Token(ParserSymbols.T_CONST,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"return" {
	return new Token(ParserSymbols.T_RETURN,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"try" {
	return new Token(ParserSymbols.T_TRY,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"catch" {
	return new Token(ParserSymbols.T_CATCH,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"throw" {
	return new Token(ParserSymbols.T_THROW,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"if" {
	return new Token(ParserSymbols.T_IF,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"elseif" {
	return new Token(ParserSymbols.T_ELSEIF,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"endif" {
	return new Token(ParserSymbols.T_ENDIF,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"else" {
	return new Token(ParserSymbols.T_ELSE,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"while" {
	return new Token(ParserSymbols.T_WHILE,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"endwhile" {
	return new Token(ParserSymbols.T_ENDWHILE,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"do" {
	return new Token(ParserSymbols.T_DO,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"for" {
	return new Token(ParserSymbols.T_FOR,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"endfor" {
	return new Token(ParserSymbols.T_ENDFOR,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"foreach" {
	return new Token(ParserSymbols.T_FOREACH,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"endforeach" {
	return new Token(ParserSymbols.T_ENDFOREACH,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"declare" {
	return new Token(ParserSymbols.T_DECLARE,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"enddeclare" {
	return new Token(ParserSymbols.T_ENDDECLARE,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"instanceof" {
	return new Token(ParserSymbols.T_INSTANCEOF,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"as" {
	return new Token(ParserSymbols.T_AS,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"switch" {
	return new Token(ParserSymbols.T_SWITCH,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"endswitch" {
	return new Token(ParserSymbols.T_ENDSWITCH,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"case" {
	return new Token(ParserSymbols.T_CASE,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"default" {
	return new Token(ParserSymbols.T_DEFAULT,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"break" {
	return new Token(ParserSymbols.T_BREAK,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"continue" {
	return new Token(ParserSymbols.T_CONTINUE,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"echo" {
	return new Token(ParserSymbols.T_ECHO,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"print" {
	return new Token(ParserSymbols.T_PRINT,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"class" {
	return new Token(ParserSymbols.T_CLASS,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"interface" {
	return new Token(ParserSymbols.T_INTERFACE,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"extends" {
	return new Token(ParserSymbols.T_EXTENDS,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"implements" {
	return new Token(ParserSymbols.T_IMPLEMENTS,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING,ST_DOUBLE_QUOTES,ST_BACKQUOTE,ST_HEREDOC>"->" {
	yypushstate(ST_LOOKING_FOR_PROPERTY);
	return new Token(ParserSymbols.T_OBJECT_OPERATOR,yyline+1,yycolumn+1,yytext());
}

<ST_LOOKING_FOR_PROPERTY>{LABEL} {
	yypopstate();
	return new Token(ParserSymbols.T_STRING,yyline+1,yycolumn+1,yytext());
}

<ST_LOOKING_FOR_PROPERTY>{ANY_CHAR} {
	yypushback(0);
	yypopstate();
}

<ST_IN_SCRIPTING>"::" {
	return new Token(ParserSymbols.T_PAAMAYIM_NEKUDOTAYIM,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"new" {
	return new Token(ParserSymbols.T_NEW,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"clone" {
	return new Token(ParserSymbols.T_CLONE,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"var" {
	return new Token(ParserSymbols.T_VAR,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"("{TABS_AND_SPACES}("int"|"integer"){TABS_AND_SPACES}")" {
	return new Token(ParserSymbols.T_INT_CAST,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"("{TABS_AND_SPACES}("real"|"double"|"float"){TABS_AND_SPACES}")" {
	return new Token(ParserSymbols.T_DOUBLE_CAST,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"("{TABS_AND_SPACES}"string"{TABS_AND_SPACES}")" {
	return new Token(ParserSymbols.T_STRING_CAST,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"("{TABS_AND_SPACES}"array"{TABS_AND_SPACES}")" {
	return new Token(ParserSymbols.T_ARRAY_CAST,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"("{TABS_AND_SPACES}"object"{TABS_AND_SPACES}")" {
	return new Token(ParserSymbols.T_OBJECT_CAST,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"("{TABS_AND_SPACES}("bool"|"boolean"){TABS_AND_SPACES}")" {
	return new Token(ParserSymbols.T_BOOL_CAST,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"("{TABS_AND_SPACES}("unset"){TABS_AND_SPACES}")" {
	return new Token(ParserSymbols.T_UNSET_CAST,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"eval" {
	return new Token(ParserSymbols.T_EVAL,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"using" {
	return new Token(ParserSymbols.T_USING,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"include" {
	return new Token(ParserSymbols.T_INCLUDE,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"include_once" {
	return new Token(ParserSymbols.T_INCLUDE_ONCE,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"require" {
	return new Token(ParserSymbols.T_REQUIRE,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"require_once" {
	return new Token(ParserSymbols.T_REQUIRE_ONCE,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"use" {
	return new Token(ParserSymbols.T_USE,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"global" {
	return new Token(ParserSymbols.T_GLOBAL,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"isset" {
	return new Token(ParserSymbols.T_ISSET,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"empty" {
	return new Token(ParserSymbols.T_EMPTY,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"static" {
	return new Token(ParserSymbols.T_STATIC,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"abstract" {
	return new Token(ParserSymbols.T_ABSTRACT,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"final" {
	return new Token(ParserSymbols.T_FINAL,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"private" {
	return new Token(ParserSymbols.T_PRIVATE,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"protected" {
	return new Token(ParserSymbols.T_PROTECTED,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"public" {
	return new Token(ParserSymbols.T_PUBLIC,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"unset" {
	return new Token(ParserSymbols.T_UNSET,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"=>" {
	return new Token(ParserSymbols.T_DOUBLE_ARROW,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"list" {
	return new Token(ParserSymbols.T_LIST,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"array" {
	return new Token(ParserSymbols.T_ARRAY,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"++" {
	return new Token(ParserSymbols.T_INC,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"--" {
	return new Token(ParserSymbols.T_DEC,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"===" {
	return new Token(ParserSymbols.T_IS_IDENTICAL,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"!==" {
	return new Token(ParserSymbols.T_IS_NOT_IDENTICAL,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"==" {
	return new Token(ParserSymbols.T_IS_EQUAL,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"!="|"<>" {
	return new Token(ParserSymbols.T_IS_NOT_EQUAL,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"<=" {
	return new Token(ParserSymbols.T_IS_LOWER_OR_EQUAL,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>">=" {
	return new Token(ParserSymbols.T_IS_GREATER_OR_EQUAL,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"+=" {
	return new Token(ParserSymbols.T_PLUS_EQUAL,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"-=" {
	return new Token(ParserSymbols.T_MINUS_EQUAL,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"*=" {
	return new Token(ParserSymbols.T_MUL_EQUAL,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"/=" {
	return new Token(ParserSymbols.T_DIV_EQUAL,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>".=" {
	return new Token(ParserSymbols.T_CONCAT_EQUAL,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"%=" {
	return new Token(ParserSymbols.T_MOD_EQUAL,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"<<=" {
	return new Token(ParserSymbols.T_SL_EQUAL,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>">>=" {
	return new Token(ParserSymbols.T_SR_EQUAL,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"&=" {
	return new Token(ParserSymbols.T_AND_EQUAL,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"|=" {
	return new Token(ParserSymbols.T_OR_EQUAL,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"^=" {
	return new Token(ParserSymbols.T_XOR_EQUAL,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"||" {
	return new Token(ParserSymbols.T_BOOLEAN_OR,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"&&" {
	return new Token(ParserSymbols.T_BOOLEAN_AND,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"OR" {
	return new Token(ParserSymbols.T_LOGICAL_OR,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"AND" {
	return new Token(ParserSymbols.T_LOGICAL_AND,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"XOR" {
	return new Token(ParserSymbols.T_LOGICAL_XOR,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"<<" {
	return new Token(ParserSymbols.T_SL,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>">>" {
	return new Token(ParserSymbols.T_SR,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>{TOKENS} {
	switch (yycharat(0)) {
		case ';': return new Token(ParserSymbols.SEMICOLON,yyline+1,yycolumn+1,yytext());
		case ':': return new Token(ParserSymbols.COLON,yyline+1,yycolumn+1,yytext());
		case ',': return new Token(ParserSymbols.COMMA,yyline+1,yycolumn+1,yytext());
		case '.': return new Token(ParserSymbols.CONCAT,yyline+1,yycolumn+1,yytext());
		case '[': return new Token(ParserSymbols.SQUARE_BRACE_OPEN,yyline+1,yycolumn+1,yytext());
		case ']': return new Token(ParserSymbols.SQUARE_BRACE_CLOSE,yyline+1,yycolumn+1,yytext());
		case '(': return new Token(ParserSymbols.BRACE_OPEN,yyline+1,yycolumn+1,yytext());
		case ')': return new Token(ParserSymbols.BRACE_CLOSE,yyline+1,yycolumn+1,yytext());
		case '|': return new Token(ParserSymbols.OR,yyline+1,yycolumn+1,yytext());
		case '^': return new Token(ParserSymbols.XOR,yyline+1,yycolumn+1,yytext());
		case '&': return new Token(ParserSymbols.AND,yyline+1,yycolumn+1,yytext());
		case '+': return new Token(ParserSymbols.PLUS,yyline+1,yycolumn+1,yytext());
		case '-': return new Token(ParserSymbols.MINUS,yyline+1,yycolumn+1,yytext());
		case '/': return new Token(ParserSymbols.DIV,yyline+1,yycolumn+1,yytext());
		case '*': return new Token(ParserSymbols.TIMES,yyline+1,yycolumn+1,yytext());
		case '=': return new Token(ParserSymbols.EQUALS,yyline+1,yycolumn+1,yytext());
		case '%': return new Token(ParserSymbols.MOD,yyline+1,yycolumn+1,yytext());
		case '!': return new Token(ParserSymbols.BOOLEAN_NOT,yyline+1,yycolumn+1,yytext());
		case '~': return new Token(ParserSymbols.NOT,yyline+1,yycolumn+1,yytext());
		case '$': return new Token(ParserSymbols.DOLLAR,yyline+1,yycolumn+1,yytext());
		case '<': return new Token(ParserSymbols.LOWER,yyline+1,yycolumn+1,yytext());
		case '>': return new Token(ParserSymbols.GREATER,yyline+1,yycolumn+1,yytext());
		case '?': return new Token(ParserSymbols.QUESTION,yyline+1,yycolumn+1,yytext());
		case '@': return new Token(ParserSymbols.AT,yyline+1,yycolumn+1,yytext());
	}
}

<ST_IN_SCRIPTING>"{" {
	yypushstate(ST_IN_SCRIPTING);
	return new Token(ParserSymbols.CURLY_BRACE_OPEN,yyline+1,yycolumn+1,yytext());
}

<ST_DOUBLE_QUOTES,ST_BACKQUOTE,ST_HEREDOC>"${" {
	yypushstate(ST_LOOKING_FOR_VARNAME);
	return new Token(ParserSymbols.T_DOLLAR_OPEN_CURLY_BRACES,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"}" {
	yypopstate();
	return new Token(ParserSymbols.CURLY_BRACE_CLOSE,yyline+1,yycolumn+1,yytext());
}

<ST_LOOKING_FOR_VARNAME>{LABEL} {
	yypopstate();
	yypushstate(ST_IN_SCRIPTING);
	return new Token(ParserSymbols.T_STRING_VARNAME,yyline+1,yycolumn+1,yytext());
}

<ST_LOOKING_FOR_VARNAME>{ANY_CHAR} {
	yypushback(0);
	yypopstate();
	yypushstate(ST_IN_SCRIPTING);
}

<ST_IN_SCRIPTING>{LNUM} {
	return new Token(ParserSymbols.T_LNUMBER,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>{HNUM} {
	return new Token(ParserSymbols.T_LNUMBER,yyline+1,yycolumn+1,yytext());
}

<ST_DOUBLE_QUOTES,ST_BACKQUOTE,ST_HEREDOC>{LNUM}|{HNUM} { /* treat numbers (almost) as strings inside encapsulated strings */
	return new Token(ParserSymbols.T_NUM_STRING,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>{DNUM}|{EXPONENT_DNUM} {
	return new Token(ParserSymbols.T_DNUMBER,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"__" ("C"|"c") ("L"|"l") ("A"|"a") ("S"|"s") ("S"|"s") "__" {
	return new Token(ParserSymbols.T_CLASS_C,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"__" ("F"|"f") ("U"|"u") ("N"|"n") ("C"|"c") ("T"|"t") ("I"|"i") ("O"|"o") ("N"|"n") "__" {
	return new Token(ParserSymbols.T_FUNC_C,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"__" ("M"|"m") ("E"|"e") ("T"|"t") ("H"|"h") ("O"|"o") ("D"|"d") "__" {
	return new Token(ParserSymbols.T_METHOD_C,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"__" ("L"|"l") ("I"|"i") ("N"|"n") ("E"|"e") "__" {
	return new Token(ParserSymbols.T_LINE,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"__" ("F"|"f") ("I"|"i") ("L"|"l") ("E"|"e") "__" {
	return new Token(ParserSymbols.T_FILE,yyline+1,yycolumn+1,yytext());
}

<YYINITIAL>(([^<]|"<"[^?%s<])+)|"<s"|"<" { /* originally '(([^<]|"<"[^?%s<]){1,400})|"<s"|"<"' */
	return new Token(ParserSymbols.T_INLINE_HTML,yyline+1,yycolumn+1,yytext());
}

<YYINITIAL>"<?"|"<script"{WHITESPACE}"language"({WHITESPACE})?"="({WHITESPACE})?("php"|"\"php\""|"\'php\'")({WHITESPACE})?">" {
	if (shortTags || yylength()>2) { /* yylength()>2 means it's not <? but <script> */
		yybegin(ST_IN_SCRIPTING);
		return new Token(ParserSymbols.T_OPEN_TAG,yyline+1,yycolumn+1,yytext());
	} else {
		return new Token(ParserSymbols.T_INLINE_HTML,yyline+1,yycolumn+1,yytext());
	}
}

<YYINITIAL>"<%="|"<?=" {
	if ((yycharat(1)=='%' && aspTags) || (yycharat(1)=='?' && shortTags)) {
		yybegin(ST_IN_SCRIPTING);
		return new Token(ParserSymbols.T_OPEN_TAG_WITH_ECHO,yyline+1,yycolumn+1,yytext());
	} else {
		return new Token(ParserSymbols.T_INLINE_HTML,yyline+1,yycolumn+1,yytext());
	}
}

<YYINITIAL>"<%" {
	if (aspTags) {
		yybegin(ST_IN_SCRIPTING);
		return new Token(ParserSymbols.T_OPEN_TAG,yyline+1,yycolumn+1,yytext());
	} else {
		return new Token(ParserSymbols.T_INLINE_HTML,yyline+1,yycolumn+1,yytext());
	}
}

<YYINITIAL>"<?php"([ \t]|{NEWLINE}) {
	yybegin(ST_IN_SCRIPTING);
	return new Token(ParserSymbols.T_OPEN_TAG,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING,ST_DOUBLE_QUOTES,ST_HEREDOC,ST_BACKQUOTE>"$"{LABEL} {
	return new Token(ParserSymbols.T_VARIABLE,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>{LABEL} {
	return new Token(ParserSymbols.T_STRING,yyline+1,yycolumn+1,yytext());
}

<ST_DOUBLE_QUOTES,ST_BACKQUOTE,ST_HEREDOC>{LABEL} {
	return new Token(ParserSymbols.T_STRING,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>{WHITESPACE} {
	return new Token(ParserSymbols.T_WHITESPACE,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"#"|"//" {
	yybegin(ST_ONE_LINE_COMMENT);
	return yymore();
}

<ST_ONE_LINE_COMMENT>"?"|"%"|">" {
	return yymore();
}

<ST_ONE_LINE_COMMENT>[^\n\r?%>]+ {
	return yymore();
}

<ST_ONE_LINE_COMMENT>{NEWLINE} {
	yybegin(ST_IN_SCRIPTING);
	return new Token(ParserSymbols.T_COMMENT,yyline+1,yycolumn+1,yytext());
}

<ST_ONE_LINE_COMMENT>"?>"|"%>" {
    if (aspTags || yycharat(yylength()-2) != '%') { /* asp comment? */
		yypushback(yylength()-2);
		yybegin(ST_IN_SCRIPTING);
		return new Token(ParserSymbols.T_COMMENT,yyline+1,yycolumn-2,yytext().Substring(0,yylength()-2));
	} else {
		return yymore();
	}
}

<ST_IN_SCRIPTING>"/**"{WHITESPACE} {
	yybegin(ST_DOC_COMMENT);
	return yymore();
}

<ST_IN_SCRIPTING>"/*" {
	yybegin(ST_COMMENT);
	return yymore();
}

<ST_COMMENT,ST_DOC_COMMENT>[^*]+ {
	return yymore();
}

<ST_DOC_COMMENT>"*/" {
	yybegin(ST_IN_SCRIPTING);
	return new Token(ParserSymbols.T_DOC_COMMENT,yyline+1,yycolumn+1,yytext());
}

<ST_COMMENT>"*/" {
	yybegin(ST_IN_SCRIPTING);
	return new Token(ParserSymbols.T_COMMENT,yyline+1,yycolumn+1,yytext());
}

<ST_COMMENT,ST_DOC_COMMENT>"*" {
	return yymore();
}

<ST_IN_SCRIPTING>("?>"|"</script"({WHITESPACE})*">"){NEWLINE}? {
	yybegin(YYINITIAL);
	return new Token(ParserSymbols.T_CLOSE_TAG,yyline+1,yycolumn+1,yytext());  /* implicit ';' at php-end tag */
}

<ST_IN_SCRIPTING>"%>"{NEWLINE}? {
	if (aspTags) {
		yybegin(YYINITIAL);
		return new Token(ParserSymbols.T_CLOSE_TAG,yyline+1,yycolumn+1,yytext());  /* implicit ';' at php-end tag */
	} else {
		yypushback(1);
		return new Token(ParserSymbols.MOD,yyline+1,yycolumn-1,yycharat(0).ToString());
	}
}

<ST_IN_SCRIPTING>([\"]([^$\"\\]|("\\".))*[\"]) {
	string s = yytext().Substring(1,yylength()-2);
	StringBuilder t = new StringBuilder(s.Length);
	for (int i=0; i<s.Length; i++) {
		if (s[i] == '\\') {
			i++;
			if (i == s.Length) {
				t.Append('\\');
				break;
			}

			switch (s[i]) {
				case 'n':
					t.Append('\n'); break;
				case 'r':
					t.Append('\r'); break;
				case 't':
					t.Append('\t'); break;
				case '\\':
				case '$':
				case '"':
					t.Append(s[i]); break;
				default:
					/* check for an octal */
					if ((i+2 <= s.Length) && isOct(s[i]) && isOct(s[i+1]) && isOct(s[i+2])) {
						char[] octal_buf = {s[i], s[i+1], s[i+2]};
						t.Append(System.Convert.ToChar(System.Convert.ToInt32(new String(octal_buf), 8)));
						i = i+3;
					}
					/* check for a hex */
					else if ((i+2 <= s.Length) && s[i] == 'x' && isHex(s[i+1]) && isHex(s[i+2])) {
						char[] hex_buf = {s[i+1], s[i+2]};
						t.Append(System.Convert.ToChar(System.Convert.ToInt32(new String(hex_buf), 16)));
						i = i+3;
					}
					else {
						t.Append('\\');
					}
					break;
			}

		}
		else {
			t.Append(s[i]);
		}
	}
	return new Token(ParserSymbols.T_CONSTANT_ENCAPSED_STRING,yyline+1,yycolumn+1-2,t.ToString());
}

<ST_IN_SCRIPTING>([']([^'\\]|("\\".))*[']) {
	string s = yytext().Substring(1,yylength()-2);
	StringBuilder t = new StringBuilder(s.Length);
	for (int i=0; i<s.Length; i++) {
		if (s[i] == '\\') {
			i++;
			if (i == s.Length) {
				t.Append('\\');
				break;
			}

			switch (s[i]) {
				case 'n':
					t.Append('\n'); break;
				case 'r':
					t.Append('\r'); break;
				case 't':
					t.Append('\t'); break;
				case '\\':
				case '$':
				case '"':
					t.Append(s[i]); break;
				default:
					/* check for an octal */
					if ((i+2 <= s.Length) && isOct(s[i]) && isOct(s[i+1]) && isOct(s[i+2])) {
						char[] octal_buf = {s[i], s[i+1], s[i+2]};
						t.Append(System.Convert.ToChar(System.Convert.ToInt32(new String(octal_buf), 8)));
						i = i+3;
					}
					/* check for a hex */
					else if ((i+2 <= s.Length) && s[i] == 'x' && isHex(s[i+1]) && isHex(s[i+2])) {
						char[] hex_buf = {s[i+1], s[i+2]};
						t.Append(System.Convert.ToChar(System.Convert.ToInt32(new String(hex_buf), 16)));
						i = i+3;
					}
					else {
						t.Append('\\');
					}
					break;
			}

		}
		else {
			t.Append(s[i]);
		}
	}
	return new Token(ParserSymbols.T_CONSTANT_ENCAPSED_STRING,yyline+1,yycolumn+1-2,t.ToString());
}

<ST_IN_SCRIPTING>[\"] { /* originally ["] */
	yybegin(ST_DOUBLE_QUOTES);
	return new Token(ParserSymbols.DOUBLE_QUOTES,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>"<<<"{TABS_AND_SPACES}{LABEL}{NEWLINE} {
	yybegin(ST_HEREDOC);
	return new Token(ParserSymbols.T_START_HEREDOC,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>[`] {
	yybegin(ST_BACKQUOTE);
	return new Token(ParserSymbols.BACK_QUOTE,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING>['] {
	yybegin(ST_SINGLE_QUOTE);
	return new Token(ParserSymbols.SINGLE_QUOTE,yyline+1,yycolumn+1,yytext());
}

<ST_HEREDOC>^{LABEL}(";")?{NEWLINE} {
	yybegin(ST_IN_SCRIPTING);
	return new Token(ParserSymbols.T_END_HEREDOC,yyline+1,yycolumn+1,yytext());
}

<ST_DOUBLE_QUOTES,ST_BACKQUOTE,ST_HEREDOC>{ESCAPED_AND_WHITESPACE} {
	return new Token(ParserSymbols.T_ENCAPSED_AND_WHITESPACE,yyline+1,yycolumn+1,yytext());
}

<ST_SINGLE_QUOTE>([^'\\]|\\[^'\\])+ {
	return new Token(ParserSymbols.T_ENCAPSED_AND_WHITESPACE,yyline+1,yycolumn+1,yytext());
}

<ST_DOUBLE_QUOTES>[`]+ {
	return new Token(ParserSymbols.T_ENCAPSED_AND_WHITESPACE,yyline+1,yycolumn+1,yytext());
}

<ST_BACKQUOTE>[\"]+ { /* originally ["]+ */
	return new Token(ParserSymbols.T_ENCAPSED_AND_WHITESPACE,yyline+1,yycolumn+1,yytext());
}

<ST_DOUBLE_QUOTES,ST_BACKQUOTE,ST_HEREDOC>"$"[^a-zA-Z_\x7f-\xff{] {
	if (yylength() == 2) {
		yypushback(1);
		return new Token(ParserSymbols.T_CHARACTER,yyline+1,yycolumn-1,yycharat(0).ToString());
	}
	return new Token(ParserSymbols.T_CHARACTER,yyline+1,yycolumn+1,yytext());
}

<ST_DOUBLE_QUOTES,ST_BACKQUOTE,ST_HEREDOC>{ENCAPSED_TOKENS} {
	switch (yycharat(0)) {
		case '[': return new Token(ParserSymbols.SQUARE_BRACE_OPEN,yyline+1,yycolumn+1,yytext());
		case ']': return new Token(ParserSymbols.SQUARE_BRACE_CLOSE,yyline+1,yycolumn+1,yytext());
		case '{': return new Token(ParserSymbols.CURLY_BRACE_OPEN,yyline+1,yycolumn+1,yytext());
		case '}': return new Token(ParserSymbols.CURLY_BRACE_CLOSE,yyline+1,yycolumn+1,yytext());
		case '$': return new Token(ParserSymbols.DOLLAR,yyline+1,yycolumn+1,yytext());
	}
}

<ST_DOUBLE_QUOTES,ST_BACKQUOTE,ST_HEREDOC>"{$" {
	yypushstate(ST_IN_SCRIPTING);
	yypushback(1);
	return new Token(ParserSymbols.T_CURLY_OPEN,yyline+1,yycolumn-1,yycharat(0).ToString());
}

<ST_SINGLE_QUOTE>"\\'" {
	return new Token(ParserSymbols.T_CHARACTER,yyline+1,yycolumn+1,'\''.ToString());
}

<ST_SINGLE_QUOTE>"\\\\" {
	return new Token(ParserSymbols.T_CHARACTER,yyline+1,yycolumn+1,'\\'.ToString());
}

<ST_DOUBLE_QUOTES>"\\\"" {
	return new Token(ParserSymbols.T_CHARACTER,yyline+1,yycolumn+1,'"'.ToString());
}

<ST_BACKQUOTE>"\\`" {
	return new Token(ParserSymbols.T_CHARACTER,yyline+1,yycolumn+1,'`'.ToString());
}

<ST_DOUBLE_QUOTES,ST_BACKQUOTE,ST_HEREDOC>"\\"[0-7]{1,3} {
	char c = System.Convert.ToChar(System.Convert.ToInt32(yytext().Substring(1,4), 8));
	return new Token(ParserSymbols.T_CHARACTER,yyline+1,yycolumn+1,c.ToString());
}

<ST_DOUBLE_QUOTES,ST_BACKQUOTE,ST_HEREDOC>"\\x"[0-9A-Fa-f]{1,2} {
	char c = System.Convert.ToChar(System.Convert.ToInt32(yytext().Substring(1,3), 16));
	return new Token(ParserSymbols.T_CHARACTER,yyline+1,yycolumn+1,c.ToString());
}

<ST_DOUBLE_QUOTES,ST_BACKQUOTE,ST_HEREDOC>"\\"{ANY_CHAR} {
	char c;
	switch (yycharat(1)) {
		case 'n':
			c = '\n'; break;
		case 'r':
			c = '\r'; break;
		case 't':
			c = '\t'; break;
		case '\\':
		case '$':
		case '"':
			c = yycharat(1); break;
		default:
			return new Token(ParserSymbols.T_BAD_CHARACTER,yyline+1,yycolumn+1,yytext());
	}
	return new Token(ParserSymbols.T_CHARACTER,yyline+1,yycolumn+1,c.ToString());
}

<ST_HEREDOC>[\"'`]+ {
	return new Token(ParserSymbols.T_ENCAPSED_AND_WHITESPACE,yyline+1,yycolumn+1,yytext());
}

<ST_DOUBLE_QUOTES>[\"] {
	yybegin(ST_IN_SCRIPTING);
	return new Token(ParserSymbols.DOUBLE_QUOTES,yyline+1,yycolumn+1,yytext());
}

<ST_BACKQUOTE>[`] {
	yybegin(ST_IN_SCRIPTING);
	return new Token(ParserSymbols.BACK_QUOTE,yyline+1,yycolumn+1,yytext());
}

<ST_SINGLE_QUOTE>['] {
	yybegin(ST_IN_SCRIPTING);
	return new Token(ParserSymbols.SINGLE_QUOTE,yyline+1,yycolumn+1,yytext());
}

<ST_DOUBLE_QUOTES,ST_BACKQUOTE,YYINITIAL,ST_IN_SCRIPTING,ST_LOOKING_FOR_PROPERTY><<EOF>> {
	return new Token(ParserSymbols.EOF,yyline+1,yycolumn+1,yytext());
}

<ST_COMMENT,ST_DOC_COMMENT><<EOF>> {
	Console.Out.WriteLine("Warning: Unterminated comment at the end of input.");
	return new Token(ParserSymbols.EOF,yyline+1,yycolumn+1,yytext());
}

<ST_IN_SCRIPTING,YYINITIAL,ST_DOUBLE_QUOTES,ST_BACKQUOTE,ST_SINGLE_QUOTE,ST_HEREDOC>{ANY_CHAR} {
	Report.Error(200, System.Convert.ToString(yycharat(0)), yyline+1, yycolumn+1);
}