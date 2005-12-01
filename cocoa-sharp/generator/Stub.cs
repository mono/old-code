//
// $Id: Stub.cs,v 1.7 2004/09/11 00:41:22 urs Exp $
//

using System;

namespace CocoaSharp {

	public class Test {

		static void Main (string [] args) {
#if HEADER
			ObjCManagedExporter exporter = new ObjCManagedExporter(args);
			exporter.Run();
#else
			foreach (string arg in args)
				new MachOFile(arg);
#endif
			foreach (MachOType t in MachOFile.Types.Values)
				if (t.fields.Length == 0)
					MachOFile.DebugOut(1,"undef {0}",t.ToString());
		}
	}
}
