namespace Enlightenment.Evas
{
   
   using System;
   using System.Collections;
   using System.Runtime.InteropServices;
   using System.Reflection;
   using System.Threading;
   
 public class Gradient : Item
     {
	const string Library = "evas";
	
	[DllImport(Library)]
	private extern static IntPtr evas_object_gradient_add(IntPtr e);

	public Gradient()
	  {}
	
      public Gradient(Canvas c) : base(c)
	  {
	     objRaw = new HandleRef(this, evas_object_gradient_add(c.Raw));
	  }
	
	[DllImport(Library)]
	private extern static void evas_object_gradient_color_add(IntPtr obj, int r, int g, int b, int a, int distance);
	
	public void ColorAdd(int r, int g, int b, int a, int distance)
	  {
	     evas_object_gradient_color_add(Raw, r, g, b, a, distance);
	  }
	
	[DllImport(Library)]
	private extern static void evas_object_gradient_colors_clear(IntPtr obj);
	
	public void ColorsClear()
	  {
	     evas_object_gradient_colors_clear(Raw);
	  }
	
	[DllImport(Library)]
	private extern static void evas_object_gradient_angle_set(IntPtr obj, int angle);
	
	[DllImport(Library)]
	private extern static int evas_object_gradient_angle_get(IntPtr obj);
	
	public int Angle
	  {
	     get { return evas_object_gradient_angle_get(Raw); }
	     set { evas_object_gradient_angle_set(Raw, value); }
	  }
     }   
}
