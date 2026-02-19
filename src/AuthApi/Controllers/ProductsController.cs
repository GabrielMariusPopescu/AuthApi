namespace AuthApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetPublic() => Ok(new[] { "Public product A", "Public product B" });

    [Authorize]
    [HttpGet("private")]
    public IActionResult GetPrivate() => Ok(new[] { "Private product 1", "Private product 2" });

    [Authorize(Roles = "Admin")]
    [HttpPost("admin")]
    public IActionResult CreateAdminOnly() => Ok("Created by admin");
}