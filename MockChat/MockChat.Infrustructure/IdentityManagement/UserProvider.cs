using System.Collections.Concurrent;
using MockChat.Core;
using MockChat.Core.Abstractions;
using MockChat.Core.Exceptions.Identity;
using MockChat.Core.Exceptions.Performance;

namespace MockChat.Infrustructure.IdentityManagement;

public sealed class UserProvider : IIdentityProvider<User, Guid>
{
	private readonly ConcurrentDictionary<Guid, User> _users = new();
	private readonly ConcurrentDictionary<string, Guid> _index = new();

	private struct Index(Guid key, string username)
	{
		public string Username { get; private init; } = username.ToLowerInvariant();
		public Guid Key { get; private init; } = key;

		public static Index Search(string username)
		{
			return new Index
			{
				Username = username.ToLowerInvariant(),
				Key = Guid.Empty,
			};
		}
	} 
	
	public Task<User> CreateAsync(string username)
	{
		User user = new(username);
		bool addResult = _users.TryAdd(user.Id, user);
		
		if (!addResult) throw new IdentityCreateException();
		
		Index index = new(user.Id, user.Username);
		bool indexResult = _index.TryAdd(index.Username, index.Key);

		if (!indexResult) throw new IndexFailedException();
		
		return Task.FromResult(user);
	}

	public Task<User> UpdateAsync(User identity)
	{
		bool found = _users.TryGetValue(identity.Id, out User? user);

		if (!found) throw new IdentityNotFoundException();
		
		user = identity;

		return Task.FromResult(user);
	}

	public Task DeleteAsync(Guid id)
	{
		bool deleteResult = _users.TryRemove(id, out _);

		if (!deleteResult) throw new IdentityDeleteException();

		return Task.CompletedTask;
	}

	public Task<IEnumerable<User>> GetAsync() => Task.FromResult(_users.Select(u => u.Value));

	public Task<User> GetByIdAsync(Guid id)
	{
		bool found = _users.TryGetValue(id, out User? user);
		
		if (!found) throw new IdentityNotFoundException();

		return Task.FromResult(user!);
	}

	public async Task<User> GetByUsernameAsync(string username)
	{
		Index index = Index.Search(username);

		bool indexResult = _index.TryGetValue(index.Username, out Guid id);
		User? user;

		if (indexResult)
		{
			user = await GetByIdAsync(id);
		}
		else
		{
			user = _users.Select(u => u.Value)
				.FirstOrDefault(u => u.Username.ToLowerInvariant() == username.ToLowerInvariant());

			if (user is null) throw new IdentityNotFoundException();
		}

		return user;
	}
}