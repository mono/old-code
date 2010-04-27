namespace Ewl {
	using System;
	using System.Runtime.InteropServices;

	public delegate void PrivEventHandler(IntPtr w, IntPtr evnt, IntPtr data);
	public delegate void EwlEventHandler(Widget w, object event_data);
		
namespace Event{

	[StructLayout(LayoutKind.Sequential)]
	public class DataKeyDown{
		uint modifiers;
		string keyname;
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public class DataKeyUp{
		uint modifiers;
		string keyname;
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public class DataWindowExpose{
		int x;
		int y;
		int w;
		int h;
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public class DataWindowConfigure{
		int x;
		int y;
		int w;
		int h;
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public class DataMouseDown{
		uint modifiers;
		int button;
		int clicks;
		int x;
		int y;
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public class DataMouseUp{
		uint modifiers;
		int button;
		int x;
		int y;
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public class DataMouseMove{
		uint modifiers;
		int x;
		int y;
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public class DataMouseIn{
		uint modifiers;
		int x;
		int y;
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public class DataMouseOut{
		uint modifiers;
		int x;
		int y;
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public class DataMouseWheel{
		uint modifiers;
		int x;
		int y;
		int z;
		int dir;
	}
}
}