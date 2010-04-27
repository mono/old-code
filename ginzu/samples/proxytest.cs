
using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

public abstract class TestClass : MarshalByRefObject {
  public abstract void Foo ();
}

public abstract class Derived : TestClass {
}

public class TestProxy : RealProxy, IRemotingTypeInfo {
  public TestProxy (Type baseType) : base (baseType)
  {
  }

  public override IMessage Invoke (IMessage msg) {
    Console.WriteLine ("Invoke: ");
    Console.WriteLine (msg.GetType());

    IMethodCallMessage mcall = msg as IMethodCallMessage;

    IMethodReturnMessage ret = new ReturnMessage (null, null, 0,
						  mcall.LogicalCallContext,
						  mcall);
    Console.WriteLine ("Finish");
    return ret;
  }

  string tn;
  string IRemotingTypeInfo.TypeName {
    get { return tn; }
    set { tn = value; }
  }

  bool IRemotingTypeInfo.CanCastTo (System.Type target, object o) {
    Console.WriteLine ("CanCastTo " + target);
    return true;
  }
}

public class Driver {
  public delegate void FooDelegate();

  public static void Main () {
    TestProxy tp = new TestProxy (typeof(TestClass));
    TestClass tc = (TestClass) tp.GetTransparentProxy();

    Derived d = (Derived) tc;
  }
}
