namespace Enlightenment.Evas
{
   
   using System;
   using System.Collections;
   using System.Runtime.InteropServices;
   using System.Reflection;
   using System.Threading;
   
   public struct TextFont
     {
	public string Font;
	public int Size;
	
	public TextFont(string font, int size)
	  {
	     Font = font;
	     Size = size;
	  }
     }
   
   public struct TextCharCoords
     {
	public int X;
	public int Y;
	public int W;
	public int H;
	
	public TextCharCoords(int x, int y, int w, int h)
	  {
	     X = x;
	     Y = y;
	     W = w;
	     H = h;
	  }
     }   
   
 public class Text : Item
     {
	const string Library = "evas";
	
	[DllImport(Library)]
	private extern static IntPtr evas_object_text_add(IntPtr e);
	
	
	
      public Text(Canvas c) : base(c)
	  {
	     objRaw = new HandleRef(this, evas_object_text_add(c.Raw));
	  }
	
	[DllImport(Library)]
	private extern static void evas_object_text_font_source_set(IntPtr obj, string font);
	
	[DllImport(Library)]
	private extern static string evas_object_text_font_source_get(IntPtr obj);
	
	public string FontSource
	  {
	     get { return evas_object_text_font_source_get(Raw); }
	     set { evas_object_text_font_source_set(Raw, value); }
	  }
	
	[DllImport(Library)]
	private extern static void evas_object_text_font_set(IntPtr obj, string font, int size);
	
	[DllImport(Library)]
	private extern static void evas_object_text_font_get(IntPtr obj, out string font, out int size);
	
	public TextFont Font 
	  {
	     get { 
		string font;
		int size;
		
		evas_object_text_font_get(Raw, out font, out size);
		return new TextFont(font, size);
	     }
	     
	     set { evas_object_text_font_set(Raw, value.Font, value.Size); }
	  }
	
	[DllImport(Library)]
	private extern static void evas_object_text_text_set(IntPtr obj, string text);
	
	[DllImport(Library)]
	private extern static string evas_object_text_text_get(IntPtr obj);
	
	public string Txt
	  {
	     get { return evas_object_text_text_get(Raw); }
	     set { evas_object_text_text_set(Raw, value); }
	  }
	
	[DllImport(Library)]
	private extern static int evas_object_text_ascent_get(IntPtr obj);
	
	public int Ascent
	  {
	     get { return evas_object_text_ascent_get(Raw); }
	  }
	
	[DllImport(Library)]
	private extern static int evas_object_text_descent_get(IntPtr obj);
	
	public int Descent
	  {
	     get { return evas_object_text_descent_get(Raw); }
	  }
	
	[DllImport(Library)]
	private extern static int evas_object_text_max_ascent_get(IntPtr obj);
	
	public int MaxAscent
	  {
	     get { return evas_object_text_max_ascent_get(Raw); }
	  }
	
	[DllImport(Library)]
	private extern static int evas_object_text_max_descent_get(IntPtr obj);
	
	public int MaxDescent
	  {
	     get { return evas_object_text_max_descent_get(Raw); }
	  }
	
	[DllImport(Library)]
	private extern static int evas_object_text_horiz_advance_get(IntPtr obj);
	
	public int HorizAdvance
	  {
	     get { return evas_object_text_horiz_advance_get(Raw); }
	  }
	
	[DllImport(Library)]
	private extern static int evas_object_text_vert_advance_get(IntPtr obj);
	
	public int VertAdvance
	  {
	     get { return evas_object_text_vert_advance_get(Raw); }
	  }
	
	[DllImport(Library)]
	private extern static int evas_object_text_inset_get(IntPtr obj);
	
	public int Inset
	  {
	     get { return evas_object_text_inset_get(Raw); }
	  }
	
	[DllImport(Library)]
	private extern static int evas_object_text_char_pos_get(IntPtr obj, int pos, out int cx, out int cy, out int cw, out int ch);
	
	public TextCharCoords CharPosGet(int pos)
	  {	  
	     int x;
	     int y;
	     int w;
	     int h;
	     
	     evas_object_text_char_pos_get(Raw, pos, out x, out y, out w, out h);
	     return new TextCharCoords(x, y, w, h);
	  }
	
	
	[DllImport(Library)]
	private extern static int evas_object_text_char_coords_get(IntPtr obj, int x, int y, out int cx, out int cy, out int cw, out int ch);
	
	public TextCharCoords CharCoordsGet(int cx, int cy)
	  {
	     int x;
	     int y;
	     int w;
	     int h;
	     
	     evas_object_text_char_coords_get(Raw, cy, cy, out x, out y, out w, out h);
	     return new TextCharCoords(x, y, w, h);
	  }	
     }
}
