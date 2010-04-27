using Enlightenment.Eblocks;

public class MainClass 
{
	private static Entry appname_entry, geninfo_entry, comments_entry,
	  exe_entry, winname_entry, winclass_entry;
	
	public static void Main (string [] args)
	{
		Application.Init ();
		SetUpGui ();
		Application.Run ();
	}
	
	static void SetUpGui ()
	{
		Window w = new Window ("Eap Editor");
		
		appname_entry = new Entry ();
		geninfo_entry = new Entry ();
		comments_entry = new Entry ();
		exe_entry = new Entry ();
		winname_entry = new Entry ();
		winclass_entry = new Entry ();
		
		VBox outerv = new VBox ();
		outerv.BorderWidth = 12;
		outerv.Spacing = 12;
		w.Add (outerv);
		
		HBox h = new HBox ();
		outerv.PackStart (h, false, false, 0);
		
		Button b = new Button ("Select Icon");
		h.PackStart (b, true, false, 0);
		
		h = new HBox ();
		h.Spacing = 6;
		outerv.PackStart (h);
		
		VBox v = new VBox ();
		v.Spacing = 6;
		h.PackStart (v, false, false, 0);
		
		b = new Button ("App name:");
		v.PackStart (b, true, false, 0);
		
		b = new Button ("Generic Info:");
		v.PackStart (b, true, false, 0);
		
		b = new Button ("Comments:");
		v.PackStart (b, true, false, 0);

		b = new Button ("Executable:");
		v.PackStart (b, true, false, 0);
		
		b = new Button ("Window Name:");
		v.PackStart (b, true, false, 0);
		
		b = new Button ("Window Class:");
		v.PackStart (b, true, false, 0);
		
		b = new Button ("Startup notify:");
		v.PackStart (b, true, false, 0);
		
		b = new Button ("Wait Exit:");
		v.PackStart (b, true, false, 0);
		
		v = new VBox ();
		v.Spacing = 6;
		h.PackStart (v, true, true, 0);
		
		v.PackStart (appname_entry, true, true, 0);
		v.PackStart (geninfo_entry, true, true, 0);
		v.PackStart (comments_entry, true, true, 0);
		v.PackStart (exe_entry, true, true, 0);
		v.PackStart (winname_entry, true, true, 0);
		v.PackStart (winclass_entry, true, true, 0);
		//v.PackStart (new Entry(), true, true, 0);
		//v.PackStart (new Entry(), true, true, 0);
		
		CheckButton start_cbox = new CheckButton ();
		v.PackStart (start_cbox);
		
		CheckButton wait_cbox= new CheckButton ();
		v.PackStart (wait_cbox);		
		
		h = new HBox ();
		h.Spacing = 0;
		outerv.PackStart (h);
		
		v = new VBox ();		
		b = new Button ("Save");
		v.PackStart (b, true, false, 0);
		h.PackStart (v, true, false, 0);
		
		v = new VBox ();		
		b = new Button ("Cancel");
		v.PackStart (b, true, false, 0);
		h.PackStart (v, true, false, 0);
		
		w.ShowAll ();
	}
}
