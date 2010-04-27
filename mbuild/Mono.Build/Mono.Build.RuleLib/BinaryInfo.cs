//
// A specialized dictionary for information about 
// running a system native binary.
//

using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Text;

using Mono.Build;

namespace Mono.Build.RuleLib {

	[Serializable]
	public abstract class BinaryInfo : CompositeResult {

		MBDictionary environment;

		// Construction

		public BinaryInfo () : base () {
			environment = new MBDictionary ();
			Architecture = ArchKind.Build;
		}

		// abstract bits

		// if ctxt = null, do the best possible to retrieve the
		// program name; the output is not expected to be useful.
		// (ie, this should only happen for display to the user or debugging.)

		public abstract string GetProgram (IBuildContext ctxt);

		public virtual string GetOtherArgs (IBuildContext ctxt) {
			return "";
		}

		// CompositeResult

		protected override int TotalItems {
			get { return base.TotalItems + 3; }
		}

		protected override void CopyItems (Result[] r) {
			int t = base.TotalItems;

			base.CopyItems (r);
			r[t] = new MBString (ForcedArgs);
			r[t + 1] = environment;
			r[t + 2] = new ArchKindResult (Architecture);
		}

		// Result

		protected override void CloneTo (Result r) {
			BinaryInfo other = (BinaryInfo) r;

			other.ForcedArgs = ForcedArgs;
			other.environment = (MBDictionary) environment.Clone ();
			other.Architecture = Architecture;
		}

		// Generic

		public string ForcedArgs;

		public ArchKind Architecture;

		public string GetEnvironment (string var) {
			return environment.GetString (var);
		}

		public void SetEnvironment (string var, string  val) {
			environment.SetString (var, val);
		}

		public IEnumerable EnvironmentKeys { get { return new EnvironmentEnumerable (this); } }

		public string ToUnixStyle (IBuildContext ctxt) {
			StringBuilder sb = new StringBuilder ();

			foreach (string k in EnvironmentKeys) {
				sb.AppendFormat ("{0}={1} ", k, GetEnvironment (k));
			}

			sb.AppendFormat ("{0} {1}", GetProgram (ctxt), GetOtherArgs (ctxt));

			if (ForcedArgs != null)
				sb.AppendFormat (" {0}", ForcedArgs);

			return sb.ToString ();
		}

		internal ProcessStartInfo MakeInfo (IBuildContext ctxt) {
			ArchKindResult.AssertCanExecute (Architecture);

			string args = GetOtherArgs (ctxt);
			string forced = ForcedArgs;

			if (forced != null) {
				if (args.Length > 0)
					args += " " + forced;
				else
					args = forced;
			}

			ProcessStartInfo si = new ProcessStartInfo (GetProgram (ctxt), args);
		       
			foreach (string k in EnvironmentKeys)
				si.EnvironmentVariables[k] = GetEnvironment (k);

			return si;
		}

		public override string ToString () {
			return String.Format ("[program: {0}]", ToUnixStyle (null));
		}

		class EnvironmentEnumerable : IEnumerable {
			BinaryInfo info;

			public EnvironmentEnumerable (BinaryInfo info) {
				this.info = info;
			}

			public IEnumerator GetEnumerator () {
				return info.environment.GetEnumerator ();
			}
		}
	}
}
