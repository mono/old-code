namespace Enlightenment.Ewl 
{
   using System;
   using System.Runtime.InteropServices;
   using Enlightenment.Evas;
   
 public class Embed : Overlay 
     {    
	[DllImport(Library)]
	private static extern IntPtr ewl_embed_evas_set(IntPtr em, IntPtr ev, IntPtr win);
	public Evas.Item Evas(Canvas ev, Window win) 
	  {
	     IntPtr ptr = ewl_embed_evas_set(Raw, ev.Raw, win.Raw);
	     return new Evas.Item(ptr);       
	  }    
     }   
}
