using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Collections;
using Enlightenment.Evas;
using Enlightenment.Ecore;
using Enlightenment.Edje;
using Enlightenment.Epsilon;
using Enlightenment.Eblocks;

public class TableImage : Enlightenment.Eblocks.TableItem
{
  public new Enlightenment.Evas.Image i;
  private Enlightenment.Evas.Image shadow;
  private string file;   
   
  public override Enlightenment.Evas.Geometry Geometry
  {
    get { return i.Geometry; }
  }
   
  public TableImage(string f)
  {
    file = f;
  }
   
  public override void Add(Enlightenment.Evas.Canvas canvas)
  {
    i = new Enlightenment.Evas.Image(canvas);	
    i.Set(file, null);
	
    shadow = new Enlightenment.Evas.Image(canvas);
    shadow.Set(file, null);
    shadow.Set(DataConfig.DATADIR + "/data/test/images/shadow.png", null);
    shadow.Border = new ImageBorder(5, 5, 5, 5);
  }
   
  public override void Delete()
  {
    i.Delete();
    shadow.Delete();
  }
   
  public override void Show()
  {
    i.Show();
    shadow.Show();
  }
   
  public override void Hide()
  {
    i.Hide();
    shadow.Hide();
  }
   
  public override void Resize(int w, int h)
  {
    i.Resize(w, h);
    i.Fill = new ImageFill (0, 0, w, h);
	
    shadow.Resize(w + 8, h + 8);
    shadow.Fill = new ImageFill (0, 0, w + 8, h + 8);
  }
   
  public override void Move(int x, int y)
  {
    i.Move(x, y);
    shadow.Move(x, y);
  }
   
  public override void StackBelow(Enlightenment.Evas.Item below)
  {
    i.StackBelow(below);
    shadow.StackBelow(i);
  }
   
  public override void Raise()
  {
    i.Raise();
    shadow.StackBelow(i);
  }
   
  public override Enlightenment.Evas.Item Clip
  {
    get { return i.Clip; }
    set { i.Clip = value; shadow.Clip = value; }
  }
   
  ~TableImage()
  {}
}


public class EcoreEvasTest
{
   
  private static Table imageTable;
  private static ArrayList items = new ArrayList();
  private static string dir;
   
  public static void AppQuitButtonYesHandler(Dialog d)
  {
    System.Console.WriteLine("Pressed Yes!!");
    MainLoop.Quit();
  }
   
  public static void AppQuitButtonNoHandler(Dialog d)
  {
    System.Console.WriteLine("Pressed No!!");
    d.Close();	
    d = null;
  }
   
      
  public static void AppQuitButtonHandler(Enlightenment.Evas.Item item, object EventInfo)
  {
    Dialog d = new Dialog("Quit Application", "Are you sure you want to quit?", "Quitting will lose all unsaved data!");
    d.AddButton("Yes", AppQuitButtonYesHandler);
    d.AddButton("No", AppQuitButtonNoHandler);
    d.Run();
  }

  public static void AppAboutButtonCloseHandler(Dialog d)
  {
    d.Close();
    d = null;
  }
   
  public static void AppAboutButtonHandler(Enlightenment.Evas.Item item, object EventInfo)
  {
    Dialog d = new Dialog("About EFL#", "EFL# Widget Test", "Coded by Hisham Mardam Bey");
    d.AddButton("Close", AppAboutButtonCloseHandler);
    d.Run();	
  }
   
  private static void Callback(object state)
  {	
    DirectoryInfo myDirectory = new DirectoryInfo(dir);
    FileInfo[] _files = myDirectory.GetFiles();
    foreach (FileInfo file in _files)
    {
      String s;
      Thumb thumb = new Thumb(file.FullName);
      if(thumb.Exists() == 0)
	thumb.Generate();
      s = thumb.Preview;
      TableImage i = new TableImage(s);
      items.Add(i);
    }

	
    imageTable.ItemSize(96, 96);
    imageTable.HorizSpacer = 10;
    imageTable.VertSpacer = 10;
    imageTable.Resize(640 - 70 - 2, 480 - 37 - 2);
    Box box_images = (Box)Application.EE.DataGet("box_images");
    box_images.Show();	
    System.GC.KeepAlive(imageTable);
  }   
   
  public static void AppResizeHandler(Enlightenment.Ecore.Canvas ee)
  {
    int w, h;
    Box box;
    w = Application.EE.Geometry.W;
    h = Application.EE.Geometry.H;	
	
    box = (Box)Application.EE.DataGet("box_icons");
    box.Resize(box.Geometry.W, h - 37);
	
    box = (Box)Application.EE.DataGet("box_left");
    box.Resize(box.Geometry.W, h - 37);	
	
    box = (Box)Application.EE.DataGet("box_images");
    box.Resize(w - 70 - 2, h - 37 - 2);	
	
    Edje win_bg = (Edje)Application.EE.DataGet("win_bg");
    win_bg.Resize(w, h);
			
    imageTable.Resize(w- 60 - 2, h - 37 - 2);

    MenuBar mb = (MenuBar)Application.EE.DataGet("mb");
    mb.Resize(w, mb.Geometry.H);
  }
        
  public static void Main(string [] args)
  {	
    Application.Init();

    Window win = new Window("EFL# Demo App");	
    win.Resize(640, 480);

    Application.EE.ResizeEvent += AppResizeHandler;

    /* integrate this code in the Window class */
    Edje win_bg = new Edje(Application.EE.Get());
    win_bg.FileSet(DataConfig.DATADIR + "/data/eblocks/themes/e17.edj","window");
    win_bg.Resize(640, 480);
    win_bg.Move(0, 0);
    win_bg.Lower();
    win_bg.Show();
    
    MenuItem item;    
    MenuItem entry;

    MenuBar mb = new MenuBar();
    mb.Move(0, 0);
    mb.Resize(640, 35);
    mb.Spacing = 15;

    item = new MenuItem("_File");
    Menu file_menu = new Menu();
    
    entry = new MenuItem(file_menu.Canvas, "_Open");
    file_menu.Append(entry);
    entry = new MenuItem(file_menu.Canvas, "_Close");
    file_menu.Append(entry);
    entry = new MenuItem(file_menu.Canvas, "_Save");
    file_menu.Append(entry);
    
    item.SubMenu = file_menu;
    
    mb.Append(item);

    item = new MenuItem("_Edit");
    Menu edit_menu = new Menu();
    
    entry = new MenuItem(edit_menu.Canvas, "_Copy");
    edit_menu.Append(entry);
    entry = new MenuItem(edit_menu.Canvas, "_Cut");
    edit_menu.Append(entry);
    entry = new MenuItem(edit_menu.Canvas, "_Paste");
    edit_menu.Append(entry);
    
    item.SubMenu = edit_menu;
    mb.Append(item);

    item = new MenuItem("_About");
    //item.SubMenu = about_menu;
    mb.Append(item);

    mb.Show();

    Button button;	

    HBox box_left = new HBox();
    box_left.Move(0, 37);
    box_left.Resize(70, 480 - 37);
	
    VBox box_icons = new VBox();
    box_icons.Spacing = 0;
    box_icons.Resize(64, 480 - 37);
	
    button = new Button("Tile");
    button.Resize(64, 64);
    box_icons.PackEnd(button);
    button = new Button("Stretch");
    button.Resize(64, 64);
    box_icons.PackEnd(button);	
    button = new Button("Rotate");
    button.Resize(64, 64);
    box_icons.PackEnd(button);	
    button = new Button("Flip");
    button.Resize(64, 64);
    box_icons.PackEnd(button);	
    button = new Button("Quit");
    button.MouseUpEvent += new Enlightenment.Evas.Item.EventHandler(AppQuitButtonHandler);
    button.Resize(64, 64);	
    box_icons.PackEnd(button);		
	
    box_left.PackEnd(box_icons);
	
    Enlightenment.Eblocks.Line vline = new Enlightenment.Eblocks.Line(Application.EE.Get());
    vline.Vertical = true;
    vline.Resize(6, 480 - 37);
	
    box_left.PackEnd(vline);
	
    box_left.Show();
	
    dir = args[0];	
	       	
    imageTable = new Table(Application.EE.Get(), items);
	
    HBox box_images = new HBox();
    box_images.Move(70, 37);
    box_images.Spacing = 0;
    box_images.Resize(640 - 70 - 2, 480 - 37 - 2);
    box_images.PackStart(imageTable);
	
    Application.EE.DataSet("box_images", box_images);
    Application.EE.DataSet("box_left", box_left);
    Application.EE.DataSet("box_icons", box_icons);
    Application.EE.DataSet("win_bg", win_bg);
    Application.EE.DataSet("vline", vline);
    Application.EE.DataSet("mb", mb);
	
    WaitCallback callback = new WaitCallback(Callback);
    ThreadPool.QueueUserWorkItem(callback);	

    win.ShowAll();
	
    Application.Run();
  }   
}
