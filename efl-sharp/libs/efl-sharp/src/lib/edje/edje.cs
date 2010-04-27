namespace Enlightenment.Edje 
{
   using System;
   using System.Runtime.InteropServices;
   using Enlightenment.Evas;
   
 public class Edje : Item
     {
	const string Library = "edje";										
		
	public enum MessageType
	  {
	     EDJE_MESSAGE_NONE = 0,
	       
	       EDJE_MESSAGE_SIGNAL = 1,
	       EDJE_MESSAGE_STRING = 2,
	       EDJE_MESSAGE_INT = 3,
	       EDJE_MESSAGE_FLOAT = 4,
	       
	       EDJE_MESSAGE_STRING_SET = 5,
	       EDJE_MESSAGE_INT_SET = 6,
	       EDJE_MESSAGE_FLOAT_SET = 7,
	       
	       EDJE_MESSAGE_STRING_INT = 8,
	       EDJE_MESSAGE_STRING_FLOAT = 9,
	       
	       EDJE_MESSAGE_STRING_INT_SET = 10,
	       EDJE_MESSAGE_STRING_FLOAT_SET = 11
	  };
	
	[DllImport(Library)]
	private extern static IntPtr edje_object_add(IntPtr evas);
	
      public Edje(Canvas c) : base(c)
	  {
	     objRaw = new HandleRef(this, edje_object_add(c.Raw));
	  }
	
	public Edje()
	  {
	  }      
	
	public Edje(IntPtr e)
	  {
	     objRaw = new HandleRef(this, e);
	     canvas = new Canvas(this.CanvasGetRaw());
	  }
	
	[DllImport(Library)]
	private extern static int edje_init();
	
	public static int Init()
	  {
	     return edje_init();
	  }
	
	[DllImport(Library)]
	private extern static int edje_shutdown();
	
	public static int Shutdown()
	  {
	     return edje_shutdown();
	  }
	
	[DllImport(Library)]
	private extern static void edje_frametime_set(double t);   
	
	[DllImport(Library)]
	private extern static double edje_frametime_get();
	
	public static double Frametime
	  {
	     get { return edje_frametime_get(); }
	     set { edje_frametime_set(value); }
	  }   
	
	[DllImport(Library)]
	private extern static void edje_freeze();
	
	public static void EFreeze()
	  {
	     edje_freeze();
	  }
	
	[DllImport(Library)]
	private extern static  void edje_thaw();
	
	public static void EThaw()
	  {
	     edje_thaw();
	  }
	
	[DllImport(Library)]
	private extern static void edje_fontset_append_set(string fonts);
	
	public static void FontsetAppendSet(string fonts)
	  {
	     edje_fontset_append_set(fonts);
	  }
	
	[DllImport(Library)]
	private extern static string edje_fontset_append_get();
	
	public static string FontsetAppengGet()
	  {
	     return edje_fontset_append_get();
	  }
	
	[DllImport(Library)]
	private extern static IntPtr edje_file_collection_list(string file);
	
	public static IntPtr FileCollectionList(string file)
	  {
	     return edje_file_collection_list(file);
	  }
	
	[DllImport(Library)]
	private extern static void edje_file_collection_list_free(IntPtr list);   
	
	public static void FileCollectionListFree(IntPtr list)
	  {
	     edje_file_collection_list_free(list);
	  }
	
	[DllImport(Library)]
	private extern static string edje_file_data_get(string file, string key);   
	
	public static string FileDataGet(string file, string key)
	  {
	     return edje_file_data_get(file, key);
	  }
	
	[DllImport(Library)]
	private extern static void edje_file_cache_set(int count);
	
	public static void FileCacheSet(int count)
	  {
	     edje_file_cache_set(count);
	  }

	[DllImport(Library)]
	private extern static int edje_file_cache_get();
	
	public static int FileCacheGet()
	  {
	     return edje_file_cache_get();
	  }

	[DllImport(Library)]
	private extern static void edje_file_cache_flush();
	
	public static void FileCacheFlush()
	  {
	     edje_file_cache_flush();
	  }
	
	
	[DllImport(Library)]
	private extern static void edje_collection_cache_set(int count);
	
	[DllImport(Library)]
	private extern static int edje_collection_cache_get();	
	
	public static int CollectionCache
	  {
	     get { return edje_collection_cache_get(); }
	     set { edje_collection_cache_set(value); }
	  }
	
	[DllImport(Library)]
	private extern static void edje_collection_cache_flush();
	
	public static void CollectionCacheFlush()
	  {
	     edje_collection_cache_flush();
	  }
	
	[DllImport(Library)]
	private extern static void edje_color_class_set(string color_class, int r, int g, int b, int a, int r2, int g2, int b2, int a2, int r3, int g3, int b3, int a3);	

	public static void EColorClassSet(string color_class, int r, int g, int b, int a, int r2, int g2, int b2, int a2, int r3, int g3, int b3, int a3)
	  {
	     edje_color_class_set(color_class, r, g, b, a, r2, g2, b2, a2, r3, g3, b3, a3);
	  }

	[DllImport(Library)]
	private extern static void edje_text_class_set(string text_class, string font, int size);
	
	public static void ETextClassSet(string text_class, string font, int size)
	  {
	     edje_text_class_set(text_class, font, size);
	  }

	[DllImport(Library)]
	private extern static void edje_extern_object_min_size_set(IntPtr obj, int minw, int minh);
	
	public void ExternObjectMinSizeSet(Evas.Item o, int minw, int minh)
	  {
	     edje_extern_object_min_size_set(o.Raw, minw, minh);
	  }

	[DllImport(Library)]
	private extern static void edje_extern_object_max_size_set(IntPtr obj, int maxw, int maxh);
	
	public void ExternObjectMaxSizeSet(Evas.Item o, int maxw, int maxh)
	  {
	     edje_extern_object_max_size_set(o.Raw, maxw, maxh);
	  }
	
	[DllImport(Library)]
	private extern static string edje_object_data_get(IntPtr obj, string key);
	
	public string DataGet(string key)
	  {
	     return edje_object_data_get(Raw, key);
	  }  
	
	[DllImport(Library)]
	private extern static int edje_object_file_set(IntPtr obj, string file, string part);
	
	public int FileSet(string file, string part)
	  {
	     return edje_object_file_set(Raw, file, part);
	  }

	[DllImport(Library)]
	private extern static void edje_object_file_get(IntPtr obj, out string file, out string part);
	
	public void FileGet(out string file, out string part)
	  {
	     edje_object_file_get(Raw, out file, out part);
	  }
	
	[DllImport(Library)]
	private extern static int edje_object_load_error_get(IntPtr obj);
	
	public int LoadErrorGet()
	  {
	     return edje_object_load_error_get(Raw);
	  }

	public delegate void edje_object_signal_callback (IntPtr data, IntPtr obj, string emission, string source);
	[DllImport(Library)]
	private extern static void edje_object_signal_callback_add(IntPtr obj, string emission, string source, edje_object_signal_callback func, IntPtr data);	
	
	public void SignalCallbackAdd(string emission, string source, edje_object_signal_callback func, object data)
	  {
	     IntPtr p = new IntPtr(dataptrs.Count);
	     /* FIXME: there no way to remove this safely yet */
	     callbacks[func] = func;
	     dataptrs[p] = data;
	     edje_object_signal_callback_add(Raw, emission, source, func, p);
	  }

	[DllImport(Library)]
	private extern static IntPtr edje_object_signal_callback_del(IntPtr obj, string emission, string source, edje_object_signal_callback func);	
	
	public IntPtr SignalCallbackDel(string emission, string source, edje_object_signal_callback func)
	  {
	     return edje_object_signal_callback_del(Raw, emission, source, func);
	  }

	[DllImport(Library)]
	private extern static void edje_object_signal_emit(IntPtr obj, string emission, string source);
	
	public void SignalEmit(string emission, string source)
	  {
	     edje_object_signal_emit(Raw, emission, source);
	  }

	[DllImport(Library)]
	private extern static void edje_object_play_set(IntPtr obj, int play);
	
	[DllImport(Library)]
	private extern static int edje_object_play_get(IntPtr obj);
	
	public int Play
	  {
	     get { return edje_object_play_get(Raw); }
	     set { edje_object_play_set(Raw, value); }
	  }
	
	[DllImport(Library)]
	private extern static void edje_object_animation_set(IntPtr obj, int on);
	
	[DllImport(Library)]
	private extern static int edje_object_animation_get(IntPtr obj);	
	
	public int Animation
	  {
	     get { return edje_object_animation_get(Raw); }
	     set {edje_object_animation_set(Raw, value); }
	  }
		
	
	[DllImport(Library)]
	private extern static int edje_object_freeze(IntPtr obj);
	
	public int Freeze()
	  {
	     return edje_object_freeze(Raw);
	  }
	
	[DllImport(Library)]
	private extern static int edje_object_thaw(IntPtr obj);
		
	public int Thaw()
	  {
	     return edje_object_thaw(Raw);
	  }
	
	[DllImport(Library)]
	private extern static void edje_object_color_class_set(IntPtr obj, string color_class, int r, int g, int b, int a, int r2, int g2, int b2, int a2, int r3, int g3, int b3, int a3);
	
	public void ColorClassSet(string color_class, int r, int g, int b, int a, int r2, int g2, int b2, int a2, int r3, int g3, int b3, int a3)
	  {
	     edje_object_color_class_set(Raw, color_class, r, g, b, a, r2, g2, b2, a2, r3, g3, b3, a3);
	  }

	[DllImport(Library)]
	private extern static void edje_object_text_class_set(IntPtr obj, string text_class, string font, int size);
	
	public void TextClassSet(string text, string font, int size)
	  {
	     edje_object_text_class_set(Raw, text, font, size);
	  }
	
	[DllImport(Library)]
	private extern static void edje_object_size_min_get(IntPtr obj, out int minw, out int minh);
	
	public void SizeMinGet(out int minw, out int minh)
	  {
	     edje_object_size_min_get(Raw, out minw, out minh);
	  }
	
	[DllImport(Library)]
	private extern static void edje_object_size_max_get(IntPtr obj, out int maxw, out int maxh);
	
	public void SizeMaxGet(out int maxw, out int maxh)
	  {
	     edje_object_size_max_get(Raw, out maxw, out maxh);
	  }
	
	[DllImport(Library)]
	private extern static void edje_object_calc_force(IntPtr obj);
	
	public void CalcForce()
	  {
	     edje_object_calc_force(Raw);
	  }
	
	[DllImport(Library)]
	private extern static void edje_object_size_min_calc(IntPtr obj, out int minw, out int minh);
	
	public void SizeMinCalc(out int minw, out int minh)
	  {
	     edje_object_size_min_calc(Raw, out minw, out minh);
	  }
	
	[DllImport(Library)]
	private extern static int edje_object_part_exists(IntPtr obj, string part);
	
	public int PartExists(string part)
	  {
	     return edje_object_part_exists(Raw, part);
	  }
	
	[DllImport(Library)]
	private extern static void edje_object_part_geometry_get(IntPtr obj, string part, out int x, out int y, out int w, out int h);
	
	public void PartGeometryGet(string part, out int x, out int y, out int w, out int h)
	  {
	     edje_object_part_geometry_get(Raw, part, out x, out y, out w, out h);
	  }

	public delegate void edje_object_text_change_cb(IntPtr data, IntPtr obj, string part);
	[DllImport(Library)]
	private extern static void edje_object_text_change_cb_set(IntPtr obj, edje_object_text_change_cb func, IntPtr data);
	
	public void TextChangeCallbackSet(edje_object_text_change_cb func, IntPtr data)
	  {
	     IntPtr p = new IntPtr(dataptrs.Count);
	     /* FIXME: there no way to remove this safely yet */
	     callbacks[func] = func;
	     dataptrs[p] = data;	
	     edje_object_text_change_cb_set(Raw, func, data);
	  }
	
	[DllImport(Library)]
	private extern static void edje_object_part_text_set(IntPtr obj, string part, string text);
	
	public void PartTextSet(string part, string text)
	  {
	     edje_object_part_text_set(Raw, part, text);
	  }

	[DllImport(Library)]
	private extern static string edje_object_part_text_get(IntPtr obj, string part);
		
	public string PartTextGet(string part)
	  {
		  string str = String.Copy(edje_object_part_text_get(Raw, part));
		  return str;
	  }
	
	[DllImport(Library)]
	private extern static void edje_object_part_swallow(IntPtr obj, string part, IntPtr obj_swallow);	
	
	public void PartSwallow(string part, Evas.Item obj_swallow)
	  {
	     edje_object_part_swallow(Raw, part, obj_swallow.Raw);
	  }
	
	[DllImport(Library)]
	private extern static void edje_object_part_unswallow(IntPtr obj, IntPtr obj_swallow);
	
	public void PartUnswallow(Evas.Item obj_swallow)
	  {
	     edje_object_part_unswallow(Raw, obj_swallow.Raw);
	  }
	
	[DllImport(Library)]
	private extern static IntPtr edje_object_part_swallow_get(IntPtr obj, string part);
	
	public IntPtr PartSwallowGet(string part)
	  {
	     return edje_object_part_swallow_get(Raw, part);
	  }
	
	[DllImport(Library)]
	private extern static string edje_object_part_state_get(IntPtr obj, string part, out double val_ret);
	
	public string PartStateGet(string part, out double val_ret)
	  {
	     return edje_object_part_state_get(Raw, part, out val_ret);
	  }
	
	[DllImport(Library)]
	private extern static int edje_object_part_drag_dir_get(IntPtr obj, string part);
	
	public int PartDragDirGet(string part)
	  {
	     return edje_object_part_drag_dir_get(Raw, part);
	  }
	
	[DllImport(Library)]
	private extern static void edje_object_part_drag_value_set(IntPtr obj, string part, double dx, double dy);
	
	public void PartGradValueSet(string part, double dx, double dy)
	  {
	     edje_object_part_drag_value_set(Raw, part, dx, dy);
	  }
	
	[DllImport(Library)]
	private extern static void edje_object_part_drag_value_get(IntPtr obj, string part, out double dx, out double dy);
	
	public void PartDragValueGet(string part, out double dx, out double dy)
	  {
	     edje_object_part_drag_value_get(Raw, part, out dx, out dy);
	  }
	
	[DllImport(Library)]
	private extern static void edje_object_part_drag_size_set(IntPtr obj, string part, double dw, double dy);	
	
	public void PartDragSizeSet(string part, double dw, double dh)
	  {
	     edje_object_part_drag_size_set(Raw, part, dw, dh);
	  }
	
	[DllImport(Library)]
	private extern static void edje_object_part_drag_size_get(IntPtr obj, string part, out double dx, out double dy);
	
	public void PartDragSizeGet(string part, out double dw, out double dh)
	  {
	     edje_object_part_drag_size_get(Raw, part, out dw, out dh);
	  }
	
	[DllImport(Library)]
	private extern static void edje_object_part_drag_step_set(IntPtr obj, string part, double dx, double dy);
	
	public void PartDragStepSet(string part, double dx, double dy)
	  {
	     edje_object_part_drag_step_set(Raw, part, dx, dy);
	  }
	
	[DllImport(Library)]
	private extern static void edje_object_part_drag_step_get(IntPtr obj, string part, out double dx, out double dy);
	
	public void PartDragStepGet(string part, out double dx, out double dy)
	  {
	     edje_object_part_drag_step_get(Raw, part, out dx, out dy);
	  }
	
	[DllImport(Library)]
	private extern static void edje_object_part_drag_page_set(IntPtr obj, string part, double dx, double dy);
	
	public void PartDragPageSet(string part, double dx, double dy)
	  {
	     edje_object_part_drag_page_set(Raw, part, dx, dy);
	  }
	
	[DllImport(Library)]
	private extern static void edje_object_part_drag_page_get(IntPtr obj, string part, out double dx, out double dy);
	
	public void PartDragPageGet(string part, out double dx, out double dy)
	  {
	     edje_object_part_drag_page_get(Raw, part, out dx, out dy);
	  }   
	
	[DllImport(Library)]
	private extern static void edje_object_part_drag_step(IntPtr obj, string part, double dx, double dy);
	
	public void PartDragStep(string part, double dx, double dy)
	  {
	     edje_object_part_drag_step(Raw, part, dx, dy);
	  }
	
	[DllImport(Library)]
	private extern static void edje_object_part_drag_page(IntPtr obj, string part, double dx, double dy);
	
	public void PartDragPage(string part, double dx, double dy)
	  {
	     edje_object_part_drag_page(Raw, part, dx, dy);
	  }
	
	[DllImport(Library)]
	private extern static void edje_object_message_send(IntPtr obj, MessageType type, int id, IntPtr msg);
	
	public void MessageSend(MessageType type, int id, object msg)
	  {
	     IntPtr p = new IntPtr(dataptrs.Count);
	     dataptrs[p] = msg;
	     edje_object_message_send(Raw, type, id, p);
	  }
	
	public delegate void edje_object_message_handler(IntPtr data, IntPtr obj, MessageType type, int id, IntPtr msg);
	[DllImport(Library)]     
	private extern static void edje_object_message_handler_set(IntPtr obj, edje_object_message_handler func, object data);
	
	public void MessageHandlerSet(edje_object_message_handler func, object data)
	  {
	     IntPtr p = new IntPtr(dataptrs.Count);
	     /* FIXME: there no way to remove this safely yet */
	     callbacks[func] = func;
	     dataptrs[p] = data;
	     edje_object_message_handler_set(Raw, func, data);
	  }
	
	[DllImport(Library)]
	private extern static void edje_object_message_signal_process(IntPtr obj);
	
	public void MessageSignalProcess()
	  {
	     edje_object_message_signal_process(Raw);
	  }
	
	[DllImport(Library)]
	private extern static void edje_message_signal_process();
	
	public void EMessageSignalPrcess()
	  {
	     edje_message_signal_process();
	  }
	
	
	
	~Edje()
	  {
	  }  
     }
}
