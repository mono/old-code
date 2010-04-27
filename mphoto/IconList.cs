// -*- c-basic-offset: 8; -*-
// IconList.cs: Implements the on-demand loading IconList widget
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// Selection:
//   Ravi Pratap     (ravi@ximian.com)
//
// (C) 2002 Ximian, Inc.
//
//#define DORKY_HIGHLIGHT
using Gtk;
using Gdk;
using GtkSharp;
using System;
using System.Collections;

using System.Runtime.InteropServices;
public delegate void SelectionChange ();

public class IconList : Gtk.DrawingArea { // , IImageNotify {
 	const int x_margin = 20;
	const int y_margin = 20;

	//
	// Configuration about the grid
	//
        int raw_icon_width, raw_icon_height;
        int icon_width, icon_height;
	int cell_width, cell_height, margin_left, margin_top;
	int max_top;
        float zoom;

	//
	// Computed: visible on this view.
	//
	int visible_cols, visible_rows;

	//
	// The top-most visible row
	//
	int top_row = 0;
	
	//
	// Number of images
	//
	int image_count;

	//
	// Tracks the selection
	//
	public BitArray Selection;
	int select_count = 0;
	int selection_start_item = -1;
	bool shift_pressed = false;
	bool control_pressed = false;

	Adjustment adjustment;

        public IIconListAdapter adapter;

	Gdk.Window window;

        // Cached gcs
        Gdk.GC white_gc, bkgr_gc, selection_gc;

	Gtk.Window status;
	Gtk.Label status_l;

	int last_item_clicked;
	uint last_click_time;
	uint double_click_time;

	int mouse_col = -1, mouse_row = -1;
	bool highlight_mouse;

	public event SelectionChange SelectionChanged;

	public int SelectedCount { get { return select_count; } }
	public int SelectedItem { get { return selection_start_item; } }
	
	public IconList () : base ()
	{
		status = new Gtk.Window ("status");
		status_l = new Gtk.Label ("Status");
		status.Add (status_l);
		//status.ShowAll ();
		
		SetSizeRequest (670, 370);
		CanFocus = true;
		
		Realized += new EventHandler (RealizeHanlder);
		Unrealized += new EventHandler (UnrealizeHandler);
		SizeAllocated += new SizeAllocatedHandler (SizeAllocatedHandler);
		MotionNotifyEvent += new MotionNotifyEventHandler (MotionHandler);
		ButtonPressEvent += new ButtonPressEventHandler (ButtonHandler);
		KeyPressEvent += new KeyPressEventHandler (KeyPressHandler);
		KeyReleaseEvent += new KeyReleaseEventHandler (KeyReleaseHandler);
		ScrollEvent += new ScrollEventHandler (ScrollHandler);
		
		AddEvents ((int) (EventMask.ExposureMask |
				  EventMask.LeaveNotifyMask |
				  EventMask.ButtonPressMask |
				  EventMask.PointerMotionMask |
				  EventMask.KeyPressMask |
				  EventMask.ScrollMask |
				  EventMask.KeyReleaseMask));

                zoom = 1.0f;

		SetPreviewSize (160, 120);
		
		adjustment = new Adjustment (0, 0, 0, 0, 0, 0);
		adjustment.ValueChanged += new EventHandler (ValueChangedHandler);

                image_count = 0;

		Gtk.Settings s = Gtk.Settings.Default;
		double_click_time = (uint) s.DoubleClickTime;

		last_click_time = 0;
	}

        public IconList (IIconListAdapter a) : this ()
        {
                this.adapter = a;
        }

        public IIconListAdapter Adapter {
                get {
                        return adapter;
                }
                set {
                        if (this.adapter != null)
                                this.adapter.IconList = null;
                        this.adapter = value;
                        this.adapter.IconList = this;

                        Refresh ();
                }
        }

	void ScrollHandler (object o, ScrollEventArgs args)
	{
		Gdk.EventScroll es = args.Event;
		double newloc = 0.0;

		switch (es.Direction){
		case ScrollDirection.Up:
			newloc = adjustment.Value - visible_cols;
			break;
			
		case ScrollDirection.Down:
			newloc = adjustment.Value + visible_cols;
			break;
		}
		if (newloc < 0)
			newloc = 0;
		else if (newloc >= max_top)
			newloc = Math.Max (max_top, 0);

		adjustment.Value = newloc;
	}
	
	/// <summary>
	///   The adjustment for the icon view
	/// </summary>
	public void SetPreviewSize (int width, int height)
	{
                raw_icon_width = width;
                raw_icon_height = height;
		icon_width = (int) (width * zoom);
		icon_height = (int) (height * zoom);

		cell_width = icon_width + (int) (x_margin * zoom);
		cell_height = icon_height + (int) (y_margin * zoom);
		margin_left = (int) (x_margin * zoom / 2);
		margin_top = (int) (y_margin * zoom / 2);
	}


        // This is necessary until the bindings get fixed
        public float Zoom {
                get {
                        return zoom;
                }
                set {
                        zoom = value;
                        SetPreviewSize (raw_icon_width, raw_icon_height);

                        // clear 
                        if (window != null) {
                                // clear
                                window.DrawRectangle
                                        (bkgr_gc,
                                         true,
                                         0, 0,
                                         Allocation.Width,
                                         Allocation.Height);
                                SizeChanged (Allocation.Width,
                                             Allocation.Height);
                                DrawAll ();
                        }
                }
        }

	public Adjustment Adjustment {
		get {
			return adjustment;
		}
	}

	void DrawCell (int c, int r, int item)
	{
                if (adapter == null)
                        return;

                Pixbuf image = adapter[item];

		if (image == null)
			return;

#if DORKY_HIGHLIGHT
		Console.WriteLine ("{0} {1} //  {2} {3} // {4}", c, r, mouse_col, mouse_row, highlight_mouse);
		if (c == mouse_col && r == mouse_row && highlight_mouse){
			Pixbuf original = image;
			image = new Pixbuf (original.Colorspace, original.HasAlpha, original.BitsPerSample,
					    original.Width, original.Height);

			original.CopyArea (0, 0, original.Width, original.Height, image, 0, 0);
			image.SaturateAndPixelate (image, 4.0f, false);
		}
#endif

		int x = c * cell_width + margin_left;
		int y =  r * cell_height + margin_top;

		int iw = (int) Math.Min (image.Width * zoom, icon_width);
		int ih = (int) Math.Min (image.Height * zoom, icon_height);

		x += (icon_width - iw) / 2;
		y += (icon_height - ih) / 2;

                // paint over any possible  old selection
                if (!Selection.Get (item)) {
                        for (int i = 0; i < 5; i++) {
                                window.DrawRectangle
                                        (bkgr_gc, false,
                                         x - i - 1, y - i - 1,
                                         iw + i * 2 + 1, ih + i * 2 + 1);
                        }
                }

                if (iw != image.Width || ih != image.Height) {
                        using (Gdk.Pixbuf scaled_image = image.ScaleSimple (iw, ih, InterpType.Tiles)) {
                                Console.WriteLine ("scalesimple pixbuf is at 0x" + scaled_image.Handle.ToInt32().ToString("x"));
                                scaled_image.RenderToDrawable (window, white_gc,
                                                               0, 0, x, y, iw, ih,
                                                               Gdk.RgbDither.None, 0, 0);
                        }
                } else {
                        image.RenderToDrawable (
                                window, white_gc,
                                0, 0, x, y, iw, ih, Gdk.RgbDither.None, 0, 0);
                }


                // then draw any selection
		if (Selection.Get (item)) {
                        window.DrawRectangle (selection_gc, false,
                                              x - 4, y - 4,
                                              iw + 7, ih + 7);
		}
		
	}

	int ItemAt (int c, int r)
	{
		int first = top_row * visible_cols;
		
		return first + r * visible_cols + c;
	}

        int ItemAtPixel (int x, int y)
        {
                if (x > visible_cols * cell_width ||
                    y > visible_rows * cell_height)
                        return -1;

                int col = (int) x / cell_width;
                int row = (int) y / cell_height;

                return ItemAt (col, row);
        }

	void DrawCell (int c, int r)
	{
		DrawCell (c, r, ItemAt (c, r));
	}

	void DrawCell (int item)
	{
		int sitem = item - (top_row * visible_cols);
		int row = sitem / visible_cols;
		int col = sitem % visible_cols;

		DrawCell (col, row, item);
	}

	void DrawAll ()
	{
		int first = top_row * visible_cols;
		int item = first;

		for (int r = 0; r < visible_rows; r++) 
			for (int c = 0; c < visible_cols; c++){
				if (item >= image_count)
					return;
				DrawCell (c, r, item++);
			}
	}

	void RealizeHanlder (object o, EventArgs sender)
        {
                white_gc = Style.WhiteGC;

                bkgr_gc = Style.BackgroundGC (StateType.Normal);

                selection_gc = new Gdk.GC (GdkWindow);
                selection_gc.Copy (Style.BackgroundGC (StateType.Normal));
                Gdk.Color fgcol = new Gdk.Color ();
                fgcol.Pixel = 0x000077ee;
                selection_gc.Foreground = fgcol;
                selection_gc.SetLineAttributes (3, LineStyle.Solid, CapStyle.NotLast, JoinStyle.Round);
        }

	void UnrealizeHandler (object o, EventArgs sender)
	{
		selection_gc.Dispose ();
		selection_gc = null;
	}

	protected override bool OnExposeEvent (Gdk.EventExpose ev)
	{
		window = ev.Window;
		Gdk.Rectangle area = ev.Area;

		//Console.WriteLine ("x={0}, y={1}, w={2}, h={3}", area.x, area.y, area.width, area.height);
		DrawAll ();
		
		return false;
	}

	void MotionHandler (object o, MotionNotifyEventArgs args)
	{
		Gdk.EventMotion pos = args.Event;
		int pixc = (int) pos.X % cell_width;
		int pixr = (int) pos.Y % cell_height;

		mouse_col = (int) pos.X / cell_width;
		mouse_row = top_row * visible_cols + (int) pos.Y / cell_height;
		
		if (((pixc > x_margin && pixc < (cell_width - x_margin)) &&
		     (pixr > y_margin && pixr < (cell_height - y_margin))))
			highlight_mouse = true;
		else
			highlight_mouse = false;

		//Console.WriteLine ("File: " + ((DirectoryProvider) provider).images [ItemAt (mouse_col, mouse_row)].File);
		//DrawCell (mouse_col, mouse_row);
	}
	
	void SetColRows (int c, int r)
	{
		visible_cols = c;
		visible_rows = r;
		
		QueueDrawArea (0, 0, Allocation.Width, Allocation.Height);

		int page_items = visible_cols * visible_rows;
		max_top = Math.Max (0, image_count - page_items);

                // add visible_cols+1 to force an extra "row" on the
                // adjustment, so that we can still scroll the
                // bottommost row to full visibility, even if it would
                // get rendered as cut off due to window size
		Console.WriteLine ("Setting bounds: {0} {1} {2} {3}", 0, image_count + visible_cols + 1, visible_cols, page_items, page_items);
		adjustment.SetBounds (0, image_count + visible_cols + 1, visible_cols, page_items, page_items);
                adjustment.ChangeValue ();
	}
	
	void SizeAllocatedHandler (object obj, SizeAllocatedArgs args)
	{
		Gdk.Rectangle rect = args.Allocation;
		if (rect.Equals (Gdk.Rectangle.Zero))
			Console.WriteLine ("ERROR: Allocation is null!");

                SizeChanged (rect.Width, rect.Height);
        }

        void SizeChanged (int width, int height)
        {
		int new_cols = width / cell_width;
		int new_rows = (int) Math.Ceiling ((double) height / (double) cell_height);

		if (new_cols != visible_cols || new_rows != visible_rows)
			SetColRows (new_cols, new_rows);
	}

	void ButtonHandler (object obj, ButtonPressEventArgs args)
	{
		Gdk.EventButton pos = args.Event;
		bool double_click = false;

		int item_clicked = ItemAtPixel ((int) pos.X, (int) pos.Y);

                int begin = ItemAt (0, 0);
                int end = Math.Min (ItemAt (visible_cols, visible_rows - 1), image_count);
                int count = visible_cols * visible_rows;

		if (item_clicked < begin || item_clicked >= end) {
			args.RetVal = true;
			return;
		}

		GrabFocus ();
		switch (pos.Button){
		case 1:
			bool [] repaint = new bool [count];

			bool control_pressed = (pos.State & Gdk.ModifierType.ControlMask) != 0;
			bool shift_pressed = (pos.State & Gdk.ModifierType.ShiftMask) != 0;
			
			if (shift_pressed && selection_start_item != -1){
				int first;
				int last;

				if (selection_start_item != item_clicked) {
					Selection.SetAll (false);
					if (selection_start_item < item_clicked) {
						first = selection_start_item;
						last = item_clicked;
					} else {
						first = item_clicked;
						last = selection_start_item;
					}

					for (int i = first; i <= last; i++) {
						Selection [i] = true;
                                                if (i >= begin)
                                                        repaint [i - begin] = true;
						select_count++;
					}
				} else {
					Selection[selection_start_item] = ! Selection[selection_start_item];
					repaint[selection_start_item] = true;
				}
			} else if (control_pressed){
				select_count++;
				if (Selection [item_clicked]){
					Selection [item_clicked] = true;
					select_count--;
				} else {
					Selection [item_clicked] = true;
					select_count++;
				}
				Selection [item_clicked] = !Selection [item_clicked];
				repaint [item_clicked-begin] = true;
			} else {
				if (select_count != 0){
					for (int i = begin; i < end; i++){
						if (Selection [i])
							repaint [i - begin] = true;
					}
					
					Selection.SetAll (false);
				}
				select_count = 1;
				Selection [item_clicked] = true;
				repaint [item_clicked-begin] = true;
				selection_start_item = item_clicked;
			} 

			for (int i = begin; i < end; i++){
				if (repaint [i - begin]){
					DrawCell (i);
				}
			}

			if (SelectionChanged != null)
				SelectionChanged ();
			
			break;

		case 3:
                        IconListPopup ip = new IconListPopup (this, item_clicked);
			ip.Activate (pos);
			break;

		default:
			return;
		}

		if (last_click_time != 0) {
			if (pos.Time - last_click_time <= double_click_time &&
                            item_clicked == last_item_clicked)
                        {
				double_click = true;
			}
		}

		if (double_click) {
                        if (this.Activated != null)
                                this.Activated (this, EventArgs.Empty);
			last_click_time = 0;
                        last_item_clicked = 0;
		} else {
			last_click_time = pos.Time;
                        last_item_clicked = item_clicked;
		}

		args.RetVal = true;
	}

	class PopupContextMenu {
		IconList icon_list;
		int item;
		
		public PopupContextMenu (IconList l, int pos)
		{
			icon_list = l;
			item = pos;
		}

		void Action_CopyImageLocation (object o, EventArgs a)
		{
			Clipboard clipboard = Clipboard.Get (Atom.Intern ("PRIMARY", false));

			string name = System.IO.Path.GetFullPath (icon_list.adapter.GetFullFilename (item));
			clipboard.SetText (name);
			
		}

		void Action_RemoveImage (object o, EventArgs a)
		{
                        icon_list.Adapter.DeleteItem (item);
		}
	
		public void Activate (Gdk.EventButton eb)
		{
			Menu popup_menu = new Menu ();
			
			if (icon_list.select_count >= 0) {
				GtkUtil.MakeMenuItem (popup_menu, "Copy Image Location", new EventHandler (Action_CopyImageLocation));
				GtkUtil.MakeMenuItem (popup_menu, "Remove Image", new EventHandler (Action_RemoveImage));
                        }
			
			popup_menu.Popup (null, null, null, IntPtr.Zero, eb.Button, eb.Time);
		}
	}

	void KeyPressHandler (object obj, KeyPressEventArgs args)
	{
		Console.WriteLine ("Pressed");
		// nothing
	}

	void KeyReleaseHandler (object obj, KeyReleaseEventArgs args)
	{
		// nothing
	}

	void UpdateStatus ()
	{
		//status_l.Text = String.Format ("Top = {0}, Last = {1}",
		//		       top_row * visible_cols,
		//		       top_row * visible_cols + visible_cols * visible_rows);
	}
       
	void ValueChangedHandler (object obj, EventArgs e)
	{
                int new_top;

                if (visible_cols == 0)
                        new_top = 0;
		else
                        new_top = ((int) adjustment.Value + 1) / visible_cols;

		if (new_top != top_row) {
			top_row = new_top;
			QueueDrawArea (0, 0, Allocation.Width, Allocation.Height);
		}

		UpdateStatus ();
	}

	public static Pixbuf LoadingImage = null;
	
	static IconList ()
	{
		LoadingImage = new Pixbuf (null, "loading.png");
	}

        public void Refresh ()
        {
                int i;
                image_count = adapter.Count;
                Selection = new BitArray (image_count);
                
                // now need to scroll to the topin some magic and repaint that i'm not familiar with
                SetColRows (visible_cols, visible_rows);
                top_row = 0;
                DrawAll ();
        }

  	public void RedrawItem (int item)
  	{
  		int first = top_row * visible_cols;
  		int last = first + visible_cols * visible_rows;
  		int i = first;

  		if (item < first || item > last)
  			return;
		
  		for (int r = 0; r < visible_rows; r++)
  			for (int c = 0; c < visible_cols; c++){
  				if (i >= image_count)
  					return;
  				if (i == item){
  					DrawCell (c, r, item);
  				}
  				i++;
  			}
  	}

        public int CountSelected {
                get {
                        return select_count;
                }
        }

	private Hashtable Signals = new Hashtable();

	public event EventHandler Activated;
}
