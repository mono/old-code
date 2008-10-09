using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Windows.Forms;
using Mono.Debugger.Utilities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;

using Mono.VisualStudio.Debugger;


namespace MonoAddin
{
	/// <summary>The object for implementing an Add-in.</summary>
	/// <seealso class='IDTExtensibility2' />
	public class Connect : IDTExtensibility2, IDTCommandTarget
	{
		/// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
		public Connect ()
		{
		}

		/// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
		/// <param term='application'>Root object of the host application.</param>
		/// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
		/// <param term='addInInst'>Object representing this Add-in.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection (object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{
			_applicationObject = (DTE2) application;
			_addInInstance = (AddIn) addInInst;
			if (connectMode == ext_ConnectMode.ext_cm_UISetup) {
				object[] contextGUIDS = new object[] { };
				Commands2 commands = (Commands2) _applicationObject.Commands;
				string toolsMenuName;

				try {
					//If you would like to move the command to a different menu, change the word "Tools" to the 
					//  English version of the menu. This code will take the culture, append on the name of the menu
					//  then add the command to that menu. You can find a list of all the top-level menus in the file
					//  CommandBar.resx.
					string resourceName;
					ResourceManager resourceManager = new ResourceManager ("MonoAddin.CommandBar", Assembly.GetExecutingAssembly ());
					CultureInfo cultureInfo = new CultureInfo (_applicationObject.LocaleID);

					if (cultureInfo.TwoLetterISOLanguageName == "zh") {
						System.Globalization.CultureInfo parentCultureInfo = cultureInfo.Parent;
						resourceName = String.Concat (parentCultureInfo.Name, "Tools");
					} else {
						resourceName = String.Concat (cultureInfo.TwoLetterISOLanguageName, "Tools");
					}
					toolsMenuName = resourceManager.GetString (resourceName);
				} catch {
					//We tried to find a localized version of the word Tools, but one was not found.
					//  Default to the en-US word, which may work for the current culture.
					toolsMenuName = "Tools";
				}

				//Place the command on the tools menu.
				//Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
				Microsoft.VisualStudio.CommandBars.CommandBar menuBarCommandBar = ((Microsoft.VisualStudio.CommandBars.CommandBars) _applicationObject.CommandBars)["MenuBar"];

				//Find the Tools command bar on the MenuBar command bar:
				CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
				CommandBarPopup toolsPopup = (CommandBarPopup) toolsControl;

				//This try/catch block can be duplicated if you wish to add multiple commands to be handled by your Add-in,
				//  just make sure you also update the QueryStatus/Exec method to include the new command names.
				try {
					//Add a command to the Commands collection:
					Command command = commands.AddNamedCommand2 (_addInInstance, "MonoAddin", "Mono: Debug Program", "Executes the command for MonoAddin", true, 59, ref contextGUIDS, (int) vsCommandStatus.vsCommandStatusSupported + (int) vsCommandStatus.vsCommandStatusEnabled, (int) vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);

					//Add a control for the command to the tools menu:
					if ((command != null) && (toolsPopup != null)) {
						command.AddControl (toolsPopup.CommandBar, 1);
					}

					//Add a command to the Commands collection:
					Command settings_command = commands.AddNamedCommand2 (_addInInstance, "MonoAddinSettings", "Mono: Debugging Settings",
						"Setings for MonoAddin", true, 59, ref contextGUIDS, (int) vsCommandStatus.vsCommandStatusSupported + (int) vsCommandStatus.vsCommandStatusEnabled, (int) vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);

					//Add a control for the command to the tools menu:
					if ((settings_command != null) && (toolsPopup != null)) {
						settings_command.AddControl (toolsPopup.CommandBar, 1);
					}

				} catch (System.ArgumentException) {
					//If we are here, then the exception is probably because a command with that name
					//  already exists. If so there is no need to recreate the command and we can 
					//  safely ignore the exception.
				}
			}
		}

		/// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
		/// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection (ext_DisconnectMode disconnectMode, ref Array custom)
		{
		}

		/// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />		
		public void OnAddInsUpdate (ref Array custom)
		{
		}

		/// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete (ref Array custom)
		{
		}

		/// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown (ref Array custom)
		{
		}

		/// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
		/// <param term='commandName'>The name of the command to determine state for.</param>
		/// <param term='neededText'>Text that is needed for the command.</param>
		/// <param term='status'>The state of the command in the user interface.</param>
		/// <param term='commandText'>Text requested by the neededText parameter.</param>
		/// <seealso class='Exec' />
		public void QueryStatus (string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
		{
			if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone) {
				switch (commandName) {
					case "MonoAddin.Connect.MonoAddin":
						status = (vsCommandStatus) vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
						break;
					case "MonoAddin.Connect.MonoAddinSettings":
						status = (vsCommandStatus) vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
						break;
					default:
						break;
				}
			}
		}

		/// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
		/// <param term='commandName'>The name of the command to execute.</param>
		/// <param term='executeOption'>Describes how the command should be run.</param>
		/// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
		/// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
		/// <param term='handled'>Informs the caller if the command was handled or not.</param>
		/// <seealso class='Exec' />
		public void Exec (string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
		{
			handled = false;
			if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault) {
				switch (commandName) {
					case "MonoAddin.Connect.MonoAddin":
						LaunchMono ();
						handled = true;
						break;
					case "MonoAddin.Connect.MonoAddinSettings":
						SettingsDialog dialog = new SettingsDialog ();
						dialog.ShowDialog ();
						handled = true;
						break;
					default:
						break;
				}
			}
		}
		private DTE2 _applicationObject;
		private AddIn _addInInstance;

		//
		// Generates debugging information, picks the executable to launch
		//
		void LaunchMono ()
		{
			Project prj = _applicationObject.Solution.Projects.Item (1);
			Configuration config = prj.ConfigurationManager.ActiveConfiguration;
			OutputGroup built_group = null;
			string msg = "";
			string executable = null;

			foreach (OutputGroup og in config.OutputGroups) {
				string name = og.CanonicalName;

				if (name == "Built")
					built_group = og;
			}

			if (built_group == null) {
				MessageBox.Show ("No built executables for this solution", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			try {
				pdb2mdb pdb = new pdb2mdb ();
				foreach (string file in (Array) built_group.FileURLs) {
					Uri fu = new Uri (file);

					pdb.CreateFromPDB (fu.LocalPath);

					if (fu.AbsolutePath.EndsWith (".exe"))
						executable = fu.LocalPath;
				}
			} catch (BadImageFormatException e) {
				MessageBox.Show (String.Format ("Not a .NET executable or library: {0}", e.FileName),
				    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (executable == null) {
				MessageBox.Show ("Did not find an executable in the collection to run", "Error",
				    MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			LaunchExecutable (executable);

#if false || cute_debugging_exploring
            foreach (OutputGroup og in config.OutputGroups) {
                msg += og.CanonicalName + "<br>";
                msg += og.Description + "<br>";
                msg += "count: " + og.FileCount + "<br>";
                foreach (string x in (Array)og.FileURLs) {
                    msg += "<li>" + x;
                }

                msg += "<br><hr/><br>";
                foreach (string x in (Array)og.FileNames) {
                    msg += "<li> + x";
                }
                msg += "<p>";
            }
            msg += "<hr/>done";
#endif
		}

		void LaunchExecutable (string executable)
		{
			Microsoft.VisualStudio.Shell.ServiceProvider sp =
			     new Microsoft.VisualStudio.Shell.ServiceProvider ((Microsoft.VisualStudio.OLE.Interop.IServiceProvider) _applicationObject);

			IVsDebugger dbg = (IVsDebugger) sp.GetService (typeof (SVsShellDebugger));
			IVsUIShell shell = (IVsUIShell) sp.GetService (typeof (SVsUIShell));

			VsDebugTargetInfo info = new VsDebugTargetInfo ();
			info.cbSize = (uint) System.Runtime.InteropServices.Marshal.SizeOf (info);
			info.dlo = Microsoft.VisualStudio.Shell.Interop.DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;

			info.bstrExe = executable;
			info.bstrCurDir = System.IO.Path.GetDirectoryName (info.bstrExe);
			info.bstrArg = null; // no command line parameters
			info.bstrRemoteMachine = null; // debug locally
			info.fSendStdoutToOutputWindow = 0; // Let stdout stay with the application.
			info.clsidPortSupplier = new Guid (Guids.PortSupplier);
			info.bstrPortName = "Mono";

			// The Mono Debug Engine.
			info.clsidCustom = new Guid (Guids.Engine);
			info.grfLaunch = 0;

			IntPtr pInfo = System.Runtime.InteropServices.Marshal.AllocCoTaskMem ((int) info.cbSize);
			System.Runtime.InteropServices.Marshal.StructureToPtr (info, pInfo, false);

			uint result = 0;
			try {
				//MessageBox.Show ("dingus");
				result = (uint) dbg.LaunchDebugTargets (1, pInfo);

				if (result == 0x89710016) {
					MessageBox.Show ("Unable to Launch Debugging Target, probably an incorrect registation of the COM component",
					    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

					// prevents a double message.
					result = COM.S_OK;
				}

			} finally {
				if (pInfo != IntPtr.Zero) {
					System.Runtime.InteropServices.Marshal.FreeCoTaskMem (pInfo);
				}
			}

			if (result != COM.S_OK) {
				string error;
				shell.GetErrorInfo (out error);
				MessageBox.Show (error, "Mono.Addin: Visual Studio reported error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}