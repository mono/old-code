// BundleDocumenter.cs -- actual documenter stuff

using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Reflection;
using System.Collections;

namespace MBBundleDoc {

	public abstract class TypeDocumenter {

		protected BundleDocumenter owner;

		public TypeDocumenter (BundleDocumenter owner) {
			if (owner == null)
				throw new ArgumentNullException ();

			this.owner = owner;
		}

		public abstract bool IsTypeMatch (Type t);
		protected abstract string ElementName { get; }
		protected abstract void DocumentDetails (Type t, XmlElement top);

		protected virtual string GetTypeName (Type t) {
			return t.Name;
		}

		public void DocumentType (Type t) {
			string path = owner.MakeDocumentPath (t);
			XmlDocument doc = owner.LoadXmlDocument (path, ElementName);

			doc.DocumentElement.SetAttribute ("name", GetTypeName (t));
			XmlSynchronizer.AssertDocsElement (doc.DocumentElement);
			DocumentDetails (t, doc.DocumentElement);

			owner.WriteXmlDocument (path, doc);
		}
	}
}
