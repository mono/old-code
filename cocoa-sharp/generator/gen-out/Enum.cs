//
//  Authors
//    - Kangaroo, Geoff Norton
//    - Urs C. Muff, Quark Inc., <umuff@quark.com>
//
//  Copyright (c) 2004 Quark Inc.  All rights reserved.
//
// $Id$
//

using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.IO;

namespace CocoaSharp {
	public class Enum : Type {
		public Enum(string name, string nameSpace) 
			: base(name, nameSpace,nameSpace + "." + name,typeof(int),OCType.@int) {
		}
		public void Initialize(ICollection items) {
			this.items = items;
		}

		// -- Public Properties --
		public ICollection Items { get { return items; } }

		// -- Members --
		private ICollection items;

		// -- Methods --
		public override void WriteCS(TextWriter _cs, Configuration config) {
			_cs.WriteLine("using System;");
			_cs.WriteLine("namespace {0} {{",Namespace);
			_cs.WriteLine("    public enum {0} {{",Name.Replace("enum ", string.Empty));
			foreach (EnumItem item in this.items)
				if (item.Value == null || item.Value.Length == 0) {
					if (item.Name != "")
						_cs.WriteLine("        {0},",item.Name);
				} else {
					if (item.Name != "" && item.Value != "")
						_cs.WriteLine("        {0} = {1},",item.Name,item.Value);
				}
			ProcessAddin(_cs, config);
			_cs.WriteLine("    }");
			_cs.WriteLine("}");
		}

		protected override bool IsEmpty() { return items.Count == 0; }

		static private string IfsBeGone(string original) {
			Regex ifRegex = new Regex(@"^#.+$", RegexOptions.Multiline);
			if(ifRegex.IsMatch(original)) 
				foreach(Match m in ifRegex.Matches(original))
					original = original.Replace(m.Value, "");
			return original;
		}
	}

	public class EnumItem {
		public EnumItem(string name, string value) { this.name = name; this.value = value; }

		// -- Public Properties --
		public string Name { get { return name; } }
		public string Value { get { return value; } }

		// -- Members --
		private string name;
		private string value;
	}
}

//
// $Log: Enum.cs,v $
// Revision 1.4  2004/09/20 20:18:23  gnorton
// More refactoring; Foundation almost gens properly now.
//
// Revision 1.3  2004/09/11 00:41:22  urs
// Move Output to gen-out
//
// Revision 1.2  2004/09/09 03:32:22  urs
// Convert methods from mach-o to out format
//
// Revision 1.1  2004/09/09 01:16:03  urs
// 1st draft of out module of 2nd generation generator
//
//
