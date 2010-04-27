using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;

namespace Gus
{
	/// <summary>
	/// File Service
	/// </summary>
	[WebService(
		 Namespace="http://novell.com/gus/",
		 Name="FileService",
		 Description="Gus File Service")]
	public class FileService : WebService
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public FileService()
		{
		}

		[WebMethod(
			 Description="Open",
			 EnableSession=true)]
		public void Open()
		{
		}
	}
}
