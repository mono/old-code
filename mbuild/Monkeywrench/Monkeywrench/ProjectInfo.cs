// Info.cs -- project-wide information table

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;

using Mono.Build;

using Monkeywrench.Compiler;

namespace Monkeywrench {

    [Serializable]
	public class ProjectInfo {

		public ProjectInfo () { 
		}

		string name;

		public string Name { 
			get { return name; }
			
			set {
				if (value == null)
					throw new ArgumentNullException ();
				name = value;
			}
		}

		string version;

		public string Version { 
			get { return version; }
			
			set {
				if (value == null)
					throw new ArgumentNullException ();
				version = value;
			}
		}

		string compat_code;

		public string CompatCode { 
			get { return compat_code; }
			
			set {
				if (value == null)
					throw new ArgumentNullException ();
				compat_code = value;
			}
		}

		string buildfile_name;

		public string BuildfileName {
			get { return buildfile_name; }
			
			set {
				if (value == null)
					throw new ArgumentNullException ();
				buildfile_name = value;
			}
		}

		ArrayList refs = new ArrayList ();

		public void AddRef (AssemblyName name) {
			refs.Add (name);
		}

		public IList Refs { get { return refs; } }

		ArrayList private_refs = new ArrayList ();

		public void AddPrivateRef (string name) {
			private_refs.Add (name);
		}

		public IList PrivateRefs { get { return private_refs; } }

		public bool LoadBundles (BundleManager bm, IWarningLogger log)
		{
		    foreach (System.Reflection.AssemblyName aname in Refs) {
			if (bm.LoadBundle (aname, log))
			    return true;
		    }

		    return false;
		}
	}
}
