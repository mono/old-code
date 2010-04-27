// SimpleLogger.cs -- a simple IWarningLogger

using System;
using System.Collections;
using System.Runtime.Serialization;
using Mono.Build;

namespace Monkeywrench {

	[Serializable]
	public abstract class SimpleLogger : IWarningLogger {

		public SimpleLogger () { }

		[NonSerialized] int num_warnings = 0;

		public int NumWarnings {
			get { return num_warnings; }
		}

		[NonSerialized] int num_errors = 0;

		public int NumErrors {
			get { return num_errors; }
		}

		[NonSerialized] ArrayList locations;

		public string Location {
			get {
				if (locations == null || locations.Count < 1)
					return null;
				return (string) locations[locations.Count - 1];
			}
		}

		protected abstract void DoWarning (string location, int category,
						   string text, string detail);

		protected abstract void DoError (string location, int category,
						 string text, string detail);

		public void PushLocation (string location) {
			if (locations == null)
				locations = new ArrayList ();
			locations.Add (location);
		}
 
		public void PopLocation () {
			if (locations == null || locations.Count < 1) {
				Warning (1011, "Unequal numbers of location push/pops", null);
				return;
			}
			locations.RemoveAt (locations.Count - 1);
		}
 
		public void Warning (int category, string text, string detail) {
			num_warnings++;
			DoWarning (Location, category, text, detail);
		}
 
		public void Error (int category, string text, string detail) {
			num_errors++;
			DoError (Location, category, text, detail);
		}
	}
}
