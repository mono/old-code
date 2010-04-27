namespace Monotalk.Browser {

	using System;
	using Gtk;

	public class Hierarchy : Gtk.Window {

		ObjectBrowser ob;

		public Hierarchy (Type type) : base (new GLib.Type (GType))
		{
			Resize (480, 480);
			ob = new ObjectBrowser ();
			ob.Namespaces = false;
			Type t = type;
			while (t != null) {
				ob.Add (t);
				t = t.BaseType;
			}
			Add (ob);
			ShowAll ();
			ob.SelectType (type);
		}

		static uint gtype = 0;
		public static new uint GType {
			get {
				if (gtype == 0)
					gtype = RegisterGType (typeof (Hierarchy)).Value;
				return gtype;
			}
		}
	}

}
