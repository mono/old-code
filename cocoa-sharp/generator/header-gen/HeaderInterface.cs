//
//  HeaderInterface.cs
//
//  Authors
//    - Kangaroo, Geoff Norton
//    - C.J. Collier, Collier Technologies, <cjcollier@colliertech.org>
//    - Urs C. Muff, Quark Inc., <umuff@quark.com>
//    - Adham Findlay
//
//  Copyright (c) 2004 Quark Inc. and Collier Technologies.  All rights reserved.
//
//	$Header: /home/miguel/third-conversion/public/cocoa-sharp/generator/header-gen/HeaderInterface.cs,v 1.3 2004/09/18 17:30:17 urs Exp $
//

using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace CocoaSharp {

	public class HeaderInterface : ElementWithMethods {
		private string mParent, mExtrasFor;
		private HeaderInterface mParentInterface;
		private string[] mProtos;
		private string[] mImports;
		private IDictionary mAllMethods;
		private Class mClass;

		public HeaderInterface(string _name, string _parent, string _protos, string _framework) : base(_name,_framework) {
			mClass = (Class)Type.RegisterType(this.Name, this.NameSpace, typeof(Class));

			mParent = _parent;
			_protos = _protos.Replace(" ", "");		
			mProtos = _protos.Split(new char[]{','});
			mAllMethods = new Hashtable();
		}

		public string Parent {
			get { return mParent; }
		}
		
		public void SetExtrasFor(HeaderInterface extrasFor) {
			mExtrasFor = extrasFor.Name;
		}

		public string ExtrasName {
			get { return mExtrasFor != null ? mExtrasFor : Name; }
		}

		public HeaderInterface ParentInterface {
			get { return mParentInterface; } set { mParentInterface = value; }
		}

		public string[] Protocols {
			get { return mProtos; }
		}

		public string[] Imports {
			get { return mImports; } set { mImports = value; }
		}

		public IDictionary AllMethods {
			get { return mAllMethods; }
		}

		public void AddAllMethods(ICollection methods,bool isProtocol) {
			foreach (HeaderMethod method in methods) {
				if (method.IsUnsupported)
					continue;

				string _methodSig = method.FullSelector;
				if(!mAllMethods.Contains(_methodSig)) 
					mAllMethods[_methodSig] = method;
				else if (!isProtocol)
					Console.WriteLine("\t\t\tWARNING: Method {0} is duplicated.", (string)_methodSig);
			}
		}

		public override OutputElement ToOutput() {
			mClass.Initialize(Class.GetClass(Parent),
				ToProtocols(Protocols),new Ivar[0],
				HeaderMethod.ToMethods(this,mAllMethods.Values,false),
				HeaderMethod.ToMethods(this,mAllMethods.Values,true));
			return mClass;
		}

		static ICollection ToProtocols(string []protocols) {
			IList ret = new ArrayList();
			foreach (string protocol in protocols) {
				if (protocol != "") {
					Protocol p = Protocol.GetProtocol(protocol);
					if (p != null)
						ret.Add(p);
					else
						Console.WriteLine("missing protocol: " + protocol);
				}
			}
			return ret;
		}
	}
}

//	$Log: HeaderInterface.cs,v $
//	Revision 1.3  2004/09/18 17:30:17  urs
//	Move CS output gen into gen-out
//
//	Revision 1.2  2004/09/11 00:41:22  urs
//	Move Output to gen-out
//	
//	Revision 1.1  2004/09/09 13:18:53  urs
//	Check header generator back in.
//	
//	Revision 1.25  2004/09/08 12:04:05  urs
//	Shut up Glue if env var "COCOASHARP_DEBUG_LEVEL" is not set to at least 1.
//	
//	Revision 1.24  2004/07/01 12:41:33  urs
//	- Better verbose support, individual verbose ignore per selector and per interface
//	- Minor improvements with monodoc
//	
//	Revision 1.23  2004/06/29 03:32:58  urs
//	Cleanup mapping usage: only one bug left
//	
//	Revision 1.22  2004/06/28 21:31:22  gnorton
//	Initial mapping support in the gen.
//	
//	Revision 1.21  2004/06/28 19:18:31  urs
//	Implement latest name bindings changes, and using objective-c reflection to see is a type is a OC class
//	
//	Revision 1.20  2004/06/26 06:57:20  urs
//	Fix constructors
//	
//	Revision 1.19  2004/06/26 06:52:32  urs
//	Remove hardcoding in TypeConvertor, and autoregister new classes
//	
//	Revision 1.18  2004/06/25 02:49:14  gnorton
//	Sample 2 now runs.
//	
//	Revision 1.17  2004/06/24 19:44:18  urs
//	Cleanup
//	
//	Revision 1.16  2004/06/24 06:29:36  gnorton
//	Make foundation compile.
//	
//	Revision 1.15  2004/06/24 05:00:38  urs
//	Unflattern C# API methods to reduce conflicts
//	Rename static methods to start with a capital letter (to reduce conflict with instance methods)
//	
//	Revision 1.14  2004/06/24 04:14:35  urs
//	Fix typo
//	
//	Revision 1.13  2004/06/24 04:09:59  urs
//	Add System.Collection to generated C# files
//	
//	Revision 1.12  2004/06/24 03:48:26  urs
//	minor fix for NSObject
//	
//	Revision 1.11  2004/06/23 22:10:19  urs
//	Adding support for out of dependecy categories, generating a new class named $(class)$(categoryFramework)Extras with a the methods of all categories in same framework
//	
//	Revision 1.10  2004/06/23 18:31:51  urs
//	Add dependency for frameworks
//	
//	Revision 1.9  2004/06/23 17:55:41  urs
//	Make test compile with the lasted glue API name change
//	
//	Revision 1.8  2004/06/23 17:52:41  gnorton
//	Added ability to override what the generator outputs on a per-file/per-method basis
//	
//	Revision 1.7  2004/06/23 17:14:20  gnorton
//	Custom addins supported on a per file basis.
//	
//	Revision 1.6  2004/06/23 15:29:29  urs
//	Major refactor, allow inheriting parent constructors
//	
//	Revision 1.5  2004/06/22 13:38:59  urs
//	More cleanup and refactoring start
//	Make output actually compile (diverse fixes)
//	
//	Revision 1.4  2004/06/22 12:04:12  urs
//	Cleanup, Headers, -out:[CS|OC], VS proj
//	
//

