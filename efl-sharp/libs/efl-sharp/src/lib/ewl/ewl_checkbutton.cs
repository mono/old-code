/*
	EWL CheckButton Class
	
	TODO: Label Alignment
*/

namespace Enlightenment.Ewl 
{

	using System;
    using System.Runtime.InteropServices;
    using Ewl.Event;
    
    public class CheckButton : Widget 
    {
	
		
		public event EwlEventHandler ValueChanged 
		{	     
		    add { events[EventType.VALUE_CHANGED] = new EventData(this, value, EventType.VALUE_CHANGED); }	     
		    remove { events[EventType.VALUE_CHANGED] = null; }
		}		
	
		[DllImport(Library)]
	    private static extern IntPtr ewl_checkbutton_new(string label);
	
		public CheckButton(string label) 
		{       
		    objRaw = new HandleRef(this, ewl_checkbutton_new(label));
		}				
    }	
}
