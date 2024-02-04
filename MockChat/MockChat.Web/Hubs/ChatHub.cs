using System.Collections.Immutable;
using Microsoft.AspNetCore.SignalR;
using MockChat.Core;
using MockChat.Core.Abstractions;

namespace MockChat.Web.Hubs;

public class ChatHub(
	IChatManager chatManager, 
	IUserManager userManager)
	: Hub
{
	private const string ClientChatCreateMethodName = "onChatCreated";
	private const string ClientMessageReceived = "onMessageReceived";
	private const string ClientInviteResult = "onInviteResult";
	private const string ClientOnInvited = "onInvited";
	private const string OnSessionTimeout = "onSessionInvalid";
	private const string OnSync = "onSync";

	public async Task Sync(Guid? userId)
	{
		try
		{
			User? user = await userManager.GetAsync(userId ?? Guid.Empty);

			if (user is null)
			{
				await Clients.Caller.SendCoreAsync(OnSessionTimeout, [true]);

				return;
			}

			ImmutableList<Chat> userChats = await chatManager.GetAsync(user!.Id);

			foreach (Chat chat in userChats)
			{
				await Groups.RemoveFromGroupAsync(user.ConnectionId, chat.Id.ToString());
			}

			await userManager.UpdateConnectionIdAsync(user, Context.ConnectionId);

			foreach (Chat chat in userChats)
			{
				await Groups.AddToGroupAsync(user.ConnectionId, chat.Id.ToString());
			}

			await Clients.Caller.SendCoreAsync(OnSync, [user]);
		}
		catch (Exception ex)
		{
			await Clients.Caller.SendCoreAsync(OnSync, [ex.Message]);
		}
	}
	
	public async Task CreateChat(string chatName)
	{
		try
		{
			User currentUser = await userManager.GetByConnectionIdAsync(Context.ConnectionId) ?? 
				throw new InvalidOperationException("Invalid connection id");
				
			Chat chat = await chatManager.CreateChatAsync(chatName, currentUser);
				
			await Clients.Caller.SendCoreAsync(ClientChatCreateMethodName, [chat]);
		}
		catch (Exception ex)
		{
			await Clients.Caller.SendCoreAsync(ClientChatCreateMethodName, [ex.Message]);
		}
	}

	public async Task InviteToChat(string username, string chatName)
	{
		try
		{
			User user = (await userManager.Snapshot())
					.FirstOrDefault(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase)) ??
				throw new InvalidOperationException("Invalid user");
			User inviting = await userManager.GetByConnectionIdAsync(Context.ConnectionId) ?? 
				throw new InvalidOperationException("Invalid connection id of inviter");
			Chat chat = await chatManager.GetAsync(inviting.Id, chatName) ?? 
				throw new InvalidOperationException("Invalid chat!");

			if (chat.Users.TryGetValue(user.Id, out User? _))
				throw new InvalidOperationException($"User {user.Username} has already been added to chat");
			
			chat.Users.TryAdd(user.Id, user);

			await Task.WhenAll([
				Groups.AddToGroupAsync(user.ConnectionId, chat.Id.ToString()),
				Clients.Client(user.ConnectionId).SendCoreAsync(ClientOnInvited, [chat]),
				Clients.Caller.SendCoreAsync(ClientInviteResult, [true, user, null])
			]);
		}
		catch (Exception ex)
		{
			await Clients.Caller.SendCoreAsync(ClientInviteResult, [false, null, ex.Message]);
		}
	}
	
	public async Task SendMessageInChat(string chatName, string messageContent)
	{
		try
		{
			User currentUser = await userManager.GetByConnectionIdAsync(Context.ConnectionId) ?? 
				throw new InvalidOperationException("Invalid connection id");
			Chat chat = await chatManager.GetAsync(currentUser.Id, chatName) ?? throw new InvalidOperationException("Invalid chat");
			Message message = new(messageContent, currentUser);
			chatManager.SendMessageAsync(chatName, message);
			await Clients.Group(chat.Id.ToString()).SendCoreAsync(ClientMessageReceived, [message]);
		}
		catch (Exception ex)
		{
			await Clients.Caller.SendCoreAsync(ClientMessageReceived, [ex.Message]);
		}
	}
}