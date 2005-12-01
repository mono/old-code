//
//  Authors
//    - Kangaroo, Geoff Norton
//    - Urs C. Muff, Quark Inc., <umuff@quark.com>
//
//  Copyright (c) 2004 Quark Inc.  All rights reserved.
//
// $Id$
//

using System;

namespace CocoaSharp {
	public class BitField : Type {
		public BitField(int bits) : base("int:" + bits,null,"int",typeof(int),OCType.bit_field) {
			this.bits = bits;
		}

		// -- Public Properties --
		public int Bits { get { return bits; } }

		public override string TypeStr {
			get {
				return "b" + this.Bits;
			}
		}

		// -- Members --
		private int bits;
	}
}

//
// $Log: BitField.cs,v $
// Revision 1.3  2004/09/20 20:18:23  gnorton
// More refactoring; Foundation almost gens properly now.
//
// Revision 1.2  2004/09/09 03:32:22  urs
// Convert methods from mach-o to out format
//
// Revision 1.1  2004/09/09 01:16:03  urs
// 1st draft of out module of 2nd generation generator
//
//
