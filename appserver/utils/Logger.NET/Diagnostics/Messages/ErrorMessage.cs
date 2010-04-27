using System;

namespace TerWoord.Diagnostics.Messages
{
	/// <summary>
	/// </summary>
	public class ErrorMessage: AbstractMessage
	{
    private Exception _ExceptionObject;
    private string _Message = string.Empty;

    public Exception ExceptionObject
    {
      get
      {
        return _ExceptionObject;
      }
      set
      {
        _ExceptionObject = value;
      }
    }

    public string Message
    {
      get
      {
        if (_Message == string.Empty
          & _ExceptionObject != null)
          return _ExceptionObject.Message;
        else
          return _Message;
      }
      set
      {
        _Message = value;           
      }
    }
	}
}