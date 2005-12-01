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

	internal class MachOMethod {
		private string name, typesStr;
		private MachOType[] types;

		internal MachOMethod(objc_method method, MachOFile file) {
			Utils.MakeBigEndian(ref method.name);
			Utils.MakeBigEndian(ref method.types);
			name = file.GetString(method.name);
			typesStr = file.GetString(method.types);
			MachOFile.DebugOut(1,"\tmethod: {0} types={1}", name, typesStr);
			types = MachOType.ParseTypes(file.Namespace, typesStr);
		}

		internal MachOMethod(string nameSpace, string name,string types) {
			this.name = name;
			this.typesStr = types;
			this.types = MachOType.ParseTypes(nameSpace, types);
			MachOFile.DebugOut(1,"\tmethod: {0} types={1}", name, types);
		}

		internal ParameterInfo[] ToParameters(string nameSpace) {
			ArrayList ret = new ArrayList();
			for (int i = 3; i < types.Length; ++i)
				ret.Add(new ParameterInfo("p" + (i-3),types[i].ToTypeUsage(nameSpace)));
			return (ParameterInfo[])ret.ToArray(typeof(ParameterInfo));
		}

		internal Method ToMethod(string nameSpace, bool isClassMethod) {
			string _csName = name.Trim(':').Replace(":","_");
			string selector = name;
			return new Method(_csName,selector,typesStr,this.types[0].ToTypeUsage(nameSpace),ToParameters(nameSpace));
		}

		static internal ICollection ToMethods(string nameSpace, bool isClassMethod, ICollection methods) {
			ArrayList ret = new ArrayList();
			foreach (MachOMethod method in methods)
				ret.Add(method.ToMethod(nameSpace, isClassMethod));
			return ret;
		}

		unsafe public static ArrayList ProcessMethods(uint methodLists,MachOFile file) {
			ArrayList ret = new ArrayList();
			if (methodLists == 0) 
				return ret;
			byte* methodsPtr = file.GetPtr(methodLists);
			if (methodsPtr == null)
				return ret;
			objc_method_list ocmethodlist = *(objc_method_list *)methodsPtr;
			byte* methodPtr = methodsPtr+Marshal.SizeOf(ocmethodlist);
			Utils.MakeBigEndian(ref ocmethodlist.method_count);
			for (int i = 0; i < ocmethodlist.method_count; ++i, methodPtr += Marshal.SizeOf(typeof(objc_method))) {
				objc_method method = *(objc_method*)methodPtr;
				ret.Add(new MachOMethod(method,file));
			}
			return ret;
		}
	}
}

//
// $Log: MachOMethod.cs,v $
// Revision 1.5  2004/09/20 20:18:23  gnorton
// More refactoring; Foundation almost gens properly now.
//
// Revision 1.4  2004/09/11 00:41:22  urs
// Move Output to gen-out
//
// Revision 1.3  2004/09/09 03:32:22  urs
// Convert methods from mach-o to out format
//
// Revision 1.2  2004/09/09 02:33:04  urs
// Fix build
//
