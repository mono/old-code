//
// Mono.AppServer.Security.FormsSecurityManager
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;
using System.Web;
using System.Web.Security;

namespace Mono.AppServer.Security
{
	public class FormsSecurityManager : SecurityManager
	{

		public FormsSecurityManager(ISecurityStorage SecurityStorage) : base(SecurityStorage)
		{
		}

		public override bool Authenticate(string Username,string Password)
		{
			if (base.Authenticate(Username,Password))
			{
				// This is the long version of 	FormsAuthentication.SetAuthCookie();
				// that allows us to push the user data into the cookie & change timeout

				// Set the Timeout using the session length
				int Timeout=HttpContext.Current.Session.Timeout-1;

				HttpContext currentContext = HttpContext.Current;
				string formsCookieStr = string.Empty;
				FormsAuthenticationTicket ticket = new    FormsAuthenticationTicket(
					1,                                  // version
					Username,                           // user name
					DateTime.Now,                       // issue time
					DateTime.Now.AddMinutes(Timeout),   // expires
					false,                              // persistent
					null								// user data
					);

				// get the encrypted representation suitable for placing in a HTTP cookie
				formsCookieStr = FormsAuthentication.Encrypt(ticket);
				HttpCookie FormsCookie = new HttpCookie(FormsAuthentication.FormsCookieName, formsCookieStr);
				currentContext.Response.Cookies.Add(FormsCookie);
				return true;
			}
			return false;
		}

		public override void Signoff()
		{
			base.Signoff();
			FormsAuthentication.SignOut();
		}
	}
}
