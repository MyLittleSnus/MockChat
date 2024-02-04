namespace MockChat.Core;

public class Message(string content, User user)
{
	public string Content { get; init; } = content;
	
	public User Sender { get; init; } = user;
}