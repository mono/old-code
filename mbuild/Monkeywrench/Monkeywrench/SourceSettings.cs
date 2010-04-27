// SourceSettings.cs -- Code for manipulating paths relative to
// the build directory and source directory, and for saving and
// restoring breadcrumbs in build directories so this information
// can be reconstructed.

using System;
using System.IO;
using System.Xml;
using System.Text;

using Mono.Build;

namespace Monkeywrench {

    public class SourceSettings {

	protected SourceSettings () {}

	// Basis-to-subpath manipulations. These are all generic
	// and so can be static.

	static readonly char DSC = Path.DirectorySeparatorChar;
	static readonly string up_one = ".." + DSC;

	public static string BasisToTopDots (string basis) 
	{
	    // Return a string along the lines of '../../..'
	    // that would take us from the subpath corresponding
	    // to basis to the top build or source directory.

	    if (basis.Length == 1)
		return ".";
	    
	    // inefficient, yes. Who cares, not me.
	    
	    string[] temp = basis.Split ('/');
	    StringBuilder sb = new StringBuilder (32);
	    
	    // - 2 to skip leading and trailing slashes
	    for (int i = 0; i < temp.Length - 2; i++)
		sb.Append (up_one);
	    
	    return sb.ToString ();
	}

	public static string SubpathToTopDots (string path) 
	{
	    // Similar to the above, except taking a subpath
	    // instead of a basis as input.

	    if (path == "." || path.Length == 0)
		// Some code treats a subpath of "" as OK.
		return ".";
	    
	    string[] temp = path.Split (DSC);
	    StringBuilder sb = new StringBuilder (32);
	    int count = temp.Length;

	    // make foo/bar and foo/bar/ be equivalent
	    if (path[path.Length - 1] == DSC)
		count--;

	    for (int i = 0; i < count; i++)
		sb.Append (up_one);
	    
	    return sb.ToString ();
	}

	static readonly bool need_repl = Path.DirectorySeparatorChar == '/';

	public static string BasisToSubpath (string basis) 
	{
	    // Convert a basis to a subpath. There does not necessarily
	    // exist an actual directory analogous to every basis:
	    // eg, /config/lang/csharp.

	    if (basis.Length == 1)
		return ".";

	    if (need_repl)
		return basis.Substring (1).Replace ('/', DSC);
	    else
		return basis.Substring (1);
	}

	public static string SubpathToBasis (string subpath) 
	{
	    // Convert a basis to a subpath. There exists a
	    // basis for every directory-only subpath.

	    if (subpath.Length == 0 || subpath == ".")
		return "/";

	    string s;

	    if (need_repl)
		s = '/' + subpath.Replace (DSC, '/');
	    else
		s = '/' + subpath;

	    if (s[s.Length - 1] != '/')
		s += '/';

	    return s;
	}

	public static string RerootBasisRelative (string path, string basis) 
	{
	    // Given a path and a basis, return a path that will get to the former
	    // from the subpath associated with the latter.

	    if (Path.IsPathRooted (path))
		return path;
	    
	    string dots = BasisToTopDots (basis);
	    return Path.Combine (dots, path);
	}
	
	public static string BasisToFileToken (string basis)
	{
	    // If we need to create a bunch of files in the same directory,
	    // one per basis, the returned string will allow uniquification
	    // of the filenames. The leading underscore in _toplevel allows
	    // disambiguation from a basis called /toplevel/.

	    if (basis.Length == 1)
		return "_toplevel_";
	    
	    return basis.Replace (DSC, '_').Substring (1);
	}


	// Sourcedir and builddir manipulations. These depend on the current
	// working directory (assumed to always be a subpath of topbuilddir)
	// and the path to the top source directory.

	// How to get to the top source directory from the top
	// build directory. May be an absolute or relative path.

	string topsrc_from_topbuild;

	public string TopSourcePath {
	    get { return topsrc_from_topbuild; }
	}

	// Path.Combine (pwdtopbuild, subpath) = PWD
	string subpath;

	// Path.Combine (PWD, pwdtopbuild) = top build dir
	string pwdtopbuild;

	// Path.Combine (PWD, pwdtopsrc) = top src dir
	string pwdtopsrc;
	
	public void SetCurrentSubpath (string subpath) 
	{
	    this.subpath = subpath;
	    this.pwdtopbuild = SubpathToTopDots (subpath);
			
	    if (Path.IsPathRooted (topsrc_from_topbuild))
		this.pwdtopsrc = topsrc_from_topbuild;
	    else
		this.pwdtopsrc = PathToBuildRelative (topsrc_from_topbuild);
	    
	    // FIXME TODO: collapse obvious combos of .. and [own directory]
	}

	public string CurrentSubpath { get { return subpath; } }

	public string PathToBuildRelative (string subpath) 
	{
	    // Given some subpath, return a path
	    // pointing from PWD to the builddir version of 
	    // it.

	    return Path.Combine (pwdtopbuild, subpath);
	}

	public string PathToSourceRelative (string subpath) 
	{
	    // As above, s/build/source/

	    return Path.Combine (pwdtopsrc, subpath);
	}
		
	public string PathToBasisBuild (string basis) 
	{
	    // Given a *provider basis*, return a path
	    // pointing to the builddir version if it. This
	    // may not always point to a real directory, if
	    // the provider is not declared in a Buildfile.
	    // (Eg, if it is defined from an 'inside' or
	    // in a bundle).

	    return PathToBuildRelative (BasisToSubpath (basis));
	}

	public string PathToBasisSource (string basis) 
	{
	    // As above, s/build/source

	    return PathToSourceRelative (BasisToSubpath (basis));
	}

	public string PathTo (MBDirectory dir) 
	{
	    switch (dir.Storage) {
	    case ResultStorageKind.System:
		return dir.SubPath;
	    case ResultStorageKind.Built:
		return PathToBuildRelative (dir.SubPath);
	    case ResultStorageKind.Source:
		return PathToSourceRelative (dir.SubPath);
	    default:
		throw new Exception ("Invalid ResultStorageKind");
	    }
	}

	// Distdir-related stuff.

	// Same semantics as topsrc_from_topbuild. Only set on demand.
	string topdist_from_topbuild;

	public void SetDistPath (string distpath)
	{
	    // This takes into account the current subpath to
	    // set topdist_from_topbuild.

	    if (distpath == null)
		throw new ArgumentNullException ();
		
	    if (Path.IsPathRooted (distpath))
		topdist_from_topbuild = distpath;
	    else
		topdist_from_topbuild = Path.Combine (subpath, distpath);
	}

	public string DistPath (MBDirectory dir) 
	{
	    if (dir.Storage != ResultStorageKind.Source)
		throw new Exception ("Don't know how to distpath " + dir.ToString ());
	    
	    string path;
	    
	    if (Path.IsPathRooted (topdist_from_topbuild))
		path = topdist_from_topbuild;
	    else
		path = PathToBuildRelative (topdist_from_topbuild);
	    
	    return Path.Combine (path, dir.SubPath);
	}

	// Another small useful thing.

	public const string StateDir = ".monkeywrench_state";

	public string PathToStateItem (string item)
	{
	    string s = PathToBuildRelative (StateDir);

	    try {
		Directory.CreateDirectory (s);
	    } catch (IOException) {
	    }
		
	    return Path.Combine (s, item);
	}

	// The basenames of the buildfiles in every directory.
	// Could be used to provide some extremely high-level
	// build configuration.

	string buildfile_name;

	public string BuildfileName {
	    get { return buildfile_name; }
	}

	// Saving and loading of the SourceSettings configuration.
	// There are three scenarios:
	// 1) Load a file in PWD
	// 2) Create a file in PWD representing a new topbuilddir
	// 3) Create a file in a subpath of build in case we need
	//    to invoke case 1) from that subpath.
	// We use XML serialization because it is pretty easy and
	// you can cat the .src file and read it.

	public const string SourceFileName = ".monkeywrench.src";

	public static SourceSettings Load (IWarningLogger log) 
	{
	    SourceSettings ss = new SourceSettings ();

	    try {
		XmlTextReader tr = new XmlTextReader (SourceFileName);

		while (!tr.EOF) {
		    if (tr.NodeType != XmlNodeType.Element) {
			tr.Read ();
			continue;
		    }

		    // Yeah this is awesome. Note that topsrc-from-topbuild
		    // must come before subpath-here otherwise nullref.

		    if (tr.Name == "topsrc-from-topbuild")
			ss.topsrc_from_topbuild = tr.ReadElementString ();
		    else if (tr.Name == "buildfile-name")
			ss.buildfile_name = tr.ReadElementString ();
		    else if (tr.Name == "subpath-here" && ss.topsrc_from_topbuild != null)
			ss.SetCurrentSubpath (tr.ReadElementString ());

		    tr.Read ();
		}

		tr.Close ();
	    } catch (Exception e) {
		log.Error (1009, "Cannot load build breadcrumb file " + 
			   SourceFileName, e.Message);
		return null;
	    }

	    if (ss.topsrc_from_topbuild == null ||
		ss.buildfile_name == null ||
		ss.subpath == null) {
		log.Error (1009, "Malformed build breadcrumb file " + SourceFileName, 
			   ss.ToString ());
		return null;
	    }

	    return ss;
	}

	public static SourceSettings CreateToplevel (string topsrc, string bfname, 
						     IWarningLogger log) 
	{
	    if (File.Exists (SourceFileName)) {
		log.Error (1009, "Build breadcrumb file " + SourceFileName + " should " +
			   "not exist here, but it does.", null);
		return null;
	    }

	    SourceSettings ss = new SourceSettings ();
	    ss.topsrc_from_topbuild = topsrc;
	    ss.buildfile_name = bfname;
	    ss.SetCurrentSubpath (".");

	    if (ss.SaveForSubpath (".", log))
		return null;

	    return ss;
	}

	public bool SaveForSubpath (string subpath, IWarningLogger log)
	{
	    try {
		string f = Path.Combine (PathToBuildRelative (subpath), SourceFileName);
		XmlTextWriter tw = new XmlTextWriter (f, Encoding.UTF8);

		tw.Formatting = Formatting.Indented;

		tw.WriteStartElement ("breadcrumb");
		tw.WriteElementString ("topsrc-from-topbuild", topsrc_from_topbuild);
		tw.WriteElementString ("buildfile-name", buildfile_name);
		tw.WriteElementString ("subpath-here", subpath);
		tw.WriteEndElement ();

		tw.Close ();
	    } catch (Exception e) {
		log.Error (1009, "Cannot write build breadcrumb file " + 
			   SourceFileName, e.Message);
		return true;
	    }

	    return false;
	}

	// Object.

	public override string ToString () 
	{
	    return String.Format ("[topsrc {0}, buildfile {1}, cur subpath {2}]",
				  topsrc_from_topbuild, buildfile_name, subpath);
	}
    }
}
