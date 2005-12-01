//
//  Application.cs
//
//  (c) 2004 Geoff Norton

using System;
using Apple.Foundation;
using Apple.AppKit;
using Apple.Tools;

namespace Apple.AppKit
{
	public class Application
	{
		static NSAutoreleasePool pool;
		static NSBundle mainBundle;

		static Application ()
		{
		}

		public static void Init ()
		{
//			pool = (NSAutoreleasePool)NSAutoreleasePool.AllocWithZone (IntPtr.Zero);
			pool = new NSAutoreleasePool ();
			pool.init ();
			mainBundle = NSBundle.MainBundle;
			Console.WriteLine (mainBundle.resourcePath.ToString ());
			// We import Foundation and AppKit by default
			LoadFramework ("Foundation");
			LoadFramework ("AppKit");
		}

		public static void Run ()
		{
			((NSApplication)NSApplication.SharedApplication).run ();
		}

		public static void LoadFramework (string frameworkName)
		{
			NSBundle frmwrkBundle = (NSBundle)NSBundle.BundleWithPath ("/System/Library/Frameworks/" + frameworkName + ".framework");
			if (frmwrkBundle == null)
				frmwrkBundle = (NSBundle)NSBundle.BundleWithPath (NSBundle.MainBundle.resourcePath.ToString () + "/" + frameworkName + ".framework");
			if (frmwrkBundle == null)
				frmwrkBundle = (NSBundle)NSBundle.BundleWithPath ("./" + frameworkName + ".framework");
			if (frmwrkBundle == null)
				throw new Exception ("Couldn't locate framework: " + frameworkName);
			frmwrkBundle.load ();
		}
			
		public static bool LoadNib (string nibName)
		{
			NSApplication sharedApp = (NSApplication)NSApplication.SharedApplication;
			NSDictionary dict = (NSDictionary)NSDictionary.DictionaryWithObject_forKey(sharedApp, new NSString ("NSOwner"));
			ObjCMessaging.objc_msgSend(Apple.Foundation.Class.Get("NSBundle"),"loadNibFile:externalNameTable:withZone:",typeof(System.SByte), typeof(System.IntPtr), NSObject.Net2NS (new NSString(nibName)), typeof(System.IntPtr), NSObject.Net2NS (dict), typeof(System.IntPtr), sharedApp.zone);
			ObjCMessaging.objc_msgSend(Apple.Foundation.Class.Get("NSBundle"),"loadNibNamed:owner:",typeof(System.SByte), typeof(System.IntPtr), NSObject.Net2NS (new NSString(nibName)), typeof(System.IntPtr), NSObject.Net2NS (sharedApp));
			return true;
		}
	}
}

