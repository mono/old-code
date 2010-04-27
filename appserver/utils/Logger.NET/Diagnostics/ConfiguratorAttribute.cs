using System;

namespace TerWoord.Diagnostics
{
	/// <summary>
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
	public class ConfiguratorAttribute: Attribute
	{
    private AbstractConfigurator _Configurator;

		public ConfiguratorAttribute(Type configurator, params object[] Params)
		{
      if (configurator == null)
        throw new Exception("Configurator cannot be null!");

      if (!configurator.IsSubclassOf(typeof(AbstractConfigurator)))
        throw new Exception("Configurator must derive from AbstractConfigurator");

      _Configurator = (AbstractConfigurator)Activator.CreateInstance(configurator, Params);
    }

    public ConfiguratorAttribute()
    {
      _Configurator = new ConfigFileConfigurator();
    }

    public AbstractConfigurator Configurator
    {
      get
      {
        return _Configurator;
      }
    }
	}
}
