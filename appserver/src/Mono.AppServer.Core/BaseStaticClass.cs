using System;
using System.Diagnostics;
#if DEBUG
  using TerWoord.Diagnostics;
#endif

namespace Mono.AppServer
{
  public class BaseStaticClass
  {
#if DEBUG
    private static Category LogCategory = new Category();
    private static bool _IsFullDebug;
    static BaseStaticClass()
    {
      LogCategory.Initialize();
      EnterMethod();
      try
      {
        _IsFullDebug = false;
        foreach (string s in Environment.GetCommandLineArgs())
        {
          if (s.ToUpper() == "/FULLDEBUG")
          {
            _IsFullDebug = true;
          }
        }
        SendValue("IsFullDebug", _IsFullDebug);
      }
      catch(Exception E)
      {
        SendError(E);
        throw E;
      }
      finally
      {
        ExitMethod();
      }
    }
#endif
      
    [Conditional("DEBUG")]
    [DebuggerHidden]
    protected void LaunchDebugger()
    {
      Debugger.Launch();
    }

    public static bool IsFullDebug()
    {
#if DEBUG
      foreach (string s in Environment.GetCommandLineArgs())
      {
        if (s.ToUpper() == "/FULLDEBUG")
        {
          return true;
        }
      }
#endif
      return false;
    }

    [Conditional("DEBUG")]
    protected static void InitLoggerNet(Type ClassType)
    {
#if DEBUG
      if (ClassType == null)
      {
        throw new ArgumentNullException("ClassType");
      }
      LogCategory = LogFactory.GetCategory(ClassType.FullName);
#endif 
    }

    /// <summary>
    ///   <para>
    ///     This method is part of the internal logging features of Ter Woord UIAL using 
    ///     Logger.NET
    ///   </para>
    /// </summary>
    [Conditional("DEBUG")]
    protected static void EnterMethod()
    {
#if DEBUG
      LogCategory.EnterMethod(1);
#endif
    }

    /// <summary>
    ///   <para>
    ///     This method is part of the internal logging features of Ter Woord UIAL using 
    ///     Logger.NET
    ///   </para>
    /// </summary>
    [Conditional("DEBUG")]
    protected static void ExitMethod()
    {
#if DEBUG
      LogCategory.ExitMethod(1);
#endif
    }

    /// <summary>
    ///   <para>
    ///     This method is part of the internal logging features of Ter Woord UIAL using 
    ///     Logger.NET
    ///   </para>
    /// </summary>
    [Conditional("DEBUG")]
    protected static void SendString(string StrToSend)
    {
#if DEBUG
      LogCategory.SendString(StrToSend);
#endif
    }

    [Conditional("DEBUG")]
    protected static void SendString(string StrToSend, params object[] Params)
    {
#if DEBUG
      LogCategory.SendString(String.Format(StrToSend, Params));
#endif
    }

    /// <summary>
    ///   <para>
    ///     This method is part of the internal logging features of Ter Woord UIAL using 
    ///     Logger.NET
    ///   </para>
    /// </summary>
    [Conditional("DEBUG")]
    protected static void SendValue(string Message, object Value)
    {
#if DEBUG
      LogCategory.SendValue(Message, Value);
#endif
    }

    /// <summary>
    ///   <para>
    ///     This method is part of the internal logging features of Ter Woord UIAL using 
    ///     Logger.NET
    ///   </para>
    /// </summary>
    [Conditional("DEBUG")]
    protected static void SendError(string Message)
    {
#if DEBUG
      LogCategory.SendError(Message);
#endif
    }

    /// <summary>
    ///   <para>
    ///     This method is part of the internal logging features of Ter Woord UIAL using 
    ///     Logger.NET
    ///   </para>
    /// </summary>
    [Conditional("DEBUG")]
    protected static void SendError(Exception ExceptionObject)
    {
#if DEBUG
      LogCategory.SendError(ExceptionObject);
#endif
    }
	}
}
