//
// Mono.AppServer.ApplicationAssembly
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;
using System.Reflection;

namespace Mono.AppServer
{
	[Serializable]
	public class ApplicationAssembly
	{
		private string _FullName;
		//public RemoteLoader loader;
		private ApplicationType[] _Types=null;

		public string FullName
		{
			get { return _FullName; }
		}

		public ApplicationAssembly(Assembly a)
		{
			_FullName=a.FullName;
		}
	}
}
