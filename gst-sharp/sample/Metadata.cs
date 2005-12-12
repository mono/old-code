using System;
using System.Runtime.InteropServices;

using Gst;

public class GstTest
{
	private static bool handoff = false;
	private static bool eos = false;
	private static Element sink;

        static void Main(string[] args)
        {		
	        Application.Init ("Metadata", ref args);
    
		if (args.Length != 1) {
		        Console.WriteLine ("usage: metadata.exe FILE");
			return;
		}

		// create a new bin to hold the elements
		Pipeline bin = new Pipeline ("pipeline");
		bin.Error += OnError;
		bin.FoundTag += OnFoundTag;
    
		// create a filesrc element to read the file
		Element filesrc = ElementFactory.Make ("filesrc", "filesrc");
		filesrc.SetProperty ("location", args[0]);
		
		Element typefind = ElementFactory.Make ("typefind", "typefind");
		
		// create the spider element
		Element spider = ElementFactory.Make ("spider", "spider");
		
		// Create the fakesink
		sink = ElementFactory.Make ("fakesink", "fakesink");
		sink.Eos += OnEos;
		((FakeSink)sink).Handoff += OnHandoff;

		// add the elements to the main pipeline
		bin.Add (filesrc);
		bin.Add (typefind);
		bin.Add (spider);
		bin.Add (sink);
    
		// connect the elements
		filesrc.Link (typefind);
		typefind.Link (spider);
		
		sink.SetProperty ("signal-handoffs", true);
		Caps caps = Caps.FromString ("audio/x-raw-int");
		spider.LinkFiltered (sink, ref caps);
		
		// start playing
		bin.SetState (ElementState.Playing);
    
		while (bin.Iterate() && !handoff && !eos);
    
		// stop the bin
		bin.SetState (ElementState.Null);
	}
	
	static void OnError (object o, ErrorArgs args)
	{
		Console.WriteLine ("OnError");
	}
	
	static void OnFoundTag (object o, FoundTagArgs args)
	{
		args.TagList.Foreach (new TagForeachFunc (ForeachTagFunc));
	}
	
	static void OnEos (object o, EventArgs args)
	{
		if (eos) {
			Console.WriteLine ("EOS reentered!");
			return;
		}
	
		sink.SetState (ElementState.Null);
		eos = true;
	}
	
	static void OnHandoff (object o, HandoffArgs args)
	{
		if (handoff) {
			Console.WriteLine ("recursive handoff!");
			return;
		} else if (eos) {
			Console.WriteLine ("caught handoff after eos!");
			return;
		}
		
		handoff = true;
	}
	
	static void ForeachTagFunc (TagList list, string tag)
	{
		GLib.Value val = GLib.Value.Empty;
		TagList.CopyValue (ref val, list, tag);
		Console.WriteLine ("{0, 15}: {1}", tag, val.Val);
	}
}
