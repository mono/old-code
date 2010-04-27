//
// A directory result. A little bit of abstraction so that we
// can support srcdir != builddir builds, etc.
//
// Sometimes I think this shouldn't be a result at all, and just 
// a utility class. Not sure.
//

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Mono.Build {

	[Serializable]
	public class MBDirectory : Result {

		protected ResultStorageKind storage;
		protected string subpath;
		protected DateTime modtime;

		protected static DateTime invalid_modtime = new DateTime (1, 1, 1);

		public MBDirectory () : base () {
		}

		public MBDirectory (ResultStorageKind storage, string subpath) : base () {
		    Init (storage, subpath);
		}

		public void Init (ResultStorageKind storage, string subpath)
		{
		    if (subpath == null)
			throw new ArgumentNullException ();

		    this.storage = storage;
		    this.subpath = subpath;
		    this.modtime = invalid_modtime;
		}

		public ResultStorageKind Storage { 
			get { return storage; }
		}

		public string SubPath {
			get {
				return subpath;
			}

			set {
				if (value == null)
					throw new ArgumentNullException ();
				subpath = value;
			}
		}

		// IO operations

		public void CreateTo (IBuildContext ctxt) {
			string path = ctxt.PathTo (this);

			ctxt.Logger.Log ("io.mkdir", path);
			System.IO.Directory.CreateDirectory (path);
		}

		protected DateTime GetModTime (IBuildContext ctxt) {
			return System.IO.Directory.GetLastWriteTime (ctxt.PathTo (this));
		}

		// external result 

		public override bool Check (IBuildContext ctxt) {
			return System.IO.Directory.Exists (ctxt.PathTo (this));
		}

		public override bool Clean (IBuildContext ctxt) {
			if (storage != ResultStorageKind.Built)
				return false;

			// FIXME
			Console.WriteLine ("Delete directory {0}", ctxt.PathTo (this));
			return true;
		}

		public override bool DistClone (IBuildContext ctxt) {
			if (storage != ResultStorageKind.Source)
				return false;

			string path = ctxt.DistPath (this);

			System.IO.Directory.CreateDirectory (path);
			return true;
		}

		// Fingerprinting

		public override Fingerprint GetFingerprint (IBuildContext ctxt, Fingerprint cached) {
			DateTime ondisk = GetModTime (ctxt);
			Fingerprint result;

			if (cached != null && modtime == ondisk)
				result = cached;
			else {
				// FIXME: right way to do this ???? Not sure at all
				result = Fingerprint.FromText (ToString (Storage) + subpath);
			}

			modtime = ondisk;
			return result;
		}

		// Equality

		protected override bool ContentEquals (Result other)
		{
		    MBDirectory d = (MBDirectory) other;

		    return (storage == d.storage) && (subpath == d.subpath);
		}

		protected override int InternalHash ()
		{
		    return storage.GetHashCode () ^ subpath.GetHashCode ();
		}

		// XML

		protected override void ExportXml (XmlWriter xw) {
			xw.WriteStartElement ("directory");
			xw.WriteAttributeString ("storage", ToString (storage));
			xw.WriteAttributeString ("path", subpath);
			xw.WriteEndElement ();
		}

		bool ParseStorageKind (string k, out ResultStorageKind v) {
			switch (k) { 
			case "system":
				v = ResultStorageKind.System;
				return false;
			case "built":
				v = ResultStorageKind.Built;
				return false;
			case "source":
				v =  ResultStorageKind.Source;
				return false;
			default:
				v = ResultStorageKind.System;
				return true;
			}
		}

		protected override bool ImportXml (XmlReader xr, IWarningLogger log) {
			while (xr.NodeType == XmlNodeType.Whitespace) {
				if (!xr.Read ()) {
					log.Warning (3019, "Empty node for directory result", null);
					return true;
				}
			}

			if (xr.Name != "directory") {
				log.Warning (3019, "Expected an element named 'directory' in XML import, " + 
					     "got '" + xr.Name + "'", null);
				return true;
			}

			string s = xr.GetAttribute ("storage");

			if (s == null) {
				log.Warning (3019, "Did not get storage kind for directory element in XML import", null);
				return true;
			}
			
			if (ParseStorageKind (s, out storage))
				return true;

			subpath = xr.GetAttribute ("path");

			if (subpath == null) {
				log.Warning (3019, "Did not get path for directory element in XML import", null);
				return true;
			}

			//xr.ReadEndElement ();
			return false;
		}

		// Clone

		protected override void CloneTo (Result dest) {
			MBDirectory ddest = (MBDirectory) dest;

			ddest.storage = storage;
			ddest.subpath = subpath;
			ddest.modtime = modtime;
		}

		// object

		public static string ToString (ResultStorageKind k) {
			switch (k) {
			case ResultStorageKind.Built:
				return "built";
			case ResultStorageKind.Source:
				return "source";
			case ResultStorageKind.System:
				return "system";
			default:
				return "[unknown ResultStorageKind]";
			}
		}

		public override string ToString () {
			string prefix = "???";

			switch (storage) {
			case ResultStorageKind.Built:
				prefix = "$builddir/";
				break;
			case ResultStorageKind.Source:
				prefix = "$srcdir/";
				break;
			case ResultStorageKind.System:
				prefix = "system:";
				break;
			default:
				throw new InvalidOperationException ();
			}

			string substr = subpath;

			if (substr == ".")
			    substr = "";
			else if (!substr.EndsWith ("/"))
			    substr += '/';

			return String.Format ("{0}{1}", prefix, substr);
		}
	}
}
