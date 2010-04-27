using System;
using System.Collections;
using System.Reflection;
using Mono.Languages.Logo.Runtime;

class X {

	private static object InvokeFunc (string name, params object[] args) {
		Type type = typeof (Mono.Languages.Logo.Runtime.Funcs);
		return type.InvokeMember (name, BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, new LogoBinder (new LogoContext (null)), null, args); 
	}

	public static void Main () {
		string str = (string) InvokeFunc ("ButFirst", "Hello world");
		Console.WriteLine (str);
		ICollection bf = (ICollection) InvokeFunc ("ButFirst", new int[] {1, 2, 3});
		InvokeFunc ("Show", bf);
	}
}

