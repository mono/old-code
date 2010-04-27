namespace Mono.Languages.Logo.Runtime {
	using System.Collections;

	public class LogoContext {
		private class NullVariableSlot {
		}

		private Hashtable dict = null;
		private LogoContext parent = null;
		private object engine = null;
		private bool stop_execution = false;
		private object output_value = null;
		private static object null_variable = null;

		public Hashtable Dict {
			get {
				if (dict == null)
					dict = new Hashtable (new CaseInsensitiveHashCodeProvider (), new CaseInsensitiveComparer ());
				
				return dict;
			}
		}

		public LogoContext Parent {
			get {
				return parent;
			}
		}

		public LogoContext RootContext {
			get {
				LogoContext context = this;
				while (context.Parent != null) {
					context = context.Parent;
				}

				return context;
			}
		}

		public object CallingEngine {
			get { return engine; }
			set { engine = value; }
		}

		public bool StopExecution {
			get { return stop_execution; }
		}

		public object OutputValue {
			get { return output_value; }
		}

		public void Output (object val) {
			output_value = val;
			stop_execution = true;
		}

		public static object NullVariable {
			get {
				if (null_variable == null)
					null_variable = new NullVariableSlot ();
				return null_variable;
			}
		}

		public LogoContext (LogoContext parent) {
			this.parent = parent;
		}
	}
}

