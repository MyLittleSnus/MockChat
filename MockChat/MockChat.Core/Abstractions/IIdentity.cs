namespace MockChat.Core.Abstractions;

public interface IIdentity<TKey> where TKey : IEquatable<TKey>
{
	public TKey Id { get; set; }
	public string Username { get; }
}