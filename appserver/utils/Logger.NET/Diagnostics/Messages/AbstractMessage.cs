using System;
using System.Diagnostics;
using System.Reflection;

namespace TerWoord.Diagnostics.Messages
{
	/// <summary>
	/// Summary description for AbstractMessage.
	/// </summary>
	public class AbstractMessage
	{
    private string _MachineName;
    private string _AppName;
    private string _AppDomainFriendlyName;
    private DateTime _UTCTime;
    private string _OSPlatform;
    private string _OSVersion;
    private StackTrace _Stack;
    private string _UserDomainName;
    private string _UserName;
    private bool   _UserInteractive;
    private string _RuntimeVersion;

    protected virtual void InitNewMessage()
    {
    }

		public AbstractMessage()
		{
		}

    public void InitializeNewMessage()
    {                      
      _MachineName = Environment.MachineName;
      if (Assembly.GetEntryAssembly() != null)
      {
        _AppName = Assembly.GetEntryAssembly().Location;
      }
      else
      {
        _AppName = "<unknown>";
      }
      _AppDomainFriendlyName = AppDomain.CurrentDomain.FriendlyName;
      _UTCTime = DateTime.UtcNow;
      _OSPlatform = Environment.OSVersion.Platform.ToString();
      _OSVersion = Environment.OSVersion.Version.ToString();
      _Stack = new StackTrace(0);
      _UserDomainName = Environment.UserDomainName;
      _UserName = Environment.UserName;
      _UserInteractive = Environment.UserInteractive;
      _RuntimeVersion = Environment.Version.ToString();
      InitNewMessage();
    }

    public string MachineName
    {
      get
      {
        return _MachineName;
      }
      set
      {
        _MachineName = value;
      }
    }
    
    public string AppName
    {
      get
      {
        return _AppName;
      }
      set
      {
        _AppName = value;
      }
    }
    
    public string AppDomainFriendlyName
    {
      get
      {
        return _AppDomainFriendlyName;
      }
      set
      {
        _AppDomainFriendlyName = value;
      }
    }
    
    public DateTime UTCTime
    {
      get
      {
        return _UTCTime;
      }
      set
      {
        _UTCTime = value;
      }
    }
    
    public string OSPlatform
    {
      get
      {
        return _OSPlatform;
      }
      set
      {
        _OSPlatform = value;
      }
    }
    
    public string OSVersion
    {
      get
      {
        return _OSVersion;
      }
      set
      {
        _OSVersion = value;
      }
    }
    
    public StackTrace Stack
    {
      get
      {
        return _Stack;
      }
      set
      {
        _Stack = value;
      }
    }
    
    public string UserDomainName
    {
      get
      {
        return _UserDomainName;
      }
      set
      {
        _UserDomainName = value;
      }
    }
    
    public string UserName
    {
      get
      {
        return _UserName;
      }
      set
      {
        _UserName = value;
      }
    }
    
    public bool UserInteractive
    {
      get
      {
        return _UserInteractive;
      }
      set
      {
        UserInteractive = value;
      }
    }
    
    public string RuntimeVersion
    {
      get
      {
        return _RuntimeVersion;
      }
      set
      {
        _RuntimeVersion = value;
      }
    }    
  }
}
