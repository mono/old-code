namespace Enlightenment.Ewl {

using System;
using System.Runtime.InteropServices;

	public class Label : Widget {
		
		[DllImport(Library)]
		static extern IntPtr ewl_text_new(string text);
		public Label(string text) {
		
			objRaw = new HandleRef(this, ewl_text_new(text));
		
		}
		
		[DllImport(Library)]
		static extern void ewl_text_text_set(IntPtr txt, string t);
		public string Text {
		
			set {
			
				ewl_text_text_set(Raw, value);
			
			}
		
		}
		
		[DllImport(Library)]
		static extern void ewl_text_font_set(IntPtr txt, string font, int size);
		public void SetFont(string font, int size) {
		
			ewl_text_font_set(Raw, font, size);
		
		}
		
		[DllImport(Library)]
		static extern string ewl_text_font_get(IntPtr txt);
		public string GetFont {
		
			get {
			
				return ewl_text_font_get(Raw);
				
			}
		
		}
		
		[DllImport(Library)]
		static extern void ewl_text_color_set(IntPtr txt, int r, int g, int b, int a);
		public void SetColor(int r, int g, int b, int a) {
		
			ewl_text_color_set(Raw, r, g, b, a);
		
		}
		
		[DllImport(Library)]
		static extern void ewl_text_style_set(IntPtr txt, string sty);
		[DllImport(Library)]
		static extern string ewl_text_style_get(IntPtr txt);
		public string Style {
		
			set {
				
				ewl_text_style_set(Raw, value);
			
			}
			
			get {
			
				return ewl_text_style_get(Raw);
			
			}
		
		}
	
	}

}
