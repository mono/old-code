// menubar2.cs - Gtk# Tutorial example
// 
// Author: Alejandro Sánchez Acosta <asanchez@gnu.org>
// 	   Carlos Alberto Cortez <is118149@mail.udlap.mx>
// 	   
// (C) 2003 Alejandro Sánchez Acosta
// (C) 2003 Carlos Alberto Cortez

using System;
using Gtk;
using GtkSharp;

public class MenuTest {

	Window win;
	VBox vbox;
	MenuBar menuBar;
	Menu fileMenu;
	MenuItem fileItem;
	MenuItem openItem;
	MenuItem saveItem;
	MenuItem quitItem;

	public MenuTest () 
	{
		win = new Window("Gtk Menu Test");
		win.DeleteEvent += new DeleteEventHandler(OnDeleteEvent);

		menuBar = new MenuBar();
		menuBar.Show();

		fileMenu = new Menu();

		fileItem = new MenuItem("File");
		fileItem.Show();

		// Create the menu items
		openItem = new MenuItem("Open");
		saveItem = new MenuItem("Save");
		quitItem = new MenuItem("Quit");

		// Add them to the menu
		fileMenu.Append(openItem);
		fileMenu.Append(saveItem);
		fileMenu.Append(quitItem);

		// Atach the callback function to the active signal
		openItem.Activated += new EventHandler(OnActiveEvent);
		saveItem.Activated += new EventHandler(OnActiveEvent);
		quitItem.Activated += new EventHandler(OnExitMenuActivated);

		openItem.Show();
		saveItem.Show();
		quitItem.Show();

		fileItem.Submenu = fileMenu;

		menuBar.Append(fileItem);

		win.Add(menuBar);
		win.Show();
	}

	public void OnDeleteEvent ( object obj, DeleteEventArgs args )
	{
		Application.Quit();
	}

	public void OnExitMenuActivated ( object obj, EventArgs args )
	{
		Application.Quit();
	}

	public void OnActiveEvent ( object obj, EventArgs args )
	{
		Console.WriteLine("A menu was activated");
	}

	public static void Main ( string[] args ) {
		Application.Init();
		new MenuTest();
		Application.Run();
	}
}
