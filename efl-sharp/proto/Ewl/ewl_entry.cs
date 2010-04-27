namespace Ewl {

using System;
using System.Runtime.InteropServices;

	public delegate void ChangedEventHandler(IntPtr w, IntPtr evnt, IntPtr data);
	
	public class Entry : Widget {
		HandleRef objRaw;
		
		[DllImport("libewl")]
		static extern int ewl_callback_append(IntPtr wid, int i, PrivEventHandler de, IntPtr data);
		
		[DllImport("libewl")]
		static extern void ewl_callback_del_with_data(IntPtr wid, int i, PrivEventHandler de, IntPtr data);
		
		//Events Section
		EwlEventHandler pChanged;
		
		private void entryChanged(IntPtr w, IntPtr evnt, object data){
			EwlEventHandler callback = (EwlEventHandler)data;
			Console.WriteLine("Debug!");
			pChanged(this, evnt);
			callback(this, evnt);
		}
		
		public event EwlEventHandler ChangedEvent {
		
			add {
				PrivEventHandler tmp = new PrivEventHandler(entryChanged);
				pChanged = value;
				ewl_callback_append(Raw, (int)EventType.VALUE_CHANGED, tmp , new IntPtr());
				System.GC.KeepAlive(tmp);
			
			}
			
			remove {
			
				ewl_callback_del_with_data(Raw, (int)EventType.VALUE_CHANGED, new PrivEventHandler(entryChanged), new IntPtr());
			}
		
		}
		
		public override IntPtr Raw {
		
			get {
			
				return this.objRaw.Handle;
				//return mRaw;
			}
		
		}
		
		[DllImport("libewl")]
		static extern IntPtr ewl_entry_new(string text);
		
		public Entry() {
			objRaw = new HandleRef(this, ewl_entry_new(string.Empty));
		}
		
		public Entry(string text) {
			objRaw = new HandleRef(this, ewl_entry_new(text));
		}
		
		[DllImport("libewl")]
		static extern void ewl_entry_text_set(IntPtr ent, string text);
		[DllImport("libewl")]
		static extern string ewl_entry_text_get(IntPtr ent);
		
		public string Text{
			set{
				//strText = value;
				ewl_entry_text_set(Raw, value);
			}
			
			get{
				return ewl_entry_text_get(Raw);
			}
		}
	
	}

}
