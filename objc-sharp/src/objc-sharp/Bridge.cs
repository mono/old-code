#define DEBUG

using System;
using System.Threading;
using System.Reflection;
using System.Collections;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace ObjCSharp {
	public class Bridge {
		protected static IDictionary RegisteredClasses = new Hashtable ();
		protected static IDictionary ClassesRegistered = new Hashtable ();
		protected static IDictionary ManagedInstances = new Hashtable ();
		protected static IDictionary NativeInstances = new Hashtable ();
		protected static IDictionary LoadedAssemblies = new Hashtable ();

		protected static IDictionary NativeClasses = new Hashtable ();

		private static IMPDelegate implement_method;
		private static IMPDelegate implement_static_method;
		private static IMPDelegate construct_object;

		public delegate IntPtr IMPDelegate (IntPtr cls, IntPtr sel, VarargStack stack);

		private static AssemblyBuilder assembly_builder;
		private static AssemblyName assembly_name;
		private static ModuleBuilder module_builder;

		public static void SetupBridge () {
			implement_method = new IMPDelegate (ImplementMethod);
			implement_static_method = new IMPDelegate (ImplementStaticMethod);
			construct_object = new IMPDelegate (ConstructObject);
			setupDelegate (implement_method);
			setupDelegate (implement_static_method);
			setupDelegate (construct_object);

			AppDomain.CurrentDomain.TypeResolve += new ResolveEventHandler(TypeResolver);
//			Mach.InstallExceptionHandler ();

			assembly_name = new AssemblyName ();
			assembly_name.Name = "ObjCSharp";
			assembly_builder = AppDomain.CurrentDomain.DefineDynamicAssembly (assembly_name, AssemblyBuilderAccess.Run);
			module_builder = assembly_builder.DefineDynamicModule (assembly_name.Name);
		}

		private static IntPtr MethodInfoToFtnptr (MethodInfo method) {
			if (module_builder.Assembly.GetType ("BridgeHelpers" + method.ToString ()) != null)
				return (IntPtr) module_builder.Assembly.GetType ("BridgeHelpers" + method.ToString ()).InvokeMember ("InternalMethodInfoToFtnPtr", BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public, null, null, new object[] { });

			TypeBuilder ftn_converter_builder = module_builder.DefineType ("BridgeHelpers" + method.ToString (), TypeAttributes.Public);
			MethodBuilder ftn_method = ftn_converter_builder.DefineMethod ("InternalMethodInfoToFtnPtr", MethodAttributes.Public | MethodAttributes.Static, typeof (IntPtr), new Type[] { }); 
			ILGenerator il_generator = ftn_method.GetILGenerator ();
			
			il_generator.Emit (OpCodes.Ldftn, method);
			il_generator.Emit (OpCodes.Ret);
			
			Type ftn_converter = ftn_converter_builder.CreateType ();
			return (IntPtr) ftn_converter.InvokeMember ("InternalMethodInfoToFtnPtr", BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public, null, null, new object[] { });
		}

		private static IntPtr ImplementStaticMethod (IntPtr cls, IntPtr sel, VarargStack stack) {
			try {
				if (!ClassesRegistered.Contains (cls)) 
					return IntPtr.Zero;
				
				ArrayList arguments = new ArrayList ();
				Type type = (Type) ClassesRegistered [cls];
				string selector = Marshal.PtrToStringAuto (sel);                                                      
				Type [] argumentTypes = ArgumentTypesForCall (class_getClassMethod (cls, sel));
	
				MethodInfo method = type.GetMethod (SelectorToMethod (selector, type), BindingFlags.Static | BindingFlags.Public, null, argumentTypes, null);
	
				unsafe {
					int argptr = (int)&sel+4;
					foreach (ParameterInfo parameter in method.GetParameters ()) {
						if (parameter.ParameterType.IsPrimitive || parameter.ParameterType.IsValueType) {
							arguments.Add (Marshal.PtrToStructure ((IntPtr)argptr, parameter.ParameterType));
						} else {
							if (parameter.ParameterType == typeof (string)) 
								arguments.Add (Marshal.PtrToStringAuto (Marshal.ReadIntPtr ((IntPtr)argptr)));
							else {
								IntPtr objectptr = Marshal.ReadIntPtr ((IntPtr)argptr);
								if (ManagedInstances.Contains (objectptr))
									arguments.Add (ManagedInstances [objectptr]);
								else {
									IntPtr objccls = (IntPtr) ObjCMessaging.objc_msgSend (objectptr, "class", typeof (IntPtr));
									if (!NativeClasses.Contains (objccls)) 
										NativeClasses [objccls] = AddTypeForClass ((string) ObjCMessaging.objc_msgSend ((IntPtr) ObjCMessaging.objc_msgSend (objccls, "className", typeof (IntPtr)), "cString", typeof (string)));
	
									object o = ((Type) NativeClasses [objccls]).GetConstructor (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type [] { typeof (IntPtr) }, null).Invoke (new object [] { objectptr });
									ManagedInstances [objectptr] = o;
									NativeInstances [o] = objectptr;
									arguments.Add (o);
								}  
							}
						}
						argptr += Marshal.SizeOf (typeof (IntPtr));
					}
				}
	
				object return_value = type.InvokeMember (SelectorToMethod (selector, type), BindingFlags.Static | BindingFlags.InvokeMethod, null, null, (object [])arguments.ToArray (typeof (object)));
				return ManagedToNative (return_value);
			} catch (TargetInvocationException e) {
				IntPtr nsex = objc_getClass ("NSException");
				IntPtr nsstr = objc_getClass ("NSString");
				IntPtr nm = (IntPtr) ObjCMessaging.objc_msgSend (nsstr, "stringWithUTF8String:", typeof (IntPtr), typeof (string), e.InnerException.GetType ().ToString ());
				IntPtr rsn = (IntPtr) ObjCMessaging.objc_msgSend (nsstr, "stringWithUTF8String:", typeof (IntPtr), typeof (string), e.InnerException.ToString ());
				IntPtr ex = (IntPtr) ObjCMessaging.objc_msgSend (nsex, "exceptionWithName:reason:userInfo:", typeof (IntPtr), typeof (IntPtr), nm, typeof (IntPtr), rsn, typeof (IntPtr), IntPtr.Zero);
				ObjCMessaging.objc_msgSend (ex, "raise", typeof (void));

				throw new Exception ("ImplementMethod post NSException should never be reached");
			} catch (Exception e) {
				IntPtr nsex = objc_getClass ("NSException");
				IntPtr nsstr = objc_getClass ("NSString");
				IntPtr nm = (IntPtr) ObjCMessaging.objc_msgSend (nsstr, "stringWithUTF8String:", typeof (IntPtr), typeof (string), e.GetType ().ToString ());
				IntPtr rsn = (IntPtr) ObjCMessaging.objc_msgSend (nsstr, "stringWithUTF8String:", typeof (IntPtr), typeof (string), e.ToString ());
				IntPtr ex = (IntPtr) ObjCMessaging.objc_msgSend (nsex, "exceptionWithName:reason:userInfo:", typeof (IntPtr), typeof (IntPtr), nm, typeof (IntPtr), rsn, typeof (IntPtr), IntPtr.Zero);
				ObjCMessaging.objc_msgSend (ex, "raise", typeof (void));

				throw new Exception ("ImplementMethod post NSException should never be reached");
			}
		}

		private static IntPtr ImplementMethod (IntPtr cls, IntPtr sel, VarargStack stack) {
			try {
				if (!ManagedInstances.Contains (cls)) 
					return IntPtr.Zero;
				
				ArrayList arguments = new ArrayList ();
				object target = (object) ManagedInstances [cls];
				Type type = target.GetType ();
				Type [] argumentTypes = ArgumentTypesForCall (class_getInstanceMethod ((IntPtr) ObjCMessaging.objc_msgSend (cls, "class", typeof (IntPtr)), sel));
				
				string selector = Marshal.PtrToStringAuto (sel);                                                      
				MethodInfo method = type.GetMethod (SelectorToMethod (selector, type), BindingFlags.Instance | BindingFlags.Public, null, argumentTypes, null);
	
				unsafe {
					int argptr = (int)&sel+4;
					foreach (ParameterInfo parameter in method.GetParameters ()) {
						if (parameter.ParameterType.IsPrimitive || parameter.ParameterType.IsValueType) {
							arguments.Add (Marshal.PtrToStructure ((IntPtr)argptr, parameter.ParameterType));
						} else {
							if (parameter.ParameterType == typeof (string)) 
								arguments.Add (Marshal.PtrToStringAuto (Marshal.ReadIntPtr ((IntPtr)argptr)));
							else {
								IntPtr objectptr = Marshal.ReadIntPtr ((IntPtr)argptr);
								if (objectptr == IntPtr.Zero)
									arguments.Add (null);
								else if (ManagedInstances.Contains (objectptr))
									arguments.Add (ManagedInstances [objectptr]);
								else {
									IntPtr objccls = (IntPtr) ObjCMessaging.objc_msgSend (objectptr, "class", typeof (IntPtr));
									if (!NativeClasses.Contains (objccls)) 
										NativeClasses [objccls] = AddTypeForClass ((string) ObjCMessaging.objc_msgSend ((IntPtr) ObjCMessaging.objc_msgSend (objccls, "className", typeof (IntPtr)), "cString", typeof (string)));
	
									object o = ((Type) NativeClasses [objccls]).GetConstructor (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type [] { typeof (IntPtr) }, null).Invoke (new object [] { objectptr });
									ManagedInstances [objectptr] = o;
									NativeInstances [o] = objectptr;
									arguments.Add (o);
								}  
							}
						}
						argptr += Marshal.SizeOf (typeof (IntPtr));
					}
				}
				object return_value = type.InvokeMember (SelectorToMethod (selector, type), BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, target, (object [])arguments.ToArray (typeof (object)));
				return ManagedToNative (return_value);
			} catch (TargetInvocationException e) {
				IntPtr nsex = objc_getClass ("NSException");
				IntPtr nsstr = objc_getClass ("NSString");
				IntPtr nm = (IntPtr) ObjCMessaging.objc_msgSend (nsstr, "stringWithUTF8String:", typeof (IntPtr), typeof (string), e.InnerException.GetType ().ToString ());
				IntPtr rsn = (IntPtr) ObjCMessaging.objc_msgSend (nsstr, "stringWithUTF8String:", typeof (IntPtr), typeof (string), e.InnerException.ToString ());
				IntPtr ex = (IntPtr) ObjCMessaging.objc_msgSend (nsex, "exceptionWithName:reason:userInfo:", typeof (IntPtr), typeof (IntPtr), nm, typeof (IntPtr), rsn, typeof (IntPtr), IntPtr.Zero);
				ObjCMessaging.objc_msgSend (ex, "raise", typeof (void));

				throw new Exception ("ImplementMethod post NSException should never be reached");
			} catch (Exception e) {
				IntPtr nsex = objc_getClass ("NSException");
				IntPtr nsstr = objc_getClass ("NSString");
				IntPtr nm = (IntPtr) ObjCMessaging.objc_msgSend (nsstr, "stringWithUTF8String:", typeof (IntPtr), typeof (string), e.GetType ().ToString ());
				IntPtr rsn = (IntPtr) ObjCMessaging.objc_msgSend (nsstr, "stringWithUTF8String:", typeof (IntPtr), typeof (string), e.ToString ());
				IntPtr ex = (IntPtr) ObjCMessaging.objc_msgSend (nsex, "exceptionWithName:reason:userInfo:", typeof (IntPtr), typeof (IntPtr), nm, typeof (IntPtr), rsn, typeof (IntPtr), IntPtr.Zero);
				ObjCMessaging.objc_msgSend (ex, "raise", typeof (void));

				throw new Exception ("ImplementMethod post NSException should never be reached");
			}
		} 
	
		private static IntPtr ConstructObject (IntPtr cls, IntPtr sel, VarargStack stack) {
			if (!ClassesRegistered.Contains (cls)) 
				return IntPtr.Zero;
			
			ArrayList arguments = new ArrayList ();
			Type type = (Type) ClassesRegistered [cls];
			Type [] argumentTypes = ArgumentTypesForCall (class_getClassMethod (cls, sel));
			if (argumentTypes.Length == 0)
				argumentTypes = new Type [0];

			ConstructorInfo constructor = type.GetConstructor (BindingFlags.Instance | BindingFlags.Public, null, argumentTypes, null);

			unsafe {
				int argptr = (int)&sel+4;
				foreach (ParameterInfo parameter in constructor.GetParameters ()) {
					if (parameter.ParameterType.IsPrimitive || parameter.ParameterType.IsValueType) {
						if (type.IsSubclassOf (typeof (Delegate)) && parameter.ParameterType == typeof (IntPtr)) {
							// This is a magic case; we know that the target is on arguments [0];
							object target = arguments [0];
							string selector = Marshal.PtrToStringAuto (Marshal.ReadIntPtr ((IntPtr)argptr));
							MethodInfo target_method = target.GetType ().GetMethod (SelectorToMethod (selector, target.GetType ()));
							arguments.Add (MethodInfoToFtnptr (target_method));
						} else 
							arguments.Add (Marshal.PtrToStructure ((IntPtr)argptr, parameter.ParameterType));
					} else {
						if (parameter.ParameterType == typeof (string)) 
							arguments.Add (Marshal.PtrToStringAuto (Marshal.ReadIntPtr ((IntPtr)argptr)));
						else {
							IntPtr objectptr = Marshal.ReadIntPtr ((IntPtr)argptr);
							if (ManagedInstances.Contains (objectptr))
								arguments.Add (ManagedInstances [objectptr]);
							else {
								IntPtr objccls = (IntPtr) ObjCMessaging.objc_msgSend (objectptr, "class", typeof (IntPtr));
								if (!NativeClasses.Contains (objccls)) 
									NativeClasses [objccls] = AddTypeForClass ((string) ObjCMessaging.objc_msgSend ((IntPtr) ObjCMessaging.objc_msgSend (objccls, "className", typeof (IntPtr)), "cString", typeof (string)));

								object o = ((Type) NativeClasses [objccls]).GetConstructor (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type [] { typeof (IntPtr) }, null).Invoke (new object [] { objectptr });
								ManagedInstances [objectptr] = o;
								NativeInstances [o] = objectptr;
								arguments.Add (o);
							}  
						}
					}
					argptr += Marshal.SizeOf (typeof (IntPtr));
				}
			}
			object return_value = constructor.Invoke ((object [])arguments.ToArray (typeof (object)));
			
			IntPtr native_object = (IntPtr) ObjCMessaging.objc_msgSend (cls, "alloc", typeof (IntPtr));

			ManagedInstances [native_object] = return_value;
			NativeInstances [return_value] = native_object;

			return native_object;
		}

		private static Type [] ArgumentTypesForCall (IntPtr native_method) {
			if (native_method == IntPtr.Zero)
				return new Type [0];

			int num_arguments = method_getNumberOfArguments (native_method);
			Type [] types = new Type [num_arguments-2];
			IntPtr typestr = IntPtr.Zero;
			int offset = 0;

			for (int i = 2; i < num_arguments; i++) {
				method_getArgumentInfo (native_method, i, ref typestr, ref offset);
				types [i-2] = DecodedType (Marshal.PtrToStringAuto (typestr));
			}
			return types;
		}

		private static object NativeToManaged (IntPtr ptr) {
			if (ptr == IntPtr.Zero)
				return null;

			if (ManagedInstances.Contains (ptr))
				return ManagedInstances [ptr];
			
			IntPtr objccls = (IntPtr) ObjCMessaging.objc_msgSend (ptr, "class", typeof (IntPtr));
			if (!NativeClasses.Contains (objccls)) 
				NativeClasses [objccls] = AddTypeForClass ((string) ObjCMessaging.objc_msgSend ((IntPtr) ObjCMessaging.objc_msgSend (objccls, "className", typeof (IntPtr)), "cString", typeof (string)));

			object o = ((Type) NativeClasses [objccls]).GetConstructor (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type [] { typeof (IntPtr) }, null).Invoke (new object [] { ptr });
			ManagedInstances [ptr] = o;
			NativeInstances [o] = ptr;

			return o;
		}

		private static IntPtr ManagedToNative (object return_value) {
			if (return_value == null)
				return IntPtr.Zero;

			if (NativeInstances.Contains (return_value)) 
				return (IntPtr) NativeInstances [return_value];

			Type type = return_value.GetType ();
			IntPtr native_object = IntPtr.Zero;

			if (type == typeof (string)) {
				native_object = Marshal.StringToHGlobalAuto ((string)return_value);
			} else if (type.IsPrimitive || type.IsValueType) {
				native_object = Marshal.AllocHGlobal (Marshal.SizeOf (return_value.GetType ()));
				Marshal.StructureToPtr (return_value, native_object, true);
			} else {
				IntPtr cls = RegisterClass (type.FullName);
				native_object = (IntPtr) ObjCMessaging.objc_msgSend (cls, "alloc", typeof (IntPtr));
				NativeInstances [return_value] = native_object;
				ManagedInstances [native_object] = return_value;
			}

			return native_object;
		}	
		
		public static void LoadAssembly (string name) {
			LoadedAssemblies [name] = AppDomain.CurrentDomain.Load (AssemblyName.GetAssemblyName (name));
			foreach (AssemblyName referenced in ((Assembly)LoadedAssemblies [name]).GetReferencedAssemblies ())
				LoadedAssemblies [referenced] = AppDomain.CurrentDomain.Load (referenced);
		}

		public static IntPtr GetClass (string classname) {
			IntPtr clsptr = RegisterClass (classname);
			return clsptr;
		}

		public static IntPtr RegisterClass (string classname) {
			if (RegisteredClasses.Contains (classname))
				return (IntPtr) RegisteredClasses [classname];
			
			Type type = Type.GetType (classname);

			if (type == null)
				throw new ArgumentException ("Attempting to register a null type");

			if (module_builder.GetType (type.ToString ()) != null)
				return (IntPtr) objc_getClass (classname);

			IntPtr super_class = objc_lookUpClass ("NSObject");
			
			IntPtr return_value = objc_lookUpClass (classname);
			if (return_value != IntPtr.Zero)
				return return_value;

			NativeRepresentation representation = GenerateNativeRepresentation (type);
			IntPtr [] methods = new IntPtr [representation.Methods.Length];
			IntPtr [] signatures = new IntPtr [representation.Methods.Length];
			IntPtr [] staticmethods = new IntPtr [representation.StaticMethods.Length];
			IntPtr [] staticsignatures = new IntPtr [representation.StaticMethods.Length];
			IntPtr [] constructors = new IntPtr [representation.Constructors.Length];
			IntPtr [] constructor_signatures = new IntPtr [representation.Constructors.Length];
			IntPtr [] member_names = new IntPtr [representation.Members.Length];
			IntPtr [] member_types = new IntPtr [representation.Members.Length];

			for (int i = 0; i < representation.Constructors.Length; i++) {
				constructors [i] = Marshal.StringToCoTaskMemAnsi (representation.Constructors [i]);
				constructor_signatures [i] = Marshal.StringToCoTaskMemAnsi (representation.ConstructorSignatures [i]);
			}

			for (int i = 0; i < representation.StaticMethods.Length; i++) {
				staticmethods [i] = Marshal.StringToCoTaskMemAnsi (representation.StaticMethods [i]);
				staticsignatures [i] = Marshal.StringToCoTaskMemAnsi (representation.StaticSignatures [i]);
			}

			for (int i = 0; i < representation.Methods.Length; i++) {
				methods [i] = Marshal.StringToCoTaskMemAnsi (representation.Methods [i]);
				signatures [i] = Marshal.StringToCoTaskMemAnsi (representation.Signatures [i]);
			}
				
			for (int i = 0; i < representation.Members.Length; i++) {
				member_names [i] = Marshal.StringToCoTaskMemAnsi (representation.Members [i].Name);
				member_types [i] = Marshal.StringToCoTaskMemAnsi (representation.Members [i].Type);
			}

			// MAGIC WARNING: This is serious voodoo; touch at your own risk
			unsafe {
				// This is from this layout:
				/*
					struct objc_class {
						struct objc_class *isa;
						struct objc_class *super_class;
						const char *name;
						long version;
						long info;
						long instance_size;
						struct objc_ivar_list *ivars;
						struct objc_method_list **methodLists;
						struct objc_cache *cache;
						struct objc_protocol_list *protocols;
					}
				*/

				// lets cache some frequently used values
				FieldInfo delegate_target = typeof (Delegate).GetField ("delegate_trampoline", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				int ptrsize = Marshal.SizeOf (typeof (IntPtr));
				int longsize = Marshal.SizeOf (typeof (int)); // Longs are 4 in native land
				int intsize = Marshal.SizeOf (typeof (int));

				char *nameptr = (char*)Marshal.StringToHGlobalAuto (classname);
				void *root_class = (void*)super_class;

				// We first need to find the root_class; we do this by walking up ->super_class
				while ((int)*((int *)((int)root_class+ptrsize)) != 0) {
					root_class = (void *)(int)*((int *)((int)root_class+ptrsize));
				}
				// allocate the class
				void *new_class = (void*)Marshal.AllocHGlobal ((ptrsize*7) + (longsize*3));
				void *meta_class = (void*)Marshal.AllocHGlobal ((ptrsize*7) + (longsize*3));

				// setup the class
				*(int *)((int)new_class+0) = (int)meta_class;
				*(int *)((int)new_class+(ptrsize*3)+longsize) = 0x1;
				*(int *)((int)meta_class+(ptrsize*3)+longsize) = 0x2;

				// set the class name
				*(int *)((int)new_class+(ptrsize*2)) = (int)nameptr;
				*(int *)((int)meta_class+(ptrsize*2)) = (int)nameptr;
				
				// set the class version
				*(int *)((int)new_class+(ptrsize*3)) = 0;
				*(int *)((int)meta_class+(ptrsize*3)) = 0;

				// connect the class heirarchy
				*(int *)((int)new_class+ptrsize) = (int)super_class;
				*(int *)((int)meta_class+ptrsize) = (int)*(int *)((int)super_class);
				*(int *)((int)meta_class) = (int)*(int *)((int)root_class);

				// put in empty method lists for now
				void *new_class_mlist = (void *)Marshal.AllocHGlobal ((ptrsize*2)+intsize);
				void *meta_class_mlist = (void *)Marshal.AllocHGlobal ((ptrsize*2)+intsize);

				*(int *)((int)new_class_mlist) = -1;
				*(int *)((int)meta_class_mlist) = -1;

				*(int *)((int)new_class+(ptrsize*4)+(longsize*3)) = (int)new_class_mlist;
				*(int *)((int)meta_class+(ptrsize*4)+(longsize*3)) = (int)meta_class_mlist;

				// add the ivars
				int ivar_size = (int)*(int *)((int)super_class+(ptrsize*3)+(longsize*2));
				void *ivar_list = (void *)Marshal.AllocHGlobal (intsize+ptrsize+((representation.Members.Length+1)*((ptrsize*2)+intsize)));
				void *ivar = (void*)((int)ivar_list+intsize);
				*(int *)((int)ivar_list) = representation.Members.Length;
				for (int i = 0; i < representation.Members.Length; i++) {
					*(int *)((int)ivar) = (int)member_names [i];
					*(int *)((int)ivar+ptrsize) = (int)member_types [i];
					*(int *)((int)ivar+(ptrsize*2)) = ivar_size;
					ivar_size += representation.Members [i].Size;
					ivar = (void *)((int)ivar+(ptrsize*2)+intsize);
				}
				*(int *)((int)new_class+(ptrsize*3)+(longsize*3)) = (int)ivar_list;
				*(int *)((int)meta_class+(ptrsize*3)+(longsize*3)) = 0;

				*(int *)((int)ivar) = (int)Marshal.StringToCoTaskMemAnsi ("methodCallback");
				*(int *)((int)ivar+ptrsize) = (int)Marshal.StringToCoTaskMemAnsi ("^?");
				*(int *)((int)ivar+(ptrsize*2)) = ivar_size;
				ivar_size += 4;
				
				*(int *)((int)new_class+(ptrsize*3)+(longsize*2)) = ivar_size;
				*(int *)((int)meta_class+(ptrsize*3)+(longsize*2)) = (int)*(int *)(((int)((int*)(int)*(int *)((int)meta_class+ptrsize)))+(ptrsize*3)+(longsize*2));

				// zero the cache and protocols;
				*(int *)((int)new_class+(ptrsize*5)+(longsize*3)) = 0;
				*(int *)((int)new_class+(ptrsize*6)+(longsize*3)) = 0;
				*(int *)((int)meta_class+(ptrsize*5)+(longsize*3)) = 0;
				*(int *)((int)meta_class+(ptrsize*6)+(longsize*3)) = 0;

				objc_addClass ((IntPtr)new_class);

				// add the methods
				void *method_list;
				void *methodptr;
				IntPtr trampoline;

				if (representation.Methods.Length > 0) {
					method_list = (void *)Marshal.AllocHGlobal (ptrsize+intsize+((representation.Methods.Length)*(ptrsize*3)));
					*(int *)((int)method_list+ptrsize) = representation.Methods.Length;
					methodptr = (void*)((int)method_list+ptrsize+intsize);
					trampoline = (IntPtr)delegate_target.GetValue (implement_method);
					for (int i = 0; i < representation.Methods.Length; i++) {
						*(int *)((int)methodptr) = (int)sel_getUid (methods [i]);
						*(int *)((int)methodptr+ptrsize) = (int)signatures [i];
						*(int *)((int)methodptr+(ptrsize*2)) = (int)trampoline;
						methodptr = (void *)((int)methodptr+(ptrsize*3));
					}
				
				
					class_addMethods ((IntPtr)new_class, (IntPtr)method_list);
				}

				if (representation.Constructors.Length > 0) {
					method_list = (void *)Marshal.AllocHGlobal (ptrsize+intsize+((representation.Constructors.Length)*(ptrsize*3)));
					*(int *)((int)method_list+ptrsize) = representation.Constructors.Length;
					methodptr = (void*)((int)method_list+ptrsize+intsize);
					trampoline = (IntPtr)delegate_target.GetValue (construct_object);
					for (int i = 0; i < representation.Constructors.Length; i++) {
						*(int *)((int)methodptr) = (int)sel_getUid (constructors [i]);
						*(int *)((int)methodptr+ptrsize) = (int)constructor_signatures [i];
						*(int *)((int)methodptr+(ptrsize*2)) = (int)trampoline;
						methodptr = (void *)((int)methodptr+(ptrsize*3));
					}
	
					class_addMethods ((IntPtr)meta_class, (IntPtr)method_list);
				}

				if (representation.StaticMethods.Length > 0) {
					method_list = (void *)Marshal.AllocHGlobal (ptrsize+intsize+((representation.StaticMethods.Length)*(ptrsize*3)));
					*(int *)((int)method_list+ptrsize) = representation.StaticMethods.Length;
					methodptr = (void*)((int)method_list+ptrsize+intsize);
					trampoline = (IntPtr)delegate_target.GetValue (implement_static_method);
					for (int i = 0; i < representation.StaticMethods.Length; i++) {
						*(int *)((int)methodptr) = (int)sel_getUid (staticmethods [i]);
						*(int *)((int)methodptr+ptrsize) = (int)staticsignatures [i];
						*(int *)((int)methodptr+(ptrsize*2)) = (int)trampoline;
						methodptr = (void *)((int)methodptr+(ptrsize*3));
					}
	
					class_addMethods ((IntPtr)meta_class, (IntPtr)method_list);
				}
				return_value = (IntPtr)new_class;
			}
			// END MAGIC
			
			RegisteredClasses [classname] = return_value;
			ClassesRegistered [return_value] = type;

			return return_value;
		}
		
		internal static NativeRepresentation GenerateNativeRepresentation (Type type) {
			NativeRepresentation representation = new NativeRepresentation ();

			ArrayList constructors = new ArrayList ();
			ArrayList constructor_signatures = new ArrayList ();
			ArrayList methods = new ArrayList ();
			ArrayList signatures = new ArrayList ();
			ArrayList staticmethods = new ArrayList ();
			ArrayList staticsignatures = new ArrayList ();
			ArrayList nativemembers = new ArrayList ();

			foreach (MethodInfo method in type.GetMethods (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
				signatures.Add (GenerateMethodSignature (method));
				methods.Add (MethodToSelector (method));
			}

			foreach (MethodInfo method in type.GetMethods (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)) {
				staticsignatures.Add (GenerateMethodSignature (method));
				staticmethods.Add (MethodToSelector (method));
			}

			foreach (ConstructorInfo constructor in type.GetConstructors (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)) {
				constructor_signatures.Add (GenerateConstructorSignature (constructor));
				constructors.Add (ConstructorToSelector (constructor, type));
			}

			// FIXME: Implement members

			representation.Constructors = (string[])constructors.ToArray (typeof (string));
			representation.ConstructorSignatures = (string[])constructor_signatures.ToArray (typeof (string));
			representation.Methods = (string[])methods.ToArray (typeof (string));
			representation.Signatures = (string[])signatures.ToArray (typeof (string));
			representation.StaticMethods = (string[])staticmethods.ToArray (typeof (string));
			representation.StaticSignatures = (string[])staticsignatures.ToArray (typeof (string));
			representation.Members = (NativeMember[])nativemembers.ToArray (typeof (NativeMember));

			return representation;
		}

		public static Type DecodedType (string objctype) {
			switch (objctype.Substring (0, 1)) {
				case "c":
					return typeof (char);
				case "C":
					return typeof (char);
				case "s":
					return typeof (short);
				case "S":
					return typeof (ushort);
				case "i":
					return typeof (int);
				case "I":
					return typeof (uint);
				case "l":
					return typeof (long);
				case "L":
					return typeof (ulong);
				case "f":
					return typeof (float);
				case "d":
					return typeof (double);
				case "v":
					return typeof (void);
				case "?":
					return typeof (object);
				case "^":
					return typeof (IntPtr);
				case "*":
					return typeof (string);
				case "B":
					return typeof (bool);
				case "@":
					return typeof (object);
				case "(": {
					int index = objctype.IndexOf (")");
					
					if (index > 0) 
						return Type.GetType (objctype.Substring (1, index-1));
					else
						throw new Exception ("DecodedType decoding exception for unknown: " + objctype);
				}
				default:
					throw new Exception ("DecodedType decoding exception for unknown: " + objctype);
			}
		}

		public static string EncodedType (Type type, out int size) {
			if (type == typeof (char)) {
				size = Marshal.SizeOf (typeof (char));
				return "c";
			}
			if (type == typeof (Int32)) {
				size = Marshal.SizeOf (typeof (Int32));
				return "i";
			}
			if (type == typeof (short)) {
				size = Marshal.SizeOf (typeof (short));
				return "s";
			}
			if (type == typeof (long)) {
				size = Marshal.SizeOf (typeof (long));
				return "l";
			}
			if (type == typeof (Int64)) {
				size = Marshal.SizeOf (typeof (Int64));
				return "q";
			}
			if (type == typeof (UInt32)) {
				size = Marshal.SizeOf (typeof (UInt32));
				return "I";
			}
			if (type == typeof (ushort)) {
				size = Marshal.SizeOf (typeof (ushort));
				return "S";
			}
			if (type == typeof (ulong)) {
				size = Marshal.SizeOf (typeof (ulong));
				return "L";
			}
			if (type == typeof (UInt64)) {
				size = Marshal.SizeOf (typeof (UInt64));
				return "Q";
			}
			if (type == typeof (float)) {
				size = Marshal.SizeOf (typeof (float));
				return "f";
			}
			if (type == typeof (double)) {
				size = Marshal.SizeOf (typeof (double));
				return "d";
			}
			if (type == typeof (bool)) {
				size = Marshal.SizeOf (typeof (bool));
				return "B";
			}
			if (type == typeof (string)) {
				size = Marshal.SizeOf (typeof (IntPtr));
				return "*";
			}
			if (type == typeof (void)) {
				size = 0;
				return "v";
			}
			size = 4;
			return "(" + type + ")";
		}

		public static string GenerateConstructorSignature (ConstructorInfo constructor) {
			int size = 0;
			int p = 0;
			int q = 0;
			string signature = "";

			foreach (ParameterInfo param in constructor.GetParameters ()) {
				if (param.ParameterType.IsPrimitive)
					size += Marshal.SizeOf (param.ParameterType);
				else
					size += Marshal.SizeOf (typeof (IntPtr));
			}

			signature += EncodedType (typeof (IntPtr), out p);
			signature += size;
			signature += "@0:4";
			p = 4;

			foreach (ParameterInfo param in constructor.GetParameters ()) {
				signature += EncodedType (param.ParameterType, out q);
				p += q;
				signature += p;
			}
			
			return signature;
		}

		public static string GenerateMethodSignature (MethodInfo method) {
			int size = 0;
			int p = 0;
			int q = 0;
			string signature = "";

			foreach (ParameterInfo param in method.GetParameters ()) {
				if (param.ParameterType.IsPrimitive)
					size += Marshal.SizeOf (param.ParameterType);
				else
					size += Marshal.SizeOf (typeof (IntPtr));
			}

			signature += EncodedType (method.ReturnType, out p);
			signature += size;
			signature += "@0:4";
			p = 4;

			foreach (ParameterInfo param in method.GetParameters ()) {
				signature += EncodedType (param.ParameterType, out q);
				p += q;
				signature += p;
			}
			
			return signature;
		}

		private static string ConstructorToSelector (ConstructorInfo constructor, Type type) {
			if (type.IsSubclassOf (typeof (Delegate))) 
				return "initWithTarget:Selector:";

			string selector = "init";

			ParameterInfo [] parameters = constructor.GetParameters ();
			if (parameters.Length > 0)
				selector += "With" + parameters [0].ParameterType.ToString ().Substring (parameters [0].ParameterType.ToString ().LastIndexOf (".") > 0 ? parameters [0].ParameterType.ToString ().LastIndexOf (".")+1 : 0) + ":";
			for (int i = 1; i < parameters.Length; i++)
				selector += parameters [i].ParameterType.ToString ().Substring (parameters [i].ParameterType.ToString ().LastIndexOf (".") > 0 ? parameters [i].ParameterType.ToString ().LastIndexOf (".")+1 : 0) + ":";

			return selector;
		}

		private static string SelectorToMethod (string selector, Type type) {
			string methodname = selector;

			if (methodname.IndexOf ("With") > 0)
				methodname = methodname.Substring (0, methodname.IndexOf ("With"));
			if (methodname.IndexOf (":") > 0)
				methodname = methodname.Substring (0, methodname.IndexOf (":"));

			return methodname;
		} 

		private static string MethodToSelector (MethodInfo method) {
			string selector = method.Name;
			ParameterInfo [] parameters = method.GetParameters ();
			if (parameters.Length > 0)
				selector += "With" + parameters [0].ParameterType.ToString ().Substring (parameters [0].ParameterType.ToString ().LastIndexOf (".") > 0 ? parameters [0].ParameterType.ToString ().LastIndexOf (".")+1 : 0) + ":";
			for (int i = 1; i < parameters.Length; i++)
				selector += parameters [i].ParameterType.ToString ().Substring (parameters [i].ParameterType.ToString ().LastIndexOf (".") > 0 ? parameters [i].ParameterType.ToString ().LastIndexOf (".")+1 : 0) + ":";

			return selector.Replace ("[]", "Array");
		}

		private static Assembly TypeResolver (object sender, ResolveEventArgs args) {
			foreach (Assembly assembly in LoadedAssemblies.Values) {
				if (assembly.GetType (args.Name) != null)
					return assembly;
			}
			if (module_builder.Assembly.GetType (args.Name) != null)
				return module_builder.Assembly;

			Type nativetype = AddTypeForClass (args.Name);
			if (nativetype != null) {
				NativeClasses [objc_getClass (args.Name)] = nativetype;
				return module_builder.Assembly;
			}

			return null;
		}

		private static Type AddTypeForClass (string classname) {
			if (classname == null)
				return null;
			if (module_builder.Assembly.GetType (classname) != null)
				return module_builder.Assembly.GetType (classname);

			IntPtr cls = objc_getClass (classname);

			if (cls == IntPtr.Zero)
				return null;

			TypeBuilder type_builder = module_builder.DefineType (classname, TypeAttributes.Public | TypeAttributes.Class);
			MethodBuilder method_builder; 
			ILGenerator il_generator;

			IntPtr iterator = IntPtr.Zero;

			IntPtr lst = class_nextMethodList (cls, ref iterator);

			FieldBuilder field_builder = type_builder.DefineField ("native_object", typeof (IntPtr), FieldAttributes.Private);
			PropertyBuilder property_builder = type_builder.DefineProperty ("NativeObject", PropertyAttributes.HasDefault, typeof (IntPtr), new Type [] { typeof (IntPtr) });

			MethodBuilder get_method_builder = type_builder.DefineMethod ("GetNativeObject", MethodAttributes.Public, typeof (IntPtr), new Type[] {});
			MethodBuilder set_method_builder = type_builder.DefineMethod ("SetNativeObject", MethodAttributes.Public, null, new Type [] { typeof (IntPtr) });

			il_generator = get_method_builder.GetILGenerator ();
			il_generator.Emit (OpCodes.Ldarg_0);
			il_generator.Emit (OpCodes.Ldfld, field_builder);
			il_generator.Emit (OpCodes.Ret);
		
			il_generator = set_method_builder.GetILGenerator ();
			il_generator.Emit (OpCodes.Ldarg_0);
			il_generator.Emit (OpCodes.Ldarg_1);
			il_generator.Emit (OpCodes.Stfld, field_builder);
			il_generator.Emit (OpCodes.Ret);

			property_builder.SetGetMethod (get_method_builder);
			property_builder.SetSetMethod (set_method_builder);

			ConstructorBuilder constructor_builder = type_builder.DefineConstructor (MethodAttributes.Public, CallingConventions.Standard, new Type [] { typeof (IntPtr) });

			il_generator = constructor_builder.GetILGenerator ();
			il_generator.Emit (OpCodes.Ldarg_0);
			il_generator.Emit (OpCodes.Call, typeof (object).GetConstructor (new Type[0]));
			il_generator.Emit (OpCodes.Ldarg_0);
			il_generator.Emit (OpCodes.Ldarg_1);
			il_generator.Emit (OpCodes.Callvirt, set_method_builder);
			il_generator.Emit (OpCodes.Ret);

			while (lst != IntPtr.Zero) {
				int methodcnt = (int)Marshal.ReadIntPtr ((IntPtr)((int)lst+4));

				for (int i = 0; i < methodcnt; i++) {
					IntPtr methptr = Marshal.ReadIntPtr ((IntPtr)((int)lst+8+(i*Marshal.SizeOf (typeof (IntPtr))*3)));
					string selector = Marshal.PtrToStringAuto (methptr);
					IntPtr typestr = Marshal.ReadIntPtr ((IntPtr)((int)lst+12+(i*Marshal.SizeOf (typeof (IntPtr))*3)));
					Type [] argtypes = ArgumentTypesForCall (class_getInstanceMethod ((IntPtr) ObjCMessaging.objc_msgSend (cls, "class", typeof (IntPtr)), sel_getUid (methptr)));
					Type returntype = DecodedType (Marshal.PtrToStringAuto (typestr));
						
					// Fixme; handle retvals and args
					method_builder = type_builder.DefineMethod (SelectorToMethod (selector, null), MethodAttributes.Public, returntype, argtypes);
					il_generator = method_builder.GetILGenerator ();

// DEBUG
					// load self.NativeObject
					il_generator.Emit (OpCodes.Ldarg_0);
					il_generator.Emit (OpCodes.Callvirt, get_method_builder);

					// load the selector
					il_generator.Emit (OpCodes.Ldstr, selector);
				
					// load the returntype
					if (returntype.IsPrimitive || returntype.IsValueType) { 
						il_generator.Emit (OpCodes.Ldtoken, returntype);
					} else {
						if (returntype == typeof (void))
							il_generator.Emit (OpCodes.Ldtoken, typeof (void));
						else
							il_generator.Emit (OpCodes.Ldtoken, typeof (IntPtr));
					}
					il_generator.Emit (OpCodes.Call, typeof (Type).GetMethod ("GetTypeFromHandle"));

					if (argtypes.Length > 0) {
						il_generator.Emit (OpCodes.Ldc_I4, argtypes.Length*2);
						il_generator.Emit (OpCodes.Newarr, typeof (Object));
						il_generator.Emit (OpCodes.Dup);

						// load the args
						for (int j = 1; j < argtypes.Length+1; j++) {
							// get the type
							il_generator.Emit (OpCodes.Ldc_I4, (j-1)*2);
							if (argtypes [j-1].IsPrimitive || argtypes [j-1].IsValueType) { 
								if (j < 4) {
									switch (j) {
										case 1:
											il_generator.Emit (OpCodes.Ldarg_1);
											break;
										case 2:
											il_generator.Emit (OpCodes.Ldarg_2);
											break;
										case 3:
											il_generator.Emit (OpCodes.Ldarg_3);
											break;
									}
								} else 
									il_generator.Emit (OpCodes.Ldarg_S, j);
								il_generator.Emit (OpCodes.Box, argtypes [j-1]);
								il_generator.Emit (OpCodes.Callvirt, typeof (Object).GetMethod ("GetType"));
							} else {
								il_generator.Emit (OpCodes.Ldtoken, typeof (IntPtr));
								il_generator.Emit (OpCodes.Call, typeof (Type).GetMethod ("GetTypeFromHandle"));
							}
							il_generator.Emit (OpCodes.Stelem_Ref);
							il_generator.Emit (OpCodes.Dup);
							// get the type
							// get the arg
							il_generator.Emit (OpCodes.Ldc_I4, ((j-1)*2)+1);
							if (j < 4) {
								switch (j) {
									case 1:
										il_generator.Emit (OpCodes.Ldarg_1);
										break;
									case 2:
										il_generator.Emit (OpCodes.Ldarg_2);
										break;
									case 3:
										il_generator.Emit (OpCodes.Ldarg_3);
										break;
								}
							} else 
								il_generator.Emit (OpCodes.Ldarg_S, j);
							if (argtypes [j-1].IsPrimitive || argtypes [j-1].IsValueType) {
								il_generator.Emit (OpCodes.Box, argtypes [j-1]);
							} else {
								il_generator.Emit (OpCodes.Call, typeof (Bridge).GetMethod ("ManagedToNative", BindingFlags.Static | BindingFlags.NonPublic));
								il_generator.Emit (OpCodes.Box, typeof (IntPtr));
							}
							il_generator.Emit (OpCodes.Stelem_Ref);
							if (j < argtypes.Length)
								il_generator.Emit (OpCodes.Dup);
						}
						il_generator.Emit (OpCodes.Call, typeof (ObjCMessaging).GetMethod ("objc_msgSend", new Type [] {typeof (IntPtr), typeof (String), typeof (Type), typeof (object []) }));
					} else
						il_generator.Emit (OpCodes.Call, typeof (ObjCMessaging).GetMethod ("objc_msgSend", new Type [] {typeof (IntPtr), typeof (String), typeof (Type) }));
					if (!returntype.IsPrimitive && !returntype.IsValueType) {
						il_generator.Emit (OpCodes.Unbox, typeof (IntPtr));
						il_generator.Emit (OpCodes.Ldind_I);
						il_generator.Emit (OpCodes.Call, typeof (Bridge).GetMethod ("NativeToManaged", BindingFlags.Static | BindingFlags.NonPublic));
					} else {
						if (returntype == typeof (void))
							il_generator.Emit (OpCodes.Pop);
					}
					il_generator.Emit (OpCodes.Ret);
				}
				lst = class_nextMethodList (cls, ref iterator);
			}
			return type_builder.CreateType ();
		}

		[DllImport ("libobjc.dylib")]
		private static extern IntPtr class_nextMethodList (IntPtr cls, ref IntPtr iterator);

/*
		[DllImport ("libObjCSharp.dylib")]
		private static extern void dumpMethodList (IntPtr cls);

		[DllImport ("libObjCSharp.dylib")]
		private static extern void dumpClass (IntPtr cls);
*/

		[DllImport ("libObjCSharp.dylib")]
		private static extern void setupDelegate (IMPDelegate d);

		[DllImport ("libobjc.dylib")]
		private static extern IntPtr objc_lookUpClass (string name);
		
		[DllImport ("libobjc.dylib")]
		private static extern IntPtr sel_getUid (IntPtr name);
		
		[DllImport ("libobjc.dylib")]
		private static extern IntPtr objc_getClass (string classname);

		[DllImport ("libobjc.dylib")]
		private static extern void objc_addClass (IntPtr cls);
		
		[DllImport ("libobjc.dylib")]
		private static extern void class_addMethods (IntPtr cls, IntPtr methods);
		
		[DllImport ("libobjc.dylib")]
		private static extern int method_getNumberOfArguments (IntPtr method);    
		
		[DllImport ("libobjc.dylib")]
		private static extern int method_getArgumentInfo (IntPtr method, int index, ref IntPtr type, ref int offset);    
		
		[DllImport ("libobjc.dylib")]
		private static extern IntPtr class_getInstanceMethod (IntPtr cls, IntPtr sel);
		
		[DllImport ("libobjc.dylib")]
		private static extern IntPtr class_getClassMethod (IntPtr cls, IntPtr sel);
	}

	internal struct objc_method {
		internal string method_name;
		internal string method_types;
		internal IntPtr method_imp;
	}
}
