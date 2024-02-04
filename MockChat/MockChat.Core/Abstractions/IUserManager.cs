using System.Collections.Immutable;

namespace MockChat.Core.Abstractions;

public interface IUserManager
{
	/// <summary>
	/// Saves user in the identity storage
	/// </summary>
	/// <param name="user"></param>
	/// <returns></returns>
	Task<User?> SaveUserAsync(User user);

	/// <summary>
	/// Sync user`s connection
	/// </summary>
	/// <param name="user"></param>
	/// <param name="newConnectionId"></param>
	/// <returns></returns>
	Task UpdateConnectionIdAsync(User user, string newConnectionId);

	Task Connect(User user, string connectionId);

	/// <summary>
	/// Get user by established connection id
	/// </summary>
	/// <param name="connectionId"></param>
	/// <returns></returns>
	Task<User?> GetByConnectionIdAsync(string connectionId);

	/// <summary>
	/// Remove user by username
	/// </summary>
	/// <param name="username"></param>
	/// <returns></returns>
	Task RemoveAsync(string username);

	/// <summary>
	/// Get all users in the storage at the call time
	/// </summary>
	/// <returns></returns>
	Task<ImmutableList<User>> Snapshot();

	/// <summary>
	/// Get user by username
	/// </summary>
	/// <param name="username"></param>
	/// <returns></returns>
	Task<User?> GetAsync(string username);

	/// <summary>
	/// Get user by id
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	Task<User?> GetAsync(Guid id);
}