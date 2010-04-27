using System;

namespace TerWoord.Diagnostics
{
  /// <summary>
  /// </summary>
  public class ItemAlreadyExistsException: Exception
  {
    /// <summary>
    /// </summary>
    public ItemAlreadyExistsException(string ItemName):base("Item \"" + ItemName + "\" does already exist")
    {       
    }
  }

  /// <summary>
  /// </summary>
  [Serializable]
	public class UniqueNameObjectCollection: NameObjectCollection
	{
    /// <summary>
    /// </summary>
    public UniqueNameObjectCollection():base()
		{}

    /// <summary>
    /// </summary>
    public override void Set(string Name, object Value)
    {
      if (this.Get(Name) == null)
      { 
        base.Set(Name, Value);
      }
      else
      {
        throw new ItemAlreadyExistsException(Name);
      }
    }
    
    /// <summary>
    /// </summary>
    public override void Add(string Name, object Value)
    {
      if (this.Get(Name) == null)
      { 
        base.Add(Name, Value);
      }
      else
      {
        throw new ItemAlreadyExistsException(Name);
      }
    }
	}
}
