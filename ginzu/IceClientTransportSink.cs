// -*- mode: csharp; c-basic-offset: 2; indent-tabs-mode: nil -*-
//
// IceClientTransportSink.cs
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
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Diagnostics;

namespace Ice {

  public class IceClientTransportSink
    : IClientChannelSink
  {
    string _host;
    int _port;
    string _objectUri;

    Ice.Endpoint _e;
    Ice.ReceiverDispatcher _rd;

    public IceClientTransportSink (string url)
    {
      IceChannelUtils.ParseIceURL (url, out _host, out _port, out _objectUri);

      Ice.TcpEndpoint te = new Ice.TcpEndpoint (_host, _port);
      _e = Ice.Manager.GetManager().GetEndpoint (te);
    }

    public IDictionary Properties {
      get {
        return null;
      }
    }

    public IClientChannelSink NextChannelSink {
      get {
        return null;
      }
    }

    public void AsyncProcessRequest (IClientChannelSinkStack sinkStack,
                                     IMessage msg,
                                     ITransportHeaders headers,
                                     Stream stream)
    {
      AsyncSendMessageStreamDelegate asmd = new AsyncSendMessageStreamDelegate (AsyncSendMessageStream);
      asmd.BeginInvoke (sinkStack, msg, headers, stream, null, null);
    }

    public void AsyncProcessResponse (IClientResponseChannelSinkStack sinkStack,
                                      object state,
                                      ITransportHeaders headers,
                                      Stream stream)
    {
      Console.WriteLine ("ClientTransport: AsyncProcessResponse");
      throw new NotSupportedException();
    }

    public Stream GetRequestStream (IMessage msg, ITransportHeaders headers)
    {
      return null;
    }

    public void ProcessMessage (IMessage msg,
                                ITransportHeaders requestHeaders,
                                Stream requestStream,
                                out ITransportHeaders responseHeaders,
                                out Stream responseStream)
    {
      IMethodCallMessage mcall = msg as IMethodCallMessage;

      if (_rd == null) {
        _rd = _e.ReceiverDispatcher;
      }

      int requestId = (int) mcall.LogicalCallContext.GetData("__iceRequestId");

      // Register the reply notification /before/ we even send the
      // request
      ManualResetEvent mre = null;

      if (requestId != 0)
         mre = _rd.RegisterReplyNotification (requestId);

      lock (_e.Stream) {
        ((MemoryStream) requestStream).WriteTo (_e.Stream);
      }

      // oneway invocation, nothing to respond
      if (requestId == 0) {
        responseHeaders = null;
        responseStream = null;
        return;
      }

      // wait for the reply to come in
      Trace.WriteLine ("ClientTransportSink Waiting on requestID " + requestId);
      mre.WaitOne();
      Trace.WriteLine ("ClientTransportSink got notification, requestID " + requestId);
      MemoryStream ms = _rd.GetMessageStream (requestId);

      responseStream = ms;
      responseHeaders = new TransportHeaders();
    }

    private delegate void AsyncSendMessageStreamDelegate (IClientChannelSinkStack sinkStack,
                                                          IMessage msg,
                                                          ITransportHeaders requestHeaders,
                                                          Stream requestStream);

    private void AsyncSendMessageStream (IClientChannelSinkStack sinkStack,
                                         IMessage msg,
                                         ITransportHeaders requestHeaders,
                                         Stream requestStream)
    {
      IMethodCallMessage mcall = msg as IMethodCallMessage;

      if (_rd == null) {
        _rd = _e.ReceiverDispatcher;
      }

      int requestId = (int) mcall.LogicalCallContext.GetData("__iceRequestId");
      ManualResetEvent mre = null;

      if (requestId != 0)
        mre = _rd.RegisterReplyNotification (requestId);

      lock (_e.Stream) {
        ((MemoryStream) requestStream).WriteTo (_e.Stream);
      }

      // oneway invocation, no response
      if (requestId == 0) {
        return;
      }

      mre.WaitOne();
      MemoryStream responseStream = _rd.GetMessageStream (requestId);
      TransportHeaders responseHeaders = new TransportHeaders();

      sinkStack.AsyncProcessResponse (responseHeaders, responseStream);
    }

  }

}
