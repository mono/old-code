using System;
using System.CodeDom;
using System.Text;

using Mono.Build;

namespace MBBundleGen {

    public class FingerprintHelper {

	StringBuilder hashtext = new StringBuilder ();

	public FingerprintHelper () {}

	public void Add (string s)
	{
	    hashtext.Append (s);
	}

	public void Add (NativeCode code)
	{
	    Add (code.Raw);
	}

	byte[] Hash {
	    get {
		return Fingerprint.FromText (hashtext.ToString ()).Value;
	    }
	}

	readonly static CodeTypeReference TRGeneric = 
	    new CodeTypeReference (typeof (Mono.Build.GenericFingerprints));
	readonly static CodeTypeReferenceExpression Generic = 
	    new CodeTypeReferenceExpression (TRGeneric);

	public void EmitGetFingerprint (CodeTypeDeclaration ctd)
	{
	    byte[] hash = Hash;
	    
	    CodeArrayCreateExpression mkdata = new CodeArrayCreateExpression (CDH.Byte, hash.Length);
	    
	    for (int i = 0; i < hash.Length; i++)
		// well, this for loop sucks
		mkdata.Initializers.Add (new CodePrimitiveExpression (hash[i]));
	    
	    CodeMemberMethod m = new CodeMemberMethod ();
	    m.Name = "GetFingerprint";
	    m.Attributes = MemberAttributes.Public | MemberAttributes.Override;
	    m.ReturnType = CDH.Fingerprint;
	    m.Parameters.Add (CDH.Param (CDH.IContext, "unused1"));
	    m.Parameters.Add (CDH.Param (CDH.Fingerprint, "unused2"));
			
	    CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression ();
	    invoke.Method = new CodeMethodReferenceExpression (Generic, "Constant");
	    invoke.Parameters.Add (mkdata);
	    
	    m.Statements.Add (new CodeMethodReturnStatement (invoke));
	    
	    ctd.Members.Add (m);
	}

	public void EmitImplementationCode (CodeTypeDeclaration ctd) 
	{
	    byte[] bytes = Hash;
	    int hash = 0;
	    
	    for (int i = 0; bytes.Length - i > 3; i += 4)
		hash ^= BitConverter.ToInt32 (bytes, i);
	    
	    // Insane Clown Property
	    
	    CodeMemberProperty icp = new CodeMemberProperty ();
	    icp.Name = "ImplementationCode";
	    icp.Attributes = MemberAttributes.Family | MemberAttributes.Override;
	    icp.Type = new CodeTypeReference (typeof (int));
	    icp.HasGet = true;
	    icp.HasSet = false;
	    
	    // get { return base.ImplementationCode ^ [number] } 
	    // becomes:
	    // get { return LameCodeDomXor (base.ImplementationCode, [number]); }
	    
	    CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression ();
	    invoke.Method = new CodeMethodReferenceExpression (CDH.This, "LameCodeDomXor");
	    invoke.Parameters.Add (new CodePropertyReferenceExpression (CDH.Base, "ImplementationCode"));
	    invoke.Parameters.Add (new CodePrimitiveExpression (hash));
	    
	    icp.GetStatements.Add (new CodeMethodReturnStatement (invoke));

	    ctd.Members.Add (icp);
	}
    }
}
