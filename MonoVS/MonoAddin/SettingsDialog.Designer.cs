namespace MonoAddin
{
	partial class SettingsDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose (bool disposing)
		{
			if (disposing && (components != null)) {
				components.Dispose ();
			}
			base.Dispose (disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent ()
		{
			this.button_ok = new System.Windows.Forms.Button ();
			this.button_cancel = new System.Windows.Forms.Button ();
			this.label_server_url = new System.Windows.Forms.Label ();
			this.server_url = new System.Windows.Forms.TextBox ();
			this.label1 = new System.Windows.Forms.Label ();
			this.windows_path = new System.Windows.Forms.TextBox ();
			this.label2 = new System.Windows.Forms.Label ();
			this.label3 = new System.Windows.Forms.Label ();
			this.linux_path = new System.Windows.Forms.TextBox ();
			this.SuspendLayout ();
			// 
			// button_ok
			// 
			this.button_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_ok.Location = new System.Drawing.Point (376, 290);
			this.button_ok.Name = "button_ok";
			this.button_ok.Size = new System.Drawing.Size (75, 23);
			this.button_ok.TabIndex = 0;
			this.button_ok.Text = "OK";
			this.button_ok.UseVisualStyleBackColor = true;
			// 
			// button_cancel
			// 
			this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button_cancel.Location = new System.Drawing.Point (457, 290);
			this.button_cancel.Name = "button_cancel";
			this.button_cancel.Size = new System.Drawing.Size (75, 23);
			this.button_cancel.TabIndex = 1;
			this.button_cancel.Text = "Cancel";
			this.button_cancel.UseVisualStyleBackColor = true;
			// 
			// label_server_url
			// 
			this.label_server_url.Location = new System.Drawing.Point (12, 9);
			this.label_server_url.Name = "label_server_url";
			this.label_server_url.Size = new System.Drawing.Size (100, 23);
			this.label_server_url.TabIndex = 2;
			this.label_server_url.Text = "Server URL:";
			// 
			// server_url
			// 
			this.server_url.Location = new System.Drawing.Point (118, 6);
			this.server_url.Name = "server_url";
			this.server_url.Size = new System.Drawing.Size (333, 20);
			this.server_url.TabIndex = 3;
			this.server_url.TextChanged += new System.EventHandler (this.textBox1_TextChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point (18, 52);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size (101, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Directory Mappings:";
			this.label1.Click += new System.EventHandler (this.label1_Click);
			// 
			// windows_path
			// 
			this.windows_path.Location = new System.Drawing.Point (118, 86);
			this.windows_path.MaxLength = 200;
			this.windows_path.Name = "windows_path";
			this.windows_path.Size = new System.Drawing.Size (100, 20);
			this.windows_path.TabIndex = 5;
			this.windows_path.TextChanged += new System.EventHandler (this.textBox2_TextChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point (18, 86);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size (79, 13);
			this.label2.TabIndex = 6;
			this.label2.Text = "Windows Path:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point (18, 121);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size (60, 13);
			this.label3.TabIndex = 7;
			this.label3.Text = "Linux Path:";
			this.label3.Click += new System.EventHandler (this.label3_Click);
			// 
			// linux_path
			// 
			this.linux_path.Location = new System.Drawing.Point (118, 118);
			this.linux_path.MaxLength = 250;
			this.linux_path.Name = "linux_path";
			this.linux_path.Size = new System.Drawing.Size (100, 20);
			this.linux_path.TabIndex = 8;
			// 
			// SettingsDialog
			// 
			this.AcceptButton = this.button_ok;
			this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.button_cancel;
			this.ClientSize = new System.Drawing.Size (544, 325);
			this.Controls.Add (this.linux_path);
			this.Controls.Add (this.label3);
			this.Controls.Add (this.label2);
			this.Controls.Add (this.windows_path);
			this.Controls.Add (this.label1);
			this.Controls.Add (this.server_url);
			this.Controls.Add (this.label_server_url);
			this.Controls.Add (this.button_cancel);
			this.Controls.Add (this.button_ok);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SettingsDialog";
			this.Text = "Mono Addin Settings";
			this.Load += new System.EventHandler (this.SettingsDialog_Load);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler (this.SettingsDialog_FormClosing);
			this.ResumeLayout (false);
			this.PerformLayout ();

		}

		#endregion

		private System.Windows.Forms.Button button_ok;
		private System.Windows.Forms.Button button_cancel;
		private System.Windows.Forms.Label label_server_url;
		private System.Windows.Forms.TextBox server_url;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox windows_path;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox linux_path;
	}
}