//
// A result that is an agglomeration of others. Ooh.
//

using System;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Runtime.Serialization;
using System.Xml;

namespace Mono.Build {

	[Serializable]
	public abstract class CompositeResult : Result {

		public CompositeResult () : base () {
		}

		// A result that is made out of several sub-Results.
		// They are implemented as fields or whatever in
		// subclasses. This class implements the serialization
		// logic. We serialize with a CompositeFingerprint, which
		// takes an array of Results. The TotalItems / CopyItems
		// mechanism provides a quasi-efficient way for creating
		// that array of Result members across the class heirarchy.

		// Return base.TotalItems plus the number
		// of Result members of your particular
		// CompositeResult subclass.

		protected virtual int TotalItems {
			get { return 0; }
		}

		// Copy the Result members of your implementation
		// into the array r, starting at the index given
		// by base.TotalItems.

		protected virtual void CopyItems (Result[] r) {
		}

		// A composite result can also have a 'default' member.
		// If it does, then if this result is passed as an argument
		// to a rule, the default member is tried as an argument
		// if the result itself can't be applied to an argument.
		//
		// This gives you a transparent way to expand a Result:
		// For instance, the CSharpCompile rule should return
		// a ClrExecutable result. But MCS also produces an associated
		// .mdb debug information file that should be installed
		// if possible. The single ClrExecutable result can be
		// replaced with a composite result holding a ClrExecutable
		// and an MDB MBFile, with the ClrExecutable being the 
		// default member. Old rules will continue to function,
		// but a smart installer can handle the MDB file.

		public virtual bool HasDefault { get { return false; } }

		public virtual Result Default { get { return null; } }

		// Helper functions

		protected Result FieldAsResult (object o) {
			if (o == null)
				return null;
			if (o is Result)
				return (Result) o;
			if (o is string)
				return new MBString ((string) o);
			if (o is bool)
				return new MBBool ((bool) o);

			throw new InvalidOperationException ("Don't know how to make object " + o.ToString () +
							     "into a Result.");
		}

		protected object CloneField (object o) {
			if (o == null)
				return null;
			if (o is Result)
				return (o as Result).Clone ();
			if (o is string)
				return o;
			if (o is bool)
				return o;

			throw new InvalidOperationException ("Don't know how to clone object " + o.ToString ());
		}

		protected Result[] GetItems () {
			Result[] r = new Result[TotalItems];
			CopyItems (r);
			return r;
		}

		protected override void CloneTo (Result r) {
		}

		public static Result FindCompatible (Result res, Type type) {
			// Check types, tunneling through default members of compat
			// results.

			bool gotit = false;

			while (!gotit) {
				if (type.IsInstanceOfType (res)) {
					gotit = true;
					break;
				}

				if (!(res is CompositeResult))
					break;

				CompositeResult cr = (CompositeResult) res;

				if (!cr.HasDefault || cr.Default == null)
					break;

				res = cr.Default;
			}

			if (gotit == false)
				return null;

			return res;
		}

		// External result commands - naive implementations.
		// If this ends up being a speed bottleneck, we can try
		// modifying mb-bundlegen to generate override functions
		// that call the function on each field directly. But
		// I doubt that will be necessary.

		public override bool Check (IBuildContext ctxt) {
			Result[] r = GetItems ();

			for (int i = 0; i < r.Length; i++) {
				if (r[i] == null)
					continue;
				if (!r[i].Check (ctxt))
					return false;
			}

			return true;
		}

		public override bool Clean (IBuildContext ctxt) {
			bool did_something = false;
			Result[] r = GetItems ();

			for (int i = 0; i < r.Length; i++) {
				if (r[i] == null)
					continue;
				did_something |= r[i].Clean (ctxt);
			}

			return did_something;
		}

		public override bool DistClone (IBuildContext ctxt) {
			bool did_something = false;
			Result[] r = GetItems ();

			for (int i = 0; i < r.Length; i++) {
				if (r[i] == null)
					continue;
				did_something |= r[i].DistClone (ctxt);
			}

			return did_something;
		}

		// Equality checking

		protected override bool ContentEquals (Result other)
		{
		    CompositeResult cr = (CompositeResult) other;

		    Result[] mine = GetItems ();
		    Result[] theirs = cr.GetItems ();

		    for (int i = 0; i < mine.Length; i++)
			if (mine[i] != theirs[i])
			    return false;

		    return true;
		}

		protected override int InternalHash ()
		{
		    int hash = 0;

		    Result[] mine = GetItems ();

		    for (int i = 0; i < mine.Length; i++)
			hash ^= mine[i].GetHashCode ();

		    return hash;
		}

		// Simple XML export / import

		ArrayList GetFieldData () {
			FieldInfo[] fi = GetType ().GetFields ();

			ArrayList list = new ArrayList ();

			for (int i = 0; i < fi.Length; i++) {
				if (!fi[i].IsDefined (typeof (CompositeResultFieldAttribute), true))
					continue;
				list.Add (fi[i]);
			}

			return list;
		}

		protected override void ExportXml (XmlWriter xw) {
			ArrayList fields = GetFieldData ();

			foreach (FieldInfo fi in fields) {
				xw.WriteStartElement ("field");
				xw.WriteAttributeString ("name", fi.Name);
				xw.WriteAttributeString ("decl_type", fi.DeclaringType.FullName);
				Result r = fi.GetValue (this) as Result;

				if (r == null)
					xw.WriteAttributeString ("is_null", "true");
				else
					r.ExportXml (xw, "");

				xw.WriteEndElement ();
			}
		}

		protected override bool ImportXml (XmlReader xr, IWarningLogger log) {
			// We try very hard to load composites because they are liable to
			// change between versions, and we don't want to trash people's 
			// configurations.

			ArrayList fields = GetFieldData ();
			bool[] check = new bool[fields.Count];
			int d = xr.Depth;

			while (!xr.EOF && xr.Depth >= d) {
				if (xr.NodeType != XmlNodeType.Element) {
					xr.Read ();
					continue;
				}

				if (xr.Name != "field") {
					log.Warning (3019, "Expected 'field' element inside composite but got '" + 
						     xr.Name + "' during XML import", null);
					xr.Read ();
					continue;
				}

				//Console.WriteLine ("composite doit: {0}: {1} = \"{2}\"; ac = {3}, d = {4}", xr.NodeType, xr.Name, xr.Value,
				//		   xr.AttributeCount, xr.Depth);

				bool is_null = false;
				string name = null;
				string decltype = null;

				name = xr.GetAttribute ("name");
				if (name == null) {
					log.Warning (3019, "Didn't find 'name' attribute in 'field' element during XML import", null);
					return true;
				}

				decltype = xr.GetAttribute ("decl_type");
				if (decltype == null)
					log.Warning (3019, "Didn't find 'decl_type' attribute in 'field' element during XML import; continuing", null);

				if (xr.GetAttribute ("is_null") != null)
					is_null = true;

				int i;
				FieldInfo fi = null;

				for (i = 0; i < fields.Count; i++) {
					fi = (FieldInfo) fields[i];

					if (fi.Name == name && fi.DeclaringType.FullName == decltype)
						break;
				}

				if (!is_null) {
					xr.Read ();

					while (xr.NodeType == XmlNodeType.Whitespace)
						xr.Read ();
				}

				//Console.WriteLine ("composite after maybe read: {0}: {1} = \"{2}\"; ac = {3}, d = {4}", xr.NodeType, xr.Name, xr.Value,
				//		   xr.AttributeCount, xr.Depth);

				if (i == fields.Count) {
					log.Warning (3019, "Didn't find the field '" + name + "' in self; still trying to load. Result may need to be uncached.", null);
					xr.Skip ();
					continue;
				}
			       
				if (is_null) {
					fi.SetValue (this, null);
					check[i] = true;
					continue;
				}

				string ignore;
				Result r = Result.ImportXml (xr, out ignore, log);
				if (r == null) {
					log.Warning (3019, "Couldn't load result for field '" + name + "'; still trying to load. Result may need to be uncached", null);
					continue;
				}

				try {
					fi.SetValue (this, r);
					check[i] = true;
				} catch (ArgumentException) {
					string msg = String.Format ("Field {0} is of type {1}, but {2} was restored from the XML. Still trying to load. Result may" +
								    " need to be uncached.", name, fi.MemberType, r.GetType());
					log.Warning (3019, msg, r.ToString ());
				}
			}

			for (int i = 0; i < fields.Count; i++) {
				if (check[i])
					continue;

				FieldInfo fi = (FieldInfo) fields[i];
				log.Warning (3019, "Didn't load field '" + fi.Name + "' in composite; still trying to load. Result may need to be uncached", null);
			}

			return false;
		}

		// Fingerprinting

		public override Fingerprint GetFingerprint (IBuildContext ctxt, Fingerprint cached) {
			return new CompositeFingerprint (GetItems (), ctxt, cached);
		}

		// Utility printing function

		public override string ToString () {
			Result[] r = GetItems ();

			StringBuilder sb = new StringBuilder (GetType ().ToString ());
			sb.Append (" {");
			for (int i = 0; i < r.Length; i++)
				sb.AppendFormat (" {0}: {1}", i, r[i]);
			sb.Append (" }");

			return sb.ToString ();
		}
	}
}
