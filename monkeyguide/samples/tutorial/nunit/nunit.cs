/// nunit.cs - Useful tools, NUnit Tutorial example
///
/// Author: Martin Willemoes Hansen <mwh@sysrq.dk>
///
/// (C) 2003 Martin Willemoes Hansen

using NUnit.Framework;

namespace NUnitTutorial {

	[TestFixture]
	public class MyUnitTest : Assertion {

		string foo;

		[SetUp]
		public void GetReady()
		{
			foo = "Foobar";
		}

		[Test]
		public void TestLength()
		{
			AssertEquals ("(1) Length", 6, foo.Length);
		}

		[TearDown]
		public void Clear()
		{
		}
	}
}
