// -*- mode: csharp; c-basic-offset: 2; indent-tabs-mode: nil -*-
//
// IceServerTransportSink.cs
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

namespace Ice {

  public class IceServerTransportSink
    : IServerChannelSink, IChannelSinkBase
  {
    IServerChannelSink _next;

    public IceServerTransportSink (IServerChannelSink next) {
      _next = next;
    }

    public IServerChannelSink NextChannelSink {
      get {
        return _next;
      }
    }

    public IDictionary Properties {
      get {
        throw new NotImplementedException();
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
      throw new NotImplementedException();
    }

    public ServerProcessing ProcessMessage (IServerChannelSinkStack sinkStack,
                                            IMessage requestMsg,
                                            ITransportHeaders requestHeaders,
                                            Stream requestStream,
                                            out IMessage responseMsg,
                                            out ITransportHeaders responseHeaders,
                                            out Stream responseStream)
    {
      // this sink is always first, and this method isn't usued as the entry point
      throw new NotSupportedException();
    }

    internal void InternalProcessMessage (Ice.Endpoint replyEp, Stream msgStream, Ice.MessageType mtype)
    {
      // at this point, the ice header has been read, but nothing higher.
      // we don't know very much about this message; we pass it up
      // to (presumably) the formatter for further evaluation

      TransportHeaders headers = new TransportHeaders();
      headers ["__iceMessageType"] = mtype;
      headers ["__iceEndpoint"] = replyEp;

      IMessage respMessage;
      ITransportHeaders respHeaders;
      Stream respStream;

      ServerProcessing res = _next.ProcessMessage (null, null, headers, msgStream,
                                                   out respMessage, out respHeaders,
                                                   out respStream);
      switch (res) {
      case ServerProcessing.Complete:
        if (!(respHeaders != null &&
              respHeaders["__iceNoReply"] != null &&
              ((bool) respHeaders["__iceNoReply"])))
        {
          lock (replyEp.Stream) {
            MemoryStream ms = respStream as MemoryStream;
            ms.WriteTo (replyEp.Stream);
          }
          
        }
        break;
      case ServerProcessing.Async:
        throw new NotImplementedException();
      case ServerProcessing.OneWay:
        // do nothing
        break;
      }
    }
  }

}
