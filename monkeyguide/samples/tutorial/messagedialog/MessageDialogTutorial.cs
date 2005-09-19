// GnomeAbout.cs - Gtk# Tutorial example
//
// Author: Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (c) 2003 Martin Willemoes Hansen

using System;
using Gtk;
using GtkSharp;
 
namespace GtkSharpTutorial {

        public class MessageDialogTutorial {

		static void Main()
		{
			Application.Init();
			Gtk.Window main = new Gtk.Window ("MessageDialog");
			main.DeleteEvent += new DeleteEventHandler (Quit);
			main.SetPosition (WindowPosition.CenterAlways);
			main.Show();

			MessageDialog dialog = new MessageDialog (main, DialogFlags.DestroyWithParent, 
								  MessageType.Question, ButtonsType.YesNo, 
								  "Do you want to destroy this dialog?");
			dialog.Response += new ResponseHandler (Response);
			dialog.Show();

			Application.Run();
		}

		static void Response (object sender, ResponseArgs args)
		{
			switch ((ResponseType)args.ResponseId) {
			case ResponseType.Yes: 
				Console.WriteLine ("Response is: Yes");
				break;
			case ResponseType.No:
				Console.WriteLine ("Response is: No");
				break;
			case ResponseType.DeleteEvent:
				Console.WriteLine ("Dialog destroyed");
				break;
			default:
				Console.WriteLine (args.ResponseId);
				break;
			}
		}

		static void Quit (object sender, DeleteEventArgs args)
		{
			Application.Quit();
		}
	}
}
