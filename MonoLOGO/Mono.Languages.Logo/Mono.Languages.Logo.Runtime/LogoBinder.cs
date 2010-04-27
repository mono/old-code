namespace Mono.Languages.Logo.Runtime {
	using System;
	using System.Reflection;
	using System.Globalization;
	
	public class LogoBinder : Binder {
		private LogoContext context = null; 

		private class ReorderState {
			public bool pass_context;
			public int vararg_count;

			public ReorderState (bool pass_context, int vararg_count) {
				this.pass_context = pass_context;
				this.vararg_count = vararg_count;
			}
		}

		public LogoBinder (LogoContext context) {
			this.context = context;
		}

		public LogoContext Context {
			get {
				return this.context;
			}
			set {
				this.context = value;
			}
		}

		private bool IsPassContext (MethodBase method) {
				object[] method_attrs = method.GetCustomAttributes (typeof (PassContextAttribute), true);
				return (method_attrs != null && method_attrs.Length > 0);
		}
		
		public override FieldInfo BindToField (BindingFlags bindingAttr, FieldInfo[] match, object value, CultureInfo culture) {
			throw new NotImplementedException ();
		}

		private bool CanConvertPrimitive (Type type_a, Type type_b) {
			return false;
			// throw new NotImplementedException ();
		}

		private int Distance (Type type_a, Type type_b) {
			if (type_a.Equals (type_b))
				return 0;

			if (type_a.IsPrimitive && type_b.IsPrimitive &&
				 CanConvertPrimitive (type_a, type_b)) {
				return 1;
			}
			
			int class_distance = 1;
			Type base_a = type_a.BaseType;
			while (base_a != null && !(base_a.Equals (typeof (object)))) {
				if (type_b.Equals (base_a))
					break;

				// FIXME: this needs to check which interfaces bind tighter
				Type[] interfaces = base_a.GetInterfaces ();
				foreach (Type iface in interfaces) {
					if (type_b.Equals (iface))
						return 1;
				}

				class_distance++;
				base_a = base_a.BaseType;
			}

			if (base_a == null || !(type_b.Equals (base_a))) {
				class_distance = -1;
			}

			return class_distance;
		}

		private int Signature (Type[] given, Type[] wanted) {
			if (given.Length != wanted.Length)
				return -1;
				
			int signature = 0;
			int i = 0;
			foreach (Type type in given) {
				int distance = Distance (type, wanted[i]);
				if (distance == -1)
					return -1;
				signature += distance;
				i++;
			}
			
			return signature;
		}

		public override MethodBase BindToMethod (BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] names, out object state) {
			Type[] types = new Type[args.Length];
			int i = 0;

			foreach (object arg in args) {
				if (arg == null)
					types[i] = typeof (object);
				else
					types[i] = arg.GetType ();
				
				i++;
			}

			int vararg_count;
			Type vararg_type;
			MethodBase method = SelectMethod (bindingAttr, match, types, modifiers, out vararg_count, out vararg_type);
			if (method == null) {
				state = null;
				return null;
			}
				
			bool pass_context = IsPassContext (method);
			if (!pass_context && vararg_count == -1) {
				state = null;
				return method;
			}
		
			object[] new_args;
			if (vararg_count != -1) {
				int new_args_length = args.Length - vararg_count + 1;
				int new_args_start = 0;
				if (pass_context) {
					new_args_length++;
					new_args_start = 1;
				}

				new_args = new object[new_args_length];
				Array.Copy (args, 0, new_args, new_args_start, new_args.Length - 1);
				Array varargs = Array.CreateInstance (vararg_type, vararg_count);
				Array.Copy (args, new_args.Length - 1, varargs, 0, varargs.Length);
				new_args[new_args.Length - 1] = varargs;

				if (pass_context) {
					new_args[0] = Context; 
				}
			} else {
				new_args = new object[args.Length + 1];
				Array.Copy (args, 0, new_args, 1, args.Length);
				new_args[0] = Context;
			}

			args = new_args;
			state = new ReorderState (pass_context, vararg_count);
			return method;
		}

		public override MethodBase SelectMethod (BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers) {
			int vararg_count;
			Type vararg_type;
			return SelectMethod (bindingAttr,match,types, modifiers, out vararg_count, out vararg_type);
		}
		
		private MethodBase SelectMethod (BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers, out int vararg_count, out Type vararg_type) {
			
			int min_signature = -1;
			int min_signature_idx = -1;
			int min_vararg_count = -1;
			Type min_vararg_type = null;

			int index = 0;
			foreach (MethodBase method in match) {
				bool pass_context = IsPassContext (method);
				int parm_types_offset = (pass_context) ? 1 : 0;
				
				ParameterInfo[] parms = method.GetParameters ();
				Type[] parm_types = new Type [types.Length];

				bool is_varargs = false;
				if (parms.Length > 0) {
					object[] attrs = parms[parms.Length - 1].GetCustomAttributes (typeof (System.ParamArrayAttribute), true);
					if (attrs != null && attrs.Length > 0) {
						is_varargs = true;
					}
				}

				int end = parms.Length - 1;
				int i = 0;
				foreach (ParameterInfo pinfo in parms) {
					if (pass_context && i == 0) {
						i++;
						continue;
					}

					if (is_varargs && i == end) {
						min_vararg_count = types.Length - i;
						min_vararg_type = pinfo.ParameterType.GetElementType ();
						for (int j = i; j < types.Length; j++) {
							parm_types[j - parm_types_offset] = min_vararg_type;
						}
					} else {
						parm_types[i - parm_types_offset] = pinfo.ParameterType;
					}
					i++;
				}

				int signature = Signature (types, parm_types);
				if (signature == 0) {
					vararg_count = min_vararg_count;
					vararg_type = min_vararg_type;
					return method;
				} else if (signature != -1) {
					if (min_signature_idx == -1 || signature < min_signature) {
						min_signature_idx = index;
						min_signature = signature;
					}
				}
				
				index++;
			}

			vararg_count = min_vararg_count;
			vararg_type = min_vararg_type;

			if (min_signature_idx != -1) 
				return match[min_signature_idx];
			else 
				return null;
		}

		public override object ChangeType (object value, Type type, CultureInfo culture) {
			return value;
			// throw new NotImplementedException ();
		}

		public override void ReorderArgumentArray (ref object[] args, object state) {
			if (state == null)
				return;

			ReorderState reorder = (ReorderState) state;
			int vararg_count = reorder.vararg_count;
			
			int args_count = args.Length;
			int args_start = 0;
			if (reorder.pass_context) {
				args_count--;
				args_start = 1;
			}
			
			object[] old_args;
			if (vararg_count != -1) {
				old_args = new object[args_count + vararg_count - 1];
				Array.Copy (args, args_start, old_args, 0, args_count);
				Array.Copy ((Array) args[args.Length - 1], 0, old_args, args.Length - 1, ((Array) args[args.Length - 1]).Length);
			} else {
				old_args = new object[args_count];	
				Array.Copy (args, args_start, old_args, 0, args_count);
			}

			args = old_args;
		}

		public override PropertyInfo SelectProperty (BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers) {
			throw new NotImplementedException ();
		}
	}
}

