// -*- mode: csharp; c-basic-offset: 2; indent-tabs-mode: nil -*-
//
// IceClientFormatterSinkProvider.cs
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

  public class IceClientFormatterSinkProvider :
    IClientFormatterSinkProvider, IClientChannelSinkProvider
  {
    IClientChannelSinkProvider _next = null;

    public IceClientFormatterSinkProvider ()
    {
    }

    public IceClientFormatterSinkProvider (IDictionary props,
                                           ICollection providerData)
    {
    }

    public IClientChannelSinkProvider Next
    {
      get {
        return _next;
      }
      set {
        _next = value;
      }
    }

    public IClientChannelSink CreateSink (IChannelSender channel,
                                          string url,
                                          object remoteChannelData)
    {
      IClientChannelSink nextsink = null;
      IceClientFormatterSink result;

      if (_next != null)
        nextsink = _next.CreateSink (channel, url, remoteChannelData);

      result = new IceClientFormatterSink (nextsink);

      return result;
    }
  }

}
