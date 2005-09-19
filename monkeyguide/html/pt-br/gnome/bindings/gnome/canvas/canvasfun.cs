using System;
using Gtk;
using GtkSharp;
using Gnome;

class CanvasTest {


public static void Main()
{

Application.Init();

Window window1 = new Window("test");
window1.DeleteEvent += new DeleteEventHandler (delete_event);
			

	Canvas canvas1 = Canvas.NewAa();
	CanvasGroup root = canvas1.Root();

	CanvasEllipse item1;

	for (double i=0.1; i<10;i = i + 0.5) {
	item1 = new CanvasEllipse(root);

	item1.X1 = i;
	item1.X2 = i*20;
	item1.Y1 = i;
	item1.Y2 = i*20;
	item1.OutlineColor = "#000000";
	item1.WidthPixels = 1;
	item1.Show();
	}


	canvas1.Show();

window1.Add(canvas1);
window1.Show();

Application.Run();
}

		static void delete_event (object obj, DeleteEventArgs args)
		{
			    Application.Quit ();
		}

}
