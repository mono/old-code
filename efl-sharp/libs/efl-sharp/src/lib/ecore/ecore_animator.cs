namespace Enlightenment.Ecore {

using System;
using System.Collections;
using System.Runtime.InteropServices;

	delegate int AnimatorFunction(IntPtr data);

	class Animator
	{
		const string Library = "ecore";

		[DllImport(Library)]
		private extern static IntPtr ecore_animator_add(AnimatorFunction func, IntPtr data);

		[DllImport(Library)]
		private extern static IntPtr ecore_animator_del(IntPtr animator);

		[DllImport(Library)]
		private extern static void ecore_animator_frametime_set(double frametime);

		[DllImport(Library)]
		private extern static double ecore_animator_frametime_get();
	}

}
