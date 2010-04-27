using NUnit.Framework;
using ObjC2;
using System;
using System.Runtime.InteropServices;

namespace TestObjC2 {
	[TestFixture]
	public class InvokeTests : ObjC2.Object {
		private string InstanceString;

		[Test]
		public void InvokeRoundTrip () {
			this.Call ("InvokeSetInstanceString");
			Assert.AreEqual (InstanceString, "AndForgetIt");
		}
		
		[Export]
		public void InvokeSetInstanceString () {
			InstanceString = "AndForgetIt";
		}
	}
}
