using System;
using System.Collections;
using System.Collections.Generic;

using Mono.Build;

using Monkeywrench.Compiler;

namespace Monkeywrench {

    public class TargetList : IEnumerable<BuildServices> {

	IEnumerable<BuildServices> inner;

	public TargetList (IEnumerable<BuildServices> inner)
	{
	    if (inner == null)
		throw new ArgumentNullException ();

	    this.inner = inner;
	}

	private TargetList ()
	{
	    inner = null;
	}

	public IEnumerator<BuildServices> GetEnumerator ()
	{
	    return inner.GetEnumerator ();
	}

	IEnumerator IEnumerable.GetEnumerator ()
	{
	    return inner.GetEnumerator ();
	}

	public bool Operate (OperationFunc func)
	{
	    if (inner == null)
		// TargetList.Null
		return false;

	    foreach (BuildServices bs in inner) {
		bs.Logger.PushLocation (bs.FullName);
		bool ret = func (bs.Project, bs);
		bs.Logger.PopLocation ();

		if (ret)
		    return true;
	    }

	    return false;
	}

	public static TargetList Null { 
	    get { return new TargetList (); }
	}

	// Exact basis matching

	IEnumerable<BuildServices> Exact (short pid)
	{
	    int matchid = (int) ((((uint) pid) << 16) & 0xFFFF0000);

	    foreach (BuildServices bs in inner) {
		if ((bs.Id & 0xFFFF0000) != matchid)
		    continue;

		yield return bs;
	    }
	}

	public TargetList FilterExact (short pid)
	{
	    if (pid < 0)
		throw new ArgumentException ();

	    if (inner == null)
		return TargetList.Null;

	    return new TargetList (Exact (pid));
	}

	// 'Here-and-below' tree matching

	IEnumerable<BuildServices> Inside (IGraphState graph, string here)
	{
	    Dictionary<short,bool> provider_ok = new Dictionary<short,bool> ();

	    foreach (BuildServices bs in inner) {
		short pid = (short) ((((uint) bs.Id) >> 16) & 0xFFFF);

		if (!provider_ok.ContainsKey (pid))
		    provider_ok[pid] = StrUtils.StartsWith (graph.GetProviderBasis (pid), here);

		if (!provider_ok[pid])
		    continue;

		yield return bs;
	    }
	}

	public TargetList FilterInside (IGraphState graph, short pid)
	{
	    if (pid < 0)
		throw new ArgumentException ();

	    if (inner == null)
		return TargetList.Null;

	    string here = graph.GetProviderBasis (pid);

	    return new TargetList (Inside (graph, here));
	}

	// Tag matching

	IEnumerable<BuildServices> Tag (int tag)
	{
	    foreach (BuildServices bs in inner) {
		bool has_it;

		if (bs.HasTag (tag, out has_it))
		    yield break;

		if (!has_it)
		    continue;

		yield return bs;
	    }
	}

	public TargetList FilterTag (int tag)
	{
	    if (tag < 0)
		throw new ArgumentException ("tag");

	    if (inner == null)
		return TargetList.Null;

	    return new TargetList (Tag (tag));
	}

	// Compat with mbuild client type interface

	public TargetList FilterScope (IGraphState graph, string here, OperationScope scope)
	{
	    if (inner == null)
		return TargetList.Null;

	    short hereid = graph.GetProviderId (here);
	    if (hereid < 0)
		throw ExHelp.App ("Nonexistant provider basis `{0}'", here);

	    switch (scope) {
	    case OperationScope.Everywhere:
		return this;
	    case OperationScope.HereAndBelow:
		return FilterInside (graph, hereid);
	    case OperationScope.HereOnly:
		return FilterExact (hereid);
	    default:
		throw ExHelp.App ("Unknown OperationScope {0} (here = {1})", scope, here);
	    }
	}
    }
}
