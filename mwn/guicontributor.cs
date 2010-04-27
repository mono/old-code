using System;
using System.Drawing;
using Gtk;
using GtkSharp;
using Glade;

namespace MonoWeeklyNews {
public class GuiContributor {
	Glade.XML ui;
	Gtk.Window window;
	static Gtk.Entry subject_entry, contrib_entry, server_entry;
	static Gtk.TextBuffer textbuffer;
	static Gtk.Statusbar statusbar;
	
	public GuiContributor ()
	{
		ui = new Glade.XML (null, "ui.glade", "window1", null);
		ui.Autoconnect (this);
		LoadPointers ();
	}

	private void LoadPointers ()
	{
		window = (Gtk.Window) ui["window1"];
		window.SetDefaultSize (450, 450);
		subject_entry = (Gtk.Entry) ui["subject_entry"];
		textbuffer = ((Gtk.TextView) ui["textview1"]).Buffer;
		contrib_entry = (Gtk.Entry) ui["contrib_entry"];
		server_entry = (Gtk.Entry) ui["server_entry"];
		statusbar = (Gtk.Statusbar) ui["statusbar1"];
	}

	public static void Main ()
	{
		Application.Init ();
		GuiContributor contributor = new GuiContributor ();
		Application.Run ();
	}

	private static void on_window1_delete_event (object o, DeleteEventArgs args)
	{
		Application.Quit ();
	}

	private static void on_save_clicked (object o, EventArgs args)
	{
		Contributor.SaveToFile (subject_entry.Text, textbuffer.Text.Replace ('\n', ' '), contrib_entry.Text);
		statusbar.Push(0, "File saved");
	}

	private static void on_send_clicked (object o, EventArgs args)
	{
		string filename = null;
		string[] frags;
		byte response;
		
		for (int i=0; i < (frags = subject_entry.Text.Split (' ')).Length; ++i)
			filename += frags[i];
		statusbar.Push (0, "Sending mail...");
		response = Contributor.SendMail (contrib_entry.Text, "MWN Contribution: " + subject_entry.Text, server_entry.Text, (filename).Substring(0, 8) + ".mwnitem");
		if (response == 0)
			statusbar.Push (0, "Mail sent");
		else
			statusbar.Push (0, "An error occurred. Try to resend.");
	}
}
}
