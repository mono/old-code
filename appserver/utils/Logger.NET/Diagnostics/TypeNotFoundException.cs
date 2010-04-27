using System;

namespace TerWoord.Diagnostics
{
	/// <summary>
	/// </summary>
	public class TypeNotFoundException: Exception
	{
    /// <summary>
    /// </summary>
    public TypeNotFoundException(string Type):base(String.Format("Type '{0}' not found", Type))
		{             
    }
	}
}
