//
//  Authors
//    - Kangaroo, Geoff Norton
//    - Urs C. Muff, Quark Inc., <umuff@quark.com>
//
//  Copyright (c) 2004 Quark Inc.  All rights reserved.
//
// $Id: Section.cs,v 1.4 2004/09/21 04:28:54 urs Exp $
//

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CocoaSharp {

	internal class Section {

		private MachOFile mfile;
		private segment_command scmd;
		private section sec;
		private string segname;
		private string sectname;

		internal Section (MachOFile mfile, segment_command scmd) {
			this.mfile = mfile;
			this.scmd = scmd;
		}

		internal string Name {
			get { return sectname; }
		}
		
		internal uint Offset {
			get { return sec.offset; }
		}
		
		internal uint Addr {
			get { return sec.addr; }
		}
		
		internal uint Size {
			get { return sec.size; }
		}

		internal bool ContainsAddress(uint offset) {
			int off = (int)(offset-Addr);
			return (off >= 0) && (off < Size);
		}

		internal uint SegmentOffsetForVMAddr(uint offset) {
			return offset - Addr;
		}

		internal void ProcessSection () {
			unsafe {
				sec = *((section *)mfile.Pointer);
				Utils.MakeBigEndian(ref sec.addr);
				Utils.MakeBigEndian(ref sec.size);
				Utils.MakeBigEndian(ref sec.offset);
				Utils.MakeBigEndian(ref sec.align);
				Utils.MakeBigEndian(ref sec.reloff);
				Utils.MakeBigEndian(ref sec.nreloc);
				Utils.MakeBigEndian(ref sec.flags);
				Utils.MakeBigEndian(ref sec.reserved1);
				Utils.MakeBigEndian(ref sec.reserved2);
				sectname = Utils.GetString(mfile.Pointer, 16);
				segname = Utils.GetString(mfile.Pointer+16, 16);
				mfile.Pointer += (int)Marshal.SizeOf (sec);
			}

			MachOFile.DebugOut("\t\tSectName: {0}", sectname);
		}
	}

	// http://developer.apple.com/documentation/DeveloperTools/Conceptual/MachORuntime/FileStructure/chapter_4_section_8.html#//apple_ref/doc/uid/20001298/section
	//
	// Directly following a segment_command data structure is an array of section data structures, with the exact count determined by the nsects field of the 
	// segment_command structure.
	internal struct section {
		// A string specifying the name of this section. The value of this field can be any sequence of ASCII characters, although section names defined by Apple 
		// begin with two underscores and consist of lowercase letters (as in __text and __data). This field is fixed at 16 bytes in length.
		internal byte sectname0,sectname1,sectname2,sectname3,sectname4,sectname5,sectname6,sectname7,
			sectname8,sectname9,sectname10,sectname11,sectname12,sectname13,sectname14,sectname15;
		// A string specifying the name of the segment that should eventually contain this section. For compactness, intermediate object filesÑfiles of type 
		// MH_OBJECTÑcontain only one segment, in which all sections are placed. The static linker places each section in the named segment when building the final 
		// product (any file that is not of type MH_OBJECT).
		internal byte segname0,segname1,segname2,segname3,segname4,segname5,segname6,segname7,
			segname8,segname9,segname10,segname11,segname12,segname13,segname14,segname15;
		// An integer specifying the virtual memory address of this section.
		internal uint addr;
		// An integer specifying the size in bytes of the virtual memory occupied by this section.
		internal uint size;
		// An integer specifying the offset to this section in the file.
		internal uint offset;
		// An integer specifying the sectionÕs byte alignment. Specify this as a power of two; for example, a section with 8-byte alignment would have an align value 
		// of 3 (2 to the 3rd power equals 8).
		internal uint align;
		// An integer specifying the file offset of the first relocation entry for this section.
		internal uint reloff;
		// An integer specifying the number of relocation entries located at reloff for this section.
		internal uint nreloc;
		// An integer divided into two parts. The least significant 8 bits contain the section type, while the most significant 24 bits contain a set of flags that 
		// specify other attributes of the section. These types and flags are primarily used by the static linker and file analysis tools, such as otool, to determine 
		// how to modify or display the section. These are the possible types:
		// - S_REGULARÑThis section has no particular type. The standard tools create a __TEXT,__text section of this type.
		// - S_ZEROFILLÑZero-fill-on-demand sectionÑwhen this section is first read from or written to, each page within is automatically filled with bytes containing 
		//   zero.
		// - S_CSTRING_LITERALSÑThis section contains only constant C strings. The standard tools create a __TEXT,__cstring section of this type.
		// - S_4BYTE_LITERALSÑThis section contains only constant values that are 4 bytes long. The standard tools create a __TEXT,__literal4 section of this type.
		// - S_8BYTE_LITERALSÑThis section contains only constant values that are 8 bytes long. The standard tools create a __TEXT,__literal8 section of this type.
		// - S_LITERAL_POINTERSÑThis section contains only pointers to constant values.
		// - S_NON_LAZY_SYMBOL_POINTERSÑThis section contains only non-lazy pointers to symbols. The standard tools create a section of the __DATA,__nl_symbol_ptrs 
		//   section of this type.
		// - S_LAZY_SYMBOL_POINTERSÑThis section contains only lazy pointers to symbols. The standard tools create a __DATA,__la_symbol_ptrs section of this type.
		// - S_SYMBOL_STUBSÑÑThis section contains symbol stubs. The standard tools create __TEXT,__symbol_stub and __TEXT,__picsymbol_stub sections of this type. 
		//   See ÒIndirect AddressingÓ for more information.
		// - S_MOD_INIT_FUNC_POINTERSÑThis section contains pointers to module initialization functions. The standard tools create __DATA,__mod_init_func sections of 
		//   this type.
		// - S_MOD_TERM_FUNC_POINTERSÑThis section contains pointers to module termination functions. The standard tools create __DATA,__mod_term_func sections of this
		//   type.
		// - S_COALESCEDÑThis section contains symbols that are coalesced by the static linker and possibly the dynamic linker. More than one file may contain coalesced
		//   definitions of the same symbol without causing multiple-defined-symbol errors.
		// The following are the possible attributes of a section:
		// - S_ATTR_PURE_INSTRUCTIONSÑThis section contains only executable machine instructions. The standard tools set this flag for the sections __TEXT,__text, 
		//   __TEXT,__symbol_stub, and __TEXT,__picsymbol_stub.
		// - S_ATTR_NO_TOCÑThis section contains coalesced symbols that must not be placed in the table of contents (SYMDEF member) of a static archive library.
		// - S_ATTR_SOME_INSTRUCTIONSÑThis section contains executable machine instructions and other data.
		// - S_ATTR_EXT_RELOCÑThis section contains references that must be relocated. These references refer to data that exists in other files (undefined symbols). 
		//   To support external relocation, the maximum virtual memory protections of the segment that contains this section must allow both reading and writing.
		// - S_ATTR_LOC_RELOCÑThis section contains references that must be relocated. These references refer to data within this file.
		// - S_ATTR_STRIP_STATIC_SYMSÑThe static symbols in this section can be stripped if the MH_DYLDLINK flag of the imageÕs mach_header header structure is set.
		// - S_ATTR_NO_DEAD_STRIPÑThis section must not be dead-stripped. See ÒDead-Code StrippingÓ in Xcode Build System for details.
		// - S_ATTR_LIVE_SUPPORTÑThis section must not be dead-stripped if they reference code that is live, but the reference is undetectable.
		internal uint flags;
		// An integer reserved for use with certain section types. For symbol pointer sections and symbol stubs sections that refer to indirect symbol table entries, 
		// this is the index into the indirect table for this sectionÕs entries. The number of entries is based on the section size divided by the size of the symbol 
		// pointer or stub. Otherwise this field is set to zero.
		internal uint reserved1;
		// For sections of type S_SYMBOL_STUBS, an integer specifying the size (in bytes) of the symbol stub entries contained in the section. Otherwise, this field is 
		// reserved for future use and should be set to zero.
		internal uint reserved2;
	}
}

//
// $Log: Section.cs,v $
// Revision 1.4  2004/09/21 04:28:54  urs
// Shut up generator
// Add namespace to generator.xml
// Search for framework
// Fix path issues
// Fix static methods
//
// Revision 1.3  2004/09/11 00:41:22  urs
// Move Output to gen-out
//
// Revision 1.2  2004/09/09 02:33:04  urs
// Fix build
//
