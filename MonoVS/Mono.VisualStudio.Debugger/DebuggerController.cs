//
// based on Lluis' DebuggerController.cs
//
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

using Mono.VisualStudio.Mdb;

namespace Mono.VisualStudio.Debugger
{
	public class DebuggerController : MarshalByRefObject, IMdbController
	{
		AutoResetEvent running_event = new AutoResetEvent (false);
		int pid = -1;

		public IMdbManager DebuggerManager
		{
			get;
			private set;
		}

		public string BaseURL
		{
			get;
			private set;
		}

		static DebuggerController ()
		{
			RemotingConfiguration.CustomErrorsEnabled (false);
			RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;

			IChannel ch = ChannelServices.GetChannel ("tcp");
			if (ch == null) {
				IDictionary dict = new Hashtable ();
				var client_provider = new BinaryClientFormatterSinkProvider ();
				var server_provider = new BinaryServerFormatterSinkProvider ();

				dict["port"] = 0;
				server_provider.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
				ChannelServices.RegisterChannel (new TcpChannel (dict, client_provider, server_provider), false);
			}
		}

		public void RegisterDebugger (IMdbManager debugger)
		{
			this.DebuggerManager = debugger;

			MonoAddin.Settings settings = MonoAddin.Settings.Instance ();
			debugger.AddDirectoryMapping (settings.WindowsPath, settings.LinuxPath);
			running_event.Set ();
		}

		public IDebuggerServer Launch (string exe, string args, string dir, string env, IClientEventSink sink)
		{
			return DebuggerManager.Launch (exe, args, dir, env, sink);
		}

		public void Kill ()
		{
			UriBuilder target = new UriBuilder (BaseURL + "/kill");
			target.Query = String.Format ("pid={0}", pid);
			Post (target.ToString ());
		}

		public void Shutdown ()
		{
			UriBuilder target = new UriBuilder (BaseURL + "/shutdown");
			target.Query = "foo=bar";
			Post (target.ToString ());
		}

		internal DebuggerController (string baseurl)
		{
			this.BaseURL = baseurl;

			lock (this) {
				string response = PostStart (CreateRemotingReference ());
				if (response == "OK") {
					Utils.Message ("STARTED IN-PROC SERVER!");
					pid = 0;
				} else if (response.StartsWith ("SERVER: ")) {
					Utils.Message ("STARTED SERVER!");
					pid = Int32.Parse (response.Substring (8));
				} else {
					throw new ApplicationException ("Got unknown response: " + response);
				}
			}

			if (!running_event.WaitOne (15000, false))
				throw new ApplicationException ("Coult not create the debugger process.");
		}

		string PostStart (string reference)
		{
			UriBuilder target = new UriBuilder (BaseURL + "/start");
			Utils.Message ("START: {0} {1}", reference.Length, reference);

			WebRequest wr = WebRequest.Create (target.ToString ());
			wr.Method = "POST";
			using (TextWriter writer = new StreamWriter (wr.GetRequestStream ())) {
				writer.WriteLine (reference);
			}

			HttpWebResponse r = (HttpWebResponse) wr.GetResponse ();
			using (TextReader reader = new StreamReader (r.GetResponseStream ())) {
				string text = reader.ReadToEnd ();
				Utils.Message ("RESPONSE: |{0}|", text);
				return text;
			}
		}

		string Post (string request)
		{
			Utils.Message ("POST: {0}", request);
			WebRequest wr = WebRequest.Create (request);
			wr.Method = "POST";
			wr.ContentLength = 0;

			// Flush the result
			HttpWebResponse r = (HttpWebResponse) wr.GetResponse ();
			using (TextReader reader = new StreamReader (r.GetResponseStream ())) {
				string text = reader.ReadToEnd ();
				Utils.Message ("RESPONSE: |{0}|", text);
				return text;
			}
		}

		string CreateRemotingReference ()
		{
			BinaryFormatter bf = new BinaryFormatter ();
			ObjRef oref = RemotingServices.Marshal (this);
			MemoryStream ms = new MemoryStream ();
			bf.Serialize (ms, oref);
			return Convert.ToBase64String (ms.ToArray ());
		}
	}
}
