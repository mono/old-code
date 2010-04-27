using System;
using System.Reflection;
using System.Collections.Generic;
using System.CodeDom;

namespace MBBundleGen {

    public class NativeCode {

	string code;
	CodeLinePragma location;

	public NativeCode (string code, CodeLinePragma location)
	{
	    this.code = code;
	    this.location = location;
	}

	// For FingerprintHelper
	public string Raw { get { return code; } }

	public CodeSnippetStatement AsStatement {
	    get {
		CodeSnippetStatement css = new CodeSnippetStatement (code);
		css.LinePragma = location;
		return css;
	    }
	}

	public CodeSnippetTypeMember AsMember {
	    get {
		CodeSnippetTypeMember csm = new CodeSnippetTypeMember (code);
		csm.LinePragma = location;
		return csm;
	    }
	}
    }
}

