// Parser-rest.cs : build the AST of our bizarre little language

using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.CodeDom;

namespace MBBundleGen {
	
    public partial class Parser {

	// Mostly copied from cs-parser.jay / BuildfileParser.jay

	Tokenizer lexer;
	Driver driver;

	static int yacc_verbose_flag = 1;
	public static bool DebugParser; // hack hack

	// public

	public Parser (StreamReader reader, string file, Driver driver)
	{
	    lexer = new Tokenizer (reader, file);
	    this.driver = driver;
	}

	public static Parser CreateForFile (string file, Driver driver)
	{
	    FileStream fs = new FileStream (file, FileMode.Open, FileAccess.Read);
	    BufferedStream bs = new BufferedStream (fs);
	    StreamReader reader = new StreamReader (bs);
	    
	    return new Parser (reader, file, driver);
	}

	public Tokenizer Lexer {
	    get {
		return lexer;
	    }
	}		  

	public int Parse ()
	{
	    try {
		if (DebugParser)
		    yyparse (lexer, new yydebug.yyDebugSimple ());
		else
		    yyparse (lexer);
		lexer.Cleanup ();	   
	    } catch (yyParser.yyException) {
		Console.WriteLine ("{0}: Fatal parse error", lexer.Location);
		return 1;
	    } catch (Exception e) {
		Console.WriteLine ("{0}: {1}", lexer.Location, e);
		return 1;
	    }
	    
	    return 0;
	}

	// Hooks

	NamespaceBuilder cur_ns = null;
	TypeExpressedItem cur_tei = null;
	MetaRuleBuilder cur_rule = null;
	BGProviderBuilder cur_prov = null;
	BGTargetBuilder cur_targ = null;
	StructureBuilder cur_nsstruct = null;
	EnumResultBuilder cur_enum = null;
	ResultBuilder cur_res = null;
	TemplateBuilder cur_tmpl = null;

	List<NamespaceBuilder> namespaces = new List<NamespaceBuilder> ();

	void NewNamespace (string name)
	{
	    // We cannot reuse because different namespace {} clauses
	    // of the same value may have different usings.

	    cur_ns = new NamespaceBuilder (name, new TypeResolveContext (driver));
	    namespaces.Add (cur_ns);
	}

	void CloseNamespace ()
	{
	    if (cur_ns.Params == null)
		// If we haven't declared any parameters for this space
		// yet, and we don't reference a library that has, 
		// create our own parameterless structurebuilder.
		new StructureBuilder (cur_ns, lexer.LinePragma, 0);

	    cur_ns = null;
	}

	void AddNamespaceParam (string ns)
	{
	    // FIXME: we can't distinguish references to A.Foo and B.Foo!
	    // Need a syntax for this.

	    UserType t = new UserType (NamespaceBuilder.MakeStructureName (ns));

	    string paramname = ns;
	    int i = ns.LastIndexOf ('.');

	    if (i >= 0)
		paramname = ns.Substring (i + 1);

	    cur_nsstruct.AddStructureParam (t, paramname, ns);
	}

	public bool Resolve (bool errors)
	{
	    bool ret = false;

	    foreach (NamespaceBuilder ns in namespaces)
		ret |= ns.Resolve (errors);

	    return ret;
	}

	public CodeCompileUnit Emit ()
	{
	    CodeCompileUnit unit = new CodeCompileUnit ();
	    
	    foreach (NamespaceBuilder ns in namespaces)
		unit.Namespaces.Add (ns.Emit ());
	    
	    return unit;
	}
	
    }
}
