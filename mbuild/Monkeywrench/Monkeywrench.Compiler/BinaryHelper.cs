using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Mono.Build;

namespace Monkeywrench.Compiler {

    public class BinaryHelper {

	BinaryHelper () {}

	public static bool DebugBinary = false;

	static void Debug (string fmt, params object[] args)
	{
	    if (!DebugBinary)
		return;

	    Console.Error.WriteLine ("BH: " + fmt, args);
	}

	public static T[] Unwrap<T> (IEnumerable<T> input)
	{
	    List<T> list = new List<T> (input);
	    T[] array = new T[list.Count];
	    list.CopyTo (array);
	    return array;
	}

	public static int ReadRawInt (BinaryReader br)
	{
	    byte[] b = br.ReadBytes (4);

	    int val = BitConverter.ToInt32 (b, 0);

	    Debug ("read one raw int: {0}", val);

	    return val;
	}

	public static int[] ReadRawInts (BinaryReader br, int count)
	{
	    if (count < 0)
		throw ExHelp.Range ("Raw int count: {0}", count);

	    int[] res = new int[count];
	    byte[] b = br.ReadBytes (count * 4);

	    for (int i = 0; i < count; i++)
		res[i] = BitConverter.ToInt32 (b, i * 4);

	    Debug ("read {0} raw ints", count);

	    return res;
	}

	public static void WriteRaw (BinaryWriter bw, int val)
	{
	    Debug ("write one raw int: {0}", val);
	    bw.Write (BitConverter.GetBytes (val));
	}

	public static void WriteRaw (BinaryWriter bw, int[] val)
	{
	    Debug ("write {0} raw ints", val.Length);

	    for (int i = 0; i < val.Length; i++)
		bw.Write (BitConverter.GetBytes (val[i]));
	}

	public static void WriteRaw (BinaryWriter bw, IEnumerable<int> val)
	{
	    Debug ("write enumerable as raw ints");

	    foreach (int v in val)
		bw.Write (BitConverter.GetBytes (v));
	}

	// Object serialization

	public static object ReadObject (BinaryReader br)
	{
	    Debug ("read binfmt object");

	    StreamingContext ctxt = new StreamingContext (StreamingContextStates.All);
	    BinaryFormatter fmt = new BinaryFormatter (null, ctxt);

	    return fmt.Deserialize (br.BaseStream);
	}

	public static void WriteObject (BinaryWriter bw, object o)
	{
	    Debug ("write binfmt object {0}", o);

	    StreamingContext ctxt = new StreamingContext (StreamingContextStates.All);
	    BinaryFormatter fmt = new BinaryFormatter (null, ctxt);

	    fmt.Serialize (bw.BaseStream, o);
	}

	// Delimiter

	const int DelimiterVal = 0x71717171;
	static int ndelim_wr = 0;
	static int ndelim_rd = 0;

	public static void WriteDelimiter (BinaryWriter bw)
	{
	    Debug ("write delimiter #{0}", ndelim_wr++);
	    bw.Write (DelimiterVal);
	}

	public static void ExpectDelimiter (BinaryReader br)
	{
	    Debug ("expect delimiter #{0}", ndelim_rd++);

	    int val = br.ReadInt32 ();

	    if (val == DelimiterVal)
		return;

	    throw ExHelp.App ("Expected a delimiter at position {0} but got (int) {1}",
			      br.BaseStream.Position, val);
	}
    }
}
