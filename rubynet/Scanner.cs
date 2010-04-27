/*
Copyright (C) 2001-2002 arton
Copyright (C) 2005      Jaen Saul
*/

using System;
using System.Text;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NETRuby
{
    public enum EXPR
    {
        BEG,        /* ignore newline, +/- is a sign. */
        END,        /* newline significant, +/- is a operator. */
        ARG,        /* newline significant, +/- is a operator. */
        MID,        /* newline significant, +/- is a operator. */
        FNAME,      /* ignore newline, no reserved words. */
        DOT,        /* right after `.' or `::', no reserved words. */
        CLASS,      /* immediate after `class', no here document. */
    }

    enum RE_OPTION
    {
        IGNORECASE = 1,
        EXTENDED = 2,
        MULTILINE = 4,
        SINGLELINE = 8,
        POSIXLINE = 16,
        LONGEST = 32,
        MAY_IGNORECASE = 64,
        OPTIMIZE_ANCHOR = 128,
        OPTIMIZE_EXACTN = 256,
        OPTIMIZE_NO_BM = 512,
        OPTIMIZE_BMATCH = 1024,
    }

    namespace yyParser
    {
        
    public class Scanner : yyInput,
                        Parser.Lexer
    {
        public Scanner(Parser psr, TextReader rdr, NetRuby rb, RThread th)
        {
            parser = psr;
            reader = rdr;
            ruby = rb;
            thread = th;
            thread.line = 0 - 1;
            thread.file = "(eval)";
            rb.__end__seen = false;
            tokenbuf = new StringBuilder(128);
        }

        public Scanner(Parser psr, TextReader rdr, string fname, int start, NetRuby rb,
                       RThread th)
        {
            parser = psr;
            reader = rdr;
            ruby = rb;
            thread = th;
            thread.line = start - 1;
            thread.file = fname;
            rb.__end__seen = false;
            tokenbuf = new StringBuilder(128);
        }

        EXPR Parser.Lexer.State
        {
            get { return lex_state; }
            set { lex_state = value; }
        }

        private RThread thread;
        private NetRuby ruby;
        private int cond_nest = 0;
        private uint cond_stack = 0;

        void Parser.Lexer.COND_PUSH()
        {
            cond_nest++;
            cond_stack = (cond_stack<<1)|1;
        }
        void Parser.Lexer.COND_POP()
        {
            cond_nest--;
            cond_stack >>= 1;
        }
        private bool COND_P()
        {
            return (cond_nest > 0 && (cond_stack & 1) == 1);
        }
        private uint cmdarg_stack = 0;
        void Parser.Lexer.CMDARG_PUSH()
        {
            cmdarg_stack = ((cmdarg_stack<<1)|1);
        }
        void Parser.Lexer.CMDARG_POP()
        {
            cmdarg_stack >>= 1;
        }
        private bool CMDARG_P()
        {
            return ((cmdarg_stack&1) != 0);
        }

        private EXPR lex_state = EXPR.BEG;
        
        private int curr = -1;
        
        bool yyInput.advance ()
        {
            curr = yylex();
#if _SCANNER_DEBUG        
            System.Console.WriteLine("token:" + tok() + ", result=" + curr.ToString() + "(" + Char.ToString((char)curr) +")");
#endif        
            return (curr <= 0) ? false : true;
        }
        
        int yyInput.token ()
        {
            return curr;
        }
        Object yyInput.value ()
        {
#if _SCANNER_DEBUG
            System.Console.WriteLine("value=" + ((yylval==null)?"null":yylval.ToString()));
#endif        
            return yylval;
        }

        struct kwtable
        {
            internal kwtable(string s, int i0, int i1, EXPR st)
            {
                name = s;
                id0 = i0;
                id1 = i1;
                state = st;
            }
            internal kwtable(string s)
            {
                name = s;
                id0 = id1 = 0;
                state = EXPR.BEG;
            }
            internal string name;
            int id0;
            int id1;
            internal EXPR state;
            public int this[int i]
            {
                get {
                    if (i == 0)
                        return id0;
                    return id1;
                }
                set {
                    if (i == 0)
                        id0 = value;
                    id1 = value;
                }
            }
            public int id(bool f)
            {
                if (f == false)
                    return id0;
                return id1;
            }
        }

        internal const int TOTAL_KEYWORDS = 40;
        internal const int MIN_WORD_LENGTH = 2;
        internal const int MAX_WORD_LENGTH = 8;
        internal const int MIN_HASH_VALUE = 6;
        internal const int MAX_HASH_VALUE = 55;

        static private byte[] asso_values = {
                56, 56, 56, 56, 56, 56, 56, 56, 56, 56,
                56, 56, 56, 56, 56, 56, 56, 56, 56, 56,
                56, 56, 56, 56, 56, 56, 56, 56, 56, 56,
                56, 56, 56, 56, 56, 56, 56, 56, 56, 56,
                56, 56, 56, 56, 56, 56, 56, 56, 56, 56,
                56, 56, 56, 56, 56, 56, 56, 56, 56, 56,
                56, 56, 56, 11, 56, 56, 36, 56,  1, 37,
                31,  1, 56, 56, 56, 56, 29, 56,  1, 56,
                56, 56, 56, 56, 56, 56, 56, 56, 56, 56,
                56, 56, 56, 56, 56,  1, 56, 32,  1,  2,
                1,  1,  4, 23, 56, 17, 56, 20,  9,  2,
                9, 26, 14, 56,  5,  1,  1, 16, 56, 21,
                20,  9, 56, 56, 56, 56, 56, 56, 56, 56,
                56, 56, 56, 56, 56, 56, 56, 56, 56, 56,
                56, 56, 56, 56, 56, 56, 56, 56, 56, 56,
                56, 56, 56, 56, 56, 56, 56, 56, 56, 56,
                56, 56, 56, 56, 56, 56, 56, 56, 56, 56,
                56, 56, 56, 56, 56, 56, 56, 56, 56, 56,
                56, 56, 56, 56, 56, 56, 56, 56, 56, 56,
                56, 56, 56, 56, 56, 56, 56, 56, 56, 56,
                56, 56, 56, 56, 56, 56, 56, 56, 56, 56,
                56, 56, 56, 56, 56, 56, 56, 56, 56, 56,
                56, 56, 56, 56, 56, 56, 56, 56, 56, 56,
                56, 56, 56, 56, 56, 56, 56, 56, 56, 56,
                56, 56, 56, 56, 56, 56, 56, 56, 56, 56,
                56, 56, 56, 56, 56, 56
            };
        private int hash(string str, int len)
        {
            int hval = len;
            switch (hval)
            {
            case 1:
                hval += asso_values[str[0]];
                break;
            case 2:
                goto case 1;
            case 3:
                hval += asso_values[str[2]];
                goto case 1;
            default:
                goto case 3;
            }
            return hval + asso_values[str[(int)len - 1]];
        }

        private kwtable nullword = new kwtable(null, 0, 0, 0);
        private kwtable[] wordlist = new kwtable[]
        {
            new kwtable(""),new kwtable(""),new kwtable(""),new kwtable(""),new kwtable(""),new kwtable(""),
            new kwtable("end", Token.kEND, Token.kEND, EXPR.END),
            new kwtable("else", Token.kELSE, Token.kELSE, EXPR.BEG),
            new kwtable("case", Token.kCASE, Token.kCASE, EXPR.BEG),
            new kwtable("ensure", Token.kENSURE, Token.kENSURE, EXPR.BEG),
            new kwtable("module", Token.kMODULE, Token.kMODULE, EXPR.BEG),
            new kwtable("elsif", Token.kELSIF, Token.kELSIF, EXPR.BEG),
            new kwtable("def", Token.kDEF, Token.kDEF, EXPR.FNAME),
            new kwtable("rescue", Token.kRESCUE, Token.kRESCUE_MOD, EXPR.END),
            new kwtable("not", Token.kNOT, Token.kNOT, EXPR.BEG),
            new kwtable("then", Token.kTHEN, Token.kTHEN, EXPR.BEG),
            new kwtable("yield", Token.kYIELD, Token.kYIELD, EXPR.ARG),
            new kwtable("for", Token.kFOR, Token.kFOR, EXPR.BEG),
            new kwtable("self", Token.kSELF, Token.kSELF, EXPR.END),
            new kwtable("false", Token.kFALSE, Token.kFALSE, EXPR.END),
            new kwtable("retry", Token.kRETRY, Token.kRETRY, EXPR.END),
            new kwtable("return", Token.kRETURN, Token.kRETURN, EXPR.MID),
            new kwtable("true", Token.kTRUE, Token.kTRUE, EXPR.END),
            new kwtable("if", Token.kIF, Token.kIF_MOD, EXPR.BEG),
            new kwtable("defined?", Token.kDEFINED, Token.kDEFINED, EXPR.ARG),
            new kwtable("super", Token.kSUPER, Token.kSUPER, EXPR.ARG),
            new kwtable("undef", Token.kUNDEF, Token.kUNDEF, EXPR.FNAME),
            new kwtable("break", Token.kBREAK, Token.kBREAK, EXPR.END),
            new kwtable("in", Token.kIN, Token.kIN, EXPR.BEG),
            new kwtable("do", Token.kDO, Token.kDO, EXPR.BEG),
            new kwtable("nil", Token.kNIL, Token.kNIL, EXPR.END),
            new kwtable("until", Token.kUNTIL, Token.kUNTIL_MOD, EXPR.BEG),
            new kwtable("unless", Token.kUNLESS, Token.kUNLESS_MOD, EXPR.BEG),
            new kwtable("or", Token.kOR, Token.kOR, EXPR.BEG),
            new kwtable("next", Token.kNEXT, Token.kNEXT, EXPR.END),
            new kwtable("when", Token.kWHEN, Token.kWHEN, EXPR.BEG),
            new kwtable("redo", Token.kREDO, Token.kREDO, EXPR.END),
            new kwtable("and", Token.kAND, Token.kAND, EXPR.BEG),
            new kwtable("begin", Token.kBEGIN, Token.kBEGIN, EXPR.BEG),
            new kwtable("__LINE__", Token.k__LINE__, Token.k__LINE__, EXPR.END),
            new kwtable("class", Token.kCLASS, Token.kCLASS, EXPR.CLASS),
            new kwtable("__FILE__", Token.k__FILE__, Token.k__FILE__, EXPR.END),
            new kwtable("END", Token.klEND, Token.klEND, EXPR.END),
            new kwtable("BEGIN", Token.klBEGIN, Token.klBEGIN, EXPR.END),
            new kwtable("while", Token.kWHILE, Token.kWHILE_MOD, EXPR.BEG),
            new kwtable(""),
            new kwtable(""),
            new kwtable(""),
            new kwtable(""),
            new kwtable(""),
            new kwtable(""),
            new kwtable(""),
            new kwtable(""),
            new kwtable(""),
            new kwtable(""),
            new kwtable("alias", Token.kALIAS, Token.kALIAS, EXPR.FNAME)
        };

        kwtable reserved_word(string str, int len)
        {
            if (len <= MAX_WORD_LENGTH && len >= MIN_WORD_LENGTH)
            {
                int key = hash(str, len);
                if (key <= MAX_HASH_VALUE && key >= 0)
                {
                    string s = wordlist[key].name;
                    if (str == s)
                    {
                        return wordlist[key];
                    }
                }
            }
            return nullword;
        }
        
        private object yylval;
        private Parser parser;
        private TextReader reader;
        private StringBuilder tokenbuf;
        private string tok() { return tokenbuf.ToString(); }
        private int toklen() { return tokenbuf.Length; }
        private char toklast() { return tokenbuf[tokenbuf.Length - 1]; }
        private int gets_ptr;
        private string lastline;
        private int pbeg;
        private int pcur;
        private int pend;
        private int heredoc_end;
        
        private int yylex()
        {
            bool space_seen = false;
            kwtable kw;

        retry:
            int c = nextc();
            switch (c)
            {
            case '\0':                /* NUL */
            case '\x0004':        /* ^D */
            case '\x001a':        /* ^Z */
            case -1:                /* end of script. */
                return 0;

            /* white spaces */
            case ' ': case '\t': case '\f': case '\r':
            case '\v':
                space_seen = true;
                goto retry;

            case '#':                /* it's a comment */
                while ((c = nextc()) != '\n') {
                    if (c == -1)
                        return 0;
                }
                /* fall through */
                goto case '\n';
            case '\n':
                switch (lex_state)
                {
                case EXPR.BEG:
                case EXPR.FNAME:
                case EXPR.DOT:
                    goto retry;
                default:
                    break;
                }
                lex_state = EXPR.BEG;
                return '\n';

            case '*':
                if ((c = nextc()) == '*') {
                    lex_state = EXPR.BEG;
                    if (nextc() == '=') {
                        yylval = Token.tPOW;
                        return Token.tOP_ASGN;
                    }
                    pushback(c);
                    return Token.tPOW;
                }
                if (c == '=') {
                    yylval = (int)'*';
                    lex_state = EXPR.BEG;
                    return Token.tOP_ASGN;
                }
                pushback(c);
                if (lex_state == EXPR.ARG && space_seen && Char.IsWhiteSpace((char)c) == false)
                {
                    ruby.warning("`*' interpreted as argument prefix");
                    c = Token.tSTAR;
                }
                else if (lex_state == EXPR.BEG || lex_state == EXPR.MID) {
                    c = Token.tSTAR;
                }
                else {
                    c = '*';
                }
                lex_state = EXPR.BEG;
                return c;

            case '!':
                lex_state = EXPR.BEG;
                if ((c = nextc()) == '=') {
                    return Token.tNEQ;
                }
                if (c == '~') {
                    return Token.tNMATCH;
                }
                pushback(c);
                return '!';

            case '=':
                if (pcur == pbeg + 1)
                {
                    /* skip embedded rd document */
                    if (String.Compare(lastline, pcur, "begin", 0, 5) == 0
                        && Char.IsWhiteSpace(lastline[pcur + 5]))
                    {
                        for (;;) {
                            pcur = pend;
                            c = nextc();
                            if (c == -1) {
                                thread.CompileError("embedded document meets end of file");
                                return 0;
                            }
                            if (c != '=') continue;
                            if (String.Compare(lastline, pcur, "end", 0, 3) == 0
                                && Char.IsWhiteSpace(lastline[pcur + 3])) {
                                break;
                            }
                        }
                        pcur = pend;
                        goto retry;
                    }
                }

                lex_state = EXPR.BEG;
                if ((c = nextc()) == '=') {
                    if ((c = nextc()) == '=') {
                        return Token.tEQQ;
                    }
                    pushback(c);
                    return Token.tEQ;
                }
                if (c == '~') {
                    return Token.tMATCH;
                }
                else if (c == '>') {
                    return Token.tASSOC;
                }
                pushback(c);
                return '=';

            case '<':
                c = nextc();
                if (c == '<' &&
                    lex_state != EXPR.END && lex_state != EXPR.CLASS &&
                    (lex_state != EXPR.ARG || space_seen)) {
                    int c2 = nextc();
                    int indent = 0;
                    if (c2 == '-') {
                        indent = 1;
                        c2 = nextc();
                    }
                    if (Char.IsWhiteSpace((char)c2) && "\"'`".IndexOf((char)c2) > 0 || is_identchar(c2))
                    {
                        return here_document(c2, indent);
                    }
                    pushback(c2);
                }
                lex_state = EXPR.BEG;
                if (c == '=') {
                    if ((c = nextc()) == '>') {
                        return Token.tCMP;
                    }
                    pushback(c);
                    return Token.tLEQ;
                }
                if (c == '<') {
                    if (nextc() == '=') {
                        yylval = Token.tLSHFT;
                        return Token.tOP_ASGN;
                    }
                    pushback(c);
                    return Token.tLSHFT;
                }
                pushback(c);
                return '<';

            case '>':
                lex_state = EXPR.BEG;
                if ((c = nextc()) == '=') {
                    return Token.tGEQ;
                }
                if (c == '>') {
                    if ((c = nextc()) == '=') {
                        yylval = Token.tRSHFT;
                        return Token.tOP_ASGN;
                    }
                    pushback(c);
                    return Token.tRSHFT;
                }
                pushback(c);
                return '>';

            case '"':
                return parse_string(c,c,c);
            case '`':
                if (lex_state == EXPR.FNAME) return c;
                if (lex_state == EXPR.DOT) return c;
                return parse_string(c,c,c);

            case '\'':
                return parse_qstring(c,0);

            case '?':
                if (lex_state == EXPR.END) {
                    lex_state = EXPR.BEG;
                    return '?';
                }
                c = nextc();
                if (c == -1) {
                    thread.CompileError("incomplete character syntax");
                    return 0;
                }
                if (lex_state == EXPR.ARG && Char.IsWhiteSpace((char)c))
                {
                    pushback(c);
                    lex_state = EXPR.BEG;
                    return '?';
                }
                if (c == '\\') {
                    c = read_escape();
                }
                c &= 0xff;
                yylval = c; //INT2FIX(c);
                lex_state = EXPR.END;
                return Token.tINTEGER;

            case '&':
                if ((c = nextc()) == '&') {
                    lex_state = EXPR.BEG;
                    if ((c = nextc()) == '=') {
                        yylval = Token.tANDOP;
                        return Token.tOP_ASGN;
                    }
                    pushback(c);
                    return Token.tANDOP;
                }
                else if (c == '=') {
                    yylval = (int)'&';
                    lex_state = EXPR.BEG;
                    return Token.tOP_ASGN;
                }
                pushback(c);
                if (lex_state == EXPR.ARG && space_seen && Char.IsWhiteSpace((char)c) == false)
                {
                    ruby.warning("`&' interpreted as argument prefix");
                    c = Token.tAMPER;
                }
                else if (lex_state == EXPR.BEG || lex_state == EXPR.MID) {
                    c = Token.tAMPER;
                }
                else {
                    c = '&';
                }
                lex_state = EXPR.BEG;
                return c;

            case '|':
                lex_state = EXPR.BEG;
                if ((c = nextc()) == '|') {
                    if ((c = nextc()) == '=') {
                        yylval = Token.tOROP;
                        return Token.tOP_ASGN;
                    }
                    pushback(c);
                    return Token.tOROP;
                }
                else if (c == '=') {
                    yylval = (int)'|';
                    return Token.tOP_ASGN;
                }
                pushback(c);
                return '|';

            case '+':
                c = nextc();
                if (lex_state == EXPR.FNAME || lex_state == EXPR.DOT) {
                    if (c == '@') {
                        return Token.tUPLUS;
                    }
                    pushback(c);
                    return '+';
                }
                if (c == '=') {
                    lex_state = EXPR.BEG;
                    yylval = (int)'+';
                    return Token.tOP_ASGN;
                }
                if (lex_state == EXPR.BEG || lex_state == EXPR.MID ||
                    (lex_state == EXPR.ARG && space_seen && Char.IsWhiteSpace((char)c) == false))
                {
                    if (lex_state == EXPR.ARG) arg_ambiguous();
                    lex_state = EXPR.BEG;
                    pushback(c);
                    if (Char.IsDigit((char)c))
                    {
                        c = '+';
                        goto start_num;
                    }
                    return Token.tUPLUS;
                }
                lex_state = EXPR.BEG;
                pushback(c);
                return '+';

            case '-':
                c = nextc();
                if (lex_state == EXPR.FNAME || lex_state == EXPR.DOT) {
                    if (c == '@') {
                        return Token.tUMINUS;
                    }
                    pushback(c);
                    return '-';
                }
                if (c == '=') {
                    lex_state = EXPR.BEG;
                    yylval = (int)'-';
                    return Token.tOP_ASGN;
                }
                if (lex_state == EXPR.BEG || lex_state == EXPR.MID ||
                    (lex_state == EXPR.ARG && space_seen && Char.IsWhiteSpace((char)c) == false))
                {
                    if (lex_state == EXPR.ARG) arg_ambiguous();
                    lex_state = EXPR.BEG;
                    pushback(c);
                    if (Char.IsDigit((char)c))
                    {
                        c = '-';
                        goto start_num;
                    }
                    return Token.tUMINUS;
                }
                lex_state = EXPR.BEG;
                pushback(c);
                return '-';

            case '.':
                lex_state = EXPR.BEG;
                if ((c = nextc()) == '.') {
                    if ((c = nextc()) == '.') {
                        return Token.tDOT3;
                    }
                    pushback(c);
                    return Token.tDOT2;
                }
                pushback(c);
                if (!Char.IsDigit((char)c))
                {
                    lex_state = EXPR.DOT;
                    return '.';
                }
                c = '.';
                /* fall through */
            start_num:
                goto case '9';
            case '0': case '1': case '2': case '3': case '4':
            case '5': case '6': case '7': case '8': case '9':
            {
                bool is_float = false;
                bool seen_point = false;
                bool seen_e = false;
                bool seen_uc = false;

                lex_state = EXPR.END;
                newtok();
                if (c == '-' || c == '+') {
                    tokadd(c);
                    c = nextc();
                }
                if (c == '0') {
                    c = nextc();
                    if (c == 'x' || c == 'X') {
                        /* hexadecimal */
                        c = nextc();
                        do {
                            if (c == '_') {
                                seen_uc = true;
                                continue;
                            }
                            if (!ISXDIGIT((char)c)) break;
                            seen_uc = false;
                            tokadd(c);
                        } while ((c = nextc()) > 0);
                        pushback(c);
                        if (toklen() == 0) {
                            parser.yyerror("hexadecimal number without hex-digits");
                        }
                        else if (seen_uc) goto trailing_uc;
                        yylval = RInteger.StringToInteger(ruby, tok(), 16);
                        return Token.tINTEGER;
                    }
                    if (c == 'b' || c == 'B') {
                        /* binary */
                        c = nextc();
                        do {
                            if (c == '_') {
                                seen_uc = true;
                                continue;
                            }
                            if (c != '0'&& c != '1') break;
                            seen_uc = false;
                            tokadd(c);
                        } while ((c = nextc()) > 0);
                        pushback(c);
                        if (toklen() == 0) {
                            parser.yyerror("numeric literal without digits");
                        }
                        else if (seen_uc) goto trailing_uc;
                        yylval = RInteger.StringToInteger(ruby, tok(), 2);
                        return Token.tINTEGER;
                    }
                    if (c >= '0' && c <= '7' || c == '_') {
                        /* octal */
                        do {
                            if (c == '_') {
                                seen_uc = true;
                                continue;
                            }
                            if (c < '0' || c > '7') break;
                            seen_uc = false;
                            tokadd(c);
                        } while ((c = nextc()) > 0);
                        pushback(c);
                        if (seen_uc) goto trailing_uc;
                        yylval = RInteger.StringToInteger(ruby, tok(), 8);
                        return Token.tINTEGER;
                    }
                    if (c > '7' && c <= '9') {
                        parser.yyerror("Illegal octal digit");
                    }
                    else if (c == '.') {
                        tokadd('0');
                    }
                    else {
                        pushback(c);
                        yylval = 0; //INT2FIX(0);
                        return Token.tINTEGER;
                    }
                }

                for (;;) {
                    switch (c) {
                    case '0': case '1': case '2': case '3': case '4':
                    case '5': case '6': case '7': case '8': case '9':
                        seen_uc = false;
                        tokadd(c);
                        break;

                    case '.':
                        if (seen_uc) goto trailing_uc;
                        if (seen_point || seen_e) {
                            goto decode_num;
                        }
                        else {
                            int c0 = nextc();
                            if (!Char.IsDigit((char)c0)) {
                                pushback(c0);
                                goto decode_num;
                            }
                            c = c0;
                        }
                        tokadd('.');
                        tokadd(c);
                        is_float = true;
                        seen_point = true;
                        seen_uc = false;
                        break;

                    case 'e':
                    case 'E':
                        if (seen_e) {
                            goto decode_num;
                        }
                        tokadd(c);
                        seen_e = true;
                        is_float = true;
                        while ((c = nextc()) == '_')
                            seen_uc = true;
                        if (c == '-' || c == '+')
                            tokadd(c);
                        else
                            continue;
                        break;
        
                    case '_':        /* `_' in number just ignored */
                        seen_uc = true;
                        break;

                    default:
                        goto decode_num;
                    }
                    c = nextc();
                }
        
            decode_num:
                pushback(c);
            trailing_uc:
                if (seen_uc) {
                    parser.yyerror("trailing `_' in number");
                }
                if (is_float) {
                    double d = 0.0;
                    try
                    {
                        d = Convert.ToDouble(tok());
                    }
                    catch (OverflowException)
                    {
                        ruby.warn("Float {0} out of range", tok());
                    }
                    yylval = d;
                    return Token.tFLOAT;
                }
                yylval = RInteger.StringToInteger(ruby, tok(), 0);
                return Token.tINTEGER;
            }

            case ']':
            case '}':
                lex_state = EXPR.END;
                return c;

            case ')':
                if (cond_nest > 0) {
                    cond_stack >>= 1;
                }
                lex_state = EXPR.END;
                return c;

            case ':':
                c = nextc();
                if (c == ':') {
                    if (lex_state == EXPR.BEG ||  lex_state == EXPR.MID ||
                        (lex_state == EXPR.ARG && space_seen)) {
                        lex_state = EXPR.BEG;
                        return Token.tCOLON3;
                    }
                    lex_state = EXPR.DOT;
                    return Token.tCOLON2;
                }
                pushback(c);
                if (lex_state == EXPR.END || Char.IsWhiteSpace((char)c)) {
                    lex_state = EXPR.BEG;
                    return ':';
                }
                lex_state = EXPR.FNAME;
                return Token.tSYMBEG;

            case '/':
                if (lex_state == EXPR.BEG || lex_state == EXPR.MID) {
                    return parse_regx('/', '/');
                }
                if ((c = nextc()) == '=') {
                    lex_state = EXPR.BEG;
                    yylval = (int)'/';
                    return Token.tOP_ASGN;
                }
                pushback(c);
                if (lex_state == EXPR.ARG && space_seen) {
                    if (!Char.IsWhiteSpace((char)c))
                    {
                        arg_ambiguous();
                        return parse_regx('/', '/');
                    }
                }
                lex_state = EXPR.BEG;
                return '/';

            case '^':
                lex_state = EXPR.BEG;
                if ((c = nextc()) == '=') {
                    yylval = (int)'^';
                    return Token.tOP_ASGN;
                }
                pushback(c);
                return '^';

            case ',':
            case ';':
                lex_state = EXPR.BEG;
                return c;

            case '~':
                if (lex_state == EXPR.FNAME || lex_state == EXPR.DOT) {
                    if ((c = nextc()) != '@') {
                        pushback(c);
                    }
                }
                lex_state = EXPR.BEG;
                return '~';

            case '(':
                if (cond_nest > 0) {
                    cond_stack = (cond_stack<<1)|0;
                }
                if (lex_state == EXPR.BEG || lex_state == EXPR.MID) {
                    c = Token.tLPAREN;
                }
                else if (lex_state == EXPR.ARG && space_seen) {
                    ruby.warning(tok() + " (...) interpreted as method call", tok());
                }
                lex_state = EXPR.BEG;
                return c;

            case '[':
                if (lex_state == EXPR.FNAME || lex_state == EXPR.DOT) {
                    if ((c = nextc()) == ']') {
                        if ((c = nextc()) == '=') {
                            return Token.tASET;
                        }
                        pushback(c);
                        return Token.tAREF;
                    }
                    pushback(c);
                    return '[';
                }
                else if (lex_state == EXPR.BEG || lex_state == EXPR.MID) {
                    c = Token.tLBRACK;
                }
                else if (lex_state == EXPR.ARG && space_seen) {
                    c = Token.tLBRACK;
                }
                lex_state = EXPR.BEG;
                return c;

            case '{':
                if (lex_state != EXPR.END && lex_state != EXPR.ARG)
                    c = Token.tLBRACE;
                lex_state = EXPR.BEG;
                return c;

            case '\\':
                c = nextc();
                if (c == '\n') {
                    space_seen = true;
                    goto retry; /* skip \\n */
                }
                pushback(c);
                return '\\';

            case '%':
                if (lex_state == EXPR.BEG || lex_state == EXPR.MID) {
                    c = nextc();
                    return quotation(c);
                }
                if ((c = nextc()) == '=') {
                    yylval = (int)'%';
                    return Token.tOP_ASGN;
                }
                if (lex_state == EXPR.ARG && space_seen && Char.IsWhiteSpace((char)c) == false) {
                    return quotation(c);
                }
                lex_state = EXPR.BEG;
                pushback(c);
                return '%';

            case '$':
                lex_state = EXPR.END;
                newtok();
                c = nextc();
                switch (c) {
                case '_':                /* $_: last read line string */
                    c = nextc();
                    if (is_identchar(c)) {
                        tokadd('$');
                        tokadd('_');
                        break;
                    }
                    pushback(c);
                    c = '_';
                    goto case '~';
                    /* fall through */
                case '~':                /* $~: match-data */
                    thread.LocalCnt((uint)c);
                    goto case '*';
                    /* fall through */
                case '*':                /* $*: argv */
                case '$':                /* $$: pid */
                case '?':                /* $?: last status */
                case '!':                /* $!: error string */
                case '@':                /* $@: error position */
                case '/':                /* $/: input record separator */
                case '\\':                /* $\: output record separator */
                case ';':                /* $;: field separator */
                case ',':                /* $,: output field separator */
                case '.':                /* $.: last read line number */
                case '=':                /* $=: ignorecase */
                case ':':                /* $:: load path */
                case '<':                /* $<: reading filename */
                case '>':                /* $>: default output handle */
                case '\"':                /* $": already loaded files */
                    tokadd('$');
                    tokadd(c);
                    yylval = ruby.intern(tok());
                    return Token.tGVAR;

                case '-':
                    tokadd('$');
                    tokadd(c);
                    c = nextc();
                    tokadd(c);
                    yylval = ruby.intern(tok());
                    /* xxx shouldn't check if valid option variable */
                    return Token.tGVAR;

                case '&':                /* $&: last match */
                case '`':                /* $`: string before last match */
                case '\'':                /* $': string after last match */
                case '+':                /* $+: string matches last paren. */
                    yylval = new RNBackRef(thread, c);
                    return Token.tBACK_REF;

                case '1': case '2': case '3':
                case '4': case '5': case '6':
                case '7': case '8': case '9':
                    tokadd('$');
                    while (Char.IsDigit((char)c))
                    {
                        tokadd(c);
                        c = nextc();
                    }
                    if (is_identchar(c))
                        break;
                    pushback(c);
                    yylval = new RNNthRef(thread, Convert.ToInt32(tok().Substring(1)));
                    return Token.tNTH_REF;

                default:
                    if (!is_identchar(c)) {
                        pushback(c);
                        return '$';
                    }
                    goto case '0';
                case '0':
                    tokadd('$');
                    break;
                }
                break;

            case '@':
                c = nextc();
                newtok();
                tokadd('@');
                if (c == '@') {
                    tokadd('@');
                    c = nextc();
                }
                if (Char.IsDigit((char)c))
                {
                    thread.CompileError(String.Format("`@{0}' is not a valid instance variable name", c));
                }
                if (!is_identchar(c)) {
                    pushback(c);
                    return '@';
                }
                break;

            default:
                if (!is_identchar(c) || Char.IsDigit((char)c))
                {
                    thread.CompileError(String.Format("Invalid char `0x{0:x2}' in expression", c));
                    goto retry;
                }

                newtok();
                break;
            }

            while (is_identchar(c)) {
                tokadd(c);
#if NONE_UCS2        
                if (ismbchar(c)) {
                    int i, len = mbclen(c)-1;

                    for (i = 0; i < len; i++) {
                        c = nextc();
                        tokadd(c);
                    }
                }
#endif        
                c = nextc();
            }
            if ((c == '!' || c == '?') && is_identchar(tok()[0]) && !peek('=')) {
                tokadd(c);
            }
            else {
                pushback(c);
            }

            {
                int result = 0;

                switch (tok()[0]) {
                case '$':
                    lex_state = EXPR.END;
                    result = Token.tGVAR;
                    break;
                case '@':
                    lex_state = EXPR.END;
                    if (tok()[1] == '@')
                        result = Token.tCVAR;
                    else
                        result = Token.tIVAR;
                    break;
                default:
                    if (lex_state != EXPR.DOT) {
                        /* See if it is a reserved word.  */
                        kw = reserved_word(tok(), toklen());
                        if (kw.name != null) {
                            EXPR state = lex_state;
                            lex_state = kw.state;
                            if (state == EXPR.FNAME) {
                                yylval = ruby.intern(kw.name);
                            }
                            if (kw[0] == Token.kDO) {
                                if (COND_P()) return Token.kDO_COND;
                                if (CMDARG_P()) return Token.kDO_BLOCK;
                                return Token.kDO;
                            }
                            if (state == EXPR.BEG)
                                return kw[0];
                            else {
                                if (kw[0] != kw[1])
                                    lex_state = EXPR.BEG;
                                return kw[1];
                            }
                        }
                    }

                    if (toklast() == '!' || toklast() == '?') {
                        result = Token.tFID;
                    }
                    else {
                        if (lex_state == EXPR.FNAME) {
                            if ((c = nextc()) == '=' && !peek('~') && !peek('>') &&
                                (!peek('=') || pcur + 1 < pend && lastline[pcur + 1] == '>')) {
                                result = Token.tIDENTIFIER;
                                tokadd(c);
                            }
                            else {
                                pushback(c);
                            }
                        }
                        if (result == 0 && Char.IsUpper(tok()[0])) {
                            result = Token.tCONSTANT;
                        }
                        else {
                            result = Token.tIDENTIFIER;
                        }
                    }
                    if (lex_state == EXPR.BEG ||
                        lex_state == EXPR.DOT ||
                        lex_state == EXPR.ARG) {
                        lex_state = EXPR.ARG;
                    }
                    else {
                        lex_state = EXPR.END;
                    }
                    break;
                }
                yylval = ruby.intern(tok());
                return result;
            }
        }

        internal bool is_identchar(int c)
        {
            if (Char.IsLetterOrDigit((char)c) || c == '_')
                return true;
            return false;
        }
        internal bool ISXDIGIT(char c)
        {
            return ("012345679ABCDEFabcdef".IndexOf(c) >= 0);
        }
        
        private string get_str(string s)
        {
            string rs;
            if (gets_ptr > 0)
            {
                if (s.Length == gets_ptr) return null;
            }
            int i = s.IndexOf('\n', gets_ptr);
            if (i < 0)
            {
                rs = s.Substring(gets_ptr);
                gets_ptr = s.Length;
                return rs;
            }
            rs = s.Substring(gets_ptr, i - gets_ptr);
            gets_ptr = i;
            return rs;
        }

        private string getline()
        {
            string s = reader.ReadLine();
            if (s == null)
            {
                return null;
            }
            return s + "\n";
        }

        private int nextc()
        {
            curr = 0;
            if (pcur == pend)
            {
                if (reader != null)
                {
                    string v = getline();

                    if (v == null)
                    {
                        return -1;
                    }
                    if (heredoc_end > 0)
                    {
                        thread.line = heredoc_end;
                        heredoc_end = 0;
                    }
                    thread.line++;
                    pbeg = 0;
                    pcur = 0;
                    pend = v.Length;
                    if (pend >= 7)
                    {
                            if (String.Compare(v, pbeg, "__END__", 0, 7) == 0
                            && (v.Length == 7 || v[7] == '\n' || v[7] == '\r'))
                        {
                            ruby.__end__seen = true;
                            lastline = String.Empty;
                            return -1;
                        }
                    }
                    lastline = v;
                }
                else
                {
                    lastline = String.Empty;
                    return -1;
                }
            }
            curr = lastline[pcur++];
            if (curr == '\r' && pcur <= pend && lastline[pcur] == '\n')
            {
                pcur++;
                curr = '\n';
            }
            return curr;
        }

        private void pushback(int c)
        {
            if (c == -1) return;
            pcur--;
        }

        private bool peek(char c)
        {
            return (pcur != pend && c == lastline[pcur]);
        }
        
        private string newtok()
        {
            tokenbuf.Length = 0;
            return String.Empty;
        }

        private void tokadd(int c)
        {
            tokenbuf.Append((char)c);
        }

        private int read_escape()
        {
            int c = nextc();
            switch (c)
            {
            case '\\':
                return c;

            case 'n':
                return '\n';

            case 't':
                return '\t';

            case 'r':
                return '\r';

            case 'f':
                return '\f';

            case 'v':
                return '\v';

            case 'a':
                return '\a';

            case 'e':
                return 0x1b;

            case '0': goto case '9';
            case '1': goto case '9';
            case '2': goto case '9';
            case '3': goto case '9';
            case '4': goto case '9';
            case '5': goto case '9';
            case '6': goto case '9';
            case '7': goto case '9';
            case '8': goto case '9';
            case '9': {
                pushback(c);
                string s = "";
                for (int i = 0; i < 3; i++)
                {
                    c = nextc();
                    if (c == -1) goto eof;
                    if (c < '0' || '7' < c) {
                        pushback(c);
                        break;
                    }
                    s += c;
                }
                c = Convert.ToByte(s, 8);
                return c; }
        
            case 'x':
                c = Convert.ToByte(lastline.Substring(pcur, 2), 16);
                pcur += 2;
                return c;

            case 'b':
                return '\b';

            case 's':
                return ' ';

            case 'M':
                if ((c = nextc()) != '-')
                {
                    return read_escape() | 0x80;
                }
                else if (c == -1) goto eof;
                else {
                    return ((c & 0xff) | 0x80);
                }

            case 'C':
                if ((c = nextc()) == '\\')
                {
                    c = read_escape();
                }
                else if (c == '?')
                    return 0177;
                else if (c == -1)
                    goto eof;
                return c & 0x9f;

            eof:
                goto case -1;
            case -1:
                parser.yyerror("Invalid escape character syntax");
                return 0;

            default:
                return c;
            }
        }

        private int tokadd_escape()
        {
            int c = nextc();
            switch (c)
            {
            case 'n':
                return 0;
        
            case '0': goto case '7';
            case '1': goto case '7';
            case '2': goto case '7';
            case '3': goto case '7';
            case '4': goto case '7';
            case '5': goto case '7';
            case '6': goto case '7';
            case '7': {
                tokadd('\\');
                tokadd(c);
                for (int i = 0; i < 2; i++)
                {
                    c = nextc();
                    if (c == -1) goto eof;
                    if (c < '0' || '7' < c) {
                        pushback(c);
                        break;
                    }
                    tokadd(c);
                }
            } return 0;
        
            case 'x': {
                tokadd('\\');
                tokadd(c);
                c = Convert.ToByte(lastline.Substring(pcur, 2), 16);
                tokadd(nextc());
                tokadd(nextc());
            } return 0;

            case 'M':
                if ((c = nextc()) != '-') {
                    parser.yyerror("Invalid escape character syntax");
                    pushback(c);
                    return 0;
                }
                tokadd('\\'); tokadd('M'); tokadd('-');
                goto escaped;

            case 'C':
                if ((c = nextc()) != '-') {
                    parser.yyerror("Invalid escape character syntax");
                    pushback(c);
                    return 0;
                }
                tokadd('\\'); tokadd('C'); tokadd('-');
                goto escaped;

            case 'c':
                tokadd('\\'); tokadd('c');

            escaped:
                if ((c = nextc()) == '\\') {
                    return tokadd_escape();
                }
                else if (c == -1) goto eof;
                tokadd(c);
                return 0;

            eof:
                goto case -1;
            case -1:
                parser.yyerror("Invalid escape character syntax");
                return -1;

            default:
                tokadd('\\');
                tokadd(c);
                break;
            }
            return 0;
        }
        
        int parse_regx(int term, int paren)
        {
            int c;
            char kcode = '\0';
            bool once = false;
            int nest = 0;
            RegexOptions options = RegexOptions.None;
            int re_start = thread.line;
            RNode list = null;

            newtok();
            while ((c = nextc()) != -1) {
                if (c == term && nest == 0) {
                    c = -100; // goto regx_end
                }

                switch (c) {
                case '#':
                    list = str_extend(list, term);
                    if (list is RNEOF) goto unterminated;
                    continue;

                case '\\':
                    if (tokadd_escape() < 0)
                        return 0;
                    continue;

                case -1:
                    goto unterminated;

                default:
                    if (paren != 0)  {
                        if (c == paren) nest++;
                        if (c == term) nest--;
                    }
                    break;

                case -100:
                regx_end:
                    for (;;) {
                        switch (c = nextc()) {
                        case 'i':
                            options |= RegexOptions.IgnoreCase;
                            break;
                        case 'x':
                            options |= RegexOptions.IgnorePatternWhitespace;
                            break;
                        case 'p':        /* /p is obsolete */
                            ruby.warn("/p option is obsolete; use /m\n\tnote: /m does not change ^, $ behavior");
                            break;
                        case 'm':
                            options |= RegexOptions.Multiline;
                            break;
                        case 'o':
                            once = true;
                            break;
                        case 'n':
                            kcode = '\x16';
                            break;
                        case 'e':
                            kcode = '\x32';
                            break;
                        case 's':
                            kcode = '\x48';
                            break;
                        case 'u':
                            kcode = '\x64';
                            break;
                        default:
                            pushback(c);
                            goto end_options;
                        }
                    }

                end_options:
                    lex_state = EXPR.END;
                    if (list != null) {
                        list.SetLine(re_start);
                        if (toklen() > 0) {
                            RNode.list_append(thread, list, new RNStr(thread, ruby, tok()));
                        }
                        if (once)
                            list = new RNDRegxOnce(list);
                        else
                            list = new RNDRegx(list);
                        list.cflag = (uint)options | (uint)kcode;
                        yylval = list;
                        return Token.tDREGEXP;
                    }
                    else {
                        yylval = RRegexpClass.s_new(ruby.cRegexp, tok(), options);
                        return Token.tREGEXP;
                    }
                }
                tokadd(c);
            }
        unterminated:
            thread.line = re_start;
            thread.CompileError("unterminated regexp meets end of file");
            return 0;
        }

        int parse_string(int func, int term, int paren)
        {
            int c;
            RNode list = null;
            int strstart;
            int nest = 0;

            if (func == '\'') {
                return parse_qstring(term, paren);
            }
            if (func == 0) {                /* read 1 line for heredoc */
                                /* -1 for chomp */
                yylval = lastline.Substring(pbeg, pend - pbeg - 1);
                pcur = pend;
                return Token.tSTRING;
            }
            strstart = thread.line;
            newtok();
            while ((c = nextc()) != term || nest > 0) {
                if (c == -1) {
                    thread.line = strstart;
                    thread.CompileError("unterminated string meets end of file");
                    return 0;
                }
                /*
                if (ismbchar(c)) {
                    int i, len = mbclen(c)-1;

                    for (i = 0; i < len; i++) {
                        tokadd(c);
                        c = nextc();
                    }
                }
                */
                else if (c == '#') {
                    list = str_extend(list, term);
                    if (list is RNEOF)
                    {
                        thread.line = strstart;
                        thread.CompileError("unterminated string meets end of file");
                        return 0;
                    }
                    continue;
                }
                else if (c == '\\') {
                    c = nextc();
                    if (c == '\n')
                        continue;
                    if (c == term) {
                        tokadd(c);
                    }
                    else {
                        pushback(c);
                        if (func != '"') tokadd('\\');
                        tokadd(read_escape());
                    }
                    continue;
                }
                if (paren != 0) {
                    if (c == paren) nest++;
                    if (c == term && nest-- == 0) break;
                }
                tokadd(c);
            }

            lex_state = EXPR.END;

            if (list != null) {
                list.SetLine(strstart);
                if (toklen() > 0) {
                    RNode.list_append(thread, list, new RNStr(thread, ruby, tok()));
                }
                yylval = list;
                if (func == '`') {
                    yylval = new RNDXStr(list);
                    return Token.tDXSTRING;
                }
                else {
                    return Token.tDSTRING;
                }
            }
            else {
                yylval = tok();
                return (func == '`') ? Token.tXSTRING : Token.tSTRING;
            }
        }

        int parse_qstring(int term, int paren)
        {
            int c;
            int nest = 0;

            int strstart = thread.line;
            newtok();
            while ((c = nextc()) != term || nest > 0) {
                if (c == -1) {
                    thread.line = strstart;
                    thread.CompileError("unterminated string meets end of file");
                    return 0;
                }
                /*
                if (ismbchar(c)) {
                    int i, len = mbclen(c)-1;

                    for (i = 0; i < len; i++) {
                        tokadd(c);
                        c = nextc();
                    }
                }
                */
                else if (c == '\\') {
                    c = nextc();
                    switch (c) {
                    case '\n':
                        continue;

                    case '\\':
                        c = '\\';
                        break;

                    case '\'':
                        if (term == '\'') {
                            c = '\'';
                            break;
                        }
                        goto default;
                    default:
                        tokadd('\\');
                        break;
                    }
                }
                if (paren != 0) {
                    if (c == paren) nest++;
                    if (c == term && nest-- == 0) break;
                }
                tokadd(c);
            }

            yylval = tok();
            lex_state = EXPR.END;
            return Token.tSTRING;
        }

        int parse_quotedwords(int term, int paren)
        {
            RNode qwords = null;
            int strstart = thread.line;
            int c;
            int nest = 0;

            newtok();

            for (c = nextc(); Char.IsWhiteSpace((char)c); c = nextc())
                ;                /* skip preceding spaces */
            pushback(c);
            while ((c = nextc()) != term || nest > 0) {
                if (c == -1) {
                    thread.line = strstart;
                    thread.CompileError("unterminated string meets end of file");
                    return 0;
                }
                /*
                if (ismbchar(c)) {
                    int i, len = mbclen(c)-1;

                    for (i = 0; i < len; i++) {
                        tokadd(c);
                        c = nextc();
                    }
                }
                */
                else if (c == '\\') {
                    c = nextc();
                    switch (c) {
                    case '\n':
                        continue;
                    case '\\':
                        c = '\\';
                        break;
                    default:
                        if (c == term) {
                            tokadd(c);
                            continue;
                        }
                        if (!Char.IsWhiteSpace((char)c))
                            tokadd('\\');
                        break;
                    }
                }
                else if (Char.IsWhiteSpace((char)c)) {

                    RNode str = new RNStr(thread, ruby, tok());
                    newtok();
                    if (qwords == null) qwords = new RNArray(thread, str);
                    else RNode.list_append(thread, qwords, str);
                    for (c = nextc(); Char.IsWhiteSpace((char)c); c = nextc())
                        ;                /* skip continuous spaces */
                    pushback(c);
                    continue;
                }

                if (paren != 0) {
                    if (c == paren) nest++;
                    if (c == term && nest-- == 0) break;
                }
                tokadd(c);
            }

            if (toklen() > 0) {
                RNode str = new RNStr(thread, ruby, tok());
                if (qwords == null) qwords = new RNArray(thread, str);
                else RNode.list_append(thread, qwords, str);
            }
            if (qwords == null) qwords = new RNZArray(thread);
            yylval = qwords;
            lex_state = EXPR.END;
            return Token.tDSTRING;
        }

        int here_document(int term, int indent)
        {
            int c;
            string line = String.Empty;
            RNode list = null;
            int linesave = thread.line;

            newtok();
            switch (term) {
            case '\'':
                goto case '`';
            case '"':
                goto case '`';
            case '`':
                while ((c = nextc()) != term) {
                    tokadd(c);
                }
                if (term == '\'') term = '\0';
                break;

            default:
                c = term;
                term = '"';
                if (!is_identchar(c)) {
                    ruby.warn("use of bare << to mean <<\"\" is deprecated");
                    break;
                }
                while (is_identchar(c)) {
                    tokadd(c);
                    c = nextc();
                }
                pushback(c);
                break;
            }
            string lastline_save = lastline;
            int offset_save = pcur - pbeg;
            string eos = string.Copy(tok());
            int len = eos.Length;

            string str = String.Empty;
            for (;;) {
                lastline = line = getline();
                if (line == null) {
                    thread.line = linesave;
                    thread.CompileError("can't find string \"" + eos + "\" anywhere before EOF");
                    return 0;
                }
                thread.line++;
                string p = line;
                if (indent > 0) {
                    while (p.Length > 0 && (p[0] == ' ' || p[0] == '\t')) {
                        p = p.Substring(1);
                    }
                }
                if (String.Compare(eos, 0, p, 0, len) == 0) {
                    if (p[len] == '\n' || p[len] == '\r')
                        break;
                    if (len == line.Length)
                        break;
                }
                pbeg = pcur = 0;
                pend = pcur + line.Length;
            retry:
                switch (parse_string(term, '\n', '\n')) {
                case Token.tSTRING:
                    // fall down to the next case
                case Token.tXSTRING:
                    {
                    yylval = (string)yylval + "\n";
                    }
                    if (list == null) {
                        str += (string)yylval;
                    }
                    else {
                        RNode.list_append(thread, list, new RNStr(thread, ruby, (string)yylval));
                    }
                    break;
                case Token.tDSTRING:
                    if (list == null) list = new RNDStr(thread, ruby, str);
                    goto case Token.tDXSTRING;
                case Token.tDXSTRING:
                    if (list == null) list = new RNDXStr(thread, ruby, str);

                    RNode.list_append(thread, (RNode)yylval, new RNStr(thread, ruby, "\n"));
                    RNStr val = new RNStr((RNStr)yylval);
                    yylval = new RNArray(thread, val);
                    ((RNode)yylval).next = ((RNode)yylval).head.next;
                    RNode.list_concat(list, (RNode)yylval);
                    break;

                case 0:
                    thread.line = linesave;
                    thread.CompileError("can't find string \"" + eos + "\" anywhere before EOF");
                    return 0;
                }
                if (pcur != pend) {
                    goto retry;
                }
            }
            lastline = lastline_save;
            pbeg = 0;
            pend = lastline.Length;
            pcur = offset_save;

            lex_state = EXPR.END;
            heredoc_end = thread.line;
            thread.line = linesave;
            if (list != null) {
                list.SetLine(linesave+1);
                yylval = list;
            }
            switch (term) {
            case '\0':
                goto case '"';
            case '\'':
                goto case '"';
            case '"':
                if (list != null) return Token.tDSTRING;
                yylval = str;
                return Token.tSTRING;
            case '`':
                if (list != null) return Token.tDXSTRING;
                yylval = str;
                return Token.tXSTRING;
            }
            return 0;
        }

        RNode str_extend(RNode list, int term)
        {
            int brace = -1;
            RNode node;
            int nest;

            int c = nextc();
            switch (c) {
            case '$':
                break;
            case '@':
                break;
            case '{':
                break;
            default:
                tokadd('#');
                pushback(c);
                return list;
            }

            string ss = tok();
            if (list == null) {
                list = new RNDStr(thread, ruby, ss);
            }
            else if (toklen() > 0) {
                RNode.list_append(thread, list, new RNStr(thread, ruby, ss));
            }
            newtok();

            switch (c) {
            case '$':
                tokadd('$');
                c = nextc();
                if (c == -1) return new RNEOF();
                switch (c) {
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    while (Char.IsDigit((char)c)) {
                        tokadd(c);
                        c = nextc();
                    }
                    pushback(c);
                    goto fetch_id;

                case '&':
                case '+':
                case '_':
                case '~':
                case '*':
                case '$':
                case '?':
                case '!':
                case '@':
                case ',':
                case '.':
                case '=':
                case ':':
                case '<':
                case '>':
                case '\\':
                refetch:
                    tokadd(c);
                    goto fetch_id;

                default:
                    if (c == term) {
                        RNode.list_append(thread, list, new RNStr(thread, ruby, "#$"));
                        pushback(c);
                        newtok();
                        return list;
                    }
                    switch (c) {
                    case '\"':
                    case '/':
                    case '\'':
                    case '`':
                        goto refetch;
                    }
                    if (!is_identchar(c)) {
                        parser.yyerror("bad global variable in string");
                        newtok();
                        return list;
                    }
                    break;
                }

                while (is_identchar(c)) {
                    tokadd(c);
                    /*
                    if (ismbchar(c)) {
                        int i, len = mbclen(c)-1;

                        for (i = 0; i < len; i++) {
                            c = nextc();
                            tokadd(c);
                        }
                    }
                    */
                    c = nextc();
                }
                pushback(c);
                break;

            case '@':
                tokadd(c);
                c = nextc();
                if (c == '@') {
                    tokadd(c);
                    c = nextc();
                }
                while (is_identchar(c)) {
                    tokadd(c);
                    /*
                    if (ismbchar(c)) {
                        int i, len = mbclen(c)-1;

                        for (i = 0; i < len; i++) {
                            c = nextc();
                            tokadd(c);
                        }
                    }
                    */
                    c = nextc();
                }
                pushback(c);
                break;

            case '{':
                if (c == '{') brace = '}';
                nest = 0;
                do {
                loop_again:
                    c = nextc();
                    switch (c) {
                    case -1:
                        if (nest > 0) {
                            parser.yyerror("bad substitution in string");
                            newtok();
                            return list;
                        }
                        return new RNEOF();
                    case '}':
                        if (c == brace) {
                            if (nest == 0) break;
                            nest--;
                        }
                        tokadd(c);
                        goto loop_again;
                    case '\\':
                        c = nextc();
                        if (c == -1) return new RNEOF();
                        if (c == term) {
                            tokadd(c);
                        }
                        else {
                            tokadd('\\');
                            tokadd(c);
                        }
                        break;
                    case '{':
                        if (brace != -1) nest++;
                        goto case '`';
                    case '\"':
                    case '/':
                    case '`':
                        if (c == term) {
                            pushback(c);
                            RNode.list_append(thread, list, new RNStr(thread, ruby, "#"));
                            ruby.warn("bad substitution in string");
                            RNode.list_append(thread, list, new RNStr(thread, ruby, tok()));
                            newtok();
                            return list;
                        }
                        goto default;
                    default:
                        tokadd(c);
                        break;
                    }
                } while (c != brace);
                break;
            }

        fetch_id:
            node = new RNEVStr(thread, ruby, tok());
            RNode.list_append(thread, list, node);
            newtok();

            return list;
        }

        private int quotation(int c)
        {
            int term;
            int paren;
            if (Char.IsLetterOrDigit((char)c) == false)
            {
                term = c;
                c = 'Q';
            }
            else {
                term = nextc();
#if CHECKQUOT        
                if (ISALNUM(term) || ismbchar(term)) {
                    parser.yyerror("unknown type of %string");
                    return 0;
                }
#endif        
            }
            if (c == -1 || term == -1) {
                thread.CompileError("unterminated quoted string meets end of file");
                return 0;
            }
            paren = term;
            if (term == '(') term = ')';
            else if (term == '[') term = ']';
            else if (term == '{') term = '}';
            else if (term == '<') term = '>';
            else paren = 0;

            switch (c) {
              case 'Q':
                return parse_string('"', term, paren);

              case 'q':
                return parse_qstring(term, paren);

              case 'w':
                return parse_quotedwords(term, paren);

              case 'x':
                return parse_string('`', term, paren);

              case 'r':
                return parse_regx(term, paren);

              default:
                parser.yyerror("unknown type of %string");
                return 0;
            }
        }
        void arg_ambiguous()
        {
            ruby.warning("ambiguous first argument; make sure");
        }
    }
    
    } // namepsace yyParser
}

// vim:et:sts=4:sw=4
