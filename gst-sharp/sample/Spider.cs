using System;
using Gst;

// This is identical to PlayMp3.cs
// except it uses a Spider to auto detect
// the file types and play it so it should
// work for any file that contains an audio
// stream assuming gst has a plugin for it.

public class SpiderSample
{
        static void Main (string[] args)
        {		
	        Application.Init ("SpiderSample", ref args);    
    
		if (args.Length != 1) {
		        Console.WriteLine ("usage: spider.exe <FILE>");
			return;
		}

		// create a new bin to hold the elements
		Pipeline bin = new Pipeline ("pipeline");
    
		// create a filesrc element to read the file
		Element filesrc = ElementFactory.Make ("filesrc", "filesrc");
		filesrc.SetProperty ("location", args[0]);
		
		Element spider = ElementFactory.Make ("spider", "spider");
		
		// Create the audio sink
		Element audiosink = Gst.Gconf.DefaultAudioSink;
		
		// add the elements to the main pipeline
		bin.Add (filesrc);
		bin.Add (spider);
		bin.Add (audiosink);
    
		// connect the elements
		filesrc.Link (spider);
		spider.Link (audiosink);
		
		// start playing
		bin.SetState (ElementState.Playing);
    
		while (bin.Iterate());
    
		// stop the bin
		bin.SetState (ElementState.Null);
	}
}
