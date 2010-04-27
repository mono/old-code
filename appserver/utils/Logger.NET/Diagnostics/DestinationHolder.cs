using System;
using System.Xml.Serialization;

namespace TerWoord.Diagnostics
{
	/// <summary>
	/// </summary>
	public class DestinationHolder
	{
		private string _Type;
    private Setting[] _Settings;

    [XmlArray("Settings")]
    [XmlArrayItem("Setting", typeof(Setting))]
    public Setting[] Settings
    {
      get
      {
        return _Settings;
      }
      set
      {
        _Settings = value;
      }
    }

    [XmlAttribute("Type")]
    public string Type
    {
      get
      {
        return _Type;
      }
      set
      {
        _Type = value;
      }
    }
	}
}
