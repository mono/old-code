using System;
using System.Xml.Serialization;

namespace TerWoord.Diagnostics
{
	/// <summary>
	///   <para>
	///     Holds settings.
	///   </para>
	/// </summary>
	[Serializable]
	public class Setting
	{ 
    private string _Name;
    private string _Value;

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
       
    [XmlAttribute("Value")]
    public string Value
    {
      get
      {
        return _Value;
      }
      set
      { 
        _Value = value;
      }
    }
	}
}
