namespace Mono.Languages.Logo.Runtime {

	using System;
	using System.Collections;
	
	public class IMessageStoreCollection : CollectionBase, ITypedMessageStore, IMessageStore, IMessageTarget {
		private Hashtable messages = new Hashtable ();

		public IMessageStoreCollection () {
		}

		public int Add (IMessageStore store) {
			return List.Add (store);
		}
		
		public void Insert (int index, IMessageStore store) {
			List.Insert (index, store);
		}

		public IMessageStore this[int index] {
			get {
				return (IMessageStore) List[index];
			}
			set {
				List[index] = value;
			}
		}
		
		// IMessageStore
		
		public bool SupportsMessage (string message) {
			foreach (IMessageStore store in List) {
				if (store.SupportsMessage (message))
					return true;
			}
			return false;
		}
		
		public MessageInfo DescribeMessage (string message) {
			foreach (IMessageStore store in List) {
				MessageInfo info = store.DescribeMessage (message);
				if (info != null)
					return info;
			}

			return null;
		}
		
		public MessageInfo[] SupportedMessages {
			get {
				MessageInfo[] ret_array;
				ArrayList ret = new ArrayList ();

				foreach (IMessageStore store in List) {
					ret.AddRange (store.SupportedMessages);
				}

				ret_array = new MessageInfo[ret.Count];
				ret.CopyTo (ret_array);

				return ret_array;
			}
		}

		// IMessageTarget
		public object SendMessage (LogoContext context, string message, ICollection arguments) {
			message = message.ToLower ();
			IMessageTarget target = (IMessageTarget) messages[message];
			if (target == null) {
				foreach (IMessageStore store in List) {
					IMessageTarget store_target = store as IMessageTarget;
					if (store_target != null && store.SupportsMessage (message)) {
						target = store_target;
						messages[message] = target;
						break;
					}
				}
			}
			
			if (target == null)
				throw new MessageNotSupportedException (message);

			return target.SendMessage (context, message, arguments);
		}

		// ITypedMessageStore

		public bool SupportsMessage (string message, Type[] args) {
			foreach (IMessageStore store in List) {
				ITypedMessageStore typed_store = store as ITypedMessageStore;
				if (typed_store != null && typed_store.SupportsMessage (message, args))
					return true;
			}

			return false;
		}

		public TypedMessageInfo DescribeMessage (string message, Type[] args) {
			foreach (IMessageStore store in List) {
				ITypedMessageStore typed_store = store as ITypedMessageStore;
				if (typed_store == null)
					continue;

				TypedMessageInfo info = typed_store.DescribeMessage (message, args);
				if (info != null)
					return info;
			}

			return null;
		}
	}
}

