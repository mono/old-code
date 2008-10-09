//
// Shared definitions
//

using System;

namespace Mono.VisualStudio.Debugger
{
    public class Guids
    {
        public const string EngineClass = "13EA87A8-A478-4c27-BD54-A1DD732EE56A";
        public const string ProgramProviderClass = "F3E09C35-C71A-4d52-A849-48AC6CFE40B3";

        public const string Engine = "FEEB0E6F-B2FA-4640-981C-C7BA64C7098E";

		public const string PortSupplier = "345CD6F9-4BDA-40b3-90B1-5D3B735D1AE3";
		public const string PortSupplierClass = "2EEE1F44-F1B4-4dda-B47E-89B779AF24FD";

		public const string VS_ProgramPublisher = "d04d550d-1ea8-4e37-830e-700fea447688";

		// Language guid for C++. Used when the language for a document context or a stack frame is requested.
		static private Guid _guidLanguageCpp = new Guid("3a12d0b7-c26c-11d0-b442-00a0244a1dd2");
		static public Guid guidLanguageCpp
		{
			get { return _guidLanguageCpp; }
		}
    }
}
