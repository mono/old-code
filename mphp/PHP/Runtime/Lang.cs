using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Reflection;


namespace PHP.Runtime {


	public class Lang {

		public static object define(object o1, object o2, object o3) {
			return Core.DefineConstant(o1, o2, o3);
		}

		public static object key(object o) {
			if (o is Array) {
				Array a = (Array)o;
				return a.Key();
			}
			else {
				Report.Warn(411);
				return null;
			}
		}

		public static object current(object o) {
			if (o is Array) {
				Array a = (Array)o;
				return a.Current();
			}
			else {
				Report.Warn(411);
				return null;
			}
		}

		public static object next(object o) {
			if (o is Array) {
				Array a = (Array)o;
				return a.Next();
			}
			else {
				Report.Warn(411);
				return null;
			}
		}

		public static object prev(object o) {
			if (o is Array) {
				Array a = (Array)o;
				return a.Prev();
			}
			else {
				Report.Warn(411);
				return null;
			}
		}

		public static object each(object o) {
			if (o is Array) {
				Array a = (Array)o;
				return a.Each();
			}
			else {
				Report.Warn(411);
				return null;
			}
		}

		public static object reset(object o) {
			if (o is Array) {
				Array a = (Array)o;
				return a.Reset();
			}
			else {
				Report.Warn(411);
				return null;
			}
		}

		public static object add_event(object o, object evenT, object delegatE) {
			// invalid type
			if (o == null) {
				Report.Warn(224);
				return null;
			}
			if (!(delegatE is Delegate)) {
				Report.Warn(225);
				return null;
			}
			String eventName = Convert.ToString(evenT);
			Delegate d = (Delegate)delegatE;
			EventInfo ei = o.GetType().GetEvent(eventName);
			if (ei == null)
				Report.Error(222);
			ei.AddEventHandler(o, d);
			return null;
		}

		public static object remove_event(object o, object evenT, object delegatE) {
			// invalid type
			if (o == null) {
				Report.Warn(224);
				return null;
			}
			if (!(delegatE is Delegate)) {
				Report.Warn(225);
				return null;
			}
			String eventName = Convert.ToString(evenT);
			Delegate d = (Delegate)delegatE;
			EventInfo ei = o.GetType().GetEvent(eventName);
			if (ei == null)
				Report.Error(222);
			ei.RemoveEventHandler(o, d);
			return null;
		}

		public static void Echo(object o) {
			Core.DeReference(ref o);
			if (o == null) { /* do nothing */ }
			else if (o is bool) {
				if ((bool)o)
					Console.Write(1);
			}
			else if (o is Array)
				Console.Write("Array");
			else if (o is Object)
				Console.Write("Object id #" + ((Object)o).__Id);
			else
				Console.Write(o);
		}

	}


}