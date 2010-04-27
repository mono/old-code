using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;

using System.Reflection;

// this is automatically generated from the .ice IDL
public abstract class Hello : Ice.Object {
  public abstract int sayHello(string[] who);
}

public class HelloI : Hello {
  public override int sayHello(string[] who) {
    foreach (string w in who) {
      Console.WriteLine ("Hello " + w);
    }
    return who.Length;
  }
}

public class Driver {
  public static void Main () {
    Ice.IceChannel ics = new Ice.IceChannel (10000);
    ChannelServices.RegisterChannel (ics);

    RemotingConfiguration.RegisterWellKnownServiceType (typeof (HelloI),
							"hello",
							WellKnownObjectMode.Singleton);

    Console.ReadLine();
  }
}
