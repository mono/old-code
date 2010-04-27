using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;

using NUnit.Framework;

namespace Gus
{
	/// <summary>
	/// Handler Tests
	/// </summary>
	[TestFixture]
	public class HandlerTests
	{
		static readonly string uri = "http://localhost:8080/FileHandler.aspx";
		//static readonly string uri = "http://localhost/Gus/FileHandler.aspx";
		static readonly string serverPath = "../web/files";
		static readonly string clientPath = "./files";

		static readonly int BUFFER_SIZE = 4096;

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			// server files
			if (Directory.Exists(serverPath))
			{
				Directory.Delete(serverPath, true);
			}

			Directory.CreateDirectory(serverPath);

			// client files
			if (Directory.Exists(clientPath))
			{
				Directory.Delete(clientPath, true);
			}

			Directory.CreateDirectory(clientPath);
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
		}

		[SetUp]
		public void Setup()
		{
		}

		[TearDown]
		public void TearDown()
		{
		}

		[Test]
		public void Download_1K()
		{
			DownloadFile(MethodInfo.GetCurrentMethod().Name, 1024);
		}

		[Test]
		public void Upload_1K()
		{
			UploadFile(MethodInfo.GetCurrentMethod().Name, 1024);
		}

		[Test]
		public void Download_1M()
		{
			DownloadFile(MethodInfo.GetCurrentMethod().Name, (1024 * 1024));
		}

		[Test]
		public void Upload_1M()
		{
			UploadFile(MethodInfo.GetCurrentMethod().Name, (1024 * 1024));
		}

		[Test]
		public void Download_4M()
		{
			DownloadFile(MethodInfo.GetCurrentMethod().Name, (4 * 1024 * 1024));
		}

		[Test]
		public void Upload_4M()
		{
			UploadFile(MethodInfo.GetCurrentMethod().Name, (4 * 1024 * 1024));
		}

		[Test]
		public void Download_16M_Chunked4M()
		{
			DownloadFile(MethodInfo.GetCurrentMethod().Name, (16 * 1024 * 1024), (4 * 1024 * 1024));
		}

		[Test]
		public void Upload_16M_Chunked4M()
		{
			UploadFile(MethodInfo.GetCurrentMethod().Name, (16 * 1024 * 1024), (4 * 1024 * 1024));
		}

		[Test]
		public void Download_1M_Plus1()
		{
			DownloadFile(MethodInfo.GetCurrentMethod().Name, (1024 * 1024) + 1);
		}

		[Test]
		public void Upload_1M_Plus1()
		{
			UploadFile(MethodInfo.GetCurrentMethod().Name, (1024 * 1024) + 1 );
		}

		[Test]
		public void Download_1M_Minus1()
		{
			DownloadFile(MethodInfo.GetCurrentMethod().Name, (1024 * 1024) - 1);
		}

		[Test]
		public void Upload_1M_Minus1()
		{
			UploadFile(MethodInfo.GetCurrentMethod().Name, (1024 * 1024) - 1 );
		}

		[Test]
		public void Download_1M_Chunked4K()
		{
			DownloadFile(MethodInfo.GetCurrentMethod().Name, (1024 * 1024), (4 * 1024));
		}

		[Test]
		public void Upload_1M_Chunked4K()
		{
			UploadFile(MethodInfo.GetCurrentMethod().Name, (1024 * 1024), (4 * 1024));
		}

		[Test]
		public void Download_4K_Chunked64K()
		{
			DownloadFile(MethodInfo.GetCurrentMethod().Name, (4 * 1024), (64 * 1024));
		}

		[Test]
		public void Upload_4K_Chunked64K()
		{
			UploadFile(MethodInfo.GetCurrentMethod().Name, (4 * 1024), (64 * 1024));
		}

		[Test]
		public void Download_1M_Chunked2K()
		{
			DownloadFile(MethodInfo.GetCurrentMethod().Name, (1024 * 1024), (2 * 1024));
		}

		[Test]
		public void Upload_1M_Chunked2K()
		{
			UploadFile(MethodInfo.GetCurrentMethod().Name, (1024 * 1024), (2 * 1024));
		}

		[Test]
		public void Download_4K_Plus1_Chunked4K()
		{
			DownloadFile(MethodInfo.GetCurrentMethod().Name, (4 * 1024) + 1, (4 * 1024));
		}

		[Test]
		public void Upload_4K_Plus1_Chunked4K()
		{
			UploadFile(MethodInfo.GetCurrentMethod().Name, (4 * 1024) + 1, (4 * 1024));
		}

		[Test]
		public void Download_4K_Minus1_Chunked4K()
		{
			DownloadFile(MethodInfo.GetCurrentMethod().Name, (4 * 1024) - 1, (4 * 1024));
		}

		[Test]
		public void Upload_4K_Minus1_Chunked4K()
		{
			UploadFile(MethodInfo.GetCurrentMethod().Name, (4 * 1024) - 1, (4 * 1024));
		}

		[Test]
		public void Download_1M_Plus1_Chunked4K()
		{
			DownloadFile(MethodInfo.GetCurrentMethod().Name, (1024 * 1024) + 1, (4 * 1024));
		}

		[Test]
		public void Upload_1M_Plus1_Chunked4K()
		{
			UploadFile(MethodInfo.GetCurrentMethod().Name, (1024 * 1024) + 1, (4 * 1024));
		}

		[Test]
		public void Download_1M_Minus1_Chunked4K()
		{
			DownloadFile(MethodInfo.GetCurrentMethod().Name, (1024 * 1024) - 1, (4 * 1024));
		}

		[Test]
		public void Upload_1M_Minus1_Chunked4K()
		{
			UploadFile(MethodInfo.GetCurrentMethod().Name, (1024 * 1024) - 1, (4 * 1024));
		}

		[Test]
		public void Download_1M_Plus1_Chunked64K()
		{
			DownloadFile(MethodInfo.GetCurrentMethod().Name, (1024 * 1024) + 1, (64 * 1024));
		}

		[Test]
		public void Upload_1M_Plus1_Chunked64K()
		{
			UploadFile(MethodInfo.GetCurrentMethod().Name, (1024 * 1024) + 1, (64 * 1024));
		}

		[Test]
		public void Download_1M_Minus1_Chunked64K()
		{
			DownloadFile(MethodInfo.GetCurrentMethod().Name, (1024 * 1024) - 1, (64 * 1024));
		}

		[Test]
		public void Upload_1M_Minus1_Chunked64K()
		{
			UploadFile(MethodInfo.GetCurrentMethod().Name, (1024 * 1024) - 1, (64 * 1024));
		}

		[Test]
		public void Download_1M_Chunked64K()
		{
			DownloadFile(MethodInfo.GetCurrentMethod().Name, (1024 * 1024), (64 * 1024));
		}

		[Test]
		public void Upload_1M_Chunked64K()
		{
			UploadFile(MethodInfo.GetCurrentMethod().Name, (1024 * 1024), (64 * 1024));
		}

		[Test]
		public void Download_Clients6_16M()
		{
			DownloadFileMultiple(MethodInfo.GetCurrentMethod().Name, (1024 * 1024 * 16), 6, 1);
		}

		[Test]
		public void Download_Clients6_Reps6_16M()
		{
			DownloadFileMultiple(MethodInfo.GetCurrentMethod().Name, (1024 * 1024 * 16), 6, 6);
		}

		public void DownloadFileMultiple(string filename, int size, int count, int reps)
		{
			string clientFile = Path.Combine(clientPath, filename);
			string serverFile = Path.Combine(serverPath, filename);

			// setup file
			TestUtilities.CreateFile(serverFile, size);

			// setup clients
			DownloadWork[] workers = new DownloadWork[count];
			Thread[] threads = new Thread[count];

			for(int i=0; i < count; i++)
			{
				workers[i] = new DownloadWork(String.Format("{0}{1}", clientFile, i), serverFile, size, reps);
				threads[i] = new Thread(new ThreadStart(workers[i].DoWork));
			}

			// start clients
			for(int i=0; i < count; i++)
			{
				threads[i].Start();
			}

			// wait on clients
			for(int i=0; i < count; i++)
			{
				try
				{
					threads[i].Join();
				}
				catch
				{
					// ignore
				}
			}
		}

		public static void DownloadFile(string filename, int size)
		{
			DownloadFile(filename, size, 0);
		}

		public static void DownloadFile(string filename, int size, int chunk)
		{
			string clientFile = Path.Combine(clientPath, filename);
			string serverFile = Path.Combine(serverPath, filename);

			TestUtilities.CreateFile(serverFile, size);

			DownloadFile(clientFile, serverFile, size, chunk);
		}

		public static void DownloadFile(string clientFile, string serverFile, int size)
		{
			DownloadFile(clientFile, serverFile, size, 0);
		}

		public static void DownloadFile(string clientFile, string serverFile, int size, int chunk)
		{
			FileStream localFile = null;

			try
			{
				localFile = File.Create(clientFile);

				int index = 0;

				while(index < size)
				{
					string url = String.Format("{0}?operation=download&filename={1}", uri, Path.GetFileName(serverFile));
				
					if (chunk != 0)
					{
						url = String.Format("{0}&index={1}&count={2}", url, index, chunk);
					}

					HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
					//request.KeepAlive = false;
					request.Method = "GET";

					HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();

					Stream webStream = webResponse.GetResponseStream();

					try
					{
						byte[] buffer = new byte[BUFFER_SIZE];
						int length = 0;

						while((length = webStream.Read(buffer, 0, BUFFER_SIZE)) > 0)
						{
							localFile.Write(buffer, 0, length);
							index += length;
						}
					}
					finally
					{
						webStream.Close();

						webResponse.Close();
					}
				}
			}
			finally
			{
				// close file
				if (localFile != null)
				{
					localFile.Close();
				}
			}

			// compare
			Assert.IsTrue(TestUtilities.FilesEqual(serverFile, clientFile), "Files Differ");
		}

		public static void UploadFile(string filename, int size)
		{
			UploadFile(filename, size, 0);
		}

		public static void UploadFile(string filename, int size, int chunk)
		{
			string clientFile = Path.Combine(clientPath, filename);
			string serverFile = Path.Combine(serverPath, filename);

			TestUtilities.CreateFile(clientFile, size);

			UploadFile(clientFile, serverFile, size, chunk);
		}

		public static void UploadFile(string clientFile, string serverFile, int size)
		{
			UploadFile(clientFile, serverFile, size, 0);
		}

		public static void UploadFile(string clientFile, string serverFile, int size, int chunk)
		{
			FileStream localFile = null;

			try
			{
				localFile = File.OpenRead(clientFile);

				int index = 0;

				while(index < size)
				{
					string url = String.Format("{0}?operation=upload&filename={1}", uri, Path.GetFileName(clientFile));

					int length = 0;

					if (chunk != 0)
					{
						length = chunk;
						url = String.Format("{0}&index={1}", url, index);

						// adjust the length to the remaing length
						length = Math.Min(length, (size - index));
					}
					else
					{
						length = size;
					}

					HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
					request.CookieContainer = new CookieContainer();
					request.Method = "POST";
					request.ContentType = "application/octet-stream";
					request.ContentLength = length;

					Stream webStream = request.GetRequestStream(); 
				
					try
					{
						int wrote = 0;

						while(wrote < length)
						{
							int count = Math.Min(length, BUFFER_SIZE);
								
							byte[] buffer = new byte[count];

							count = localFile.Read(buffer, 0, count);

							Assert.IsTrue(count > 0);

							webStream.Write(buffer, 0, count);
	
							wrote += count;
						}

						index += wrote;
					}
					finally
					{
						webStream.Close();
					}

					request.GetResponse().Close();
				}
			}
			finally
			{
				// close file
				if (localFile != null)
				{
					localFile.Close();
				}
			}

			// compare
			Assert.IsTrue(TestUtilities.FilesEqual(serverFile, clientFile), "Files Differ");
		}
	}

	public class DownloadWork
	{
		private string clientFile;
		private string serverFile;
		private int size;
		private int reps;

		public DownloadWork(string clientFile, string serverFile, int size, int reps)
		{
			this.clientFile = clientFile;
			this.serverFile = serverFile;
			this.size = size;
			this.reps = reps;
		}

		public void DoWork()
		{
			for(int i=0; i < reps; i++)
			{
				HandlerTests.DownloadFile(clientFile, serverFile, size);
			}
		}
	}

	public class UploadWork
	{
		private string clientFile;
		private string serverFile;
		private int size;

		public UploadWork(string clientFile, string serverFile, int size)
		{
			this.clientFile = clientFile;
			this.serverFile = serverFile;
			this.size = size;
		}

		public void DoWork()
		{
			HandlerTests.UploadFile(clientFile, serverFile, size);
		}
	}
}
