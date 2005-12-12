using System;
using Gst;

class PlaySample
{
	static void Main (string[] args)
	{
		Gst.Application.Init ("PlaySample", ref args);
		new PlaySample (args);
	}

	PlaySample (string[] args)
	{
		if (args.Length != 1)
		{
			Console.WriteLine ("usage play.exe <filename>");
			Environment.Exit (0);
		}

		Play play = new Play ();

		play.StreamLength += OnStreamLength;
		play.HaveVideoSize += OnVideoSize;
		play.FoundTag += OnFoundTag;
		play.Eos += OnEos;

		play.SetDataSrc (ElementFactory.Make ("filesrc", "filesrc"));
		//Element audiosink = Gst.Gconf.DefaultAudioSink;
		Element audiosink = ElementFactory.Make ("alsasink", "alsasink");
		//using (GLib.Value val = new GLib.Value("hw:0")) {
		//  audiosink.SetProperty("device", val);
		//}

		play.SetAudioSink (audiosink);
		play.SetVideoSink (ElementFactory.Make ("xvimagesink", "xvimagesink"));
		play.SetLocation (args[0]);
		play.SetState (ElementState.Playing);

		//play.SetState (ElementState.Null);
	}

	void OnStreamLength (object o, StreamLengthArgs args)
	{
		Console.WriteLine ("Length: " + args.LengthNanos);
	}

	void OnFoundTag (object o, FoundTagArgs args)
	{
		Console.WriteLine ("Found tag: " + args.TagList);
	}

	void OnVideoSize (object o, HaveVideoSizeArgs args)
	{
		Console.WriteLine ("width {0}", args.Width);
		Console.WriteLine ("height {0}", args.Height);
	}

	void OnEos (object o, EventArgs args)
	{
		Console.WriteLine ("Eos");
		Environment.Exit (0);
	}
}

