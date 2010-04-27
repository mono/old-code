namespace Mono.Languages.Logo.Compiler
{
	using System.IO;
	using System.Text;

	public class IndentingWriter : TextWriter
	{
		TextWriter writer;
		int level;
		string tab;
		bool do_indent = true;

		public IndentingWriter (TextWriter writer)
		{
			this.writer = writer;
			this.Level = 0;
			this.Tab = "\t";
		}

		public int Level {
			get { return level; }
			set { level = value; }
		}
		
		public string Tab {
			get { return tab; }
			set { tab = value; }
		}

		public void Indent () {
			Level = Level + 1;
		}

		public void Deindent () {
			Level = Level - 1;
		}

		public override Encoding Encoding { get { return writer.Encoding; } }
							 
		public override void Write (string value)
		{
			if (do_indent) {
				for (int i = 0; i < level; i++)
					writer.Write (Tab);
				do_indent = false;
			}

			writer.Write (value);
		}

		public override void WriteLine () {
			writer.WriteLine ();
			do_indent = true;
		}
	}
}
