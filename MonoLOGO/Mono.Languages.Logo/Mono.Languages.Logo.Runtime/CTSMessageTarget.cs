namespace Mono.Languages.Logo.Runtime {
	using System;
	using System.Collections;
	using System.Reflection;

	public class CTSMessageTarget : IMessageTarget, IMessageStore, ITypedMessageStore {
		private object obj;
		private Type obj_type;
		private bool wrap_obj;
		private static LogoBinder binder = new LogoBinder (null);

		public object TargetObject {
			get {
				return obj;
			}
		}

		private void Init (object obj) {
			if (obj is Type) {
				this.obj = null;
				this.obj_type = (Type) obj;
			} else {
				this.obj = obj;
				this.obj_type = obj.GetType ();
			}
		}

		public CTSMessageTarget (object obj, bool wrap_obj) {
			Init (obj);
			this.wrap_obj = wrap_obj;
		}
		
		public CTSMessageTarget (object obj) {
			Init (obj);
			this.wrap_obj = true;
		}

		// IMessageTarget
		public object SendMessage (LogoContext context, string message, ICollection arguments) {
			object[] args_array = new object[arguments.Count];
			
			int i = 0;
			foreach (object o in arguments) {
				if (o is CTSMessageTarget) {
					args_array[i] = ((CTSMessageTarget) o).TargetObject;
				} else {
					args_array[i] = o;
				}
				i++;
			}
			
			binder.Context = context;
			object target_obj = obj;
			object raw_result = obj_type.InvokeMember (message, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.InvokeMethod, binder, target_obj, args_array);

			if (!wrap_obj ||
			    raw_result == null ||
		       raw_result is IMessageTarget ||
				 raw_result.GetType ().IsPrimitive) {
				return raw_result;
			} else {
				return new CTSMessageTarget (raw_result);
			}
		}

		private MessageInfo[] ListMessages (string opt_filter) {
			ICollection method_list;
			if (obj == null) {
				ArrayList list = new ArrayList (obj_type.GetMethods ());
				list.InsertRange (list.Count, typeof (Type).GetMethods ());
				method_list = list;
			} else {
				method_list = obj_type.GetMethods ();
			}

			ArrayList info_list = new ArrayList ();
			Hashtable methods = new Hashtable (); 

			if (opt_filter != String.Empty) {
				opt_filter = opt_filter.ToLower ();
			}

			foreach (MethodInfo method in method_list)
			{
				string method_lower = method.Name.ToLower ();
				if (opt_filter != String.Empty && method.Name.ToLower () != opt_filter)
					continue;

				MessageInfo info = (MessageInfo) methods[method_lower];
				if (info == null) {
					info = new TypedMessageInfo ();
					info.message = method.Name;
					info.min_argc = -1;
					info.default_argc = -1;
					info.max_argc = 0;
					methods[method_lower] = info;
					info_list.Add (info);
				}

				ParameterInfo[] parms = method.GetParameters ();
				int count = (parms != null) ? parms.Length : 0;

				if (count > 0) {
					object[] attrs = parms[count - 1].GetCustomAttributes (typeof (System.ParamArrayAttribute), true);
					if (attrs != null && attrs.Length > 0) 
						info.max_argc = -1;
				}

				object[] method_attrs = method.GetCustomAttributes (typeof (PassContextAttribute), true);
				if (method_attrs != null && method_attrs.Length > 0) {
					count--;
				}
		
				if (info.min_argc == -1 || count <= info.min_argc) {
					info.min_argc = count;
					info.default_argc = count;
				}

				method_attrs = method.GetCustomAttributes (typeof (DefaultArgumentCountAttribute), true);
				if (method_attrs != null && method_attrs.Length > 0) {
					info.default_argc = ((DefaultArgumentCountAttribute) method_attrs[0]).DefaultCount;
				}
			
				if (info.max_argc != -1 && count > info.max_argc) {
					info.max_argc = count;
				}
			}

			MessageInfo[] ret = new MessageInfo[info_list.Count];
			info_list.CopyTo (ret, 0);
			return ret;
		}

		// IMessageStore
			
		public MessageInfo DescribeMessage (string message) {
			MessageInfo[] info = ListMessages (message);
			if (info.Length > 0) 
				return info[0];
			else
				return null;
		}

		public bool SupportsMessage (string message) {
			MethodInfo[] methods = obj_type.GetMethods ();
			foreach (MethodInfo method in methods) {
				if (String.Compare (method.Name, message, true) == 0) {
					return true;
				}
			}
			
			methods = typeof (Type).GetMethods ();
			foreach (MethodInfo method in methods) {
				if (String.Compare (method.Name, message, true) == 0) {
					return true;
				}
			}

			return false;
		}
		
		public MessageInfo[] SupportedMessages { 
			get {
				return ListMessages (String.Empty);
			}
		}

		// ITypedMessageStore

		public bool SupportsMessage (string message, Type[] args) {
			// FIXME: This is the lazy approach. It does not
			// compare against types, it only sees if there
			// is only one, unambiguous, message

			bool supports = false; 
			MethodInfo[] methods = obj_type.GetMethods ();
			foreach (MethodInfo method in methods) {
				if (String.Compare (method.Name, message, true) == 0) {
					if (supports)
						return false;
					else
						supports = true;
				}
			}
			
			methods = typeof (Type).GetMethods ();
			foreach (MethodInfo method in methods) {
				if (String.Compare (method.Name, message, true) == 0) {
					if (supports)
						return false;
					else
						supports = true;
				}
			}

			return supports;
		}

		public TypedMessageInfo DescribeMessage (string message, Type[] args) {
			return (TypedMessageInfo) DescribeMessage (message);
		}
	}
}

