//
// DataSourceConnection.cs
//
// Author:
//     Daniel Morgan <monodanmorg@yahoo.com>
//
// (C)Copyright 2004 by Daniel Morgan
//
// To be included with Mono as a SQL query tool licensed under the GPL license.
//

namespace Mono.Data.SqlSharp.GtkSharp
{
	using System;
	using System.Data;
	using Mono.Data;

	public class DataSourceConnection
	{
		public string name;
		public string setting;
		public Provider provider;
		public ConnectionString cstring;
		public IDbConnection con;

		public DataSourceConnection (string name, string setting, Provider provider, 
									ConnectionString cstring, IDbConnection con) 
		{
			this.name = name;
			this.setting = setting;
			this.provider = provider;
			this.cstring = cstring;
			this.con = con;
		}

		public string Name 
		{
			get 
			{
				return name;
			}
		}

		public string Setting 
		{
			get 
			{
				return setting;
			}
		}

		public Provider Provider 
		{
			get 
			{
				return provider;
			}
		}

		public ConnectionString ConnectionString 
		{
			get 
			{
				return cstring;
			}
		}

		public IDbConnection Connection 
		{
			get 
			{
				return con;
			}
		}

		public void Dispose () 
		{
			provider = null;
			cstring = null;
			if (con != null) {
				if (con.State == ConnectionState.Open)
					con.Close ();
				con = null;
			}
		}
	}
}
