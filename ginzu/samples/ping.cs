using System;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Reflection;

// this is automatically generated from the .ice IDL
public abstract class Hello : Ice.Object {
  public abstract int sayHello(string[] who);
}

public abstract class Ping : Ice.Object {
}

public class Driver {
  public static void Main () {
    Ice.IceClientChannel icc = new Ice.IceClientChannel ();
    ChannelServices.RegisterChannel (icc);

    Ping p = (Ping) Activator.GetObject (typeof (Ping), "ice://localhost:10000/ping");

    p.ice_ping();

    int start = Environment.TickCount;

    for (int i = 0; i < 100000; i++) {
      p.ice_ping();
    }

    int end = Environment.TickCount;

    Console.WriteLine ("100000 pings took: {0}", ((float) (end - start)) / 1000.0f);
  }
}

