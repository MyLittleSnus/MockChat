namespace MockChat.Web.Models.Requests;

public class IdentityRequest(string username)
{
	public string Username { get; set; } = username;
}