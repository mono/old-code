// GnomeAbout.cs - Gtk# Tutorial example
//
// Author: Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (c) 2003 Martin Willemoes Hansen

using System;
using Gdk;
using Gtk;
using Gnome;
 
namespace GtkSharpTutorial {

        public class GnomeAbout {

		static void Main()
		{
			Application.Init();
			About();
			Application.Run();
		}

		static void About()
		{
			string [] authors = new String [] {
				"I am weasel",
				"I are baboon",
			};

			string [] documenters = new String [] {
				"Cow",
				"Chicken",	
			};

			string translators = "Mom, Dad";
			Pixbuf pixbuf = new Pixbuf("logo.png");
			
			Gnome.About about = new Gnome.About ("Cartoon viewer", "1.0.0",
							     String.Format ("{0}\n{1}\n{2}\n{3}\n{4}\n{5}",
									    "Copyright (C) 2003 Cartoon Company",
									    "Cartoon viewer comes with ABSOLUTELY NO WARRANTY;",
									    "This is free software, and you are welcome to",
									    "redistribute it under certain conditions;",
									    "see the text file: COPYRIGHT, distributed ",
									    "with this program."),
							     "Mono and Gtk# rocks!",
							     authors, documenters, translators, pixbuf);
			about.Show();
		}
	}
}
