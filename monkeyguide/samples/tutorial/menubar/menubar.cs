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

public class MenuClass {

	Window window;
	Menu menu;
	MenuBar menuBar;
	MenuItem rootMenu;
	MenuItem menuItems;
	VBox vbox;
	Button button;
	string bufs;

	public MenuClass () 
	{
		/* Create the window */
		window = new Window(Gtk.WindowType.Toplevel);
		window.SetSizeRequest(200, 200);
		window.Title = "Gtk# Menu Test";
		window.DeleteEvent += new DeleteEventHandler (OnWindowDeleteEvent);

		/* Init the menu-widget */
		menu = new Menu();

		/* Next we make a little loop that make three menu entries */
		for ( int i =0; i<3; i++) {
			/* Copy the names to the buf */
			bufs = "Test-undermenu - " + i;

			/* Create a new menu item with a name */
			menuItems = new MenuItem(bufs);

			/* And add it to the menu */
			menu.Append(menuItems);

			/* Do something interesting when the menu item is selected */
			menuItems.Activated += new EventHandler (MenuItemResponse);

			menuItems.Show();
		}

		/* This is the root menu */
		rootMenu = new MenuItem("Root Menu");
		rootMenu.Show();

		/* Now we specify what we want our newly created "menu"
		    to be the menu for the "root menu" 			*/
		rootMenu.Submenu = menu;

		/* A VBox to put a menu and a button in */
		vbox = new VBox(false, 0);
		window.Add(vbox);
		vbox.Show();

		/* Create the menu bar to hold the menus */
		menuBar = new MenuBar();
		vbox.PackStart(menuBar, false, false, 2);
		menuBar.Show();

		/* Create a button to which to attach menu as popup*/
		button = new Button("Press me!");
		//button.Event += new EventHandler(OnButtonPress);
		vbox.PackStart(button, true, true, 2);
		button.Show();

		/* And finally we append the menu item to the menu bar */
		menuBar.Append(rootMenu);

		window.Show();
	}

	public void OnWindowDeleteEvent ( object obj, DeleteEventArgs args )
	{
		Application.Quit();
	}

	public void OnButtonPress ( object obj, EventArgs args )
	{
	}

	public void MenuItemResponse ( object obj, EventArgs args )
	{
	}

	public static void Main ( string[] args ) 
	{
		Application.Init();

		new MenuClass();
		
		Application.Run();
	}
}
