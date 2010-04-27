using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Mono.Build;

namespace Monkeywrench.Compiler {

    public class BinaryLoadedGraph : IGraphState {

	BinaryLoadedGraph () {}

	public static BinaryLoadedGraph Load (string f, IWarningLogger log)
	{
	    FileStream fs = File.OpenRead (f);

	    using (BufferedStream bs = new BufferedStream (fs))
		return Load (bs, log);
	}

	public static BinaryLoadedGraph Load (Stream s, IWarningLogger log)
	{
	    BinaryLoadedGraph blg = new BinaryLoadedGraph ();

	    try {
		if (blg.LoadInternal (s, log))
		    return null;
	    } catch (Exception e) {
		log.Error (1012, "Unhandled exception during graph load", e.Message);
		return null;
	    }

	    return blg;
	}

	public const byte BinaryFormatIdent = (byte) '1';

	bool LoadInternal (Stream s, IWarningLogger log)
	{
	    BinaryReader br = new BinaryReader (s);

	    // 4-byte identification header.

	    byte[] b = br.ReadBytes (4);
	    if (b[0] != (byte) 'M' ||
		b[1] != (byte) 'B' ||
		b[2] != (byte) 'G' ||
		b[3] != BinaryFormatIdent) {
		log.Error (1012, "Invalid header in saved graph file", null);
		return true;
	    }

	    // 32-bit int check for endianness

	    if (BinaryHelper.ReadRawInt (br) != 0x01B2C3D4) {
		log.Error (1012, "Endianness change in saved graph file", null);
		return true;
	    }

	    // Actual data

	    ReadProjectInfo (br);
	    BinaryHelper.ExpectDelimiter (br);
	    ReadDependents (br);
	    BinaryHelper.ExpectDelimiter (br);
	    ReadTags (br);
	    BinaryHelper.ExpectDelimiter (br);
	    ReadTargetTables (br);
	    BinaryHelper.ExpectDelimiter (br);
	    ReadProviders (br);
	    BinaryHelper.ExpectDelimiter (br);
	    ReadTagTable (br);
	    BinaryHelper.ExpectDelimiter (br);
	    ReadTypeTable (br);
	    BinaryHelper.ExpectDelimiter (br);
	    ReadResultTable (br);
	    return false;
	}

	// Helper

	void GetOffsetRange (int id, int[] table, int chunklen, out int start, out int bound)
	{
	    if (id < 0 || id >= table.Length)
		throw ExHelp.Range ("Id {0} is out of range (max {1})", id, table.Length);

	    start = table[id];

	    if (id < table.Length - 1)
		bound = table[id + 1];
	    else
		bound = table[0] + chunklen;
	}

	// Project Info

	ProjectInfo pinfo;

	void ReadProjectInfo (BinaryReader br)
	{
	    pinfo = (ProjectInfo) BinaryHelper.ReadObject (br);
	}

	public ProjectInfo GetProjectInfo ()
	{
	    return pinfo;
	}

	// Dependent items

	DependentItemInfo[] dep_files;
	DependentItemInfo[] dep_bundles;

	void ReadDependents (BinaryReader br)
	{
	    dep_files = (DependentItemInfo[]) BinaryHelper.ReadObject (br);
	    dep_bundles = (DependentItemInfo[]) BinaryHelper.ReadObject (br);
	}

	public IEnumerable<DependentItemInfo> GetDependentFiles ()
	{
	    return dep_files;
	}

	public IEnumerable<DependentItemInfo> GetDependentBundles ()
	{
	    return dep_bundles;
	}

	// Tags

	Dictionary<string,int> tags;

	void ReadTags (BinaryReader br)
	{
	    tags = new Dictionary<string,int> ();
	
	    int nentries = br.ReadInt32 ();

	    for (int i = 0; i < nentries; i++) {
		string name = br.ReadString ();
		int id = br.ReadInt32 ();

		tags[name] = id;
	    }
	}

	public int GetTagId (string tag)
	{
	    if (!tags.ContainsKey (tag))
		return -1;

	    return tags[tag];
	}

	public string GetTagName (int tag)
	{
	    // Not efficient, but this function is called incredibly rarely.

	    foreach (string s in tags.Keys)
		if (tags[s] == tag)
		    return s;

	    return null;
	}

	// Tags on targets

	int[] tag_offsets; // array of indices into tag_data
	int[] tag_data;

	void ReadTagTable (BinaryReader br)
	{
	    int n = br.ReadInt32 ();
	    tag_data = BinaryHelper.ReadRawInts (br, n);
	    tag_offsets = BinaryHelper.ReadRawInts (br, tags.Count);
	}

	public IEnumerable<TargetTagInfo> GetTargetsWithTag (int tag)
	{
	    int start, bound;
	    GetOffsetRange (tag, tag_offsets, tag_data.Length, out start, out bound);

	    if (((bound - start) & 0x1) != 0)
		throw ExHelp.App ("Target tag data segment length {0} is odd!", bound - start);

	    for (int idx = start; idx < bound; idx += 2) {
		TargetTagInfo tti;
		int valcode = tag_data[idx + 1];

		if ((valcode & 0x7FFF0000) == 0x7FFF0000)
		    tti = new TargetTagInfo (tag, tag_data[idx], ResultFromCode (valcode));
		else
		    tti = new TargetTagInfo (tag, tag_data[idx], valcode);
		
		yield return tti;
	    }
	}

	public object GetTargetTag (int tid, int tag)
	{
	    // The usual drill: Inefficient but infrequently called.

	    foreach (TargetTagInfo tti in GetTargetsWithTag (tag)) {
		if (tti.Target != tid)
		    continue;

		if (tti.ValResult != null)
		    return tti.ValResult;
		else
		    return tti.ValTarget;
	    }

	    return null;
	}

	// Providers

	short nprov;

	struct PData {
	    public short ntarg;
	    public string basis; // FIXME: lotta redundancy here, maybe worth compressing?
	    public string declloc;

	    // stuff for targets

	    public int dep_chunk_len;
	    public int[] dep_offsets;

	    public int name_chunk_len;
	    public int[] name_offsets;

	    public int[] rule_indices; // indices into array of rule Types
	}

	PData[] providers;

	void ReadProviders (BinaryReader br)
	{
	    int n = br.ReadInt32 ();

	    if (n < 0 ||  n > 0x7FFE)
		throw ExHelp.App ("Graph has too many ({0}) providers (pos {1})",
				  n, br.BaseStream.Position);

	    nprov = (short) n;
	    providers = new PData[n];

	    for (int i = 0; i < nprov; i++) {
		int ntarg = br.ReadInt32 ();

		if (ntarg < 0 || ntarg > 0xFFFF)
		    throw ExHelp.App ("Provider {0} has too many targets ({1}) (pos {2})}",
				      i, ntarg, br.BaseStream.Position);

		providers[i].ntarg = (short) ntarg;
		providers[i].basis = br.ReadString ();
		providers[i].declloc = br.ReadString ();
		providers[i].dep_chunk_len = br.ReadInt32 ();
		providers[i].dep_offsets = BinaryHelper.ReadRawInts (br, ntarg);
		providers[i].name_chunk_len = br.ReadInt32 ();
		providers[i].name_offsets = BinaryHelper.ReadRawInts (br, ntarg);
		providers[i].rule_indices = BinaryHelper.ReadRawInts (br, ntarg);
	    }
	}

	public string GetProviderBasis (short id)
	{
	    if (id < 0 || id >= nprov)
		throw new ArgumentOutOfRangeException ();

	    return providers[id].basis;
	}

	public string GetProviderDeclarationLoc (short id)
	{
	    if (id < 0 || id >= nprov)
		throw new ArgumentOutOfRangeException ();

	    return providers[id].declloc;
	}

	public short GetProviderId (string basis)
	{
	    if (basis == null || basis.Length < 1)
		throw new ArgumentException ();

	    for (short i = 0; i < nprov; i++) {
		if (providers[i].basis == basis)
		    return i;
	    }

	    return -1;
	}

	public short NumProviders { 
	    get { return nprov; }
	}

	public int GetProviderTargetBound (short id)
	{
	    if (id < 0)
		throw new ArgumentOutOfRangeException ();

	    return (int) (((uint) id << 16) + ((uint) providers[id].ntarg));
	}

	// Targets

	byte[] target_name_data;
	Encoding enc = Encoding.UTF8;

	public string GetTargetName (int tid)
	{
	    short pid = (short) ((((uint) tid) >> 16) & 0xFFFF);
	    int start, bound;
	    GetOffsetRange (tid & 0xFFFF, providers[pid].name_offsets, 
			    providers[pid].name_chunk_len, out start, out bound);

	    return enc.GetString (target_name_data, start, bound - start);
	}

	public int GetTargetId (string target)
	{
	    // This one always kind of sucks.

	    int idx = target.LastIndexOf ('/');
	    string basis = target.Substring (0, idx + 1);
	    string basename = target.Substring (idx + 1);

	    short pid = GetProviderId (basis);
	    if (pid < 0)
		throw ExHelp.Argument ("target", "No such target `{0}'", target);

	    int bound = GetProviderTargetBound (pid);

	    for (int i = ((int) pid) << 16; i < bound; i++) {
		if (GetTargetName (i) == basename)
		    return i;
	    }

	    throw ExHelp.Argument ("target", "No such target `{0}'", target);
	}

	Type[] type_table;

	public Type GetTargetRuleType (int tid)
	{
	    if (tid < 0)
		throw new ArgumentOutOfRangeException ();

	    short pid = (short) ((((uint) tid) >> 16) & 0xFFFF);

	    return type_table[providers[pid].rule_indices[tid & 0xFFFF]];
	}

	void ReadTypeTable (BinaryReader br)
	{
	    type_table = (Type[]) BinaryHelper.ReadObject (br);
	}

	int[] dep_data;

	void ReadTargetTables (BinaryReader br)
	{
	    int dep_len = br.ReadInt32 ();
	    dep_data = BinaryHelper.ReadRawInts (br, dep_len);

	    BinaryHelper.ExpectDelimiter (br);

	    int name_len = br.ReadInt32 ();
	    target_name_data = br.ReadBytes (name_len);
	}

	public bool ApplyTargetDependencies (int tid, ArgCollector ac, IWarningLogger logger)
	{
	    short pid = (short) ((((uint) tid) >> 16) & 0xFFFF);
	    int start, bound;
	    GetOffsetRange (tid & 0xFFFF, providers[pid].dep_offsets, 
			    providers[pid].dep_chunk_len, out start, out bound);

	    int cur_arg = -3;
	    bool res = true;

	    ac.AddTargetName (GetTargetName (tid));

	    // the dep_data chunk is encoded as a sequence of:
	    // [arg id] [default] [dep 0] [dep 1] ... [dep n]
	    //
	    // arg id's are negative. -1 means 'unnamed dep',
	    // -2 means 'assigned to arg 0', -3 means 'assigned
	    // to arg 1', etc.
	    //
	    // Deps are standard targets-or-results. If the high
	    // bits (provider ID) are 0x7FFF, the low bits are 
	    // treated as an index into the result table. Otherwise,
	    // the entire Int32 is treated as a target ID number.
	    //
	    // Defaults are encoded like deps; if negative, there is
	    // no default for the given arg.
	    //
	    // Since deps are always positive, we know that we've found
	    // the next arg id when we find a negative entry.

	    for (int i = start; i < bound; i++) {
		int v = dep_data[i];

		if (v < 0) {
		    cur_arg = (-v) - 3;

		    // Now read the default.
		    v = dep_data[++i];

		    if (v > 0 && cur_arg >= 0) {
			if ((v & 0x7FFF0000) == 0x7FFF0000) 
			    res = ac.SetDefault (cur_arg, ResultFromCode (v), logger);
			else
			    res = ac.SetDefault (cur_arg, v, logger);

			if (res)
			    return true;
		    }

		    continue;
		}

		bool is_res = (v & 0x7FFF0000) == 0x7FFF0000;

		if (cur_arg == -2) {
		    if (is_res)
			res = ac.Add (ResultFromCode (v), logger);
		    else {
			ac.Add (v);
			res = false;
		    }
		} else if (cur_arg == -1) {
		    if (is_res)
			res = ac.AddDefaultOrdered (ResultFromCode (v), logger);
		    else
			res = ac.AddDefaultOrdered (v, logger);
		} else {
		    if (is_res)
			res = ac.Add (cur_arg, ResultFromCode (v), logger);
		    else
			res = ac.Add (cur_arg, v, logger);
		}

		if (res)
		    return true;
	    }

	    return false;
	}

	// Result table

	Result[] result_table;

	Result ResultFromCode (int code)
	{
	    if ((code & 0x7FFF0000) != 0x7FFF0000)
		throw ExHelp.Argument ("code", "Result code {0:x} must have PID 0x7FFF", code);

	    return result_table[code & 0x0000FFFF];
	}

	void ReadResultTable (BinaryReader br)
	{
	    result_table = (Result[]) BinaryHelper.ReadObject (br);
	}
    }
}
