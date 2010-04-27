using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Mono.Unix;
using Mono.Unix.Native;

namespace Xnb
{
	using Protocol.Xnb;

	public class XMarshaler : ICustomMarshaler
	{
		public XMarshaler ()
		{
		}

		public void CleanUpManagedData (object ManagedObj)
		{
		}

		public void CleanUpNativeData (IntPtr pNativeData)
		{
		}

		public int GetNativeDataSize ()
		{
			Trace.WriteLine ("S");
			return 32;
		}

		public IntPtr MarshalManagedToNative (object ManagedObj)
		{
			Trace.WriteLine ("N");
			return IntPtr.Zero;
		}

		public object MarshalNativeToManaged (IntPtr pNativeData)
		{
			ConnSetupSuccessRep rep = new ConnSetupSuccessRep ();
			Trace.WriteLine ("m");

			GCHandle gch;
			
			gch = GCHandle.Alloc (rep);
			
			IntPtr startPtr = gch.AddrOfPinnedObject ();
			int sz = Marshal.SizeOf (rep);

			Trace.WriteLine (sz);

			for (int i = 0 ; i != sz ; i++) {
				Marshal.WriteByte (startPtr, i, Marshal.ReadByte (pNativeData, i));
			}
			
			gch.Free ();
			
			Trace.WriteLine ("vendorlen: " + rep.VendorLen);

			/*
			IntPtr newSz = new IntPtr (sz + rep.VendorLen);
			Marshal.ReAllocHGlobal (startPtr, newSz);
			*/

			//IntPtr nu = Marshal.AllocHGlobal (new IntPtr (rep.VendorLen));
			//for (int i = 0 ; i != rep.VendorLen ; i++) {
			//	Marshal.WriteByte (nu, i, Marshal.ReadByte (pNativeData, i));
			//}

			IntPtr nu = new IntPtr ((int)pNativeData + 40);
			string str = Marshal.PtrToStringAnsi (nu, rep.VendorLen);
			Trace.WriteLine ("str: " + str);
			//rep.Vendor = str;
			
			//Marshal.PtrToStructure(ptr, typeof()
			
			//Marshal.ReAllocCoTaskMem (pNativeData, sz + 40);
			//Marshal.ReAllocHGlobal (pNativeData, new IntPtr (sz + 40));



			return rep;
		}

		public static ICustomMarshaler GetInstance (string cookie)
		{
			Trace.WriteLine ("Cookie: " + cookie);

			XMarshaler xm = new XMarshaler ();

			return xm;
		}
	}
}
