using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace TerWoord.Diagnostics
{
	/// <summary>
	/// </summary>
	public class XmlFileConfigurator: AbstractConfigurator
	{
    private string _XmlFile;
    public XmlFileConfigurator(string XmlFile)
    { 
      _XmlFile = XmlFile;
    }
    public override UniqueNameObjectCollection GetCategories(ref string DefaultCategory)
    {   
      UniqueNameObjectCollection Categories = new UniqueNameObjectCollection();    
      try
      {
        if (_XmlFile.IndexOf("${ASSEMBLYDIR}") != -1)
        { 
          string TempDir = @"c:\";
          try
          {
            TempDir = Path.GetDirectoryName(LogFactory.ReferencingAssembly.Location);            
          }
          catch
          {
            TempDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Logger.NET");
          }
          Directory.CreateDirectory(TempDir);
          if (TempDir.EndsWith(Path.DirectorySeparatorChar.ToString())
            | TempDir.EndsWith(Path.AltDirectorySeparatorChar.ToString())
            )
            _XmlFile = _XmlFile.Replace("${ASSEMBLYDIR}", TempDir);
          else
            _XmlFile = _XmlFile.Replace("${ASSEMBLYDIR}", TempDir + Path.DirectorySeparatorChar);
        }
        if (File.Exists(_XmlFile))
        {         
          XmlDocument XmlDoc = new XmlDocument();
          XmlDoc.Load(_XmlFile);        
          XmlNode XNode = XmlDoc.SelectSingleNode("/TerWoord/Logger.NET");
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
      catch
      {
        Categories.Add("default", new Category());
        DefaultCategory = "default";
        throw;
      }
      return new UniqueNameObjectCollection();
    }
	}
}
