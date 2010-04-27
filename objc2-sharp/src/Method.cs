using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ObjC2 {
	public class Method {
		private static AssemblyBuilder builder;
		private static ModuleBuilder module;
		private static MethodInfo objectGetPtr;
		private static MethodInfo objectGetObj;

		private Selector selector;
		private Delegate del;
		private Type delegate_type;
		private string signature;
		private Type [] native_parameters;
		private Type [] real_parameters;
				
		static Method () {
			objectGetPtr = typeof (Object).GetMethod ("Get", new Type [] {typeof (IntPtr)});
			objectGetObj = typeof (Object).GetMethod ("Get", new Type [] {typeof (Object)});
			builder = AppDomain.CurrentDomain.DefineDynamicAssembly (new AssemblyName {Name = "MethodImplementation"}, AssemblyBuilderAccess.Run, null, null, null,  null, null, true);
			module = builder.DefineDynamicModule ("Implementations", true);
		}

		public Method (MethodInfo minfo) {
			List <Type> native_parms = new List <Type> ();
			List <Type> real_parms = new List <Type> ();

			// hold this and the selector
			native_parms.Add (typeof (IntPtr));
			native_parms.Add (typeof (IntPtr));
			real_parms.Add (typeof (IntPtr));
			real_parms.Add (typeof (IntPtr));

			selector = new Selector (minfo.Name);

			signature = TypeToNativeRepresentation (minfo.ReturnType).ToString ();
			signature += "@:";
			foreach (ParameterInfo param in minfo.GetParameters ()) {
				if (param.ParameterType == typeof (Object))
					native_parms.Add (typeof (IntPtr));
				else
					native_parms.Add (param.ParameterType);
				real_parms.Add (param.ParameterType);
				signature += TypeToNativeRepresentation (param.ParameterType);
			}

			native_parameters = (Type []) native_parms.ToArray ();
			real_parameters = (Type []) real_parms.ToArray ();
			
			delegate_type = CreateDelegateType (minfo);

			del = CreateDelegate (minfo);
		}

		public IntPtr Selector {
			get { return selector.handle; }
		}

		public Delegate Delegate {
			get { return del; }
		}

		public string Signature {
			get { return signature; }
		}

		private Delegate CreateDelegate (MethodInfo minfo) {
			DynamicMethod method = new DynamicMethod (Guid.NewGuid ().ToString (), (minfo.ReturnType == typeof (Object) ? typeof (IntPtr) : minfo.ReturnType), native_parameters, module);
			ILGenerator il = method.GetILGenerator ();
			
			il.Emit (OpCodes.Ldarg_0);
			il.Emit (OpCodes.Call, objectGetPtr);

			for (int i = 2; i < native_parameters.Length; i++) {
				il.Emit (OpCodes.Ldarg, i);
				if (real_parameters [i] == typeof (Object)) {
					il.Emit (OpCodes.Call, objectGetPtr);
				}
			}

			il.Emit (OpCodes.Call, minfo);
			if (minfo.ReturnType == typeof (Object)) {
				il.Emit (OpCodes.Call, objectGetObj);
			}
			il.Emit (OpCodes.Ret);

			return method.CreateDelegate (delegate_type);
		}

		private Type CreateDelegateType (MethodInfo minfo) {
			TypeBuilder type = module.DefineType (Guid.NewGuid ().ToString (), TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass, typeof (MulticastDelegate));
			type.SetCustomAttribute (new CustomAttributeBuilder (typeof (MarshalAsAttribute).GetConstructor (new Type [] { typeof (UnmanagedType) }), new object [] { UnmanagedType.FunctionPtr }));

			ConstructorBuilder constructor = type.DefineConstructor (MethodAttributes.Public, CallingConventions.Standard, new Type [] { typeof (object), typeof (int) });

			constructor.SetImplementationFlags (MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

			MethodBuilder method = null;

			method = type.DefineMethod ("Invoke", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, (minfo.ReturnType == typeof (Object) ? typeof (IntPtr) : minfo.ReturnType), native_parameters);
			if (method == null)
				throw new ArgumentException ("target must be a Constructor or a Method");

			method.SetImplementationFlags (MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

			return type.CreateType ();
		}
			 
		private static char TypeToNativeRepresentation (Type type) {
			if (type == typeof (char))
				return 'c';
			if (type == typeof (Int32))
				return 'i';
			if (type == typeof (short))
				return 's';
			if (type == typeof (long))
				return 'l';
			if (type == typeof (Int64))
				return 'q';
			if (type == typeof (UInt32))
				return 'I';
			if (type == typeof (ushort))
				return 'S';
			if (type == typeof (ulong))
				return 'L';
			if (type == typeof (UInt64))
				return 'Q';
			if (type == typeof (float))
				return 'f';
			if (type == typeof (double))
				return 'd';
			if (type == typeof (bool))
				return 'B';
			if (type == typeof (string))
				return '*';
			if (type == typeof (void))
				return 'v';
			return '@';
		}
	}
}
