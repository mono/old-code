using System;
using Gtk;

public class MonoSkel {
	public static void Main (string[] args)
	{
		Application.Init ();
		Window w = new Window ("MonoSkel project");
		w.DefaultHeight = 200;
		w.DefaultWidth = 250;
		w.DeleteEvent += new DeleteEventHandler (OnDelete);
		Button b = new Button ("This is MonoSkel");
		b.Clicked += new EventHandler (OnClick);
		w.Add (b);
		w.ShowAll ();
		Application.Run ();
	}

	static void OnClick (object obj, EventArgs args)
	{
		Application.Quit ();
	}

	static void OnDelete (object obj, DeleteEventArgs args)
	{
		Application.Quit ();
	}
}

