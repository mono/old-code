using System;
using System.Collections.Generic;
using MDB=Mono.Debugger;

namespace Mono.VisualStudio.Mdb {

public class ServerBreakpoint : MarshalByRefObject, IServerBreakpoint
{
	public MDB.Event Event;

	public ServerBreakpoint (MDB.Event e)
	{
		this.Event = e;
	}
}
}
