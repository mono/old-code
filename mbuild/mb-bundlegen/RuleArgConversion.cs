namespace MBBundleGen {

    public enum RuleArgConversion { 
	// The type of the argument field is a Result type
	None,

	// The type of the argument field is a reference type
	// that can be mapped to a Result (eg, string -> MBString)
	ToRefType,

	// As above, value type: bool -> MBBool.
	ToValueType
    }
}
	
