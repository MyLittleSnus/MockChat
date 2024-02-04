using System.Collections.Immutable;

namespace MockChat.Core.Abstractions;

public interface IChatManager
{
	/// <summary>
	/// Create a chat by creator with provided name
	/// </summary>
	/// <param name="chatName"></param>
	/// <param name="creator"></param>
	/// <returns></returns>
	Task<Chat> CreateChatAsync(string chatName, User creator);

	/// <summary>
	/// Send a message to chat by chat name
	/// </summary>
	/// <param name="chatName"></param>
	/// <param name="message"></param>
	/// <returns></returns>
	Task SendMessageAsync(string chatName, Message message);

	/// <summary>
	/// Get current user`s chat by chat name
	/// </summary>
	/// <param name="currentUserId"></param>
	/// <param name="chatName"></param>
	/// <returns></returns>
	Task<Chat?> GetAsync(Guid currentUserId, string chatName);

	/// <summary>
	/// Get all current user`s chats
	/// </summary>
	/// <param name="currentUserId"></param>
	/// <returns></returns>
	Task<ImmutableList<Chat>> GetAsync(Guid currentUserId);
}