// -*- mode: csharp; c-basic-offset: 2; indent-tabs-mode: nil -*-
//
// IceServerFormatterSink.cs
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
using System.Reflection;
using System.Diagnostics;

namespace Ice {

  public class IceServerFormatterSink
    : IServerChannelSink, IChannelSinkBase
  {
    IServerChannelSink _next;

    public IceServerFormatterSink (IServerChannelSink next) {
      _next = next;
    }

    public IServerChannelSink NextChannelSink {
      get {
        return _next;
      }
      set {
        _next = value;
      }
    }

    public IDictionary Properties {
      get {
        return null;
      }
    }

    public void AsyncProcessResponse (IServerResponseChannelSinkStack sinkStack,
                                      object state,
                                      IMessage msg,
                                      ITransportHeaders headers,
                                      Stream stream)
    {
      throw new NotImplementedException();
    }

    public Stream GetResponseStream (IServerResponseChannelSinkStack sinkStack,
                                     object state,
                                     IMessage msg,
                                     ITransportHeaders headers)
    {
      throw new NotSupportedException();
    }

    public ServerProcessing ProcessMessage (IServerChannelSinkStack sinkStack,
                                            IMessage requestMsg,
                                            ITransportHeaders requestHeaders,
                                            Stream requestStream,
                                            out IMessage responseMsg,
                                            out ITransportHeaders responseHeaders,
                                            out Stream responseStream)
    {
      IMessage call;
      MessageType mtype = (Ice.MessageType) requestHeaders["__iceMessageType"];
      bool isBatched = (mtype == Ice.MessageType.BatchRequest);

      try {
        if (requestMsg == null) {
          requestMsg = IceChannelUtils.ProtocolRequestToMessage (requestStream, isBatched);
        } else {
          call = requestMsg;
        }

        Trace.WriteLine ("IceServerFormatterSink: passing upstream");
        _next.ProcessMessage (sinkStack, requestMsg, requestHeaders, null,
                              out responseMsg, out responseHeaders, out responseStream);
        Trace.WriteLine ("IceServerFormatterSink: returned");

        responseStream = new MemoryStream();
        IceChannelUtils.MessageToProtocolReply (requestMsg, responseMsg, responseStream);
      } catch (Exception e) {
        Console.WriteLine (e.ToString());
        throw e;
      }
      return ServerProcessing.Complete;
    }
  }

}
