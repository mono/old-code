namespace Mono.Languages.Logo.Runtime {
	using System.Collections;
	using Mono.Languages.Logo.Compiler;
	
	public class LogoMessageTarget : IMessageTarget, IMessageStore {
		private Hashtable funcs = new Hashtable (new CaseInsensitiveHashCodeProvider (), new CaseInsensitiveComparer ());

		// IMessageTarget
		public object SendMessage (LogoContext context, string message, ICollection arguments) {
			object result = null;
			Function func = funcs[message] as Function;
			if (func == null)
				throw new MessageNotSupportedException (message);
			
			func.Invoke (context, arguments, ref result);
			return result;
		}

		// IMessageStore
		public bool SupportsMessage (string message) {
			return funcs.ContainsKey (message);
		}

		public MessageInfo DescribeMessage (string message) {
			Function func = funcs[message] as Function;
			if (func == null)
				throw new MessageNotSupportedException (message);
			return func.Describe ();
		}
		
		public MessageInfo[] SupportedMessages {
			get {
				ArrayList messages = new ArrayList ();
				foreach (Function f in funcs.Values) {
					messages.Add (f.Describe ());
				}

				return (MessageInfo[]) messages.ToArray (typeof (MessageInfo));
			}
		}

		// Custom
		public void AddMessage (Function func) {
			funcs[func.Name] = func;
		}
	}
}

