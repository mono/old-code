using System;
using System.Runtime.InteropServices;

namespace Mono.Guile
{
	public delegate int MainFunc ();

	public class Interpreter
	{
		[DllImport("libguile-mono")]
		static extern int scm_mono_boot_guile (MainFunc main_func);

		[DllImport("libguile-mono")]
		static extern void scm_mono_guile_repl ();

		static bool initialized = false;

		public static void Initialize (MainFunc main_func)
		{
			if (initialized)
				throw new Exception ("This function cannot be called recursively.");

			initialized = true;
			int retval = scm_mono_boot_guile (main_func);
			Environment.Exit (retval);
		}

		public static void Shell ()
		{
			if (!initialized)
				throw new Exception ("Guile is not initialized.");

			scm_mono_guile_repl ();
		}
	}

	public class Test
	{
		static int GuileMain ()
		{
			Console.WriteLine ("Hello World!");
			Interpreter.Shell ();
			Console.WriteLine ("Goodbye.");
			return 4;
		}

		static void Main ()
		{
			Interpreter.Initialize (new MainFunc (GuileMain));
		}
	}
}
