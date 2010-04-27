namespace Mono.Languages.Logo
{
	using Gtk;
	using GtkSharp;
	using System.IO;
	using System.Text;

	public class TextWriterGtk : TextWriter
	{
		TextBuffer buf;

		public TextWriterGtk (TextBuffer buf)
		{
			this.buf = buf;
		}
		
		public override Encoding Encoding { get { return new UTF8Encoding (); } }
							 
		public override void Write (string value)
		{
			buf.Insert (buf.EndIter, value, value.Length);
		}					 
	}
}
