/***************************************************************************
 *  Copyright 2005 Daniel Drake <dsd@gentoo.org>
 *  Copyright 2005 Boris Peterbarg <boris-p@zahav.net.il>
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */

using System.IO;
using System.Text;
using Entagged.Audioformats.Tracker;

namespace Entagged.Audioformats.Tracker.Util {

	public class TrackerTagReader {

		public Tag Read(Stream fs, TrackerFormat format)
		{
			Tag tag = new Tag();
			int start_offset = 0;
			int end_offset = 0;
			string sig = null;

			switch (format) {
			case TrackerFormat.S3m:
				end_offset = 28;
				break;
			case TrackerFormat.Mod:
				end_offset = 20;
				break;
			case TrackerFormat.Xm:
				start_offset = 17;
				end_offset = 20;
				break;
			case TrackerFormat.It:
				sig = "IMPM";
				end_offset = 26;
				break;
			}

			//Parse the tag -)------------------------------------------------
			fs.Seek(start_offset, SeekOrigin.Begin);
			byte[] b;

			if (sig != null) {
				b = new byte[sig.Length];
				fs.Read(b, 0, sig.Length);
				if (Encoding.ASCII.GetString(b) != sig)
					return tag;
			}

			b = new byte[end_offset];
			fs.Read(b, 0, end_offset);

			string content = Encoding.ASCII.GetString(b, 0, end_offset);
			int term = content.IndexOf('\0');
			if (term == -1)
				term = end_offset;
			content = content.Substring(0, term).Trim();
			tag.AddTitle(content);

			return tag;
		}

	}

}
