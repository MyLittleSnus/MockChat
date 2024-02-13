using MockChat.Core;
using MockChat.Infrustructure.IdentityManagement;

namespace MockChat.Infrustructure.Extensions;

internal static class UserExtensions
{
	public static UserWithPassword ToUserWithPassword(this User user)
	{
		return new UserWithPassword(user.Username, "");
	}
}