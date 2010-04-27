/*
	EWL Enumerations
	
	TODO: A lot :)
*/

namespace Enlightenment.Ewl {
   	using System;
   
   	public class EwlFlags
	{
		const int ALIGN_CENTER = 0;
		const int ALIGN_LEFT = 0x1;
		const int ALIGN_RIGHT = 0x2;
		const int ALIGN_TOP = 0x4;
		const int ALIGN_BOTTOM = 0x8;
		const int FILL_NONE = 0;
		const int FILL_HSHRINK = 0x10;
		const int FILL_VSHRINK = 0x20;
		const int FILL_SHRINK = (0x20 + 1);
		const int FILL_HFILL = 0x40;
		const int FILL_VFILL = 0x80;
		const int FILL_FILL = FILL_HFILL | FILL_VFILL;
		const int FILL_ALL = FILL_FILL | FILL_SHRINK;
    }
	
	public class EventType
	{
		public const int EXPOSE = 0;
		public const int REALIZE = 1;
		public const int UNREALIZE = 2;
		public const int SHOW = 3;
		public const int HIDE = 4;
		public const int DESTROY = 5;
		public const int DELETE_WINDOW = 6;
		public const int CONFIGURE = 7;
		public const int REPARENT = 8;
		public const int KEY_DOWN = 9;
		public const int KEY_UP = 10;
		public const int MOUSE_DOWN = 11;
		public const int MOUSE_UP = 12;
		public const int MOUSE_MOVE = 13;
		public const int MOUSE_WHEEL = 14;
		public const int FOCUS_IN = 15;
		public const int FOCUS_OUT = 16;
		public const int SELECT = 17;
		public const int DESELECT = 18;
		public const int CLICKED = 19;
		public const int DOUBLE_CLICKED = 20;
		public const int HILITED = 21;
		public const int VALUE_CHANGED = 22;
		public const int STATE_CHANGED = 23;
		public const int APPEARANCE_CHANGED = 24;
		public const int WIDGET_ENABLE = 25;
		public const int WIDGET_DISABLE = 26;
		public const int PASTE = 27;
		public const int MAX = 28;
	};
	
}
