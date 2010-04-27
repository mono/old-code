namespace Enlightenment.Ecore {

using System;
using System.Collections;
using System.Runtime.InteropServices;

	public class MainLoop
	{
		const string Library = "ecore";
   
		[DllImport(Library)]
		private extern static int ecore_init();
		public static int Init()
		{
			return ecore_init();
		}
   
		[DllImport(Library)]
		private extern static int ecore_shutdown();
		public static int Shutdown()
		{
			return ecore_shutdown();
		}
   
		[DllImport(Library)]
		private extern static void ecore_app_restart();
		public static void Restart()
		{
			ecore_app_restart();
		}
   
		[DllImport(Library)]
		private extern static void ecore_main_loop_iterate();
		public static void Iterate()
		{
			ecore_main_loop_iterate();
		}
   
		[DllImport(Library)]
		private extern static void ecore_main_loop_begin();
		public static void Begin()
		{
			ecore_main_loop_begin();
		}

		[DllImport(Library)]
		private extern static void ecore_main_loop_quit();
		public static void Quit()
		{
			ecore_main_loop_quit();
		}

		[DllImport(Library)]
		private extern static void ecore_app_args_set(int argc, string [] argv);
		public static void ArgsSet(int argc, string [] argv)
		{
			ecore_app_args_set(argc, argv);
		}
   
		[DllImport(Library)]
		private extern static void ecore_app_args_get(out int argc, out string [] argv);
		public static void ArgsGet(out int argc, out string [] argv)
		{
			ecore_app_args_get(out argc, out argv);
		}

	}

}
