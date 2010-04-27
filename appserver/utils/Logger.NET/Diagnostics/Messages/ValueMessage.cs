using System;

namespace TerWoord.Diagnostics.Messages
{
	/// <summary>
	/// </summary>
	public class ValueMessage: AbstractMessage
	{
    private string _Message;
    private object _Value;
    private Type _Type;

    public string Message
    {
      get
      {
        return _Message;
      }
      set
      {
        _Message = value;
      }
    }

    public object Value
    {
      get
      {
        return _Value;
      }
      set
      {
        if (_Value != value)
        { 
          _Value = value;
          if (_Value == null)
          {
            _Type = null;
          }
          else
          {
            _Type = _Value.GetType();
          }
        }
      }
    }

    public Type Type
    {
      get
      {
        return _Type;
      }
    }
	}
}
