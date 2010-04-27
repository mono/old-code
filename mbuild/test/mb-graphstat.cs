// A quick tool to print statistics about a binary graph
// file.

using System;
using System.IO;

using Mono.Build;
using Monkeywrench;
using Monkeywrench.Compiler;

class X {

    public static int Main (string[] args)
    {
	if (args.Length != 1) {
	    Console.Error.WriteLine ("Usage: mb-graphstat graph-file-name");
	    return 1;
	}

	try {
	    Stream s = File.OpenRead (args[0]);
	    BufferedStream bs = new BufferedStream (s);

	    using (BinaryReader br = new BinaryReader (bs))
		Stats (br);
	} catch (Exception e) {
	    Console.Error.WriteLine ("Error reading file {0}: {1}", args[0], e);
	    return 2;
	}

	return 0;
    }

    public static void Stats (BinaryReader br)
    {
	byte[] dat;
	int val;

	dat = br.ReadBytes (4);

	if (dat[0] != (byte) 'M' || dat[1] != (byte) 'B' ||
	    dat[2] != (byte) 'G' || dat[3] != (byte) BinaryLoadedGraph.BinaryFormatIdent)
	{
	    throw ExHelp.App ("Unexpected header values: {0},{1},{2},{3}",
			      dat[0], dat[1], dat[2], dat[3]);
	}

	Console.WriteLine (" * Header OK");

	val = BinaryHelper.ReadRawInt (br);

	if (val != 0x01B2C3D4)
	    throw ExHelp.App ("Endianness marker not valid: got {0:x}", val);

	Console.WriteLine (" * Endianness OK");

	long pstart = br.BaseStream.Position;
	ProjectInfo pi = (ProjectInfo) BinaryHelper.ReadObject (br);
	long pend = br.BaseStream.Position;

	Console.WriteLine (" * Project info: name {0}, version {1}, compat code {2}, bfname {3}",
			   pi.Name, pi.Version, pi.CompatCode, pi.BuildfileName);
	Console.WriteLine (" * Project info entry consumes {0} bytes", pend - pstart);

	BinaryHelper.ExpectDelimiter (br);

	pstart = br.BaseStream.Position;
	DependentItemInfo[] dii = (DependentItemInfo[]) BinaryHelper.ReadObject (br);
	Console.WriteLine (" * Depends on {0} files", dii.Length);
	dii = (DependentItemInfo[]) BinaryHelper.ReadObject (br);
	Console.WriteLine (" * Depends on {0} bundles", dii.Length);
	pend = br.BaseStream.Position;
	Console.WriteLine (" * Dependency info consumes {0} bytes", pend - pstart);

	BinaryHelper.ExpectDelimiter (br);

	pstart = br.BaseStream.Position;
	int ntags = br.ReadInt32 ();
	Console.WriteLine (" * {0} tags", ntags);

	for (int i = 0; i < ntags; i++) {
	    string s = br.ReadString ();
	    int tagid = br.ReadInt32 ();
	    Console.WriteLine ("    {0} : {1}", s, tagid);
	}

	pend = br.BaseStream.Position;
	Console.WriteLine (" * Tag listing consumes {0} bytes", pend - pstart);

	BinaryHelper.ExpectDelimiter (br);

	int n = br.ReadInt32 ();
	Console.WriteLine (" * Dependency data table is {0} ints ({1} bytes)", n, n * 4);
	dat = br.ReadBytes (n * 4);

	BinaryHelper.ExpectDelimiter (br);

	n = br.ReadInt32 ();
	Console.WriteLine (" * Name data table is {0} bytes", n);
	dat = br.ReadBytes (n);

	BinaryHelper.ExpectDelimiter (br);

	pstart = br.BaseStream.Position;
	n = br.ReadInt32 ();
	Console.WriteLine (" * There are {0} providers", n);

	for (int i = 0; i < n; i++) {
	    int ntarg = br.ReadInt32 ();
	    string basis = br.ReadString ();
	    string declloc = br.ReadString ();
	    int deplen = br.ReadInt32 ();
	    dat = br.ReadBytes (ntarg * 4);
	    int namelen = br.ReadInt32 ();
	    dat = br.ReadBytes (ntarg * 4);
	    dat = br.ReadBytes (ntarg * 4);

	    Console.WriteLine ("   {0}: {1} targs, {2} bytes of dep, {3} bytes of name, decl {4}",
			       basis, ntarg, deplen * 4, namelen * 4, declloc);
	}

	pend = br.BaseStream.Position;
	Console.WriteLine (" * Provider listing consumes {0} bytes", pend - pstart);

	BinaryHelper.ExpectDelimiter (br);

	pstart = br.BaseStream.Position;
	n = br.ReadInt32 ();
	Console.WriteLine (" * There are {0} ints ({1} bytes) in the tag data table", 
			   n, n * 4);
	dat = br.ReadBytes (n * 4);
	int[] ofs = BinaryHelper.ReadRawInts (br, ntags);

	Console.Write (" * Tag chunk lengths:");

	for (int i = 0; i < ntags - 1; i++)
	    Console.Write (" {0}", ofs[i + 1] - ofs[i]);

	Console.WriteLine (" {0}", n - ofs[ntags - 1]);

	pend = br.BaseStream.Position;
	Console.WriteLine (" * Tag data consumes {0} bytes", pend - pstart);

	BinaryHelper.ExpectDelimiter (br);

	pstart = br.BaseStream.Position;
	Type[] typetab = (Type[]) BinaryHelper.ReadObject (br);
	pend = br.BaseStream.Position;
	Console.WriteLine (" * Type table has {0} entries, consumes {1} bytes", 
			   typetab.Length, pend - pstart);

	BinaryHelper.ExpectDelimiter (br);

	pstart = br.BaseStream.Position;
	Result[] rtab = (Result[]) BinaryHelper.ReadObject (br);
	pend = br.BaseStream.Position;
	Console.WriteLine (" * Result table has {0} entries, consumes {1} bytes", 
			   rtab.Length, pend - pstart);

	for (int i = 0; i < rtab.Length; i++)
	    Console.WriteLine ("    {0}: {1}", i, rtab[i]);

	try {
	    br.ReadByte ();
	} catch (Exception) {
	    Console.WriteLine (" * Found EOF as expected");
	    return;
	}

	throw ExHelp.App ("Did not find EOF at position {0} as expected", 
			  br.BaseStream.Position);
    }
}
