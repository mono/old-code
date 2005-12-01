//
//  Authors
//    - Kangaroo, Geoff Norton
//    - Urs C. Muff, Quark Inc., <umuff@quark.com>
//
//  Copyright (c) 2004 Quark Inc.  All rights reserved.
//
// $Id: LoadCommand.cs,v 1.3 2004/09/11 00:41:22 urs Exp $
//

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CocoaSharp {

	internal class LoadCommand : ICommand {

		private MachOFile mfile;
		private load_command lcmd;

		internal LoadCommand (MachOFile mfile, load_command lcmd) {
			this.mfile = mfile;
			this.lcmd = lcmd;
		}

		public void ProcessCommand () {
			unsafe {
				mfile.Pointer += (int)(lcmd.cmdsize - Marshal.SizeOf (lcmd));
			}
		}
	}
}

//
// $Log: LoadCommand.cs,v $
// Revision 1.3  2004/09/11 00:41:22  urs
// Move Output to gen-out
//
// Revision 1.2  2004/09/09 02:33:04  urs
// Fix build
//
