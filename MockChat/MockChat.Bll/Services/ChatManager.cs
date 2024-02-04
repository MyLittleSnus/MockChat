using System.Collections.Concurrent;
using System.Collections.Immutable;
using MockChat.Core;
using MockChat.Core.Abstractions;

namespace MockChat.Bll.Services;

public class ChatManager : IChatManager
{
	//TODO: Move to chat repository
	private static ConcurrentDictionary<Guid, Chat> Chats { get; } = [];

	public Task<Chat> CreateChatAsync(string chatName, User creator)
	{
		Chat chat = new();
		if (!string.IsNullOrWhiteSpace(chatName))
			chat.Name = chatName;
		chat.Users.TryAdd(creator.Id, creator);
		
		Chats.TryAdd(chat.Id, chat);

		return Task.FromResult(chat);
	}

	public Task SendMessageAsync(string chatName, Message message)
	{
		Guid chatId = ResolveChatId(chatName) ?? Guid.Empty;
		bool chatInvalid = !Chats.TryGetValue(chatId, out Chat? chat);

		if (chatInvalid) throw new InvalidOperationException($"Chat with name {chatName} was not found");
		
		chat!.Messages.Add(message);

		return Task.CompletedTask;
	}

	public Task<ImmutableList<Chat>> GetAsync(Guid currentUserId) => 
		Task.FromResult(Chats.Values
			.Where(chat => chat.Users.ContainsKey(currentUserId))
			.ToImmutableList());

	public async Task<Chat?> GetAsync(Guid currentUserId, string chatName) => 
		(await GetAsync(currentUserId))
			.FirstOrDefault(chat => 
				chat.Name.Equals(chatName, StringComparison.InvariantCultureIgnoreCase) && 
				chat.Users.ContainsKey(currentUserId));
	
	private Guid? ResolveChatId(string chatName)
	{
		Chat? chat = Chats.Values.FirstOrDefault(chat => 
			chat.Name.Equals(chatName, StringComparison.InvariantCultureIgnoreCase));

		return chat?.Id;
	}
}