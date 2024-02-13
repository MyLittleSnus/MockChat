using AutoMapper;
using MockChat.Core;
using MockChat.Infrustructure.IdentityManagement;

namespace MockChat.Infrustructure.Profiles;

public class UserProfile : Profile
{
	public UserProfile()
	{
		CreateMap<User, UserWithPassword>();
	}
}