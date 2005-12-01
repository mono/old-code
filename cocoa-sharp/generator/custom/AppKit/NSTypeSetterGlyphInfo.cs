
using System;
using Apple.Foundation;

namespace Apple.AppKit {
	public struct NSTypesetterGlyphInfo {
		NSPoint curLocation;
		float extent;
		float belowBaseline;
		float aboveBaseline;
		int glyphCharacterIndex;
		IntPtr font;
		NSSize attachmentSize;
/*
How are we gonna handle bitpacking???
		struct {
		BOOL defaultPositioning:1;
		BOOL dontShow:1;
		BOOL isAttachment:1;
		} _giflags;
*/
	}
}
