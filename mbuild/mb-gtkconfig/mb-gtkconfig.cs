// mb-gtkconfig.cs -- Simple graphical configuration for
// MBuild projects

using System;
using System.Collections;

using Gtk;
using Glade;

using Mono.GetOptions;

using Mono.Build;
using Monkeywrench;

class GtkLogger : SimpleLogger {

	public GtkLogger () : base () {}

	// FIXME: graphical!

	void WritePrefixy (string text) {
		string[] lines = text.Split ('\n');

		for (int i = 0; i < lines.Length; i++)
			Console.Error.WriteLine ("   {0}", lines[i]);
	}

	protected override void DoWarning (string location, int category, string text, string detail) {
		if (location != null)
			Console.Error.Write ("{0}: ", location);

		Console.Error.WriteLine ("warning {0}: {1}", category, text);

		if (detail != null)
			WritePrefixy (detail);
	}

	protected override void DoError (string location, int category, string text, string detail) {
		if (location != null)
			Console.Error.Write ("{0}: ", location);

		Console.Error.WriteLine ("error {0}: {1}", category, text);

		if (detail != null)
			WritePrefixy (detail);
	}
}

public class MainWindow {
	WrenchProject project;

	Glade.XML ui;

	[Glade.Widget] Window main_window;
	[Glade.Widget] Notebook notebook;
	[Glade.Widget] OptionMenu group_option;

	Menu group_menu;
	Hashtable group_widgets = new Hashtable ();
	Hashtable group_items = new Hashtable ();
	Hashtable timeouts = new Hashtable ();

	const string DefaultGroupName = "Main";

	public MainWindow (WrenchProject project) {
		this.project = project;

		ui = new Glade.XML (null, "mb-gtkconfig.glade", "main_window", null);
                ui.Autoconnect (this);

		main_window.Title = project.Info.Name + " " + 
			project.Info.Version + " - Build Configuration";

		group_menu = new Menu ();
		group_menu.Show ();
		group_option.Menu = group_menu;

		// kill the default page that glade forces us to have
		notebook.RemovePage (0);
		AddNewGroup (DefaultGroupName);
		group_option.SetHistory (0);

		LoadConfigItems ();
 	}

	void LoadConfigItems () {
		OperationFunc f = new OperationFunc (LoadConfigItem);

		if (project.Operate ("/", OperationScope.Everywhere, "config", f)) {
			// FIXME
			Console.Error.WriteLine ("Error loading configuration items!");
		}
	}

	bool LoadConfigItem (WrenchProject proj, BuildServices bs) {
		string prompt, group;
		string target = bs.FullName;
		Result res = bs.GetValue ().Result;

		if (res == null)
			return true;

		Result tag;
		if (bs.GetTag ("prompt", out tag))
			return true;

		MBString rstr = tag as MBString;

		if (rstr == null) {
			// FIXME
			Console.Error.WriteLine ("Configurable option " + target + " does not have a string \'prompt\' tag.");
			prompt = target;
		} else {
			// TODO: i18n
			prompt = rstr.Value;
		}

		if (bs.GetTag ("config_group", out tag))
			return true;

		rstr = tag as MBString;

		if (rstr == null)
			group = DefaultGroupName;
		else {
			// TODO: i18n
			group = rstr.Value;
		}

		Widget widget = null;

		if (res is MBBool)
			widget = MakeBoolItem (bs, prompt, (MBBool) res);
		else if (res is MBString)
			widget = MakeStringItem (bs, prompt, (MBString) res);
		else {
			// FIXME
			Console.Error.WriteLine ("Don't know how to configure the option {0}.", target);
			return true;
		}

		AddGuiItem (group, widget);
		return false;
	}

	static object result_key = "mb-gtkconfig::result";
	static object services_key = "mb-gtkconfig::services";

	Widget MakeBoolItem (BuildServices services, string prompt, MBBool res) {
		CheckButton cb = new CheckButton (prompt);
		cb.Active = res.Value;
		cb.Data[result_key] = res;
		cb.Data[services_key] = services;
		cb.Toggled += new EventHandler (OnBoolToggled);

		return cb;
	}

	Widget MakeStringItem (BuildServices services, string prompt, MBString res) {
		HBox box = new HBox ();
		box.Show ();

		Label label = new Label (prompt);
		label.Show ();
		label.Xalign = 1.0F;

		Entry entry = new Entry (res.Value);
		entry.Show ();
		entry.Data[result_key] = res;
		entry.Data[services_key] = services;
		entry.Changed += new EventHandler (OnStringChanged);

		box.PackStart (label, true, true, 2);
		box.PackStart (entry, false, true, 2);
		return box;
	}

	// UI helpers

	public void Show () {
		main_window.Show ();
	}

	void AddGuiItem (string group, Widget widget) {
		if (!group_widgets.Contains (group))
			AddNewGroup (group);

		Box box = (Box) group_widgets[group];
		box.PackStart (widget, false, false, 2);
		widget.Show ();
	}

	void AddNewGroup (string name) {
		MenuItem mi = new MenuItem (name);
		mi.Show ();
		mi.Activated += new EventHandler (OnGroupItemActivated);

		group_menu.Append (mi);

		ScrolledWindow sw = new ScrolledWindow ();
		sw.Show ();
		sw.HscrollbarPolicy = PolicyType.Automatic;
		sw.VscrollbarPolicy = PolicyType.Automatic;

		VBox box = new VBox ();
		box.Show ();
		box.Homogeneous = false;
		box.Spacing = 2;
		sw.AddWithViewport (box);

		notebook.AppendPage (sw, new Gtk.Label ("not shown"));
		group_widgets[name] = box;
		group_items[mi] = notebook.NPages - 1;
	}

	// Callbacks

	void OnBoolToggled (object sender, EventArgs args) {
		CheckButton cb = (CheckButton) sender;
		BuildServices services = (BuildServices) cb.Data[services_key];
		MBBool val = (MBBool) cb.Data[result_key];

		val.Value = cb.Active;
		Console.WriteLine ("Fixing {0} = {1}", services.FullName, val);
		services.FixValue (val);
	}

	void OnStringChanged (object sender, EventArgs args) {
		Entry entry = (Entry) sender;

		if (timeouts.Contains (entry))
			return;

		// Preserve a ref so our class doesn't get GC'd.
		timeouts[entry] = new StringChangeTimeout (entry, this);
	}

	class StringChangeTimeout {
		Entry entry;
		MainWindow owner;

		public StringChangeTimeout (Entry entry, MainWindow owner) {
			GLib.Timeout.Add (500, new GLib.TimeoutHandler (Timeout));
			this.entry = entry;
			this.owner = owner;
		}

		bool Timeout () {
			owner.OnStringTimeout (entry);
			return false;
		}
	}

	void OnStringTimeout (Entry entry) {
		BuildServices services = (BuildServices) entry.Data[services_key];
		MBString val = (MBString) entry.Data[result_key];
		
		val.Value = entry.Text;
		Console.WriteLine ("Fixing {0} = {1}", services.FullName, val);
		services.FixValue (val);
		timeouts.Remove (entry);
	}

	void OnGroupItemActivated (object sender, EventArgs args) {
		int page = (int) group_items[sender];

		notebook.CurrentPage = page;
	}

	void OnDelete (object o, EventArgs args)
        {
                Application.Quit ();
        }

	void OnClose (object o, EventArgs args)
        {
		OnDelete (o, args);
        }
}

public class MBGtkConfig : Options {

	public MBGtkConfig () : base () {
		//BreakSingleDashManyLettersIntoManyOptions = true;
		//ParsingMode = OptionsParsingMode.Linux;
	}

	public override WhatToDoNext DoAbout () {
		base.DoAbout ();
		return WhatToDoNext.AbandonProgram;
	}

	// App

	GtkLogger log = new GtkLogger ();

	int DoIt () {
		ProjectManager pm = new ProjectManager (log);
		if (pm.Load ())
			return 1;

		MainWindow mw = new MainWindow (pm.Project);
		mw.Show();

		Application.Run ();
		pm.Dispose ();
		return 0;
	}

	// Go, go, go

	public static int Main (string[] args) {
		MBGtkConfig program = new MBGtkConfig ();

		Application.Init ("mb-gtkconfig", ref args);
		program.ProcessArgs (args);
		return program.DoIt ();
	}
}
