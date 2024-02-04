using Microsoft.AspNetCore.Mvc;

namespace MockChat.Web.Controllers;

[Route("")]
[Route("home")]
public class HomeController : Controller
{
	public IActionResult Get()
	{
		return View("IdentityPage");
	}
}