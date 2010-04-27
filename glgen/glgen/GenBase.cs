// GtkSharp.Generation.GenBase.cs - The Generatable base class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2002 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Xml;

	public abstract class GenBase {
		
		protected string ns;
		protected XmlElement elem;

		protected GenBase (string ns, XmlElement elem)
		{
			this.ns = ns;
			this.elem = elem;
		}

		public XmlElement Elem {
			get {
				return elem;
			}
		}

		public string Name {
			get {
				return elem.GetAttribute ("name");
			}
		}

		public string Namespace {
			get {
				return ns;
			}
		}

		public string QualifiedName {
			get {
				return ns + "." + Name;
			}
		}
	}
}
