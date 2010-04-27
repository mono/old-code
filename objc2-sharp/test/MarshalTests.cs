using NUnit.Framework;
using ObjC2;
using System;
using System.Runtime.InteropServices;

namespace TestObjC2 {
	[TestFixture]
	public class MarshalTests : ObjC2.Object {
		[Test]
		public void MarshalInt32 () {
			Assert.AreEqual (-1, this.Call ("MarshalReturnInt32", new object [] {7}));
		}

		[Export]
		public Int32 MarshalReturnInt32 (Int32 a) {
			return a-8;
		}
		
		[Test]
		public void MarshalIntPtr () {
			Assert.AreEqual ((IntPtr) 0xBEEF, this.Call ("MarshalReturnIntPtr", new object [] {0xFEED}));
		}
		
		[Export]
		public IntPtr MarshalReturnIntPtr (IntPtr a) {
			if (a == (IntPtr)0xFEED)
				return (IntPtr) 0xBEEF;
			return a;
		}
		
		[Test]
		public void MarshalFloat () {
			float val = (float) this.Call ("MarshalReturnSingle", new object [] {1.0f});
			Assert.AreEqual (-1.999f, val);
		}

		[Export]
		public float MarshalReturnSingle (float a) {
			return a-2.999f;
		}
		
		[Test]
		public void MarshalDouble () {
			double val = (double) this.Call ("MarshalReturnDouble", new object [] {1.0d});
			Assert.AreEqual (-2.999f, val);
		}
		
		[Export]
		public double MarshalReturnDouble (double a) {
			return a-3.999d;
		}
		
		[Test]
		public void MarshalBool () {
			bool val = (bool) this.Call ("MarshalReturnBool", new object [] {false});
			Assert.AreEqual (true, val);
		}

		[Export]
		public bool MarshalReturnBool (bool a) {
			return !a;
		}
		
		/*
		 * This test works, but something in nunit's appdomains or something causes it to have horrible horrible crashes.
		 * We should probably write a mini test-harness to do asserts and such for our use-case until we can fix the 
		 * nunit crasher.  Someone needs to diagnose exactly whats going on here too
		 */
		[Test]
		public void MarshalObject () {
			/*
			 * See comment above
			 * 
			ObjC2.Object val = (ObjC2.Object) ObjC2.Object.Invoke (this, "MarshalReturnObject", new object [] {this});
			Assert.AreEqual (this, val);
			 */
		}
		[Export]
		public ObjC2.Object MarshalReturnObject (ObjC2.Object a) {
			return a;
		}
	}
}
