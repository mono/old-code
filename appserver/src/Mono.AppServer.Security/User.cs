//
// Mono.AppServer.Security.User
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Specialized;

namespace Mono.AppServer.Security
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	/// 

	[Serializable()]
	public class User
	{
		private string _UserName;
		private string _PasswordHash=null;
		private string _FullName;
		private string _EmailAddress;
		private bool _Active;
		private StringCollection _Roles;
		private PropertyCollection _Properties;


		public User()
		{
			_Roles=new StringCollection();
			_Properties=new PropertyCollection();
			_Active=true;
		}

		public StringCollection Roles
		{
			get
			{
				return _Roles;
			}
			set
			{
				_Roles=value;
			}
		}

		public PropertyCollection Properties
		{
			get
			{
				return _Properties;
			}
			set
			{
				_Properties=value;
			}
		}

		public string UserName
		{
			get 
			{
				return _UserName;
			}
			set
			{
				_UserName = value;
			}
		}

		public string FullName
		{
			get
			{
				return _FullName;
			}
			set
			{
				_FullName=value;
			}
		}

		public string EmailAddress
		{
			get
			{
				return _EmailAddress;
			}
			set
			{
				_EmailAddress=value;
			}
		}

		public bool Active
		{
			get
			{
				return _Active;
			}
			set
			{
				_Active=value;
			}
		}

		public string PasswordHash
		{
			get
			{
				return _PasswordHash;
			}
			set
			{
				_PasswordHash=value;
			}
		}

		protected string CalcPasswordHash(string s)
		{
			MemoryStream stream=new MemoryStream();
			StreamWriter writer=new StreamWriter(stream);
			writer.Write(s);
			stream.Seek(0,SeekOrigin.Begin);

			CryptoStream encStream = new CryptoStream(stream, HashAlgorithm.Create(), CryptoStreamMode.Read);
			BinaryReader reader=new BinaryReader(encStream);

			return Convert.ToBase64String( reader.ReadBytes(Convert.ToInt32(stream.Length)) );
		}

		public void SetPassword(string NewPassword)
		{
			_PasswordHash=CalcPasswordHash(NewPassword);
		}

		public bool IsPassword(string aPassword)
		{
			return (PasswordHash==CalcPasswordHash(aPassword));
		}

		public bool IsInRole(string Role)
		{
			return (Roles.IndexOf(Role)>=0);
		}

	}
}
