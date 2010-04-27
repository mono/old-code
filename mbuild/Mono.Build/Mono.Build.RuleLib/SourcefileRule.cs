//
// SourcefileRule.cs -- a rule that converts its .target into a source file
//

using System;
using System.Reflection;
using System.Collections;

using Mono.Build;

namespace Mono.Build.RuleLib {

	public class SourcefileRule : OutputFileRule {
		
		public SourcefileRule () : base () {}

		public override Type GeneralResultType
		{
		    get { return typeof (MBFile); }
		}

		protected override string OutputArgName {
		    get { return "input"; }
		}

		public override Result Build (IBuildContext ctxt) {
			string name = GetOutputName (ctxt);

			if (name == null)
				return null;

			MBFile result = (MBFile) CreateResultObject ();
			result.Dir = ctxt.SourceDirectory;
			result.Name = name;
			return result;
		}

		public override Fingerprint GetFingerprint (IBuildContext ctxt, Fingerprint cached) {
			return GenericFingerprints.Null;
		}
	}
}
