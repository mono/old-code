using System;
using System.Collections;
using System.IO;

namespace Mono.AppServer
{
	public class ApplicationDirectory : MarshalByRefObject
	{
		private DirectoryInfo dir;

		internal ApplicationDirectory(DirectoryInfo dir)
		{
			this.dir=dir;
		}

		public string FullName
		{
			get
			{
				return dir.FullName;
			}
		}

		public string Name
		{
			get
			{
				return dir.Name;
			}
		}

		public ApplicationDirectory[] GetDirectories()
		{
			ArrayList subdirs=new ArrayList();
			foreach (DirectoryInfo subdir in dir.GetDirectories())
			{
				subdirs.Add(new ApplicationDirectory(subdir));
			}
			return (ApplicationDirectory[]) subdirs.ToArray(typeof(ApplicationDirectory));
		}

		public ApplicationFile[] GetFiles()
		{
			ArrayList filelist=new ArrayList();
			foreach (FileInfo file in dir.GetFiles())
			{
				filelist.Add(new ApplicationFile(file));
			}
			return (ApplicationFile[]) filelist.ToArray(typeof(ApplicationFile));
		}
	}
}
