using System;
using System.Drawing;
using System.Reflection;
using Gdk;
using Gtk;
using Glade;
using GtkSharp;
using Gnome;
using GConf;
using Monotalk.Browser;

public class DemoApp : Program
{
	private static readonly int versionMajor = 0;
	private static readonly int versionMinor = 2;
	private static readonly string version = versionMajor + "." + versionMinor;

	private ObjectBrowser browser;
	private Glade.XML gXML;

	[Glade.Widget("DemoApp")]
	private App window;
	private string[] args;

	private static GConf.Client gconf = new GConf.Client ();
	private static readonly string gconfPath = "/apps/monotalk/objectbrowser/";

	private void WindowDeleteCallback (object obj, DeleteEventArgs args)
	{
		QuitObjectBrowser ();
		args.RetVal = true;
	}
		
	private void QuitCallback (object o, EventArgs args)
	{
		QuitObjectBrowser ();
	}
		
	private void EditFindMemberCallback (object o, EventArgs args)
	{
		browser.FindMember ();
	}

	private void ViewAllMembersCallback (object o, EventArgs args)
	{
		if (((RadioMenuItem) o).Active)
			browser.Flags &= ~BindingFlags.DeclaredOnly;
	}

	private void ViewDeclaredOnlyCallback (object o, EventArgs args)
	{
		if (((RadioMenuItem) o).Active)
			browser.Flags |= BindingFlags.DeclaredOnly;
	}

	private void ViewPublicCallback (object o, EventArgs args)
	{
		if (((CheckMenuItem) o).Active)
			browser.Flags |= BindingFlags.Public;
		else
			browser.Flags &= ~BindingFlags.Public;
	}

	private void ViewNonPublicCallback (object o, EventArgs args)
	{
		if (((CheckMenuItem) o).Active)
			browser.Flags |= BindingFlags.NonPublic;
		else
			browser.Flags &= ~BindingFlags.NonPublic;
	}

	private void ViewFullTypeNamesCallback (object o, EventArgs args)
	{
		TypeAliases.FullNames = ((CheckMenuItem) o).Active;
		browser.RefreshMemberView ();
		browser.RefreshFindBar ();
	}

	private void ViewNamespacesCallback (object o, EventArgs args)
	{
		browser.Namespaces = ((CheckMenuItem) o).Active;
	}

	private void ViewClassesCallback (object o, EventArgs args)
	{
		browser.Classes = ((CheckMenuItem) o).Active;
	}

	private void ViewInterfacesCallback (object o, EventArgs args)
	{
		browser.Interfaces = ((CheckMenuItem) o).Active;
	}

	private void ViewEnumsCallback (object o, EventArgs args)
	{
		browser.Enums = ((CheckMenuItem) o).Active;
	}

	private void AboutCallback (object o, EventArgs args)
	{
		Pixbuf logo = new Pixbuf (".." + System.IO.Path.DirectorySeparatorChar + "art" + System.IO.Path.DirectorySeparatorChar + "about.png");
		String[] authors = new string[] {
			"Petr Danecek (petr@ucl.cas.cz)",
			"Radek Doulik (rodo@matfyz.cz)"
		};
		string[] documentors = new string[] {};

		About about = new About ("Object Browser", version,
					 "Copyright (C) 2002, 2003 Monotalk team",
					 "Monotalk Object Browser",
					 authors, documentors, "", logo);
		about.Show ();
	}

	private void QuitObjectBrowser ()
	{
		browser.Save ();
		Save ();
		Application.Quit ();
	}

	private void Save ()
	{
		gconf.Set (gconfPath + "width", window.Allocation.width);
		gconf.Set (gconfPath + "height", window.Allocation.height);
		gconf.SuggestSync ();
	}

	private void Load ()
	{
		window.Resize ((int) gconf.Get (gconfPath + "width"), (int) gconf.Get (gconfPath + "height"));
	}

	private DemoApp (string[] args, params object[] props) : base ("ObjectBrowserDemo", version, Modules.UI, args, props)
	{
		gXML = new Glade.XML (null, "demoapp.glade", "DemoApp", null);
		gXML.Autoconnect (this);
		
		browser = new ObjectBrowser ();
		this.args = args;

		window = (App) gXML.GetWidget ("DemoApp");
		Load ();

		((MenuItem) gXML.GetWidget ("HelpMenu")).RightJustified = true;

		window.Contents = browser;
		window.ShowAll ();
		browser.AppBar = (AppBar) gXML.GetWidget ("AppBar");;
	}

	private int LoadAssembly (string name)
	{
		Assembly a = null;

		try {
			a = Assembly.LoadFrom (name);
		} catch {
			try {
				a = Assembly.Load (name);
			} catch {
				Console.WriteLine ("Cannot load assembly: {0}", name);
				return 0;
			}
		}
		browser.Add (a);

		return 1;
	}

	private bool Load (string[] args)
	{
		int n = 0;

		if (args.Length > 0) {
			int i;
			for (i = 0; i < args.Length; i ++) {
				/* browser.ParseFile (args [i]);
				   browser.AppBar.ProgressPercentage = (float) i / (args.Length - 1);
				   while (GLib.MainContext.Iteration ());
				*/
				n += LoadAssembly (args [i]);
			}
			if (args.Length > 1)
				browser.AppBar.SetStatus (String.Format ("{0} type(s) and {1} member(s) loaded from supplied source code.",
									 browser.indexer.db.Types, browser.indexer.db.Members));
		} else {
			browser.Add (Assembly.Load ("corlib"));
			n ++;
		}

		window.Title = "Object Browser: [" + n + " assemblies]";

		return true;
	}

	public bool LoadIdle ()
	{
		if (!Load (args))
			Quit ();

		return false;
	}

	public void AddLoadIdle ()
	{
		GLib.Idle.Add (new GLib.IdleHandler (LoadIdle));
	}

	public static int Main (string[] args)
	{
		DemoApp app = new DemoApp (args);

		app.AddLoadIdle ();
		app.Run ();

		return 0;
	}
}
