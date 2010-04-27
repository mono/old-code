using Enlightenment.Eblocks;
using System;
using System.Collections;
using System.IO;

public class ImageViewer
{
	static void Main (string [] args)
	{
		Application.Init ();

		if (args.Length <= 0) {
			Console.WriteLine ("\nUSAGE: ImageBrowser.exe <directory>\n");
			return;
		}
	
		string dir = args [0];

		Window window = new Window ("Image Browser");
		ScrolledWindow scroll = new ScrolledWindow ();//(new Adjustment (IntPtr.Zero), new Adjustment (IntPtr.Zero));

		ArrayList images = GetItemsFromDirectory (dir);
		
		Table table = PopulateTable (images);
		
		
		window.Title = String.Format ("{0}: {1} ({2} images)", window.Title, dir, images.Count);
		window.SetDefaultSize (300, 200);
		//window.DeleteEvent += Window_Delete;
		scroll.AddWithViewport (table);
		//scroll.SetPolicy (PolicyType.Automatic, PolicyType.Automatic);
		window.Add (scroll);
		window.ShowAll ();
		Application.Run ();
	}

	public static Table PopulateTable (ArrayList items)
	{
		Table table = new Table (0, 0, false);
		uint x = 0;
		uint y = 0;
		
		foreach (object item in items) {
			if (x > 4) {
				x = 0;
				y++;
			}
			table.Attach (((Widget) item), x, x + 1, y, y + 1, AttachOptions.Fill, AttachOptions.Fill, 5, 5);
			x++;
		}

		return table;
	}

	public static ArrayList GetItemsFromDirectory (string directory)
	{
		DirectoryInfo dir_info = new DirectoryInfo (directory);

		if (dir_info.Exists == false)
			throw new DirectoryNotFoundException (directory + " does not exist.");

		FileInfo [] files = dir_info.GetFiles ();

		ArrayList file_items = new ArrayList (files.Length);

		int i = 0;
		foreach (FileInfo fi in files) {
			if (!fi.Name.EndsWith ("png") &&
			    !fi.Name.EndsWith ("jpg") &&
			    !fi.Name.EndsWith ("jpeg") &&
			    !fi.Name.EndsWith ("gif"))
				continue;
			else {
				file_items.Add (new ImageFileBox (fi));
				i++;
			}
		}

		return file_items;
	}

	static void Window_Delete (object o, object args)
	{
		//SignalArgs s = (SignalArgs) args;
		Application.Quit ();
		//s.RetVal = true;
	}
}

class ImageFileFrame : Frame
{
	public ImageFileFrame (FileInfo fileInfo)
		: base (fileInfo.Name)
	{
		string filename = String.Format ("{0}/{1}", fileInfo.DirectoryName, fileInfo.Name);

		Image image = new Image (filename);
		image.Scale (64, 64);
 		
		Frame inner_frame = new Frame (String.Empty);
				
		inner_frame.Add (image);
		//inner_frame.ShadowType = ShadowType.In;
		//inner_frame.BorderWidth = 10;
		this.Add (inner_frame);
		//this.ShadowType = ShadowType.Out;
	}		
}

class ImageFileBox : VBox
{
	public ImageFileBox (FileInfo fileInfo)
		: base (false, 0)
	{
		string filename = String.Format ("{0}/{1}", fileInfo.DirectoryName, fileInfo.Name);

		Image image/*pixbuf*/ = new Image (filename);
		image.Scale (64, 64);

		Label file_label = new Label (fileInfo.Name);

		this.PackStart (image, false, false, 0);
		this.PackStart (file_label, false, false, 0);
	}
}
