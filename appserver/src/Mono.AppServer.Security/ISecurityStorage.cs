//
// Mono.AppServer.Security.ISecurityStorage
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;

namespace Mono.AppServer.Security
{
	public interface ISecurityStorage
	{
		User GetUser(string Username);
		void SaveUser(User user);
		UserCollection GetUserList();
		void DeleteUser(User user);
	}
}
