//
// QuotedPrintable.cs:
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
using System.Globalization;
using System.Text;

public class QuotedPrintable {
	#region Fields
	private const byte EQUALS = 61;
	private const byte CR = 13;
	private const byte LF = 10;
	private const byte SPACE = 32;
	private const byte TAB = 9;
	#endregion

	/// <summary>
	/// Encodes a string to QuotedPrintable
	/// </summary>
	/// <param name="s">String to encode</param>
	/// <returns>QuotedPrintable encoded string</returns>
	public static string Encode(string s, string charset) {
		s = Encoding.GetEncoding(charset).GetString(Encoding.Default.GetBytes(s));
		
		StringBuilder encoded = new StringBuilder();
		string hex;
		byte[] bytes = Encoding.Default.GetBytes(s);
		int count = 0;

		for (int i = 0; i < bytes.Length; i++) {
			//these characters must be encoded
			if ((bytes[i] < 33 || bytes[i] > 126 || bytes[i] == EQUALS) && bytes[i] != CR && bytes[i] != LF && bytes[i] != SPACE) {
				if (bytes[i].ToString("X").Length < 2) {
					hex = "0" + bytes[i].ToString("X");
					encoded.Append("=" + hex);
				} else {
					hex = bytes[i].ToString("X");
					encoded.Append("=" + hex);
				}
			} else {
				//check if index out of range
				if ((i + 1) < bytes.Length) {
					//if TAB is at the end of the line - encode it!
					if ((bytes[i] == TAB && bytes[i + 1] == LF) || (bytes[i] == TAB && bytes[i + 1] == CR)) {
						encoded.Append("=0" + bytes[i].ToString("X"));
					}
						//if SPACE is at the end of the line - encode it!
					else if ((bytes[i] == SPACE && bytes[i + 1] == LF) || (bytes[i] == SPACE && bytes[i + 1] == CR)) {
						encoded.Append("=" + bytes[i].ToString("X"));
					} else {
						encoded.Append(Convert.ToChar(bytes[i]));
					}
				} else {
					encoded.Append(Convert.ToChar(bytes[i]));
				}
			}
			if (count == 75) {
				encoded.Append("=\r\n"); //insert soft-linebreak
				count = 0;
			}
			count++;
		}

		return encoded.ToString();
	}

	/// <summary>
	/// Decodes a QuotedPrintable encoded string 
	/// </summary>
	/// <param name="s">The encoded string to decode</param>
	/// <returns>Decoded string</returns>
	public static string Decode(string s, string charset) {
		s = Encoding.GetEncoding(charset).GetString(Encoding.Default.GetBytes(s));
		
		StringBuilder decoded = new StringBuilder();

		//remove soft-linebreaks first
		s = s.Replace("=\r\n", "");

		char[] chars = s.ToCharArray();

		for (int i = 0; i < chars.Length; i++) {
			// if encoded character found decode it
			if (chars[i] == '=') {
				decoded.Append(Convert.ToChar(int.Parse(chars[i + 1].ToString() + chars[i + 2].ToString(), NumberStyles.HexNumber)));
				//jump ahead
				i += 2;
			} else {
				decoded.Append(Convert.ToChar(chars[i]));
			}
		}

		return decoded.ToString();
	}
}