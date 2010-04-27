// -*- mode: csharp; c-basic-offset: 2; indent-tabs-mode: nil -*-
//
// IceChannel.cs
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

  public class IceChannel
    : IChannelReceiver, IChannelSender, IChannel
  {
    IceServerChannel _svrChannel;
    IceClientChannel _cltChannel;

    string name = "ice";

    public IceChannel() 
      : this(0)
    {
    }

    public IceChannel(int port) {
      Ice.TcpEndpoint te = new Ice.TcpEndpoint ("localhost", port);
      te.Incoming = true;
      _svrChannel = new IceServerChannel (te);
      _cltChannel = new IceClientChannel ();
    }

    public IceChannel (IDictionary props,
                       IClientChannelSinkProvider clientSinkProvider,
                       IServerChannelSinkProvider serverSinkProvider)
    {
      throw new NotImplementedException();
    }

    public object ChannelData {
      get {
        return _svrChannel.ChannelData;
      }
    }

    public string ChannelName {
      get {
        return name;
      }
    }

    public int ChannelPriority {
      get {
        return _svrChannel.ChannelPriority;
      }
    }

    public IMessageSink CreateMessageSink (string url,
                                           object remoteChannelData,
                                           out string objectUri)
    {
      return _cltChannel.CreateMessageSink (url, remoteChannelData, out objectUri);
    }

    public string[] GetUrlsForUri (string objectUri) {
      return _svrChannel.GetUrlsForUri (objectUri);
    }

    public string Parse (string url, out string objectUri) {
      return _svrChannel.Parse (url, out objectUri);
    }

    public void StartListening (object data) {
      _svrChannel.StartListening (data);
    }

    public void StopListening (object data) {
      _svrChannel.StopListening (data);
    }
  }

}
