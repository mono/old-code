namespace Monotalk.Browser
{
	using System;
	using System.Collections;
	using System.Reflection;
	using System.Threading;
	using Gdk;
	using Gtk;
	using Glade;
	using GtkSharp;

	public class FindBar : VBox {
		private Glade.XML gXML;
		private Entry entry;
		private ObjectBrowser browser;
		private ListStore membersStore, typesStore;
		private TreeView members, types;
		private System.Threading.Thread findThread;
		private string text;
		private Queue found;
		private ThreadNotify notify;
		private uint idle;
		private Queue todoTypes = new Queue ();
		private int foundTypes, foundMembers;

		public FindBar (ObjectBrowser objectBrowser) : base (false, 0)
		{
			browser = objectBrowser;
			found = new Queue ();
			notify = new ThreadNotify (new ReadyEvent (FoundEvent));
			BorderWidth = Spacing = 5;

			SetupUI ();
		}

		private void ReparentAdd (Widget W, bool Expand)
		{
			(W.Parent as Container).Remove (W);
			PackStart (W, Expand, true, 0);
		}

		/* private void KeyPressed (object o, KeyPressEventArgs args)
		{
			Console.WriteLine ("key {0}", args.Event.keyval);
			switch (args.Event.keyval) {
			case 65293: //FIXME: Enter
				//Select ();
				break;
			}
			} */

		private void RowActivated (object o, RowActivatedArgs args)
		{
			TreeModel store = (o as TreeView).Model;
			TreeIter iter;

			if (store.GetIter (out iter, args.Path)) {
				browser.SelectType ((string) store.GetValue (iter, 1));
				browser.SelectAllMembers ();
				if (!(bool) store.GetValue (iter, 3))
					browser.SelectMember ((string) store.GetValue (iter, 0));
			}
		}

		private void SetupUI ()
		{
			gXML = new Glade.XML (null, "demoapp.glade", "FindWindow", null);
			gXML.Autoconnect (this);

			ReparentAdd (gXML.GetWidget ("FindTop"), false);
			ReparentAdd (gXML.GetWidget ("FindBottom"), true);
			members = (TreeView) gXML.GetWidget ("FindMembersTreeView");
			// members.KeyPressEvent += new KeyPressEventHandler (KeyPressed);
			members.RowActivated += new RowActivatedHandler (RowActivated);
			types = (TreeView) gXML.GetWidget ("FindTypesTreeView");
			types.RowActivated += new RowActivatedHandler (RowActivated);
			SetupViews ();

			entry = (Entry) gXML.GetWidget ("FindEntry");
			entry.Changed += new EventHandler (EntryChanged);

			ShowAll ();
		}		

		private void SetupViews ()
		{
			typesStore = new ListStore ((int) GLib.TypeFundamentals.TypeString,
						    (int) GLib.TypeFundamentals.TypeString,
						    Gdk.Pixbuf.GType,
						    (int) GLib.TypeFundamentals.TypeBoolean);
			membersStore = new ListStore ((int) GLib.TypeFundamentals.TypeString,
						      (int) GLib.TypeFundamentals.TypeString,
						      Gdk.Pixbuf.GType,
						      (int) GLib.TypeFundamentals.TypeBoolean);

			TreeViewColumn column = new TreeViewColumn ();
			CellRenderer text = new CellRendererText ();
			CellRenderer image = new CellRendererPixbuf ();
			column.Title = "Member";
			column.SortColumnId = 0;
			column.Sizing = TreeViewColumnSizing.Autosize;
			column.PackStart (image, false);
			column.AddAttribute (image, "pixbuf", 2);
			column.PackStart (text, true);
			column.AddAttribute (text, "markup", 0);
			members.AppendColumn (column);

			column = new TreeViewColumn ();
			column.SortColumnId = 1;
			column.Sizing = TreeViewColumnSizing.Autosize;
			text = new CellRendererText ();
			column.Title = "Location";
			column.PackStart (text, true);
			column.AddAttribute (text, "markup", 1);
			members.AppendColumn (column);
			members.Model = membersStore;

			column = new TreeViewColumn ();
			column.Title = "Type";
			column.SortColumnId = 0;
			column.Sizing = TreeViewColumnSizing.Autosize;
			image = new CellRendererPixbuf ();
			column.PackStart (image, false);
			column.AddAttribute (image, "pixbuf", 2);
			text = new CellRendererText ();
			column.PackStart (text, true);
			column.AddAttribute (text, "markup", 0);
			types.AppendColumn (column);
			types.Model = typesStore;
		}

		private void Append (ListStore store, MemberRecord mr, Type type)
		{
			TreeIter iter;
			store.Append (out iter);

			store.SetValue (iter, 0, new GLib.Value (mr.Label));
			store.SetValue (iter, 1, new GLib.Value (type.FullName));
			store.SetValue (iter, 2, new GLib.Value (mr.Icon));
			store.SetValue (iter, 3, new GLib.Value (mr.GetType () == typeof (TypeRecord)));
		}

		private bool FoundIdle ()
		{
			int i = 0;
			bool again;

			lock (found) {
				while (found.Count > 0 && i < 10) {
					object info = found.Dequeue ();
					Type infoType = info.GetType ();

					if (infoType.IsSubclassOf (typeof (Type))) {
						Append (typesStore, new TypeRecord (info as Type), info as Type);
						foundTypes ++;
					} else {
						Type type = found.Dequeue () as Type;
						if (infoType.IsSubclassOf (typeof (MethodInfo)))
							Append (membersStore, new MethodRecord (info as MethodInfo), type);
						else if (infoType.IsSubclassOf (typeof (ConstructorInfo)))
							Append (membersStore, new ConstructorRecord (info as ConstructorInfo), type);
						else if (infoType.IsSubclassOf (typeof (EventInfo)))
							Append (membersStore, new EventRecord (info as EventInfo), type);
						else if (infoType.IsSubclassOf (typeof (FieldInfo)))
							Append (membersStore, new FieldRecord (info as FieldInfo), type);
						else if (infoType.IsSubclassOf (typeof (PropertyInfo)))
							Append (membersStore, new PropertyRecord (info as PropertyInfo), type);
						foundMembers ++;
					}
					i ++;
				}
				again = found.Count > 0;
				browser.AppBar.SetStatus (String.Format ("Found {0} type(s) and {1} member(s).", foundTypes, foundMembers));
			}

			if (!again)
				idle = 0;

			return again;
		}

		private void FoundEvent ()
		{
			if (idle == 0)
				idle = GLib.Idle.Add (new GLib.IdleHandler (FoundIdle));
		}

		public new void GrabFocus ()
		{
			entry.GrabFocus ();
		}

		private void EntryChanged (object obj, EventArgs args)
		{
			lock (this) {
				lock (found) {
					lock (todoTypes) {
						bool startThread = todoTypes.Count == 0;

						// clear current results
						typesStore.Clear ();
						membersStore.Clear ();
						todoTypes.Clear ();
						found.Clear ();
						if (idle != 0) {
							GLib.Source.Remove (idle);
							idle = 0;
						}
						foundTypes = foundMembers = 0;
						browser.AppBar.SetStatus ("");

						// prepare new work
						text = entry.Text;
						if (text.Length > 0) {
							foreach (Type type in browser.types.Values) {
								todoTypes.Enqueue (type);
							}

							if (startThread) {
								ThreadStart start = new ThreadStart (WorkThread);
							
								findThread = new System.Threading.Thread (start);
								findThread.Start ();
							}
						}
					}
				}
			}
		}

		public void Refresh ()
		{
			EntryChanged (entry, null);
		}

		private void WorkThread ()
		{
			bool added, typeAdded;
			BindingFlags flagsCopy;
			Type type;
			string textCopy;

			//Console.WriteLine ("Entering work thread");

			while (true) {
				lock (todoTypes) {
					type = (Type) todoTypes.Dequeue ();
					flagsCopy = browser.Flags;
					textCopy = (string) text.Clone ();
				}
				if (browser.IsVisible (type)) {
					added = typeAdded = false;
					if (type.Name.IndexOf (textCopy) >= 0) {
						lock (found) {
							found.Enqueue (type);
							added = typeAdded = true;
						}
					}
					foreach (MemberInfo mi in type.GetMembers (flagsCopy | BindingFlags.Static | BindingFlags.Instance)) {
						if (mi.Name.IndexOf (textCopy) >= 0 || typeAdded && mi.MemberType == MemberTypes.Constructor) {
							if (mi.MemberType == MemberTypes.Method && ((MethodInfo) mi).IsSpecialName)
								continue;
							lock (found) {
								found.Enqueue (mi);
								found.Enqueue (type);
								added = true;
							}
						}
					}
					lock (found) {
						if (added)
							notify.WakeupMain ();
					}
				}
				lock (todoTypes) {
					if (todoTypes.Count == 0)
						break;
				}
			}
			//Console.WriteLine ("Finished work thread");
		}
	}
}
