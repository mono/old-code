// ExecutableCopyInstaller.cs -- copy and make the result executable
// (It'd be better to also be able to make results in the build tree
// executable, but that's harder.)

using System;
using System.IO;
using System.Runtime.Serialization;

using Mono.Build;

namespace Mono.Build.RuleLib {

	[Serializable]
	public class ExecutableCopyInstaller : CopyInstaller {

		protected override bool PostCopy (MBFile src, MBFile dest, bool backwards, IBuildContext ctxt) {
			if (backwards)
				return false;

			try {
				dest.MakeExecutable (ctxt);
			} catch (Exception e) {
				ctxt.Logger.Error (1004, "Unhandled exception while attempting to make file executable",
						   e.ToString ());
				return true;
			}

			return false;
		}

		public override string DescribeAction (Result other, IBuildContext ctxt) {
			return String.Format ("Copy {0} to {1} and make it executable", 
					      ((MBFile) other).GetPath (ctxt), DestDir);
		}
	}
}
