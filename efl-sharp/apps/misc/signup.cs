using Enlightenment.Eblocks;

public class MainClass 
{
	private static Entry firstname_entry, lastname_entry, email_entry;
	
	public static void Main (string [] args)
	{
		Application.Init ();
		SetUpGui ();
		Application.Run ();
	}
	
	static void SetUpGui ()
	{
		Window w = new Window ("Sign Up");
		
		firstname_entry = new Entry ();
		lastname_entry = new Entry ();
		email_entry = new Entry ();
		
		VBox outerv = new VBox ();
		outerv.BorderWidth = 12;
		outerv.Spacing = 12;
		w.Add (outerv);
		
		Label l = new Label ("Enter your name and preferred address");
		l.Xalign = 0;
		//l.UseMarkup = true;
		outerv.PackStart (l, false, false, 0);
		
		HBox h = new HBox ();
		//h.Spacing = 6;
		outerv.PackStart (h);
		
		VBox v = new VBox ();
		//v.Spacing = 6;
		h.PackStart (v, false, false, 0);
		
		Button l2;
		l2 = new Button ("First Name:");
		//l.Xalign = 0;
		v.PackStart (l2, true, false, 0);
		//l.MnemonicWidget = firstname_entry;
		
		l2 = new Button ("Last Name:");
		//l.Xalign = 0;
		v.PackStart (l2, true, false, 0);
		//l.MnemonicWidget = firstname_entry;
		
		l2 = new Button ("Email Address:");
		//l.Xalign = 0;
		v.PackStart (l2, true, false, 0);
		//l.MnemonicWidget = firstname_entry;

		v = new VBox ();
		//v.Spacing = 6;
		h.PackStart (v, true, true, 0);
		
		v.PackStart (firstname_entry, true, true, 0);
		v.PackStart (lastname_entry, true, true, 0);
		v.PackStart (email_entry, true, true, 0);
		
		w.ShowAll ();
	}
}
