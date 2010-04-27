using Gtk;

public class MainClass 
{	
	public static void Main (string [] args)
	{
		Application.Init ();
		SetUpGui ();
		Application.Run ();
	}
	
	static void SetUpGui ()
	{
		Window w = new Window ("Sign Up");
		
		VBox vbox = new VBox ();
		
		Button b = new Button ("Testing");
		vbox.PackStart (b, false, false, 0);
		
		b = new Button ("Testing II");
		vbox.PackStart (b, false, false, 0);
				
		ScrolledWindow sw = new ScrolledWindow ();
		
		sw.AddWithViewport (vbox);
		
		w.Add (sw);
//		w.SetDefaultSize (12, 12);
		w.ShowAll ();
	}
}
