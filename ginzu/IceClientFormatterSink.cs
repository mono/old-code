// -*- mode: csharp; c-basic-offset: 2; indent-tabs-mode: nil -*-
//
// IceClientFormatterSink.cs
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
using System.Diagnostics;


namespace Ice {

  public class IceClientFormatterSink :
    IClientFormatterSink, IMessageSink, IClientChannelSink, IChannelSinkBase
  {
    IClientChannelSink _next;

    public IceClientFormatterSink (IClientChannelSink nextSink) {
      _next = nextSink;
    }

    public IClientChannelSink NextChannelSink {
      get {
        return _next;
      }
    }

    public IMessageSink NextSink {
      get {
        return (IMessageSink) _next;
      }
    }

    public IDictionary Properties {
      get {
        return null;
      }
    }

    public Stream GetRequestStream (IMessage msg,
                                    ITransportHeaders headers)
    {
      throw new NotSupportedException ();
    }

    public IMessageCtrl AsyncProcessMessage (IMessage msg,
                                             IMessageSink replySink)
    {
      Stream reqStream;
      FormatMessage (msg, out reqStream);

      TransportHeaders reqHeaders = new TransportHeaders();
      ClientChannelSinkStack stack = new ClientChannelSinkStack (replySink);
      stack.Push (this, msg);

      _next.AsyncProcessRequest (stack, msg, reqHeaders, reqStream);

      return null;
    }

    public void AsyncProcessRequest (IClientChannelSinkStack sinkStack,
                                     IMessage msg,
                                     ITransportHeaders headers,
                                     Stream stream)
    {
      // should never be called; we're a message transport
      throw new NotSupportedException();
    }

    public void AsyncProcessResponse (IClientResponseChannelSinkStack sinkStack,
                                      object state,
                                      ITransportHeaders headers,
                                      Stream stream)
    {
      IMessage reqMessage = (IMessage) state;
      IMessage replyMessage = (IMessage) IceChannelUtils.ProtocolReplyToMessage (stream, reqMessage);
      stream.Close();

      sinkStack.DispatchReplyMessage (replyMessage);
    }

    public void ProcessMessage (IMessage msg,
                                ITransportHeaders requestHeaders,
                                Stream requestStream,
                                out ITransportHeaders responseHeaders,
                                out Stream responseStream)
    {
      // never called; this should always be the first sink in the
      // chain.
      throw new InvalidOperationException();
    }

    public IMessage SyncProcessMessage (IMessage msg)
    {
      IMethodCallMessage mcall = msg as IMethodCallMessage;

      Trace.WriteLine ("IceClientFormatterSink: ProcessMessage: " + mcall.MethodBase);

      try {
        Stream msgStream;

        FormatMessage (msg, out msgStream);

        // send downstream for processing
        TransportHeaders reqHeaders = new TransportHeaders();
        ITransportHeaders respHeaders;
        Stream respStream;
        _next.ProcessMessage (msg, reqHeaders, msgStream, out respHeaders, out respStream);

        // convert back into a response message
        IMessage result = (IMessage) IceChannelUtils.ProtocolReplyToMessage (respStream, msg);
        respStream.Close();

        return result;
      } catch (Exception e) {
        return new ReturnMessage (e, mcall);
      }
    }

    private void FormatMessage (IMessage msg, out Stream msgStream)
    {
      IMethodCallMessage mcall = msg as IMethodCallMessage;

      // if the identity wasn't set by the custom proxy, then we
      // extract it from the uri
      if (mcall.LogicalCallContext.GetData ("__iceIdentity") == null) {
        // extract the Identity from the URI
        // FIXME FIXME FIXME -- this is supposed to be a Uri, why's it look like a Url??
        Trace.WriteLine ("IceClientFormatterSink: using identity from " + mcall.Uri);
        if (mcall.Uri == null)
          throw new Exception("__iceIdentity property not set by IceClientFormatterSink upstream, and mcall.Uri == null!");

        string h, r;
        int p;
        IceChannelUtils.ParseIceURL (mcall.Uri, out h, out p, out r);
        mcall.LogicalCallContext.SetData ("__iceIdentity", new Ice.Identity (r));
      }

      mcall.LogicalCallContext.SetData ("__iceFacetPath", new string[0]);
      mcall.LogicalCallContext.SetData ("__iceContext", new Ice.Context());
        
      OperationMode opmode = OperationMode.Normal;
      Ice.OperationModeAttribute opmodeattr =
        (Ice.OperationModeAttribute) Attribute.GetCustomAttribute (mcall.MethodBase, typeof(Ice.OperationModeAttribute));
      if (opmodeattr != null) opmode = opmodeattr.mode;
      mcall.LogicalCallContext.SetData ("__iceOperationMode", opmode);

      if (Attribute.GetCustomAttribute (mcall.MethodBase, typeof(OneWayAttribute)) != null)
        mcall.LogicalCallContext.SetData ("__iceOneWay", true);
      else
        mcall.LogicalCallContext.SetData ("__iceOneWay", false);

      msgStream = new MemoryStream();
      IceChannelUtils.MessageToProtocolRequest (msgStream, msg);
    }

  }

}
