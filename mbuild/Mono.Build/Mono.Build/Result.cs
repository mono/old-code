//
// A result of building something (of executing a rule)
//
// Basically, a stateless blob of data. Because it is stateless, 
// we can clone it or serialize it to save it for later.
//

using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;

namespace Mono.Build {

	[Serializable]
	public abstract class Result : IFingerprintable, ICloneable {
		// Default constructor (needed it seems...)

		public Result () {}

		// Clone

		// because we use derived types of Results, we need to Clone () an object of the
		// correct type, so this gets a little tricky.

		public virtual object Clone () {
			Result res = (Result) Activator.CreateInstance (GetType ());

			CloneTo (res);
			return res;
		}

		public void CopyValueTo (Result r)
		{
		    if (!GetType ().Equals (r.GetType ()))
			throw ExHelp.Argument ("r", "Argument type {0} doesn't match self type {1}",
					       r.GetType (), GetType ());

		    CloneTo (r);
		}

		protected abstract void CloneTo (Result dest);

		// Equality and hashcode

		public static bool operator == (Result left, Result right)
		{
		    // Copied from System.String

		    if (Object.ReferenceEquals (left, right))
			return true;

		    if (null == left || null == right)
			return false;

		    if (! left.GetType ().Equals (right.GetType ()))
			return false;

		    return left.ContentEquals (right);
		}

		public static bool operator != (Result left, Result right)
		{
		    return !(left == right);
		}

		public override bool Equals (object o)
		{
		    return (this == (o as Result));
		}

		protected abstract bool ContentEquals (Result other);

		public override int GetHashCode ()
		{
		    return InternalHash ();
		}

		protected abstract int InternalHash ();

		// Fingerprinting -- an MD5 of the result's data.
		// Cached is a cached value of the fingerprint; the
		// result implementation may compare some saved hints
		// and return the cached value if deemed appropriate.
		// 
		// It is acceptable if ctxt or cached is null. If cached
		// is null, the cache comparison should be skipped.
		//
		// If the result class needs a context to evaluate the fingerprint,
		// and ctxt is null, and the fingerprint is not cached,
		// throw an exception.

		public abstract Fingerprint GetFingerprint (IBuildContext ctxt, Fingerprint cached);

		public Fingerprint GetFingerprint () {
			return GetFingerprint (null, null);
		}

		// check that the result is valid. Return true if it is.
		// only meaningful for a result that is associated with
		// a resource stored external to the result cache; eg
		// an MBFile, etc.

		public virtual bool Check (IBuildContext ctxt) {
			return true;
		}

		// remove any external data associated with the Result.
		// returns true if any action was taken. 

		public virtual bool Clean (IBuildContext ctxt) {
			return false;
		}

		// clone the result to the 'dist' directory,
		// using the DistPath function in ctxt. Again, a noop
		// unless the result is stored external to the result
		// cache. Return true if anything was done.

		public virtual bool DistClone (IBuildContext ctxt) {
			return false;
		}

		// Simple XML export / import.

		public void ExportXml (XmlWriter xw, string id) {
			xw.WriteStartElement (null, "result", null);
			xw.WriteAttributeString ("id", id);
			xw.WriteAttributeString ("type", GetType().FullName);
			ExportXml (xw);
			xw.WriteEndElement ();
		}

		static Result MyCreateInstance (Type t)
		{
		    // We can't just use CreateInstance(Type) because we want to
		    // possibly grab protected parameterless ctors.

		    System.Globalization.CultureInfo inv = 
			System.Globalization.CultureInfo.InvariantCulture;

		    BindingFlags bf =
			BindingFlags.Default | BindingFlags.NonPublic | BindingFlags.Public |
			BindingFlags.Instance | BindingFlags.CreateInstance;

		    return (Result) System.Activator.CreateInstance (t, bf, 
								     Type.DefaultBinder, 
								     new object[0] {}, inv);
		}

		public static Result ImportXml (XmlReader xr, out string id, IWarningLogger log) {
			id = null;

			if (xr.Name != "result") {
				log.Warning (3019, "Expected 'result' node but got " + xr.Name + " on XML import", null);
				return null;
			}

			id = xr.GetAttribute ("id");

			if (id == null) {
				log.Warning (3019, "No 'id' attribute on result element during XML import", null);
				return null;
			}

			log.PushLocation (id);

			string type = xr.GetAttribute ("type");

			if (type == null) {
				log.Warning (3019, "No 'type' attribute on result element during XML import", null);
				log.PopLocation ();
				return null;
			}

			int depth = xr.Depth;
			Result r;

			while (xr.Read ())
				if (xr.NodeType != XmlNodeType.Attribute &&
				    xr.NodeType != XmlNodeType.Whitespace)
					break;

			//Console.WriteLine ("here: id {0}, type {1}", id, type);
			//Console.WriteLine ("here: {0}: {1} = \"{2}\"; ac = {3}, d = {4} (vs {5})", xr.NodeType, xr.Name, xr.Value,
			//		   xr.AttributeCount, xr.Depth, depth);

			try {
				Type t;

				t = System.Type.GetType (type, false);

				if (t == null) {
					log.Warning (3019, "Unknown result type during XML import", type);
					log.PopLocation ();
					return null;
				}

				if (!t.IsSubclassOf (typeof (Result))) {
					log.Warning (3019, "Type is not a subclass of Result", t.FullName);
					log.PopLocation ();
					return null;
				}
				
				r = MyCreateInstance (t);

				if (r.ImportXml (xr, log)) {
					log.PopLocation ();
					// error will be reported
					return null;
				}
				log.PopLocation ();
			} finally {
				while (xr.Depth > depth) {
					if (!xr.Read ()) {
						log.Warning (3019, "Unexpected end of XML document", null);
						break;
					}
				}
			}

			return r;
		}

		protected abstract void ExportXml (XmlWriter xw);

		protected abstract bool ImportXml (XmlReader xr, IWarningLogger log);
	}
}
