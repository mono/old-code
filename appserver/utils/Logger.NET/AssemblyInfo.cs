using System;
using System.Reflection;
using System.Runtime.InteropServices;

// Make assembly as NOT visible to COM
[assembly: ComVisible(false)]

// Mark assembly CLS compliant
[assembly: CLSCompliant(true)]

[assembly: AssemblyTitle("Logger.NET")]
[assembly: AssemblyDescription("A .NET Logging Tool")]
[assembly: AssemblyCompany("http://www.sourceforge.net/projects/logger-net")]
[assembly: AssemblyProduct("Logger.NET")]
[assembly: AssemblyCopyright("Copyright (C) 2004 Matthijs ter Woord")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyVersion("0.2.*")]
#if DEBUG
  [assembly: AssemblyConfiguration("debug")]
#endif
#if RELEASE
  //[assembly: AssemblyKeyFile("..\\Logger.NET.snk")]
#endif
