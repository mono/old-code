using Enlightenment.Eblocks;

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
		w.BorderWidth = 10;
		
		Button b = new Button ("Testing");
		b.BorderWidth = 10;		
		w.Add (b);
		//w.SetDefaultSize (120, 120);
		w.ShowAll ();
	}
}
