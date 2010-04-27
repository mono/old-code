using System;
using System.Collections;

namespace TerWoord.Diagnostics
{
	/// <summary>
	/// </summary>
	[Serializable]
  public class NameObjectCollection: SortedList
  {
    /// <summary>
    /// </summary>
    public NameObjectCollection():base(){}

    /// <summary>
    /// </summary>
    /// <param name="Name">
    /// </param>
    /// <param name="Value">
    /// </param>
    public virtual void Add(string Name, object Value)
    {
      base.Add(Name.ToLower(), Value);
    }

    /// <summary>
    /// </summary>
    public virtual object this[string Name]
    {
      get
      {
        return this.Get(Name.ToLower());
      }
      set
      {
        this.Set(Name.ToLower(), value);        
      }
    }

    /// <summary>
    /// </summary>
    /// <param name="Name">
    /// </param>
    /// <returns>
    /// </returns>
    public virtual object Get(string Name)
    {       
      return base[Name.ToLower()];
    }

    /// <summary>
    /// </summary>
    /// <param name="Name">
    /// </param>
    /// <param name="Value">
    /// </param>
    public virtual void Set(string Name, object Value)
    {
      base[Name.ToLower()] = Value;
    }

    /// <summary>
    /// </summary>
    /// <param name="Name">
    /// </param>
    public virtual void Remove(string Name)
    {
      base.Remove(Name.ToLower());
    }
  }
}
