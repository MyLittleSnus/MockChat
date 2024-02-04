using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using MockChat.Core;
using MockChat.Core.Abstractions;
using MockChat.Web.Models.ViewModels;

namespace MockChat.Web.Controllers;

[Route("{currentUserId:guid}/chats")]
public class ChatsController(IChatManager chatManager) : Controller
{
	[HttpGet]
	public async Task<IActionResult> Get(Guid currentUserId)
	{
		if (!HttpContext.Request.Cookies.TryGetValue("userId", out string? cookieUserId) ||
			!Guid.TryParse(cookieUserId, out Guid cookieGuidVal) ||
			cookieGuidVal != currentUserId) 
			return StatusCode(StatusCodes.Status403Forbidden);
		
		ImmutableList<Chat> chats = await chatManager
			.GetAsync(currentUserId: currentUserId);

		ChatListViewModel view = new(chats);
		
		return View("ChatList", view);
	}

	[HttpGet]
	[Route("{chatName}")]
	public async Task<IActionResult> GetIntoChat(Guid currentUserId, string chatName)
	{
		if (!HttpContext.Request.Cookies.TryGetValue("userId", out string? cookieUserId) ||
			!Guid.TryParse(cookieUserId, out Guid cookieGuidVal) ||
			cookieGuidVal != currentUserId) 
			return StatusCode(StatusCodes.Status403Forbidden);
		
		Chat? chat = await chatManager.GetAsync(currentUserId, chatName);

		ChatViewModel view = new(chat);

		return View("Chat", view);

	}
}