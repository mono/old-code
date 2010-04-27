namespace Ewl {

using System;
using System.Runtime.InteropServices;

	public class Overlay : Container {
	
		HandleRef objRaw;
		
		public override IntPtr Raw {
		
			get {
			
				return this.objRaw.Handle;
			
			}
		
		}
	
	}

}
