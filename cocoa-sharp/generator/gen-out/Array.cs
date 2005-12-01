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
	public class Array : Type {
		public Array(Type elementType,int dim) 
		      : base(elementType.ApiType + "[]",null,elementType.ApiType + "[]",typeof(IntPtr),OCType.array) {
			this.elementType = elementType;
			this.dim = dim;
		}

		// -- Public Properties --
		public Type ElementType { get { return elementType; } }
		public int Dim { get { return dim; } }

		public override string TypeStr {
			get {
				return "[" + this.Dim + this.ElementType.TypeStr + "]";
			}
		}

		// -- Members --
		private Type elementType;
		private int dim;
	}
}

//
// $Log: Array.cs,v $
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
