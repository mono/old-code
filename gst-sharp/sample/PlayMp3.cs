// Copyright (c) 2002 Alp Toker, (c) 2004 Owen Fraser-Green

using System;
using System.Runtime.InteropServices;

using Gst;

public class GstTest
{
        static void Main(string[] args)
        {		
	        Application.Init ("PlayMp3", ref args);    
    
		if (args.Length != 1) {
		        Console.WriteLine ("usage: play-mp3.exe FILE.mp3");
			return;
		}

		// create a new bin to hold the elements
		Pipeline bin = new Pipeline ("pipeline");
    
		// create a filesrc element to read the file
		Element filesrc = ElementFactory.Make ("filesrc", "filesrc");
		filesrc.SetProperty ("location", args[0]);
		
		// create the mad decoder
		Element mad = ElementFactory.Make ("mad", "mad");
		
		// Create the audio sink
		Element sink = Gst.Gconf.DefaultAudioSink;
		

		// add the elements to the main pipeline
		bin.Add (filesrc);
		bin.Add (mad);
		bin.Add (sink);
    
		// connect the elements
		filesrc.Link (mad);
		mad.Link (sink);
		
		// start playing
		bin.SetState (ElementState.Playing);
    
		while (bin.Iterate());
    
		// stop the bin
		bin.SetState (ElementState.Null);
	}
}
