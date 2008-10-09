//
// Package.cs: The basics to support packaging with Visual Studio.
//
// Currently disabled.

#if false
using System;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;

using MsVsShell = Microsoft.VisualStudio.Shell;

// 
// Currently commented out as it seems like it needs a hotfix:
// https://connect.microsoft.com/VisualStudio/Downloads/DownloadDetails.aspx?DownloadID=10671
//
// The error is that deriving from MsVsShell.Package here produces
// the error "assembly ...Shell.Interop can 
//
// a KB article is here:
// http://support.microsoft.com/kb/946308

namespace Mono.VisualStudio.Debugger
{

    // 
    // The number 150 is the resource ID for the PKL key
    //
    [MsVsShell.ProvideLoadKey ("standard", "9.0", "Mono Integration.", "Novell", 150)]
    [MsVsShell.DefaultRegistryRoot(@"Software\Microsoft\VisualStudio\9.0")]
	[MsVsShell.PackageRegistration(UseManagedResourcesOnly = true)]
    
    
    [Guid ("4900AC1D-ED3A-40dc-8DD0-E72034AA9C54")]
    public class DebuggerPackage : MsVsShell.Package
    {
        public DebuggerPackage ()
        {
            Console.WriteLine ("Here");
        }
    }

}
#endif