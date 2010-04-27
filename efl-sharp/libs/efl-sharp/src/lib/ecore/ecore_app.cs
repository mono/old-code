namespace Enlightenment.Ecore {

using System;
using System.Collections;
using System.Runtime.InteropServices;

	class Application
	{
		const string Library = "ecore";

		[DllImport(Library)]
		private extern static IntPtr ecore_exe_run(string exe_cmd, IntPtr data);
   
		[DllImport(Library)]
		private extern static IntPtr ecore_exe_free(IntPtr exe);
   
		[DllImport(Library)]
		private extern static int ecore_exe_pid_get(IntPtr exe);
   
		[DllImport(Library)]
		private extern static IntPtr ecore_exe_data_get(IntPtr exe);
   
		[DllImport(Library)]
		private extern static void ecore_exe_pause(IntPtr exe);
   
		[DllImport(Library)]
		private extern static void ecore_exe_continue(IntPtr exe);
   
		[DllImport(Library)]
		private extern static void ecore_exe_terminate(IntPtr exe);
   
		[DllImport(Library)]
		private extern static void ecore_exe_kill(IntPtr exe);
   
		[DllImport(Library)]
		private extern static void ecore_exe_signal(IntPtr exe, int num);
   
		[DllImport(Library)]
		private extern static void ecore_exe_hup(IntPtr exe);
	}

}
