namespace Ewl {

using System;
using System.Runtime.InteropServices;

	public delegate void DeleteEventHandler(IntPtr w, IntPtr evnt, IntPtr data);

	public class Window : Embed {

		HandleRef objRaw;
		
		[DllImport("libewl")]
		static extern int ewl_callback_append(IntPtr wid, int i, DeleteEventHandler de, IntPtr data);
		
		[DllImport("libewl")]
		static extern void ewl_callback_del_with_data(IntPtr wid, int i, DeleteEventHandler de, IntPtr data);
		
		public event DeleteEventHandler DeleteEvent {
		
			add {
			
				ewl_callback_append(Raw, 6, value, new IntPtr());
				System.GC.KeepAlive(value);
			
			}
			
			remove {
			
				ewl_callback_del_with_data(Raw, 6, value, new IntPtr());
			
			}
		
		}
		
		public override IntPtr Raw {
		
			get {
			
				return this.objRaw.Handle;
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_widget_destroy(IntPtr wid);

		[DllImport("libewl")]
		static extern IntPtr ewl_window_new();
		 
		public Window() {
		 
		 	objRaw = new HandleRef(this, ewl_window_new());
		 
		}
		
		[DllImport("libewl")]
		static extern void ewl_window_title_set(IntPtr win, string title);
		
		public string Title {
		   
			set {
			   
				ewl_window_title_set(Raw, value);
			   
			}
		   
		}
		
		[DllImport("libewl")]
		static extern void ewl_window_class_set(IntPtr win, string cls);
		
		public string Class {
		
			set {
			
				ewl_window_class_set(Raw, value);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_window_name_set(IntPtr win, string name);
		[DllImport("libewl")]
		static extern string ewl_window_name_get(IntPtr win);
		public string Name {
		
			get {
			
				return ewl_window_name_get(Raw);
			
			}
			
			set {
			
				ewl_window_name_set(Raw, value);
			
			}
		
		}
	
	}
	
}
