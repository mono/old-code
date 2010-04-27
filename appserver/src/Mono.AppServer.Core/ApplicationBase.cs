//
// Mono.AppServer.ApplicationBase
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;
using System.Reflection;
using System.Threading;
using System.IO;
using System.Xml;

using TerWoord.Diagnostics;

using Mono.AppServer;
using Mono.AppServer.Security;

namespace Mono.AppServer
{
  public class ApplicationBase : MarshalByRefObject
  {
    private Category _LogCategory = new Category();
    protected AppDomain _Domain;
    protected string _BaseDirectory;
    protected string _Name;
    protected bool _Loaded;
    protected ApplicationLog log;
    protected ApplicationServer AppServer;
    protected SecurityManager SecurityManager;
    protected string[] Roles;
    
    protected void EnterMethod()
    {
      _LogCategory.EnterMethod(1);          
    }
    
    protected void ExitMethod()
    {
      _LogCategory.ExitMethod(1);
    }
    
    protected void SendString(string StrToSend)
    {
      _LogCategory.SendString(StrToSend);
    }
    
    protected void SendString(string StrToSend, params object[] Params)
    {
      _LogCategory.SendString(String.Format(StrToSend, Params));
    }
    
    protected void SendValue(string Message, object Value)
    {
      _LogCategory.SendValue(Message, Value);
    }
    
    protected void SendError(string Message)
    {
      _LogCategory.SendError(Message);
    }
    
    protected void SendError(Exception ExceptObj)
    {
      _LogCategory.SendError(ExceptObj);
    }

    public virtual void Load()
    {
    }
    
    public virtual void Unload()
    {
    }
    
    public virtual ApplicationAssembly[] GetLoadedAssemblies()
    {
      return null;
    }
    
    public virtual ApplicationType[] GetLoadedTypes()
    {
      return null;
    }

    public ApplicationBase()
    {
    }

    public ApplicationBase(string AppName, string BaseDir, ApplicationServer server):base()
    {
      _Name=AppName;
      _BaseDirectory=BaseDir;
      AppServer=server;
      SecurityManager=server.SecurityManager;        
      log = new ApplicationLog(_BaseDirectory + AppName + ".app.log");
      log.WriteLine("System","Application Started");
      _LogCategory.Name = _Name;
      EnterMethod();
      try
      {
        SendValue("Application Name", AppName);
        SendValue("Base Directory", BaseDir);
        SendValue("Log file", _BaseDirectory + AppName + ".app.log");
        SendString("App Started");      
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

    public static string ReadAppSetting(XmlDocument doc, string key, string DefaultValue)
    {    
      XmlElement setting=(XmlElement) doc.SelectSingleNode(string.Format("/configuration/appSettings/add[@key='{0}']",key));
      if (setting==null)
        return DefaultValue;
      else
        return setting.Attributes["value"].Value;
    }

    public ApplicationDirectory GetApplicationDirectory()
    {
      return new ApplicationDirectory(new DirectoryInfo(_BaseDirectory));
    }

    public string[] GetPublishedRoles()
    {
      return Roles;
    }

    public ApplicationLog Log 
    {
      get 
      {
        return log; 
      }
    }

    public string Type
    {
      get
      {
        return this.GetType().Name;
      }
    }

    public bool Loaded
    {
      get
      {
        return _Loaded;
      }
    }

    public AppDomain Domain
    {
      get 
      {
        return _Domain;
      }
    }

    public string BaseDirectory
    {
      get
      {
        return _BaseDirectory;
      }
    }

    public string Name
    {
      get
      {
        return _Name;
      }
    }

    public void Reload()
    {
      EnterMethod();
      try
      {
        Thread ReloaderThread=new Thread(new System.Threading.ThreadStart(ReloadDelegate));
        ReloaderThread.Start();
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

    protected void UnloadDelegate()
    {    
      AppDomain UnloadDomain=Domain;
      _Domain = null;      
      AppDomain.Unload(UnloadDomain);
      _Loaded=false;
    }

    protected void ReloadDelegate()
    {
      if (Loaded)
        UnloadDelegate();
      Load();
    }

    /// <summary>
    /// InitializeLifetimeService() returns "null" which means indefinite lifetime!
    /// http://www.dotnetremoting.cc/FAQs/PUBLISHING_OBJECT.asp
    /// </summary>
    /// <returns></returns>
    public override Object InitializeLifetimeService()
    {
      return null;
    }

    public virtual void Configure(ApplicationServer server, DirectoryInfo BaseDir, XmlDocument ConfigFile)
    {
      if (Boolean.Parse(ReadAppSetting(ConfigFile, "Debug", "false")))
      {
        TerWoord.Diagnostics.Destinations.FileDestination TempDest = new TerWoord.Diagnostics.Destinations.FileDestination();        
        TempDest.LogDirectory = Path.Combine(BaseDir.FullName, "detaillogs");                
        TempDest.Initialize();
        _LogCategory.Destins.Add(TempDest);
      }
      EnterMethod();
      try
      {      
        this.AppServer=server;
        this._BaseDirectory=BaseDir.FullName;
        SendValue("Base Directory", _BaseDirectory);
        _Name=ReadAppSetting(ConfigFile,"Application.Name",BaseDir.Name);
        SendValue("Name", _Name);
        AppServer=server;
        SecurityManager=server.SecurityManager;
        log=new ApplicationLog(_BaseDirectory + Path.DirectorySeparatorChar + "app.log");
        SendValue("Log file", _BaseDirectory + Path.DirectorySeparatorChar + "app.log");
        log.WriteLine("System","Application Started");
        SendString("Application Started");
        // Load Roles from Conig File
        XmlNodeList RoleList=ConfigFile.SelectNodes("/configuration/publishedRoles/role");
        if (RoleList.Count!=0)
        {
          Roles=new string[RoleList.Count];
          int i=0;
          foreach (XmlElement roleElement in RoleList)
          {
            Roles[i]=roleElement.InnerText;
            i++;
          }
        }
        SendValue("Roles", Roles);
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