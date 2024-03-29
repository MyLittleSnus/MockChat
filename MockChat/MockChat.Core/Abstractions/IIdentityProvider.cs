namespace MockChat.Core.Abstractions;

public interface IIdentityProvider<TIdentity, in TKey>
	where TKey : IEquatable<TKey>
	where TIdentity : IIdentity<TKey>
{
	Task<User> CreateAsync(string username);

	Task<User> UpdateAsync(TIdentity identity);

	Task DeleteAsync(TKey id);

	Task<IEnumerable<TIdentity>> GetAsync();

	Task<TIdentity> GetByIdAsync(TKey id);

	Task<User> GetByUsernameAsync(string username);
}