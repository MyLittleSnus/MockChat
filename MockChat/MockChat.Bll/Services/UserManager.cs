using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using MockChat.Core;
using MockChat.Core.Abstractions;
using MockChat.Core.Exceptions.Identity;
using MockChat.Core.Exceptions.Performance;

namespace MockChat.Bll.Services;

public class UserManager(
	IIdentityProvider<User, Guid> userProvider, 
	ILogger<UserManager> logger) : IUserManager
{
	private readonly UserConnectionResolver _resolver = new();

	public async Task<User?> SaveUserAsync(User user)
	{
		try
		{
			User created = await userProvider.CreateAsync(user.Username);

			logger.LogInformation(
				"User {Username} {UserId} has been created successfully",
				created.Username, created.Id);

			return created;
		}
		catch (IndexFailedException)
		{
			logger.LogWarning("User {Username} indexing has failed", user.Username);
		}
		catch (IdentityCreateException ex)
		{
			logger.LogError(
				"User {Username} has failed to be created with error: {ErrorMessage}", 
				user.Username, ex.Message);
		}

		return null;
	}

	public async Task Connect(User user, string connectionId)
	{
		user.ConnectionId = connectionId;
		await userProvider.UpdateAsync(user);
		_resolver.Track(user);
	}

	public Task UpdateConnectionIdAsync(User user, string newConnectionId)
	{ 
		_resolver.Drop(user);
		user.ConnectionId = newConnectionId;
		userProvider.UpdateAsync(user);
		bool trackSuccessful = _resolver.Track(user);

		if (!trackSuccessful)
			throw new InvalidOperationException("Connection id was not properly synchronized with the server");
		
		return Task.CompletedTask;
	}

	public Task<User?> GetByConnectionIdAsync(string connectionId) 
		=> Task.FromResult(_resolver.Resolve(connectionId));
	
	public async Task RemoveAsync(string username)
	{
		try
		{
			User user = await userProvider.GetByUsernameAsync(username);
			await userProvider.DeleteAsync(user.Id);
			_resolver.Drop(user);
		}
		catch (IdentityNotFoundException)
		{
			logger.LogError("User with {Username} was not found", username);
		}
		catch (IdentityDeleteException)
		{
			logger.LogError("User {Username} delete operation has failed", username);
		}
	}

	public async Task<ImmutableList<User>> Snapshot()
		=> (await userProvider.GetAsync()).ToImmutableList();

	public async Task<User?> GetAsync(string username)
	{
		try
		{
			User user = await userProvider.GetByUsernameAsync(username);

			return user;
		}
		catch (IdentityNotFoundException)
		{
			logger.LogError("User {Username} was not found", username);

			return null;
		}
	}

	public async Task<User?> GetAsync(Guid id)
	{
		try
		{
			User user = await userProvider.GetByIdAsync(id);

			return user;
		}
		catch (IdentityNotFoundException)
		{
			logger.LogError("User with id {UserId} was not found", id);

			return null;
		}
	}
}