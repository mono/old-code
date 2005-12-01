//
//  HeaderEnum.cs
//
//  Authors
//    - Kangaroo, Geoff Norton
//    - C.J. Collier, Collier Technologies, <cjcollier@colliertech.org>
//    - Urs C. Muff, Quark Inc., <umuff@quark.com>
//    - Adham Findlay
//
//  Copyright (c) 2004 Quark Inc. and Collier Technologies.  All rights reserved.
//
//	$Header: /home/miguel/third-conversion/public/cocoa-sharp/generator/header-gen/HeaderEnum.cs,v 1.3 2004/09/18 17:30:17 urs Exp $
//

namespace CocoaSharp {
	using System;
	using System.Collections;

	public class HeaderEnum : Element {
		EnumItem[] mItems;
		Enum mEnum;

		public HeaderEnum(string _name, string _enum, string _framework) : base(_enum,_name,_framework)
		{
			mEnum = (Enum)Type.RegisterType(this.Name, this.NameSpace, typeof(Enum));

			ArrayList items = new ArrayList();
			foreach (string line in _enum.Split(',')) {
				string [] valueSep = line.Split('=');
				string name = valueSep[0].Trim();
				if (valueSep.Length > 1)
					items.Add(new EnumItem(name, valueSep[1].Trim()));
				else
					items.Add(new EnumItem(name, string.Empty));
			}
			mItems = (EnumItem[])items.ToArray(typeof(EnumItem));
		}

		public Enum EnumType { get { return this.mEnum; } }
		public override OutputElement ToOutput() {
			mEnum.Initialize(mItems);
			return mEnum;
		}
	}
}

//	$Log: HeaderEnum.cs,v $
//	Revision 1.3  2004/09/18 17:30:17  urs
//	Move CS output gen into gen-out
//
//	Revision 1.2  2004/09/11 00:41:22  urs
//	Move Output to gen-out
//	
//	Revision 1.1  2004/09/09 13:18:53  urs
//	Check header generator back in.
//	
//	Revision 1.7  2004/06/24 18:56:53  gnorton
//	AppKit compiles
//	Foundation compiles
//	Output setMethod() for protocols not just the property so Interfaces are met.
//	Ignore static protocol methods (.NET doesn't support static in interfaces).
//	Resolve compiler errors.
//	
//	Revision 1.6  2004/06/24 06:29:36  gnorton
//	Make foundation compile.
//	
//	Revision 1.5  2004/06/23 17:14:20  gnorton
//	Custom addins supported on a per file basis.
//	
//	Revision 1.4  2004/06/23 15:29:29  urs
//	Major refactor, allow inheriting parent constructors
//	
//	Revision 1.3  2004/06/22 13:38:59  urs
//	More cleanup and refactoring start
//	Make output actually compile (diverse fixes)
//	
//	Revision 1.2  2004/06/22 12:04:12  urs
//	Cleanup, Headers, -out:[CS|OC], VS proj
//	
//
