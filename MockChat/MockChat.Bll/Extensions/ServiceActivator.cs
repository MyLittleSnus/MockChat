using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MockChat.Bll.Services;
using MockChat.Core.Abstractions;

namespace MockChat.Bll.Extensions;

public static class ServiceActivator
{
	public static IServiceCollection AddChatServices(this IServiceCollection collection)
	{
		collection.TryAddScoped<IUserManager, UserManager>();
		collection.TryAddSingleton<IChatManager, ChatManager>();
		collection.TryAddSingleton<UserConnectionResolver>();

		return collection;
	}
}