namespace Enlightenment.Ewl {
   
   using System;
   using System.Runtime.InteropServices;
   using Enlightenment.Ewl.Event;
   
   public class Entry : Widget 
     {    
	
	public event EwlEventHandler ChangedEvent 
	  {	     
	     add { events[(int)EventType.VALUE_CHANGED] = new EventData(this, value, (int)EventType.VALUE_CHANGED); }			
	     remove { events[(int)EventType.VALUE_CHANGED] = null; }		
	  }
		
	[DllImport(Library)]
	static extern IntPtr ewl_entry_new(string text);
	
	public Entry() 
	  {
	     objRaw = new HandleRef(this, ewl_entry_new(string.Empty));
	  }
		
	public Entry(string text) 
	  {
	     objRaw = new HandleRef(this, ewl_entry_new(text));
	  }
		
	[DllImport(Library)]
	  static extern void ewl_entry_text_set(IntPtr ent, string text);
	[DllImport(Library)]
	  static extern string ewl_entry_text_get(IntPtr ent);
	
	public string Text
	  {
	     set { //strText = value;
		ewl_entry_text_set(Raw, value);
	     }
			
	     get{ return ewl_entry_text_get(Raw); }
	  }	
     }   
}
