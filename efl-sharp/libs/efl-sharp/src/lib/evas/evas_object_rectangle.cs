namespace Enlightenment.Evas
{
   
   using System;
   using System.Collections;
   using System.Runtime.InteropServices;
   using System.Reflection;
   using System.Threading;
   
 public class Rectangle : Item
     {
	const string Library = "evas";
	
	[DllImport(Library)]
	private extern static IntPtr evas_object_rectangle_add(IntPtr e);
	
	public Rectangle()
	  {}
	        
      public Rectangle(Canvas c) : base(c)
	  {
	     objRaw = new HandleRef(this, evas_object_rectangle_add(c.Raw));
	  }
	
	
	public Rectangle(Item o)
	  {
	     objRaw = new HandleRef(this, o.Raw);
	  }
     }
}
