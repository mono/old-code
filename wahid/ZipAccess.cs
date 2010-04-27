using System;
using System.IO;

namespace Wahid {

	//
	// A simple wrapper, so we can have multiple loaders later
	//
	public abstract class ZipAccess {

		public ZipAccess ()
		{
		}

		public abstract Stream Get (string resource);
	}
}
