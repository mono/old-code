using System;
using Gst;
using GLib;

public class MixerSample
{
        static void Main (string[] args)
        {		
	        Application.Init ("Mixer", ref args);
    
		Element alsamixer = ElementFactory.Make ("alsamixer", "alsamixer");
		using (GLib.Value val = new GLib.Value("hw:0")) {
		  alsamixer.SetProperty("device", val);
		}

		alsamixer.SetState (ElementState.Ready);

		if (!alsamixer.ImplementsInterface(Mixer.GType)) {
			throw new ApplicationException("Alsamixer does not appear to support the mixer interface.");
		}
		
		Mixer mixer = new Mixer(ImplementsInterface.Cast(alsamixer.Handle, Mixer.GType));
    
		foreach (MixerTrack track in mixer.ListTracks()) {
			System.Console.WriteLine("Track: " + track.Label);
			if (track.GetType() == typeof(MixerTrack)) {
				System.Console.Write("  Volume (" + track.MinVolume + "-" + track.MaxVolume + "): ");
				int[] volumes = mixer.GetVolume(track);
				foreach (int volume in volumes) {
					System.Console.Write(volume + " ");
				}
				System.Console.WriteLine();
			} else {
				MixerOptions mixerOptions = (MixerOptions) track;
				System.Console.WriteLine("  Option: " + mixer.GetOption(mixerOptions));
			}
		}
	}
}
