namespace Ewl {

using System;
using System.Runtime.InteropServices;

	public class Container : Widget {
	
		HandleRef objRaw;
		
		public override IntPtr Raw {
		
			get {
			
				return this.objRaw.Handle;
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_container_child_append(IntPtr wid, IntPtr obj);
		
		public void Append(Widget wid) {
		
			ewl_container_child_append(Raw, wid.Raw);
		
		}
		
	}

}
