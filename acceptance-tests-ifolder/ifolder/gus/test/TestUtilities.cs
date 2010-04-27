using System;
using System.IO;
using System.Threading;
using System.Security.Cryptography;

namespace Gus
{
	/// <summary>
	/// Test Utilities
	/// </summary>
	public class TestUtilities
	{
		static readonly int BUFFER_SIZE = 4096;
		
		static byte[] data = new byte[BUFFER_SIZE];

		static TestUtilities()
		{
			for(int i = 0; i < BUFFER_SIZE; i++)
			{
				data[i] = (byte) i;
			}
		}

		public static void CreateFile(string path, int size)
		{
			// create file
			FileStream file = File.Create(path);

			int length = 0;

			while (length < size)
			{
				int count = Math.Min((size - length), BUFFER_SIZE); 
				
				file.Write(data, 0, count);

				length += count;
			}

			file.Close();
		}

		public static bool FilesEqual(string path1, string path2)
		{
			bool result = false;

			MD5 md5 = new MD5CryptoServiceProvider();

			FileStream stream1 = File.OpenRead(path1);
			FileStream stream2 = File.OpenRead(path2);
			
			byte[] hash1 = md5.ComputeHash(stream1);
			byte[] hash2 = md5.ComputeHash(stream2);

			stream1.Close();
			stream2.Close();

			if (hash1.Length == hash2.Length)
			{
				result = true;

				for(int i=0; i < hash1.Length; i++)
				{
					if (hash1[i] != hash2[i])
					{
						result = false;
						break;
					}
				}
			}

			return result;
		}
	}
}
