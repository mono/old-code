/*
	EWL Spinner Class
	
	TODO: -
*/

namespace Enlightenment.Ewl {
   
	using System;
	using System.Runtime.InteropServices;
	using Enlightenment.Ewl.Event;
    
   	public class Spinner : Widget 
    {
		[DllImport(Library)]
		static extern IntPtr ewl_spinner_new();
		
		public Spinner(){
			objRaw = new HandleRef(this, ewl_spinner_new());
		}

		[DllImport(Library)]
		static extern void ewl_spinner_digits_set(IntPtr s, byte digits);
		byte Digits{
			set{ ewl_spinner_digits_set(Raw, value);}
		}

		[DllImport(Library)]
		static extern void ewl_spinner_step_set(IntPtr s, double step);
		double Step{
			set{ ewl_spinner_step_set(Raw, value);}
		}
				
		[DllImport(Library)]
		static extern double ewl_spinner_value_get(IntPtr s);
		[DllImport(Library)]
		static extern void ewl_spinner_value_set(IntPtr s, double v);
		double Value{
			get{ return ewl_spinner_value_get(Raw);}
			set{ ewl_spinner_value_set(Raw, value);}
		}
		
		[DllImport(Library)]
		static extern double ewl_spinner_min_val_get(IntPtr s);
		[DllImport(Library)]
		static extern void ewl_spinner_min_val_set(IntPtr s, double v);
		double Min{
			get{ return ewl_spinner_min_val_get(Raw);}
			set{ ewl_spinner_min_val_set(Raw, value);}
		}

		[DllImport(Library)]
		static extern double ewl_spinner_max_val_get(IntPtr s);
		[DllImport(Library)]
		static extern void ewl_spinner_max_val_set(IntPtr s, double v);
		double Max{
			get{ return ewl_spinner_max_val_get(Raw);}
			set{ ewl_spinner_max_val_set(Raw, value);}
		}		
	}
}
