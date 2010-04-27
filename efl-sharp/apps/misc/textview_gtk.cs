using System;
using Gtk;

class TextViewSample
{
   static void Main ()
   {
	   new TextViewSample ();
   }
   
   TextViewSample ()
   {
	   Application.Init ();
	   Window win = new Window ("TextViewSample");
	   win.DeleteEvent += new DeleteEventHandler (OnWinDelete);
	   win.SetDefaultSize (600,400);
	   
	   Gtk.TextView view;
	   Gtk.TextBuffer buffer;
	   
	   view = new Gtk.TextView ();
	   buffer = view.Buffer;
	   
	   buffer.Text = "Hello, this is some text";
	   
	   win.Add (view);
	   win.ShowAll ();
	   
	   Application.Run ();
   }
   
   void OnWinDelete (object obj, DeleteEventArgs args)
   {
	   Application.Quit ();
   }
}
