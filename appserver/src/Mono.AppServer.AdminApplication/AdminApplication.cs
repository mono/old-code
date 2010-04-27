//
// Mono.AppServer.AdminApplication
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;
using System.Xml;
using System.IO;
using System.Configuration;

namespace Mono.AppServer
{
  public class AdminApplication : WebApplication 
  {
    private FTPApplication _FTPApplication;
    private string _ApplicationBaseDir;
    private bool _FTPEnabled;
    public int FTPPort;

    public AdminApplication() : base()
    {
    }

    protected void StartFTP(int FTPPort)
    {
      EnterMethod();
      try
      {
        this.FTPPort=FTPPort;
        _FTPApplication=new FTPApplication(FTPPort, this.Name+".FTP", _ApplicationBaseDir, this.AppServer);
        _FTPApplication.Load();
      }
      catch(Exception E)
      {
        SendError(E);
        throw;
      }
      finally
      {
        ExitMethod();
      }
    }

    protected void StopFTP()
    {
      EnterMethod();
      try
      {
        if (FTPApplication!=null)
        {
          FTPApplication.Unload();
          _FTPApplication=null;
        }
      }
      catch(Exception E)
      {
        SendError(E);
        throw;
      }
      finally
      {
        ExitMethod();
      }
    }

    public bool FTPEnabled
    {
      get
      {
        return (FTPApplication!=null);
      }
      set
      {
        if (value && FTPApplication==null)
          StartFTP(FTPPort);
        else
          StopFTP();
      }
    }

    public FTPApplication FTPApplication
    {
      get
      {
        return _FTPApplication;
      }
    }

    public override void Load()
    {
      EnterMethod();
      try
      {
        if (FTPApplication!=null)
          FTPApplication.Load();
        base.Load();			
      }
      catch(Exception E)
      {
        SendError(E);
        throw;
      }
      finally
      {
        ExitMethod();
      }
    }

    public override void Unload()
    {
      EnterMethod();
      try
      {
        if (FTPApplication != null)
          FTPApplication.Unload();
        base.Unload();
      }
      catch(Exception E)
      {
        SendError(E);
        throw;
      }
      finally
      {
        ExitMethod();
      }
    }

    public override void Configure(ApplicationServer server, DirectoryInfo BaseDir, XmlDocument ConfigFile)
    {
      EnterMethod();
      try
      {
        base.Configure(server,BaseDir,ConfigFile);
        _ApplicationBaseDir=server.ApplicationsBaseDir;
        SendValue("Application BaseDir", _ApplicationBaseDir);
        _Port=int.Parse(ReadAppSetting(ConfigFile,"AdminApplication.Port","8080"));        
        SendValue("Port", _Port);
        FTPPort=int.Parse(ReadAppSetting(ConfigFile,"AdminApplication.FTPPort","8021"));
        SendValue("FTP Port", FTPPort);
        _FTPEnabled=bool.Parse(ReadAppSetting(ConfigFile,"AdminApplication.FTPEnabled","false"));
        SendValue("FTP Enabled", _FTPEnabled);
        if (_FTPEnabled)
        {
          _FTPApplication=new FTPApplication(FTPPort, this.Name+".FTP", AppServer.ApplicationsBaseDir, this.AppServer);
        }
      }
      catch(Exception E)
      {
        SendError(E);
        throw;
      }
      finally
      {
        ExitMethod();
      }
    }
  }
}