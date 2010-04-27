namespace Enlightenment.Evas
{
   
      using System;
      using System.Collections;
      using System.Runtime.InteropServices;
      using System.Reflection;
      using System.Threading;
   
   public class Hash
     {
	Hashtable hash;
	
	public Hash()
	  {
	     hash = new Hashtable();
	  }
	
	public void Add(string key, object data)
	  {
	     hash[key] = data;
	  }
	
	public void Del(string key)
	  {
	     hash.Remove(key);
	  }
	
	public object Find(string key)
	  {
	     return hash[key];
	  }
	
	public int Size()
	  {
	     return hash.Count;
	  }
	
	~Hash()
	  {
	  }
	
     }
}
