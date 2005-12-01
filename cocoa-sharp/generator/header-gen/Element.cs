//
//  Element.cs
//
//  Authors
//    - Kangaroo, Geoff Norton
//    - C.J. Collier, Collier Technologies, <cjcollier@colliertech.org>
//    - Urs C. Muff, Quark Inc., <umuff@quark.com>
//    - Adham Findlay
//
//  Copyright (c) 2004 Quark Inc. and Collier Technologies.  All rights reserved.
//
//	$Header: /home/miguel/third-conversion/public/cocoa-sharp/generator/header-gen/Element.cs,v 1.3 2004/09/18 17:30:17 urs Exp $
//

using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

namespace CocoaSharp {

	public abstract class Element {
		protected string mOriginal;
		private string mName;
		private string mFramework;

		public Element(string _original, string _name, string _framework) {
			mOriginal = _original;
			mName = _name;
			mFramework = _framework;
		}

		public string Name {
			get { return mName; }
			set { mName = value; }
		}

		public string Framework {
			get { return mFramework; }
		}

		public string NameSpace {
			get { return "Apple." + this.Framework; }
		}

		public string FullName {
			get { return Type.FullName(this.Name, this.NameSpace); }
		}

		public abstract OutputElement ToOutput();

		public static ICollection ToOutput(ICollection collection) {
			IList ret = new ArrayList();
			foreach (Element s in collection)
				ret.Add(s.ToOutput());
			return ret;
		}
	}

	public abstract class ElementWithMethods : Element {
		private IDictionary mMethods;
		private static Regex mMethodRegex = new Regex(@"\s*([+-])\s*(?:\(([^\)]+)\))?(.+)");

		public ElementWithMethods(string _name, string _framework) : base(string.Empty,_name,_framework) {
			mMethods = new Hashtable();
		}

		public IDictionary Methods {
			get { return mMethods; }
		}

		public void AddMethods(string methods) {
			string[] splitMethods = methods.Split('\n');
			foreach(string method in splitMethods) 
				if(mMethodRegex.IsMatch(method) && mMethods[method] == null)
					mMethods.Add(method, new HeaderMethod(method, this.Name));
		}

	}
}

//	$Log: Element.cs,v $
//	Revision 1.3  2004/09/18 17:30:17  urs
//	Move CS output gen into gen-out
//
//	Revision 1.2  2004/09/11 00:41:22  urs
//	Move Output to gen-out
//	
//	Revision 1.1  2004/09/09 13:18:53  urs
//	Check header generator back in.
//	
//	Revision 1.4  2004/06/25 02:49:14  gnorton
//	Sample 2 now runs.
//	
//	Revision 1.3  2004/06/23 17:14:20  gnorton
//	Custom addins supported on a per file basis.
//	
//	Revision 1.2  2004/06/23 15:29:29  urs
//	Major refactor, allow inheriting parent constructors
//	
//	Revision 1.1  2004/06/22 13:38:59  urs
//	More cleanup and refactoring start
//	Make output actually compile (diverse fixes)
//	
//	Revision 1.3  2004/06/22 12:04:12  urs
//	Cleanup, Headers, -out:[CS|OC], VS proj
//	
//
