namespace Monotalk.Indexer
{
	using System;
	using System.IO;
	using System.Collections;

	using Monotalk;
	using Monotalk.CSharp;

	public class Indexer
	{
		public SourceDB db = new SourceDB ();
		Hashtable fileIDs = new Hashtable ();
		Hashtable fileNames = new Hashtable ();
		int files = 0;

		public string this [int ID] {
			get {
				return (string) fileNames [ID];
			}
		}

		public void Parse (string filename)
		{
			CSharpParser parser;
			ArrayList defines = new ArrayList ();
			Stream input;
			int id;

			if (fileIDs [filename] == null) {
				fileIDs [filename] = (object)files;
				id = (int)fileIDs [filename];
				fileNames [id] = filename;
				files ++;
			} else
				id = (int) fileIDs [filename];

			try {
				input = File.OpenRead (filename);
			} catch {
				Console.WriteLine ("Source file '" + filename + "' could not be opened");
				return;
			}

			StreamReader reader = new StreamReader (input);
				
			parser = new CSharpParser (reader, filename, defines, db, id);
			//parser.yacc_verbose = yacc_verbose;
			try {
				parser.parse ();
			} catch (Exception ex) {
				Console.WriteLine ("Compilation aborted: " + ex);
			} finally {
				input.Close ();
			}
		}
	}
}
