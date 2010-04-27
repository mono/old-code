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
		
		Button b = new Button ("Testing");
		
		Frame f = new Frame ("My Frame");
		
		f.Add (b);
		
		w.Add (f);
		//w.SetDefaultSize (120, 120);
		w.ShowAll ();
	}
}
