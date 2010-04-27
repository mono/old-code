// created by jay 0.7 (c) 1998 Axel.Schreiner@informatik.uni-osnabrueck.de

//line 12 "parse.y"
    
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;

namespace NETRuby
{
    public class Parser
    {
        public interface Lexer
        {
            EXPR State
            {
               get;
               set;
            }
            void COND_PUSH();
            void COND_POP();
            void CMDARG_PUSH();
            void CMDARG_POP();
        }

        public Parser(NetRuby rb, RThread th, bool in_eval)
        {
            evalTree = null;
            ruby = rb;
            thread = th;
            compile_for_eval = in_eval;
        }

        public Parser(NetRuby rb)
        {
            evalTree = null;
            ruby = rb;
            thread = rb.GetCurrentContext();
            compile_for_eval = false;
        }

        public object Parse(Lexer lexer)
        {
            evalTree = null;
            lex = lexer;
            object n = yyparse((yyParser.yyInput)lex, new yydebug.yyDebugSimple());
            if (n == null)
                return evalTree;
            return null;
        }
        
        private Lexer lex;
        internal RNode evalTree;

        internal enum ID
        {
            SCOPE_SHIFT = 3,
            SCOPE_MASK = 7,
            LOCAL = 1,
            INSTANCE = 2,
            GLOBAL = 3,
            ATTRSET = 4,
            CONST = 5,
            CLASS = 6,
        }

        internal string sourcefile
        {
            get { return thread.file; }
        }
        internal int sourceline
        {
            get { return thread.line; }
            set { thread.line = value; }
        }

        public NetRuby ruby;
        internal RThread thread;
        internal bool verbose
        {
            get { return ruby.verbose; }
        }
        internal void warn(string s)
        {
            ruby.warn(s);
        }
/*
        internal uint[] Locals
        {
            get { return thread.Locals; }
        }
*/        
        private int class_nest = 0;
        private int in_single = 0;
        private int in_def = 0;
        private bool compile_for_eval = false;
        private uint cur_mid = 0;
        private bool in_defined = false;

//line default

  /** simplified error message.
      @see <a href="#yyerror(java.lang.String, java.lang.String[])">yyerror</a>
    */
  public void yyerror (string message) {
    yyerror(message, null);
  }

  /** (syntax) error message.
      Can be overwritten to control message format.
      @param message text to be displayed.
      @param expected vector of acceptable tokens, if available.
    */
  public void yyerror (string message, string[] expected) {
    if ((expected != null) && (expected.Length  > 0)) {
      StringBuilder sb = new StringBuilder();    
      sb.AppendFormat("{0}, expecting", message);
      for (int n = 0; n < expected.Length; ++ n)
        sb.AppendFormat(" {0}", expected[n]);
        thread.CompileError(sb.ToString());
    } else
      thread.CompileError (message);
  }

  /** debugging support, requires the package jay.yydebug.
      Set to null to suppress debugging messages.
    */
//t  protected yydebug.yyDebug yydebug;

  protected static  int yyFinal = 1;
//t  public static  string [] yyRule = {
//t    "$accept : program",
//t    "$$1 :",
//t    "program : $$1 compstmt",
//t    "compstmt : stmts opt_terms",
//t    "stmts : none",
//t    "stmts : stmt",
//t    "stmts : stmts terms stmt",
//t    "stmts : error stmt",
//t    "$$2 :",
//t    "stmt : kALIAS fitem $$2 fitem",
//t    "stmt : kALIAS tGVAR tGVAR",
//t    "stmt : kALIAS tGVAR tBACK_REF",
//t    "stmt : kALIAS tGVAR tNTH_REF",
//t    "stmt : kUNDEF undef_list",
//t    "stmt : stmt kIF_MOD expr",
//t    "stmt : stmt kUNLESS_MOD expr",
//t    "stmt : stmt kWHILE_MOD expr",
//t    "stmt : stmt kUNTIL_MOD expr",
//t    "stmt : stmt kRESCUE_MOD stmt",
//t    "$$3 :",
//t    "stmt : klBEGIN $$3 '{' compstmt '}'",
//t    "stmt : klEND '{' compstmt '}'",
//t    "stmt : lhs '=' command_call",
//t    "stmt : mlhs '=' command_call",
//t    "stmt : lhs '=' mrhs_basic",
//t    "stmt : expr",
//t    "expr : mlhs '=' mrhs",
//t    "expr : kRETURN ret_args",
//t    "expr : command_call",
//t    "expr : expr kAND expr",
//t    "expr : expr kOR expr",
//t    "expr : kNOT expr",
//t    "expr : '!' command_call",
//t    "expr : arg",
//t    "command_call : command",
//t    "command_call : block_command",
//t    "block_command : block_call",
//t    "block_command : block_call '.' operation2 command_args",
//t    "block_command : block_call tCOLON2 operation2 command_args",
//t    "command : operation command_args",
//t    "command : primary '.' operation2 command_args",
//t    "command : primary tCOLON2 operation2 command_args",
//t    "command : kSUPER command_args",
//t    "command : kYIELD ret_args",
//t    "mlhs : mlhs_basic",
//t    "mlhs : tLPAREN mlhs_entry ')'",
//t    "mlhs_entry : mlhs_basic",
//t    "mlhs_entry : tLPAREN mlhs_entry ')'",
//t    "mlhs_basic : mlhs_head",
//t    "mlhs_basic : mlhs_head mlhs_item",
//t    "mlhs_basic : mlhs_head tSTAR mlhs_node",
//t    "mlhs_basic : mlhs_head tSTAR",
//t    "mlhs_basic : tSTAR mlhs_node",
//t    "mlhs_basic : tSTAR",
//t    "mlhs_item : mlhs_node",
//t    "mlhs_item : tLPAREN mlhs_entry ')'",
//t    "mlhs_head : mlhs_item ','",
//t    "mlhs_head : mlhs_head mlhs_item ','",
//t    "mlhs_node : variable",
//t    "mlhs_node : primary '[' aref_args ']'",
//t    "mlhs_node : primary '.' tIDENTIFIER",
//t    "mlhs_node : primary tCOLON2 tIDENTIFIER",
//t    "mlhs_node : primary '.' tCONSTANT",
//t    "mlhs_node : backref",
//t    "lhs : variable",
//t    "lhs : primary '[' aref_args ']'",
//t    "lhs : primary '.' tIDENTIFIER",
//t    "lhs : primary tCOLON2 tIDENTIFIER",
//t    "lhs : primary '.' tCONSTANT",
//t    "lhs : backref",
//t    "cname : tIDENTIFIER",
//t    "cname : tCONSTANT",
//t    "fname : tIDENTIFIER",
//t    "fname : tCONSTANT",
//t    "fname : tFID",
//t    "fname : op",
//t    "fname : reswords",
//t    "fitem : fname",
//t    "fitem : symbol",
//t    "undef_list : fitem",
//t    "$$4 :",
//t    "undef_list : undef_list ',' $$4 fitem",
//t    "op : '|'",
//t    "op : '^'",
//t    "op : '&'",
//t    "op : tCMP",
//t    "op : tEQ",
//t    "op : tEQQ",
//t    "op : tMATCH",
//t    "op : '>'",
//t    "op : tGEQ",
//t    "op : '<'",
//t    "op : tLEQ",
//t    "op : tLSHFT",
//t    "op : tRSHFT",
//t    "op : '+'",
//t    "op : '-'",
//t    "op : '*'",
//t    "op : tSTAR",
//t    "op : '/'",
//t    "op : '%'",
//t    "op : tPOW",
//t    "op : '~'",
//t    "op : tUPLUS",
//t    "op : tUMINUS",
//t    "op : tAREF",
//t    "op : tASET",
//t    "op : '`'",
//t    "reswords : k__LINE__",
//t    "reswords : k__FILE__",
//t    "reswords : klBEGIN",
//t    "reswords : klEND",
//t    "reswords : kALIAS",
//t    "reswords : kAND",
//t    "reswords : kBEGIN",
//t    "reswords : kBREAK",
//t    "reswords : kCASE",
//t    "reswords : kCLASS",
//t    "reswords : kDEF",
//t    "reswords : kDEFINED",
//t    "reswords : kDO",
//t    "reswords : kELSE",
//t    "reswords : kELSIF",
//t    "reswords : kEND",
//t    "reswords : kENSURE",
//t    "reswords : kFALSE",
//t    "reswords : kFOR",
//t    "reswords : kIF_MOD",
//t    "reswords : kIN",
//t    "reswords : kMODULE",
//t    "reswords : kNEXT",
//t    "reswords : kNIL",
//t    "reswords : kNOT",
//t    "reswords : kOR",
//t    "reswords : kREDO",
//t    "reswords : kRESCUE",
//t    "reswords : kRETRY",
//t    "reswords : kRETURN",
//t    "reswords : kSELF",
//t    "reswords : kSUPER",
//t    "reswords : kTHEN",
//t    "reswords : kTRUE",
//t    "reswords : kUNDEF",
//t    "reswords : kUNLESS_MOD",
//t    "reswords : kUNTIL_MOD",
//t    "reswords : kWHEN",
//t    "reswords : kWHILE_MOD",
//t    "reswords : kYIELD",
//t    "reswords : kRESCUE_MOD",
//t    "arg : lhs '=' arg",
//t    "$$5 :",
//t    "arg : variable tOP_ASGN $$5 arg",
//t    "arg : primary '[' aref_args ']' tOP_ASGN arg",
//t    "arg : primary '.' tIDENTIFIER tOP_ASGN arg",
//t    "arg : primary '.' tCONSTANT tOP_ASGN arg",
//t    "arg : primary tCOLON2 tIDENTIFIER tOP_ASGN arg",
//t    "arg : backref tOP_ASGN arg",
//t    "arg : arg tDOT2 arg",
//t    "arg : arg tDOT3 arg",
//t    "arg : arg '+' arg",
//t    "arg : arg '-' arg",
//t    "arg : arg '*' arg",
//t    "arg : arg '/' arg",
//t    "arg : arg '%' arg",
//t    "arg : arg tPOW arg",
//t    "arg : tUPLUS arg",
//t    "arg : tUMINUS arg",
//t    "arg : arg '|' arg",
//t    "arg : arg '^' arg",
//t    "arg : arg '&' arg",
//t    "arg : arg tCMP arg",
//t    "arg : arg '>' arg",
//t    "arg : arg tGEQ arg",
//t    "arg : arg '<' arg",
//t    "arg : arg tLEQ arg",
//t    "arg : arg tEQ arg",
//t    "arg : arg tEQQ arg",
//t    "arg : arg tNEQ arg",
//t    "arg : arg tMATCH arg",
//t    "arg : arg tNMATCH arg",
//t    "arg : '!' arg",
//t    "arg : '~' arg",
//t    "arg : arg tLSHFT arg",
//t    "arg : arg tRSHFT arg",
//t    "arg : arg tANDOP arg",
//t    "arg : arg tOROP arg",
//t    "$$6 :",
//t    "arg : kDEFINED opt_nl $$6 arg",
//t    "arg : arg '?' arg ':' arg",
//t    "arg : primary",
//t    "aref_args : none",
//t    "aref_args : command_call opt_nl",
//t    "aref_args : args ',' command_call opt_nl",
//t    "aref_args : args trailer",
//t    "aref_args : args ',' tSTAR arg opt_nl",
//t    "aref_args : assocs trailer",
//t    "aref_args : tSTAR arg opt_nl",
//t    "paren_args : '(' none ')'",
//t    "paren_args : '(' call_args opt_nl ')'",
//t    "paren_args : '(' block_call opt_nl ')'",
//t    "paren_args : '(' args ',' block_call opt_nl ')'",
//t    "opt_paren_args : none",
//t    "opt_paren_args : paren_args",
//t    "call_args : command",
//t    "call_args : args ',' command",
//t    "call_args : args opt_block_arg",
//t    "call_args : args ',' tSTAR arg opt_block_arg",
//t    "call_args : assocs opt_block_arg",
//t    "call_args : assocs ',' tSTAR arg opt_block_arg",
//t    "call_args : args ',' assocs opt_block_arg",
//t    "call_args : args ',' assocs ',' tSTAR arg opt_block_arg",
//t    "call_args : tSTAR arg opt_block_arg",
//t    "call_args : block_arg",
//t    "$$7 :",
//t    "command_args : $$7 call_args",
//t    "block_arg : tAMPER arg",
//t    "opt_block_arg : ',' block_arg",
//t    "opt_block_arg : none",
//t    "args : arg",
//t    "args : args ',' arg",
//t    "mrhs : arg",
//t    "mrhs : mrhs_basic",
//t    "mrhs_basic : args ',' arg",
//t    "mrhs_basic : args ',' tSTAR arg",
//t    "mrhs_basic : tSTAR arg",
//t    "ret_args : call_args",
//t    "primary : literal",
//t    "primary : string",
//t    "primary : tXSTRING",
//t    "primary : tQWORDS",
//t    "primary : tDXSTRING",
//t    "primary : tDREGEXP",
//t    "primary : var_ref",
//t    "primary : backref",
//t    "primary : tFID",
//t    "primary : kBEGIN compstmt rescue opt_else ensure kEND",
//t    "primary : tLPAREN compstmt ')'",
//t    "primary : primary tCOLON2 tCONSTANT",
//t    "primary : tCOLON3 cname",
//t    "primary : primary '[' aref_args ']'",
//t    "primary : tLBRACK aref_args ']'",
//t    "primary : tLBRACE assoc_list '}'",
//t    "primary : kRETURN '(' ret_args ')'",
//t    "primary : kRETURN '(' ')'",
//t    "primary : kRETURN",
//t    "primary : kYIELD '(' ret_args ')'",
//t    "primary : kYIELD '(' ')'",
//t    "primary : kYIELD",
//t    "$$8 :",
//t    "primary : kDEFINED opt_nl '(' $$8 expr ')'",
//t    "primary : operation brace_block",
//t    "primary : method_call",
//t    "primary : method_call brace_block",
//t    "primary : kIF expr then compstmt if_tail kEND",
//t    "primary : kUNLESS expr then compstmt opt_else kEND",
//t    "$$9 :",
//t    "$$10 :",
//t    "primary : kWHILE $$9 expr do $$10 compstmt kEND",
//t    "$$11 :",
//t    "$$12 :",
//t    "primary : kUNTIL $$11 expr do $$12 compstmt kEND",
//t    "primary : kCASE expr opt_terms case_body kEND",
//t    "primary : kCASE opt_terms case_body kEND",
//t    "$$13 :",
//t    "$$14 :",
//t    "primary : kFOR block_var kIN $$13 expr do $$14 compstmt kEND",
//t    "$$15 :",
//t    "primary : kCLASS cname superclass $$15 compstmt kEND",
//t    "$$16 :",
//t    "$$17 :",
//t    "primary : kCLASS tLSHFT expr $$16 term $$17 compstmt kEND",
//t    "$$18 :",
//t    "primary : kMODULE cname $$18 compstmt kEND",
//t    "$$19 :",
//t    "primary : kDEF fname $$19 f_arglist compstmt rescue opt_else ensure kEND",
//t    "$$20 :",
//t    "$$21 :",
//t    "primary : kDEF singleton dot_or_colon $$20 fname $$21 f_arglist compstmt rescue opt_else ensure kEND",
//t    "primary : kBREAK",
//t    "primary : kNEXT",
//t    "primary : kREDO",
//t    "primary : kRETRY",
//t    "then : term",
//t    "then : kTHEN",
//t    "then : term kTHEN",
//t    "do : term",
//t    "do : kDO_COND",
//t    "if_tail : opt_else",
//t    "if_tail : kELSIF expr then compstmt if_tail",
//t    "opt_else : none",
//t    "opt_else : kELSE compstmt",
//t    "block_var : lhs",
//t    "block_var : mlhs",
//t    "opt_block_var : none",
//t    "opt_block_var : '|' '|'",
//t    "opt_block_var : tOROP",
//t    "opt_block_var : '|' block_var '|'",
//t    "$$22 :",
//t    "do_block : kDO_BLOCK $$22 opt_block_var compstmt kEND",
//t    "block_call : command do_block",
//t    "block_call : block_call '.' operation2 opt_paren_args",
//t    "block_call : block_call tCOLON2 operation2 opt_paren_args",
//t    "method_call : operation paren_args",
//t    "method_call : primary '.' operation2 opt_paren_args",
//t    "method_call : primary tCOLON2 operation2 paren_args",
//t    "method_call : primary tCOLON2 operation3",
//t    "method_call : kSUPER paren_args",
//t    "method_call : kSUPER",
//t    "$$23 :",
//t    "brace_block : '{' $$23 opt_block_var compstmt '}'",
//t    "$$24 :",
//t    "brace_block : kDO $$24 opt_block_var compstmt kEND",
//t    "case_body : kWHEN when_args then compstmt cases",
//t    "when_args : args",
//t    "when_args : args ',' tSTAR arg",
//t    "when_args : tSTAR arg",
//t    "cases : opt_else",
//t    "cases : case_body",
//t    "exc_list : none",
//t    "exc_list : args",
//t    "exc_var : tASSOC lhs",
//t    "exc_var : none",
//t    "rescue : kRESCUE exc_list exc_var then compstmt rescue",
//t    "rescue : none",
//t    "ensure : none",
//t    "ensure : kENSURE compstmt",
//t    "literal : numeric",
//t    "literal : symbol",
//t    "literal : tREGEXP",
//t    "string : tSTRING",
//t    "string : tDSTRING",
//t    "string : string tSTRING",
//t    "string : string tDSTRING",
//t    "symbol : tSYMBEG sym",
//t    "sym : fname",
//t    "sym : tIVAR",
//t    "sym : tGVAR",
//t    "sym : tCVAR",
//t    "numeric : tINTEGER",
//t    "numeric : tFLOAT",
//t    "variable : tIDENTIFIER",
//t    "variable : tIVAR",
//t    "variable : tGVAR",
//t    "variable : tCONSTANT",
//t    "variable : tCVAR",
//t    "variable : kNIL",
//t    "variable : kSELF",
//t    "variable : kTRUE",
//t    "variable : kFALSE",
//t    "variable : k__FILE__",
//t    "variable : k__LINE__",
//t    "var_ref : variable",
//t    "backref : tNTH_REF",
//t    "backref : tBACK_REF",
//t    "superclass : term",
//t    "$$25 :",
//t    "superclass : '<' $$25 expr term",
//t    "superclass : error term",
//t    "f_arglist : '(' f_args opt_nl ')'",
//t    "f_arglist : f_args term",
//t    "f_args : f_arg ',' f_optarg ',' f_rest_arg opt_f_block_arg",
//t    "f_args : f_arg ',' f_optarg opt_f_block_arg",
//t    "f_args : f_arg ',' f_rest_arg opt_f_block_arg",
//t    "f_args : f_arg opt_f_block_arg",
//t    "f_args : f_optarg ',' f_rest_arg opt_f_block_arg",
//t    "f_args : f_optarg opt_f_block_arg",
//t    "f_args : f_rest_arg opt_f_block_arg",
//t    "f_args : f_block_arg",
//t    "f_args :",
//t    "f_norm_arg : tCONSTANT",
//t    "f_norm_arg : tIVAR",
//t    "f_norm_arg : tGVAR",
//t    "f_norm_arg : tCVAR",
//t    "f_norm_arg : tIDENTIFIER",
//t    "f_arg : f_norm_arg",
//t    "f_arg : f_arg ',' f_norm_arg",
//t    "f_opt : tIDENTIFIER '=' arg",
//t    "f_optarg : f_opt",
//t    "f_optarg : f_optarg ',' f_opt",
//t    "f_rest_arg : tSTAR tIDENTIFIER",
//t    "f_rest_arg : tSTAR",
//t    "f_block_arg : tAMPER tIDENTIFIER",
//t    "opt_f_block_arg : ',' f_block_arg",
//t    "opt_f_block_arg : none",
//t    "singleton : var_ref",
//t    "$$26 :",
//t    "singleton : '(' $$26 expr opt_nl ')'",
//t    "assoc_list : none",
//t    "assoc_list : assocs trailer",
//t    "assoc_list : args trailer",
//t    "assocs : assoc",
//t    "assocs : assocs ',' assoc",
//t    "assoc : arg tASSOC arg",
//t    "operation : tIDENTIFIER",
//t    "operation : tCONSTANT",
//t    "operation : tFID",
//t    "operation2 : tIDENTIFIER",
//t    "operation2 : tCONSTANT",
//t    "operation2 : tFID",
//t    "operation2 : op",
//t    "operation3 : tIDENTIFIER",
//t    "operation3 : tFID",
//t    "operation3 : op",
//t    "dot_or_colon : '.'",
//t    "dot_or_colon : tCOLON2",
//t    "opt_terms :",
//t    "opt_terms : terms",
//t    "opt_nl :",
//t    "opt_nl : '\\n'",
//t    "trailer :",
//t    "trailer : '\\n'",
//t    "trailer : ','",
//t    "term : ';'",
//t    "term : '\\n'",
//t    "terms : term",
//t    "terms : terms ';'",
//t    "none :",
//t  };
  protected static  string [] yyNames = {    
    "end-of-file",null,null,null,null,null,null,null,null,null,"'\\n'",
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,"'!'",null,null,null,"'%'",
    "'&'",null,"'('","')'","'*'","'+'","','","'-'","'.'","'/'",null,null,
    null,null,null,null,null,null,null,null,"':'","';'","'<'","'='","'>'",
    "'?'",null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,"'['",null,"']'","'^'",null,"'`'",null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,"'{'","'|'","'}'","'~'",null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,"kCLASS","kMODULE","kDEF","kUNDEF","kBEGIN","kRESCUE","kENSURE",
    "kEND","kIF","kUNLESS","kTHEN","kELSIF","kELSE","kCASE","kWHEN",
    "kWHILE","kUNTIL","kFOR","kBREAK","kNEXT","kREDO","kRETRY","kIN",
    "kDO","kDO_COND","kDO_BLOCK","kRETURN","kYIELD","kSUPER","kSELF",
    "kNIL","kTRUE","kFALSE","kAND","kOR","kNOT","kIF_MOD","kUNLESS_MOD",
    "kWHILE_MOD","kUNTIL_MOD","kRESCUE_MOD","kALIAS","kDEFINED","klBEGIN",
    "klEND","k__LINE__","k__FILE__","tIDENTIFIER","tFID","tGVAR","tIVAR",
    "tCONSTANT","tCVAR","tINTEGER","tFLOAT","tSTRING","tXSTRING",
    "tREGEXP","tDSTRING","tDXSTRING","tDREGEXP","tNTH_REF","tBACK_REF",
    "tQWORDS","tUPLUS","tUMINUS","tPOW","tCMP","tEQ","tEQQ","tNEQ","tGEQ",
    "tLEQ","tANDOP","tOROP","tMATCH","tNMATCH","tDOT2","tDOT3","tAREF",
    "tASET","tLSHFT","tRSHFT","tCOLON2","tCOLON3","tOP_ASGN","tASSOC",
    "tLPAREN","tLBRACK","tLBRACE","tSTAR","tAMPER","tSYMBEG","LAST_TOKEN",
  };

  /** index-checked interface to yyNames [].
      @param token single character or %token value.
      @return token name or [illegal] or [unknown].
    */
//t  public static string yyname (int token) {
//t    if ((token < 0) || (token > yyNames .Length)) return "[illegal]";
//t    string name;
//t    if ((name = yyNames [token]) != null) return name;
//t    return "[unknown]";
//t  }

  /** computes list of expected tokens on error by tracing the tables.
      @param state for which to compute the list.
      @return list of token names.
    */
  protected string[] yyExpecting (int state) {
    int token, n, len = 0;
    bool[] ok = new bool[yyNames .Length];

    if ((n = yySindex[state]) != 0)
      for (token = n < 0 ? -n : 0;
           (token < yyNames .Length) && (n+token < yyTable.Length); ++ token)
        if (yyCheck[n+token] == token && !ok[token] && yyNames [token] != null) {
          ++ len;
          ok[token] = true;
        }
    if ((n = yyRindex[state]) != 0)
      for (token = n < 0 ? -n : 0;
           (token < yyNames .Length) && (n+token < yyTable.Length); ++ token)
        if (yyCheck[n+token] == token && !ok[token] && yyNames [token] != null) {
          ++ len;
          ok[token] = true;
        }

    string [] result = new string[len];
    for (n = token = 0; n < len;  ++ token)
      if (ok[token]) result[n++] = yyNames [token];
    return result;
  }

  /** the generated parser, with debugging messages.
      Maintains a state and a value stack, currently with fixed maximum size.
      @param yyLex scanner.
      @param yydebug debug message writer implementing yyDebug, or null.
      @return result of the last reduction, if any.
      @throws yyException on irrecoverable parse error.
    */
  public Object yyparse (yyParser.yyInput yyLex, Object yydebug)
				 {
//t    this.yydebug = (yydebug.yyDebug)yydebug;
    return yyparse(yyLex);
  }

  /** initial size and increment of the state/value stack [default 256].
      This is not final so that it can be overwritten outside of invocations
      of yyparse().
    */
  protected int yyMax;

  /** executed at the beginning of a reduce action.
      Used as $$ = yyDefault($1), prior to the user-specified action, if any.
      Can be overwritten to provide deep copy, etc.
      @param first value for $1, or null.
      @return first.
    */
  protected Object yyDefault (Object first) {
    return first;
  }

  /** the generated parser.
      Maintains a state and a value stack, currently with fixed maximum size.
      @param yyLex scanner.
      @return result of the last reduction, if any.
      @throws yyException on irrecoverable parse error.
    */
  public Object yyparse (yyParser.yyInput yyLex)
				{
    if (yyMax <= 0) yyMax = 256;			// initial size
    int yyState = 0;                                   // state stack ptr
    int [] yyStates = new int[yyMax];	                // state stack 
    Object yyVal = null;                               // value stack ptr
    Object [] yyVals = new Object[yyMax];	        // value stack
    int yyToken = -1;					// current input
    int yyErrorFlag = 0;				// #tks to shift

    int yyTop = 0;
    goto skip;
    yyLoop:
    yyTop++;
    skip:
    for (;; ++ yyTop) {
      if (yyTop >= yyStates.Length) {			// dynamically increase
        int[] i = new int[yyStates.Length+yyMax];
        System.Array.Copy(yyStates, i, 0);
        yyStates = i;
        Object[] o = new Object[yyVals.Length+yyMax];
        System.Array.Copy(yyVals, o, 0);
        yyVals = o;
      }
      yyStates[yyTop] = yyState;
      yyVals[yyTop] = yyVal;
//t      if (yydebug != null) yydebug.push(yyState, yyVal);

      yyDiscarded: for (;;) {	// discarding a token does not change stack
        int yyN;
        if ((yyN = yyDefRed[yyState]) == 0) {	// else [default] reduce (yyN)
          if (yyToken < 0) {
            yyToken = yyLex.advance() ? yyLex.token() : 0;
//t            if (yydebug != null)
//t              yydebug.lex(yyState, yyToken, yyname(yyToken), yyLex.value());
          }
          if ((yyN = yySindex[yyState]) != 0 && ((yyN += yyToken) >= 0)
              && (yyN < yyTable.Length) && (yyCheck[yyN] == yyToken)) {
//t            if (yydebug != null)
//t              yydebug.shift(yyState, yyTable[yyN], yyErrorFlag-1);
            yyState = yyTable[yyN];		// shift to yyN
            yyVal = yyLex.value();
            yyToken = -1;
            if (yyErrorFlag > 0) -- yyErrorFlag;
            goto yyLoop;
          }
          if ((yyN = yyRindex[yyState]) != 0 && (yyN += yyToken) >= 0
              && yyN < yyTable.Length && yyCheck[yyN] == yyToken)
            yyN = yyTable[yyN];			// reduce (yyN)
          else
            switch (yyErrorFlag) {
  
            case 0:
              yyerror("syntax error", yyExpecting(yyState));
//t              if (yydebug != null) yydebug.error("syntax error");
              goto case 1;
            case 1: case 2:
              yyErrorFlag = 3;
              do {
                if ((yyN = yySindex[yyStates[yyTop]]) != 0
                    && (yyN += Token.yyErrorCode) >= 0 && yyN < yyTable.Length
                    && yyCheck[yyN] == Token.yyErrorCode) {
//t                  if (yydebug != null)
//t                    yydebug.shift(yyStates[yyTop], yyTable[yyN], 3);
                  yyState = yyTable[yyN];
                  yyVal = yyLex.value();
                  goto yyLoop;
                }
//t                if (yydebug != null) yydebug.pop(yyStates[yyTop]);
              } while (-- yyTop >= 0);
//t              if (yydebug != null) yydebug.reject();
              throw new yyParser.yyException("irrecoverable syntax error");
  
            case 3:
              if (yyToken == 0) {
//t                if (yydebug != null) yydebug.reject();
                throw new yyParser.yyException("irrecoverable syntax error at end-of-file");
              }
//t              if (yydebug != null)
//t                yydebug.discard(yyState, yyToken, yyname(yyToken),
//t  							yyLex.value());
              yyToken = -1;
              goto yyDiscarded;		// leave stack alone
            }
        }
        int yyV = yyTop + 1-yyLen[yyN];
//t        if (yydebug != null)
//t          yydebug.reduce(yyState, yyStates[yyV-1], yyN, yyRule[yyN], yyLen[yyN]);
        yyVal = yyDefault(yyV > yyTop ? null : yyVals[yyV]);
        switch (yyN) {
case 1:
//line 230 "parse.y"
  {
                        yyVal = thread.dyna_vars;
                        lex.State = EXPR.BEG;
                        thread.TopLocalInit();
                        /*
                        if ((VALUE)ruby_class == rb_cObject) class_nest = 0;
                        else class_nest = 1;
                        */
                    }
  break;
case 2:
//line 240 "parse.y"
  {
                        if (((RNode)yyVals[0+yyTop]) != null && !compile_for_eval) {
                            /* last expression should not be void */
                            if (((RNode)yyVals[0+yyTop]) is RNBlock)
                            {
                                RNode node = ((RNode)yyVals[0+yyTop]);
                                while (node.next != null) {
                                    node = node.next;
                                }
                                void_expr(node.head);
                            }
                            else
                            {
                                void_expr(((RNode)yyVals[0+yyTop]));
                            }
                        }
                        evalTree = block_append(evalTree, ((RNode)yyVals[0+yyTop]));
                        thread.TopLocalSetup();
                        class_nest = 0;
                        thread.dyna_vars = ((RVarmap)yyVals[-1+yyTop]);
                    }
  break;
case 3:
//line 263 "parse.y"
  {
                        void_stmts(((RNode)yyVals[-1+yyTop]));
                        yyVal = ((RNode)yyVals[-1+yyTop]);
                    }
  break;
case 5:
//line 270 "parse.y"
  {
                        yyVal = new RNNewLine(thread, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 6:
//line 274 "parse.y"
  {
                        yyVal = block_append(((RNode)yyVals[-2+yyTop]), new RNNewLine(thread, ((RNode)yyVals[0+yyTop])));
                    }
  break;
case 7:
//line 278 "parse.y"
  {
                        yyVal = ((RNode)yyVals[0+yyTop]);
                    }
  break;
case 8:
//line 282 "parse.y"
  {lex.State = EXPR.FNAME;}
  break;
case 9:
//line 283 "parse.y"
  {
                        if (in_def != 0 || in_single != 0)
                            yyerror("alias within method");
                        yyVal = new RNAlias(thread, ((uint)yyVals[-2+yyTop]), ((uint)yyVals[0+yyTop]));
                    }
  break;
case 10:
//line 289 "parse.y"
  {
                        if (in_def != 0 || in_single != 0)
                            yyerror("alias within method");
                        yyVal = new RNVAlias(thread, ((uint)yyVals[-1+yyTop]), ((uint)yyVals[0+yyTop]));
                    }
  break;
case 11:
//line 295 "parse.y"
  {
                        if (in_def != 0 || in_single != 0)
                            yyerror("alias within method");
                        string buf = "$" + Convert.ToChar(((RNode)yyVals[0+yyTop]).nth);
                        yyVal = new RNVAlias(thread, ((uint)yyVals[-1+yyTop]), intern(buf));
                    }
  break;
case 12:
//line 302 "parse.y"
  {
                        yyerror("can't make alias for the number variables");
                        yyVal = null;
                    }
  break;
case 13:
//line 307 "parse.y"
  {
                        if (in_def != 0 || in_single != 0)
                            yyerror("undef within method");
                        yyVal = ((RNode)yyVals[0+yyTop]);
                    }
  break;
case 14:
//line 313 "parse.y"
  {
                        value_expr(((RNode)yyVals[0+yyTop]));
                        yyVal = new RNIf(thread, cond(((RNode)yyVals[0+yyTop])), ((RNode)yyVals[-2+yyTop]));
                    }
  break;
case 15:
//line 318 "parse.y"
  {
                        value_expr(((RNode)yyVals[0+yyTop]));
                        yyVal = new RNUnless(thread, cond(((RNode)yyVals[0+yyTop])), ((RNode)yyVals[-2+yyTop]));
                    }
  break;
case 16:
//line 323 "parse.y"
  {
                        value_expr(((RNode)yyVals[0+yyTop]));
                        if (((RNode)yyVals[-2+yyTop]) != null && ((RNode)yyVals[-2+yyTop]) is RNBegin) {
                            yyVal = new RNWhile(thread, cond(((RNode)yyVals[0+yyTop])), ((RNode)yyVals[-2+yyTop]).body, false);
                        }
                        else {
                            yyVal = new RNWhile(thread, cond(((RNode)yyVals[0+yyTop])), ((RNode)yyVals[-2+yyTop]), true);
                        }
                    }
  break;
case 17:
//line 333 "parse.y"
  {
                        value_expr(((RNode)yyVals[0+yyTop]));
                        if (((RNode)yyVals[-2+yyTop]) != null && ((RNode)yyVals[-2+yyTop]) is RNBegin) {
                            yyVal = new RNUntil(thread, cond(((RNode)yyVals[0+yyTop])), ((RNode)yyVals[-2+yyTop]).body, false);
                        }
                        else {
                            yyVal = new RNUntil(thread, cond(((RNode)yyVals[0+yyTop])), ((RNode)yyVals[-2+yyTop]), true);
                        }
                    }
  break;
case 18:
//line 343 "parse.y"
  {
                        yyVal = new RNRescue(thread, ((RNode)yyVals[-2+yyTop]), new RNResBody(thread, null, ((RNode)yyVals[0+yyTop]), null), null);
                    }
  break;
case 19:
//line 347 "parse.y"
  {
                        if (in_def != 0 || in_single != 0) {
                            yyerror("BEGIN in method");
                        }
                        thread.LocalPush();
                    }
  break;
case 20:
//line 354 "parse.y"
  {
                        thread.evalTreeBegin = block_append(thread.evalTreeBegin,
                                                     new RNPreExe(thread, ((RNode)yyVals[-1+yyTop])));
                        thread.LocalPop();
                        yyVal = null;
                    }
  break;
case 21:
//line 361 "parse.y"
  {
                        if (compile_for_eval && (in_def != 0|| in_single != 0)) {
                            yyerror("END in method; use at_exit");
                        }

                        yyVal = new RNIter(thread, null, new RNPostExe(thread), ((RNode)yyVals[-1+yyTop]));
                    }
  break;
case 22:
//line 370 "parse.y"
  {
                        value_expr(((RNode)yyVals[0+yyTop]));
                        yyVal = node_assign(((RNode)yyVals[-2+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 23:
//line 375 "parse.y"
  {
                        value_expr(((RNode)yyVals[0+yyTop]));
                        ((RNode)yyVals[-2+yyTop]).val = ((RNode)yyVals[0+yyTop]);
                        yyVal = ((RNode)yyVals[-2+yyTop]);
                    }
  break;
case 24:
//line 381 "parse.y"
  {
                        yyVal = node_assign(((RNode)yyVals[-2+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 26:
//line 387 "parse.y"
  {
                        /* value_expr($3); // nakada ruby-dev:15905*/
                        ((RNode)yyVals[-2+yyTop]).val = ((RNode)yyVals[0+yyTop]);
                        yyVal = ((RNode)yyVals[-2+yyTop]);
                    }
  break;
case 27:
//line 393 "parse.y"
  {
                        if (!compile_for_eval && in_def != 0 && in_single != 0)
                            yyerror("return appeared outside of method");
                        yyVal = new RNReturn(thread, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 29:
//line 400 "parse.y"
  {
                        yyVal = logop(typeof(RNAnd), ((RNode)yyVals[-2+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 30:
//line 404 "parse.y"
  {
                        yyVal = logop(typeof(RNOr), ((RNode)yyVals[-2+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 31:
//line 408 "parse.y"
  {
                        value_expr(((RNode)yyVals[0+yyTop]));
                        yyVal = new RNNot(thread, cond(((RNode)yyVals[0+yyTop])));
                    }
  break;
case 32:
//line 413 "parse.y"
  {
                        yyVal = new RNNot(thread, cond(((RNode)yyVals[0+yyTop])));
                    }
  break;
case 37:
//line 423 "parse.y"
  {
                        value_expr(((RNode)yyVals[-3+yyTop]));
                        yyVal = new_call(((RNode)yyVals[-3+yyTop]), ((uint)yyVals[-1+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 38:
//line 428 "parse.y"
  {
                        value_expr(((RNode)yyVals[-3+yyTop]));
                        yyVal = new_call(((RNode)yyVals[-3+yyTop]), ((uint)yyVals[-1+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 39:
//line 434 "parse.y"
  {
                        yyVal = new_fcall(((uint)yyVals[-1+yyTop]), ((RNode)yyVals[0+yyTop]));
                        ((RNode)yyVal).FixPos(((RNode)yyVals[0+yyTop]));
                   }
  break;
case 40:
//line 439 "parse.y"
  {
                        value_expr(((RNode)yyVals[-3+yyTop]));
                        yyVal = new_call(((RNode)yyVals[-3+yyTop]), ((uint)yyVals[-1+yyTop]), ((RNode)yyVals[0+yyTop]));
                        ((RNode)yyVal).FixPos(((RNode)yyVals[-3+yyTop]));
                    }
  break;
case 41:
//line 445 "parse.y"
  {
                        value_expr(((RNode)yyVals[-3+yyTop]));
                        yyVal = new_call(((RNode)yyVals[-3+yyTop]), ((uint)yyVals[-1+yyTop]), ((RNode)yyVals[0+yyTop]));
                        ((RNode)yyVal).FixPos(((RNode)yyVals[-3+yyTop]));
                    }
  break;
case 42:
//line 451 "parse.y"
  {
                        if (!compile_for_eval && in_def == 0 && in_single == 0)
                            yyerror("super called outside of method");
                        yyVal = new_super(((RNode)yyVals[0+yyTop]));
                        ((RNode)yyVal).FixPos(((RNode)yyVals[0+yyTop]));
                    }
  break;
case 43:
//line 458 "parse.y"
  {
                        yyVal = new RNYield(thread, ((RNode)yyVals[0+yyTop]));
                        ((RNode)yyVal).FixPos(((RNode)yyVals[0+yyTop]));
                    }
  break;
case 45:
//line 465 "parse.y"
  {
                        yyVal = ((RNode)yyVals[-1+yyTop]);
                    }
  break;
case 47:
//line 471 "parse.y"
  {
                        yyVal = new RNMAsgn(thread, new RNArray(thread, ((RNode)yyVals[-1+yyTop])));
                    }
  break;
case 48:
//line 476 "parse.y"
  {
                        yyVal = new RNMAsgn(thread, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 49:
//line 480 "parse.y"
  {
                        yyVal = new RNMAsgn(thread, RNode.list_append(thread,((RNode)yyVals[-1+yyTop]),((RNode)yyVals[0+yyTop])));
                    }
  break;
case 50:
//line 484 "parse.y"
  {
                        yyVal = new RNMAsgn(thread, ((RNode)yyVals[-2+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 51:
//line 488 "parse.y"
  {
                        yyVal = new RNMAsgn(thread, ((RNode)yyVals[-1+yyTop]), -1);
                    }
  break;
case 52:
//line 492 "parse.y"
  {
                        yyVal = new RNMAsgn(thread, null, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 53:
//line 496 "parse.y"
  {
                        yyVal = new RNMAsgn(thread, null, -1);
                    }
  break;
case 55:
//line 502 "parse.y"
  {
                        yyVal = ((RNode)yyVals[-1+yyTop]);
                    }
  break;
case 56:
//line 507 "parse.y"
  {
                        yyVal = new RNArray(thread, ((RNode)yyVals[-1+yyTop]));
                    }
  break;
case 57:
//line 511 "parse.y"
  {
                        yyVal = RNode.list_append(thread, ((RNode)yyVals[-2+yyTop]), ((RNode)yyVals[-1+yyTop]));
                    }
  break;
case 58:
//line 516 "parse.y"
  {
                        yyVal = assignable(((uint)yyVals[0+yyTop]), null);
                    }
  break;
case 59:
//line 520 "parse.y"
  {
                        yyVal = aryset(((RNode)yyVals[-3+yyTop]), ((RNode)yyVals[-1+yyTop]));
                    }
  break;
case 60:
//line 524 "parse.y"
  {
                        yyVal = attrset(((RNode)yyVals[-2+yyTop]), ((uint)yyVals[0+yyTop]));
                    }
  break;
case 61:
//line 528 "parse.y"
  {
                        yyVal = attrset(((RNode)yyVals[-2+yyTop]), ((uint)yyVals[0+yyTop]));
                    }
  break;
case 62:
//line 532 "parse.y"
  {
                        yyVal = attrset(((RNode)yyVals[-2+yyTop]), ((uint)yyVals[0+yyTop]));
                    }
  break;
case 63:
//line 536 "parse.y"
  {
                        backref_error(((RNode)yyVals[0+yyTop]));
                        yyVal = null;
                    }
  break;
case 64:
//line 542 "parse.y"
  {
                        yyVal = assignable(((uint)yyVals[0+yyTop]), null);
                    }
  break;
case 65:
//line 546 "parse.y"
  {
                        yyVal = aryset(((RNode)yyVals[-3+yyTop]), ((RNode)yyVals[-1+yyTop]));
                    }
  break;
case 66:
//line 550 "parse.y"
  {
                        yyVal = attrset(((RNode)yyVals[-2+yyTop]), ((uint)yyVals[0+yyTop]));
                    }
  break;
case 67:
//line 554 "parse.y"
  {
                        yyVal = attrset(((RNode)yyVals[-2+yyTop]), ((uint)yyVals[0+yyTop]));
                    }
  break;
case 68:
//line 558 "parse.y"
  {
                        yyVal = attrset(((RNode)yyVals[-2+yyTop]), ((uint)yyVals[0+yyTop]));
                    }
  break;
case 69:
//line 562 "parse.y"
  {
                        backref_error(((RNode)yyVals[0+yyTop]));
                        yyVal = null;
                    }
  break;
case 70:
//line 568 "parse.y"
  {
                        yyerror("class/module name must be CONSTANT");
                    }
  break;
case 75:
//line 577 "parse.y"
  {
                        lex.State = EXPR.END;
                        if (((object)yyVals[0+yyTop]) is int)
                            yyVal = ((int)yyVals[0+yyTop]);
                        else
                            yyVal = ((char)yyVals[0+yyTop]);
                    }
  break;
case 76:
//line 585 "parse.y"
  {
                        lex.State = EXPR.END;
                        yyVal = ((uint)yyVals[0+yyTop]);
                    }
  break;
case 79:
//line 594 "parse.y"
  {
                        yyVal = new RNUndef(thread, ((uint)yyVals[0+yyTop]));
                    }
  break;
case 80:
//line 597 "parse.y"
  {lex.State = EXPR.FNAME;}
  break;
case 81:
//line 598 "parse.y"
  {
                        yyVal = block_append(((RNode)yyVals[-3+yyTop]), new RNUndef(thread, ((uint)yyVals[0+yyTop])));
                    }
  break;
case 82:
//line 602 "parse.y"
  { yyVal = '|'; }
  break;
case 83:
//line 603 "parse.y"
  { yyVal = '^'; }
  break;
case 84:
//line 604 "parse.y"
  { yyVal = '&'; }
  break;
case 85:
//line 605 "parse.y"
  { yyVal = Token.tCMP; }
  break;
case 86:
//line 606 "parse.y"
  { yyVal = Token.tEQ; }
  break;
case 87:
//line 607 "parse.y"
  { yyVal = Token.tEQQ; }
  break;
case 88:
//line 608 "parse.y"
  { yyVal = Token.tMATCH; }
  break;
case 89:
//line 609 "parse.y"
  { yyVal = '>'; }
  break;
case 90:
//line 610 "parse.y"
  { yyVal = Token.tGEQ; }
  break;
case 91:
//line 611 "parse.y"
  { yyVal = '<'; }
  break;
case 92:
//line 612 "parse.y"
  { yyVal = Token.tLEQ; }
  break;
case 93:
//line 613 "parse.y"
  { yyVal = Token.tLSHFT; }
  break;
case 94:
//line 614 "parse.y"
  { yyVal = Token.tRSHFT; }
  break;
case 95:
//line 615 "parse.y"
  { yyVal = '+'; }
  break;
case 96:
//line 616 "parse.y"
  { yyVal = '-'; }
  break;
case 97:
//line 617 "parse.y"
  { yyVal = '*'; }
  break;
case 98:
//line 618 "parse.y"
  { yyVal = '*'; }
  break;
case 99:
//line 619 "parse.y"
  { yyVal = '/'; }
  break;
case 100:
//line 620 "parse.y"
  { yyVal = '%'; }
  break;
case 101:
//line 621 "parse.y"
  { yyVal = Token.tPOW; }
  break;
case 102:
//line 622 "parse.y"
  { yyVal = '~'; }
  break;
case 103:
//line 623 "parse.y"
  { yyVal = Token.tUPLUS; }
  break;
case 104:
//line 624 "parse.y"
  { yyVal = Token.tUMINUS; }
  break;
case 105:
//line 625 "parse.y"
  { yyVal = Token.tAREF; }
  break;
case 106:
//line 626 "parse.y"
  { yyVal = Token.tASET; }
  break;
case 107:
//line 627 "parse.y"
  { yyVal = '`'; }
  break;
case 149:
//line 638 "parse.y"
  {
                        value_expr(((RNode)yyVals[0+yyTop]));
                        yyVal = node_assign(((RNode)yyVals[-2+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 150:
//line 642 "parse.y"
  {yyVal = assignable(((uint)yyVals[-1+yyTop]), null);}
  break;
case 151:
//line 643 "parse.y"
  {
                        uint id = (uint)((int)yyVals[-2+yyTop]);
                        if (((RNode)yyVals[-1+yyTop]) != null) {
                            if (id == Token.tOROP) {
                                ((RNode)yyVals[-1+yyTop]).val = ((RNode)yyVals[0+yyTop]);
                                yyVal = new RNOpAsgnOr(thread, gettable(((uint)yyVals[-3+yyTop])), ((RNode)yyVals[-1+yyTop]));
                                if (is_instance_id(((uint)yyVals[-3+yyTop]))) {
                                    ((RNode)yyVal).aid = ((uint)yyVals[-3+yyTop]);
                                }
                            }
                            else if (id == Token.tANDOP) {
                                ((RNode)yyVals[-1+yyTop]).val = ((RNode)yyVals[0+yyTop]);
                                yyVal = new RNOpAsgnAnd(thread, gettable(((uint)yyVals[-3+yyTop])), ((RNode)yyVals[-1+yyTop]));
                            }
                            else {
                                yyVal = ((RNode)yyVals[-1+yyTop]);
                                ((RNode)yyVal).val = call_op(gettable(((uint)yyVals[-3+yyTop])),id,1,((RNode)yyVals[0+yyTop]));
                            }
                            ((RNode)yyVal).FixPos(((RNode)yyVals[0+yyTop]));
                        }
                        else {
                            yyVal = null;
                        }
                    }
  break;
case 152:
//line 668 "parse.y"
  {
                        RNode args = new RNArray(thread, ((RNode)yyVals[0+yyTop]));

                        RNode tail = RNode.list_append(thread, ((RNode)yyVals[-3+yyTop]), new RNNil(thread));
                        RNode.list_concat(args, tail);
                        uint id = (uint)((int)yyVals[-1+yyTop]);
                        if (id == Token.tOROP) {
                            id = 0;
                        }
                        else if (id == Token.tANDOP) {
                            id = 1;
                        }
                        yyVal = new RNOpAsgn1(thread, ((RNode)yyVals[-5+yyTop]), id, args);
                        ((RNode)yyVal).FixPos(((RNode)yyVals[-5+yyTop]));
                    }
  break;
case 153:
//line 684 "parse.y"
  {
                        uint id = (uint)((int)yyVals[-1+yyTop]);
                        if (id == Token.tOROP) {
                            id = 0;
                        }
                        else if (id == Token.tANDOP) {
                            id = 1;
                        }
                        yyVal = new RNOpAsgn2(thread, ((RNode)yyVals[-4+yyTop]), ((uint)yyVals[-2+yyTop]), id, ((RNode)yyVals[0+yyTop]));
                        ((RNode)yyVal).FixPos(((RNode)yyVals[-4+yyTop]));
                    }
  break;
case 154:
//line 696 "parse.y"
  {
                        uint id = (uint)((int)yyVals[-1+yyTop]);
                        if (id == Token.tOROP) {
                            id = 0;
                        }
                        else if (id == Token.tANDOP) {
                            id = 1;
                        }
                        yyVal = new RNOpAsgn2(thread, ((RNode)yyVals[-4+yyTop]), ((uint)yyVals[-2+yyTop]), id, ((RNode)yyVals[0+yyTop]));
                        ((RNode)yyVal).FixPos(((RNode)yyVals[-4+yyTop]));
                    }
  break;
case 155:
//line 708 "parse.y"
  {
                        uint id = (uint)((int)yyVals[-1+yyTop]);
                        if (id == Token.tOROP) {
                            id = 0;
                        }
                        else if (id == Token.tANDOP) {
                            id = 1;
                        }
                        yyVal = new RNOpAsgn2(thread, ((RNode)yyVals[-4+yyTop]), ((uint)yyVals[-2+yyTop]), id, ((RNode)yyVals[0+yyTop]));
                        ((RNode)yyVal).FixPos(((RNode)yyVals[-4+yyTop]));
                    }
  break;
case 156:
//line 720 "parse.y"
  {
                        backref_error(((RNode)yyVals[-2+yyTop]));
                        yyVal = null;
                    }
  break;
case 157:
//line 725 "parse.y"
  {
                        yyVal = new RNDot2(thread, ((RNode)yyVals[-2+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 158:
//line 729 "parse.y"
  {
                        yyVal = new RNDot3(thread, ((RNode)yyVals[-2+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 159:
//line 733 "parse.y"
  {
                        yyVal = call_op(((RNode)yyVals[-2+yyTop]), '+', 1, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 160:
//line 737 "parse.y"
  {
                        yyVal = call_op(((RNode)yyVals[-2+yyTop]), '-', 1, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 161:
//line 741 "parse.y"
  {
                        yyVal = call_op(((RNode)yyVals[-2+yyTop]), '*', 1, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 162:
//line 745 "parse.y"
  {
                        yyVal = call_op(((RNode)yyVals[-2+yyTop]), '/', 1, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 163:
//line 749 "parse.y"
  {
                        yyVal = call_op(((RNode)yyVals[-2+yyTop]), '%', 1, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 164:
//line 753 "parse.y"
  {
/* TODO*/
/*                bool need_negate = false;*/

/* #if UMINUS*/
/*                if ($1 is RNLit) {*/
/*                    if ($1.lit is long ||*/
/*                     $1.lit is double ||*/
/*                     $1.lit is int /* ||*/
/*                     $1.lit is BIGNUM * /*/
/*                     )*/
/*                    {*/
/*                     if (RTEST(rb_funcall($1.lit,'<',1,0))) {*/
/*                         $1.lit = rb_funcall($1.lit,intern("-@"),0,0);*/
/*                         need_negate = true;*/
/*                     }*/
/*                    }*/
/*                }*/
/* #endif*/
                        yyVal = call_op(((RNode)yyVals[-2+yyTop]), Token.tPOW, 1, ((RNode)yyVals[0+yyTop]));
/*                if (need_negate) {*/
/*                    $$ = call_op($<RNode>$, Token.tUMINUS, 0, null);*/
/*                }*/
                    }
  break;
case 165:
//line 778 "parse.y"
  {
                        if (((RNode)yyVals[0+yyTop]) != null && ((RNode)yyVals[0+yyTop]) is RNLit) {
                            yyVal = ((RNode)yyVals[0+yyTop]);
                        }
                        else {
                            yyVal = call_op(((RNode)yyVals[0+yyTop]), Token.tUPLUS, 0, null);
                        }
                    }
  break;
case 166:
//line 787 "parse.y"
  {
                        if (((RNode)yyVals[0+yyTop]) != null && ((RNode)yyVals[0+yyTop]) is RNLit && (((RNode)yyVals[0+yyTop]).lit is int ||
                                                          ((RNode)yyVals[0+yyTop]).lit is long)) {
                            if (((RNode)yyVals[0+yyTop]).lit is int)
                            {
                                int i = (int)((RNode)yyVals[0+yyTop]).lit;
                                ((RNode)yyVals[0+yyTop]).lit = -i;
                            }
                            else
                            {
                                long i = (long)((RNode)yyVals[0+yyTop]).lit;
                                ((RNode)yyVals[0+yyTop]).lit = -i;
                            }
                            yyVal = ((RNode)yyVals[0+yyTop]);
                        }
                        else {
                            yyVal = call_op(((RNode)yyVals[0+yyTop]), Token.tUMINUS, 0, null);
                        }
                    }
  break;
case 167:
//line 807 "parse.y"
  {
                        yyVal = call_op(((RNode)yyVals[-2+yyTop]), '|', 1, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 168:
//line 811 "parse.y"
  {
                        yyVal = call_op(((RNode)yyVals[-2+yyTop]), '^', 1, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 169:
//line 815 "parse.y"
  {
                        yyVal = call_op(((RNode)yyVals[-2+yyTop]), '&', 1, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 170:
//line 819 "parse.y"
  {
                        yyVal = call_op(((RNode)yyVals[-2+yyTop]), Token.tCMP, 1, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 171:
//line 823 "parse.y"
  {
                        yyVal = call_op(((RNode)yyVals[-2+yyTop]), '>', 1, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 172:
//line 827 "parse.y"
  {
                        yyVal = call_op(((RNode)yyVals[-2+yyTop]), Token.tGEQ, 1, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 173:
//line 831 "parse.y"
  {
                        yyVal = call_op(((RNode)yyVals[-2+yyTop]), '<', 1, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 174:
//line 835 "parse.y"
  {
                        yyVal = call_op(((RNode)yyVals[-2+yyTop]), Token.tLEQ, 1, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 175:
//line 839 "parse.y"
  {
                        yyVal = call_op(((RNode)yyVals[-2+yyTop]), Token.tEQ, 1, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 176:
//line 843 "parse.y"
  {
                        yyVal = call_op(((RNode)yyVals[-2+yyTop]), Token.tEQQ, 1, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 177:
//line 847 "parse.y"
  {
                        yyVal = new RNNot(thread, call_op(((RNode)yyVals[-2+yyTop]), Token.tEQ, 1, ((RNode)yyVals[0+yyTop])));
                    }
  break;
case 178:
//line 851 "parse.y"
  {
                        yyVal = match_gen(((RNode)yyVals[-2+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 179:
//line 855 "parse.y"
  {
                        yyVal = new RNNot(thread, match_gen(((RNode)yyVals[-2+yyTop]), ((RNode)yyVals[0+yyTop])));
                    }
  break;
case 180:
//line 859 "parse.y"
  {
                        value_expr(((RNode)yyVals[0+yyTop]));
                        yyVal = new RNNot(thread, cond(((RNode)yyVals[0+yyTop])));
                    }
  break;
case 181:
//line 864 "parse.y"
  {
                        yyVal = call_op(((RNode)yyVals[0+yyTop]), '~', 0, null);
                    }
  break;
case 182:
//line 868 "parse.y"
  {
                        yyVal = call_op(((RNode)yyVals[-2+yyTop]), Token.tLSHFT, 1, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 183:
//line 872 "parse.y"
  {
                        yyVal = call_op(((RNode)yyVals[-2+yyTop]), Token.tRSHFT, 1, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 184:
//line 876 "parse.y"
  {
                        yyVal = logop(typeof(RNAnd), ((RNode)yyVals[-2+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 185:
//line 880 "parse.y"
  {
                        yyVal = logop(typeof(RNOr), ((RNode)yyVals[-2+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 186:
//line 883 "parse.y"
  {in_defined = true;}
  break;
case 187:
//line 884 "parse.y"
  {
                        in_defined = false;
                        yyVal = new RNDefined(thread, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 188:
//line 889 "parse.y"
  {
                        value_expr(((RNode)yyVals[-4+yyTop]));
                        yyVal = new RNIf(thread, cond(((RNode)yyVals[-4+yyTop])), ((RNode)yyVals[-2+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 189:
//line 894 "parse.y"
  {
                        yyVal = ((RNode)yyVals[0+yyTop]);
                    }
  break;
case 191:
//line 900 "parse.y"
  {
                        yyVal = new RNArray(thread, ((RNode)yyVals[-1+yyTop]));
                    }
  break;
case 192:
//line 904 "parse.y"
  {
                        yyVal = RNode.list_append(thread, ((RNode)yyVals[-3+yyTop]), ((RNode)yyVals[-1+yyTop]));
                    }
  break;
case 193:
//line 908 "parse.y"
  {
                        yyVal = ((RNode)yyVals[-1+yyTop]);
                    }
  break;
case 194:
//line 912 "parse.y"
  {
                        value_expr(((RNode)yyVals[-1+yyTop]));
                        yyVal = arg_concat(((RNode)yyVals[-4+yyTop]), ((RNode)yyVals[-1+yyTop]));
                    }
  break;
case 195:
//line 917 "parse.y"
  {
                        yyVal = new RNArray(thread, new RNHash(thread, ((RNode)yyVals[-1+yyTop])));
                    }
  break;
case 196:
//line 921 "parse.y"
  {
                        value_expr(((RNode)yyVals[-1+yyTop]));
                        yyVal = new RNRestArgs(thread, ((RNode)yyVals[-1+yyTop]));
                    }
  break;
case 197:
//line 927 "parse.y"
  {
                        yyVal = ((RNode)yyVals[-1+yyTop]);
                    }
  break;
case 198:
//line 931 "parse.y"
  {
                        yyVal = ((RNode)yyVals[-2+yyTop]);
                    }
  break;
case 199:
//line 935 "parse.y"
  {
                        yyVal = new RNArray(thread, ((RNode)yyVals[-2+yyTop]));
                    }
  break;
case 200:
//line 939 "parse.y"
  {
                        yyVal = RNode.list_append(thread, ((RNode)yyVals[-4+yyTop]), ((RNode)yyVals[-2+yyTop]));
                    }
  break;
case 203:
//line 947 "parse.y"
  {
                        yyVal = new RNArray(thread, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 204:
//line 951 "parse.y"
  {
                        yyVal = RNode.list_append(thread, ((RNode)yyVals[-2+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 205:
//line 955 "parse.y"
  {
                        yyVal = arg_blk_pass(((RNode)yyVals[-1+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 206:
//line 959 "parse.y"
  {
                        value_expr(((RNode)yyVals[-1+yyTop]));
                        yyVal = arg_concat(((RNode)yyVals[-4+yyTop]), ((RNode)yyVals[-1+yyTop]));
                        yyVal = arg_blk_pass(((RNode)yyVal), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 207:
//line 965 "parse.y"
  {
                        yyVal = new RNArray(thread, new RNHash(thread, ((RNode)yyVals[-1+yyTop])));
                        yyVal = arg_blk_pass(((RNode)yyVal), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 208:
//line 970 "parse.y"
  {
                        value_expr(((RNode)yyVals[-1+yyTop]));
                        yyVal = arg_concat(new RNArray(thread, new RNHash(thread, ((RNode)yyVals[-4+yyTop]))), ((RNode)yyVals[-1+yyTop]));
                        yyVal = arg_blk_pass(((RNode)yyVal), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 209:
//line 976 "parse.y"
  {
                        yyVal = RNode.list_append(thread, ((RNode)yyVals[-3+yyTop]), new RNHash(thread, ((RNode)yyVals[-1+yyTop])));
                        yyVal = arg_blk_pass(((RNode)yyVal), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 210:
//line 981 "parse.y"
  {
                        value_expr(((RNode)yyVals[-1+yyTop]));
                        yyVal = arg_concat(RNode.list_append(thread, ((RNode)yyVals[-6+yyTop]), new RNHash(thread, ((RNode)yyVals[-4+yyTop]))), ((RNode)yyVals[-1+yyTop]));
                        yyVal = arg_blk_pass(((RNode)yyVal), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 211:
//line 987 "parse.y"
  {
                        value_expr(((RNode)yyVals[-1+yyTop]));
                        yyVal = arg_blk_pass(new RNRestArgs(thread, ((RNode)yyVals[-1+yyTop])), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 213:
//line 993 "parse.y"
  {lex.CMDARG_PUSH();}
  break;
case 214:
//line 994 "parse.y"
  {
                        lex.CMDARG_POP();
                        yyVal = ((RNode)yyVals[0+yyTop]);
                    }
  break;
case 215:
//line 1000 "parse.y"
  {
                        value_expr(((RNode)yyVals[0+yyTop]));
                        yyVal = new RNBlockPass(thread, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 216:
//line 1006 "parse.y"
  {
                        yyVal = ((RNode)yyVals[0+yyTop]);
                    }
  break;
case 218:
//line 1012 "parse.y"
  {
                        value_expr(((RNode)yyVals[0+yyTop]));
                        yyVal = new RNArray(thread, ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 219:
//line 1017 "parse.y"
  {
                        value_expr(((RNode)yyVals[0+yyTop]));
                        yyVal = RNode.list_append(thread, ((RNode)yyVals[-2+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 220:
//line 1023 "parse.y"
  {
                        value_expr(((RNode)yyVals[0+yyTop]));
                        yyVal = ((RNode)yyVals[0+yyTop]);
                    }
  break;
case 222:
//line 1030 "parse.y"
  {
                        value_expr(((RNode)yyVals[0+yyTop]));
                        yyVal = RNode.list_append(thread, ((RNode)yyVals[-2+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 223:
//line 1035 "parse.y"
  {
                        value_expr(((RNode)yyVals[0+yyTop]));
                        yyVal = arg_concat(((RNode)yyVals[-3+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 224:
//line 1040 "parse.y"
  {
                        value_expr(((RNode)yyVals[0+yyTop]));
                        yyVal = ((RNode)yyVals[0+yyTop]);
                    }
  break;
case 225:
//line 1046 "parse.y"
  {
                        yyVal = ((RNode)yyVals[0+yyTop]);
                        if (((RNode)yyVals[0+yyTop]) != null) {
                            if (((RNode)yyVals[0+yyTop]) is RNArray && ((RNode)yyVals[0+yyTop]).next == null) {
                                yyVal = ((RNode)yyVals[0+yyTop]).head;
                            }
                            else if (((RNode)yyVals[0+yyTop]) is RNBlockPass) {
                                thread.CompileError("block argument should not be given");
                            }
                        }
                    }
  break;
case 226:
//line 1059 "parse.y"
  {
                        yyVal = new RNLit(thread, ((object)yyVals[0+yyTop]));
                    }
  break;
case 228:
//line 1064 "parse.y"
  {
                        yyVal = new RNXStr(thread, ruby, ((object)yyVals[0+yyTop]));
                    }
  break;
case 234:
//line 1073 "parse.y"
  {
                        yyVal = new RNVCall(thread, ((uint)yyVals[0+yyTop]));
                    }
  break;
case 235:
//line 1082 "parse.y"
  {
                        RNode nd = ((RNode)yyVals[-4+yyTop]);
                        if (((RNode)yyVals[-3+yyTop]) == null && ((RNode)yyVals[-2+yyTop]) == null && ((RNode)yyVals[-1+yyTop]) == null)
                            yyVal = new RNBegin(thread, ((RNode)yyVals[-4+yyTop]));
                        else {
                            if (((RNode)yyVals[-3+yyTop]) != null) nd = new RNRescue(thread, ((RNode)yyVals[-4+yyTop]), ((RNode)yyVals[-3+yyTop]), ((RNode)yyVals[-2+yyTop]));
                            else if (((RNode)yyVals[-2+yyTop]) != null) {
                                ruby.warn("else without rescue is useless");
                                nd = block_append(((RNode)yyVals[-4+yyTop]), ((RNode)yyVals[-2+yyTop]));
                            }
                            if (((RNode)yyVals[-1+yyTop]) != null) nd = new RNEnsure(thread, ((RNode)yyVals[-4+yyTop]), ((RNode)yyVals[-1+yyTop]));
                            yyVal = nd;
                        }
                        ((RNode)yyVal).FixPos(nd);
                    }
  break;
case 236:
//line 1098 "parse.y"
  {
                        yyVal = ((RNode)yyVals[-1+yyTop]);
                    }
  break;
case 237:
//line 1102 "parse.y"
  {
                        value_expr(((RNode)yyVals[-2+yyTop]));
                        yyVal = new RNColon2(thread, ((RNode)yyVals[-2+yyTop]), ((uint)yyVals[0+yyTop]));
                    }
  break;
case 238:
//line 1107 "parse.y"
  {
                        yyVal = new RNColon3(thread, ((uint)yyVals[0+yyTop]));
                    }
  break;
case 239:
//line 1111 "parse.y"
  {
                        value_expr(((RNode)yyVals[-3+yyTop]));
                        yyVal = new RNCall(thread, ((RNode)yyVals[-3+yyTop]), Token.tAREF, ((RNode)yyVals[-1+yyTop]));
                    }
  break;
case 240:
//line 1116 "parse.y"
  {
                        if (((RNode)yyVals[-1+yyTop]) == null)
                            yyVal = new RNZArray(thread); /* zero length array*/
                        else {
                            yyVal = ((RNode)yyVals[-1+yyTop]);
                        }
                    }
  break;
case 241:
//line 1124 "parse.y"
  {
                        yyVal = new RNHash(thread, ((RNode)yyVals[-1+yyTop]));
                    }
  break;
case 242:
//line 1128 "parse.y"
  {
                        if (!compile_for_eval && in_def == 0 && in_single == 0)
                            yyerror("return appeared outside of method");
                        value_expr(((RNode)yyVals[-1+yyTop]));
                        yyVal = new RNReturn(thread, ((RNode)yyVals[-1+yyTop]));
                    }
  break;
case 243:
//line 1135 "parse.y"
  {
                        if (!compile_for_eval && in_def == 0 && in_single == 0)
                            yyerror("return appeared outside of method");
                        yyVal = new RNReturn(thread);
                    }
  break;
case 244:
//line 1141 "parse.y"
  {
                        if (!compile_for_eval && in_def == 0 && in_single == 0)
                            yyerror("return appeared outside of method");
                        yyVal = new RNReturn(thread);
                    }
  break;
case 245:
//line 1147 "parse.y"
  {
                        value_expr(((RNode)yyVals[-1+yyTop]));
                        yyVal = new RNYield(thread, ((RNode)yyVals[-1+yyTop]));
                    }
  break;
case 246:
//line 1152 "parse.y"
  {
                        yyVal = new RNYield(thread);
                    }
  break;
case 247:
//line 1156 "parse.y"
  {
                        yyVal = new RNYield(thread);
                    }
  break;
case 248:
//line 1159 "parse.y"
  {in_defined = true;}
  break;
case 249:
//line 1160 "parse.y"
  {
                        in_defined = false;
                        yyVal = new RNDefined(thread, ((RNode)yyVals[-1+yyTop]));
                    }
  break;
case 250:
//line 1165 "parse.y"
  {
                        ((RNode)yyVals[0+yyTop]).iter = new RNFCall(thread, ((uint)yyVals[-1+yyTop]));
                        yyVal = ((RNode)yyVals[0+yyTop]);
                    }
  break;
case 252:
//line 1171 "parse.y"
  {
                        if (((RNode)yyVals[-1+yyTop]) != null && ((RNode)yyVals[-1+yyTop]) is RNBlockPass) {
                            thread.CompileError("both block arg and actual block given");
                        }
                        ((RNode)yyVals[0+yyTop]).iter = ((RNode)yyVals[-1+yyTop]);
                        yyVal = ((RNode)yyVals[0+yyTop]);
                        ((RNode)yyVal).FixPos(((RNode)yyVals[-1+yyTop]));
                    }
  break;
case 253:
//line 1183 "parse.y"
  {
                        value_expr(((RNode)yyVals[-4+yyTop]));
                        yyVal = new RNIf(thread, cond(((RNode)yyVals[-4+yyTop])), ((RNode)yyVals[-2+yyTop]), ((RNode)yyVals[-1+yyTop]));
                        ((RNode)yyVal).FixPos(((RNode)yyVals[-4+yyTop]));
                    }
  break;
case 254:
//line 1192 "parse.y"
  {
                        value_expr(((RNode)yyVals[-4+yyTop]));
                      yyVal = new RNUnless(thread, cond(((RNode)yyVals[-4+yyTop])), ((RNode)yyVals[-2+yyTop]), ((RNode)yyVals[-1+yyTop]));
                        ((RNode)yyVal).FixPos(((RNode)yyVals[-4+yyTop]));
                    }
  break;
case 255:
//line 1197 "parse.y"
  {lex.COND_PUSH();}
  break;
case 256:
//line 1197 "parse.y"
  {lex.COND_POP();}
  break;
case 257:
//line 1200 "parse.y"
  {
                        value_expr(((RNode)yyVals[-4+yyTop]));
                        yyVal = new RNWhile(thread, cond(((RNode)yyVals[-4+yyTop])), ((RNode)yyVals[-1+yyTop]), true);
                        ((RNode)yyVal).FixPos(((RNode)yyVals[-4+yyTop]));
                    }
  break;
case 258:
//line 1205 "parse.y"
  {lex.COND_PUSH();}
  break;
case 259:
//line 1205 "parse.y"
  {lex.COND_POP();}
  break;
case 260:
//line 1208 "parse.y"
  {
                        value_expr(((RNode)yyVals[-4+yyTop]));
                        yyVal = new RNUntil(thread, cond(((RNode)yyVals[-4+yyTop])), ((RNode)yyVals[-1+yyTop]), true);
                        ((RNode)yyVal).FixPos(((RNode)yyVals[-4+yyTop]));
                    }
  break;
case 261:
//line 1216 "parse.y"
  {
                        value_expr(((RNode)yyVals[-3+yyTop]));
                        yyVal = new RNCase(thread, ((RNode)yyVals[-3+yyTop]), ((RNode)yyVals[-1+yyTop]));
                        ((RNode)yyVal).FixPos(((RNode)yyVals[-3+yyTop]));
                    }
  break;
case 262:
//line 1222 "parse.y"
  {
                        yyVal = ((RNode)yyVals[-1+yyTop]);
                    }
  break;
case 263:
//line 1225 "parse.y"
  {lex.COND_PUSH();}
  break;
case 264:
//line 1225 "parse.y"
  {lex.COND_POP();}
  break;
case 265:
//line 1228 "parse.y"
  {
                        value_expr(((RNode)yyVals[-4+yyTop]));
                        yyVal = new RNFor(thread, ((RNode)yyVals[-7+yyTop]), ((RNode)yyVals[-4+yyTop]), ((RNode)yyVals[-1+yyTop]));
                    }
  break;
case 266:
//line 1233 "parse.y"
  {
                        if (in_def != 0|| in_single != 0)
                            yyerror("class definition in method body");
                        class_nest++;
                        thread.LocalPush();
                        yyVal = sourceline;
                    }
  break;
case 267:
//line 1242 "parse.y"
  {
                        yyVal = new RNClass(thread, ((uint)yyVals[-4+yyTop]), ((RNode)yyVals[-1+yyTop]), ((RNode)yyVals[-3+yyTop]));
                        ((RNode)yyVal).SetLine(((int)yyVals[-2+yyTop]));
                        thread.LocalPop();
                        class_nest--;
                    }
  break;
case 268:
//line 1249 "parse.y"
  {
                        yyVal = in_def;
                        in_def = 0;
                    }
  break;
case 269:
//line 1254 "parse.y"
  {
                        yyVal = in_single;
                        in_single = 0;
                        class_nest++;
                        thread.LocalPush();
                    }
  break;
case 270:
//line 1262 "parse.y"
  {
                        yyVal = new RNSClass(thread, ((RNode)yyVals[-5+yyTop]), ((RNode)yyVals[-1+yyTop]));
                        thread.LocalPop();
                        class_nest--;
                        in_def = ((int)yyVals[-4+yyTop]);
                        in_single = ((int)yyVals[-2+yyTop]);
                    }
  break;
case 271:
//line 1270 "parse.y"
  {
                        if (in_def != 0|| in_single != 0)
                            yyerror("module definition in method body");
                        class_nest++;
                        thread.LocalPush();
                        yyVal = sourceline;
                    }
  break;
case 272:
//line 1279 "parse.y"
  {
                        yyVal = new RNModule(thread, ((uint)yyVals[-3+yyTop]), ((RNode)yyVals[-1+yyTop]));
                        ((RNode)yyVal).SetLine(((int)yyVals[-2+yyTop]));
                        thread.LocalPop();
                        class_nest--;
                    }
  break;
case 273:
//line 1286 "parse.y"
  {
                        if (in_def != 0|| in_single != 0)
                            yyerror("nested method definition");
                        yyVal = cur_mid;
                        if (((object)yyVals[0+yyTop]) is uint)
                            cur_mid = ((uint)yyVals[0+yyTop]);
                        else
                            cur_mid = (uint)((int)yyVals[0+yyTop]);
                        in_def++;
                        thread.LocalPush();
                    }
  break;
case 274:
//line 1303 "parse.y"
  {
                        RNode nd = ((RNode)yyVals[-4+yyTop]);
                        if (((RNode)yyVals[-3+yyTop]) != null) nd = new RNRescue(thread, ((RNode)yyVals[-4+yyTop]), ((RNode)yyVals[-3+yyTop]), ((RNode)yyVals[-2+yyTop]));
                        else if (((RNode)yyVals[-2+yyTop]) != null) {
                            ruby.warn("else without rescue is useless");
                            nd = block_append(((RNode)yyVals[-4+yyTop]), ((RNode)yyVals[-2+yyTop]));
                        }
                        if (((RNode)yyVals[-1+yyTop]) != null) nd = new RNEnsure(thread, ((RNode)yyVals[-4+yyTop]), ((RNode)yyVals[-1+yyTop]));

                        /* NOEX_PRIVATE for toplevel */
                        uint id;
                        if (((object)yyVals[-7+yyTop]) is uint)
                        {
                            id = ((uint)yyVals[-7+yyTop]);
                        }
                        else
                        {
                            id = (uint)((int)yyVals[-7+yyTop]);
                        }
                        yyVal = new RNDefn(thread, id, ((RNode)yyVals[-5+yyTop]), nd, (class_nest > 0) ? NOEX.PUBLIC : NOEX.PRIVATE);
                        if (is_attrset_id(id)) ((RNode)yyVal).noex = NOEX.PUBLIC;
                        ((RNode)yyVal).FixPos(((RNode)yyVals[-5+yyTop]));
                        thread.LocalPop();
                        in_def--;
                        cur_mid = ((uint)yyVals[-6+yyTop]);
                    }
  break;
case 275:
//line 1329 "parse.y"
  {lex.State = EXPR.FNAME;}
  break;
case 276:
//line 1330 "parse.y"
  {
                        value_expr(((RNode)yyVals[-3+yyTop]));
                        in_single++;
                        thread.LocalPush();
                        lex.State = EXPR.END; /* force for args */
                    }
  break;
case 277:
//line 1342 "parse.y"
  {
                        RNode nd = ((RNode)yyVals[-4+yyTop]);
                        if (((RNode)yyVals[-3+yyTop]) != null) nd = new RNRescue(thread, ((RNode)yyVals[-4+yyTop]), ((RNode)yyVals[-3+yyTop]), ((RNode)yyVals[-2+yyTop]));
                        else if (((RNode)yyVals[-2+yyTop]) != null) {
                            ruby.warn("else without rescue is useless");
                            nd = block_append(((RNode)yyVals[-4+yyTop]), ((RNode)yyVals[-2+yyTop]));
                        }
                        if (((RNode)yyVals[-1+yyTop]) != null) nd = new RNEnsure(thread, ((RNode)yyVals[-4+yyTop]), ((RNode)yyVals[-1+yyTop]));

                        yyVal = new RNDefs(thread, ((RNode)yyVals[-10+yyTop]), ((uint)yyVals[-7+yyTop]), ((RNode)yyVals[-5+yyTop]), nd);
                        ((RNode)yyVal).FixPos(((RNode)yyVals[-10+yyTop]));
                        thread.LocalPop();
                        in_single--;
                    }
  break;
case 278:
//line 1357 "parse.y"
  {
                        yyVal = new RNBreak(thread);
                    }
  break;
case 279:
//line 1361 "parse.y"
  {
                        yyVal = new RNNext(thread);
                    }
  break;
case 280:
//line 1365 "parse.y"
  {
                        yyVal = new RNRedo(thread);
                    }
  break;
case 281:
//line 1369 "parse.y"
  {
                        yyVal = new RNRetry(thread);
                    }
  break;
case 288:
//line 1384 "parse.y"
  {
                        value_expr(((RNode)yyVals[-3+yyTop]));
                        yyVal = new RNIf(thread, cond(((RNode)yyVals[-3+yyTop])), ((RNode)yyVals[-1+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 290:
//line 1391 "parse.y"
  {
                        yyVal = ((RNode)yyVals[0+yyTop]);
                    }
  break;
case 294:
//line 1400 "parse.y"
  {
                        yyVal = new RNBlockNoArg(thread);
                    }
  break;
case 295:
//line 1404 "parse.y"
  {
                        yyVal = new RNBlockNoArg(thread);
                    }
  break;
case 296:
//line 1408 "parse.y"
  {
                        yyVal = ((RNode)yyVals[-1+yyTop]);
                    }
  break;
case 297:
//line 1414 "parse.y"
  {
                        yyVal = thread.DynaPush();
                    }
  break;
case 298:
//line 1420 "parse.y"
  {
                        yyVal = new RNIter(thread, ((RNode)yyVals[-2+yyTop]), null, ((RNode)yyVals[-1+yyTop]));
                        ((RNode)yyVal).FixPos((((RNode)yyVals[-2+yyTop]) != null) ? ((RNode)yyVals[-2+yyTop]) : ((RNode)yyVals[-1+yyTop]));
                        thread.DynaPop(((RVarmap)yyVals[-3+yyTop]));
                    }
  break;
case 299:
//line 1427 "parse.y"
  {
                        if (((RNode)yyVals[-1+yyTop]) != null && ((RNode)yyVals[-1+yyTop]) is RNBlockPass) {
                            thread.CompileError("both block arg and actual block given");
                        }
                        ((RNode)yyVals[0+yyTop]).iter = ((RNode)yyVals[-1+yyTop]);
                        yyVal = ((RNode)yyVals[0+yyTop]);
                        ((RNode)yyVal).FixPos(((RNode)yyVals[0+yyTop]));
                    }
  break;
case 300:
//line 1436 "parse.y"
  {
                        value_expr(((RNode)yyVals[-3+yyTop]));
                        yyVal = new_call(((RNode)yyVals[-3+yyTop]), ((uint)yyVals[-1+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 301:
//line 1441 "parse.y"
  {
                        value_expr(((RNode)yyVals[-3+yyTop]));
                        yyVal = new_call(((RNode)yyVals[-3+yyTop]), ((uint)yyVals[-1+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 302:
//line 1447 "parse.y"
  {
                        yyVal = new_fcall(((uint)yyVals[-1+yyTop]), ((RNode)yyVals[0+yyTop]));
                        ((RNode)yyVal).FixPos(((RNode)yyVals[0+yyTop]));
                    }
  break;
case 303:
//line 1452 "parse.y"
  {
                        value_expr(((RNode)yyVals[-3+yyTop]));
                        yyVal = new_call(((RNode)yyVals[-3+yyTop]), ((uint)yyVals[-1+yyTop]), ((RNode)yyVals[0+yyTop]));
                        ((RNode)yyVal).FixPos(((RNode)yyVals[-3+yyTop]));
                    }
  break;
case 304:
//line 1458 "parse.y"
  {
                        value_expr(((RNode)yyVals[-3+yyTop]));
                        yyVal = new_call(((RNode)yyVals[-3+yyTop]), ((uint)yyVals[-1+yyTop]), ((RNode)yyVals[0+yyTop]));
                        ((RNode)yyVal).FixPos(((RNode)yyVals[-3+yyTop]));
                    }
  break;
case 305:
//line 1464 "parse.y"
  {
                        value_expr(((RNode)yyVals[-2+yyTop]));
                        yyVal = new_call(((RNode)yyVals[-2+yyTop]), ((uint)yyVals[0+yyTop]), null);
                    }
  break;
case 306:
//line 1469 "parse.y"
  {
                        if (!compile_for_eval && in_def == 0 &&
                            in_single == 0 && !in_defined)
                            yyerror("super called outside of method");
                        yyVal = new_super(((RNode)yyVals[0+yyTop]));
                    }
  break;
case 307:
//line 1476 "parse.y"
  {
                        if (!compile_for_eval && in_def == 0 &&
                            in_single == 0 && !in_defined)
                            yyerror("super called outside of method");
                        yyVal = new RNZSuper(thread, ruby);
                    }
  break;
case 308:
//line 1484 "parse.y"
  {
                        yyVal = thread.DynaPush();
                    }
  break;
case 309:
//line 1489 "parse.y"
  {
                        yyVal = new RNIter(thread, ((RNode)yyVals[-2+yyTop]), null, ((RNode)yyVals[-1+yyTop]));
                        ((RNode)yyVal).FixPos(((RNode)yyVals[-1+yyTop]));
                        thread.DynaPop(((RVarmap)yyVals[-3+yyTop]));
                    }
  break;
case 310:
//line 1495 "parse.y"
  {
                        yyVal = thread.DynaPush();
                    }
  break;
case 311:
//line 1500 "parse.y"
  {
                        yyVal = new RNIter(thread, ((RNode)yyVals[-2+yyTop]), null, ((RNode)yyVals[-1+yyTop]));
                        ((RNode)yyVal).FixPos(((RNode)yyVals[-1+yyTop]));
                        thread.DynaPop(((RVarmap)yyVals[-3+yyTop]));
                    }
  break;
case 312:
//line 1509 "parse.y"
  {
                        yyVal = new RNWhen(thread, ((RNode)yyVals[-3+yyTop]), ((RNode)yyVals[-1+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 314:
//line 1515 "parse.y"
  {
                        value_expr(((RNode)yyVals[0+yyTop]));
                        yyVal = RNode.list_append(thread, ((RNode)yyVals[-3+yyTop]), new RNWhen(thread, ((RNode)yyVals[0+yyTop])));
                    }
  break;
case 315:
//line 1520 "parse.y"
  {
                        value_expr(((RNode)yyVals[0+yyTop]));
                        yyVal = new RNArray(thread, new RNWhen(thread, ((RNode)yyVals[0+yyTop])));
                    }
  break;
case 320:
//line 1532 "parse.y"
  {
                        yyVal = ((RNode)yyVals[0+yyTop]);
                    }
  break;
case 322:
//line 1540 "parse.y"
  {
                        RNode nd = ((RNode)yyVals[-3+yyTop]);
                        RNode nd2 = ((RNode)yyVals[-1+yyTop]);
                        if (nd != null) {
                            nd = node_assign(((RNode)yyVals[-3+yyTop]), new RNGVar(thread, ruby, intern("$!")));
                            nd2 = block_append(nd, ((RNode)yyVals[-1+yyTop]));
                        }
                        yyVal = new RNResBody(thread, ((RNode)yyVals[-4+yyTop]), nd2, ((RNode)yyVals[0+yyTop]));
                        ((RNode)yyVal).FixPos((((RNode)yyVals[-4+yyTop]) != null) ? ((RNode)yyVals[-4+yyTop]) : nd2);
                    }
  break;
case 325:
//line 1554 "parse.y"
  {
                        if (((RNode)yyVals[0+yyTop]) != null)
                            yyVal = ((RNode)yyVals[0+yyTop]);
                        else
                            /* place holder */
                            yyVal = new RNNil(thread);
                    }
  break;
case 327:
//line 1564 "parse.y"
  {
                        
                if (((object)yyVals[0+yyTop]) is uint)
                    yyVal = Symbol.ID2SYM(((uint)yyVals[0+yyTop]));
                else
                    yyVal = Symbol.ID2SYM(((char)yyVals[0+yyTop]));
                    }
  break;
case 329:
//line 1574 "parse.y"
  {
                        yyVal = new RNStr(thread, ruby, ((object)yyVals[0+yyTop]));
                    }
  break;
case 331:
//line 1579 "parse.y"
  {
                        string s = ((object)yyVals[0+yyTop]).ToString();
                        
                        if (((RNode)yyVals[-1+yyTop]) is RNDStr) {
                            RNode.list_append(thread, ((RNode)yyVals[-1+yyTop]), new RNStr(thread, ruby, s));
                        }
                        else {
#if STRCONCAT
                            rb_str_concat(((RNode)yyVals[-1+yyTop]).lit, ((object)yyVals[0+yyTop]));
#else
                            ((RNode)yyVals[-1+yyTop]).lit = new RString(ruby, (((RNode)yyVals[-1+yyTop]).lit).ToString() + s);
#endif                
                        }
                        yyVal = ((RNode)yyVals[-1+yyTop]);
                    }
  break;
case 332:
//line 1595 "parse.y"
  {
                        if (((RNode)yyVals[-1+yyTop]) is RNStr) {
                            yyVal = new RNDStr(thread, ruby, ((RNode)yyVals[-1+yyTop]).lit);
                        }
                        else {
                            yyVal = ((RNode)yyVals[-1+yyTop]);
                        }
                        /* Bugfix*/
                        /* $2.head = new RNStr(thread, ruby, $2.lit);*/
                        RNode.list_concat(((RNode)yyVal), new RNArray(thread, ((RNode)yyVals[0+yyTop])));
                    }
  break;
case 333:
//line 1608 "parse.y"
  {
                        lex.State = EXPR.END;
                        if (((object)yyVals[0+yyTop]) is uint)
                            yyVal = ((uint)yyVals[0+yyTop]);
                        else
                            yyVal = ((char)yyVals[0+yyTop]);
                    }
  break;
case 345:
//line 1629 "parse.y"
  {yyVal = (uint)Token.kNIL;}
  break;
case 346:
//line 1630 "parse.y"
  {yyVal = (uint)Token.kSELF;}
  break;
case 347:
//line 1631 "parse.y"
  {yyVal = (uint)Token.kTRUE;}
  break;
case 348:
//line 1632 "parse.y"
  {yyVal = (uint)Token.kFALSE;}
  break;
case 349:
//line 1633 "parse.y"
  {yyVal = (uint)Token.k__FILE__;}
  break;
case 350:
//line 1634 "parse.y"
  {yyVal = (uint)Token.k__LINE__;}
  break;
case 351:
//line 1637 "parse.y"
  {
                        yyVal = gettable(((uint)yyVals[0+yyTop]));
                    }
  break;
case 354:
//line 1645 "parse.y"
  {
                        yyVal = null;
                    }
  break;
case 355:
//line 1649 "parse.y"
  {
                        lex.State = EXPR.BEG;
                    }
  break;
case 356:
//line 1653 "parse.y"
  {
                        yyVal = ((RNode)yyVals[-1+yyTop]);
                    }
  break;
case 357:
//line 1656 "parse.y"
  {yyErrorFlag = 0; yyVal = null;}
  break;
case 358:
//line 1659 "parse.y"
  {
                        yyVal = ((RNode)yyVals[-2+yyTop]);
                        lex.State = EXPR.BEG;
                    }
  break;
case 359:
//line 1664 "parse.y"
  {
                        yyVal = ((RNode)yyVals[-1+yyTop]);
                    }
  break;
case 360:
//line 1669 "parse.y"
  {
                        yyVal = block_append(new RNArgs(thread, ((int)yyVals[-5+yyTop]), ((RNode)yyVals[-3+yyTop]), ((int)yyVals[-1+yyTop])), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 361:
//line 1673 "parse.y"
  {
                        yyVal = block_append(new RNArgs(thread, ((int)yyVals[-3+yyTop]), ((RNode)yyVals[-1+yyTop]), -1), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 362:
//line 1677 "parse.y"
  {
                        yyVal = block_append(new RNArgs(thread, ((int)yyVals[-3+yyTop]), null, ((int)yyVals[-1+yyTop])), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 363:
//line 1681 "parse.y"
  {
                        yyVal = block_append(new RNArgs(thread, ((int)yyVals[-1+yyTop]), null, -1), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 364:
//line 1685 "parse.y"
  {
                        yyVal = block_append(new RNArgs(thread, 0, ((RNode)yyVals[-3+yyTop]), ((uint)yyVals[-1+yyTop])), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 365:
//line 1689 "parse.y"
  {
                        yyVal = block_append(new RNArgs(thread, 0, ((RNode)yyVals[-1+yyTop]), -1), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 366:
//line 1693 "parse.y"
  {
                        yyVal = block_append(new RNArgs(thread, 0, null, ((int)yyVals[-1+yyTop])), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 367:
//line 1697 "parse.y"
  {
                        yyVal = block_append(new RNArgs(thread, 0, null, -1), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 368:
//line 1701 "parse.y"
  {
                        yyVal = new RNArgs(thread, 0, null, -1);
                    }
  break;
case 369:
//line 1706 "parse.y"
  {
                        yyerror("formal argument cannot be a constant");
                    }
  break;
case 370:
//line 1710 "parse.y"
  {
                        yyerror("formal argument cannot be an instance variable");
                    }
  break;
case 371:
//line 1714 "parse.y"
  {
                        yyerror("formal argument cannot be a global variable");
                    }
  break;
case 372:
//line 1718 "parse.y"
  {
                        yyerror("formal argument cannot be a class variable");
                    }
  break;
case 373:
//line 1722 "parse.y"
  {
                        if (!is_local_id(((uint)yyVals[0+yyTop])))
                            yyerror("formal argument must be local variable");
                        else if (thread.LocalID(((uint)yyVals[0+yyTop])))
                            yyerror("duplicate argument name");
                        thread.LocalCnt(((uint)yyVals[0+yyTop]));
                        yyVal = 1;
                    }
  break;
case 375:
//line 1733 "parse.y"
  {
                        yyVal = ((int)yyVal) + 1;
                    }
  break;
case 376:
//line 1738 "parse.y"
  {
                        if (!is_local_id(((uint)yyVals[-2+yyTop])))
                            yyerror("formal argument must be local variable");
                        else if (thread.LocalID(((uint)yyVals[-2+yyTop])))
                            yyerror("duplicate optional argument name");
                        yyVal = assignable(((uint)yyVals[-2+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 377:
//line 1747 "parse.y"
  {
                        yyVal = new RNBlock(thread, ((RNode)yyVals[0+yyTop]));
                        ((RNode)yyVal).end = ((RNode)yyVal);
                    }
  break;
case 378:
//line 1752 "parse.y"
  {
                        yyVal = block_append(((RNode)yyVals[-2+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 379:
//line 1757 "parse.y"
  {
                        if (!is_local_id(((uint)yyVals[0+yyTop])))
                            yyerror("rest argument must be local variable");
                        else if (thread.LocalID(((uint)yyVals[0+yyTop])))
                            yyerror("duplicate rest argument name");
                        yyVal = thread.LocalCnt(((uint)yyVals[0+yyTop]));
                    }
  break;
case 380:
//line 1765 "parse.y"
  {
                        yyVal = -2;
                    }
  break;
case 381:
//line 1770 "parse.y"
  {
                        if (!is_local_id(((uint)yyVals[0+yyTop])))
                            yyerror("block argument must be local variable");
                        else if (thread.LocalID(((uint)yyVals[0+yyTop])))
                            yyerror("duplicate block argument name");
                        yyVal = new RNBlockArg(thread, ((uint)yyVals[0+yyTop]), thread.LocalCnt(((uint)yyVals[0+yyTop])));
                    }
  break;
case 382:
//line 1779 "parse.y"
  {
                        yyVal = ((RNode)yyVals[0+yyTop]);
                    }
  break;
case 384:
//line 1785 "parse.y"
  {
                        if (((RNode)yyVals[0+yyTop]) is RNSelf) {
                            yyVal = new RNSelf(thread);
                        }
                        else {
                            yyVal = ((RNode)yyVals[0+yyTop]);
                        }
                    }
  break;
case 385:
//line 1793 "parse.y"
  {lex.State = EXPR.BEG;}
  break;
case 386:
//line 1794 "parse.y"
  {
                        if (((RNode)yyVals[-2+yyTop]) is RNStr ||
                            ((RNode)yyVals[-2+yyTop]) is RNDStr ||
                            ((RNode)yyVals[-2+yyTop]) is RNXStr ||
                            ((RNode)yyVals[-2+yyTop]) is RNDXStr ||
                            ((RNode)yyVals[-2+yyTop]) is RNDRegx ||
                            ((RNode)yyVals[-2+yyTop]) is RNLit ||
                            ((RNode)yyVals[-2+yyTop]) is RNArray ||
                            ((RNode)yyVals[-2+yyTop]) is RNZArray)
                        {
                            yyerror("can't define single method for literals.");
                        }
                        yyVal = ((RNode)yyVals[-2+yyTop]);
                    }
  break;
case 388:
//line 1811 "parse.y"
  {
                        yyVal = ((RNode)yyVals[-1+yyTop]);
                    }
  break;
case 389:
//line 1815 "parse.y"
  {
                        if (((RNode)yyVals[-1+yyTop]).alen % 2 != 0) {
                            yyerror("odd number list for Hash");
                        }
                        yyVal = ((RNode)yyVals[-1+yyTop]);
                    }
  break;
case 391:
//line 1824 "parse.y"
  {
                        yyVal = RNode.list_concat(((RNode)yyVals[-2+yyTop]), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 392:
//line 1829 "parse.y"
  {
                        yyVal = RNode.list_append(thread, new RNArray(thread, ((RNode)yyVals[-2+yyTop])), ((RNode)yyVals[0+yyTop]));
                    }
  break;
case 412:
//line 1859 "parse.y"
  {yyErrorFlag = 0;}
  break;
case 415:
//line 1863 "parse.y"
  {yyErrorFlag = 0;}
  break;
case 416:
//line 1866 "parse.y"
  {
                        yyVal = null;
                    }
  break;
//line default
        }
        yyTop -= yyLen[yyN];
        yyState = yyStates[yyTop];
        int yyM = yyLhs[yyN];
        if (yyState == 0 && yyM == 0) {
//t          if (yydebug != null) yydebug.shift(0, yyFinal);
          yyState = yyFinal;
          if (yyToken < 0) {
            yyToken = yyLex.advance() ? yyLex.token() : 0;
//t            if (yydebug != null)
//t               yydebug.lex(yyState, yyToken,yyname(yyToken), yyLex.value());
          }
          if (yyToken == 0) {
//t            if (yydebug != null) yydebug.accept(yyVal);
            return yyVal;
          }
          goto yyLoop;
        }
        if (((yyN = yyGindex[yyM]) != 0) && ((yyN += yyState) >= 0)
            && (yyN < yyTable.Length) && (yyCheck[yyN] == yyState))
          yyState = yyTable[yyN];
        else
          yyState = yyDgoto[yyM];
//t        if (yydebug != null) yydebug.shift(yyStates[yyTop], yyState);
	 goto yyLoop;
      }
    }
  }

   static  short [] yyLhs  = {              -1,
   74,    0,    5,    6,    6,    6,    6,   77,    7,    7,
    7,    7,    7,    7,    7,    7,    7,    7,   78,    7,
    7,    7,    7,    7,    7,    8,    8,    8,    8,    8,
    8,    8,    8,   12,   12,   37,   37,   37,   11,   11,
   11,   11,   11,   55,   55,   58,   58,   57,   57,   57,
   57,   57,   57,   59,   59,   56,   56,   60,   60,   60,
   60,   60,   60,   53,   53,   53,   53,   53,   53,   68,
   68,   69,   69,   69,   69,   69,   61,   61,   47,   80,
   47,   70,   70,   70,   70,   70,   70,   70,   70,   70,
   70,   70,   70,   70,   70,   70,   70,   70,   70,   70,
   70,   70,   70,   70,   70,   70,   70,   79,   79,   79,
   79,   79,   79,   79,   79,   79,   79,   79,   79,   79,
   79,   79,   79,   79,   79,   79,   79,   79,   79,   79,
   79,   79,   79,   79,   79,   79,   79,   79,   79,   79,
   79,   79,   79,   79,   79,   79,   79,   79,    9,   81,
    9,    9,    9,    9,    9,    9,    9,    9,    9,    9,
    9,    9,    9,    9,    9,    9,    9,    9,    9,    9,
    9,    9,    9,    9,    9,    9,    9,    9,    9,    9,
    9,    9,    9,    9,    9,   83,    9,    9,    9,   29,
   29,   29,   29,   29,   29,   29,   26,   26,   26,   26,
   27,   27,   25,   25,   25,   25,   25,   25,   25,   25,
   25,   25,   85,   28,   31,   30,   30,   22,   22,   33,
   33,   34,   34,   34,   23,   10,   10,   10,   10,   10,
   10,   10,   10,   10,   10,   10,   10,   10,   10,   10,
   10,   10,   10,   10,   10,   10,   10,   86,   10,   10,
   10,   10,   10,   10,   88,   90,   10,   91,   92,   10,
   10,   10,   93,   94,   10,   95,   10,   97,   98,   10,
   99,   10,  100,   10,  102,  103,   10,   10,   10,   10,
   10,   87,   87,   87,   89,   89,   14,   14,   15,   15,
   49,   49,   50,   50,   50,   50,  104,   52,   36,   36,
   36,   13,   13,   13,   13,   13,   13,  105,   51,  106,
   51,   16,   24,   24,   24,   17,   17,   19,   19,   20,
   20,   18,   18,   21,   21,    3,    3,    3,    2,    2,
    2,    2,   64,   63,   63,   63,   63,    4,    4,   62,
   62,   62,   62,   62,   62,   62,   62,   62,   62,   62,
   32,   48,   48,   35,  107,   35,   35,   38,   38,   39,
   39,   39,   39,   39,   39,   39,   39,   39,   72,   72,
   72,   72,   72,   73,   73,   41,   40,   40,   71,   71,
   42,   43,   43,    1,  108,    1,   44,   44,   44,   45,
   45,   46,   65,   65,   65,   66,   66,   66,   66,   67,
   67,   67,  101,  101,   75,   75,   82,   82,   84,   84,
   84,   96,   96,   76,   76,   54,
  };
   static  short [] yyLen = {           2,
    0,    2,    2,    1,    1,    3,    2,    0,    4,    3,
    3,    3,    2,    3,    3,    3,    3,    3,    0,    5,
    4,    3,    3,    3,    1,    3,    2,    1,    3,    3,
    2,    2,    1,    1,    1,    1,    4,    4,    2,    4,
    4,    2,    2,    1,    3,    1,    3,    1,    2,    3,
    2,    2,    1,    1,    3,    2,    3,    1,    4,    3,
    3,    3,    1,    1,    4,    3,    3,    3,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    0,
    4,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    3,    0,
    4,    6,    5,    5,    5,    3,    3,    3,    3,    3,
    3,    3,    3,    3,    2,    2,    3,    3,    3,    3,
    3,    3,    3,    3,    3,    3,    3,    3,    3,    2,
    2,    3,    3,    3,    3,    0,    4,    5,    1,    1,
    2,    4,    2,    5,    2,    3,    3,    4,    4,    6,
    1,    1,    1,    3,    2,    5,    2,    5,    4,    7,
    3,    1,    0,    2,    2,    2,    1,    1,    3,    1,
    1,    3,    4,    2,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    6,    3,    3,    2,    4,    3,
    3,    4,    3,    1,    4,    3,    1,    0,    6,    2,
    1,    2,    6,    6,    0,    0,    7,    0,    0,    7,
    5,    4,    0,    0,    9,    0,    6,    0,    0,    8,
    0,    5,    0,    9,    0,    0,   12,    1,    1,    1,
    1,    1,    1,    2,    1,    1,    1,    5,    1,    2,
    1,    1,    1,    2,    1,    3,    0,    5,    2,    4,
    4,    2,    4,    4,    3,    2,    1,    0,    5,    0,
    5,    5,    1,    4,    2,    1,    1,    1,    1,    2,
    1,    6,    1,    1,    2,    1,    1,    1,    1,    1,
    2,    2,    2,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    0,    4,    2,    4,    2,    6,
    4,    4,    2,    4,    2,    2,    1,    0,    1,    1,
    1,    1,    1,    1,    3,    3,    1,    3,    2,    1,
    2,    2,    1,    1,    0,    5,    1,    2,    2,    1,
    3,    3,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    0,    1,    0,    1,    0,    1,
    1,    1,    1,    1,    2,    0,
  };
   static  short [] yyDefRed = {            1,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  255,  258,    0,  278,  279,  280,  281,    0,    0,
    0,  346,  345,  347,  348,    0,    0,    0,   19,    0,
  350,  349,    0,    0,  342,  341,    0,  344,  338,  339,
  329,  228,  328,  330,  230,  231,  352,  353,  229,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  226,  326,    2,    0,    0,    0,    0,    0,    0,   28,
    0,  232,    0,   35,    0,    0,    4,    0,    0,   44,
    0,   54,    0,  327,    0,    0,   70,   71,    0,    0,
  271,  117,  129,  118,  142,  114,  135,  124,  123,  140,
  122,  121,  116,  145,  126,  115,  130,  134,  136,  128,
  120,  137,  147,  139,    0,    0,    0,    0,  113,  133,
  132,  127,  143,  146,  144,  148,  112,  119,  110,  111,
    0,    0,    0,   74,    0,  103,  104,  101,   85,   86,
   87,   90,   92,   88,  105,  106,   93,   94,   98,   89,
   91,   82,   83,   84,   95,   96,   97,   99,  100,  102,
  107,  385,    0,  384,  351,  273,   75,   76,  138,  131,
  141,  125,  108,  109,   72,   73,    0,   79,   78,   77,
    0,    0,    0,    0,    0,  413,  412,    0,    0,    0,
  414,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  291,  292,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  203,    0,   27,  225,  212,    0,  390,    0,    0,
    0,   43,    0,  306,   42,    0,   31,    0,    8,  408,
    0,    0,    0,    0,    0,    0,  238,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  190,    0,    0,    0,
  387,    0,    0,   52,    0,  336,  335,  337,  333,  334,
    0,   32,    0,  331,  332,    3,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  297,  299,  310,  308,  252,    0,    0,
    0,    0,    0,    0,    0,    0,   56,  150,  302,   39,
  250,    0,    0,  355,  266,  354,    0,    0,  404,  403,
  275,    0,   80,    0,    0,  323,  283,    0,    0,    0,
    0,    0,    0,    0,    0,  415,    0,    0,    0,    0,
    0,    0,  263,    0,    0,  243,    0,    0,    0,    0,
    0,    0,  205,  217,    0,  207,  246,    0,    0,    0,
    0,    0,    0,  214,   10,   12,   11,    0,  248,    0,
    0,    0,    0,    0,    0,  236,    0,    0,  191,    0,
  410,  193,  240,    0,  195,    0,  389,  241,  388,    0,
    0,    0,    0,    0,    0,    0,    0,   18,   29,   30,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  305,    0,    0,  398,    0,    0,  399,    0,    0,    0,
    0,  396,  397,    0,    0,    0,    0,    0,   22,    0,
   24,    0,   23,   26,  221,    0,   50,   57,    0,    0,
  357,    0,    0,    0,    0,    0,    0,  371,  370,  369,
  372,    0,    0,    0,    0,    0,    0,  377,  367,    0,
  374,    0,    0,    0,    0,    0,  318,    0,    0,  289,
    0,  284,    0,    0,    0,    0,    0,    0,  262,  286,
  256,  285,  259,    0,    0,    0,    0,    0,    0,    0,
    0,  211,  242,    0,    0,    0,    0,    0,    0,    0,
  204,  216,    0,    0,    0,  391,  245,    0,    0,    0,
    0,    0,  197,    9,    0,    0,    0,   21,    0,  196,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  304,
   41,    0,    0,  202,  303,   40,  201,    0,  295,    0,
    0,  293,    0,    0,  301,   38,  300,   37,    0,    0,
   55,    0,  269,    0,    0,  272,    0,  276,    0,  379,
  381,    0,    0,  359,    0,  365,  383,    0,  366,    0,
  363,   81,    0,    0,  321,    0,  290,    0,    0,  324,
    0,    0,  287,    0,  261,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  209,    0,    0,    0,  198,
    0,    0,  199,    0,   20,    0,  192,    0,    0,    0,
    0,    0,    0,  294,    0,    0,    0,    0,    0,    0,
    0,  356,  267,  386,    0,    0,    0,    0,    0,  378,
  382,    0,    0,    0,  375,    0,    0,  320,    0,    0,
  325,  235,    0,  253,  254,    0,    0,    0,    0,  264,
  206,    0,  208,    0,  249,  194,    0,  296,  298,  311,
  309,    0,    0,    0,  358,    0,  364,    0,  361,  362,
    0,    0,    0,    0,    0,    0,  316,  317,  312,  257,
  260,    0,    0,  200,  270,    0,    0,    0,    0,    0,
    0,    0,  322,    0,    0,  210,    0,  274,  360,    0,
  288,  265,    0,    0,  277,
  };
  protected static  short [] yyDgoto  = {             1,
  163,   60,   61,   62,  239,   64,   65,   66,   67,  235,
   69,   70,   71,  612,  613,  345,  709,  335,  495,  604,
  609,  244,  214,  508,  215,  564,  565,  225,  245,  363,
  216,   72,  464,  465,  325,   73,   74,  485,  486,  487,
  488,  661,  596,  249,  217,  218,  177,  219,  200,  571,
  321,  305,  183,   77,   78,   79,   80,  241,   81,   82,
  178,  220,  259,   84,  204,  515,  441,   90,  180,  447,
  490,  491,  492,    2,  189,  190,  378,  232,  168,  493,
  469,  231,  380,  392,  226,  545,  338,  192,  511,  619,
  193,  620,  520,  712,  473,  339,  470,  651,  327,  332,
  331,  476,  655,  449,  451,  450,  472,  328,
  };
  protected static  short [] yySindex = {            0,
    0,11518,11916,  229,  -56,14558,14285,11518,12414,12414,
11397,    0,    0,14447,    0,    0,    0,    0,12019,12113,
   15,    0,    0,    0,    0,12414,14192,   52,    0,    1,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,13793,
13793,  -56,11619,13005,13793,16170,14649,13887,13793,   90,
    0,    0,    0,  360,  538,  -29, 3209,  -21, -152,    0,
  -91,    0,  -44,    0, -185,  104,    0,  132,16077,    0,
  160,    0, -113,    0,  100,  538,    0,    0,12414,  471,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  -43,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  193,    0,    0,    0,
   12,  223,  228,  237,  223,    0,    0,  167,   37,  269,
    0,12414,12414,  344,  368,   15,   52,   17,    0,  185,
    0,    0,    0,  100,11518,13793,13793,13793,12217, 2335,
   29,    0,  522,    0,    0,    0,  526,    0, -185, -113,
12311,    0,12508,    0,    0,12508,    0,  246,    0,    0,
  442,  456,11518,  262,   47,  262,    0,11619,  545,    0,
  547,13793,   52,  140,  497,  171,    0,  176,  466,  171,
    0,   64,    0,    0,    0,    0,    0,    0,    0,    0,
  262,    0,  262,    0,    0,    0,11822,12414,12414,12414,
12414,11916,12414,12414,13793,13793,13793,13793,13793,13793,
13793,13793,13793,13793,13793,13793,13793,13793,13793,13793,
13793,13793,13793,13793,13793,13793,13793,13793,13793,13793,
14960,14997,13005,    0,    0,    0,    0,    0,15034,15034,
13793,13099,13099,11619,16170,  558,    0,    0,    0,    0,
    0,  -29,  360,    0,    0,    0,11518,12414,    0,    0,
    0,  274,    0,13793,  339,    0,    0,11518,  345,13793,
13202,11518,   37,13296,  350,    0,  175,  175,  442,15337,
15374,13005,    0, 2772, 3209,    0,  578,13793,15411,15448,
13005,12611,    0,    0,12705,    0,    0,  588, -152,  586,
   52,   69,  591,    0,    0,    0,    0,14285,    0,13793,
11518,  509,15411,15448,  595,    0,    0, 8032,    0,13399,
    0,    0,    0,13793,    0,13793,    0,    0,    0,15485,
15522,13005,  538,  -29,  -29,  -29,  -29,    0,    0,    0,
  262, 4854, 4854, 4854, 4854,  778,  778, 8697, 4518, 4854,
 4854, 3646, 3646,   -1,   -1, 4083,  778,  778,  202,  202,
  715,   54,   54,  262,  262,  262,  296,    0,    0,   15,
    0,    0,  297,    0,  299,   15,    0,  549,  -75,  -75,
  -75,    0,    0,   15,   15, 3209,13793, 3209,    0,  599,
    0, 3209,    0,    0,    0,  607,    0,    0,13793,  360,
    0,12414,11518,  387,   85,14922,  594,    0,    0,    0,
    0,  348,  354,  765,11518,  360,  622,    0,    0,  625,
    0,  632,14285, 3209,  341,  647,    0,11518,  432,    0,
  280,    0, 3209,  339,  433,13793,  656,   95,    0,    0,
    0,    0,    0,    0,   15,    0,    0,   15,  609,12414,
  361,    0,    0, 3209,  296,  297,  299,  624,13793, 2335,
    0,    0,  682,13793, 2335,    0,    0,12611,  693,15034,
15034,  698,    0,    0,12414, 3209,  617,    0,    0,    0,
13793, 3209,   52,    0,    0,    0,  654,13793,13793,    0,
    0,13793,13793,    0,    0,    0,    0,  407,    0,15984,
11518,    0,11518,11518,    0,    0,    0,    0, 3209,13493,
    0, 3209,    0,  167,  486,    0,  720,    0,13793,    0,
    0,   52,   12,    0, -205,    0,    0,  408,    0,  765,
    0,    0,16170,   95,    0,13793,    0,11518,  500,    0,
12414,  501,    0,  503,    0, 3209,13596,11518,11518,11518,
    0,  175,  407, 2772,12808,    0, 2772, -152,   69,    0,
   15,   15,    0,  123,    0, 8032,    0,    0, 3209, 3209,
 3209, 3209,13793,    0,  646,  511,  516,  648,13793, 3209,
11518,    0,    0,    0,  274, 3209,  736,  339,  594,    0,
    0,  625,  738,  625,    0,   67,    0,    0,    0,11518,
    0,    0,  223,    0,    0,13793,  203,  521,  523,    0,
    0,13793,    0,  745,    0,    0, 3209,    0,    0,    0,
    0, 3209,  524,11518,    0,  432,    0, -205,    0,    0,
15559,15862,13005,   12,11518, 3209,    0,    0,    0,    0,
    0,11518, 2772,    0,    0,   12,  527,  625,    0,    0,
    0,  697,    0,  280,  530,    0,  339,    0,    0,    0,
    0,    0,  432,  534,    0,
  };
  protected static  short [] yyRindex = {            0,
    0,  243,    0,    0,    0,    0,    0,  452,    0,    0,
  528,    0,    0,    0,    0,    0,    0,    0,11109, 6370,
 3516,    0,    0,    0,    0,    0,    0,13690,    0,    0,
    0,    0, 1753, 2737,    0,    0, 1855,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,   78,  708,  687,  214,    0,    0,    0, 5789,
    0,    0,    0, 1965, 1193, 5066,14376,11688, 8502,    0,
 5885,    0,10496,    0,10624,    0,    0,    0,  532,    0,
    0,    0,10709,    0,12902, 1598,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  247,  679,  959,  984,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
 1105, 1127, 1523,    0, 1649,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0, 5272,    0,    0,    0,
  241,    0,    0,    0,    0,    0,    0,  528,    0,  542,
    0,    0,    0, 6189, 6274, 4917,  774,    0,  513,    0,
    0,    0,  702,    0,   78,    0,    0,    0,    0,10925,
 6759,    0, 6008,    0,    0,    0, 6008,    0, 5310, 5400,
    0,    0,  777,    0,    0,    0,    0,    0,    0,    0,
13990,    0,   93, 6854, 6674, 7157,    0,   78,    0,   88,
    0,    0,  731,  743,    0,  743,    0,  704,    0,  704,
    0,    0, 1243,    0, 1479,    0,    0,    0,    0,    0,
 7242,    0, 7337,    0,    0,    0, 4888,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  708,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   78,  566,  570,    0,    0,    0,    0,
    0,  410,    0,    0,    0,    0,  458,    0,    0,    0,
    0,  450,    0,    6,  311,    0,    0,   35,10986,    0,
    0,  231,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  708,    0, 6008, 6493,    0,    0,    0,    0,    0,
  708,    0,    0,    0,    0,    0,    0,    0,  159,  222,
  786,  786,    0,    0,    0,    0,    0,    0,    0,    0,
   93,    0,    0,    0,    0,    0,  596,  731,    0,  747,
    0,    0,    0,  129,    0,  716,    0,    0,    0,    0,
    0,  708, 1700, 5900, 6385, 6810, 6975,    0,    0,    0,
 7640, 9102, 9187, 9275, 9367, 8719, 8824, 9452, 9717, 9540,
 9632, 9805, 9847, 1217, 8383,    0, 8909, 8997, 7941, 8631,
 8546, 8201, 8286, 7725, 7820, 8123, 4048, 3079, 3611,12902,
    0, 3174, 4390,    0, 4485, 3953,    0,    0,11194,11194,
11301,    0,    0, 4827, 4827, 1084,    0, 5160,    0,    0,
    0, 5028,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  458,    0,  786,    0,  115,    0,    0,    0,
    0,  634,    0,  266,  452,    0,   78,    0,    0,   78,
    0,   78,    0,   13,  157,   24,    0,   57,  582,    0,
  582,    0, 9902,  582,    0,    0,  158,    0,    0,    0,
    0,    0,    0,  136,    0,  905, 1414, 5221,    0,    0,
    0,    0,    0,14125, 2205, 2300, 2642,    0,    0,15943,
    0,    0, 6008,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0, 9987,    0,    0,  447,    0,
    0,   48,  731,  559,  756,  851,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,10805,    0,    0,
  458,    0,  458,   93,    0,    0,    0,    0,15704,    0,
    0,10117,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  786,  241,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  458,    0,    0,
    0,    0,    0,    0,    0,  225,    0,  416,  458,  458,
  995,    0, 5704, 6008,    0,    0, 6008,  275,  786,    0,
   63,   63,    0,    0,    0,  731,    0, 1513,10168,10231,
10276,10313,    0,    0,    0,    0,    0,    0,    0,10047,
  458,    0,    0,    0,  450,  722,    0,  311,    0,    0,
    0,   78,   78,   78,    0,    0,  101,    0,  107,  452,
    0,    0,    0,    0,    0,    0,  582,    0,    0,    0,
    0,    0,    0,    0,    0,    0,10382,    0,    0,    0,
    0,15794,    0,  452,    0,  582,    0,    0,    0,    0,
    0,    0,  708,  241,   35,  236,    0,    0,    0,    0,
    0,  458, 6008,    0,    0,  241,    0,   78,   72,  218,
  438,    0,    0,  582,    0,    0,  311,    0,    0,  192,
    0,    0,  582,    0,    0,
  };
  protected static  short [] yyGindex = {            0,
    0,    0,    0,    0,  286,    0,   99,  662, 1107,   -2,
  -15,   20,    0,  118, -316, -322,    0, -120,    0,    0,
 -612,  734,  426,    0,  337,    5,   80,   -4, -276, -206,
 -296,  842,    0,  540,    0, -195,    0,  206,  374,  265,
 -552, -272,  245,    0,  -17, -294,    0,  651,  292,  126,
  795,    0,  441, 1227,  161,    0,  -33, -115,  788,  -23,
  -14,  536,    0,    8,  125, -262,    0,  109,    4,   23,
 -536,  268,    0,    0,   22,  805,    0,    0,    0,    0,
    0, -175,    0,  276,    0,    0, -171,    0, -317,    0,
    0,    0,    0,    0,    0,   42,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,
  };
  protected static  short [] yyTable = {            68,
   68,  310,  330,  212,  212,   68,   68,   68,   68,  166,
  366,  198,  229,  342,  179,  416,  211,  211,  499,  240,
  505,  349,  218,   68,  302,  224,  448,  372,  167,  167,
  513,  307,  254,  319,  179,  300,  246,  250,  440,  446,
  298,  296,  660,  297,  416,  299,  454,  455,  570,  167,
   68,  211,  191,  252,  223,  211,  218,  219,  662,  489,
  260,  230,  351,  664,  416,  532,  416,  389,  532,  303,
  536,  218,  416,  243,  360,  519,  252,  262,  230,  167,
  320,   67,  319,  717,  528,  266,   68,  416,  518,  319,
  300,  219,  384,  416,  230,  298,  440,  446,  659,  536,
  299,   86,  416,  416,  186,  191,  219,  352,  416,  401,
   69,  396,  702,   91,  541,  416,   64,  400,  416,  361,
  734,  518,  385,  233,  373,  557,   85,   85,   46,  304,
   67,  326,   85,   85,   85,   85,  416,  361,  518,  223,
  219,  482,  483,   85,   85,  660,  233,  522,   44,  391,
   85,  416,  351,  187,  402,  373,  311,  703,  373,   69,
  237,  718,  400,  685,  312,   64,  416,  313,  203,  184,
  184,  184,  219,  373,  202,  396,  186,   85,   85,   61,
  391,  400,   85,  390,  186,  391,  184,  614,  306,   68,
   68,  233,  313,  212,  400,  539,  542,  351,  466,  203,
  224,   65,   68,  317,  240,  212,  211,  369,  319,  343,
  212,  489,  550,   85,  394,  416,  313,  416,  211,  396,
  211,  411,  307,  211,  532,  187,  400,   66,  318,  191,
   68,  416,  186,  187,  315,   68,  333,  239,  300,  295,
  416,  532,  416,  298,  296,  314,  297,   87,  299,  184,
   65,   88,  416,  411,   53,  569,  138,  396,  400,   67,
  273,  274,  416,  396,   68,   68,   68,   68,   68,   68,
   68,   68,  416,  334,   53,  368,   66,  631,  632,  218,
  240,  187,  239,  315,  204,  246,  138,   63,  340,  416,
  319,  467,  346,  181,  314,  309,  329,  341,  416,  587,
  211,  416,  416,  416,  680,  138,  368,  344,  396,  211,
  211,   68,  252,  484,  219,  204,   85,   85,  301,  416,
  416,  275,  243,  442,   68,   68,  626,  346,  532,   85,
  536,  459,  463,   85,  246,   68,  618,   53,   67,   68,
  396,  696,  629,  246,  533,   85,  531,   85,  416,  211,
   85,  400,  184,  184,  708,  218,  350,   85,  211,  211,
  707,  337,   85,  544,  471,  403,  319,   69,  359,  186,
  408,  243,  442,   64,  273,  274,  275,  637,   68,  306,
  243,  442,  489,  209,  246,  179,  383,  211,  512,  512,
  219,   85,   85,   85,   85,   85,   85,   85,   85,  211,
  167,  264,  416,  400,  265,  442,  701,  221,  540,  553,
  733,  400,  273,  274,   67,  400,  657,  681,  187,  268,
  683,  243,  442,  416,  313,  416,  722,   85,  184,  184,
  184,  184,  670,  184,  184,  561,   85,   85,   85,  518,
  233,  566,   76,   76,  560,  222,  351,   68,   76,  576,
  578,   85,   85,  684,  201,  510,  273,  274,   65,  368,
  686,  416,   85,  353,  273,  274,   85,  416,  268,   68,
   68,  498,  658,  344,  416,  400,   85,  397,  602,  588,
  186,  379,   68,  397,   66,   85,   85,   47,  184,  337,
   55,  315,   53,   76,  416,   68,   68,  396,  167,  416,
  179,  705,  314,  416,  416,   85,  726,   45,  368,  416,
  416,  583,  273,  274,   85,  167,  416,   68,  382,  560,
  533,  395,  628,  397,  275,  399,   85,  594,  397,  187,
  324,  239,   87,  575,  577,  211,   88,   83,   83,  288,
  289,  165,   68,   83,   83,   83,   83,  611,  498,  203,
  138,  375,  138,  138,  138,  138,   63,  396,  233,  371,
  397,   83,  374,  376,  377,  362,   89,  198,   68,  365,
   68,   68,   48,  416,  416,  573,  574,  477,  381,  478,
  479,  480,  481,  723,  275,  386,  346,  387,   83,  393,
  398,  255,   48,  138,  138,  727,   85,   85,  396,   61,
  666,  468,   61,  233,  400,   68,   51,  498,   68,   85,
   49,  502,  474,  509,  255,   68,   68,   68,  523,   61,
  482,  483,   85,  501,   83,  652,   51,  504,  537,  538,
   49,  543,  184,  548,  357,  549,   69,  559,  562,   55,
  563,  568,  580,  380,   85,   76,  368,  581,   68,  400,
  586,  590,   75,   75,  589,   48,   45,  591,   75,   75,
   75,   75,   85,  512,  199,  595,  547,   68,  598,   85,
  182,  185,  188,   76,  380,  600,   75,  380,   76,  416,
  184,  400,   61,  603,  416,  246,  416,  227,  131,   51,
  606,   68,  380,   49,  608,   85,  615,   85,   85,  617,
  211,  621,   68,   75,   68,  184,  253,   76,  207,   68,
  575,  577,   76,  416,  416,  416,  623,  397,  131,   45,
  416,  416,  243,  442,  345,  625,  323,   83,   83,  253,
  202,  376,   85,  630,  599,   85,  601,  131,  633,   75,
   83,  635,   85,   85,   85,   58,  638,  351,  643,  653,
  322,  300,  213,  213,   76,  483,  298,  296,  585,  297,
  654,  299,  376,  672,  674,  376,  675,   76,   83,  688,
  593,  184,  691,   83,  689,   85,  695,  397,   76,  690,
  376,  698,   76,  607,  710,  714,  711,  715,  248,  730,
  728,   69,  351,  732,   85,  396,   60,  735,  405,   60,
  416,  396,   83,   83,   83,   83,   83,   83,   83,   83,
   48,  416,  406,  407,  300,  295,   60,  416,   85,  298,
  296,   76,  297,  407,  299,   64,  407,   85,  409,   85,
  268,  269,  270,  271,  272,  409,   85,   61,  400,  411,
  411,  731,   75,   75,   51,  416,  396,  164,   49,   83,
  255,  461,  233,  347,  348,   75,  646,  592,  647,  648,
  694,  645,   83,   83,  663,  308,  316,  665,  267,    0,
    0,  294,    0,   83,   45,    0,    0,   83,  396,   60,
    0,    0,    0,   75,    0,    0,    0,    0,   75,    0,
  397,   62,    0,  671,   62,    0,  397,    0,  400,    0,
    0,  293,    0,  677,  678,  679,  697,  699,  700,    0,
    0,   62,    0,   76,    0,    0,   83,   75,   75,   75,
   75,   75,   75,   75,   75,   76,    0,    0,    0,  404,
  405,  406,  407,    0,  409,  410,  693,    0,   76,    0,
    0,  397,  213,    0,  396,    0,    0,    0,   60,    0,
  396,    0,    0,    0,  213,  704,  370,    0,    0,  213,
    0,    0,  729,    0,   75,  253,    0,    0,  141,    0,
    0,    0,    0,  397,   62,    0,    0,   75,   75,  716,
   64,    0,  131,    0,  131,  131,  131,  131,   75,  475,
  724,    0,   75,  125,    0,  396,    0,  725,  141,    0,
    0,    0,    0,    0,  347,    0,    0,   83,   83,    0,
  201,   76,    0,   76,   76,    0,    0,  141,  345,    0,
   83,    0,    0,  125,    0,  131,  131,  396,   66,  348,
    0,   75,    0,   83,   60,  396,    0,  275,   59,    0,
  239,  351,  125,  668,    0,  460,  460,    0,   76,    0,
    0,    0,  288,  289,    0,   83,    0,    0,   76,   76,
   76,    0,    0,    0,    0,    0,    0,  496,  477,    0,
  478,  479,  480,  481,  460,    0,    0,  507,    0,    0,
   83,    0,    0,  156,    0,  239,    0,    0,    0,    0,
    0,   76,    0,  156,    0,  396,    0,    0,    0,    0,
  275,    0,    0,    0,    0,  203,   83,    0,   83,   83,
   76,  482,  483,    0,  108,  288,  289,    0,   65,    0,
    0,    0,   75,   75,  156,  210,  210,  156,    0,   62,
  397,    0,    0,  584,   76,   75,  109,    0,  669,    0,
    0,  156,  156,   83,  108,   76,   83,    0,   75,    0,
  350,    0,   76,   83,   83,   83,  234,  236,    0,    0,
  210,  210,    0,  108,  261,  263,  109,    0,    0,    0,
   75,    0,  349,    0,    0,    0,  156,    0,    0,    0,
    0,  622,    0,   66,  396,  109,   83,    0,    0,    0,
  397,    0,    5,    0,    0,   75,    0,    0,    0,    0,
    0,    0,    5,    0,    0,   83,  634,    0,  156,    0,
    0,    0,    0,    0,    0,    0,  182,    0,    0,    0,
  199,   75,    0,   75,   75,    0,  182,    0,    0,   83,
    0,    0,    0,    5,    0,    0,    0,    0,    0,    0,
   83,    0,    0,    0,  396,    0,    0,   83,    0,    0,
    0,    5,    0,  667,  182,    0,    0,  182,   75,    0,
  182,   75,  141,    0,  141,  141,  141,  141,   75,   75,
   75,    0,  673,   65,  182,  182,  182,    0,  182,  182,
  247,  251,    0,   63,    0,    0,   63,  125,  233,  125,
  125,  125,  125,    0,    0,    0,    0,    0,  347,    0,
    0,   75,    0,   63,    0,  141,  141,    0,    0,  182,
  182,    0,  354,  355,  261,  210,    0,    5,    0,    0,
   75,    0,    0,  348,    0,    0,    0,  210,    0,  210,
  125,  125,  210,  233,  239,    0,    0,    0,    0,    0,
  182,  182,    0,    0,   75,  156,  156,  156,  388,    0,
  156,  156,  156,    0,  156,   75,    0,    0,    0,    0,
    0,    0,   75,    0,  156,  156,   63,    0,    0,    0,
    0,    0,    0,  156,  156,    0,  156,  156,  156,  156,
  156,  411,  412,  413,  414,  415,  416,  417,  418,  419,
  420,  421,  422,  423,  424,  425,  426,  427,  428,  429,
  430,  431,  432,  433,  434,  435,  436,  336,  108,  210,
  108,  108,  108,  108,    0,    0,    0,  456,  458,  462,
    0,    0,    0,    0,    0,    0,  156,    0,    0,    0,
  109,    0,  109,  109,  109,  109,    0,    0,    0,  364,
  494,    0,    0,  364,  350,    0,  503,  462,    0,  373,
  494,  108,  108,  397,    5,    5,    5,   62,  210,  397,
    5,    5,    0,    5,  524,    0,  349,  210,  530,    0,
    0,  535,    0,  109,  109,    0,    0,    0,  182,  182,
  182,    0,    0,  182,  182,  182,  546,  182,    0,    0,
    0,    0,    0,    0,    0,    0,  552,  182,  182,    0,
  535,    0,  552,    0,  397,    0,  182,  182,  210,  182,
  182,  182,  182,  182,    0,    0,    0,    0,    0,   58,
    0,   63,   58,    0,  351,    0,    0,    0,    0,  247,
    0,    0,   72,    0,    0,    0,  397,   68,    0,   58,
  182,  182,  182,  182,  182,  182,  182,  182,  182,  182,
  182,  182,    0,   59,  182,  182,   59,    0,  239,  182,
  497,  500,   72,  579,    0,    0,    0,    0,  340,  351,
    0,    0,    0,   59,    0,  582,    0,    0,  247,    0,
  364,   72,  233,    0,    0,    0,    0,  247,    0,    0,
    0,    0,    0,    0,    0,    0,  364,    7,    0,    0,
    0,    0,   58,  239,    0,    0,    0,    7,    0,    0,
    0,    0,  616,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  247,    0,
    0,    0,    0,    0,    0,  624,   59,    0,    7,    0,
  627,    0,    0,    0,  530,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    7,  636,   73,    0,
    0,    0,    0,    0,  639,  640,    0,    0,  641,  642,
    0,    0,  567,    0,    0,  572,  572,  572,    0,    0,
  567,  567,    0,    0,    0,    0,  650,    0,   73,    0,
    0,    0,   68,  397,  343,  656,    0,    0,    0,    6,
    0,    0,    0,    0,    0,    0,    0,   73,    0,    6,
    0,    0,  552,  597,    0,    0,  597,    0,  597,    0,
    0,  605,    7,  552,    0,  610,    0,  500,    0,    0,
  500,  535,    0,    0,    0,    0,    0,    0,    0,    0,
    6,    0,    0,    0,  567,    0,    0,    0,    0,  687,
    0,    0,  340,  397,    0,  692,    0,   58,    6,  364,
    0,    0,  340,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  706,    0,    0,  393,    0,    0,  713,  340,
  340,   59,  393,  340,  340,  340,  340,  340,  340,  340,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  210,
  340,  340,  340,  340,  340,  340,    0,    0,  351,  336,
    0,    0,    0,    0,    6,    0,   72,    0,   72,   72,
   72,   72,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  340,    0,  340,  340,    0,    0,    0,
  364,    0,  239,  364,  343,    0,    0,  567,  567,    7,
    7,    7,  340,    0,  343,    7,    7,    0,    7,   72,
   72,    0,    0,    0,    0,  393,  340,  340,  393,    0,
    0,    0,    0,    0,  500,    0,    0,  394,  597,  597,
  597,  343,  343,    0,  394,  343,  343,  343,  343,  343,
  343,  343,    0,  500,    0,    0,    0,    0,    0,    0,
    0,    0,  343,  343,  343,  343,  343,  343,    0,    0,
    0,    0,  610,    0,    0,    0,    0,    0,    0,  247,
  336,    0,    0,    0,    0,    0,    0,    0,    0,  364,
    0,    0,  336,    0,  597,  343,    0,  343,  343,    0,
  500,    0,   73,  500,   73,   73,   73,   73,    0,  610,
    0,    6,    6,    6,  405,    0,    0,    6,    6,    0,
    6,    0,    0,    0,    0,    0,    0,  394,  343,  343,
  394,    0,    0,    0,    0,    0,    0,    0,  343,    0,
    0,    0,    0,    0,    0,   73,   73,    0,    0,    0,
    0,    0,    0,    0,    0,  405,    0,    0,    0,  393,
  393,  393,    0,  393,  340,  340,  340,  393,  393,  340,
  340,  340,  393,  340,  393,  393,  393,  393,  393,  393,
  393,  340,  393,  340,  340,  393,  393,  393,  393,  393,
  393,  393,  340,  340,    0,  340,  340,  340,  340,  340,
    0,  393,    0,    0,  393,  393,  393,  393,  393,  393,
  393,  393,  393,  393,  393,  393,  393,  393,  393,  393,
  393,  393,  393,  393,  393,  340,  340,  340,  340,  340,
  340,  340,  340,  340,  340,  340,  340,  340,    0,  405,
  340,  340,  340,  393,  340,  340,  393,  393,  393,  393,
  393,  393,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  394,  394,  394,    0,  394,  343,  343,  343,  394,
  394,  343,  343,  343,  394,  343,  394,  394,  394,  394,
  394,  394,  394,  343,  394,  343,  343,  394,  394,  394,
  394,  394,  394,  394,  343,  343,    0,  343,  343,  343,
  343,  343,    0,  394,    0,    0,  394,  394,  394,  394,
  394,  394,  394,  394,  394,  394,  394,  394,  394,  394,
  394,  394,  394,  394,  394,  394,  394,  343,  343,  343,
  343,  343,  343,  343,  343,  343,  343,  343,  343,  343,
    0,    0,  343,  343,  343,  394,  343,  343,  394,  394,
  394,  394,  394,  394,  400,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  400,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  405,  405,  405,    0,
    0,    0,  405,  405,    0,  405,    0,  396,    0,    0,
    0,  400,  400,    0,  396,  400,  400,  400,  400,  400,
  400,  400,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  400,  400,  400,   67,  400,  400,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  400,    0,  400,  400,  396,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  396,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  400,  400,  400,
  396,    0,  396,    0,    0,    0,  396,  396,    0,  396,
  396,  396,  396,  396,  396,  396,  396,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  396,  396,  396,
   66,  396,  396,    0,    0,    0,    0,    0,    0,    0,
    0,  300,  295,    0,    0,    0,  298,  296,    0,  297,
    0,  299,    0,    0,    0,    0,    0,    0,    0,    0,
  396,    0,  396,  396,  292,    0,  291,  290,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  396,  396,  396,  396,    0,    0,  294,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  293,    0,
    0,  396,  396,  396,    0,  396,  400,  400,  400,  396,
  396,  400,  400,  400,  396,  400,  396,  396,  396,  396,
  396,  396,  396,    0,  400,  400,  400,  396,  396,  396,
  396,  396,  396,  396,  400,  400,    0,  400,  400,  400,
  400,  400,    0,  396,    0,    0,  396,  396,  396,  396,
  396,  396,  396,  396,  396,  396,  396,  396,  396,  396,
  396,  396,  396,  396,  396,  396,  396,  400,  400,  400,
  400,  400,  400,  400,  400,  400,  400,  400,  400,  400,
    0,    0,  400,  400,  400,  396,    0,  400,  396,  396,
  396,  396,  396,  396,    0,    0,  396,  396,  396,    0,
  396,  396,  396,  396,  396,  396,  396,  396,  396,  396,
  396,  396,  396,  396,  396,  396,  396,  396,    0,  396,
  396,  396,  396,  396,  396,  396,  396,  396,  396,  396,
  396,    0,  396,  396,  396,  396,  396,    0,  396,    0,
    0,  396,  396,  396,  396,  396,  396,  396,  396,  396,
  396,  396,  396,  396,  396,  396,  396,  396,  396,  396,
  396,  396,  396,  396,  396,  396,  396,  396,  396,  396,
  396,  396,  396,  396,  396,    0,    0,  396,  396,  396,
  396,  397,  396,  396,  396,  396,  396,  396,  396,    0,
    0,  397,    0,    0,    0,    0,    0,  275,  276,  277,
  278,  279,  280,  281,  282,  283,  284,  285,  286,  287,
    0,    0,  288,  289,  397,    0,    0,  358,  397,  397,
    0,  397,  397,  397,  397,  397,  397,  397,  397,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  397,
  397,  397,   68,  397,  397,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  397,    0,  397,  397,  234,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  234,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  397,  397,  397,  397,    0,  395,
    0,    0,    0,  234,  234,    0,  395,  234,  234,  234,
  234,  234,  234,  234,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  234,  234,  234,    0,  234,  234,
    0,    0,    0,    0,    0,    0,    0,    0,  300,  295,
    0,    0,    0,  298,  296,  521,  297,    0,  299,    0,
    0,    0,    0,    0,    0,    0,    0,  234,    0,  234,
  234,  292,    0,  291,  290,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  395,
  234,  234,  395,    0,    0,  294,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  293,    0,    0,  397,  397,
  397,    0,  397,  397,  397,  397,  397,  397,  397,  397,
  397,  397,  397,  397,  397,  397,  397,  397,  397,  397,
    0,  397,  397,  397,  397,  397,  397,  397,  397,  397,
  397,  397,  397,    0,  397,  397,  397,  397,  397,    0,
  397,    0,    0,  397,  397,  397,  397,  397,  397,  397,
  397,  397,  397,  397,  397,  397,  397,  397,  397,  397,
  397,  397,  397,  397,  397,  397,  397,  397,  397,  397,
  397,  397,  397,  397,  397,  397,  397,    0,    0,  397,
  397,  397,  397,    0,  397,  397,  397,  397,  397,  397,
  397,    0,    0,  395,  395,  395,    0,  395,  234,  234,
  234,  395,  395,  234,  234,  234,  395,  234,  395,  395,
  395,  395,  395,  395,  395,    0,  395,  234,  234,  395,
  395,  395,  395,  395,  395,  395,  234,  234,    0,  234,
  234,  234,  234,  234,    0,  395,    0,    0,  395,  395,
  395,  395,  395,  395,  395,  395,  395,  395,  395,  395,
  395,  395,  395,  395,  395,  395,  395,  395,  395,  234,
  234,  234,  234,  234,  234,  234,  234,  234,  234,  234,
  234,  234,    0,    0,  234,  234,  234,  395,  401,  234,
  395,  395,  395,  395,  395,  395,    0,    0,  401,    0,
    0,    0,    0,    0,  275,  276,  277,  278,  279,  280,
  281,  282,  283,  284,  285,  286,  287,    0,    0,  288,
  289,  398,    0,    0,    0,  401,  401,    0,  398,  401,
  401,  401,  401,  401,  401,  401,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  401,  401,  401,    0,
  401,  401,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  401,
    0,  401,  401,  402,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  402,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  401,  401,  401,  398,    0,  399,    0,    0,    0,
  402,  402,    0,  399,  402,  402,  402,  402,  402,  402,
  402,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  402,  402,  402,    0,  402,  402,    0,    0,    0,
    0,    0,    0,    0,    0,  300,  295,    0,    0,    0,
  298,  296,    0,  297,    0,  299,    0,    0,    0,    0,
    0,    0,    0,    0,  402,    0,  402,  402,  292,    0,
  291,  290,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  402,  402,  402,  399,
    0,    0,  294,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  293,    0,    0,  398,  398,  398,    0,  398,
  401,  401,  401,  398,  398,  401,  401,  401,  398,  401,
  398,  398,  398,  398,  398,  398,  398,    0,  401,  401,
  401,  398,  398,  398,  398,  398,  398,  398,  401,  401,
    0,  401,  401,  401,  401,  401,    0,  398,    0,    0,
  398,  398,  398,  398,  398,  398,  398,  398,  398,  398,
  398,  398,  398,  398,  398,  398,  398,  398,  398,  398,
  398,  401,  401,  401,  401,  401,  401,  401,  401,  401,
  401,  401,  401,  401,    0,    0,  401,  401,  401,  398,
    0,  401,  398,  398,  398,  398,  398,  398,    0,    0,
  399,  399,  399,    0,  399,  402,  402,  402,  399,  399,
  402,  402,  402,  399,  402,  399,  399,  399,  399,  399,
  399,  399,    0,  402,  402,  402,  399,  399,  399,  399,
  399,  399,  399,  402,  402,    0,  402,  402,  402,  402,
  402,    0,  399,    0,    0,  399,  399,  399,  399,  399,
  399,  399,  399,  399,  399,  399,  399,  399,  399,  399,
  399,  399,  399,  399,  399,  399,  402,  402,  402,  402,
  402,  402,  402,  402,  402,  402,  402,  402,  402,    0,
    0,  402,  402,  402,  399,  307,  402,  399,  399,  399,
  399,  399,  399,    0,    0,  307,    0,    0,    0,    0,
    0,  275,  276,  277,  278,  279,  280,  281,  282,  283,
  284,  285,  286,  287,    0,    0,  288,  289,  213,    0,
    0,    0,  307,  307,    0,    0,  307,  307,  307,  307,
  307,  307,  307,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  307,  307,    0,  307,  307,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  307,    0,  307,  307,
  237,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  237,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  307,  307,
  307,  213,    0,  397,    0,    0,    0,  237,  237,    0,
  397,  237,  237,  237,  237,  237,  237,  237,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  237,  237,
  237,    0,  237,  237,    0,    0,    0,    0,    0,    0,
    0,    0,  300,  295,    0,    0,    0,  298,  296,    0,
  297,    0,  299,    0,    0,    0,    0,    0,    0,    0,
    0,  237,    0,  237,  237,  292,    0,  291,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  237,  237,  397,    0,    0,  294,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  293,
    0,    0,  213,  213,  213,    0,  213,  307,  307,  307,
  213,  213,  307,  307,  307,  213,  307,  213,  213,  213,
  213,  213,  213,  213,    0,  307,  307,  307,  213,  213,
  213,  213,  213,  213,  213,  307,  307,    0,  307,  307,
  307,  307,  307,    0,  213,    0,    0,  213,  213,  213,
  213,  213,  213,  213,  213,  213,  213,  213,  213,  213,
  213,  213,  213,  213,  213,  213,  213,  213,  307,  307,
  307,  307,  307,  307,  307,  307,  307,  307,  307,  307,
  307,    0,    0,  307,  307,  307,  213,    0,  307,  213,
  213,  213,  213,  213,  213,    0,    0,  397,  397,  397,
    0,  397,  237,  237,  237,  397,  397,  237,  237,  237,
  397,  237,  397,  397,  397,  397,  397,  397,  397,    0,
    0,  237,  237,  397,  397,  397,  397,  397,  397,  397,
  237,  237,    0,  237,  237,  237,  237,  237,    0,  397,
    0,    0,  397,  397,  397,  397,  397,  397,  397,  397,
  397,  397,  397,  397,  397,  397,  397,  397,  397,  397,
  397,  397,  397,  237,  237,  237,  237,  237,  237,  237,
  237,  237,  237,  237,  237,  237,    0,    0,  237,  237,
  237,  397,  416,  237,  397,  397,  397,  397,  397,  397,
    0,    0,  416,    0,    0,    0,    0,    0,  275,  276,
  277,  278,  279,  280,  281,  282,  283,  284,  285,    0,
    0,    0,    0,  288,  289,  213,    0,    0,    0,  416,
  416,    0,    0,  416,  416,  416,  416,  416,  416,  416,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  416,  416,    0,  416,  416,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  416,    0,  416,  416,  400,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  400,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  416,  416,  416,  213,    0,
  396,    0,    0,    0,  400,  400,    0,  396,  400,  400,
  400,   61,  400,  400,  400,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  400,  400,   67,  400,
  400,    0,    0,    0,    0,    0,    0,    0,    0,  300,
  295,    0,    0,    0,  298,  296,    0,  297,    0,  299,
    0,    0,    0,    0,    0,    0,    0,    0,  400,    0,
  558,  400,  292,    0,  291,  290,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  400,  400,  400,  396,    0,    0,  294,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  293,    0,    0,  213,
  213,  213,    0,  213,  416,  416,  416,  213,  213,  416,
  416,  416,  213,  416,  213,  213,  213,  213,  213,  213,
  213,    0,  416,  416,  416,  213,  213,  213,  213,  213,
  213,  213,  416,  416,    0,  416,  416,  416,  416,  416,
    0,  213,    0,    0,  213,  213,  213,  213,  213,  213,
  213,  213,  213,  213,  213,  213,  213,  213,  213,  213,
  213,  213,  213,  213,  213,  416,  416,  416,  416,  416,
  416,  416,  416,  416,  416,  416,  416,  416,    0,    0,
  416,  416,  416,  213,    0,  416,  213,  213,  213,  213,
  213,  213,    0,    0,  396,  396,  396,    0,  396,  400,
  400,  400,  396,  396,  400,  400,  400,  396,  400,  396,
  396,  396,  396,  396,  396,  396,    0,  400,  400,    0,
  396,  396,  396,  396,  396,  396,  396,  400,  400,    0,
  400,  400,  400,  400,  400,    0,  396,    0,    0,  396,
  396,  396,  396,  396,  396,  396,  396,  396,  396,  396,
  396,  396,  396,  396,  396,  396,  396,  396,  396,  396,
  400,  400,  400,  400,  400,  400,  400,  400,  400,  400,
  400,  400,  400,    0,    0,  400,  400,  400,  396,  396,
    0,  396,  396,  396,  396,  396,  396,    0,    0,  396,
    0,    0,    0,    0,    0,  275,  276,  277,  278,  279,
  280,  281,  282,  283,  284,  285,  286,  287,    0,    0,
  288,  289,  396,    0,    0,    0,  396,  396,    0,  396,
  396,  396,  396,   60,  396,  396,  396,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  396,  396,
   66,  396,  396,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  396,    0,    0,  396,  397,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  397,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  396,  396,  396,  396,    0,  397,    0,    0,
    0,  397,  397,    0,  397,  397,  397,  397,   62,  397,
  397,  397,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  397,  397,   68,  397,  397,    0,    0,
    0,    0,    0,    0,  300,  295,    0,    0,    0,  298,
  296,    0,  297,    0,  299,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  397,    0,  292,  397,  291,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  397,  397,  397,
  397,  294,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  293,    0,    0,    0,    0,  396,  396,  396,    0,
  396,  396,  396,  396,  396,  396,  396,  396,  396,  396,
  396,  396,  396,  396,  396,  396,  396,  396,    0,  396,
  396,    0,  396,  396,  396,  396,  396,  396,  396,  396,
  396,    0,  396,  396,  396,  396,  396,    0,  396,    0,
    0,  396,  396,  396,  396,  396,  396,  396,  396,  396,
  396,  396,  396,  396,  396,  396,  396,  396,  396,  396,
  396,  396,  396,  396,  396,  396,  396,  396,  396,  396,
  396,  396,  396,  396,  396,    0,    0,  396,  396,  396,
  396,    0,    0,  396,  396,  396,  396,  396,  396,    0,
    0,  397,  397,  397,    0,  397,  397,  397,  397,  397,
  397,  397,  397,  397,  397,  397,  397,  397,  397,  397,
  397,  397,  397,    0,  397,  397,    0,  397,  397,  397,
  397,  397,  397,  397,  397,  397,    0,  397,  397,  397,
  397,  397,    0,  397,    0,    0,  397,  397,  397,  397,
  397,  397,  397,  397,  397,  397,  397,  397,  397,  397,
  397,  397,  397,  397,  397,  397,  397,  397,  397,  397,
  397,  397,  397,  397,  397,  397,  397,  397,  397,  397,
    0,    0,  397,  397,  397,  397,  416,    0,  397,  397,
  397,  397,  397,  397,    0,    0,  416,    0,    0,    0,
  275,  276,  277,  278,  279,  280,  281,  282,    0,  284,
  285,    0,    0,    0,    0,  288,  289,    0,    0,  213,
    0,    0,    0,    0,    0,    0,    0,  416,    0,    0,
    0,    0,  416,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  416,    0,  406,    0,    0,
  300,  295,    0,    0,    0,  298,  296,    0,  297,    0,
  299,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  292,    0,  291,  307,    0,    0,  416,
    0,    0,    0,    0,    0,    0,  307,    0,  406,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  294,    0,    0,
    0,  416,  213,  307,  307,    0,    0,  307,  307,  307,
  307,  307,  307,  307,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  307,  307,  307,  293,  307,  307,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  307,    0,  307,
  307,    0,  406,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  220,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  220,    0,  307,
  307,  307,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   25,    0,    0,  220,    0,
    0,  218,    0,    0,    0,   25,    0,    0,    0,    0,
    0,    0,    0,  213,  213,  213,  220,  213,  416,  416,
  416,  213,  213,  416,  416,  416,  213,  416,  213,  213,
  213,  213,  213,  213,  213,    0,   25,  416,    0,  213,
  213,  213,  213,  213,  213,  213,  416,  416,    0,  416,
  416,  416,  416,  416,   25,  213,    0,    0,  213,  213,
  213,  213,  213,  213,  213,  213,  213,  213,  213,  213,
  213,  213,  213,  213,  213,  213,  213,  213,  213,  406,
  406,  406,  220,    0,    0,  406,  406,    0,  406,  149,
    0,    0,    0,    0,    0,    0,  416,  213,    0,  149,
  213,  213,  213,  213,  213,  213,  275,    0,  307,  307,
  307,  280,  281,  307,  307,  307,    0,  307,    0,    0,
   25,  288,  289,    0,    0,    0,  307,  307,  307,    0,
  149,    0,    0,  218,    0,    0,  307,  307,    0,  307,
  307,  307,  307,  307,    0,    0,    0,    0,  149,    0,
  416,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  416,    0,    0,    0,    0,    0,    0,    0,    0,  307,
  307,  307,  307,  307,  307,  307,  307,  307,  307,  307,
  307,  307,    0,    0,  307,  307,  307,  416,  416,  307,
    0,  416,  416,  416,  416,  416,  416,  416,    0,    0,
    0,   13,    0,    0,    0,    0,    0,    0,  416,  416,
  416,   13,  416,  416,  149,    0,    0,    0,    0,  220,
  220,  220,    0,    0,  220,  220,  220,    0,  220,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  220,  233,
    0,  416,   13,  416,  416,    0,    0,  220,  220,  233,
  220,  220,  220,  220,  220,    0,    0,   25,   25,   25,
   13,    0,    0,   25,   25,    0,   25,    0,    0,    0,
    0,    0,    0,  416,  416,  416,  233,  233,    0,    0,
  233,  233,  233,  233,  233,  233,  233,    0,   25,   25,
   25,   25,   25,    0,    0,    0,    0,  233,  233,  233,
   69,  233,  233,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   13,    0,    0,  351,
  233,    0,  233,  233,    0,    0,    0,    0,    0,  351,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  149,  149,  149,    0,    0,    0,  149,  149,    0,
  149,    0,    0,  233,  233,    0,  351,  351,    0,    0,
  351,  351,  351,  351,  351,  351,  351,    0,    0,  149,
  149,    0,  149,  149,  149,  149,  149,  351,  351,  351,
   64,  351,  351,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  416,  416,  416,    0,    0,  416,  416,  416,
  351,  416,  351,  351,    0,    0,    0,    0,    0,    0,
  416,  416,  416,    0,    0,    0,    0,    0,    0,    0,
  416,  416,    0,  416,  416,  416,  416,  416,    0,    0,
    0,    0,    0,  351,  351,    0,    0,    0,    0,    0,
    0,    0,    0,   13,   13,   13,    0,    0,    0,   13,
   13,    0,   13,  416,  416,  416,  416,  416,  416,  416,
  416,  416,  416,  416,  416,  416,    0,    0,  416,  416,
  416,    0,    0,  416,   13,   13,   13,   13,   13,    0,
    0,  233,  233,  233,    0,    0,  233,  233,  233,    0,
  233,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  233,  233,    0,    0,    0,    0,    0,    0,    0,  233,
  233,    0,  233,  233,  233,  233,  233,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  233,  233,  233,  233,  233,  233,  233,  233,
  233,  233,  233,  233,  233,    0,    0,  233,  233,  233,
    0,    0,  233,    0,    0,    0,    0,    0,    0,    0,
    0,  351,  351,  351,    0,    0,  351,  351,  351,    0,
  351,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  351,  351,    0,    0,    0,    0,    0,    0,    0,  351,
  351,    0,  351,  351,  351,  351,  351,    0,    0,    0,
    0,    0,    0,  239,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  239,    0,    0,    0,    0,    0,    0,
    0,    0,  351,  351,  351,  351,  351,  351,  351,  351,
  351,  351,  351,  351,  351,    0,    0,  351,  351,  351,
  239,  239,  351,    0,  239,  239,  239,  239,  239,  239,
  239,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  239,  239,  239,   65,  239,  239,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  227,    0,
    0,    0,    0,    0,  239,    0,  239,  239,  227,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  227,  227,  239,  239,  227,
  227,  227,  227,  227,  227,  227,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  227,  227,  227,    0,
  227,  227,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  227,
    0,  227,  227,    0,  251,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  251,    0,    0,    0,    0,   14,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   14,
    0,    0,  227,  227,    0,    0,    0,    0,    0,    0,
    0,  251,  251,    0,    0,  251,  251,  251,  251,  251,
  251,  251,    0,    0,    0,    0,    0,    0,    0,    0,
   14,    0,  251,  251,  251,    0,  251,  251,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   14,    0,
    0,    0,    0,    0,    0,  239,  239,  239,    0,    0,
  239,  239,  239,    0,  239,  251,    0,  251,  251,    0,
    0,    0,    0,    0,  239,  239,    0,    0,    0,    0,
    0,    0,    0,  239,  239,    0,  239,  239,  239,  239,
  239,    0,    0,    0,    0,    0,    0,  416,  251,  251,
    0,    0,    0,    0,    0,    0,    0,  416,    0,    0,
    0,    0,    0,    0,   14,    0,  239,  239,  239,  239,
  239,  239,  239,  239,  239,  239,  239,  239,  239,    0,
    0,  239,  239,  239,    0,    0,  239,    0,  416,    0,
  227,  227,  227,    0,    0,  227,  227,  227,    0,  227,
    0,    0,    0,    0,    0,    0,  416,    0,    0,  227,
  227,    0,    0,    0,    0,    0,    0,    0,  227,  227,
    0,  227,  227,  227,  227,  227,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  416,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  227,  227,  227,  227,  227,  227,  227,  227,  227,
  227,  227,  227,  227,    0,    0,  227,  227,  227,    0,
    0,  227,  416,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  251,  251,  251,    0,
    0,  251,  251,  251,    0,  251,    0,    0,    0,    0,
    0,   14,   14,   14,    0,  251,  251,   14,   14,    0,
   14,    0,    0,    0,  251,  251,    0,  251,  251,  251,
  251,  251,    0,    0,    0,    0,    0,    0,  244,    0,
    0,    0,   14,   14,   14,   14,   14,    0,  244,    0,
    0,    0,    0,    0,    0,    0,    0,  251,  251,  251,
  251,  251,  251,  251,  251,  251,  251,  251,  251,  251,
    0,    0,  251,  251,  251,  244,  244,  251,    0,  244,
  244,  244,  244,  244,  244,  244,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  244,  244,  244,    0,
  244,  244,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  416,
  416,  416,    0,  247,  416,  416,  416,    0,  416,  244,
    0,  244,  244,  247,    0,    0,    0,    0,  416,  416,
    0,    0,    0,    0,    0,    0,    0,  416,  416,    0,
  416,  416,  416,  416,  416,    0,    0,    0,    0,    0,
  247,  247,  244,  244,  247,  247,  247,  247,  247,  247,
  247,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  247,  247,  247,    0,  247,  247,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  247,    0,  247,  247,    0,  247,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  247,
    0,    0,    0,    0,   15,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   15,    0,    0,  247,  247,    0,
    0,    0,    0,    0,    0,    0,  247,  247,    0,    0,
  247,  247,  247,  247,  247,  247,  247,    0,    0,    0,
    0,    0,    0,    0,    0,   15,    0,    0,  247,  247,
    0,  247,  247,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   15,    0,    0,    0,    0,    0,    0,
  244,  244,  244,    0,    0,  244,  244,  244,    0,  244,
  247,    0,  247,  247,    0,    0,    0,    0,    0,  244,
  244,    0,    0,    0,    0,    0,    0,    0,  244,  244,
    0,  244,  244,  244,  244,  244,    0,    0,    0,    0,
    0,    0,  215,  247,  247,    0,    0,    0,    0,    0,
    0,    0,  215,    0,    0,    0,    0,    0,    0,   15,
    0,  244,  244,  244,  244,  244,  244,  244,  244,  244,
  244,  244,  244,  244,    0,    0,  244,  244,  244,    0,
    0,  244,    0,  215,    0,  247,  247,  247,    0,    0,
  247,  247,  247,    0,  247,    0,    0,    0,    0,    0,
    0,  215,    0,    0,  247,  247,    0,    0,    0,    0,
    0,    0,    0,  247,  247,    0,  247,  247,  247,  247,
  247,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  215,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  247,  247,  247,  247,
  247,  247,  247,  247,  247,  247,  247,  247,  247,    0,
    0,  247,  247,  247,    0,    0,  247,  215,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  247,  247,  247,    0,    0,  247,  247,  247,    0,
  247,    0,    0,    0,    0,    0,   15,   15,   15,    0,
  247,  247,   15,   15,    0,   15,    0,    0,    0,  247,
  247,    0,  247,  247,  247,  247,  247,    0,    0,    0,
    0,    0,    0,  189,    0,    0,    0,   15,   15,   15,
   15,   15,    0,  189,    0,    0,    0,    0,    0,    0,
    0,    0,  247,  247,  247,  247,  247,  247,  247,  247,
  247,  247,  247,  247,  247,    0,    0,  247,  247,  247,
  189,  189,  247,    0,  189,  189,  189,  189,  189,    0,
  189,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  189,  189,  189,    0,  189,  189,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  215,  215,  215,    0,  189,  215,
  215,  215,    0,  215,    0,    0,  189,  189,  189,    0,
    0,    0,    0,  215,  215,    0,    0,    0,    0,    0,
    0,    0,  215,  215,    0,  215,  215,  215,  215,  215,
    0,    0,    0,    0,    0,  189,  189,  189,  189,  189,
  189,  189,  189,  189,    0,  189,    0,    0,    0,   16,
    0,    0,    0,    0,    0,    0,    0,  189,  189,   16,
  189,  189,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   16,  189,  189,  165,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  165,    0,    0,    0,    0,   16,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  189,  189,    0,    0,    0,    0,    0,    0,
  165,  165,    0,    0,  165,  165,  165,  165,  165,    0,
  165,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  165,  165,  165,    0,  165,  165,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   16,  189,  189,  189,    0,    0,
  189,  189,  189,    0,  189,    0,  165,  165,    0,    0,
    0,    0,    0,    0,  189,  189,    0,    0,    0,    0,
    0,    0,    0,  189,  189,    0,  189,  189,  189,  189,
  189,    0,    0,    0,   17,    0,    0,  165,  165,    0,
    0,    0,    0,    0,   17,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  189,  189,  189,  189,
  189,  189,  189,  189,  189,  189,  189,  189,  189,    0,
    0,  189,  189,    0,    0,   17,  189,    0,    0,    0,
  189,  189,  189,    0,    0,  189,  189,  189,    0,  189,
    0,    0,    0,   17,    0,    0,    0,    0,    0,  189,
  189,    0,    0,    0,    0,    0,    0,    0,  189,  189,
    0,  189,  189,  189,  189,  189,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   16,   16,   16,    0,    0,    0,   16,   16,    0,
   16,  189,  189,  189,  189,  189,  189,  189,  189,  189,
  189,  189,  189,  189,    0,    0,  189,  189,    0,   17,
    0,  189,   16,   16,   16,   16,   16,    0,    0,    0,
    0,    0,    0,    0,    0,  165,  165,  165,    0,    0,
  165,  165,  165,    0,  165,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  165,  165,    0,    0,    0,    0,
    0,    0,    0,  165,  165,    0,  165,  165,  165,  165,
  165,    0,    0,    0,    0,    0,  166,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  166,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  165,  165,  165,
  165,  165,  165,  165,  165,  165,  165,  165,  165,    0,
    0,  165,  165,  166,  166,    0,  165,  166,  166,  166,
  166,  166,    0,  166,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  166,  166,  166,    0,  166,  166,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   17,   17,   17,    0,
    0,  180,   17,   17,    0,   17,    0,    0,    0,  166,
  166,  180,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   17,   17,   17,
   17,   17,    0,    0,    0,    0,    0,    0,  180,  180,
  166,  166,  180,  180,  180,  180,  180,    0,  180,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  180,
  180,  180,    0,  180,  180,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  180,  180,  181,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  181,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  180,  180,    0,    0,    0,
    0,    0,    0,  181,  181,    0,    0,  181,  181,  181,
  181,  181,    0,  181,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  181,  181,  181,    0,  181,  181,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  166,  166,
  166,    0,    0,  166,  166,  166,    0,  166,    0,  181,
  181,    0,    0,    0,    0,    0,    0,  166,  166,    0,
    0,    0,    0,    0,    0,    0,  166,  166,    0,  166,
  166,  166,  166,  166,    0,    0,    0,    0,    0,    0,
  181,  181,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  166,  166,  166,  166,  166,  166,  166,  166,  166,  166,
  166,  166,    0,    0,  166,  166,    0,    0,    0,  166,
    0,    0,    0,  180,  180,  180,    0,    0,  180,  180,
  180,    0,  180,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  180,  180,    0,    0,    0,    0,    0,    0,
    0,  180,  180,    0,  180,  180,  180,  180,  180,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  180,  180,  180,  180,  180,
  180,  180,  180,  180,  180,  180,  180,    0,    0,  180,
  180,    0,    0,    0,  180,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  181,  181,
  181,    0,    0,  181,  181,  181,    0,  181,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  181,  181,    0,
    0,    0,    0,    0,    0,    0,  181,  181,    0,  181,
  181,  181,  181,  181,    0,    0,    0,    0,    0,  164,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  164,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  181,  181,  181,  181,  181,  181,  181,  181,  181,  181,
  181,  181,    0,    0,  181,  181,  164,  164,    0,  181,
  164,  164,  164,  164,  164,    0,  164,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  164,  164,  164,
    0,  164,  164,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  161,    0,    0,    0,    0,    0,
    0,    0,  164,  164,  161,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  161,  161,  164,  164,  161,  161,  161,  161,  161,
    0,  161,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  161,  161,  161,    0,  161,  161,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  161,  161,  162,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  162,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  161,  161,
    0,    0,    0,    0,    0,    0,  162,  162,    0,    0,
  162,  162,  162,  162,  162,    0,  162,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  162,  162,  162,
    0,  162,  162,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  164,  164,  164,    0,    0,  164,  164,  164,    0,
  164,    0,  162,  162,    0,    0,    0,    0,    0,    0,
  164,  164,    0,    0,    0,    0,    0,    0,    0,  164,
  164,    0,  164,  164,  164,  164,  164,    0,    0,    0,
  167,    0,    0,  162,  162,    0,    0,    0,    0,    0,
  167,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  164,  164,  164,  164,  164,  164,  164,
  164,  164,  164,  164,  164,    0,    0,  164,  164,    0,
    0,  167,  164,    0,  167,    0,  161,  161,  161,    0,
    0,  161,  161,  161,    0,  161,    0,    0,  167,  167,
  167,    0,  167,  167,    0,  161,  161,    0,    0,    0,
    0,    0,    0,    0,  161,  161,    0,  161,  161,  161,
  161,  161,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  167,  167,    0,    0,    0,    0,    0,
    0,  230,    0,    0,    0,    0,    0,    0,  161,  161,
  161,  161,  161,  161,  161,  161,  161,  161,  161,  161,
    0,    0,  161,  161,  167,  167,    0,  161,  300,  295,
    0,    0,    0,  298,  296,    0,  297,    0,  299,    0,
    0,  162,  162,  162,    0,    0,  162,  162,  162,    0,
  162,  292,    0,  291,  290,    0,    0,    0,    0,    0,
  162,  162,    0,    0,    0,    0,    0,    0,    0,  162,
  162,    0,  162,  162,  162,  162,  162,    0,    0,    0,
    0,    0,  163,    0,    0,  294,    0,    0,    0,    0,
    0,    0,  163,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  162,  162,  162,  162,  162,  162,  162,
  162,  162,  162,  162,  162,  293,    0,  162,  162,  163,
  163,    0,  162,  163,  163,  163,  163,  163,    0,  163,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  163,  163,  163,    0,  163,  163,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  159,    0,  167,  167,  167,    0,    0,  167,  167,  167,
  159,  167,    0,    0,    0,  163,  163,    0,    0,    0,
    0,  167,  167,    0,    0,    0,    0,    0,    0,    0,
  167,  167,    0,  167,  167,  167,  167,  167,  159,    0,
    0,  159,    0,  159,  159,  159,  163,  163,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  159,  159,
  159,    0,  159,  159,  167,  167,  167,  167,  167,  167,
  167,  167,  167,  167,  167,  167,    0,    0,    0,    0,
    0,    0,    0,  167,    0,  160,    0,    0,    0,    0,
    0,    0,    0,  159,  159,  160,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  160,  159,  159,  160,    0,  160,  160,
  160,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  160,  160,  160,    0,  160,  160,    0,
    0,    0,    0,    0,  275,  276,  277,  278,  279,  280,
  281,  282,  283,  284,  285,  286,  287,    0,    0,  288,
  289,    0,    0,    0,    0,    0,    0,    0,  160,  160,
    0,    0,  183,    0,  163,  163,  163,    0,    0,  163,
  163,  163,  183,  163,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  163,  163,    0,    0,    0,    0,  160,
  160,    0,  163,  163,    0,  163,  163,  163,  163,  163,
  183,    0,    0,  183,    0,    0,  183,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  183,  183,  183,    0,  183,  183,  163,  163,  163,  163,
  163,  163,  163,  163,  163,  163,  163,  163,    0,    0,
  163,  163,  159,  159,  159,  163,    0,  159,  159,  159,
    0,  159,    0,    0,    0,  183,  183,    0,    0,    0,
    0,  159,  159,    0,    0,    0,    0,    0,    0,    0,
  159,  159,    0,  159,  159,  159,  159,  159,    0,    0,
    0,   34,    0,    0,    0,    0,  183,  183,    0,    0,
    0,   34,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  159,  159,  159,  159,  159,  159,
  159,  159,  159,  159,  159,  159,    0,    0,  159,  159,
    0,    0,   34,  159,    0,  169,    0,  160,  160,  160,
    0,    0,  160,  160,  160,  169,  160,    0,    0,    0,
   34,    0,    0,    0,    0,    0,  160,  160,    0,    0,
    0,    0,    0,    0,    0,  160,  160,    0,  160,  160,
  160,  160,  160,  169,    0,    0,  169,    0,    0,  169,
    0,    0,    0,    0,   34,    0,    0,    0,    0,    0,
    0,    0,    0,  169,  169,  169,    0,  169,  169,  160,
  160,  160,  160,  160,  160,  160,  160,  160,  160,  160,
  160,    0,    0,  160,  160,    0,   34,    0,  160,    0,
  168,    0,    0,    0,    0,    0,    0,    0,  169,  169,
  168,    0,    0,    0,  183,  183,  183,    0,    0,  183,
  183,  183,    0,  183,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  183,  183,    0,    0,    0,    0,  169,
  169,  168,  183,  183,  168,  183,  183,  183,  183,  183,
    0,    0,    0,    0,    0,    0,    0,    0,  168,  168,
  168,    0,  168,  168,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  183,  183,  183,  183,
  183,  183,  183,  183,  183,  183,  183,  183,  172,    0,
  183,  183,    0,  168,  168,  183,    0,    0,  172,    0,
    0,    0,    0,  300,  295,    0,    0,    0,  298,  296,
    0,  297,    0,  299,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  168,  168,  292,    0,  291,  172,
    0,    0,  172,   34,   34,   34,    0,    0,   34,   34,
   34,    0,   34,    0,    0,    0,  172,  172,  172,    0,
  172,  172,   34,    0,    0,    0,    0,    0,    0,    0,
  294,   34,   34,    0,   34,   34,   34,   34,   34,    0,
    0,    0,    0,    0,    0,    0,    0,  169,  169,  169,
    0,  172,  169,  169,  169,    0,  169,    0,    0,    0,
  293,    0,    0,  174,    0,    0,  169,  169,    0,    0,
    0,    0,    0,  174,    0,  169,  169,    0,  169,  169,
  169,  169,  169,  172,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  174,    0,    0,  174,    0,  169,
  169,  169,  169,  169,  169,  169,  169,  169,  169,  169,
  169,  174,  174,  174,    0,  174,  174,    0,  169,    0,
    0,    0,  168,  168,  168,    0,    0,  168,  168,  168,
    0,  168,    0,    0,    0,    0,    0,    0,  171,    0,
    0,  168,  168,    0,    0,    0,  174,    0,  171,    0,
  168,  168,    0,  168,  168,  168,  168,  168,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  174,  171,
    0,    0,  171,    0,  168,  168,  168,  168,  168,  168,
  168,  168,  168,  168,  168,  168,  171,  171,  171,    0,
  171,  171,    0,  168,    0,    0,    0,    0,    0,    0,
  172,  172,  172,    0,    0,  172,  172,  172,    0,  172,
    0,    0,    0,    0,    0,    0,  173,    0,    0,  172,
  172,  171,    0,    0,    0,    0,  173,    0,  172,  172,
    0,  172,  172,  172,  172,  172,    0,    0,    0,  275,
  276,  277,  278,  279,  280,  281,    0,    0,  284,  285,
    0,    0,    0,  171,  288,  289,    0,  173,    0,    0,
  173,    0,  172,  172,  172,  172,  172,  172,  172,  172,
  172,  172,  172,  172,  173,  173,  173,    0,  173,  173,
    0,  172,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  174,  174,  174,    0,  173,
  174,  174,  174,    0,  174,    0,    0,    0,    0,    0,
    0,  170,    0,    0,  174,  174,    0,    0,    0,    0,
    0,  170,    0,  174,  174,    0,  174,  174,  174,  174,
  174,  173,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  170,    0,    0,  170,    0,  174,  174,  174,
  174,  174,  174,  174,  174,  174,  174,  174,  174,  170,
  170,    0,    0,    0,  170,    0,  174,    0,    0,    0,
  171,  171,  171,    0,    0,  171,  171,  171,    0,  171,
    0,    0,    0,    0,    0,    0,  175,    0,    0,  171,
  171,    0,    0,    0,  170,    0,  175,    0,  171,  171,
    0,  171,  171,  171,  171,  171,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  170,  175,    0,    0,
  175,    0,  171,  171,  171,  171,  171,  171,  171,  171,
  171,  171,  171,  171,  175,  175,    0,    0,    0,  175,
    0,  171,    0,    0,    0,    0,    0,    0,  173,  173,
  173,    0,    0,  173,  173,  173,    0,  173,    0,    0,
    0,    0,    0,    0,  176,    0,    0,  173,  173,  175,
    0,    0,    0,    0,  176,    0,  173,  173,    0,  173,
  173,  173,  173,  173,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  175,    0,    0,    0,  176,    0,    0,  176,    0,
  173,  173,  173,  173,  173,  173,  173,  173,  173,  173,
  173,  173,  176,  176,    0,    0,    0,  176,    0,  173,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  170,  170,  170,  177,  176,  170,  170,
  170,    0,  170,    0,    0,    0,  177,    0,    0,    0,
    0,    0,  170,  170,    0,    0,    0,    0,    0,    0,
    0,  170,  170,    0,  170,  170,  170,  170,  170,  176,
    0,    0,    0,    0,    0,    0,    0,  177,    0,    0,
  177,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  177,  177,    0,    0,    0,  177,
    0,  170,  170,    0,    0,  170,  170,    0,    0,    0,
    0,    0,    0,    0,  170,    0,    0,    0,  175,  175,
  175,  184,    0,  175,  175,  175,    0,  175,    0,  177,
    0,  184,    0,    0,    0,    0,    0,  175,  175,    0,
    0,    0,    0,    0,    0,    0,  175,  175,    0,  175,
  175,  175,  175,  175,    0,    0,    0,    0,    0,    0,
    0,  177,  184,    0,    0,  184,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  184,
  184,    0,    0,    0,  184,    0,  175,  175,    0,    0,
  175,  175,    0,    0,    0,    0,    0,    0,    0,  175,
    0,    0,    0,    0,    0,    0,  176,  176,  176,  178,
    0,  176,  176,  176,  184,  176,    0,    0,    0,  178,
    0,    0,    0,    0,    0,  176,  176,    0,    0,    0,
    0,    0,    0,    0,  176,  176,    0,  176,  176,  176,
  176,  176,    0,    0,    0,    0,  184,    0,    0,    0,
  178,    0,    0,  178,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  178,  178,    0,
    0,    0,  178,    0,  176,  176,    0,    0,  176,  176,
    0,    0,    0,    0,    0,    0,    0,  176,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  177,  177,
  177,  179,  178,  177,  177,  177,    0,  177,    0,    0,
    0,  179,    0,    0,    0,    0,    0,  177,  177,    0,
    0,    0,    0,    0,    0,    0,  177,  177,    0,  177,
  177,  177,  177,  177,  178,    0,    0,    0,    0,    0,
    0,    0,  179,    0,    0,  179,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  179,
  179,    0,    0,    0,  179,    0,  177,  177,    0,    0,
  177,  177,    0,    0,    0,    0,    0,    0,    0,  177,
    0,    0,    0,  184,  184,  184,  185,    0,  184,  184,
  184,    0,  184,    0,  179,    0,  185,    0,    0,    0,
    0,    0,  184,  184,    0,    0,    0,    0,    0,    0,
    0,  184,  184,    0,  184,  184,  184,  184,  184,    0,
    0,    0,    0,    0,    0,    0,  179,  185,    0,    0,
  185,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  185,  185,    0,    0,    0,  185,
    0,  184,  184,    0,    0,  184,  184,    0,    0,    0,
    0,    0,    0,    0,  184,    0,    0,    0,    0,    0,
    0,  178,  178,  178,  157,    0,  178,  178,  178,  185,
  178,    0,    0,    0,  157,    0,    0,    0,    0,    0,
  178,  178,    0,    0,    0,    0,    0,    0,    0,  178,
  178,    0,  178,  178,  178,  178,  178,    0,    0,    0,
    0,  185,    0,    0,    0,  157,  158,    0,  157,    0,
    0,    0,    0,    0,    0,    0,  158,    0,    0,    0,
    0,    0,  157,  157,    0,    0,    0,  157,    0,  178,
  178,    0,    0,  178,  178,    0,    0,    0,    0,    0,
    0,    0,  178,    0,    0,    0,    0,  158,    0,    0,
  158,    0,    0,  179,  179,  179,    0,  157,  179,  179,
  179,  149,  179,    0,  158,  158,    0,    0,    0,  158,
    0,  149,  179,  179,    0,    0,    0,    0,    0,    0,
    0,  179,  179,    0,  179,  179,  179,  179,  179,  157,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  158,
    0,    0,  149,    0,    0,  149,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  149,
  149,  179,  179,    0,    0,  179,  179,    0,    0,    0,
    0,  158,    0,    0,  179,    0,    0,    0,  185,  185,
  185,    0,    0,  185,  185,  185,  187,  185,    0,    0,
    0,    0,    0,    0,  149,    0,  187,  185,  185,    0,
    0,    0,    0,    0,    0,    0,  185,  185,    0,  185,
  185,  185,  185,  185,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  149,  187,    0,    0,
  187,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  187,  187,  222,  185,    0,    0,
  185,  185,    0,    0,    0,    0,  222,    0,    0,  185,
    0,    0,    0,    0,    0,    0,  157,  157,  157,    0,
    0,  157,  157,  157,    0,  157,    0,    0,    0,  187,
    0,    0,    0,    0,    0,  157,  157,  222,    0,    0,
  219,    0,    0,    0,  157,  157,    0,  157,  157,  157,
  157,  157,    0,    0,    0,  222,    0,    0,  158,  158,
  158,  187,    0,  158,  158,  158,  151,  158,    0,    0,
    0,    0,    0,    0,    0,    0,  151,  158,  158,    0,
    0,    0,    0,    0,    0,    0,  158,  158,    0,  158,
  158,  158,  158,  158,    0,    0,    0,  157,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  151,    0,    0,
  151,    0,    0,  149,  149,  149,    0,  188,  149,  149,
  149,  222,  149,    0,  151,  151,    0,  188,    0,    0,
    0,    0,  149,  149,    0,    0,    0,    0,    0,  158,
    0,  149,  149,    0,  149,  149,  149,  149,  149,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  188,  151,
    0,  188,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  188,  188,    0,    0,    0,
  155,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  155,  151,    0,    0,  149,    0,    0,    0,  187,  187,
  187,    0,    0,  187,  187,  187,    0,  187,    0,    0,
  188,    0,    0,    0,    0,    0,    0,  187,  187,    0,
    0,  155,    0,    0,  155,  153,  187,  187,    0,  187,
  187,  187,  187,  187,    0,  153,    0,    0,  155,  155,
    0,    0,  188,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  222,  222,
  222,    0,  154,  222,  222,  222,  153,  222,    0,  153,
    0,    0,  154,  155,    0,    0,    0,  222,    0,  187,
    0,    0,    0,  153,  153,    0,  222,  222,    0,  222,
  222,  222,  222,  222,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  154,    0,  155,  154,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  153,    0,
  154,  154,    0,    0,    0,    0,    0,    0,  151,  151,
  151,  152,    0,  151,  151,  151,    0,  151,    0,    0,
    0,  152,    0,    0,    0,    0,    0,  151,  151,    0,
  153,    0,    0,    0,    0,  154,  151,  151,    0,  151,
  151,  151,  151,  151,    0,    0,    0,    0,    0,    0,
    0,    0,  152,    0,    0,  152,    0,    0,    0,  188,
  188,  188,    0,    0,  188,  188,  188,  154,  188,  152,
  152,    0,    0,    0,    0,    0,    0,    0,  188,  188,
    0,    0,    0,    0,    0,    0,    0,  188,  188,  151,
  188,  188,  188,  188,  188,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  152,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  155,  155,  155,   36,    0,  155,  155,  155,
    0,  155,    0,    0,    0,   36,  152,    0,    0,    0,
  188,  155,  155,    0,    0,    0,    0,    0,    0,    0,
  155,  155,    0,  155,  155,  155,  155,  155,    0,    0,
    0,    0,    0,    0,    0,    0,   36,  153,  153,  153,
    0,    0,  153,  153,  153,    0,  153,    0,    0,    0,
    0,    0,    0,    0,   36,    0,  153,  153,    0,    0,
    0,    0,    0,    0,    0,  153,  153,    0,  153,  153,
  153,  153,  153,  155,  154,  154,  154,    0,    0,  154,
  154,  154,    0,  154,    0,    0,    0,    0,   36,    0,
    0,    0,    0,  154,  154,    0,    0,    0,    0,    0,
    0,    0,  154,  154,    0,  154,  154,  154,  154,  154,
    0,    0,    0,    0,    0,    0,    0,    0,  153,    0,
   36,    0,    0,  233,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  233,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  152,  152,  152,    0,    0,  152,  152,
  152,    0,  152,    0,    0,  154,    0,    0,    0,    0,
  233,  233,  152,  152,  233,  233,  233,   63,  233,  233,
  233,  152,  152,    0,  152,  152,  152,  152,  152,    0,
    0,    0,  233,  233,   69,  233,  233,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  351,    0,
    0,    0,    0,    0,  233,    0,    0,  233,  351,    0,
    0,    0,    0,    0,  152,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  351,  351,  233,  233,  351,
  351,  351,   58,  351,  351,  351,    0,   36,   36,   36,
    0,    0,   36,   36,   36,    0,   36,  351,  351,   64,
  351,  351,    0,    0,    0,    0,   36,    0,    0,    0,
    0,    0,    0,    0,    0,   36,   36,    0,   36,   36,
   36,   36,   36,    0,    0,    0,    0,    0,    0,  351,
    0,    0,  351,    0,  239,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  239,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  351,  351,    0,    0,    0,    0,    0,    0,
    0,  239,  239,    0,    0,  239,  239,  239,   59,  239,
  239,  239,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  239,  239,   65,  239,  239,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  233,  233,  233,    0,    0,
  233,  233,  233,    0,  233,  239,    0,    0,  239,    0,
    0,    0,    0,    0,  233,    0,    0,    0,    0,    0,
    0,    0,    0,  233,  233,    0,  233,  233,  233,  233,
  233,    0,    0,    0,  218,    0,    0,    0,  239,  239,
    0,    0,    0,    0,  218,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  233,  233,  233,  233,
  233,  233,  233,  233,  233,  233,  233,  233,  233,    0,
    0,  233,  233,  233,    0,  218,    0,    0,  218,    0,
  351,  351,  351,    0,    0,  351,  351,  351,    0,  351,
    0,    0,    0,  218,    0,    0,    0,    0,    0,  351,
    0,    0,    0,    0,    0,  282,    0,    0,  351,  351,
    0,  351,  351,  351,  351,  351,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  218,  282,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  351,  351,  351,  351,  351,  351,  351,  351,  351,
  351,  351,  351,  351,  282,    0,  351,  351,  351,  218,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  239,  239,  239,    0,
    0,  239,  239,  239,    0,  239,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  239,    0,    0,    0,    0,
    0,    0,    0,    0,  239,  239,    0,  239,  239,  239,
  239,  239,    0,    0,    0,    0,    0,    0,  244,    0,
    0,  282,    0,    0,    0,    0,    0,    0,  244,    0,
    0,    0,    0,    0,    0,    0,    0,  239,  239,  239,
  239,  239,  239,  239,  239,  239,  239,  239,  239,  239,
    0,    0,  239,  239,  239,  244,  244,    0,    0,  244,
  244,  244,    0,  244,  244,  244,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  244,  244,    0,
  244,  244,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  218,  218,  218,    0,
    0,  218,  218,  218,    0,  218,    0,    0,    0,  244,
    0,    0,  244,  416,    0,  218,  218,    0,    0,    0,
    0,    0,    0,    0,  218,  218,    0,  218,  218,  218,
  218,  218,    0,    0,    0,    0,  416,    0,    0,    0,
    0,    0,  244,  244,    0,    0,    0,    0,    0,    0,
    0,  282,  282,  282,  282,  282,  282,  282,  282,  282,
  282,  282,  416,  282,  282,  282,  282,  282,  282,  282,
  282,  282,  282,  282,    0,    0,    0,    0,  282,  282,
  282,  282,  282,  282,  282,    0,    0,  282,    0,    0,
    0,    0,    0,  282,  282,  282,  282,  282,  282,  282,
  282,  282,  282,  282,  282,  282,  282,  282,  282,  282,
  282,  282,  282,  282,  282,  282,  282,  282,    0,    0,
  416,    0,    0,    0,    0,    0,    0,    0,    0,  416,
    0,    0,    0,    0,    0,    0,  282,    0,    0,  282,
  282,  282,  282,  416,  282,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  416,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  244,  244,  244,    0,    0,  244,  244,  244,    0,  244,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  244,
    0,    0,    0,    0,    0,    0,    0,    0,  244,  244,
    0,  244,  244,  244,  244,  244,  186,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  416,  416,    0,    0,   58,
    0,  244,  244,  244,  244,  244,  244,  244,  244,  244,
  244,  244,  244,  244,    0,    0,  244,  244,  244,  416,
  416,  416,  416,  416,  416,  187,    0,  416,  416,  416,
    0,    0,    0,  416,    0,  416,  416,  416,  416,  416,
  416,  416,    0,    0,    0,    0,  416,  416,  416,  416,
  416,  416,  416,    0,    0,  416,    0,    0,    0,    0,
    0,  416,  416,  416,  416,  416,  416,  416,  416,  416,
  416,  416,  416,  416,  416,  416,  416,  416,  416,  416,
  416,  416,  416,  416,  416,  416,    0,    0,    0,    0,
    0,    0,   59,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  416,    0,    0,  416,  416,  416,
  416,    0,  416,    0,    0,    0,    0,    0,    0,    0,
   58,    0,    0,    0,    0,    0,  416,  416,  416,  416,
  416,  416,    0,    0,    0,  416,  416,    0,    0,    0,
  416,    0,  416,  416,  416,  416,  416,  416,  416,    0,
    0,    0,    0,  416,  416,  416,  416,  416,  416,  416,
    0,    0,  416,    0,    0,    0,    0,    0,  416,  416,
  416,  416,  416,  416,  416,  416,  416,  416,  416,  416,
  416,  416,  416,  416,  416,  416,  416,  416,  416,  416,
  416,  416,  416,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  416,    0,   59,  416,  416,  416,  416,    0,  416,
    0,   58,    0,    4,    5,    6,    0,    8,    0,    0,
    0,    9,   10,    0,    0,    0,   11,    0,   12,   13,
   14,   15,   16,   17,   18,    0,    0,    0,    0,   19,
   20,   21,   22,   23,   24,   25,    0,  189,   26,    0,
    0,    0,    0,    0,    0,   28,    0,  189,   31,   32,
   33,   34,   35,   36,   37,   38,   39,   40,   41,   42,
   43,   44,   45,   46,   47,   48,   49,   50,   51,    0,
    0,    0,    0,    0,  189,  189,    0,    0,  189,  189,
  189,    0,  189,    0,  189,    0,    0,   52,    0,    0,
   53,   54,   55,   56,   59,   57,  189,  189,    0,  189,
  189,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    3,    4,    5,    6,    7,    8,    0,
    0,  189,    9,   10,    0,    0,    0,   11,    0,   12,
   13,   14,   15,   16,   17,   18,    0,    0,    0,    0,
   19,   20,   21,   22,   23,   24,   25,    0,    0,   26,
    0,  189,  189,    0,    0,   27,   28,   29,   30,   31,
   32,   33,   34,   35,   36,   37,   38,   39,   40,   41,
   42,   43,   44,   45,   46,   47,   48,   49,   50,   51,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   58,    0,    0,    0,   52,    0,
    0,   53,   54,   55,   56,    0,   57,    0,    0,    0,
    0,    0,    0,    0,    3,    4,    5,    6,    7,    8,
  346,    0,    0,    9,   10,    0,    0,    0,   11,    0,
   12,   13,   14,   15,   16,   17,   18,    0,    0,    0,
    0,   19,   20,   21,   22,   23,   24,   25,    0,    0,
   26,    0,    0,    0,    0,    0,   27,   28,   29,   30,
   31,   32,   33,   34,   35,   36,   37,   38,   39,   40,
   41,   42,   43,   44,   45,   46,   47,   48,   49,   50,
   51,    0,    0,    0,    0,    0,    0,   59,   58,  189,
  189,  189,    0,    0,  189,  189,  189,    0,  189,   52,
    0,    0,  238,   54,   55,   56,    0,   57,  189,    0,
    0,    0,    0,    0,    0,    0,    0,  189,  189,    0,
  189,  189,  189,  189,  189,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  189,  189,  189,  189,  189,  189,  189,  189,  189,  189,
  189,  189,  189,    0,    0,  189,  189,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   59,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  208,    0,    0,    0,    0,    0,    0,  209,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    4,    5,
    6,    7,    8,    0,    0,    0,    9,   10,    0,    0,
    0,   11,    0,   12,   13,   14,   15,   16,   17,   18,
    0,    0,    0,    0,   19,   20,   21,   22,   23,   24,
   25,    0,    0,   26,    0,    0,    0,    0,    0,   27,
   28,   29,   30,   31,   32,   33,   34,   35,   36,   37,
   38,   39,   40,   41,   42,   43,   44,   45,   46,   47,
   48,   49,   50,   51,   59,  208,    0,    0,    0,    0,
    0,    0,  221,    0,    0,    0,    0,    0,    0,    0,
    0,    0,   52,    0,    0,   53,   54,   55,   56,    0,
   57,    0,    4,    5,    6,    7,    8,    0,    0,    0,
    9,   10,    0,    0,    0,   11,    0,   12,   13,   14,
   15,   16,   17,   18,    0,    0,    0,    0,   19,   20,
   21,   22,   23,   24,   25,    0,    0,   26,    0,    0,
    0,    0,    0,   27,   28,   29,   30,   31,   32,   33,
   34,   35,   36,   37,   38,   39,   40,   41,   42,   43,
   44,   45,   46,   47,   48,   49,   50,   51,   59,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  208,
    0,    0,    0,    0,    0,    0,   52,  356,    0,   53,
   54,   55,   56,    0,   57,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    4,    5,    6,    0,    8,
    0,    0,    0,    9,   10,    0,    0,    0,   11,    0,
   12,   13,   14,   15,   16,   17,   18,    0,    0,    0,
    0,  194,   20,   21,   22,   23,   24,   25,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   28,    0,    0,
   31,   32,   33,   34,   35,   36,   37,   38,   39,   40,
   41,   42,   43,   44,   45,   46,   47,   48,   49,   50,
   51,    0,   59,  208,    0,    0,    0,    0,    0,    0,
    0,  367,    0,    0,    0,    0,    0,    0,    0,   52,
    0,    0,  205,   54,   55,  206,  207,   57,    0,    4,
    5,    6,    0,    8,    0,    0,    0,    9,   10,    0,
    0,    0,   11,    0,   12,   13,   14,   15,   16,   17,
   18,    0,    0,    0,    0,  194,   20,   21,   22,   23,
   24,   25,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   28,    0,    0,   31,   32,   33,   34,   35,   36,
   37,   38,   39,   40,   41,   42,   43,   44,   45,   46,
   47,   48,   49,   50,   51,    0,   59,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   58,    0,    0,    0,
    0,    0,    0,   52,    0,    0,  205,   54,   55,  206,
  207,   57,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    4,    5,    6,    0,    8,    0,    0,
    0,    9,   10,    0,    0,    0,   11,    0,   12,   13,
   14,   15,   16,   17,   18,    0,    0,    0,    0,  194,
   20,   21,   22,   23,   24,   25,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   28,    0,    0,   31,   32,
   33,   34,   35,   36,   37,   38,   39,   40,   41,   42,
   43,   44,   45,   46,   47,   48,   49,   50,   51,   59,
  208,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   52,    0,    0,
  205,   54,   55,  206,  207,   57,    0,    4,    5,    6,
    0,    8,    0,    0,    0,    9,   10,    0,    0,    0,
   11,    0,   12,   13,   14,   15,   16,   17,   18,    0,
    0,    0,    0,  194,   20,   21,   22,   23,   24,   25,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   28,
    0,    0,   31,   32,   33,   34,   35,   36,   37,   38,
   39,   40,   41,   42,   43,   44,   45,   46,   47,   48,
   49,   50,   51,   59,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  208,    0,    0,    0,    0,    0,    0,
    0,   52,    0,    0,  205,   54,   55,  206,  207,   57,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    4,    5,    6,    0,    8,    0,    0,    0,    9,   10,
    0,    0,    0,   11,    0,   12,   13,   14,   15,   16,
   17,   18,    0,    0,    0,    0,   19,   20,   21,   22,
   23,   24,   25,    0,    0,   26,    0,    0,    0,    0,
    0,    0,   28,    0,    0,   31,   32,   33,   34,   35,
   36,   37,   38,   39,   40,   41,   42,   43,   44,   45,
   46,   47,   48,   49,   50,   51,   59,  208,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   52,    0,    0,   53,   54,   55,
   56,    0,   57,    0,    4,    5,    6,    0,    8,    0,
    0,    0,    9,   10,    0,    0,    0,   11,    0,   12,
   13,   14,   15,   16,   17,   18,    0,    0,    0,    0,
  194,   20,   21,   22,   23,   24,   25,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   28,    0,    0,   31,
   32,   33,   34,   35,   36,   37,   38,   39,   40,   41,
   42,   43,   44,   45,   46,   47,   48,   49,   50,   51,
   59,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  208,    0,    0,    0,    0,    0,    0,    0,   52,    0,
    0,  205,   54,   55,  206,  207,   57,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    4,    5,    6,
    0,    8,    0,    0,    0,    9,   10,    0,    0,    0,
   11,    0,   12,   13,   14,   15,   16,   17,   18,    0,
    0,    0,    0,  194,   20,   21,   22,   23,   24,   25,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   28,
    0,    0,   31,   32,   33,   34,   35,   36,   37,   38,
   39,   40,   41,   42,   43,   44,   45,   46,   47,   48,
   49,   50,   51,   59,  213,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   52,    0,    0,  205,   54,   55,  529,  207,   57,
    0,    4,    5,    6,    0,    8,    0,    0,    0,    9,
   10,    0,    0,    0,   11,    0,   12,   13,   14,   15,
   16,   17,   18,    0,    0,    0,    0,  194,  195,  196,
   22,   23,   24,   25,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   28,    0,    0,   31,   32,   33,   34,
   35,   36,   37,   38,   39,   40,   41,   42,   43,   44,
   45,   46,   47,   48,   49,   50,   51,  213,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  208,    0,    0,
    0,    0,    0,    0,    0,   52,    0,    0,  205,   54,
   55,  534,  207,   57,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    4,    5,    6,    0,    8,    0,
    0,    0,    9,   10,    0,    0,    0,   11,    0,   12,
   13,   14,   15,   16,   17,   18,    0,    0,    0,    0,
  194,  195,  196,   22,   23,   24,   25,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   28,    0,    0,   31,
   32,   33,   34,   35,   36,   37,   38,   39,   40,   41,
   42,   43,   44,   45,   46,   47,   48,   49,   50,   51,
   59,  208,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   52,    0,
    0,  205,   54,   55,  682,  207,   57,    0,  213,  213,
  213,    0,  213,    0,    0,    0,  213,  213,    0,    0,
    0,  213,    0,  213,  213,  213,  213,  213,  213,  213,
    0,    0,    0,    0,  213,  213,  213,  213,  213,  213,
  213,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  213,    0,    0,  213,  213,  213,  213,  213,  213,  213,
  213,  213,  213,  213,  213,  213,  213,  213,  213,  213,
  213,  213,  213,  213,   59,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  208,    0,    0,    0,    0,    0,
    0,    0,  213,    0,    0,  213,  213,  213,  213,  213,
  213,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    4,    5,    6,    0,    8,    0,    0,    0,    9,
   10,    0,    0,    0,   11,    0,   12,   13,   14,   15,
   16,   17,   18,    0,    0,    0,    0,  194,   20,   21,
   22,   23,   24,   25,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   28,    0,    0,   31,   32,   33,   34,
   35,   36,   37,   38,   39,   40,   41,   42,   43,   44,
   45,   46,   47,   48,   49,   50,   51,   59,  208,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   52,    0,    0,  205,   54,
   55,  242,    0,   57,    0,    4,    5,    6,    0,    8,
    0,    0,    0,    9,   10,    0,    0,    0,   11,    0,
   12,   13,   14,   15,   16,   17,   18,    0,    0,    0,
    0,  194,   20,   21,   22,   23,   24,   25,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   28,    0,    0,
   31,   32,   33,   34,   35,   36,   37,   38,   39,   40,
   41,   42,   43,   44,   45,   46,   47,   48,   49,   50,
   51,   59,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  208,    0,    0,    0,    0,    0,    0,    0,   52,
    0,    0,  205,   54,   55,  457,    0,   57,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    4,    5,
    6,    0,    8,    0,    0,    0,    9,   10,    0,    0,
    0,   11,    0,   12,   13,   14,   15,   16,   17,   18,
    0,    0,    0,    0,  194,  195,  196,   22,   23,   24,
   25,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   28,    0,    0,   31,   32,   33,   34,   35,   36,   37,
   38,   39,   40,   41,   42,   43,   44,   45,   46,   47,
   48,   49,   50,   51,   59,  208,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,   52,    0,    0,  205,   54,   55,  457,    0,
   57,    0,    4,    5,    6,    0,    8,    0,    0,    0,
    9,   10,    0,    0,    0,   11,    0,   12,   13,   14,
   15,   16,   17,   18,    0,    0,    0,    0,  194,  195,
  196,   22,   23,   24,   25,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   28,    0,    0,   31,   32,   33,
   34,   35,   36,   37,   38,   39,   40,   41,   42,   43,
   44,   45,   46,   47,   48,   49,   50,   51,   59,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  208,    0,
    0,    0,    0,    0,    0,    0,   52,    0,    0,  205,
   54,   55,  506,    0,   57,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    4,    5,    6,    0,    8,
    0,    0,    0,    9,   10,    0,    0,    0,   11,    0,
   12,   13,   14,   15,   16,   17,   18,    0,    0,    0,
    0,  194,   20,   21,   22,   23,   24,   25,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   28,    0,    0,
   31,   32,   33,   34,   35,   36,   37,   38,   39,   40,
   41,   42,   43,   44,   45,   46,   47,   48,   49,   50,
   51,   59,  407,    0,    0,    0,    0,    0,    0,  407,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   52,
    0,    0,  205,   54,   55,  551,    0,   57,    0,    4,
    5,    6,    0,    8,    0,    0,    0,    9,   10,    0,
    0,    0,   11,    0,   12,   13,   14,   15,   16,   17,
   18,    0,    0,    0,    0,  194,  195,  196,   22,   23,
   24,   25,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   28,    0,    0,   31,   32,   33,   34,   35,   36,
   37,   38,   39,   40,   41,   42,   43,   44,   45,   46,
   47,   48,   49,   50,   51,  407,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  208,    0,    0,    0,    0,
    0,    0,    0,   52,    0,    0,  205,   54,   55,  649,
    0,   57,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    4,    5,    6,    0,    8,    0,    0,    0,
    9,   10,    0,    0,    0,   11,    0,   12,   13,   14,
   15,   16,   17,   18,    0,    0,    0,    0,  194,  195,
  196,   22,   23,   24,   25,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   28,    0,    0,   31,   32,   33,
   34,   35,   36,   37,   38,   39,   40,   41,   42,   43,
   44,   45,   46,   47,   48,   49,   50,   51,   59,  208,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   52,    0,    0,  205,
   54,   55,  676,    0,   57,    0,  407,  407,  407,    0,
  407,    0,    0,    0,  407,  407,    0,    0,    0,  407,
    0,  407,  407,  407,  407,  407,  407,  407,    0,    0,
    0,    0,  407,  407,  407,  407,  407,  407,  407,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  407,    0,
    0,  407,  407,  407,  407,  407,  407,  407,  407,  407,
  407,  407,  407,  407,  407,  407,  407,  407,  407,  407,
  407,  407,   59,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  186,    0,    0,    0,    0,    0,    0,    0,
  407,    0,    0,  407,  407,  407,    0,    0,  407,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    4,
    5,    6,    0,    8,    0,    0,    0,    9,   10,    0,
    0,    0,   11,    0,   12,   13,   14,   15,   16,   17,
   18,    0,    0,    0,    0,  194,  195,  196,   22,   23,
   24,   25,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   28,    0,    0,   31,   32,   33,   34,   35,   36,
   37,   38,   39,   40,   41,   42,   43,   44,   45,   46,
   47,   48,   49,   50,   51,  186,    0,    0,    0,    0,
    0,    0,    0,    0,  392,    0,    0,    0,    0,    0,
    0,    0,    0,   52,  392,    0,  205,   54,   55,    0,
    0,   57,    0,    4,    5,    6,    0,    8,    0,    0,
    0,    9,   10,    0,    0,    0,   11,    0,   12,   13,
   14,   15,   16,   17,   18,  392,    0,    0,  392,  194,
   20,   21,   22,   23,   24,   25,    0,    0,    0,    0,
    0,    0,    0,  392,    0,   28,    0,    0,   31,   32,
   33,   34,   35,   36,   37,   38,   39,   40,   41,   42,
   43,   44,   45,   46,   47,   48,   49,   50,   51,    0,
    0,    0,    0,    0,    0,    0,    0,  392,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   52,  159,  154,
  205,   54,   55,  157,  155,   57,  156,    0,  158,    0,
    0,    0,    0,    0,    0,    0,  186,  186,  186,  392,
  186,  151,    0,  150,  186,  186,    0,    0,    0,  186,
    0,  186,  186,  186,  186,  186,  186,  186,    0,    0,
    0,    0,  186,  186,  186,  186,  186,  186,  186,    0,
    0,    0,    0,    0,    0,  153,    0,  161,  186,    0,
    0,  186,  186,  186,  186,  186,  186,  186,  186,  186,
  186,  186,  186,  186,  186,  186,  186,  186,  186,  186,
  186,  186,    0,    0,    0,  152,    0,  160,    0,    0,
    0,  159,  154,    0,    0,    0,  157,  155,    0,  156,
  186,  158,    0,  186,  186,  186,    0,    0,  186,    0,
    0,    0,    0,    0,  151,    0,  150,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   33,    0,    0,  153,    0,
  161,    0,    0,    0,    0,   33,  392,  392,  392,    0,
    0,  392,  392,  392,    0,  392,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  392,  392,    0,  152,    0,
  160,    0,    0,    0,  392,  392,   33,  392,  392,  392,
  392,  392,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   33,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   92,   93,
   94,   95,   96,   97,   98,   99,    0,    0,  100,  101,
  102,  103,  104,    0,    0,  105,  106,  107,  108,  109,
  110,  111,    0,    0,  112,  113,  114,  169,  170,  171,
  172,  119,  120,  121,  122,  123,  124,  125,  126,  127,
  128,  129,  130,  173,  174,  175,  134,  228,    0,  176,
   33,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  136,  137,  138,  139,  140,  141,    0,  142,
  143,    0,    0,  144,    0,    0,    0,  145,  146,  147,
  148,    0,    0,    0,    0,    0,    0,    0,  149,    0,
   57,   92,   93,   94,   95,   96,   97,   98,   99,    0,
    0,  100,  101,  102,  103,  104,    0,    0,  105,  106,
  107,  108,  109,  110,  111,    0,    0,  112,  113,  114,
  169,  170,  171,  172,  119,  120,  121,  122,  123,  124,
  125,  126,  127,  128,  129,  130,  173,  174,  175,  134,
    0,    0,  176,    0,  159,  154,    0,  162,    0,  157,
  155,    0,  156,    0,  158,  136,  137,  138,  139,  140,
  141,    0,  142,  143,    0,    0,  144,  151,    0,  150,
  145,  146,  147,  148,    0,    0,    0,    0,    0,    0,
    0,  149,    0,   57,    0,    0,    0,   33,   33,   33,
    0,    0,   33,   33,   33,    0,   33,    0,    0,    0,
    0,  153,    0,  161,    0,    0,   33,    0,    0,    0,
    0,    0,    0,    0,    0,   33,   33,    0,   33,   33,
   33,   33,   33,    0,    0,    0,    0,    0,    0,    0,
    0,  152,    0,  160,    0,  159,  154,    0,    0,    0,
  157,  155,    0,  156,    0,  158,    0,    0,    0,    0,
    0,    0,    0,    4,    5,    6,    0,    8,  151,    0,
  150,    9,   10,    0,    0,    0,   11,    0,   12,   13,
   14,   15,   16,   17,   18,    0,    0,    0,    0,  194,
  195,  196,   22,   23,   24,   25,    0,    0,    0,    0,
    0,    0,  153,    0,  161,  197,    0,    0,   31,   32,
   33,   34,   35,   36,   37,   38,   39,   40,   41,   42,
   43,   44,   45,   46,   47,   48,   49,    0,    0,    0,
    0,    0,  152,    0,  160,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   52,    0,    0,
   53,   54,   55,   56,    0,   57,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   92,   93,   94,   95,   96,   97,
   98,   99,    0,    0,  100,  101,  102,  103,  104,    0,
    0,  105,  106,  107,  108,  109,  110,  111,    0,    0,
  112,  113,  114,  115,  116,  117,  118,  119,  120,  121,
  122,  123,  124,  125,  126,  127,  128,  129,  130,  131,
  132,  133,  134,   35,   36,  135,   38,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  136,  137,
  138,  139,  140,  141,    0,  142,  143,    0,    0,  144,
    0,    0,    0,  145,  146,  147,  148,    0,    0,    0,
    0,    0,    0,    0,  149,   92,   93,   94,   95,   96,
   97,   98,   99,    0,    0,  100,  101,  102,  103,  104,
    0,    0,  105,  106,  107,  108,  109,  110,  111,    0,
    0,  112,  113,  114,  169,  170,  171,  172,  119,  120,
  121,  122,  123,  124,  125,  126,  127,  128,  129,  130,
  173,  174,  175,  134,  256,  257,  176,  258,  159,  154,
    0,    0,    0,  157,  155,    0,  156,    0,  158,  136,
  137,  138,  139,  140,  141,    0,  142,  143,    0,    0,
  144,  151,    0,  150,  145,  146,  147,  148,    0,    0,
    0,    0,    0,    0,    0,  149,  159,  154,    0,    0,
    0,  157,  155,    0,  156,    0,  158,    0,    0,    0,
    0,    0,    0,    0,    0,  153,    0,  161,    0,  151,
    0,  150,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  159,  154,    0,    0,    0,  157,  155,
    0,  156,    0,  158,    0,  152,    0,  160,    0,    0,
    0,    0,    0,  153,    0,  161,  151,    0,  150,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  159,  154,    0,    0,    0,  157,  155,    0,  156,    0,
  158,    0,    0,  152,    0,  160,    0,    0,    0,    0,
  153,    0,  161,  151,    0,  150,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  152,    0,  160,    0,    0,    0,    0,  153,    0,  161,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  152,    0,  160,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   92,   93,
   94,   95,   96,   97,   98,   99,    0,    0,  100,  101,
  102,  103,  104,    0,    0,  105,  106,  107,  108,  109,
  110,  111,    0,    0,  112,  113,  114,  169,  170,  171,
  172,  119,  120,  121,  122,  123,  124,  125,  126,  127,
  128,  129,  130,  173,  174,  175,  134,    0,    0,  176,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  136,  137,  138,  139,  140,  141,    0,  142,
  143,    0,    0,  144,    0,    0,    0,  145,  146,  147,
  148,    0,    0,  437,  438,    0,    0,  439,  149,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  136,  137,  138,  139,  140,  141,    0,  142,  143,    0,
    0,  144,    0,    0,    0,  145,  146,  147,  148,    0,
  443,  444,    0,    0,  445,    0,  149,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  136,  137,  138,
  139,  140,  141,    0,  142,  143,    0,    0,  144,    0,
    0,    0,  145,  146,  147,  148,    0,  452,  444,    0,
    0,  453,    0,  149,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  136,  137,  138,  139,  140,  141,
    0,  142,  143,    0,    0,  144,    0,    0,    0,  145,
  146,  147,  148,  159,  154,    0,    0,    0,  157,  155,
  149,  156,    0,  158,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  151,    0,  150,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  159,  154,    0,    0,    0,  157,  155,    0,  156,    0,
  158,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  153,    0,  161,  151,    0,  150,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  159,  154,    0,
    0,    0,  157,  155,    0,  156,    0,  158,    0,    0,
  152,    0,  160,    0,    0,    0,    0,  153,    0,  161,
  151,    0,  150,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  159,  154,    0,    0,    0,  157,
  155,    0,  156,    0,  158,    0,    0,  152,    0,  160,
    0,    0,    0,    0,  153,    0,  161,  151,    0,  150,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  159,  154,    0,    0,    0,  157,  155,    0,  156,
    0,  158,    0,    0,  152,    0,  160,    0,    0,    0,
    0,  153,    0,  161,  151,    0,  150,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  159,  154,
    0,    0,    0,  157,  155,    0,  156,    0,  158,    0,
    0,  152,    0,  160,    0,    0,    0,    0,  153,    0,
  161,  151,    0,  150,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  159,  154,    0,    0,    0,
  157,  155,    0,  156,    0,  158,    0,    0,  152,    0,
  160,    0,    0,    0,    0,  153,    0,  161,  151,    0,
  150,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  514,  438,    0,    0,  439,  152,    0,  160,    0,    0,
    0,    0,  153,    0,  161,    0,    0,  136,  137,  138,
  139,  140,  141,    0,  142,  143,    0,    0,  144,    0,
    0,    0,  145,  146,  147,  148,    0,  516,  444,    0,
    0,  517,  152,  149,  160,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  136,  137,  138,  139,  140,  141,
    0,  142,  143,  224,    0,  144,    0,    0,    0,  145,
  146,  147,  148,  224,  525,  438,    0,    0,  439,    0,
  149,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  136,  137,  138,  139,  140,  141,    0,  142,  143,
    0,    0,  144,    0,  224,    0,  145,  146,  147,  148,
    0,  526,  444,    0,    0,  527,    0,  149,    0,    0,
    0,    0,  224,    0,    0,    0,    0,    0,  136,  137,
  138,  139,  140,  141,    0,  142,  143,    0,    0,  144,
    0,    0,    0,  145,  146,  147,  148,    0,  554,  438,
    0,    0,  439,  223,  149,    0,    0,    0,    0,    0,
    0,    0,    0,  223,    0,  136,  137,  138,  139,  140,
  141,    0,  142,  143,    0,    0,  144,    0,    0,    0,
  145,  146,  147,  148,    0,  555,  444,    0,  224,  556,
    0,  149,    0,    0,  223,    0,    0,    0,    0,    0,
    0,    0,  136,  137,  138,  139,  140,  141,    0,  142,
  143,    0,  223,  144,    0,    0,    0,  145,  146,  147,
  148,    0,  719,  438,    0,    0,  439,    0,  149,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  136,
  137,  138,  139,  140,  141,    0,  142,  143,    0,    0,
  144,    0,    0,    0,  145,  146,  147,  148,  159,  154,
    0,    0,    0,  157,  155,  149,  156,    0,  158,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  223,    0,
    0,  151,    0,  150,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  219,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  219,    0,    0,  153,    0,  161,    0,    0,
    0,    0,    0,    0,    0,  224,  224,  224,    0,    0,
  224,  224,  224,    0,  224,    0,    0,    0,    0,    0,
    0,    0,    0,  219,  224,  152,  219,  160,    0,    0,
    0,    0,    0,  224,  224,    0,  224,  224,  224,  224,
  224,  219,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  219,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  223,  223,  223,    0,    0,
  223,  223,  223,    0,  223,    0,    0,  219,    0,    0,
    0,    0,    0,    0,  223,    0,    0,    0,    0,    0,
    0,    0,    0,  223,  223,    0,  223,  223,  223,  223,
  223,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  644,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  720,  444,    0,    0,  721,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  136,  137,  138,  139,  140,  141,    0,  142,
  143,    0,    0,  144,    0,    0,    0,  145,  146,  147,
  148,    0,    0,    0,  219,  219,  219,    0,  149,  219,
  219,  219,    0,  219,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  219,  219,    0,    0,    0,    0,    0,
    0,    0,  219,  219,    0,  219,  219,  219,  219,  219,
    4,    5,    6,    0,    8,    0,    0,    0,    9,   10,
    0,    0,    0,   11,    0,   12,   13,   14,   15,   16,
   17,   18,    0,    0,    0,    0,  194,  195,  196,   22,
   23,   24,   25,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  197,    0,    0,   31,   32,   33,   34,   35,
   36,   37,   38,   39,   40,   41,   42,   43,   44,   45,
   46,   47,   48,   49,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   52,    0,    0,   53,   54,   55,
   56,    0,   57,    4,    5,    6,    0,    8,    0,    0,
    0,    9,   10,    0,    0,    0,   11,    0,   12,   13,
   14,   15,   16,   17,   18,    0,    0,    0,    0,  194,
  195,  196,   22,   23,   24,   25,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  197,    0,    0,   31,   32,
   33,   34,   35,   36,   37,   38,   39,   40,   41,   42,
   43,   44,   45,   46,   47,   48,   49,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   52,    0,    0,
  314,   54,   55,  315,    0,   57,    4,    5,    6,    0,
    8,    0,    0,    0,    9,   10,    0,    0,    0,   11,
    0,   12,   13,   14,   15,   16,   17,   18,    0,    0,
    0,    0,  194,  195,  196,   22,   23,   24,   25,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  197,    0,
    0,   31,   32,   33,   34,   35,   36,   37,   38,   39,
   40,   41,   42,   43,   44,   45,   46,   47,   48,   49,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   52,    0,    0,  205,   54,   55,    0,    0,   57,
  };
  protected static  short [] yyCheck = {             2,
    3,   46,   46,   19,   20,    8,    9,   10,   11,    6,
  217,   14,   27,  185,    7,   10,   19,   20,  335,   53,
  343,  197,   10,   26,   46,   21,  303,  223,    6,    7,
  348,  123,   56,   10,   27,   37,   54,   55,  301,  302,
   42,   43,  595,   45,   10,   47,  309,  310,  124,   27,
   53,   54,   11,   56,   40,   58,   44,   10,  595,  332,
   57,   10,   46,  600,   59,  362,   10,  243,  365,   91,
  365,   59,   10,   54,   46,  352,   79,   58,   10,   57,
   85,   10,   59,  696,  361,   64,   89,   10,  351,   85,
   37,   44,   46,   59,   10,   42,  359,  360,  304,  394,
   47,    3,   10,   41,   10,   64,   59,   91,   46,   46,
   10,   40,   46,    5,   46,   59,   10,   46,   41,   91,
  733,  384,  238,  123,   10,  402,    2,    3,   41,  282,
   59,   90,    8,    9,   10,   11,   59,   91,  401,   40,
   93,  347,  348,   19,   20,  698,   46,  354,   61,   10,
   26,   59,   46,   59,   91,   41,  342,   91,   44,   59,
   52,  698,   91,   41,   61,   59,   10,   10,   10,    9,
   10,   11,  125,   59,   14,   40,   10,   53,   54,   44,
   10,   46,   58,   44,   10,   10,   26,  504,  280,  192,
  193,   91,   61,  209,  123,  371,  372,   91,  314,   41,
  196,   10,  205,   44,  238,  221,  209,  223,  204,  188,
  226,  484,  388,   89,   44,   59,   59,  125,  221,   44,
  223,   93,  123,  226,  521,   59,   91,   10,  342,  188,
  233,   10,   10,   59,   10,  238,   44,   46,   37,   38,
   10,  538,    0,   42,   43,   10,   45,  304,   47,   89,
   59,  308,   10,  125,   41,  331,   10,   40,  123,  124,
  290,  291,   41,   46,  267,  268,  269,  270,  271,  272,
  273,  274,  267,  262,   61,   10,   59,  540,  541,  267,
  314,   59,   91,   59,   10,  303,   40,    2,   61,   59,
  267,  315,   46,    8,   59,  340,  340,   61,  264,  475,
  303,   59,  268,  269,  622,   59,   41,  271,   91,  312,
  313,  314,  315,   40,  267,   41,  192,  193,  340,  263,
  264,  323,  303,  301,  327,  328,  533,   59,  625,  205,
  625,  312,  313,  209,  352,  338,  508,  124,  267,  342,
  123,  658,  538,  361,  362,  221,  362,  223,  343,  352,
  226,  280,  192,  193,  677,  343,  340,  233,  361,  362,
  677,  267,  238,  378,  323,  267,  343,  267,  340,   10,
  272,  352,  350,  267,  290,  291,  323,  553,  381,  280,
  361,  359,  655,   40,  402,  378,  340,  390,  347,  348,
  343,  267,  268,  269,  270,  271,  272,  273,  274,  402,
  378,  312,  340,  340,  315,  383,  340,   40,  340,  390,
  727,  340,  290,  291,  279,  280,  592,  624,   59,   10,
  627,  402,  400,  267,  267,   10,  703,  303,  268,  269,
  270,  271,  604,  273,  274,  440,  312,  313,  314,  702,
  340,  446,    2,    3,  440,   20,  340,   10,    8,  454,
  455,  327,  328,  629,   14,  281,  290,  291,  267,   10,
  636,   10,  338,  279,  290,  291,  342,   10,   59,  472,
  473,  269,  593,  271,   59,  340,  352,   40,  493,  476,
   10,   40,  485,   46,  267,  361,  362,   41,  328,  267,
   44,  267,  279,   53,  264,  498,   59,  280,  476,  269,
  493,  673,  267,  263,  264,  381,  713,   61,   59,  269,
   59,  470,  290,  291,  390,  493,   59,  520,  233,  515,
  538,  246,  538,  248,  323,  250,  402,  486,   91,   59,
   60,  340,  304,  454,  455,  538,  308,    2,    3,  338,
  339,    6,  545,    8,    9,   10,   11,  268,  269,   14,
  304,  306,  306,  307,  308,  309,   44,  340,   46,  223,
  123,   26,  226,  318,  319,   44,  338,  570,  571,   44,
  573,  574,   41,  263,  264,  450,  451,  304,  123,  306,
  307,  308,  309,  704,  323,   41,  340,   41,   53,   93,
  125,   56,   61,  347,  348,  716,  472,  473,   40,   41,
  603,   44,   44,   91,   46,  608,   41,  269,  611,  485,
   41,  267,  327,  264,   79,  618,  619,  620,   41,   61,
  347,  348,  498,  338,   89,  584,   61,  342,   41,   44,
   61,   41,  472,  125,  209,   41,  124,  342,  342,   44,
  342,   93,   44,   10,  520,  205,  221,   41,  651,   91,
  264,  304,    2,    3,   61,  124,   61,  304,    8,    9,
   10,   11,  538,  622,   14,   44,  381,  670,   44,  545,
    9,   10,   11,  233,   41,   44,   26,   44,  238,  264,
  520,  123,  124,  343,  269,  703,  271,   26,   10,  124,
   44,  694,   59,  124,  263,  571,  264,  573,  574,   44,
  703,   93,  705,   53,  267,  545,   56,  267,  348,  712,
  631,  632,  272,  262,  263,  264,   93,  280,   40,  124,
  269,  264,  703,  701,   46,   44,  256,  192,  193,   79,
  570,   10,  608,   41,  490,  611,  492,   59,   41,   89,
  205,  125,  618,  619,  620,   44,   93,   46,  342,  264,
   89,   37,   19,   20,  314,  348,   42,   43,  473,   45,
   41,   47,   41,  264,  264,   44,  264,  327,  233,  124,
  485,  611,  125,  238,  264,  651,   41,  340,  338,  264,
   59,   44,  342,  498,  264,   41,  264,  264,   55,   93,
  264,  279,   91,  264,  670,   40,   41,  264,  271,   44,
   93,   46,  267,  268,  269,  270,  271,  272,  273,  274,
  279,  125,  271,   40,   37,   38,   61,   41,  694,   42,
   43,  381,   45,   93,   47,  124,   41,  703,  125,  705,
  293,  294,  295,  296,  297,   93,  712,  279,  280,   93,
  125,  724,  192,  193,  279,  264,   91,    6,  279,  314,
  315,  312,  340,  192,  193,  205,  571,  484,  573,  574,
  655,  570,  327,  328,  600,   71,   79,  600,   64,   -1,
   -1,   94,   -1,  338,  279,   -1,   -1,  342,  123,  124,
   -1,   -1,   -1,  233,   -1,   -1,   -1,   -1,  238,   -1,
   40,   41,   -1,  608,   44,   -1,   46,   -1,  340,   -1,
   -1,  124,   -1,  618,  619,  620,  662,  663,  664,   -1,
   -1,   61,   -1,  473,   -1,   -1,  381,  267,  268,  269,
  270,  271,  272,  273,  274,  485,   -1,   -1,   -1,  268,
  269,  270,  271,   -1,  273,  274,  651,   -1,  498,   -1,
   -1,   91,  209,   -1,   40,   -1,   -1,   -1,   44,   -1,
   46,   -1,   -1,   -1,  221,  670,  223,   -1,   -1,  226,
   -1,   -1,  718,   -1,  314,  315,   -1,   -1,   10,   -1,
   -1,   -1,   -1,  123,  124,   -1,   -1,  327,  328,  694,
  279,   -1,  304,   -1,  306,  307,  308,  309,  338,  328,
  705,   -1,  342,   10,   -1,   91,   -1,  712,   40,   -1,
   -1,   -1,   -1,   -1,   46,   -1,   -1,  472,  473,   -1,
  570,  571,   -1,  573,  574,   -1,   -1,   59,  340,   -1,
  485,   -1,   -1,   40,   -1,  347,  348,  123,  124,   46,
   -1,  381,   -1,  498,  279,  280,   -1,  323,   44,   -1,
   46,  340,   59,  603,   -1,  312,  313,   -1,  608,   -1,
   -1,   -1,  338,  339,   -1,  520,   -1,   -1,  618,  619,
  620,   -1,   -1,   -1,   -1,   -1,   -1,  334,  304,   -1,
  306,  307,  308,  309,  341,   -1,   -1,  344,   -1,   -1,
  545,   -1,   -1,    0,   -1,   91,   -1,   -1,   -1,   -1,
   -1,  651,   -1,   10,   -1,  340,   -1,   -1,   -1,   -1,
  323,   -1,   -1,   -1,   -1,  570,  571,   -1,  573,  574,
  670,  347,  348,   -1,   10,  338,  339,   -1,  124,   -1,
   -1,   -1,  472,  473,   41,   19,   20,   44,   -1,  279,
  280,   -1,   -1,  472,  694,  485,   10,   -1,  603,   -1,
   -1,   58,   59,  608,   40,  705,  611,   -1,  498,   -1,
   46,   -1,  712,  618,  619,  620,   50,   51,   -1,   -1,
   54,   55,   -1,   59,   58,   59,   40,   -1,   -1,   -1,
  520,   -1,   46,   -1,   -1,   -1,   93,   -1,   -1,   -1,
   -1,  520,   -1,  279,  280,   59,  651,   -1,   -1,   -1,
  340,   -1,    0,   -1,   -1,  545,   -1,   -1,   -1,   -1,
   -1,   -1,   10,   -1,   -1,  670,  545,   -1,  125,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,    0,   -1,   -1,   -1,
  570,  571,   -1,  573,  574,   -1,   10,   -1,   -1,  694,
   -1,   -1,   -1,   41,   -1,   -1,   -1,   -1,   -1,   -1,
  705,   -1,   -1,   -1,  340,   -1,   -1,  712,   -1,   -1,
   -1,   59,   -1,  603,   38,   -1,   -1,   41,  608,   -1,
   44,  611,  304,   -1,  306,  307,  308,  309,  618,  619,
  620,   -1,  611,  279,   58,   59,   60,   -1,   62,   63,
   54,   55,   -1,   41,   -1,   -1,   44,  304,   46,  306,
  307,  308,  309,   -1,   -1,   -1,   -1,   -1,  340,   -1,
   -1,  651,   -1,   61,   -1,  347,  348,   -1,   -1,   93,
   94,   -1,  206,  207,  208,  209,   -1,  125,   -1,   -1,
  670,   -1,   -1,  340,   -1,   -1,   -1,  221,   -1,  223,
  347,  348,  226,   91,  340,   -1,   -1,   -1,   -1,   -1,
  124,  125,   -1,   -1,  694,  262,  263,  264,  242,   -1,
  267,  268,  269,   -1,  271,  705,   -1,   -1,   -1,   -1,
   -1,   -1,  712,   -1,  281,  282,  124,   -1,   -1,   -1,
   -1,   -1,   -1,  290,  291,   -1,  293,  294,  295,  296,
  297,  275,  276,  277,  278,  279,  280,  281,  282,  283,
  284,  285,  286,  287,  288,  289,  290,  291,  292,  293,
  294,  295,  296,  297,  298,  299,  300,  181,  304,  303,
  306,  307,  308,  309,   -1,   -1,   -1,  311,  312,  313,
   -1,   -1,   -1,   -1,   -1,   -1,  343,   -1,   -1,   -1,
  304,   -1,  306,  307,  308,  309,   -1,   -1,   -1,  213,
  334,   -1,   -1,  217,  340,   -1,  340,  341,   -1,  223,
  344,  347,  348,   40,  262,  263,  264,   44,  352,   46,
  268,  269,   -1,  271,  358,   -1,  340,  361,  362,   -1,
   -1,  365,   -1,  347,  348,   -1,   -1,   -1,  262,  263,
  264,   -1,   -1,  267,  268,  269,  380,  271,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  390,  281,  282,   -1,
  394,   -1,  396,   -1,   91,   -1,  290,  291,  402,  293,
  294,  295,  296,  297,   -1,   -1,   -1,   -1,   -1,   41,
   -1,  279,   44,   -1,   46,   -1,   -1,   -1,   -1,  303,
   -1,   -1,   10,   -1,   -1,   -1,  123,  124,   -1,   61,
  324,  325,  326,  327,  328,  329,  330,  331,  332,  333,
  334,  335,   -1,   41,  338,  339,   44,   -1,   46,  343,
  334,  335,   40,  457,   -1,   -1,   -1,   -1,   46,   91,
   -1,   -1,   -1,   61,   -1,  469,   -1,   -1,  352,   -1,
  354,   59,  340,   -1,   -1,   -1,   -1,  361,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  370,    0,   -1,   -1,
   -1,   -1,  124,   91,   -1,   -1,   -1,   10,   -1,   -1,
   -1,   -1,  506,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  402,   -1,
   -1,   -1,   -1,   -1,   -1,  529,  124,   -1,   41,   -1,
  534,   -1,   -1,   -1,  538,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   59,  551,   10,   -1,
   -1,   -1,   -1,   -1,  558,  559,   -1,   -1,  562,  563,
   -1,   -1,  446,   -1,   -1,  449,  450,  451,   -1,   -1,
  454,  455,   -1,   -1,   -1,   -1,  580,   -1,   40,   -1,
   -1,   -1,  279,  280,   46,  589,   -1,   -1,   -1,    0,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   59,   -1,   10,
   -1,   -1,  606,  487,   -1,   -1,  490,   -1,  492,   -1,
   -1,  495,  125,  617,   -1,  499,   -1,  501,   -1,   -1,
  504,  625,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   41,   -1,   -1,   -1,  518,   -1,   -1,   -1,   -1,  643,
   -1,   -1,    0,  340,   -1,  649,   -1,  279,   59,  533,
   -1,   -1,   10,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  676,   -1,   -1,   33,   -1,   -1,  682,   37,
   38,  279,   40,   41,   42,   43,   44,   45,   46,   47,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  703,
   58,   59,   60,   61,   62,   63,   -1,   -1,  340,  593,
   -1,   -1,   -1,   -1,  125,   -1,  304,   -1,  306,  307,
  308,  309,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   91,   -1,   93,   94,   -1,   -1,   -1,
  624,   -1,  340,  627,    0,   -1,   -1,  631,  632,  262,
  263,  264,  340,   -1,   10,  268,  269,   -1,  271,  347,
  348,   -1,   -1,   -1,   -1,  123,  124,  125,  126,   -1,
   -1,   -1,   -1,   -1,  658,   -1,   -1,   33,  662,  663,
  664,   37,   38,   -1,   40,   41,   42,   43,   44,   45,
   46,   47,   -1,  677,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   58,   59,   60,   61,   62,   63,   -1,   -1,
   -1,   -1,  696,   -1,   -1,   -1,   -1,   -1,   -1,  703,
  704,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  713,
   -1,   -1,  716,   -1,  718,   91,   -1,   93,   94,   -1,
  724,   -1,  304,  727,  306,  307,  308,  309,   -1,  733,
   -1,  262,  263,  264,    0,   -1,   -1,  268,  269,   -1,
  271,   -1,   -1,   -1,   -1,   -1,   -1,  123,  124,  125,
  126,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  340,   -1,
   -1,   -1,   -1,   -1,   -1,  347,  348,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   41,   -1,   -1,   -1,  257,
  258,  259,   -1,  261,  262,  263,  264,  265,  266,  267,
  268,  269,  270,  271,  272,  273,  274,  275,  276,  277,
  278,  279,  280,  281,  282,  283,  284,  285,  286,  287,
  288,  289,  290,  291,   -1,  293,  294,  295,  296,  297,
   -1,  299,   -1,   -1,  302,  303,  304,  305,  306,  307,
  308,  309,  310,  311,  312,  313,  314,  315,  316,  317,
  318,  319,  320,  321,  322,  323,  324,  325,  326,  327,
  328,  329,  330,  331,  332,  333,  334,  335,   -1,  125,
  338,  339,  340,  341,  342,  343,  344,  345,  346,  347,
  348,  349,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  257,  258,  259,   -1,  261,  262,  263,  264,  265,
  266,  267,  268,  269,  270,  271,  272,  273,  274,  275,
  276,  277,  278,  279,  280,  281,  282,  283,  284,  285,
  286,  287,  288,  289,  290,  291,   -1,  293,  294,  295,
  296,  297,   -1,  299,   -1,   -1,  302,  303,  304,  305,
  306,  307,  308,  309,  310,  311,  312,  313,  314,  315,
  316,  317,  318,  319,  320,  321,  322,  323,  324,  325,
  326,  327,  328,  329,  330,  331,  332,  333,  334,  335,
   -1,   -1,  338,  339,  340,  341,  342,  343,  344,  345,
  346,  347,  348,  349,    0,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   10,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  262,  263,  264,   -1,
   -1,   -1,  268,  269,   -1,  271,   -1,   33,   -1,   -1,
   -1,   37,   38,   -1,   40,   41,   42,   43,   44,   45,
   46,   47,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   58,   59,   60,   61,   62,   63,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   91,   -1,   93,   94,    0,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   10,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  123,  124,  125,
  126,   -1,   33,   -1,   -1,   -1,   37,   38,   -1,   40,
   41,   42,   43,   44,   45,   46,   47,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   58,   59,   60,
   61,   62,   63,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   37,   38,   -1,   -1,   -1,   42,   43,   -1,   45,
   -1,   47,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   91,   -1,   93,   94,   60,   -1,   62,   63,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  123,  124,  125,  126,   -1,   -1,   94,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  124,   -1,
   -1,  257,  258,  259,   -1,  261,  262,  263,  264,  265,
  266,  267,  268,  269,  270,  271,  272,  273,  274,  275,
  276,  277,  278,   -1,  280,  281,  282,  283,  284,  285,
  286,  287,  288,  289,  290,  291,   -1,  293,  294,  295,
  296,  297,   -1,  299,   -1,   -1,  302,  303,  304,  305,
  306,  307,  308,  309,  310,  311,  312,  313,  314,  315,
  316,  317,  318,  319,  320,  321,  322,  323,  324,  325,
  326,  327,  328,  329,  330,  331,  332,  333,  334,  335,
   -1,   -1,  338,  339,  340,  341,   -1,  343,  344,  345,
  346,  347,  348,  349,   -1,   -1,  257,  258,  259,   -1,
  261,  262,  263,  264,  265,  266,  267,  268,  269,  270,
  271,  272,  273,  274,  275,  276,  277,  278,   -1,  280,
  281,  282,  283,  284,  285,  286,  287,  288,  289,  290,
  291,   -1,  293,  294,  295,  296,  297,   -1,  299,   -1,
   -1,  302,  303,  304,  305,  306,  307,  308,  309,  310,
  311,  312,  313,  314,  315,  316,  317,  318,  319,  320,
  321,  322,  323,  324,  325,  326,  327,  328,  329,  330,
  331,  332,  333,  334,  335,   -1,   -1,  338,  339,  340,
  341,    0,  343,  344,  345,  346,  347,  348,  349,   -1,
   -1,   10,   -1,   -1,   -1,   -1,   -1,  323,  324,  325,
  326,  327,  328,  329,  330,  331,  332,  333,  334,  335,
   -1,   -1,  338,  339,   33,   -1,   -1,  343,   37,   38,
   -1,   40,   41,   42,   43,   44,   45,   46,   47,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   58,
   59,   60,   61,   62,   63,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   91,   -1,   93,   94,    0,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   10,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  123,  124,  125,  126,   -1,   33,
   -1,   -1,   -1,   37,   38,   -1,   40,   41,   42,   43,
   44,   45,   46,   47,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   58,   59,   60,   -1,   62,   63,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   37,   38,
   -1,   -1,   -1,   42,   43,   44,   45,   -1,   47,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   91,   -1,   93,
   94,   60,   -1,   62,   63,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  123,
  124,  125,  126,   -1,   -1,   94,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  124,   -1,   -1,  257,  258,
  259,   -1,  261,  262,  263,  264,  265,  266,  267,  268,
  269,  270,  271,  272,  273,  274,  275,  276,  277,  278,
   -1,  280,  281,  282,  283,  284,  285,  286,  287,  288,
  289,  290,  291,   -1,  293,  294,  295,  296,  297,   -1,
  299,   -1,   -1,  302,  303,  304,  305,  306,  307,  308,
  309,  310,  311,  312,  313,  314,  315,  316,  317,  318,
  319,  320,  321,  322,  323,  324,  325,  326,  327,  328,
  329,  330,  331,  332,  333,  334,  335,   -1,   -1,  338,
  339,  340,  341,   -1,  343,  344,  345,  346,  347,  348,
  349,   -1,   -1,  257,  258,  259,   -1,  261,  262,  263,
  264,  265,  266,  267,  268,  269,  270,  271,  272,  273,
  274,  275,  276,  277,  278,   -1,  280,  281,  282,  283,
  284,  285,  286,  287,  288,  289,  290,  291,   -1,  293,
  294,  295,  296,  297,   -1,  299,   -1,   -1,  302,  303,
  304,  305,  306,  307,  308,  309,  310,  311,  312,  313,
  314,  315,  316,  317,  318,  319,  320,  321,  322,  323,
  324,  325,  326,  327,  328,  329,  330,  331,  332,  333,
  334,  335,   -1,   -1,  338,  339,  340,  341,    0,  343,
  344,  345,  346,  347,  348,  349,   -1,   -1,   10,   -1,
   -1,   -1,   -1,   -1,  323,  324,  325,  326,  327,  328,
  329,  330,  331,  332,  333,  334,  335,   -1,   -1,  338,
  339,   33,   -1,   -1,   -1,   37,   38,   -1,   40,   41,
   42,   43,   44,   45,   46,   47,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   58,   59,   60,   -1,
   62,   63,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   91,
   -1,   93,   94,    0,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   10,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  123,  124,  125,  126,   -1,   33,   -1,   -1,   -1,
   37,   38,   -1,   40,   41,   42,   43,   44,   45,   46,
   47,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   58,   59,   60,   -1,   62,   63,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   37,   38,   -1,   -1,   -1,
   42,   43,   -1,   45,   -1,   47,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   91,   -1,   93,   94,   60,   -1,
   62,   63,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  123,  124,  125,  126,
   -1,   -1,   94,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  124,   -1,   -1,  257,  258,  259,   -1,  261,
  262,  263,  264,  265,  266,  267,  268,  269,  270,  271,
  272,  273,  274,  275,  276,  277,  278,   -1,  280,  281,
  282,  283,  284,  285,  286,  287,  288,  289,  290,  291,
   -1,  293,  294,  295,  296,  297,   -1,  299,   -1,   -1,
  302,  303,  304,  305,  306,  307,  308,  309,  310,  311,
  312,  313,  314,  315,  316,  317,  318,  319,  320,  321,
  322,  323,  324,  325,  326,  327,  328,  329,  330,  331,
  332,  333,  334,  335,   -1,   -1,  338,  339,  340,  341,
   -1,  343,  344,  345,  346,  347,  348,  349,   -1,   -1,
  257,  258,  259,   -1,  261,  262,  263,  264,  265,  266,
  267,  268,  269,  270,  271,  272,  273,  274,  275,  276,
  277,  278,   -1,  280,  281,  282,  283,  284,  285,  286,
  287,  288,  289,  290,  291,   -1,  293,  294,  295,  296,
  297,   -1,  299,   -1,   -1,  302,  303,  304,  305,  306,
  307,  308,  309,  310,  311,  312,  313,  314,  315,  316,
  317,  318,  319,  320,  321,  322,  323,  324,  325,  326,
  327,  328,  329,  330,  331,  332,  333,  334,  335,   -1,
   -1,  338,  339,  340,  341,    0,  343,  344,  345,  346,
  347,  348,  349,   -1,   -1,   10,   -1,   -1,   -1,   -1,
   -1,  323,  324,  325,  326,  327,  328,  329,  330,  331,
  332,  333,  334,  335,   -1,   -1,  338,  339,   33,   -1,
   -1,   -1,   37,   38,   -1,   -1,   41,   42,   43,   44,
   45,   46,   47,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   59,   60,   -1,   62,   63,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   91,   -1,   93,   94,
    0,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   10,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  123,  124,
  125,  126,   -1,   33,   -1,   -1,   -1,   37,   38,   -1,
   40,   41,   42,   43,   44,   45,   46,   47,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   58,   59,
   60,   -1,   62,   63,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   37,   38,   -1,   -1,   -1,   42,   43,   -1,
   45,   -1,   47,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   91,   -1,   93,   94,   60,   -1,   62,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  124,  125,  126,   -1,   -1,   94,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  124,
   -1,   -1,  257,  258,  259,   -1,  261,  262,  263,  264,
  265,  266,  267,  268,  269,  270,  271,  272,  273,  274,
  275,  276,  277,  278,   -1,  280,  281,  282,  283,  284,
  285,  286,  287,  288,  289,  290,  291,   -1,  293,  294,
  295,  296,  297,   -1,  299,   -1,   -1,  302,  303,  304,
  305,  306,  307,  308,  309,  310,  311,  312,  313,  314,
  315,  316,  317,  318,  319,  320,  321,  322,  323,  324,
  325,  326,  327,  328,  329,  330,  331,  332,  333,  334,
  335,   -1,   -1,  338,  339,  340,  341,   -1,  343,  344,
  345,  346,  347,  348,  349,   -1,   -1,  257,  258,  259,
   -1,  261,  262,  263,  264,  265,  266,  267,  268,  269,
  270,  271,  272,  273,  274,  275,  276,  277,  278,   -1,
   -1,  281,  282,  283,  284,  285,  286,  287,  288,  289,
  290,  291,   -1,  293,  294,  295,  296,  297,   -1,  299,
   -1,   -1,  302,  303,  304,  305,  306,  307,  308,  309,
  310,  311,  312,  313,  314,  315,  316,  317,  318,  319,
  320,  321,  322,  323,  324,  325,  326,  327,  328,  329,
  330,  331,  332,  333,  334,  335,   -1,   -1,  338,  339,
  340,  341,    0,  343,  344,  345,  346,  347,  348,  349,
   -1,   -1,   10,   -1,   -1,   -1,   -1,   -1,  323,  324,
  325,  326,  327,  328,  329,  330,  331,  332,  333,   -1,
   -1,   -1,   -1,  338,  339,   33,   -1,   -1,   -1,   37,
   38,   -1,   -1,   41,   42,   43,   44,   45,   46,   47,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   59,   60,   -1,   62,   63,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   91,   -1,   93,   94,    0,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   10,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  123,  124,  125,  126,   -1,
   33,   -1,   -1,   -1,   37,   38,   -1,   40,   41,   42,
   43,   44,   45,   46,   47,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   59,   60,   61,   62,
   63,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   37,
   38,   -1,   -1,   -1,   42,   43,   -1,   45,   -1,   47,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   91,   -1,
   58,   94,   60,   -1,   62,   63,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  123,  124,  125,  126,   -1,   -1,   94,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  124,   -1,   -1,  257,
  258,  259,   -1,  261,  262,  263,  264,  265,  266,  267,
  268,  269,  270,  271,  272,  273,  274,  275,  276,  277,
  278,   -1,  280,  281,  282,  283,  284,  285,  286,  287,
  288,  289,  290,  291,   -1,  293,  294,  295,  296,  297,
   -1,  299,   -1,   -1,  302,  303,  304,  305,  306,  307,
  308,  309,  310,  311,  312,  313,  314,  315,  316,  317,
  318,  319,  320,  321,  322,  323,  324,  325,  326,  327,
  328,  329,  330,  331,  332,  333,  334,  335,   -1,   -1,
  338,  339,  340,  341,   -1,  343,  344,  345,  346,  347,
  348,  349,   -1,   -1,  257,  258,  259,   -1,  261,  262,
  263,  264,  265,  266,  267,  268,  269,  270,  271,  272,
  273,  274,  275,  276,  277,  278,   -1,  280,  281,   -1,
  283,  284,  285,  286,  287,  288,  289,  290,  291,   -1,
  293,  294,  295,  296,  297,   -1,  299,   -1,   -1,  302,
  303,  304,  305,  306,  307,  308,  309,  310,  311,  312,
  313,  314,  315,  316,  317,  318,  319,  320,  321,  322,
  323,  324,  325,  326,  327,  328,  329,  330,  331,  332,
  333,  334,  335,   -1,   -1,  338,  339,  340,  341,    0,
   -1,  344,  345,  346,  347,  348,  349,   -1,   -1,   10,
   -1,   -1,   -1,   -1,   -1,  323,  324,  325,  326,  327,
  328,  329,  330,  331,  332,  333,  334,  335,   -1,   -1,
  338,  339,   33,   -1,   -1,   -1,   37,   38,   -1,   40,
   41,   42,   43,   44,   45,   46,   47,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   59,   60,
   61,   62,   63,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   91,   -1,   -1,   94,    0,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   10,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  123,  124,  125,  126,   -1,   33,   -1,   -1,
   -1,   37,   38,   -1,   40,   41,   42,   43,   44,   45,
   46,   47,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   59,   60,   61,   62,   63,   -1,   -1,
   -1,   -1,   -1,   -1,   37,   38,   -1,   -1,   -1,   42,
   43,   -1,   45,   -1,   47,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   91,   -1,   60,   94,   62,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  123,  124,  125,
  126,   94,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  124,   -1,   -1,   -1,   -1,  257,  258,  259,   -1,
  261,  262,  263,  264,  265,  266,  267,  268,  269,  270,
  271,  272,  273,  274,  275,  276,  277,  278,   -1,  280,
  281,   -1,  283,  284,  285,  286,  287,  288,  289,  290,
  291,   -1,  293,  294,  295,  296,  297,   -1,  299,   -1,
   -1,  302,  303,  304,  305,  306,  307,  308,  309,  310,
  311,  312,  313,  314,  315,  316,  317,  318,  319,  320,
  321,  322,  323,  324,  325,  326,  327,  328,  329,  330,
  331,  332,  333,  334,  335,   -1,   -1,  338,  339,  340,
  341,   -1,   -1,  344,  345,  346,  347,  348,  349,   -1,
   -1,  257,  258,  259,   -1,  261,  262,  263,  264,  265,
  266,  267,  268,  269,  270,  271,  272,  273,  274,  275,
  276,  277,  278,   -1,  280,  281,   -1,  283,  284,  285,
  286,  287,  288,  289,  290,  291,   -1,  293,  294,  295,
  296,  297,   -1,  299,   -1,   -1,  302,  303,  304,  305,
  306,  307,  308,  309,  310,  311,  312,  313,  314,  315,
  316,  317,  318,  319,  320,  321,  322,  323,  324,  325,
  326,  327,  328,  329,  330,  331,  332,  333,  334,  335,
   -1,   -1,  338,  339,  340,  341,    0,   -1,  344,  345,
  346,  347,  348,  349,   -1,   -1,   10,   -1,   -1,   -1,
  323,  324,  325,  326,  327,  328,  329,  330,   -1,  332,
  333,   -1,   -1,   -1,   -1,  338,  339,   -1,   -1,   33,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   41,   -1,   -1,
   -1,   -1,   46,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   59,   -1,    0,   -1,   -1,
   37,   38,   -1,   -1,   -1,   42,   43,   -1,   45,   -1,
   47,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   60,   -1,   62,    0,   -1,   -1,   93,
   -1,   -1,   -1,   -1,   -1,   -1,   10,   -1,   41,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   94,   -1,   -1,
   -1,  125,  126,   37,   38,   -1,   -1,   41,   42,   43,
   44,   45,   46,   47,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   58,   59,   60,  124,   62,   63,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   91,   -1,   93,
   94,   -1,  125,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,    0,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   10,   -1,  123,
  124,  125,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,    0,   -1,   -1,   41,   -1,
   -1,   44,   -1,   -1,   -1,   10,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  257,  258,  259,   59,  261,  262,  263,
  264,  265,  266,  267,  268,  269,  270,  271,  272,  273,
  274,  275,  276,  277,  278,   -1,   41,  281,   -1,  283,
  284,  285,  286,  287,  288,  289,  290,  291,   -1,  293,
  294,  295,  296,  297,   59,  299,   -1,   -1,  302,  303,
  304,  305,  306,  307,  308,  309,  310,  311,  312,  313,
  314,  315,  316,  317,  318,  319,  320,  321,  322,  262,
  263,  264,  125,   -1,   -1,  268,  269,   -1,  271,    0,
   -1,   -1,   -1,   -1,   -1,   -1,  340,  341,   -1,   10,
  344,  345,  346,  347,  348,  349,  323,   -1,  262,  263,
  264,  328,  329,  267,  268,  269,   -1,  271,   -1,   -1,
  125,  338,  339,   -1,   -1,   -1,  280,  281,  282,   -1,
   41,   -1,   -1,   44,   -1,   -1,  290,  291,   -1,  293,
  294,  295,  296,  297,   -1,   -1,   -1,   -1,   59,   -1,
    0,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   10,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  323,
  324,  325,  326,  327,  328,  329,  330,  331,  332,  333,
  334,  335,   -1,   -1,  338,  339,  340,   37,   38,  343,
   -1,   41,   42,   43,   44,   45,   46,   47,   -1,   -1,
   -1,    0,   -1,   -1,   -1,   -1,   -1,   -1,   58,   59,
   60,   10,   62,   63,  125,   -1,   -1,   -1,   -1,  262,
  263,  264,   -1,   -1,  267,  268,  269,   -1,  271,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  281,    0,
   -1,   91,   41,   93,   94,   -1,   -1,  290,  291,   10,
  293,  294,  295,  296,  297,   -1,   -1,  262,  263,  264,
   59,   -1,   -1,  268,  269,   -1,  271,   -1,   -1,   -1,
   -1,   -1,   -1,  123,  124,  125,   37,   38,   -1,   -1,
   41,   42,   43,   44,   45,   46,   47,   -1,  293,  294,
  295,  296,  297,   -1,   -1,   -1,   -1,   58,   59,   60,
   61,   62,   63,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  125,   -1,   -1,    0,
   91,   -1,   93,   94,   -1,   -1,   -1,   -1,   -1,   10,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  262,  263,  264,   -1,   -1,   -1,  268,  269,   -1,
  271,   -1,   -1,  124,  125,   -1,   37,   38,   -1,   -1,
   41,   42,   43,   44,   45,   46,   47,   -1,   -1,  290,
  291,   -1,  293,  294,  295,  296,  297,   58,   59,   60,
   61,   62,   63,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  262,  263,  264,   -1,   -1,  267,  268,  269,
   91,  271,   93,   94,   -1,   -1,   -1,   -1,   -1,   -1,
  280,  281,  282,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  290,  291,   -1,  293,  294,  295,  296,  297,   -1,   -1,
   -1,   -1,   -1,  124,  125,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  262,  263,  264,   -1,   -1,   -1,  268,
  269,   -1,  271,  323,  324,  325,  326,  327,  328,  329,
  330,  331,  332,  333,  334,  335,   -1,   -1,  338,  339,
  340,   -1,   -1,  343,  293,  294,  295,  296,  297,   -1,
   -1,  262,  263,  264,   -1,   -1,  267,  268,  269,   -1,
  271,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  281,  282,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  290,
  291,   -1,  293,  294,  295,  296,  297,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  323,  324,  325,  326,  327,  328,  329,  330,
  331,  332,  333,  334,  335,   -1,   -1,  338,  339,  340,
   -1,   -1,  343,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  262,  263,  264,   -1,   -1,  267,  268,  269,   -1,
  271,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  281,  282,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  290,
  291,   -1,  293,  294,  295,  296,  297,   -1,   -1,   -1,
   -1,   -1,   -1,    0,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   10,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  323,  324,  325,  326,  327,  328,  329,  330,
  331,  332,  333,  334,  335,   -1,   -1,  338,  339,  340,
   37,   38,  343,   -1,   41,   42,   43,   44,   45,   46,
   47,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   58,   59,   60,   61,   62,   63,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,    0,   -1,
   -1,   -1,   -1,   -1,   91,   -1,   93,   94,   10,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   37,   38,  124,  125,   41,
   42,   43,   44,   45,   46,   47,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   58,   59,   60,   -1,
   62,   63,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   91,
   -1,   93,   94,   -1,    0,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   10,   -1,   -1,   -1,   -1,    0,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   10,
   -1,   -1,  124,  125,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   37,   38,   -1,   -1,   41,   42,   43,   44,   45,
   46,   47,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   41,   -1,   58,   59,   60,   -1,   62,   63,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   59,   -1,
   -1,   -1,   -1,   -1,   -1,  262,  263,  264,   -1,   -1,
  267,  268,  269,   -1,  271,   91,   -1,   93,   94,   -1,
   -1,   -1,   -1,   -1,  281,  282,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  290,  291,   -1,  293,  294,  295,  296,
  297,   -1,   -1,   -1,   -1,   -1,   -1,    0,  124,  125,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   10,   -1,   -1,
   -1,   -1,   -1,   -1,  125,   -1,  323,  324,  325,  326,
  327,  328,  329,  330,  331,  332,  333,  334,  335,   -1,
   -1,  338,  339,  340,   -1,   -1,  343,   -1,   41,   -1,
  262,  263,  264,   -1,   -1,  267,  268,  269,   -1,  271,
   -1,   -1,   -1,   -1,   -1,   -1,   59,   -1,   -1,  281,
  282,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  290,  291,
   -1,  293,  294,  295,  296,  297,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   93,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  323,  324,  325,  326,  327,  328,  329,  330,  331,
  332,  333,  334,  335,   -1,   -1,  338,  339,  340,   -1,
   -1,  343,  125,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  262,  263,  264,   -1,
   -1,  267,  268,  269,   -1,  271,   -1,   -1,   -1,   -1,
   -1,  262,  263,  264,   -1,  281,  282,  268,  269,   -1,
  271,   -1,   -1,   -1,  290,  291,   -1,  293,  294,  295,
  296,  297,   -1,   -1,   -1,   -1,   -1,   -1,    0,   -1,
   -1,   -1,  293,  294,  295,  296,  297,   -1,   10,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  323,  324,  325,
  326,  327,  328,  329,  330,  331,  332,  333,  334,  335,
   -1,   -1,  338,  339,  340,   37,   38,  343,   -1,   41,
   42,   43,   44,   45,   46,   47,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   58,   59,   60,   -1,
   62,   63,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  262,
  263,  264,   -1,    0,  267,  268,  269,   -1,  271,   91,
   -1,   93,   94,   10,   -1,   -1,   -1,   -1,  281,  282,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  290,  291,   -1,
  293,  294,  295,  296,  297,   -1,   -1,   -1,   -1,   -1,
   37,   38,  124,  125,   41,   42,   43,   44,   45,   46,
   47,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   58,   59,   60,   -1,   62,   63,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   91,   -1,   93,   94,   -1,    0,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   10,
   -1,   -1,   -1,   -1,    0,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   10,   -1,   -1,  124,  125,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   37,   38,   -1,   -1,
   41,   42,   43,   44,   45,   46,   47,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   41,   -1,   -1,   59,   60,
   -1,   62,   63,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   59,   -1,   -1,   -1,   -1,   -1,   -1,
  262,  263,  264,   -1,   -1,  267,  268,  269,   -1,  271,
   91,   -1,   93,   94,   -1,   -1,   -1,   -1,   -1,  281,
  282,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  290,  291,
   -1,  293,  294,  295,  296,  297,   -1,   -1,   -1,   -1,
   -1,   -1,    0,  124,  125,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   10,   -1,   -1,   -1,   -1,   -1,   -1,  125,
   -1,  323,  324,  325,  326,  327,  328,  329,  330,  331,
  332,  333,  334,  335,   -1,   -1,  338,  339,  340,   -1,
   -1,  343,   -1,   41,   -1,  262,  263,  264,   -1,   -1,
  267,  268,  269,   -1,  271,   -1,   -1,   -1,   -1,   -1,
   -1,   59,   -1,   -1,  281,  282,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  290,  291,   -1,  293,  294,  295,  296,
  297,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   93,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  323,  324,  325,  326,
  327,  328,  329,  330,  331,  332,  333,  334,  335,   -1,
   -1,  338,  339,  340,   -1,   -1,  343,  125,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  262,  263,  264,   -1,   -1,  267,  268,  269,   -1,
  271,   -1,   -1,   -1,   -1,   -1,  262,  263,  264,   -1,
  281,  282,  268,  269,   -1,  271,   -1,   -1,   -1,  290,
  291,   -1,  293,  294,  295,  296,  297,   -1,   -1,   -1,
   -1,   -1,   -1,    0,   -1,   -1,   -1,  293,  294,  295,
  296,  297,   -1,   10,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  323,  324,  325,  326,  327,  328,  329,  330,
  331,  332,  333,  334,  335,   -1,   -1,  338,  339,  340,
   37,   38,  343,   -1,   41,   42,   43,   44,   45,   -1,
   47,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   58,   59,   60,   -1,   62,   63,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  262,  263,  264,   -1,    0,  267,
  268,  269,   -1,  271,   -1,   -1,   93,   94,   10,   -1,
   -1,   -1,   -1,  281,  282,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  290,  291,   -1,  293,  294,  295,  296,  297,
   -1,   -1,   -1,   -1,   -1,   37,   38,  124,  125,   41,
   42,   43,   44,   45,   -1,   47,   -1,   -1,   -1,    0,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   59,   60,   10,
   62,   63,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   41,   93,   94,    0,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   10,   -1,   -1,   -1,   -1,   59,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  124,  125,   -1,   -1,   -1,   -1,   -1,   -1,
   37,   38,   -1,   -1,   41,   42,   43,   44,   45,   -1,
   47,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   58,   59,   60,   -1,   62,   63,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  125,  262,  263,  264,   -1,   -1,
  267,  268,  269,   -1,  271,   -1,   93,   94,   -1,   -1,
   -1,   -1,   -1,   -1,  281,  282,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  290,  291,   -1,  293,  294,  295,  296,
  297,   -1,   -1,   -1,    0,   -1,   -1,  124,  125,   -1,
   -1,   -1,   -1,   -1,   10,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  323,  324,  325,  326,
  327,  328,  329,  330,  331,  332,  333,  334,  335,   -1,
   -1,  338,  339,   -1,   -1,   41,  343,   -1,   -1,   -1,
  262,  263,  264,   -1,   -1,  267,  268,  269,   -1,  271,
   -1,   -1,   -1,   59,   -1,   -1,   -1,   -1,   -1,  281,
  282,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  290,  291,
   -1,  293,  294,  295,  296,  297,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  262,  263,  264,   -1,   -1,   -1,  268,  269,   -1,
  271,  323,  324,  325,  326,  327,  328,  329,  330,  331,
  332,  333,  334,  335,   -1,   -1,  338,  339,   -1,  125,
   -1,  343,  293,  294,  295,  296,  297,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  262,  263,  264,   -1,   -1,
  267,  268,  269,   -1,  271,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  281,  282,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  290,  291,   -1,  293,  294,  295,  296,
  297,   -1,   -1,   -1,   -1,   -1,    0,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   10,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  324,  325,  326,
  327,  328,  329,  330,  331,  332,  333,  334,  335,   -1,
   -1,  338,  339,   37,   38,   -1,  343,   41,   42,   43,
   44,   45,   -1,   47,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   58,   59,   60,   -1,   62,   63,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  262,  263,  264,   -1,
   -1,    0,  268,  269,   -1,  271,   -1,   -1,   -1,   93,
   94,   10,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  293,  294,  295,
  296,  297,   -1,   -1,   -1,   -1,   -1,   -1,   37,   38,
  124,  125,   41,   42,   43,   44,   45,   -1,   47,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   58,
   59,   60,   -1,   62,   63,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   93,   94,    0,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   10,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  124,  125,   -1,   -1,   -1,
   -1,   -1,   -1,   37,   38,   -1,   -1,   41,   42,   43,
   44,   45,   -1,   47,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   58,   59,   60,   -1,   62,   63,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  262,  263,
  264,   -1,   -1,  267,  268,  269,   -1,  271,   -1,   93,
   94,   -1,   -1,   -1,   -1,   -1,   -1,  281,  282,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  290,  291,   -1,  293,
  294,  295,  296,  297,   -1,   -1,   -1,   -1,   -1,   -1,
  124,  125,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  324,  325,  326,  327,  328,  329,  330,  331,  332,  333,
  334,  335,   -1,   -1,  338,  339,   -1,   -1,   -1,  343,
   -1,   -1,   -1,  262,  263,  264,   -1,   -1,  267,  268,
  269,   -1,  271,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  281,  282,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  290,  291,   -1,  293,  294,  295,  296,  297,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  324,  325,  326,  327,  328,
  329,  330,  331,  332,  333,  334,  335,   -1,   -1,  338,
  339,   -1,   -1,   -1,  343,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  262,  263,
  264,   -1,   -1,  267,  268,  269,   -1,  271,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  281,  282,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  290,  291,   -1,  293,
  294,  295,  296,  297,   -1,   -1,   -1,   -1,   -1,    0,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   10,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  324,  325,  326,  327,  328,  329,  330,  331,  332,  333,
  334,  335,   -1,   -1,  338,  339,   37,   38,   -1,  343,
   41,   42,   43,   44,   45,   -1,   47,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   58,   59,   60,
   -1,   62,   63,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,    0,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   93,   94,   10,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   37,   38,  124,  125,   41,   42,   43,   44,   45,
   -1,   47,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   58,   59,   60,   -1,   62,   63,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   93,   94,    0,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   10,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  124,  125,
   -1,   -1,   -1,   -1,   -1,   -1,   37,   38,   -1,   -1,
   41,   42,   43,   44,   45,   -1,   47,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   58,   59,   60,
   -1,   62,   63,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  262,  263,  264,   -1,   -1,  267,  268,  269,   -1,
  271,   -1,   93,   94,   -1,   -1,   -1,   -1,   -1,   -1,
  281,  282,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  290,
  291,   -1,  293,  294,  295,  296,  297,   -1,   -1,   -1,
    0,   -1,   -1,  124,  125,   -1,   -1,   -1,   -1,   -1,
   10,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  324,  325,  326,  327,  328,  329,  330,
  331,  332,  333,  334,  335,   -1,   -1,  338,  339,   -1,
   -1,   41,  343,   -1,   44,   -1,  262,  263,  264,   -1,
   -1,  267,  268,  269,   -1,  271,   -1,   -1,   58,   59,
   60,   -1,   62,   63,   -1,  281,  282,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  290,  291,   -1,  293,  294,  295,
  296,  297,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   93,   94,   -1,   -1,   -1,   -1,   -1,
   -1,   10,   -1,   -1,   -1,   -1,   -1,   -1,  324,  325,
  326,  327,  328,  329,  330,  331,  332,  333,  334,  335,
   -1,   -1,  338,  339,  124,  125,   -1,  343,   37,   38,
   -1,   -1,   -1,   42,   43,   -1,   45,   -1,   47,   -1,
   -1,  262,  263,  264,   -1,   -1,  267,  268,  269,   -1,
  271,   60,   -1,   62,   63,   -1,   -1,   -1,   -1,   -1,
  281,  282,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  290,
  291,   -1,  293,  294,  295,  296,  297,   -1,   -1,   -1,
   -1,   -1,    0,   -1,   -1,   94,   -1,   -1,   -1,   -1,
   -1,   -1,   10,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  324,  325,  326,  327,  328,  329,  330,
  331,  332,  333,  334,  335,  124,   -1,  338,  339,   37,
   38,   -1,  343,   41,   42,   43,   44,   45,   -1,   47,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   58,   59,   60,   -1,   62,   63,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
    0,   -1,  262,  263,  264,   -1,   -1,  267,  268,  269,
   10,  271,   -1,   -1,   -1,   93,   94,   -1,   -1,   -1,
   -1,  281,  282,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  290,  291,   -1,  293,  294,  295,  296,  297,   38,   -1,
   -1,   41,   -1,   43,   44,   45,  124,  125,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   58,   59,
   60,   -1,   62,   63,  324,  325,  326,  327,  328,  329,
  330,  331,  332,  333,  334,  335,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  343,   -1,    0,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   93,   94,   10,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   38,  124,  125,   41,   -1,   43,   44,
   45,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   58,   59,   60,   -1,   62,   63,   -1,
   -1,   -1,   -1,   -1,  323,  324,  325,  326,  327,  328,
  329,  330,  331,  332,  333,  334,  335,   -1,   -1,  338,
  339,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   93,   94,
   -1,   -1,    0,   -1,  262,  263,  264,   -1,   -1,  267,
  268,  269,   10,  271,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  281,  282,   -1,   -1,   -1,   -1,  124,
  125,   -1,  290,  291,   -1,  293,  294,  295,  296,  297,
   38,   -1,   -1,   41,   -1,   -1,   44,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   58,   59,   60,   -1,   62,   63,  324,  325,  326,  327,
  328,  329,  330,  331,  332,  333,  334,  335,   -1,   -1,
  338,  339,  262,  263,  264,  343,   -1,  267,  268,  269,
   -1,  271,   -1,   -1,   -1,   93,   94,   -1,   -1,   -1,
   -1,  281,  282,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  290,  291,   -1,  293,  294,  295,  296,  297,   -1,   -1,
   -1,    0,   -1,   -1,   -1,   -1,  124,  125,   -1,   -1,
   -1,   10,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  324,  325,  326,  327,  328,  329,
  330,  331,  332,  333,  334,  335,   -1,   -1,  338,  339,
   -1,   -1,   41,  343,   -1,    0,   -1,  262,  263,  264,
   -1,   -1,  267,  268,  269,   10,  271,   -1,   -1,   -1,
   59,   -1,   -1,   -1,   -1,   -1,  281,  282,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  290,  291,   -1,  293,  294,
  295,  296,  297,   38,   -1,   -1,   41,   -1,   -1,   44,
   -1,   -1,   -1,   -1,   93,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   58,   59,   60,   -1,   62,   63,  324,
  325,  326,  327,  328,  329,  330,  331,  332,  333,  334,
  335,   -1,   -1,  338,  339,   -1,  125,   -1,  343,   -1,
    0,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   93,   94,
   10,   -1,   -1,   -1,  262,  263,  264,   -1,   -1,  267,
  268,  269,   -1,  271,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  281,  282,   -1,   -1,   -1,   -1,  124,
  125,   41,  290,  291,   44,  293,  294,  295,  296,  297,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   58,   59,
   60,   -1,   62,   63,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  324,  325,  326,  327,
  328,  329,  330,  331,  332,  333,  334,  335,    0,   -1,
  338,  339,   -1,   93,   94,  343,   -1,   -1,   10,   -1,
   -1,   -1,   -1,   37,   38,   -1,   -1,   -1,   42,   43,
   -1,   45,   -1,   47,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  124,  125,   60,   -1,   62,   41,
   -1,   -1,   44,  262,  263,  264,   -1,   -1,  267,  268,
  269,   -1,  271,   -1,   -1,   -1,   58,   59,   60,   -1,
   62,   63,  281,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   94,  290,  291,   -1,  293,  294,  295,  296,  297,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  262,  263,  264,
   -1,   93,  267,  268,  269,   -1,  271,   -1,   -1,   -1,
  124,   -1,   -1,    0,   -1,   -1,  281,  282,   -1,   -1,
   -1,   -1,   -1,   10,   -1,  290,  291,   -1,  293,  294,
  295,  296,  297,  125,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   41,   -1,   -1,   44,   -1,  324,
  325,  326,  327,  328,  329,  330,  331,  332,  333,  334,
  335,   58,   59,   60,   -1,   62,   63,   -1,  343,   -1,
   -1,   -1,  262,  263,  264,   -1,   -1,  267,  268,  269,
   -1,  271,   -1,   -1,   -1,   -1,   -1,   -1,    0,   -1,
   -1,  281,  282,   -1,   -1,   -1,   93,   -1,   10,   -1,
  290,  291,   -1,  293,  294,  295,  296,  297,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  125,   41,
   -1,   -1,   44,   -1,  324,  325,  326,  327,  328,  329,
  330,  331,  332,  333,  334,  335,   58,   59,   60,   -1,
   62,   63,   -1,  343,   -1,   -1,   -1,   -1,   -1,   -1,
  262,  263,  264,   -1,   -1,  267,  268,  269,   -1,  271,
   -1,   -1,   -1,   -1,   -1,   -1,    0,   -1,   -1,  281,
  282,   93,   -1,   -1,   -1,   -1,   10,   -1,  290,  291,
   -1,  293,  294,  295,  296,  297,   -1,   -1,   -1,  323,
  324,  325,  326,  327,  328,  329,   -1,   -1,  332,  333,
   -1,   -1,   -1,  125,  338,  339,   -1,   41,   -1,   -1,
   44,   -1,  324,  325,  326,  327,  328,  329,  330,  331,
  332,  333,  334,  335,   58,   59,   60,   -1,   62,   63,
   -1,  343,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  262,  263,  264,   -1,   93,
  267,  268,  269,   -1,  271,   -1,   -1,   -1,   -1,   -1,
   -1,    0,   -1,   -1,  281,  282,   -1,   -1,   -1,   -1,
   -1,   10,   -1,  290,  291,   -1,  293,  294,  295,  296,
  297,  125,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   41,   -1,   -1,   44,   -1,  324,  325,  326,
  327,  328,  329,  330,  331,  332,  333,  334,  335,   58,
   59,   -1,   -1,   -1,   63,   -1,  343,   -1,   -1,   -1,
  262,  263,  264,   -1,   -1,  267,  268,  269,   -1,  271,
   -1,   -1,   -1,   -1,   -1,   -1,    0,   -1,   -1,  281,
  282,   -1,   -1,   -1,   93,   -1,   10,   -1,  290,  291,
   -1,  293,  294,  295,  296,  297,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  125,   41,   -1,   -1,
   44,   -1,  324,  325,  326,  327,  328,  329,  330,  331,
  332,  333,  334,  335,   58,   59,   -1,   -1,   -1,   63,
   -1,  343,   -1,   -1,   -1,   -1,   -1,   -1,  262,  263,
  264,   -1,   -1,  267,  268,  269,   -1,  271,   -1,   -1,
   -1,   -1,   -1,   -1,    0,   -1,   -1,  281,  282,   93,
   -1,   -1,   -1,   -1,   10,   -1,  290,  291,   -1,  293,
  294,  295,  296,  297,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  125,   -1,   -1,   -1,   41,   -1,   -1,   44,   -1,
  324,  325,  326,  327,  328,  329,  330,  331,  332,  333,
  334,  335,   58,   59,   -1,   -1,   -1,   63,   -1,  343,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  262,  263,  264,    0,   93,  267,  268,
  269,   -1,  271,   -1,   -1,   -1,   10,   -1,   -1,   -1,
   -1,   -1,  281,  282,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  290,  291,   -1,  293,  294,  295,  296,  297,  125,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   41,   -1,   -1,
   44,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   58,   59,   -1,   -1,   -1,   63,
   -1,  330,  331,   -1,   -1,  334,  335,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  343,   -1,   -1,   -1,  262,  263,
  264,    0,   -1,  267,  268,  269,   -1,  271,   -1,   93,
   -1,   10,   -1,   -1,   -1,   -1,   -1,  281,  282,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  290,  291,   -1,  293,
  294,  295,  296,  297,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  125,   41,   -1,   -1,   44,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   58,
   59,   -1,   -1,   -1,   63,   -1,  330,  331,   -1,   -1,
  334,  335,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  343,
   -1,   -1,   -1,   -1,   -1,   -1,  262,  263,  264,    0,
   -1,  267,  268,  269,   93,  271,   -1,   -1,   -1,   10,
   -1,   -1,   -1,   -1,   -1,  281,  282,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  290,  291,   -1,  293,  294,  295,
  296,  297,   -1,   -1,   -1,   -1,  125,   -1,   -1,   -1,
   41,   -1,   -1,   44,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   58,   59,   -1,
   -1,   -1,   63,   -1,  330,  331,   -1,   -1,  334,  335,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  343,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  262,  263,
  264,    0,   93,  267,  268,  269,   -1,  271,   -1,   -1,
   -1,   10,   -1,   -1,   -1,   -1,   -1,  281,  282,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  290,  291,   -1,  293,
  294,  295,  296,  297,  125,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   41,   -1,   -1,   44,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   58,
   59,   -1,   -1,   -1,   63,   -1,  330,  331,   -1,   -1,
  334,  335,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  343,
   -1,   -1,   -1,  262,  263,  264,    0,   -1,  267,  268,
  269,   -1,  271,   -1,   93,   -1,   10,   -1,   -1,   -1,
   -1,   -1,  281,  282,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  290,  291,   -1,  293,  294,  295,  296,  297,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  125,   41,   -1,   -1,
   44,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   58,   59,   -1,   -1,   -1,   63,
   -1,  330,  331,   -1,   -1,  334,  335,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  343,   -1,   -1,   -1,   -1,   -1,
   -1,  262,  263,  264,    0,   -1,  267,  268,  269,   93,
  271,   -1,   -1,   -1,   10,   -1,   -1,   -1,   -1,   -1,
  281,  282,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  290,
  291,   -1,  293,  294,  295,  296,  297,   -1,   -1,   -1,
   -1,  125,   -1,   -1,   -1,   41,    0,   -1,   44,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   10,   -1,   -1,   -1,
   -1,   -1,   58,   59,   -1,   -1,   -1,   63,   -1,  330,
  331,   -1,   -1,  334,  335,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  343,   -1,   -1,   -1,   -1,   41,   -1,   -1,
   44,   -1,   -1,  262,  263,  264,   -1,   93,  267,  268,
  269,    0,  271,   -1,   58,   59,   -1,   -1,   -1,   63,
   -1,   10,  281,  282,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  290,  291,   -1,  293,  294,  295,  296,  297,  125,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   93,
   -1,   -1,   41,   -1,   -1,   44,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   58,
   59,  330,  331,   -1,   -1,  334,  335,   -1,   -1,   -1,
   -1,  125,   -1,   -1,  343,   -1,   -1,   -1,  262,  263,
  264,   -1,   -1,  267,  268,  269,    0,  271,   -1,   -1,
   -1,   -1,   -1,   -1,   93,   -1,   10,  281,  282,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  290,  291,   -1,  293,
  294,  295,  296,  297,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  125,   41,   -1,   -1,
   44,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   58,   59,    0,  331,   -1,   -1,
  334,  335,   -1,   -1,   -1,   -1,   10,   -1,   -1,  343,
   -1,   -1,   -1,   -1,   -1,   -1,  262,  263,  264,   -1,
   -1,  267,  268,  269,   -1,  271,   -1,   -1,   -1,   93,
   -1,   -1,   -1,   -1,   -1,  281,  282,   41,   -1,   -1,
   44,   -1,   -1,   -1,  290,  291,   -1,  293,  294,  295,
  296,  297,   -1,   -1,   -1,   59,   -1,   -1,  262,  263,
  264,  125,   -1,  267,  268,  269,    0,  271,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   10,  281,  282,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  290,  291,   -1,  293,
  294,  295,  296,  297,   -1,   -1,   -1,  343,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   41,   -1,   -1,
   44,   -1,   -1,  262,  263,  264,   -1,    0,  267,  268,
  269,  125,  271,   -1,   58,   59,   -1,   10,   -1,   -1,
   -1,   -1,  281,  282,   -1,   -1,   -1,   -1,   -1,  343,
   -1,  290,  291,   -1,  293,  294,  295,  296,  297,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   41,   93,
   -1,   44,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   58,   59,   -1,   -1,   -1,
    0,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   10,  125,   -1,   -1,  343,   -1,   -1,   -1,  262,  263,
  264,   -1,   -1,  267,  268,  269,   -1,  271,   -1,   -1,
   93,   -1,   -1,   -1,   -1,   -1,   -1,  281,  282,   -1,
   -1,   41,   -1,   -1,   44,    0,  290,  291,   -1,  293,
  294,  295,  296,  297,   -1,   10,   -1,   -1,   58,   59,
   -1,   -1,  125,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  262,  263,
  264,   -1,    0,  267,  268,  269,   41,  271,   -1,   44,
   -1,   -1,   10,   93,   -1,   -1,   -1,  281,   -1,  343,
   -1,   -1,   -1,   58,   59,   -1,  290,  291,   -1,  293,
  294,  295,  296,  297,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   41,   -1,  125,   44,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   93,   -1,
   58,   59,   -1,   -1,   -1,   -1,   -1,   -1,  262,  263,
  264,    0,   -1,  267,  268,  269,   -1,  271,   -1,   -1,
   -1,   10,   -1,   -1,   -1,   -1,   -1,  281,  282,   -1,
  125,   -1,   -1,   -1,   -1,   93,  290,  291,   -1,  293,
  294,  295,  296,  297,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   41,   -1,   -1,   44,   -1,   -1,   -1,  262,
  263,  264,   -1,   -1,  267,  268,  269,  125,  271,   58,
   59,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  281,  282,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  290,  291,  343,
  293,  294,  295,  296,  297,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   93,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  262,  263,  264,    0,   -1,  267,  268,  269,
   -1,  271,   -1,   -1,   -1,   10,  125,   -1,   -1,   -1,
  343,  281,  282,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  290,  291,   -1,  293,  294,  295,  296,  297,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   41,  262,  263,  264,
   -1,   -1,  267,  268,  269,   -1,  271,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   59,   -1,  281,  282,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  290,  291,   -1,  293,  294,
  295,  296,  297,  343,  262,  263,  264,   -1,   -1,  267,
  268,  269,   -1,  271,   -1,   -1,   -1,   -1,   93,   -1,
   -1,   -1,   -1,  281,  282,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  290,  291,   -1,  293,  294,  295,  296,  297,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  343,   -1,
  125,   -1,   -1,    0,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   10,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  262,  263,  264,   -1,   -1,  267,  268,
  269,   -1,  271,   -1,   -1,  343,   -1,   -1,   -1,   -1,
   37,   38,  281,  282,   41,   42,   43,   44,   45,   46,
   47,  290,  291,   -1,  293,  294,  295,  296,  297,   -1,
   -1,   -1,   59,   60,   61,   62,   63,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,    0,   -1,
   -1,   -1,   -1,   -1,   91,   -1,   -1,   94,   10,   -1,
   -1,   -1,   -1,   -1,  343,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   37,   38,  124,  125,   41,
   42,   43,   44,   45,   46,   47,   -1,  262,  263,  264,
   -1,   -1,  267,  268,  269,   -1,  271,   59,   60,   61,
   62,   63,   -1,   -1,   -1,   -1,  281,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  290,  291,   -1,  293,  294,
  295,  296,  297,   -1,   -1,   -1,   -1,   -1,   -1,   91,
   -1,   -1,   94,   -1,    0,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   10,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  124,  125,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   37,   38,   -1,   -1,   41,   42,   43,   44,   45,
   46,   47,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   59,   60,   61,   62,   63,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  262,  263,  264,   -1,   -1,
  267,  268,  269,   -1,  271,   91,   -1,   -1,   94,   -1,
   -1,   -1,   -1,   -1,  281,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  290,  291,   -1,  293,  294,  295,  296,
  297,   -1,   -1,   -1,    0,   -1,   -1,   -1,  124,  125,
   -1,   -1,   -1,   -1,   10,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  323,  324,  325,  326,
  327,  328,  329,  330,  331,  332,  333,  334,  335,   -1,
   -1,  338,  339,  340,   -1,   41,   -1,   -1,   44,   -1,
  262,  263,  264,   -1,   -1,  267,  268,  269,   -1,  271,
   -1,   -1,   -1,   59,   -1,   -1,   -1,   -1,   -1,  281,
   -1,   -1,   -1,   -1,   -1,   10,   -1,   -1,  290,  291,
   -1,  293,  294,  295,  296,  297,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   93,   33,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  323,  324,  325,  326,  327,  328,  329,  330,  331,
  332,  333,  334,  335,   59,   -1,  338,  339,  340,  125,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  262,  263,  264,   -1,
   -1,  267,  268,  269,   -1,  271,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  281,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  290,  291,   -1,  293,  294,  295,
  296,  297,   -1,   -1,   -1,   -1,   -1,   -1,    0,   -1,
   -1,  126,   -1,   -1,   -1,   -1,   -1,   -1,   10,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  323,  324,  325,
  326,  327,  328,  329,  330,  331,  332,  333,  334,  335,
   -1,   -1,  338,  339,  340,   37,   38,   -1,   -1,   41,
   42,   43,   -1,   45,   46,   47,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   59,   60,   -1,
   62,   63,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  262,  263,  264,   -1,
   -1,  267,  268,  269,   -1,  271,   -1,   -1,   -1,   91,
   -1,   -1,   94,   10,   -1,  281,  282,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  290,  291,   -1,  293,  294,  295,
  296,  297,   -1,   -1,   -1,   -1,   33,   -1,   -1,   -1,
   -1,   -1,  124,  125,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  256,  257,  258,  259,  260,  261,  262,  263,  264,
  265,  266,   59,  268,  269,  270,  271,  272,  273,  274,
  275,  276,  277,  278,   -1,   -1,   -1,   -1,  283,  284,
  285,  286,  287,  288,  289,   -1,   -1,  292,   -1,   -1,
   -1,   -1,   -1,  298,  299,  300,  301,  302,  303,  304,
  305,  306,  307,  308,  309,  310,  311,  312,  313,  314,
  315,  316,  317,  318,  319,  320,  321,  322,   -1,   -1,
   10,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  126,
   -1,   -1,   -1,   -1,   -1,   -1,  341,   -1,   -1,  344,
  345,  346,  347,   33,  349,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   59,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  262,  263,  264,   -1,   -1,  267,  268,  269,   -1,  271,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  281,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  290,  291,
   -1,  293,  294,  295,  296,  297,   10,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  125,  126,   -1,   -1,   33,
   -1,  323,  324,  325,  326,  327,  328,  329,  330,  331,
  332,  333,  334,  335,   -1,   -1,  338,  339,  340,  256,
  257,  258,  259,  260,  261,   59,   -1,  264,  265,  266,
   -1,   -1,   -1,  270,   -1,  272,  273,  274,  275,  276,
  277,  278,   -1,   -1,   -1,   -1,  283,  284,  285,  286,
  287,  288,  289,   -1,   -1,  292,   -1,   -1,   -1,   -1,
   -1,  298,  299,  300,  301,  302,  303,  304,  305,  306,
  307,  308,  309,  310,  311,  312,  313,  314,  315,  316,
  317,  318,  319,  320,  321,  322,   -1,   -1,   -1,   -1,
   -1,   -1,  126,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  341,   -1,   -1,  344,  345,  346,
  347,   -1,  349,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   33,   -1,   -1,   -1,   -1,   -1,  256,  257,  258,  259,
  260,  261,   -1,   -1,   -1,  265,  266,   -1,   -1,   -1,
  270,   -1,  272,  273,  274,  275,  276,  277,  278,   -1,
   -1,   -1,   -1,  283,  284,  285,  286,  287,  288,  289,
   -1,   -1,  292,   -1,   -1,   -1,   -1,   -1,  298,  299,
  300,  301,  302,  303,  304,  305,  306,  307,  308,  309,
  310,  311,  312,  313,  314,  315,  316,  317,  318,  319,
  320,  321,  322,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  341,   -1,  126,  344,  345,  346,  347,   -1,  349,
   -1,   33,   -1,  257,  258,  259,   -1,  261,   -1,   -1,
   -1,  265,  266,   -1,   -1,   -1,  270,   -1,  272,  273,
  274,  275,  276,  277,  278,   -1,   -1,   -1,   -1,  283,
  284,  285,  286,  287,  288,  289,   -1,    0,  292,   -1,
   -1,   -1,   -1,   -1,   -1,  299,   -1,   10,  302,  303,
  304,  305,  306,  307,  308,  309,  310,  311,  312,  313,
  314,  315,  316,  317,  318,  319,  320,  321,  322,   -1,
   -1,   -1,   -1,   -1,   37,   38,   -1,   -1,   41,   42,
   43,   -1,   45,   -1,   47,   -1,   -1,  341,   -1,   -1,
  344,  345,  346,  347,  126,  349,   59,   60,   -1,   62,
   63,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  256,  257,  258,  259,  260,  261,   -1,
   -1,   94,  265,  266,   -1,   -1,   -1,  270,   -1,  272,
  273,  274,  275,  276,  277,  278,   -1,   -1,   -1,   -1,
  283,  284,  285,  286,  287,  288,  289,   -1,   -1,  292,
   -1,  124,  125,   -1,   -1,  298,  299,  300,  301,  302,
  303,  304,  305,  306,  307,  308,  309,  310,  311,  312,
  313,  314,  315,  316,  317,  318,  319,  320,  321,  322,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   33,   -1,   -1,   -1,  341,   -1,
   -1,  344,  345,  346,  347,   -1,  349,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  256,  257,  258,  259,  260,  261,
   59,   -1,   -1,  265,  266,   -1,   -1,   -1,  270,   -1,
  272,  273,  274,  275,  276,  277,  278,   -1,   -1,   -1,
   -1,  283,  284,  285,  286,  287,  288,  289,   -1,   -1,
  292,   -1,   -1,   -1,   -1,   -1,  298,  299,  300,  301,
  302,  303,  304,  305,  306,  307,  308,  309,  310,  311,
  312,  313,  314,  315,  316,  317,  318,  319,  320,  321,
  322,   -1,   -1,   -1,   -1,   -1,   -1,  126,   33,  262,
  263,  264,   -1,   -1,  267,  268,  269,   -1,  271,  341,
   -1,   -1,  344,  345,  346,  347,   -1,  349,  281,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  290,  291,   -1,
  293,  294,  295,  296,  297,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  323,  324,  325,  326,  327,  328,  329,  330,  331,  332,
  333,  334,  335,   -1,   -1,  338,  339,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  126,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   33,   -1,   -1,   -1,   -1,   -1,   -1,   40,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  257,  258,
  259,  260,  261,   -1,   -1,   -1,  265,  266,   -1,   -1,
   -1,  270,   -1,  272,  273,  274,  275,  276,  277,  278,
   -1,   -1,   -1,   -1,  283,  284,  285,  286,  287,  288,
  289,   -1,   -1,  292,   -1,   -1,   -1,   -1,   -1,  298,
  299,  300,  301,  302,  303,  304,  305,  306,  307,  308,
  309,  310,  311,  312,  313,  314,  315,  316,  317,  318,
  319,  320,  321,  322,  126,   33,   -1,   -1,   -1,   -1,
   -1,   -1,   40,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  341,   -1,   -1,  344,  345,  346,  347,   -1,
  349,   -1,  257,  258,  259,  260,  261,   -1,   -1,   -1,
  265,  266,   -1,   -1,   -1,  270,   -1,  272,  273,  274,
  275,  276,  277,  278,   -1,   -1,   -1,   -1,  283,  284,
  285,  286,  287,  288,  289,   -1,   -1,  292,   -1,   -1,
   -1,   -1,   -1,  298,  299,  300,  301,  302,  303,  304,
  305,  306,  307,  308,  309,  310,  311,  312,  313,  314,
  315,  316,  317,  318,  319,  320,  321,  322,  126,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   33,
   -1,   -1,   -1,   -1,   -1,   -1,  341,   41,   -1,  344,
  345,  346,  347,   -1,  349,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  257,  258,  259,   -1,  261,
   -1,   -1,   -1,  265,  266,   -1,   -1,   -1,  270,   -1,
  272,  273,  274,  275,  276,  277,  278,   -1,   -1,   -1,
   -1,  283,  284,  285,  286,  287,  288,  289,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  299,   -1,   -1,
  302,  303,  304,  305,  306,  307,  308,  309,  310,  311,
  312,  313,  314,  315,  316,  317,  318,  319,  320,  321,
  322,   -1,  126,   33,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   41,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  341,
   -1,   -1,  344,  345,  346,  347,  348,  349,   -1,  257,
  258,  259,   -1,  261,   -1,   -1,   -1,  265,  266,   -1,
   -1,   -1,  270,   -1,  272,  273,  274,  275,  276,  277,
  278,   -1,   -1,   -1,   -1,  283,  284,  285,  286,  287,
  288,  289,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  299,   -1,   -1,  302,  303,  304,  305,  306,  307,
  308,  309,  310,  311,  312,  313,  314,  315,  316,  317,
  318,  319,  320,  321,  322,   -1,  126,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   33,   -1,   -1,   -1,
   -1,   -1,   -1,  341,   -1,   -1,  344,  345,  346,  347,
  348,  349,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  257,  258,  259,   -1,  261,   -1,   -1,
   -1,  265,  266,   -1,   -1,   -1,  270,   -1,  272,  273,
  274,  275,  276,  277,  278,   -1,   -1,   -1,   -1,  283,
  284,  285,  286,  287,  288,  289,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  299,   -1,   -1,  302,  303,
  304,  305,  306,  307,  308,  309,  310,  311,  312,  313,
  314,  315,  316,  317,  318,  319,  320,  321,  322,  126,
   33,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  341,   -1,   -1,
  344,  345,  346,  347,  348,  349,   -1,  257,  258,  259,
   -1,  261,   -1,   -1,   -1,  265,  266,   -1,   -1,   -1,
  270,   -1,  272,  273,  274,  275,  276,  277,  278,   -1,
   -1,   -1,   -1,  283,  284,  285,  286,  287,  288,  289,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  299,
   -1,   -1,  302,  303,  304,  305,  306,  307,  308,  309,
  310,  311,  312,  313,  314,  315,  316,  317,  318,  319,
  320,  321,  322,  126,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   33,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  341,   -1,   -1,  344,  345,  346,  347,  348,  349,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  257,  258,  259,   -1,  261,   -1,   -1,   -1,  265,  266,
   -1,   -1,   -1,  270,   -1,  272,  273,  274,  275,  276,
  277,  278,   -1,   -1,   -1,   -1,  283,  284,  285,  286,
  287,  288,  289,   -1,   -1,  292,   -1,   -1,   -1,   -1,
   -1,   -1,  299,   -1,   -1,  302,  303,  304,  305,  306,
  307,  308,  309,  310,  311,  312,  313,  314,  315,  316,
  317,  318,  319,  320,  321,  322,  126,   33,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  341,   -1,   -1,  344,  345,  346,
  347,   -1,  349,   -1,  257,  258,  259,   -1,  261,   -1,
   -1,   -1,  265,  266,   -1,   -1,   -1,  270,   -1,  272,
  273,  274,  275,  276,  277,  278,   -1,   -1,   -1,   -1,
  283,  284,  285,  286,  287,  288,  289,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  299,   -1,   -1,  302,
  303,  304,  305,  306,  307,  308,  309,  310,  311,  312,
  313,  314,  315,  316,  317,  318,  319,  320,  321,  322,
  126,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   33,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  341,   -1,
   -1,  344,  345,  346,  347,  348,  349,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  257,  258,  259,
   -1,  261,   -1,   -1,   -1,  265,  266,   -1,   -1,   -1,
  270,   -1,  272,  273,  274,  275,  276,  277,  278,   -1,
   -1,   -1,   -1,  283,  284,  285,  286,  287,  288,  289,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  299,
   -1,   -1,  302,  303,  304,  305,  306,  307,  308,  309,
  310,  311,  312,  313,  314,  315,  316,  317,  318,  319,
  320,  321,  322,  126,   33,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  341,   -1,   -1,  344,  345,  346,  347,  348,  349,
   -1,  257,  258,  259,   -1,  261,   -1,   -1,   -1,  265,
  266,   -1,   -1,   -1,  270,   -1,  272,  273,  274,  275,
  276,  277,  278,   -1,   -1,   -1,   -1,  283,  284,  285,
  286,  287,  288,  289,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  299,   -1,   -1,  302,  303,  304,  305,
  306,  307,  308,  309,  310,  311,  312,  313,  314,  315,
  316,  317,  318,  319,  320,  321,  322,  126,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   33,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  341,   -1,   -1,  344,  345,
  346,  347,  348,  349,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  257,  258,  259,   -1,  261,   -1,
   -1,   -1,  265,  266,   -1,   -1,   -1,  270,   -1,  272,
  273,  274,  275,  276,  277,  278,   -1,   -1,   -1,   -1,
  283,  284,  285,  286,  287,  288,  289,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  299,   -1,   -1,  302,
  303,  304,  305,  306,  307,  308,  309,  310,  311,  312,
  313,  314,  315,  316,  317,  318,  319,  320,  321,  322,
  126,   33,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  341,   -1,
   -1,  344,  345,  346,  347,  348,  349,   -1,  257,  258,
  259,   -1,  261,   -1,   -1,   -1,  265,  266,   -1,   -1,
   -1,  270,   -1,  272,  273,  274,  275,  276,  277,  278,
   -1,   -1,   -1,   -1,  283,  284,  285,  286,  287,  288,
  289,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  299,   -1,   -1,  302,  303,  304,  305,  306,  307,  308,
  309,  310,  311,  312,  313,  314,  315,  316,  317,  318,
  319,  320,  321,  322,  126,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   33,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  341,   -1,   -1,  344,  345,  346,  347,  348,
  349,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  257,  258,  259,   -1,  261,   -1,   -1,   -1,  265,
  266,   -1,   -1,   -1,  270,   -1,  272,  273,  274,  275,
  276,  277,  278,   -1,   -1,   -1,   -1,  283,  284,  285,
  286,  287,  288,  289,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  299,   -1,   -1,  302,  303,  304,  305,
  306,  307,  308,  309,  310,  311,  312,  313,  314,  315,
  316,  317,  318,  319,  320,  321,  322,  126,   33,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  341,   -1,   -1,  344,  345,
  346,  347,   -1,  349,   -1,  257,  258,  259,   -1,  261,
   -1,   -1,   -1,  265,  266,   -1,   -1,   -1,  270,   -1,
  272,  273,  274,  275,  276,  277,  278,   -1,   -1,   -1,
   -1,  283,  284,  285,  286,  287,  288,  289,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  299,   -1,   -1,
  302,  303,  304,  305,  306,  307,  308,  309,  310,  311,
  312,  313,  314,  315,  316,  317,  318,  319,  320,  321,
  322,  126,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   33,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  341,
   -1,   -1,  344,  345,  346,  347,   -1,  349,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  257,  258,
  259,   -1,  261,   -1,   -1,   -1,  265,  266,   -1,   -1,
   -1,  270,   -1,  272,  273,  274,  275,  276,  277,  278,
   -1,   -1,   -1,   -1,  283,  284,  285,  286,  287,  288,
  289,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  299,   -1,   -1,  302,  303,  304,  305,  306,  307,  308,
  309,  310,  311,  312,  313,  314,  315,  316,  317,  318,
  319,  320,  321,  322,  126,   33,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  341,   -1,   -1,  344,  345,  346,  347,   -1,
  349,   -1,  257,  258,  259,   -1,  261,   -1,   -1,   -1,
  265,  266,   -1,   -1,   -1,  270,   -1,  272,  273,  274,
  275,  276,  277,  278,   -1,   -1,   -1,   -1,  283,  284,
  285,  286,  287,  288,  289,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  299,   -1,   -1,  302,  303,  304,
  305,  306,  307,  308,  309,  310,  311,  312,  313,  314,
  315,  316,  317,  318,  319,  320,  321,  322,  126,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   33,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  341,   -1,   -1,  344,
  345,  346,  347,   -1,  349,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  257,  258,  259,   -1,  261,
   -1,   -1,   -1,  265,  266,   -1,   -1,   -1,  270,   -1,
  272,  273,  274,  275,  276,  277,  278,   -1,   -1,   -1,
   -1,  283,  284,  285,  286,  287,  288,  289,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  299,   -1,   -1,
  302,  303,  304,  305,  306,  307,  308,  309,  310,  311,
  312,  313,  314,  315,  316,  317,  318,  319,  320,  321,
  322,  126,   33,   -1,   -1,   -1,   -1,   -1,   -1,   40,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  341,
   -1,   -1,  344,  345,  346,  347,   -1,  349,   -1,  257,
  258,  259,   -1,  261,   -1,   -1,   -1,  265,  266,   -1,
   -1,   -1,  270,   -1,  272,  273,  274,  275,  276,  277,
  278,   -1,   -1,   -1,   -1,  283,  284,  285,  286,  287,
  288,  289,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  299,   -1,   -1,  302,  303,  304,  305,  306,  307,
  308,  309,  310,  311,  312,  313,  314,  315,  316,  317,
  318,  319,  320,  321,  322,  126,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   33,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  341,   -1,   -1,  344,  345,  346,  347,
   -1,  349,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  257,  258,  259,   -1,  261,   -1,   -1,   -1,
  265,  266,   -1,   -1,   -1,  270,   -1,  272,  273,  274,
  275,  276,  277,  278,   -1,   -1,   -1,   -1,  283,  284,
  285,  286,  287,  288,  289,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  299,   -1,   -1,  302,  303,  304,
  305,  306,  307,  308,  309,  310,  311,  312,  313,  314,
  315,  316,  317,  318,  319,  320,  321,  322,  126,   33,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  341,   -1,   -1,  344,
  345,  346,  347,   -1,  349,   -1,  257,  258,  259,   -1,
  261,   -1,   -1,   -1,  265,  266,   -1,   -1,   -1,  270,
   -1,  272,  273,  274,  275,  276,  277,  278,   -1,   -1,
   -1,   -1,  283,  284,  285,  286,  287,  288,  289,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  299,   -1,
   -1,  302,  303,  304,  305,  306,  307,  308,  309,  310,
  311,  312,  313,  314,  315,  316,  317,  318,  319,  320,
  321,  322,  126,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   33,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  341,   -1,   -1,  344,  345,  346,   -1,   -1,  349,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  257,
  258,  259,   -1,  261,   -1,   -1,   -1,  265,  266,   -1,
   -1,   -1,  270,   -1,  272,  273,  274,  275,  276,  277,
  278,   -1,   -1,   -1,   -1,  283,  284,  285,  286,  287,
  288,  289,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  299,   -1,   -1,  302,  303,  304,  305,  306,  307,
  308,  309,  310,  311,  312,  313,  314,  315,  316,  317,
  318,  319,  320,  321,  322,  126,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,    0,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  341,   10,   -1,  344,  345,  346,   -1,
   -1,  349,   -1,  257,  258,  259,   -1,  261,   -1,   -1,
   -1,  265,  266,   -1,   -1,   -1,  270,   -1,  272,  273,
  274,  275,  276,  277,  278,   41,   -1,   -1,   44,  283,
  284,  285,  286,  287,  288,  289,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   59,   -1,  299,   -1,   -1,  302,  303,
  304,  305,  306,  307,  308,  309,  310,  311,  312,  313,
  314,  315,  316,  317,  318,  319,  320,  321,  322,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   93,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  341,   37,   38,
  344,  345,  346,   42,   43,  349,   45,   -1,   47,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  257,  258,  259,  125,
  261,   60,   -1,   62,  265,  266,   -1,   -1,   -1,  270,
   -1,  272,  273,  274,  275,  276,  277,  278,   -1,   -1,
   -1,   -1,  283,  284,  285,  286,  287,  288,  289,   -1,
   -1,   -1,   -1,   -1,   -1,   94,   -1,   96,  299,   -1,
   -1,  302,  303,  304,  305,  306,  307,  308,  309,  310,
  311,  312,  313,  314,  315,  316,  317,  318,  319,  320,
  321,  322,   -1,   -1,   -1,  124,   -1,  126,   -1,   -1,
   -1,   37,   38,   -1,   -1,   -1,   42,   43,   -1,   45,
  341,   47,   -1,  344,  345,  346,   -1,   -1,  349,   -1,
   -1,   -1,   -1,   -1,   60,   -1,   62,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,    0,   -1,   -1,   94,   -1,
   96,   -1,   -1,   -1,   -1,   10,  262,  263,  264,   -1,
   -1,  267,  268,  269,   -1,  271,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  281,  282,   -1,  124,   -1,
  126,   -1,   -1,   -1,  290,  291,   41,  293,  294,  295,
  296,  297,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   59,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  257,  258,
  259,  260,  261,  262,  263,  264,   -1,   -1,  267,  268,
  269,  270,  271,   -1,   -1,  274,  275,  276,  277,  278,
  279,  280,   -1,   -1,  283,  284,  285,  286,  287,  288,
  289,  290,  291,  292,  293,  294,  295,  296,  297,  298,
  299,  300,  301,  302,  303,  304,  305,  306,   -1,  308,
  125,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  321,  322,  323,  324,  325,  326,   -1,  328,
  329,   -1,   -1,  332,   -1,   -1,   -1,  336,  337,  338,
  339,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  347,   -1,
  349,  257,  258,  259,  260,  261,  262,  263,  264,   -1,
   -1,  267,  268,  269,  270,  271,   -1,   -1,  274,  275,
  276,  277,  278,  279,  280,   -1,   -1,  283,  284,  285,
  286,  287,  288,  289,  290,  291,  292,  293,  294,  295,
  296,  297,  298,  299,  300,  301,  302,  303,  304,  305,
   -1,   -1,  308,   -1,   37,   38,   -1,   40,   -1,   42,
   43,   -1,   45,   -1,   47,  321,  322,  323,  324,  325,
  326,   -1,  328,  329,   -1,   -1,  332,   60,   -1,   62,
  336,  337,  338,  339,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  347,   -1,  349,   -1,   -1,   -1,  262,  263,  264,
   -1,   -1,  267,  268,  269,   -1,  271,   -1,   -1,   -1,
   -1,   94,   -1,   96,   -1,   -1,  281,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  290,  291,   -1,  293,  294,
  295,  296,  297,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  124,   -1,  126,   -1,   37,   38,   -1,   -1,   -1,
   42,   43,   -1,   45,   -1,   47,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  257,  258,  259,   -1,  261,   60,   -1,
   62,  265,  266,   -1,   -1,   -1,  270,   -1,  272,  273,
  274,  275,  276,  277,  278,   -1,   -1,   -1,   -1,  283,
  284,  285,  286,  287,  288,  289,   -1,   -1,   -1,   -1,
   -1,   -1,   94,   -1,   96,  299,   -1,   -1,  302,  303,
  304,  305,  306,  307,  308,  309,  310,  311,  312,  313,
  314,  315,  316,  317,  318,  319,  320,   -1,   -1,   -1,
   -1,   -1,  124,   -1,  126,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  341,   -1,   -1,
  344,  345,  346,  347,   -1,  349,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  257,  258,  259,  260,  261,  262,
  263,  264,   -1,   -1,  267,  268,  269,  270,  271,   -1,
   -1,  274,  275,  276,  277,  278,  279,  280,   -1,   -1,
  283,  284,  285,  286,  287,  288,  289,  290,  291,  292,
  293,  294,  295,  296,  297,  298,  299,  300,  301,  302,
  303,  304,  305,  306,  307,  308,  309,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  321,  322,
  323,  324,  325,  326,   -1,  328,  329,   -1,   -1,  332,
   -1,   -1,   -1,  336,  337,  338,  339,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  347,  257,  258,  259,  260,  261,
  262,  263,  264,   -1,   -1,  267,  268,  269,  270,  271,
   -1,   -1,  274,  275,  276,  277,  278,  279,  280,   -1,
   -1,  283,  284,  285,  286,  287,  288,  289,  290,  291,
  292,  293,  294,  295,  296,  297,  298,  299,  300,  301,
  302,  303,  304,  305,  306,  307,  308,  309,   37,   38,
   -1,   -1,   -1,   42,   43,   -1,   45,   -1,   47,  321,
  322,  323,  324,  325,  326,   -1,  328,  329,   -1,   -1,
  332,   60,   -1,   62,  336,  337,  338,  339,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  347,   37,   38,   -1,   -1,
   -1,   42,   43,   -1,   45,   -1,   47,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   94,   -1,   96,   -1,   60,
   -1,   62,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   37,   38,   -1,   -1,   -1,   42,   43,
   -1,   45,   -1,   47,   -1,  124,   -1,  126,   -1,   -1,
   -1,   -1,   -1,   94,   -1,   96,   60,   -1,   62,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   37,   38,   -1,   -1,   -1,   42,   43,   -1,   45,   -1,
   47,   -1,   -1,  124,   -1,  126,   -1,   -1,   -1,   -1,
   94,   -1,   96,   60,   -1,   62,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  124,   -1,  126,   -1,   -1,   -1,   -1,   94,   -1,   96,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  124,   -1,  126,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  257,  258,
  259,  260,  261,  262,  263,  264,   -1,   -1,  267,  268,
  269,  270,  271,   -1,   -1,  274,  275,  276,  277,  278,
  279,  280,   -1,   -1,  283,  284,  285,  286,  287,  288,
  289,  290,  291,  292,  293,  294,  295,  296,  297,  298,
  299,  300,  301,  302,  303,  304,  305,   -1,   -1,  308,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  321,  322,  323,  324,  325,  326,   -1,  328,
  329,   -1,   -1,  332,   -1,   -1,   -1,  336,  337,  338,
  339,   -1,   -1,  304,  305,   -1,   -1,  308,  347,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  321,  322,  323,  324,  325,  326,   -1,  328,  329,   -1,
   -1,  332,   -1,   -1,   -1,  336,  337,  338,  339,   -1,
  304,  305,   -1,   -1,  308,   -1,  347,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  321,  322,  323,
  324,  325,  326,   -1,  328,  329,   -1,   -1,  332,   -1,
   -1,   -1,  336,  337,  338,  339,   -1,  304,  305,   -1,
   -1,  308,   -1,  347,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  321,  322,  323,  324,  325,  326,
   -1,  328,  329,   -1,   -1,  332,   -1,   -1,   -1,  336,
  337,  338,  339,   37,   38,   -1,   -1,   -1,   42,   43,
  347,   45,   -1,   47,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   60,   -1,   62,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   37,   38,   -1,   -1,   -1,   42,   43,   -1,   45,   -1,
   47,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   94,   -1,   96,   60,   -1,   62,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   37,   38,   -1,
   -1,   -1,   42,   43,   -1,   45,   -1,   47,   -1,   -1,
  124,   -1,  126,   -1,   -1,   -1,   -1,   94,   -1,   96,
   60,   -1,   62,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   37,   38,   -1,   -1,   -1,   42,
   43,   -1,   45,   -1,   47,   -1,   -1,  124,   -1,  126,
   -1,   -1,   -1,   -1,   94,   -1,   96,   60,   -1,   62,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   37,   38,   -1,   -1,   -1,   42,   43,   -1,   45,
   -1,   47,   -1,   -1,  124,   -1,  126,   -1,   -1,   -1,
   -1,   94,   -1,   96,   60,   -1,   62,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   37,   38,
   -1,   -1,   -1,   42,   43,   -1,   45,   -1,   47,   -1,
   -1,  124,   -1,  126,   -1,   -1,   -1,   -1,   94,   -1,
   96,   60,   -1,   62,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   37,   38,   -1,   -1,   -1,
   42,   43,   -1,   45,   -1,   47,   -1,   -1,  124,   -1,
  126,   -1,   -1,   -1,   -1,   94,   -1,   96,   60,   -1,
   62,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  304,  305,   -1,   -1,  308,  124,   -1,  126,   -1,   -1,
   -1,   -1,   94,   -1,   96,   -1,   -1,  321,  322,  323,
  324,  325,  326,   -1,  328,  329,   -1,   -1,  332,   -1,
   -1,   -1,  336,  337,  338,  339,   -1,  304,  305,   -1,
   -1,  308,  124,  347,  126,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  321,  322,  323,  324,  325,  326,
   -1,  328,  329,    0,   -1,  332,   -1,   -1,   -1,  336,
  337,  338,  339,   10,  304,  305,   -1,   -1,  308,   -1,
  347,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  321,  322,  323,  324,  325,  326,   -1,  328,  329,
   -1,   -1,  332,   -1,   41,   -1,  336,  337,  338,  339,
   -1,  304,  305,   -1,   -1,  308,   -1,  347,   -1,   -1,
   -1,   -1,   59,   -1,   -1,   -1,   -1,   -1,  321,  322,
  323,  324,  325,  326,   -1,  328,  329,   -1,   -1,  332,
   -1,   -1,   -1,  336,  337,  338,  339,   -1,  304,  305,
   -1,   -1,  308,    0,  347,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   10,   -1,  321,  322,  323,  324,  325,
  326,   -1,  328,  329,   -1,   -1,  332,   -1,   -1,   -1,
  336,  337,  338,  339,   -1,  304,  305,   -1,  125,  308,
   -1,  347,   -1,   -1,   41,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  321,  322,  323,  324,  325,  326,   -1,  328,
  329,   -1,   59,  332,   -1,   -1,   -1,  336,  337,  338,
  339,   -1,  304,  305,   -1,   -1,  308,   -1,  347,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  321,
  322,  323,  324,  325,  326,   -1,  328,  329,   -1,   -1,
  332,   -1,   -1,   -1,  336,  337,  338,  339,   37,   38,
   -1,   -1,   -1,   42,   43,  347,   45,   -1,   47,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  125,   -1,
   -1,   60,   -1,   62,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,    0,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   10,   -1,   -1,   94,   -1,   96,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  262,  263,  264,   -1,   -1,
  267,  268,  269,   -1,  271,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   41,  281,  124,   44,  126,   -1,   -1,
   -1,   -1,   -1,  290,  291,   -1,  293,  294,  295,  296,
  297,   59,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   93,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  262,  263,  264,   -1,   -1,
  267,  268,  269,   -1,  271,   -1,   -1,  125,   -1,   -1,
   -1,   -1,   -1,   -1,  281,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  290,  291,   -1,  293,  294,  295,  296,
  297,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  124,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  304,  305,   -1,   -1,  308,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  321,  322,  323,  324,  325,  326,   -1,  328,
  329,   -1,   -1,  332,   -1,   -1,   -1,  336,  337,  338,
  339,   -1,   -1,   -1,  262,  263,  264,   -1,  347,  267,
  268,  269,   -1,  271,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  281,  282,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  290,  291,   -1,  293,  294,  295,  296,  297,
  257,  258,  259,   -1,  261,   -1,   -1,   -1,  265,  266,
   -1,   -1,   -1,  270,   -1,  272,  273,  274,  275,  276,
  277,  278,   -1,   -1,   -1,   -1,  283,  284,  285,  286,
  287,  288,  289,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  299,   -1,   -1,  302,  303,  304,  305,  306,
  307,  308,  309,  310,  311,  312,  313,  314,  315,  316,
  317,  318,  319,  320,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  341,   -1,   -1,  344,  345,  346,
  347,   -1,  349,  257,  258,  259,   -1,  261,   -1,   -1,
   -1,  265,  266,   -1,   -1,   -1,  270,   -1,  272,  273,
  274,  275,  276,  277,  278,   -1,   -1,   -1,   -1,  283,
  284,  285,  286,  287,  288,  289,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  299,   -1,   -1,  302,  303,
  304,  305,  306,  307,  308,  309,  310,  311,  312,  313,
  314,  315,  316,  317,  318,  319,  320,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  341,   -1,   -1,
  344,  345,  346,  347,   -1,  349,  257,  258,  259,   -1,
  261,   -1,   -1,   -1,  265,  266,   -1,   -1,   -1,  270,
   -1,  272,  273,  274,  275,  276,  277,  278,   -1,   -1,
   -1,   -1,  283,  284,  285,  286,  287,  288,  289,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  299,   -1,
   -1,  302,  303,  304,  305,  306,  307,  308,  309,  310,
  311,  312,  313,  314,  315,  316,  317,  318,  319,  320,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  341,   -1,   -1,  344,  345,  346,   -1,   -1,  349,
  };

//line 1870 "parse.y"
        RNode block_append(RNode head, RNode tail)
        {
            return RNode.block_append(thread, head, tail);
        }
        
        RNode node_assign(RNode lhs, RNode rhs)
        {
            if (lhs == null) return null;

            value_expr(rhs);
            if (lhs is RNGAsgn ||
                lhs is RNIAsgn ||
                lhs is RNLAsgn ||
                lhs is RNDAsgn ||
                lhs is RNDAsgnCurr ||
                lhs is RNMAsgn ||
                lhs is RNCDecl ||
                lhs is RNCVDecl ||
                lhs is RNCVAsgn)
            {
                lhs.val = rhs;
            }
            else if (lhs is RNCall)
            {
                lhs.args = arg_add(lhs.args, rhs);
            }
            if (rhs != null)
            {
                lhs.FixPos(rhs);
            }
            return lhs;
        }
    
        bool value_expr(RNode node)
        {
            if (node == null) return true;

            if (node is RNReturn ||
                node is RNBreak ||
                node is RNNext ||
                node is RNRedo ||
                node is RNRetry ||
                node is RNWhile ||
                node is RNUntil ||
                node is RNClass ||
                node is RNModule ||
                node is RNDefn ||
                node is RNDefs)
            {
                yyerror("void value expression");
                return false;
            }
            else if (node is RNBlock)
            {
                while (node.next != null) {
                    node = node.next;
                }
                return value_expr(node.head);
            }
            else if (node is RNBegin)
            {
                return value_expr(node.body);
            }
            else if (node is RNIf)
            {
                return value_expr(node.body) && value_expr(node.nd_else);
            }
            else if (node is RNNewLine)
            {
                return value_expr(node.next);
            }
            return true;
        }
        
        void void_expr(RNode node)
        {
            string useless = "";
            if (!ruby.verbose) return;
            if (node == null) return;
        again:
            if (node is RNNewLine)
            {
                node = node.next;
                goto again;
            }
            else if (node is RNCall)
            {
                switch (node.mid) {
                case '+':
                case '-':
                case '*':
                case '/':
                case '%':
                case Token.tPOW:
                case Token.tUPLUS:
                case Token.tUMINUS:
                case '|':
                case '^':
                case '&':
                case Token.tCMP:
                case '>':
                case Token.tGEQ:
                case '<':
                case Token.tLEQ:
                case Token.tEQ:
                case Token.tNEQ:
                    useless = id2name(node.mid);
                    break;
                }
            }
            else if (node is RNLVar ||
                     node is RNDVar ||
                     node is RNIVar ||
                     node is RNCVar ||
                     node is RNNthRef ||
                     node is RNBackRef)
            {
                useless = "a variable";
            }
            else if (node is RNConst /*||
                     node is RNCRef*/)
            {
                useless = "a constant";
            }
            else if (node is RNLit ||
                     node is RNStr ||
                     node is RNDStr ||
                     node is RNDRegx ||
                     node is RNDRegxOnce)
            {
                useless = "a literal";
            }
            else if (node is RNColon2 ||
                     node is RNColon3)
            {
                useless = "::";
            }
            else if (node is RNDot2)
            {
                useless = "..";
            }
            else if (node is RNDot3)
            {
                useless = "...";
            }
            else if (node is RNSelf)
            {
                useless = "self";
            }
            else if (node is RNNil)
            {
                useless = "nil";
            }
            else if (node is RNTrue)
            {
                useless = "true";
            }
            else if (node is RNFalse)
            {
                useless = "false";
            }
            else if (node is RNDefined)
            {
                useless = "defined?";
            }

            if (useless.Length > 0) {
                int line = sourceline;
                sourceline = node.Line;
                ruby.warn("useless use of {0} in void context", useless);
                sourceline = line;
            }
        }

        bool assign_in_cond(RNode node)
        {
            if (node is RNMAsgn)
            {
                yyerror("multiple assignment in conditional");
                return true;
            }
            else if (node is RNLAsgn ||
                     node is RNDAsgn ||
                     node is RNGAsgn ||
                     node is RNIAsgn)
            {
            }
            else if (node is RNNewLine)
            {
                return false;
            }
            else
            {
                return false;
            }

            RNode rn = node.val;
            if (rn is RNLit ||
                rn is RNStr ||
                rn is RNNil ||
                rn is RNTrue ||
                rn is RNFalse)
            {
                /* reports always */
                ruby.warn("found = in conditional, should be ==");
                return true;
            }
            else if (rn is RNDStr ||
                     rn is RNXStr ||
                     rn is RNDXStr ||
                     rn is RNEvStr ||
                     rn is RNDRegx)
            {
            }
            return true;
        }
        
        void void_stmts(RNode node)
        {
            if (!ruby.verbose) return;
            if (node == null) return;
            if (node is RNBlock == false) return;

            for (;;) {
                if (node.next == null) return;
                void_expr(node.head);
                node = node.next;
            }
        }

        RNode call_op(RNode recv, uint id, int narg, RNode arg1)
        {
            value_expr(recv);
            if (narg == 1) {
                value_expr(arg1);
            }
            return new RNCall(thread, recv, id, narg==1?new RNArray(thread, arg1):null);
        }
        
        RNode cond0(RNode node)
        {
            // check bad assignment
            assign_in_cond(node);

            if (node is RNDRegx ||
                node is RNDRegxOnce)
            {
                thread.LocalCnt('_');
                thread.LocalCnt('~');
                return new RNMatch2(thread, node, new RNGVar(thread, ruby, intern("$_")));
            }
            else if (node is RNDot2 || node is RNDot3)
            {
                node.beg = cond2(node.beg);
                node.end = cond2(node.end);
                if (node is RNDot2)
                {
                    node = new RNFlip2(node);
                }
                else
                {
                    node = new RNFlip3(node);
                }
                node.cnt = (int)thread.LocalAppend(0);
                return node;
            }
            else if (node is RNLit)
            {
                if (node.lit is RRegexp) {
                    thread.LocalCnt('_');
                    thread.LocalCnt('~');
                    return new RNMatch(thread, node);
                }
                if (node.lit is string || node.lit is RString) {
                    thread.LocalCnt('_');
                    thread.LocalCnt('~');
                    return new RNMatch(thread, RRegexpClass.s_new(ruby.cRegexp, node.lit));
                }
            }
            return node;
        }
        
        RNode cond(RNode node)
        {
            if (node == null) return null;
            if (node is RNNewLine)
            {
                node = cond0(node.next);
                return node;
            }
            return cond0(node);
        }

        RNode cond2(RNode node)
        {
            node = cond(node);
            if (node is RNNewLine)
            {
                node = node.next;
            }
            else if (node is RNLit && (node.lit is int || node.lit is Decimal))
            {
                return call_op(node,Token.tEQ,1,new RNGVar(thread, ruby, intern("$.")));
            }
            return node;
        }

        RNode logop(Type tp, RNode left, RNode right)
        {
            value_expr(left);
            BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic
                | BindingFlags.InvokeMethod;
            ConstructorInfo ctr = tp.GetConstructor(bf, null,
                 new Type[] {typeof(RThread), typeof(RNode), typeof(RNode)}, null);
            return (RNode)ctr.Invoke(new object[] {thread, cond(left), cond(right)});
        }

        RNode arg_blk_pass(RNode node1, RNode node2)
        {
            if (node2 != null)
            {
                node2.head = node1;
                return node2;
            }
            return node1;
        }

        RNode new_call(RNode r, uint m, RNode a)
        {
            if (a != null && a is RNBlockPass) {
                a.iter = new RNCall(thread, r, m, a.head);
                return a;
            }
            return new RNCall(thread, r, m, a);
        }

        RNode new_fcall(uint m, RNode a)
        {
            if (a != null && a is RNBlockPass) {
                a.iter = new RNFCall(thread, m, a.head);
                return a;
            }
            return new RNFCall(thread, m, a);
        }

        RNode new_super(RNode a)
        {
            if (a != null && a is RNBlockPass) {
                a.iter = new RNSuper(thread, a.head);
                return a;
            }
            return new RNSuper(thread, a);
        }

        RNode aryset(RNode recv, RNode idx)
        {
            value_expr(recv);
            return new RNCall(thread, recv, Token.tASET, idx);
        }

        static internal uint id_attrset(uint id)
        {
            id &= ~(uint)ID.SCOPE_MASK;
            id |= (uint)ID.ATTRSET;
            return id;
        }
        
        RNode attrset(RNode recv, uint id)
        {
            value_expr(recv);

            return new RNCall(thread, recv, id_attrset(id), null);
        }

        void backref_error(RNode node)
        {
            if (node is RNNthRef)
            {
                thread.CompileError("Can't set variable $" + node.nth.ToString());
            }
            else if (node is RNBackRef)
            {
                thread.CompileError("Can't set variable $" + node.nth.ToString());
            }
        }
        
        RNode arg_concat(RNode node1, RNode node2)
        {
            if (node2 == null) return node1;
            return new RNArgsCat(thread, node1, node2);
        }

        RNode arg_add(RNode node1, RNode node2)
        {
            if (node1 == null) return new RNArray(thread, node2);
            if (node1 is RNArray)
            {
                return ((RNArray)node1).append(thread, node2);
            }
            else {
                return new RNArgsPush(thread, node1, node2);
            }
        }

        static public bool is_notop_id(uint id)
        {
            return (id > Token.LAST_TOKEN);
        }
        static public bool is_local_id(uint id)
        {
            return (id > Token.LAST_TOKEN && ((id & (uint)ID.SCOPE_MASK) == (uint)ID.LOCAL));
        }
        static public uint make_const_id(uint id)
        {
            return (id & ~(uint)ID.SCOPE_MASK) | (uint)ID.CONST;
        }           
        static public bool is_global_id(uint id)
        {
            return (id > Token.LAST_TOKEN && ((id & (uint)ID.SCOPE_MASK) == (uint)ID.GLOBAL));
        }
        static public bool is_instance_id(uint id)
        {
            return (id > Token.LAST_TOKEN && ((id & (uint)ID.SCOPE_MASK) == (uint)ID.INSTANCE));
        }
        static public bool is_attrset_id(uint id)
        {
            return (id > Token.LAST_TOKEN && ((id & (uint)ID.SCOPE_MASK) == (uint)ID.ATTRSET));
        }
        static public uint make_local_id(uint id)
        {
            return (id & ~(uint)ID.SCOPE_MASK) | (uint)ID.LOCAL;
        }
        static public bool is_const_id(uint id)
        {
            return (id > Token.LAST_TOKEN && ((id & (uint)ID.SCOPE_MASK) == (uint)ID.CONST));
        }
        static public bool is_class_id(uint id)
        {
            return (id > Token.LAST_TOKEN && ((id & (uint)ID.SCOPE_MASK) == (uint)ID.CLASS));
        }

        RNode match_gen(RNode node1, RNode node2)
        {
            thread.LocalCnt('~');

            if (node1 is RNDRegx || node1 is RNDRegxOnce)
            {
                return new RNMatch2(thread, node1, node2);
            }
            else if (node1 is RNLit)
            {
                if (node1.lit is Regex)
                {
                    return new RNMatch2(thread, node1, node2);
                }
            }

            if (node2 is RNDRegx || node2 is RNDRegxOnce)
            {
                return new RNMatch3(thread, node2, node1);
            }
            else if (node2 is RNLit)
            {
                if (node2.lit is Regex) {
                    return new RNMatch3(thread, node2, node1);
                }
            }

            return new RNCall(thread,node1, Token.tMATCH, new RNArray(thread, node2));
        }

        RNode gettable(uint id)
        {
            if (id == Token.kSELF) {
                return new RNSelf(thread);
            }
            else if (id == Token.kNIL) {
                return new RNNil(thread);
            }
            else if (id == Token.kTRUE) {
                return new RNTrue(thread);
            }
            else if (id == Token.kFALSE) {
                return new RNFalse(thread);
            }
            else if (id == Token.k__FILE__) {
                RString f = new RString(ruby, sourcefile);
                f.Freeze();
                return new RNStr(thread, ruby, f);
            }
            else if (id == Token.k__LINE__) {
                return new RNLit(thread, sourceline);
            }
            else if (is_local_id(id)) {
                if (thread.IsDynaInBlock && thread.dvar_defined(id))
                    return new RNDVar(thread, id);
                if (thread.LocalID(id)) return new RNLVar(thread, id);
                /* method call without arguments */
                return new RNVCall(thread, id);
            }
            else if (is_global_id(id)) {
                return new RNGVar(thread, ruby, id);
            }
            else if (is_instance_id(id)) {
                return new RNIVar(thread, id);
            }
            else if (is_const_id(id)) {
                return new RNConst(thread, id);
            }
            else if (is_class_id(id)) {
                if (in_single > 0) return new RNCVar2(thread, id);
                return new RNCVar(thread, id);
            }
            ruby.bug("invalid id for gettable");
            return null;
        }


        RNode assignable(uint id, RNode val)
        {
            value_expr(val);
            if (id == Token.kSELF) {
                yyerror("Can't change the value of self");
            }
            else if (id == Token.kNIL) {
                yyerror("Can't assign to nil");
            }
            else if (id == Token.kTRUE) {
                yyerror("Can't assign to true");
            }
            else if (id == Token.kFALSE) {
                yyerror("Can't assign to false");
            }
            else if (id == Token.k__FILE__) {
                yyerror("Can't assign to __FILE__");
            }
            else if (id == Token.k__LINE__) {
                yyerror("Can't assign to __LINE__");
            }
            else if (is_local_id(id)) {
                if (thread.dvar_curr(id)) {
                    return new RNDAsgnCurr(thread, id, val);
                }
                else if (thread.dvar_defined(id)) {
                    return new RNDAsgn(thread, id, val);
                }
                else if (thread.LocalID(id) || !thread.IsDynaInBlock) {
                    return new RNLAsgn(thread, id, val);
                }
                else{
                    thread.dvar_push(id, null);
                    return new RNDAsgn(thread, id, val);
                }
            }
            else if (is_global_id(id)) {
                return new RNGAsgn(thread, ruby, id, val);
            }
            else if (is_instance_id(id)) {
                return new RNIAsgn(thread, id, val);
            }
            else if (is_const_id(id)) {
                if (in_def > 0 || in_single > 0)
                    yyerror("dynamic constant assignment");
                return new RNCDecl(thread, id, val);
            }
            else if (is_class_id(id)) {
                if (in_single > 0) return new RNCVAsgn(thread, id, val);
                return new RNCVDecl(thread, id, val);
            }
            else {
                ruby.bug("bad id for variable");
            }
            return null;
        }
        
        private uint intern(string name)
        {
            return ruby.intern(name);
        }

        private string id2name(uint id)
        {
            if (id < Token.LAST_TOKEN) {
                int i = 0;

                for (i=0; i < NetRuby.op_tbl.Length; i++) {
                    if (NetRuby.op_tbl[i].token == id)
                        return NetRuby.op_tbl[i].name;
                }
            }
            string name = null;

            if (ruby.sym_rev_tbl.lookup(id, out name))
                return name;

            if (is_attrset_id(id)) {
                uint id2 = make_local_id(id);

            again:
                name = id2name(id2);
                if (name != null) {
                    string buf = name + "=";
                    intern(name + "=");
                    return id2name(id);
                }
                if (is_local_id(id2)) {
                    id2 = make_const_id(id);
                    goto again;
                }
            }
            return null;
        }

        
    }
 
//line default
namespace yydebug {
        using System;
	 public interface yyDebug {
		 void push (int state, Object value);
		 void lex (int state, int token, string name, Object value);
		 void shift (int from, int to, int errorFlag);
		 void pop (int state);
		 void discard (int state, int token, string name, Object value);
		 void reduce (int from, int to, int rule, string text, int len);
		 void shift (int from, int to);
		 void accept (Object value);
		 void error (string message);
		 void reject ();
	 }
	 
	 class yyDebugSimple : yyDebug {
		 void println (string s){
			 Console.WriteLine (s);
		 }
		 
		 public void push (int state, Object value) {
			 println ("push\tstate "+state+"\tvalue "+value);
		 }
		 
		 public void lex (int state, int token, string name, Object value) {
			 println("lex\tstate "+state+"\treading "+name+"\tvalue "+value);
		 }
		 
		 public void shift (int from, int to, int errorFlag) {
			 switch (errorFlag) {
			 default:				// normally
				 println("shift\tfrom state "+from+" to "+to);
				 break;
			 case 0: case 1: case 2:		// in error recovery
				 println("shift\tfrom state "+from+" to "+to
					     +"\t"+errorFlag+" left to recover");
				 break;
			 case 3:				// normally
				 println("shift\tfrom state "+from+" to "+to+"\ton error");
				 break;
			 }
		 }
		 
		 public void pop (int state) {
			 println("pop\tstate "+state+"\ton error");
		 }
		 
		 public void discard (int state, int token, string name, Object value) {
			 println("discard\tstate "+state+"\ttoken "+name+"\tvalue "+value);
		 }
		 
		 public void reduce (int from, int to, int rule, string text, int len) {
			 println("reduce\tstate "+from+"\tuncover "+to
				     +"\trule ("+rule+") "+text);
		 }
		 
		 public void shift (int from, int to) {
			 println("goto\tfrom state "+from+" to "+to);
		 }
		 
		 public void accept (Object value) {
			 println("accept\tvalue "+value);
		 }
		 
		 public void error (string message) {
			 println("error\t"+message);
		 }
		 
		 public void reject () {
			 println("reject");
		 }
		 
	 }
}
// %token constants
 class Token {
  public const int kCLASS = 257;
  public const int kMODULE = 258;
  public const int kDEF = 259;
  public const int kUNDEF = 260;
  public const int kBEGIN = 261;
  public const int kRESCUE = 262;
  public const int kENSURE = 263;
  public const int kEND = 264;
  public const int kIF = 265;
  public const int kUNLESS = 266;
  public const int kTHEN = 267;
  public const int kELSIF = 268;
  public const int kELSE = 269;
  public const int kCASE = 270;
  public const int kWHEN = 271;
  public const int kWHILE = 272;
  public const int kUNTIL = 273;
  public const int kFOR = 274;
  public const int kBREAK = 275;
  public const int kNEXT = 276;
  public const int kREDO = 277;
  public const int kRETRY = 278;
  public const int kIN = 279;
  public const int kDO = 280;
  public const int kDO_COND = 281;
  public const int kDO_BLOCK = 282;
  public const int kRETURN = 283;
  public const int kYIELD = 284;
  public const int kSUPER = 285;
  public const int kSELF = 286;
  public const int kNIL = 287;
  public const int kTRUE = 288;
  public const int kFALSE = 289;
  public const int kAND = 290;
  public const int kOR = 291;
  public const int kNOT = 292;
  public const int kIF_MOD = 293;
  public const int kUNLESS_MOD = 294;
  public const int kWHILE_MOD = 295;
  public const int kUNTIL_MOD = 296;
  public const int kRESCUE_MOD = 297;
  public const int kALIAS = 298;
  public const int kDEFINED = 299;
  public const int klBEGIN = 300;
  public const int klEND = 301;
  public const int k__LINE__ = 302;
  public const int k__FILE__ = 303;
  public const int tIDENTIFIER = 304;
  public const int tFID = 305;
  public const int tGVAR = 306;
  public const int tIVAR = 307;
  public const int tCONSTANT = 308;
  public const int tCVAR = 309;
  public const int tINTEGER = 310;
  public const int tFLOAT = 311;
  public const int tSTRING = 312;
  public const int tXSTRING = 313;
  public const int tREGEXP = 314;
  public const int tDSTRING = 315;
  public const int tDXSTRING = 316;
  public const int tDREGEXP = 317;
  public const int tNTH_REF = 318;
  public const int tBACK_REF = 319;
  public const int tQWORDS = 320;
  public const int tUPLUS = 321;
  public const int tUMINUS = 322;
  public const int tPOW = 323;
  public const int tCMP = 324;
  public const int tEQ = 325;
  public const int tEQQ = 326;
  public const int tNEQ = 327;
  public const int tGEQ = 328;
  public const int tLEQ = 329;
  public const int tANDOP = 330;
  public const int tOROP = 331;
  public const int tMATCH = 332;
  public const int tNMATCH = 333;
  public const int tDOT2 = 334;
  public const int tDOT3 = 335;
  public const int tAREF = 336;
  public const int tASET = 337;
  public const int tLSHFT = 338;
  public const int tRSHFT = 339;
  public const int tCOLON2 = 340;
  public const int tCOLON3 = 341;
  public const int tOP_ASGN = 342;
  public const int tASSOC = 343;
  public const int tLPAREN = 344;
  public const int tLBRACK = 345;
  public const int tLBRACE = 346;
  public const int tSTAR = 347;
  public const int tAMPER = 348;
  public const int tSYMBEG = 349;
  public const int LAST_TOKEN = 350;
  public const int yyErrorCode = 256;
 }
 namespace yyParser {
  using System;
  /** thrown for irrecoverable syntax errors and stack overflow.
    */
  public class yyException : System.Exception {
    public yyException (string message) : base (message) {
    }
  }

  /** must be implemented by a scanner object to supply input to the parser.
    */
  public interface yyInput {
    /** move on to next token.
        @return false if positioned beyond tokens.
        @throws IOException on input error.
      */
    bool advance (); // throws java.io.IOException;
    /** classifies current token.
        Should not be called if advance() returned false.
        @return current %token or single character.
      */
    int token ();
    /** associated with current token.
        Should not be called if advance() returned false.
        @return value for token().
      */
    Object value ();
  }
 }
} // close outermost namespace, that MUST HAVE BEEN opened in the prolog
