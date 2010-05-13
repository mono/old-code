//
// AppWindowManager.cs: 
//   Handes basic GTK+ window handling (window menu, etc.)
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002-2004 Jonathan Pryor
//

using System;
using System.Collections;
using System.Diagnostics;

using Gtk;
using GtkSharp;
using Glade;

namespace Mono.TypeReflector.Displayers.gtk
{
	public sealed class AppWindowInfo
	{
		private Window app_window;
		private MenuItem app;
		private MenuItem window;
		private CheckMenuItem fullscreen;
		private CheckMenuItem maximize;
		private string title;

		public AppWindowInfo ()
		{
		}

		public AppWindowInfo (Window appWindow, MenuItem app, MenuItem window, CheckMenuItem fullscreen, CheckMenuItem maximize)
		{
			this.app_window = appWindow;
			this.app = app;
			this.window = window;
			this.fullscreen = fullscreen;
			this.maximize = maximize;
		}

		public Window AppWindow {
			get {return app_window;}
			set {
				app_window = value;
				app_window.WindowStateEvent += 
					new WindowStateEventHandler (OnWindowStateEvent);
			}
		}

		public MenuItem AppMenu {
			get {return app;}
			set {app = value;}
		}

		public MenuItem WindowMenu {
			get {return window;}
			set {window = value;}
		}

		public CheckMenuItem FullscreenMenu {
			get {return fullscreen;}
			set {fullscreen = value;}
		}

		public CheckMenuItem MaximizeMenu {
			get {return maximize;}
			set {maximize = value;}
		}
		
		public string Title {
			get {return title;}
		}

		internal void SetTitle (string title)
		{
			this.title = title;
		}

		private void OnWindowStateEvent (object sender, WindowStateEventArgs e)
		{
			Console.WriteLine ("state event: type=" + e.Event.Type + 
				"; new_window_state=" + e.Event.NewWindowState);

			if (e.Event.Type != Gdk.EventType.WindowState)
				return;

			switch (e.Event.NewWindowState) {
			case Gdk.WindowState.Maximized:
				MaximizeMenu.Active = true;
				FullscreenMenu.Active = false;
				break;
			case Gdk.WindowState.Fullscreen:
				FullscreenMenu.Active = true;
				MaximizeMenu.Active = false;
				break;
			case (Gdk.WindowState.Fullscreen | Gdk.WindowState.Maximized):
				MaximizeMenu.Active = true;
				FullscreenMenu.Active = true;
				break;
			case (Gdk.WindowState) 0:
				FullscreenMenu.Active = false;
				MaximizeMenu.Active = false;
				break;
			default:
				// ignore
				break;
			}
		}

		internal void OnPresentWindow (object sender, EventArgs e)
		{
			AppWindow.Present ();
		}
	}

	public sealed class AppWindowManager
	{
		// type: List<AppWindowInfo>
		private IList windows = new ArrayList ();

		private int window_offset;

		private string appName;

		public string AppName {
			get {return appName;}
			set {appName = value;}
		}

		public AppWindowManager (string appName)
		{
			this.appName = appName;
		}

		public void Add (AppWindowInfo window)
		{
			if (windows.Count == 0) {
				Application.Init ();

				window_offset = ((Menu) window.WindowMenu.Submenu).Children.Length - 1;
			}

			foreach (Label l in window.AppMenu.Children) {
				l.UseMarkup = true;
				l.UseUnderline = true;
				l.MarkupWithMnemonic = 
					string.Format ("<span weight=\"heavy\">{0}</span>", AppName);
			}

			InitWindowMenu (window);
			AddWindowMenu (window, "unknown");
		}

		public void Remove (AppWindowInfo window)
		{
			int me = windows.IndexOf (window);
			windows.RemoveAt (me);

			SyncWindowMenu ();

			window.AppWindow.Destroy ();

			if (windows.Count == 0)
				Application.Quit ();
		}

		public void ActivateNext (AppWindowInfo win)
		{
			int cur = windows.IndexOf (win);
			int next = cur+1;
			if (next == windows.Count)
				next = 0;
			AppWindowInfo w = (AppWindowInfo) windows[next];
			w.AppWindow.Present ();
		}

		public void ActivatePrevious (AppWindowInfo win)
		{
			int cur = windows.IndexOf (win);
			int prev = cur-1;
			if (prev == -1)
				prev = windows.Count - 1;
			AppWindowInfo w = (AppWindowInfo) windows[prev];
			w.AppWindow.Present ();
		}

		public void AllToFront (AppWindowInfo main)
		{
			foreach (AppWindowInfo w in windows)
				w.AppWindow.Present ();
			main.AppWindow.Present ();
		}

		public void ShowAll (AppWindowInfo main)
		{
			foreach (Window w in Window.ListToplevels()) {
				w.Present ();
			}
			main.AppWindow.Present ();
		}

		public void HideAll ()
		{
			foreach (AppWindowInfo w in windows)
				w.AppWindow.Iconify ();
		}

		public void HideOthers (AppWindowInfo main)
		{
			foreach (Window w in Window.ListToplevels())
				w.Iconify ();
			AllToFront (main);
		}

		public void SetTitle (AppWindowInfo window, string title)
		{
			window.SetTitle (title);
			SyncWindowMenu ();
		}

		private void InitWindowMenu (AppWindowInfo window)
		{
			Console.WriteLine ("creating window menu");
			Menu m = (Menu) window.WindowMenu.Submenu;
			int i = 0;
			foreach (AppWindowInfo w in windows) {
				MenuItem r = CreateMenuItem (w.Title, i++);
				m.Append (r);
			}
			window.WindowMenu.ShowAll ();
		}

		private MenuItem CreateMenuItem (string title, int accel)
		{
			// Console.WriteLine ("creating menu item with accel: accel=" + 
			// 		accel.ToString() + "; # windows=" + s_windows.Count.ToString());
			MenuItem r = new MenuItem (title);
			UpdateMenuItem (r, title, accel);
			return r;
		}

		private void AddWindowMenu (AppWindowInfo w, string title)
		{
			Console.WriteLine ("adding window menu: " + title);
			windows.Add (w);
			int n = windows.IndexOf (w);
			foreach (AppWindowInfo d in windows) {
				Menu m = (Menu) d.WindowMenu.Submenu;
				MenuItem mi = CreateMenuItem (title, n);
				m.Append (mi);
				d.WindowMenu.ShowAll ();
			}
		}


		private void SyncWindowMenu ()
		{
			foreach (AppWindowInfo w in windows) {
				Menu m = (Menu) w.WindowMenu.Submenu;

				// Console.WriteLine ("  # menu items: " + m.Children.Count);

				IEnumerator e = m.Children.GetEnumerator ();
				int cur = 0;

				// Move past non-variable menu items
				while (e.MoveNext() && cur != window_offset)
					++cur;

				// We should now be at the variable portion of the Window menu.
				// Update the titles.
				while (e.MoveNext() && (cur - window_offset) != windows.Count) {
					++cur;
					MenuItem mi = (MenuItem) e.Current;
					int window = cur-window_offset-1;
					// Console.WriteLine ("  syncing menu item: " + cur.ToString() +
					// 		"; is now=" + s_assemblies[window].ToString());
					UpdateMenuItem (mi, ((AppWindowInfo) windows[window]).Title, window);
				}

				// Any extra menu items should be removed, probably because the window
				// was closed
				if (++cur < m.Children.Length)
					do {
						// Console.WriteLine ("removing widget... " + cur.ToString());
						m.Remove ((Widget) e.Current);
						++cur;
					} while (e.MoveNext());

				w.WindowMenu.ShowAll ();
			}
		}

		private void UpdateMenuItem (MenuItem mi, string title, int accel)
		{
			((Label) mi.Child).Text = title;
			/*
			foreach (Label l in mi.Children) {
				// Console.WriteLine ("    updating menu item; was={0}; is={1}",
				// 		l.Text, title);
				l.Text = title;
			}
			 */
			AppWindowInfo d = (AppWindowInfo) windows[accel];
			// TODO: clear out mi.Activated, so we don't have lots of windows try to
			// present themselves...
			mi.Activated += new EventHandler (d.OnPresentWindow);
		}
	}
}

// vim: noexpandtab
