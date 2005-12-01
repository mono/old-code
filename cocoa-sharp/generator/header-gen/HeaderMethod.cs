//
//  HeaderMethod.cs
//
//  Authors
//    - Kangaroo, Geoff Norton
//    - C.J. Collier, Collier Technologies, <cjcollier@colliertech.org>
//    - Urs C. Muff, Quark Inc., <umuff@quark.com>
//    - Adham Findlay
//
//  Copyright (c) 2004 Quark Inc. and Collier Technologies.  All rights reserved.
//
//	$Header: /home/miguel/third-conversion/public/cocoa-sharp/generator/header-gen/HeaderMethod.cs,v 1.3 2004/09/18 17:30:17 urs Exp $
//

using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace CocoaSharp {

	public class HeaderMethod {
		#region -- Members --
		private string mClassName;
		private string mMethodDeclaration;
		private string mCSMethodName;
		private string[] mMessageParts;
		private string[] mArgumentNames;
		private string[] mArgumentDeclarationTypes;
		private bool mIsClassMethod, mIsUnsupported;
		private string mReturnDeclarationType;

		private static Regex[] sUnsupported = new Regex[] {
			new Regex(@"<.*>"),
			new Regex(@"\.\.\.")
		};
		private static Regex sMatch1 = new Regex(@"\s*([+-])\s*(?:\(([^\)]+)\))?(.+)");
		#endregion

		#region -- Constructor --
		public HeaderMethod(string methodDeclaration, string className) {
			mMethodDeclaration = methodDeclaration.Trim();
			mClassName = className;

			// Check for unsupported methods and return commented function
			// Unsupported methods include:
			// <.*> or ...
			foreach (Regex r in sUnsupported)
				if (r.IsMatch(mMethodDeclaration)) {
					mIsUnsupported = true;
					return;
				}

			// It seems that methods take one of two formats.  Zero arguments:
			// - (RETURNTYPE)MethodName;
			// or N arguments
			// - (RETURNTYPE)MethodName:(TYPE0)Arg0 ... ArgNName:(TYPEN)ArgN;
			if (!sMatch1.IsMatch(mMethodDeclaration)) {
				mIsUnsupported = true;
				return;
			}

			// \s*([+-])\s*(?:\(([^\)]+)\))?(.+)
			string methodDecl = mMethodDeclaration.Replace("oneway ",string.Empty);
			methodDecl = methodDecl.Replace("IBAction","void");
			Match match = sMatch1.Match(methodDecl);

			string methodType = match.Groups[1].Value;
			mReturnDeclarationType = Method.StripComments(match.Groups[2].Value.Trim());
			if (mReturnDeclarationType.Length == 0)
				mReturnDeclarationType = "id";
			string remainder = match.Groups[3].Value;

			mIsClassMethod = methodType == "+";

			// get rid of comments
			// remainder =~ s://.*::;
			// remainder =~ s:/\*.*\*/::;

			// These arrays store our method names, their arg names and types
			Regex noarg_rx = new Regex(@"^\s*(\w+)\s*([;\{]|$)");
			Regex arg_rx   = new Regex(@"(\w+):\s*(?:\(([^\)]+)\))?\s*(\w+)?(?:\s+|;)");

			ArrayList messageParts = new ArrayList();
			ArrayList argTypes = new ArrayList();
			ArrayList argNames = new ArrayList();
			if(noarg_rx.IsMatch(remainder)) {
				// If there are no arguments (only matches method name)
				match = noarg_rx.Match(remainder);
				messageParts.Add(match.Groups[1].Value);
				mArgumentNames = new string[0];
				mArgumentDeclarationTypes = new string[0];
			} 
			else if(arg_rx.IsMatch(remainder)) {
				while(arg_rx.IsMatch(remainder)) {
					// If there are arguments, parse them
					GroupCollection grps = arg_rx.Match(remainder).Groups;
					for (int i = 1; i < grps.Count; ) {
						messageParts.Add(grps[i++].Value);
						string argType = grps[i++].Value.Trim();
						string argName = grps[i++].Value.Trim();
						remainder = remainder.Replace(grps[0].Value, "");

						if (argType == string.Empty)
							argType = "id";
						else if (argName == string.Empty) {
							argName = argType;
							argType = "id";
						}
						else
							argType = Method.StripComments(argType);

						argTypes.Add(argType);
						argNames.Add(argName);
					}
				}
				mArgumentNames = (string[])argNames.ToArray(typeof(string));
				mArgumentDeclarationTypes = (string[])argTypes.ToArray(typeof(string));
			} 
			else {
				// If we can't parse the method, complain
				mIsUnsupported = true;
				return;
			}

			mMessageParts = (string[])messageParts.ToArray(typeof(string));
			
			mCSMethodName = Method.MakeCSMethodName(mIsClassMethod,/*mMessageParts[0]*/ string.Join("_", mMessageParts));
		}
		#endregion

		#region -- Properties --
		public bool IsUnsupported { get { return mIsUnsupported; } }
		public bool IsClassMethod { get { return mIsClassMethod; } }
		public string MethodDeclaration { get { return mMethodDeclaration; } }
		public string ReturnDeclarationType { get { return mReturnDeclarationType; } }
		public string Selector {
			get {
				string ret = string.Empty;

				if (mMessageParts.Length == 1 && mArgumentNames.Length == 0) 
					return ret + mMessageParts[0];

				for(int i = 0; i < mMessageParts.Length; ++i)
					ret += mMessageParts[i] + ":";
				return ret;
			}
		}
		public string FullSelector { get { return (this.IsClassMethod ? "+" : "-") + Selector; } }

		#endregion

		public void BuildArgs(string name) {
#if false
			if (mCSAPIParameters != null)
				return;

			ArrayList _params = new ArrayList();
			ArrayList _glueArgs = new ArrayList();

			if (mIsClassMethod)
				_glueArgs.Add(name + "_classPtr");
			else
				_glueArgs.Add("Raw");
			
			for(int i = 0; i < mArgumentDeclarationTypes.Length; ++i) {
				string t = mArgumentAPITypes[i];
				_params.Add(t + " p" + i + "/*" + mArgumentNames[i] + "*/");
				_glueArgs.Add(ArgumentExpression(mArgumentDeclarationTypes[i],
					mArgumentGlueTypes[i],mArgumentAPITypes[i],
					"p" + i + "/*" + mArgumentNames[i] + "*/"));
			}

			mCSAPIParameters = (string[])_params.ToArray(typeof(string));
			mCSGlueArguments = (string[])_glueArgs.ToArray(typeof(string));
#endif
		}

		public Method ToOutput() {
			if (this.IsUnsupported)
				return null;
			ParameterInfo[] paramInfos = new ParameterInfo[this.mArgumentNames.Length];
			for (int i = 0; i < paramInfos.Length; ++i)
				paramInfos[i] = new ParameterInfo(this.mArgumentNames[i], TypeUsage.FromDecl(this.mArgumentDeclarationTypes[i]));
			return new Method(this.mClassName, this.mIsClassMethod, this.mCSMethodName,this.Selector,
				TypeUsage.FromDecl(this.mReturnDeclarationType),paramInfos, this.MethodDeclaration);
		}

		public static ICollection ToMethods(ElementWithMethods container, ICollection methods,bool classMethod) {
			IList ret = new ArrayList();
			foreach (HeaderMethod m in methods)
				if (m.IsClassMethod == classMethod) {
					Method o = m.ToOutput();
					if (o != null)
						ret.Add(o);
				}
			return ret;
		}
	}
}

//	$Log: HeaderMethod.cs,v $
//	Revision 1.3  2004/09/18 17:30:17  urs
//	Move CS output gen into gen-out
//
//	Revision 1.2  2004/09/11 00:41:22  urs
//	Move Output to gen-out
//	
//	Revision 1.1  2004/09/09 13:18:53  urs
//	Check header generator back in.
//	
//	Revision 1.48  2004/09/07 20:51:21  urs
//	Fix line endings
//	
//	Revision 1.47  2004/07/01 16:01:41  urs
//	Fix some GC issues, but mostly just do stuff more explicit
//	Still not working with GC on
//	
//	Revision 1.46  2004/07/01 12:41:33  urs
//	- Better verbose support, individual verbose ignore per selector and per interface
//	- Minor improvements with monodoc
//	
//	Revision 1.45  2004/06/30 19:29:22  urs
//	Cleanup
//	
//	Revision 1.44  2004/06/30 16:51:00  urs
//	Making monodoc happy
//	
//	Revision 1.43  2004/06/29 13:35:51  urs
//	make tree green again, I like green :)
//	
//	Revision 1.42  2004/06/29 03:32:58  urs
//	Cleanup mapping usage: only one bug left
//	
//	Revision 1.41  2004/06/28 22:59:43  gnorton
//	Bugfixes
//	
//	Revision 1.40  2004/06/28 22:07:43  gnorton
//	Updates/bugfixes
//	
//	Revision 1.39  2004/06/28 21:31:22  gnorton
//	Initial mapping support in the gen.
//	
//	Revision 1.38  2004/06/28 19:20:38  gnorton
//	Added mapping classes
//	
//	Revision 1.37  2004/06/28 19:18:31  urs
//	Implement latest name bindings changes, and using objective-c reflection to see is a type is a OC class
//	
//	Revision 1.36  2004/06/25 22:30:07  urs
//	Add better logging
//	
//	Revision 1.35  2004/06/25 17:39:10  urs
//	Handle char* as argument and return value
//	
//	Revision 1.34  2004/06/25 02:49:14  gnorton
//	Sample 2 now runs.
//	
//	Revision 1.33  2004/06/24 20:09:24  urs
//	fix constructor gen
//	
//	Revision 1.32  2004/06/24 18:56:53  gnorton
//	AppKit compiles
//	Foundation compiles
//	Output setMethod() for protocols not just the property so Interfaces are met.
//	Ignore static protocol methods (.NET doesn't support static in interfaces).
//	Resolve compiler errors.
//	
//	Revision 1.31  2004/06/24 06:29:36  gnorton
//	Make foundation compile.
//	
//	Revision 1.30  2004/06/24 05:21:04  urs
//	Fix typo
//	
//	Revision 1.29  2004/06/24 05:00:38  urs
//	Unflattern C# API methods to reduce conflicts
//	Rename static methods to start with a capital letter (to reduce conflict with instance methods)
//	
//	Revision 1.28  2004/06/24 03:37:07  gnorton
//	Some performance increates on the dynamic type converter (convert the <type /> entries to a IDictionary to access an indexer; rather than foreaching)
//	
//	Revision 1.27  2004/06/24 02:16:05  gnorton
//	Updated out typeconversions to be loaded from an XML file; instead of being hard coded.  In the future we wont need to update the app to update the types.
//	
//	Revision 1.26  2004/06/23 18:18:32  urs
//	Allow same case get/set properties
//	
//	Revision 1.25  2004/06/23 17:55:41  urs
//	Make test compile with the lasted glue API name change
//	
//	Revision 1.24  2004/06/23 17:52:41  gnorton
//	Added ability to override what the generator outputs on a per-file/per-method basis
//	
//	Revision 1.23  2004/06/23 17:23:41  urs
//	Rename glue methods to include argument count to differenciate 'init' from 'init:'.
//	
//	Revision 1.22  2004/06/23 17:05:33  urs
//	Add selector to Method
//	
//	Revision 1.21  2004/06/23 16:32:35  urs
//	Add SEL support
//	
//	Revision 1.20  2004/06/23 15:29:29  urs
//	Major refactor, allow inheriting parent constructors
//	
//	Revision 1.19  2004/06/22 19:54:21  urs
//	Add property support
//	
//	Revision 1.18  2004/06/22 15:13:18  urs
//	New fixing
//	
//	Revision 1.17  2004/06/22 14:16:20  urs
//	Minor fix
//	
//	Revision 1.16  2004/06/22 13:38:59  urs
//	More cleanup and refactoring start
//	Make output actually compile (diverse fixes)
//	
//	Revision 1.15  2004/06/22 12:04:12  urs
//	Cleanup, Headers, -out:[CS|OC], VS proj
//	
//
