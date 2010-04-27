namespace Enlightenment.Evas
{
   
   using System;
   using System.Collections;
   using System.Runtime.InteropServices;
   using System.Reflection;
   using System.Threading;
   
   public struct ImageBorder
     {
	public int Left;
	public int Right;
	public int Top;
	public int Bottom;
	
	public ImageBorder(int L, int R, int T, int B)
	  {
	     Left = L;
	     Right = R;
	     Top = T;
	     Bottom = B;
	  }		
     }
   
   public struct ImageFill
     {
	public int X;
	public int Y;
	public int W;
	public int H;
	
	public ImageFill(int x, int y, int w, int h)
	  {
	     X = x;
	     Y = y;
	     W = w;
	     H = h;
	  }
     }

   public struct ImageSize
     {
	public int W;
	public int H;
	
	public ImageSize(int w, int h)
	  {
	     W = w;
	     H = h;
	  }
     }
         
 public class Image : Item
     {
	const string Library = "evas";		
	     
	[DllImport(Library)]
	private extern static IntPtr evas_object_image_add(IntPtr obj);      
	
	[DllImport(Library)]
	private extern static void evas_object_image_file_set(IntPtr obj, string filename, string key);
	
	[DllImport(Library)]
	private extern static void evas_object_image_file_get(IntPtr obj, out string file, out string key);
	
	[DllImport(Library)]
	private extern static void evas_object_image_border_set(IntPtr obj, int l, int r, int t, int b);
	
	[DllImport(Library)]
	private extern static void evas_object_image_border_get(IntPtr obj, out int l, out int r, out int t, out int b);
	
	[DllImport(Library)]
	private extern static void evas_object_image_border_center_fill_set(IntPtr obj, int fill);
	
	[DllImport(Library)]
	private extern static int evas_object_image_border_center_fill_get(IntPtr obj);
	
	[DllImport(Library)]
	private extern static void evas_object_image_fill_set(IntPtr obj, int x, int y, int w, int h);
	
	[DllImport(Library)]
	private extern static void evas_object_image_fill_get(IntPtr obj, out int x, out int y, out int w, out int h);
	
	[DllImport(Library)]
	private extern static void evas_object_image_size_set(IntPtr obj, int w, int h);
	
	[DllImport(Library)]
	private extern static void evas_object_image_size_get(IntPtr obj, out int w, out int h);
	
	[DllImport(Library)]
	private extern static int evas_object_image_load_error_get(IntPtr obj);
	
	[DllImport(Library)]
	private extern static void evas_object_image_data_set(IntPtr obj, int[] data);
	
	[DllImport(Library)]
	private extern static int[] evas_object_image_data_get(IntPtr obj, int for_writing);
	
	[DllImport(Library)]
	private extern static void evas_object_image_data_copy_set(IntPtr obj, IntPtr data);
	
	[DllImport(Library)]
	private extern static void evas_object_image_data_update_add(IntPtr obj, int x, int y, int w, int h);
	
	[DllImport(Library)]
	private extern static void evas_object_image_alpha_set(IntPtr obj, int has_alpha);
	
	[DllImport(Library)]
	private extern static int evas_object_image_alpha_get(IntPtr obj);
	
	[DllImport(Library)]
	private extern static void evas_object_image_smooth_scale_set(IntPtr obj, int smooth_scale);
	
	[DllImport(Library)]
	private extern static int evas_object_image_smooth_scale_get(IntPtr obj);
	
	[DllImport(Library)]
	private extern static void evas_object_image_reload(IntPtr obj);
	
	[DllImport(Library)]
	private extern static int evas_object_image_pixels_import(IntPtr obj, IntPtr pixels);

	public delegate void evas_object_image_pixels_get_callback(IntPtr data, IntPtr o);
	[DllImport(Library)]
	private extern static void evas_object_image_pixels_get_callback_set(IntPtr obj, evas_object_image_pixels_get_callback func, IntPtr data);
	
	[DllImport(Library)]
	private extern static void evas_object_image_pixels_dirty_set(IntPtr obj, int dirty);
	
	[DllImport(Library)]
	private extern static int evas_object_image_pixels_dirty_get(IntPtr obj);
	
	
	public Image()
	  {}

      public Image(Canvas c) : base(c)
	  {	     
	     objRaw = new HandleRef(this, evas_object_image_add(c.Raw));
	  }
	
	
      public Image(Item o)
	  {	     
	     objRaw = new HandleRef(this, o.Raw);
	  }			  

	
	public void Set(string filename, string key)
	  {
	     if(Raw == IntPtr.Zero)
	       objRaw = new HandleRef(this, evas_object_image_add(canvas.Raw));
	     evas_object_image_file_set(Raw, filename, key);
	  }
	
	public void Get(out string file, out string key)
	  {
	     evas_object_image_file_get(Raw, out file, out key);
	  }
	
	public int[] PixelsGet(int for_writing)
	  {
	     return evas_object_image_data_get(Raw, for_writing);
	  }
	
	public void PixelsSet(int[] data)
	  {
	     evas_object_image_data_set(Raw, data);
	  }			
	
	public ImageBorder Border
	  {
	     get
	       {
		  int Left, Right, Top, Bottom;
		  evas_object_image_border_get(Raw, out Left, out Right, out Top, out Bottom);
		  return new ImageBorder (Left, Right, Top, Bottom);
	       }
	     
	     set { evas_object_image_border_set(Raw, value.Left, value.Right, value.Top,	value.Bottom); }
	  }
	
	public int BorderCenterFill
	  {
	     get { return evas_object_image_border_center_fill_get(Raw);}
	     set { evas_object_image_border_center_fill_set(Raw, value);}
	  }

	
	public ImageFill Fill
	  {
	     get
	       {
		  int x, y, w, h;
		  evas_object_image_fill_get(Raw, out x, out y, out w, out h);
		  return new ImageFill (x, y , w, h);
	       }	     
	     set { evas_object_image_fill_set(Raw, value.Y, value.Y, value.W, value.H); }
	  }
	
	public ImageSize Size
	  {
	     get 
	       {
		  int w, h;
		  evas_object_image_size_get(Raw, out w, out h);
		  return new ImageSize(w, h);
	       }
	     set { evas_object_image_size_set(Raw, value.W, value.H); }
	  }
	
	public int LoadError
	  {
	     get { return evas_object_image_load_error_get(Raw); }
	  }	       	            
	
	public IntPtr DataCopy
	  {
	     set { evas_object_image_data_copy_set(Raw, value); }
	  }
	
	public void DataUpdateAdd(int x, int y, int w, int h)
	  {
	     evas_object_image_data_update_add(Raw, x, y, w, h);
	  }
	
	public int Alpha
	  {
	     get { return evas_object_image_alpha_get(Raw); }
	     set { evas_object_image_alpha_set(Raw, value); }
	  }
	
	public int SmoothScale
	  {
	     get { return evas_object_image_smooth_scale_get(Raw); }
	     set { evas_object_image_smooth_scale_set(Raw, value); }
	  }
	
	public void Reload()
	  {
	     evas_object_image_reload(Raw);
	  }
	
	public int PixelsImport(IntPtr pixels)
	  {
	     return evas_object_image_pixels_import(Raw, pixels);
	  }
	
	public void PixelsGetCallbackSet(evas_object_image_pixels_get_callback func, object data)
	  {
	     IntPtr p = new IntPtr(dataptrs.Count);
	     callbacks[-1] = func;
	     dataptrs[p] = data;
	     evas_object_image_pixels_get_callback_set(Raw, func, p);
	  }
	
	public int PixelsDirty
	  {
	     get { return evas_object_image_pixels_dirty_get(Raw); }
	     set { evas_object_image_pixels_dirty_set(Raw, value); }
	  }		     
	     
	~Image()
	  {
	  }	
     }
}
   
