// Relaxed ng validator 
//    using Atsushi Enomoto's relax ng validating XmlReader
//
// author: Per Arneng <pt99par@student.bth.se> 
//
using System;
using System.IO;
using System.Xml;
using System.Text;
using Commons.Xml.Relaxng;

namespace ValDoc {

    public class  MainClass {
	
	protected static string schemaFile = "";
	protected static string xmlFile = "";

	public static void Main( string[] args ) {
	    try { 
		
		if( args.Length != 1 ) {
		    Console.Error.WriteLine("valdoc: wrong nr of arguments");
		    Console.Error.WriteLine("valdoc: usage: valdoc <xml file>");
		    Environment.Exit(1);
		}
	    
		Stream schemaStream = new MemoryStream( Encoding.UTF8.GetBytes( RelaxNgSchema.SCHEMA ) ); 
		xmlFile = args[0];

		XmlReader schema = new XmlTextReader( schemaStream );
		XmlReader document = new XmlTextReader( xmlFile );
	    
		XmlDocument doc = new XmlDocument ();
		XmlReader rvr = new RelaxngValidatingReader ( document, schema);
	    
		while( rvr.Read() );
		Console.WriteLine("valdoc: document {0} is VALID.", new FileInfo(xmlFile).Name );
		
	    } catch ( RngException ex ) {
		Console.Error.WriteLine("valdoc: document {0} is NOT VALID.",xmlFile);
		Console.Error.WriteLine( "valdoc: {0}" , ex.Message );
		Environment.Exit(1);
	    } catch ( Exception ex ) {
		Console.Error.WriteLine("valdoc: internal error");
		Console.Error.WriteLine("valdoc: {0}", ex.Message );
		Environment.Exit(1);
	    }
	}
	
	
    }

}

