using System;

namespace TerWoord.Diagnostics
{
	/// <summary>
	/// </summary>
	public class SettingNotFoundException: Exception
	{
    /// <summary>
    /// </summary>
    public SettingNotFoundException(string Name):base(string.Format("Setting '{0}' not found!", Name))
		{
		}
	}
}
