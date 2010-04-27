using System;

namespace Xnb
{
	public enum Gravity
	{
		//protected?
		Forget,
			NorthWest,
			North,
			NorthEast,
			West,
			Center,
			East,
			SouthWest,
			South,
			SouthEast,
			Static,
	}

	public enum HostFamily
	{
		Internet,
			DECnet,
			Chaos,
			//FIXME
			Unused0,
			Unused1,
			ServerInterpreted,
			InternetV6,
	}

	/*
		 0x42 MSB first
		 0x6C LSB first

		 public enum ByteOrder
		 {
		 LSBFirst,
		 MSBFirst,
		 }

		 public enum ByteOrder
		 {
		 LeastSignificant,
		 MostSignificant,
		 }
		 */

	public enum BackingStoreUsage
	{
		Never,
			NotUseful = Never,
			WhenMapped,
			Always,
	}

	public enum VisualType
	{
		StaticGray,
			//formerly GrayScale
			Grayscale,
			StaticColor,
			PseudoColor,
			TrueColor,
			DirectColor,
	}

	//also for PIXMAP, so DrawableClass?
	public enum WindowClass : ushort
	{
		CopyFromParent,
			//ParentRelative = CopyFromParent,
			InputOutput,
			InputOnly,
	}

	/*
	 public enum WindowFOO : ushort
		 {
		 None,
		 ParentRelative,
		 }
		 */

	public enum MapState
	{
		Unmapped,
			Unviewable,
			Viewable,
	}

	public enum SaveSetMode : byte
	{
		Insert,
			Delete,
	}

	/*

	//from xfixes.xml
	public enum SaveSetTarget
	{
	Nearest,
	Root,
	}
	*/

	public enum StackMode
	{
		Above,
			Below,
			TopIf,
			BottomIf,
			Opposite,
	}

	public enum CirculateDirection : byte
	{
		RaiseLowest,
			LowerHighest,
	}

	public enum PropertyMode : byte
	{
		Replace,
			Prepend,
			Append,
	}

	public enum EventDestination
	{
		PointerWindow,
			InputFocus,
	}

	public enum GrabMode
	{
		Synchronous,
			Asynchronous,
	}

	public enum GrabStatus
	{
		Success,
			AlreadyGrabbed,
			InvalidTime,
			NotViewable,
			Frozen,
	}

	/*
		 {
		 AsyncPointer,
		 SyncPointer,
		 ReplayPointer,
		 AsyncKeyboard,
		 SyncKeyboard,
		 ReplayKeyboard,
		 AsyncBoth,
		 SyncBoth,
		 }
		 */

	public enum FocusRevert
	{
		None,
			PointerRoot,
			Parent,
	}

	//DrawDirection
	public enum Direction
	{
		LeftToRight,
			RightToLeft,
	}

	public enum LineStyle
	{
		Solid,
			OnOffDash,
			DoubleDash,
	}

	public enum CapStyle
	{
		NotLast,
			Butt,
			Round,
			Projecting,
	}

	public enum JoinStyle
	{
		Miter,
			Round,
			Bevel,
	}

	public enum FillStyle
	{
		Solid,
			Tiled,
			Stippled,
			OpaqueStippled,
	}

	public enum FillRule
	{
		EvenOdd,
			Winding,
	}

	public enum SubwindowMode
	{
		ClipByChildren,
			IncludeInferiors,
	}

	public enum ArcMode
	{
		Chord,
			PieSlice,
	}

	public enum Ordering
	{
		UnSorted,
			YSorted,
			YXSorted,
			YXBanded,
	}

	/*
		 public enum CoordinateMode
		 {
		 Origin,
		 Previous,
		 }
		 */

	//names refactored
	public enum CoordinateMode
	{
		Absolute,
			Relative,
	}

	public enum ShapeType
	{
		Complex,
			Nonconvex,
			Convex,
	}

	public enum ImageFormat
	{
		Bitmap,
			XYPixmap,
			ZPixmap,
	}

	public enum Allocation
	{
		None,
			All,
	}

	public enum QueryClass
	{
		Cursor,
			Tile,
			Stipple,
	}

	/*
		 public enum led-mode/toggle
		 {
		 Off,
		 On,
		 Default,
		 }

		 public enum Blanking
		 {
		 No,
		 Yes,
		 Default,
		 }

		 {
		 Disabled,
		 Enabled,
		 }

		 {
		 Disable,
		 Enable,
		 }
		 */

	public enum CloseDownMode
	{
		Destroy,
			RetainPermanent,
			RetainTemporary,
	}

	public enum ForceMode
	{
		Reset,
			Activate,
	}

	public enum Success
	{
		Busy,
			Failed,
	}

	public enum NotifyType
	{
		Normal,
			Hint,
	}

	//TODO: event DETAILS!

	//names refactored
	public enum Visibility
	{
		Full,
			Partial,
			None,
	}

	/*
		 public enum VisibilityState
		 {
		 Unobscured,
		 PartiallyObscured,
		 FullyObscured,
		 }
		 */

	public enum ColormapState
	{
		Uninstalled,
			Installed,
	}

	public enum MappingRequestType
	{
		Modifer,
			Keyboard,
			Pointer,
	}

	public enum Place
	{
		Top,
			Bottom,
	}

	//GCFunction?
	public enum GraphicsFunction
	{
		Clear,
			And,
			AndReverse,
			Copy,
			AndInverted,
			NoOp,
			Xor,
			Or,
			Nor,
			Equiv,
			Invert,
			OrReverse,
			CopyInverted,
			OrInverted,
			Nand,
			Set,
	}

	//masks, until they are done more intelligently
	//seems to be XA_PRIMARY << value_index
	[Flags]
		public enum WindowValueMask : uint
		{
			BackgroundPixmap = 1 << 0,
											 BackgroundPixel = 1 << 1,
											 BorderPixmap = 1 << 2,
											 BorderPixel = 1 << 3,
											 BitGravity = 1 << 4,
											 WinGravity = 1 << 5,
											 BackingStore = 1 << 6,
											 BackingPlanes = 1 << 7,
											 BackingPixel = 1 << 8,
											 //FIXME: docs have an extra (incorrect?) entry here
											 OverrideRedirect = 1 << 9,
											 SaveUnder = 1 << 10,
											 EventMask = 1 << 11,
											 DoNotPropagateMask = 1 << 12,
											 Colormap = 1 << 13,
											 Cursor = 1 << 14,
		}


	//TODO: this is probably not the right place/way to do this
	//XA_
	public enum AtomType {
		AnyPropertyType,
			PRIMARY,
			SECONDARY,
			ARC,
			ATOM,
			BITMAP,
			CARDINAL,
			COLORMAP,
			CURSOR,
			CUT_BUFFER0,
			CUT_BUFFER1,
			CUT_BUFFER2,
			CUT_BUFFER3,
			CUT_BUFFER4,
			CUT_BUFFER5,
			CUT_BUFFER6,
			CUT_BUFFER7,
			DRAWABLE,
			FONT,
			INTEGER,
			PIXMAP,
			POINT,
			RECTANGLE,
			RESOURCE_MANAGER,
			RGB_COLOR_MAP,
			RGB_BEST_MAP,
			RGB_BLUE_MAP,
			RGB_DEFAULT_MAP,
			RGB_GRAY_MAP,
			RGB_GREEN_MAP,
			RGB_RED_MAP,
			STRING,
			VISUALID,
			WINDOW,
			WM_COMMAND,
			WM_HINTS,
			WM_CLIENT_MACHINE,
			WM_ICON_NAME,
			WM_ICON_SIZE,
			WM_NAME,
			WM_NORMAL_HINTS,
			WM_SIZE_HINTS,
			WM_ZOOM_HINTS,
			MIN_SPACE,
			NORM_SPACE,
			MAX_SPACE,
			END_SPACE,
			SUPERSCRIPT_X,
			SUPERSCRIPT_Y,
			SUBSCRIPT_X,
			SUBSCRIPT_Y,
			UNDERLINE_POSITION,
			UNDERLINE_THICKNESS,
			STRIKEOUT_ASCENT,
			STRIKEOUT_DESCENT,
			ITALIC_ANGLE,
			X_HEIGHT,
			QUAD_WIDTH,
			WEIGHT,
			POINT_SIZE,
			RESOLUTION,
			COPYRIGHT,
			NOTICE,
			FONT_NAME,
			FAMILY_NAME,
			FULL_NAME,
			CAP_HEIGHT,
			WM_CLASS,
			WM_TRANSIENT_FOR,

			LAST_PREDEFINED	= WM_TRANSIENT_FOR,
	}
}
