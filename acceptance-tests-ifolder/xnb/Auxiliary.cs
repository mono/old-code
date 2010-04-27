using System;
using System.Diagnostics;

namespace Xnb
{
	using Protocol.Xnb;

	public static class Aux
	{
		public static byte GetDepth (Connection c, Screen screen)
		{
			//Drawable drawable;

			//drawable.Window = screen.Root;

			return 0;
		}

		public static Screen GetScreen (Connection c, int screen)
		{
			//foreach (Screen s in c.Setup.Roots) {}
			
			//Trace.WriteLine (c.Setup.Roots.Length);
			//return c.Setup.Roots[screen];
			
			return new Screen ();
		}

		public static Visualtype GetVisualtype (Connection c, int scr, uint vid)
		{
			return new Visualtype ();
		}
	}
}
