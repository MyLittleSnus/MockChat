using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MockChat.Core;
using MockChat.Core.Abstractions;
using MockChat.Infrustructure.IdentityManagement;

namespace MockChat.Infrustructure.Extensions.DI;

public static class InfrustructureActivator
{
	public static IServiceCollection AddInfrustructure(this IServiceCollection services)
	{
		services.TryAddSingleton<IIdentityProvider<User, Guid>, UserProvider>();

		return services;
	}
}