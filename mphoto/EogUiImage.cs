/*
 * EogUiImage.cs
 */

using System;
using System.Runtime.InteropServices;

class EogUiImage : Gtk.ScrolledWindow {

	public EogUiImage (IntPtr raw) : base (raw)
	{
	}

	[DllImport("eog")]
	static extern IntPtr ui_image_new ();

	public EogUiImage ()
	{
		Raw = ui_image_new ();
	}

	[DllImport("eog")]
	static extern IntPtr ui_image_get_image_view (IntPtr ui);

	public EogImageView ImageView {
		get {
			IntPtr raw_view = ui_image_get_image_view (Handle);
			if (raw_view != IntPtr.Zero) {
				return new EogImageView (raw_view);
			}
			return null;
		}
	}

	[DllImport("eog")]
	static extern void ui_image_zoom_fit (IntPtr ui);

	public void ZoomFit ()
	{
		ui_image_zoom_fit (Handle);
	}

//    [DllImport("eog")]
//    static extern void ui_image_fit_to_screen (IntPtr ui);

//    public void FitToScreen () {
//        ui_image_fit_to_screen (Handle);
//    }
}
