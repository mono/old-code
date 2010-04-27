using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Enlightenment;
using Enlightenment.Edje;
using Enlightenment.Evas;
using Enlightenment.Ecore;
using Enlightenment.Ecore.X;

public class EcoreEvasTest
{

	private static Enlightenment.Ecore.Window window;
	private static int drag = 0;
	public static int x;
	public static int y;   
   
	public static void EdjeMouseDownEventCallback(object o, object e)
	{
		EventMouseDown ev = (EventMouseDown) e;
		System.Console.WriteLine("Mouse down in edje!");
		System.Console.WriteLine("Pressed button " + ev.button + " on coord " + ev.canvas.x + " "  + ev.canvas.y);
  }
   
	public static void EdjeMouseInEventCallback(object o, object e)
	{
		EventMouseIn ev = (EventMouseIn) e;
		System.Console.WriteLine("Mouse in on edje!");
	}
   
	public static void EdjeMouseOutEventCallback(object o, object e)
	{
		EventMouseOut ev = (EventMouseOut) e;
		System.Console.WriteLine("Mouse out on edje!");	
	}   
   
	public static void EdjeKeyDownEventCallback(object o, object e)
	{
		EventKeyDown ev = (EventKeyDown) e;
		Edje edje = (Edje)o;		   
		System.Console.WriteLine("pressed " + ev.key);
	}
   
	public static void ImageMouseDownEventCallback(object o, object e)
	{
		EventMouseDown ev = (EventMouseDown) e;
		Image image = (Image)o;
		Geometry g;

		drag = 1;
		g = image.Geometry;
		x = ev.canvas.x - g.X;
		y = ev.canvas.y - g.Y;
	}
   
	public static void ImageMouseUpEventCallback(object o, object e)
	{
		EventMouseUp ev = (EventMouseUp) e;
		Image image = (Image)o;

		drag = 0;
	}
   
	public static void ImageMouseMoveEventCallback(object o, object e)
	{		
		EventMouseMove ev = (EventMouseMove) e;
		Image image = (Image)o;

		if(drag == 1)
		{
			image.Move(ev.cur.canvas.x - x, ev.cur.canvas.y - y);
		}
	}   
   
	public static int AppSignalExitHandler(object info)
	{
		System.Console.WriteLine("Quitting main application!");
		MainLoop.Quit();
		return 1;
	}
   
	public static int AppDndEnterHandler(object info)
	{
		EcoreEventDndEnter e = (EcoreEventDndEnter)info;	

		System.Console.WriteLine("Dnd Enter Handler!");
		System.Console.WriteLine("We have " + e.num_types + " types : ");
	
		for (int i=0; i < e.num_types; i++)
		{
			System.Console.Write(e.types[i] + " ");
		}

		System.Console.WriteLine("");
		return 1;
	}   
   
	public static int AppDndLeaveHandler(object info)
	{
		System.Console.WriteLine("Dnd Leave Handler!");
		return 1;
	}
   
	public static int AppDndPositionHandler(object info)
	{
		System.Console.WriteLine("Dnd Position Handler!");
		XWindow win = (XWindow)window.DataGet("xwindow");
		win.Dnd.SendStatus(1, 1, new Enlightenment.Ecore.X.Rectangle(500, 20, (uint)640, (uint)480), XDnd.ActionPrivate);
		return 1;
  }  
   
	public static int AppDndDropHandler(object info)
	{
		System.Console.WriteLine("Dnd Drop Handler!");
		XWindow win = (XWindow)window.DataGet("xwindow");
		win.Selection.DndRequest("text/uri-list");
		return 1;
	}      
   
	public static int AppSelectionNotifyHandler(object info)
	{
		EcoreEventSelectionNotify e = (EcoreEventSelectionNotify)info;
		EcoreEventSelectionDataFiles data = new EcoreEventSelectionDataFiles(e.data);
		XWindow win = (XWindow)window.DataGet("xwindow");
		
		System.Console.WriteLine("Selection Notify Handler!");
		System.Console.WriteLine("Selection length: " + data.num_files);

		for (int i=0; i < data.num_files; i++)
		{
			System.Console.Write(data.files[i] + " ");
		}
		
		System.Console.WriteLine("");
		
		if (data.num_files == 1)
    {
			Regex imgExtention = new Regex("(jpg|jpeg|png)$");
			
			if (imgExtention.IsMatch(data.files[0].ToLower()))
			{
				Image image = (Image)window.DataGet("image");
				image.Set(data.files[0].Substring(5, data.files[0].Length - 5), null);
				image.Resize(320, 320);
				image.Fill = new ImageFill (0, 0, 320, 320);
				image.Move(0, 0);
			}
		}

		System.Console.WriteLine("Target: " + e.target);
		Enlightenment.Ecore.X.XDnd.SendFinished();
		
		return 1;
	}
   
	public static void _xwindow_resize_cb(Enlightenment.Ecore.Window ee)
	{
		Edje background;
	
		System.Console.WriteLine("Resizing Application XWindow!");	
	
		background = (Edje)window.DataGet("background");
		background.Resize(window.Geometry.W, window.Geometry.H);
  }      
   
	public static void Main(string [] args)
  {	
		MainLoop.Init();
		Enlightenment.Ecore.Window.Init();
		Edje.Init();
	
		window = Enlightenment.Ecore.Window.SoftwareX11New(":0", null, 0, 0, 640, 480);
		window.ResizeEvent += new Enlightenment.Ecore.Window.EventHandler(_xwindow_resize_cb);
		window.Show();		
	
		XWindow xwindow = window.SoftwareX11WindowGet();
		xwindow.Dnd.Aware = 1;
		xwindow.Dnd.TypeSet("*", 1);
	
		if (xwindow.Dnd.TypeIsSet("*") == 1)
		{
    	Enlightenment.Ecore.Events.SignalExitEvent += new Enlightenment.Ecore.Events.EventHandler(AppSignalExitHandler);
			Enlightenment.Ecore.Events.DndEnterEvent += new Enlightenment.Ecore.Events.EventHandler(AppDndEnterHandler);
			Enlightenment.Ecore.Events.DndLeaveEvent += new Enlightenment.Ecore.Events.EventHandler(AppDndLeaveHandler);
			Enlightenment.Ecore.Events.DndPositionEvent += new Enlightenment.Ecore.Events.EventHandler(AppDndPositionHandler);
			Enlightenment.Ecore.Events.DndDropEvent += new Enlightenment.Ecore.Events.EventHandler(AppDndDropHandler);
			Enlightenment.Ecore.Events.SelectionNotifyEvent += new Enlightenment.Ecore.Events.EventHandler(AppSelectionNotifyHandler);
		}

		Edje background = new Edje(window.Get());
		if (background.FileSet(DataConfig.DATADIR + "/data/test/flower.edj","desktop/background") == 0)
		{
     	System.Console.WriteLine("Can't load edje object!");
		}
		
		background.Focus = 1;
		background.MouseDownEvent  += new Enlightenment.Evas.Item.EventHandler(EdjeMouseDownEventCallback);
		background.MouseInEvent    += new Enlightenment.Evas.Item.EventHandler(EdjeMouseInEventCallback);
		background.MouseOutEvent   += new Enlightenment.Evas.Item.EventHandler(EdjeMouseOutEventCallback);
		background.KeyDownEvent    += new Enlightenment.Evas.Item.EventHandler(EdjeKeyDownEventCallback);
		background.Move(0, 0);
		background.Resize(640, 480);
		background.Show();

		Image image = new Image(window.Get());
		image.Set(DataConfig.DATADIR + "/data/test/images/e.png", null);
		image.Move(0, 0);	
		image.Resize(320, 320);
		image.Fill = new ImageFill (0, 0, 320, 320);
		image.MouseDownEvent += new Enlightenment.Evas.Item.EventHandler(ImageMouseDownEventCallback);
		image.MouseUpEvent += new Enlightenment.Evas.Item.EventHandler(ImageMouseUpEventCallback);
		image.MouseMoveEvent += new Enlightenment.Evas.Item.EventHandler(ImageMouseMoveEventCallback);	
		image.Raise();
		image.Show();
		
		/* Put back when textblock is done */
		/*
		TextBlock textblock = new TextBlock(window.Get());
		textblock.FormatInsert("color=#ffffffff font="+DataConfig.DATADIR+"/data/fonts/Vera.ttf size=16");
		textblock.TextInsert("This is some text for the text block!");
		textblock.FormatInsert("\n");
		textblock.TextInsert("And this text should be on another line!");
		textblock.Move(300, 300);
		textblock.Resize(300, 100);
		textblock.Raise();
		textblock.Show();
		*/

		window.DataSet("image", image);	
		window.DataSet("background", background);
		window.DataSet("xwindow", xwindow);
		
		MainLoop.Begin();
  }
}
