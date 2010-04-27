//
// Mono.AppServer.ApplicationType
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;
using System.IO;
using System.Xml;
using System.Runtime.Remoting;

namespace Mono.AppServer
{
	public class ApplicationType
	{
		public string ApplicationTypeName;
		public string AssemblyName;
		public string TypeName;

		public ApplicationType(string ApplicationTypeName,string AssemblyName,string TypeName)
		{
			this.ApplicationTypeName=ApplicationTypeName;
			this.AssemblyName=AssemblyName;
			this.TypeName=TypeName;
		}

		public ApplicationBase ApplicationFactory(ApplicationServer server, XmlDocument ConfigFile, DirectoryInfo BaseDir)
		{
			ObjectHandle handle=Activator.CreateInstance(AssemblyName,TypeName);
			ApplicationBase app=(ApplicationBase) handle.Unwrap();
			app.Configure(server,BaseDir,ConfigFile);
			return app;
		}
	}
}
