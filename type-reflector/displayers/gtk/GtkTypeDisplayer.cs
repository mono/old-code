//
// GtkTypeDisplayer.cs: 
//   Display types using Gtk#.
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002-2004 Jonathan Pryor
//

// #define TRACE
#define SHOW_ICONS

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Mono.TypeReflector.Formatters;
using Mono.TypeReflector.Finders;

// for GUI support
using Glade;
using Gtk;
using Pixbuf = Gdk.Pixbuf;
using System.Drawing;

namespace Mono.TypeReflector.Displayers.gtk
{
	delegate void OpenFileSuccess (string[] filenames);

	class OpenFileSelection : FileSelection
	{
		private OpenFileSuccess onSuccess;

		public OpenFileSelection (OpenFileSuccess success)
      : base ("Open an Assembly")
		{
			onSuccess = success;
			DeleteEvent += new DeleteEventHandler (delete_event);
			CancelButton.Clicked += new EventHandler (cancel_event);
			OkButton.Clicked += new EventHandler (ok_event);

			// See ok_event; using Selections property (needed to access multiple
			// selections) doesn't work yet.
			// SelectMultiple = true;
		}

		private void delete_event (object o, DeleteEventArgs args)
		{
			cancel_event (o, args);
		}

		private void cancel_event (object o, EventArgs args)
		{
			Hide ();
		}

		private void ok_event (object o, EventArgs args)
		{
			Hide ();
			// Fails; causes NullReferenceException in get_Selections.
			// onSuccess (Selections);
			onSuccess (new string[]{Filename});
		}
	}

	public class GtkTypeDisplayer : TypeDisplayer
	{
		private TreeStore memberStore;
		private TreeStore optionStore;
		private TreeStore formatterStore;

		private const string dummyText = "dummy: You Shouldn't See This!";

		private static TargetEntry[] accept_table = new TargetEntry[]{
			new TargetEntry ("text/uri-list", 0, 0),
		};

		[Widget ("members")]
		private TreeView treeView;

		[Widget ("options")]
		private TreeView options;

		[Widget ("formatter")]
		private TreeView formatter;

		[Widget("main_window")]
		private Window mainWindow;

		[Widget("toolbar_handlebox")]
		private HandleBox toolbar;

		[Widget("status_bar")]
		private Widget statusbar;

		[Widget("message_entry")]
		private Gtk.Entry messageEntry;

		[Widget("progress_bar")]
		private ProgressBar progressBar;

		[Widget("view_formatter")]
		private MenuItem viewFormatter;
		
		[Widget ("reflector_menu")]
		private MenuItem reflector_menu;

		[Widget("view_finder")]
		private MenuItem viewFinder;

		[Widget("file_close")]
		private MenuItem close;

		[Widget("window_menu")]
		private MenuItem window;

		[Widget("window_fullscreen")]
		private CheckMenuItem window_fullscreen;

		[Widget("options_pane")]
		private VPaned optionsPane;

		[Widget("window_maximize")]
		private CheckMenuItem window_maximize;

		private int totalTypes;

		private static AppWindowManager appWindows = new AppWindowManager ("_Reflector");
		private AppWindowInfo appWindowInfo;

		private static TraceSwitch info = 
			new TraceSwitch ("gtk-displayer", "Gtk# Displayer messages");

		private class RadioMenuItemInfo
		{
			public RadioMenuItem menuItem;
			public string key;

			public RadioMenuItemInfo (RadioMenuItem item, string key)
			{
				menuItem = item;
				this.key = key;
			}
		}

		// list<RadioMenuItemInfo>
		private IList formatterInfo = new ArrayList(3);
		private IList finderInfo = new ArrayList(2);

		public override int MaxDepth {
			set {/* ignore */}
		}

		public override bool AssembliesRequired {
			get {return false;}
		}

		static GtkTypeDisplayer ()
		{
			Application.Init ();
		}

		public GtkTypeDisplayer ()
		{
		}

		public override void InitializeInterface ()
		{
			Glade.XML gxml = new Glade.XML (null, "type-reflector.glade", "main_window", null);
			try {
				gxml.Autoconnect (this);
			} catch (Exception e) {
				Trace.WriteLineIf (info.TraceError, 
					"Error with glade: " + e.ToString());
				throw;
			}

			memberStore = new TreeStore (typeof (TreeCell));
			treeView.Model = memberStore;

			treeView.RowExpanded += new RowExpandedHandler (OnRowExpanded);

			TreeViewColumn column = new TreeViewColumn ();

			column.Title = "Type Information";

#if SHOW_ICONS
			CellRenderer image = new CellRendererPixbuf ();
			column.PackStart (image, false);
			// column.AddAttribute (image, "pixbuf", 2);
			column.SetCellDataFunc (image, new Gtk.TreeCellDataFunc (OnRenderCellIcon));
#endif

			CellRenderer text = new CellRendererText ();
			column.PackStart (text, true);
			column.SetCellDataFunc (text, new Gtk.TreeCellDataFunc (OnRenderCellText));

			treeView.AppendColumn (column);

			CreateOptions ();
			CreateFormatters ();
			CreateFinders ();

			// mark certain menu entries as disabled
			string[] menus = {
				"button_preferences",
				"reflector_preferences", 
				"file_close", "file_save", "file_save_as",
				"edit_copy", 
				// "edit_find",
				"edit_find_panel", "edit_find_next", "edit_find_prev", 
				"edit_find_selection",
				// "window_new", "window_next", "window_prev", 
				// "window_all_to_front",
				"help_manual"
			};
			foreach (string m in menus) {
				Widget menu = (Widget) gxml[m];
				menu.Sensitive = false;
			}

			appWindowInfo = new AppWindowInfo ();
			appWindowInfo.AppWindow = mainWindow;
			appWindowInfo.AppMenu = reflector_menu;
			appWindowInfo.WindowMenu = window;
			appWindowInfo.FullscreenMenu = window_fullscreen;
			appWindowInfo.MaximizeMenu = window_maximize;

			appWindows.Add (appWindowInfo);

			Drag.DestSet (mainWindow, DestDefaults.All, accept_table, Gdk.DragAction.Copy);
			mainWindow.DragDataReceived += new DragDataReceivedHandler (OnDropFiles);

			mainWindow.ShowAll ();

			// Allow the interface to be fully rendered.  If we don't do this, and
			// we do (e.g.) --load-default-assemblies, then the UI will be
			// incompletely rendered until AddType is called, which could be awhile.
			while (GLib.MainContext.Iteration())
				;
		}

		private class TreeCell
		{
			private string message;
			private NodeInfo node;
			
			public TreeCell (string message)
			{
				this.message = message;
			}

			public TreeCell (NodeInfo node)
			{
				this.node = node;
			}

			public string Message {
				get {return message;}
			}

			public NodeInfo Node {
				get {return node;}
			}

			private static readonly Pixbuf icon_transparent = new Pixbuf (null, "transparent.png");
			private static readonly Pixbuf icon_class       = new Pixbuf (null, "class.png");
			private static readonly Pixbuf icon_sealed      = new Pixbuf (null, "sealed.png");
			private static readonly Pixbuf icon_abstract    = new Pixbuf (null, "abstract.png");
			private static readonly Pixbuf icon_enum        = new Pixbuf (null, "enum.png");
			private static readonly Pixbuf icon_interface   = new Pixbuf (null, "interface.png");
			private static readonly Pixbuf icon_method      = new Pixbuf (null, "method.png");
			private static readonly Pixbuf icon_constructor = new Pixbuf (null, "constructor.png");
			private static readonly Pixbuf icon_event       = new Pixbuf (null, "event.png");
			private static readonly Pixbuf icon_field       = new Pixbuf (null, "field.png");
			private static readonly Pixbuf icon_prop_ro     = new Pixbuf (null, "prop-read-only.png");
			private static readonly Pixbuf icon_prop_wo     = new Pixbuf (null, "prop-write-only.png");
			private static readonly Pixbuf icon_prop_rw     = new Pixbuf (null, "prop-read-write.png");

			public Pixbuf Icon {
				get {
					if (Message != null)
						return icon_transparent;

					switch (Node.NodeType) {
						case NodeTypes.Assembly:    return icon_transparent;
						case NodeTypes.Library:     return icon_transparent;
						case NodeTypes.Namespace:   return icon_transparent;
						case NodeTypes.Module:      return icon_transparent;
						case NodeTypes.Type:        return icon_class;
						case NodeTypes.BaseType:    return icon_class;
						case NodeTypes.Interface:   return icon_interface;
						case NodeTypes.Field:       return icon_field;
						case NodeTypes.Constructor: return icon_constructor;
						case NodeTypes.Method:      return icon_method;
						case NodeTypes.Parameter:   return icon_transparent;
						case NodeTypes.Event:       return icon_event;
						case NodeTypes.ReturnValue: return icon_transparent;
						case NodeTypes.Alias:       return icon_transparent;
						case NodeTypes.Other:       return icon_transparent;
						case NodeTypes.CustomAttributeProvider: return icon_transparent;
						case NodeTypes.Property: {
							PropertyInfo pi = (PropertyInfo) Node.ReflectionObject;
							if (pi.CanRead && pi.CanWrite)
								return icon_prop_rw;
							if (pi.CanRead)
								return icon_prop_ro;
							if (pi.CanWrite)
								return icon_prop_wo;
							break;
						}
						default: 
							Trace.WriteLineIf (info.TraceInfo, "Unknown node type!"); break;
					}
					return icon_transparent;
				}
			}
		}

		private class Option
		{
			private string description;

			internal Option (string desc)
			{
				description = desc;
			}

			public string Description {
				get {return description;}
			}
		}

		private class BindingFlagsOption : Option
		{
			private BindingFlags flag;

			internal BindingFlagsOption (string desc, BindingFlags flag)
				: base (desc)
			{
				this.flag = flag;
			}

			public BindingFlags BindingFlag {
				get {return flag;}
			}
		}

		private class FindMemberTypesOption : Option
		{
			private FindMemberTypes type;

			internal FindMemberTypesOption (string desc, FindMemberTypes type)
				: base (desc)
			{
				this.type = type;
			}

			public FindMemberTypes MemberType {
				get {return type;}
			}
		}

		private static readonly BindingFlagsOption[] _bindingOptions = new BindingFlagsOption[] {
			new BindingFlagsOption ("Declared Only", BindingFlags.DeclaredOnly),
			new BindingFlagsOption ("Public", BindingFlags.Public),
			new BindingFlagsOption ("Instance", BindingFlags.Instance),
			new BindingFlagsOption ("Static", BindingFlags.Static),
			new BindingFlagsOption ("Inherited", BindingFlags.FlattenHierarchy),
			new BindingFlagsOption ("Non Public", BindingFlags.NonPublic)
		};

		private static readonly FindMemberTypesOption[] _memberOptions = new FindMemberTypesOption[] {
			new FindMemberTypesOption ("Base Class", FindMemberTypes.Base),
			new FindMemberTypesOption ("Interfaces", FindMemberTypes.Interfaces),
			new FindMemberTypesOption ("Fields", FindMemberTypes.Fields),
			new FindMemberTypesOption ("Constructors", FindMemberTypes.Constructors),
			new FindMemberTypesOption ("Methods", FindMemberTypes.Methods),
			new FindMemberTypesOption ("Properties", FindMemberTypes.Properties),
			new FindMemberTypesOption ("Events", FindMemberTypes.Events),
			new FindMemberTypesOption ("Type Properties", FindMemberTypes.TypeProperties),
			new FindMemberTypesOption ("Verbose Output", FindMemberTypes.VerboseOutput),
			new FindMemberTypesOption ("Mono \"Broken\"", FindMemberTypes.MonoBroken)
		};

		private enum OptionTypes {
			BindingFlag,
			FindMemberTypes
		}

		private void CreateOptions ()
		{
			optionStore = new TreeStore (
				typeof(bool), 	// checked?
				typeof(String), // display string
				typeof(int),   // OptionTypes
				typeof(int),   // OptionTypes-specific mask value
				typeof(bool));  // visible?
			options.Model = optionStore;

			TreeIter binding = optionStore.AppendValues (false, "Binding", 0, 0, false);
			TreeIter types = optionStore.AppendValues (false, "Members", 0, 0, false);

			foreach (BindingFlagsOption bfo in _bindingOptions)
				optionStore.AppendValues (
					binding,
					(Finder.BindingFlags & bfo.BindingFlag) != 0,
					bfo.Description,
					(int) OptionTypes.BindingFlag,
					(int) bfo.BindingFlag,
					true);

			foreach (FindMemberTypesOption fmto in _memberOptions)
				optionStore.AppendValues (
					types,
					(Finder.FindMembers & fmto.MemberType) != 0,
					fmto.Description,
					(int) OptionTypes.FindMemberTypes,
					(int) fmto.MemberType, 
					true);

			TreeViewColumn description = new TreeViewColumn ();
			CellRenderer dr = new CellRendererText ();
			description.PackStart (dr, true);
			description.AddAttribute (dr, "text", 1);
			options.AppendColumn (description);

			TreeViewColumn selected = new TreeViewColumn ();
			CellRendererToggle tr = new CellRendererToggle ();
			tr.Toggled += new ToggledHandler (OnOptionToggled);
			selected.PackStart (tr, true);
			selected.AddAttribute (tr, "active", 0);
			selected.AddAttribute (tr, "visible", 4);
			options.AppendColumn (selected);

			options.ExpandAll ();
		}

		private void CreateFormatters ()
		{
			formatterStore = new TreeStore (
				typeof(bool),     // selected item?
				typeof(string),   // formatter to display
				typeof(string));  // formatter "key", used to select new Formatters.
			formatter.Model = formatterStore;

			CreateEntriesFromFactory (viewFormatter, 
				formatterStore, 
				new EventHandler (OnFormatterChanged),
				Factories.Formatter,
				formatterInfo);

			TreeViewColumn selected = new TreeViewColumn ();
			CellRendererToggle tr = new CellRendererToggle ();
			tr.Radio = true;
			tr.Toggled += new ToggledHandler (OnFormatterToggled);
			selected.PackStart (tr, true);
			selected.AddAttribute (tr, "active", 0);
			formatter.AppendColumn (selected);

			TreeViewColumn languages = new TreeViewColumn ();
			CellRenderer lr = new CellRendererText ();
			languages.PackStart (lr, true);
			languages.AddAttribute (lr, "text", 1);
			formatter.AppendColumn (languages);

			SetFormatterView (Formatter.FactoryKey);
			SetFormatterMenu (Formatter.FactoryKey);
		}

		private static void CreateEntriesFromFactory (MenuItem menu, TreeStore store, EventHandler handler, TypeFactory factory, IList menuEntries)
		{
			GLib.SList group = new GLib.SList (IntPtr.Zero);
			Menu submenu = new Menu ();

			foreach (DictionaryEntry de in factory) {
				TypeFactoryEntry entry = (TypeFactoryEntry) de.Value;
				if (store != null)
					store.AppendValues (false, entry.Description, entry.Key);
				RadioMenuItem item = new RadioMenuItem (group, entry.Description);
				item.Activated += handler;
				group = item.Group;
				submenu.Append (item);
				menuEntries.Add (new RadioMenuItemInfo (item, entry.Key));
			}

			menu.Submenu = submenu;
			menu.ShowAll ();
		}

		private void CreateFinders ()
		{
			CreateEntriesFromFactory (
				viewFinder, 
				null, 
				new EventHandler (OnFinderChanged),
				Factories.Finder,
				finderInfo);
		}

		private void OnRowExpanded (object o, RowExpandedArgs args)
		{
			TreeIter child;
			if (memberStore.IterNthChild (out child, args.Iter, 0)) {
				TreeCell cell = (TreeCell) memberStore.GetValue (child, 0);

				if (cell.Message == dummyText) {
					// Find parent node
					TreeIter piter;
					memberStore.IterParent (out piter, child);
					NodeInfo parent = ((TreeCell) memberStore.GetValue (piter, 0)).Node;

					// remove dummy value
					memberStore.Remove (ref child);

					// Insert children
					if (parent != null) {
						AddChildren (parent, args.Iter);

						// For reasons I can't fathom, if nodes are added the `args.Iter'
						// row isn't expanded until the 2nd click.  So first-click
						// populates the children, 2nd-click shows them.
						//
						// This is annoying.
						//
						// Explicitly expand the row.
						treeView.ExpandRow (args.Path, false);
					}
				}
			}
		}

		private void OnRenderCellIcon (Gtk.TreeViewColumn tree_column, 
			Gtk.CellRenderer cell, Gtk.TreeModel tree_model, Gtk.TreeIter iter)
		{
#if SHOW_ICONS
			TreeCell tc = (TreeCell) memberStore.GetValue (iter, 0);
			((CellRendererPixbuf) cell).Pixbuf = tc.Icon;
#endif
		}

		private void OnRenderCellText (Gtk.TreeViewColumn tree_column, 
			Gtk.CellRenderer cell, Gtk.TreeModel tree_model, Gtk.TreeIter iter)
		{
			TreeCell tc = (TreeCell) memberStore.GetValue (iter, 0);
			((CellRendererText) cell).Text = 
				tc.Message != null 
					? tc.Message
					: Formatter.GetDescription (tc.Node);
		}

		private void AddChildren (NodeInfo r, TreeIter parent)
		{
			foreach (NodeInfo child in Finder.GetChildren (r)) {
				TreeIter i = memberStore.Append (parent);
				memberStore.SetValue (i, 0, new TreeCell (child));
				TreeIter j = memberStore.Append (i);
				memberStore.SetValue (j, 0, new TreeCell (dummyText));
			}
		}

		public override void AddType (Type type, int curType, int totalTypes)
		{
			base.AddType (type, curType, totalTypes);
			this.totalTypes = totalTypes;
			progressBar.Fraction = (double) curType / (double) totalTypes;
			if (curType % 10 == 0)
				messageEntry.Text = CreateTypesLoadedMessage (curType);

			while (GLib.MainContext.Iteration())
				;
		}

		private static string CreateTypesLoadedMessage (int ntypes)
		{
			return string.Format ("{0} type(s) loaded", ntypes);
		}

		public override void Run ()
		{
			ShowTypes ();

			Application.Run ();
		}

		private void ShowTypes ()
		{
			messageEntry.Text = CreateTypesLoadedMessage (totalTypes);

			string assemblyName = null;
			foreach (Assembly a in Assemblies) {
				TreeIter ai;
				memberStore.Append (out ai);
				memberStore.SetValue (ai, 0, new TreeCell (
						string.Format ("Assembly: {0}", a.FullName)));

				if (assemblyName == null)
					assemblyName = a.GetName().Name;

				foreach (string ns in Namespaces (a)) {
					TreeIter ni = memberStore.Append (ai);
					memberStore.SetValue (ni, 0, new TreeCell (
						string.Format ("Namespace: {0}", ns)));

					foreach (Type type in Types (a, ns))
						AddType (type, ni);
				}
			}

			string title = "";
			switch (Assemblies.Count) {
			case 0:
				title = "No Assemblies Loaded";
				close.Sensitive = false;
				break;
			case 1:
				title = assemblyName;
				close.Sensitive = true;
				break;
			default:
				title = string.Format ("{0}...", assemblyName);
				close.Sensitive = true;
				break;
			}
			mainWindow.Title = string.Format ("{0} - Type Reflector", title);
			appWindows.SetTitle (appWindowInfo, title);
		}
		
		private void AddType (Type type, TreeIter parent)
		{
			TreeIter p = memberStore.Append (parent);
			NodeInfo r = new NodeInfo (null, type);
			memberStore.SetValue (p, 0, new TreeCell (r));

			TreeIter d = memberStore.Append (p);
			memberStore.SetValue (d, 0, new TreeCell (dummyText));
		}

		private void OnOptionToggled (object sender, ToggledArgs e)
		{
			TreeIter option;
			if (optionStore.GetIter (out option, new TreePath (e.Path))) {
				bool cur = (bool) optionStore.GetValue (option, 0);
				optionStore.SetValue (option, 0, !cur);

				OptionTypes ot = (OptionTypes) (int) optionStore.GetValue (option, 2);
				switch (ot) {
					case OptionTypes.BindingFlag:
						BindingFlags bf = (BindingFlags) (int) optionStore.GetValue (option, 3);
						if (!cur)
							Finder.BindingFlags |= bf;
						else
							Finder.BindingFlags &= ~bf;
						break;
					case OptionTypes.FindMemberTypes:
						FindMemberTypes fmt = (FindMemberTypes) (int) optionStore.GetValue (option, 3);
						if (!cur)
							Finder.FindMembers |= fmt;
						else
							Finder.FindMembers &= ~fmt;
						break;
				}
			}
		}

		private class FormatterToggler
		{
			private string active;

			public FormatterToggler (string active)
			{
				this.active = active;
			}

			public bool Foreach (Gtk.TreeModel model, Gtk.TreePath path, Gtk.TreeIter iter)
			{
				TreeStore store = (TreeStore) model;
				string desc = (string) store.GetValue (iter, 2);
				store.SetValue (iter, 0, desc == active);
				return false;
			}
		}

		private void OnFormatterChanged (object sender, EventArgs e)
		{
			RadioMenuItem item = (RadioMenuItem) sender;
			string key = null;
			foreach (RadioMenuItemInfo ri in formatterInfo)
				if (object.ReferenceEquals (ri.menuItem, sender)) {
					key = ri.key;
					break;
				}
			if (key != null) {
				SetFormatterView (key);
				SetNewFormatter (key);
			}
		}

		private void OnFinderChanged (object sender, EventArgs e)
		{
			Console.WriteLine ("Finder Changed");
		}

		private void OnFormatterToggled (object sender, ToggledArgs e)
		{
			TreeIter lang;
			if (formatterStore.GetIter (out lang, new TreePath (e.Path))) {
				// change the language...
				string key = (string) formatterStore.GetValue (lang, 2);

				SetFormatterView (key);
				SetFormatterMenu (key);
				SetNewFormatter (key);
			}
		}

		private void SetFormatterView (string newLanguage)
		{
			formatterStore.Foreach (new TreeModelForeachFunc (
					new FormatterToggler (newLanguage).Foreach));
		}

		private void SetFormatterMenu (string key)
		{
			RadioMenuItem item = null;
			foreach (RadioMenuItemInfo ri in formatterInfo)
				if (ri.key == key) {
					item = ri.menuItem;
					break;
				}
			if (item != null)
				item.Active = true;
		}

		//
		// Gtk#/Glade# Required Functions...
		//

		public void app_file_open (object o, EventArgs args)
		{
			OpenFileSelection ofd = new OpenFileSelection (new OpenFileSuccess (OpenAssembly));
      ofd.Run ();
			ofd.Destroy ();
		}

		private void OnDropFiles (object o, DragDataReceivedArgs args)
		{
			string data = System.Text.Encoding.UTF8.GetString (args.SelectionData.Data);
			string[] urls = Regex.Split (data, "\r\n");
			int count = 0;
			foreach (string s in urls)
				if (s != null && s.Length > 0) ++count;
			string[] files = new string[count];
			count = 0;
			foreach (string s in urls)
				if (s != null && s.Length > 0) files[count++] = s;

			OpenAssembly (files);

			Drag.Finish (args.Context, true, false, args.Time);
		}

		private void OpenAssembly (string[] assemblies)
		{
			try {
				foreach (string s in assemblies)
					Trace.WriteLineIf (info.TraceInfo, string.Format ("loading assembly: {0}", s));

				GtkTypeDisplayer d = null;
				if (base.Assemblies.Count == 0)
					d = this;
				else
					d = CreateDisplayer ();

				TypeLoader tl = TypeReflectorApp.CreateLoader (Options);
				tl.Assemblies = assemblies;

				TypeReflectorApp.FindTypes (d, tl, new string[]{"."});

				d.ShowTypes ();
			}
			catch (Exception e) {
				Trace.WriteLineIf (info.TraceError, 
						string.Format ("Error opening assembly: {0}", e.ToString()));
			}
		}

		private GtkTypeDisplayer CreateDisplayer ()
		{
			GtkTypeDisplayer d = new GtkTypeDisplayer ();
			d.Finder = (INodeFinder) Finder.Clone ();
			d.Formatter = (INodeFormatter) Formatter.Clone ();
			d.Options = Options;
			d.InitializeInterface ();
			return d;
		}

		public override void ShowError (string message)
		{
			MessageDialog m = new MessageDialog (
				mainWindow,
				DialogFlags.Modal | DialogFlags.DestroyWithParent, /* Modal, DestroyWithParent, NoSeparator */
				MessageType.Error, 
				ButtonsType.Ok, 
				message);
			m.Run ();
			m.Destroy ();
		}

		public void app_file_save (object o, EventArgs args)
		{
			/* ignore */
		}

		public void app_file_save_as (object o, EventArgs args)
		{
			/* ignore */
		}

		public void app_file_close (object o, EventArgs args)
		{
			memberStore.Clear ();
			treeView.QueueDraw ();
			base.Clear ();
			close.Sensitive = false;
			mainWindow.Destroy ();
		}

		private void RemoveWidgetFromTreeView (Widget o)
		{
			treeView.Remove (o);
		}

		public void app_copy_selection (object o, EventArgs args)
		{
			/* ignore */
		}

		private void SetNewFormatter (string formatter)
		{
			INodeFormatter f = TypeReflectorApp.CreateFormatter (formatter, Options);
			if (f != null) {
				Formatter = f;
				// Cause TreeView to refresh
				treeView.QueueDraw ();
			}
		}

		public void app_toggle_status_bar (object o, EventArgs args)
		{
			if (statusbar.Visible)
				statusbar.HideAll ();
			else
				statusbar.ShowAll ();
		}

		public void app_toggle_toolbar (object o, EventArgs args)
		{
			if (toolbar.Visible)
				toolbar.HideAll ();
			else
				toolbar.ShowAll ();
		}

		public void app_toggle_options_pane (object o, EventArgs args)
		{
			if (optionsPane.Visible)
				optionsPane.HideAll ();
			else
				optionsPane.ShowAll ();
		}

		public void app_show_about (object o, EventArgs args)
		{
			// We don't have a custom logo yet; use nothing.
			Gdk.Pixbuf logo = new Gdk.Pixbuf (Gdk.Colorspace.Rgb, false, 8, 0, 0);

			string[] authors = {"Jonathan Pryor (jonpryor@vt.edu)"};
			string[] documentors = {};

			Gnome.About a = new Gnome.About ("Type Reflector", 
				TypeReflectorApp.Version,
				"Copyright (C) 2002, 2003 Jonathan Pryor",
				"Mono Type Reflector",
				authors, documentors, "", logo);
			a.Show ();
		}

		public void app_show_preferences (object o, EventArgs args)
		{
			ShowError ("Preferences have not been implemented");
		}

		public void app_quit (object o, EventArgs args)
		{
			Application.Quit ();
		}

		public void app_show_manual (object o, EventArgs args)
		{
			ShowError ("A manual needs to be written.");
		}

		public void app_show_find_panel (object o, EventArgs e)
		{
		}

		public void app_find_next (object o, EventArgs e)
		{
		}

		public void app_find_prev (object o, EventArgs e)
		{
		}

		public void app_find_selection (object o, EventArgs e)
		{
		}

		public void app_scroll_to_selection (object o, EventArgs e)
		{
			// TODO: WTF is wrong?
			// ScrollToCell is documented as taking a null for either tp or tvc, but
			// when I pass null for tpc I get a NullRefException.  If I leave tvc
			// alone (don't change after GetCursor), I get an error message on the
			// console about tvc being invalid (or something).  Plus, it scrolls to
			// a stupid location -- always right aligned, when i want left-aligned.
			TreePath tp = new TreePath ();
			TreeViewColumn tvc = new TreeViewColumn ();
			treeView.GetCursor (out tp, out tvc);
			treeView.ScrollToCell (tp, tvc, true, 0.5F, 0.0F);
		}

		public void app_view_expand_all (object o, EventArgs e)
		{
			treeView.ExpandAll ();
		}

		public void app_view_collapse_all (object o, EventArgs e)
		{
			treeView.CollapseAll ();
		}

		public void app_window_new (object o, EventArgs args)
		{
			GtkTypeDisplayer d = CreateDisplayer ();
			d.ShowTypes ();
		}

		public void app_window_close (object o, EventArgs e)
		{
			appWindows.Remove (appWindowInfo);
		}

		public void app_window_present (object o, EventArgs e)
		{
			mainWindow.Present ();
		}

		public void app_window_toggle_fullscreen (object o, EventArgs e)
		{
			// this doesn't make sense to me, either
			if (window_fullscreen.Active)
				mainWindow.Fullscreen();
			else
				mainWindow.Unfullscreen ();
		}

		public void app_window_toggle_maximize (object o, EventArgs e)
		{
			// this doesn't make sense to me, either
			if (window_maximize.Active)
				mainWindow.Maximize ();
			else
				mainWindow.Unmaximize ();
		}

		public void app_window_minimize (object o, EventArgs e)
		{
			mainWindow.Iconify ();
		}

		public void app_window_next (object o, EventArgs e)
		{
			appWindows.ActivateNext (appWindowInfo);
		}

		public void app_window_prev (object o, EventArgs e)
		{
			appWindows.ActivatePrevious (appWindowInfo);
		}

		public void app_window_all_to_front (object o, EventArgs e)
		{
			appWindows.AllToFront (appWindowInfo);
		}

		public void app_window_hide_all (object o, EventArgs e)
		{
			appWindows.HideAll ();
		}

		public void app_window_hide_others (object o, EventArgs e)
		{
			appWindows.HideOthers (appWindowInfo);
		}

		public void app_window_show_all (object o, EventArgs e)
		{
			appWindows.ShowAll (appWindowInfo);
		}
	}
}

// vim: noexpandtab
