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

public class Driver {
  public delegate int helloDelegate(string[] who);

  public static void Main () {
    Ice.IceClientChannel icc = new Ice.IceClientChannel ();
    ChannelServices.RegisterChannel (icc);

    Hello h = (Hello) Activator.GetObject (typeof (Hello), "ice://localhost:10000/hello");

    string[] ids = h.ice_ids();
    foreach (string id in ids) {
      Console.WriteLine (id);
    }

    helloDelegate hd = new helloDelegate (h.sayHello);

    string[] who = {".NET", "Mono", "C#"};
    IAsyncResult ar = hd.BeginInvoke (who, null, null);
    Console.WriteLine ("Waiting");
    ar.AsyncWaitHandle.WaitOne();
    int ret = hd.EndInvoke (ar);
    //    int ret = h.sayHello(who);
    Console.WriteLine ("Said " + ret + " hellos.");
  }
}

