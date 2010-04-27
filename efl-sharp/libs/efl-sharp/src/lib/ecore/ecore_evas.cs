namespace Enlightenment.Ecore
{

  using System;
  using System.Collections;
  using System.Runtime.InteropServices;
  using Enlightenment.Ecore.X;
   
  public class Window
  {

    public Hashtable events = new Hashtable();    
	
    public enum EcoreEvasCallback {
      Resize,
      Move,
      Show,
      Hide,
      DeleteRequest,
      Destroy,
      FocusIn,
      FocusOut,
      MouseIn,
      MouseOut,
      PreRender,
      PostRender
    }
	
    public delegate void PrivEventHandler(IntPtr ee);
    public delegate void EventHandler(Enlightenment.Ecore.Window ee);
    public delegate void ecore_evas_callback_add(IntPtr ee, PrivEventHandler cb);
	
    public class EventController
    {
      const string Library = "ecore_evas";
	   
      EcoreEvasCallback evnt_num;
      Enlightenment.Ecore.Window window;
      EventHandler user_callback;
      PrivEventHandler private_callback;
      ecore_evas_callback_add event_clib_callback;     
      public event EventHandler InternalHandler;
	   
      public EventController(EcoreEvasCallback Event, Enlightenment.Ecore.Window c)
      {
	window = c;
	evnt_num = Event;
	private_callback = new PrivEventHandler(EventCallback);
	switch(evnt_num) 
	{
	 case EcoreEvasCallback.Resize: ecore_evas_callback_resize_set(window.Raw, private_callback); break;
	 case EcoreEvasCallback.Move: ecore_evas_callback_move_set(window.Raw, private_callback); break;
	 case EcoreEvasCallback.Show: ecore_evas_callback_show_set(window.Raw, private_callback); break;
	 case EcoreEvasCallback.Hide: ecore_evas_callback_hide_set(window.Raw, private_callback); break;
	 case EcoreEvasCallback.DeleteRequest: ecore_evas_callback_delete_request_set(window.Raw, private_callback); break;
	 case EcoreEvasCallback.Destroy: ecore_evas_callback_destroy_set(window.Raw, private_callback); break;
	 case EcoreEvasCallback.FocusIn: ecore_evas_callback_focus_in_set(window.Raw, private_callback); break;
	 case EcoreEvasCallback.FocusOut: ecore_evas_callback_focus_out_set(window.Raw, private_callback); break;
	 case EcoreEvasCallback.MouseIn: ecore_evas_callback_mouse_in_set(window.Raw, private_callback); break;
	 case EcoreEvasCallback.MouseOut: ecore_evas_callback_mouse_out_set(window.Raw, private_callback); break;
	 case EcoreEvasCallback.PreRender: ecore_evas_callback_pre_render_set(window.Raw, private_callback); break;
	 case EcoreEvasCallback.PostRender: ecore_evas_callback_post_render_set(window.Raw, private_callback); break;
	}
      }
	   
      /* TODO: This still needs to be done */
      public bool Remove(EventHandler cb)
      {
	if(InternalHandler == null)
	{
	  InternalHandler -= cb;
	  return true;
	}
	return false;
      }
	     
      public void EventCallback(IntPtr ee)
      {
	InternalHandler(window);
      }	     	     
    }	
	
    const string Library = "ecore_evas";
	
    [DllImport(Library)]
    private extern static int ecore_evas_engine_type_supported_get(IntPtr engine);       
	
    [DllImport(Library)]
    private extern static int ecore_evas_init();
	
    [DllImport(Library)]
    private extern static int ecore_evas_shutdown();      
	
    [DllImport(Library)]
    private extern static IntPtr ecore_evas_software_x11_new(string disp_name,
							     IntPtr parent,
							     int x, int y, 
							     int w, int h);
    [DllImport(Library)]
    private extern static IntPtr ecore_evas_software_x11_window_get(IntPtr ee);
	
    [DllImport(Library)]
    private extern static IntPtr ecore_evas_software_x11_subwindow_get(IntPtr ee);
	
    [DllImport(Library)]
    private extern static void ecore_evas_software_x11_direct_resize_set(IntPtr ee, int on);
	
    [DllImport(Library)]
    private extern static int ecore_evas_software_x11_direct_resize_get(IntPtr ee);
	
    [DllImport(Library)]
    private extern static IntPtr ecore_evas_gl_x11_new(string disp_name, IntPtr parent, int x, int y, int w, int h);
	
    [DllImport(Library)]
    private extern static IntPtr ecore_evas_gl_x11_window_get(IntPtr ee);
	
    [DllImport(Library)]
    private extern static IntPtr ecore_evas_gl_x11_subwindow_get(IntPtr ee);
	
    [DllImport(Library)]
    private extern static void ecore_evas_gl_x11_direct_resize_set(IntPtr ee, int on);
	
    [DllImport(Library)]
    private extern static int ecore_evas_gl_x11_direct_resize_get(IntPtr ee);
	
    [DllImport(Library)]
    private extern static IntPtr ecore_evas_fb_new(string disp_name, int rotation, int w, int h);
	
    [DllImport(Library)]
    private extern static IntPtr ecore_evas_buffer_new(int w, int h);
	
    [DllImport(Library)]
    private extern static int ecore_evas_buffer_pixels_get(IntPtr ee);
	
    [DllImport(Library)]
    private extern static IntPtr ecore_evas_object_image_new(IntPtr ee_target);
	
    [DllImport(Library)]
    private extern static void ecore_evas_free(IntPtr ee);
	
    [DllImport(Library)]
    private extern static IntPtr ecore_evas_data_get(IntPtr ee, string key);
	
    [DllImport(Library)]
    private extern static void ecore_evas_data_set(IntPtr ee, string key, IntPtr data);
	
    [DllImport(Library)]
    private extern static void ecore_evas_callback_resize_set(IntPtr ee, PrivEventHandler func);

    private void AddEventHandler(EcoreEvasCallback Event, EventHandler value)
    {
      EventController ec = (EventController)events[Event];
      if(ec == null) events[Event] = ec = new EventController(Event, this);
      ec.InternalHandler += value;
    }
    
    private void RemoveEventHandler(EcoreEvasCallback Event, EventHandler value)
    {
      EventController ec = (EventController)events[Event];
      if(ec == null) return;
      if(ec.Remove(value))
	events.Remove(Event);
    }
	
    public event EventHandler ResizeEvent
    {
      add { AddEventHandler(EcoreEvasCallback.Resize, value); }
      remove { RemoveEventHandler(EcoreEvasCallback.Resize, value); }
    }
			
    [DllImport(Library)]
    private extern static void ecore_evas_callback_move_set(IntPtr ee, PrivEventHandler func);
	
    public event EventHandler MoveEvent
    {
      add { AddEventHandler(EcoreEvasCallback.Move, value); }
      remove { RemoveEventHandler(EcoreEvasCallback.Move, value); }
    }
	
	
    [DllImport(Library)]
    private extern static void ecore_evas_callback_show_set(IntPtr ee, PrivEventHandler func);
	
    public event EventHandler ShowEvent
    {
      add { AddEventHandler(EcoreEvasCallback.Show, value); }
      remove { RemoveEventHandler(EcoreEvasCallback.Show, value); }      
    }
	
    [DllImport(Library)]
    private extern static void ecore_evas_callback_hide_set(IntPtr ee, PrivEventHandler func);
	
    public event EventHandler HideEvent
    {
      add { AddEventHandler(EcoreEvasCallback.Hide, value); }
      remove { RemoveEventHandler(EcoreEvasCallback.Hide, value); }
    }
	
    [DllImport(Library)]
    private extern static void ecore_evas_callback_delete_request_set(IntPtr ee, PrivEventHandler func);
	
    public event EventHandler DeleteRequestEvent
    {
      add { AddEventHandler(EcoreEvasCallback.DeleteRequest, value); }
      remove { RemoveEventHandler(EcoreEvasCallback.DeleteRequest, value); }
    }	
	
    [DllImport(Library)]
    private extern static void ecore_evas_callback_destroy_set(IntPtr ee, PrivEventHandler func);
	
    public event EventHandler DestroyEvent
    {
      add { AddEventHandler(EcoreEvasCallback.Destroy, value); }
      remove { RemoveEventHandler(EcoreEvasCallback.Destroy, value); }
    }	
	
    [DllImport(Library)]
    private extern static void ecore_evas_callback_focus_in_set(IntPtr ee, PrivEventHandler func);
	
    public event EventHandler FocusInEvent
    {
      add { AddEventHandler(EcoreEvasCallback.FocusIn, value); }
      remove { RemoveEventHandler(EcoreEvasCallback.FocusIn, value); }
    }	
	
    [DllImport(Library)]
    private extern static void ecore_evas_callback_focus_out_set(IntPtr ee, PrivEventHandler func);
	
    public event EventHandler FocusOutEvent
    {
      add { AddEventHandler(EcoreEvasCallback.FocusOut, value); }
      remove { RemoveEventHandler(EcoreEvasCallback.FocusOut, value); }
    }	
	
    [DllImport(Library)]
    private extern static void ecore_evas_callback_mouse_in_set(IntPtr ee, PrivEventHandler func);
	
    public event EventHandler MouseInEvent
    {
      add { AddEventHandler(EcoreEvasCallback.MouseIn, value); }
      remove { RemoveEventHandler(EcoreEvasCallback.MouseIn, value); }
    }	
	
    [DllImport(Library)]
    private extern static void ecore_evas_callback_mouse_out_set(IntPtr ee, PrivEventHandler func);
	
    public event EventHandler MouseOutEvent
    {
      add { AddEventHandler(EcoreEvasCallback.MouseOut, value); }
      remove { RemoveEventHandler(EcoreEvasCallback.MouseOut, value); }
    }	
	
    [DllImport(Library)]
    private extern static void ecore_evas_callback_pre_render_set(IntPtr ee, PrivEventHandler func);
	
    public event EventHandler PreRenderEvent
    {
      add { AddEventHandler(EcoreEvasCallback.PreRender, value); }
      remove { RemoveEventHandler(EcoreEvasCallback.PreRender, value); }
    }	
	
    [DllImport(Library)]
    private extern static void ecore_evas_callback_post_render_set(IntPtr ee, PrivEventHandler func);   
	
    public event EventHandler PostRenderEvent
    {
      add { AddEventHandler(EcoreEvasCallback.PostRender, value); }
      remove { RemoveEventHandler(EcoreEvasCallback.PostRender, value); }
    }	
	
    [DllImport(Library)]
    private extern static IntPtr ecore_evas_get(IntPtr ee);
	
    [DllImport(Library)]
    private extern static void ecore_evas_move(IntPtr ee, int x, int y);   
	
    [DllImport(Library)]
    private extern static void ecore_evas_resize(IntPtr ee, int w, int h);
	
    [DllImport(Library)]
    private extern static void ecore_evas_move_resize(IntPtr ee, int x, int y, int w, int h);
	
    [DllImport(Library)]
    private extern static void ecore_evas_geometry_get(IntPtr ee, out int x, out int y, out int w, out int h);
	
    [DllImport(Library)]
    private extern static void ecore_evas_rotation_set(IntPtr ee, int rot);
	
    [DllImport(Library)]
    private extern static int ecore_evas_rotation_get(IntPtr ee);
	
    [DllImport(Library)]
    private extern static void ecore_evas_shaped_set(IntPtr ee, int shaped);
	
    [DllImport(Library)]
    private extern static int ecore_evas_shaped_get(IntPtr ee);
	
    [DllImport(Library)]
    private extern static void ecore_evas_show(IntPtr ee);  
	
    [DllImport(Library)]
    private extern static void ecore_evas_hide(IntPtr ee);
	
    [DllImport(Library)]
    private extern static bool ecore_evas_visibility_get(IntPtr ee);
	
    [DllImport(Library)]
    private extern static void ecore_evas_raise(IntPtr ee);
	
    [DllImport(Library)]
    private extern static void ecore_evas_lower(IntPtr ee);
	
    [DllImport(Library)]
    private extern static void ecore_evas_title_set(IntPtr ee, string t);
	
    [DllImport(Library)]
    private extern static string ecore_evas_title_get(IntPtr ee);
	
    [DllImport(Library)]
    private extern static void ecore_evas_name_class_set(IntPtr ee, string n, string c);
	
    [DllImport(Library)]
    private extern static void ecore_evas_name_class_get(IntPtr ee, out string n, out string c);
	
    [DllImport(Library)]
    private extern static void ecore_evas_size_min_set(IntPtr ee, int w, int h);
	
    [DllImport(Library)]
    private extern static void ecore_evas_size_min_get(IntPtr ee, out int w, out int h);
	
    [DllImport(Library)]
    private extern static void ecore_evas_size_max_set(IntPtr ee, int w, int h);
	
    [DllImport(Library)]
    private extern static void ecore_evas_size_max_get(IntPtr ee, out int w, out int h);
	
    [DllImport(Library)]
    private extern static void ecore_evas_size_base_set(IntPtr ee, int w, int h);
	
    [DllImport(Library)]
    private extern static void ecore_evas_size_base_get(IntPtr ee, out int w, out int h);
	
    [DllImport(Library)]
    private extern static void ecore_evas_size_step_set(IntPtr ee, int w, int h);
	
    [DllImport(Library)]
    private extern static void ecore_evas_size_step_get(IntPtr ee, out int w, out int h);
	
    [DllImport(Library)]
    private extern static void ecore_evas_cursor_set(IntPtr ee, string file, int layer, int hot_x, int hot_y);
	
    [DllImport(Library)]
    private extern static void ecore_evas_cursor_get(IntPtr ee, out string file, out int layer, out int hot_x, out int hot_y);
	
    [DllImport(Library)]
    private extern static void ecore_evas_layer_set(IntPtr ee, int layer);
	
    [DllImport(Library)]
    private extern static int ecore_evas_layer_get(IntPtr ee);
	
    [DllImport(Library)]
    private extern static void ecore_evas_focus_set(IntPtr ee, int on);
	
    [DllImport(Library)]
    private extern static int ecore_evas_focus_get(IntPtr ee);
	
    [DllImport(Library)]
    private extern static void ecore_evas_iconified_set(IntPtr ee, int on);
	
    [DllImport(Library)]
    private extern static int ecore_evas_iconified_get(IntPtr ee);
	
    [DllImport(Library)]
    private extern static void ecore_evas_borderless_set(IntPtr ee, int on);
	
    [DllImport(Library)]
    private extern static int ecore_evas_borderless_get(IntPtr ee);
	
    [DllImport(Library)]
    private extern static void ecore_evas_override_set(IntPtr ee, int on);
	
    [DllImport(Library)]
    private extern static int ecore_evas_override_get(IntPtr ee);
	
    [DllImport(Library)]
    private extern static void ecore_evas_maximized_set(IntPtr ee, int on);
	
    [DllImport(Library)]
    private extern static int ecore_evas_maximized_get(IntPtr ee);
	
    [DllImport(Library)]
    private extern static void ecore_evas_fullscreen_set(IntPtr ee, int on);
	
    [DllImport(Library)]
    private extern static int ecore_evas_fullscreen_get(IntPtr ee);
	
    [DllImport(Library)]
    private extern static void ecore_evas_avoid_damage_set(IntPtr ee, int on);
	
    [DllImport(Library)]
    private extern static int ecore_evas_avoid_damage_get(IntPtr ee);
	
    [DllImport(Library)]
    private extern static void ecore_evas_withdrawn_set(IntPtr ee, int withdrawn);
	
    [DllImport(Library)]
    private extern static int ecore_evas_withdrawn_get(IntPtr ee);
	
    [DllImport(Library)]
    private extern static void ecore_evas_sticky_set(IntPtr ee, int sticky);
	
    [DllImport(Library)]
    private extern static int ecore_evas_sticky_get(IntPtr ee);
	
    private HandleRef objRaw;
    private static Hashtable callbacks = new Hashtable ();
    private static Hashtable dataptrs = new Hashtable ();
    private static Hashtable internal_objs = new Hashtable();
	
    public const int ECORE_EVAS_ENGINE_SOFTWARE_X11 = 0;
    public const int ECORE_EVAS_ENGINE_SOFTWARE_FB = 1;
    public const int ECORE_EVAS_ENGINE_GL_X11 = 2;
    public const int ECORE_EVAS_ENGINE_SOFTWARE_BUFFER = 3;
	
		/* we cant instantiate this, we use one of the get calls
		 * to get an ecore_evas specific to an engine
		 */
    protected Window()
    { }
	
    /* ************************* */
	
    public static int EngineTypeSupportedGet(IntPtr engine)
    {
      return ecore_evas_engine_type_supported_get(engine);
    }
	
    /* ************************* */
	
    public static int Init()
    {
      return ecore_evas_init();
    }
	
    public static int ShutDown()
    {
      return ecore_evas_shutdown();
    }
	
    /* ************************* */
	
    public static Window SoftwareX11New(string disp_name, Window parent, int x, int y, int w, int h)
    {
			Window window = new Window();
			IntPtr parentIntPtr;

			if (parent != null)
			{
				parentIntPtr = parent.Raw;
			}
			else
			{
				parentIntPtr = IntPtr.Zero;
			}

      window.objRaw = new HandleRef(window, ecore_evas_software_x11_new(disp_name, parentIntPtr, x, y, w, h));
			return window;
    }
	
	
    /* FIXME: Make this look what type of window we have and return the window accordingly */
    public Ecore.X.XWindow XWindow
    {
      get { return new XWindow(ecore_evas_software_x11_window_get(Raw)); }
    }
		
    public Ecore.X.XWindow SubWindow
    {
      get { return new XWindow(ecore_evas_software_x11_subwindow_get(Raw)); }
    }
    
	
    public XWindow SoftwareX11WindowGet()
    {
      return new XWindow(ecore_evas_software_x11_window_get(Raw));
    }
	
    public IntPtr SoftwareX11SubWindowGet()
    {
      return ecore_evas_software_x11_subwindow_get(Raw);
    }
	
    public void SoftwareX11DirectResizeSet(int on)
    {
      ecore_evas_software_x11_direct_resize_set(Raw, on);
    }
	
    public int SoftwareX11DirectResizeGet()
    {
      return ecore_evas_software_x11_direct_resize_get(Raw);
    }
	
    /* ************************* */
	
    public void GlX11New(string disp_name, IntPtr parent, int x, int y, int w, int h)
    {
      objRaw = new HandleRef(this, ecore_evas_gl_x11_new(disp_name, parent, x, y, w, h));
    }
	
    public IntPtr GlX11WindowGet()
    {
      return ecore_evas_gl_x11_window_get(Raw);
    }
	
    public IntPtr GlX11SubWindowGet()
    {
      return ecore_evas_gl_x11_subwindow_get(Raw);
    }
	
    public void GlX11DirectResizeSet(int on)
    {
      ecore_evas_gl_x11_direct_resize_set(Raw, on);
    }
	
    public int GlX11DirectResizeGet()
    {
      return ecore_evas_gl_x11_direct_resize_get(Raw);
    }
	
    /* ************************* */
	
    public void FbNew(string DispName, int Rotation, int w, int h)
    {
      ecore_evas_fb_new(DispName, Rotation, w, h);
    }
	
    /* ************************* */
	
    public void BufferNew(int w, int h)
    {
      objRaw = new HandleRef(this, ecore_evas_buffer_new(w, h));
    }
	
    public int BufferPixelsGet()
    {
      return ecore_evas_buffer_pixels_get(Raw);
    }
	
    /* ************************* */
	
    public IntPtr ObjectImageNew()
    {
      return ecore_evas_object_image_new(Raw);
    }
	
    /* ************************* */
	
    public void Free()
    {
      ecore_evas_free(Raw);
    }
	
    public object DataGet(string key)
    {
      //IntPtr p = ecore_evas_data_get(Raw, key);
      //return dataptrs[p];
      return internal_objs[key];
    }
	
    public void DataSet(string key, object data)
    {
      //IntPtr p = new IntPtr(dataptrs.Count);
      //dataptrs[p] = data;
      //ecore_evas_data_set(Raw, key, p);
      internal_objs[key] = data;
    }
	
    public Evas.Canvas Get()
    {
      return new Enlightenment.Evas.Canvas(ecore_evas_get(Raw));
    }
	
    public void Move(int x, int y)
    {
      ecore_evas_move(Raw, x, y);
    }
	
    public void Resize(int w, int h)
    {
      ecore_evas_resize(Raw, w, h);
    }
	
    public void MoveResize(int x, int y, int w, int h)
    {
      ecore_evas_move_resize(Raw, x, y, w, h);
    }
	
    public Enlightenment.Evas.Geometry Geometry {
	  
      get {
	int x, y, w, h;	    
	ecore_evas_geometry_get(Raw, out x, out y, out w, out h);
	return new Enlightenment.Evas.Geometry(x, y, w, h);
      }

    }
	
    public void RotationSet(int rot)
    {
      ecore_evas_rotation_set(Raw, rot);
    }
	
    public int RotationGet()
    {
      return ecore_evas_rotation_get(Raw);
    }
	
    public void ShapedSet(int shaped)
    {
      ecore_evas_shaped_set(Raw, shaped);
    }
	
    public int ShapedGet()
    {
      return ecore_evas_shaped_get(Raw);
    }
	
    public void Show()
    {
      ecore_evas_show(Raw);
    }
	
    public void Hide()
    {
      ecore_evas_hide(Raw);
    }
	
    public bool Visible {
      get { return ecore_evas_visibility_get(Raw); }
    }
	
    public void Raise()
    {
      ecore_evas_raise(Raw);
    }
	
    public void Lower()
    {
      ecore_evas_lower(Raw);
    }
	
    public string Title
    {
      get { return ecore_evas_title_get(Raw); }
      set { ecore_evas_title_set(Raw, value); }
    }
		
    public void NameClassSet(string n, string c)
    {
      ecore_evas_name_class_set(Raw, n, c);
    }
	
    public void NameClassGet(out string n, out string c)
    {
      ecore_evas_name_class_get(Raw, out n, out c);
    }

    public void SizeMinSet(int w, int h)
    {
      ecore_evas_size_min_set(Raw, w, h);
    }
	
    public void SizeMinGet(out int w, out int h)
    {
      ecore_evas_size_min_get(Raw, out w, out h);
    }
	
    public void SizeMaxSet(int w, int h)
    {
      ecore_evas_size_max_set(Raw, w, h);
    }
	
    public void SizeMaxGet(out int w, out int h)
    {
      ecore_evas_size_max_get(Raw, out w, out h);
    }
	
    public void SizeBaseSet(int w, int h)
    {
      ecore_evas_size_base_set(Raw, w, h);
    }
	
    public void SizeBaseGet(out int w, out int h)
    {
      ecore_evas_size_base_get(Raw, out w, out h);
    }   
	
    public void SizeStepSet(int w, int h)
    {
      ecore_evas_size_step_set(Raw, w, h);
    }
	
    public void SizeStepGet(out int w, out int h)
    {
      ecore_evas_size_step_get(Raw, out w, out h);
    }   
	
    public void CursorSet(string file, int layer, int HotX, int HotY)
    {
      ecore_evas_cursor_set(Raw, file, layer, HotX, HotY);
    }
	
    public void CursorGet(out string file, out int layer, out int HotX, out int HotY)
    {
      ecore_evas_cursor_get(Raw, out file, out layer, out HotX, out HotY);
    }
	
    public int Layer
    {
      get { return ecore_evas_layer_get(Raw); }
      set { ecore_evas_layer_set(Raw, value); }
    }		   
	
    public int LayerSet()
    {
      return ecore_evas_layer_get(Raw);
    }
	
    public void FocusSet(int on)
    {
      ecore_evas_focus_set(Raw, on);
    }
	
    public int FocusGet()
    {
      return ecore_evas_focus_get(Raw);
    }
	
    public void IconifiedSet(int on)
    {
      ecore_evas_iconified_set(Raw, on);
    }
	
    public int IconifiedGet()
    {
      return ecore_evas_iconified_get(Raw);
    }   
	
    public int Borderless {
      get { return ecore_evas_borderless_get(Raw); }
      set { ecore_evas_borderless_set(Raw, value); }
    }
		
    public void OverrideSet(int on)
    {
      ecore_evas_override_set(Raw, on);
    }
	
    public int OverrideGet()
    {
      return ecore_evas_override_get(Raw);
    }   
	
    public void MaximizedSet(int on)
    {
      ecore_evas_maximized_set(Raw, on);
    }
	
    public int MaximizedGet()
    {
      return ecore_evas_maximized_get(Raw);
    }
	
    public void FullscreenSet(int on)
    {
      ecore_evas_fullscreen_set(Raw, on);
    }
	
    public int FullscreenGet()
    {
      return ecore_evas_fullscreen_get(Raw);
    }   
	
    public void AvoidDamageSet(int on)
    {
      ecore_evas_avoid_damage_set(Raw, on);
    }
	
    public int AvoidDamageGet()
    {
      return ecore_evas_avoid_damage_get(Raw);
    }
	
    public void WithdraenSet(int on)
    {
      ecore_evas_withdrawn_set(Raw, on);
    }
	
    public int WithdrawnGet()
    {
      return ecore_evas_withdrawn_get(Raw);
    }
	
    public void StickySet(int on)
    {
      ecore_evas_sticky_set(Raw, on);
    }
	
    public int StickyGet()
    {
      return ecore_evas_sticky_get(Raw);
    }
	
    public virtual IntPtr Raw
    {
      get { return objRaw.Handle; }
    }
	
    ~Window()
    {
      ecore_evas_free(Raw);
    }   	
  }   
}
