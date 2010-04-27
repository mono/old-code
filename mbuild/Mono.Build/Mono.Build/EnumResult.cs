using System;
using System.Xml;

using Mono.Build;

namespace Mono.Build {
    
    [Serializable]
	public class EnumResult<T> : Result where T : struct, IConvertible {

	// FIXME: disallow flags enums, or handle them.

	protected EnumResult ()
	{
	    if (!(typeof (T).IsSubclassOf (typeof (Enum))))
		throw new Exception ("EnumResult must wrap an enum");
	}

	public T Value;

	string ValueToString (T e)
	{
	    Array values = Enum.GetValues (typeof (T));

	    // See comment above. The int cast seems to be the only
	    // good way to get the value comparison to work.

	    for (int i = 0; i < values.Length; i++) {
		if (e.ToInt32 (null) == (int) values.GetValue (i))
		    return Enum.GetNames (typeof (T))[i];
	    }

	    throw new ArgumentException ("Illegal enumeration value \"" + e.ToString () + "\"");
	}

	public string ValueString
	{
	    get { return ValueToString (Value); }
	    
	    set { Value = (T) Enum.Parse (typeof (T), value); }
	}
	
	// object overrides
	
	protected override bool ContentEquals (Result other)
	{
	    EnumResult<T> er = (EnumResult<T>) other;
	    
	    return er.Value.ToInt32 (null) == Value.ToInt32 (null);
	}
	
	protected override int InternalHash ()
	{
	    return Value.GetHashCode ();
	}
	
	// XML
	
	protected override void ExportXml (XmlWriter xw) 
	{
	    xw.WriteStartElement ("enum");
	    xw.WriteString (ValueString);
	    xw.WriteEndElement ();
	}
	
	protected override bool ImportXml (XmlReader xr, IWarningLogger log)
	{
	    int depth = xr.Depth;
	    
	    while (xr.Depth >= depth) {
		if (xr.NodeType != XmlNodeType.Element) {
		    xr.Read ();
		    continue;
		}
		
		if (xr.Name != "enum") {
		    log.Warning (3019, "Unknown element in EnumResult during XML import", 
				 xr.Name);
		    xr.Skip ();
		    break;
		}
		
		string s = xr.ReadElementString ();
		
		try {
		    ValueString = s;
		} catch (Exception e) {
		    log.Error (3019, "Error converting input string to enumeration value", 
			       e.Message);
		    return true;
		}
	    }
	    
	    return false;
	}
	
	public override Fingerprint GetFingerprint (IBuildContext ctxt, Fingerprint cached)
	{
	    return GenericFingerprints.Constant (BitConverter.GetBytes (Value.ToInt32 (null)));
	}
	
	protected override void CloneTo (Result dest)
	{
	    EnumResult<T> er = (EnumResult<T>) dest;
	    
	    er.Value = this.Value;
	}
	
	public override string ToString ()
	{
	    return ValueString;
	}
    }
}

