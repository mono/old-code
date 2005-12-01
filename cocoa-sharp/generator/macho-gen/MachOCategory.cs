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
using System.Collections;
using System.Runtime.InteropServices;

namespace CocoaSharp {

	internal class MachOCategory {
		
		private objc_category occategory;
		private string class_name, name;
		private ArrayList instanceMethods, classMethods;

		unsafe internal MachOCategory (byte *ptr, MachOFile file) {
			occategory = *(objc_category *)ptr;
			Utils.MakeBigEndian(ref occategory.category_name);
			Utils.MakeBigEndian(ref occategory.class_name);
			Utils.MakeBigEndian(ref occategory.instance_methods);
			Utils.MakeBigEndian(ref occategory.class_methods);
			Utils.MakeBigEndian(ref occategory.protocols);
			name = file.GetString(occategory.category_name);
			class_name = file.GetString(occategory.class_name);
			MachOFile.DebugOut(1,"Category: {0} class_name : {1}",name,class_name);
			instanceMethods = MachOMethod.ProcessMethods(occategory.instance_methods,file);
			classMethods = MachOMethod.ProcessMethods(occategory.class_methods,file);
		}

		internal Category ToCategory(string nameSpace) {
			return new Category(name, nameSpace, Class.GetClass(class_name), 
				MachOMethod.ToMethods(nameSpace, false, instanceMethods), 
				MachOMethod.ToMethods(nameSpace, true, classMethods));
		}
	}

	internal struct objc_category {
		internal uint category_name;
		internal uint class_name;
		internal uint instance_methods;
		internal uint class_methods;
		internal uint protocols;
	}
}

//
// $Log: MachOCategory.cs,v $
// Revision 1.4  2004/09/11 00:41:22  urs
// Move Output to gen-out
//
// Revision 1.3  2004/09/09 03:32:22  urs
// Convert methods from mach-o to out format
//
// Revision 1.2  2004/09/09 02:33:04  urs
// Fix build
//
