namespace Enlightenment.Ecore.X {

   using System;
   using System.Collections;
   using System.Runtime.InteropServices;

   public static class Main
   {
	   const string Library = "ecore_x";
	   
	   [DllImport(Library)]
	   private extern static int ecore_x_init (string name);
	   
	   public static int Init (string name)
	   {
		   return ecore_x_init (name);
	   }
	   
	   [DllImport(Library)]
	   private extern static int ecore_x_shutdown ();
	   
	   public static int Shutdown ()
	   {
		   return ecore_x_shutdown ();
	   }
	   
	   [DllImport(Library)]
	   private extern static int ecore_x_disconnect ();
	   
	   public static int Disconnect ()
	   {
		   return ecore_x_disconnect ();
	   }
	   
	   [DllImport(Library)]
	   private extern static uint ecore_x_current_time_get ();
	   
	   public static uint CurrentTime {
		   get { return ecore_x_current_time_get (); }
	   }
   }
   
   public class XWindowSize
   {
	   public int X;
	   public int Y;

	   public XWindowSize(int x, int y)
	   {
		   X = x;
		   Y = y;
	   }
   }

   public class XWindowGeometry
   {
	   public int X;
	   public int Y;
	   public int W;
	   public int H;

	   public XWindowGeometry(int x, int y, int w, int h)
	   {
		   X = x;
		   Y = y;
		   W = w;
		   H = h;
	   }
   }

   public class XWindowProperty
   {
	   public uint type;
	   public uint format;
	   public int size;
	   public string data;
	   public int number;

	   public XWindowProperty (uint t, uint f, int s, string d, int n)
	   {
		   type = t;
		   format = f;
		   size = s;
		   data = d;
		   number = n;
	   }
   }

   [StructLayout(LayoutKind.Sequential)]
   public struct Rectangle
   {
	   public int X;
	   public int Y;
	   public uint Width;
	   public uint Height;

	   public Rectangle(int x, int y, uint w, uint h)
	   {
		   X = x;
		   Y = y;
		   Width = w;
		   Height = h;
	   }
   }

   public class XWindow
   {
	   const string Library = "ecore_x";

	   HandleRef objRaw;
	   public XDnd Dnd;
	   public XSelection Selection;

	   public enum ConfigureMask
	   {
		   ConfigureMaskX = (1 << 0),
		   ConfigureMaskY = (1 << 1),
		   ConfigureMaskW = (1 << 2),
		   ConfigureMaskH = (1 << 3),
		   ConfigureMaskBorderWidth = (1 << 4),
		   ConfigureMaskSibling = (1 << 5),
		   ConfigureMaskStackMode =(1 << 6)
	   }

	   public enum GravityType
	   {
		   Forget = 0,
		     Unmap = 0,
		     Nw = 1,
		     N = 2,
		     Ne = 3,
		     W = 4,
		     Center = 5,
		     E = 6,
		     Sw = 7,
		     S = 8,
		     Se = 9,
		     Static = 10
	   }

	   /* Multiple constructors for creating Window objects */
	   
	   [DllImport(Library)]
	   private extern static IntPtr ecore_x_window_override_new(IntPtr parent, int x, int y, int w, int h);

	   [DllImport(Library)]
	   private extern static IntPtr ecore_x_window_input_new(IntPtr parent, int x, int y, int w, int h);

	   public static XWindow XWindowInput (XWindow parent, int x, int y, int w, int h)
	   {
		   IntPtr win = ecore_x_window_input_new (parent.Raw, x, y, w, h);
		   return new XWindow (win);
	   }

	   [DllImport(Library)]
	   private extern static IntPtr ecore_x_window_new(IntPtr parent, int x, int y, int w, int h);

	   public delegate IntPtr EcoreXWindowNew(IntPtr parent, int x, int y, int w, int h);

	   /* FIXME - we need to see what happens if the user passed 'null'
	    * instead of a valid window
	    */
	   public XWindow (XWindow parent, int x, int y, int w, int h)
	   {
		   objRaw = new HandleRef(this, ecore_x_window_new(parent.Raw, x, y, w, h));
		   Dnd = new XDnd(this);
		   Selection = new XSelection(this);
	   }

	   public XWindow (int x, int y, int w, int h)
	   {
		   objRaw = new HandleRef(this, ecore_x_window_new(IntPtr.Zero, x, y,  w, h));
		   Dnd = new XDnd(this);
		   Selection = new XSelection(this);
	   }

	   public XWindow (XWindow parent, int x, int y, int w, int h, bool option)
	   {
		   EcoreXWindowNew func;

		   if(option)
		     func = new EcoreXWindowNew(ecore_x_window_override_new);
		   else
		     func = new EcoreXWindowNew(ecore_x_window_override_new);

		   objRaw = new HandleRef(this, func(parent.Raw, x, y, w, h));
		   Dnd = new XDnd(this);
		   Selection = new XSelection(this);
	   }

	   public XWindow (IntPtr win)
	   {
		   objRaw = new HandleRef(this, win);
		   Dnd = new XDnd(this);
		   Selection = new XSelection(this);
	   }

	   public XWindow ()
	   {
		   Dnd = new XDnd();
		   Selection = new XSelection();
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_window_configure(IntPtr win, ConfigureMask mask,
							       int x, int y, int w, int h,
							       int border_width,
							       IntPtr sibling,
							       int stack_mode);

	   public void Configure(ConfigureMask mask, int x, int y, int w, int h,
				 int border_width, XWindow sibling, int stack_mode)
	   {
		   ecore_x_window_configure(Raw, mask, x, y, w, h, border_width,
					    sibling.Raw, stack_mode);
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_window_cursor_set(IntPtr win, uint c);

	   public uint Cursor
	   {
		   set { ecore_x_window_cursor_set(Raw, value); }
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_window_delete_request_send(IntPtr win);

	   public void DeleteRequestSend()
	   {
		   ecore_x_window_delete_request_send(Raw);
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_window_show(IntPtr win);

	   public void Show()
	   {
		   ecore_x_window_show(Raw);
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_window_hide(IntPtr win);

	   public void Hide()
	   {
		   ecore_x_window_hide(Raw);
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_window_move(IntPtr win, int x, int y);

	   public void Move(int x, int y)
	   {
		   ecore_x_window_move(Raw, x, y);
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_window_resize(IntPtr win, int w, int h);

	   public void Resize(int w, int h)
	   {
		   ecore_x_window_resize(Raw, w, h);
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_window_move_resize(IntPtr win, int x,
								 int y, int w, int h);

	   public void MoveResize(int x, int y, int w, int h)
	   {
		   ecore_x_window_move_resize(Raw, x, y, w, h);
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_window_focus(IntPtr win);

	   public void Focus()
	   {
		   ecore_x_window_focus(Raw);
	   }

	   [DllImport(Library)]
	   private extern static IntPtr ecore_x_window_focus_get();

	   public static XWindow FocusGet()
	   {
		   return new XWindow(ecore_x_window_focus_get());
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_window_focus_at_time(IntPtr win, uint time);

	   public void FocusAtTime(uint t)
	   {
		   ecore_x_window_focus_at_time(Raw, t);
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_window_raise(IntPtr win);

	   public void Raise()
	   {
		   ecore_x_window_raise(Raw);
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_window_lower(IntPtr win);

	   public void Lower()
	   {
		   ecore_x_window_lower(Raw);
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_window_reparent(IntPtr win,
							      IntPtr new_parent,
							      int x, int y);

	   public void Reparent(XWindow new_parent, int x, int y)
	   {
		   ecore_x_window_reparent(Raw, new_parent.Raw, x, y);
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_window_size_get(IntPtr win,
							      out int x,
							      out int y);

	   public XWindowSize Size
	   {
		   get {
			   int x, y;
			   ecore_x_window_size_get(Raw, out x, out y);
			   return new XWindowSize(x, y);
		   }
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_window_geometry_get(IntPtr win,
								  out int x,
								  out int y,
								  out int w,
								  out int h);

	   public XWindowGeometry Geometry
	   {
		   get {
			   int x, y, w, h;
			   ecore_x_window_geometry_get(Raw, out x, out y, out w, out h);
			   return new XWindowGeometry(x, y, w, h);
		   }
	   }

	   [DllImport(Library)]
	   private extern static int ecore_x_window_border_width_get(IntPtr win);

	   [DllImport(Library)]
	   private extern static void ecore_x_window_border_width_set(IntPtr win,
								      int width);

	   public int Width
	   {
		   get { return ecore_x_window_border_width_get(Raw); }
		   set { ecore_x_window_border_width_set(Raw, value); }
	   }

	   [DllImport(Library)]
	   private extern static int ecore_x_window_depth_get(IntPtr win);

	   public int Depth
	   {
		   get { return ecore_x_window_depth_get(Raw); }
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_window_cursor_show(IntPtr win,
								 int show);

	   public int CursorShow
	   {
		   set { ecore_x_window_cursor_show(Raw, value); }
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_window_defaults_set(IntPtr win);

	   public void Defaults()
	   {
		   ecore_x_window_defaults_set(Raw);
	   }

	   [DllImport(Library)]
	   private extern static int ecore_x_window_visible_get(IntPtr win);

	   public int Visible
	   {
		   get { return ecore_x_window_visible_get(Raw); }
	   }

	   [DllImport(Library)]
	   private extern static IntPtr ecore_x_window_at_xy_get(int x, int y);

	   public static XWindow AtXY(int x, int y)
	   {
		   return new XWindow(ecore_x_window_at_xy_get(x, y));
	   }

	   [DllImport(Library)]
	   private extern static IntPtr ecore_x_window_parent_get(IntPtr win);

	   public XWindow Parent
	   {
		   get { return new XWindow(ecore_x_window_parent_get(Raw)); }
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_window_background_color_set(IntPtr win,
									  ushort r,
									  ushort g,
									  ushort b);
	   public void BackgroundColor(ushort r, ushort g, ushort b)
	   {
		   ecore_x_window_background_color_set(Raw, r, g, b);
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_window_gravity_set(IntPtr win,
								 GravityType g);

	   public GravityType Gravity
	   {
		   set { ecore_x_window_gravity_set(Raw, value); }
	   }

	   [DllImport(Library)]
	   private extern static uint ecore_x_window_prop_any_type();

	   public static uint AnyType()
	   {
		   return ecore_x_window_prop_any_type();
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_window_prop_property_set(IntPtr win,
								       uint type,
								       uint format,
								       int size,
								       string data,
								       int number);

	   [DllImport(Library)]
	   private extern static void ecore_x_window_prop_property_get(IntPtr win,
								       out uint type,
								       out uint format,
								       out int size,
								       out string data,
								       out int number);
	   public XWindowProperty Property
	   {
		   get {
			   uint type;
			   uint format;
			   int size;
			   string data;
			   int number;
			   ecore_x_window_prop_property_get(Raw, out type, out format,
							    out size, out data, out number
							    );
			   return new XWindowProperty(type, format, size, data, number);
		   }
		   set {
			   ecore_x_window_prop_property_set(Raw, value.type, value.format,
							    value.size, value.data,
							    value.number);
		   }
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_window_prop_property_del(IntPtr win,
								       uint prop);

	   public void PropertyDel(uint prop)
	   {
		   ecore_x_window_prop_property_del(Raw, prop);
	   }

	   [DllImport(Library)]
	   private extern static int ecore_x_pointer_grab(IntPtr win);

	   public int PointerGrab()
	   {
		   return ecore_x_pointer_grab(Raw);
	   }

	   [DllImport(Library)]
	   private extern static int ecore_x_pointer_confine_grab(IntPtr win);

	   public int PointerConfineGrab()
	   {
		   return ecore_x_pointer_confine_grab(Raw);
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_pointer_ungrab();

	   public void PointerUnGrab()
	   {
		   ecore_x_pointer_ungrab();
	   }

	   public virtual IntPtr Raw
	   {
		   get { return objRaw.Handle; }
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_window_del(IntPtr win);

	   public void Delete()
	   {
		   ecore_x_window_del(Raw);
	   }

	   ~XWindow()
	   {
		   //ecore_x_window_del(Raw);
	   }

   }

   public class XSelection
   {
	   const string Library = "ecore_x";

	   XWindow win;

	   public XSelection(XWindow w)
	   {
		   win = w;
	   }

	   public XSelection()
	   {}

	   [DllImport(Library)]
	   private extern static int ecore_x_selection_xdnd_set(IntPtr win,
								string data,
								int size);

	   public int DndSet(string data, int size)
	   {
		   return ecore_x_selection_xdnd_set(win.Raw, data, size);
	   }

	   [DllImport(Library)]
	   private extern static int ecore_x_selection_xdnd_clear();

	   public static int DndClear()
	   {
		   return ecore_x_selection_xdnd_clear();
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_selection_xdnd_request(IntPtr win,
								     string target
								     );

	   public void DndRequest(string target)
	   {
		   ecore_x_selection_xdnd_request(win.Raw, target);
	   }
   }

   public class XDnd
   {
	   const string Library = "ecore_x";
	   const string LibraryGlue = "libeflsharpglue";

	   XWindow win;

	   public XDnd(XWindow w)
	   {
		   win = w;
	   }

	   public XDnd()
	   {}

	   [DllImport(Library)]
	   private extern static void ecore_x_dnd_aware_set(IntPtr win, int on);

	   public int Aware
	   {
		   set { ecore_x_dnd_aware_set(win.Raw, value); }
	   }

	   [DllImport(Library)]
	   private extern static int ecore_x_dnd_version_get(IntPtr win);

	   public int Version
	   {
		   get { return ecore_x_dnd_version_get(win.Raw); }
	   }

	   [DllImport(Library)]
	   private extern static int ecore_x_dnd_type_isset(IntPtr win,
							    string type);

	   public int TypeIsSet(string type)
	   {
		   return ecore_x_dnd_type_isset(win.Raw, type);
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_dnd_type_set(IntPtr win,
							   string type, int on);

	   public void TypeSet(string type, int on)
	   {
		   ecore_x_dnd_type_set(win.Raw, type, on);
	   }

	   [DllImport(Library)]
	   private extern static int ecore_x_dnd_begin(IntPtr win, string data,
						       int size);

	   public int Begin(string data, int size)
	   {
		   return ecore_x_dnd_begin(win.Raw, data, size);
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_dnd_drop();

	   public static void Drop()
	   {
		   ecore_x_dnd_drop();
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_dnd_send_status(int will_accept,
							      int supress,
							      Rectangle rectangle,
							      uint action);

	   public void SendStatus(int will_accept, int supress, Rectangle r,
				  uint action)
	   {
		   ecore_x_dnd_send_status(will_accept, supress, r, action);
	   }

	   [DllImport(Library)]
	   private extern static void ecore_x_dnd_send_finished();

	   public static void SendFinished()
	   {
		   ecore_x_dnd_send_finished();
	   }

    /* Actions */
	   [DllImport(LibraryGlue)]
	   private extern static uint _ecore_x_dnd_action_private();
	   public static uint ActionPrivate
	   {
		   get { return _ecore_x_dnd_action_private(); }
	   }
   }
}
