using System;
using Xnb;
using Xnb.Protocol.Xnb;
using Xnb.Protocol.XProto;

using System.Runtime.InteropServices;

public class Driver
{
	public static void Main (string[] args)
	{
		if (args.Length != 3) {
			Console.Error.WriteLine ("Usage: testsimple [depth] [root] [visual]");
			return;
		}

		Connection c = new Connection (null, 0);
		//Connection c = new Connection ("localhost", 0);

		Console.WriteLine ("Release: " + c.Setup.ReleaseNumber);
		Console.WriteLine ("Vendor: " + c.Setup.Vendor);
		Console.WriteLine ("VendorLen: " + c.Setup.VendorLen);
		Console.WriteLine ("RootsLen: " + c.Setup.RootsLen);
		Console.WriteLine ("PixmapFormatsLen: " + c.Setup.PixmapFormatsLen);

		/*
			 Screen root;
			 int screen_num = 0;
			 root = Aux.GetScreen (c, screen_num);
			 int depth = 1;
			 Console.WriteLine (root);
			 Console.WriteLine (root.Root);
			 Console.WriteLine (root.WhitePixel);
			 Console.WriteLine (root.WidthInPixels);
			 Console.WriteLine (root.HeightInPixels);
			 */

		XCMisc xcmisc = new XCMisc ();
		xcmisc.Init (c);

		/*
			 XFixes xfixes = new XFixes ();
			 xfixes.Init (c);
			 */

		XidManager xm = new XidManager (xcmisc);

		XProto xp = new XProto ();
		xp.Init (c);

		WindowId wid = (WindowId)xm.Generate ();
		Console.WriteLine ("New xid: " + (uint)wid);

		//use xdpyinfo to find root window id
		WindowId parentId = new WindowId ((uint)Int32.Parse (args[1], System.Globalization.NumberStyles.HexNumber));

		//use xdpyinfo to find a visual
		//uint visualId = 0x23;
		uint visualId = (uint)Int32.Parse (args[2], System.Globalization.NumberStyles.HexNumber);

		//byte depth = 16;
		byte depth = (byte)Int32.Parse (args[0]);

		uint[] flags = new uint[4];

		//Mask
		flags[0] = (uint) (WindowValueMask.BackgroundPixel | WindowValueMask.EventMask | WindowValueMask.DoNotPropagateMask);
		//flags[0] = (uint) (Cw.BackPixel | Cw.EventMask | Cw.DontPropagate);
		//flags[0] = 0x00000100 | 0x00000800 | 0x00001000;

		//BackgroundPixel
		flags[1] = 0xffff0000;

		//EventMask
		//flags[2] = (uint) (EventMask.ButtonReleaseMask | EventMask.ExposureMask);
		flags[2] = 0x00000008 | 0x00008000;
		
		//DoNotPropagateMask
		//flags[3] = (uint) (EventMask.ButtonPressMask);
		flags[3] = 0x00000004;

		xp.CreateWindow (depth, wid, parentId, 10, 10, 100, 100, 0, (ushort)WindowClass.InputOutput, visualId, flags);

		xp.ChangeProperty ((byte)PropertyMode.Replace, wid, AtomType.WM_NAME, AtomType.STRING, 8, 0);

		xp.MapWindow (wid);

		PixmapId pid = (PixmapId)xm.Generate ();
		xp.CreatePixmap (depth, pid, wid, 100, 100);

		Rectangle rect = new Rectangle ();
		rect.X = 0;
		rect.Y = 0;
		rect.Width = 100;
		rect.Height = 100;

		Rectangle[] rects = new Rectangle[1];
		rects[0] = rect;

		//xc.PolyFillRectangle (pid, whitegc, rects);

		xp.ClearArea (false, wid, 10, 10, 20, 20);

		//Sync:
		//xp.GetInputFocus ();

		while (true) {
			c.xrr.ReadMessage ();
		}
	}
}
