// ExtractAssemblyResource.cs -- create a source file from a resource
// embedded in the executing assembly

using System;
using System.Reflection;
using System.IO;

namespace Mono.Build.RuleLib {

	public abstract class ExtractAssemblyResource : OutputFileRule {

		public ExtractAssemblyResource () : base () {}

		protected abstract string GetResourceName (IBuildContext ctxt);

		public override Type GeneralResultType
		{
		    get { return typeof (MBFile); }
		}

		public override Result Build (IBuildContext ctxt) {
			string outname = GetOutputName (ctxt);
			if (outname == null)
				return null;

			string resname = GetResourceName (ctxt);

			// Output

			MBFile file = (MBFile) CreateResultObject ();
			file.Dir = ctxt.WorkingDirectory;
			file.Name = outname;
			StreamWriter writer = new StreamWriter (file.OpenWrite (ctxt));

			// Input stream

			Assembly caller = Assembly.GetAssembly (GetType ());
			Stream rstream = caller.GetManifestResourceStream (resname);
			if (rstream == null) {
				ctxt.Logger.Error (9999, "No such resource stream " + resname + 
						   " in assembly " + caller.FullName, null);
				return null;
			}

			StreamReader reader = new StreamReader (rstream);

			// Do it

			try {
				char[] buf = new char[512];
				int read;

				do {
					read = reader.Read (buf, 0, buf.Length);
					writer.Write (buf, 0, read);
				} while (read > 0);
			} catch (Exception e) {
				ctxt.Logger.Error (9999, "Exception while writing to file from resource stream" + resname, 
						   e.Message);
				return null;
			} finally {
				reader.Close ();
				writer.Close ();
			}

			return file;
		}

		// FIXME: return fingerprint of resource stream somehow

		public override Fingerprint GetFingerprint (IBuildContext ctxt, Fingerprint cached) {
			return GenericFingerprints.Null;
		}
	}
}
