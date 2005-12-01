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
using System.Collections;
using System.IO;

namespace CocoaSharp {
	public class WriteCS {
		public WriteCS(Configuration config) {
			mConfig = config;
		}

		public void AddRange(ICollection outputElements) {
			mOutputElements.AddRange(outputElements);
		}

		public void OutputNamespace(string nameSpace, string framework) {
			Console.Write("Output Namespace ({0}): ", nameSpace);
			Console.Write("00%");

			IList outList = new ArrayList();
			AddNamespaceElements(outList,mOutputElements,nameSpace);

			int count = 0, total = outList.Count;
			foreach(OutputElement e in outList) {
				e.WriteFile(mConfig);
				WriteProgress(ref count,total);
				mOutputElements.Remove(e);
			}

			if(Directory.Exists(Path.Combine(mConfig.CorePath, framework))) {
				DirectoryInfo _frameworkDirectory = new DirectoryInfo(Path.Combine(mConfig.CorePath, framework));
				FileSystemInfo[] _infos = _frameworkDirectory.GetFileSystemInfos();
				foreach(FileSystemInfo _file in _infos) {
					if(_file.Name.EndsWith(".cs")) {
						string fileName = Path.Combine(Path.Combine("src", nameSpace), _file.Name);
						if(File.Exists(fileName))
							File.Delete(fileName);
						File.Copy(Path.Combine(Path.Combine(mConfig.CorePath, framework), _file.Name), fileName);
					}
				}
			}

			Console.WriteLine("\b\b\b100% (" + this.mOutputElements.Count + " left)");
		}

		#region -- Members --
		Configuration mConfig;
		ArrayList mOutputElements = new ArrayList();
		#endregion

		#region -- Internals --
		void WriteProgress(ref int count,int total) {
			Console.Write("\b\b\b{0:00}%", count++/(float)total*100);
		}

		void AddNamespaceElements(IList outputList,ICollection inputCollection,string nameSpace) {
			foreach(OutputElement e in inputCollection)
				if(e.Namespace == nameSpace)
					outputList.Add(e);
		}
		#endregion
	}
}

//
// $Log: WriteCS.cs,v $
// Revision 1.3  2004/09/20 20:18:23  gnorton
// More refactoring; Foundation almost gens properly now.
//
// Revision 1.2  2004/09/20 16:42:52  gnorton
// More generator refactoring.  Start using the MachOGen for our classes.
//
// Revision 1.1  2004/09/18 17:30:17  urs
// Move CS output gen into gen-out
//
