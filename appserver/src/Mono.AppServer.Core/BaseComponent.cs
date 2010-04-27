using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
#if DEBUG
  using TerWoord.Diagnostics;
#endif


namespace Mono.AppServer
{
	/// <summary>
	///   <para>
	///     Base class for all components.
	///   </para>
	/// </summary>
	public class BaseComponent : System.ComponentModel.Component
	{
#if DEBUG
    private Category LogCategory;
#endif
		private System.ComponentModel.Container components = null;

    /// <summary>
    ///   <para>
    ///     This method is part of the internal logging features of Ter Woord UIAL using 
    ///     Logger.NET
    ///   </para>
    /// </summary>
    [Conditional("DEBUG")]
    protected void EnterMethod()
    {
#if DEBUG
      LogCategory.EnterMethod(1);
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

    /// <summary>
    ///   <para>
    ///     This method is part of the internal logging features of Ter Woord UIAL using 
    ///     Logger.NET
    ///   </para>
    /// </summary>
    [Conditional("DEBUG")]
    protected void ExitMethod()
    {
#if DEBUG
      LogCategory.ExitMethod(1);
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
    protected void SendString(string StrToSend)
    {
#if DEBUG
      LogCategory.SendString(StrToSend);
#endif
    }

    [Conditional("DEBUG")]
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
    protected void SendError(Exception ExceptionObject)
    {
#if DEBUG
      LogCategory.SendError(ExceptionObject);
#endif
    }

    /// <summary>
    ///   <para>
    ///     Creates a new instance of <see cref="BaseComponent"/>.
    ///   </para>
    /// </summary>
    /// <param name="container">
    ///   <para>
    ///     The parent of this component.
    ///   </para>
    /// </param>
		public BaseComponent(System.ComponentModel.IContainer container)
		{
#if DEBUG
      LogCategory = LogFactory.GetCategory(this.GetType().FullName);
#endif

			container.Add(this);
			InitializeComponent();
		}

    /// <summary>
    ///   <para>
    ///     Creates a new instance of <see cref="BaseComponent"/>.
    ///   </para>
    /// </summary>
    public BaseComponent()
		{
#if DEBUG
      LogCategory = LogFactory.GetCategory(this.GetType().FullName);
#endif

			InitializeComponent();
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion
	}
}
