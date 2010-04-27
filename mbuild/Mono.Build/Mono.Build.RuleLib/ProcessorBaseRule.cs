// ProcessorBaseRule.cs -- generic base for a tool that processes one file
// to generate another. Assumes that .target is the destination file.
// Used to make generic the pattern of "write output to a temporary file,
// then copy it to the final destination once we're sure the tool succeeded."

using System;

using Mono.Build;

namespace Mono.Build.RuleLib {

	public abstract class ProcessorBaseRule : OutputFileRule {
		public ProcessorBaseRule () : base () { }

		public override Type GeneralResultType 
		{
		    get { return typeof (MBFile); }
		}

		protected bool GetOutputAndTemp (IBuildContext ctxt, MBFile output, 
						 out MBFile tempdest) 
		{
			string name = GetOutputName (ctxt);
			if (name == null) {
				output = null;
				tempdest = null;
				return true;
			}

			output.Dir = ctxt.WorkingDirectory;
			output.Name = name;

			tempdest = (MBFile) output.Clone ();

			// TODO: add in some random characters if you're really anal
			tempdest.Name += ".tmp";

			return false;
		}

		protected bool MoveToFinal (MBFile output, MBFile tempdest, IBuildContext ctxt) {
			// We should probably move away the file before clobbering it ...
			
			if (output.Exists (ctxt))
                                output.Delete (ctxt);
 
                        tempdest.MoveTo (output, ctxt);
			return false;
		}
	}
}
