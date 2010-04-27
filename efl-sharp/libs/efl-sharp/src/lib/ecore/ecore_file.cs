namespace Enlightenment.Ecore {

using System;
using System.Collections;
using System.Runtime.InteropServices;

	class File
	{
		const string Library = "ecore";

		[DllImport(Library)]
		private extern static IntPtr ecore_main_fd_handler_add(int fd, IntPtr flags, IntPtr func, IntPtr buf_func, IntPtr buf_data);

		[DllImport(Library)]
		private extern static void ecore_main_fd_handler_prepare_callback_set(IntPtr fd_handler, IntPtr func, IntPtr data);

		[DllImport(Library)]
		private extern static IntPtr ecore_main_fd_handler_del(IntPtr fd_handler);

		[DllImport(Library)]
		private extern static int ecore_main_fd_handler_fd_get(IntPtr fd_handler);

		[DllImport(Library)]
		private extern static int ecore_main_fd_handler_active_get(IntPtr fd_handler, IntPtr flags);

		[DllImport(Library)]
		private extern static void ecore_main_fd_handler_active_set(IntPtr fd_handler, IntPtr flags);
	}

}
