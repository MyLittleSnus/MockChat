using System.ComponentModel.DataAnnotations;

namespace MockChat.Web.Models.Requests;

public class IdentityRequest(string username, string password)
{
	[Required] 
	public string Username { get; } = username;
	
	[Required] 
	[RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}$\n")] 
	public string Password { get; } = password;
}