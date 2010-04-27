using Gtk;

public class MainClass 
{	
	public static void hscale_value_changed_cb (object o, object ev)
	{
		HScale hscale = (HScale)o;
		System.Console.WriteLine ("Value changed to {0}!", hscale.Value);
	}
	
	public static void Main (string [] args)
	{
		Application.Init ();
		SetUpGui ();
		Application.Run ();
	}
	
	static void SetUpGui ()
	{
		Window w = new Window ("Scale Test");
		
		HScale hscale = new HScale (1, 100, 10);
		hscale.ValueChanged += hscale_value_changed_cb;
		hscale.Value = 50;
		
		w.Add (hscale);
		w.SetDefaultSize (160, 120);
		w.ShowAll ();
	}
}
