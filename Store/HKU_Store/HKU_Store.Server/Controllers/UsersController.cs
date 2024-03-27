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
    private readonly SignInManager<ApplicationUser> _signInManager;

    public UsersController(AppDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.AppUsers
            .Select(user => new { user.UserName, user.Email }) // Selecteer alleen de naam en e-mail
            .ToListAsync();
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

    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (ModelState.IsValid)
        {
            // Zoek de gebruiker op via e-mailadres
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Ongeldige inlogpoging.");
                return BadRequest(ModelState);
            }
            var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return Ok(); // Of retourneer een relevante response
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Ongeldige inlogpoging.");
                return BadRequest(ModelState);
            }
        }
        return BadRequest(ModelState);
    }

    [HttpGet("currentuser")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            return Ok(new { user.UserName, user.Email });
        }
        return NotFound();
    }


}
