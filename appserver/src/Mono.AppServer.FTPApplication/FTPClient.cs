//
// Mono.AppServer.FTPClient
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;

namespace Mono.AppServer
{
	[Serializable]
	public class FTPClient
	{
		public int _ClientID;
		public string _IPAddr;
		public string _Username;

		public int ClientID
		{
			get
			{
				return _ClientID;
			}
			set
			{
				_ClientID=value;
			}
		}

		public string IPAddr
		{
			get
			{
				return _IPAddr;
			}
			set
			{
				_IPAddr=value;
			}
		}

		public string Username
		{
			get
			{
				return _Username;
			}
			set
			{
				_Username=value;
			}
		}
	}
}
