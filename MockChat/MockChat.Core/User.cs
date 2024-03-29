using MockChat.Core.Abstractions;

namespace MockChat.Core;

public class User(string username, string? connectionId = null) : IIdentity<Guid>
{
	public User() : this("default") { }
	public Guid Id { get; set; } = Guid.NewGuid();

	public string ConnectionId { get; set; } = connectionId ?? string.Empty;
	public string Username { get; } = username;
}