using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace MonoAddin
{
	public class Settings
	{
		const string folder_name = "MonoAddin";
		const string file_name = "MonoAddin.xml";

		const string element_settings = "Settings";
		const string element_setting_server_url = "ServerURL";
		const string element_settings_windows_path = "WindowsPath";
		const string element_settings_linux_path = "LinuxPath";

		static Settings instance;

		public static Settings Instance ()
		{
			if (instance == null) {
				instance = new Settings ();
				instance.Load ();
			}

			return instance;
		}

		Settings ()
		{
			ServerURL = "http://164.99.120.105:7777/mono-debugger/start";
			WindowsPath = "c:\\share";
			LinuxPath = "/work/mordor";
		}

		public string ServerURL { get; set; }
		public string WindowsPath { get; set; }
		public string LinuxPath { get; set; }

		public void Save ()
		{
			string file = GetSettingsFile ();
			using (XmlTextWriter writer = new XmlTextWriter (file, Encoding.UTF8)) {
				writer.Formatting = Formatting.Indented;
				writer.WriteStartDocument ();
				writer.WriteStartElement (element_settings);

				writer.WriteStartElement (element_setting_server_url);
				writer.WriteValue (ServerURL);
				writer.WriteEndElement ();

				writer.WriteStartElement (element_settings_windows_path);
				writer.WriteValue (WindowsPath);
				writer.WriteEndElement ();

				writer.WriteStartElement (element_settings_linux_path);
				writer.WriteValue (LinuxPath);
				writer.WriteEndElement ();

				writer.WriteEndElement ();
				writer.WriteEndDocument ();
			}
		}

		public void Load ()
		{
			string file = GetSettingsFile ();

			// we have no persisted settings
			if (!File.Exists (file))
				return;

			using (XmlTextReader reader = new XmlTextReader (file)) {
				string element = string.Empty;
				while (!reader.EOF) {
					switch (reader.NodeType)
					{
						case XmlNodeType.Document:
							break;
						case XmlNodeType.Element:
							element = reader.Name;
							break;
						case XmlNodeType.EndElement:
							element = string.Empty;
							break;
						case XmlNodeType.Text:
							if (element != string.Empty) {
								switch (element) {
									case element_settings:
										break;
									case element_setting_server_url:
										ServerURL = reader.Value;
										break;
									case element_settings_linux_path:
										LinuxPath = reader.Value;
										break;
									case element_settings_windows_path:
										WindowsPath = reader.Value;
										break;
								}
							}
							break;
						default:
							break;
					}
					reader.Read ();
				}
			}
		}

		string GetSettingsFile ()
		{
			string folder = Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData);
			folder = Path.Combine (folder, folder_name);
			if (!Directory.Exists (folder))
				Directory.CreateDirectory (folder);

			string file = Path.Combine (folder, file_name);
			return file;
		}
	}
}
