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

	internal class MachOClass {
		
		private objc_class occlass;
		private string superClass, name;
		private bool isClass;
		private ArrayList ivars = new ArrayList();
		private ArrayList methods, classMethods;
		private IDictionary protocols = new Hashtable();
		private Class mClass;

		unsafe internal MachOClass (byte *ptr, MachOFile file) {
			occlass = *(objc_class *)ptr;
			Utils.MakeBigEndian(ref occlass.isa);
			Utils.MakeBigEndian(ref occlass.super_class);
			Utils.MakeBigEndian(ref occlass.name);
			Utils.MakeBigEndian(ref occlass.version);
			Utils.MakeBigEndian(ref occlass.info);
			Utils.MakeBigEndian(ref occlass.instance_size);
			Utils.MakeBigEndian(ref occlass.ivars);
			Utils.MakeBigEndian(ref occlass.methodLists);
			Utils.MakeBigEndian(ref occlass.protocols);
			superClass = file.GetString(occlass.super_class);
			name = file.GetString(occlass.name);
			mClass = (Class)Type.RegisterType(this.name, file.Namespace, typeof(Class));
			isClass = (occlass.info & 1) != 0;
			MachOFile.DebugOut(1,"Class: {0} : {1} iSize={2} info={3,8:x} isClass={4}",name,superClass,occlass.instance_size,occlass.info,isClass);

			// Process ivars
			if (isClass && occlass.ivars != 0) {
				byte* ivarsPtr = file.GetPtr(occlass.ivars);
				objc_ivar_list ocivars = *(objc_ivar_list*)ivarsPtr;
				Utils.MakeBigEndian(ref ocivars.ivar_count);
				byte* ivarPtr = ivarsPtr + Marshal.SizeOf(typeof(objc_ivar_list));

				for (int i = 0; i < ocivars.ivar_count; ++i, ivarPtr += Marshal.SizeOf(typeof(objc_ivar))) {
					objc_ivar ivar = *(objc_ivar*)ivarPtr;
					ivars.Add(new MachOIvar(ivar,file));
				}
			}

			methods = MachOMethod.ProcessMethods(occlass.methodLists,file);

			if (isClass) {
				// Process meta class
				objc_class metaClass = *(objc_class *)file.GetPtr(occlass.isa);
				Utils.MakeBigEndian(ref metaClass.methodLists);
				classMethods = MachOMethod.ProcessMethods(metaClass.methodLists,file);
			}

			AddProtocolsFromArray(ProcessProtocolList(occlass.protocols,file));
		}

		public Class ToClass(string nameSpace) {
			mClass.Initialize(Class.GetClass(superClass), 
				MachOProtocol.ToProtocols(protocols.Values),
				MachOIvar.ToVariables(nameSpace, ivars),
				MachOMethod.ToMethods(nameSpace,false, methods),
				MachOMethod.ToMethods(nameSpace,true, classMethods));
			return mClass;
		}

		void AddProtocolsFromArray(IList protocols) {
			foreach (MachOProtocol p in protocols)
				AddProtocol(p);
		}

		void AddProtocol(MachOProtocol p) {
			if (protocols.Contains(p.Name))
				protocols[p.Name] = p;
		}

		unsafe static ArrayList ProcessProtocolList(uint protocolListAddr,MachOFile file) {
			ArrayList protocols = new ArrayList();
			if (protocolListAddr == 0)
				return protocols;

			byte *ptr = file.GetPtr(protocolListAddr);
			objc_protocol_list protocolList = *(objc_protocol_list*)ptr;
			Utils.MakeBigEndian(ref protocolList.count);
			uint *protocolPtrs = (uint*)(ptr + Marshal.SizeOf(typeof(objc_protocol_list)));
			for (int index = 0; index < protocolList.count; index++, protocolPtrs++) {
				Utils.MakeBigEndian(ref *protocolPtrs);
				ptr = file.GetPtr(*protocolPtrs);
				if (ptr != null)
					protocols.Add(ProcessProtocol(ptr,file));
			}

			return protocols;
		}

		static IDictionary mProtocolsByName = new Hashtable();
		unsafe static MachOProtocol ProcessProtocol(byte *ptr,MachOFile file) {
			objc_protocol protocolPtr = *(objc_protocol*)ptr;
			Utils.MakeBigEndian(ref protocolPtr.isa);
			Utils.MakeBigEndian(ref protocolPtr.protocol_name);
			Utils.MakeBigEndian(ref protocolPtr.protocol_list);
			Utils.MakeBigEndian(ref protocolPtr.instance_methods);
			Utils.MakeBigEndian(ref protocolPtr.class_methods);
			string name = file.GetString(protocolPtr.protocol_name);
			ArrayList protocols = ProcessProtocolList(protocolPtr.protocol_list,file);

			MachOProtocol protocol = (MachOProtocol)mProtocolsByName[name];
			if (protocol == null) {
				protocol = new MachOProtocol(name, file.Namespace);
				mProtocolsByName[name] = protocol;
			}

			protocol.AddProtocolsFromArray(protocols);
			if (protocol.instanceMethods.Count == 0)
				protocol.instanceMethods = ProcessProtocolMethods(protocolPtr.instance_methods,file);

			if (protocol.classMethods.Count == 0)
				protocol.classMethods = ProcessProtocolMethods(protocolPtr.class_methods,file);

			// TODO (2003-12-09): Maybe we should add any missing methods.  But then we'd lose the original order.
			return protocol;
		}

		unsafe static ArrayList ProcessProtocolMethods(uint methodsAddr, MachOFile file) {
			ArrayList methods = new ArrayList();
			if (methodsAddr == 0)
				return methods;
			byte *ptr = file.GetPtr(methodsAddr);
			objc_protocol_method_list methodsPtr = *(objc_protocol_method_list*)ptr;
			ptr += Marshal.SizeOf(typeof(objc_protocol_method_list));

			Utils.MakeBigEndian(ref methodsPtr.method_count);
			for (int index = 0; index < methodsPtr.method_count; index++, ptr += Marshal.SizeOf(typeof(objc_protocol_method))) {
				objc_protocol_method methodPtr = *(objc_protocol_method*)ptr;
				Utils.MakeBigEndian(ref methodPtr.name);
				Utils.MakeBigEndian(ref methodPtr.types);
				MachOMethod method = new MachOMethod(file.Namespace, 
					file.GetString(methodPtr.name),
					file.GetString(methodPtr.types)
				);
				methods.Insert(0,method);
			}

			return methods;
		}
	}

	internal struct objc_class {
		internal uint isa;
		internal uint super_class;
		internal uint name;
		internal uint version;
		internal uint info;
		internal uint instance_size;
		internal uint ivars;
		internal uint methodLists;
		internal uint cache;
		internal uint protocols;
	}

	internal struct objc_method_list {
		internal uint obsolete;
		internal uint method_count;
	}

	internal struct objc_method {
		internal uint name;
		internal uint types;
		internal uint imp;
	}
}

//
// $Log: MachOClass.cs,v $
// Revision 1.6  2004/09/21 04:28:54  urs
// Shut up generator
// Add namespace to generator.xml
// Search for framework
// Fix path issues
// Fix static methods
//
// Revision 1.5  2004/09/20 16:42:52  gnorton
// More generator refactoring.  Start using the MachOGen for our classes.
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
