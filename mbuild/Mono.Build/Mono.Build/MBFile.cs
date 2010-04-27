//
// A file result.
//

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

using Mono.Unix;

namespace Mono.Build {

	[Serializable]
	public class MBFile : Result {

		protected MBDirectory dir;
		protected string name;
		protected DateTime modtime;

		protected static DateTime invalid_modtime = new DateTime (1, 1, 1);

		public MBFile () : base () {
		}

		public MBFile (MBDirectory dir, string name) : base () {
			if (dir == null)
				throw new ArgumentNullException ("dir");
			if (name == null)
				throw new ArgumentNullException ("name");

			this.dir = dir;
			this.name = name;
			this.modtime = invalid_modtime;
		}

		public MBDirectory Dir {
			get {
				return dir;
			}

			set {
				if (value == null)
					throw new ArgumentNullException ();

				dir = value;
			}
		}

		public string Name {
			get {
				return name;
			}

			set {
				if (value == null)
					throw new ArgumentNullException ();

				name = value;
			}
		}

		public string GetPath (IBuildContext ctxt) {
			if (dir == null)
				return null;

			string p = ctxt.PathTo (dir);
			return Path.Combine (p, name);
		}

		public void SetFromSystemPath (string path)
		{
		    dir = new MBDirectory (ResultStorageKind.System, 
					   Path.GetDirectoryName (path));
		    name = Path.GetFileName (path);
		}

		public void CreateAsTemporary (IBuildContext ctxt)
		{
		    SetFromSystemPath (Path.GetTempFileName ());
		    ctxt.Logger.Log ("io.file_create_temp", GetPath (ctxt));
		}

		public void SetToInsecureTemporary (string ext, IBuildContext ctxt)
		{
		    dir = ctxt.WorkingDirectory;
		    name = String.Format ("tmp{0}.{1}", new Random ().Next (), ext);
		}

		// IO util -- all IO operations should go through one of
		// these functions, so we can standardize the logging of them.

		public Stream OpenRead (IBuildContext ctxt) {
			string path = GetPath (ctxt);

			return new BufferedStream (new FileStream (path, FileMode.Open, 
								   FileAccess.Read));
		}

		public Stream OpenWrite (IBuildContext ctxt) {
			string path = GetPath (ctxt);

			return new BufferedStream (new FileStream (path, FileMode.Create, 
								   FileAccess.Write));
		}

		public void MoveTo (MBFile dest, IBuildContext ctxt) {
			// FIXME: exception handling
			string src = GetPath (ctxt);
			string dp = dest.GetPath (ctxt);

			ctxt.Logger.Log ("io.file_move", src + " -> " + dp);
			File.Move (src, dp);

			this.Dir = dest.Dir;
			this.Name = dest.Name;
		}

		public void Delete (IBuildContext ctxt) {
			string p = GetPath (ctxt);

			ctxt.Logger.Log ("io.file_delete", p);
			File.Delete (p);
		}

		protected void CopyToUnsafe (string dest, IBuildContext ctxt) {
			string src = GetPath (ctxt);

			ctxt.Logger.Log ("io.file_copy", src + " -> " + dest);
			File.Copy (src, dest, true);
		}

		public void CopyTo (MBDirectory dir, IBuildContext ctxt) {
			string dest = Path.Combine (ctxt.PathTo (dir), Name);

			CopyToUnsafe (dest, ctxt);
		}

		public void CopyTo (MBFile f, IBuildContext ctxt) {
			CopyToUnsafe (f.GetPath (ctxt), ctxt);
		}

		public void LinkFromOrCopyTo (MBFile f, IBuildContext ctxt)
		{
		    // FIXME: like MakeExecutable, probably a hack.
		    string path = GetPath (ctxt);

		    if (!RuntimeEnvironment.MonoUnixSupported) {
			// emulate with copying
			CopyTo (f, ctxt);
			return;
		    }

		    // Try and emulate the copy semantics by obliterating
		    // f if it already exists.

		    string other = f.GetPath (ctxt);

		    try {
			File.Delete (other);
		    } catch (IOException) {
		    }

		    // FIXME: does this create absolute paths always?
		    // that would be highly lame.

		    UnixFileInfo ufi = new UnixFileInfo (path);
		    ctxt.Logger.Log ("io.file_link", other + " -> " + path);
		    ufi.CreateSymbolicLink (other);
		}

		public bool Exists (IBuildContext ctxt) {
			string path = GetPath (ctxt);

			return File.Exists (path);
		}

		public void MakeExecutable (IBuildContext ctxt) {
		    // FIXME: this is kind of a lame hack to
		    // try and abstract this operation.

		    if (!RuntimeEnvironment.MonoUnixSupported)
			// no action needed on MS
			return;

		    string path = GetPath (ctxt);

		    new UnixFileInfo (path).Protection |= Mono.Unix.Native.FilePermissions.S_IXUSR |
			Mono.Unix.Native.FilePermissions.S_IXGRP |
			Mono.Unix.Native.FilePermissions.S_IXOTH;
		}

		protected DateTime GetModTime (IBuildContext ctxt) {
			return File.GetLastWriteTime (GetPath (ctxt));
		}

		// external

		public override bool Check (IBuildContext ctxt) {
			return File.Exists (GetPath (ctxt));
		}

		public override bool Clean (IBuildContext ctxt) {
			if (dir.Storage != ResultStorageKind.Built)
				return false;

			Delete (ctxt);
			return true;
		}

		public override bool DistClone (IBuildContext ctxt) {
			if (!dir.DistClone (ctxt))
				return false;

			string path = Path.Combine (ctxt.DistPath (dir), Name);
			CopyToUnsafe (path, ctxt);
			return true;
		}

		// XML

		protected override void ExportXml (XmlWriter xw) {
			dir.ExportXml (xw, "");

			xw.WriteStartElement ("name");
			xw.WriteString (name);
			xw.WriteEndElement ();
		}

		protected override bool ImportXml (XmlReader xr, IWarningLogger log) {
			bool gotdir = false;
			bool gotname = false;
			int depth = xr.Depth;

			while (xr.Depth >= depth) {
				if (xr.NodeType != XmlNodeType.Element) {
					//Console.WriteLine ("skipping {0}: {1} = \"{2}\"", xr.NodeType, xr.Name, xr.Value);
					xr.Read ();
					continue;
				}

				switch (xr.Name) {
				case "result":
					string ignore;

					Result r = Result.ImportXml (xr, out ignore, log);
					if (r == null)
						return true;
					if (!(r is MBDirectory)) {
						log.Warning (3019, "Result embedded in file result is not directory during XML import", null);
						return true;
					}
					dir = (MBDirectory) r;
					gotdir = true;
					break;
				case "name":
					name = xr.ReadElementString ();
					gotname = true;
					break;
				default:
					log.Warning (3019, "Unknown element in file result during XML import", xr.Name);
					xr.Skip ();
					break;
				}
			}

			if (!gotdir) {
				log.Warning (3019, "Did not find directory in file element during XML import", null);
				return true;
			}

			if (!gotname) {
				log.Warning (3019, "Did not find name in file element during XML import", null);
				return true;
			}
			
			return false;
		}

		// fingerprint

		public override Fingerprint GetFingerprint (IBuildContext ctxt, Fingerprint cached) {
			DateTime ondisk = GetModTime (ctxt);
			Fingerprint result;

			if (cached != null && modtime == ondisk) {
				//string path = GetPath (ctxt);
				//ctxt.Logger.Log ("io.file_fingerCACHE", "XXXXXXXX " + path);
				result = cached;
			} else {
				// Don't use OpenRead: the buffered stream would be useless
				string path = GetPath (ctxt);
				//string s = cached == null ? "nocached " : "modtime  ";
				//ctxt.Logger.Log ("io.file_fingerprint", s + path);
				ctxt.Logger.Log ("io.file_fingerprint", path);
				result = Fingerprint.FromFile (path);
				modtime = ondisk;
			}

			return result;
		}

		// Clone

		protected override void CloneTo (Result dest) {
			MBFile dfile = (MBFile) dest;

			dfile.dir = (MBDirectory) dir.Clone ();
			dfile.name = (string) name.Clone ();
			dfile.modtime = modtime; // cloning?
		}

		// Compare

		protected override bool ContentEquals (Result other)
		{
		    MBFile f = (MBFile) other;

		    return (dir == f.dir) && (name == f.name);
		}

		protected override int InternalHash ()
		{
		    return dir.GetHashCode () ^ name.GetHashCode ();
		}

		// object

		public override string ToString () {
			if (dir == null)
				return System.String.Format ("$nodirectory/{0} ({1})", 
							     name, GetType().Name);

			return System.String.Format ("{0}{1} ({2})", dir, name, GetType().Name);
		}
	}
}
