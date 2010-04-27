//
// Mono.AppServer.WebApplicationHost
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;
using Mono.ASPNET;
using Mono.AppServer;
using System.Collections;
using System.Reflection;

namespace Mono.AppServer
{
	public class WebApplicationHost : XSPApplicationHost
	{
		public WebApplicationHost() : base()
		{
		}

		public ApplicationAssembly[] GetLoadedAssemblies()
		{
			ArrayList AssemblyList=new ArrayList();
			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			{
				// ApplicationAssembly assembly=new ApplicationAssembly(a,this);
				Mono.AppServer.ApplicationAssembly assembly=new ApplicationAssembly(a);
				AssemblyList.Add(assembly);
			}
			return (ApplicationAssembly[]) AssemblyList.ToArray(typeof(ApplicationAssembly));
		}
	}
}
