namespace Mono.Languages.Logo
{
	using Mono.LOGO.Lib;
	using System;
	using System.IO;

	public abstract class LogoConsole
	{
		LogoStack stack = new LogoStack ();
		LogoParser parser = new LogoParser ();

		public LogoConsole ()
		{
		}

		public TextWriter Out
		{
			get { return Console.Out; }
			set { Console.SetOut (value); }
		}

		public void InputCommand (string cmd)
		{
			int ret = -1;
			Console.WriteLine ("? " + cmd);
			parser.ParseString (cmd);
			if (parser.Tree != null)
			{
				try
				{
					ret = parser.Tree.Eval (stack);
				}
				catch (LogoException e)
				{
					Console.WriteLine (e.Message);
				}
			}
			else
				Console.WriteLine ("parse error");
			stack.Clear ();
		}
	}
}
