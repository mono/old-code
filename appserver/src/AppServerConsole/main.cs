//
// Mono.AppServer.ApplicationServerConsole
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;
using System.IO;
using Mono.AppServer;
using System.Xml;


namespace Mono.AppServer
{
  class ApplicationServerConsole: BaseStaticClass
  {
    static ApplicationServerConsole()
    {
      InitLoggerNet(typeof(ApplicationServerConsole));
    }
    
    static void LogString(string StringToLog)
    {
      Console.WriteLine(StringToLog);
    }
    
    [STAThread]
    static void Main(string[] args)
    {
      EnterMethod();
      try
      {
        Console.WriteLine("Starting Mono.AppServer...\n");
        ApplicationServer Server=new ApplicationServer(1033, "." + Path.DirectorySeparatorChar + "applications", new LogStringEvent(LogString));        
        SendString("ApplicationServer Running");
        Console.WriteLine("\nApplicationServer Running.  Press enter to exit...");
        Console.ReadLine();
        Server.Unload();
        System.Diagnostics.Process.GetCurrentProcess().Kill();
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
