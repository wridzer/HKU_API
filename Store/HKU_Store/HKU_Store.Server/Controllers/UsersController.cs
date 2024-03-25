using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(AppDbContext context, UserManager<ApplicationUser> usermanager)
    {
        _context = context;
        _userManager = usermanager;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.AppUsers.ToListAsync();
        return Ok(users);
    }
    public class RegisterModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser { UserName = model.Username, Email = model.Email};
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Logica na succesvolle registratie (bijv. inloggen van de gebruiker)
                return Ok();
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // Retourneer fouten als de registratie niet succesvol was
        return BadRequest(ModelState);
    }
}
