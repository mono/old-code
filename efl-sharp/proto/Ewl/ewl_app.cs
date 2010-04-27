namespace Ewl {

using System;
using System.Runtime.InteropServices;

	public class Application {
	
		[DllImport("libewl")]
		//static extern int ewl_init(ref int argc, IntPtr argv);
		static extern int ewl_init(ref int argc, params string []argv);
		public static int Init(string []args) {
		
			int argc = args.Length;
		
			return ewl_init(ref argc, args);
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_main();
		public static void Main() {
		
			ewl_main();
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_main_quit();
		public static void Quit() {
		
			ewl_main_quit();
		
		}
		
		[DllImport("libewl")]
		static extern int ewl_shutdown();
		public static int Shutdown() {
		
			return ewl_shutdown();
		
		}
	
	}
	
}
