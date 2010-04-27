//
// ParamList.cs:
//
// Author:
//	Antonello Provenzano (antonello@deveel.com)
//
// (C) 2006, Deveel srl, (http://deveel.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.Collections;

namespace Deveel.Pim {
	class ParamList {
		#region .ctor
		// Constructs a list of tokens starting from head and ending at tail, parses
		// the list and extracts informations about parameters
		public ParamList(Token head, Token tail) {
			this.head = head;
			this.tail = tail;

			typelist = new ArrayList();

			for (Token tok = head; tok != tail; tok = tok.next) {
				if (!(tok.image.ToUpper().IndexOf("CHARSET=") == -1)) {
					chrset = tok.image.Substring(9);
				} else if (!(tok.image.ToUpper().IndexOf("LANGUAGE=") == -1)) {
					language = tok.image.Substring(10);
				} else if (!(tok.image.ToUpper().IndexOf("VALUE=") == -1)) {
					value = tok.image.Substring(7);
				} else if (!(tok.image.ToUpper().IndexOf("ENCODING=") == -1)) {
					encoding = tok.image.Substring(10);
				} else if (!(tok.image.ToUpper().IndexOf(";URL") == -1) |
						   !(tok.image.ToUpper().IndexOf(";INLINE") == -1) |
						   !(tok.image.ToUpper().IndexOf(";CONTENT-ID") == -1) |
						   !(tok.image.ToUpper().IndexOf(";CID") == -1)) {
					value = tok.image.Substring(1);
				} else if (!(tok.image.ToUpper().IndexOf(";7BIT") == -1) |
						   !(tok.image.ToUpper().IndexOf(";8BIT") == -1) |
						   !(tok.image.ToUpper().IndexOf(";QUOTED-PRINTABLE") == -1) |
						   !(tok.image.ToUpper().IndexOf(";BASE64") == -1)) {
					encoding = tok.image.Substring(1);
				} else if (!(tok.image.ToUpper().IndexOf("TYPE=") == -1)) {
					typelist.Add(tok.image.Substring(6));
				}
					// Specific for iCalendar
				else if (!(tok.image.ToUpper().IndexOf("ALTREP=") == -1)) {
					altrep = tok.image.Substring(8);
				} else if (!(tok.image.ToUpper().IndexOf("CN=") == -1)) {
					cn = tok.image.Substring(4);
				} else if (!(tok.image.ToUpper().IndexOf("CUTYPE=") == -1)) {
					cutype = tok.image.Substring(8);
				} else if (!(tok.image.ToUpper().IndexOf("DELEGATED-FROM=") == -1)) {
					delegatedFrom = tok.image.Substring(16);
				} else if (!(tok.image.ToUpper().IndexOf("DELEGATED-TO=") == -1)) {
					delegatedTo = tok.image.Substring(14);
				} else if (!(tok.image.ToUpper().IndexOf("DIR=") == -1)) {
					dir = tok.image.Substring(5);
				} else if (!(tok.image.ToUpper().IndexOf("MEMBER=") == -1)) {
					member = tok.image.Substring(8);
				} else if (!(tok.image.ToUpper().IndexOf("PARTSTAT=") == -1)) {
					partstat = tok.image.Substring(10);
				} else if (!(tok.image.ToUpper().IndexOf("RELATED=") == -1)) {
					related = tok.image.Substring(9);
				} else if (!(tok.image.ToUpper().IndexOf("SENT-BY=") == -1)) {
					sentby = tok.image.Substring(9);
				} else if (!(tok.image.ToUpper().IndexOf("X-") == -1)) {
					xProps.Add(tok.image.Substring(0, tok.image.IndexOf('=')));
				} else {
					// if we're here AND there wasn't a parse error the token must be a TYPE
					typelist.Add(tok.image.Substring(1));
				}
			}
		}

		public ParamList() {
			hash = new Hashtable();
			xHash = new Hashtable();
		}
		#endregion
		
		#region Fields
		private Token head;
		private Token tail;

		private String encoding;
		private String chrset;
		private String language;
		private String value;
		private ArrayList typelist;  // A Vector of Strings containing the types

		private String altrep;
		private String cn;
		private String cutype;
		private String delegatedFrom;
		private String delegatedTo;
		private String dir;
		private String member;
		private String partstat;
		private String related;
		private String sentby;
		private ArrayList xProps;

		private Hashtable hash;
		private Hashtable xHash;

		private const string ENCODING = "ENCODING";
		private const string CHARSET = "CHARSET";
		private const string LANGUAGE = "LANGUAGE";
		private const string VALUE = "VALUE";
		#endregion

		#region Properties
		public int Count {
			get { return hash.Count; }
		}

		public string this[string key] {
			get { return (string)hash[key]; }
		}

		public string Encoding {
			get {
				if (hash != null && hash.ContainsKey(ENCODING))
					return (String)hash[(String)ENCODING];
				return null;
			}
		}

		public string Charset {
			get {
				if (hash != null && hash.ContainsKey(CHARSET))
					return (String)hash[(String)CHARSET];
				return null;
			}
		}

		public string Language {
			get {
				if (hash != null && hash.ContainsKey(LANGUAGE))
					return (String)hash[(String)LANGUAGE];
				return null;
			}
		}

		public string Value {
			get {
				if (hash != null && hash.ContainsKey(VALUE))
					return (String)hash[(String)VALUE];
				return null;
			}
		}

		public Hashtable XParams {
			get { return xHash; }
		}

		// Returns the size of the list of TYPE parameters.
		public int TypeListCount {
			get { return typelist.Count; }
		}

		// Returns the list of TYPE parameters formatted in a comma-separated format.s
		public string FormattedTypes {
			get {
				string result = "";
				for (int i = 0; i < typelist.Count; i++) {
					result += typelist[i] + " ";
				}
				return result.Trim();
			}
		}

		// Returns the value of the ALTREP parameter.
		public string Altrep {
			get { return altrep; }
		}

		// Returns the value of the CN parameter.
		public string Cn {
			get { return cn; }
		}

		// Returns the value of the CUTYPE parameter.
		public string Cutype {
			get { return cutype; }
		}

		// Returns the value of the DELEGATED-FROM parameter.
		public string DelegatedFrom {
			get { return delegatedFrom; }
		}

		// Returns the value of the DELEGATED-TO parameter.
		public string DelegatedTo {
			get { return delegatedTo; }
		}

		// Returns the value of the DIR parameter.

		public string Dir {
			get { return dir; }
		}

		// Returns the value of the MEMBER parameter.
		public string Member {
			get { return member; }
		}

		// Returns the value of the PARTSTAT parameter.
		public string Partstat {
			get { return partstat; }
		}

		// Returns the value of the RELATED parameter.
		public string Related {
			get { return related; }
		}

		// Returns the value of the SENT-BY parameter.
		public string Sentby {
			get { return sentby; }
		}

		// Returns the list of X-PROP parameters.
		public ArrayList XProps {
			get { return xProps; }
		}

		// Returns the size of the list of X-PROP parameters.
		public int XPropsCount {
			get { return xProps.Count; }
		}

		// Returns the list of X-PRO parameters formatted in a comma-separated format.
		public string FormattedXProps {
			get {
				String result = "";
				for (int i = 0; i < xProps.Count; i++) {
					result += xProps[i] + " ";
				}
				return result.Trim();
			}
		}
		#endregion

		#region Public Methods
		public bool ContainsKey(String key) {
			return hash.ContainsKey((String)key);
		}

		public void Add(string paramName, string paramValue) {
			// to manage a thing like TEL;TYPE=HOME;TYPE=VOICE:123456
			// Consider like TEL;HOME;VOICE=123456
			if (paramName.Equals("TYPE")) {
				hash.Add(paramValue, null);
			}
			// to manager thing like N;BASE64:john;defoe;;;
			// Consider like N;ENCODING=BASE64
			else if (paramName.Equals("7BIT") |
					 paramName.Equals("8BIT") |
					 paramName.Equals("QUOTED-PRINTABLE") |
					 paramName.Equals("BASE64")) {
				hash.Add(ENCODING, paramName);
			}
			// to manager thing like N;INLINE:john;defoe;;;
			// Consider like N;VALUE=INLINE:john;defoe;;;
			else if (paramName.Equals("URL") |
					 paramName.Equals("INLINE") |
					 paramName.Equals("CONTENT-ID") |
					 paramName.Equals("CID")) {
				hash.Add(VALUE, paramName);
			} else if (paramName.StartsWith("X-")) {
				xHash.Add(paramName, paramValue);
			} else {
				hash.Add(paramName, paramValue);
			}
		}

		// Checks if the type list contains the specified item
		public bool TypeListContains(string item) {
			for (int i = 0; i < typelist.Count; i++) {
				if (((string)typelist[i]).ToUpper().Equals(item)) {
					return true;
				}
			}
			return false;
		}

		// Checks if the xProps list contains the specified item
		public bool XPropsContains(string item) {
			for (int i = 0; i < xProps.Count; i++) {
				if (((String)xProps[i]).ToUpper().Equals(item)) {
					return true;
				}
			}
			return false;
		}
		#endregion
	}
}