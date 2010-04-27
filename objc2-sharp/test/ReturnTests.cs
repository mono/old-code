using NUnit.Framework;
using ObjC2;
using System;
using System.Runtime.InteropServices;

namespace TestObjC2 {
	[TestFixture]
	public class ReturnTests : ObjC2.Object {
		[Test]
		public void ReturnInt32 () {
			Assert.AreEqual (-1, this.Call ("GetInt32"));
		}

		[Export]
		public Int32 GetInt32 () {
			return -1;
		}
		
		[Test]
		public void ReturnIntPtr () {
			Assert.AreEqual ((IntPtr) 0xBEEF, this.Call ("GetIntPtr"));
		}
		
		[Export]
		public IntPtr GetIntPtr () {
			return (IntPtr) 0xBEEF;
		}
		
		[Test]
		public void ReturnFloat () {
			float val = (float) this.Call ("GetSingle");
			Assert.AreEqual (-1.999f, val);
		}
		
		[Export]
		public float GetSingle () {
			return -1.999f;
		}
		
		[Test]
		public void ReturnDouble () {
			double val = (double) this.Call ("GetDouble");
			Assert.AreEqual (-2.999f, val);
		}
		
		[Export]
		public double GetDouble () {
			return -2.999d;
		}
		
		[Test]
		public void ReturnBool () {
			bool val = (bool) this.Call ("GetBool");
			Assert.AreEqual (true, val);
		}
		
		[Export]
		public bool GetBool () {
			return true;
		}
		
		[Test]
		public void ReturnObject () {
			ObjC2.Object val = (ObjC2.Object) this.Call ("GetObject");
			Assert.AreEqual (this, val);
		}

		[Export]
		public ObjC2.Object GetObject () {
			return this;
		}
	}
}
