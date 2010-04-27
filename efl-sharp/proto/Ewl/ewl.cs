using System;
using Ewl;

public class HelloWorld {

	Application app;
	Window win;
	Entry lbl;
	
	public HelloWorld() {
	
		win = new Window();
		win.Title = "Hello World";
		win.Name = "EWL_WINDOW";
		win.Class = "EWLWindow";
		win.SizeRequest(200, 100);
		//int i = EventType.VALUE_CHANGED;
		//Console.WriteLine(i);
		win.DeleteEvent += new DeleteEventHandler(winDelete);
		
		win.Show();
		
		lbl = new Entry();
		//lbl.SetFont("/usr/share/fonts/ttf/western/Adeventure.ttf", 12);
		//lbl.SetColor(255, 0 , 0 , 255);
		//lbl.Style = "soft_shadow";
		//lbl.SizeRequest(win.Width, win.Height);
		//lbl.Disable();
		lbl.ChangedEvent += new EwlEventHandler(txtChanged);
		lbl.Text = "Enlightenment";
		
		Console.WriteLine(lbl.Text);
		Console.WriteLine(lbl.Text);
		
		lbl.TabOrderPush();
		
		win.Append(lbl);

		lbl.Show();
	
	}

	void txtChanged(object w, object evnt){
		Console.WriteLine(lbl.Text);
	}
	
	public static void Main(string []args) {
		
		if (Application.Init(args) == 0) return;
		
		new HelloWorld();
		
		Application.Main();
		
	}
	
	void winDelete(IntPtr w, IntPtr evnt, IntPtr data) {

		win.Destroy();
		Application.Quit();
	
	}
		
}
