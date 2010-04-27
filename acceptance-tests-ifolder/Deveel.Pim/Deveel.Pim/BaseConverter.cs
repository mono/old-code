//
// BaseConverter.cs:
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
using System.Text;

namespace Deveel.Pim {
	/// <summary>
	/// Represent a converter base class. Provides some common methods.
	/// </summary>
	abstract class BaseConverter {
		#region .ctor
		public BaseConverter(TimeZone timezone, String charset) {
			this.timezone = timezone;
			this.charset = charset;
		}
		#endregion

		#region Fields
		public const string ENCODING_QT = "QUOTED-PRINTABLE";
		public const string CHARSET_UTF8 = "UTF-8";
		public const string ENCODING_B64 = "BASE64";
		public const string PLAIN_CHARSET = "plain";

		private TimeZone timezone = null;
		private string charset = null;
		#endregion

		#region Properties
		protected TimeZone TimeZone {
			get { return timezone; }
		}

		protected string Charset {
			get { return charset; }
		}
		#endregion

		#region Protected Methods
		protected string Encode(string value, string encoding, string charset) {
			try {
				if (value == null)
					return null;

				// If input charset is null then set it with default value as UTF-8
				if (charset == null)
					charset = "UTF-8";

				if (encoding.Equals(ENCODING_B64)) {
					//truncate the b64 encoded text into lines of no more that 76 chars
					StringBuilder sb = new StringBuilder();
					sb.Append("\r\n ");
					while (value.Length > 75) {
						sb.Append(value.Substring(0, 75));
						sb.Append("\r\n");
						sb.Append(" ");

						value = value.Substring(75);
					}
					sb.Append(value);
					sb.Append("\r\n");

					value = sb.ToString();

				} else if (encoding.Equals(ENCODING_QT)) {
					value = QuotedPrintable.Encode(value, charset);
				}

			} catch (ArgumentException e) {
				throw new ConverterException("The Character Encoding (" + charset + ") is not supported", e);
			}

			return value;
		}

		protected string GetEncoding(ArrayList properties) {
			for (int i = 0; i < properties.Count; i++) {
				if (((Property)properties[i]).Encoding != null) {
					return ((Property)properties[i]).Encoding;
				}
			}
			return null;
		}

		protected string GetCharset(ArrayList properties) {
			for (int i = 0; i < properties.Count; i++) {
				if (((Property)properties[i]).Charset != null) {
					return ((Property)properties[i]).Charset;
				}
			}
			return null;
		}

		protected string GetGrouping(ArrayList properties) {
			for (int i = 0; i < properties.Count; i++) {
				if (((Property)properties[i]).Group != null)
					return ((Property)properties[i]).Group;
			}
			return null;
		}

		protected string GetLanguage(ArrayList properties) {
			for (int i = 0; i < properties.Count; i++) {
				if (((Property)properties[i]).Language != null) {
					return ((Property)properties[i]).Language;
				}
			}
			return null;
		}

		protected string GetValue(ArrayList properties) {
			for (int i = 0; i < properties.Count; i++) {
				if (((Property)properties[i]).Value != null)
					return ((Property)properties[i]).Value;
			}
			return null;
		}

		protected string GetType(ArrayList properties) {
			for (int i = 0; i < properties.Count; i++) {
				if (((Property)properties[i]).PropertyType != null) {
					return ((Property)properties[i]).PropertyType;
				}
			}
			return null;
		}

		protected StringBuilder GetXParams(Property property) {
			ArrayList properties = new ArrayList();
			properties.Add(property);
			return GetXParams(properties);
		}

		protected StringBuilder GetXParams(ArrayList properties) {
			Hashtable hm;
			IEnumerator it;
			Property xtagProp;
			string tag;
			StringBuilder pars = new StringBuilder();

			for (int i = 0; i < properties.Count; i++) {
				xtagProp = (Property)properties[i];
				hm = xtagProp.XParams;
				it = hm.Keys.GetEnumerator();
				while (it.MoveNext()) {
					tag = (string)it.Current;
					pars.Append(";");
					pars.Append(tag);
					pars.Append("=");
					pars.Append((string)hm[tag]);
				}
			}
			return pars;
		}
		#endregion

		#region Protected Static Methods
		protected static string HandleUTCDateConversion(string sDate, TimeZone timezone) {
			try {
				sDate = TimeUtils.convertLocalDateToUTC(sDate, timezone);
			} catch (Exception) {
				throw new ConverterException("Error handling date");
			}
			return sDate;
		}

		protected static string HandleLocalDateConversion(string sDate, TimeZone timezone) {
			if (timezone == null)
				return sDate;

			try {
				sDate = TimeUtils.convertUTCDateToLocal(sDate, timezone);
			} catch (Exception) {
				throw new ConverterException("Error handling date");
			}

			return sDate;
		}
		#endregion

		#region Public Methods
		public abstract string Convert(object obj);
		#endregion
	}
}