//
// Mono.AppServer.Security.UserCollection
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
	///       A collection that stores <see cref='.User'/> objects.
	///    </para>
	/// </summary>
	/// <seealso cref='.UserCollection'/>
	
	[Serializable()]
	public class UserCollection : CollectionBase 
	{
        
		/// <summary>
		///     <para>
		///       Initializes a new instance of <see cref='.UserCollection'/>.
		///    </para>
		/// </summary>
		public UserCollection() 
		{
		}
		        
		/// <summary>
		///     <para>
		///       Initializes a new instance of <see cref='.UserCollection'/> based on another <see cref='.UserCollection'/>.
		///    </para>
		/// </summary>
		/// <param name='value'>
		///       A <see cref='.UserCollection'/> from which the contents are copied
		/// </param>
		public UserCollection(UserCollection value) 
		{
			this.AddRange(value);
		}
        
		/// <summary>
		///     <para>
		///       Initializes a new instance of <see cref='.UserCollection'/> containing any array of <see cref='.User'/> objects.
		///    </para>
		/// </summary>
		/// <param name='value'>
		///       A array of <see cref='.User'/> objects with which to intialize the collection
		/// </param>
		public UserCollection(User[] value) 
		{
			this.AddRange(value);
		}
        
		/// <summary>
		/// <para>Represents the entry at the specified index of the <see cref='.User'/>.</para>
		/// </summary>
		/// <param name='index'><para>The zero-based index of the entry to locate in the collection.</para></param>
		/// <value>
		///    <para> The entry at the specified index of the collection.</para>
		/// </value>
		/// <exception cref='System.ArgumentOutOfRangeException'><paramref name='index'/> is outside the valid range of indexes for the collection.</exception>
		public User this[int index] 
		{
			get 
			{
				return ((User)(List[index]));
			}
			set 
			{
				List[index] = value;
			}
		}
        
		/// <summary>
		///    <para>Adds a <see cref='.User'/> with the specified value to the 
		///    <see cref='.UserCollection'/> .</para>
		/// </summary>
		/// <param name='value'>The <see cref='.User'/> to add.</param>
		/// <returns>
		///    <para>The index at which the new element was inserted.</para>
		/// </returns>
		/// <seealso cref='.UserCollection.AddRange'/>
		public int Add(User value) 
		{
			return List.Add(value);
		}
        
		/// <summary>
		/// <para>Copies the elements of an array to the end of the <see cref='.UserCollection'/>.</para>
		/// </summary>
		/// <param name='value'>
		///    An array of type <see cref='.User'/> containing the objects to add to the collection.
		/// </param>
		/// <returns>
		///   <para>None.</para>
		/// </returns>
		/// <seealso cref='.UserCollection.Add'/>
		public void AddRange(User[] value) 
		{
			for (int i = 0; (i < value.Length); i = (i + 1)) 
			{
				this.Add(value[i]);
			}
		}
        
		/// <summary>
		///     <para>
		///       Adds the contents of another <see cref='.UserCollection'/> to the end of the collection.
		///    </para>
		/// </summary>
		/// <param name='value'>
		///    A <see cref='.UserCollection'/> containing the objects to add to the collection.
		/// </param>
		/// <returns>
		///   <para>None.</para>
		/// </returns>
		/// <seealso cref='.UserCollection.Add'/>
		public void AddRange(UserCollection value) 
		{
			for (int i = 0; (i < value.Count); i = (i + 1)) 
			{
				this.Add(value[i]);
			}
		}
        
		/// <summary>
		/// <para>Gets a value indicating whether the 
		///    <see cref='.UserCollection'/> contains the specified <see cref='.User'/>.</para>
		/// </summary>
		/// <param name='value'>The <see cref='.User'/> to locate.</param>
		/// <returns>
		/// <para><see langword='true'/> if the <see cref='.User'/> is contained in the collection; 
		///   otherwise, <see langword='false'/>.</para>
		/// </returns>
		/// <seealso cref='.UserCollection.IndexOf'/>
		public bool Contains(User value) 
		{
			return List.Contains(value);
		}
        
		/// <summary>
		/// <para>Copies the <see cref='.UserCollection'/> values to a one-dimensional <see cref='System.Array'/> instance at the 
		///    specified index.</para>
		/// </summary>
		/// <param name='array'><para>The one-dimensional <see cref='System.Array'/> that is the destination of the values copied from <see cref='.UserCollection'/> .</para></param>
		/// <param name='index'>The index in <paramref name='array'/> where copying begins.</param>
		/// <returns>
		///   <para>None.</para>
		/// </returns>
		/// <exception cref='System.ArgumentException'><para><paramref name='array'/> is multidimensional.</para> <para>-or-</para> <para>The number of elements in the <see cref='.UserCollection'/> is greater than the available space between <paramref name='arrayIndex'/> and the end of <paramref name='array'/>.</para></exception>
		/// <exception cref='System.ArgumentNullException'><paramref name='array'/> is <see langword='null'/>. </exception>
		/// <exception cref='System.ArgumentOutOfRangeException'><paramref name='arrayIndex'/> is less than <paramref name='array'/>'s lowbound. </exception>
		/// <seealso cref='System.Array'/>
		public void CopyTo(User[] array, int index) 
		{
			List.CopyTo(array, index);
		}
        
		/// <summary>
		///    <para>Returns the index of a <see cref='.User'/> in 
		///       the <see cref='.UserCollection'/> .</para>
		/// </summary>
		/// <param name='value'>The <see cref='.User'/> to locate.</param>
		/// <returns>
		/// <para>The index of the <see cref='.User'/> of <paramref name='value'/> in the 
		/// <see cref='.UserCollection'/>, if found; otherwise, -1.</para>
		/// </returns>
		/// <seealso cref='.UserCollection.Contains'/>
		public int IndexOf(User value) 
		{
			return List.IndexOf(value);
		}
        
		/// <summary>
		/// <para>Inserts a <see cref='.User'/> into the <see cref='.UserCollection'/> at the specified index.</para>
		/// </summary>
		/// <param name='index'>The zero-based index where <paramref name='value'/> should be inserted.</param>
		/// <param name=' value'>The <see cref='.User'/> to insert.</param>
		/// <returns><para>None.</para></returns>
		/// <seealso cref='.UserCollection.Add'/>
		public void Insert(int index, User value) 
		{
			List.Insert(index, value);
		}
        
		/// <summary>
		///    <para>Returns an enumerator that can iterate through 
		///       the <see cref='.UserCollection'/> .</para>
		/// </summary>
		/// <returns><para>None.</para></returns>
		/// <seealso cref='System.Collections.IEnumerator'/>
		public new UserEnumerator GetEnumerator() 
		{
			return new UserEnumerator(this);
		}
        
		/// <summary>
		///    <para> Removes a specific <see cref='.User'/> from the 
		///    <see cref='.UserCollection'/> .</para>
		/// </summary>
		/// <param name='value'>The <see cref='.User'/> to remove from the <see cref='.UserCollection'/> .</param>
		/// <returns><para>None.</para></returns>
		/// <exception cref='System.ArgumentException'><paramref name='value'/> is not found in the Collection. </exception>
		public void Remove(User value) 
		{
			List.Remove(value);
		}
        
		public class UserEnumerator : object, IEnumerator 
		{
            
			private IEnumerator baseEnumerator;
            
			private IEnumerable temp;
            
			public UserEnumerator(UserCollection mappings) 
			{
				this.temp = ((IEnumerable)(mappings));
				this.baseEnumerator = temp.GetEnumerator();
			}
            
			public User Current 
			{
				get 
				{
					return ((User)(baseEnumerator.Current));
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