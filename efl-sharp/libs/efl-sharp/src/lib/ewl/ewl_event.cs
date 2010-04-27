/*
	EWL Event handling classes
	- Please don't change the identation
	  it looks really messy ;)
*/

namespace Enlightenment.Ewl {   
	using System;
	using System.Runtime.InteropServices;
	
	public delegate void PrivEventHandler(IntPtr w, IntPtr evnt, IntPtr data);
	public delegate void EwlEventHandler(Widget w, object event_data);
   
	namespace Event
	{

		public class EventData
		{
	   		const string Library = "ewl";
	   		[DllImport(Library)]
	     	static extern int ewl_callback_append(IntPtr wid, int i, PrivEventHandler de, IntPtr data);
	   
	   		[DllImport(Library)]
	     	static extern void ewl_callback_del_with_data(IntPtr wid, int i, PrivEventHandler de, IntPtr data);
	   
	   		Widget w;
	   		EwlEventHandler usercallback;
	   		PrivEventHandler mycallback;
	   		int evnt_num;
	   		
	   		/*  TODO: Do we really need this empty constructor?  */
	   		public EventData()
	     	{
				//evnt_num = EventType.MAX;
	     	}
	   
	   		public EventData(Widget wi, EwlEventHandler e, int eventnum)
	     	{
				w = wi;
				usercallback = e;
				evnt_num = eventnum;
				mycallback = new PrivEventHandler(EventCallback);
		
				ewl_callback_append(w.Raw, evnt_num, mycallback, new IntPtr());
	     	}
	   
	   		public void Finalize()
	     	{
				ewl_callback_del_with_data(w.Raw, evnt_num, mycallback, new IntPtr());
	     	}
	   
			/*  TODO: Remove should be deprecated */
	   		public void Remove()
	     	{
				//if (evnt_num < EventType.MAX)
				ewl_callback_del_with_data(w.Raw, evnt_num, mycallback, new IntPtr());
				//evnt_num = EventType.MAX;
	     	}
	   
	   		public void EventCallback(IntPtr wi, IntPtr evnt, IntPtr data)
	     	{
				usercallback(w, (object)data);
	     	}
		}
      
		[StructLayout(LayoutKind.Sequential)]
      	public class DataKeyDown
		{
	   		uint modifiers;
	   		string keyname;
		}
      
      	[StructLayout(LayoutKind.Sequential)]
      	public class DataKeyUp
		{
	   		uint modifiers;
	   		string keyname;
		}
      
      	[StructLayout(LayoutKind.Sequential)]
      	public class DataWindowExpose
		{
	   		int x;
	   		int y;
	   		int w;
	   		int h;
		}
      
      	[StructLayout(LayoutKind.Sequential)]
      	public class DataWindowConfigure
		{
	   		int x;
	   		int y;
	   		int w;
	   		int h;
		}
      
      	[StructLayout(LayoutKind.Sequential)]
      	public class DataMouseDown
		{
	   		uint modifiers;
	   		int button;
	   		int clicks;
	   		int x;
	   		int y;
		}
      
      	[StructLayout(LayoutKind.Sequential)]
      	public class DataMouseUp
		{
	   		uint modifiers;
	   		int button;
	   		int x;
	   		int y;
		}
      
      	[StructLayout(LayoutKind.Sequential)]
      	public class DataMouseMove
		{
	   		uint modifiers;
	   		int x;
	   		int y;
		}
      
      	[StructLayout(LayoutKind.Sequential)]
      	public class DataMouseIn
		{
	   		uint modifiers;
	   		int x;
	   		int y;
		}
      
      	[StructLayout(LayoutKind.Sequential)]
      	public class DataMouseOut
		{
	   		uint modifiers;
	   		int x;
	   		int y;
		}
      
      	[StructLayout(LayoutKind.Sequential)]
      	public class DataMouseWheel
		{
	   		uint modifiers;
	   		int x;
	   		int y;
	   		int z;
	   		int dir;
		}
   	}
}
