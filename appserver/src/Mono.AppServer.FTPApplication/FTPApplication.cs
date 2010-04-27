//
// Mono.AppServer.FTPApplication
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;
using System.IO;
using System.Xml;
using System.Collections;
using Mono.AppServer.Security;

namespace Mono.AppServer
{
  public class FTPApplication : ApplicationBase
  {
    protected FTP_Server.FTPServer ftpserver;
    protected FTPClientCollection _Clients;
    protected int _port;
    protected int _path;

    public FTPClientCollection Clients
    {
      get
      {
        return _Clients;
      }
    }

    public FTPApplication() : base()
    {
    }

    public FTPApplication(int Port, string AppName, string BaseDirectory, ApplicationServer server) : base(AppName,BaseDirectory,server)
    {
      _Name=AppName;
      _BaseDirectory=BaseDirectory;
      _port=Port;
    }

    public int Port
    {
      get
      {
        return ftpserver.Port;
      }
    }

    public override void Unload()
    {
      if (ftpserver != null)          
        ftpserver.EndSession();
      if (_Clients != null)
        _Clients.Clear();
      _Loaded=false;
    }

    public override void Load()
    {
      if (ftpserver==null)
      {
        ftpserver=new FTP_Server.FTPServer();
        ftpserver.OnAuthenticate+=new FTP_Server.Authenticate(Authenticate);
        ftpserver.OnConnected+=new FTP_Server.Connected(Connected);
        ftpserver.OnConnect+=new FTP_Server.Connect(Connect);
        ftpserver.OnDisconnect+=new FTP_Server.Disconnect(Disconnect);
        ftpserver.OnDisconnected+=new FTP_Server.Disconnected(Disconnected);
      }
      ftpserver.Start(_port,BaseDirectory);
      _Loaded=true;
    }

    public bool Authenticate(int ClientID,string Username, string Password)
    {
      if (SecurityManager.Authenticate(Username,Password))
      {
        FTPClient ClientInfo=Clients[ClientID];
        ClientInfo.Username=Username;
        return true;
      }
      else
        return false;
    }

    public void Connected(int ClientID,string IP)
    {
      FTPClient ClientInfo=new FTPClient();
      ClientInfo.ClientID=ClientID;
      ClientInfo.IPAddr=IP;
      Clients.Add(ClientInfo);
    }

    public void Connect(int ClientID, string Msg)
    {
      FTPClient ClientInfo=Clients[ClientID];
      if (Msg.Substring(4)=="PASS")
        Msg="PASS xxxxxx";
      else
        Msg=Msg.Replace("\r","").Replace("\n","");
      Log.WriteLine("System",ClientID.ToString()+": "+Msg);
    }

    public void Disconnect(int ClientID)
    {
      Clients.Remove(ClientID);
    }

    public void Disconnected(int ClientID, string IP)
    {
      Clients.Remove(ClientID);
    }

    public override void Configure(ApplicationServer server, DirectoryInfo BaseDir, XmlDocument ConfigFile)
    {
      base.Configure(server,BaseDir,ConfigFile);
      _port=int.Parse(ReadAppSetting(ConfigFile,"FTPApplication.Port","21"));
      _BaseDirectory=ReadAppSetting(ConfigFile,"FTPApplication.Path",BaseDir.FullName);
      _Clients=new FTPClientCollection();
    }
  }
}