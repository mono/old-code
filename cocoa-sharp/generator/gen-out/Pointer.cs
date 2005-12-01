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
	public class Pointer : Type {
		public Pointer(string name, Type reference) : base(name,null,"IntPtr/*FIXME*/",typeof(IntPtr),OCType.pointer) {
			System.Diagnostics.Debug.Assert(reference.OCType != OCType.id || name.Split('*').Length > 2 || reference.Name == "id");
			this.reference = reference;
		}

		// -- Public Properties --
		public Type Reference { get { return reference; } }

		public override string TypeStr {
			get {
				return "^" + this.Reference.TypeStr;
			}
		}

		// -- Members --
		private Type reference;
	}
}

//
// $Log: Pointer.cs,v $
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
