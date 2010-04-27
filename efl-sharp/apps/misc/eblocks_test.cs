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
		
		Window w = new Window ("Eblocks Test Suite");
		
		Button b;
		
		Table tableLayout = new Table (3, 1, false);
		w.Add (tableLayout);
		
		b = new Button ("Buttons");
		b.Clicked += TestBoxesButtonMenus;
		tableLayout.Attach (b, 0, 1, 0, 1);
		
		b = new Button ("Scale");
		b.Clicked += TestScale;
		tableLayout.Attach (b, 0, 1, 1, 2);
		
		b = new Button ("Entry");
		b.Clicked += TestEntryLabel;
		tableLayout.Attach (b, 0, 1, 2, 3);
		
		w.ShowAll ();
	}
	
	
	
	
	
	
	
	static Entry input;
	
	public static void TestLabel_GetText (object o, EventArgs args)
	{
		System.Console.WriteLine ("Text is: {0}", input.Text);
	}	

	/****************************
	 * Box / Button / Menu Test *
	 ****************************/	
	
	public static void TestButtons_ButtonClicked (object o, EventArgs args)
	{
		System.Console.WriteLine ("Button Clicked!");
	}

	public static void TestButtons_FileOpenHandler (object o, EventArgs args)
	{
		System.Console.WriteLine ("File->Open Clicked");
	}
	
	public static void TestButtons_FileQuitHandler (object o, EventArgs args)
	{
		Application.Quit();
	}
	
	public static void TestButtons_AboutHelpHandler (object o, EventArgs args)
	{
		System.Console.WriteLine ("Show Help Window");
	}
	
	public static void TestButtons_AboutAuthorsHandler (object o, EventArgs args)
	{
		System.Console.WriteLine ("About Authors");
	}	
	
	public static void TestBoxesButtonMenus(object o, object eventargs)
	{
		Window w = new Window("Boxes, Buttons & Menus Test");
		
		VBox vbx = new VBox ();
		vbx.Spacing = 4;
		
		MenuBar mb = new MenuBar();
		
		vbx.PackStart (mb, false, false, 0);
		
		MenuItem item = new MenuItem ("File");
		Menu file_menu = new Menu ();
		item.Submenu = file_menu;
		
		MenuItem file_item = new MenuItem ("Open");
		file_item.Activated += TestButtons_FileOpenHandler;
		file_menu.Append (file_item);
								
		file_item = new MenuItem ("Close");
		file_menu.Append (file_item);
		Menu close_menu = new Menu ();
		file_item.Submenu = close_menu;
		MenuItem close_menu_item = new MenuItem ("Close This");
		close_menu.Append (close_menu_item);
		close_menu_item = new MenuItem ("Close All");
		close_menu.Append (close_menu_item);
								
		file_item = new MenuItem ("Save");
		file_menu.Append (file_item);
		
		file_item = new MenuItem ("Save As");
                file_menu.Append (file_item);
		
		file_item = new MenuItem ("Quit");
		file_item.Activated += TestButtons_FileQuitHandler;
                file_menu.Append (file_item);
		
		mb.Append (item);
		
		item = new MenuItem ("Edit");
		Menu edit_menu = new Menu ();
		item.Submenu = edit_menu;
		mb.Append (item);
		
		item = new MenuItem ("About");
		Menu about_menu = new Menu ();		
		item.Submenu = about_menu;
		
		MenuItem about_item = new MenuItem ("Help");
		about_item.Activated += TestButtons_AboutHelpHandler;
		about_menu.Append (about_item);
		
		about_item = new MenuItem ("Authors");
		about_item.Activated += TestButtons_AboutAuthorsHandler;
		about_menu.Append (about_item);
		
		mb.Append (item);
				
		HBox bx = new HBox ();
		
		vbx.PackStart (bx);
		
		bx.Spacing = 0;
		Button l1 = new Button ("one");				
		Button l2 = new Button ("two");
		Button l3 = new Button ("three");
		Button l4 = new Button ("four");
		Button l5 = new Button ("five");		

		l5.Clicked +=  TestButtons_ButtonClicked;
		
		bx.PackStart(l1);
		bx.PackStart(l2);
		bx.PackStart(l3);
		bx.PackStart(l4);
		bx.PackStart(l5);

/*		
		HBox inbox = new HBox ();
		inbox.Spacing = 5;
		inbox.BorderWidth = 2;
		vbx.PackStart (inbox);
		Label input_label = new Label ("What is your name?");
		input_label.Xalign = 0;
		inbox.PackStart (input_label, false, false, 0);
		input = new Entry ();
		inbox.PackStart (input);
		
		HBox bbox = new HBox ();
		Button get_text = new Button ("Get Text");
		get_text.Clicked += Get_Text;
		bbox.PackStart (get_text, false, false, 0);
		vbx.PackStart (bbox, false, false, 0);
*/		
		w.Add(vbx);
		w.SetDefaultSize(300, 200);
		w.ShowAll();			
	}								
	

	/**************
	 * SCALE TEST *
	 **************/ 
	
	static Label scale_label;
	
	public static void TestScale_HscaleValueChanged (object o, object ev)
	{
		HScale hscale = (HScale)o;
		
		scale_label.Text = hscale.Value.ToString();
	}
		
	static void TestScale (object o, object eventargs)
	{
		Window w = new Window ("Scale Test");
		
		VBox vbox = new VBox ();
		
		HScale hscale = new HScale (1, 100, 10);
		hscale.ValueChanged += TestScale_HscaleValueChanged;
		hscale.Value = 50;
		
		scale_label = new Label (hscale.Value.ToString());
				
		vbox.PackStart (scale_label, true, false, 0);
		vbox.PackStart (hscale, true, false, 0);
		
		w.Add (vbox);
		w.SetDefaultSize (160, 120);		
		w.ShowAll ();
	}	
	
	/**********************
	 * Entry / Label Test *
	 **********************/
	
	private static Entry firstname_entry, lastname_entry, email_entry;
		
	static void TestEntryLabel (object o, object eventargs)
	{
		Window w = new Window ("Entry & Label Test");
		
		firstname_entry = new Entry ();
		lastname_entry = new Entry ();
		email_entry = new Entry ();
		
		VBox outerv = new VBox ();
		//outerv.BorderWidth = 12;
		//outerv.Spacing = 12;
		w.Add (outerv);
		
		Label l = new Label ("Enter your name and preferred address");
		l.Xalign = 0;
		//l.UseMarkup = true;
		outerv.PackStart (l, false, false, 0);
		
		HBox h = new HBox ();
		h.Spacing = 6;
		outerv.PackStart (h);
		
		VBox v = new VBox ();
		v.Spacing = 6;
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
		v.Spacing = 6;
		h.PackStart (v, true, true, 0);
		
		v.PackStart (firstname_entry, true, true, 0);
		v.PackStart (lastname_entry, true, true, 0);
		v.PackStart (email_entry, true, true, 0);
		
		w.ShowAll ();
	}	
}
