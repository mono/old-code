using System;
using System.Collections;


namespace PHP {


	public class Report {

		public static bool WarningsEnabled = true;
		public static int NumberOfErrors = 0;
		public static int NumberOfWarnings = 0;
		public const int ERROR = 0;
		public const int WARNING = 1;
		public const int EXCEPTION = 2;

		public static void Error(string options) {
			Process(ERROR, -1, "", -1, -1);
		}

		public static void Error(int nr) {
			Process(ERROR, nr, "", -1, -1);
		}

		public static void Error(int nr, string options) {
			Process(ERROR, nr, options, -1, -1);
		}

		public static void Error(int nr, int line, int column) {
			Process(ERROR, nr, "", line, column);
		}

		public static void Error(int nr, string options, int line, int column) {
			Process(ERROR, nr, options, line, column);
		}

		public static void Warn(string options) {
			Process(WARNING, -1, "", -1, -1);
		}

		public static void Warn(int nr) {
			Process(WARNING, nr, "", -1, -1);
		}

		public static void Warn(int nr, string options) {
			Process(WARNING, nr, options, -1, -1);
		}

		public static void Warn(int nr, int line, int column) {
			Process(WARNING, nr, "", line, column);
		}

		public static void Warn(int nr, string options, int line, int column) {
			Process(WARNING, nr, options, line, column);
		}

		public static Exception Exception(string options) {
			return Process(EXCEPTION, -1, "", -1, -1);
		}

		public static Exception Exception(int nr) {
			return Process(EXCEPTION, nr, "", -1, -1);
		}

		public static Exception Exception(int nr, string options) {
			return Process(EXCEPTION, nr, options, -1, -1);
		}

		public static Exception Exception(int nr, int line, int column) {
			return Process(EXCEPTION, nr, "", line, column);
		}

		public static Exception Exception(int nr, string options, int line, int column) {
			return Process(EXCEPTION, nr, options, line, column);
		}

		public static Exception Process(int kind, int nr, string options, int line, int column) {
			// count errors and warnings
			if (kind == Report.ERROR)
				NumberOfErrors++;
			if (kind == Report.WARNING)
				NumberOfWarnings++;
			// create text about kind
			string kindString = null;
			if (kind == ERROR)
				kindString = "Error ";
			else if (kind == WARNING)
				kindString = "Warning ";
			else if (kind == EXCEPTION)
				kindString = "Exception ";
			// create text about line/column, if supplied
			string location = "";
			if (line != -1 && column != -1)
				location = " (" + line + "," + column + ")";
			// create message
			string intro = kindString + nr + location + ": ";
			string msg;
			switch (nr) {
				case 000: msg = "No file to compile was specified"; break;
				case 001: msg = "Cannot open source file " + options; break;
				case 002: msg = "Cannot write to output file " + options; break;
				case 004: msg = "The option " + options + " is unknown"; break;
				case 005: msg = "The target " + options + " is unknown"; break;
				case 006: msg = "The assembly " + options + " cannot be found"; break;
				case 007: msg = "The file " + options + " is not a valid assembly"; break;
				case 100: msg = "Error in inheritance - missing parent declaration or cycle"; break;
				case 101: msg = "Cannot extend final class " + options; break;
				case 102: msg = "Cannot override final function " + options; break;
				case 103: msg = "The modifier " + options + " is not allowed for class members"; break;
				case 104: msg = "The modifier " + options + " is not allowed for constructors"; break;
				case 105: msg = "Multiple access type modifiers are not allowed"; break;
				case 106: msg = "The class " + options + " contains abstract functions and must therefore be declared abstract"; break;
				case 107: msg = "The abstract function " + options + " cannot override a function at all"; break;
				case 108: msg = "The function " + options + " must have same or weaker visibility as the overridden function"; break;
				case 109: msg = "The class " + options + " must override all inherited abstract functions"; break;
				case 110: msg = "The interface function " + options + " cannot override a function at all"; break;
				case 111: msg = "The modifier " + options + " is not allowed for interface functions"; break;
				case 112: msg = "The interface function " + options + " cannot contain a body"; break;
				case 113: msg = "The abstract function " + options + " cannot contain a body"; break;
				case 114: msg = "Interfaces cannot include class member variables"; break;
				case 115: msg = "The non-abstract function " + options + " must contain a body"; break;
				case 116: msg = "The class " + options + " must implement all functions inherited from interfaces"; break;
				case 117: msg = "The function " + options + " has already been declared by an implemented interface"; break;
				case 118: msg = "Ambigious constructor call"; break;
				case 119: msg = "Ambigious function call"; break;
				case 120: msg = "The namespace " + options + " cannot be assigned an alias"; break;
				case 121: msg = "The class name " + options + " is ambigious"; break;
				case 122: msg = "An object of type " + options + " was returned where a Reference is expected"; break;
				case 123: msg = "Cannot throw object of type " + options; break;
				case 200: msg = "Syntax error in input: " + options; break;
				case 201: msg = "The symbol " + options + " is a reserved word and cannot be declared"; break;
				case 202: msg = "The class " + options + " has already been declared"; break;
				case 203: msg = "The class " + options + " cannot be found"; break;
				case 204: msg = "The class member " + options + " has already been declared"; break;
				case 205: msg = "The class member " + options + " cannot be found"; break;
				case 206: msg = "The class member " + options + " is not visible"; break;
				case 208: msg = "The class member " + options + " is not static"; break;
				case 209: msg = "The static class member " + options + " cannot be found"; break;
				case 210: msg = "The static class member " + options + " is not visible"; break;
				case 211: msg = "The function " + options + " has already been declared"; break;
				case 212: msg = "The function " + options + " cannot be found"; break;
				case 213: msg = "The function " + options + " is not visible"; break;
				case 215: msg = "The function " + options + " is not static"; break;
				case 216: msg = "The static function " + options + " cannot be found"; break;
				case 217: msg = "The static function " + options + " is not visible"; break;
				case 218: msg = "The variable " + options + " cannot be found"; break;
				case 219: msg = "The constant " + options + " has already been defined"; break;
				case 220: msg = "The interface " + options + " has already been declared"; break;
				case 221: msg = "The constructor for class " + options + " cannot be found"; break;
				case 222: msg = "The event " + options + " cannot be found"; break;
				case 223: msg = "The enumeration member " + options + " cannot be found"; break;
				case 224: msg = "Cannot add an event to a null object"; break;
				case 225: msg = "A delegate is required to add an event"; break;
				case 226: msg = "The class member " + options + " cannot be read"; break;
				case 227: msg = "The class member " + options + " cannot be written"; break;
				case 228: msg = "The class or namespace " + options + " cannot be found"; break;
				case 300: msg = "Missing parameter " + options; break;
				case 301: msg = "Parameter " + options + " cannot be passed by reference"; break;
				case 302: msg = "Parameter " + options + " has the wrong type"; break;
				case 303: msg = "Assigning the return value of new by reference is deprecated and will be ignored"; break;
				case 304: msg = "Cannot convert to type " + options; break;
				case 305: msg = "The function " + options + " cannot be referenced as it does not return a reference"; break;
				case 306: msg = "The expression cannot be referenced"; break;
				case 400: msg = "The constant " + options + " may only evaluate to a scalar value"; break;
				case 401: msg = "The specified data type is not an array but of type " +  options + " and can therefore not be accessed by offset"; break;
				case 402: msg = "The specified key is of type " + options + " instead of int or string and can therefore not be used"; break;
				case 403: msg = "Trying to store to class member of a non-object type"; break;
				case 404: msg = "Trying to get class member of a non-object type"; break;
				case 405: msg = "Trying to invoke from class member of a non-object type"; break;
				case 406: msg = "A function call cannot be used as an argument for foreach: " + options; break;
				case 407: msg = "The argument supplied for foreach is not an array but of type " + options + " and can therefore not be used"; break;
				case 408: msg = "Cannot use a function return value in write context"; break;
				case 409: msg = "Cannot instantiate abstract class " + options; break;
				case 410: msg = "Cannot remove class member " + options; break;
				case 500: msg = "Unsupported operand types when using operator " + options; break;
				case 501: msg = "Break/Continue without a loop"; break;
				case 502: msg = "Using $this in a static function will only succeed when called from a non-static context"; break;
				case 503: msg = "Cannot access parent:: because current class doesn't inherit from a class"; break;
				case 504: msg = "Invalid number of levels to break"; break;
				case 900: msg = "The language element " + options + " is not supported in the current version of mPHP"; break;
				default: msg = "Unknown"; break;
			}
			if (kind == ERROR) {
				Console.WriteLine(intro + msg);
				// report fail and exit, if it is not a usage error
				if (nr == -1 || nr >= 100) {
					string fail = "Compiling failed: ";
					if (Report.NumberOfErrors == 1)
						fail += Report.NumberOfErrors + " error, ";
					else
						fail += Report.NumberOfErrors + " errors, ";
					if (Report.NumberOfWarnings == 1)
						fail += Report.NumberOfWarnings + " warning";
					else
						fail += Report.NumberOfWarnings + " warnings";
					Console.WriteLine(fail);
				}
				Environment.Exit(0);
			}
			else if (kind == WARNING && WarningsEnabled) {
				Console.WriteLine(intro + msg);
			}
			else if (kind == EXCEPTION)
				return new PHP.Exception(msg, nr);
			return null;
		}

	}


}