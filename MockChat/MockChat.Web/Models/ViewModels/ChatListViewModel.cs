using System.Collections.Immutable;
using MockChat.Core;

namespace MockChat.Web.Models.ViewModels;

public class ChatListViewModel(ImmutableList<Chat> chats)
{
	public ImmutableList<Chat> Chats { get; init; } = chats;
}