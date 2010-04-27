namespace Mono.Languages.Logo.Runtime {

	using System;

	public interface ITypedMessageStore {
		
		bool SupportsMessage (string message, Type[] args);
		TypedMessageInfo DescribeMessage (string message, Type[] args);
		
	}
	
}

