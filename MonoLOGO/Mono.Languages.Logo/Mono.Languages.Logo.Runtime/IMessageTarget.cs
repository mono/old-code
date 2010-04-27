namespace Mono.Languages.Logo.Runtime {
	using System.Collections;
	
	public interface IMessageTarget {
		
		object SendMessage (LogoContext context, string message, ICollection arguments);
				
	}
}

