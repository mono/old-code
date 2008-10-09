using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MonoAddin
{
	public partial class SettingsDialog : Form
	{
		Settings s;
		public SettingsDialog ()
		{
			InitializeComponent ();
		}

		private void SettingsDialog_Load (object sender, EventArgs e)
		{
			s = Settings.Instance ();
			s.Load ();
			server_url.Text = s.ServerURL;
			windows_path.Text = s.WindowsPath;
			linux_path.Text = s.LinuxPath;
		}

		private void SettingsDialog_FormClosing (object sender, FormClosingEventArgs e)
		{
			s.ServerURL = server_url.Text;
			s.WindowsPath = windows_path.Text;
			s.LinuxPath = linux_path.Text;
			if (this.DialogResult == DialogResult.OK)
			try {
				s.Save ();
			}
			catch (Exception ex) {
				Console.WriteLine ("Failed to save settings");
				e.Cancel = true;
			}
		}

		private void label1_Click (object sender, EventArgs e)
		{

		}

		private void label3_Click (object sender, EventArgs e)
		{

		}

		private void textBox2_TextChanged (object sender, EventArgs e)
		{

		}

		private void textBox1_TextChanged (object sender, EventArgs e)
		{

		}
	}
}
