using System;

namespace TerWoord.Diagnostics
{
	/// <summary>
	/// </summary>	
	public abstract class AbstractConfigurator
	{
    public abstract UniqueNameObjectCollection GetCategories(ref string DefaultCategory);
	}
}
