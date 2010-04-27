using System;
using System.Collections;
using System.Xml.Serialization;

using TerWoord.Diagnostics.Messages;

namespace TerWoord.Diagnostics
{
	/// <summary>
	/// Summary description for Category.
	/// </summary>
	[Serializable]
  [XmlRoot("Category")]
	public class Category
	{
    private Exception LastException;
    private ArrayList _Destins = new ArrayList();
    private string _Name;
    private DestinationHolder[] _Destinations = new DestinationHolder[] {};
    
    [XmlIgnore]
    public ArrayList Destins
    {
      get
      {
        return _Destins;
      }
    }

    [XmlAttribute]
    public string Name
    { 
      get
      {
        return _Name;
      }
      set
      {
        _Name = value;
      }
    }

    [XmlArray("Destinations")]
    [XmlArrayItem("Destination", typeof(DestinationHolder))]
    public DestinationHolder[] Destinations
    {
      get
      {
        return _Destinations;
      }
      set
      {
        _Destinations = value;
      }
    }

    public void Initialize()
    {                  
      if (_Destinations != null)
      {
        foreach (DestinationHolder dh in _Destinations)
        {
          if (Type.GetType(dh.Type) != null)
          {
            Destination d = (Destination)Activator.CreateInstance(Type.GetType(dh.Type));
            _Destins.Add(d);
            d.Category = this._Name;
            d.Settings = dh.Settings;
            d.Initialize();
          }
          else
          {
            throw new TypeNotFoundException(dh.Type);
          }
        }
      }
    }

    public void SendString(string StrToSend)
    {
      StringMessage SM = new StringMessage();
      SM.Message = StrToSend;
      SM.InitializeNewMessage();
      foreach (Destination d in _Destins)
      {
        d.SendString(SM);
      }
    }

    public void SendValue(string Message, object Value)
    {
      ValueMessage VM = new ValueMessage();
      VM.InitializeNewMessage();
      VM.Message = Message;
      VM.Value = Value;
      foreach (Destination d in _Destins)
      {
        d.SendValue(VM);
      }
    }

    public void EnterMethod()
    {
      EnterMethod(1);
    }

    public void EnterMethod(int MethodOffSet)
    {
      try
      {
        EnterMethodMessage EMM = new EnterMethodMessage();
        EMM._MethodOffset = MethodOffSet + 2;
        EMM.InitializeNewMessage();        
        foreach (Destination d in _Destins)
        {
          d.EnterMethod(EMM);
        }
      }
      finally
      {      
        lock(typeof(LogFactory))
        {
          LogFactory.Indent += 1;
        }
      }
    }

    public void ExitMethod()
    {
      ExitMethod(1);
    }

    public void ExitMethod(int MethodOffSet)
    {
      try
      {
        ExitMethodMessage EMM = new ExitMethodMessage();
        EMM._MethodOffset = MethodOffSet + 2;
        EMM.InitializeNewMessage();        
        foreach (Destination d in _Destins)
        {
          d.ExitMethod(EMM);
        }
      }
      finally
      {
        lock(typeof(LogFactory))
        {
          LogFactory.Indent -= 1;
        }
      }
    }

    public void SendError(string Message)
    {
      ErrorMessage EM = new ErrorMessage();
      EM.InitializeNewMessage();
      EM.Message = Message;
      foreach (Destination d in _Destins)
      {
        d.SendError(EM);
      }
    }

    public void SendError(Exception Error)
    {
      if (Error != LastException)
      {
        LastException = Error;
        ErrorMessage EM = new ErrorMessage();
        EM.InitializeNewMessage();
        EM.ExceptionObject = Error;
        foreach (Destination d in _Destins)
        {
          d.SendError(EM);
        }
      }
    }
	}
}
