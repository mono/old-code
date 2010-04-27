//
// Mono.AppServer.WebApplication
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;
using System.IO;
using System.Xml;
using System.Web.Hosting;
using Mono.ASPNET;
using System.Collections;
using System.Net;

namespace Mono.AppServer
{
	public class WebApplication : ApplicationBase, IWebApplication1
	{
		protected WebApplicationHost host;
		protected int _Port;
		protected string _VirtDir;

		public WebApplication() : base()
		{
		}

		public int Port
		{
			get
			{
				return _Port;
			}
		}

		public string VirtualDirectory
		{
			get
			{
				return _VirtDir;
			}
		}

		public override void Load()
		{
          base.Load();
			if (host==null)
			{
				Type type = typeof (WebApplicationHost);
				host =  (WebApplicationHost) ApplicationHost.CreateApplicationHost (type, VirtualDirectory, BaseDirectory);
				host.SetListenAddress (Port);
			}
			host.Start ();
			_Loaded=true;
		}

		public override void Unload()
		{
			host.Stop();
			_Loaded=false;
            base.Unload();
		}

		public override ApplicationAssembly[] GetLoadedAssemblies()
		{
			return host.GetLoadedAssemblies();
		}

		public override void Configure(ApplicationServer server, DirectoryInfo BaseDir, XmlDocument ConfigFile)
		{
			base.Configure(server,BaseDir,ConfigFile);
			_Port=int.Parse(ReadAppSetting(ConfigFile,"WebApplication.Port","21"));
			_VirtDir="/";
		}

		public string[] GetWebServices()
		{
			ArrayList services=new ArrayList();
			DirectoryInfo BaseDir=new DirectoryInfo(BaseDirectory);
			Console.WriteLine("Looking in {0} for Web Services",BaseDirectory);
			foreach (FileInfo f in BaseDir.GetFiles("*.asmx"))
			{
				Console.WriteLine("Found: {0}",f.Name);
				string url=string.Format("http://localhost:{0}/{1}", Port, f.Name);
				HttpWebRequest request=(HttpWebRequest) WebRequest.Create(url);
				WebResponse response=request.GetResponse();
				StreamReader reader=new StreamReader(response.GetResponseStream());
				string s=reader.ReadToEnd();
				int i1=s.IndexOf(".asmx?WSDL");
				int i2=s.IndexOf("</ul>",i1);
				s=s.Substring(i1,i2-i1);
				services.Add(f.Name+":"+s);
			}
			return (string[]) services.ToArray(typeof(string));
		}
	}
}
