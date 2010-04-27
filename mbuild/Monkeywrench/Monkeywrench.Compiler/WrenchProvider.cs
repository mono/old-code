// WrenchProvider.cs -- Monkeywrench implementation of the abstract
// ProviderBuilder class.

using System;
using System.Collections;
using System.Collections.Generic;

using Mono.Build;
using Mono.Build.Bundling;

namespace Monkeywrench.Compiler {

    public class WrenchProvider : ProviderBuilder {
	short id;
	short next_target_id = 0;

	NameLookupContext namecontext;

	internal WrenchProvider (GraphBuilder gb, string basis, short id) 
	    : base (gb, basis) 
	{
	    namecontext = new NameLookupContext ();

	    this.id = id;
	}

	public short Id { get { return id; } }

	public short NumTargets { get { return next_target_id; } }

	// Context

	public NameLookupContext NameContext { 
	    get { return namecontext; } 
	    
	    set {
		if (value == null)
		    throw new ArgumentNullException ();
		
		namecontext = value;
	    }
	}

	public override void AddContextStructure (StructureTemplate st)
	{
	    namecontext.UseStructure (st);
	}

	// Target table

	Dictionary<string,int> targids = new Dictionary<string,int> ();

	protected override TargetBuilder CreateTarget (string name)
	{
	    WrenchTarget wt = new WrenchTarget (this, name, next_target_id++);

	    targids[name] = wt.Id;

	    return wt;
	}

	public int LookupTarget (string name)
	{
	    return targids[name];
	}

	public bool Finish (BundleManager bm, IWarningLogger log) 
	{
	    if (DeclarationLoc == null)
		throw ExHelp.App ("WP Finish, basis {0}", Basis);

	    if (namecontext.SetManager (bm, log))
		return true;

	    if (DoneRequesting (namecontext, log))
		return true;

	    foreach (WrenchTarget wt in DefinedTargets) {
		if (wt.DoFixup (namecontext, log))
		    return true;
	    }

	    return false;
	}
    }
}	
