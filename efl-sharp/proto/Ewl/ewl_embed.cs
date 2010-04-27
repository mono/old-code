namespace Ewl {

using System;
using System.Runtime.InteropServices;
using Evas;

	public class Embed : Overlay {
	
		HandleRef objRaw;
		
		public override IntPtr Raw {
		
			get {
			
				return this.objRaw.Handle;
			
			}
		
		}
	
		[DllImport("libewl")]
		static extern IntPtr ewl_embed_evas_set(IntPtr em, IntPtr ev, IntPtr win);
		public Evas.Object Evas(Canvas ev, Window win) {
		
			IntPtr ptr = ewl_embed_evas_set(Raw, ev.Raw, win.Raw);
			return new Evas.Object(ptr);
		
		}
	
	}

}
