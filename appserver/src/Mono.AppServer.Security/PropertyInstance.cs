//
// Mono.AppServer.Security.PropertyInstance
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;

namespace Mono.AppServer.Security
{
	/// <summary>
	/// Summary description for PropertyInstance.
	/// </summary>
	/// 

	[Serializable()]
	public class PropertyInstance
	{
		string _Name;
		object _Value;

		public PropertyInstance()
		{
		}

		public PropertyInstance(string n,object v)
		{
			_Name=n;
			_Value=v;
		}

		public string Name
		{
			get
			{
				return _Name;
			}
			set
			{
				_Name=value;
			}
		}

		public object Value
		{
			get
			{
				return _Value;
			}
			set
			{
				_Value=value;
			}
		}
	}
}
