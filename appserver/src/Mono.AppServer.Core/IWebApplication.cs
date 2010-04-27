using System;

namespace Mono.AppServer
{
	public interface IWebApplication1
	{
		int Port
		{
			get;
		}

		string VirtualDirectory
		{
			get;
		}

		string[] GetWebServices();
	}
}
