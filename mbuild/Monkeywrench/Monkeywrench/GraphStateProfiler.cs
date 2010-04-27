using System;
using System.Text;
using System.Collections.Generic;

using Mono.Build;

namespace Monkeywrench.Compiler {

    public class GraphStateProfiler : IGraphState {

	IGraphState inner;

	public GraphStateProfiler (IGraphState inner)
	{
	    if (inner == null)
		throw new ArgumentNullException ("inner");

	    this.inner = inner;
	}

	int n_GetProviderBasis = 0;
	public string GetProviderBasis (short id)
	{
	    n_GetProviderBasis++;
	    return inner.GetProviderBasis (id);
	}

	int n_GetProviderDeclarationLoc = 0;
	public string GetProviderDeclarationLoc (short id)
	{
	    n_GetProviderDeclarationLoc++;
	    return inner.GetProviderDeclarationLoc (id);
	}

	int n_GetProviderId = 0;
	public short GetProviderId (string basis)
	{
	    n_GetProviderId++;
	    return inner.GetProviderId (basis);
	}

	int n_NumProviders = 0;
	public short NumProviders {
	    get {
		n_NumProviders++;
		return inner.NumProviders;
	    }
	}

	int n_GetProviderTargetBound = 0;
	public int GetProviderTargetBound (short id)
	{
	    n_GetProviderTargetBound++;
	    return inner.GetProviderTargetBound (id);
	}

	int n_GetTargetName = 0;
	public string GetTargetName (int tid)
	{
	    n_GetTargetName++;
	    return inner.GetTargetName (tid);
	}

	int n_GetTargetId = 0;
	public int GetTargetId (string target)
	{
	    n_GetTargetId++;
	    return inner.GetTargetId (target);
	}

	int n_GetTargetRuleType = 0;
	public Type GetTargetRuleType (int tid)
	{
	    n_GetTargetRuleType++;
	    return inner.GetTargetRuleType (tid);
	}

	int n_ApplyTargetDependencies = 0;
	public bool ApplyTargetDependencies (int tid, ArgCollector ac, IWarningLogger logger)
	{
	    n_ApplyTargetDependencies++;
	    return inner.ApplyTargetDependencies (tid, ac, logger);
	}

	int n_GetTargetTag = 0;
	public object GetTargetTag (int tid, int tag)
	{
	    n_GetTargetTag++;
	    return inner.GetTargetTag (tid, tag);
	}

	int n_GetTagId = 0;
	public int GetTagId (string tag)
	{
	    n_GetTagId++;
	    return inner.GetTagId (tag);
	}

	int n_GetTagName = 0;
	public string GetTagName (int tag)
	{
	    n_GetTagName++;
	    return inner.GetTagName (tag);
	}

	int n_GetTargetsWithTag = 0;
	public IEnumerable<TargetTagInfo> GetTargetsWithTag (int tag)
	{
	    n_GetTargetsWithTag++;
	    return inner.GetTargetsWithTag (tag);
	}

	int n_GetDependentFiles = 0;
	public IEnumerable<DependentItemInfo> GetDependentFiles ()
	{
	    n_GetDependentFiles++;
	    return inner.GetDependentFiles ();
	}

	int n_GetDependentBundles = 0;
	public IEnumerable<DependentItemInfo> GetDependentBundles ()
	{
	    n_GetDependentBundles++;
	    return inner.GetDependentBundles ();
	}

	int n_GetProjectInfo = 0;
	public ProjectInfo GetProjectInfo ()
	{
	    n_GetProjectInfo++;
	    return inner.GetProjectInfo ();
	}

	// And finally ...

	public override string ToString ()
	{
	    StringBuilder sb = new StringBuilder ();
	    string nl = Environment.NewLine;

	    sb.AppendFormat ("GetProviderBasis: {0}{1}", n_GetProviderBasis, nl);
	    sb.AppendFormat ("GetProviderDeclarationLoc: {0}{1}", n_GetProviderDeclarationLoc, nl);
	    sb.AppendFormat ("GetProviderId: {0}{1}", n_GetProviderId, nl);
	    sb.AppendFormat ("NumProviders: {0}{1}", n_NumProviders, nl);
	    sb.AppendFormat ("GetProviderTargetBound: {0}{1}", n_GetProviderTargetBound, nl);
	    sb.AppendFormat ("GetTargetName: {0}{1}", n_GetTargetName, nl);
	    sb.AppendFormat ("GetTargetId: {0}{1}", n_GetTargetId, nl);
	    sb.AppendFormat ("GetTargetRuleType: {0}{1}", n_GetTargetRuleType, nl);
	    sb.AppendFormat ("ApplyTargetDependencies: {0}{1}", n_ApplyTargetDependencies, nl);
	    sb.AppendFormat ("GetTargetTag: {0}{1}", n_GetTargetTag, nl);
	    sb.AppendFormat ("GetTagId: {0}{1}", n_GetTagId, nl);
	    sb.AppendFormat ("GetTagName: {0}{1}", n_GetTagName, nl);
	    sb.AppendFormat ("GetTargetsWithTag: {0}{1}", n_GetTargetsWithTag, nl);
	    sb.AppendFormat ("GetDependentFiles: {0}{1}", n_GetDependentFiles, nl);
	    sb.AppendFormat ("GetDependentBundles: {0}{1}", n_GetDependentBundles, nl);
	    sb.AppendFormat ("GetProjectInfo: {0}{1}", n_GetProjectInfo, nl);

	    return sb.ToString ();
	}
    }
}
