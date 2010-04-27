/*
NetRuby (C# Parser for Jay)

Copyright (C) 1993-2001 Yukihiro Matsumoto
Copyright (C) 2001-2002 arton
Copyright (C) 2005      Jaen Saul
*/

// vim:et:sts=4:sw=4

%{
    
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

%}

%token  kCLASS
        kMODULE
        kDEF
        kUNDEF
        kBEGIN
        kRESCUE
        kENSURE
        kEND
        kIF
        kUNLESS
        kTHEN
        kELSIF
        kELSE
        kCASE
        kWHEN
        kWHILE
        kUNTIL
        kFOR
        kBREAK
        kNEXT
        kREDO
        kRETRY
        kIN
        kDO
        kDO_COND
        kDO_BLOCK
        kRETURN
        kYIELD
        kSUPER
        kSELF
        kNIL
        kTRUE
        kFALSE
        kAND
        kOR
        kNOT
        kIF_MOD
        kUNLESS_MOD
        kWHILE_MOD
        kUNTIL_MOD
        kRESCUE_MOD
        kALIAS
        kDEFINED
        klBEGIN
        klEND
        k__LINE__
        k__FILE__

%token <uint>   tIDENTIFIER tFID tGVAR tIVAR tCONSTANT tCVAR
%token <object>  tINTEGER tFLOAT tSTRING tXSTRING tREGEXP
%token <RNode> tDSTRING tDXSTRING tDREGEXP tNTH_REF tBACK_REF tQWORDS

%type <RNode> singleton string
%type <object>  literal numeric
%type <RNode> compstmt stmts stmt expr arg primary command command_call method_call
%type <RNode> if_tail opt_else case_body cases rescue exc_list exc_var ensure
%type <RNode> args ret_args when_args call_args paren_args opt_paren_args
%type <RNode> command_args aref_args opt_block_arg block_arg var_ref
%type <RNode> mrhs mrhs_basic superclass block_call block_command
%type <RNode> f_arglist f_args f_optarg f_opt f_block_arg opt_f_block_arg
%type <RNode> assoc_list assocs assoc undef_list backref
%type <RNode> block_var opt_block_var brace_block do_block lhs none
%type <RNode> mlhs mlhs_head mlhs_basic mlhs_entry mlhs_item mlhs_node
%type <uint>   fitem variable sym symbol operation operation2 operation3
%type <uint>   cname fname op f_rest_arg
%type <int>  f_norm_arg f_arg
%token tUPLUS     /* unary+ */
%token tUMINUS   /* unary- */
%token tPOW          /* ** */
%token tCMP        /* <=> */
%token tEQ          /* == */
%token tEQQ        /* === */
%token tNEQ        /* != */
%token tGEQ        /* >= */
%token tLEQ        /* <= */
%token tANDOP tOROP     /* && and || */
%token tMATCH tNMATCH   /* =~ and !~ */
%token tDOT2 tDOT3      /* .. and ... */
%token tAREF tASET      /* [] and []= */
%token tLSHFT tRSHFT    /* << and >> */
%token tCOLON2    /* :: */
%token tCOLON3    /* :: at EXPR.BEG */
%token <int> tOP_ASGN  /* +=, -=  etc. */
%token tASSOC      /* => */
%token tLPAREN    /* ( */
%token tLBRACK    /* [ */
%token tLBRACE    /* { */
%token tSTAR        /* * */
%token tAMPER      /* & */
%token tSYMBEG

/*
 *      precedence table
 */

%left  kIF_MOD kUNLESS_MOD kWHILE_MOD kUNTIL_MOD kRESCUE_MOD
%left  kOR kAND
%right kNOT
%nonassoc kDEFINED
%right '=' tOP_ASGN
%right '?' ':'
%nonassoc tDOT2 tDOT3
%left  tOROP
%left  tANDOP
%nonassoc  tCMP tEQ tEQQ tNEQ tMATCH tNMATCH
%left  '>' tGEQ '<' tLEQ
%left  '|' '^'
%left  '&'
%left  tLSHFT tRSHFT
%left  '+' '-'
%left  '*' '/' '%'
%right '!' '~' tUPLUS tUMINUS
%right tPOW

%token LAST_TOKEN

%%
program  :  {
                        $$ = thread.dyna_vars;
                        lex.State = EXPR.BEG;
                        thread.TopLocalInit();
                        /*
                        if ((VALUE)ruby_class == rb_cObject) class_nest = 0;
                        else class_nest = 1;
                        */
                    }
                  compstmt
                    {
                        if ($2 != null && !compile_for_eval) {
                            /* last expression should not be void */
                            if ($2 is RNBlock)
                            {
                                RNode node = $2;
                                while (node.next != null) {
                                    node = node.next;
                                }
                                void_expr(node.head);
                            }
                            else
                            {
                                void_expr($2);
                            }
                        }
                        evalTree = block_append(evalTree, $2);
                        thread.TopLocalSetup();
                        class_nest = 0;
                        thread.dyna_vars = $<RVarmap>1;
                    }

compstmt        : stmts opt_terms
                    {
                        void_stmts($1);
                        $$ = $1;
                    }

stmts      : none
                | stmt
                    {
                        $$ = new RNNewLine(thread, $1);
                    }
                | stmts terms stmt
                    {
                        $$ = block_append($1, new RNNewLine(thread, $3));
                    }
                | error stmt
                    {
                        $$ = $2;
                    }

stmt        : kALIAS fitem {lex.State = EXPR.FNAME;} fitem
                    {
                        if (in_def != 0 || in_single != 0)
                            yyerror("alias within method");
                        $$ = new RNAlias(thread, $2, $4);
                    }
                | kALIAS tGVAR tGVAR
                    {
                        if (in_def != 0 || in_single != 0)
                            yyerror("alias within method");
                        $$ = new RNVAlias(thread, $2, $3);
                    }
                | kALIAS tGVAR tBACK_REF
                    {
                        if (in_def != 0 || in_single != 0)
                            yyerror("alias within method");
                        string buf = "$" + Convert.ToChar($3.nth);
                        $$ = new RNVAlias(thread, $2, intern(buf));
                    }
                | kALIAS tGVAR tNTH_REF
                    {
                        yyerror("can't make alias for the number variables");
                        $$ = null;
                    }
                | kUNDEF undef_list
                    {
                        if (in_def != 0 || in_single != 0)
                            yyerror("undef within method");
                        $$ = $2;
                    }
                | stmt kIF_MOD expr
                    {
                        value_expr($3);
                        $$ = new RNIf(thread, cond($3), $1);
                    }
                | stmt kUNLESS_MOD expr
                    {
                        value_expr($3);
                        $$ = new RNUnless(thread, cond($3), $1);
                    }
                | stmt kWHILE_MOD expr
                    {
                        value_expr($3);
                        if ($1 != null && $1 is RNBegin) {
                            $$ = new RNWhile(thread, cond($3), $1.body, false);
                        }
                        else {
                            $$ = new RNWhile(thread, cond($3), $1, true);
                        }
                    }
                | stmt kUNTIL_MOD expr
                    {
                        value_expr($3);
                        if ($1 != null && $1 is RNBegin) {
                            $$ = new RNUntil(thread, cond($3), $1.body, false);
                        }
                        else {
                            $$ = new RNUntil(thread, cond($3), $1, true);
                        }
                    }
                | stmt kRESCUE_MOD stmt
                    {
                        $$ = new RNRescue(thread, $1, new RNResBody(thread, null, $3, null), null);
                    }
                | klBEGIN
                    {
                        if (in_def != 0 || in_single != 0) {
                            yyerror("BEGIN in method");
                        }
                        thread.LocalPush();
                    }
                  '{' compstmt '}'
                    {
                        thread.evalTreeBegin = block_append(thread.evalTreeBegin,
                                                     new RNPreExe(thread, $4));
                        thread.LocalPop();
                        $$ = null;
                    }
                | klEND '{' compstmt '}'
                    {
                        if (compile_for_eval && (in_def != 0|| in_single != 0)) {
                            yyerror("END in method; use at_exit");
                        }

                        $$ = new RNIter(thread, null, new RNPostExe(thread), $3);
                    }
               
                | lhs '=' command_call
                    {
                        value_expr($3);
                        $$ = node_assign($1, $3);
                    }
                | mlhs '=' command_call
                    {
                        value_expr($3);
                        $1.val = $3;
                        $$ = $1;
                    }
                | lhs '=' mrhs_basic
                    {
                        $$ = node_assign($1, $3);
                    }
                | expr

expr        : mlhs '=' mrhs
                    {
                        // value_expr($3); // nakada ruby-dev:15905
                        $1.val = $3;
                        $$ = $1;
                    }
                | kRETURN ret_args
                    {
                        if (!compile_for_eval && in_def != 0 && in_single != 0)
                            yyerror("return appeared outside of method");
                        $$ = new RNReturn(thread, $2);
                    }
                | command_call
                | expr kAND expr
                    {
                        $$ = logop(typeof(RNAnd), $1, $3);
                    }
                | expr kOR expr
                    {
                        $$ = logop(typeof(RNOr), $1, $3);
                    }
                | kNOT expr
                    {
                        value_expr($2);
                        $$ = new RNNot(thread, cond($2));
                    }
                | '!' command_call
                    {
                        $$ = new RNNot(thread, cond($2));
                    }
                | arg

command_call    : command
                | block_command

block_command   : block_call
                | block_call '.' operation2 command_args
                    {
                        value_expr($1);
                        $$ = new_call($1, $3, $4);
                    }
                | block_call tCOLON2 operation2 command_args
                    {
                        value_expr($1);
                        $$ = new_call($1, $3, $4);
                    }

command  :  operation command_args
                    {
                        $$ = new_fcall($1, $2);
                        $<RNode>$.FixPos($2);
                   }
                | primary '.' operation2 command_args
                    {
                        value_expr($1);
                        $$ = new_call($1, $3, $4);
                        $<RNode>$.FixPos($1);
                    }
                | primary tCOLON2 operation2 command_args
                    {
                        value_expr($1);
                        $$ = new_call($1, $3, $4);
                        $<RNode>$.FixPos($1);
                    }
                | kSUPER command_args
                    {
                        if (!compile_for_eval && in_def == 0 && in_single == 0)
                            yyerror("super called outside of method");
                        $$ = new_super($2);
                        $<RNode>$.FixPos($2);
                    }
                | kYIELD ret_args
                    {
                        $$ = new RNYield(thread, $2);
                        $<RNode>$.FixPos($2);
                    }

mlhs        : mlhs_basic
                | tLPAREN mlhs_entry ')'
                    {
                        $$ = $2;
                    }

mlhs_entry      : mlhs_basic
                | tLPAREN mlhs_entry ')'
                    {
                        $$ = new RNMAsgn(thread, new RNArray(thread, $2));
                    }

mlhs_basic      : mlhs_head
                    {
                        $$ = new RNMAsgn(thread, $1);
                    }
                | mlhs_head mlhs_item
                    {
                        $$ = new RNMAsgn(thread, RNode.list_append(thread,$1,$2));
                    }
                | mlhs_head tSTAR mlhs_node
                    {
                        $$ = new RNMAsgn(thread, $1, $3);
                    }
                | mlhs_head tSTAR
                    {
                        $$ = new RNMAsgn(thread, $1, -1);
                    }
                | tSTAR mlhs_node
                    {
                        $$ = new RNMAsgn(thread, null, $2);
                    }
                | tSTAR
                    {
                        $$ = new RNMAsgn(thread, null, -1);
                    }

mlhs_item       : mlhs_node
                | tLPAREN mlhs_entry ')'
                    {
                        $$ = $2;
                    }

mlhs_head       : mlhs_item ','
                    {
                        $$ = new RNArray(thread, $1);
                    }
                | mlhs_head mlhs_item ','
                    {
                        $$ = RNode.list_append(thread, $1, $2);
                    }

mlhs_node       : variable
                    {
                        $$ = assignable($1, null);
                    }
                | primary '[' aref_args ']'
                    {
                        $$ = aryset($1, $3);
                    }
                | primary '.' tIDENTIFIER
                    {
                        $$ = attrset($1, $3);
                    }
                | primary tCOLON2 tIDENTIFIER
                    {
                        $$ = attrset($1, $3);
                    }
                | primary '.' tCONSTANT
                    {
                        $$ = attrset($1, $3);
                    }
                | backref
                    {
                        backref_error($1);
                        $$ = null;
                    }

lhs          : variable
                    {
                        $$ = assignable($1, null);
                    }
                | primary '[' aref_args ']'
                    {
                        $$ = aryset($1, $3);
                    }
                | primary '.' tIDENTIFIER
                    {
                        $$ = attrset($1, $3);
                    }
                | primary tCOLON2 tIDENTIFIER
                    {
                        $$ = attrset($1, $3);
                    }
                | primary '.' tCONSTANT
                    {
                        $$ = attrset($1, $3);
                    }
                | backref
                    {
                        backref_error($1);
                        $$ = null;
                    }

cname      : tIDENTIFIER
                    {
                        yyerror("class/module name must be CONSTANT");
                    }
                | tCONSTANT

fname      : tIDENTIFIER
                | tCONSTANT
                | tFID
                | op
                    {
                        lex.State = EXPR.END;
                        if ($<object>1 is int)
                            $$ = $<int>1;
                        else
                            $$ = $<char>1;
                    }
                | reswords
                    {
                        lex.State = EXPR.END;
                        $$ = $<uint>1;
                    }

fitem      : fname
                | symbol

undef_list      : fitem
                    {
                        $$ = new RNUndef(thread, $1);
                    }
                | undef_list ',' {lex.State = EXPR.FNAME;} fitem
                    {
                        $$ = block_append($1, new RNUndef(thread, $4));
                    }

op            : '|'  { $$ = '|'; }
                | '^'      { $$ = '^'; }
                | '&'      { $$ = '&'; }
                | tCMP    { $$ = Token.tCMP; }
                | tEQ      { $$ = Token.tEQ; }
                | tEQQ    { $$ = Token.tEQQ; }
                | tMATCH        { $$ = Token.tMATCH; }
                | '>'      { $$ = '>'; }
                | tGEQ    { $$ = Token.tGEQ; }
                | '<'      { $$ = '<'; }
                | tLEQ    { $$ = Token.tLEQ; }
                | tLSHFT        { $$ = Token.tLSHFT; }
                | tRSHFT        { $$ = Token.tRSHFT; }
                | '+'      { $$ = '+'; }
                | '-'      { $$ = '-'; }
                | '*'      { $$ = '*'; }
                | tSTAR  { $$ = '*'; }
                | '/'      { $$ = '/'; }
                | '%'      { $$ = '%'; }
                | tPOW    { $$ = Token.tPOW; }
                | '~'      { $$ = '~'; }
                | tUPLUS        { $$ = Token.tUPLUS; }
                | tUMINUS       { $$ = Token.tUMINUS; }
                | tAREF  { $$ = Token.tAREF; }
                | tASET  { $$ = Token.tASET; }
                | '`'      { $$ = '`'; }

reswords        : k__LINE__ | k__FILE__  | klBEGIN | klEND
                | kALIAS | kAND | kBEGIN | kBREAK | kCASE | kCLASS | kDEF
                | kDEFINED | kDO | kELSE | kELSIF | kEND | kENSURE | kFALSE
                | kFOR | kIF_MOD | kIN | kMODULE | kNEXT | kNIL | kNOT
                | kOR | kREDO | kRESCUE | kRETRY | kRETURN | kSELF | kSUPER
                | kTHEN | kTRUE | kUNDEF | kUNLESS_MOD | kUNTIL_MOD | kWHEN
                | kWHILE_MOD | kYIELD | kRESCUE_MOD

arg          : lhs '=' arg
                    {
                        value_expr($3);
                        $$ = node_assign($1, $3);
                    }
                | variable tOP_ASGN {$$ = assignable($1, null);} arg
                    {
                        uint id = (uint)$2;
                        if ($<RNode>3 != null) {
                            if (id == Token.tOROP) {
                                $<RNode>3.val = $4;
                                $$ = new RNOpAsgnOr(thread, gettable($1), $<RNode>3);
                                if (is_instance_id($1)) {
                                    $<RNode>$.aid = $1;
                                }
                            }
                            else if (id == Token.tANDOP) {
                                $<RNode>3.val = $4;
                                $$ = new RNOpAsgnAnd(thread, gettable($1), $<RNode>3);
                            }
                            else {
                                $$ = $<RNode>3;
                                $<RNode>$.val = call_op(gettable($1),id,1,$4);
                            }
                            $<RNode>$.FixPos($4);
                        }
                        else {
                            $$ = null;
                        }
                    }
                | primary '[' aref_args ']' tOP_ASGN arg
                    {
                        RNode args = new RNArray(thread, $6);

                        RNode tail = RNode.list_append(thread, $3, new RNNil(thread));
                        RNode.list_concat(args, tail);
                        uint id = (uint)$5;
                        if (id == Token.tOROP) {
                            id = 0;
                        }
                        else if (id == Token.tANDOP) {
                            id = 1;
                        }
                        $$ = new RNOpAsgn1(thread, $1, id, args);
                        $<RNode>$.FixPos($1);
                    }
                | primary '.' tIDENTIFIER tOP_ASGN arg
                    {
                        uint id = (uint)$4;
                        if (id == Token.tOROP) {
                            id = 0;
                        }
                        else if (id == Token.tANDOP) {
                            id = 1;
                        }
                        $$ = new RNOpAsgn2(thread, $1, $3, id, $5);
                        $<RNode>$.FixPos($1);
                    }
                | primary '.' tCONSTANT tOP_ASGN arg
                    {
                        uint id = (uint)$4;
                        if (id == Token.tOROP) {
                            id = 0;
                        }
                        else if (id == Token.tANDOP) {
                            id = 1;
                        }
                        $$ = new RNOpAsgn2(thread, $1, $3, id, $5);
                        $<RNode>$.FixPos($1);
                    }
                | primary tCOLON2 tIDENTIFIER tOP_ASGN arg
                    {
                        uint id = (uint)$4;
                        if (id == Token.tOROP) {
                            id = 0;
                        }
                        else if (id == Token.tANDOP) {
                            id = 1;
                        }
                        $$ = new RNOpAsgn2(thread, $1, $3, id, $5);
                        $<RNode>$.FixPos($1);
                    }
                | backref tOP_ASGN arg
                    {
                        backref_error($1);
                        $$ = null;
                    }
                | arg tDOT2 arg
                    {
                        $$ = new RNDot2(thread, $1, $3);
                    }
                | arg tDOT3 arg
                    {
                        $$ = new RNDot3(thread, $1, $3);
                    }
                | arg '+' arg
                    {
                        $$ = call_op($1, '+', 1, $3);
                    }
                | arg '-' arg
                    {
                        $$ = call_op($1, '-', 1, $3);
                    }
                | arg '*' arg
                    {
                        $$ = call_op($1, '*', 1, $3);
                    }
                | arg '/' arg
                    {
                        $$ = call_op($1, '/', 1, $3);
                    }
                | arg '%' arg
                    {
                        $$ = call_op($1, '%', 1, $3);
                    }
                | arg tPOW arg
                    {
// TODO
//                bool need_negate = false;

// #if UMINUS
//                if ($1 is RNLit) {
//                    if ($1.lit is long ||
//                     $1.lit is double ||
//                     $1.lit is int /* ||
//                     $1.lit is BIGNUM */
//                     )
//                    {
//                     if (RTEST(rb_funcall($1.lit,'<',1,0))) {
//                         $1.lit = rb_funcall($1.lit,intern("-@"),0,0);
//                         need_negate = true;
//                     }
//                    }
//                }
// #endif
                        $$ = call_op($<RNode>1, Token.tPOW, 1, $<RNode>3);
//                if (need_negate) {
//                    $$ = call_op($<RNode>$, Token.tUMINUS, 0, null);
//                }
                    }
                | tUPLUS arg
                    {
                        if ($2 != null && $2 is RNLit) {
                            $$ = $2;
                        }
                        else {
                            $$ = call_op($2, Token.tUPLUS, 0, null);
                        }
                    }
                | tUMINUS arg
                    {
                        if ($2 != null && $2 is RNLit && ($2.lit is int ||
                                                          $2.lit is long)) {
                            if ($2.lit is int)
                            {
                                int i = (int)$2.lit;
                                $2.lit = -i;
                            }
                            else
                            {
                                long i = (long)$2.lit;
                                $2.lit = -i;
                            }
                            $$ = $2;
                        }
                        else {
                            $$ = call_op($2, Token.tUMINUS, 0, null);
                        }
                    }
                | arg '|' arg
                    {
                        $$ = call_op($1, '|', 1, $3);
                    }
                | arg '^' arg
                    {
                        $$ = call_op($1, '^', 1, $3);
                    }
                | arg '&' arg
                    {
                        $$ = call_op($1, '&', 1, $3);
                    }
                | arg tCMP arg
                    {
                        $$ = call_op($1, Token.tCMP, 1, $3);
                    }
                | arg '>' arg
                    {
                        $$ = call_op($1, '>', 1, $3);
                    }
                | arg tGEQ arg
                    {
                        $$ = call_op($1, Token.tGEQ, 1, $3);
                    }
                | arg '<' arg
                    {
                        $$ = call_op($1, '<', 1, $3);
                    }
                | arg tLEQ arg
                    {
                        $$ = call_op($1, Token.tLEQ, 1, $3);
                    }
                | arg tEQ arg
                    {
                        $$ = call_op($1, Token.tEQ, 1, $3);
                    }
                | arg tEQQ arg
                    {
                        $$ = call_op($1, Token.tEQQ, 1, $3);
                    }
                | arg tNEQ arg
                    {
                        $$ = new RNNot(thread, call_op($1, Token.tEQ, 1, $3));
                    }
                | arg tMATCH arg
                    {
                        $$ = match_gen($1, $3);
                    }
                | arg tNMATCH arg
                    {
                        $$ = new RNNot(thread, match_gen($1, $3));
                    }
                | '!' arg
                    {
                        value_expr($2);
                        $$ = new RNNot(thread, cond($2));
                    }
                | '~' arg
                    {
                        $$ = call_op($2, '~', 0, null);
                    }
                | arg tLSHFT arg
                    {
                        $$ = call_op($1, Token.tLSHFT, 1, $3);
                    }
                | arg tRSHFT arg
                    {
                        $$ = call_op($1, Token.tRSHFT, 1, $3);
                    }
                | arg tANDOP arg
                    {
                        $$ = logop(typeof(RNAnd), $1, $3);
                    }
                | arg tOROP arg
                    {
                        $$ = logop(typeof(RNOr), $1, $3);
                    }
                | kDEFINED opt_nl {in_defined = true;} arg
                    {
                        in_defined = false;
                        $$ = new RNDefined(thread, $4);
                    }
                | arg '?' arg ':' arg
                    {
                        value_expr($1);
                        $$ = new RNIf(thread, cond($1), $3, $5);
                    }
                | primary
                    {
                        $$ = $1;
                    }

aref_args       : none
                | command_call opt_nl
                    {
                        $$ = new RNArray(thread, $1);
                    }
                | args ',' command_call opt_nl
                    {
                        $$ = RNode.list_append(thread, $1, $3);
                    }
                | args trailer
                    {
                        $$ = $1;
                    }
                | args ',' tSTAR arg opt_nl
                    {
                        value_expr($4);
                        $$ = arg_concat($1, $4);
                    }
                | assocs trailer
                    {
                        $$ = new RNArray(thread, new RNHash(thread, $1));
                    }
                | tSTAR arg opt_nl
                    {
                        value_expr($2);
                        $$ = new RNRestArgs(thread, $2);
                    }

paren_args      : '(' none ')'
                    {
                        $$ = $2;
                    }
                | '(' call_args opt_nl ')'
                    {
                        $$ = $2;
                    }
                | '(' block_call opt_nl ')'
                    {
                        $$ = new RNArray(thread, $2);
                    }
                | '(' args ',' block_call opt_nl ')'
                    {
                        $$ = RNode.list_append(thread, $2, $4);
                    }

opt_paren_args  : none
                | paren_args

call_args       : command
                    {
                        $$ = new RNArray(thread, $1);
                    }
                | args ',' command
                    {
                        $$ = RNode.list_append(thread, $1, $3);
                    }
                | args opt_block_arg
                    {
                        $$ = arg_blk_pass($1, $2);
                    }
                | args ',' tSTAR arg opt_block_arg
                    {
                        value_expr($4);
                        $$ = arg_concat($1, $4);
                        $$ = arg_blk_pass($<RNode>$, $5);
                    }
                | assocs opt_block_arg
                    {
                        $$ = new RNArray(thread, new RNHash(thread, $1));
                        $$ = arg_blk_pass($<RNode>$, $2);
                    }
                | assocs ',' tSTAR arg opt_block_arg
                    {
                        value_expr($4);
                        $$ = arg_concat(new RNArray(thread, new RNHash(thread, $1)), $4);
                        $$ = arg_blk_pass($<RNode>$, $5);
                    }
                | args ',' assocs opt_block_arg
                    {
                        $$ = RNode.list_append(thread, $1, new RNHash(thread, $3));
                        $$ = arg_blk_pass($<RNode>$, $4);
                    }
                | args ',' assocs ',' tSTAR arg opt_block_arg
                    {
                        value_expr($6);
                        $$ = arg_concat(RNode.list_append(thread, $1, new RNHash(thread, $3)), $6);
                        $$ = arg_blk_pass($<RNode>$, $7);
                    }
                | tSTAR arg opt_block_arg
                    {
                        value_expr($2);
                        $$ = arg_blk_pass(new RNRestArgs(thread, $2), $3);
                    }
                | block_arg

command_args    : {lex.CMDARG_PUSH();} call_args
                    {
                        lex.CMDARG_POP();
                        $$ = $2;
                    }

block_arg       : tAMPER arg
                    {
                        value_expr($2);
                        $$ = new RNBlockPass(thread, $2);
                    }

opt_block_arg   : ',' block_arg
                    {
                        $$ = $2;
                    }
                | none

args       : arg
                    {
                        value_expr($1);
                        $$ = new RNArray(thread, $1);
                    }
                | args ',' arg
                    {
                        value_expr($3);
                        $$ = RNode.list_append(thread, $1, $3);
                    }

mrhs        : arg
                    {
                        value_expr($1);
                        $$ = $1;
                    }
                | mrhs_basic

mrhs_basic      : args ',' arg
                    {
                        value_expr($3);
                        $$ = RNode.list_append(thread, $1, $3);
                    }
                | args ',' tSTAR arg
                    {
                        value_expr($4);
                        $$ = arg_concat($1, $4);
                    }
                | tSTAR arg
                    {
                        value_expr($2);
                        $$ = $2;
                    }

ret_args        : call_args
                    {
                        $$ = $1;
                        if ($1 != null) {
                            if ($1 is RNArray && $1.next == null) {
                                $$ = $1.head;
                            }
                            else if ($1 is RNBlockPass) {
                                thread.CompileError("block argument should not be given");
                            }
                        }
                    }

primary  : literal
                    {
                        $$ = new RNLit(thread, $1);
                    }
                | string
                | tXSTRING
                    {
                        $$ = new RNXStr(thread, ruby, $1);
                    }
                | tQWORDS
                | tDXSTRING
                | tDREGEXP
                | var_ref
                | backref
                | tFID
                    {
                        $$ = new RNVCall(thread, $1);
                    }
                | kBEGIN
                  compstmt
                  rescue
                  opt_else
                  ensure
                  kEND
                    {
                        RNode nd = $2;
                        if ($3 == null && $4 == null && $5 == null)
                            $$ = new RNBegin(thread, $2);
                        else {
                            if ($3 != null) nd = new RNRescue(thread, $2, $3, $4);
                            else if ($4 != null) {
                                ruby.warn("else without rescue is useless");
                                nd = block_append($2, $4);
                            }
                            if ($5 != null) nd = new RNEnsure(thread, $2, $5);
                            $$ = nd;
                        }
                        $<RNode>$.FixPos(nd);
                    }
                | tLPAREN compstmt ')'
                    {
                        $$ = $2;
                    }
                | primary tCOLON2 tCONSTANT
                    {
                        value_expr($1);
                        $$ = new RNColon2(thread, $1, $3);
                    }
                | tCOLON3 cname
                    {
                        $$ = new RNColon3(thread, $2);
                    }
                | primary '[' aref_args ']'
                    {
                        value_expr($1);
                        $$ = new RNCall(thread, $1, Token.tAREF, $3);
                    }
                | tLBRACK aref_args ']'
                    {
                        if ($2 == null)
                            $$ = new RNZArray(thread); /* zero length array*/
                        else {
                            $$ = $2;
                        }
                    }
                | tLBRACE assoc_list '}'
                    {
                        $$ = new RNHash(thread, $2);
                    }
                | kRETURN '(' ret_args ')'
                    {
                        if (!compile_for_eval && in_def == 0 && in_single == 0)
                            yyerror("return appeared outside of method");
                        value_expr($3);
                        $$ = new RNReturn(thread, $3);
                    }
                | kRETURN '(' ')'
                    {
                        if (!compile_for_eval && in_def == 0 && in_single == 0)
                            yyerror("return appeared outside of method");
                        $$ = new RNReturn(thread);
                    }
                | kRETURN
                    {
                        if (!compile_for_eval && in_def == 0 && in_single == 0)
                            yyerror("return appeared outside of method");
                        $$ = new RNReturn(thread);
                    }
                | kYIELD '(' ret_args ')'
                    {
                        value_expr($3);
                        $$ = new RNYield(thread, $3);
                    }
                | kYIELD '(' ')'
                    {
                        $$ = new RNYield(thread);
                    }
                | kYIELD
                    {
                        $$ = new RNYield(thread);
                    }
                | kDEFINED opt_nl '(' {in_defined = true;} expr ')'
                    {
                        in_defined = false;
                        $$ = new RNDefined(thread, $5);
                    }
                | operation brace_block
                    {
                        $2.iter = new RNFCall(thread, $1);
                        $$ = $2;
                    }
                | method_call
                | method_call brace_block
                    {
                        if ($1 != null && $1 is RNBlockPass) {
                            thread.CompileError("both block arg and actual block given");
                        }
                        $2.iter = $1;
                        $$ = $2;
                        $<RNode>$.FixPos($1);
                    }
                | kIF expr then
                  compstmt
                  if_tail
                  kEND
                    {
                        value_expr($2);
                        $$ = new RNIf(thread, cond($2), $4, $5);
                        $<RNode>$.FixPos($2);
                    }
                | kUNLESS expr then
                  compstmt
                  opt_else
                  kEND
                    {
                        value_expr($2);
                      $$ = new RNUnless(thread, cond($2), $4, $5);
                        $<RNode>$.FixPos($2);
                    }
                | kWHILE {lex.COND_PUSH();} expr do {lex.COND_POP();}
                  compstmt
                  kEND
                    {
                        value_expr($3);
                        $$ = new RNWhile(thread, cond($3), $6, true);
                        $<RNode>$.FixPos($3);
                    }
                | kUNTIL {lex.COND_PUSH();} expr do {lex.COND_POP();}
                  compstmt
                  kEND
                    {
                        value_expr($3);
                        $$ = new RNUntil(thread, cond($3), $6, true);
                        $<RNode>$.FixPos($3);
                    }
                | kCASE expr opt_terms
                  case_body
                  kEND
                    {
                        value_expr($2);
                        $$ = new RNCase(thread, $2, $4);
                        $<RNode>$.FixPos($2);
                    }
                | kCASE opt_terms case_body kEND
                    {
                        $$ = $3;
                    }
                | kFOR block_var kIN {lex.COND_PUSH();} expr do {lex.COND_POP();}
                  compstmt
                  kEND
                    {
                        value_expr($5);
                        $$ = new RNFor(thread, $2, $5, $8);
                    }
                | kCLASS cname superclass
                    {
                        if (in_def != 0|| in_single != 0)
                            yyerror("class definition in method body");
                        class_nest++;
                        thread.LocalPush();
                        $$ = sourceline;
                    }
                  compstmt
                  kEND
                    {
                        $$ = new RNClass(thread, $2, $5, $3);
                        $<RNode>$.SetLine($<int>4);
                        thread.LocalPop();
                        class_nest--;
                    }
                | kCLASS tLSHFT expr
                    {
                        $$ = in_def;
                        in_def = 0;
                    }
                  term
                    {
                        $$ = in_single;
                        in_single = 0;
                        class_nest++;
                        thread.LocalPush();
                    }
                  compstmt
                  kEND
                    {
                        $$ = new RNSClass(thread, $3, $7);
                        thread.LocalPop();
                        class_nest--;
                        in_def = $<int>4;
                        in_single = $<int>6;
                    }
                | kMODULE cname
                    {
                        if (in_def != 0|| in_single != 0)
                            yyerror("module definition in method body");
                        class_nest++;
                        thread.LocalPush();
                        $$ = sourceline;
                    }
                  compstmt
                  kEND
                    {
                        $$ = new RNModule(thread, $2, $4);
                        $<RNode>$.SetLine($<int>3);
                        thread.LocalPop();
                        class_nest--;
                    }
                | kDEF fname
                    {
                        if (in_def != 0|| in_single != 0)
                            yyerror("nested method definition");
                        $$ = cur_mid;
                        if ($<object>2 is uint)
                            cur_mid = $2;
                        else
                            cur_mid = (uint)$<int>2;
                        in_def++;
                        thread.LocalPush();
                    }
                  f_arglist
                  compstmt
                  rescue
                  opt_else
                  ensure
                  kEND
                    {
                        RNode nd = $5;
                        if ($6 != null) nd = new RNRescue(thread, $5, $6, $7);
                        else if ($7 != null) {
                            ruby.warn("else without rescue is useless");
                            nd = block_append($5, $7);
                        }
                        if ($8 != null) nd = new RNEnsure(thread, $5, $8);

                        /* NOEX_PRIVATE for toplevel */
                        uint id;
                        if ($<object>2 is uint)
                        {
                            id = $2;
                        }
                        else
                        {
                            id = (uint)$<int>2;
                        }
                        $$ = new RNDefn(thread, id, $4, nd, (class_nest > 0) ? NOEX.PUBLIC : NOEX.PRIVATE);
                        if (is_attrset_id(id)) $<RNode>$.noex = NOEX.PUBLIC;
                        $<RNode>$.FixPos($4);
                        thread.LocalPop();
                        in_def--;
                        cur_mid = $<uint>3;
                    }
                | kDEF singleton dot_or_colon {lex.State = EXPR.FNAME;} fname
                    {
                        value_expr($2);
                        in_single++;
                        thread.LocalPush();
                        lex.State = EXPR.END; /* force for args */
                    }
                  f_arglist
                  compstmt
                  rescue
                  opt_else
                  ensure
                  kEND
                    {
                        RNode nd = $8;
                        if ($9 != null) nd = new RNRescue(thread, $8, $9, $10);
                        else if ($10 != null) {
                            ruby.warn("else without rescue is useless");
                            nd = block_append($8, $10);
                        }
                        if ($11 != null) nd = new RNEnsure(thread, $8, $11);

                        $$ = new RNDefs(thread, $2, $5, $7, nd);
                        $<RNode>$.FixPos($2);
                        thread.LocalPop();
                        in_single--;
                    }
                | kBREAK
                    {
                        $$ = new RNBreak(thread);
                    }
                | kNEXT
                    {
                        $$ = new RNNext(thread);
                    }
                | kREDO
                    {
                        $$ = new RNRedo(thread);
                    }
                | kRETRY
                    {
                        $$ = new RNRetry(thread);
                    }

then        : term
                | kTHEN
                | term kTHEN

do            : term
                | kDO_COND

if_tail  : opt_else
                | kELSIF expr then
                  compstmt
                  if_tail
                    {
                        value_expr($2);
                        $$ = new RNIf(thread, cond($2), $4, $5);
                    }

opt_else        : none
                | kELSE compstmt
                    {
                        $$ = $2;
                    }

block_var       : lhs
                | mlhs

opt_block_var   : none
                | '|' /* none */ '|'
                    {
                        $$ = new RNBlockNoArg(thread);
                    }
                | tOROP
                    {
                        $$ = new RNBlockNoArg(thread);
                    }
                | '|' block_var '|'
                    {
                        $$ = $2;
                    }


do_block        : kDO_BLOCK
                    {
                        $$ = thread.DynaPush();
                    }
                  opt_block_var
                  compstmt
                  kEND
                    {
                        $$ = new RNIter(thread, $3, null, $4);
                        $<RNode>$.FixPos(($3 != null) ? $3 : $4);
                        thread.DynaPop($<RVarmap>2);
                    }

block_call      : command do_block
                    {
                        if ($1 != null && $1 is RNBlockPass) {
                            thread.CompileError("both block arg and actual block given");
                        }
                        $2.iter = $1;
                        $$ = $2;
                        $<RNode>$.FixPos($2);
                    }
                | block_call '.' operation2 opt_paren_args
                    {
                        value_expr($1);
                        $$ = new_call($1, $3, $4);
                    }
                | block_call tCOLON2 operation2 opt_paren_args
                    {
                        value_expr($1);
                        $$ = new_call($1, $3, $4);
                    }

method_call     : operation paren_args
                    {
                        $$ = new_fcall($1, $2);
                        $<RNode>$.FixPos($2);
                    }
                | primary '.' operation2 opt_paren_args
                    {
                        value_expr($1);
                        $$ = new_call($1, $3, $4);
                        $<RNode>$.FixPos($1);
                    }
                | primary tCOLON2 operation2 paren_args
                    {
                        value_expr($1);
                        $$ = new_call($1, $3, $4);
                        $<RNode>$.FixPos($1);
                    }
                | primary tCOLON2 operation3
                    {
                        value_expr($1);
                        $$ = new_call($1, $3, null);
                    }
                | kSUPER paren_args
                    {
                        if (!compile_for_eval && in_def == 0 &&
                            in_single == 0 && !in_defined)
                            yyerror("super called outside of method");
                        $$ = new_super($2);
                    }
                | kSUPER
                    {
                        if (!compile_for_eval && in_def == 0 &&
                            in_single == 0 && !in_defined)
                            yyerror("super called outside of method");
                        $$ = new RNZSuper(thread, ruby);
                    }

brace_block     : '{'
                    {
                        $$ = thread.DynaPush();
                    }
                  opt_block_var
                  compstmt '}'
                    {
                        $$ = new RNIter(thread, $3, null, $4);
                        $<RNode>$.FixPos($4);
                        thread.DynaPop($<RVarmap>2);
                    }
                | kDO
                    {
                        $$ = thread.DynaPush();
                    }
                  opt_block_var
                  compstmt kEND
                    {
                        $$ = new RNIter(thread, $3, null, $4);
                        $<RNode>$.FixPos($4);
                        thread.DynaPop($<RVarmap>2);
                    }

case_body       : kWHEN when_args then
                  compstmt
                  cases
                    {
                        $$ = new RNWhen(thread, $2, $4, $5);
                    }

when_args       : args
                | args ',' tSTAR arg
                    {
                        value_expr($4);
                        $$ = RNode.list_append(thread, $1, new RNWhen(thread, $4));
                    }
                | tSTAR arg
                    {
                        value_expr($2);
                        $$ = new RNArray(thread, new RNWhen(thread, $2));
                    }

cases      : opt_else
                | case_body

exc_list        : none
                | args

exc_var  : tASSOC lhs
                    {
                        $$ = $2;
                    }
                | none

rescue    : kRESCUE exc_list exc_var then
                  compstmt
                  rescue
                    {
                        RNode nd = $3;
                        RNode nd2 = $5;
                        if (nd != null) {
                            nd = node_assign($3, new RNGVar(thread, ruby, intern("$!")));
                            nd2 = block_append(nd, $5);
                        }
                        $$ = new RNResBody(thread, $2, nd2, $6);
                        $<RNode>$.FixPos(($2 != null) ? $2 : nd2);
                    }
                | none

ensure    : none
                | kENSURE compstmt
                    {
                        if ($2 != null)
                            $$ = $2;
                        else
                            /* place holder */
                            $$ = new RNNil(thread);
                    }

literal  : numeric
                | symbol
                    {
                        
                if ($<object>1 is uint)
                    $$ = Symbol.ID2SYM($1);
                else
                    $$ = Symbol.ID2SYM($<char>1);
                    }
                | tREGEXP

string    : tSTRING
                    {
                        $$ = new RNStr(thread, ruby, $1);
                    }
                | tDSTRING
                | string tSTRING
                    {
                        string s = $<object>2.ToString();
                        
                        if ($1 is RNDStr) {
                            RNode.list_append(thread, $1, new RNStr(thread, ruby, s));
                        }
                        else {
#if STRCONCAT
                            rb_str_concat($1.lit, $2);
#else
                            $1.lit = new RString(ruby, ($<RNode>1.lit).ToString() + s);
#endif                
                        }
                        $$ = $1;
                    }
                | string tDSTRING
                    {
                        if ($1 is RNStr) {
                            $$ = new RNDStr(thread, ruby, $1.lit);
                        }
                        else {
                            $$ = $1;
                        }
                        // Bugfix
                        // $2.head = new RNStr(thread, ruby, $2.lit);
                        RNode.list_concat($<RNode>$, new RNArray(thread, $2));
                    }

symbol    : tSYMBEG sym
                    {
                        lex.State = EXPR.END;
                        if ($<object>2 is uint)
                            $$ = $<uint>2;
                        else
                            $$ = $<char>2;
                    }

sym          : fname
                | tIVAR
                | tGVAR
                | tCVAR

numeric  : tINTEGER
                | tFLOAT

variable        : tIDENTIFIER
                | tIVAR
                | tGVAR
                | tCONSTANT
                | tCVAR
                | kNIL {$$ = (uint)Token.kNIL;}
                | kSELF {$$ = (uint)Token.kSELF;}
                | kTRUE {$$ = (uint)Token.kTRUE;}
                | kFALSE {$$ = (uint)Token.kFALSE;}
                | k__FILE__ {$$ = (uint)Token.k__FILE__;}
                | k__LINE__ {$$ = (uint)Token.k__LINE__;}

var_ref  : variable
                    {
                        $$ = gettable($1);
                    }

backref  : tNTH_REF
                | tBACK_REF

superclass      : term
                    {
                        $$ = null;
                    }
                | '<'
                    {
                        lex.State = EXPR.BEG;
                    }
                  expr term
                    {
                        $$ = $3;
                    }
                | error term {yyErrorFlag = 0; $$ = null;}

f_arglist       : '(' f_args opt_nl ')'
                    {
                        $$ = $2;
                        lex.State = EXPR.BEG;
                    }
                | f_args term
                    {
                        $$ = $1;
                    }

f_args    : f_arg ',' f_optarg ',' f_rest_arg opt_f_block_arg
                    {
                        $$ = block_append(new RNArgs(thread, $1, $3, $<int>5), $6);
                    }
                | f_arg ',' f_optarg opt_f_block_arg
                    {
                        $$ = block_append(new RNArgs(thread, $1, $3, -1), $4);
                    }
                | f_arg ',' f_rest_arg opt_f_block_arg
                    {
                        $$ = block_append(new RNArgs(thread, $1, null, $<int>3), $4);
                    }
                | f_arg opt_f_block_arg
                    {
                        $$ = block_append(new RNArgs(thread, $1, null, -1), $2);
                    }
                | f_optarg ',' f_rest_arg opt_f_block_arg
                    {
                        $$ = block_append(new RNArgs(thread, 0, $1, $3), $4);
                    }
                | f_optarg opt_f_block_arg
                    {
                        $$ = block_append(new RNArgs(thread, 0, $1, -1), $2);
                    }
                | f_rest_arg opt_f_block_arg
                    {
                        $$ = block_append(new RNArgs(thread, 0, null, $<int>1), $2);
                    }
                | f_block_arg
                    {
                        $$ = block_append(new RNArgs(thread, 0, null, -1), $1);
                    }
                | /* none */
                    {
                        $$ = new RNArgs(thread, 0, null, -1);
                    }

f_norm_arg      : tCONSTANT
                    {
                        yyerror("formal argument cannot be a constant");
                    }
                | tIVAR
                    {
                        yyerror("formal argument cannot be an instance variable");
                    }
                | tGVAR
                    {
                        yyerror("formal argument cannot be a global variable");
                    }
                | tCVAR
                    {
                        yyerror("formal argument cannot be a class variable");
                    }
                | tIDENTIFIER
                    {
                        if (!is_local_id($1))
                            yyerror("formal argument must be local variable");
                        else if (thread.LocalID($1))
                            yyerror("duplicate argument name");
                        thread.LocalCnt($1);
                        $$ = 1;
                    }

f_arg      : f_norm_arg
                | f_arg ',' f_norm_arg
                    {
                        $$ = $<int>$ + 1;
                    }

f_opt      : tIDENTIFIER '=' arg
                    {
                        if (!is_local_id($1))
                            yyerror("formal argument must be local variable");
                        else if (thread.LocalID($1))
                            yyerror("duplicate optional argument name");
                        $$ = assignable($1, $3);
                    }

f_optarg        : f_opt
                    {
                        $$ = new RNBlock(thread, $1);
                        $<RNode>$.end = $<RNode>$;
                    }
                | f_optarg ',' f_opt
                    {
                        $$ = block_append($1, $3);
                    }

f_rest_arg      : tSTAR tIDENTIFIER
                    {
                        if (!is_local_id($2))
                            yyerror("rest argument must be local variable");
                        else if (thread.LocalID($2))
                            yyerror("duplicate rest argument name");
                        $$ = thread.LocalCnt($2);
                    }
                | tSTAR
                    {
                        $$ = -2;
                    }

f_block_arg     : tAMPER tIDENTIFIER
                    {
                        if (!is_local_id($2))
                            yyerror("block argument must be local variable");
                        else if (thread.LocalID($2))
                            yyerror("duplicate block argument name");
                        $$ = new RNBlockArg(thread, $2, thread.LocalCnt($2));
                    }

opt_f_block_arg : ',' f_block_arg
                    {
                        $$ = $2;
                    }
                | none

singleton       : var_ref
                    {
                        if ($1 is RNSelf) {
                            $$ = new RNSelf(thread);
                        }
                        else {
                            $$ = $1;
                        }
                    }
                | '(' {lex.State = EXPR.BEG;} expr opt_nl ')'
                    {
                        if ($3 is RNStr ||
                            $3 is RNDStr ||
                            $3 is RNXStr ||
                            $3 is RNDXStr ||
                            $3 is RNDRegx ||
                            $3 is RNLit ||
                            $3 is RNArray ||
                            $3 is RNZArray)
                        {
                            yyerror("can't define single method for literals.");
                        }
                        $$ = $3;
                    }

assoc_list      : none
                | assocs trailer
                    {
                        $$ = $1;
                    }
                | args trailer
                    {
                        if ($1.alen % 2 != 0) {
                            yyerror("odd number list for Hash");
                        }
                        $$ = $1;
                    }

assocs    : assoc
                | assocs ',' assoc
                    {
                        $$ = RNode.list_concat($1, $3);
                    }

assoc      : arg tASSOC arg
                    {
                        $$ = RNode.list_append(thread, new RNArray(thread, $1), $3);
                    }

operation       : tIDENTIFIER
                | tCONSTANT
                | tFID

operation2      : tIDENTIFIER
                | tCONSTANT
                | tFID
                | op

operation3      : tIDENTIFIER
                | tFID
                | op

dot_or_colon    : '.'
                | tCOLON2

opt_terms       : /* none */
                | terms

opt_nl    : /* none */
                | '\n'

trailer  : /* none */
                | '\n'
                | ','

term        : ';' {yyErrorFlag = 0;}
                | '\n'

terms      : term
                | terms ';' {yyErrorFlag = 0;}

none        : /* none */
                    {
                        $$ = null;
                    }
%%
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
 
