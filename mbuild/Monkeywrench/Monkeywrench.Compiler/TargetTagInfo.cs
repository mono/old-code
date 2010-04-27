using System;

using Mono.Build;

namespace Monkeywrench.Compiler {

    public struct TargetTagInfo {

	public int Tag;
	public int Target;
	public Result ValResult;
	public int ValTarget;

	public TargetTagInfo (int tag, int targ, Result val)
	{
	    Tag = tag;
	    Target = targ;
	    ValResult = val;
	    ValTarget = -1;
	}

	public TargetTagInfo (int tag, int targ, int val)
	{
	    Tag = tag;
	    Target = targ;
	    ValResult = null;
	    ValTarget = val;
	}
    }
}
