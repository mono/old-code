// -*- mode: csharp; c-basic-offset: 2; indent-tabs-mode: nil -*-
//
// IceServerChannel.cs
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
using System.IO;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels;

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Ice {

  public class IceServerChannel : IChannelReceiver, IChannel {
    string _name = "ice";
    int _priority = 1;
    IceServerTransportSink _sink;

    Thread _listenerThread;
    TcpListener _listener;
    ChannelDataStore _channelData;

    Ice.Endpoint _e;

    void Init (IServerChannelSinkProvider provider) {
      if (!_e.Incoming)
        throw new InvalidOperationException("Non-listening Endpoint passed to IceServerChannel");

      if (!(_e is TcpEndpoint))
        throw new NotSupportedException("Only TcpEndpoints are supported as servers (for now)");

      if (provider == null) {
        provider = new IceServerFormatterSinkProvider();
      }

      IServerChannelSink nextSink = ChannelServices.CreateServerChannelSinkChain (provider, this);

      TcpEndpoint te = _e as TcpEndpoint;

      string[] uris = null;
      if (te.port != 0) {
        uris = new string[1];
        uris[0] = "ice://" + te.host + ":" + te.port;
      }

      _channelData = new ChannelDataStore (uris);
      _channelData["__iceEndpoint"] = _e;

      _sink = new IceServerTransportSink (nextSink);
      //      _listener = new TcpListener (te.IPAddress, te.port);
      _listener = new TcpListener (IPAddress.Any, te.port);
      _listenerThread = null;

      StartListening (null);
    }

    public IceServerChannel (Ice.Endpoint ep) {
      _e = Ice.Manager.GetManager().GetEndpoint (ep);
      Init (null);
    }

    public IceServerChannel (IDictionary properties,
                             IServerChannelSinkProvider serverSinkProvider)
    {
      throw new NotImplementedException();
    }

    public IceServerChannel (Ice.Endpoint ep,
                             IServerChannelSinkProvider serverSinkProvider)
    {
      _e = Ice.Manager.GetManager().GetEndpoint (ep);
      Init (serverSinkProvider);
    }

    // IChannel
    public string ChannelName {
      get {
        return _name;
      }
    }

    public int ChannelPriority {
      get {
        return _priority;
      }
    }

    public string Parse (string url, out string objectURI)
    {
      string host;
      int port;
      string rest;

      IceChannelUtils.ParseIceURL (url, out host, out port, out rest);

      objectURI = rest;

      return "ice://" + host + ":" + port;
    }

    // IChannelReceiver
    public object ChannelData {
      get {
        return _channelData;
      }
    }

    public string[] GetUrlsForUri (string uri)
    {
      Ice.TcpEndpoint te = _e as Ice.TcpEndpoint;
      if (te == null)
        return null;
      string[] ret = new string[1];
      ret[0] = "ice://" + te.host + ":" + te.port + "/" + uri;
      return ret;
    }

    public void StartListening (object data)
    {
      Ice.TcpEndpoint te = _e as Ice.TcpEndpoint;

      if (_listenerThread == null) {
        _listener.Start();
        if (te.port == 0) {
          te.port = ((IPEndPoint) _listener.LocalEndpoint).Port;
          _channelData.ChannelUris = new string[1];
          _channelData.ChannelUris[0] = "ice://" + te.host + ":" + te.port;
        }

        _listenerThread = new Thread (new ThreadStart (ListenerThread));
        _listenerThread.Start();
      }
    }

    public void StopListening (object data)
    {
      if (_listenerThread != null) {
        _listener.Stop();
        _listenerThread.Abort();
        _listenerThread = null;
      }
    }

    private void ListenerThread ()
    {
      if (_e is Ice.TcpEndpoint) {
        // loop and accept socket connections, then convert them
        // into Endpoints, and instantiate a ReceiverDispatcher for
        // them.
        while (true) {
          Socket clientSocket = _listener.AcceptSocket();

          Ice.Endpoint ne = (Ice.Endpoint) _e.Clone();
          Ice.TcpEndpoint tcpep = ne as Ice.TcpEndpoint;

          if (tcpep == null) {
            throw new NotSupportedException ("Only TCP endpoints are supported");
          }

          tcpep.Socket = clientSocket;

          if (tcpep.HasConnection) {
            // validate the connection
            MemoryStream ms = new MemoryStream();
            Ice.ProtocolWriter pw = new Ice.ProtocolWriter(ms);
            pw.BeginMessage (MessageType.ValidateConnection);
            pw.EndMessage ();
            ms.WriteTo (tcpep.Stream);
          }

          tcpep.ReceiverDispatcher = new Ice.ReceiverDispatcher (tcpep, new MessageRequestDelegate (MessageRequestHandler));
        }
      } else {
        throw new NotImplementedException("ListenerThread managed to get a non-Tcp Ice.Endpoint");
      }
    }

    private void MessageRequestHandler (Ice.Endpoint e,
                                        Stream msgStream,
                                        Ice.MessageType mtype)
    {
      _sink.InternalProcessMessage (e, msgStream, mtype);
    }

  }

}
