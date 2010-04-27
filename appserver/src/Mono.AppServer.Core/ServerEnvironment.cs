//
// Mono.AppServer.ServerEnvironment
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;

namespace Mono.AppServer
{
  public class ServerEnvironment : MarshalByRefObject
  {
    public ServerEnvironment()
    {
    }

    public long TickCount
    {
      get
      {
        return Environment.TickCount;
      }
    }

    public PlatformID Platform
    {
      get
      {
        return Environment.OSVersion.Platform;
      }
    }

    public string MachineName
    {
      get
      {
        return Environment.MachineName;
      }
    }

    public Version OSVersion
    {
      get
      {
        return Environment.OSVersion.Version;
      }
    }

    public Version CLRVersion
    {
      get
      {
        return Environment.Version;
      }
    }
  }
}