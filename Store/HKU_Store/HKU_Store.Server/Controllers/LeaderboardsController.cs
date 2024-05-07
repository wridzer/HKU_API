
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class LeaderboardsController : ControllerBase
{
    private readonly AppDbContext _context;

    public LeaderboardsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateLeaderboardTable([FromBody] AppLeaderboardInfo leaderboardInfo)
    {
        // Create leaderboard info entry
        _context.AppLeaderboardsInfo.Add(leaderboardInfo);
        await _context.SaveChangesAsync();

        // Dynamically create the leaderboard table
        string tableName = $"`Leaderboard_{leaderboardInfo.ID}`";
        string sql = $"CREATE TABLE {tableName} (ID INT PRIMARY KEY AUTO_INCREMENT, PlayerID VARCHAR(255), Score INT)";
        try
        {
            await _context.ExecuteSqlCommand(sql);
            return Ok($"Leaderboard table {tableName} created successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest($"An error occurred while creating the table: {ex.Message}");
        }
    }

    [HttpPost("addentry/{leaderboardId}")]
    public async Task<IActionResult> AddEntryToLeaderboard(string leaderboardId, [FromBody] AppLeaderboard entry)
    {
        string tableName = $"Leaderboard_{leaderboardId}";
        string sql = $"INSERT INTO {tableName} (PlayerID, Score) VALUES ('{entry.PlayerID}', {entry.Score})";
        await _context.ExecuteSqlCommand(sql);

        return Ok("Entry added successfully.");
    }

    // Update leaderboard configuration
    [HttpPut("update/{leaderboardId}")]
    public async Task<IActionResult> UpdateLeaderboard(string leaderboardId, [FromBody] AppLeaderboardInfo updatedInfo)
    {
        var existingInfo = await _context.AppLeaderboardsInfo.FindAsync(leaderboardId);
        if (existingInfo == null)
            return NotFound("Leaderboard not found.");

        existingInfo.Name = updatedInfo.Name;
        existingInfo.Description = updatedInfo.Description;
        existingInfo.SortMethod = updatedInfo.SortMethod;
        existingInfo.DisplayType = updatedInfo.DisplayType;

        _context.AppLeaderboardsInfo.Update(existingInfo);
        await _context.SaveChangesAsync();

        return Ok("Leaderboard updated successfully.");
    }

    // Delete leaderboard
    [HttpDelete("delete/{leaderboardId}")]
    public async Task<IActionResult> DeleteLeaderboard(string leaderboardId)
    {
        var leaderboardInfo = await _context.AppLeaderboardsInfo.FindAsync(leaderboardId);
        if (leaderboardInfo == null)
            return NotFound("Leaderboard not found.");

        _context.AppLeaderboardsInfo.Remove(leaderboardInfo);
        await _context.SaveChangesAsync();

        string tableName = $"Leaderboard_{leaderboardId}";
        string sql = $"DROP TABLE IF EXISTS {tableName}";
        await _context.ExecuteSqlCommand(sql);

        return Ok($"Leaderboard {tableName} deleted successfully.");
    }

    // Get entries - Updated for correct fetching
    [HttpGet("entries/{leaderboardId}")]
    public async Task<IActionResult> GetEntries(string leaderboardId)
    {
        string tableName = $"Leaderboard_{leaderboardId}";
        string sql = $"SELECT * FROM {tableName}";

        using (var command = _context.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = sql;
            _context.Database.OpenConnection();
            using (var result = await command.ExecuteReaderAsync())
            {
                var entries = new List<AppLeaderboard>();
                while (result.Read())
                {
                    entries.Add(new AppLeaderboard
                    {
                        PlayerID = result.GetString(result.GetOrdinal("PlayerID")),
                        Score = result.GetString(result.GetOrdinal("Score"))
                    });
                }
                return Ok(entries);
            }
        }
    }

    [HttpGet("by-project/{projectId}")]
    public async Task<IActionResult> GetLeaderboardsByProject(string projectId)
    {
        var leaderboards = await _context.AppLeaderboardsInfo
                                         .Where(l => l.ProjectID == projectId)
                                         .ToListAsync();

        if (!leaderboards.Any())
        {
            return NotFound("No leaderboards found for the specified project.");
        }

        return Ok(leaderboards);
    }

}
