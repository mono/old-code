//
// This is a temporary hack while I figured out how is it that a program
// is supposed to actually register properly with VisualStudio.
//
// There are various assorted ways, but nothing comprehensive:
//   * DebugEngineSample depends on C++ auto-registration to install
//     a RGS definition into the registry
//
//   * VSPackages use some attributes, and our Mono.VisualStudio.Debugger
//     should probably become one of these (but currently fails to build
//     without the hotfix, notice that doing a VSPackage using the wizard
//     seems to work, although that one has not been configured as "expose
//     to COM", so it might not even work).
// 
//     VSPackages only contain some of the attributes necessary, they do not
//     seem to support Debug Engine registration.   The tool RegPkg is used
//     for this, but its use is discouraged on every documentation page.
//
//   * RgsReg is an MS tool that is available for download, but it is another
//     dependency that must be downloaded and used.
//
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using Mono.VisualStudio.Debugger;
using System.Reflection;

namespace RegisterDebugger
{
    class RegisterDebugger
    {
        static string Key (string plain_guid)
        {
            return "{" + plain_guid + "}";
        }

        static int Main (string[] args)
        {
            
            string basedir = (args.Length == 0) ? "c:\\Work\\MonoVS\\Mono.VisualStudio.Debugger\\bin\\Debug" : args [0];

            Console.WriteLine ("RegisterDebugger, base directory: {0}", basedir);
            RegistryKey root = Registry.LocalMachine;
			RegistryKey vs = root.OpenSubKey("Software", false);
 
			// handle fact that VS is 32-bit on 64-bit OS
			if (IntPtr.Size == 8)
				vs = vs.OpenSubKey("Wow6432Node");

			vs = vs.OpenSubKey("Microsoft", false)
				.OpenSubKey("VisualStudio", false);

            RegistryKey vsver = vs.OpenSubKey ("9.0");
            RegistryKey metrics_engine = vsver.OpenSubKey ("AD7Metrics", false).OpenSubKey ("Engine", true);
			RegistryKey port_supplier = vsver.OpenSubKey ("AD7Metrics", false).OpenSubKey ("PortSupplier", true);
            //
            // HKLM\Software\Microsoft\VisualStudio\9.0\AD7Metrics\Engine\
            //
            // Cleaup old installations
            try {
                metrics_engine.DeleteSubKeyTree (Key (Guids.Engine));
            } catch { }

            // Recreate
            RegistryKey mono_engine = metrics_engine.CreateSubKey (Key (Guids.Engine));

            // 
			// The values here are the "Debug Engine Properites" documented in "Debugger SDK Helpers"
			//
            mono_engine.SetValue ("CLSID", Key (Guids.EngineClass));
            mono_engine.SetValue ("Name", "Mono.VisualStudio.Debugger");
            mono_engine.SetValue ("ProgramProvider", Key (Guids.ProgramProviderClass));

			// This is the default PortSupplier:
			//mono_engine.SetValue ("PortSupplier", "{708C1ECA-FF48-11D2-904F-00C04FA302A1}");
			mono_engine.SetValue ("PortSupplier", Key (Guids.PortSupplierClass));

            // I have no idea why all these do, and Google did not find much,
            // this is all from the DebugEngineSample:
            
            mono_engine.SetValue ("CallstackBP", 1);
            mono_engine.SetValue ("AutoselectPriority", 4);
            mono_engine.SetValue ("Attach", 1);     // Supports attaching
            mono_engine.SetValue ("AddressBP", 1);  // Supports address-based breakpoints
			mono_engine.SetValue ("RemotingDebugging", 1);

			//
			// HKLM\Software\Microsoft\VisualStudio\9.0\AD7Metrics\PortSupplier
			//
			try {
				port_supplier.DeleteSubKeyTree (Key (Guids.PortSupplier));
			} catch { }

			RegistryKey mono_ports = port_supplier.CreateSubKey (Key (Guids.PortSupplier));
			mono_ports.SetValue ("CLSID", Key (Guids.PortSupplierClass));
			mono_ports.SetValue ("Name", "Mono Remote Cross Platform Debugging");

            //
            // HKLM\Software\Microsoft\VisualStudio\9.0\CLSID
            //
            RegistryKey vsclsid = vsver.OpenSubKey ("CLSID", true);
            try {
                vsclsid.DeleteSubKeyTree (Key (Guids.ProgramProviderClass));
            } catch { }

            try {
                vsclsid.DeleteSubKeyTree (Key (Guids.EngineClass));
            } catch { }

			string sysdir = Environment.SystemDirectory;
			// handle fact that VS is 32-bit on 64-bit OS
			if (IntPtr.Size == 8)
				sysdir = sysdir.Replace("system32", "SysWOW64");
            string dbgassembly = Path.Combine (basedir, "Mono.VisualStudio.Debugger.dll");
            string mscoree = Path.Combine (sysdir, "mscoree.dll");

            RegistryKey ppc = vsclsid.CreateSubKey (Key (Guids.ProgramProviderClass));
            ppc.SetValue ("Assembly", "Mono.VisualStudio.Debugger");
            ppc.SetValue ("Class", "Mono.VisualStudio.Debugger.ProgramProvider");
            ppc.SetValue ("CodeBase", dbgassembly);
            ppc.SetValue ("InProcServer32", mscoree);

            RegistryKey ec = vsclsid.CreateSubKey (Key (Guids.EngineClass));
            ec.SetValue ("Assembly", "Mono.VisualStudio.Debugger");
            ec.SetValue ("Class", "Mono.VisualStudio.Debugger.Engine");
            ec.SetValue ("Codebase", dbgassembly);
            ec.SetValue ("InProcServer32", mscoree);

			RegistryKey psc = vsclsid.CreateSubKey (Key (Guids.PortSupplierClass));
			psc.SetValue ("Assembly", "Mono.VisualStudio.Debugger");
			psc.SetValue ("Class", "Mono.VisualStudio.Debugger.PortSupplier");
			psc.SetValue ("Codebase", dbgassembly);
			psc.SetValue ("InProcServer32", mscoree);

			// sigh, on 64-bit system we need to register Debugger as 32-bit and 64-bit for COM Interop
			// as the msvsmon.exe is 64-bit
			if (IntPtr.Size == 8)
			{
				Assembly debugger_asm = Assembly.LoadFrom(dbgassembly);
				System.Runtime.InteropServices.RegistrationServices s = new System.Runtime.InteropServices.RegistrationServices();
				if (!s.RegisterAssembly(debugger_asm, System.Runtime.InteropServices.AssemblyRegistrationFlags.SetCodeBase))
				{
					Console.WriteLine("Failed to register {0} for 64-bit COM Interop.", dbgassembly);
					return 1;
				}
			}

			return 0;
        }
    }
}
