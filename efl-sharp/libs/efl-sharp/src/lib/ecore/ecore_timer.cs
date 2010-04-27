namespace Enlightenment.Ecore {

   using System;
   using System.Collections;
   using System.Runtime.InteropServices;

   public delegate int TimerFunction(IntPtr data);   
   
   public class Timer
     {
	const string Library = "ecore";
	
	[DllImport(Library)]
	private extern static double ecore_time_get();
	
	[DllImport(Library)]     
	private extern static IntPtr ecore_timer_add(double _in, IntPtr func, IntPtr data);
	
	[DllImport(Library)]
	private extern static IntPtr ecore_timer_del(IntPtr timer);
	
	[DllImport(Library)]
	private extern static void ecore_timer_interval_set(IntPtr timer, double _in);
     }   
}
