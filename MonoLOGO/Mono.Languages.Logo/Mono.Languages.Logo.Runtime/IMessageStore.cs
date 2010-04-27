namespace Mono.Languages.Logo.Runtime {
	
	public interface IMessageStore {
		
		bool SupportsMessage (string message);
		MessageInfo DescribeMessage (string message);
		
		MessageInfo[] SupportedMessages { get; }
		
	}
	
}

