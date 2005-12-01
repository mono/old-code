//
//  Authors
//    - Kangaroo, Geoff Norton
//    - Urs C. Muff, Quark Inc., <umuff@quark.com>
//
//  Copyright (c) 2004 Quark Inc.  All rights reserved.
//
// $Id: Ivar.cs,v 1.3 2004/09/09 03:32:22 urs Exp $
//

using System;
using System.Collections;

namespace CocoaSharp {
	public class Ivar {
		public Ivar(string name, Type type, int offset) {
			this.name = name;
			this.type = type;
			this.offset = offset;
		}

		// -- Public Properties --
		public string Name { get { return name; } }
		public Type Type { get { return type; } }
		public int Offset { get { return offset; } }

		// -- Members --
		private string name;
		private Type type;
		private int offset;
	}
}

//
// $Log: Ivar.cs,v $
// Revision 1.3  2004/09/09 03:32:22  urs
// Convert methods from mach-o to out format
//
// Revision 1.2  2004/09/09 02:33:04  urs
// Fix build
//
// Revision 1.1  2004/09/09 01:16:03  urs
// 1st draft of out module of 2nd generation generator
//
//
