// -*- mode: csharp; c-basic-offset: 2; indent-tabs-mode: nil -*-
//
// IceClientChannel.cs
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

namespace Ice {

  public class IceClientChannel : IChannelSender, IChannel {
    private int _priority = 1;
    private string _name = "ice";
    private IClientChannelSinkProvider _sinkProvider;

    public IceClientChannel ()
    {
      _sinkProvider = new IceClientFormatterSinkProvider();
      _sinkProvider.Next = new IceClientTransportSinkProvider();
    }

    public IceClientChannel (IDictionary properties, IClientChannelSinkProvider sp)
    {
      // properties are ignored for now

      _sinkProvider = sp;

      // find the end of the chain to put the transport provider
      IClientChannelSinkProvider prov = _sinkProvider;
      while (prov.Next != null) prov = prov.Next;

      prov.Next = new IceClientTransportSinkProvider();
    }

    public IceClientChannel (string n, IClientChannelSinkProvider sp)
    {
      _name = n;
      _sinkProvider = sp;

      // find the end of the chain to put the transport provider
      IClientChannelSinkProvider prov = _sinkProvider;
      while (prov.Next != null) prov = prov.Next;

      prov.Next = new IceClientTransportSinkProvider();
    }

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

    public IMessageSink CreateMessageSink (string url,
                                           object remoteChannelData,
                                           out string objectURI)
    {
      if (url != null) {
        if (Parse (url, out objectURI) != null)
          return (IMessageSink) _sinkProvider.CreateSink (this, url, remoteChannelData);
      }

      if (remoteChannelData != null) {
        IChannelDataStore ds = remoteChannelData as IChannelDataStore;
        if (ds != null) {
          foreach (string chURI in ds.ChannelUris) {
            if (Parse (chURI, out objectURI) == null)
              continue;
            return (IMessageSink) _sinkProvider.CreateSink (this, chURI, remoteChannelData);
          }
        }
      }

      objectURI = null;
      return null;
    }

    public string Parse (string url, out string objectURI)
    {
      int port;
      string host;
      string uri;

      objectURI = null;

      if (!IceChannelUtils.ParseIceURL (url, out host, out port, out uri))
        return null;

      objectURI = uri;
      return "ice://" + host + ":" + port;
    }
  }
}
