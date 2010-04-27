/*
 * EogImageView.cs
 */

using System;
using System.Runtime.InteropServices;
using Gdk;

class EogImageView : Gtk.Widget {

	public enum CheckType {
		CheckTypeDark,
		CheckTypeMidtone,
		CheckTypeLight,
		CheckTypeBlack,
		CheckTypeGray,
		CheckTypeWhite
	}

	public enum CheckSize {
		CheckSizeSmall,
		CheckSizeMedium,
		CheckSizeLarge
	}

	public EogImageView (IntPtr raw) : base (raw)
	{
	}

	[DllImport("eog")]
	static extern int image_view_get_type ();

	public static new int GType {
			get {
				int raw_ret = image_view_get_type();
				int ret = raw_ret;
				return ret;
			}
		}

	[DllImport("eog")]
	static extern IntPtr image_view_new ();

	public EogImageView ()
	{
		Raw = image_view_new ();
		Console.WriteLine ("ImageView Raw: " + Raw);
	}

	[DllImport("eog")]
	static extern IntPtr image_view_get_pixbuf (IntPtr view);

	[DllImport("eog")]
	static extern void image_view_set_pixbuf (IntPtr view, IntPtr pixbuf);

	public Gdk.Pixbuf Pixbuf {
		get {
			return new Gdk.Pixbuf (image_view_get_pixbuf (Handle));
		}
		set {
			image_view_set_pixbuf (Handle, value == null ? IntPtr.Zero : value.Handle);
		}
	}

	[DllImport("eog")]
	static extern void image_view_get_zoom (IntPtr ui, out double zoomx, out double zoomy);

	[DllImport("eog")]
	static extern void image_view_set_zoom (IntPtr view, double zoomx, double zoomy, bool have_anchor, int anchorx, int anchory);

	// Assume x and y zoom has the same value
	public double Zoom {
		get {
			double x, y;
			image_view_get_zoom (Handle, out x, out y);
			return x;
		}
		set {
			image_view_set_zoom (Handle, value, value, false, 0, 0);
		}
	}

	[DllImport("eog")]
	static extern int image_view_get_interp_type (IntPtr view);

	[DllImport("eog")]
	static extern void image_view_set_interp_type (IntPtr view, int interp_type);

	public Gdk.InterpType InterpolationType {
		get {
			return (Gdk.InterpType) image_view_get_interp_type (Handle);
		}
		set {
			image_view_set_interp_type (Handle, (int) value);
		}
	}

	[DllImport("eog")]
	static extern int image_view_get_check_type (IntPtr view);

	[DllImport("eog")]
	static extern void image_view_set_check_type (IntPtr view, int check_type);

	public CheckType CheckerType {
		get {
			return (CheckType) image_view_get_check_type (Handle);
		}
		set {
			image_view_set_check_type (Handle, (int) value);
		}
	}


	[DllImport("eog")]
	static extern int image_view_get_check_size (IntPtr view);

	[DllImport("eog")]
	static extern void image_view_set_check_size (IntPtr view, int check_size);

	public CheckSize CheckerSize {
		get {
			return (CheckSize) image_view_get_check_size (Handle);
		}
		set {
			image_view_set_check_size (Handle, (int) value);
		}
	}


	[DllImport("eog")]
	static extern int image_view_get_dither (IntPtr view);

	[DllImport("eog")]
	static extern void image_view_set_dither (IntPtr view, int dither);

	public Gdk.RgbDither Dither {
		get {
			return (Gdk.RgbDither) image_view_get_dither (Handle);
		}
		set {
			image_view_set_dither (Handle, (int) value);
		}
	}
}
