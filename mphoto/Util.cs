//
// Utility functions.
//
// Miguel de Icaza (miguel@ximian.com).
//
// (C) 2002 Ximian, Inc.
//
//

using System.Runtime.InteropServices;
using System.Threading;
using System;

class Util {

#if WIN32
	public const char DirSep = '\\';
#else
	public const char DirSep = '/';
#endif

}

class Semaphore {
	int count = 0;
	
	public Semaphore ()
	{ }

	public void Down ()
	{
		lock (this){
			while (count <= 0){
				Monitor.Wait (this, Timeout.Infinite);
			}
			count--;
		}
	}

	public void Up ()
	{
		lock (this){
			count++;
			Monitor.Pulse (this);
		}
	}
}

class GtkUtil {
	public static void MakeMenuItem (Gtk.Menu menu, string l, EventHandler e)
    {
        MakeMenuItem (menu, l, e, true);
    }

	public static void MakeMenuItem (Gtk.Menu menu, string l, EventHandler e, bool enabled)
	{
		Gtk.MenuItem i = new Gtk.MenuItem (l);
		i.Activated += e;
                i.Sensitive = enabled;

		menu.Append (i);
		i.Show ();
	}

    public static void MakeMenuSeparator (Gtk.Menu menu)
    {
        Gtk.SeparatorMenuItem i = new Gtk.SeparatorMenuItem ();
        menu.Append (i);
        i.Show ();
    }
}
