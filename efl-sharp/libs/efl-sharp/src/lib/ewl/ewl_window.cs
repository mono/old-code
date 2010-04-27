namespace Enlightenment.Ewl 
{

    using System;
    using System.Runtime.InteropServices;
    using Ewl.Event;
    
    public class Window : Embed 
    {
	
	public event EwlEventHandler DeleteEvent 
	{	     
	    add { events[6] = new EventData(this, value, 6); }	     
	    remove { events[6] = null; }
	}	
	
	[DllImport(Library)]
	    private static extern void ewl_widget_destroy(IntPtr wid);
	
	[DllImport(Library)]
	    private static extern IntPtr ewl_window_new();
	
	public Window() 
	{       
	    objRaw = new HandleRef(this, ewl_window_new());	     
	}
	
	[DllImport(Library)]
	    private static extern void ewl_window_title_set(IntPtr win, string title);
	
	public string Title 
	{       
	    set { ewl_window_title_set(Raw, value); }	     
	}
	
	[DllImport(Library)]
	    static extern void ewl_window_class_set(IntPtr win, string cls);
	
	public string Class 
	{		
	    set { ewl_window_class_set(Raw, value); }	     
	}
	
	[DllImport(Library)]
	    private static extern void ewl_window_name_set(IntPtr win, string name);
	[DllImport(Library)]
	    private static extern string ewl_window_name_get(IntPtr win);
	
	public string Name 
	{		
	    get { return ewl_window_name_get(Raw); }			
	    set { ewl_window_name_set(Raw, value); }		
	}	
    }	
}
