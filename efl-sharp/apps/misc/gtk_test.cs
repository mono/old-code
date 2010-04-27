using Gtk;

public class EblocksTest
{
	public static void Button_Clicked (object o, object args)
	{
		System.Console.WriteLine ("Button Clicked!");
	}

	public static void FileOpenHandler (object o, object args)
	{
		System.Console.WriteLine ("File->Open Clicked");
	}
	
	public static void FileQuitHandler (object o, object args)
	{
		Application.Quit();
	}
	
	public static void AboutHelpHandler (object o, object args)
	{
		System.Console.WriteLine ("Show Help Window");
	}
	
	public static void AboutAuthorsHandler (object o, object args)
	{
		System.Console.WriteLine ("About Authors");
	}	
	
	public static void Main(string [] args)
	{
		Application.Init();

		Window w = new Window("EFL# Demo App");
		
		VBox vbx = new VBox ();
		vbx.Spacing = 4;
		
		MenuBar mb = new MenuBar();
		
		vbx.PackStart (mb, false, false, 0);
		
		MenuItem item = new MenuItem ("File");
		Menu file_menu = new Menu ();
		item.Submenu = file_menu;
		
		MenuItem file_item = new MenuItem ("Open");
		file_item.Activated += FileOpenHandler;
		file_menu.Append (file_item);
		
		file_item = new MenuItem ("Close");
		file_menu.Append (file_item);
		
		file_item = new MenuItem ("Save");
		file_menu.Append (file_item);
		
		file_item = new MenuItem ("Save As");
                file_menu.Append (file_item);
		
		file_item = new MenuItem ("Quit");
		file_item.Activated += FileQuitHandler;		
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
		about_item.Activated += AboutHelpHandler;
		about_menu.Append (about_item);
		
		about_item = new MenuItem ("Authors");
		about_item.Activated += AboutAuthorsHandler;
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

		l5.Clicked +=  Button_Clicked;
		
		bx.PackStart(l1);
		bx.PackStart(l2);
		bx.PackStart(l3);
		bx.PackStart(l4);
		bx.PackStart(l5);
		
		Label input_label = new Label ("What is your name?");
		input_label.Xalign = 0.5f;
		input_label.Yalign = 1;
		vbx.PackStart (input_label);
		Entry input = new Entry ();
		vbx.PackStart (input);
		
		w.Add(vbx);
		//w.SetDefaultSize(300, 200);
		w.ShowAll();
		
		Application.Run();
	}
}
