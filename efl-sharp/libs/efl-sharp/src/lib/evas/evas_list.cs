namespace Enlightenment.Evas
{
   
      using System;
      using System.Collections;
      using System.Runtime.InteropServices;
      using System.Reflection;
      using System.Threading;
   
   public class List
     {   
	private ArrayList list;
	
	public List()
	  {
	     list = new ArrayList();
	  }
	
	public void Append(object data)
	  {
	     list.Add(data);
	  }
	
	public void Prepend(object data)
	  {
	     list.Insert(0, data);
	  }
	
	public void AppendRelative(object data, object relative)
	  {
	     list.Insert(list.IndexOf(relative) + 1, data);
	  }
	
	public void PrependRelative(object data, object relative)
	  {
	     list.Insert(list.IndexOf(relative), data);
	  }
	
	public void Remove(object data)
	  {
	     list.Remove(data);
	  }
	
	public void RemoveList(object data)
	  {
	     // TODO
	  }
	
	public object Find(object data)
	  {
	     return list.Contains(data);
	  }
	
	public void FindList(object data)
	  {
	     // TODO
	  }
	
	public void Reverse()
	  {
	     list.Reverse();
	  }
	
	public void Sort()
	  {
	     list.Sort();
	  }
	
	~List()
	  {
	  }
     }   
}
