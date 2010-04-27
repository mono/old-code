namespace Enlightenment.Evas
{
   
   using System;
   using System.Collections;
   using System.Runtime.InteropServices;
   using System.Reflection;
   using System.Threading;
   
   public struct LineXY
     {
	public int X1;
	public int Y1;
	public int X2;
	public int Y2;
	
	public LineXY(int x1, int y1, int x2, int y2)
	  {
	     X1 = x1;
	     Y1 = y1;
	     X2 = x2;
	     Y2 = y2;
	  }
     }
   
 public class Line : Item
     {
	
	const string Library = "evas";
	
	[DllImport(Library)]
	private extern static IntPtr evas_object_line_add(IntPtr e);
	
	public Line()
	  {}
	
      public Line(Canvas c) : base(c)
	  {
	     objRaw = new HandleRef(this, evas_object_line_add(c.Raw));
	  }
	
	[DllImport(Library)]
	private extern static void evas_object_line_xy_set(IntPtr obj, int x1, int y1, int x2, int y2);
	
	[DllImport(Library)]
	private extern static void evas_object_line_xy_get(IntPtr obj, out int x1, out int y1, out int x2, out int y2);
	
	public LineXY XY
	  {
	     get {
		int x1, y1, x2, y2;
		evas_object_line_xy_get(Raw, out x1, out y1, out x2, out y2);
		return new LineXY(x1, y1, x2, y2);
	     }
	     set {
		evas_object_line_xy_set(Raw, value.X1, value.Y1, value.X2, value.Y2);
	     }
	  }
     }
}
