namespace MBBundleGen {

    public enum ArgCount { 
	// The argument has a single value and must be specified
	Standard, 

	// The argument has a single value and may be left unset
	Optional,

	// The argument has multiple values and must be specified
	OneOrMore,

	// The argument has multiple values and may be left unset
	ZeroOrMore
    }
}
	
