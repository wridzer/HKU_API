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

    // Get all/current users
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.AppUsers
            .Select(user => new { user.UserName, user.Email }) // Selecteer alleen de naam en e-mail
            .ToListAsync();
        return Ok(users);
    }
    [HttpGet("currentuser")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            return Ok(new { user.Id, user.UserName, user.Email });
        }
        return NotFound();
    }

    [HttpGet("by-username/{username}")]
    public async Task<IActionResult> GetUserIdByUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return BadRequest("Username is required");
        }

        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
        {
            return NotFound("User not found");
        }

        return Ok(new { UserId = user.Id });
    }

    [HttpGet("by-id/{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("User ID is required");
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound("User not found");
        }

        return Ok(new { user.UserName, user.Email });
    }


    // Register new user
    public class RegisterModel
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string? Name { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
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

    // Login/logout user
    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return BadRequest(ModelState);
            }
            var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                
                // Sla de huidige gebruiker op
                return Ok(new { Status = "success", UserId = user.Id });
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return BadRequest(ModelState);
            }
        }
        return BadRequest(ModelState);
    }

    [HttpGet("callback")]
    public IActionResult Callback(string status, string user_id)
    {
        if (status == "success" && !string.IsNullOrEmpty(user_id))
        {
            return Ok(new { Status = "success", UserId = user_id });
        }
        return BadRequest("Invalid callback request.");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok("Logged out successfully.");
    }

    // Update user profile
    [HttpPost("updateprofile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        user.Email = model.Email;
        user.UserName = model.Username;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok("Profile updated successfully.");
    }
    public class UpdateProfileModel
    {
        [Required]
        public string? Username { get; set; }
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
