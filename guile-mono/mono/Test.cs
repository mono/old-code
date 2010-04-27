using System;

public delegate int Foo (long a, float b);

public class A
{
	public static void Test ()
	{
		Console.WriteLine ("Hello from base class!");
	}

	public virtual void Foo ()
	{
		Console.WriteLine ("Base Foo");
	}

	public ulong Bar {
		get {
			ulong a = (ulong) Math.Pow (2,60);
			Console.WriteLine ("VALUE: {0:x} - {1}", a, a);
			return a;
		}

		set { Console.WriteLine ("SETTING: {0}", value); }
	}
}

public class X : A
{
	public override void Foo ()
	{
		Console.WriteLine ("Foo");
	}

	public void Hello (int a)
	{
		Console.WriteLine ("Hello World!");
		Console.WriteLine (a);
	}

	public static X DoHello ()
	{
		X x = new X ();
		x.Hello (7);
		return x;
	}

	public static int StaticProperty {
		get {
			return 4;
		}
	}

	static void Main ()
	{
	}
}
