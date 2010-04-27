/*
	EWL RadioButton Class
	
	TODO: -
*/

namespace Enlightenment.Ewl 
{

	using System;
    using System.Runtime.InteropServices;
    using Ewl.Event;
    
    public class RadioButton : Widget 
    {		
		public event EwlEventHandler ValueChanged 
		{	     
		    add { events[EventType.VALUE_CHANGED] = new EventData(this, value, EventType.VALUE_CHANGED); }	     
		    remove { events[EventType.VALUE_CHANGED] = null; }
		}		
			
		[DllImport(Library)]
	    private static extern IntPtr ewl_radiobutton_new(string label);
	
		public RadioButton(string label) 
		{       
		    objRaw = new HandleRef(this, ewl_radiobutton_new(label));
		}

		[DllImport(Library)]
	    private static extern void ewl_radiobutton_chain_set(IntPtr	w, IntPtr c);
		
		public void Chain(RadioButton r)
		{
			ewl_radiobutton_chain_set(Raw, r.Raw);
		}
    }	
}
