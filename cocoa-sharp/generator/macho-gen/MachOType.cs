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
using System.Text.RegularExpressions;

namespace CocoaSharp {

	// Meaning				Code
	//--------------------------
	// id					`@'
	// Class				`#'
	// SEL					`:'
	// void					`v'
	// char					`c'
	// unsigned char		`C'
	// short				`s'
	// unsigned short		`S'
	// int					`i'
	// unsigned int			`I'
	// long					`l'
	// unsigned long		`L'
	// long long			`q'
	// unsigned long long	`Q'
	// float				`f'
	// double				`d'
	// C++ bool or a C99 _Bool `B'
	// char *				`*'
	// any pointer			`^'
	// an undefined type	`?'
	// a bitfield			`b'
	// begin an array		`['
	// end an array			`]'
	// begin a union		`('
	// end a union			`)'
	// begin a structure	`{'
	// end a structure		`}'

	// The same codes are used for methods declared in a protocol, but with these additions for type modifiers:
	// const				`r'
	// in					`n'
	// inout				`N'
	// out					`o'
	// bycopy				`O'
	// oneway				`V'

	public class MachOType {
		public OCType kind = OCType.id;
		public TypeModifiers modifiers;
		public int offset, arrayDim, bitCount;
		public string name, nameSpace;
		public MachOType reference;
		public MachOType[] fields;
		public TypeUsage typeUsage;

		// match "NSObject"" or "NSObject") or "NSObject"} or "NSObject"]
		private static Regex _idHintRegex = new Regex("^(\"[-_a-zA-Z0-9]+\")?@(?<hint>\"[-_a-zA-Z0-9]+\"[})\"\\]])");

		private MachOType(string nameSpace) { this.nameSpace = nameSpace; }

		public bool IsPrimitive {
			get { return kind != OCType.structure && kind != OCType.array && kind != OCType.pointer && kind != OCType.union; }
		}

		public string DeclType {
			get {
				string decl = Type.OCTypeToDeclType(kind);
				if (decl == null) {
					switch (kind) {
						case OCType.id:
							decl = this.name == "id" ? "id" : this.name + " *";
							break;
						case OCType.array:
							decl = this.reference.DeclType + "[" + this.arrayDim + "]";
							break;
						case OCType.bit_field:
							decl = "int:" + this.bitCount;
							break;
						case OCType.pointer:
							decl = (this.reference.kind == OCType.undefined_type ? "void" : this.reference.DeclType) + " *";
							break;
						case OCType.structure:
							decl = "struct " + this.name;
							break;
						case OCType.undefined_type:
							break;
						case OCType.union:
							decl = "union " + this.name;
							break;
						default:
							decl = null;
							break;
					}
				}
				return decl;
			}
		}

		public void RegisterType() {
			Type type = Type.FromOcType(this.kind, this.name);
			if (type == null) {
				string decl = this.DeclType;
				if (this.kind == OCType.pointer)
					decl = decl.Trim();
				if (decl != null)
					type = Type.FromDecl(decl);
				else
					type = new Type(name, nameSpace, this.ApiType, this.GlueType, this.kind);
			}
			this.typeUsage = new TypeUsage(type, modifiers);
		}

		public override string ToString() {
			string detail = reference != null ? reference.ToString() : string.Empty;
			switch (kind) {
				case OCType.structure:
				case OCType.union:
					foreach (MachOType field in fields) {
						if (detail != string.Empty) detail += ",";
						detail += field.ToString();
					}
					break;
				case OCType.array:
					detail = detail + "[" + arrayDim + "]";
					break;
				case OCType.bit_field:
					detail = detail + ":" + bitCount;
					break;
			}
			string ret = kind.ToString();
			if (name != null)
				ret = name + "=" + ret;
			if (modifiers != 0)
				ret += " " + modifiers;
			if (detail != string.Empty)
				ret += " (" + detail + ")";
			if (offset != 0)
				ret += " offset=" + offset;
			return ret;
		}

		internal System.Type GlueType {
			get {
				return Type.OCTypeToGlueType(this.kind);
			}
		}

		internal string ApiType {
			get {
				if (this.kind == OCType.array)
					return this.reference.ApiType + "[]";
				return Type.OCTypeToApiType(this.kind);
			}
		}

		internal TypeUsage ToTypeUsage(string nameSpace) {
			return typeUsage;
		}
		internal Type ToType(string nameSpace) {
			return typeUsage.Type;
		}

		static public MachOType[] ParseTypes(string nameSpace, string types) {
			ArrayList ret = new ArrayList();
#if DEBUG
			bool hasNonPrimitive = false;
#endif
			int read = 0;
			string tmp = types;
			do {
				tmp = tmp.Substring(read);
				MachOType t = ParseType(nameSpace, tmp,true,out read);
#if DEBUG
				if (!hasNonPrimitive)
					hasNonPrimitive = !t.IsPrimitive;
#endif
				ret.Add(t);
			} while (read < tmp.Length);
#if DEBUG
			if (hasNonPrimitive) {
				MachOFile.DebugOut(1,"Parsing '{0}'",types);
				MachOFile.DebugOut(1,"   ret={0}",ret[0]);
				for (int i = 3; i < ret.Count; ++i)
					MachOFile.DebugOut(1,"   #{0}={1}",i-3,ret[i]);
			}
#endif
			return (MachOType[])ret.ToArray(typeof(MachOType));
		}

		static int ParseInt(string type,ref int read) {
			string intStr = string.Empty;
			while (read < type.Length && char.IsDigit(type[read]))
				intStr += type[read++];

			if (intStr != string.Empty)
				return int.Parse(intStr);
			return 0;
		}

		static public MachOType ParseType(string nameSpace, string type) {
			int tmpRead;
			return ParseType(nameSpace, type,false,out tmpRead);
		}
		
		static MachOType ParseSubType(string nameSpace, string type, ref int read) {
			int tmpRead;
			MachOType ret = ParseType(nameSpace, type.Substring(read),false,out tmpRead);
			read += tmpRead;
			return ret;
		}

		static public MachOType ParseType(string nameSpace, string type,bool readOff,out int read) {
			MachOType ret = new MachOType(nameSpace);
			read = 0;
			MachOFile.DebugOut(1,"- Parsing '{0}'",type);
			bool cont;
			do {
				cont = false;
				switch (type[read]) {
					case '@': // id
						++read;
						if (ret.name == null)
							ret.name = "id";
						ret.kind = OCType.id;
						if (read < type.Length && type[read] == '"') {
							/*
							 * Tiger seems to "hint" at an object type that it might
							 * want in a message in a way that < 10.4 did not.  It looks like this:
							 *
							 * "paramName"@"NSBundle"
							 *
							 * This is almost always followed by another name, so:
							 *
							 * "param"@"NSBundle""param2"I
							 * 
							 * but it could also come at the end of a structure, so make sure to match "} also.
							 */
							Match h = _idHintRegex.Match(type);
							if (h.Success) { // found it, so skip it but not the last char
								read += h.Groups["hint"].Value.Length - 1;
							}
							cont = false;
						} else if (read < type.Length && type[read] == '{') {
							ret.reference = ParseSubType(nameSpace, type,ref read);
							cont = false;
						} else {
							cont = ret.name == "id" && read < type.Length && type[read] == '"';
						}
						break;
					case '#': // Class
						++read;
						ret.kind = OCType.Class;
						break;
					case ':': // SEL
						++read;
						ret.kind = OCType.SEL;
						break;
					case 'v': // void
						++read;
						ret.kind = OCType.@void;
						break;
					case 'c': // char
						++read;
						ret.kind = OCType.@char;
						break;
					case 'C': // unsigned char
						++read;
						ret.kind = OCType.unsigned_char;
						break;
					case 's': // short
						++read;
						ret.kind = OCType.@short;
						break;
					case 'S': // unsigned short
						++read;
						ret.kind = OCType.unsigned_short;
						break;
					case 'i': // int
						++read;
						ret.kind = OCType.@int;
						break;
					case 'I': // unsigned int
						++read;
						ret.kind = OCType.unsigned_int;
						break;
					case 'l': // long
						++read;
						ret.kind = OCType.@long;
						break;
					case 'L': // unsigned long
						++read;
						ret.kind = OCType.unsigned_long;
						break;
					case 'q': // long long
						++read;
						ret.kind = OCType.long_long;
						break;
					case 'Q': // unsigned long long
						++read;
						ret.kind = OCType.unsigned_long_long;
						break;
					case 'f': // float
						++read;
						ret.kind = OCType.@float;
						break;
					case 'd': // double
						++read;
						ret.kind = OCType.@double;
						break;
					case 'B': // C++ bool or a C99 _Bool
						++read;
						ret.kind = OCType.@bool;
						break;
					case '*': // char *
						++read;
						ret.kind = OCType.char_ptr;
						break;
					case '^': // any pointer
						++read;
						ret.kind = OCType.pointer;
						ret.reference = ParseSubType(nameSpace, type,ref read);
						break;
					case '?': // an undefined type
						++read;
						ret.name = "?";
						ret.kind = OCType.undefined_type;
						break;
					case 'b': // a bitfield
						++read;
						ret.kind = OCType.bit_field;
						ret.bitCount = ParseInt(type, ref read);
						break;
					case '[': // begin an array
						++read;
						ret.kind = OCType.array;
						ret.arrayDim = ParseInt(type, ref read);
						ret.reference = ParseSubType(nameSpace, type, ref read);
						if (type[read] != ']')
							MachOFile.DebugOut(0,"ERROR: array does not end with ']' ({0}) #{1}",type,read);
						else
							++read;
						break;
					case '"': // begin a name
						cont = read == 0;
						++read; {
						int nameOff = type.IndexOf('"',read);
						ret.name = type.Substring(read,nameOff-read);
						read = nameOff+1;
					}
						break;
					case '(': // begin a union
					case '{': // begin a structure
						ret.kind = type[read] == '(' ? OCType.union : OCType.structure;
						++read; {
						char close = ret.kind == OCType.union ? ')' : '}';
						int nameOff = type.IndexOfAny(new char[]{'=',close},read);
						if (nameOff >= 0) {
							ArrayList fields = new ArrayList();
							ret.name = type.Substring(read,nameOff-read);
							read = nameOff;
							if (type[read] == '=')
								++read;
							/*
							 * XXX: This handles _NSRepresentationInfo, which looks like this:
							 * ^{_NSRepresentationInfo=#@{...stuff
							 * ...which was getting parsed as a struct, when it should be (I think) an object.
							 */
							if (type[read] == '#')
								ret.kind = OCType.Class;

							while (type[read] != close)
								fields.Add(ParseSubType(nameSpace, type,ref read));
							if (type[read] == close)
								++read;
							else
								MachOFile.DebugOut(0,"ERROR: structure/union does not end with '{1}' ({0})",type.Substring(read),close);

							ret.fields = (MachOType[])fields.ToArray(typeof(MachOType));
						}
						else
							MachOFile.DebugOut(0,"ERROR: structure/union does not end with '{1}' ({0})",type.Substring(read),close);

						MachOType existing = (MachOType)MachOFile.Types[ret.name];
						if (existing == null)
							MachOFile.Types[ret.name] = ret;
						else if (existing.fields.Length == 0 && ret.fields.Length != 0)
							MachOFile.Types[ret.name] = ret;
						else if (existing.fields.Length > 0 && existing.fields[0].name == null && ret.fields.Length > 0 && ret.fields[0].name != null)
							MachOFile.Types[ret.name] = ret;
					}
						break;
					case 'r': // const
						cont = true;
						++read;
						ret.modifiers |= TypeModifiers.@const;
						break;
					case 'n': // in
						cont = true;
						++read;
						ret.modifiers |= TypeModifiers.@in;
						break;
					case 'N': // inout
						cont = true;
						++read;
						ret.modifiers |= TypeModifiers.inout;
						break;
					case 'o': // out
						cont = true;
						++read;
						ret.modifiers |= TypeModifiers.@out;
						break;
					case 'O': // bycopy
						cont = true;
						++read;
						ret.modifiers |= TypeModifiers.bycopy;
						break;
					case 'V': // oneway
						cont = true;
						++read;
						ret.modifiers |= TypeModifiers.oneway;
						break;
					default:
						if (ret.modifiers == 0) {
							MachOFile.DebugOut(0,"ERROR: unknown type ({0}) #{1}",type,read);
							read = type.Length;
							return null;
						}
						break;
				}
			} while (cont);
			if (readOff && read < type.Length)
				ret.offset = ParseInt(type,ref read);

			ret.RegisterType();
			return ret;
		}
	}
}

//
// $Log: MachOType.cs,v $
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
