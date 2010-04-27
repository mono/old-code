//
// Microsoft.JScript JScriptCodeProvider Class implementation
//
// Author:
//   Jeroen Janssen	<japj@xs4all.nl>
//
// (C) 2002 Ximian, Inc.
//

namespace Microsoft.JScript
{
	using System;
	using System.CodeDom.Compiler;
	using System.ComponentModel;

	public class JScriptCodeProvider
		: CodeDomProvider
	{
		//
		// Constructors
		//
		public JScriptCodeProvider()
		{
		}

		//
		// Properties
		//
		public override string FileExtension {
			get {
				return "js";
			}
		}

		//
		// Methods
		//
		[MonoTODO]
		public override ICodeCompiler CreateCompiler()
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public override ICodeGenerator CreateGenerator()
		{
			throw new NotImplementedException();
		}
		
	}
}
