/***************************************************************************
 *  Copyright 2005 Daniel Drake <dsd@gentoo.org>
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

using System;
using System.IO;
using Entagged.Audioformats.Util;
using Entagged.Audioformats.Tracker.Util;

namespace Entagged.Audioformats.Tracker {

	public enum TrackerFormat {
		It,
		Mod,
		S3m,
		Xm,
	}

	[SupportedMimeType ("audio/x-s3m")]
	[SupportedMimeType ("audio/x-mod")]	
	[SupportedMimeType ("audio/x-xm")]
	[SupportedMimeType ("audio/x-it")]
	[SupportedMimeType ("entagged/s3m")]
	[SupportedMimeType ("entagged/mod")]
	[SupportedMimeType ("entagged/xm")]
	[SupportedMimeType ("entagged/it")]
	public class TrackerFileReader : AudioFileReader {

		private TrackerTagReader tr = new TrackerTagReader();

		protected override EncodingInfo GetEncodingInfo(Stream raf, string mime)
		{
			return new EncodingInfo();
		}

		protected override Tag GetTag(Stream raf, string mime)
		{
			return tr.Read(raf, GetTrackerFormat(mime));
		}

		private TrackerFormat GetTrackerFormat(string mime)
		{
			string segment = mime.Substring(mime.IndexOf('/') + 1);
			if (segment.StartsWith ("x-"))
				segment = segment.Substring(2);

			try {
				return (TrackerFormat) Enum.Parse(typeof(TrackerFormat), segment, true);
			} catch(Exception e) {
				throw new Exception("Unrecognised tracker file format: " + mime);
			}
		}

	}
}
