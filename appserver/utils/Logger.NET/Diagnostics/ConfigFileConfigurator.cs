using System;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace TerWoord.Diagnostics
{
	/// <summary>
	/// </summary>
	public class ConfigFileConfigurator: AbstractConfigurator
	{
    public override UniqueNameObjectCollection GetCategories(ref string DefaultCategory)
    {
      UniqueNameObjectCollection Categories = new UniqueNameObjectCollection();
      XmlNode XNode = (XmlNode)ConfigurationSettings.GetConfig("TerWoord/Logger.NET");
      if (XNode == null)
      {
        Categories.Add("default", new Category());
        DefaultCategory = "default";
        return Categories;
      }
      StringReader SR;
      XmlSerializer XS = new XmlSerializer(typeof(Category));
      Categories.Clear();
      foreach (XmlNode xn in XNode.ChildNodes)
      {
        if (xn is XmlElement)
        {
          if (xn.Name == "Default")
          {
            if ( xn.Attributes["Name"] != null
              && xn.Attributes["Name"].Value != null)
            {
              DefaultCategory = xn.Attributes["Name"].Value;
            } 
          }
          else
          {           
            SR = new StringReader(xn.OuterXml);
            Category c = (Category)XS.Deserialize(SR);
            c.Initialize();
            Categories.Add(c.Name, c);
          }
        }
      }
      return Categories;
    }    
	}
}
