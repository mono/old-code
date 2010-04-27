// -*- mode: csharp; c-basic-offset: 2; indent-tabs-mode: nil -*-
//
// IceProxy.cs
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
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Reflection;
using System.IO;

namespace Ice {

  public class Proxy : RealProxy, IRemotingTypeInfo {
    private Ice.Identity _identity;
    private string _url;
    private string _uri;
    private string[] _facetPath;
    //    public ProxyMode mode;
    private bool _secure;

    private string[] _typeIds;

    private IMessageSink _sinkChain;

    // if it's a remote proxy, it has an endpoint
    // private Ice.Endpoint _endpoint;

    // if it's a local servant, it has a realObject
    private Ice.Object _realObject;

    public Proxy (string url)
      : this (typeof (Ice.Object), url)
    {
    }

    public Proxy (Type rootType,
                  string url)
      : base (rootType)
    {
      string host;
      int port;
      string rest;
      IceChannelUtils.ParseIceURL (url, out host, out port, out rest);

      _url = url;
      _identity = new Ice.Identity (rest);
      _facetPath = new string[0];
      _secure = false;

      _typeIds = null;
      _realObject = null;

      FindChannel();
    }

    private void FindChannel() {
      IChannel[] registeredChannels = ChannelServices.RegisteredChannels;
      foreach (IChannel ch in registeredChannels) {
        if (ch is IChannelSender) {
          IChannelSender chs = (IChannelSender) ch;
          _sinkChain = chs.CreateMessageSink (_url, null, out _uri);
          if (_sinkChain != null)
            break;
        }
      }

      if (_sinkChain == null) {
        throw new Exception("No channel found for " + _url);
      }
    }


    public override IMessage Invoke (IMessage msg) {
      if (msg is IMethodCallMessage) {
        IMethodCallMessage mcall = msg as IMethodCallMessage;

        msg.Properties["__Uri"] = _url;

        if (mcall.LogicalCallContext == null)
          Console.WriteLine ("null!");

        mcall.LogicalCallContext.SetData ("__RequestUri", _uri);
        mcall.LogicalCallContext.SetData ("__iceIdentity", _identity);
        mcall.LogicalCallContext.SetData ("__iceSecure", _secure);

        IMessage retMsg = _sinkChain.SyncProcessMessage (msg);
        return retMsg;
      } else {
        throw new Ice.UnknownException();
      }
    }

    public override ObjRef CreateObjRef (Type requestedType) {
      Console.WriteLine ("CreateObjRef");
      return base.CreateObjRef(requestedType);
    }

    // IRemotingTypeInfo
    string type_name;
    string IRemotingTypeInfo.TypeName {
      get {
        Console.WriteLine ("IRemotingTypeInfo.get_TypeName");
        if (type_name == null)
          return "Ice.Object";  // ???
        return type_name;
      }
      set {
        type_name = value;
      }
    }

    bool IRemotingTypeInfo.CanCastTo (System.Type targetType, object o)
    {
      //      Console.WriteLine ("CanCastTo: " + targetType);

      // if this is a local servant, check it directly
      if (_realObject != null)
        return targetType.IsInstanceOfType (_realObject);

      // check if the thing was created with a valid type
      if (targetType.IsInstanceOfType (o))
        return true;

      // otherwise, perform the remote query if necessary
      string icename = IceUtil.TypeToIceName (targetType);

      if (_typeIds == null) {
        Ice.Object iob = o as Ice.Object;
        _typeIds = iob.ice_ids();
      }

      return ((IList) _typeIds).Contains (icename);
    }

  }
}
