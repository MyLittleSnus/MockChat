using System.Collections.Concurrent;

namespace MockChat.Core;

public class Chat
{
	public Guid Id { get; } = Guid.NewGuid();
	
	public string Name { get; set; }
	
	public ConcurrentDictionary<Guid, User> Users { get; set; } = [];
	public ConcurrentBag<Message> Messages { get; set; } = [];
}