using System;
using Wahid;
using Wahid.SpreadsheetML;

class Test {
	static void Main ()
	{
		Xlsx xlsx = Xlsx.Load ("test");
		if (xlsx.Error == null)
			Console.WriteLine ("Success");
		else
			Console.WriteLine ("Result: " + xlsx.Error);
	}
}