//
// Mono.AppServer.ApplicationCollection
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;
using System.Collections;

namespace Mono.AppServer {
	
	/// <summary>
	///     <para>
	///       A collection that stores <see cref='.ApplicationBase'/> objects.
	///    </para>
	/// </summary>
	/// <seealso cref='.ApplicationCollection'/>
	[Serializable()]
	public class ApplicationCollection : CollectionBase {
		
		/// <summary>
		///     <para>
		///       Initializes a new instance of <see cref='.ApplicationCollection'/>.
		///    </para>
		/// </summary>
		public ApplicationCollection() {
		}
		
		/// <summary>
		///     <para>
		///       Initializes a new instance of <see cref='.ApplicationCollection'/> based on another <see cref='.ApplicationCollection'/>.
		///    </para>
		/// </summary>
		/// <param name='value'>
		///       A <see cref='.ApplicationCollection'/> from which the contents are copied
		/// </param>
		public ApplicationCollection(ApplicationCollection value) {
			this.AddRange(value);
		}
		
		/// <summary>
		///     <para>
		///       Initializes a new instance of <see cref='.ApplicationCollection'/> containing any array of <see cref='.ApplicationBase'/> objects.
		///    </para>
		/// </summary>
		/// <param name='value'>
		///       A array of <see cref='.ApplicationBase'/> objects with which to intialize the collection
		/// </param>
		public ApplicationCollection(ApplicationBase[] value) {
			this.AddRange(value);
		}
		
		/// <summary>
		/// <para>Represents the entry at the specified index of the <see cref='.ApplicationBase'/>.</para>
		/// </summary>
		/// <param name='index'><para>The zero-based index of the entry to locate in the collection.</para></param>
		/// <value>
		///    <para> The entry at the specified index of the collection.</para>
		/// </value>
		/// <exception cref='System.ArgumentOutOfRangeException'><paramref name='index'/> is outside the valid range of indexes for the collection.</exception>
		public ApplicationBase this[int index] {
			get {
				return ((ApplicationBase)(List[index]));
			}
			set {
				List[index] = value;
			}
		}
		
		public ApplicationBase this[string AppName]
		{
			get
			{
				foreach (ApplicationBase app in this.InnerList)
				{
					if (app.Name.ToLower()==AppName.ToLower())
					{
						return app;
					}
				}
				return null;
			}
		}

		/// <summary>
		///    <para>Adds a <see cref='.ApplicationBase'/> with the specified value to the 
		///    <see cref='.ApplicationCollection'/> .</para>
		/// </summary>
		/// <param name='value'>The <see cref='.ApplicationBase'/> to add.</param>
		/// <returns>
		///    <para>The index at which the new element was inserted.</para>
		/// </returns>
		/// <seealso cref='.ApplicationCollection.AddRange'/>
		public int Add(ApplicationBase value) {
			return List.Add(value);
		}
		
		/// <summary>
		/// <para>Copies the elements of an array to the end of the <see cref='.ApplicationCollection'/>.</para>
		/// </summary>
		/// <param name='value'>
		///    An array of type <see cref='.ApplicationBase'/> containing the objects to add to the collection.
		/// </param>
		/// <returns>
		///   <para>None.</para>
		/// </returns>
		/// <seealso cref='.ApplicationCollection.Add'/>
		public void AddRange(ApplicationBase[] value) {
			for (int i = 0; (i < value.Length); i = (i + 1)) {
				this.Add(value[i]);
			}
		}
		
		/// <summary>
		///     <para>
		///       Adds the contents of another <see cref='.ApplicationCollection'/> to the end of the collection.
		///    </para>
		/// </summary>
		/// <param name='value'>
		///    A <see cref='.ApplicationCollection'/> containing the objects to add to the collection.
		/// </param>
		/// <returns>
		///   <para>None.</para>
		/// </returns>
		/// <seealso cref='.ApplicationCollection.Add'/>
		public void AddRange(ApplicationCollection value) {
			for (int i = 0; (i < value.Count); i = (i + 1)) {
				this.Add(value[i]);
			}
		}
		
		/// <summary>
		/// <para>Gets a value indicating whether the 
		///    <see cref='.ApplicationCollection'/> contains the specified <see cref='.ApplicationBase'/>.</para>
		/// </summary>
		/// <param name='value'>The <see cref='.ApplicationBase'/> to locate.</param>
		/// <returns>
		/// <para><see langword='true'/> if the <see cref='.ApplicationBase'/> is contained in the collection; 
		///   otherwise, <see langword='false'/>.</para>
		/// </returns>
		/// <seealso cref='.ApplicationCollection.IndexOf'/>
		public bool Contains(ApplicationBase value) {
			return List.Contains(value);
		}
		
		/// <summary>
		/// <para>Copies the <see cref='.ApplicationCollection'/> values to a one-dimensional <see cref='System.Array'/> instance at the 
		///    specified index.</para>
		/// </summary>
		/// <param name='array'><para>The one-dimensional <see cref='System.Array'/> that is the destination of the values copied from <see cref='.ApplicationCollection'/> .</para></param>
		/// <param name='index'>The index in <paramref name='array'/> where copying begins.</param>
		/// <returns>
		///   <para>None.</para>
		/// </returns>
		/// <exception cref='System.ArgumentException'><para><paramref name='array'/> is multidimensional.</para> <para>-or-</para> <para>The number of elements in the <see cref='.ApplicationCollection'/> is greater than the available space between <paramref name='arrayIndex'/> and the end of <paramref name='array'/>.</para></exception>
		/// <exception cref='System.ArgumentNullException'><paramref name='array'/> is <see langword='null'/>. </exception>
		/// <exception cref='System.ArgumentOutOfRangeException'><paramref name='arrayIndex'/> is less than <paramref name='array'/>'s lowbound. </exception>
		/// <seealso cref='System.Array'/>
		public void CopyTo(ApplicationBase[] array, int index) {
			List.CopyTo(array, index);
		}
		
		/// <summary>
		///    <para>Returns the index of a <see cref='.ApplicationBase'/> in 
		///       the <see cref='.ApplicationCollection'/> .</para>
		/// </summary>
		/// <param name='value'>The <see cref='.ApplicationBase'/> to locate.</param>
		/// <returns>
		/// <para>The index of the <see cref='.ApplicationBase'/> of <paramref name='value'/> in the 
		/// <see cref='.ApplicationCollection'/>, if found; otherwise, -1.</para>
		/// </returns>
		/// <seealso cref='.ApplicationCollection.Contains'/>
		public int IndexOf(ApplicationBase value) {
			return List.IndexOf(value);
		}
		
		/// <summary>
		/// <para>Inserts a <see cref='.ApplicationBase'/> into the <see cref='.ApplicationCollection'/> at the specified index.</para>
		/// </summary>
		/// <param name='index'>The zero-based index where <paramref name='value'/> should be inserted.</param>
		/// <param name=' value'>The <see cref='.ApplicationBase'/> to insert.</param>
		/// <returns><para>None.</para></returns>
		/// <seealso cref='.ApplicationCollection.Add'/>
		public void Insert(int index, ApplicationBase value) {
			List.Insert(index, value);
		}
		
		/// <summary>
		///    <para>Returns an enumerator that can iterate through 
		///       the <see cref='.ApplicationCollection'/> .</para>
		/// </summary>
		/// <returns><para>None.</para></returns>
		/// <seealso cref='System.Collections.IEnumerator'/>
		public new ApplicationBaseEnumerator GetEnumerator() {
			return new ApplicationBaseEnumerator(this);
		}
		
		/// <summary>
		///    <para> Removes a specific <see cref='.ApplicationBase'/> from the 
		///    <see cref='.ApplicationCollection'/> .</para>
		/// </summary>
		/// <param name='value'>The <see cref='.ApplicationBase'/> to remove from the <see cref='.ApplicationCollection'/> .</param>
		/// <returns><para>None.</para></returns>
		/// <exception cref='System.ArgumentException'><paramref name='value'/> is not found in the Collection. </exception>
		public void Remove(ApplicationBase value) {
			List.Remove(value);
		}
		
		public class ApplicationBaseEnumerator : object, IEnumerator {
			
			private IEnumerator baseEnumerator;
			
			private IEnumerable temp;
			
			public ApplicationBaseEnumerator(ApplicationCollection mappings) {
				this.temp = ((IEnumerable)(mappings));
				this.baseEnumerator = temp.GetEnumerator();
			}
			
			public ApplicationBase Current {
				get {
					return ((ApplicationBase)(baseEnumerator.Current));
				}
			}
			
			object IEnumerator.Current {
				get {
					return baseEnumerator.Current;
				}
			}
			
			public bool MoveNext() {
				return baseEnumerator.MoveNext();
			}
			
			bool IEnumerator.MoveNext() {
				return baseEnumerator.MoveNext();
			}
			
			public void Reset() {
				baseEnumerator.Reset();
			}
			
			void IEnumerator.Reset() {
				baseEnumerator.Reset();
			}
		}
	}
}
