namespace Enlightenment.Ecore {

using System;
using System.Collections;
using System.Runtime.InteropServices;

	class Idler
	{
		const string Library = "ecore";

		[DllImport(Library)]
		private extern static IntPtr ecore_idler_add(IntPtr func, IntPtr data);
   
		[DllImport(Library)]
		private extern static IntPtr ecore_idler_del(IntPtr idler);
   
		[DllImport(Library)]
		private extern static IntPtr ecore_idle_enterer_add(IntPtr func, IntPtr data);
   
		[DllImport(Library)]
		private extern static IntPtr ecore_idle_enterer_del(IntPtr idle_enterer);
   
		[DllImport(Library)]
		private extern static IntPtr ecore_idle_exiter_add(IntPtr func, IntPtr data);
   
		[DllImport(Library)]
		private extern static IntPtr ecore_idle_exiter_del(IntPtr idle_exiter);
	}

}
