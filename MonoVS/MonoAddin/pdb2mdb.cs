//
// Author:
//    Robert Jordan (robertj@gmx.net)
//
// Licensed under the terms of the MIT X11 license
//
// Copyright 2006 Robert Jordan
// Copyright 2008 Novell, Inc.
// 

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics.SymbolStore;
using Mono.CompilerServices.SymbolWriter;
using Mono.Cecil;

namespace Mono.Debugger.Utilities
{
	//
	// Converts PDB files into MDB files
	//
	public class pdb2mdb
	{
		static int Main (string[] args)
		{
			if (args.Length < 1) {
				Console.Error.WriteLine ("usage: pdb2mdb assembly");
				return 1;
			}

			pdb2mdb my = new pdb2mdb ();
			my.ConvertAssembly (args[0]);
			return 0;
		}

		public void CreateFromPDB (string path)
		{
			ConvertAssembly (path);
		}

		AssemblyDefinition assembly;
		ISymbolReader reader;
		MonoSymbolWriter writer;
		Dictionary<string, SourceFile> documentMap = new Dictionary<string, SourceFile> ();

		void ConvertAssembly (string fileName)
		{
			Guid moduleGuid;

			assembly = AssemblyFactory.GetAssembly (fileName);
			reader = Util.GetSymbolReaderForFile (fileName, null, out moduleGuid);
			writer = new MonoSymbolWriter (fileName);

			// import documents
			foreach (ISymbolDocument doc in reader.GetDocuments ()) {
				if (!documentMap.ContainsKey (doc.URL)) {
					SourceFileEntry file = writer.DefineDocument (doc.URL);
					CompileUnitEntry comp_unit = writer.DefineCompilationUnit (file);
					documentMap[doc.URL] = new SourceFile (comp_unit, file);
				}
			}

			// converts types
			foreach (TypeDefinition t in assembly.MainModule.Types) {
				ConvertType (t);
			}

			writer.WriteSymbolFile (moduleGuid);
		}

		void ConvertType (TypeDefinition t)
		{
			foreach (MethodDefinition md in t.Methods) {
				try {
					ConvertMethod (md, reader.GetMethod (new SymbolToken ((int) md.MetadataToken.ToUInt ())));
				} catch (COMException) {
					Console.Error.WriteLine ("no symbol for method {0}", md.Name);
				}
			}
		}

		void ConvertMethod (MethodDefinition md, ISymbolMethod method)
		{
			int count = method.SequencePointCount;
			if (count == 0) return;

			int[] offsets = new int[count];
			ISymbolDocument[] docs = new ISymbolDocument[count];
			int[] lines = new int[count];
			int[] cols = new int[count];
			int[] endLines = new int[count];
			int[] endCols = new int[count];

			method.GetSequencePoints (offsets, docs, lines, cols, endLines, endCols);

			SourceFile sf = documentMap[docs[0].URL];
			SourceMethod sm = new SourceMethod (md.Name, 0, (int) md.MetadataToken.ToUInt ());
			SourceMethodBuilder builder = writer.OpenMethod (sf, 0, sm);

			int last_line = 0;
			for (int i = 0; i < count; i++) {
				if (lines[i] == 0xfeefee)
					writer.MarkSequencePoint (offsets[i], sf.Entry.SourceFile, last_line, cols[i], true);
				else {
					writer.MarkSequencePoint (offsets[i], sf.Entry.SourceFile, lines[i], cols[i], false);
					last_line = lines[i];
				}
			}

			writer.CloseMethod ();
		}
	}

	class SourceFile : ISourceFile, ICompileUnit
	{
		SourceFileEntry entry;
		CompileUnitEntry comp_unit;

		public SourceFile (CompileUnitEntry comp_unit, SourceFileEntry entry)
		{
			this.entry = entry;
			this.comp_unit = comp_unit;
		}

		SourceFileEntry ISourceFile.Entry
		{
			get { return entry; }
		}

		public CompileUnitEntry Entry
		{
			get { return comp_unit; }
		}
	}

	class SourceMethod : IMethodDef
	{
		string name;
		int namespaceId;
		int token;

		public SourceMethod (string name, int namespaceId, int token)
		{
			this.name = name;
			this.namespaceId = namespaceId;
			this.token = token;
		}

		public string Name
		{
			get { return name; }
		}

		public int NamespaceID
		{
			get { return namespaceId; }
		}

		public int Token
		{
			get { return token; }
		}

	}

	static class Util
	{
		public static ISymbolReader GetSymbolReaderForFile (string pathModule, string searchPath, out Guid moduleGuid)
		{
			return GetSymbolReaderForFile (new SymBinder (), pathModule, searchPath, out moduleGuid);
		}

		public static ISymbolReader GetSymbolReaderForFile (SymBinder binder, string pathModule, string searchPath, out Guid moduleGuid)
		{
			Guid importerIID = new Guid ("FCE5EFA0-8BBA-4f8e-A036-8F2022B08466");
			object objDispenser = Activator.CreateInstance (Type.GetTypeFromCLSID (new Guid ("E5CB7A31-7512-11d2-89CE-0080C792E5D8")));

			// Now open an Importer on the given filename. We'll end up passing this importer straight
			// through to the Binder.
			object objImporter;
			IMetaDataDispenser dispenser = (IMetaDataDispenser) objDispenser;
			dispenser.OpenScope (pathModule, 0, ref importerIID, out objImporter);

			IntPtr importerPtr = IntPtr.Zero;
			ISymbolReader reader;
			try {
				// This will manually AddRef the underlying object, so we need to be very careful to Release it.
				importerPtr = Marshal.GetComInterfaceForObject (objImporter, typeof (IMetadataImport));

				reader = binder.GetReader (importerPtr, pathModule, searchPath);

				StringBuilder b = new StringBuilder (256);
				int length;
				((IMetadataImport) objImporter).GetScopeProps (b, b.Capacity, out length, out moduleGuid);
			} finally {
				if (importerPtr != IntPtr.Zero) {
					Marshal.Release (importerPtr);
				}
			}
			return reader;
		}
	}

	#region ComImports

	[Guid ("31BCFCE2-DAFB-11D2-9F81-00C04F79A0A3")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	interface IMetaDataDispenser
	{
		void Dummy ();
		void OpenScope ([In, MarshalAs (UnmanagedType.LPWStr)] String szScope, [In] Int32 dwOpenFlags, [In] ref Guid riid, [Out, MarshalAs (UnmanagedType.IUnknown)] out Object punk);
	}

	[Guid ("7DAC8207-D3AE-4c75-9B67-92801A497D44")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	public interface IMetadataImport
	{
		void Dummy1 ();
		void Dummy2 ();
		void Dummy3 ();
		void Dummy4 ();
		void Dummy5 ();
		void Dummy6 ();
		void Dummy7 ();

		void GetScopeProps (StringBuilder szName, int cchName, out int pchName, out Guid pmvid);
	}

	#endregion
}
