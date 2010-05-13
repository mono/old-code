//
// TypeLoader.cs: Loads types from a list of Assemblies
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Mono.TypeReflector
{
	public class TypeLoader {

		private static TraceSwitch info = 
			new TraceSwitch ("type-loader", "TypeLoader messages");

		// String collection
		private ICollection assemblies = null;
		private ICollection references = null;

		private bool matchFullName = true;
		private bool matchName = false;
		private bool matchBase = false;
		private bool matchMethodReturnType = false;
		private bool matchNamespace = false;

		public bool MatchFullName {
			get {return matchFullName;}
			set {matchFullName = value;}
		}

		public bool MatchClassName {
			get {return matchName;}
			set {matchName = value;}
		}

		public bool MatchBase {
			get {return matchBase;}
			set {matchBase = value;}
		}

		public bool MatchMethodReturnType {
			get {return matchMethodReturnType;}
			set {matchMethodReturnType = value;}
		}

		public bool MatchNamespace {
			get {return matchNamespace;}
			set {matchNamespace = value;}
		}

		public TypeLoader ()
		{
		}

		public TypeLoader (ICollection assemblies, ICollection references)
		{
			Assemblies = assemblies;
			References = references;
		}

		public ICollection Assemblies {
			get {return assemblies;}
			set {assemblies = GetAssemblies (value);}
		}

		public ICollection References {
			get {return references;}
			set {references = GetPartialAssemblies (value);}
		}

		private ICollection GetAssemblies (ICollection assemblies)
		{
			// Assemblies may contain directories; if it's a directory, load all
			// assemblies in the directory.
			IList realAssemblies = new ArrayList ();
			foreach (string a in assemblies) {
				DirectoryInfo di = new DirectoryInfo (a);
				// TODO: should the assembly search on directories be recursive?
				if (di.Exists) {
					foreach (FileInfo fi in di.GetFiles ("*.dll"))
						AddAssembly (realAssemblies, Assembly.LoadFrom (fi.FullName));
					foreach (FileInfo fi in di.GetFiles ("*.exe"))
						AddAssembly (realAssemblies, Assembly.LoadFrom (fi.FullName));
				}
				else
					AddAssembly (realAssemblies, Assembly.LoadFrom (a));
			}

			return realAssemblies;
		}

		private void AddAssembly (IList list, Assembly a)
		{
			Trace.WriteLineIf (info.TraceInfo, "Adding Assembly: " + a);
			if (a != null)
				list.Add (a);
		}

		private ICollection GetPartialAssemblies (ICollection partialNames)
		{
			IList realAssemblies = new ArrayList ();

			AssemblyName an = new AssemblyName ();
			// an.Version = new Version (-1, -1, -1, -1);

			foreach (string a in partialNames) {
				an.Name = a;
				AddAssembly (realAssemblies, Assembly.LoadWithPartialName (a));
			}
			return realAssemblies;
		}

		public ICollection LoadTypes (IList match)
		{
			if (assemblies == null)
				throw new ArgumentNullException ("Assemblies");
			if (match == null || match.Count == 0)
				throw new ArgumentNullException ("match");

			StringBuilder regex = new StringBuilder ();
			regex.Append (match[0]);
			for (int i = 1; i < match.Count; ++i)
				regex.AppendFormat ("|{0}", match[i]);

			Trace.WriteLineIf (info.TraceInfo, 
					string.Format ("using regex: '{0}'", regex.ToString()));

			Regex re = new Regex (regex.ToString());

			IList found = new ArrayList ();

			foreach (Assembly a in assemblies) {
				LoadMatchingTypesFrom (a, regex.ToString(), re, found);
			}

			foreach (Assembly a in references) {
				LoadMatchingTypesFrom (a, regex.ToString(), re, found);
			}

			return found;
		}

		private void LoadMatchingTypesFrom (Assembly where, string regex, Regex re, IList types)
		{
			try {
				Type[] _types = where.GetTypes();
				foreach (Type t in _types) {
					if (Matches (re, t))
						types.Add (t);
				}
			} catch (Exception e) {
				Trace.WriteLineIf (info.TraceError, String.Format (
					"Unable to load type regex `{0}' from `{1}'.",
					regex, where));
				Trace.WriteLineIf (info.TraceError, e.ToString());
			}
		}

		private bool Matches (Regex r, Type t)
		{
			bool f, c, b, rt, n;
			f = c = b = rt = n = false;
			if (MatchFullName)
				f = r.Match(t.FullName).Success;
			else if (MatchClassName)
				c = r.Match(t.Name).Success;
			else if (MatchNamespace)
				n = r.Match(t.Namespace).Success;
			if (MatchBase) {
				b = (!MatchFullName ? false : r.Match (t.BaseType.FullName).Success) ||
				    (!MatchClassName ? false : r.Match (t.BaseType.Name).Success) ||
				    (!MatchNamespace ? false : r.Match (t.BaseType.Namespace).Success);
			}
			// TODO: MatchMethodReturnType
			Trace.WriteLineIf (info.TraceVerbose, String.Format("TypeLoader.Matches: c={0}, b={1}, rt={2}, n={3}", c, b, rt, n));
			return f || c || b || rt || n;
		}
	}
}

// vim: noexpandtab
