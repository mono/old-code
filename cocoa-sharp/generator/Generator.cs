//
//  Authors
//    - Kangaroo, Geoff Norton
//    - Urs C. Muff, Quark Inc., <umuff@quark.com>
//
//  Copyright (c) 2004 Quark Inc.  All rights reserved.
//
// $Id$
//

using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Xml.Serialization;

namespace CocoaSharp {

	/// <summary>
	/// 
	/// </summary>
	public class Generator {
		private static string mXmlFile = "generator.xml";
		private static Configuration mConfig;

		public Generator() {}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="framework"></param>
		/// <returns></returns>
		string GetFileName(string framework) {
			foreach (string format in new string[] {
				"/System/Library/Frameworks/{0}.framework/{0}",
				"/Library/Frameworks/{0}.framework/{0}",
				"~/Library/Frameworks/{0}.framework/{0}",
				"{0}",
			}) {
				string fileName = string.Format(format,framework);
				if (File.Exists(fileName))
					return fileName;
			}
			Console.Error.WriteLine("ERROR: Framework {0} not found",framework);
			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="f"></param>
		/// <returns></returns>
		public bool Run(Framework f) {
			MachOFile mfile = ProcessFramework(f);
			if (mfile == null)
				return false;

			WriteCS csWriter = new WriteCS(mConfig);
			csWriter.AddRange(mfile.Classes);
			csWriter.OutputNamespace(f.NameSpace, f.Name);
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="f"></param>
		/// <returns></returns>
		public MachOFile ProcessFramework(Framework f) {
			string fileName = GetFileName(f.Name);
			if (fileName == null)
				return null;
			MachOFile mfile = new MachOFile(fileName);
			// ToClass needs namespace we set that property in MachOFile
			mfile.Namespace = f.NameSpace;
			return mfile;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public int Run() {
			foreach (Framework f in mConfig.Frameworks)
				if (!Run(f))
					continue;

			return 1;
		}

		static int Main (string[] args) {
			if(!LoadConfiguration(args))
				return -1;

#if !MACH_O
			ObjCManagedExporter exporter = new ObjCManagedExporter(mConfig);
			Generator g = new Generator();
			WriteCS csWriter = new WriteCS(mConfig);
			ArrayList mFiles = new ArrayList();

			foreach (Framework f in mConfig.Frameworks) {
				exporter.ProcessFramework(f);
				mFiles.Add(g.ProcessFramework(f));
			}

			exporter.BuildInterfaces();

			csWriter.AddRange(HeaderEnum.ToOutput(exporter.Enums.Values));
			csWriter.AddRange(HeaderStruct.ToOutput(exporter.Structs.Values));
			csWriter.AddRange(HeaderProtocol.ToOutput(exporter.Protocols.Values));
			csWriter.AddRange(HeaderInterface.ToOutput(exporter.Interfaces.Values));

			foreach (MachOFile mfile in mFiles)
				csWriter.AddRange(mfile.Classes);

			Console.WriteLine("Output per namespace");
			foreach (Framework f in mConfig.Frameworks)
				csWriter.OutputNamespace(f.NameSpace, f.Name);

			int retval = 0;
#else
            int retval = g.Run();
#endif
			Console.WriteLine("Updating mapping.");
			Method.SaveMapping();
			return retval;
		}

		private static bool LoadConfiguration(string[] args) {
			// Ensure the file exists
			foreach (string a in args)
				if(a.IndexOf("-xml:") == 0)
					mXmlFile = a.Substring(5);
			if(!File.Exists(mXmlFile)) {
				Console.WriteLine("ERROR: Generator cannot run; XML File ({0}) does not exist", mXmlFile);
				return false;
			}

			Configuration.XmlPath = new FileInfo(mXmlFile).DirectoryName;

			// Deserialize our frameworks file
			XmlTextReader _xmlreader = new XmlTextReader(mXmlFile);
			XmlSerializer _serializer = new XmlSerializer(typeof(Configuration));
			mConfig = (Configuration)_serializer.Deserialize(_xmlreader);
			return true;
		}
	}
}

//	$Log: Generator.cs,v $
//	Revision 1.2  2004/09/21 04:28:53  urs
//	Shut up generator
//	Add namespace to generator.xml
//	Search for framework
//	Fix path issues
//	Fix static methods
//
//	Revision 1.1  2004/09/20 16:42:52  gnorton
//	More generator refactoring.  Start using the MachOGen for our classes.
//	
