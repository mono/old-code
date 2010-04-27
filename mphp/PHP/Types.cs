using System;


namespace PHP {


	public class Exception : System.Exception {

		// exception message
		internal string message;
		// user defined exception code
		internal int code;
		// source filename of exception
		//protected string file;
		// source line of exception
		//protected int line;

		public Exception() : this("Unknown exception", 0) { }

		public Exception(string message) : this(message, 0) { }

		public Exception(string message, int code)
			: base(message) {
			this.message = message;
			this.code = code;
		}

		// message of exception
		public string getMessage() {
			return message;
		}

		// code of exception
		public int getCode() {
			return code;
		}

		// TODO: source filename
		public string getFile() {
			Report.Error(900, "getFile()");
			return null;
		}

		// TODO: source line
		public int getLine() {
			Report.Error(900, "getLine()");
			return 0;
		}

		// TODO: an array of the backtrace()
		public Array getTrace() {
			Report.Error(900, "getTrace()");
			return null;
		}

		// TODO: formated string of trace
		public string getTraceAsString() {
			Report.Error(900, "getTraceAsString()");
			return null;
		}

		// formated string for display
		public virtual string __toString() {
			// TODO: add file and stack trace
			return GetType().FullName + ": " + message + "";
		}

		// formated string for display
		public override string ToString() {
			return __toString();
		}

	}

}