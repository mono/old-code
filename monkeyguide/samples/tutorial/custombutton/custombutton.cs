// menubar.cs - Gtk# Tutorial example
// 
// Author: Alejandro Sánchez Acosta <asanchez@gnu.org>
// 	   Carlos Alberto Cortez <is118149@mail.udlap.mx>
// 	   
// (C) 2003 Alejandro Sánchez Acosta
// (C) 2003 Carlos Alberto Cortez


using System;
using Gtk;
using GtkSharp;

public class CustomButton {

	private static void Callback ( object obj, EventArgs args )
	{
		Button button = (Button) obj;
		Console.WriteLine("Hello again! the button Cool Button was clicked again." );
	}

	private static HBox XpmLabelBox ( string xpmFilename, string labelText )
	{
		HBox box;
		Label label;
		Image image;

		box = new HBox (false, 0);
		box.BorderWidth = 2;

		image = new Image(xpmFilename);

		label = new Label(labelText);

		box.PackStart(image, false, false, 3);
		box.PackStart(label, false, false, 3);

		image.Show();
		label.Show();

		return box;
	}

	private static void OnWindowDelete (object obj, DeleteEventArgs args )
	{
		Application.Quit();
	}

	public static void Main ( string[] args )
	{
		Window window;
		Button button;
		HBox hbox;
		
		Application.Init();

		window = new Window("Pixmap'd Buttons!");
		window.DeleteEvent += new DeleteEventHandler(OnWindowDelete);
		window.BorderWidth = 10;

		button = new Button();
		button.Clicked += new EventHandler(Callback);

		hbox = XpmLabelBox("image.xpm", "Cool Button");
		hbox.Show();

		button.Add(hbox);
		button.Show();

		window.Add(button);
		window.Show();

		Application.Run();		
	}
}
