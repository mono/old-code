namespace Monotalk.Browser {

	using System;
	using System.Collections;

	public class TypePool : Hashtable {
		public void Add (Type t)
		{
			this [t.FullName] = t;
		}
	}
}
