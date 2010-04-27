using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;


namespace PHP.Compiler {


	public class PEmitter {

		public const int EXECUTABLE = 0;
		public const int LIBRARY = 1;

		public static FileInfo OutputFile = null;
		public static int Target = EXECUTABLE;
		public static string ModuleName;
		public static AssemblyName AsmNam;
		public static AssemblyBuilder AsmBld;
		public static ModuleBuilder ModBld;

		public static void BeginModule() {
			ModuleName = OutputFile.Name.Substring(0, OutputFile.Name.Length - 4);
			AsmNam = new AssemblyName();
			AsmNam.Name = ModuleName;
			AsmNam.Version = new Version(0, 1, 0, 0);
			AsmBld = AppDomain.CurrentDomain.DefineDynamicAssembly(AsmNam, AssemblyBuilderAccess.Save, OutputFile.DirectoryName);
			ModBld = AsmBld.DefineDynamicModule(ModuleName, OutputFile.Name);
		}

		public static void EndModule() {
			//Console.WriteLine("Saving to file \"" + fileName + "\"...");
			try {
				AsmBld.Save(OutputFile.Name);
			}
			catch (Exception e) {
				Report.Error(002, e.Message);
			}
		}
	}


}