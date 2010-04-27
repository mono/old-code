using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Threading;

namespace Enlightenment.Evas
{
   public enum ButtonFlags
   {
	   None = 0,
	   DoubleClick = (1 << 0),
	   TripleClick = (1 << 1)
   }
   
   public struct Color
   {
	   public int R;
	   public int G;
	   public int B;
	   public int A;

	   public Color(int r, int g, int b, int a)
	   {
		   R = r;
		   G = g;
		   B = b;
		   A = a;
	   }
   }

   public struct Geometry
   {
	   public int X;
	   public int Y;
	   public int W;
	   public int H;

	   public Geometry(int x, int y, int w, int h)
	   {
		   X = x;
		   Y = y;
		   W = w;
		   H = h;
	   }
   }

   public class Canvas
   {
	   const string Library = "evas";

	   protected HandleRef objRaw;
	   public Hashtable callbacks = new Hashtable ();
	   public Hashtable dataptrs = new Hashtable ();

	   [DllImport(Library)]
	   private extern static int evas_alloc_error();

	   [DllImport(Library)]
	   private extern static int evas_init();

	   [DllImport(Library)]
	   private extern static int evas_shutdown();

	   [DllImport(Library)]
	   private extern static IntPtr evas_new();

	   [DllImport(Library)]
	   private extern static IntPtr evas_free(IntPtr e);

	   [DllImport(Library)]
	   private extern static int evas_render_method_lookup(string name);

	   // FIXME: make this work
	   [DllImport(Library)]
	   private extern static IntPtr evas_render_method_list();

	   [DllImport(Library)]
	   private extern static void evas_render_method_list_free(IntPtr list);

	   [DllImport(Library)]
	   private extern static void evas_output_method_set(IntPtr e, int render_method);

	   [DllImport(Library)]
	   private extern static int evas_output_method_get(IntPtr e);

	   [DllImport(Library)]
	   private extern static IntPtr evas_engine_info_get(IntPtr e);

	   // FIXME: make this work
	   [DllImport(Library)]
	   private extern static void evas_engine_info_set(IntPtr e, IntPtr info);

	   [DllImport(Library)]
	   private extern static void evas_output_size_set(IntPtr e, int w, int h);

	   [DllImport(Library)]
	   private extern static void evas_output_size_get(IntPtr e, out int w, out int h);

	   [DllImport(Library)]
	   private extern static void evas_output_viewport_set(IntPtr e, int x, int y, int w, int h);

	   [DllImport(Library)]
	   private extern static void evas_output_viewport_get(IntPtr e, out int x, out int y, out int w, out int h);

	   [DllImport(Library)]
	   private extern static int evas_coord_screen_x_to_world(IntPtr e, int x);

	   [DllImport(Library)]
	   private extern static int evas_coord_screen_y_to_world(IntPtr e, int y);

	   [DllImport(Library)]
	   private extern static int evas_coord_world_x_to_screen(IntPtr e, int x);

	   [DllImport(Library)]
	   private extern static int evas_coord_world_y_to_screen(IntPtr e, int y);

	   [DllImport(Library)]
	   private extern static void evas_pointer_output_xy_get(IntPtr e, out int x, out int y);

	   [DllImport(Library)]
	   private extern static void evas_pointer_canvas_xy_get(IntPtr e, out int x, out int y);

	   [DllImport(Library)]
	   private extern static int evas_pointer_button_down_mask_get(IntPtr e);

	   [DllImport(Library)]
	   private extern static int evas_pointer_inside_get(IntPtr e);

	   public Canvas()
	   {
		   objRaw = new HandleRef(this, evas_new());
	   }

	   public Canvas(IntPtr e)
	   {
		   objRaw = new HandleRef(this, e);
	   }

	   public static int AllowError()
	   {
		   return evas_alloc_error();
	   }

	   public static int Init()
	   {
		   return evas_init();
	   }

	   public static int Shutdown()
	   {
		   return evas_shutdown();
	   }

	   public static int RenderMethodLookup(string name)
	   {
		   return evas_render_method_lookup(name);
	   }

	   // FIXME: make this work
	   public static IntPtr RenderMethodList()
	   {
		   return evas_render_method_list();
	   }

	   public static void RenderMethodListFree(IntPtr list)
	   {
		   evas_render_method_list_free(list);
	   }

	   public void OutputMethodSet(int render_method)
	   {
		   evas_output_method_set(Raw, render_method);
	   }

	   public int OutputMethodGet()
	   {
		   return evas_output_method_get(Raw);
	   }

	   // FIXME: what do we want to do about the struct?
	   public IntPtr EngineInfoGet()
	   {
		   return evas_engine_info_get(Raw);
	   }

	   // FIXME: what do we want to do about the struct?
	   public void EngineInfoSet(IntPtr info)
	   {
		   evas_engine_info_set(Raw, info);
	   }

	   public void OutputSizeSet(int w, int h)
	   {
		   evas_output_size_set(Raw, w, h);
	   }

	   public void OutputSizeGet(out int w, out int h)
	   {
		   evas_output_size_get(Raw, out w, out h);
	   }

	   public void OutputViewportSet(int x, int y, int w, int h)
	   {
		   evas_output_viewport_set(Raw, x, y, w, h);
	   }

	   public void OutputViewportGet(out int x, out int y, out int w, out int h)
	   {
		   evas_output_viewport_get(Raw, out x, out y, out w, out h);
	   }

	   public int CoordScreenXToWorld(int x)
	   {
		   return evas_coord_screen_x_to_world(Raw, x);
	   }

	   public int CoordScreenYToWorld(int y)
	   {
		   return evas_coord_screen_y_to_world(Raw, y);
	   }

	   public int CoordWorldXToScreen(int x)
	   {
		   return evas_coord_world_x_to_screen(Raw, x);
	   }

	   public int CoordWorldYToScreen(int y)
	   {
		   return evas_coord_world_y_to_screen(Raw, y);
	   }

	   public void PointerOutputXYGet(out int x, out int y)
	   {
		   evas_pointer_output_xy_get(Raw, out x, out y);
	   }

	   public void PointerCanvasXYGet(out int x, out int y)
	   {
		   evas_pointer_canvas_xy_get(Raw, out x, out y);
	   }

	   public int PointerButtonDownMaskGet()
	   {
		   return evas_pointer_button_down_mask_get(Raw);
	   }

	   public int PointerInsideGet()
	   {
		   return evas_pointer_inside_get(Raw);
	   }

	   [DllImport(Library)]
	   private extern static void evas_image_cache_flush(IntPtr e);

	   public void CacheFlush()
	   {
		   evas_image_cache_flush(Raw);
	   }

	   [DllImport(Library)]
	   private extern static void evas_image_cache_reload(IntPtr e);

	   public void CacheReload()
	   {
		   evas_image_cache_reload(Raw);
	   }

	   [DllImport(Library)]
	   private extern static void evas_image_cache_set(IntPtr e, int size);

	   [DllImport(Library)]
	   private extern static int evas_image_cache_get(IntPtr e);

	   public int Cache
	   {
		   get { return evas_image_cache_get(Raw); }
		   set { evas_image_cache_set(Raw, value); }
	   }

	   [DllImport(Library)]
	   private extern static void evas_font_path_clear(IntPtr e);

	   public void PathClear()
	   {
		   evas_font_path_clear(Raw);
	   }

	   [DllImport(Library)]
	   private extern static void evas_font_path_append(IntPtr e, string path);

	   public void FontPathAppend(string path)
	   {
		   evas_font_path_append(Raw, path);
	   }

	   [DllImport(Library)]
	   private extern static void evas_font_path_prepend(IntPtr e, string path);

	   public void FontPathPrepend(string path)
	   {
		   evas_font_path_prepend(Raw, path);
	   }

	   [DllImport(Library)]
	   private extern static IntPtr evas_font_path_list(IntPtr e);

	   // this is currently useless, we need to go thru the list and
	   // return something meaninful to C# programmers. I think we need
	   // to turn the previous 2 functions and this one into some ArrayList
	   // object thats heavily overloaded.
	   public IntPtr PathList()
	   {
		   return evas_font_path_list(Raw);
	   }

	   [DllImport(Library)]
	   private extern static void evas_font_cache_flush(IntPtr e);

	   public void FontCacheFlush()
	   {
		   evas_font_cache_flush(Raw);
	   }

	   [DllImport(Library)]
	   private extern static void evas_font_cache_set(IntPtr e, int size);

	   [DllImport(Library)]
	   private extern static int evas_font_cache_get(IntPtr e);

	   public int FontCache
	   {
		   get { return evas_font_cache_get(Raw); }
		   set { evas_font_cache_set(Raw, value); }
	   }

	   [DllImport(Library)]
	   private extern static IntPtr evas_object_bottom_get(IntPtr e);

	   public Item Bottom
	   {
		   get { return new Item(evas_object_bottom_get(Raw)); }
	   }

	   [DllImport(Library)]
	   private extern static IntPtr evas_object_top_get(IntPtr e);

	   public Item Top
	   {
		   get { return new Item(evas_object_top_get(Raw)); }
	   }

	   [DllImport(Library)]
	   private extern static void evas_event_freeze(IntPtr e);

	   public void Freeze()
	   {
		   evas_event_freeze(Raw);
	   }

	   [DllImport(Library)]
	   private extern static void evas_event_thaw(IntPtr e);

	   public void Thaw()
	   {
		   evas_event_thaw(Raw);
	   }

	   [DllImport(Library)]
	   private extern static bool evas_event_freeze_get(IntPtr e);

	   public bool Frozen
	   {
		   get { return evas_event_freeze_get(Raw); }
	   }

	   [DllImport(Library)]
	   private extern static void evas_render(IntPtr e);

	   public void Render()
	   {
		   evas_render(Raw);
	   }

	   [DllImport(Library)]
	   private extern static void evas_event_feed_mouse_down (IntPtr e, int b, ButtonFlags flags, uint timestamp, IntPtr data);
	   
	   public static void EventFeedMouseDown (Canvas c, int b, ButtonFlags flags, uint timestamp)
	   {
		   evas_event_feed_mouse_down (c.Raw, b, flags, timestamp, new IntPtr());
	   }
	   
	   [DllImport(Library)]
	   private extern static void evas_event_feed_mouse_up (IntPtr e, int b, ButtonFlags flags, uint timestamp, IntPtr data);
	   
	   public static void EventFeedMouseUp (Canvas c, int b, ButtonFlags flags, uint timestamp)
	   {
		   evas_event_feed_mouse_up (c.Raw, b, flags, timestamp, new IntPtr());
	   }	   
	   
	   [DllImport(Library)]
	   private extern static void evas_event_feed_mouse_move (IntPtr e, int x, int y, uint timestamp, IntPtr data);
	   
	   public static void EventFeedMouseMove (Canvas c, int x, int y, uint timestamp)
	   {
		   evas_event_feed_mouse_move (c.Raw, x, y, timestamp, new IntPtr());
	   }	   
	   
	   [DllImport(Library)]
	   private extern static void evas_event_feed_mouse_in (IntPtr e, uint timestamp, IntPtr data);
	   
	   public static void EventFeedMouseIn (Canvas c, uint timestamp)
	   {
		   evas_event_feed_mouse_in (c.Raw, timestamp, new IntPtr());
	   }
	   
	   [DllImport(Library)]
	   private extern static void evas_event_feed_mouse_out (IntPtr e, uint timestamp, IntPtr data);
	   
	   public static void EventFeedMouseOut (Canvas c, uint timestamp)
	   {
		   evas_event_feed_mouse_out (c.Raw, timestamp, new IntPtr());
	   }	   
	   
	   [DllImport(Library)]
	   private extern static void evas_event_feed_mouse_wheel (IntPtr e, int direction, int z, uint timestamp, IntPtr data);
	   
	   public static void EventFeedMouseWheel (Canvas c, int direction, int z, uint timestamp)
	   {
		   evas_event_feed_mouse_wheel (c.Raw, direction, z, timestamp, new IntPtr());
	   }	   

	   [DllImport(Library)]
	   private extern static void evas_event_feed_key_down (IntPtr e, string keyname, string key, string str, string compose, uint timestamp, IntPtr data);
	   
	   public static void EventFeedKeyDown (Canvas c, string keyname, string key, string str, string compose, uint timestamp)
	   {
		   evas_event_feed_key_down (c.Raw, keyname, key, str, compose, timestamp, new IntPtr());
	   }
	   
	   [DllImport(Library)]
	   private extern static void evas_event_feed_key_up (IntPtr e, string keyname, string key, string str, string compose, uint timestamp, IntPtr data);
	   
	   public static void EventFeedKeyUp (Canvas c, string keyname, string key, string str, string compose, uint timestamp)
	   {
		   evas_event_feed_key_up (c.Raw, keyname, key, str, compose, timestamp, new IntPtr());
	   }	   
	   
	   public virtual IntPtr Raw
	   {
		   get { return this.objRaw.Handle; }
	   }

	   ~Canvas()
	   {
		   evas_free(Raw);
	   }
   }

   [StructLayout(LayoutKind.Sequential)]
   public class EventKeyDown {
	   public string keyname;
	   public IntPtr data;
	   public IntPtr modifiers; // handle this later;
	   public IntPtr locks; // handle this later;
	   public string key;
	   public string str;
	   public string compose;

	   public EventKeyDown()
	   {}

	   public EventKeyDown(IntPtr EventInfo)
	   {
		   EventKeyDown e = new EventKeyDown();
		   e = (EventKeyDown)Marshal.PtrToStructure(EventInfo, typeof(EventKeyDown));
		   keyname = e.keyname;
		   data = e.data;
		   modifiers = e.modifiers;
		   locks = e.locks;
		   key = e.key;
		   str = e.str;
		   compose = e.compose;
	   }
   }

   [StructLayout(LayoutKind.Sequential)]
   public class EventKeyUp {
	   public string keyname;
	   public IntPtr data;
	   public IntPtr modifiers; // handle this later;
	   public IntPtr locks; // handle this later;
	   public string key;
	   public string str;
	   public string compose;

	   public EventKeyUp()
	   {}

	   public EventKeyUp(IntPtr EventInfo)
	   {
		   EventKeyUp e = new EventKeyUp();
		   e = (EventKeyUp)Marshal.PtrToStructure(EventInfo, typeof(EventKeyUp));
		   keyname = e.keyname;
		   data = e.data;
		   modifiers = e.modifiers;
		   locks = e.locks;
		   key = e.key;
		   str = e.str;
		   compose = e.compose;
	   }
   }

   [StructLayout(LayoutKind.Sequential)]
   public struct EventMouseOutput { public int x, y; };
   [StructLayout(LayoutKind.Sequential)]
   public struct EventMouseCanvas { public int x, y; };

   [StructLayout(LayoutKind.Sequential)]
   public class EventMouseDown {
	   public int button;
	   public EventMouseOutput output;
	   public EventMouseCanvas canvas;
	   public IntPtr data;
	   public IntPtr modifiers; // handle this later;
	   public IntPtr locks; // handle this later;
	   public int flags;

	   public EventMouseDown()
	   {}

	   public EventMouseDown(IntPtr EventInfo)
	   {
		   EventMouseDown e = new EventMouseDown();
		   e = (EventMouseDown)Marshal.PtrToStructure(EventInfo, typeof(EventMouseDown));
		   button = e.button;
		   output.x = e.output.x;
		   output.y = e.output.y;
		   canvas.x = e.canvas.x;
		   canvas.y = e.canvas.y;
		   data = e.data;
		   locks = e.locks;
		   flags = flags;
	   }
   }

   [StructLayout(LayoutKind.Sequential)]
   public class EventMouseUp {
	   public int button;
	   public EventMouseOutput output;
	   public EventMouseCanvas canvas;
	   public IntPtr data;
	   public IntPtr modifiers; // handle this later;
	   public IntPtr locks; // handle this later;
	   public int flags;

	   public EventMouseUp()
	   {}

	   public EventMouseUp(IntPtr EventInfo)
	   {
		   EventMouseUp e = new EventMouseUp();
		   e = (EventMouseUp)Marshal.PtrToStructure(EventInfo, typeof(EventMouseUp));
		   button = e.button;
		   output.x = e.output.x;
		   output.y = e.output.y;
		   canvas.x = e.canvas.x;
		   canvas.y = e.canvas.y;
		   data = e.data;
		   locks = e.locks;
		   flags = flags;
	   }
   }

   [StructLayout(LayoutKind.Sequential)]
   public class EventMouseIn {
	   public int buttons;
	   public EventMouseOutput output;
	   public EventMouseCanvas canvas;
	   public IntPtr data;
	   public IntPtr modifiers; // handle this later;
	   public IntPtr locks; // handle this later;

	   public EventMouseIn()
	   {}

	   public EventMouseIn(IntPtr EventInfo)
	   {
		   EventMouseIn e = new EventMouseIn();
		   e = (EventMouseIn)Marshal.PtrToStructure(EventInfo, typeof(EventMouseIn));
		   buttons = e.buttons;
		   output.x = e.output.x;
		   output.y = e.output.y;
		   canvas.x = e.canvas.x;
		   canvas.y = e.canvas.y;
		   data = e.data;
		   locks = e.locks;
	   }
   }

   [StructLayout(LayoutKind.Sequential)]
   public class EventMouseOut {
	   public int button;
	   public EventMouseOutput output;
	   public EventMouseCanvas canvas;
	   public IntPtr data;
	   public IntPtr modifiers; // handle this later;
	   public IntPtr locks; // handle this later;

	   public EventMouseOut()
	   {}

	   public EventMouseOut(IntPtr EventInfo)
	   {
		   EventMouseOut e = new EventMouseOut();
		   e = (EventMouseOut)Marshal.PtrToStructure(EventInfo, typeof(EventMouseOut));
		   button = e.button;
		   output.x = e.output.x;
		   output.y = e.output.y;
		   canvas.x = e.canvas.x;
		   canvas.y = e.canvas.y;
		   data = e.data;
		   locks = e.locks;
	   }
   }

   public struct EventMouseMovePos {
	   public EventMouseOutput output;
	   public EventMouseCanvas canvas;
   }

   [StructLayout(LayoutKind.Sequential)]
   public class EventMouseMove {
	   public int buttons;
	   public EventMouseMovePos cur;
	   public EventMouseMovePos prev;
	   public IntPtr data;
	   public IntPtr modifiers; // handle this later;
	   public IntPtr locks; // handle this later;

	   public EventMouseMove()
	   {}

	   public EventMouseMove(IntPtr EventInfo)
	   {
		   EventMouseMove e = new EventMouseMove();
		   e = (EventMouseMove)Marshal.PtrToStructure(EventInfo, typeof(EventMouseMove));
		   buttons = e.buttons;
		   cur.output.x = e.cur.output.x;
		   cur.output.y = e.cur.output.y;
		   cur.canvas.x = e.cur.canvas.x;
		   cur.canvas.y = e.cur.canvas.y;
		   prev.output.x = e.prev.output.x;
		   prev.output.y = e.prev.output.y;
		   prev.canvas.x = e.prev.canvas.x;
		   prev.canvas.y = e.prev.canvas.y;
		   data = e.data;
		   locks = e.locks;
	   }
   }

   [StructLayout(LayoutKind.Sequential)]
   public class EventMouseWheel {
	   public int direction;
	   public int z;
	   public EventMouseOutput output;
	   public EventMouseCanvas canvas;
	   public IntPtr data;
	   public IntPtr modifiers; // handle this later;
	   public IntPtr locks; // handle this later;
	   public int flags;

	   public EventMouseWheel()
	   {}

	   public EventMouseWheel(IntPtr EventInfo)
	   {
		   EventMouseWheel e = new EventMouseWheel();
		   e = (EventMouseWheel)Marshal.PtrToStructure(EventInfo, typeof(EventMouseWheel));
		   direction = e.direction;
		   z = e.z;
		   output.x = e.output.x;
		   output.y = e.output.y;
		   canvas.x = e.canvas.x;
		   canvas.y = e.canvas.y;
		   data = e.data;
		   locks = e.locks;
	   }
   }
}

