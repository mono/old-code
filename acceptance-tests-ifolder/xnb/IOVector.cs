using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mono.Unix;

namespace Xnb
{
	public struct IOVector
	{
		public IntPtr Base;
		public int Length;
	}

	/*
		 public class VectorSet : IEnumerable<IOVector>
		 {
		 public IEnumerator<IOVector> GetEnumerator()
		 {
		 yield return new IOVector ();
		 }
		 }
		 */

	public class VectorIO
	{
		/*
			 public static void Write (IOVector[] vec)
			 {
			 foreach (IOVector v in vec) {
			 Trace.WriteLine (v.Base);
			 Trace.WriteLine (v.Length);
			 Trace.WriteLine ();
			 }
			 }
			 */

		public static void Print (IOVector[] vector, bool dirOut)
		{
			Trace.WriteLine ("<message>");
			Trace.Indent ();

			foreach (IOVector v in vector) {
				//Trace.WriteLine (v.Base);
				Trace.WriteLine ("Vector Length: " + v.Length);
				for (int i = 0 ; i != v.Length ; i++) {
					byte b = Marshal.ReadByte (v.Base, i);
					if (dirOut)
						Trace.WriteLine ("-> " + i + " " + b);
					else
						Trace.WriteLine ("<- " + i + " " + b);
				}
			}

			Trace.Unindent ();
			Trace.WriteLine ("</message>");
			Trace.WriteLine ("");
		}

		public static int Read (int fd, IOVector[] vector)
		{
			int ret;
			ret = readv (fd, vector, vector.Length);
			Print (vector, false);
			Trace.WriteLine ("Actually read: " + ret);
			return ret;
		}

		public static int Write (int fd, IOVector[] vector)
		{
			int ret;
			Print (vector, true);
			ret = writev (fd, vector, vector.Length);
			Trace.WriteLine ("Actually wrote: " + ret);
			return ret;
		}

		[DllImport ("libc")]
			protected static extern int readv (int filedes, IOVector[] vector, int count);
		[DllImport ("libc")]
			protected static extern int writev (int filedes, IOVector[] vector, int count);
	}

	/*
#include <sys/uio.h>

int readv(int filedes, const struct iovec *vector,
size_t count);

int writev(int filedes, const struct iovec *vector,
size_t count);

*/

	public interface IMessagePart : IEnumerable<IOVector>, IEnumerable
	{
		int Read (IntPtr ptr);

		//IEnumerator<IOVector> GetEnumerator();
		//IOVector[] GetVectors ();
	}

	public static class XMarshal
	{
		public unsafe static IOVector Do (ref byte[] ary)
		{
			IOVector vec = new IOVector ();

			unsafe {
				fixed(byte* pData = &ary[0]) {
					vec.Base = (IntPtr)pData;
					vec.Length = sizeof(byte)*ary.Length;
				}
				//vec.Length = ary.Length;
			}

			return vec;
		}

		public unsafe static IOVector Do (ref uint[] ary)
		{
			IOVector vec = new IOVector ();

			unsafe {
				fixed(uint* pData = &ary[0]) {
					vec.Base = (IntPtr)pData;
					vec.Length = sizeof(uint)*ary.Length;
				}
				//vec.Length = ary.Length;
			}

			return vec;
		}

		public unsafe static IOVector Do (void* ptr, int len)
		{
			return new IOVector ();
		}

		public static IOVector Do (IntPtr ptr, int len)
		{
			return new IOVector ();
		}

		/*
			 public static IOVector Do<T> (ref T[] ary) where T: struct
			 {
			 IOVector vec = new IOVector ();

			 unsafe {
			 fixed(T* pData = &ary[0]) {
			 vec.Base = (IntPtr)pData;
			 vec.Length = sizeof(T)*ary.Length;
			 }
			 }

			 return vec;
			 }
			 */

		public static IOVector Do<T> (ref T data)// where T: struct
		{
			IOVector vec = new IOVector ();

			GCHandle gch = GCHandle.Alloc(data, GCHandleType.Pinned);
			vec.Base = gch.AddrOfPinnedObject ();
			vec.Length = Padded (Marshal.SizeOf (typeof(T)));
			gch.Free ();

			/*
				 unsafe {
				 fixed (void* ptr = &data) {
				 vec.Base = (IntPtr)ptr;
				 vec.Length = sizeof(T);
				 }
				 }
				 */

			return vec;
		}

		//maybe return iovec tree or array
		public static IOVector Do (ref string value)
		{
			IOVector vec = new IOVector ();

			//FIXME: SECURITY: uncleared memory used for padding
			vec.Base = UnixMarshal.StringToHeap (@value);
			vec.Length = Padded (value.Length);
			//TODO: use pad func and custom stringtoheap

			return vec;
		}

		public static int Pad (int len)
		{
			int pad = len % 4;
			pad = pad == 0 ? 0 : 4 - pad;

			return pad;
		}

		public static int Padded (int len)
		{
			return len + Pad (len);
		}
	}
}
