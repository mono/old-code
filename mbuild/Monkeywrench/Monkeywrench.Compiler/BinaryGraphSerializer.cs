using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Mono.Build;
using Mono.Build.Bundling;

namespace Monkeywrench.Compiler {

    public class BinaryGraphSerializer {

	public static void Write (GraphBuilder gb, string file)
	{
	    BinaryGraphSerializer bsg = new BinaryGraphSerializer (gb, file);

	    try {
		bsg.Write ();
	    } catch (Exception e) {
		File.Delete (file);
		throw e;
	    }
	}

	GraphBuilder gb;
	Stream s;

	const int BufferSize = 1024;

	BinaryGraphSerializer (GraphBuilder gb, string file)
	{
	    this.gb = gb;
	    s = File.Create (file, BufferSize);
	}

	BinaryWriter bw;

	void Write ()
	{
	    using (bw = new BinaryWriter (s)) {
		bw.Write ((byte) 'M');
		bw.Write ((byte) 'B');
		bw.Write ((byte) 'G');
		bw.Write (BinaryLoadedGraph.BinaryFormatIdent);
		
		BinaryHelper.WriteRaw (bw, 0x01B2C3D4);
		
		WriteProjectInfo ();
		BinaryHelper.WriteDelimiter (bw);
		WriteDependents ();
		BinaryHelper.WriteDelimiter (bw);
		WriteTags ();
		BinaryHelper.WriteDelimiter (bw);
		WriteTargetTables ();
		BinaryHelper.WriteDelimiter (bw);
		WriteProviders ();
		BinaryHelper.WriteDelimiter (bw);
		WriteTagTables ();
		BinaryHelper.WriteDelimiter (bw);
		WriteTypeTable ();
		BinaryHelper.WriteDelimiter (bw);
		WriteResultTable ();
	    }
	}

	void WriteProjectInfo ()
	{
	    BinaryHelper.WriteObject (bw, gb.PInfo);
	}

	void WriteDependents ()
	{
	    DependentItemInfo[] diis;

	    diis = BinaryHelper.Unwrap<DependentItemInfo> (gb.GetDependentFiles ());
	    BinaryHelper.WriteObject (bw, diis);
	    diis = BinaryHelper.Unwrap<DependentItemInfo> (gb.GetDependentBundles ());
	    BinaryHelper.WriteObject (bw, diis);
	}

	void WriteTags ()
	{
	    bw.Write (gb.NumTags);

	    int nwritten = 0;

	    foreach (string name in gb.GetTags ()) {
		int id = gb.GetTagId (name);

		bw.Write (name);
		bw.Write (id);
		nwritten ++;
	    }

	    if (nwritten != gb.NumTags)
		throw ExHelp.App ("Report NumTags ({0}) is not actual ({1})?", 
				  gb.NumTags, nwritten);
	}

	// Table generation

	Encoding enc = Encoding.UTF8;

	struct ProvData {
	    public WrenchProvider pb;
	    public Dictionary<int,WrenchTarget> targids;

	    public int dep_chunk_len;
	    public int[] dep_offsets;
	    public int name_chunk_len;
	    public int[] name_offsets;

	    public ProvData (WrenchProvider pb)
	    {
		this.pb = pb;

		int ntarg = pb.NumTargets;

		dep_chunk_len = 0;
		name_chunk_len = 0;

		dep_offsets = new int[ntarg];
		name_offsets = new int[ntarg];

		targids = new Dictionary<int,WrenchTarget> ();
		int nseen = 0;

		foreach (WrenchTarget tb in pb.DefinedTargets) {
		    targids[tb.ShortId] = tb;
		    nseen++;
		}

		if (nseen != ntarg)
		    throw ExHelp.App ("Report NumTargets ({0}) for provider {1} not number " +
				      "actually seen ({2})", ntarg, pb.Basis, nseen);
	    }

	    class DepsCollector : IDependencyVisitor {
		List<int> unnamed_vals = new List<int> ();
		List<int> deford_vals = new List<int> ();
		Dictionary<int,List<int>> named_vals = new Dictionary<int,List<int>> ();
		Dictionary<int,int> default_vals = new Dictionary<int,int> ();

		BinaryGraphSerializer bsg;

		public DepsCollector (BinaryGraphSerializer bsg)
		{
		    this.bsg = bsg;
		}

		public bool VisitUnnamed (SingleValue<TargetBuilder> sv)
		{
		    unnamed_vals.Add (bsg.CompileSingleValue (sv));
		    return false;
		}

		public bool VisitNamed (int arg, SingleValue<TargetBuilder> sv)
		{
		    if (!named_vals.ContainsKey (arg))
			named_vals[arg] = new List<int> ();

		    named_vals[arg].Add (bsg.CompileSingleValue (sv));
		    return false;
		}
		
		public bool VisitDefaultOrdered (SingleValue<TargetBuilder> sv)
		{
		    deford_vals.Add (bsg.CompileSingleValue (sv));
		    return false;
		}

		public bool VisitDefaultValue (int arg, SingleValue<TargetBuilder> sv)
		{
		    default_vals[arg] = bsg.CompileSingleValue (sv);
		    return false;
		}

		public int Write ()
		{
		    int len = 0;

		    if (unnamed_vals.Count > 0) {
			BinaryHelper.WriteRaw (bsg.bw, -1); // arg ID
			BinaryHelper.WriteRaw (bsg.bw, -1); // default value - none
			BinaryHelper.WriteRaw (bsg.bw, unnamed_vals); // deps
			len += 2 + unnamed_vals.Count;
		    }

		    if (deford_vals.Count > 0) {
			BinaryHelper.WriteRaw (bsg.bw, -2); // arg ID
			BinaryHelper.WriteRaw (bsg.bw, -1); // no default value
			BinaryHelper.WriteRaw (bsg.bw, deford_vals); // deps
			len += 2 + deford_vals.Count;
		    }

		    foreach (int arg in named_vals.Keys) {
			List<int> list = named_vals[arg];

			BinaryHelper.WriteRaw (bsg.bw, -(arg + 3));

			if (default_vals.ContainsKey (arg))
			    BinaryHelper.WriteRaw (bsg.bw, default_vals[arg]);
			else
			    BinaryHelper.WriteRaw (bsg.bw, -1);

			BinaryHelper.WriteRaw (bsg.bw, list);

			len += 2 + list.Count;
		    }

		    // I am so glad that I remembered to add this section
		    // of code the first time around.

		    foreach (int arg in default_vals.Keys) {
			if (named_vals.ContainsKey (arg))
			    // only need to add sections for args with
			    // only a default value specified
			    continue;

			BinaryHelper.WriteRaw (bsg.bw, -(arg + 3));
			BinaryHelper.WriteRaw (bsg.bw, default_vals[arg]);
			len += 2;
		    }

		    return len;
		}
	    }

	    public int WriteDeps (BinaryGraphSerializer bsg, int offset)
	    {
		int orig_ofs = offset;

		for (int i = 0; i < dep_offsets.Length; i++) {
		    dep_offsets[i] = offset;
		    offset += WriteDepsForTarget (bsg, i, offset);
		}

		dep_chunk_len = offset - orig_ofs;
		return offset;
	    }

	    int WriteDepsForTarget (BinaryGraphSerializer bsg, int tid, int offset)
	    {
		WrenchTarget tb = targids[tid];
		int nwritten = 0;

		DepsCollector dc = new DepsCollector (bsg);
		tb.VisitDependencies (dc);
		nwritten = dc.Write ();

		return nwritten;
	    }

	    public int WriteNames (BinaryGraphSerializer bsg, int offset)
	    {
		int orig_ofs = offset;

		for (int i = 0; i < dep_offsets.Length; i++) {
		    name_offsets[i] = offset;
		    offset += WriteNameForTarget (bsg, i, offset);
		}

		name_chunk_len = offset - orig_ofs;
		return offset;
	    }

	    int WriteNameForTarget (BinaryGraphSerializer bsg, int lowid, int offset)
	    {
		WrenchTarget tb = targids[lowid];

		byte[] data = bsg.enc.GetBytes (tb.Name);
		bsg.bw.Write (data);

		return data.Length;
	    }

	    public void WriteProviderData (BinaryGraphSerializer bsg)
	    {
		bsg.bw.Write (dep_offsets.Length);
		bsg.bw.Write (pb.Basis);
		bsg.bw.Write (pb.DeclarationLoc);
		bsg.bw.Write (dep_chunk_len);
		BinaryHelper.WriteRaw (bsg.bw, dep_offsets);
		bsg.bw.Write (name_chunk_len);
		BinaryHelper.WriteRaw (bsg.bw, name_offsets);

		for (int i = 0; i < dep_offsets.Length; i++) {
		    WrenchTarget tb = targids[i];
		    BinaryHelper.WriteRaw (bsg.bw, bsg.RegisterType (tb.RuleType));
		    bsg.ScanTags (tb); // For later ...
		}
	    }
	}

	ProvData[] provdata;

	void WriteTargetTables ()
	{
	    // Initialize provider offset tables

	    int nprov = gb.NumProviders;
	    provdata = new ProvData[nprov];
	    int numseen = 0;

	    foreach (WrenchProvider pb in gb.Providers) {
		provdata[pb.Id] = new ProvData (pb);
		numseen++;
	    }

	    if (numseen != nprov)
		throw ExHelp.App ("Reported NumProviders ({0}) does not agree with seen ({1})?",
				  nprov, numseen);

	    // First dep chunks

	    long pos = bw.BaseStream.Position;
	    int totallen = 0;
	    bw.Write (-1); // placeholder

	    for (int i = 0; i < nprov; i++)
		totallen = provdata[i].WriteDeps (this, totallen);

	    long retpos = bw.BaseStream.Position;
	    // there is a bw.Seek that maybe we should use, but it only 
	    // takes int arguments
	    bw.BaseStream.Position = pos;
	    bw.Write (totallen);
	    bw.BaseStream.Position = retpos;

	    BinaryHelper.WriteDelimiter (bw);

	    // Now name chunks

	    pos = bw.BaseStream.Position;
	    totallen = 0;
	    bw.Write (-1); // placeholder

	    for (int i = 0; i < nprov; i++)
		totallen = provdata[i].WriteNames (this, totallen);

	    retpos = bw.BaseStream.Position;
	    bw.BaseStream.Position = pos;
	    bw.Write (totallen);
	    bw.BaseStream.Position = retpos;
	}

	void WriteProviders ()
	{
	    bw.Write (provdata.Length);

	    for (int i = 0; i < provdata.Length; i++)
		provdata[i].WriteProviderData (this);
	}

	Dictionary<int,List<int>> tag_items = new Dictionary<int,List<int>> ();

	int CompileSingleValue (SingleValue<TargetBuilder> sv)
	{
	    if (sv.IsTarget)
		return ((WrenchTarget) (TargetBuilder) sv).Id;

	    return RegisterResult ((Result) sv);
	}

	void ScanTags (WrenchTarget wt)
	{
	    foreach (KeyValuePair<string,SingleValue<TargetBuilder>> kvp in wt.Tags) {
		int tagid = gb.GetTagId (kvp.Key);

		if (tagid < 0)
		    throw ExHelp.Argument ("tag", "Invalid tag name {0}", kvp.Key);

		List<int> list;

		if (tag_items.ContainsKey (tagid))
		    list = tag_items[tagid];
		else {
		    list = new List<int> ();
		    tag_items[tagid] = list;
		}

		list.Add (wt.Id);
		list.Add (CompileSingleValue (kvp.Value));
	    }
	}

	void WriteTagTables ()
	{
	    int totallen = 0;
	    int[] tag_offsets = new int[gb.NumTags];

	    for (int i = 0; i < gb.NumTags; i++) {
		tag_offsets[i] = totallen;
		totallen += tag_items[i].Count;
	    }

	    bw.Write (totallen);

	    for (int i = 0; i < gb.NumTags; i++)
		BinaryHelper.WriteRaw (bw, tag_items[i]);

	    
	    BinaryHelper.WriteRaw (bw, tag_offsets);
	}

	List<Type> type_table = new List<Type> ();

	int RegisterType (Type t)
	{
	    int idx = type_table.IndexOf (t);

	    if (idx < 0) {
		type_table.Add (t);
		idx = type_table.Count - 1;
	    }

	    return idx;
	}

	void WriteTypeTable ()
	{
	    BinaryHelper.WriteObject (bw, BinaryHelper.Unwrap<Type> (type_table));
	}
		
	List<Result> result_table = new List<Result> ();

	int RegisterResult (Result r)
	{
	    int idx = result_table.IndexOf (r);

	    if (idx < 0) {
		result_table.Add (r);
		idx = result_table.Count - 1;

		if (idx > 0xFFFF)
		    throw ExHelp.App ("Too many constant results in graph!");
	    }

	    return 0x7FFF0000 | idx;
	}

	void WriteResultTable ()
	{
	    BinaryHelper.WriteObject (bw, BinaryHelper.Unwrap<Result> (result_table));
	}
		
    }
}
