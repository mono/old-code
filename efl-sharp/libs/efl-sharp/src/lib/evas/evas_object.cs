// Copyrights:
//   hisham@hisham.cc ?
//   Laurent Debacker <debackerl gmail com>

namespace Enlightenment.Evas
{
   
  using System;
  using System.Collections;
  using System.Runtime.InteropServices;
  using System.Reflection;
  using System.Threading;
   
  public class Item
  {
	
    public delegate void PrivEventHandler(IntPtr data, IntPtr evas, IntPtr obj, IntPtr event_info);
    public delegate void EventHandler(Item w, object event_info);
	
    protected class EventController
    {
      const string Library = "evas";
      [DllImport(Library)]
      static extern void evas_object_event_callback_add(IntPtr obj, int type, PrivEventHandler func, IntPtr data);
	     
      [DllImport(Library)]
      static extern void evas_object_event_callback_del(IntPtr obj, int type, PrivEventHandler func);

      public event EventHandler InternalHandler;

      Item o;
      PrivEventHandler private_callback;
      EvasCallback evnt_num;
      Type t;
	     
      public EventController(EvasCallback Event, Item Master)
      {
	o = Master;
	evnt_num = Event;
	private_callback = new PrivEventHandler(EventCallback);
	evas_object_event_callback_add(o.Raw, (int)evnt_num, private_callback, new IntPtr());
			
	switch(evnt_num)
	{
	case EvasCallback.MouseIn: t = typeof(EventMouseIn); break;
	case EvasCallback.MouseOut: t = typeof(EventMouseOut); break;
	case EvasCallback.MouseDown: t = typeof(EventMouseDown); break;
	case EvasCallback.MouseUp: t = typeof(EventMouseUp); break;
	case EvasCallback.MouseMove: t = typeof(EventMouseMove); break;
	case EvasCallback.MouseWheel: t = typeof(EventMouseWheel); break;
	case EvasCallback.Free: t = null; break;
	case EvasCallback.KeyDown: t = typeof(EventKeyDown); break;
	case EvasCallback.KeyUp: t = typeof(EventKeyUp); break;
	case EvasCallback.FocusIn:
	case EvasCallback.FocusOut:
	case EvasCallback.Show:
	case EvasCallback.Hide:
	case EvasCallback.Move:
	case EvasCallback.Resize:
	case EvasCallback.Restack:
	  t = null;
	  break;
	}
      }
	   
      public void Finalize()
      {
		  
      }
	   
      public bool Remove(EventHandler cb)
      {
	if(InternalHandler == null)
	{
	  InternalHandler -= cb;
	  evas_object_event_callback_del(o.Raw, (int)evnt_num, private_callback);
	  return true;
	}
	return false;
      }
	     
      public void EventCallback(IntPtr data, IntPtr evas, IntPtr obj, IntPtr event_info)
      {
	object e = (t == null ? null : (object)Activator.CreateInstance (t, 
									 BindingFlags.Public | BindingFlags.Instance, null, new 
									 object[] {event_info}, Thread.CurrentThread.CurrentCulture, null));
	InternalHandler(o, e);
      }
	     

	     
    }
	
    const string Library = "evas";
	
    protected HandleRef objRaw;
    protected static Hashtable callbacks = new Hashtable ();
    protected static Hashtable dataptrs = new Hashtable ();
	
    public Canvas canvas = null;		
	
    //protected EventController[] events = new EventController[16];
    public Hashtable events = new Hashtable();
	
    public enum EvasCallback	// Do not use constants to use the type check feature of C#, and not all thoses ints from C
      {
	MouseIn = 0,
	MouseOut = 1,
	MouseDown = 2,
	MouseUp = 3,
	MouseMove = 4,
	MouseWheel = 5,
	Free = 6,
	KeyDown = 7,
	KeyUp = 8,
	FocusIn = 9,
	FocusOut = 10,
	Show = 11,
	Hide = 12,
	Move = 13,
	Resize = 14,
	Restack = 15,
      }
	
    public Item()
    {
	     
    }        
	
    public Item(Canvas c)
    {
      canvas = c;
      objRaw = new HandleRef(this, c.Raw);
    }

	
    public Item(IntPtr e)
    {
      objRaw = new HandleRef(this, e);
    }
	

    [DllImport(Library)]
    private extern static void evas_object_del(IntPtr obj);
    public virtual void Delete()
    {
      evas_object_del(Raw);
    }
	
    // see what we want to do with this
    [DllImport(Library)]
    private extern static string evas_object_type_get(IntPtr obj);
    public virtual string Type
    {
      get { return evas_object_type_get(Raw); }
    }
	
    [DllImport(Library)]
    private extern static void evas_object_layer_set(IntPtr obj, int layer);
	
    [DllImport(Library)]
    private extern static int evas_object_layer_get(IntPtr obj);
	
    public virtual int Layer
    {
      get { return evas_object_layer_get(Raw); }
      set { evas_object_layer_set(Raw, value); }
    }
	
    [DllImport(Library)]
    private extern static void evas_object_raise(IntPtr obj);
	
    public virtual void Raise()
    {
      evas_object_raise(Raw);
    }
	
    [DllImport(Library)]
    private extern static void evas_object_lower(IntPtr obj);
	
    public virtual void Lower()
    {
      evas_object_lower(Raw);
    }
	
    [DllImport(Library)]
    private extern static void evas_object_stack_above(IntPtr obj, IntPtr above);
	
    public virtual void StackAbove(Item above)
    {
      evas_object_stack_above(Raw, above.Raw);
    }
	
    [DllImport(Library)]
    private extern static void evas_object_stack_below(IntPtr obj, IntPtr below);
	
    public virtual void StackBelow(Item below)
    {
      evas_object_stack_below(Raw, below.Raw);
    }
	
	
    // Review Above and Below and look into how we need to
    // turn them into other objects, eg: Image, TextBlock etc...
    [DllImport(Library)]
    private extern static IntPtr evas_object_above_get(IntPtr obj);
	
    public virtual Item Above
    {
      get { return new Item(evas_object_above_get(Raw)); }
    }
	
    [DllImport(Library)]
    private extern static IntPtr evas_object_below_get(IntPtr obj);
	
    public virtual Item Below
    {
      get { return new Item(evas_object_below_get(Raw)); }
    }		
	
    [DllImport(Library)]
    private extern static void evas_object_move(IntPtr obj, int x, int y);
	
    public virtual void Move(int x, int y)
    {
      evas_object_move(Raw, x, y);
    }
	
    [DllImport(Library)]
    private extern static void evas_object_geometry_get(IntPtr obj, out int x, out int y, out int w, out int h);

    public virtual Evas.Geometry Geometry
    {
      get {
	int x, y, w, h;
	evas_object_geometry_get(Raw, out x, out y, out w, out h);
	return new Evas.Geometry(x, y, w, h);
      }
    }
	
    [DllImport(Library)]
    private extern static void evas_object_resize(IntPtr obj, int w, int h);
	
    public virtual void Resize(int w, int h)
    {
      evas_object_resize(Raw, w, h);
    }
		
    [DllImport(Library)]
    private extern static void evas_object_show(IntPtr obj);
	
    public virtual void Show()
    {
      evas_object_show(Raw);
    }
		  
    [DllImport(Library)]	
    private extern static void evas_object_hide(IntPtr obj);
	
    public virtual void Hide()
    {
      evas_object_hide(Raw);
    }
			
    [DllImport(Library)]	
    private extern static bool evas_object_visible_get(IntPtr obj);
	
    public virtual bool Visible
    {
      get { return evas_object_visible_get(Raw); }
      set {
	if(value)
	  evas_object_show(Raw);
	else
	  evas_object_hide(Raw);
      }
    }	
	
    [DllImport(Library)]	
    private extern static void evas_object_color_set(IntPtr obj, int r, int g, int b, int a);
	
    [DllImport(Library)]	
    private extern static void evas_object_color_get(IntPtr obj, out int r, out int g, out int b, out int a);
	
    public virtual Evas.Color Color
    {
      get {
	int r, g, b, a;
	evas_object_color_get(Raw, out r, out g, out b, out a);
	return new Color(r, g, b, a);
      }
	     
      set {
	evas_object_color_set(Raw, value.R, value.G, value.B, value.A);
      }
    }
	
    [DllImport(Library)]
    private extern static void evas_object_clip_set(IntPtr obj, IntPtr clip);
	
    [DllImport(Library)]
    private extern static IntPtr evas_object_clip_get(IntPtr obj);
	
    public virtual Evas.Item Clip
    {
      get { return new Item(evas_object_clip_get(Raw)); }
      set { evas_object_clip_set(Raw, value.Raw); }
    }
	
    [DllImport(Library)]
    private extern static IntPtr evas_object_evas_get(IntPtr obj);
	
    public IntPtr CanvasGetRaw()
    {
      return evas_object_evas_get(Raw);
    }
	
    [DllImport(Library)]
    private extern static void evas_object_focus_set(IntPtr obj, int focus);
	
    [DllImport(Library)]
    private extern static int evas_object_focus_get(IntPtr obj);
	
    public virtual int Focus
    {
      get { return evas_object_focus_get(Raw); }
      set { evas_object_focus_set(Raw, value); }
    }		
			
    [DllImport(Library)]
    private extern static void evas_object_repeat_events_set(IntPtr obj, bool repeat);
	
    [DllImport(Library)]
    private extern static bool evas_object_repeat_events_get(IntPtr obj);
	
    public virtual bool RepeatEvents
    {
      get { return evas_object_repeat_events_get(Raw); }
      set { evas_object_repeat_events_set(Raw, value); }
    }
	
    [DllImport(Library)]
    private extern static void evas_object_pass_events_set(IntPtr obj, bool pass);
	
    [DllImport(Library)]
    private extern static bool evas_object_pass_events_get(IntPtr obj);
	
    public virtual bool PassEvents
    {
      get { return evas_object_pass_events_get(Raw); }
      set { evas_object_pass_events_set(Raw, value); }
    }	
	
    internal void AddEventHandler(EvasCallback Event, EventHandler value)
    {
      EventController ec = (EventController)events[Event];
      if(ec == null) events[Event] = ec = new EventController(Event, this);
      ec.InternalHandler += value;
    }
	
    internal void RemoveEventHandler(EvasCallback Event, EventHandler value)
    {
      EventController ec = (EventController)events[Event];
      if(ec == null) return;
      if(ec.Remove(value))
	events.Remove(Event);
    }
	
    public event EventHandler MouseInEvent
    {
      add { AddEventHandler(EvasCallback.MouseIn, value); }
      remove { RemoveEventHandler(EvasCallback.MouseIn, value); }
    }
	
    public event EventHandler MouseOutEvent
    {
      add { AddEventHandler(EvasCallback.MouseOut, value); }
      remove { RemoveEventHandler(EvasCallback.MouseOut, value); }
    }	
	
    public event EventHandler MouseUpEvent
    {
      add { AddEventHandler(EvasCallback.MouseUp, value); }
      remove { RemoveEventHandler(EvasCallback.MouseUp, value); }
    }	
	
    public event EventHandler MouseDownEvent
    {
      add { AddEventHandler(EvasCallback.MouseDown, value); }
      remove { RemoveEventHandler(EvasCallback.MouseDown, value); }
    }	
	
    public event EventHandler MouseMoveEvent
    {
      add { AddEventHandler(EvasCallback.MouseMove, value); }
      remove { RemoveEventHandler(EvasCallback.MouseMove, value); }
    }	
	
    public event EventHandler MouseWheelEvent
    {
      add { AddEventHandler(EvasCallback.MouseWheel, value); }
      remove { RemoveEventHandler(EvasCallback.MouseWheel, value); }
    }	
	
    public event EventHandler FreeEvent
    {
      add { AddEventHandler(EvasCallback.Free, value); }
      remove { RemoveEventHandler(EvasCallback.Free, value); }
    }	
	
    public event EventHandler KeyDownEvent
    {
      add { AddEventHandler(EvasCallback.KeyDown, value); }
      remove { RemoveEventHandler(EvasCallback.KeyDown, value); }
    }
	
    public event EventHandler KeyUpEvent
    {
      add { AddEventHandler(EvasCallback.KeyUp, value); }
      remove { RemoveEventHandler(EvasCallback.KeyUp, value); }
    }
	
    public event EventHandler FocusInEvent
    {
      add { AddEventHandler(EvasCallback.FocusIn, value); }
      remove { RemoveEventHandler(EvasCallback.FocusIn, value); }
    }
	
    public event EventHandler FocusOutEvent
    {
      add { AddEventHandler(EvasCallback.FocusOut, value); }
      remove { RemoveEventHandler(EvasCallback.FocusOut, value); }
    }
	
    public event EventHandler ShowEvent
    {
      add { AddEventHandler(EvasCallback.Show, value); }
      remove { RemoveEventHandler(EvasCallback.Show, value); }
    }
	
    public event EventHandler HideEvent
    {
      add { AddEventHandler(EvasCallback.Hide, value); }
      remove { RemoveEventHandler(EvasCallback.Hide, value); }
    }
	
    public event EventHandler MoveEvent
    {
      add { AddEventHandler(EvasCallback.Move, value); }
      remove { RemoveEventHandler(EvasCallback.Move, value); }
    }
	
    public event EventHandler ResizeEvent
    {
      add { AddEventHandler(EvasCallback.Resize, value); }
      remove { RemoveEventHandler(EvasCallback.Resize, value); }
    }
	
    public event EventHandler RestackEvent
    {
      add { AddEventHandler(EvasCallback.Restack, value); }
      remove { RemoveEventHandler(EvasCallback.Restack, value); }
    }
	
    public Canvas evas
    {
      get { return canvas; }
      set { canvas = value; }
    }

	
    public object GetItem(IntPtr p)
    {
      if(!dataptrs.Contains(p))
	throw new Exception("Item pointer is invalid");
      // FIXME: Should we remove it?
      return dataptrs[p];
    }   	
	
    public virtual IntPtr Raw
    {
      get { return objRaw.Handle; }    
    }	
	
    ~Item()
    {
    }  
  }
}
