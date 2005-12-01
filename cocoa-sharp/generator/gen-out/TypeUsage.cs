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
	public class TypeUsage {
		public TypeUsage(Type type, TypeModifiers typeModifiers) {
			this.type = type;
			this.typeModifiers = typeModifiers;
		}

		public static TypeUsage FromDecl(string objcDecl) {
			TypeModifiers mods = TypeModifiers.none;
			if (objcDecl.StartsWith("const ")) {
				objcDecl = objcDecl.Substring("const ".Length);
				mods |= TypeModifiers.@const;
			}
			return new TypeUsage(Type.FromDecl(objcDecl), mods);
		}

		public bool Merge(TypeUsage machoType) {
			this.typeModifiers = machoType.TypeModifiers;
			if ((this.Name == "BOOL" || this.Name == "Boolean") && (machoType.Type.OCType == OCType.@char || machoType.Type.OCType == OCType.unsigned_char))
				return true;
			else if (this.Type.OCType == OCType.id && machoType.Type.OCType == OCType.pointer)
				return true;
			else {
				if (this.GlueType != machoType.GlueType)
					return false;
				if (this.Type.OCType != machoType.Type.OCType)
					return false;
				return true;
			}
		}

		// -- Public Properties --
		public string Name { get { return type.Name; } }
		public string TypeStr {
			get {
				string mod = string.Empty;
				if ((this.typeModifiers & TypeModifiers.@const) != 0)
					mod += "r";
				if ((this.typeModifiers & TypeModifiers.@in) != 0)
					mod += "n";
				if ((this.typeModifiers & TypeModifiers.inout) != 0)
					mod += "N";
				if ((this.typeModifiers & TypeModifiers.@out) != 0)
					mod += "o";
				if ((this.typeModifiers & TypeModifiers.bycopy) != 0)
					mod += "O";
				if ((this.typeModifiers & TypeModifiers.oneway) != 0)
					mod += "V";
				return mod + type.TypeStr;
			}
		}
		public Type Type { get { return type; } }
		public TypeModifiers TypeModifiers { get { return typeModifiers; } }
		public string GlueType { get { return Type.GlueType; } }
		public string ApiType { get { return Type.ApiType; } }

		// -- Members --
		private Type type;
		private TypeModifiers typeModifiers;
	}

	[Flags]
	public enum TypeModifiers {
		none = 0,
		@const 	= 1 << 1,
		@in 	= 1 << 2,
		inout 	= 1 << 3,
		@out 	= 1 << 4,
		bycopy 	= 1 << 5,
		oneway 	= 1 << 6,
	}
}

//
// $Log: TypeUsage.cs,v $
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
