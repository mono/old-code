//
// A specialized dictionary for information about 
// running a system native binary.
//

using System;
using System.Runtime.Serialization;
using System.Diagnostics;

using Mono.Build;

namespace Mono.Build.RuleLib {

	[Serializable]
	public class FileBinaryInfo : BinaryInfo {

		public FileBinaryInfo () : this ((MBFile) null, (string) null) {}

		public FileBinaryInfo (MBFile program) : this (program, null) {}

		public FileBinaryInfo (MBFile program, string args) : base () {
			if (program != null)
				Program = program;
			if (args != null)
				ForcedArgs = args;
		}

		public FileBinaryInfo (ArchKind arch, MBFile program, string args) : base () {
			Architecture = arch;

			if (program != null)
				Program = program;
			if (args != null)
				ForcedArgs = args;
		}

		// CompositeResult

		protected override int TotalItems {
			get { return base.TotalItems + 1; }
		}

		protected override void CopyItems (Result[] r) {
			int t = base.TotalItems;

			base.CopyItems (r);
			r[t] = Program;
		}

		public override bool HasDefault { get { return true; } }

		public override Result Default { get { return Program; } }

		// Result

		protected override void CloneTo (Result r) {
			FileBinaryInfo fbi = (FileBinaryInfo) r;

			fbi.Program = (MBFile) Program.Clone ();
		}

		// Real stuff

		public MBFile Program;

		public override string GetProgram (IBuildContext ctxt) {
			if (ctxt == null)
				return Program.ToString ();

			return Program.GetPath (ctxt);
		}
	}
}
