//
// main.cs: The driver for the MonoPhoto application
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2002 Ximian, Inc.
//

using GLib;
using Gtk;
using Gdk;
using GtkSharp;
using System;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Runtime.InteropServices;

class Driver {
        [DllImport("sqlite")]
        static extern void sqliteParserTrace (IntPtr fp, string prompt);

        [DllImport("/lib/libc.so.6")]
        static extern int open (string path, int flags, int mode);

        [DllImport("/lib/libc.so.6")]
        static extern IntPtr fdopen (int fd, string mode);

	static int Main (string [] args)
	{
		Application.Init ();
//		string dbname = "mphoto";
                string dbname;
                string dirtoimport;
                string mphotodir = System.Environment.GetEnvironmentVariable ("HOME") + "/.mphoto";

                dbname = "URI=file:" + mphotodir + "/" + "mphoto.db";

                IImageRepository repo;

                System.IO.DirectoryInfo dinfo = new System.IO.DirectoryInfo (mphotodir);
                dinfo.Create ();

                // turn on sqlite debugging
//                int fd = open ("/tmp/sqlite.debug", 576, 420);
//                sqliteParserTrace (fdopen (fd, "rw"), ">");

		if (args.Length != 0) {
                    repo = new DirImageRepository (args);
                } else {
                    repo = new DbImageRepository (dbname);
                }

                MphotoToplevel tl = new MphotoToplevel (repo, args);
                tl.Toplevel.Show ();

		try {
			Application.Run ();
		} catch (TargetInvocationException e) {
			Console.WriteLine ("TIE: " + e.InnerException);
		} finally {
                        // yuck
                        DbProvider.CloseProviders ();
                        Thumbnailer.KillThreads ();
		}
		
		return 0;
	}
}
