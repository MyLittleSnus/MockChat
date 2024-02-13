using Microsoft.AspNetCore.Mvc;
using MockChat.Core;
using MockChat.Core.Abstractions;
using MockChat.Web.Models.Requests;

namespace MockChat.Web.Controllers.API;

[Route("api/identity")]
public class IdentityController(IUserManager userManager) : Controller
{
	[HttpPost]
	[Route("register")]
	public async Task<IActionResult> Register([FromBody] IdentityRequest request)
	{
		int attempts = 0;
		bool registerNotSuccessful;
		User? newUser;
		do
		{
			attempts += 1;

			switch (attempts)
			{
				case > 1 and <= 3:
					await Task.Delay(200);
					break;
				case > 3:
					return StatusCode(408, "Sorry, registration process has failed, try again later");
			}

			bool usernameTaken = await userManager.GetAsync(request.Username) is not null;

			if (usernameTaken) return Conflict();

			newUser = await userManager.SaveAsync(new User(request.Username), request.Password);
			registerNotSuccessful = newUser is null;
		} while (registerNotSuccessful);

		await userManager.Connect(newUser!, HttpContext.Connection.Id);
		return Ok(newUser);
	}
	
	[HttpPost]
	[Route("login")]
	public async Task<IActionResult> Login([FromBody] IdentityRequest request)
	{
		User? user = await userManager.GetAsync(request.Username);
		bool passwordValid = await userManager.ValidatePasswordAsync(user, request.Password);

		if (user is null || !passwordValid) return Unauthorized();

		return Ok(user);
	}

	[HttpGet]
	[Route("{currentUserId:guid}/validate")]
	public async Task<IActionResult> Validate(Guid currentUserId)
	{
		User? user = await userManager.GetAsync(currentUserId);

		if (user is null) return NotFound();

		return Ok();
	}
}