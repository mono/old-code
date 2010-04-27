using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;

using System.Reflection;

// this is automatically generated from the .ice IDL
public abstract class Ping : Ice.Object {
}

public class PingI : Ping {
}

public class Driver {
  public static void Main () {
    Ice.IceChannel ics = new Ice.IceChannel (10000);
    ChannelServices.RegisterChannel (ics);

    RemotingConfiguration.RegisterWellKnownServiceType (typeof (PingI),
							"ping",
							WellKnownObjectMode.Singleton);

    Console.ReadLine();
  }
}
