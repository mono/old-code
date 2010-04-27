using System;

namespace Xnb
{
	using Protocol.XProto;
	using Protocol.XCMisc;

	public class XidManager
	{
		uint last, max, inc, idbase, count;

		XCMisc xcmisc;
		Connection c;

		protected XidManager () {}

		//TODO: only get the extension if we actually need a new Xid alloc
		public XidManager (XCMisc xcmisc)
		{
			this.xcmisc = xcmisc;
			c = xcmisc.Connection;

			last = 0;
			idbase = c.Setup.ResourceIdBase;
			max = c.Setup.ResourceIdMask;
			inc = (uint) (c.Setup.ResourceIdMask & -(c.Setup.ResourceIdMask));
		}

		public Id Generate ()
		{
			if (last == max)
			{
				GetXidRangeReply range = xcmisc.GetXidRange ();

				last = range.StartId;
				max = range.StartId + (count - 1) * inc;
			}

			uint ret = last | idbase;
			last += inc;

			return new Id (ret);
		}
	}
}
