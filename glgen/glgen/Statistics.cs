// Statistics.cs : Generation statistics class implementation
//
// Author: Mike Kestner  <mkestner@speakeasy.net>
//
// <c> 2002 Mike Kestner

namespace GtkSharp.Generation {
	
	using System;
	using System.Collections;
	
	public class Statistics {
		
		static int consts = 0;
		static int types = 0;
		static int funcs = 0;
		static int ignore = 0;
		
		public static int ConstsCount {
			get {
				return consts;
			}
			set {
				consts = value;
			}
		}

		public static int TypesCount {
			get {
				return types;
			}
			set {
				types = value;
			}
		}

		public static int FuncsCount {
			get {
				return funcs;
			}
			set {
				funcs = value;
			}
		}

		public static int IgnoreCount {
			get {
				return ignore;
			}
			set {
				ignore = value;
			}
		}

		public static void Report()
		{
			Console.WriteLine("Generation Summary:");
			Console.WriteLine("\tTypes: " + types);
			Console.WriteLine("\tConsts: " + consts);
			Console.WriteLine("\tFuncs: " + funcs);
			Console.WriteLine("\tIgnored: " + ignore);
			Console.WriteLine("Total Nodes: " + 
					 (funcs+types+consts+ignore));
		}
	}
}
