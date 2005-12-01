//
//  HeaderStruct.cs
//
//  Authors
//    - Kangaroo, Geoff Norton
//    - C.J. Collier, Collier Technologies, <cjcollier@colliertech.org>
//    - Urs C. Muff, Quark Inc., <umuff@quark.com>
//    - Adham Findlay
//
//  Copyright (c) 2004 Quark Inc. and Collier Technologies.  All rights reserved.
//
//	$Header: /home/miguel/third-conversion/public/cocoa-sharp/generator/header-gen/HeaderStruct.cs,v 1.3 2004/09/18 17:30:17 urs Exp $
//

using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

namespace CocoaSharp {

	public class HeaderStruct : Element {
		StructItem[] mItems;
		Struct mStruct;

		public HeaderStruct(string _name, string _struct, string _framework) : base(_struct,_name,_framework)
		{
			mStruct = (Struct)Type.RegisterType(this.Name, this.NameSpace, typeof(Struct));

			ArrayList items = new ArrayList();
			foreach (string line in _struct.Split(';')) {
				string l = line.Trim();
				if (l == string.Empty)
					continue;
				string[] lineSplit = l.Split(' ','\t');
				string type = lineSplit[0];
				for (int i = 1; i < lineSplit.Length-1; ++i)
					type += " " + lineSplit[i];
				string name = lineSplit[lineSplit.Length-1];
				int bitField = name.IndexOf(":");
				int array = name.IndexOf("[");
				bool isPointer = name.StartsWith("*");
				items.Add(new StructItem(null,name));
			}
			mItems = (StructItem[])items.ToArray(typeof(StructItem));
		}

		public Struct StructType { get { return this.mStruct; } }

		public override OutputElement ToOutput() {
			mStruct.Initialize(mItems);
			return mStruct;
		}
	}
}

//	$Log: HeaderStruct.cs,v $
//	Revision 1.3  2004/09/18 17:30:17  urs
//	Move CS output gen into gen-out
//
//	Revision 1.2  2004/09/11 00:41:22  urs
//	Move Output to gen-out
//	
//	Revision 1.1  2004/09/09 13:18:53  urs
//	Check header generator back in.
//	
//	Revision 1.7  2004/06/23 17:14:20  gnorton
//	Custom addins supported on a per file basis.
//	
//	Revision 1.6  2004/06/23 15:29:29  urs
//	Major refactor, allow inheriting parent constructors
//	
//	Revision 1.5  2004/06/22 15:13:18  urs
//	New fixing
//	
//	Revision 1.4  2004/06/22 13:38:59  urs
//	More cleanup and refactoring start
//	Make output actually compile (diverse fixes)
//	
//	Revision 1.3  2004/06/22 12:04:12  urs
//	Cleanup, Headers, -out:[CS|OC], VS proj
//	
//
