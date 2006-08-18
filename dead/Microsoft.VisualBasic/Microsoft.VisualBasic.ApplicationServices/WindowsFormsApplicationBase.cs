//
// Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase.cs
//
// Authors:
//   Miguel de Icaza (miguel@novell.com)
//
// Copyright (C) 2006 Novell (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0
using System;
using System.Threading;
using System.Windows.Forms;

namespace Microsoft.VisualBasic.ApplicationServices {

	public class WindowsFormsApplicationBase : ConsoleApplicationBase {
		public WindowsFormsApplicationBase ()
		{
		}

		public WindowsFormsApplicationBase (AuthenticationMode mode)
		{
		}

		protected static bool UseCompatibleTextRendering {
			get {
				return false;
			}
		}

		[MonoTODO("We ignore the commandLine argument")]
		public void Run (string [] commandLine)
		{
			throw new Exception ("Visual Basic 2005 applications are not supported");
			Application.Run ();
		}

		bool is_single_instance = false;
		protected bool IsSingleInstance {
			get {
				return is_single_instance;
			}

			set {
				is_single_instance = value;
			}
		}

		bool enable_visual_styles = false;
		protected bool EnableVisualStyles {
			get {
				return enable_visual_styles;
			}

			set {
				enable_visual_styles = value;
			}
		}

		bool save_my_settings_on_exit = false;
		protected bool SaveMySettingsOnExit {
			get {
				return save_my_settings_on_exit;
			}

			set {
				save_my_settings_on_exit = value;
			}
		}

		ShutdownMode shutdown_style;
		protected ShutdownMode ShutdownStyle {
			get {
				return shutdown_style;
			}

			set {
				shutdown_style = value;
			}
		}
	}
}

#endif
