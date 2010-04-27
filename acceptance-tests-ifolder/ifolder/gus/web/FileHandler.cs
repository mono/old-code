using System;
using System.IO;
using System.Web;

namespace Gus
{
	/// <summary>
	/// File Handler
	/// </summary>
	public class FileHandler : IHttpHandler
	{
		public enum Operation
		{
			Download,
			Upload
		};

		public FileHandler()
		{
		}

		#region IHttpHandler Members

		public void ProcessRequest(HttpContext context)
		{
			// query
			Operation op = (Operation)Enum.Parse(typeof(Operation), context.Request.QueryString["operation"], true);
			string filename = context.Request.QueryString["filename"];
			
			string path = Path.Combine(context.Server.MapPath("files"), filename);

			int index = 0;
			int count = 0;

			try
			{
				index = int.Parse(context.Request.QueryString["index"]);
				count = int.Parse(context.Request.QueryString["count"]);
			}
			catch
			{
				// ignore
			}

			FileStream localFile;

			// operation
			switch(op)
			{
				case Operation.Download:
					
					// lock the file
					localFile = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			
					long length = (count == 0) ? localFile.Length : count;
					
					// adjust the length to the remaing length
					length = Math.Min(length, (localFile.Length - index));

					try
					{
						// response
						context.Response.Clear();
						context.Response.AddHeader("Content-Disposition", "attachment; filename=" + localFile.Name);
						context.Response.AddHeader("Content-Length", length.ToString());
						context.Response.ContentType = "application/octet-stream";
						context.Response.WriteFile(path, index, length);
						context.Response.End();
					}
					catch(Exception e)
					{
						throw e;
					}
					finally
					{
						// release the file
						localFile.Close();
						localFile = null;
					}
					break;

				case Operation.Upload:
					
					// lock the file
					localFile = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
					localFile.Seek(index, SeekOrigin.Begin);
			
					// read file
					Stream reader = context.Request.InputStream;

					try
					{
						const int BUFFER_SIZE = 4096;

						byte[] buffer = new byte[BUFFER_SIZE];

						int read = 0;

						while((read = reader.Read(buffer, 0, BUFFER_SIZE)) > 0)
						{
							localFile.Write(buffer, 0, read);
						}
					}
					catch(Exception e)
					{
						throw e;
					}
					finally
					{
						// release the file
						localFile.Close();
						localFile = null;

						// release the reader
						reader.Close();
					}
					break;

				default:
					break;
			}
		}

		public bool IsReusable
		{
			get { return false; }
		}

		#endregion
	}
}
