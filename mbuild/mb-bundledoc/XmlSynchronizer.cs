// XmlSynchronizer.cs -- class for synchronizing XML documents and some aspect of a type

using System;
using System.Xml;
using System.Collections;

namespace MBBundleDoc {

	public abstract class XmlSynchronizer {

		XmlNode top;

		public XmlSynchronizer () {}

		public XmlNode Top {
			get { return top; }

			set { 
				if (value == null)
					throw new ArgumentNullException ();
				top = value;
			}
		}

		protected abstract IEnumerable SynchronizedItems { get; }
		protected abstract string ElementName { get; }
		protected abstract string GetItemIdentifier (object o);
		protected abstract void SynchronizeItem (object o, XmlElement e);

		protected abstract bool IsIgnoredElement (XmlElement e);
		protected abstract void HandleRemovedElement (XmlElement e);

		public virtual string XPath {
			get { return ElementName; }
			
			set { 
				throw new InvalidOperationException ();
			}
		}

		protected virtual bool IsIgnoredItem (object o) {
			return false;
		}

		protected virtual string GetElementIdentifier (XmlElement e) {
			return e.GetAttribute ("name");
		}

		protected virtual void InitializeItemElement (object o, XmlElement e) {
		}

		protected virtual XmlElement CreateItemElement (object o) {
			string ident = GetItemIdentifier (o);

			XmlElement e = top.OwnerDocument.CreateElement (ElementName);
			e.SetAttribute ("name", ident);
			InitializeItemElement (o, e);

			return e;
		}

		protected virtual void HandleIdentlessElement (XmlElement e) {
			Console.Error.WriteLine ("Synchronized XML node without identifier: {0}", e);
		}

		public void Synchronize () {
			if (top == null)
				throw new InvalidOperationException ("Need to initialize top XML node");
			if (XPath == null)
				throw new InvalidOperationException ("Need to initialize xpath expression");

			XmlNodeList nodes = top.SelectNodes (XPath);
			Hashtable idents = new Hashtable ();

			foreach (XmlNode node in nodes) {
				XmlElement e = node as XmlElement;

				//Console.WriteLine ("Exisiting node: {0}", node);

				if (e == null)
					continue;

				if (IsIgnoredElement (e))
					continue;

				string ident = GetElementIdentifier (e);

				if (ident == null) {
					HandleIdentlessElement (e);
					continue;
				}
				
				idents[ident] = e;
			}

			foreach (object o in SynchronizedItems) {
				string ident = GetItemIdentifier (o);
				XmlElement e;

				if (IsIgnoredItem (o))
					continue;

				//Console.WriteLine ("Existing item: {0}", ident);

				if (idents.ContainsKey (ident)) {
					e = (XmlElement) idents[ident];
					idents.Remove (ident);
				} else {
					e = CreateItemElement (o);
					top.AppendChild (e);
				}

				SynchronizeItem (o, e);
			}

			foreach (XmlElement e in idents.Values) {
				//Console.WriteLine ("removed item: {0}", e);
				HandleRemovedElement (e);
			}
		}

		// XML util

		public static XmlElement AssertChildElement (XmlElement parent, string name) {
			XmlNode child = parent.SelectSingleNode (name);

			if (child != null) {
				XmlElement e = child as XmlElement;

				if (e != null) 
					return e;

				string s = String.Format ("Expected child {0} of {1} to be an element, but it is {2}",
							  name, parent, child);
				throw new Exception (s);
			}

			XmlElement c_elem = parent.OwnerDocument.CreateElement (name);
			parent.AppendChild (c_elem);
			return c_elem;
		}

		public static XmlElement AssertDocsElement (XmlElement parent) {
			XmlElement docs = AssertChildElement (parent, "docs");

			XmlElement e = AssertChildElement (docs, "summary");
			if (e.InnerText == null || e.InnerText.Length == 0)
				e.InnerText = "To be added.";

			e = AssertChildElement (docs, "remarks");
			if (e.InnerText == null || e.InnerText.Length == 0)
				e.InnerText = "To be added.";

			return docs;
		}
	}
}
