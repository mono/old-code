namespace Enlightenment.Epsilon
{
   
   using System;
   using System.Collections;
   using System.Runtime.InteropServices;
   using System.Reflection;
   using System.Threading;
   
   [StructLayout(LayoutKind.Sequential)]     
   public class Item
     {
	public string hash;
	public string src;
	public string thumb;
	
	public Item()
	  {}
	
	public Item(string h, string s, string t)
	  {
	     hash = h;
	     src = s;
	     thumb = t;
	  }
	
	public Item(IntPtr eps)
	  {
	     Item e = new Item();
	     e = (Item)Marshal.PtrToStructure(eps, typeof(Item));
	     
	     hash = e.hash;
	     src = e.src;
	     thumb = e.thumb;
	  }
     }
   
   public class Thumb
     {
	const string Library = "epsilon";	
	
	private IntPtr eps;
	
	[DllImport(Library)]
	private extern static IntPtr epsilon_new(string file);	
	
	public Thumb()
	  {}
	
	public Thumb(string file)
	  {
	     eps = epsilon_new(file);
	  }
	             
	                
	public Thumb(IntPtr e)
	  {
	     eps = e;
	  }		
	
	[DllImport(Library)]
	private extern static void epsilon_init();
	
	public static void Init()
	  {
	     epsilon_init();
	  }		
	
	[DllImport(Library)]
	private extern static int epsilon_exists(IntPtr e);
	
	public int Exists()
	  {
	     return epsilon_exists(eps);
	  }
	
	[DllImport(Library)]
	private extern static int epsilon_generate(IntPtr e);
	
	public int Generate()
	  {
	     return epsilon_generate(eps);
	  }

	[DllImport(Library)]
	private extern static string epsilon_file_get(IntPtr e);
	
	public string Image
	  {
	     get { return epsilon_file_get(eps); }
	  }	
	
	[DllImport(Library)]
	private extern static string epsilon_thumb_file_get(IntPtr e);
	
	public string Preview
	  {
	     get { return epsilon_thumb_file_get(eps); }
	  }
	
	~Thumb()
	  {}
     }
}
