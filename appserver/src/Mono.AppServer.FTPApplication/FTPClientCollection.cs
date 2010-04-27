//
// Mono.AppServer.FTPClientCollection
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;
using System.Collections;

namespace Mono.AppServer 
{
	
	/// <summary>
	///     <para>
	///       A collection that stores <see cref='.FTPClient'/> objects.
	///    </para>
	/// </summary>
	/// <seealso cref='.FTPClientCollection'/>
	[Serializable()]
	public class FTPClientCollection : CollectionBase 
	{
		
		/// <summary>
		///     <para>
		///       Initializes a new instance of <see cref='.FTPClientCollection'/>.
		///    </para>
		/// </summary>
		public FTPClientCollection() 
		{
		}
		
		/// <summary>
		///     <para>
		///       Initializes a new instance of <see cref='.FTPClientCollection'/> based on another <see cref='.FTPClientCollection'/>.
		///    </para>
		/// </summary>
		/// <param name='value'>
		///       A <see cref='.FTPClientCollection'/> from which the contents are copied
		/// </param>
		public FTPClientCollection(FTPClientCollection value) 
		{
			this.AddRange(value);
		}
		
		/// <summary>
		///     <para>
		///       Initializes a new instance of <see cref='.FTPClientCollection'/> containing any array of <see cref='.FTPClient'/> objects.
		///    </para>
		/// </summary>
		/// <param name='value'>
		///       A array of <see cref='.FTPClient'/> objects with which to intialize the collection
		/// </param>
		public FTPClientCollection(FTPClient[] value) 
		{
			this.AddRange(value);
		}
		
		/// <summary>
		/// <para>Represents the entry at the specified index of the <see cref='.FTPClient'/>.</para>
		/// </summary>
		/// <param name='index'><para>The zero-based index of the entry to locate in the collection.</para></param>
		/// <value>
		///    <para> The entry at the specified index of the collection.</para>
		/// </value>
		/// <exception cref='System.ArgumentOutOfRangeException'><paramref name='index'/> is outside the valid range of indexes for the collection.</exception>
		public FTPClient this[int ClientID]
		{
			get
			{
				foreach (FTPClient client in List)
				{
					if (client.ClientID==ClientID)
						return client;
				}
				return null;
			}
		}
		
		/// <summary>
		///    <para>Adds a <see cref='.FTPClient'/> with the specified value to the 
		///    <see cref='.FTPClientCollection'/> .</para>
		/// </summary>
		/// <param name='value'>The <see cref='.FTPClient'/> to add.</param>
		/// <returns>
		///    <para>The index at which the new element was inserted.</para>
		/// </returns>
		/// <seealso cref='.FTPClientCollection.AddRange'/>
		public int Add(FTPClient value) 
		{
			return List.Add(value);
		}
		
		/// <summary>
		/// <para>Copies the elements of an array to the end of the <see cref='.FTPClientCollection'/>.</para>
		/// </summary>
		/// <param name='value'>
		///    An array of type <see cref='.FTPClient'/> containing the objects to add to the collection.
		/// </param>
		/// <returns>
		///   <para>None.</para>
		/// </returns>
		/// <seealso cref='.FTPClientCollection.Add'/>
		public void AddRange(FTPClient[] value) 
		{
			for (int i = 0; (i < value.Length); i = (i + 1)) 
			{
				this.Add(value[i]);
			}
		}
		
		/// <summary>
		///     <para>
		///       Adds the contents of another <see cref='.FTPClientCollection'/> to the end of the collection.
		///    </para>
		/// </summary>
		/// <param name='value'>
		///    A <see cref='.FTPClientCollection'/> containing the objects to add to the collection.
		/// </param>
		/// <returns>
		///   <para>None.</para>
		/// </returns>
		/// <seealso cref='.FTPClientCollection.Add'/>
		public void AddRange(FTPClientCollection value) 
		{
			for (int i = 0; (i < value.Count); i = (i + 1)) 
			{
				this.Add(value[i]);
			}
		}
		
		/// <summary>
		/// <para>Gets a value indicating whether the 
		///    <see cref='.FTPClientCollection'/> contains the specified <see cref='.FTPClient'/>.</para>
		/// </summary>
		/// <param name='value'>The <see cref='.FTPClient'/> to locate.</param>
		/// <returns>
		/// <para><see langword='true'/> if the <see cref='.FTPClient'/> is contained in the collection; 
		///   otherwise, <see langword='false'/>.</para>
		/// </returns>
		/// <seealso cref='.FTPClientCollection.IndexOf'/>
		public bool Contains(FTPClient value) 
		{
			return List.Contains(value);
		}
		
		/// <summary>
		/// <para>Copies the <see cref='.FTPClientCollection'/> values to a one-dimensional <see cref='System.Array'/> instance at the 
		///    specified index.</para>
		/// </summary>
		/// <param name='array'><para>The one-dimensional <see cref='System.Array'/> that is the destination of the values copied from <see cref='.FTPClientCollection'/> .</para></param>
		/// <param name='index'>The index in <paramref name='array'/> where copying begins.</param>
		/// <returns>
		///   <para>None.</para>
		/// </returns>
		/// <exception cref='System.ArgumentException'><para><paramref name='array'/> is multidimensional.</para> <para>-or-</para> <para>The number of elements in the <see cref='.FTPClientCollection'/> is greater than the available space between <paramref name='arrayIndex'/> and the end of <paramref name='array'/>.</para></exception>
		/// <exception cref='System.ArgumentNullException'><paramref name='array'/> is <see langword='null'/>. </exception>
		/// <exception cref='System.ArgumentOutOfRangeException'><paramref name='arrayIndex'/> is less than <paramref name='array'/>'s lowbound. </exception>
		/// <seealso cref='System.Array'/>
		public void CopyTo(FTPClient[] array, int index) 
		{
			CopyTo(array, index);
		}
		
		/// <summary>
		///    <para>Returns the index of a <see cref='.FTPClient'/> in 
		///       the <see cref='.FTPClientCollection'/> .</para>
		/// </summary>
		/// <param name='value'>The <see cref='.FTPClient'/> to locate.</param>
		/// <returns>
		/// <para>The index of the <see cref='.FTPClient'/> of <paramref name='value'/> in the 
		/// <see cref='.FTPClientCollection'/>, if found; otherwise, -1.</para>
		/// </returns>
		/// <seealso cref='.FTPClientCollection.Contains'/>
		public int IndexOf(FTPClient value) 
		{
			return List.IndexOf(value);
		}
		
		/// <summary>
		/// <para>Inserts a <see cref='.FTPClient'/> into the <see cref='.FTPClientCollection'/> at the specified index.</para>
		/// </summary>
		/// <param name='index'>The zero-based index where <paramref name='value'/> should be inserted.</param>
		/// <param name=' value'>The <see cref='.FTPClient'/> to insert.</param>
		/// <returns><para>None.</para></returns>
		/// <seealso cref='.FTPClientCollection.Add'/>
		public void Insert(int index, FTPClient value) 
		{
			List.Insert(index, value);
		}
		
		/// <summary>
		///    <para>Returns an enumerator that can iterate through 
		///       the <see cref='.FTPClientCollection'/> .</para>
		/// </summary>
		/// <returns><para>None.</para></returns>
		/// <seealso cref='System.Collections.IEnumerator'/>
		public new FTPClientEnumerator GetEnumerator() 
		{
			return new FTPClientEnumerator(this);
		}
		
		/// <summary>
		///    <para> Removes a specific <see cref='.FTPClient'/> from the 
		///    <see cref='.FTPClientCollection'/> .</para>
		/// </summary>
		/// <param name='value'>The <see cref='.FTPClient'/> to remove from the <see cref='.FTPClientCollection'/> .</param>
		/// <returns><para>None.</para></returns>
		/// <exception cref='System.ArgumentException'><paramref name='value'/> is not found in the Collection. </exception>
		public void Remove(int ClientID) 
		{
			foreach (FTPClient client in List)
			{
				if (client.ClientID==ClientID)
				{
					List.Remove(client);
					break;
				}
			}
		}
		
		public class FTPClientEnumerator : object, IEnumerator 
		{
			
			private IEnumerator baseEnumerator;
			
			private IEnumerable temp;
			
			public FTPClientEnumerator(FTPClientCollection mappings) 
			{
				this.temp = ((IEnumerable)(mappings));
				this.baseEnumerator = temp.GetEnumerator();
			}
			
			public FTPClient Current 
			{
				get 
				{
					return ((FTPClient)(baseEnumerator.Current));
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
