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

public class HelloDispatcherI : HelloDispatcher {
  public override void tellHello([Ice.AsProxy] HelloReceiver hr, string who) {
    Console.WriteLine ("HelloDispatcher.tellHello start");
    HelloSayerI hsi = new HelloSayerI();
    RemotingServices.Marshal (hsi, "blargh");
    hr.sayHelloUsing (hsi, who);
    Console.WriteLine ("HelloDispatcher.tellHello done");
  }
}

public class HelloSayerI : HelloSayer {
  public override void sayHello(string who) {
    Console.WriteLine ("Hello " + who);
  }
}

public class Driver {
  public static void Main () {
    Ice.IceChannel ics = new Ice.IceChannel (10000);
    ChannelServices.RegisterChannel (ics);

    RemotingConfiguration.RegisterWellKnownServiceType (typeof (HelloDispatcherI),
							"hellodispatcher",
							WellKnownObjectMode.Singleton);

    Console.ReadLine();
  }
}
