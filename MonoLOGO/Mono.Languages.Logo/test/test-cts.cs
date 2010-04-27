using System;
using System.Collections;
using Mono.Languages.Logo.Runtime;

class X {
	static object Ask (object target_obj, string message, params object[] args) {
		object result = null;
		IMessageTarget target = new CTSMessageTarget (target_obj);
		result = target.SendMessage (new LogoContext (null), message, args);
		return result;
	}

	static void Describe (object target_obj, string message) {
		Console.WriteLine ();
		IMessageStore store = new CTSMessageTarget (target_obj);
		MessageInfo info = store.DescribeMessage (message);
		if (info == null) {
			Console.WriteLine ("{0} does not know about the message \"{1}\".", target_obj, message);
			return;
		}
		Console.WriteLine ("Target: {0}", target_obj);
		Console.WriteLine ("Message: {0}", info.message);
		Console.WriteLine ("Arguments: ({0}, {1}, {2})", info.min_argc, info.max_argc, info.default_argc);
	}
	
	public static void Main () {
		Ask (typeof (System.Console), "WriteLine", "Hello World");

		ArrayList list = new ArrayList ();
		Ask (typeof (System.Console), "WriteLine", Ask (list, "ToString"));
		Ask (list, "Add", 37);
		Ask (typeof (System.Console), "WriteLine", Ask (list, "get_Item", 0));

		Describe (typeof (System.Console), "WriteLine");
		Describe (typeof (System.Console), "GetMethod");
		Describe (list, "Add");
		Describe (typeof (System.Console), "FooBar");
	}
}

