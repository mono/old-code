namespace Monotalk.Browser {

	using System;
	using System.Collections;
	using System.Drawing;
	using System.Reflection;
	using GConf;

	// TEMPORARY
	using Monotalk.Indexer;
	// TEMPORARY END

	using Mono.CSharp.Debugger;

	using Monotalk.SourceView;

	using Gdk;
	using Gtk;

        public enum MemberFilter {
		AllMembers = 0,
		Fields,
		Properties,
		Methods,
		Constructors,
		Events,
	}

	public class ObjectBrowser : VPaned {
                private HPaned hPaned1, hPaned2;
		private VBox vbox;
                private TreeStore typeStore;
		private ListStore memberStore, ICStore;
		private TreeView typeView, ICView, MTView;
                private MemberView memberView;
                private TextView text = null;
		private Notebook Content;
		public SourceView buffer;  // FIXME: private
		private Label SourceLabel;
		private BindingFlags flags;
		private MemberRecordFactory[] recordFactory;
		private MemberRecordFactory factory;
		private bool assemblyOnly = true;
		public TypePool types = new TypePool ();
		private FindBar findBar;
		private bool showClasses;
		private bool showInterfaces;
		private bool showEnums;
		private Type type;
		private Hashtable typeIters;
		private bool namespaces = true;
		private Label sourceLabel;
		private Gtk.Image sourceIcon;
		public Gnome.AppBar AppBar;

		private MonoSymbolFile symbolFile;

		private static TypeAliases Alias = new TypeAliases ();
		private static GConf.Client gconf = new GConf.Client ();
		private static readonly string gconfPath = "/apps/monotalk/objectbrowser/";

		// TEMPORARY
		public Indexer indexer = new Indexer ();
		// TEMPORARY END

		public ObjectBrowser () : base ()
		{
			showClasses = true;
			showInterfaces = showEnums = false;
			flags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
			SetupFormatters ();
			SetupUI ();

			typeIters = new Hashtable ();
		}

		public void Refresh ()
		{
			RefreshTypes ();
			RefreshMemberView ();
			RefreshMemberSelector ();
			RefreshICSelector ();
			RefreshFindBar ();
		}

		public bool Namespaces {
			set {
				namespaces = value;
				RefreshTypes ();
			}
		}

		public bool Classes {
			get {
				return showClasses;
			}
			set {
				bool changed = showClasses != value;
				showClasses = value;
				if (changed)
					Refresh ();
			}
		}

		public bool Enums {
			get {
				return showEnums;
			}
			set {
				bool changed = showEnums != value;
				showEnums = value;
				if (changed)
					Refresh ();
			}
		}

		public bool Interfaces {
			get {
				return showInterfaces;
			}
			set {
				bool changed = showInterfaces != value;
				showInterfaces = value;
				if (changed)
					Refresh ();
			}
		}

		private void SetupFormatters ()
		{
			AllRecordFactory all;

			recordFactory = new MemberRecordFactory [6];
			recordFactory [0] = all = new AllRecordFactory (flags);
			recordFactory [1] = new FieldRecordFactory (flags);
			recordFactory [2] = new PropertyRecordFactory (flags);
			recordFactory [3] = new MethodRecordFactory (flags);
			recordFactory [4] = new ConstructorRecordFactory (flags);
			recordFactory [5] = new EventRecordFactory (flags);
			all.Add (recordFactory [1]);
			all.Add (recordFactory [2]);
			all.Add (recordFactory [3]);
			all.Add (recordFactory [4]);
			all.Add (recordFactory [5]);
			factory = recordFactory [0];
		}

		private Widget SetupTypeTree ()
		{
			typeStore = new TreeStore ((int) GLib.TypeFundamentals.TypeString, (int) GLib.TypeFundamentals.TypeString, Gdk.Pixbuf.GType);
			typeView = new TreeView (typeStore);
			TreeViewColumn col = new TreeViewColumn ();
			CellRenderer renderer = new CellRendererText ();
			CellRenderer image = new CellRendererPixbuf ();
			col.Title = "Type";
			col.PackStart (image, false);
			col.AddAttribute (image, "pixbuf", 2);
			col.PackStart (renderer, true);
			col.AddAttribute (renderer, "markup", 0);
			//col.AddAttribute (renderer, "text", 0);
			typeView.AppendColumn (col);
			typeView.Selection.Changed += new EventHandler (TypeSelectionChanged);
			typeView.ButtonPressEvent += new GtkSharp.ButtonPressEventHandler (TypeSelectorButtonPressed);
			typeStore.SetSortColumnId (0, SortType.Ascending);

			ScrolledWindow sw = new ScrolledWindow ();
			sw.Add (typeView);

			return sw;
		}

		private void SetupUI ()
		{
			hPaned1 = new HPaned ();
			hPaned2 = new HPaned ();
			hPaned1.Add1 (SetupTypeTree ());
			hPaned1.Add2 (hPaned2);

			hPaned1.Position = (int) gconf.Get (gconfPath + "hpaned1position");
			hPaned2.Position = (int) gconf.Get (gconfPath + "hpaned2position");

			vbox = new VBox (true, 2);
			vbox.Spacing = 2;
			Frame frame = new Frame ();
			frame.Shadow = ShadowType.In;
			frame.Add (MemberSelector ());
			vbox.Add (frame);
			frame = new Frame ();
			frame.Shadow = ShadowType.In;
			frame.Add (ICSelector ());
			vbox.Add (frame);
			hPaned2.Add1 (vbox);

			memberView = new MemberView ();
			memberView.MemberSelectedEvent += new MemberView.MemberSelected (MemberSelected);
			memberView.RecordFactory = recordFactory [0];

			ScrolledWindow sw = new ScrolledWindow ();
			sw.Add (memberView);
			hPaned2.Add2 (sw);

			Content = new Notebook ();
			findBar = new FindBar (this);
			Content.AppendPage (findBar, new Label ("Find"));

			Add1 (hPaned1);
			Add2 (Content);

			Position = (int) gconf.Get (gconfPath + "vpaned1position");
		}

		public BindingFlags Flags {
			set {
				bool changedVisibility = ((value & (BindingFlags.Public | BindingFlags.NonPublic))
							  != (flags & (BindingFlags.Public | BindingFlags.NonPublic)));

				flags = value;
				recordFactory [0].Flags = flags;
				if (changedVisibility) {
					RefreshTypes ();
					RefreshFindBar ();
				}
				RefreshMemberSelector ();
				RefreshMemberView ();
				RefreshICSelector ();
			}
			get {
				return flags;
			}
		}

		public void RefreshMemberView ()
		{
			memberView.Refresh ();
		}

		public void RefreshFindBar ()
		{
			if (findBar != null)
				findBar.Refresh ();
		}

		private Type Type {
			set {
				recordFactory [0].Type = type = value;
				RefreshMemberSelector ();
				RefreshMemberView ();
				RefreshICSelector ();

				if (type != null) {
					Part part = indexer.db.LookupType (type.Namespace != null ? type.Namespace : "", type.Name);
					if (part != null)
						ShowSource ("<b>" + type.FullName + "</b>", part);
				}
			}
		}

		private int Row (TreeIter iter)
		{
			return Convert.ToInt32 (memberStore.GetPath (iter).ToString ());
		}

		private void MemberFilterSelectionChanged (object o, EventArgs args)
		{
			TreeSelection selection = (TreeSelection) o;
			TreeModel model;
			TreeIter iter;

			//Console.WriteLine ("changed {0}", selection);
			if (selection != null) {
				selection.GetSelected (out model, out iter);
				if (model != null) {
					memberView.RecordFactory = factory = recordFactory [Row (iter)];
				}
			}
			RefreshICSelector ();
		}

		private void RefreshMemberSelector ()
		{
			TreeIter iter;

			memberStore.IterChildren (out iter);
			foreach (MemberRecordFactory rv in recordFactory) {
				memberStore.SetValue (iter, 0, new GLib.Value (rv.FullTitle));
				memberStore.IterNext (out iter);
			}
		}

		public void SelectAllMembers ()
		{
			MTView.Selection.SelectPath (new TreePath ("0"));
			ICView.Selection.SelectPath (new TreePath ("0"));
		}

		private Container MemberSelector ()
		{
			ScrolledWindow sw = new ScrolledWindow ();
			memberStore = new ListStore ((int) GLib.TypeFundamentals.TypeString, (int) GLib.TypeFundamentals.TypeInt);
			MTView = new TreeView (memberStore);
			TreeViewColumn col = new TreeViewColumn ();
			TreeIter iter;
			CellRenderer renderer = new CellRendererText ();

			col.Title = "MemberType";
			col.PackStart (renderer, true);
			col.AddAttribute (renderer, "markup", 0);
			MTView.AppendColumn (col);
			MTView.HeadersVisible = false;

			foreach (MemberRecordFactory rv in recordFactory) {
				memberStore.Append (out iter);
				memberStore.SetValue (iter, 0, new GLib.Value (rv.FullTitle));
			}

			MTView.Selection.SelectPath (new TreePath ("0"));
			MTView.Selection.Changed += new EventHandler (MemberFilterSelectionChanged);
			MTView.BorderWidth = 5;

			sw.Add (MTView);

			return sw;
		}

		private void RefreshICSelector ()
		{
			TreeIter iter;

			ICStore.IterChildren (out iter);
			ICStore.SetValue (iter, 0, new GLib.Value (ICLabel ("all", factory.staticCount + factory.instanceCount)));
			ICStore.IterNext (out iter);
			ICStore.SetValue (iter, 0, new GLib.Value (ICLabel ("instance", factory.instanceCount)));
			ICStore.IterNext (out iter);
			ICStore.SetValue (iter, 0, new GLib.Value (ICLabel ("class", factory.staticCount)));
		}

		private void ICSelectionChanged (object o, EventArgs args)
		{
			TreeSelection selection = (TreeSelection) o;
			TreeModel model;
			TreeIter iter;

			//Console.WriteLine ("changed {0}", selection);
			selection.GetSelected (out model, out iter);

			Flags = (flags & ~(BindingFlags.Instance | BindingFlags.Static)) | (BindingFlags) ((int) model.GetValue (iter, 1));
		}

		private string ICLabel (string name, int count)
		{
			return (count > 0 ? "<b>" : "") + name + (count > 0 ? " (" + count + ")</b>" : "");
		}

		private Container ICSelector ()
		{
			ScrolledWindow sw = new ScrolledWindow ();
			ICStore = new ListStore ((int) GLib.TypeFundamentals.TypeString, (int) GLib.TypeFundamentals.TypeInt);
			ICView = new TreeView (ICStore);
			TreeViewColumn col = new TreeViewColumn ();
			TreeIter iter;
			CellRenderer renderer = new CellRendererText ();

			col.Title = "MemberType";
			col.PackStart (renderer, true);
			col.AddAttribute (renderer, "markup", 0);
			ICView.AppendColumn (col);
			ICView.HeadersVisible = false;

			ICStore.Append (out iter);
			ICStore.SetValue (iter, 0, new GLib.Value (ICLabel ("all", factory.staticCount + factory.instanceCount)));
			ICStore.SetValue (iter, 1, new GLib.Value ((int) (BindingFlags.Static | BindingFlags.Instance)));
			ICStore.Append (out iter);
			ICStore.SetValue (iter, 0, new GLib.Value (ICLabel ("instance", factory.instanceCount)));
			ICStore.SetValue (iter, 1, new GLib.Value ((int) BindingFlags.Instance));
			ICStore.Append (out iter);
			ICStore.SetValue (iter, 0, new GLib.Value (ICLabel ("class", factory.staticCount)));
			ICStore.SetValue (iter, 1, new GLib.Value ((int) BindingFlags.Static));

			ICView.Selection.SelectPath (new TreePath ("0"));
			ICView.Selection.Changed += new EventHandler (ICSelectionChanged);
			ICView.BorderWidth = 5;

			sw.Add (ICView);
			return sw;
		}

		private void SetCount (string label, int row, MemberInfo[] arr)
		{
			Console.WriteLine ("changed {0} {1}", row, row.ToString ());
			TreeIter iter;
			if (memberStore.GetIterFromString (out iter, row.ToString ())) {
				int len = arr.Length;
				memberStore.SetValue (iter, 0, new GLib.Value (len == 0 ? label : label + " (" + len + ")"));
			}
		}

		private void TypeSelectionChanged (object o, EventArgs args)
		{
			TreeSelection selection = (TreeSelection) o;
			TreeModel model;
			TreeIter iter;

			if (selection.GetSelected (out model, out iter)) {
				string type = (string) model.GetValue (iter, 1);
				Type = type != "" ? (Type) types [type] : null;
			} else
				Type = null;
		}

		private void TypeSelectorShowHierarchy (object o, EventArgs args)
		{
			Console.WriteLine ("show hierarchy");

			if (type != null) {
				Hierarchy h = new Hierarchy (type);
				h.Show ();
			}
		}		

		private void TypeSelectorButtonPressed (object o, GtkSharp.ButtonPressEventArgs args)
		{
			if (args.Event.type == Gdk.EventType.ButtonPress && args.Event.button == 3) {
				TreePath path = null;

				Console.WriteLine ("event: " + args.Event.x + "," + args.Event.y);

				if (typeView.GetPathAtPos ((int) args.Event.x, (int) args.Event.y, out path)) {
					TreeIter iter;
					if (typeStore.GetIter (out iter, path)) {
						if (SelectType ((string) typeStore.GetValue (iter, 1))) {
							Menu menu = new Menu ();
							MenuItem item = new MenuItem ("Show hierarchy");

							item.Activated += new EventHandler (TypeSelectorShowHierarchy);
							item.Show ();
							menu.Append (item);
							menu.Popup (null, null, null, IntPtr.Zero, args.Event.button, args.Event.time);
							args.RetVal = true;
						}
					}
				}
			}
			args.RetVal = false;
		}

		internal bool IsVisible (Type t)
		{
			if (t.IsSpecialName)
				return false;
			if (t.IsNotPublic && ((flags & BindingFlags.NonPublic) != BindingFlags.NonPublic))
				return false;
			if (t.IsPublic && ((flags & BindingFlags.Public) != BindingFlags.Public))
				return false;
			if (t.IsClass && !showClasses)
				return false;
			if (t.IsInterface && !showInterfaces)
				return false;
			if (t.IsEnum && !showEnums)
				return false;
			return true;			
		}

		object AddTypeWithoutNamespace (Type t, Hashtable ht)
		{
			TreeIter iter;

			//Console.WriteLine ("add type {0}", t.FullName);

			if (!IsVisible (t))
				return null;

			//Console.WriteLine ("try type {0}", t.FullName);
			if (ht [t] != null)
				return (TreeIter) ht [t];

			//Console.WriteLine ("new line {0}", t.FullName);

			iter = new TreeIter ();
			if (t.BaseType == null) // FIXME? || t.BaseType.Assembly != assembly)
				typeStore.Append (out iter);
			else
				typeStore.Append (out iter, (TreeIter) AddTypeWithoutNamespace (t.BaseType, ht));

			TypeRecord tr = new TypeRecord (t);
			typeStore.SetValue (iter, 0, new GLib.Value (tr.Label));
			typeStore.SetValue (iter, 1, new GLib.Value (t.FullName));
			typeStore.SetValue (iter, 2, new GLib.Value (tr.Icon));

			ht [t] = iter;

			return iter;
		}

		TreeIter NamespaceIter (String nspace, Hashtable ht)
		{
			if (ht [nspace] != null)
				return (TreeIter) ht [nspace];

			int idx = nspace.LastIndexOf ('.');
			TreeIter iter;

			if (idx > 0) {
				string parentNspace = nspace.Remove (idx, nspace.Length - idx);
				TreeIter parent = NamespaceIter (parentNspace, ht);
				typeStore.Append (out iter, parent);
			} else {
				typeStore.Append (out iter);
			}
			typeStore.SetValue (iter, 0, new GLib.Value (idx > 0 ? nspace.Remove (0, idx + 1) : nspace));
			typeStore.SetValue (iter, 1, new GLib.Value (""));
			ht [nspace] = iter;
			return iter;
		}

		object AddTypeWithNamespace (Type t, Hashtable ht)
		{
			//Console.WriteLine ("add type {0}", t.FullName);

			if (!IsVisible (t))
				return null;

			//Console.WriteLine ("try type {0}", t.FullName);
			if (ht [t] != null)
				return (TreeIter) ht [t];

			//Console.WriteLine ("new line {0}", t.FullName);

			TreeIter iter = new TreeIter ();

			if (t.BaseType != null /* FIXME? && t.BaseType.Assembly == assembly */ && t.BaseType.Namespace == t.Namespace)
				typeStore.Append (out iter, (TreeIter) AddTypeWithNamespace (t.BaseType, ht));
			else {
				if (t.Namespace != null && t.Namespace != "")
					typeStore.Append (out iter, NamespaceIter (t.Namespace, ht));
				else
					typeStore.Append (out iter);
			}

			TypeRecord tr = new TypeRecord (t);
			typeStore.SetValue (iter, 0, new GLib.Value (tr.Label));
			typeStore.SetValue (iter, 1, new GLib.Value (t.FullName));
			typeStore.SetValue (iter, 2, new GLib.Value (tr.Icon));

			ht [t] = iter;

			return iter;
		}

		object AddType (Type t, Hashtable ht)
		{
			return namespaces ? AddTypeWithNamespace (t, ht) : AddTypeWithoutNamespace (t, ht);
		}

		private bool FindRow (ref TreeIter iter, string name)
		{
			do
				if ((string) typeStore.GetValue (iter, 1) == name)
					return true;
			while (typeStore.IterNext (out iter));

			return false;
		}

		public bool SelectType (Type t)
		{
			if (t != null && typeIters [t] != null) {
				TreeIter iter = (TreeIter) typeIters [t];

				TreePath path = typeStore.GetPath (iter);
				Stack stack = new Stack ();
				while (path.Up ())
					stack.Push (path.Copy ());
				while (stack.Count > 0)
					typeView.ExpandRow ((TreePath) stack.Pop (), false);
				typeView.Selection.SelectIter (iter);
				typeView.ScrollToCell (typeStore.GetPath (iter), typeView.GetColumn (0), true, (float) 0.5, 0);

				return true;
			}
			typeView.Selection.UnselectAll ();
			return false;
		}

		public void ShowSource (string label, Part part)
		{
			ShowSourceView (label);
			buffer.Clear ();
			buffer.InsertSource (buffer.StartIter, indexer [part.fileID], part.startRow, part.endRow);
			Content.CurrentPage = 1;
		}

		public bool SelectType (string name)
		{
			return SelectType ((Type) types [name]);
		}

		public bool SelectMember (string label)
		{
			return memberView.SelectMember (label);
		}

		public int RefreshTypes ()
		{
			Type old = type;
			int nTypes = 0;
			int progress = 0;

			typeIters.Clear ();
			typeStore.Clear ();
			foreach (Type t in types.Values) {
				if (t.IsSpecialName)
					continue;
			        if (AddType (t, typeIters) != null)
					nTypes ++;
				progress ++;
				/* if (AppBar != null) {
					AppBar.ProgressPercentage = (float) progress / types.Length;
					if (nTypes % 10 == 0)
						AppBar.SetStatus (String.Format ("{0} type(s) loaded.", types));
						} */
				// refresh Gtk
				while (GLib.MainContext.Iteration ());
			}

			if (AppBar != null) {
				AppBar.SetStatus (String.Format ("{0} type(s) loaded.", types));
			}
			SelectType (old);

			return nTypes;
		}

                public void Add (Assembly a) {
			foreach (Type t in a.GetTypes ()) {
				Add (t);
			}
				//symbolFile = MonoSymbolFile.ReadSymbolFile (assembly);
				//RefreshTypes ();
		}

		public void Add (Type t)
		{
			types.Add (t);
			AddType (t, typeIters);
		}

		public void FindMember ()
		{
			Content.CurrentPage = 0;
			findBar.GrabFocus ();
		}

		public void ShowSourceView (string label)
		{
			if (text == null) {
				ScrolledWindow sw = new ScrolledWindow ();
				buffer = new SourceView (new TextTagTable ());
				text = new TextView (buffer);
				text.Editable = false;
				text.CanFocus = false;
				text.Name = "SourceView";
				Frame frame = new Frame ();
				frame.BorderWidth = 5;
				frame.ShadowType = ShadowType.In;
				sw.Add (text);
				frame.Add (sw);
				frame.ShowAll ();
				SourceLabel = new Label ("");
				Content.InsertPage (frame, SourceLabel, 1);
			}
			SourceLabel.Markup = "Source: " + label;
		}

		public void MemberSelected (MemberRecord mr)
		{
			Part part = null;
			if (mr != null) {
				if (symbolFile != null && mr.MemberInfo.GetType ().IsSubclassOf (typeof (MethodBase))) {
					MethodEntry me = symbolFile.GetMethod ((MethodBase) mr.MemberInfo);
					Console.WriteLine ("Lookup {0}", mr.MemberInfo);
					if (me != null) {
						Console.WriteLine ("Found");
						//part = new Part ();
					}
				}
				part = indexer.db.LookupMember (mr.MemberInfo.DeclaringType.Namespace, Alias [mr.MemberInfo.DeclaringType], mr.SourceKey);
			}
			if (part != null) {
				ShowSource (mr.Label, part);
				return;
			}
			if (text != null) {
				Content.RemovePage (1);
				text = null;
			}
		}

		public void Save ()
		{
			gconf.Set (gconfPath + "hpaned1position", hPaned1.Position);
			gconf.Set (gconfPath + "hpaned2position", hPaned2.Position);
			gconf.Set (gconfPath + "vpaned1position", Position);
		}

		// TEMPORARY
		public void ParseFile (string filename)
		{
			indexer.Parse (filename);
		}
		// TEMPORARY END
	}
}
