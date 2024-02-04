using MockChat.Core;

namespace MockChat.Web.Models.ViewModels;

public class ChatViewModel(Chat chat)
{
	public Chat Chat { get; set; } = chat;
}