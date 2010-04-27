//
// Mono.AppServer.Security.SecurityManager
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;
using System.IO;
using System.Threading;
using System.Security.Principal;
using System.Xml;
using System.Xml.Serialization;

namespace Mono.AppServer.Security
{
	/// <summary>
	/// Summary description for SecurityManager.
	/// </summary>
	public class SecurityManager : MarshalByRefObject
	{
		private ISecurityStorage _SecurityStorage;

		public User GetUser(string Username)
		{
			return _SecurityStorage.GetUser(Username);
		}

		public void SaveUser(User user)
		{
			_SecurityStorage.SaveUser(user);
		}

		public SecurityManager(ISecurityStorage SecurityStorage)
		{
			_SecurityStorage=SecurityStorage;
		}

		public User CurrentUser
		{
			get
			{
				string Username=Thread.CurrentPrincipal.Identity.Name;
				return GetUser(Username);
			}
		}

		public void DeleteUser(User user)
		{
			_SecurityStorage.DeleteUser(user);
		}

		public UserCollection GetUserList()
		{
			return _SecurityStorage.GetUserList();
		}

		public void SetPrincipal(string Username)
		{
			SetPrincipal(GetUser(Username));
		}

		public void SetPrincipal(User user)
		{
			GenericIdentity identity=new GenericIdentity(user.UserName);
			string[] Roles=new String[user.Roles.Count];
			user.Roles.CopyTo(Roles,0);
			GenericPrincipal principal=new GenericPrincipal(identity, Roles);
			Thread.CurrentPrincipal=principal;
		}

		public virtual bool Authenticate(string Username,string Password)
		{
			try
			{
				User u=GetUser(Username);
				if (u.IsPassword(Password) && u.Active)
				{
					SetPrincipal(u);
					return true;
				}
				else
					return false;
			}
			catch
			{
				return false;
			}
		}



		public virtual void Signoff()
		{
		}


	}
}
