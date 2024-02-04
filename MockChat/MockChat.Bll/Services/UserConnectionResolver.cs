using System.Collections.Concurrent;
using MockChat.Core;

namespace MockChat.Bll.Services;

internal class UserConnectionResolver
{
	private static readonly ConcurrentDictionary<string, User> UserConnections = [];

	internal bool Track(User user)
	{
		ValidateConnection(user.ConnectionId);
		
		return UserConnections.TryAdd(user.ConnectionId, user);
	}

	internal bool Drop(User user)
	{
		ValidateConnection(user.ConnectionId);

		bool removeSuccessful = UserConnections.TryRemove(user.ConnectionId, out User? removed);

		return removeSuccessful;
	}

	internal User? Resolve(string connectionId)
	{
		ValidateConnection(connectionId);

		UserConnections.TryGetValue(connectionId, out User? user);

		return user;
	}

	private void ValidateConnection(string connectionId)
	{
		if (string.IsNullOrWhiteSpace(connectionId))
			throw new InvalidOperationException("Invalid connection id");
	}
}