// ConnectionString.cs
// Author:
//     Daniel Morgan <monodanmorg@yahoo.com>
//
// Copyright (C) Daniel Morgan, 2004
//
// SetConnectionString() from Mono's SqlConnection by Tim Coleman
// Copyright (C) Tim Coleman, 2002, 2003

using Mono.Data;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Text;

namespace Mono.Data.SqlSharp.GtkSharp
{
	public class ConnectionString
	{
		private string conString = "";

		private NameValueCollection parms;

		public ConnectionString() 
		{
			Clear ();
		}

		public ConnectionString(string connectionString) 
		{
			SetConnectionString (connectionString);
		}

		public string Connection 
		{
			get 
			{
				return conString;
			}		

			set 
			{
				SetConnectionString (value);
			}
		}

		public NameValueCollection Parameters
		{
			get 
			{
				return parms;
			}
		}

		public void Add (string name, string value) 
		{
			parms.Add (name, value);
		}

		public void Clear ()
		{
			conString = "";
			parms = new NameValueCollection ();
		}

		private string TrimSemicolon (string s) 
		{
			if (s != null) 
				if (s.Length > 0) 
					if (s[s.Length - 1].Equals(';')) 
						s = s.Substring (0, s.Length - 1);
			return s;
		}

		public string GetOtherOptions () 
		{
			// Other Options
			StringBuilder cString = new StringBuilder ();

			foreach (String sName in parms.AllKeys)  
			{
				switch (sName) 
				{
					case "FACTORY":
					case "SERVER":
					case "DATA SOURCE":
					case "NETWORK ADDRESS":
					case "ADDRESS":
					case "ADDR":
					case "DATABASE":
					case "INITIAL CATALOG":
					case "UID":
					case "USER":
					case "USER ID":
					case "PASSWORD":
					case "PWD":
						break;
					default:
						string sValue = parms[sName];
						string parm = sName + "=" + sValue + ";";
						cString.Append (parm);
						break;
				}
			}

			return TrimSemicolon (cString.ToString ());
		}

		// get the connection string without the Factory parameter
		public string GetConnectionString () 
		{
			StringBuilder connectionString = new StringBuilder ();
			foreach ( String sName in parms.AllKeys )  
			{
				if (!sName.Equals("FACTORY")) 
				{
					string sValue = parms[sName];
					string parm = sName + "=" + sValue + ";";
					connectionString.Append (parm);
				}
			}

			return TrimSemicolon (connectionString.ToString ());
		}
        
		void SetConnectionString (string connectionString)
		{
			NameValueCollection parameters = new NameValueCollection ();

			if (( connectionString == null)||( connectionString.Length == 0)) 
				return;
			connectionString += ";";

			bool inQuote = false;
			bool inDQuote = false;
			bool inName = true;

			string name = String.Empty;
			string value = String.Empty;
			StringBuilder sb = new StringBuilder ();

			for (int i = 0; i < connectionString.Length; i += 1) 
			{
				char c = connectionString [i];
				char peek;
				if (i == connectionString.Length - 1)
					peek = '\0';
				else
					peek = connectionString [i + 1];

				switch (c) 
				{
					case '\'':
						if (inDQuote)
							sb.Append (c);
						else if (peek.Equals (c)) 
						{
							sb.Append (c);
							i += 1;
						}
						else
							inQuote = !inQuote;
						break;
					case '"':
						if (inQuote)
							sb.Append (c);
						else if (peek.Equals (c)) 
						{
							sb.Append (c);
							i += 1;
						}
						else
							inDQuote = !inDQuote;
						break;
					case ';':
						if (inDQuote || inQuote)
							sb.Append (c);
						else 
						{
							if (name != String.Empty && name != null) 
							{
								value = sb.ToString ();
								parameters [name.ToUpper ().Trim ()] = value.Trim ();
							}
							inName = true;
							name = String.Empty;
							value = String.Empty;
							sb = new StringBuilder ();
						}
						break;
					case '=':
						if (inDQuote || inQuote || !inName)
							sb.Append (c);
						else if (peek.Equals (c)) 
						{
							sb.Append (c);
							i += 1;
						}
						else 
						{
							name = sb.ToString ();
							sb = new StringBuilder ();
							inName = false;
						}
						break;
					case ' ':
						if (inQuote || inDQuote)
							sb.Append (c);
						else if (sb.Length > 0 && !peek.Equals (';'))
							sb.Append (c);
						break;
					default:
						sb.Append (c);
						break;
				}
			}

			this.conString = TrimSemicolon (connectionString);
			parms = parameters;
		}
	}
}

