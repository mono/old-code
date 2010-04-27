using Enlightenment.Eblocks;

public class MainClass 
{
	private static Entry appname_entry, geninfo_entry, comments_entry,
	  exe_entry, winname_entry, winclass_entry;

	public static void toggle_start_cbox (object o, object ev)
	{
		CheckButton cb = (CheckButton)o;
		System.Console.WriteLine ("toggled start check box: {0}", cb.Active);
	}
	
	public static void toggle_wait_cbox (object o, object ev)
	{
		CheckButton cb = (CheckButton)o;
		System.Console.WriteLine ("toggled wait check box: {0}", cb.Active);
	}	
	
	public static void Main (string [] args)
	{
		Application.Init ();
		SetUpGui ();
		Application.Run ();
	}
	
	static void SetUpGui ()
	{
		Button b;
		
		Window w = new Window ("Eap Editor");
		//w.BorderWidth = 10;
		Table tableLayout = new Table(10, 2, false);
		//tableLayout.BorderWidth = 6;
		//tableLayout.ColumnSpacing = 6;
		//tableLayout.RowSpacing = 6;
		
		Image im = new Image ("data/icon.png");
//		b = new Button (im);
		tableLayout.Attach (im, 0, 1, 0, 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
								
		b = new Button ("App name:");
		tableLayout.Attach (b, 0, 1, 1, 2, AttachOptions.Fill, AttachOptions.Expand, 0, 0);
		
		b = new Button ("Generic Info:");
		tableLayout.Attach (b, 0, 1, 2, 3, AttachOptions.Fill, AttachOptions.Expand, 0, 0);
		
		b = new Button ("Comments:");
		tableLayout.Attach (b, 0, 1, 3, 4, AttachOptions.Fill, AttachOptions.Expand, 0, 0);		

		b = new Button ("Executable:");
                tableLayout.Attach (b, 0, 1, 4, 5, AttachOptions.Fill, AttachOptions.Expand, 0, 0);		
		
		b = new Button ("Window Name:");
                tableLayout.Attach (b, 0, 1, 5, 6, AttachOptions.Fill, AttachOptions.Expand, 0, 0);		
		
		b = new Button ("Window Class:");
                tableLayout.Attach (b, 0, 1, 6, 7, AttachOptions.Fill, AttachOptions.Expand, 0, 0);		
		
		b = new Button ("Startup notify:");
                tableLayout.Attach (b, 0, 1, 7, 8, AttachOptions.Fill, AttachOptions.Expand, 0, 0);		
		
		b = new Button ("Wait Exit:");
                tableLayout.Attach (b, 0, 1, 8, 9, AttachOptions.Fill, AttachOptions.Expand, 0, 0);
	
		b = new Button ("Select Icon");
		tableLayout.Attach (b, 1, 2, 0, 1, AttachOptions.Expand, AttachOptions.Expand, 0, 0);
				
		appname_entry = new Entry ();
                tableLayout.Attach (appname_entry, 1, 2, 1, 2);
		
		geninfo_entry = new Entry ();
                tableLayout.Attach (geninfo_entry, 1, 2, 2, 3);
		
		comments_entry = new Entry ();
                tableLayout.Attach (comments_entry, 1, 2, 3, 4);
		
		exe_entry = new Entry ();
                tableLayout.Attach (exe_entry, 1, 2, 4, 5);
		
		winname_entry = new Entry ();
                tableLayout.Attach (winname_entry, 1, 2, 5, 6);
		
		winclass_entry = new Entry ();
                tableLayout.Attach (winclass_entry, 1, 2, 6, 7);
		
		CheckButton start_cbox = new CheckButton ();
		start_cbox.Toggled += toggle_start_cbox;
                tableLayout.Attach (start_cbox, 1, 2, 7, 8);
		
		CheckButton wait_cbox= new CheckButton ();
		wait_cbox.Toggled += toggle_wait_cbox;
                tableLayout.Attach (wait_cbox, 1, 2, 8, 9);	
		
		HBox h = new HBox ();
		h.Spacing = 0;
		tableLayout.Attach (h, 0, 2, 9, 10);
		
		VBox v = new VBox ();		
		b = new Button ("Save");
		v.PackStart (b, true, false, 0);
		h.PackStart (v, true, false, 0);
		
		v = new VBox ();		
		b = new Button ("Cancel");
		v.PackStart (b, true, false, 0);
		h.PackStart (v, true, false, 0);
		
		w.Add (tableLayout);
		w.ShowAll ();
	}
}
