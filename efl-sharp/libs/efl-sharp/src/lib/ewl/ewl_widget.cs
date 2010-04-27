namespace Enlightenment.Ewl 
{

   using System;
   using System.Runtime.InteropServices;
   using Enlightenment.Ewl.Event;
   
 public class Widget : Ewl.Item 
     {
	
	protected EventData[] events = new EventData[28];
	
	
	[DllImport(Library)]
	  static extern int ewl_widget_init(IntPtr wid, string appearance);
	public int Init(string appearance) {
	   
	   return ewl_widget_init(Raw, appearance);
	   
	}
	
	[DllImport(Library)]
	  static extern void ewl_widget_reparent(IntPtr wid);
	public void ReParent() {
	   
	   ewl_widget_reparent(Raw);
	   
	}
	
	[DllImport(Library)]
	  static extern void ewl_widget_realize(IntPtr wid);
	public void Realize() {
	   
	   ewl_widget_realize(Raw);
	   
	}
	
	[DllImport(Library)]
	  static extern void ewl_widget_unrealize(IntPtr wid);
	public void UnRealize() {
	   
	   ewl_widget_unrealize(Raw);
	   
	}
	
	[DllImport(Library)]
	  static extern void ewl_widget_show(IntPtr wid);
	public void Show() {
	   
	   ewl_widget_show(Raw);
	   
	}
	
	[DllImport(Library)]
	  static extern void ewl_widget_hide(IntPtr wid);
	public void Hide() {
	   
	   ewl_widget_hide(Raw);
	   
	}
	
	[DllImport(Library)]
	  static extern void ewl_widget_destroy(IntPtr wid);
	public void Destroy() {
	   
	   ewl_widget_destroy(Raw);
	   
	}
	
	[DllImport(Library)]
	  static extern void ewl_widget_configure(IntPtr wid);
	public void Configure() {
	   
	   ewl_widget_configure(Raw);
	   
	}
	
	[DllImport(Library)]
	  static extern void ewl_widget_theme_update(IntPtr wid);
	public void UpdateTheme() {
	   
	   ewl_widget_theme_update(Raw);
	   
	}
	
	[DllImport(Library)]
	  static extern void ewl_widget_state_set(IntPtr wid, string state);
	public string State {
	   
	   set {
	      
	      ewl_widget_state_set(Raw, value);
	      
	   }
	   
	}
	
	[DllImport(Library)]
	  static extern void ewl_widget_appearance_set(IntPtr wid, string appearance);
	[DllImport(Library)]
	  static extern string ewl_widget_appearance_get(IntPtr wid);
	public string Appearance {
	   
	   get {
	      
	      return ewl_widget_appearance_get(Raw);
	      
	   }
	   
	   set {
	      
	      ewl_widget_appearance_set(Raw, value);
	      
	   }
	   
	}
	
	[DllImport(Library)]
	  static extern void ewl_widget_inherit(IntPtr wid, string type);
	public void Inherit(string type) {
	   
	   ewl_widget_inherit(Raw, type);
	   
	}
	
	[DllImport(Library)]
	  static extern uint ewl_widget_type_is(IntPtr wid, string type);
	public bool IsType(string type) {
	   
	   if (ewl_widget_type_is(Raw, type) > 0) return true;
	   
	   return false;
	   
	}
	
	[DllImport(Library)]
	  static extern void ewl_widget_parent_set(IntPtr wid, IntPtr parent);
	public Widget Parent {
	   
	   set {
	      
	      ewl_widget_parent_set(Raw, value.Raw);
	      
	   }
	   
	}
	
	[DllImport(Library)]
	  static extern void ewl_widget_enable(IntPtr wid);
	public void Enable() {
	   
	   ewl_widget_enable(Raw);
	   
	}
	
	[DllImport(Library)]
	  static extern void ewl_widget_disable(IntPtr wid);
	public void Disable() {
	   
	   ewl_widget_disable(Raw);
	   
		}
	
	[DllImport(Library)]
	  static extern void ewl_widget_print_tree(IntPtr wid);
	public void PrintTree() {
	   
	   ewl_widget_print_tree(Raw);
	   
	}
	
	[DllImport(Library)]
	  static extern void ewl_widget_print(IntPtr wid);
	public void Print() {
	   
	   ewl_widget_print(Raw);
	   
	}
	
	[DllImport(Library)]
	  static extern int ewl_widget_layer_sum_get(IntPtr wid);
	public int LayerSum {
	   
	   get {
	      
	      return ewl_widget_layer_sum_get(Raw);
	      
	   }
	   
	}
	
	[DllImport(Library)]
	  static extern void ewl_widget_layer_set(IntPtr wid, int layer);
	[DllImport(Library)]
	  static extern int ewl_widget_layer_get(IntPtr wid);
	public int Layer {
	   
	   get {
	      
	      return ewl_widget_layer_get(Raw);
	      
	   }
	   
	   set {
	      
	      ewl_widget_layer_set(Raw, value);
	      
	   }
	   
	}
	
	[DllImport(Library)]
	  static extern void ewl_widget_internal_set(IntPtr wid, uint val);
	public uint Internal {
	   
	   set {
	      
	      ewl_widget_internal_set(Raw, value);
	      
	   }
	   
	}
	
	[DllImport(Library)]
	  static extern uint ewl_widget_internal_is(IntPtr wid);
	public bool IsInternal {
	   
	   get {
	      
	      if (ewl_widget_internal_is(Raw) > 0) return true;
	      
	      return false;
	      
	   }
	   
	}
	
	[DllImport(Library)]
	  static extern void ewl_widget_clipped_set(IntPtr wid, uint val);
	public uint Clipped {
	   
	   set {
	      
	      ewl_widget_clipped_set(Raw, value);
	      
	   }
	   
	}
	
	[DllImport(Library)]
	  static extern uint ewl_widget_clipped_is(IntPtr wid);
	public bool IsClipped {
	   
	   get {
	      
	      if (ewl_widget_clipped_is(Raw) > 0) return true;
	      
	      return false;
	      
	   }
	   
	}
	
	[DllImport(Library)]
	  static extern void ewl_widget_focus_send(IntPtr wid);
	public void Focus() {
	   
	   ewl_widget_focus_send(Raw);
	   
	}
	
	[DllImport(Library)]
	  static extern IntPtr ewl_widget_focused_get();
	public static IntPtr GetFocused {
	   
	   get {
	      
	      return ewl_widget_focused_get();
	      
	   }
	   
	}
	
	public bool Focused {
	   
	   get {
	      
	      if (ewl_widget_focused_get() == Raw) return true;
	      
	      return false;
	      
	   }
	   
	}
	
	[DllImport(Library)]
	  static extern void ewl_widget_tab_order_push(IntPtr wid);
	public void TabOrderPush() {
	   
	   ewl_widget_tab_order_push(Raw);
	   
	}
	
     }   
}
