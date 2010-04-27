// -*- mode: csharp; c-basic-offset: 2; indent-tabs-mode: nil -*-
//
// IceServerFormatterSinkProvider.cs
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

  public class IceServerFormatterSinkProvider
    : IServerFormatterSinkProvider, IServerChannelSinkProvider
  {
    IServerChannelSinkProvider _next;

    public IceServerFormatterSinkProvider () {
      _next = null;
    }

    public IceServerFormatterSinkProvider (IDictionary props,
                                           ICollection provData)
    {
      throw new NotImplementedException();
    }

    public IServerChannelSinkProvider Next {
      get {
        return _next;
      }
      set {
        _next = value;
      }
    }

    public IServerChannelSink CreateSink (IChannelReceiver channel) {
      IServerChannelSink nextSink = null;
      IceServerFormatterSink result;

      if (_next != null)
        nextSink = _next.CreateSink (channel);

      result = new IceServerFormatterSink (nextSink);

      return result;
    }

    public void GetChannelData (IChannelDataStore channelData) {
      // nothing to do; there should be...
    }
  }

}


