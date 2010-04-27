using System;
using System.Diagnostics;
#if DEBUG
  using TerWoord.Diagnostics;
#endif

namespace Mono.AppServer
{
  /// <summary>
  ///   <para>
  ///     Declares global functionality for object descendants
  ///   </para>
  /// </summary>
  [DebuggerStepThrough]    
  public abstract class BaseObject
  {
#if DEBUG
    private Category LogCategory;
#endif
    /// <summary>
    /// Creates a new instance of <see cref="BaseObject"/>
    /// </summary>
    public BaseObject()
		{
#if DEBUG
      LogCategory = LogFactory.GetCategory(this.GetType().FullName);
#endif
		}
        
    protected bool IsFullDebug()
    {
#if DEBUG
      return BaseStaticClass.IsFullDebug();
#else 
      return false;
#endif
    }

    [Conditional("DEBUG")]
    [DebuggerHidden]
    protected void LaunchDebugger()
    {
      Debugger.Launch();
    }

    /// <summary>
    ///   <para>
    ///     This method is part of the internal logging features of Ter Woord UIAL using 
    ///     Logger.NET
    ///   </para>
    /// </summary>
    [Conditional("DEBUG")]
    [DebuggerHidden]
    protected void EnterMethod()
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
    [DebuggerHidden]
    protected void ExitMethod()
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
    [DebuggerHidden]
    protected void SendString(string StrToSend)
    {
#if DEBUG
      LogCategory.SendString(StrToSend);
#endif
    }

    /// <summary>
    ///   <para>
    ///     This method is part of the internal logging features of Ter Woord UIAL using 
    ///     Logger.NET
    ///   </para>
    /// </summary>
    [Conditional("DEBUG")]
    [DebuggerHidden]
    protected void SendString(string StrToSend, params object[] Params)
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
    [DebuggerHidden]
    protected void SendValue(string Message, object Value)
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
    [DebuggerHidden]
    protected void SendError(string Message)
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
    [DebuggerHidden]
    protected void SendError(Exception ExceptionObject)
    {
#if DEBUG
      LogCategory.SendError(ExceptionObject);
#endif
    }
  }
}
