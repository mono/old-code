//
//  Authors
//    - Kangaroo, Geoff Norton
//    - Urs C. Muff, Quark Inc., <umuff@quark.com>
//
//  Copyright (c) 2004 Quark Inc.  All rights reserved.
//
// $Id: Category.cs,v 1.3 2004/09/11 00:41:22 urs Exp $
//

using System;
using System.Collections;
using System.IO;

namespace CocoaSharp {
	public class Category : OutputElement {
		public Category(string name, string nameSpace, Class class_,ICollection instanceMethods,ICollection classMethods)
			: base(name, nameSpace) {
			this.class_ = class_;
			this.instanceMethods = instanceMethods;
			this.classMethods = classMethods;
		}

		// -- Public Properties --
		public Class Class { get { return class_; } }
		public ICollection InstanceMethods { get { return instanceMethods; } }
		public ICollection ClassMethods { get { return classMethods; } }

		// -- Members --
		private Class class_;
		private ICollection instanceMethods;
		private ICollection classMethods;

		public override void WriteCS(TextWriter _cs, Configuration config) {
			throw new NotSupportedException();
		}
	}
}

//
// $Log: Category.cs,v $
// Revision 1.3  2004/09/11 00:41:22  urs
// Move Output to gen-out
//
// Revision 1.2  2004/09/09 03:32:22  urs
// Convert methods from mach-o to out format
//
// Revision 1.1  2004/09/09 01:16:03  urs
// 1st draft of out module of 2nd generation generator
//
//
