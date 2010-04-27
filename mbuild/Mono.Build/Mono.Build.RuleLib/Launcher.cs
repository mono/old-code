//
// Launcher.cs -- class for running external programs in MBuild framework
//

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading; // !!!

using Mono.Build;

namespace Mono.Build.RuleLib {

	public class Launcher {

		private Launcher () { }

		static ProcessStartInfo PrepareInfo (BinaryInfo info, string extra_args, 
						     IBuildContext ctxt) {
			ProcessStartInfo si = info.MakeInfo (ctxt);

			if (extra_args != null)
				si.Arguments += " " + extra_args;

			// tools should not spew to console or want user input

			si.UseShellExecute = false;
			si.RedirectStandardInput = true;
			si.RedirectStandardOutput = true;
			si.RedirectStandardError = true;

			return si;
		}

		public class OutputReader {
			TextReader input;
			TextWriter output;
			Thread thread;
			char[] buf;

			public OutputReader (TextReader input, TextWriter output) {
				this.input = input;
				this.output = output;
				this.buf = new char[512];
				this.thread = new Thread (new ThreadStart (Worker));
			}

			public static OutputReader Launch (StreamReader input, TextWriter output) {
				OutputReader reader = new OutputReader (StreamReader.Synchronized (input), 
									TextWriter.Synchronized (output));

				reader.thread.Start ();

				return reader;
			}

			public void Join () {
				thread.Join ();
			}

			void Worker() {
				int read;

				while (true) {
					//Console.WriteLine ("* before read");
					read = input.Read (buf, 0, buf.Length);
					//Console.WriteLine ("* transferring {0} chars from process", read);

					if (read < 1)
						break;

					//Console.WriteLine ("* before stdout write");
					output.Write (buf, 0, read);
					//Console.WriteLine ("* after stdout write");
				}

				output.Close ();
				output = null;
				input = null;
				buf = null;
			}
		}

		public static int Start (BinaryInfo info, string extra_args, TextReader stdin, 
					 TextWriter stdout, TextWriter stderr, IBuildContext ctxt) {
			ProcessStartInfo si = PrepareInfo (info, extra_args, ctxt);

			string command = si.FileName + " " + si.Arguments;
			if (command.Length > 511)
				ctxt.Logger.Warning (3002, "Command line is too long for Windows shell", command);

			ctxt.Logger.Log ("launcher.launch", command);
			Process ps = Process.Start (si);

			bool capture_stdout = (stdout == null);
			bool capture_stderr = (stderr == null);

			// Transfer data from our TextReader/Writers to the process
			// FIXME: Do we really have to spawn new threads? That totally bites!
			// (Before I had a loop that would read/write from std{in,out,err}, but I
			// would get locks as mbuild would want to write to one pipe while
			// the child (jay) wanted to write to another. Sigh.

			OutputReader stdout_reader = null;
			OutputReader stderr_reader = null;

			if (stdout != null)
				stdout_reader = OutputReader.Launch (ps.StandardOutput, stdout);
			if (stderr != null)
				stderr_reader = OutputReader.Launch (ps.StandardError, stderr);

			if (stdin == null)
				ps.StandardInput.Close ();
			else {
				char[] buf = new char[512];
				int read;

				do {
					//Console.WriteLine ("* before stdin read");
					read = stdin.Read (buf, 0, buf.Length);
					//Console.WriteLine ("* transferring {0} chars to process stdin", read);

					//System.Text.StringBuilder sb = new System.Text.StringBuilder ();
					//sb.Append (buf, 0, read);
					//Console.WriteLine ("[{0}]", sb.ToString ());

					if (read > 0) {
						//Console.WriteLine ("* before stdin write");
						ps.StandardInput.Write (buf, 0, read);
						//Console.WriteLine ("* after stdin write");
					} else {
						ps.StandardInput.Close ();
						stdin = null;
					}
				} while (read > 0);
			}

			ps.WaitForExit ();

			if (stdout_reader != null)
				stdout_reader.Join ();
			if (stderr_reader != null)
				stderr_reader.Join ();

			// Cleanup -- if not dumping stdout and stderr to a file,
			// then log them

			string data;

			if (capture_stdout) {
				data = ps.StandardOutput.ReadToEnd ().Trim ();
				if (data.Length > 0)
					ctxt.Logger.Log ("launcher.stdout", data);
			}

			if (capture_stderr) {
				data = ps.StandardError.ReadToEnd ().Trim ();
				if (data.Length > 0)
					ctxt.Logger.Log ("launcher.stderr", data);
			}

			ctxt.Logger.Log ("launcher.exit", ps.ExitCode.ToString ());
			return ps.ExitCode;
		}

		public static int Start (BinaryInfo info, string extra_args, MBFile stdin, 
					 MBFile stdout, MBFile stderr, IBuildContext ctxt) {
			StreamReader stdin_stream = null;
			StreamWriter stdout_stream = null;
			StreamWriter stderr_stream = null;

			if (stdin != null) {
				stdin_stream = new StreamReader (stdin.OpenRead (ctxt));
				ctxt.Logger.Log ("launcher.stdin_from", stdin.GetPath (ctxt));
			}

			if (stdout != null) {
				stdout_stream = new StreamWriter (stdout.OpenWrite (ctxt));
				ctxt.Logger.Log ("launcher.stdout_to", stdout.GetPath (ctxt));
			}

			if (stderr != null) {
				stderr_stream = new StreamWriter (stderr.OpenWrite (ctxt));
				ctxt.Logger.Log ("launcher.stderr_to", stderr.GetPath (ctxt));
			}

			return Start (info, extra_args, stdin_stream, stdout_stream,
				      stderr_stream, ctxt);
		}

		public static int Start (BinaryInfo info, string extra_args, MBFile stdin, 
					 MBFile stdout, out string stderr, IBuildContext ctxt) {
			MemoryStream ms = new MemoryStream (32);
			StreamReader stdin_stream = null;
			StreamWriter stdout_stream = null;
			StreamWriter stderr_stream = null;
			int exit_code;

			if (stdin != null) {
				stdin_stream = new StreamReader (stdin.OpenRead (ctxt));
				ctxt.Logger.Log ("launcher.stdin_from", stdin.GetPath (ctxt));
			}

			if (stdout != null) {
				stdout_stream = new StreamWriter (stdout.OpenWrite (ctxt));
				ctxt.Logger.Log ("launcher.stdout_to", stdout.GetPath (ctxt));
			}

			stderr_stream = new StreamWriter (ms);

			exit_code = Start (info, extra_args, stdin_stream, stdout_stream,
					   stderr_stream, ctxt);

			byte[] buf = ms.ToArray ();
			stderr = System.Text.Encoding.Default.GetString (buf).Trim ();

			return exit_code;
		}

		public static int Start (BinaryInfo info, string extra_args, TextReader stdin,
					 IMiniStreamSink stdout, IMiniStreamSink stderr, IBuildContext ctxt) {
			StreamWriter stdout_stream = null;
			StreamWriter stderr_stream = null;

			if (stdout != null)
				stdout_stream = new StreamWriter (new MiniStream (stdout));

			if (stderr != null)
				stderr_stream = new StreamWriter (new MiniStream (stderr));

			return Start (info, extra_args, stdin, stdout_stream,
				      stderr_stream, ctxt);
		}

		public static string GetToolStdout (BinaryInfo info, string extra_args, out int exit_code, out string stderr,
						    IBuildContext ctxt) {
			MemoryStream msout = new MemoryStream (32);
			StreamWriter stdout = new StreamWriter (msout);
			MemoryStream mserr = new MemoryStream (32);
			StreamWriter err = new StreamWriter (mserr);

			exit_code = Start (info, extra_args, null, stdout, err, ctxt);

			byte[] buf = msout.ToArray ();
			string result = System.Text.Encoding.Default.GetString (buf).Trim ();

			buf = mserr.ToArray ();
			stderr = System.Text.Encoding.Default.GetString (buf).Trim ();

			ctxt.Logger.Log ("launcher.stdout", result);
			ctxt.Logger.Log ("launcher.stderr", stderr);
			return result;
		}

		public static string GetToolStdout (BinaryInfo info, string extra_args, out int exit_code, IBuildContext ctxt) {
			string ignore;

			return GetToolStdout (info, extra_args, out exit_code, out ignore, ctxt);
		}

		public static string GetToolStdout (BinaryInfo info, string extra_args, out string stderr, IBuildContext ctxt) {
			int exit_code;
			string res = GetToolStdout (info, extra_args, out exit_code, out stderr, ctxt);

			if (exit_code != 0)
				return null;
			return res;
		}

		public static string GetToolStdout (BinaryInfo info, string extra_args, bool report, 
						    IBuildContext ctxt) {
		    int exit_code;
		    string stderr;

		    string res = GetToolStdout (info, extra_args, out exit_code, out stderr, ctxt);

		    if (exit_code != 0) {
			if (report) {
			    string detail = info.ToUnixStyle (ctxt) + " " + extra_args + ":\n" + stderr;
			    ctxt.Logger.Error (3003, "Tool error:", detail);
			}

			return null;
		    }

		    return res;
		}

		public static string GetToolStdout (BinaryInfo info, string extra_args, IBuildContext ctxt) {
		    return GetToolStdout (info, extra_args, true, ctxt);
		}

		public static int SaveToolStdout (BinaryInfo info, string extra_args, MBFile stdout, 
						  out string stderr, IBuildContext ctxt) 
		{
		    Stream sout = stdout.OpenWrite (ctxt);
		    StreamWriter wout = new StreamWriter (sout);
		    MemoryStream mserr = new MemoryStream (32);
		    StreamWriter err = new StreamWriter (mserr);

		    int result = Start (info, extra_args, null, wout, err, ctxt);

		    byte[] buf = mserr.ToArray ();
		    stderr = System.Text.Encoding.Default.GetString (buf).Trim ();

		    ctxt.Logger.Log ("launcher.stdout_to", stdout.GetPath (ctxt));
		    ctxt.Logger.Log ("launcher.stderr", stderr);
		    return result;
		}


		public static int RunTool (BinaryInfo info, string extra_args, out string stdout, out string stderr, IBuildContext ctxt) {
			ProcessStartInfo si = PrepareInfo (info, extra_args, ctxt);

			ctxt.Logger.Log ("launcher.launch", si.FileName + " " + si.Arguments);
			Process ps = Process.Start (si);
			ps.WaitForExit ();

			stdout = "";
			stderr = "";

			// FIXME: we should be able to interweave the stderr and
			// stdout output as it comes, as opposed to one after
			// the other as we lamely do here.

			string data = ps.StandardOutput.ReadToEnd ().Trim ();
			if (data.Length > 0) {
				ctxt.Logger.Log ("launcher.stdout", data);
				stdout += data;
			}

			data = ps.StandardError.ReadToEnd ().Trim ();
			if (data.Length > 0) {
				ctxt.Logger.Log ("launcher.stderr", data);
				stderr += data;
			}
			
			ctxt.Logger.Log ("launcher.exit", ps.ExitCode.ToString ());
			return ps.ExitCode;
		}

		public static int RunTool (BinaryInfo info, string extra_args, IBuildContext ctxt,
					   bool fatal, string message) {
			string stdout, stderr;
			int result = RunTool (info, extra_args, out stdout, out stderr, ctxt);

			if (result != 0) {
				string detail = info.ToUnixStyle (ctxt) + " " + extra_args + ":\n" + stdout + "\n" + stderr;

				if (fatal)
					ctxt.Logger.Error (3003, message, detail);
				else
					ctxt.Logger.Warning (3004, message, detail);
			} else if (stderr.Length > 0) {
				// FIXME: is this a good idea?
				string detail = info.ToUnixStyle (ctxt) + " " + extra_args + ":\n" + stderr;

				ctxt.Logger.Warning (3010, "Tool warning:", detail);
			}

			return result;
		}

		public static int RunTool (BinaryInfo info, out string stdout, out string stderr, IBuildContext ctxt) {
			return RunTool (info, null, out stdout, out stderr, ctxt);
		}

		public static int RunTool (BinaryInfo info, IBuildContext ctxt, bool fatal, string message) {
			return RunTool (info, null, ctxt, fatal, message);
		}

		public static int RunTool (BinaryInfo info, IBuildContext ctxt, string message) {
			return RunTool (info, null, ctxt, true, message);
		}

		public static int RunTool (BinaryInfo info, string extra_args, IBuildContext ctxt, string message) {
			return RunTool (info, extra_args, ctxt, true, message);
		}

		public static int SaveToolStdout (BinaryInfo info, string extra_args, MBFile stdout, 
						  IBuildContext ctxt, bool fatal, string message) 
		{
		    string stderr;
		    int code = SaveToolStdout (info, extra_args, stdout, out stderr, ctxt);

		    if (code != 0) {
			string detail = info.ToUnixStyle (ctxt) + " " + extra_args + ":\n" + stderr;

			if (fatal)
			    ctxt.Logger.Error (3003, message, detail);
			else
			    ctxt.Logger.Warning (3004, message, detail);
		    } else if (stderr.Length > 0) {
			// FIXME: is this a good idea?
			string detail = info.ToUnixStyle (ctxt) + " " + extra_args + ":\n" + stderr;

			ctxt.Logger.Warning (3010, "Tool warning:", detail);
		    }

		    return code;
		}

		public static int SaveToolStdout (BinaryInfo info, string extra_args, MBFile stdout, 
						  IBuildContext ctxt, string message) 
		{
		    return SaveToolStdout (info, extra_args, stdout, ctxt, true, message);
		}

		public static string EscapeForShell (string input)
		{
		    // based on g_shell_quote.
		    // FIXME: we also need to handle the Windows shell! Joy of joys!

		    char[] c = input.ToCharArray ();
		    StringBuilder sb = new StringBuilder ("\'");

		    for (int i = 0; i < c.Length; i++) {
			if (c[i] == '\'')
			    sb.Append ("'\\''");
			else
			    sb.Append (c[i]);
		    }

		    sb.Append ('\'');
		    return sb.ToString ();
		}
	}
}
