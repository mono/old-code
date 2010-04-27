using System;
using System.Collections;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;

namespace TerWoord.Diagnostics
{
	/// <summary>
	///   <para>
	///     Handles all logging stuff
	///   </para>
	/// </summary>
	public sealed class LogFactory
	{
    private static Assembly GetReferencingAssembly()
    {
      StackTrace ST = new StackTrace();
      for (int i = 0; i < ST.FrameCount; i++) 
      {
        if (ST.GetFrame(i).GetMethod().DeclaringType.Assembly != typeof(LogFactory).Assembly)
        {
          Type ConfigAttrib = typeof(ConfiguratorAttribute);
          object[] AttribList = ST.GetFrame(i).GetMethod().DeclaringType.Assembly.GetCustomAttributes(ConfigAttrib, true);
          if ( AttribList != null
            && AttribList.Length == 1)
            return ST.GetFrame(i).GetMethod().DeclaringType.Assembly;
        }
      }
      return null;
    }

    public static Assembly ReferencingAssembly;

    internal static int Indent = 0;
    private static UniqueNameObjectCollection Categories = new UniqueNameObjectCollection();
    private static string DefaultName;
    static LogFactory()
    {
      try
      {
        ReferencingAssembly = GetReferencingAssembly();
        AbstractConfigurator Configurator = new ConfigFileConfigurator();        
        if (ReferencingAssembly != null)
        {
          object[] Attribs = ReferencingAssembly.GetCustomAttributes(typeof(ConfiguratorAttribute), false);
          if (Attribs != null)
          {
            if (Attribs.Length == 1)
            {
              if (Attribs[0] is ConfiguratorAttribute)
              {
                Configurator = ((ConfiguratorAttribute)Attribs[0]).Configurator;
              }
            }
          }
        }
        Categories = Configurator.GetCategories(ref DefaultName);                
      }
      finally
      {
      }
      if (DefaultName == null)
      {
        if (Categories.Count > 0)
        {
          DefaultName = (string)Categories.GetKey(0);
        }
        else
        {
          Category Temp = new Category();
          Temp.Name = "Default";
          Temp.Initialize();
          Categories.Add("Default", Temp);
          DefaultName = "Default";
        }
      }
    }

    public static Category GetCategory(string CategoryName)
    {
      if (Categories.ContainsKey(CategoryName))
      {
        return (Category)Categories[CategoryName];
      }
      else
      {
        return (Category)Categories[DefaultName];
      }
    }
	}
}
