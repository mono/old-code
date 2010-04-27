// COPIED FROM: mcs/class/System.Web/System.Web.Util/StrUtils.cs

//
// System.Web.Util.StrUtils
//
// Author(s):
//  Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2005 Novell, Inc, (http://www.novell.com)
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

//namespace System.Web.Util {
namespace Mono.Build {
	public static class StrUtils {
		static CultureInfo invariant = CultureInfo.InvariantCulture;
		
		public static bool StartsWith (string str1, string str2)
		{
			return StartsWith (str1, str2, false);
		}

		public static bool StartsWith (string str1, string str2, bool ignore_case)
		{
			int l2 = str2.Length;
			if (l2 == 0)
				return true;

			int l1 = str1.Length;
			if (l2 > l1)
				return false;

			return (0 == String.Compare (str1, 0, str2, 0, l2, ignore_case, invariant));
		}

		public static bool EndsWith (string str1, string str2)
		{
			return EndsWith (str1, str2, false);
		}

		public static bool EndsWith (string str1, string str2, bool ignore_case)
		{
			int l2 = str2.Length;
			if (l2 == 0)
				return true;

			int l1 = str1.Length;
			if (l2 > l1)
				return false;

			return (0 == String.Compare (str1, l1 - l2, str2, 0, l2, ignore_case, invariant));
		}

		// This is MBuild-specific

		public static string CanonicalizeTarget (string target, string relbasis)
		{
		    if (target == null)
			throw new ArgumentNullException ("target");

		    if (target[0] != '/') {
			// Allow canonicalization of absolute target paths by
			// passing a null relbasis. Useful for something that
			// might look like /a/b/../c/./e

			if (relbasis == null)
			    throw new ArgumentNullException ("relbasis");
			if (relbasis[0] != '/' || relbasis[relbasis.Length - 1] != '/')
			    throw ExHelp.Argument ("relbasis", "Invalid basis `{0}'", relbasis);

			target = relbasis + target;
		    }

		    int tlen = target.Length;

		    char[] buf = new char[tlen];
		    int[] slashlocs = new int[tlen / 2];
		    int bufidx = 0, slashidx = 0, tidx;
		    bool saw_slash = false;

		    for (tidx = 0; tidx < tlen; /*nothing*/) {
			char c = target[tidx++];

			if (!saw_slash) {
			    buf[bufidx++] = c;

			    if (c == '/') {
				slashlocs[slashidx++] = bufidx;
				saw_slash = true;
			    }

			    continue;
			}

			if (c == '/')
			    throw ExHelp.Argument ("target", "Target name `{0}' cannot " + 
						   "contain `//'", target);
			    
			if (c != '.') {
			    // Short-circuit. This cannot be a special construct.
			    buf[bufidx++] = c;
			    saw_slash = false;
			    continue;
			}

			// We're a period after a slash.
			// If there are no characters left, that's illegal.

			int numleft = tlen - tidx;

			if (numleft < 1)
			    throw ExHelp.Argument ("target", "Target name `{0}' cannot end " +
						   "in bare `/.'", target);

			// If there are two characters left, we may
			// be a '/./' construct.

			if (numleft > 0 && target[tidx] == '/') {
			    // Preserve saw_slash!
			    tidx ++;
			    continue;
			}

			// Or a trailing /.., which is also illegal.

			if (numleft == 1 && target[tidx] == '.')
			    throw ExHelp.Argument ("target", "Target name `{0}' cannot end " +
						   "in bare `/..'", target);

			
			// If there are three, we may be a '/../' construct.

			if (numleft > 1 && target[tidx] == '.' && target[tidx+1] == '/') {
			    // Eat up to the previous point in buf,
			    // and preserve saw_slash
			    tidx += 2;
			    slashidx -= 2;

			    if (slashidx < 0)
				throw ExHelp.Argument ("target", "Target name `{0}' has too " +
						       "many `..' sequences.", target);

			    bufidx = slashlocs[slashidx++];
			    continue;
			}

			// We're not a specal construct. Continue as usual.

			buf[bufidx++] = c;
			saw_slash = false;
		    }			    
			
		    return new String (buf, 0, bufidx);
		}
	}
}

