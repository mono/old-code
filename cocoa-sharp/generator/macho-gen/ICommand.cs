//
//  Authors
//    - Kangaroo, Geoff Norton
//    - Urs C. Muff, Quark Inc., <umuff@quark.com>
//
//  Copyright (c) 2004 Quark Inc.  All rights reserved.
//
// $Id: ICommand.cs,v 1.2 2004/09/09 02:33:04 urs Exp $
//

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CocoaSharp {
	internal interface ICommand {
		void ProcessCommand ();
	}
}

//
// $Log: ICommand.cs,v $
// Revision 1.2  2004/09/09 02:33:04  urs
// Fix build
//
