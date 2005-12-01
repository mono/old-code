using System;
using System.Runtime.InteropServices;

using Apple.Foundation;
using Apple.AppKit;

namespace CocoaSharp.Samples {

	public class OpenGLViewSample {

		public static void Main (string[] args) {
			Application.Init ();
			Application.LoadNib ("NSOpenGLView.nib");

			Application.Run ();
		}

	}

	[Register ("Controller")]
	public class Controller : NSObject {

		[Connect] public SimpleOpenGLView itsView;
		public NSTimer timer;

		public Controller (IntPtr raw, bool rel) : base(raw, rel) { }

		[Export ("endTimer:")]
		public void EndTimer (object sender) {
			if (timer != null) {
				timer.invalidate ();
				timer = null;
			}
		}

		[Export ("startTimer:")]
		public void StartTimer (object sender) {
			if (timer == null) {
				timer = NSTimer.TimerWithTimeInterval_target_selector_userInfo_repeats (0.05, itsView, "viewTick:", null, true);
				NSRunLoop.CurrentRunLoop.addTimer_forMode (timer, "NSDefaultRunLoopMode");
			}
		}
		[Export ("interfaceChanged:")]
		public object InterfaceChanged (object sender) {
			itsView.display ();
			return null;
		}
	}

	[Register ("SimpleOpenGLView")]
	public class SimpleOpenGLView : NSOpenGLView {

		private NSRect viewFrame;
		private float xrotate = 0.0f;
		private float yrotate = 0.0f;
		private float zrotate = 0.0f;
		private float zoom = 0.0f;
#if RECTANGLE
		private NSColor color1;
		private NSColor color2;
		private NSColor color3;
		private NSColor color4;
#endif
		private float zoomdelta = 0.1f;
		public Random random = new Random ();

		public SimpleOpenGLView (IntPtr raw, bool rel) : base(raw, rel) { }

#if RECTANGLE
		private NSColor SetColor (NSColor input) {
			return NSColor.ColorWithDeviceHue_saturation_brightness_alpha ((random.Next(360)%360)/360.0f, 1.0f, 1.0f, 1.0f);
		}
#endif

		[Export ("viewTick:")]
		public void ViewTick (object sender) {
			xrotate += 1.0f; if (xrotate > 350.0f) { xrotate = 0.0f; }
			yrotate += 1.0f; if (yrotate > 350.0f) { yrotate = 0.0f; }
			zrotate += 1.0f; if (zrotate > 350.0f) { zrotate = 0.0f; }
			zoom += zoomdelta; if (zoom > 4.9f) { zoomdelta = -0.1f; } if (zoom < 0.2f) { zoomdelta = 0.1f; }
#if RECTANGLE
			color1 = SetColor (color1);
			color2 = SetColor (color2);
			color3 = SetColor (color3);
			color4 = SetColor (color4);
#endif
			Draw (viewFrame);
		}

		[Export ("initWithFrame:")]
		public object InitWithFrame (NSRect aRect) {
			viewFrame = aRect;
#if RECTANGLE
			color1 = NSColor.WhiteColor;
			color2 = NSColor.RedColor;
			color3 = NSColor.BlueColor;
			color4 = NSColor.GreenColor;
#endif
			return this;
		}

		[Export ("drawRect:")]
		public void Draw (NSRect aRect) {
#if RECTANGLE
			NSColor rgbColor;
#endif
			glViewport (0, 0, 520, 310);
			glMatrixMode (0x1701);
			glLoadIdentity ();
			glClearColor (0.0f, 0.0f, 0.0f, 0.0f);
			glClear (0x00004000);

			gluPerspective (60.0, 1.0, 1.5, 20.0);
			glMatrixMode (0x1700);

			glLoadIdentity ();
			gluLookAt (0.0, 0.0, 5.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0);

			glRotatef (xrotate, 1.0f, 0.0f, 0.0f);
			glRotatef (yrotate, 0.0f, 1.0f, 0.0f);
			glRotatef (zrotate, 0.0f, 0.0f, 1.0f);
			glScalef (zoom, zoom, zoom);

			glBegin (0x0007);
#if RECTANGLE
			rgbColor = color1.colorUsingColorSpaceName ("NSDeviceRGBColorSpace");
			glColor3f (rgbColor.redComponent, rgbColor.greenComponent, rgbColor.blueComponent);
			glVertex2f (-1.0f, -1.0f);

			rgbColor = color2.colorUsingColorSpaceName ("NSDeviceRGBColorSpace");
			glColor3f (rgbColor.redComponent, rgbColor.greenComponent, rgbColor.blueComponent);
			glVertex2f (1.0f, -1.0f);

			rgbColor = color3.colorUsingColorSpaceName ("NSDeviceRGBColorSpace");
			glColor3f (rgbColor.redComponent, rgbColor.greenComponent, rgbColor.blueComponent);
			glVertex2f (1.0f, 1.0f);

			rgbColor = color4.colorUsingColorSpaceName ("NSDeviceRGBColorSpace");
			glColor3f (rgbColor.redComponent, rgbColor.greenComponent, rgbColor.blueComponent);
			glVertex2f (-1.0f, 1.0f);
#endif

#if CUBE
			glColor3f(0.0f,1.0f,0.0f);  // Color Blue
			glVertex3f( 1.0f, 1.0f,-1.0f);      // Top Right Of The Quad (Top)
			glVertex3f(-1.0f, 1.0f,-1.0f);      // Top Left Of The Quad (Top)
			glVertex3f(-1.0f, 1.0f, 1.0f);      // Bottom Left Of The Quad (Top)
			glVertex3f( 1.0f, 1.0f, 1.0f); 

			glColor3f(1.0f,0.5f,0.0f);  // Color Orange
			glVertex3f( 1.0f,-1.0f, 1.0f);      // Top Right Of The Quad (Bottom)
			glVertex3f(-1.0f,-1.0f, 1.0f);      // Top Left Of The Quad (Bottom)
			glVertex3f(-1.0f,-1.0f,-1.0f);      // Bottom Left Of The Quad (Bottom)
			glVertex3f( 1.0f,-1.0f,-1.0f);   

			glColor3f(1.0f,0.0f,0.0f);  // Color Red    
			glVertex3f( 1.0f, 1.0f, 1.0f);      // Top Right Of The Quad (Front)
			glVertex3f(-1.0f, 1.0f, 1.0f);      // Top Left Of The Quad (Front)
			glVertex3f(-1.0f,-1.0f, 1.0f);      // Bottom Left Of The Quad (Front)
			glVertex3f( 1.0f,-1.0f, 1.0f); 

			glColor3f(1.0f,1.0f,0.0f);  // Color Yellow
			glVertex3f( 1.0f,-1.0f,-1.0f);      // Top Right Of The Quad (Back)
			glVertex3f(-1.0f,-1.0f,-1.0f);      // Top Left Of The Quad (Back)
			glVertex3f(-1.0f, 1.0f,-1.0f);      // Bottom Left Of The Quad (Back)
			glVertex3f( 1.0f, 1.0f,-1.0f); 

			glColor3f(0.0f,0.0f,1.0f);  // Color Blue
			glVertex3f(-1.0f, 1.0f, 1.0f);      // Top Right Of The Quad (Left)
			glVertex3f(-1.0f, 1.0f,-1.0f);      // Top Left Of The Quad (Left)
			glVertex3f(-1.0f,-1.0f,-1.0f);      // Bottom Left Of The Quad (Left)
			glVertex3f(-1.0f,-1.0f, 1.0f);      // Bottom Right Of The Quad (Left)

			glColor3f(1.0f,0.0f,1.0f);  // Color Violet
			glVertex3f( 1.0f, 1.0f,-1.0f);      // Top Right Of The Quad (Right)
			glVertex3f( 1.0f, 1.0f, 1.0f);      // Top Left Of The Quad (Right)
			glVertex3f( 1.0f,-1.0f, 1.0f);      // Bottom Left Of The Quad (Right)
			glVertex3f( 1.0f,-1.0f,-1.0f);      // Bottom Right Of The Quad (Right)
#endif
			glEnd ();
			glFlush ();
		}

		[DllImport ("/System/Library/Frameworks/OpenGL.Framework/Versions/Current/OpenGL")]
		public static extern void glViewport (int x, int y, int width, int height);
		[DllImport ("/System/Library/Frameworks/OpenGL.Framework/Versions/Current/OpenGL")]
		public static extern void glMatrixMode (int type);
		[DllImport ("/System/Library/Frameworks/OpenGL.Framework/Versions/Current/OpenGL")]
		public static extern void glLoadIdentity ();
		[DllImport ("/System/Library/Frameworks/OpenGL.Framework/Versions/Current/OpenGL")]
		public static extern void glClearColor (float r, float g, float b, float a);
		[DllImport ("/System/Library/Frameworks/OpenGL.Framework/Versions/Current/OpenGL")]
		public static extern void glRotatef (float angle, float x, float y, float z);
		[DllImport ("/System/Library/Frameworks/OpenGL.Framework/Versions/Current/OpenGL")]
		public static extern void glScalef (float x, float y, float z);
		[DllImport ("/System/Library/Frameworks/OpenGL.Framework/Versions/Current/OpenGL")]
		public static extern void gluPerspective (double a, double b, double c, double d);
		[DllImport ("/System/Library/Frameworks/OpenGL.Framework/Versions/Current/OpenGL")]
		public static extern void gluLookAt (double eyeX, double eyeY, double eyeZ, double centerX, double centerY, double centerZ, double upX, double upY, double upZ);
		[DllImport ("/System/Library/Frameworks/OpenGL.Framework/Versions/Current/OpenGL")]
		public static extern void glClear (int mask);
		[DllImport ("/System/Library/Frameworks/OpenGL.Framework/Versions/Current/OpenGL")]
		public static extern void glBegin (int mode);
		[DllImport ("/System/Library/Frameworks/OpenGL.Framework/Versions/Current/OpenGL")]
		public static extern void glVertex2f (float x, float y);
		[DllImport ("/System/Library/Frameworks/OpenGL.Framework/Versions/Current/OpenGL")]
		public static extern void glVertex3f (float x, float y, float z);
		[DllImport ("/System/Library/Frameworks/OpenGL.Framework/Versions/Current/OpenGL")]
		public static extern void glEnd ();
		[DllImport ("/System/Library/Frameworks/OpenGL.Framework/Versions/Current/OpenGL")]
		public static extern void glFlush ();
		[DllImport ("/System/Library/Frameworks/OpenGL.Framework/Versions/Current/OpenGL")]
		public static extern void glColor3f (float r, float g, float b);
	}
}
