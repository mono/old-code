//
// Mono.AppServer.Security.EncryptedFileStorage
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;
using System.IO;
using System.Xml.Serialization;
using System.Security.Cryptography;

namespace Mono.AppServer.Security
{
	/// <summary>
	/// Summary description for EncryptedFileStorage.
	/// </summary>
	public class EncryptedFileStorage : MarshalByRefObject, ISecurityStorage
	{
		string path;
		byte[] Key=new byte[8] { 5,100,5,2,4,24,34,55 };
		byte[] IV=new byte[8] { 5,100,5,2,4,24,34,55};

		public EncryptedFileStorage(string path, byte[] key, byte[] iv)
		{
			this.path=path;
			this.Key=key;
			this.IV=iv;
		}

		protected string GetFilename(string Username)
		{
			return path+Username+".user";
		}

		public void DeleteUser(User user)
		{
			File.Delete(GetFilename(user.UserName));
		}

		public void SaveUser(User user)
		{
			//Create new file
			FileStream fs=new FileStream(GetFilename(user.UserName), FileMode.Create);
			//Fire-up crypto to secure file
			DES cryptoalgorithm = DES.Create();
			CryptoStream encStream = new CryptoStream(fs, cryptoalgorithm.CreateEncryptor(Key, IV), CryptoStreamMode.Write);
			//Serialize user into encrypted file
			XmlSerializer serializer=new XmlSerializer(user.GetType());
			serializer.Serialize(encStream, user);
			encStream.Close();
			fs.Close();
		}


		public User GetUser(string Username)
		{
			//Open existing file
			FileStream fs=new FileStream(GetFilename(Username), FileMode.Open);
			//Fire-up crypto to decrypt file
			DES cryptoalgorithm = DES.Create(); 
			CryptoStream encStream = new CryptoStream(fs, cryptoalgorithm.CreateDecryptor(Key, IV), CryptoStreamMode.Read);
			//Deserialize user from encrypted file
			XmlSerializer serializer=new XmlSerializer(typeof(User));
			User u=(User) serializer.Deserialize(encStream);
			encStream.Close();
			fs.Close();
			return u;
		}

		public UserCollection GetUserList()
		{
			DirectoryInfo dir=new DirectoryInfo(path);
			UserCollection userlist=new UserCollection();
			foreach (FileInfo f in dir.GetFiles("*.user"))
			{
				User u=GetUser(f.Name.Substring(0,f.Name.IndexOf(f.Extension)));
				userlist.Add(u);
			}
			return userlist;
		}

	}
}
