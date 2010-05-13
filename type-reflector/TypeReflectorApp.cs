//
// TypeReflectorApp.cs: 
//   Finds types and sends them to a displayer.
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

// #define TRACE

using System;
using System.Collections;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Mono.TypeReflector.Displayers;
using Mono.TypeReflector.Finders;
using Mono.TypeReflector.Formatters;

namespace Mono.TypeReflector
{
	public class TypeReflectorApp
	{
		private static BooleanSwitch console = new BooleanSwitch ("console",
				"console-specific and command-line handling output");

		private static void TraceArray (string message, IEnumerable contents)
		{
			Trace.WriteLineIf (console.Enabled, message);
			foreach (object o in contents) {
				Trace.WriteLineIf (console.Enabled, "  " + o);
			}
		}

		public static string Version {
			get {return "0.8.2003-09-28";}
		}

		public static void PrintVersion ()
		{
			Console.WriteLine ("type-reflector {0}", Version);
			Console.WriteLine ("Written by Jonathan Pryor.");
			Console.WriteLine ();
			Console.WriteLine ("Copyright (C) 2002-2003 Jonathan Pryor.");
		}

		private static void InitFactories ()
		{
			InitFactory ("displayers", Factories.Displayer);
			InitFactory ("finders",    Factories.Finder);
			InitFactory ("formatters", Factories.Formatter);
		}

		private static void InitFactory (string section, TypeFactory factory)
		{
			try {
				IDictionary d = (IDictionary) ConfigurationSettings.GetConfig (section);
				foreach (DictionaryEntry de in d) {
					try {
						string[] keys = de.Key.ToString().Split (':');
						string key = keys[0];
						string desc = keys.Length > 1 ? keys[1] : key;
						factory.Add (
							new TypeFactoryEntry (
								key, 
								desc,
								Type.GetType (de.Value.ToString(), true)));
					}
					catch (Exception e) {
						Trace.WriteLineIf (console.Enabled, 
								string.Format ("Error adding {0} ({1}): {2}",
									de.Key, de.Value, e.Message));
					}
				}
			}
			catch {
				Trace.WriteLineIf (console.Enabled, 
						string.Format ("Unable to open section: {0}", section));
			}
		}

		public static void Main (string[] args)
		{
			try {
				Execute (args);
			}
			catch (Exception e) {
				Console.WriteLine ("Internal Error: Unhandled Exception: {0}", e.ToString());
			}
			finally {
				Trace.Flush ();
			}
		}

		public static void Execute (string[] args)
		{
			InitFactories ();

			TypeReflectorOptions options = new TypeReflectorOptions ();

			bool quit = false;

			try {
				options.ParseOptions (args);
			} catch (Exception e) {
				Console.WriteLine (e.Message);
				Console.WriteLine ("See `{0} --help' for more information", ProgramOptions.ProgramName);
				return;
			}

			foreach (DictionaryEntry de in Factories.Displayer) {
				Trace.WriteLine (
					string.Format("registered displayer: {0}={1}", de.Key, 
						((TypeFactoryEntry)de.Value).Type));
			}

			if (options.FoundHelp) {
				Console.WriteLine (options.OptionsHelp);
				quit = true;
			}

			if (options.DefaultAssemblies) {
				Console.WriteLine ("The default search assemblies are:");
				foreach (string s in TypeReflectorOptions.GetDefaultAssemblies ()) {
					Console.WriteLine ("  {0}", s);
				}
				quit = true;
			}

			if (options.Version) {
				PrintVersion ();
				quit = true;
			}

			if (quit)
				return;

			TraceArray ("Explicit Assemblies: ", options.Assemblies);
			TraceArray ("Referenced Assemblies: ", options.References);
			TraceArray ("Search for Types: ", options.Types);

			TypeLoader loader = CreateLoader (options);

			TraceArray ("Actual Search Assemblies: ", loader.Assemblies);
			TraceArray ("Actual Search Assemblies: ", loader.References);

			ITypeDisplayer displayer = CreateDisplayer (options);
			if (displayer == null) {
				Console.WriteLine ("Error: invalid displayer: " + options.Displayer);
				return;
			}

			if (loader.Assemblies.Count == 0 && loader.References.Count == 0 && 
          displayer.AssembliesRequired) {
				Console.WriteLine ("Error: no assemblies specified.");
				Console.WriteLine ("See `{0} --help' for more information",
					ProgramOptions.ProgramName);
				return;
			}

			INodeFormatter formatter = CreateFormatter (options);
			if (formatter == null) {
				Console.WriteLine ("Error: invalid formatter: " + options.Formatter);
				return;
			}

			INodeFinder finder = CreateFinder (options);
			if (finder == null) {
				Console.WriteLine ("Error: invalid finder: " + options.Finder);
				return;
			}

			displayer.Finder = finder;
			displayer.Formatter = formatter;
			displayer.Options = options;

      displayer.InitializeInterface ();

			IList types = options.Types;
			if (types.Count == 0)
				types = new string[]{"."};

			// Find the requested types and display them.
			if (loader.Assemblies.Count != 0 || loader.References.Count != 0)
				FindTypes (displayer, loader, types);

			displayer.Run ();
		}
		
		public static void FindTypes (ITypeDisplayer displayer, TypeLoader loader, IList types)
		{
			try {
				ICollection typesFound = loader.LoadTypes (types);
				Trace.WriteLine (
					string.Format ("Types Found: {0}", typesFound.Count.ToString()));
				if (typesFound.Count > 0) {
					int curType = 1;
					foreach (Type type in typesFound) {
						displayer.AddType (type, curType++, typesFound.Count);
					}
				}
				else
					displayer.ShowError ("Unable to find types.");
			} catch (Exception e) {
				displayer.ShowError (string.Format ("Unable to display type: {0}.", 
					e.ToString()));
			}
		}

		public static TypeLoader CreateLoader (TypeReflectorOptions options)
		{
			TypeLoader loader = new TypeLoader (options.Assemblies, options.References);
			loader.MatchBase = options.MatchBase;
			loader.MatchFullName = options.MatchFullName;
			loader.MatchClassName = options.MatchClassName;
			loader.MatchNamespace = options.MatchNamespace;
			loader.MatchMethodReturnType = options.MatchReturnType;
			return loader;
		}

		public static ITypeDisplayer CreateDisplayer (TypeReflectorOptions options)
		{
			ITypeDisplayer d = null;
			if (options.Displayer != string.Empty)
				d = Factories.Displayer.Create (options.Displayer);
			else
				d = CreateDefaultDisplayer ();

			if (d != null) {
				d.MaxDepth = options.MaxDepth;
			}

			return d;
		}

		private static ITypeDisplayer CreateDefaultDisplayer ()
		{
			try {
				// Get the correct order to load displayers...
				string order = ConfigurationSettings.AppSettings["displayer-order"];
				foreach (string d in order.Split (' ')) {
					ITypeDisplayer displayer = Factories.Displayer.Create (d);
					if (displayer != null)
						return displayer;
				}
			}
			catch {
			}
			return null;
		}

		public static INodeFinder CreateFinder (TypeReflectorOptions options)
		{
			return CreateFinder (options.Finder, options);
		}

		public static INodeFinder CreateFinder (string finder, TypeReflectorOptions options)
		{
			INodeFinder f = Factories.Finder.Create (finder);

			uint cur = (uint) f.BindingFlags;

			SetFlag (ref cur, options.FlattenHierarchy, (uint) BindingFlags.FlattenHierarchy);
			SetFlag (ref cur, options.ShowNonPublic, (uint) BindingFlags.NonPublic);
			SetFlag (ref cur, options.ShowInheritedMembers, (uint) BindingFlags.FlattenHierarchy);

			f.BindingFlags = (BindingFlags) cur;

			cur = (uint) f.FindMembers;
			SetFlag (ref cur, options.ShowBase, (uint) FindMemberTypes.Base);
			SetFlag (ref cur, options.ShowConstructors, (uint) FindMemberTypes.Constructors);
			SetFlag (ref cur, options.ShowEvents, (uint) FindMemberTypes.Events);
			SetFlag (ref cur, options.ShowFields, (uint) FindMemberTypes.Fields);
			SetFlag (ref cur, options.ShowInterfaces, (uint) FindMemberTypes.Interfaces);
			SetFlag (ref cur, options.ShowMethods, (uint) FindMemberTypes.Methods);
			SetFlag (ref cur, options.ShowProperties, (uint) FindMemberTypes.Properties);
			SetFlag (ref cur, options.ShowTypeProperties, (uint) FindMemberTypes.TypeProperties);
			SetFlag (ref cur, options.ShowMonoBroken, (uint) FindMemberTypes.MonoBroken);
			SetFlag (ref cur, options.VerboseOutput, (uint) FindMemberTypes.VerboseOutput);
			f.FindMembers = (FindMemberTypes) cur;

			return f;
		}

		private static void SetFlag (ref uint cur, bool set, uint add)
		{
			if (set)
				cur |= add;
			else
				cur &= ~add;
		}

		public static INodeFormatter CreateFormatter (TypeReflectorOptions options)
		{
			return CreateFormatter (options.Formatter, options);
		}

		public static INodeFormatter CreateFormatter (string formatter, TypeReflectorOptions options)
		{
			INodeFormatter nformatter = Factories.Formatter.Create (formatter);
			NodeFormatter f = nformatter as NodeFormatter;

			if (f != null) {
				f.InvokeMethods = options.InvokeMethods;
			}

			return nformatter;
		}
	}
}

