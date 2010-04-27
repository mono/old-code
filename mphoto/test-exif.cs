//
// test-exif.cs : Sample app to test our LibExif wrapper
//
// Author:
//   Ravi Pratap     (ravi@ximian.com)
// (C) 2002 Ximian, Inc.
//

using System;

public class TextExif {

	public static int Main (String [] args)
	{
		if (args.Length != 1) {
			Console.WriteLine ("Usage : text-exif [filename]");
			return 1;
		}
		
		string filename = args [0];

		ExifData ed = new ExifData (filename);

		ed.Assemble ();
		return 0;
	}
}
