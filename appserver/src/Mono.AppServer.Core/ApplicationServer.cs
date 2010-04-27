//
// Mono.AppServer.AppServer
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;
using System.IO;
using System.Xml;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Configuration;
using Mono.AppServer;
using Mono.AppServer.Security;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;

namespace Mono.AppServer
{
  public delegate void LogStringEvent(string StringToLog);
  /// <summary>
  /// Summary description for ApplicationServer.
  /// </summary>
  public class ApplicationServer : BaseMarshalByRefObject
  {
    public ApplicationCollection Applications;
    public TcpChannel AdminChannel;
    public SecurityManager SecurityManager;
    private ServerEnvironment _ServerEnvironment;
    public ApplicationType[] AvailableApplicationTypes;
    public string ApplicationsBaseDir;
    private LogStringEvent _OnLog = null;
    
    public event LogStringEvent OnLog
    {
      add
      {
        _OnLog += value;
      }
      remove
      {
        _OnLog -= value;
      }
    }    
    
    private void LogString(string StringToLog)
    {
      if (_OnLog != null)
      {
        _OnLog(StringToLog);
      }
      SendString(StringToLog);
    }
    
    private void LogString(string StringToLog, params object[] Params)
    {
      string TempString = String.Format(StringToLog, Params);
      if (_OnLog != null)
      {
        _OnLog(TempString);
      }
      SendString(TempString);
    }

    public ServerEnvironment ServerEnvironment
    {
      get
      {
        return _ServerEnvironment;
      }
    }
    
    void ExceptionHandler(object Sender, UnhandledExceptionEventArgs e)
    {
//      try
//      {
        SendError((Exception)e.ExceptionObject);
//      }
//      catch
//      {
      if (e.ExceptionObject != null)
        Console.WriteLine(e.ExceptionObject.ToString());
//      }
    }    

    /// <summary>
    /// Create applications
    /// 
    /// Publish specific instance via remoting.
    /// http://www.dotnetremoting.cc/FAQs/PUBLISHING_OBJECT.asp
    /// </summary>
    public ApplicationServer(int AdminPort, string ApplicationBaseDir, LogStringEvent logstringevent):base()
    {
      EnterMethod();
      try
      {
        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(ExceptionHandler);
        if (logstringevent != null)
        {
          OnLog += logstringevent;
        }
        Applications=new ApplicationCollection();
        LogString("{0}: {1}",
          GetType().Assembly.GetName().Name,
          GetType().Assembly.GetName().Version.ToString());
        LogString("Publishing server objects on port {0}",AdminPort);
        AdminChannel = new TcpChannel( AdminPort ); 
        ChannelServices.RegisterChannel( AdminChannel ); 
        _ServerEnvironment=new ServerEnvironment();
        string path=@"users"+Path.DirectorySeparatorChar.ToString ();
        byte[] Key=new byte[8] { 5,100,5,2,4,24,34,55 };
        byte[] IV=new byte[8] { 5,100,5,2,4,24,34,55};
        SecurityManager=new SecurityManager(new EncryptedFileStorage(path,Key,IV));
        RemotingServices.Marshal(this,"Mono.AppServer.ApplicationServer");
        AvailableApplicationTypes=(ApplicationType[]) ConfigurationSettings.GetConfig("Mono.AppServer");
        DirectoryInfo curdir=new DirectoryInfo(ApplicationBaseDir);
        this.ApplicationsBaseDir=curdir.FullName+Path.DirectorySeparatorChar;
        LogString("\nHosting applications in {0}",Path.GetFullPath(ApplicationBaseDir));
        foreach (DirectoryInfo dir in curdir.GetDirectories())
        {
          try
          {
            LoadApplication(dir);
          }
          catch (Exception e)
          {
            SendError(e);
            LogString("ERROR: " + e.Message);
          }
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
    
    public new object GetLifetimeService()
    {
      EnterMethod();
      try
      {
        object tmp = base.GetLifetimeService();
        SendValue("Return Value", tmp);
        if (tmp == null)
          Console.WriteLine("GetLifetimeService of ApplicationServer is NULL");
        else
          Console.WriteLine("GetLifetimeService of ApplicationServer is not NULL");
          
        return tmp;
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

    /// <summary>
    /// InitializeLifetimeService() returns "null" which means indefinite lifetime!
    /// http://www.dotnetremoting.cc/FAQs/PUBLISHING_OBJECT.asp
    /// </summary>
    /// <returns></returns>
    public override Object InitializeLifetimeService()
    {
      Console.WriteLine("InitializeLifetimeService");
      return null;
//      return base.InitializeLifetimeService();
    }
    
    public void Unload()
    {
      EnterMethod();
      try
      {
        while (Applications.Count > 0)
        {
          SendString("Unloading " + Applications[0].Name + "...");
          try
          {
            Applications[0].Unload();
            Applications.Remove(Applications[0]);
          }
          catch(Exception E)
          {
            SendError(E);
          }
          SendString("Done.");
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

    private ApplicationBase LoadApplication(DirectoryInfo dir)
    {
      EnterMethod();
      try
      {
        XmlDocument doc=new XmlDocument();
        if (dir.GetFiles("app.config").Length!=0)
          doc.Load(dir.FullName+Path.DirectorySeparatorChar+"app.config");
        string AppTypeName=ApplicationBase.ReadAppSetting(doc,"Application.Type","unknown");
        ApplicationBase app=null;
        foreach (ApplicationType apptype in AvailableApplicationTypes)
        {
          if (apptype.ApplicationTypeName==AppTypeName)
          {
            LogString("Loading {0}...", AppTypeName);
            app=apptype.ApplicationFactory(this,doc,dir);
            LogString("Created {0}: {1}",AppTypeName,app.Name);
            Applications.Add(app);
            app.Load();
            break;
          }
        }
        if (app==null)
          LogString("Unknown application: " + AppTypeName);
        return app;      
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
  
    public ApplicationBase DeployNewApplication(byte[] ArchiveBuffer, string Filename)
    {
      EnterMethod();
      try
      {
        MemoryStream InputStream=new MemoryStream(ArchiveBuffer);
        string ext=Path.GetExtension(Filename).ToLower();
        if (ext==".zip" || ext==".tar" || ext==".gz")
        {
          string appname=Path.GetFileNameWithoutExtension(Filename);
          string BasePath=ApplicationsBaseDir+appname;
          if (Directory.Exists(BasePath))
            throw new ApplicationException(string.Format("Application Directory '{0}' already exists",appname));
          LogString("Deploying to "+BasePath);
          switch (ext)
          {
            case ".zip":
              UnzipFile(InputStream,BasePath);
              break;
            case ".tar":
              UnTarFile(InputStream,BasePath,false);
              break;
            case ".gz":
              UnTarFile(InputStream,BasePath,true);
              break;
          }
          ApplicationBase app=LoadApplication(new DirectoryInfo(BasePath));
          if (app==null)
          {
            Directory.Delete(BasePath,true);
            throw new ApplicationException("Invalid application archive.  Unable to read valid configuration file.");
          }
          return app;
        }
        else
          throw new ApplicationException(string.Format("Unable to deploy application from file type: {0}",ext));
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

    private void UnTarFile(Stream InputStream, string BasePath, bool Compressed)
    {
      EnterMethod();
      try
      {
        Directory.CreateDirectory(BasePath);
        if (Compressed)
          InputStream=new GZipOutputStream(InputStream);
        TarArchive archive=TarArchive.CreateInputTarArchive(InputStream);
        archive.ExtractContents(BasePath);
        archive.CloseArchive();
      }
      catch(Exception E)
      {
        SendError(E);
        throw;
      }
    }

    private void UnzipFile(Stream InputStream, string BasePath)    
    {
      EnterMethod();
      try
      {
        BasePath+=Path.DirectorySeparatorChar;
        ZipInputStream s = new ZipInputStream(InputStream);
        ZipEntry theEntry;
        while ((theEntry = s.GetNextEntry()) != null) 
        {
          LogString(" -"+theEntry.Name);
          string Fullname=BasePath+theEntry.Name;
          string directoryName = Path.GetDirectoryName(Fullname);
          string fileName      = Path.GetFileName(Fullname);
          // create directory
          Directory.CreateDirectory(directoryName);
          if (!theEntry.IsDirectory)
          {
            FileStream streamWriter = File.Create(Fullname);
            int size = 2048;
            byte[] data = new byte[2048];
            while (true) 
            {
              size = s.Read(data, 0, data.Length);
              if (size > 0) 
              {
                streamWriter.Write(data, 0, size);
              } 
              else 
              {
                break;
              }
            }
            streamWriter.Close();
          }
        }
        s.Close();
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