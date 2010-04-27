using System;
using System.Diagnostics;

namespace TerWoord.Diagnostics.Messages
{
	/// <summary>
	/// </summary>
	public class ExitMethodMessage: AbstractMessage
	{
    private string _MethodName;
    internal int _MethodOffset = 0;

    protected override void InitNewMessage()
    {
      base.InitNewMessage();
      this.MethodName = this.Stack.GetFrame(_MethodOffset).GetMethod().DeclaringType.FullName + "." + this.Stack.GetFrame(_MethodOffset).GetMethod().Name;
    }

    public string MethodName
    {
      get
      {
        return _MethodName;
      }
      set
      {
        _MethodName = value;
      }
    }
	}
}
