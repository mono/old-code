namespace Enlightenment.Ewl
{
	using System;
	using System.Runtime.InteropServices;
	using Enlightenment.Ewl.Event;
	
	class Image : Widget
	{
		[DllImport(Library)]
		static extern IntPtr ewl_image_new(string imgpath, string imgkey);
	
		public Image(string imgpath, string imgkey)
		{
			objRaw = new HandleRef(this, ewl_image_new(imgpath, imgkey));
		}

		[DllImport(Library)]
		static extern string ewl_image_file_get(IntPtr img);
				
		public string File
		{
			get{ return ewl_image_file_get(Raw);}
		}

		[DllImport(Library)]
		static extern string ewl_image_scale(IntPtr img, double wp, double hp);
				
		public void Scale(double wp, double hp)
		{
			ewl_image_scale(Raw, wp, hp);
		}
		
		[DllImport(Library)]
		static extern string ewl_image_scale_to(IntPtr img, int w, int h);
		
		public void ScaleTo(int w, int h)
		{
			ewl_image_scale_to(Raw, w, h);
		}
	}
}
