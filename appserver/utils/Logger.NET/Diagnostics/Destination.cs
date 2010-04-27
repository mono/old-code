using System;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Serialization;

using TerWoord.Diagnostics.Messages;

namespace TerWoord.Diagnostics
{
	/// <summary>
	///   <para>
	///     Takes care of message sending.
	///   </para>
	/// </summary>
	public abstract class Destination
	{ 	
    private Setting[] _Settings = new Setting[0];

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

    public string Category;

    public virtual void Initialize()
    {
      Type ti = this.GetType();
      if (_Settings != null)
      {
        foreach (Setting s in _Settings)
        {
          MemberInfo[] MIs = ti.GetMember(s.Name,MemberTypes.Field | MemberTypes.Property, BindingFlags.Instance | BindingFlags.Public);
          if (MIs != null)
          {
            if (MIs.Length > 0)
            {
              if (MIs[0].MemberType == MemberTypes.Property)
              {
                ((PropertyInfo)MIs[0]).SetValue(this, Convert.ChangeType(s.Value, ((PropertyInfo)MIs[0]).PropertyType), new object[0]);
              }
              else
              {
                ((FieldInfo)MIs[0]).SetValue(this, Convert.ChangeType(s.Value, ((FieldInfo)MIs[0]).FieldType));
              }
            }
            else
            {
              throw new SettingNotFoundException(s.Name);
            }
          }
          else
          {
            throw new SettingNotFoundException(s.Name);
          }
        }
      }
    }

    public abstract void EnterMethod(EnterMethodMessage EMM);
    public abstract void ExitMethod(ExitMethodMessage EMM);
    public abstract void SendString(StringMessage SM);
    public abstract void SendValue(ValueMessage VM);
    public abstract void SendError(ErrorMessage EM);
	}
}
