using System;
using System.IO;

namespace Mono.AppServer
{
	public class ApplicationFile : MarshalByRefObject
	{
		private FileInfo file;

		internal ApplicationFile(FileInfo file)
		{
			this.file=file;
		}

		public string FullName
		{
			get
			{
				return file.FullName;
			}
		}

		public string Name
		{
			get
			{
				return file.Name;
			}
		}

		public DateTime LastWriteTime
		{
			get
			{
				return file.LastWriteTime;
			}
		}

		public long Length
		{
			get
			{
				return file.Length;
			}
		}

	}
}
