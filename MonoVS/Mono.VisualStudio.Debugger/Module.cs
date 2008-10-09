using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Mono.VisualStudio.Debugger
{
	public class Module : IDebugModule2, IDebugModule3
	{
		#region IDebugModule2 Members

		public int GetInfo (uint dwFields, MODULE_INFO[] pinfo)
		{
			throw new NotImplementedException ();
		}

		public int ReloadSymbols_Deprecated (string pszUrlToSymbols, out string pbstrDebugMessage)
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region IDebugModule3 Members


		public int GetSymbolInfo (uint dwFields, MODULE_SYMBOL_SEARCH_INFO[] pinfo)
		{
			throw new NotImplementedException ();
		}

		public int IsUserCode (out int pfUser)
		{
			throw new NotImplementedException ();
		}

		public int LoadSymbols ()
		{
			throw new NotImplementedException ();
		}

		public int SetJustMyCodeState (int fIsUserCode)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
