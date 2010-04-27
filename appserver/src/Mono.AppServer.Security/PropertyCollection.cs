//
// Mono.AppServer.Security.PropertyCollection
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
namespace Mono.AppServer.Security
{
	using System;
	using System.Collections;
    
    
	/// <summary>
	///     <para>
	///       A collection that stores <see cref='.PropertyInstance'/> objects.
	///    </para>
	/// </summary>
	/// <seealso cref='.PropertyCollection'/>
	
	[Serializable()]
	public class PropertyCollection : CollectionBase 
	{
        
		/// <summary>
		///     <para>
		///       Initializes a new instance of <see cref='.PropertyCollection'/>.
		///    </para>
		/// </summary>
		public PropertyCollection() 
		{
		}
		        
		/// <summary>
		///     <para>
		///       Initializes a new instance of <see cref='.PropertyCollection'/> based on another <see cref='.PropertyCollection'/>.
		///    </para>
		/// </summary>
		/// <param name='value'>
		///       A <see cref='.PropertyCollection'/> from which the contents are copied
		/// </param>
		public PropertyCollection(PropertyCollection value) 
		{
			this.AddRange(value);
		}
        
		/// <summary>
		///     <para>
		///       Initializes a new instance of <see cref='.PropertyCollection'/> containing any array of <see cref='.PropertyInstance'/> objects.
		///    </para>
		/// </summary>
		/// <param name='value'>
		///       A array of <see cref='.PropertyInstance'/> objects with which to intialize the collection
		/// </param>
		public PropertyCollection(PropertyInstance[] value) 
		{
			this.AddRange(value);
		}
        
		/// <summary>
		/// <para>Represents the entry at the specified index of the <see cref='.PropertyInstance'/>.</para>
		/// </summary>
		/// <param name='index'><para>The zero-based index of the entry to locate in the collection.</para></param>
		/// <value>
		///    <para> The entry at the specified index of the collection.</para>
		/// </value>
		/// <exception cref='System.ArgumentOutOfRangeException'><paramref name='index'/> is outside the valid range of indexes for the collection.</exception>
		public PropertyInstance this[int index] 
		{
			get 
			{
				return ((PropertyInstance)(List[index]));
			}
			set 
			{
				List[index] = value;
			}
		}

		public object this[string Name]
		{
			get
			{
				foreach (PropertyInstance inst in List)
				{
					if (inst.Name==Name)
						return inst.Value;
				}
				return null;
			}
		}
        
		/// <summary>
		///    <para>Adds a <see cref='.PropertyInstance'/> with the specified value to the 
		///    <see cref='.PropertyCollection'/> .</para>
		/// </summary>
		/// <param name='value'>The <see cref='.PropertyInstance'/> to add.</param>
		/// <returns>
		///    <para>The index at which the new element was inserted.</para>
		/// </returns>
		/// <seealso cref='.PropertyCollection.AddRange'/>
		public int Add(PropertyInstance value) 
		{
			return List.Add(value);
		}

		public int Add(string Name, object Value)
		{
			return List.Add(new PropertyInstance(Name,Value));
		}
        
		/// <summary>
		/// <para>Copies the elements of an array to the end of the <see cref='.PropertyCollection'/>.</para>
		/// </summary>
		/// <param name='value'>
		///    An array of type <see cref='.PropertyInstance'/> containing the objects to add to the collection.
		/// </param>
		/// <returns>
		///   <para>None.</para>
		/// </returns>
		/// <seealso cref='.PropertyCollection.Add'/>
		public void AddRange(PropertyInstance[] value) 
		{
			for (int i = 0; (i < value.Length); i = (i + 1)) 
			{
				this.Add(value[i]);
			}
		}
        
		/// <summary>
		///     <para>
		///       Adds the contents of another <see cref='.PropertyCollection'/> to the end of the collection.
		///    </para>
		/// </summary>
		/// <param name='value'>
		///    A <see cref='.PropertyCollection'/> containing the objects to add to the collection.
		/// </param>
		/// <returns>
		///   <para>None.</para>
		/// </returns>
		/// <seealso cref='.PropertyCollection.Add'/>
		public void AddRange(PropertyCollection value) 
		{
			for (int i = 0; (i < value.Count); i = (i + 1)) 
			{
				this.Add(value[i]);
			}
		}
        
		/// <summary>
		/// <para>Gets a value indicating whether the 
		///    <see cref='.PropertyCollection'/> contains the specified <see cref='.PropertyInstance'/>.</para>
		/// </summary>
		/// <param name='value'>The <see cref='.PropertyInstance'/> to locate.</param>
		/// <returns>
		/// <para><see langword='true'/> if the <see cref='.PropertyInstance'/> is contained in the collection; 
		///   otherwise, <see langword='false'/>.</para>
		/// </returns>
		/// <seealso cref='.PropertyCollection.IndexOf'/>
		public bool Contains(PropertyInstance value) 
		{
			return List.Contains(value);
		}
        
		/// <summary>
		/// <para>Copies the <see cref='.PropertyCollection'/> values to a one-dimensional <see cref='System.Array'/> instance at the 
		///    specified index.</para>
		/// </summary>
		/// <param name='array'><para>The one-dimensional <see cref='System.Array'/> that is the destination of the values copied from <see cref='.PropertyCollection'/> .</para></param>
		/// <param name='index'>The index in <paramref name='array'/> where copying begins.</param>
		/// <returns>
		///   <para>None.</para>
		/// </returns>
		/// <exception cref='System.ArgumentException'><para><paramref name='array'/> is multidimensional.</para> <para>-or-</para> <para>The number of elements in the <see cref='.PropertyCollection'/> is greater than the available space between <paramref name='arrayIndex'/> and the end of <paramref name='array'/>.</para></exception>
		/// <exception cref='System.ArgumentNullException'><paramref name='array'/> is <see langword='null'/>. </exception>
		/// <exception cref='System.ArgumentOutOfRangeException'><paramref name='arrayIndex'/> is less than <paramref name='array'/>'s lowbound. </exception>
		/// <seealso cref='System.Array'/>
		public void CopyTo(PropertyInstance[] array, int index) 
		{
			List.CopyTo(array, index);
		}
        
		/// <summary>
		///    <para>Returns the index of a <see cref='.PropertyInstance'/> in 
		///       the <see cref='.PropertyCollection'/> .</para>
		/// </summary>
		/// <param name='value'>The <see cref='.PropertyInstance'/> to locate.</param>
		/// <returns>
		/// <para>The index of the <see cref='.PropertyInstance'/> of <paramref name='value'/> in the 
		/// <see cref='.PropertyCollection'/>, if found; otherwise, -1.</para>
		/// </returns>
		/// <seealso cref='.PropertyCollection.Contains'/>
		public int IndexOf(PropertyInstance value) 
		{
			return List.IndexOf(value);
		}
        
		/// <summary>
		/// <para>Inserts a <see cref='.PropertyInstance'/> into the <see cref='.PropertyCollection'/> at the specified index.</para>
		/// </summary>
		/// <param name='index'>The zero-based index where <paramref name='value'/> should be inserted.</param>
		/// <param name=' value'>The <see cref='.PropertyInstance'/> to insert.</param>
		/// <returns><para>None.</para></returns>
		/// <seealso cref='.PropertyCollection.Add'/>
		public void Insert(int index, PropertyInstance value) 
		{
			List.Insert(index, value);
		}
        
		/// <summary>
		///    <para>Returns an enumerator that can iterate through 
		///       the <see cref='.PropertyCollection'/> .</para>
		/// </summary>
		/// <returns><para>None.</para></returns>
		/// <seealso cref='System.Collections.IEnumerator'/>
		public new PropertyInstanceEnumerator GetEnumerator() 
		{
			return new PropertyInstanceEnumerator(this);
		}
        
		/// <summary>
		///    <para> Removes a specific <see cref='.PropertyInstance'/> from the 
		///    <see cref='.PropertyCollection'/> .</para>
		/// </summary>
		/// <param name='value'>The <see cref='.PropertyInstance'/> to remove from the <see cref='.PropertyCollection'/> .</param>
		/// <returns><para>None.</para></returns>
		/// <exception cref='System.ArgumentException'><paramref name='value'/> is not found in the Collection. </exception>
		public void Remove(PropertyInstance value) 
		{
			List.Remove(value);
		}
        
		public class PropertyInstanceEnumerator : object, IEnumerator 
		{
            
			private IEnumerator baseEnumerator;
            
			private IEnumerable temp;
            
			public PropertyInstanceEnumerator(PropertyCollection mappings) 
			{
				this.temp = ((IEnumerable)(mappings));
				this.baseEnumerator = temp.GetEnumerator();
			}
            
			public PropertyInstance Current 
			{
				get 
				{
					return ((PropertyInstance)(baseEnumerator.Current));
				}
			}
            
			object IEnumerator.Current 
			{
				get 
				{
					return baseEnumerator.Current;
				}
			}
            
			public bool MoveNext() 
			{
				return baseEnumerator.MoveNext();
			}
            
			bool IEnumerator.MoveNext() 
			{
				return baseEnumerator.MoveNext();
			}
            
			public void Reset() 
			{
				baseEnumerator.Reset();
			}
            
			void IEnumerator.Reset() 
			{
				baseEnumerator.Reset();
			}
		}
	}
}