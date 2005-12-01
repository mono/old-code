using System;
using System.Drawing;
using System.Runtime.InteropServices;

using Apple.Foundation;
using Apple.AppKit;
using Apple.Tools;

namespace CocoaSharp.Samples {

	public class OpenGLViewSample {

		public static void Main (string[] args) {
			Application.Init ();
			Application.LoadNib ("NSView.nib");

			Application.Run ();
		}

	}

	[Register ("Controller")]
	public class Controller : NSObject {

		[Connect] public SimpleView itsView;

		[Export ("windowDidResize:")]
		public void WindowDidResize (NSNotification aNotification) {
			itsView.invalidate (itsView.bounds);
		}

		public Controller (IntPtr raw, bool rel) : base(raw, rel) { }
	}

	[Register ("SimpleView")]
	public class SimpleView : NSView {

		public SimpleView (IntPtr raw, bool rel) : base(raw, rel) { }

		[Export ("initWithFrame:")]
		public object InitWithFrame (NSRect aRect) {
			return this;
		}

		[Export ("drawRect:")]
		public void Draw (NSRect aRect) {
#if SYSD
			Graphics g = Graphics.FromHwnd (this.Raw);
			Rectangle r = new Rectangle ((int)this.bounds.origin.x, (int)this.bounds.origin.y, (int)this.bounds.size.width, (int)this.bounds.size.height);
			Brush b = new SolidBrush (Color.Red);
			g.FillRectangle (b, r);
			Font f = new Font ("Times New Roman", (int)(this.bounds.size.height/15));
			b = new SolidBrush (Color.White);
			g.DrawString ("This is System.Drawing Text\non a g.FillRectangle background!\nTry Resizing the Window!", f, b, 10, 10);
#else
			NSBezierPath.FillRect (this.bounds);
			Graphics g = Graphics.FromHwnd (this.Raw);
			Font f = new Font ("Times New Roman", (int)(this.bounds.size.height/15));
			Brush b = new SolidBrush (Color.White);
			g.DrawString ("This is System.Drawing Text\non a NSBezierPath background!\nTry Resizing the Window!", f, b, 10, 10);
#endif
		}
	}
}
