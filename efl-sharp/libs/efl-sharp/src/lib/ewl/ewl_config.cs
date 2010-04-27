namespace Enlightenment.Ewl {
   
	using System;
	using System.Runtime.InteropServices;
	
	class Config
	{
		const string Library = "ewl";
		[DllImport(Library)]
		static extern int ewl_config_init();
		
		public Config()
		{
		}

		[DllImport(Library)]
		static extern int ewl_config_str_set(string key, string v);
		[DllImport(Library)]
		static extern string ewl_config_str_get(string key);
		[DllImport(Library)]
		static extern int ewl_config_int_set(string key, int v);
		[DllImport(Library)]
		static extern int ewl_config_int_get(string key);
		[DllImport(Library)]
		static extern int ewl_config_float_set(string key, float v);
		[DllImport(Library)]
		static extern float ewl_config_float_get(string key);
								
	}
}
