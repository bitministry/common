namespace BitMinistry.OpenAi{

	public abstract class ChatMessage
	{
		public string Role { get; }
		public string Content { get; }

		protected ChatMessage(string role, string content)
		{
			Role = role;
			Content = content;
		}
	}
	
	public class UserChatMessage : ChatMessage
	{
		public UserChatMessage(string content) : base("user", content) { }
	}	
	
	public class SystemChatMessage : ChatMessage
	{
		public SystemChatMessage(string content) : base("system", content) { }
	}
	
	public class AssistantChatMessage : ChatMessage
	{
		public AssistantChatMessage(string content) : base("assistant", content) { }
	}	
	
}