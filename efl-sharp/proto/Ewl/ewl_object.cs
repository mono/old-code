namespace Ewl {

using System;
using System.Runtime.InteropServices;

	public class Object {
	
		HandleRef objRaw;
		
		public virtual IntPtr Raw {
		
			get {
			
				return this.objRaw.Handle;
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern int ewl_object_init(IntPtr obj);
		public int Init() {
		
			return ewl_object_init(Raw);
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_object_geometry_request(IntPtr obj, int x, int y, int w, int h);
		public void GeometryRequest(int x, int y, int w, int h) {
		
			ewl_object_geometry_request(Raw, x, y, w, h);
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_object_current_geometry_get(IntPtr obj, ref int x, ref int y, ref int w, ref int h);
		public int Geometry {
			//TODO: Return struct
			get {
			
				int x = 0, y = 0, w = 0, h = 0;
				ewl_object_current_geometry_get(Raw, ref x , ref y, ref w, ref h);
				
				Console.WriteLine(x + " " + y + " " + w + " " + h);
				
				return 1;
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_object_position_request(IntPtr obj, int x, int y);
		public void PositionRequest(int x, int y) {
		
			ewl_object_position_request(Raw, x, y);
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_object_x_request(IntPtr obj, int x);
		public int XRequest {
		
			set {
			
				ewl_object_x_request(Raw, value);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern int ewl_object_current_x_get(IntPtr obj);
		public int X {
				
			get {
			
				return ewl_object_current_x_get(Raw);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_object_y_request(IntPtr obj, int y);
		public int YRequest {
		
			set {
			
				ewl_object_y_request(Raw, value);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern int ewl_object_current_y_get(IntPtr obj);
		public int Y {
				
			get {
			
				return ewl_object_current_y_get(Raw);
			
			}
		
		}
		
		//The actual size of the object
		[DllImport("libewl")]
		static extern void ewl_object_current_size_get(IntPtr obj, ref int w, ref int h);
		public int Size {
		
			get {
			
				int w = 0, h = 0;
				ewl_object_current_size_get(Raw, ref w, ref h);
				
				Console.WriteLine(w + " " + h);
				
				return 1;
			
			}
		
		}
		
		//Set the requested size of the object
		[DllImport("libewl")]
		static extern void ewl_object_size_request(IntPtr obj, int w, int h);
		public void SizeRequest(int w, int h) {
		
			ewl_object_size_request(Raw, w, h);
		
		}
		
		//Return the requested size of the object
		[DllImport("libewl")]
		static extern void ewl_object_preferred_size_get(IntPtr obj, ref int w, ref int h);
		public int PreferredSize {
		
			get {
			
				int w = 0, h = 0;
				ewl_object_preferred_size_get(Raw, ref w, ref h);
				
				Console.WriteLine(w + " " + h);
				
				return 1;
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern int ewl_object_current_w_get(IntPtr obj);
		public int Width {
				
			get {
			
				return ewl_object_current_w_get(Raw);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_object_w_request(IntPtr obj, int w);
		public int WidthRequest {
		
			set {
		
				ewl_object_w_request(Raw, value);
				
			}
		
		}
		
		[DllImport("libewl")]
		static extern int ewl_object_preferred_w_get(IntPtr obj);
		public int PreferredWidth {
		
			get {
		
				return ewl_object_preferred_w_get(Raw);
				
			}
		
		}
		
		[DllImport("libewl")]
		static extern int ewl_object_current_h_get(IntPtr obj);
		public int Height {
				
			get {
			
				return ewl_object_current_h_get(Raw);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_object_h_request(IntPtr obj, int h);
		public int HeightRequest {
		
			set {
			
				ewl_object_h_request(Raw, value);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern int ewl_object_preferred_h_get(IntPtr obj);
		public int PreferredHeight {
		
			get {
			
				return ewl_object_preferred_h_get(Raw);
				
			}
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_object_minimum_size_set(IntPtr obj, int w, int h);
		[DllImport("libewl")]
		static extern void ewl_object_minimum_size_get(IntPtr obj, ref int w, ref int h);
		public int MinimumSize {
		
			get {
			
				int w = 0, h = 0;
				ewl_object_minimum_size_get(Raw, ref w, ref h);
				
				return 1;
			
			}
			
			set {
			
				int w = 0, h = 0;
				ewl_object_minimum_size_set(Raw, w, h);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_object_minimum_w_set(IntPtr obj, int w);
		[DllImport("libewl")]
		static extern int ewl_object_minimum_w_get(IntPtr obj);
		public int MinimumWidth {
		
			get {
			
				return ewl_object_minimum_w_get(Raw);
			
			}
			
			set {
			
				ewl_object_minimum_w_set(Raw, value);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_object_minimum_h_set(IntPtr obj, int h);
		[DllImport("libewl")]
		static extern int ewl_object_minimum_h_get(IntPtr obj);
		public int MinimumHeight {
		
			get {
			
				return ewl_object_minimum_h_get(Raw);
			
			}
			
			set {
			
				ewl_object_minimum_h_set(Raw, value);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_object_maximum_size_set(IntPtr obj, int w, int h);
		[DllImport("libewl")]
		static extern void ewl_object_maximum_size_get(IntPtr obj, ref int w, ref int h);
		public int MaximumSize {
		
			get {
			
				int w = 0, h = 0;
				ewl_object_maximum_size_get(Raw, ref w, ref h);
				
				return 1;
			
			}
			
			set {
			
				int w = 0, h = 0;
				ewl_object_maximum_size_set(Raw, w, h);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_object_maximum_w_set(IntPtr obj, int w);
		[DllImport("libewl")]
		static extern int ewl_object_maximum_w_get(IntPtr obj);
		public int MaximumWidth {
		
			get {
			
				return ewl_object_maximum_w_get(Raw);
			
			}
			
			set {
			
				ewl_object_maximum_w_set(Raw, value);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_object_maximum_h_set(IntPtr obj, int h);
		[DllImport("libewl")]
		static extern int ewl_object_maximum_h_get(IntPtr obj);
		public int MaximumHeight {
		
			get {
			
				return ewl_object_maximum_h_get(Raw);
			
			}
			
			set {
			
				ewl_object_maximum_h_set(Raw, value);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_object_preferred_inner_size_set(IntPtr obj, int w, int h);
		[DllImport("libewl")]
		static extern void ewl_object_preferred_inner_size_get(IntPtr obj, ref int w, ref int h);
		public int PreferredInnerSize {
		
			get {
			
				int w = 0, h = 0;
				ewl_object_preferred_inner_size_get(Raw, ref w, ref h);
				
				Console.WriteLine(w + " " + h);
			
				return 1;
			
			}
			
			set {
			
				int w = 150, h = 100;
				ewl_object_preferred_inner_size_set(Raw, w, h);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_object_preferred_inner_w_set(IntPtr obj, int w);
		[DllImport("libewl")]
		static extern int ewl_object_preferred_inner_w_get(IntPtr obj);
		public int PreferredInnerWidth {
		
			get {
			
				return ewl_object_preferred_inner_w_get(Raw);
			
			}
			
			set {
			
				ewl_object_preferred_inner_w_set(Raw, value);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_object_preferred_inner_h_set(IntPtr obj, int w);
		[DllImport("libewl")]
		static extern int ewl_object_preferred_inner_h_get(IntPtr obj);
		public int PreferredInnerHeight {
		
			get {
			
				return ewl_object_preferred_inner_h_get(Raw);
			
			}
			
			set {
			
				ewl_object_preferred_inner_h_set(Raw, value);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_object_alignment_set(IntPtr obj, uint align);
		[DllImport("libewl")]
		static extern uint ewl_object_alignment_get(IntPtr obj);
		public uint Alignment {
		
			get {
			
				return ewl_object_alignment_get(Raw);
			
			}
			
			set {
			
				ewl_object_alignment_set(Raw, value);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_object_place(IntPtr obj, int x, int y, int w, int h);
		public void Place(int x, int y, int w, int h) {
		
			ewl_object_place(Raw, x, y, w, h);
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_object_fill_policy_set(IntPtr obj, uint fill);
		[DllImport("libewl")]
		static extern uint ewl_object_fill_policy_get(IntPtr obj);
		public uint FillPolicy {
		
			get {
			
				return ewl_object_fill_policy_get(Raw);
			
			}
			
			set {
			
				ewl_object_fill_policy_set(Raw, value);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_object_padding_set(IntPtr obj, int l, int r, int t, int b);
		[DllImport("libewl")]
		static extern void ewl_object_padding_get(IntPtr obj, ref int l, ref int r, ref int t, ref int b);
		public int Padding {
		
			get {
			
				int l = 0, r = 0, t = 0, b = 0;
				ewl_object_padding_get(Raw, ref l, ref r, ref t, ref b);
				
				Console.WriteLine(l + " " + r + " " + t + " " + b);
				
				return 1;
			
			}
			
			set {
			
				int l = 0, r = 0, t = 0, b = 0;
				ewl_object_padding_set(Raw, l, r, t, b);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern int ewl_object_padding_top_get(IntPtr obj);
		public int TopPadding {
		
			get {
			
				return ewl_object_padding_top_get(Raw);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern int ewl_object_padding_bottom_get(IntPtr obj);
		public int BottomPadding {
		
			get {
			
				return ewl_object_padding_bottom_get(Raw);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern int ewl_object_padding_left_get(IntPtr obj);
		public int LeftPadding {
		
			get {
			
				return ewl_object_padding_left_get(Raw);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern int ewl_object_padding_right_get(IntPtr obj);
		public int RightPadding {
		
			get {
			
				return ewl_object_padding_right_get(Raw);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_object_insets_set(IntPtr obj, int l, int r, int t, int b);
		[DllImport("libewl")]
		static extern void ewl_object_insets_get(IntPtr obj, ref int l, ref int r, ref int t, ref int b);
		public int Insets {
		
			get {
			
				int l = 0, r = 0, t = 0, b = 0;
				ewl_object_insets_get(Raw, ref l, ref r, ref t, ref b);
				
				Console.WriteLine(l + " " + r + " " + t + " " + b);
				
				return 1;
			
			}
			
			set {
			
				int l = 0, r = 0, t = 0, b = 0;
				ewl_object_insets_set(Raw, l, r, t, b);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern int ewl_object_insets_top_get(IntPtr obj);
		public int TopInset {
		
			get {
			
				return ewl_object_insets_top_get(Raw);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern int ewl_object_insets_bottom_get(IntPtr obj);
		public int BottomInset {
		
			get {
		
				return ewl_object_insets_bottom_get(Raw);
				
			}
		
		}
		
		[DllImport("libewl")]
		static extern int ewl_object_insets_left_get(IntPtr obj);
		public int LeftInset {
		
			get {
			
				return ewl_object_insets_left_get(Raw);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern int ewl_object_insets_right_get(IntPtr obj);
		public int RightInset {
		
			get {
			
				return ewl_object_insets_right_get(Raw);
			
			}
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_object_flags_add(IntPtr obj, uint flags, uint mask);
		public void AddFlags(uint flags, uint mask) {
		
			ewl_object_flags_add(Raw, flags, mask);
		
		}
		
		[DllImport("libewl")]
		static extern void ewl_object_flags_remove(IntPtr obj, uint flags, uint mask);
		public void RemoveFlags(uint flags, uint mask) {
		
			ewl_object_flags_remove(Raw, flags, mask);
		
		}
		
		[DllImport("libewl")]
		static extern uint ewl_object_flags_has(IntPtr obj, uint flags, uint mask);
		public uint HasFlags(uint flags, uint mask) {
		
			return ewl_object_flags_has(Raw, flags, mask);
		
		}
		
		[DllImport("libewl")]
		static extern uint ewl_object_flags_get(IntPtr obj, uint mask);
		public uint GetFlags(uint mask) {
		
			return ewl_object_flags_get(Raw, mask);
		
		}
	
	}
	
}
