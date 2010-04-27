using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Collections;
using Enlightenment.Evas;
using Enlightenment.Ecore;
using Enlightenment.Eblocks;

public class TableImage : Enlightenment.Eblocks.TableItem
{
  public new Image i;
  private FileInfo file;

  public override Enlightenment.Evas.Geometry Geometry
  {
    get { return i.Geometry; }
  }
   
  public TableImage(FileInfo f)
  {
    file = f;
  }
      
  public override void Add(Enlightenment.Evas.Canvas canvas)
  {
    i = new Image(canvas);
    i.Set(file.FullName, null);
  }
         
  public override void Delete()
  {
    i.Delete();
  }
   
  public override void Show()
  {
    i.Show();
  }
   
  public override void Hide()
  {
    i.Hide();
  }
   
  public override void Resize(int w, int h)
  {
    i.Resize(w, h);
    i.Fill = new ImageFill (0, 0, w, h);
  }
   
  public override void Move(int x, int y)
  {
    i.Move(x, y);
  }   
   
  public override void StackBelow(Enlightenment.Evas.Item below)
  {
    i.StackBelow(below);
  }

  public override void Raise()
  {
    i.Raise();
  }
   
  public override Enlightenment.Evas.Item Clip
  {
    get { return i.Clip; }
    set { i.Clip = value; }
  }
   
  ~TableImage()
  {}
}

public class TableTest
{

  private static Enlightenment.Ecore.Canvas EE = new Enlightenment.Ecore.Canvas();
  private static Table cont;
  private static Thread thread;
  private static ArrayList items = new ArrayList();

  private static int app_w = 800;
  private static int app_h = 600;
   
  public static void DrawGui(object state)
  {
    thread = Thread.CurrentThread;
    cont = new Table(EE.Get(), items);
    cont.ItemSize(48, 48);
    cont.Move(100, 50);
    cont.Resize(app_w - 100, app_h - 50);
    cont.Show();   
	
    thread.Abort();
  }
   
  public static int AppSignalExitHandler(object info)
  {
    System.Console.WriteLine("Quitting main application!");
    Enlightenment.Ecore.MainLoop.Quit();
    return 1;
  }
   
  public static void AppResizeHandler(Enlightenment.Ecore.Canvas ee)
  {
    int x, y, w, h;
    Rectangle rect = (Rectangle)EE.DataGet("bg_rect");
    EE.GeometryGet(out x, out y, out w, out h);
    rect.Resize(w, h);
    rect = (Rectangle)EE.DataGet("left_toolbar");
    rect.Resize(100, h);
    rect = (Rectangle)EE.DataGet("top_toolbar");
    rect.Resize(w, 50);
    cont.Resize(w - 100, h - 50);	
  }
   
  public static void Main(string [] args)
  {	
    Enlightenment.Ecore.MainLoop.Init();
    Enlightenment.Ecore.Canvas.Init();
			
    EE.SoftwareX11New(":0", IntPtr.Zero, 0, 0, app_w, app_h);
    EE.Title = "Sharp Table";
    EE.NameClassSet("sharp_container","sharp_container");
    EE.ResizeEvent += new Enlightenment.Ecore.Canvas.EventHandler(AppResizeHandler);
    EE.Show();
	
    Enlightenment.Ecore.Events.SignalExitEvent += new Enlightenment.Ecore.Events.EventHandler(AppSignalExitHandler);
	
    Rectangle bg_rect = new Rectangle(EE.Get());
    bg_rect.Move(0, 0);
    bg_rect.Color = new Color(228, 226, 212, 255);
    bg_rect.Resize(app_w, app_h);
    bg_rect.Lower();
    bg_rect.Show();
	
    Rectangle left_toolbar = new Rectangle(EE.Get());
    left_toolbar.Move(0, 0);
    left_toolbar.Resize(100, app_h);
    left_toolbar.Color = new Color(124, 126, 101, 200);
    left_toolbar.Show();
	
    Rectangle top_toolbar = new Rectangle(EE.Get());
    top_toolbar.Move(0, 0);
    top_toolbar.Resize(app_w, 50);
    top_toolbar.Color = new Color(124, 126, 101, 200);
    top_toolbar.Show();	
	
	
    DirectoryInfo myDirectory = new DirectoryInfo(args[0]);
    FileInfo[] _files = myDirectory.GetFiles();
    foreach (FileInfo file in _files)
    {
      TableImage i = new TableImage(file);
      items.Add(i);
    }

		              
    EE.DataSet("bg_rect", bg_rect);
    EE.DataSet("left_toolbar", left_toolbar);
    EE.DataSet("top_toolbar", top_toolbar);
	
    WaitCallback callback = new WaitCallback(DrawGui);
    ThreadPool.QueueUserWorkItem(callback);
	

    Enlightenment.Ecore.MainLoop.Begin();
	
  }   
}
