// -*- mode: csharp; c-basic-offset: 2; indent-tabs-mode: nil -*-
//
// IceClientTransportSinkProvider.cs
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

  public class IceClientTransportSinkProvider
    : IClientChannelSinkProvider
  {
    Hashtable _uriMap;

    public IceClientTransportSinkProvider ()
    {
      _uriMap = new Hashtable();
    }

    public IClientChannelSinkProvider Next {
      get {
        return null;
      }
      set {
        // nothing
      }
    }

    public IClientChannelSink CreateSink (IChannelSender channel,
                                          string url,
                                          object remoteChannelData)
    {
      string host;
      int port;
      string rest;
      IceChannelUtils.ParseIceURL (url, out host, out port, out rest);

      string key = host + ":" + port;

      if (!_uriMap.Contains (key)) {
        IClientChannelSink cs = new IceClientTransportSink (url);
        _uriMap[key] = cs;
      }

      return (IClientChannelSink) _uriMap[key];
    }

  }

}
