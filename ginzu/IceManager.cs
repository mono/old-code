// -*- mode: csharp; c-basic-offset: 2; indent-tabs-mode: nil -*-
//
// IceManager.cs
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
using System.Threading;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;
using System.Diagnostics;
using System.IO;

namespace Ice {

  // Manager
  // 
  // Ice.Manager performs some global housekeeping operations.
  // It is a singleton; all access must be through GetManager().
  
  public class Manager {
    private static Manager _mgr = null;

    private Hashtable _endpointMap;

    public static Manager GetManager () {
      if (_mgr == null)
        _mgr = new Manager();
      return _mgr;
    }

    protected Manager () {
      _endpointMap = new Hashtable();
    }

    public Ice.Endpoint GetEndpoint (Ice.Endpoint e) {
      lock (_endpointMap) {
        if (_endpointMap.Contains(e)) {
          Ice.Endpoint ret = (Ice.Endpoint) _endpointMap[e];
          Trace.WriteLine ("GetEndpoint: NEW " + e);
          return ret;
        } else {
          _endpointMap[e] = e;
          Trace.WriteLine ("GetEndpoint: OLD " + e);
          return e;
        }
      }
    }

    public void ReleaseEndpoint (Ice.Endpoint e) {
      lock (_endpointMap) {
        Console.WriteLine ("Not sure how to release endpoints!!");
      }
    }

    public void Destroy () {
    }

    public void Shutdown () {
    }

    public void WaitForShutdown () {
    }

    public Ice.Object GetProxy (string url)
    {
      return GetProxy (url, typeof (Ice.Object));
    }

    public Ice.Object GetProxy (string url, Type t) 
    {
      Ice.Proxy rprx = new Ice.Proxy (t, url);
      Ice.Object prx = (Ice.Object) rprx.GetTransparentProxy();
      return prx;
    }

    public Ice.Object GetProxy (Ice.Identity id, Ice.Endpoint ep)
    {
      return GetProxy (id, ep, typeof(Ice.Object));
    }

    public Ice.Object GetProxy (Ice.Identity id, Ice.Endpoint ep, Type t)
    {
      throw new NotImplementedException();
    }

    public Ice.Object StringToProxy (string str) {
      throw new NotImplementedException();
    }

    public string ProxyToString (Ice.Object obj) {
      throw new NotImplementedException();
    }

  }

}
