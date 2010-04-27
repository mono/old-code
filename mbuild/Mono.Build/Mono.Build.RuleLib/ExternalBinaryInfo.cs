//
// A specialized dictionary for information about 
// running a system native binary.
//

using System;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Text;

using Mono.Build;

namespace Mono.Build.RuleLib {

	[Serializable]
	public class ExternalBinaryInfo : BinaryInfo {

		public ExternalBinaryInfo () : this ((string) null, (string) null) {}

		public ExternalBinaryInfo (string program) : this (program, null) {}

		public ExternalBinaryInfo (string program, string args) : base () {
			if (program != null)
				Program = program;
			if (args != null)
				ForcedArgs = args;
		}

		public bool SetFromUnixStyle (string command, IBuildContext ctxt) 
		{
		    string[] parts = command.Trim ().Split (' ', '\t');
		    StringBuilder sb = new StringBuilder ();
		    bool seen_prog = false;

		    for (int i = 0; i < parts.Length; i++) {
			if (seen_prog) {
			    sb.Append (" ");
			    sb.Append (parts[i]);
			    continue;
			}

			int eq_index = parts[i].IndexOf ('=');

			if (eq_index == -1) {
			    Program = parts[i];
			    seen_prog = true;
			    continue;
			} else if (eq_index == 0) {
			    if (ctxt != null)
				ctxt.Logger.Error (3005, "Misplaced equals sign in Unix-style program " +
						   "specification \"" + command + "\"", null);
			    return true;
			}

			string var = parts[i].Substring (0, eq_index);
			string val = parts[i].Substring (eq_index + 1, parts[i].Length - (eq_index + 1));
			SetEnvironment (var, val);
		    }

		    if (sb.Length > 0)
			ForcedArgs = sb.ToString ();

		    return false;
		}

		public static ExternalBinaryInfo ParseFromUnixStyle (string command, IBuildContext ctxt) 
		{
		    ExternalBinaryInfo info = new ExternalBinaryInfo ();

		    if (info.SetFromUnixStyle (command, ctxt))
			return null;

		    return info;
		}

		public static ExternalBinaryInfo ParseFromUnixStyle (string command) {
		    return ParseFromUnixStyle (command, null);
		}

		// CompositeResult

		public string Program;

		protected override int TotalItems {
			get { return base.TotalItems + 1; }
		}

		protected override void CopyItems (Result[] r) {
			int t = base.TotalItems;

			base.CopyItems (r);
			r[t] = new MBString (Program);
		}

		// Result

		protected override void CloneTo (Result r) {
			ExternalBinaryInfo ebi = (ExternalBinaryInfo) r;

			ebi.Program = Program;
		}

		// implementation

		public override string GetProgram (IBuildContext ctxt) {
			return Program;
		}
	}
}
