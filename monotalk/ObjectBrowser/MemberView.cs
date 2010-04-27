namespace Monotalk.Browser
{
	using System;
	using System.Collections;
	using GLib;
	using Gtk;

        public class MemberView : TreeView {
                private TreeStore store;
		private MemberRecordFactory factory;
		private TreeViewColumn column;
		private Hashtable memberRecords;

		public delegate void MemberSelected (MemberRecord mr);
		public event MemberSelected MemberSelectedEvent;

		public MemberRecordFactory RecordFactory {
			set {
				factory = value;
				column.Title = factory.ColumnTitle;
				Refresh ();
			}
			get {
				return factory;
			}
		}

		public void Refresh () {
			store.Clear ();
			memberRecords.Clear ();
			if (factory != null && factory.Info != null) {
				Hashtable count = new Hashtable ();
				Hashtable parent = new Hashtable ();
				TreeIter iter;

				foreach (MemberRecord mr in factory.Info) {
					if (factory.HideDuplicates && count [mr.Name] != null) {
						if ((int) count [mr.Name] == 1) {
							TreeIter orig = (TreeIter) parent [mr.Name];
							TreeIter root;
							object name, value;
							GLib.Value image = new Value ();

							name = store.GetValue (orig, 0);
							value = store.GetValue (orig, 1);
							store.GetValue (orig, 2, image);
							store.Remove (out orig);

							store.Append (out root);
							store.SetValue (root, 0, new GLib.Value (mr.Name));
							store.SetValue (root, 1, new GLib.Value (mr.Name + " (2)"));

							parent [mr.Name] = root;

							store.Append (out orig, root);
							store.SetValue (orig, 0, name);
							store.SetValue (orig, 1, value);
							store.SetValue (orig, 2, image);

							store.Append (out iter, root);
							memberRecords [orig] = memberRecords [root];
							memberRecords [root] = null;
						} else {
							//Console.WriteLine ("4");
							store.Append (out iter, (TreeIter) parent [mr.Name]);
							store.SetValue ((TreeIter) parent [mr.Name], 0, new GLib.Value (mr.Name));
							store.SetValue ((TreeIter) parent [mr.Name], 1, new GLib.Value (mr.Name
															+ " (" + ((int) count [mr.Name] + 1) + ")"));
						}
						
						count [mr.Name] = 1 + (int) count [mr.Name];

					} else {
						store.Append (out iter);
						count [mr.Name] = 1;
						parent [mr.Name] = iter;
					}

					memberRecords [iter] = mr;
					store.SetValue (iter, 0, mr.Name);
					store.SetValue (iter, 1, mr.Label);
					store.SetValue (iter, 2, mr.Icon);
				}
			}
		}

                public MemberView () : base ()
		{
			memberRecords = new Hashtable ();

			store = new TreeStore ((int) TypeFundamentals.TypeString, (int) TypeFundamentals.TypeString, Gdk.Pixbuf.GType);
			store.SetSortColumnId (0, SortType.Ascending);

			column = new TreeViewColumn ();
			CellRenderer image = new CellRendererPixbuf ();
			CellRenderer text = new CellRendererText ();
			column.Title = "Member";
			column.PackStart (image, false);
			column.AddAttribute (image, "pixbuf", 2);
			column.PackStart (text, true);
			column.AddAttribute (text, "markup", 1);

			AppendColumn (column);
			Selection.Changed += new EventHandler (MemberSelectionChanged);

			Model = store;
		}

                private void MemberSelectionChanged (object o, EventArgs args)
		{
			TreeSelection selection = (TreeSelection) o;
			TreeModel model;
			TreeIter iter;
			GLib.Value val = new GLib.Value ();

			selection.GetSelected (out model, out iter);
			if (MemberSelectedEvent != null)
				MemberSelectedEvent ((MemberRecord) memberRecords [iter]);
		}

		private bool FindChild (TreeIter parent, out TreeIter iter, string label)
		{
			store.IterChildren (out iter, parent);

			do
				if ((string) store.GetValue (iter, 1) == label)
					return true;
			while (store.IterNext (out iter));

			return false;
		}

		public bool SelectMember (string label)
		{
			if (label != null) {
				TreeIter iter, child;
				store.GetIterFirst (out iter);
				do {
					string curLabel = (string) store.GetValue (iter, 1);

					if (curLabel == label
					    || (store.IterHasChild (iter) && FindChild (iter, out child, label))) {
						TreeIter result;

						if (curLabel == label)
							result = iter;
						else {
							ExpandRow (store.GetPath (iter), false);
							result = child;
						}
						Selection.SelectIter (result);
						ScrollToCell (store.GetPath (iter), GetColumn (0), true, (float) 0.5, 0);
						
						return true;
					}
				} while (store.IterNext (out iter));
			} else
				Selection.UnselectAll ();
			return false;
		}
	}
}
