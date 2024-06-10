using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // for user identification

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProjectsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/projects
    [HttpGet]
    public async Task<IActionResult> GetAllProjects()
    {
        var projects = await _context.AppProjects.ToListAsync();
        return Ok(projects);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProject(string id)
    {
        var project = await _context.AppProjects.FindAsync(id);
        if (project == null)
            return NotFound();

        return Ok(project);
    }

    // GET: api/projects/user
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetProjectsByUser(string userId)
    {
        if (!User.Identity.IsAuthenticated || User.FindFirstValue(ClaimTypes.NameIdentifier) != userId)
        {
            return Unauthorized("Access denied");
        }

        // First fetch all projects and then filter in-memory (client-side)
        var projects = await _context.AppProjects.ToListAsync();
        var userProjects = projects.Where(p => p.Contributors.Contains(userId)).ToList();

        return Ok(userProjects);
    }


    // POST: api/projects
    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] ApplicationProject project)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _context.AppProjects.Add(project);
        await _context.SaveChangesAsync();

        // Ensure 'GetProject' action is correctly referenced and id is passed properly
        return Ok(new { id = project.ID });
    }

    // PUT: api/projects/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProject(string id, [FromBody] ApplicationProject projectUpdate)
    {
        var project = await _context.AppProjects.FindAsync(id);
        if (project == null)
            return NotFound();

        project.Name = projectUpdate.Name;
        project.Description = projectUpdate.Description;
        project.Image = projectUpdate.Image;
        project.Contributors = projectUpdate.Contributors;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/projects/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(string id)
    {
        var project = await _context.AppProjects.FindAsync(id);
        if (project == null)
            return NotFound();

        _context.AppProjects.Remove(project);
        await _context.SaveChangesAsync();
        return NoContent();
    }

}
