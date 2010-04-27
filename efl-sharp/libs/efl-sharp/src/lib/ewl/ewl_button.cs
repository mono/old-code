/*
	EWL Button Class
	
	TODO: Label Alignment
*/

namespace Enlightenment.Ewl 
{

	using System;
	using System.Runtime.InteropServices;
	using Enlightenment.Ewl.Event;

	public class Button : Widget
	{		
		[DllImport(Library)]
	    private static extern IntPtr ewl_button_new(string text);
	    
	    [DllImport(Library)]
	    private static extern string ewl_button_label_get(IntPtr button);
	    
	    [DllImport(Library)]
	    private static extern void ewl_button_label_set(IntPtr button, string text);
	    
	    public Button(string text){
	    	objRaw = new HandleRef(this, ewl_button_new(text));
	    }
	    
	    public event EwlEventHandler ClickedEvent
		{	     
		   add { events[EventType.CLICKED] = new EventData(this, value, EventType.CLICKED); }
			remove { events[EventType.CLICKED] = null; }
		}
	    
	    public string Label
	    {
	    	get{ return ewl_button_label_get(Raw);}
	        set{ ewl_button_label_set(Raw, value);}
	    }	    
	}
}
