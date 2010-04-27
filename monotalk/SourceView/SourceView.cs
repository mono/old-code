namespace Monotalk.SourceView
{
	using System;
	using System.IO;
	using System.Collections;
	using Gtk;

	public class SourceView : TextBuffer
	{
		int leadingWhiteSpaceLen;
		string[] whiteSpace;

		readonly int tabChars = 8;

                static Highlights highlights;
                static Config config;

		static SourceView ()
		{
			config = new Config ();
			highlights = new Highlights (config.patterns);
		}

		private void HighlightConfigChanged (object sender, GConf.NotifyEventArgs args)
		{
			TextTag tag;
			foreach (Style s in config.styles) {
			        s.Read ();
				tag = TagTable.Lookup (s.path);
				if (tag != null) {
					tag.Foreground = s.color;
					//tag.Weight = Convert.ToInt32 (s.weight);
				}
			}
			Highlight ();
		}

		public SourceView (TextTagTable table) : base (table) 
		{
			AddTags ();
			Config.gconf.AddNotify (Config.GCONF_DIR, new GConf.NotifyEventHandler (HighlightConfigChanged));
		}

                private void AddTags ()
                {
			foreach (Monotalk.SourceView.Style s in config.styles) {
				Gtk.TextTag tag = new TextTag (s.path);
				tag.Foreground  = s.color;
				//tag.Weight = Convert.ToInt32 (s.weight);
			        TagTable.Add (tag);
			}
                }

                private void Highlight ()
                {
			Token [] tokens = highlights.Search (Text);
        
			foreach (Token t in tokens)
			{
				Gtk.TextIter siter, eiter;

				GetIterAtOffset(out siter, t.sindex);
				GetIterAtOffset(out eiter, t.eindex);
				ApplyTag (TagTable.Lookup(t.style.path), siter, eiter);
			}
                }

		public bool InsertSource (TextIter iter, string filename, int startRow, int endRow)
		{
			Stream input;

			try {
				input = File.OpenRead (filename);
			} catch {
				return false;
			}

			StreamReader reader = new StreamReader (input);
			string text;
			int line = 1;

			PlaceCursor (iter);
			while ((text = reader.ReadLine()) != null) {
				if (line >= startRow && line <= endRow) {
					if (line == startRow)
						FirstLine (ref text, endRow - startRow + 1);
					else
						NextLine (ref text, line - startRow);
					InsertAtCursor (text + "\n");
				}
				line ++;
			}

                        Highlight();

			return true;
		}

		private void FirstLine (ref string text, int len)
		{
			int idx = 0;

			leadingWhiteSpaceLen = 0;
			while (idx < text.Length
			       && (text [idx] == ' ' || text [idx] == '\t')) {
				leadingWhiteSpaceLen += text [idx] == ' ' ? 1 : tabChars;
				idx ++;
			}
			whiteSpace = new string [len];
			whiteSpace [0] = text.Remove (idx, text.Length - idx);
			text = text.Substring (idx);
		}

		private void NextLine (ref string text, int line)
		{
			int idx = 0;
			int len = 0;

			while (idx < text.Length && len < leadingWhiteSpaceLen &&
			       (text [idx] == ' ' || text [idx] == '\t')) {
				len += text [idx] == ' ' ? 1 : tabChars;
				idx ++;
			}

			whiteSpace [line] = text.Remove (idx, text.Length - idx);
			text = text.Substring (idx);
		}
	}
}
