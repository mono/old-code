namespace Enlightenment.Ecore
{

   using System;
   using System.Collections;
   using System.Runtime.InteropServices;
   using System.Reflection;
   using System.Threading;
   using Enlightenment;
   using Enlightenment.Ecore.X;
  /* ****************** */
  /* Event Info Structs */
  /* ****************** */

   [StructLayout(LayoutKind.Sequential)]
   public class EcoreEventExeExit
   {
	   public int pid;
	   public int exit_code;
	   public IntPtr exe; // do this later;
	   public int exit_signal;
	   public char exited;
	   public char signalled;
	   public IntPtr ext_data;
	   public IntPtr data; // do this later, its a siginfo_t

	   public EcoreEventExeExit()
	   {}

	   public EcoreEventExeExit(IntPtr EventInfo)
	   {
		   EcoreEventExeExit e = new EcoreEventExeExit();
		   e = (EcoreEventExeExit)Marshal.PtrToStructure(EventInfo, typeof(EcoreEventExeExit));
		   pid = e.pid;
		   exit_code = e.exit_code;
		   exe = e.exe;
		   exit_signal = e.exit_signal;
		   exited = e.exited;
		   signalled = e.signalled;
		   ext_data = e.ext_data;
		   data = e.data;
	   }
   }

   [StructLayout(LayoutKind.Sequential)]
   public class EcoreEventSignalUser
   {
	   public int number;
	   public IntPtr ext_data;
	   public IntPtr data; // do this later, its a siginfo_t

	   public EcoreEventSignalUser()
	   {}

	   public EcoreEventSignalUser(IntPtr EventInfo)
	   {
		   EcoreEventSignalUser e = new EcoreEventSignalUser();
		   e = (EcoreEventSignalUser)Marshal.PtrToStructure(EventInfo, typeof(EcoreEventSignalUser));
		   number = e.number;
		   ext_data = e.ext_data;
		   data = data;
	   }
   }

   [StructLayout(LayoutKind.Sequential)]
   public class EcoreEventSignalHup
   {
	   public IntPtr ext_data;
	   public IntPtr data; // do this later, its a siginfo_t

	   public EcoreEventSignalHup()
	   {}

	   public EcoreEventSignalHup(IntPtr EventInfo)
	   {
		   EcoreEventSignalHup e = new EcoreEventSignalHup();
		   e = (EcoreEventSignalHup)Marshal.PtrToStructure(EventInfo, typeof(EcoreEventSignalHup));
		   ext_data = e.ext_data;
		   data = data;
	   }
   }

   [StructLayout(LayoutKind.Sequential)]
   public class EcoreEventSignalExit
   {
	   public int interrupt;
	   public int quit;
	   public int temrinate;
	   public IntPtr ext_data;
	   public IntPtr data; // do this later, its a siginfo_t

	   public EcoreEventSignalExit()
	   {}

	   public EcoreEventSignalExit(IntPtr EventInfo)
	   {
		   EcoreEventSignalExit e = new EcoreEventSignalExit();
		   e = (EcoreEventSignalExit)Marshal.PtrToStructure(EventInfo, typeof(EcoreEventSignalExit));
		   ext_data = e.ext_data;
		   data = data;
	   }
   }

   [StructLayout(LayoutKind.Sequential)]
   public class EcoreEventSignalPower
   {
	   public IntPtr ext_data;
	   public IntPtr data; // do this later, its a siginfo_t

	   public EcoreEventSignalPower()
	   {}

	   public EcoreEventSignalPower(IntPtr EventInfo)
	   {
		   EcoreEventSignalPower e = new EcoreEventSignalPower();
		   e = (EcoreEventSignalPower)Marshal.PtrToStructure(EventInfo, typeof(EcoreEventSignalPower));
		   ext_data = e.ext_data;
		   data = data;
	   }
   }

   [StructLayout(LayoutKind.Sequential)]
   public class EcoreEventSignalRealtime
   {
	   public IntPtr ext_data;
	   public IntPtr data; // do this later, its a siginfo_t

	   public EcoreEventSignalRealtime()
	   {}

	   public EcoreEventSignalRealtime(IntPtr EventInfo)
	   {
		   EcoreEventSignalRealtime e = new EcoreEventSignalRealtime();
		   e = (EcoreEventSignalRealtime)Marshal.PtrToStructure(EventInfo, typeof(EcoreEventSignalRealtime));
		   ext_data = e.ext_data;
		   data = data;
	   }
   }

   [StructLayout(LayoutKind.Sequential)]
   public class _EcoreEventDndEnter
   {
	   public IntPtr win;
	   public IntPtr src;
	   public IntPtr types;
	   public int num_types;

	   public _EcoreEventDndEnter()
	   {}
   }

   public class EcoreEventDndEnter
   {
	   public XWindow win;
	   public XWindow src;
	   public string[] types;
	   public int num_types;

	   public EcoreEventDndEnter()
	   {}

	   public EcoreEventDndEnter(IntPtr EventInfo)
	   {
		   _EcoreEventDndEnter e = new _EcoreEventDndEnter();
		   e = (_EcoreEventDndEnter)Marshal.PtrToStructure(EventInfo, typeof(_EcoreEventDndEnter));
		   win = new XWindow(e.win);
		   src = new XWindow(e.src);
		   types = Common.PtrToStringArray(e.num_types, e.types);
		   num_types = e.num_types;
	   }
   }

   [StructLayout(LayoutKind.Sequential)]
   public class EcoreEventDndPosition
   {
	   public IntPtr win;
	   public IntPtr src;
	   public uint action;

	   public EcoreEventDndPosition()
	   {}

	   public EcoreEventDndPosition(IntPtr EventInfo)
	   {
		   EcoreEventDndPosition e = new EcoreEventDndPosition();
		   e = (EcoreEventDndPosition)Marshal.PtrToStructure(EventInfo, typeof(EcoreEventDndPosition));
		   win = e.win;
		   src = e.src;
		   action = e.action;
	   }
   }

   [StructLayout(LayoutKind.Sequential)]
   public class EcoreEventDndStatus
   {
	   public IntPtr target;
	   public int will_accept;
	   public Ecore.X.Rectangle rectangle;
	   public uint action;

	   public EcoreEventDndStatus()
	   {}

	   public EcoreEventDndStatus(IntPtr EventInfo)
	   {
		   EcoreEventDndStatus e = new EcoreEventDndStatus();
		   e = (EcoreEventDndStatus)Marshal.PtrToStructure(EventInfo, typeof(EcoreEventDndStatus));
		   target = e.target;
		   will_accept = e.will_accept;
		   rectangle.X = e.rectangle.X;
		   rectangle.Y = e.rectangle.Y;
		   rectangle.Width = e.rectangle.Width;
		   rectangle.Height = e.rectangle.Height;
		   action = e.action;
	   }
   }

   [StructLayout(LayoutKind.Sequential)]
   public class EcoreEventDndLeave
   {
	   public IntPtr win;
	   public IntPtr source;

	   public EcoreEventDndLeave()
	   {}

	   public EcoreEventDndLeave(IntPtr EventInfo)
	   {
		   EcoreEventDndLeave e = new EcoreEventDndLeave();
		   e = (EcoreEventDndLeave)Marshal.PtrToStructure(EventInfo, typeof(EcoreEventDndLeave));
		   win = e.win;
		   source = e.source;
	   }
   }

   public class Position
   {
	   public int X;
	   public int Y;

	   public Position(int x, int y)
	   {
		   X = x;
		   Y = y;
	   }
   }

   [StructLayout(LayoutKind.Sequential)]
   public class EcoreEventDndDrop
   {
	   public IntPtr win;
	   public IntPtr source;
	   public uint action;
	   public Position position;

	   public EcoreEventDndDrop()
	   {}

	   public EcoreEventDndDrop(IntPtr EventInfo)
	   {
		   EcoreEventDndDrop e = new EcoreEventDndDrop();
		   e = (EcoreEventDndDrop)Marshal.PtrToStructure(EventInfo, typeof(EcoreEventDndDrop));
		   win = e.win;
		   source = e.source;
		   action = e.action;
		   position = new Position(e.position.X, e.position.Y);
	   }
   }

   [StructLayout(LayoutKind.Sequential)]
   public class EcoreEventDndFinished
   {
	   public IntPtr win;
	   public IntPtr target;
	   public int completed;
	   public uint action;

	   public EcoreEventDndFinished()
	   {}

	   public EcoreEventDndFinished(IntPtr EventInfo)
	   {
		   EcoreEventDndFinished e = new EcoreEventDndFinished();
		   e = (EcoreEventDndFinished)Marshal.PtrToStructure(EventInfo, typeof(EcoreEventDndFinished));
		   win = e.win;
		   target = e.target;
		   completed = e.completed;
		   action = e.action;
	   }
   }

   public enum Selection
   {
	   ECORE_X_SELECTION_PRIMARY,
	     ECORE_X_SELECTION_SECONDARY,
	     ECORE_X_SELECTION_XDND,
	     ECORE_X_SELECTION_CLIPBOARD
   };

   public enum Content {
	   ECORE_X_SELECTION_CONTENT_NONE,
	     ECORE_X_SELECTION_CONTENT_TEXT,
	     ECORE_X_SELECTION_CONTENT_FILES,
	     ECORE_X_SELECTION_CONTENT_TARGETS,
	     ECORE_X_SELECTION_CONTENT_CUSTOM
   };

   [StructLayout(LayoutKind.Sequential)]
   public class EcoreEventSelectionNotify
   {
	   public IntPtr win;
	   public uint time;
	   public Selection selection;
	   public string target;
	   public IntPtr data;
	   public Content content;

	   public EcoreEventSelectionNotify()
	   {}

	   public EcoreEventSelectionNotify(IntPtr EventInfo)
	   {
		   EcoreEventSelectionNotify e = new EcoreEventSelectionNotify();
		   e = (EcoreEventSelectionNotify)Marshal.PtrToStructure(EventInfo, typeof(EcoreEventSelectionNotify));
		   win = e.win;
		   time = e.time;
		   selection = e.selection;
		   target = e.target;
		   data = e.data;
		   content = e.content;
	   }
   }

   public delegate int EcoreEventSelectionDataFree (IntPtr data);
   [StructLayout(LayoutKind.Sequential)]
   public class EcoreEventSelectionData
   {
	   public string data;
	   public int length;
	   public EcoreEventSelectionDataFree cb;

	   public EcoreEventSelectionData()
	   {
	   }
   }

   [StructLayout(LayoutKind.Sequential)]
   public class _EcoreEventSelectionDataFiles
   {
	   public EcoreEventSelectionData data;
	   public IntPtr files;
	   public int num_files;

	   public _EcoreEventSelectionDataFiles()
	   {}
   }

   public class EcoreEventSelectionDataFiles
   {
	   public EcoreEventSelectionData data;
	   public string[] files;
	   public int num_files;

	   public EcoreEventSelectionDataFiles()
	   {
		   data = new EcoreEventSelectionData();
	   }

	   public EcoreEventSelectionDataFiles(IntPtr EventInfo)
	   {
		   _EcoreEventSelectionDataFiles e = new _EcoreEventSelectionDataFiles();
		   e = (_EcoreEventSelectionDataFiles)Marshal.PtrToStructure(EventInfo, typeof(_EcoreEventSelectionDataFiles));
		   files = Common.PtrToStringArray(e.num_files, e.files);
		   num_files = e.num_files;
	   }
   }

   [StructLayout(LayoutKind.Sequential)]
   public class EcoreEventMouseButtonRoot
   {
	   public int x;
	   public int y;

	   public EcoreEventMouseButtonRoot()
	   {}

	   public EcoreEventMouseButtonRoot(int _x, int _y)
	   {
		   x = _x;
		   y = _y;
	   }
   }

   [StructLayout(LayoutKind.Sequential)]
   public class EcoreEventMouseButtonDown
   {
	   public int button;
	   public int modifiers;
	   public int x, y;
	   public EcoreEventMouseButtonRoot root;
	   public IntPtr win;
	   public IntPtr event_win;
	   public int time;
	   public int double_click;
	   public int triple_click;

	   public EcoreEventMouseButtonDown()
	   {
		   root = new EcoreEventMouseButtonRoot();
	   }

	   public EcoreEventMouseButtonDown(IntPtr EventInfo)
	   {
		   EcoreEventMouseButtonDown e = new EcoreEventMouseButtonDown();
		   e = (EcoreEventMouseButtonDown)Marshal.PtrToStructure(EventInfo, typeof(EcoreEventMouseButtonDown));

		   button = e.button;
		   modifiers = e.modifiers;
		   x = e.x;
		   y = e.y;
		   root = new EcoreEventMouseButtonRoot(e.root.x, e.root.y);
		   win = e.win;
		   event_win = e.event_win;
		   double_click = e.double_click;
		   triple_click = e.triple_click;
	   }
   }

   [StructLayout(LayoutKind.Sequential)]
   public class EcoreEventMouseButtonUp
   {
	   public int button;
	   public int modifiers;
	   public int x, y;
	   public EcoreEventMouseButtonRoot root;
	   public IntPtr win;
	   public IntPtr event_win;
	   public int time;

	   public EcoreEventMouseButtonUp()
	   {
		   root = new EcoreEventMouseButtonRoot();
	   }

	   public EcoreEventMouseButtonUp(IntPtr EventInfo)
	   {
		   EcoreEventMouseButtonUp e = new EcoreEventMouseButtonUp();
		   e = (EcoreEventMouseButtonUp)Marshal.PtrToStructure(EventInfo, typeof(EcoreEventMouseButtonUp));

		   button = e.button;
		   modifiers = e.modifiers;
		   x = e.x;
		   y = e.y;
		   root = new EcoreEventMouseButtonRoot(e.root.x, e.root.y);
		   win = e.win;
		   event_win = e.event_win;
		   time = e.time;
	   }
   }

   [StructLayout(LayoutKind.Sequential)]
   public class EcoreEventMouseMove
   {
	   public int modifiers;
	   public int x, y;
	   public EcoreEventMouseButtonRoot root;
	   public IntPtr win;
	   public IntPtr event_win;
	   public int time;

	   public EcoreEventMouseMove()
	   {
		   root = new EcoreEventMouseButtonRoot();
	   }

	   public EcoreEventMouseMove(IntPtr EventInfo)
	   {
		   EcoreEventMouseMove e = new EcoreEventMouseMove();
		   e = (EcoreEventMouseMove)Marshal.PtrToStructure(EventInfo, typeof(EcoreEventMouseMove));

		   modifiers = e.modifiers;
		   x = e.x;
		   y = e.y;
		   root = new EcoreEventMouseButtonRoot(e.root.x, e.root.y);
		   win = e.win;
		   event_win = e.event_win;
		   time = e.time;
	   }
   }

   public enum EcoreXEventMode {
	   Normal,
	     WhileGrabbed,
	     Grab,
	     UnGrab
   }

   public enum EcoreXEventDetail {
	   Ancestor,
	     Virtual,
	     Inferior,
	     NonLinear,
	     NonLinearVirtual,
	     Pointer,
	     PointerRoot,
	     DetailNone
   }

   [StructLayout(LayoutKind.Sequential)]
   public class EcoreEventMouseIn
   {
	   public int modifiers;
	   public int x, y;
	   public EcoreEventMouseButtonRoot root;
	   public IntPtr win;
	   public IntPtr event_win;
	   public EcoreXEventMode mode;
	   public EcoreXEventDetail detail;
	   public int time;

	   public EcoreEventMouseIn()
	   {
		   root = new EcoreEventMouseButtonRoot();
	   }

	   public EcoreEventMouseIn(IntPtr EventInfo)
	   {
		   EcoreEventMouseIn e = new EcoreEventMouseIn();
		   e = (EcoreEventMouseIn)Marshal.PtrToStructure(EventInfo, typeof(EcoreEventMouseIn));

		   modifiers = e.modifiers;
		   x = e.x;
		   y = e.y;
		   root = new EcoreEventMouseButtonRoot(e.root.x, e.root.y);
		   win = e.win;
		   event_win = e.event_win;
		   mode = e.mode;
		   detail = e.detail;
		   time = e.time;

	   }
   }

  /* ***************** */
  /* Main Events Class */
  /* ***************** */

   public class Events
   {
	   const string Library = "ecore";
	   const string LibraryEcoreX = "ecore_x";
	   const string LibraryGlue = "libeflsharpglue";

	   public static Hashtable event_types = new Hashtable();

    /* PUBLIC METHODS */

	   public const int EventNone = 0;
	   public const int EventExeExit = 1;
	   public const int EventSignalUser = 2;
	   public const int EventSignalHup = 3;
	   public const int EventSignalExit = 4;
	   public const int EventSignalPower = 5;
	   public const int EventSignalRealtime = 6;
	   public const int EventCount = 7;

	   public static Hashtable events = new Hashtable();

	   public delegate int PrivEventHandler(IntPtr data, int type, IntPtr ev);
	   public delegate int EventHandler(object event_info);

	   protected class EventController
	   {
		   const string Library = "ecore";
		   [DllImport(Library)]
		   private extern static IntPtr ecore_event_handler_add(int type, PrivEventHandler func, IntPtr data);

		   [DllImport(Library)]
		   private extern static IntPtr ecore_event_handler_del(IntPtr event_handler);

		   public event EventHandler InternalHandler;

		   PrivEventHandler private_callback;
		   IntPtr private_callback_ptr;
		   int evnt_num;
		   Type t;

		   public EventController(int Event)
		   {

			   evnt_num = Event;
			   private_callback = new PrivEventHandler(EventCallback);
			   private_callback_ptr = ecore_event_handler_add(evnt_num, private_callback, new IntPtr());
		   }

		   public void Finalize()
		   {

		   }

		   public bool Remove(EventHandler cb)
		   {
			   InternalHandler -= cb;
			   if(InternalHandler == null)
			   {
				   ecore_event_handler_del(private_callback_ptr);
				   return true;
			   }
			   return false;
		   }

		   public int EventCallback(IntPtr data, int type, IntPtr event_info)
		   {
			   
			   Type t = (Type)event_types[evnt_num];

			   object e = (t == null ? null : (object)Activator.CreateInstance (t,
											    BindingFlags.Public | BindingFlags.Instance, null, new
											    object[] {event_info}, Thread.CurrentThread.CurrentCulture));
			   InternalHandler(e);
			   
			   /*
			   if (evnt_num == EventXMouseButtonUp) {
				   EcoreEventMouseButtonUp e = new EcoreEventMouseButtonUp (event_info);
				   InternalHandler((object)e);
			   }
			   else if (evnt_num ==  EventXMouseButtonDown) {
				   EcoreEventMouseButtonDown e1 = new EcoreEventMouseButtonDown (event_info);
				   InternalHandler((object)e1);
			   }
			   else if (evnt_num == EventXMouseMove) {
				   EcoreEventMouseMove e2 = new EcoreEventMouseMove (event_info);
				   //System.Console.WriteLine ("moving.....");
				   InternalHandler((object)e2);
			   }
			   else {
				   System.Console.WriteLine ("trash");
				   //   object e3 = (t == null ? null : (object)Activator.CreateInstance (t,
				   //								     BindingFlags.Public | BindingFlags.Instance, null, new
				   //								     object[] {event_info}, Thread.CurrentThread.CurrentCulture));
				   //   InternalHandler((object)e3);
			   }
			   */
			   return 1;
		   }
	   }

	   private static void AddEventHandler(int Event, EventHandler value)
	   {
		   EventController ec = (EventController)events[Event];
		   if(ec == null) events[Event] = ec = new EventController(Event);
		   ec.InternalHandler += value;
	   }

	   private static void RemoveEventHandler(int Event, EventHandler value)
	   {
		   if(!events.ContainsKey(Event)) return;
		   EventController ec = (EventController)events[Event];
		   if(ec.Remove(value))
		     events.Remove(Event);
	   }

	   /* Mouse Button Down Event */
	   [DllImport(LibraryGlue)]
	   private extern static int _ecore_x_mouse_button_down();
	   private static int _EventXMouseButtonDown = 0;
	   private static int EventXMouseButtonDown
	   {
		   get {
			   if(_EventXMouseButtonDown == 0)
			   {
				   _EventXMouseButtonDown = _ecore_x_mouse_button_down();
				   event_types[_EventXMouseButtonDown] = typeof(EcoreEventMouseButtonDown);
			   }
			   return _EventXMouseButtonDown;
		   }
	   }
	   public static event EventHandler MouseButtonDown
	   {
		   add { AddEventHandler(EventXMouseButtonDown, value); }
		   remove { RemoveEventHandler(EventXMouseButtonDown, value); }
	   }

	   /* Mouse Button Up Event */
	   [DllImport(LibraryGlue)]
	   private extern static int _ecore_x_mouse_button_up();
	   private static int _EventXMouseButtonUp = 0;
	   private static int EventXMouseButtonUp
	   {
		   get {
			   if(_EventXMouseButtonUp == 0)
			   {
				   _EventXMouseButtonUp = _ecore_x_mouse_button_up();
				   event_types[_EventXMouseButtonUp] = typeof(EcoreEventMouseButtonUp);
			   }
			   return _EventXMouseButtonUp;
		   }
	   }
	   public static event EventHandler MouseButtonUp
	   {
		   add { AddEventHandler(EventXMouseButtonUp, value); }
		   remove { RemoveEventHandler(EventXMouseButtonUp, value); }
	   }

	   /* Mouse Move Event */
	   [DllImport(LibraryGlue)]
	   private extern static int _ecore_x_mouse_move();
	   private static int _EventXMouseMove = 0;
	   private static int EventXMouseMove
	   {
		   get {
			   if(_EventXMouseMove == 0)
			   {
				   _EventXMouseMove = _ecore_x_mouse_move();
				   event_types[_EventXMouseMove] = typeof(EcoreEventMouseMove);
			   }
			   return _EventXMouseMove;
		   }
	   }
	   public static event EventHandler MouseMove
	   {
		   add { AddEventHandler(EventXMouseMove, value); }
		   remove { RemoveEventHandler(EventXMouseMove, value); }
	   }

	   /* Mouse In Event */
	   [DllImport(LibraryGlue)]
	   private extern static int _ecore_x_mouse_in();
	   private static int _EventXMouseIn = 0;
	   private static int EventXMouseIn
	   {
		   get {
			   if(_EventXMouseIn == 0)
			   {
				   _EventXMouseIn = _ecore_x_mouse_in();
				   event_types[_EventXMouseIn] = typeof(EcoreEventMouseIn);
			   }
			   return _EventXMouseIn;
		   }
	   }
	   public static event EventHandler MouseIn
	   {
		   add { AddEventHandler(EventXMouseIn, value); }
		   remove { RemoveEventHandler(EventXMouseIn, value); }
	   }

	   /* Dnd Enter Event */
	   [DllImport(LibraryGlue)]
	   private extern static int _ecore_x_dnd_enter();
	   private static int _EventXDndEnter = 0;
	   private static int EventXDndEnter
	   {
		   get {
			   if(_EventXDndEnter == 0)
			   {
				   _EventXDndEnter = _ecore_x_dnd_enter();
				   event_types[_EventXDndEnter] = typeof(EcoreEventDndEnter);
			   }
			   return _EventXDndEnter;
		   }
	   }
	   public static event EventHandler DndEnterEvent
	   {
		   add { AddEventHandler(EventXDndEnter, value); }
		   remove { RemoveEventHandler(EventXDndEnter, value); }

	   }

    /* Dnd Position Event */
	   [DllImport(LibraryGlue)]
	   private extern static int _ecore_x_dnd_position();
	   private static int _EventXDndPosition = 0;
	   private static int EventXDndPosition
	   {
		   get {
			   if(_EventXDndPosition == 0)
			   {
				   _EventXDndPosition = _ecore_x_dnd_position();
				   event_types[_EventXDndPosition] = typeof(EcoreEventDndPosition);
			   }
			   return _EventXDndPosition;
		   }
	   }
	   public static event EventHandler DndPositionEvent
	   {
		   add { AddEventHandler(EventXDndPosition, value); }
		   remove { RemoveEventHandler(EventXDndPosition, value); }

	   }

    /* Dnd Status Event */
	   [DllImport(LibraryGlue)]
	   private extern static int _ecore_x_dnd_status();
	   private static int _EventXDndStatus = 0;
	   private static int EventXDndStatus
	   {
		   get {
			   if(_EventXDndStatus == 0)
			   {
				   _EventXDndStatus = _ecore_x_dnd_status();
				   event_types[_EventXDndStatus] = typeof(EcoreEventDndStatus);
			   }
			   return _EventXDndStatus;
		   }
	   }
	   public static event EventHandler DndStatusEvent
	   {
		   add { AddEventHandler(EventXDndStatus, value); }
		   remove { RemoveEventHandler(EventXDndStatus, value); }
	   }

    /* Dnd Leave Event */
	   [DllImport(LibraryGlue)]
	   private extern static int _ecore_x_dnd_leave();
	   private static int _EventXDndLeave = 0;
	   private static int EventXDndLeave
	   {
		   get {
			   if(_EventXDndLeave == 0)
			   {
				   _EventXDndLeave = _ecore_x_dnd_leave();
				   event_types[_EventXDndLeave] = typeof(EcoreEventDndLeave);
			   }
			   return _EventXDndLeave;
		   }
	   }
	   public static event EventHandler DndLeaveEvent
	   {
		   add { AddEventHandler(EventXDndLeave, value); }
		   remove { RemoveEventHandler(EventXDndLeave, value); }
	   }

    /* Dnd Drop Event */
	   [DllImport(LibraryGlue)]
	   private extern static int _ecore_x_dnd_drop();
	   private static int _EventXDndDrop = 0;
	   private static int EventXDndDrop
	   {
		   get {
			   if(_EventXDndDrop == 0)
			   {
				   _EventXDndDrop = _ecore_x_dnd_drop();
				   event_types[_EventXDndDrop] = typeof(EcoreEventDndDrop);
			   }
			   return _EventXDndDrop;
		   }
	   }
	   public static event EventHandler DndDropEvent
	   {
		   add { AddEventHandler(EventXDndDrop, value); }
		   remove { RemoveEventHandler(EventXDndDrop, value); }
	   }

    /* Dnd Finished Event */
	   [DllImport(LibraryGlue)]
	   private extern static int _ecore_x_dnd_finished();
	   private static int _EventXDndFinished = 0;
	   private static int EventXDndFinished
	   {
		   get {
			   if(_EventXDndFinished == 0)
			   {
				   _EventXDndFinished = _ecore_x_dnd_finished();
				   event_types[_EventXDndFinished] = typeof(EcoreEventDndFinished);
			   }
			   return _EventXDndFinished;
		   }
	   }
	   public static event EventHandler DndFinishedEvent
	   {
		   add { AddEventHandler(EventXDndFinished, value); }
		   remove { RemoveEventHandler(EventXDndFinished, value); }
	   }

    /* Selection Notify Event */
	   [DllImport(LibraryGlue)]
	   private extern static int _ecore_x_selection_notify();
	   private static int _EventXSelectionNotify = 0;
	   private static int EventXSelectionNotify
	   {
		   get {
			   if(_EventXSelectionNotify == 0)
			   {
				   _EventXSelectionNotify = _ecore_x_selection_notify();
				   event_types[_EventXSelectionNotify] = typeof(EcoreEventSelectionNotify);
			   }
			   return _EventXSelectionNotify;
		   }
	   }
	   public static event EventHandler SelectionNotifyEvent
	   {
		   add { AddEventHandler(EventXSelectionNotify, value); }
		   remove { RemoveEventHandler(EventXSelectionNotify, value); }
	   }

    /* Built in ecore signal events follow */

	   public static event EventHandler NoneEvent
	   {
		   add { AddEventHandler(EventNone, value); }
		   remove { RemoveEventHandler(EventNone, value); }
	   }

	   public static event EventHandler ExeExitEvent
	   {
		   add { AddEventHandler(EventExeExit, value); }
		   remove { RemoveEventHandler(EventExeExit, value); }
	   }

	   public static event EventHandler SignalUserEvent
	   {
		   add { AddEventHandler(EventSignalUser, value); }
		   remove { RemoveEventHandler(EventSignalUser, value); }
	   }

	   public static event EventHandler SignalHupEvent
	   {
		   add { AddEventHandler(EventSignalHup, value); }
		   remove { RemoveEventHandler(EventSignalHup, value); }
	   }

	   public static event EventHandler SignalExitEvent
	   {
		   add { AddEventHandler(EventSignalExit, value); }
		   remove { RemoveEventHandler(EventSignalExit, value); }
	   }

	   public static event EventHandler SignapPowerEvent
	   {
		   add { AddEventHandler(EventSignalPower, value); }
		   remove { RemoveEventHandler(EventSignalPower, value); }
	   }

	   public static event EventHandler SignalRealtimeEvent
	   {
		   add { AddEventHandler(EventSignalRealtime, value); }
		   remove { RemoveEventHandler(EventSignalRealtime, value); }
	   }

	   [DllImport(Library)]
	   private extern static IntPtr ecore_event_filter_add(IntPtr func_start, IntPtr func_filter, IntPtr func_end, IntPtr data);

	   [DllImport(Library)]
	   private extern static IntPtr ecore_event_filter_del(IntPtr ef);

	   [DllImport(Library)]
	   private extern static int ecore_event_current_type_get();

	   [DllImport(Library)]
	   private extern static IntPtr ecore_event_current_event_get();

	   /* this needs fixing / porting */
	   /*
	    public class UserEvent
	    {
	    const string Library = "ecore";

	    public int event_id;
	    public object data;
	    * 
	    [DllImport(Library)]
	    private extern static IntPtr ecore_event_add(int type, IntPtr ev, EventFunction func, IntPtr data);
	    * 
	    [DllImport(Library)]
      private extern static IntPtr ecore_event_del(IntPtr events);

      [DllImport(Library)]
      private extern static IntPtr ecore_event_handler_add(int type, EcorePrivateEventHandler func, IntPtr data);

      [DllImport(Library)]
      private extern static int ecore_event_type_new();

      public UserEvent()
      {
      event_id = ecore_event_type_new();
      }

     private void user_event_free_func(IntPtr data, IntPtr ev)
     {
     System.Console.WriteLine("Calling free function!");
     }

      public void Add()
      {
      EventFunction tmp_handler = new EventFunction(user_event_free_func);
      ecore_event_add(event_id, new IntPtr(), tmp_handler, new IntPtr());
      System.GC.KeepAlive(tmp_handler);
      }

      // event handling code for custom user event

      // User event handler type
      public delegate int EcoreEventUserEventHandler(UserEvent EventInfo);

      // user's event handler is saved here
      EcoreEventUserEventHandler user_event_user_handler;

      private int user_event_private_handler(IntPtr data, int type, IntPtr EventInfo)
      {
      System.Console.WriteLine("calling private callback for " + type);
      return user_event_user_handler(this);
      }

     public event EcoreEventUserEventHandler Handler {
     add
      {
      EcorePrivateEventHandler tmp_handler = new EcorePrivateEventHandler(user_event_private_handler);
      user_event_user_handler = value;
      ecore_event_handler_add(event_id, tmp_handler, new IntPtr());
      System.GC.KeepAlive(tmp_handler);
      }

      remove
      {

      }
      }

      }
    */
   }

}
