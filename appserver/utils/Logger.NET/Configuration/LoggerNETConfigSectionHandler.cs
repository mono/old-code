using System;
using System.Configuration;
using System.Xml;

namespace TerWoord.Configuration
{
	/// <summary>
	///   <para>
	///     Handles configuration of Logger.NET
	///   </para>
	/// </summary>
	public class LoggerNETConfigSectionHandler: IConfigurationSectionHandler
	{
    /// <summary>
    ///   <para>
    ///   </para>
    /// </summary>
    /// <param name="parent">
    ///   <para>
    ///   </para>
    /// </param>
    /// <param name="configContext">
    ///   <para>
    ///   </para>
    /// </param>
    /// <param name="section">
    ///   <para>
    ///   </para>
    /// </param>
    /// <returns>
    ///   <para>
    ///   </para>
    /// </returns>
    public object Create(
      object parent,
      object configContext,
      XmlNode section
      )
    {
      return section;
    }
	}
}
