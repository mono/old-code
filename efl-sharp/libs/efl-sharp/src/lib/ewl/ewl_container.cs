namespace Enlightenment.Ewl {

   using System;
   using System.Runtime.InteropServices;
   
 public class Container : Widget 
     {	

	[DllImport(Library)]
	  static extern void ewl_container_child_append(IntPtr wid, IntPtr obj);
	
	public void Append(Widget wid) 
	  {	   
	     ewl_container_child_append(Raw, wid.Raw);	     
	  }	
     }   
}
