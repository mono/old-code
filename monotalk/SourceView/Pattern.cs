namespace Monotalk.SourceView
{
	public class Pattern
	{
		string Path;
		public string pattern;
		public string signore;
		public string spanto;
		protected string [] sdelim;
		protected string [] edelim;

		public Style style;

		public Pattern (Style style, string path, string pattern, string spanto, string sdelim, string edelim, string signore)
		{
			this.style = style;
			this.pattern = pattern;
			this.spanto = ( spanto == null ) ? null : UnEscape(spanto);
			this.sdelim = sdelim.Replace(@"\ ", @"\") . Split(' ');
	                this.edelim = edelim.Replace(@"\ ", @"\") . Split(' ');
			this.signore = signore;

			//Console.WriteLine("style \"{0}\" pattern \"{1}\" delim <{2}> ", name, pattern, sdelim);
			for (int i=0; i < this.sdelim.Length; i++)
				this.sdelim[i] = UnEscape (this.sdelim[i]);

			for (int i=0; i < this.edelim.Length; i++)
				this.edelim[i] = UnEscape (this.edelim[i]);

			//Console.WriteLine("style \"{0}\" pattern \"{1}\" delims ", name, pattern);
			//foreach (string s in this.sdelim)
			//	Console.Write(" <{0}>", s);
			//Console.WriteLine("");
		}

		protected string UnEscape (string str)
		{
			str = str.Replace(@"\t", "\t");
			str = str.Replace(@"\n", "\n");
			str = str.Replace(@"\r", "\r");
			str = str.Replace(@"\f", "\f");
			str = str.Replace(@"\",  " ");

			return str;
		}


		protected string Escape (string str)
		{
			str = str.Replace("\t", @"\t");
			str = str.Replace("\n", @"\n");
			str = str.Replace("\r", @"\r");
			str = str.Replace("\f", @"\f");
			str = str.Replace(" ",  @"\ ");

			return str;
		}

		public bool FalseAlarm (string buffer, int eindex)
		{
			bool false_alarm = true;

				//Console.WriteLine("alarm for \"{0}\" at pos {1}", pattern, eindex+1);

			int bi = eindex - pattern.Length;

			if ( bi >= 0 && sdelim[0].Length != 0 ) {
				foreach (string delim in sdelim) {
					int i;
					bi = eindex - pattern.Length;
    
					for (i = delim.Length - 1; i  >= 0; i--) {
						if ( delim[i] == buffer[bi--] ) 
							continue;
						else 
							break;
					}

					if ( i < 0 ) 
						{
							//Console.WriteLine("0 alarm: matched \"{0}\" before", Escape(delim));
							false_alarm = false;
							break;
						}
					else
						;//Console.WriteLine("0 alarm: did not match \"{0}\"", Escape(delim));
				}
			}
			else 
				false_alarm = false;


			if ( false_alarm )
				return true;

			if ( edelim[0].Length == 0 )
				return false;

			bi = eindex + 1;

			if ( bi >= buffer.Length ) 
				return false;

			foreach (string delim in edelim) {
				int i; 
				bi = eindex + 1;

				for (i=0; i < delim.Length; i++) {
					//Console.WriteLine("compare: {0} .. {1}", delim[i], buffer[bi]);
					if ( delim[i] == buffer[bi++] )
						continue;
					else
						break;
				}

				if ( i == delim.Length )  {
					//Console.WriteLine("1 alarm: matched \"{0}\"", Escape(delim));
					return false;
				} else
					;//Console.WriteLine("1 alarm: did not match \"{0}\"", Escape(delim));
			}

			return true;
		}
	}
}
