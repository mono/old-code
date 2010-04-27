//
// FileSelectionDialog.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//         Daniel Morgan <monodanmorg@yahoo.com>
//
// Copyright (C) 2002, Duncan Mak, Ximian Inc.
// Copyright (C) 2002-2005, Daniel Morgan
//

using System;
using Gtk;

namespace Mono.Data.SqlSharp.GtkSharp
{
	public class FileSelectionEventArgs 
	{
		private string filename;

		public FileSelectionEventArgs (string filename) 
		{
			this.filename = filename;
		}

		public string Filename {
			get {
				return filename;
			}
		}
	}

	public delegate void FileSelectionEventHandler (object sender, FileSelectionEventArgs e);

	public class FileSelectionDialog 
	{
		private FileSelection window = null;
		private string caption = "";

		public event FileSelectionEventHandler fh;
		
		public FileSelectionDialog (string title, FileSelectionEventHandler fileSelectedHandler) 
		{
			if(fileSelectedHandler == null)
				throw new Exception ("FileSelectionDialog fileSelectedHandler is null");

			caption = title;
			fh = fileSelectedHandler;

			Show ();
		}

		void Show() 
		{
			window = new FileSelection (caption);

			window.OkButton.Clicked += new EventHandler (OnFileSelectionOk);
			window.CancelButton.Clicked += new EventHandler (OnFileSelectionCancel);

			window.ShowAll ();
		}

		void OnFileSelectionOk(object o, EventArgs args) 
		{
			string filename = window.Filename;
			FileSelectionEventArgs fa = new FileSelectionEventArgs (filename);
			
			if (fh != null) {
				fh (this, fa); 
			}

			window.OkButton.Clicked -= new EventHandler (OnFileSelectionOk);
			window.CancelButton.Clicked -= new EventHandler (OnFileSelectionCancel);

			fa = null;
			fh = null;
			o = null;

			window.Destroy ();
			window = null;
		}

		void OnFileSelectionCancel (object o, EventArgs args) 
		{
			window.OkButton.Clicked -= new EventHandler (OnFileSelectionOk);
			window.CancelButton.Clicked -= new EventHandler (OnFileSelectionCancel);

			fh = null;
			window.Destroy ();
			window = null;
		}
	}
}

