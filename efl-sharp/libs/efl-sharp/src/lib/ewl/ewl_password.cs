/*
	EWL Password Class
	
	TODO: -
*/

namespace Enlightenment.Ewl {
   
	using System;
	using System.Runtime.InteropServices;
	using Enlightenment.Ewl.Event;
  
   
   	public class Password : Widget 
    {    
	
		public event EwlEventHandler ChangedEvent 
	  	{	     
	     	add { events[(int)EventType.VALUE_CHANGED] = new EventData(this, value, (int)EventType.VALUE_CHANGED); }			
			remove { events[(int)EventType.VALUE_CHANGED] = null; }		
		}
		
		[DllImport(Library)]
		static extern IntPtr ewl_password_new(string text);
	
		public Password() 
	  	{
	    	objRaw = new HandleRef(this, ewl_password_new(string.Empty));
	  	}
		
		public Password(string text) 
	  	{
	    	objRaw = new HandleRef(this, ewl_password_new(text));
	  	}
		
		[DllImport(Library)]
	  	static extern void ewl_password_text_set(IntPtr ent, string text);
		[DllImport(Library)]
	  	static extern string ewl_password_text_get(IntPtr ent);
	
		public string Text
	  	{
	    	set { ewl_password_text_set(Raw, value);}
			
			get { return ewl_password_text_get(Raw);}
	  	}

		[DllImport(Library)]
	  	static extern void ewl_password_obscure_set(IntPtr ent, char obsc);
		[DllImport(Library)]
	  	static extern char ewl_password_obscure_get(IntPtr ent);
				
		public char Obscure
	  	{
	    	set { ewl_password_obscure_set(Raw, value);}
			
			get { return ewl_password_obscure_get(Raw);}
	  	}
	}   
}
