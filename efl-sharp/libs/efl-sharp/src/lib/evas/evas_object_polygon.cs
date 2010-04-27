namespace Enlightenment.Evas
{
   
   using System;
   using System.Collections;
   using System.Runtime.InteropServices;
   using System.Reflection;
   using System.Threading;
   
 public class Polygon : Item
     {
	const string Library = "evas";
	
	[DllImport(Library)]
	private extern static IntPtr evas_object_polygon_add(IntPtr e);
	
	public Polygon()
	  {}
	
      public Polygon(Canvas c) : base(c)
	  {
	     objRaw = new HandleRef(this, evas_object_polygon_add(c.Raw));
	  }
   
	[DllImport(Library)]
	private extern static void evas_object_polygon_point_add(IntPtr obj, int x, int y);
	
	public void PointAdd(int x, int y)
	  {
	     evas_object_polygon_point_add(Raw, x, y);
	  }
	
	[DllImport(Library)]
	private extern static void evas_object_polygon_points_clear(IntPtr obj);
	
	public void PointsClear()
	  {
	     evas_object_polygon_points_clear(Raw);
	  }
     }
}
