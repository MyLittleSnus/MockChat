using System.Collections.Concurrent;
using AutoMapper;
using MockChat.Core;
using MockChat.Core.Abstractions;
using MockChat.Core.Exceptions.Identity;
using MockChat.Core.Exceptions.Performance;

namespace MockChat.Infrustructure.IdentityManagement;

internal class UserWithPassword(string username, string password, string? connectionId = null) 
	: User(username, connectionId)
{
	public string PasswordHash { get; set; }
}

public sealed class UserProvider(IMapper mapper) : IIdentityProvider<User, Guid>
{
	private readonly ConcurrentDictionary<Guid, UserWithPassword> _users = new();
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
	
	public Task<User> CreateAsync(string username, string password)
	{
		string passwordHash = HashProvider.Sha512(password);
		UserWithPassword user = new(username, passwordHash);
		
		bool addResult = _users.TryAdd(user.Id, user);
		
		if (!addResult) throw new IdentityCreateException();
		
		Index index = new(user.Id, user.Username);
		bool indexResult = _index.TryAdd(index.Username, index.Key);

		if (!indexResult) throw new IndexFailedException();
		
		return Task.FromResult(user as User);
	}

	public Task<User> UpdateAsync(User identity)
	{
		bool found = _users.TryGetValue(identity.Id, out UserWithPassword? user);
		if (!found) throw new IdentityNotFoundException();

		mapper.Map(identity, user);

		return Task.FromResult(user as User)!;
	}

	public Task DeleteAsync(Guid id)
	{
		bool deleteResult = _users.TryRemove(id, out _);

		if (!deleteResult) throw new IdentityDeleteException();

		return Task.CompletedTask;
	}

	public Task<IEnumerable<User>> GetAsync() => Task.FromResult(_users.Select(u => u.Value as User));

	public Task<User> GetByIdAsync(Guid id)
	{
		bool found = _users.TryGetValue(id, out UserWithPassword? user);
		if (!found) throw new IdentityNotFoundException();

		return Task.FromResult(user as User)!;
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

	public Task<bool> ValidatePassword(User identity, string password)
	{
		bool found = _users.TryGetValue(identity.Id, out UserWithPassword? user);
		
		return Task.FromResult(found && user!.PasswordHash.Equals(password));
	}
}