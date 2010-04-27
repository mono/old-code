using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;

using System.Reflection;

// this is automatically generated from the .ice IDL
public abstract class HelloDispatcher : Ice.Object {
  public abstract void tellHello([Ice.AsProxy] HelloReceiver hr, string who);
}

public abstract class HelloReceiver : Ice.Object {
  public abstract void sayHelloUsing([Ice.AsProxy] HelloSayer hs, string who);
}

public abstract class HelloSayer : Ice.Object {
  public abstract void sayHello(string who);
}

public class HelloReceiverI : HelloReceiver {
  public override void sayHelloUsing([Ice.AsProxy] HelloSayer hs, string who) {
    Console.WriteLine ("HelloReceiver.sayHelloUsing start");
    hs.sayHello (who);
    Console.WriteLine ("HelloReceiver.sayHelloUsing done");
  }
}

public class Driver {
  public static void Main () {
    Ice.IceChannel ic = new Ice.IceChannel ();
    ChannelServices.RegisterChannel (ic);

    HelloDispatcher hd = (HelloDispatcher) Activator.GetObject (typeof (HelloDispatcher), "ice://localhost:10000/hellodispatcher");

    HelloReceiverI hr = new HelloReceiverI();
    RemotingServices.Marshal (hr, "asdf");
    hd.tellHello (hr, "proxying");
  }
}

