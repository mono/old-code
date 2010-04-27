// SafeFileSerializer.cs -- serializes objects to files with some safeguards

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Mono.Build;

namespace Monkeywrench {

	public class SafeFileSerializer {

		protected const string newext = ".new";
		protected const string oldext = ".old";

		private SafeFileSerializer () {}

		protected static void prereqs (string path) {
			// file.new should never exist if serialization
			// completes successfully. If it does exist,
			// that means something went wrong last time.

			if (File.Exists (path + newext))
				File.Delete (path + newext);

			// file.old should only exist if we get a bizarre
			// power failure or something in the middle
			// of our filesystem ops. If file.old exists,
			// file should never exist ... if it does, we assume
			// that file is loadable and nuke file.old so that
			// we can serialize later. If not, move it 
			// back to file so we can deserialize later.

			if (File.Exists (path + oldext)) {
				if (File.Exists (path))
					File.Delete (path + oldext);
				else
					File.Move (path + oldext, path);
			}
		}
			
		public static object Load (string path, IWarningLogger log) {
			prereqs (path);

			if (!File.Exists (path))
				return null;

			object result = null;

			StreamingContext ctxt = new StreamingContext (StreamingContextStates.All);
			BinaryFormatter fmt = new BinaryFormatter (null, ctxt);

			try {
                                using (FileStream stream = new FileStream (path, FileMode.Open, FileAccess.Read)) {
                                        result = fmt.Deserialize (stream);
                                }
			} catch (Exception e) {
				if (log != null)
					log.Warning (1005, "Error recovering data from " + path, e.Message);
				else
					Console.Error.WriteLine ("Unable to log error loading log; you can " +
								 "probably ignore this message: {0}", e.Message);
				return null;
			}

			return result;
		}

		// return true on error
		public static bool Save (string path, object obj, IWarningLogger log) {
			prereqs (path);

			StreamingContext ctxt = new StreamingContext (StreamingContextStates.All);
			BinaryFormatter fmt = new BinaryFormatter (null, ctxt);
			
			try {
				using (FileStream stream = new FileStream (path + newext, FileMode.Create, FileAccess.Write)) {
					fmt.Serialize (stream, obj);
				}

				bool exists = File.Exists (path);

				if (exists)
					File.Move (path, path + oldext);

				File.Move (path + newext, path);

				if (exists)
					File.Delete (path + oldext);
			} catch (Exception e) {
				log.Warning (1006, "Error writing data to " + path + newext, e.Message);
				File.Delete (path + newext);
				return true;
			}

			return false;
		}
	}
}
