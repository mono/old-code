//
// ConstantTarget.cs -- a target that just yields a specified result
//

using System;
using System.Collections;

using Mono.Build.RuleLib;

namespace Mono.Build {

    public class ConstantTarget : ITarget { 
		
	string name;
	Result result;

	public ConstantTarget (string name, Result result) 
	{
	    if (name == null)
		throw new ArgumentNullException ("name");
	    if (result == null)
		throw new ArgumentNullException ("result");

	    this.name = name;
	    this.result = result;
	}

	public string Name { get { return name; } }

	public Type RuleType { get { return typeof (CloneRule); } }
		
	public bool VisitDependencies (IDependencyVisitor<string,string> idv)
	{
	    return idv.VisitUnnamedResult (result);
	}

	public bool VisitTags (ITagVisitor<string> itv)
	{
	    return false;
	}
    }
}
