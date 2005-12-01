//
//  Authors
//    - Kangaroo, Geoff Norton
//    - Urs C. Muff, Quark Inc., <umuff@quark.com>
//
//  Copyright (c) 2004 Quark Inc.  All rights reserved.
//
// $Id: DylibCommand.cs,v 1.3 2004/09/11 00:41:22 urs Exp $
//

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CocoaSharp {

	internal class DylibCommand : ICommand {

		private load_command lcmd;
		private dylib dld;
		private string name;
		private MachOFile mfile;

		internal DylibCommand (MachOFile mfile, load_command lcmd) {
			this.mfile = mfile;
			this.lcmd = lcmd;
		}

		public void ProcessCommand () {
			unsafe {
				dld = *((dylib *)mfile.Pointer);
				Utils.MakeBigEndian(ref dld.offset);
				Utils.MakeBigEndian(ref dld.timestamp);
				Utils.MakeBigEndian(ref dld.current_version);
				Utils.MakeBigEndian(ref dld.compatability_version);
				name = Marshal.PtrToStringAuto (new IntPtr (mfile.Pointer + dld.offset - Marshal.SizeOf (lcmd)));
				mfile.Pointer += (int)(lcmd.cmdsize - Marshal.SizeOf (lcmd));
			}
		}
	}


	// http://developer.apple.com/documentation/DeveloperTools/Conceptual/MachORuntime/FileStructure/chapter_4_section_12.html#//apple_ref/doc/c_ref/dylib
	//
	// Defines the data used by the dynamic linker to match a shared library against the files that have linked to it. Used exclusively in the dylib_command
	// data structure.
	internal struct dylib {
		// A data structure of type lc_str. Specifies the name of the shared library.
		internal uint offset;
		// The date and time when the shared library was built.
		internal uint timestamp;
		// The current version of the shared library.
		internal uint current_version;
		// The compatibility version of the shared library.
		internal uint compatability_version;
	}
}

//
// $Log: DylibCommand.cs,v $
// Revision 1.3  2004/09/11 00:41:22  urs
// Move Output to gen-out
//
// Revision 1.2  2004/09/09 02:33:04  urs
// Fix build
//
