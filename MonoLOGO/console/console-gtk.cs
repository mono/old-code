namespace Mono.Languages.Logo
{
	using System;
	using System.Drawing;
	using Gtk;
	using GtkSharp;

	public class ConsoleGtk : LogoConsole
	{
		public ConsoleGtk ()
		{
			Window win = new Window ("MonoLOGO");
			win.DeleteEvent += new EventHandler (Window_Delete);
			win.BorderWidth = 4;
			win.DefaultSize = new Size (450, 300);
			
			VBox vbox = new VBox (false, 4);
			win.EmitAdd (vbox);
			
			ScrolledWindow swin = new ScrolledWindow (new Adjustment (0.0, 0.0, 0.0, 0.0, 0.0, 0.0), new Adjustment (0.0, 0.0, 0.0, 0.0, 0.0, 0.0));
			swin.HscrollbarPolicy = Gtk.PolicyType.Automatic;
			swin.VscrollbarPolicy = Gtk.PolicyType.Automatic;
			swin.ShadowType = Gtk.ShadowType.In;
			vbox.PackStart (swin, true, true, 0);
			
			TextBuffer buf = new TextBuffer (new TextTagTable ());
			Out = new TextWriterGtk (buf);
			TextView text = new TextView (buf);
			text.Editable = false;
			swin.EmitAdd (text);

			Entry entry = new Entry ();
			entry.Activate += new EventHandler (Entry_Activate);
			vbox.PackStart (entry, false, false, 0);
			
			win.ShowAll ();
		}

		public static int Main (string[] args)
		{
			Application.Init ();
			ConsoleGtk console = new ConsoleGtk ();
			Application.Run ();

			return 0;
		}

		public static void Window_Delete (object obj, EventArgs args)
		{
			Application.Quit ();
		}

		public void Entry_Activate (object obj, EventArgs args)
		{
			Entry entry = (Entry) obj;	
			InputCommand (entry.Text);
			entry.Text = "";
		}
	}
}
