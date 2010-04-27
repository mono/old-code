// CopyInstaller.cs -- an installer that copies a MBFile with room
// for expansion

using System;
using System.IO;
using System.Runtime.Serialization;

using Mono.Build;

namespace Mono.Build.RuleLib {

	[Serializable]
	public class CopyInstaller : CompositeResult, IResultInstaller {

		// one might expect destdir to be an MBDirectory. However,
		// the directory that we install to might not exist in advance;
		// an MBDirectory needs to exist so that it can check modtimes
		// and be a valid external result. This seems like the right
		// solution.

		public CopyInstaller (string destdir) 
		{
		    Init (destdir);
		}

		public CopyInstaller () : this (null) {
		}

		public void Init (string destdir) 
		{
			this.DestDir = destdir;
		}

		 
		// Public

		public string DestDir;

		// CompositeResult

		protected override int TotalItems { get { return base.TotalItems + 1; } }

		protected override void CopyItems (Result[] r) {
			r[base.TotalItems] = new MBString (DestDir);
		}

		protected override void CloneTo (Result r) {
			CopyInstaller other = (CopyInstaller) r;

			base.CloneTo (r);
			other.DestDir = DestDir;
		}

		// Implementation

		protected virtual string MutateDestDir (string destdir, MBFile other, IBuildContext ctxt) {
		    return destdir;
		}

		protected virtual string GetDestName (MBFile file, MBDirectory dest, IBuildContext ctxt) {
			// return the basename of the destination file, derived from the arguments
			// in whatever manner is fit.
			return file.Name;
		}

		protected virtual bool PreCopy (MBFile src, MBFile dest, bool backwards, IBuildContext ctxt) {
			// return true on error
			return false;
		}

		protected virtual bool PostCopy (MBFile src, MBFile dest, bool backwards, IBuildContext ctxt) {
			// return true on error
			return false;
		}

		protected MBFile MakeDestination (MBFile src, IBuildContext ctxt)
		{
		    string ddir = MutateDestDir (DestDir, src, ctxt);

		    if (ddir == null) {
			string t = String.Format ("No destination directory for the installation of {0}", src);

			ctxt.Logger.Error (2013, t, String.Format ("Base dest directory \"{0}\"", DestDir));
			return null;
		    }

		    MBDirectory dir = new MBDirectory (ResultStorageKind.System, ddir);
		    return new MBFile (dir, GetDestName (src, dir, ctxt));
		}

		protected bool CopyFile (MBFile src, bool backwards, IBuildContext ctxt) {
		    MBFile dest = MakeDestination (src, ctxt);

		    if (dest == null)
			return true;
		       
		    if (PreCopy (src, dest, backwards, ctxt))
			// Error will be reported
			return true;
		    
		    try {
			if (backwards) {
			    // FIXME: delete containing dirs if empty? probably a bad idea.
			    dest.Delete (ctxt);
			} else {
			    dest.Dir.CreateTo (ctxt);
			    src.CopyTo (dest, ctxt);
			}
		    } catch (IOException ioex) {
			string t1;
			
			if (backwards)
			    t1 = String.Format ("There was an error deleting {0}.", dest.GetPath (ctxt));
			else
			    t1 = String.Format ("There was an error copying {0} to {1}.", 
						src.GetPath (ctxt), dest.GetPath (ctxt));
			
			// Different error # for the delete exception?
			ctxt.Logger.Error (3023, t1, ioex.Message);
			return true;
		    } catch (UnauthorizedAccessException uaex) {
			string t1;
			
			if (backwards)
			    t1 = String.Format ("You do not have permission to delete {0}.", 
						dest.GetPath (ctxt));
			else
			    t1 = String.Format ("You do not have permission to copy {0} to {1}.", 
						src.GetPath (ctxt), dest.GetPath (ctxt));
			
			ctxt.Logger.Error (3023, t1, uaex.Message);
			return true;
		    }
		    
		    if (PostCopy (src, dest, backwards, ctxt)) {
			// Error will be reported but we need to clean up
			
			if (!backwards) {
			    try {
				dest.Delete (ctxt);
			    } catch (IOException ioex) {
				string t1 = String.Format ("There was an error removing {0}.", 
							   dest.GetPath (ctxt));
				
				ctxt.Logger.Error (3022, t1, ioex.ToString ());
			    }
			}
			
			return true;
		    }
		    
		    return false;
		}

		public virtual Type OtherType { get { return typeof (MBFile); } }

		public virtual bool InstallResult (Result other, bool backwards, IBuildContext ctxt) {
			return CopyFile ((MBFile) other, backwards, ctxt);
		}

		public virtual string DescribeAction (Result other, IBuildContext ctxt) {
		    MBFile dest = MakeDestination ((MBFile) other, ctxt);

		    if (dest == null)
			return "[error determining destination]";

		    return String.Format ("Copy {0} to {1}", 
					  ((MBFile) other).GetPath (ctxt), 
					  dest.GetPath (ctxt));
		}
	}
}
