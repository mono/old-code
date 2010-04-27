namespace gl {
	using System.Runtime.InteropServices;
	using System;
	public class glut {
/**
 GLUT API revision history:

 GLUT_API_VERSION is updated to reflect incompatible GLUT
 API changes (interface changes, semantic changes, deletions,
 or additions).

 GLUT_API_VERSION=1  First public release of GLUT.  11/29/94

 GLUT_API_VERSION=2  Added support for OpenGL/GLX multisampling,
 extension.  Supports new input devices like tablet, dial and button
 box, and Spaceball.  Easy to query OpenGL extensions.

 GLUT_API_VERSION=3  glutMenuStatus added.

 GLUT_API_VERSION=4  glutInitDisplayString, glutWarpPointer,
 glutBitmapLength, glutStrokeLength, glutWindowStatusFunc, dynamic
 video resize subAPI, glutPostWindowRedisplay, glutKeyboardUpFunc,
 glutSpecialUpFunc, glutIgnoreKeyRepeat, glutSetKeyRepeat,
 glutJoystickFunc, glutForceJoystickFunc (NOT FINALIZED!).
**/
		public const uint GLUT_API_VERSION		 = 3;

/**
 GLUT implementation revision history:

 GLUT_XLIB_IMPLEMENTATION is updated to reflect both GLUT
 API revisions and implementation revisions (ie, bug fixes).

 GLUT_XLIB_IMPLEMENTATION=1  mjk's first public release of
 GLUT Xlib-based implementation.  11/29/94

 GLUT_XLIB_IMPLEMENTATION=2  mjk's second public release of
 GLUT Xlib-based implementation providing GLUT version 2
 interfaces.

 GLUT_XLIB_IMPLEMENTATION=3  mjk's GLUT 2.2 images. 4/17/95

 GLUT_XLIB_IMPLEMENTATION=4  mjk's GLUT 2.3 images. 6/?/95

 GLUT_XLIB_IMPLEMENTATION=5  mjk's GLUT 3.0 images. 10/?/95

 GLUT_XLIB_IMPLEMENTATION=7  mjk's GLUT 3.1+ with glutWarpPoitner.  7/24/96

 GLUT_XLIB_IMPLEMENTATION=8  mjk's GLUT 3.1+ with glutWarpPoitner
 and video resize.  1/3/97

 GLUT_XLIB_IMPLEMENTATION=9 mjk's GLUT 3.4 release with early GLUT 4 routines.

 GLUT_XLIB_IMPLEMENTATION=11 Mesa 2.5's GLUT 3.6 release.

 GLUT_XLIB_IMPLEMENTATION=12 mjk's GLUT 3.6 release with early GLUT 4 routines + signal handling.

 GLUT_XLIB_IMPLEMENTATION=13 mjk's GLUT 3.7 beta with GameGLUT support.

 GLUT_XLIB_IMPLEMENTATION=14 mjk's GLUT 3.7 beta with f90gl friend interface.

 GLUT_XLIB_IMPLEMENTATION=15 mjk's GLUT 3.7 beta sync'ed with Mesa <GL/glut.h>
**/
		public const uint GLUT_XLIB_IMPLEMENTATION	 = 15;

/* Display mode bit masks. */
		public const uint GLUT_RGB			 = 0;
		public const uint GLUT_RGBA			 = GLUT_RGB;
		public const uint GLUT_INDEX			 = 1;
		public const uint GLUT_SINGLE			 = 0;
		public const uint GLUT_DOUBLE			 = 2;
		public const uint GLUT_ACCUM			 = 4;
		public const uint GLUT_ALPHA			 = 8;
		public const uint GLUT_DEPTH			 = 16;
		public const uint GLUT_STENCIL			 = 32;
		public const uint GLUT_MULTISAMPLE		 = 128;
		public const uint GLUT_STEREO			 = 256;
		public const uint GLUT_LUMINANCE			 = 512;

/* Mouse buttons. */
		public const uint GLUT_LEFT_BUTTON		 = 0;
		public const uint GLUT_MIDDLE_BUTTON		 = 1;
		public const uint GLUT_RIGHT_BUTTON		 = 2;

/* Mouse button  state. */
		public const uint GLUT_DOWN			 = 0;
		public const uint GLUT_UP				 = 1;

/* function keys */
		public const uint GLUT_KEY_F1			 = 1;
		public const uint GLUT_KEY_F2			 = 2;
		public const uint GLUT_KEY_F3			 = 3;
		public const uint GLUT_KEY_F4			 = 4;
		public const uint GLUT_KEY_F5			 = 5;
		public const uint GLUT_KEY_F6			 = 6;
		public const uint GLUT_KEY_F7			 = 7;
		public const uint GLUT_KEY_F8			 = 8;
		public const uint GLUT_KEY_F9			 = 9;
		public const uint GLUT_KEY_F10			 = 10;
		public const uint GLUT_KEY_F11			 = 11;
		public const uint GLUT_KEY_F12			 = 12;
/* directional keys */
		public const uint GLUT_KEY_LEFT			 = 100;
		public const uint GLUT_KEY_UP			 = 101;
		public const uint GLUT_KEY_RIGHT			 = 102;
		public const uint GLUT_KEY_DOWN			 = 103;
		public const uint GLUT_KEY_PAGE_UP		 = 104;
		public const uint GLUT_KEY_PAGE_DOWN		 = 105;
		public const uint GLUT_KEY_HOME			 = 106;
		public const uint GLUT_KEY_END			 = 107;
		public const uint GLUT_KEY_INSERT			 = 108;

/* Entry/exit  state. */
		public const uint GLUT_LEFT			 = 0;
		public const uint GLUT_ENTERED			 = 1;

/* Menu usage  state. */
		public const uint GLUT_MENU_NOT_IN_USE		 = 0;
		public const uint GLUT_MENU_IN_USE		 = 1;

/* Visibility  state. */
		public const uint GLUT_NOT_VISIBLE		 = 0;
		public const uint GLUT_VISIBLE			 = 1;

/* Window status  state. */
		public const uint GLUT_HIDDEN			 = 0;
		public const uint GLUT_FULLY_RETAINED		 = 1;
		public const uint GLUT_PARTIALLY_RETAINED		 = 2;
		public const uint GLUT_FULLY_COVERED		 = 3;

/* Color index component selection values. */
		public const uint GLUT_RED			 = 0;
		public const uint GLUT_GREEN			 = 1;
		public const uint GLUT_BLUE			 = 2;

/* Layers for use. */
		public const uint GLUT_NORMAL			 = 0;
		public const uint GLUT_OVERLAY			 = 1;

/* Stroke font opaque addresses (use constants instead in source code). */
//  GLUTAPI void *glutStrokeRoman;
//  GLUTAPI void *glutStrokeMonoRoman;

/* Stroke font constants (use these in GLUT program). */
//  		public const uint GLUT_STROKE_ROMAN		 = (&glutStrokeRoman);
//  		public const uint GLUT_STROKE_MONO_ROMAN		 = (&glutStrokeMonoRoman);

/* Bitmap font opaque addresses (use constants instead in source code). */
//  GLUTAPI void *glutBitmap9By15;
//  GLUTAPI void *glutBitmap8By13;
//  GLUTAPI void *glutBitmapTimesRoman10;
//  GLUTAPI void *glutBitmapTimesRoman24;
//  GLUTAPI void *glutBitmapHelvetica10;
//  GLUTAPI void *glutBitmapHelvetica12;
//  GLUTAPI void *glutBitmapHelvetica18;

/* Bitmap font constants (use these in GLUT program). */
//  		public const uint GLUT_BITMAP_9_BY_15		 = (&glutBitmap9By15);
//  		public const uint GLUT_BITMAP_8_BY_13		 = (&glutBitmap8By13);
//  		public const uint GLUT_BITMAP_TIMES_ROMAN_10	 = (&glutBitmapTimesRoman10);
//  		public const uint GLUT_BITMAP_TIMES_ROMAN_24	 = (&glutBitmapTimesRoman24);
//  		public const uint GLUT_BITMAP_HELVETICA_10	 = (&glutBitmapHelvetica10);
//  		public const uint GLUT_BITMAP_HELVETICA_12	 = (&glutBitmapHelvetica12);
//  		public const uint GLUT_BITMAP_HELVETICA_18	 = (&glutBitmapHelvetica18);

/* glutGet parameters. */
		public const uint GLUT_WINDOW_X			 = 100;
		public const uint GLUT_WINDOW_Y			 = 101;
		public const uint GLUT_WINDOW_WIDTH		 = 102;
		public const uint GLUT_WINDOW_HEIGHT		 = 103;
		public const uint GLUT_WINDOW_BUFFER_SIZE		 = 104;
		public const uint GLUT_WINDOW_STENCIL_SIZE	 = 105;
		public const uint GLUT_WINDOW_DEPTH_SIZE		 = 106;
		public const uint GLUT_WINDOW_RED_SIZE		 = 107;
		public const uint GLUT_WINDOW_GREEN_SIZE		 = 108;
		public const uint GLUT_WINDOW_BLUE_SIZE		 = 109;
		public const uint GLUT_WINDOW_ALPHA_SIZE		 = 110;
		public const uint GLUT_WINDOW_ACCUM_RED_SIZE	 = 111;
		public const uint GLUT_WINDOW_ACCUM_GREEN_SIZE	 = 112;
		public const uint GLUT_WINDOW_ACCUM_BLUE_SIZE	 = 113;
		public const uint GLUT_WINDOW_ACCUM_ALPHA_SIZE	 = 114;
		public const uint GLUT_WINDOW_DOUBLEBUFFER	 = 115;
		public const uint GLUT_WINDOW_RGBA		 = 116;
		public const uint GLUT_WINDOW_PARENT		 = 117;
		public const uint GLUT_WINDOW_NUM_CHILDREN	 = 118;
		public const uint GLUT_WINDOW_COLORMAP_SIZE	 = 119;
		public const uint GLUT_WINDOW_NUM_SAMPLES		 = 120;
		public const uint GLUT_WINDOW_STEREO		 = 121;
		public const uint GLUT_WINDOW_CURSOR		 = 122;
		public const uint GLUT_SCREEN_WIDTH		 = 200;
		public const uint GLUT_SCREEN_HEIGHT		 = 201;
		public const uint GLUT_SCREEN_WIDTH_MM		 = 202;
		public const uint GLUT_SCREEN_HEIGHT_MM		 = 203;
		public const uint GLUT_MENU_NUM_ITEMS		 = 300;
		public const uint GLUT_DISPLAY_MODE_POSSIBLE	 = 400;
		public const uint GLUT_INIT_WINDOW_X		 = 500;
		public const uint GLUT_INIT_WINDOW_Y		 = 501;
		public const uint GLUT_INIT_WINDOW_WIDTH		 = 502;
		public const uint GLUT_INIT_WINDOW_HEIGHT		 = 503;
		public const uint GLUT_INIT_DISPLAY_MODE		 = 504;
		public const uint GLUT_ELAPSED_TIME		 = 700;
		public const uint GLUT_WINDOW_FORMAT_ID		 = 123;

/* glutDeviceGet parameters. */
		public const uint GLUT_HAS_KEYBOARD		 = 600;
		public const uint GLUT_HAS_MOUSE			 = 601;
		public const uint GLUT_HAS_SPACEBALL		 = 602;
		public const uint GLUT_HAS_DIAL_AND_BUTTON_BOX	 = 603;
		public const uint GLUT_HAS_TABLET			 = 604;
		public const uint GLUT_NUM_MOUSE_BUTTONS		 = 605;
		public const uint GLUT_NUM_SPACEBALL_BUTTONS	 = 606;
		public const uint GLUT_NUM_BUTTON_BOX_BUTTONS	 = 607;
		public const uint GLUT_NUM_DIALS			 = 608;
		public const uint GLUT_NUM_TABLET_BUTTONS		 = 609;
		public const uint GLUT_DEVICE_IGNORE_KEY_REPEAT    = 610;
		public const uint GLUT_DEVICE_KEY_REPEAT           = 611;
		public const uint GLUT_HAS_JOYSTICK		 = 612;
		public const uint GLUT_OWNS_JOYSTICK		 = 613;
		public const uint GLUT_JOYSTICK_BUTTONS		 = 614;
		public const uint GLUT_JOYSTICK_AXES		 = 615;
		public const uint GLUT_JOYSTICK_POLL_RATE		 = 616;

/* glutLayerGet parameters. */
		public const uint GLUT_OVERLAY_POSSIBLE            = 800;
		public const uint GLUT_LAYER_IN_USE		 = 801;
		public const uint GLUT_HAS_OVERLAY		 = 802;
		public const uint GLUT_TRANSPARENT_INDEX		 = 803;
		public const uint GLUT_NORMAL_DAMAGED		 = 804;
		public const uint GLUT_OVERLAY_DAMAGED		 = 805;

/* glutVideoResizeGet parameters. */
		public const uint GLUT_VIDEO_RESIZE_POSSIBLE	 = 900;
		public const uint GLUT_VIDEO_RESIZE_IN_USE	 = 901;
		public const uint GLUT_VIDEO_RESIZE_X_DELTA	 = 902;
		public const uint GLUT_VIDEO_RESIZE_Y_DELTA	 = 903;
		public const uint GLUT_VIDEO_RESIZE_WIDTH_DELTA	 = 904;
		public const uint GLUT_VIDEO_RESIZE_HEIGHT_DELTA	 = 905;
		public const uint GLUT_VIDEO_RESIZE_X		 = 906;
		public const uint GLUT_VIDEO_RESIZE_Y		 = 907;
		public const uint GLUT_VIDEO_RESIZE_WIDTH		 = 908;
		public const uint GLUT_VIDEO_RESIZE_HEIGHT	 = 909;

/* glutUseLayer parameters. */
		//public const uint GLUT_NORMAL			 = 0;
		//public const uint GLUT_OVERLAY			 = 1;

/* glutGetModifiers return mask. */
		public const uint GLUT_ACTIVE_SHIFT                = 1;
		public const uint GLUT_ACTIVE_CTRL                 = 2;
		public const uint GLUT_ACTIVE_ALT                  = 4;

/* glutSetCursor parameters. */
/* Basic arrows. */
		public const uint GLUT_CURSOR_RIGHT_ARROW		 = 0;
		public const uint GLUT_CURSOR_LEFT_ARROW		 = 1;
/* Symbolic cursor shapes. */
		public const uint GLUT_CURSOR_INFO		 = 2;
		public const uint GLUT_CURSOR_DESTROY		 = 3;
		public const uint GLUT_CURSOR_HELP		 = 4;
		public const uint GLUT_CURSOR_CYCLE		 = 5;
		public const uint GLUT_CURSOR_SPRAY		 = 6;
		public const uint GLUT_CURSOR_WAIT		 = 7;
		public const uint GLUT_CURSOR_TEXT		 = 8;
		public const uint GLUT_CURSOR_CROSSHAIR		 = 9;
/* Directional cursors. */
		public const uint GLUT_CURSOR_UP_DOWN		 = 10;
		public const uint GLUT_CURSOR_LEFT_RIGHT		 = 11;
/* Sizing cursors. */
		public const uint GLUT_CURSOR_TOP_SIDE		 = 12;
		public const uint GLUT_CURSOR_BOTTOM_SIDE		 = 13;
		public const uint GLUT_CURSOR_LEFT_SIDE		 = 14;
		public const uint GLUT_CURSOR_RIGHT_SIDE		 = 15;
		public const uint GLUT_CURSOR_TOP_LEFT_CORNER	 = 16;
		public const uint GLUT_CURSOR_TOP_RIGHT_CORNER	 = 17;
		public const uint GLUT_CURSOR_BOTTOM_RIGHT_CORNER	 = 18;
		public const uint GLUT_CURSOR_BOTTOM_LEFT_CORNER	 = 19;
/* Inherit from parent window. */
		public const uint GLUT_CURSOR_INHERIT		 = 100;
/* Blank cursor. */
		public const uint GLUT_CURSOR_NONE		 = 101;
/* Fullscreen crosshair (if available). */
		public const uint GLUT_CURSOR_FULL_CROSSHAIR	 = 102;


/* GLUT initialization sub-API. */
[DllImport("glut")]
static extern void glutInit(ref int argcp, string[] argv);
public static void Init(int argcp, string[] argv)
{
	//glutInit(argcp, argv);
	int argc = argv.Length;
	if (argc == 0) {
		argc = 1;
		string[] aargv = {"monogl"};
		glutInit(ref argc, aargv);
	} else {
		glutInit(ref argc, argv);
	}
}


[DllImport("glut")]
static extern void glutInitDisplayMode(uint mode);
public static void InitDisplayMode(uint mode)
{
	glutInitDisplayMode(mode);
}

[DllImport("glut")]
static extern void glutInitDisplayString(string str);
public static void InitDisplayString(string str)
{
	glutInitDisplayString(str);
}

[DllImport("glut")]
static extern void glutInitWindowPosition(int x, int y);
public static void InitWindowPosition(int x, int y)
{
	glutInitWindowPosition(x, y);
}

[DllImport("glut")]
static extern void glutInitWindowSize(int width, int height);
public static void InitWindowSize(int width, int height)
{
	glutInitWindowSize(width, height);
}

[DllImport("glut")]
static extern void glutMainLoop();
public static void MainLoop()
{
	glutMainLoop();
}


/* GLUT window sub-API. */
[DllImport("glut")]
static extern int glutCreateWindow(string title);
public static int CreateWindow(string title)
{
	return glutCreateWindow(title);
}


[DllImport("glut")]
static extern int glutCreateSubWindow(int win, int x, int y, int width, int height);
public static int CreateSubWindow(int win, int x, int y, int width, int height)
{
	return glutCreateSubWindow(win, x, y, width, height);
}

[DllImport("glut")]
static extern void glutDestroyWindow(int win);
public static void DestroyWindow(int win)
{
	glutDestroyWindow(win);
}

[DllImport("glut")]
static extern void glutPostRedisplay();
public static void PostRedisplay()
{
	glutPostRedisplay();
}

[DllImport("glut")]
static extern void glutPostWindowRedisplay(int win);
public static void PostWindowRedisplay(int win)
{
	glutPostWindowRedisplay(win);
}

[DllImport("glut")]
static extern void glutSwapBuffers();
public static void SwapBuffers()
{
	glutSwapBuffers();
}

[DllImport("glut")]
static extern int glutGetWindow();
public static int GetWindow()
{
	return glutGetWindow();
}

[DllImport("glut")]
static extern void glutSetWindow(int win);
public static void SetWindow(int win)
{
	glutSetWindow(win);
}

[DllImport("glut")]
static extern void glutSetWindowTitle(string title);
public static void SetWindowTitle(string title)
{
	glutSetWindowTitle(title);
}

[DllImport("glut")]
static extern void glutSetIconTitle(string title);
public static void SetIconTitle(string title)
{
	glutSetIconTitle(title);
}

[DllImport("glut")]
static extern void glutPositionWindow(int x, int y);
public static void PositionWindow(int x, int y)
{
	glutPositionWindow(x, y);
}

[DllImport("glut")]
static extern void glutReshapeWindow(int width, int height);
public static void ReshapeWindow(int width, int height)
{
	glutReshapeWindow(width, height);
}

[DllImport("glut")]
static extern void glutPopWindow();
public static void PopWindow()
{
	glutPopWindow();
}

[DllImport("glut")]
static extern void glutPushWindow();
public static void PushWindow()
{
	glutPushWindow();
}

[DllImport("glut")]
static extern void glutIconifyWindow();
public static void IconifyWindow()
{
	glutIconifyWindow();
}

[DllImport("glut")]
static extern void glutShowWindow();
public static void ShowWindow()
{
	glutShowWindow();
}

[DllImport("glut")]
static extern void glutHideWindow();
public static void HideWindow()
{
	glutHideWindow();
}

[DllImport("glut")]
static extern void glutFullScreen();
public static void FullScreen()
{
	glutFullScreen();
}

[DllImport("glut")]
static extern void glutSetCursor(int cursor);
public static void SetCursor(int cursor)
{
	glutSetCursor(cursor);
}

[DllImport("glut")]
static extern void glutWarpPointer(int x, int y);
public static void WarpPointer(int x, int y)
{
	glutWarpPointer(x, y);
}


/* GLUT overlay sub-API. */
[DllImport("glut")]
static extern void glutEstablishOverlay();
public static void EstablishOverlay()
{
	glutEstablishOverlay();
}

[DllImport("glut")]
static extern void glutRemoveOverlay();
public static void RemoveOverlay()
{
	glutRemoveOverlay();
}

[DllImport("glut")]
static extern void glutUseLayer(int layer);
public static void UseLayer(int layer)
{
	glutUseLayer(layer);
}

[DllImport("glut")]
static extern void glutPostOverlayRedisplay();
public static void PostOverlayRedisplay()
{
	glutPostOverlayRedisplay();
}

[DllImport("glut")]
static extern void glutPostWindowOverlayRedisplay(int win);
public static void PostWindowOverlayRedisplay(int win)
{
	glutPostWindowOverlayRedisplay(win);
}

[DllImport("glut")]
static extern void glutShowOverlay();
public static void ShowOverlay()
{
	glutShowOverlay();
}

[DllImport("glut")]
static extern void glutHideOverlay();
public static void HideOverlay()
{
	glutHideOverlay();
}


/* GLUT menu sub-API. */
/*
[DllImport("glut")]
static extern int glutCreateMenu(void (glutCALLBACK *func)(int));
public static int glutCreateMenu(void (CALLBACK *func)(int))
{
	glutCALLBACK *func)(int));
}

[DllImport("glut")]
static extern void glutDestroyMenu(int menu);
public static void DestroyMenu(int menu)
{
	glutDestroyMenu(int menu);
}

[DllImport("glut")]
static extern int glutGetMenu();
public static int GetMenu()
{
	glutGetMenu();
}

[DllImport("glut")]
static extern void glutSetMenu(int menu);
public static void SetMenu(int menu)
{
	glutSetMenu(int menu);
}

[DllImport("glut")]
static extern void glutAddMenuEntry(string label, int value);
public static void AddMenuEntry(string label, int value)
{
	glutAddMenuEntry(string label, int value);
}

[DllImport("glut")]
static extern void glutAddSubMenu(string label, int submenu);
public static void AddSubMenu(string label, int submenu)
{
	glutAddSubMenu(string label, int submenu);
}

[DllImport("glut")]
static extern void glutChangeToMenuEntry(int item, string label, int value);
public static void ChangeToMenuEntry(int item, string label, int value)
{
	glutChangeToMenuEntry(int item, string label, int value);
}

[DllImport("glut")]
static extern void glutChangeToSubMenu(int item, string label, int submenu);
public static void ChangeToSubMenu(int item, string label, int submenu)
{
	glutChangeToSubMenu(int item, string label, int submenu);
}

[DllImport("glut")]
static extern void glutRemoveMenuItem(int item);
public static void RemoveMenuItem(int item)
{
	glutRemoveMenuItem(int item);
}

[DllImport("glut")]
static extern void glutAttachMenu(int button);
public static void AttachMenu(int button)
{
	glutAttachMenu(int button);
}

[DllImport("glut")]
static extern void glutDetachMenu(int button);
public static void DetachMenu(int button)
{
	glutDetachMenu(int button);
}
*/

/* GLUT window callback sub-API. */

public delegate void VoidCB();
public delegate void IntCB(int a);
public delegate void IntIntCB(int a, int b);
public delegate void IntIntIntCB(int a, int b, int c);
public delegate void IntIntIntIntCB(int a, int b, int c, int d);
public delegate void KeyCB(byte key, int x, int y);

[DllImport("glut")]
static extern void glutDisplayFunc(VoidCB func);
public static void DisplayFunc(VoidCB func)
{
	glutDisplayFunc(func);
}

[DllImport("glut")]
static extern void glutReshapeFunc(IntIntCB func);
public static void ReshapeFunc(IntIntCB func)
{
	glutReshapeFunc(func);
}

[DllImport("glut")]
static extern void glutKeyboardFunc(KeyCB func);
public static void KeyboardFunc(KeyCB func)
{
	glutKeyboardFunc(func);
}


[DllImport("glut")]
static extern void glutMouseFunc(IntIntIntIntCB func);
public static void MouseFunc(IntIntIntIntCB func)
{
	glutMouseFunc(func);
}

[DllImport("glut")]
static extern void glutMotionFunc(IntIntCB func);
public static void MotionFunc(IntIntCB func)
{
	glutMotionFunc(func);
}

[DllImport("glut")]
static extern void glutPassiveMotionFunc(IntIntCB func);
public static void PassiveMotionFunc(IntIntCB func)
{
	glutPassiveMotionFunc(func);
}

[DllImport("glut")]
static extern void glutEntryFunc(IntCB func);
public static void EntryFunc(IntCB func)
{
	glutEntryFunc(func);
}

[DllImport("glut")]
static extern void glutVisibilityFunc(IntCB func);
public static void VisibilityFunc(IntCB func)
{
	glutVisibilityFunc(func);
}

[DllImport("glut")]
static extern void glutIdleFunc(VoidCB func);
public static void IdleFunc(VoidCB func)
{
	glutIdleFunc(func);
}

[DllImport("glut")]
static extern void glutTimerFunc(uint millis, IntCB func, int value);
public static void TimerFunc(uint millis, IntCB func, int value)
{
	 glutTimerFunc(millis, func, value);
}


[DllImport("glut")]
static extern void glutMenuStateFunc(IntCB func);
public static void MenuStateFunc(IntCB func)
{
	glutMenuStateFunc(func);
}

[DllImport("glut")]
static extern void glutSpecialFunc(IntIntIntCB func);
public static void SpecialFunc(IntIntIntCB func)
{
	glutSpecialFunc(func);
}

[DllImport("glut")]
static extern void glutSpaceballMotionFunc(IntIntIntCB func);
public static void SpaceballMotionFunc(IntIntIntCB func)
{
	glutSpaceballMotionFunc(func);
}

[DllImport("glut")]
static extern void glutSpaceballRotateFunc(IntIntIntCB func);
public static void SpaceballRotateFunc(IntIntIntCB func)
{
	glutSpaceballRotateFunc(func);
}

[DllImport("glut")]
static extern void glutSpaceballButtonFunc(IntIntCB func);
public static void SpaceballButtonFunc(IntIntCB func)
{
	glutSpaceballButtonFunc(func);
}

[DllImport("glut")]
static extern void glutButtonBoxFunc(IntIntCB func);
public static void ButtonBoxFunc(IntIntCB func)
{
	glutButtonBoxFunc(func);
}

[DllImport("glut")]
static extern void glutDialsFunc(IntIntCB func);
public static void DialsFunc(IntIntCB func)
{
	glutDialsFunc(func);
}

[DllImport("glut")]
static extern void glutTabletMotionFunc(IntIntCB func);
public static void TabletMotionFunc(IntIntCB func)
{
	glutTabletMotionFunc(func);
}

[DllImport("glut")]
static extern void glutTabletButtonFunc(IntIntIntIntCB func);
public static void TabletButtonFunc(IntIntIntIntCB func)
{
	glutTabletButtonFunc(func);
}

[DllImport("glut")]
static extern void glutMenuStatusFunc(IntIntIntCB func);
public static void MenuStatusFunc(IntIntIntCB func)
{
	glutMenuStatusFunc(func);
}

[DllImport("glut")]
static extern void glutOverlayDisplayFunc(VoidCB func);
public static void OverlayDisplayFunc(VoidCB func)
{
	glutOverlayDisplayFunc(func);
}

[DllImport("glut")]
static extern void glutWindowStatusFunc(IntCB func);
public static void WindowStatusFunc(IntCB func)
{
	glutWindowStatusFunc(func);
}

[DllImport("glut")]
static extern void glutKeyboardUpFunc(KeyCB func);
public static void KeyboardUpFunc(KeyCB func)
{
	glutKeyboardUpFunc(func);
}

[DllImport("glut")]
static extern void glutSpecialUpFunc(IntIntIntCB func);
public static void SpecialUpFunc(IntIntIntCB func)
{
	glutSpecialUpFunc(func);
}

[DllImport("glut")]
static extern void glutJoystickFunc(IntIntIntIntCB func, int pollInterval);
public static void JoystickFunc(IntIntIntIntCB func, int pollInterval)
{
	glutJoystickFunc(func, pollInterval);
}



/* GLUT color index sub-API. */
/*
[DllImport("glut")]
static extern void glutSetColor(int, GLfloat red, GLfloat green, GLfloat blue);
public static void SetColor(int, GLfloat red, GLfloat green, GLfloat blue)
{
	glutSetColor(int, GLfloat red, GLfloat green, GLfloat blue);
}

[DllImport("glut")]
static extern GLfloat glutGetColor(int ndx, int component);
public static GLfloat GetColor(int ndx, int component)
{
	glutGetColor(ndx, component);
}

[DllImport("glut")]
static extern void glutCopyColormap(int win);
public static void CopyColormap(int win)
{
	glutCopyColormap(win);
}
*/

/* GLUT state retrieval sub-API. */
[DllImport("glut")]
static extern int glutGet(int type);
public static int Get(int type)
{
	return glutGet(type);
}

[DllImport("glut")]
static extern int glutDeviceGet(int type);
public static int DeviceGet(int type)
{
	return glutDeviceGet(type);
}


/* GLUT extension support sub-API */
[DllImport("glut")]
static extern int glutExtensionSupported(string name);
public static int ExtensionSupported(string name)
{
	return glutExtensionSupported(name);
}

[DllImport("glut")]
static extern int glutGetModifiers();
public static int GetModifiers()
{
	return glutGetModifiers();
}

[DllImport("glut")]
static extern int glutLayerGet(int type);
public static int LayerGet(int type)
{
	return glutLayerGet(type);
}


/* GLUT font sub-API */
/*
[DllImport("glut")]
static extern void glutBitmapCharacter(void *font, int character);
public static void BitmapCharacter(void *font, int character)
{
	glutBitmapCharacter(font, character);
}

[DllImport("glut")]
static extern int glutBitmapWidth(void *font, int character);
public static int BitmapWidth(void *font, int character)
{
	glutBitmapWidth(font, character);
}

[DllImport("glut")]
static extern void glutStrokeCharacter(void *font, int character);
public static void StrokeCharacter(void *font, int character)
{
	glutStrokeCharacter(font, character);
}

[DllImport("glut")]
static extern int glutStrokeWidth(void *font, int character);
public static int StrokeWidth(void *font, int character)
{
	glutStrokeWidth(font, character);
}

[DllImport("glut")]
static extern int glutBitmapLength(void *font, unsigned string str);
public static int BitmapLength(void *font, unsigned string str)
{
	glutBitmapLength(font, str);
}

[DllImport("glut")]
static extern int glutStrokeLength(void *font, unsigned string str);
public static int StrokeLength(void *font, unsigned string str)
{
	glutStrokeLength(font, str);
}
*/

/* GLUT pre-built models sub-API */
[DllImport("glut")]
static extern void glutWireSphere(double radius, int slices, int stacks);
public static void WireSphere(double radius, int slices, int stacks)
{
	glutWireSphere(radius, slices, stacks);
}

[DllImport("glut")]
static extern void glutSolidSphere(double radius, int slices, int stacks);
public static void SolidSphere(double radius, int slices, int stacks)
{
	glutSolidSphere(radius, slices, stacks);
}

[DllImport("glut")]
static extern void glutWireCone(double bse, double height, int slices, int stacks);
public static void WireCone(double bse, double height, int slices, int stacks)
{
	glutWireCone(bse, height, slices, stacks);
}

[DllImport("glut")]
static extern void glutSolidCone(double bse, double height, int slices, int stacks);
public static void SolidCone(double bse, double height, int slices, int stacks)
{
	glutSolidCone(bse, height, slices, stacks);
}

[DllImport("glut")]
static extern void glutWireCube(double size);
public static void WireCube(double size)
{
	glutWireCube(size);
}

[DllImport("glut")]
static extern void glutSolidCube(double size);
public static void SolidCube(double size)
{
	glutSolidCube(size);
}

[DllImport("glut")]
static extern void glutWireTorus(double innerRadius, double outerRadius, int sides, int rings);
public static void WireTorus(double innerRadius, double outerRadius, int sides, int rings)
{
	glutWireTorus(innerRadius, outerRadius, sides, rings);
}

[DllImport("glut")]
static extern void glutSolidTorus(double innerRadius, double outerRadius, int sides, int rings);
public static void SolidTorus(double innerRadius, double outerRadius, int sides, int rings)
{
	glutSolidTorus(innerRadius, outerRadius, sides, rings);
}

[DllImport("glut")]
static extern void glutWireDodecahedron();
public static void WireDodecahedron()
{
	glutWireDodecahedron();
}

[DllImport("glut")]
static extern void glutSolidDodecahedron();
public static void SolidDodecahedron()
{
	glutSolidDodecahedron();
}

[DllImport("glut")]
static extern void glutWireTeapot(double size);
public static void WireTeapot(double size)
{
	glutWireTeapot(size);
}

[DllImport("glut")]
static extern void glutSolidTeapot(double size);
public static void SolidTeapot(double size)
{
	glutSolidTeapot(size);
}

[DllImport("glut")]
static extern void glutWireOctahedron();
public static void WireOctahedron()
{
	glutWireOctahedron();
}

[DllImport("glut")]
static extern void glutSolidOctahedron();
public static void SolidOctahedron()
{
	glutSolidOctahedron();
}

[DllImport("glut")]
static extern void glutWireTetrahedron();
public static void WireTetrahedron()
{
	glutWireTetrahedron();
}

[DllImport("glut")]
static extern void glutSolidTetrahedron();
public static void SolidTetrahedron()
{
	glutSolidTetrahedron();
}

[DllImport("glut")]
static extern void glutWireIcosahedron();
public static void WireIcosahedron()
{
	glutWireIcosahedron();
}

[DllImport("glut")]
static extern void glutSolidIcosahedron();
public static void SolidIcosahedron()
{
	glutSolidIcosahedron();
}


/* GLUT video resize sub-API. */
[DllImport("glut")]
static extern int glutVideoResizeGet(int param);
public static int VideoResizeGet(int param)
{
	return glutVideoResizeGet(param);
}

[DllImport("glut")]
static extern void glutSetupVideoResizing();
public static void SetupVideoResizing()
{
	glutSetupVideoResizing();
}

[DllImport("glut")]
static extern void glutStopVideoResizing();
public static void StopVideoResizing()
{
	glutStopVideoResizing();
}

[DllImport("glut")]
static extern void glutVideoResize(int x, int y, int width, int height);
public static void VideoResize(int x, int y, int width, int height)
{
	glutVideoResize(x, y, width, height);
}

[DllImport("glut")]
static extern void glutVideoPan(int x, int y, int width, int height);
public static void VideoPan(int x, int y, int width, int height)
{
	glutVideoPan(x, y, width, height);
}


/* GLUT debugging sub-API. */
[DllImport("glut")]
static extern void glutReportErrors();
public static void ReportErrors()
{
	glutReportErrors();
}


/* GLUT device control sub-API. */
/* glutSetKeyRepeat modes. */
		public uint GLUT_KEY_REPEAT_OFF		 = 0;
		public uint GLUT_KEY_REPEAT_ON		 = 1;
		public uint GLUT_KEY_REPEAT_DEFAULT		 = 2;

/* Joystick button masks. */
		public uint GLUT_JOYSTICK_BUTTON_A		 = 1;
		public uint GLUT_JOYSTICK_BUTTON_B		 = 2;
		public uint GLUT_JOYSTICK_BUTTON_C		 = 4;
		public uint GLUT_JOYSTICK_BUTTON_D		 = 8;

[DllImport("glut")]
static extern void glutIgnoreKeyRepeat(int ignore);
public static void IgnoreKeyRepeat(int ignore)
{
	glutIgnoreKeyRepeat(ignore);
}

[DllImport("glut")]
static extern void glutSetKeyRepeat(int repeatMode);
public static void SetKeyRepeat(int repeatMode)
{
	glutSetKeyRepeat(repeatMode);
}

[DllImport("glut")]
static extern void glutForceJoystickFunc();
public static void ForceJoystickFunc()
{
	glutForceJoystickFunc();
}


/* GLUT game mode sub-API. */
/* glutGameModeGet. */
		public uint GLUT_GAME_MODE_ACTIVE            = 0;
		public uint GLUT_GAME_MODE_POSSIBLE          = 1;
		public uint GLUT_GAME_MODE_WIDTH             = 2;
		public uint GLUT_GAME_MODE_HEIGHT            = 3;
		public uint GLUT_GAME_MODE_PIXEL_DEPTH       = 4;
		public uint GLUT_GAME_MODE_REFRESH_RATE      = 5;
		public uint GLUT_GAME_MODE_DISPLAY_CHANGED   = 6;

[DllImport("glut")]
static extern void glutGameModeString(string str);
public static void GameModeString(string str)
{
	glutGameModeString(str);
}

[DllImport("glut")]
static extern int glutEnterGameMode();
public static int EnterGameMode()
{
	return glutEnterGameMode();
}

[DllImport("glut")]
static extern void glutLeaveGameMode();
public static void LeaveGameMode()
{
	glutLeaveGameMode();
}

[DllImport("glut")]
static extern int glutGameModeGet(int mode);
public static int GameModeGet(int mode)
{
	return glutGameModeGet(mode);
}

}
}
