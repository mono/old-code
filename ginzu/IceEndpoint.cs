// -*- mode: csharp; c-basic-offset: 2; indent-tabs-mode: nil -*-
//
// IceEndpoint.cs
//
// Written by:
//   Vladimir Vukicevic <vladimir@sparklestudios.com>
//
// Copyright (C) 2003 Sparkle Studios, LLC
//
// This file is distributed under the terms of the license
// agreement contained in the LICENSE file in the top level
// of this distribution.
//

using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections;

namespace Ice {
  public enum EndpointType : short {
    Unknown = 0,
    Tcp = 1,
    Ssl = 2,
    Udp = 3
  }

  // Endpoint
  //
  // An endpoint is an encapsulation of the Ice endpoint
  // parameters, as well as functions to obtain
  // communication streams and methods that correspond
  // to the endpoint.  Endpoints should generally always
  // go through Ice.Manager.GetEndpoint().

  public abstract class Endpoint : ICloneable {
    internal EndpointType _type;
    protected System.IO.Stream _stream;
    protected ProtocolWriter _pw;
    protected ProtocolReader _pr;
    protected ReceiverDispatcher _rd;
    protected bool _incoming;

    public Endpoint () {
      _type = EndpointType.Unknown;
    }

    public Endpoint (EndpointType t) {
      _type = t;
    }

    public EndpointType Type {
      get {
        return _type;
      }
    }


    public object Clone () {
      return this.MemberwiseClone();
    }

    // true if this endpoint is used to listen for
    // connections
    public bool Incoming {
      get {
        return _incoming;
      }
      set {
        _incoming = value;
      }
    }

    // true if this endpoint is "connected" (i.e. tcp),
    // false if not (i.e udp)
    public abstract bool HasConnection { get; }

    public System.IO.Stream Stream {
      get {
        if (_stream == null)
          CreateStream();
        return _stream;
      }
    }

    public ProtocolWriter ProtocolWriter {
      get {
        if (_pw == null)
          CreateProtocolWriter();
        return _pw;
      }
      set {
        _pw = value;
      }
    }

    public ProtocolReader ProtocolReader {
      get {
        if (_pr == null)
          CreateProtocolReader();
        return _pr;
      }
      set {
        _pr = value;
      }
    }

    public ReceiverDispatcher ReceiverDispatcher {
      get {
        if (_rd == null)
          CreateReceiverDispatcher();
        return _rd;
      }
      set {
        _rd = value;
      }
    }

    protected abstract void CreateStream();

    protected virtual void CreateProtocolWriter() {
      _pw = new ProtocolWriter (this.Stream);
    }

    protected virtual void CreateProtocolReader() {
      _pr = new ProtocolReader (this.Stream);
    }

    protected virtual void CreateReceiverDispatcher() {
      _rd = new ReceiverDispatcher (this);
    }
  }

  public abstract class IpEndpoint : Endpoint {
    public string host;
    public int port;

    protected Socket _socket;
    protected IPAddress _ipaddr;

    public IpEndpoint()
    {
    }

    public IpEndpoint(EndpointType t)
      : base (t)
    {
    }

    public Socket Socket {
      get {
        if (_socket == null)
          CreateSocket();
        return _socket;
      }
      set {
        _socket = value;
      }
    }

    public IPAddress IPAddress {
      get {
        if (_ipaddr == null)
          CreateIPAddress();
        return _ipaddr;
      }
      set {
        _ipaddr = value;
      }
    }

    protected abstract void CreateSocket();

    protected virtual void CreateIPAddress() {
      IPHostEntry iphe = Dns.Resolve (host);
      if (iphe.AddressList.Length > 0)
        _ipaddr = iphe.AddressList[0];
    }

    protected override void CreateStream() {
      _stream = new NetworkStream (this.Socket, true);
    }
  }

  public class TcpEndpoint : IpEndpoint {
    public int timeout;
    public bool compress;

    public override bool Equals (object other) {
      TcpEndpoint te = other as TcpEndpoint;
      if (te != null &&
          te.host == host &&
          te.port == port)
      {
        return true;
      }
      return false;
    }

    public override int GetHashCode () {
      return host.GetHashCode() + port.GetHashCode();
    }

    public TcpEndpoint ()
    {
    }

    public TcpEndpoint (string h, int p)
      : base (EndpointType.Tcp)
    {
      host = h;
      port = p;
      timeout = 10000;
      compress = false;
    }

    public override bool HasConnection {
      get {
        return true;
      }
    }

    protected override void CreateSocket () {
      Socket sock = null;

      // try to use a given ipaddr, if one was given
      if (_ipaddr != null) {
        IPEndPoint ipe = new IPEndPoint (_ipaddr, port);
        sock = new Socket (ipe.AddressFamily,
                           SocketType.Stream,
                           ProtocolType.Tcp);
        sock.Connect (ipe);
      }

      if (sock == null || !sock.Connected) {
        IPHostEntry iphe = Dns.Resolve (host);
        foreach (IPAddress ipaddr in iphe.AddressList) {
          IPEndPoint ipe = new IPEndPoint (ipaddr, port);
          sock = new Socket (ipe.AddressFamily,
                             SocketType.Stream,
                             ProtocolType.Tcp);
          sock.Connect (ipe);
          if (sock.Connected) {
            _ipaddr = ipaddr;
          }
        }
      }

      if (sock.Connected) {
        _socket = sock;
        return;
      }

      throw new Ice.UnknownException ();
    }

  }

  public class SslEndpoint : IpEndpoint {
    public int timeout;
    public bool compress;

    public SslEndpoint ()
    {
    }

    public SslEndpoint (string h, int p)
      : base (EndpointType.Ssl)
    {
      host = h;
      port = p;
      timeout = 0;
      compress = false;
    }

    public override bool HasConnection {
      get {
        return true;
      }
    }

    protected override void CreateSocket () {
      throw new NotSupportedException ("SslEndpoint: Can't create SSL Sockets");
    }
  }

  public class UdpEndpoint : IpEndpoint {
    public byte protocolMajor;
    public byte protocolMinor;
    public byte encodingMajor;
    public byte encodingMinor;
    public bool compress;

    public UdpEndpoint ()
    {
    }

    public UdpEndpoint (string h, int p)
      : base (EndpointType.Udp)
    {
      host = h;
      port = p;
      protocolMajor = 1;
      protocolMinor = 0;
      encodingMajor = 1;
      encodingMinor = 0;
      compress = false;
    }

    public override bool HasConnection {
      get {
        return false;
      }
    }

    protected override void CreateSocket () {
      Socket sock = null;

      IPHostEntry iphe = Dns.Resolve (host);
      if (iphe.AddressList.Length > 0) {
        IPEndPoint ipe = new IPEndPoint (iphe.AddressList[0], port);
        sock = new Socket (ipe.AddressFamily,
                           SocketType.Dgram,
                           ProtocolType.Udp);
      }

      if (sock != null) {
        _socket = sock;
      } else {
        throw new Ice.UnknownException ();
      }
    }
  }
}
