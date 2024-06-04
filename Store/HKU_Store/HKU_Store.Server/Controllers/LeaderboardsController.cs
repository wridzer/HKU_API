using Microsoft.Data.Sqlite;
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
        var leaderboardInfo = await _context.AppLeaderboardsInfo.FindAsync(leaderboardId);
        if (leaderboardInfo == null)
            return NotFound("Leaderboard not found.");

        var connection = _context.Database.GetDbConnection();

        try
        {
            await connection.OpenAsync();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"SELECT Score FROM `Leaderboard_{leaderboardId}` WHERE PlayerID = '{entry.PlayerID}'";
                var currentScoreObj = await command.ExecuteScalarAsync(); // safer, as it returns the first column of the first row
                int? currentScore = currentScoreObj as int?;

                bool shouldUpdate = currentScore == null ||
                                    (leaderboardInfo.SortMethod == ESortMethod.Descending && entry.Score > currentScore) ||
                                    (leaderboardInfo.SortMethod == ESortMethod.Ascending && entry.Score < currentScore);

                if (shouldUpdate)
                {
                    command.CommandText = currentScore == null ?
                                          $"INSERT INTO `Leaderboard_{leaderboardId}` (PlayerID, Score) VALUES ('{entry.PlayerID}', {entry.Score})" :
                                          $"UPDATE `Leaderboard_{leaderboardId}` SET Score = {entry.Score} WHERE PlayerID = '{entry.PlayerID}'";
                    await command.ExecuteNonQueryAsync();
                }

                command.CommandText = $"SELECT COUNT(*) FROM `Leaderboard_{leaderboardId}` WHERE Score {(leaderboardInfo.SortMethod == ESortMethod.Descending ? ">" : "<")} {entry.Score}";
                var rank = 1 + Convert.ToInt32(await command.ExecuteScalarAsync());

                return Ok(new { Message = "Entry added successfully.", Rank = rank });
            }
        }
        finally
        {
            await connection.CloseAsync();
        }
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

    public enum GetEntryOptions
    {
        Highest,
        AroundMe,
        AtRank,
        Friends
    }

    [HttpGet("entries/{leaderboardId}")]
    public async Task<IActionResult> GetEntries(string leaderboardId, [FromQuery] int amount = 10, [FromQuery] GetEntryOptions option = GetEntryOptions.Highest, [FromQuery] string playerId = null)
    {
        string tableName = $"\"Leaderboard_{leaderboardId}\""; // Properly quote the table name
        string sql;
        int playerScore = 0;

        switch (option)
        {
            case GetEntryOptions.Highest:
                sql = $"SELECT * FROM {tableName} ORDER BY Score DESC LIMIT @amount";
                break;
            case GetEntryOptions.AroundMe:
                if (string.IsNullOrEmpty(playerId))
                    return BadRequest("Player ID is required for AroundMe option.");

                sql = $"SELECT Score FROM {tableName} WHERE PlayerID = @playerId";
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.Add(new SqliteParameter("@playerId", playerId));
                    _context.Database.OpenConnection();
                    var result = await command.ExecuteScalarAsync();
                    if (result == null)
                        return NotFound("Player not found.");
                    playerScore = Convert.ToInt32(result);
                }

                sql = $@"
            (SELECT * FROM {tableName} WHERE Score > @playerScore ORDER BY Score ASC LIMIT @halfAmount)
            UNION
            (SELECT * FROM {tableName} WHERE Score <= @playerScore ORDER BY Score DESC LIMIT @halfAmount)
            ORDER BY Score DESC";
                break;
            case GetEntryOptions.AtRank:
                if (amount < 1)
                    return BadRequest("Amount must be at least 1 for AtRank option.");

                sql = $"SELECT * FROM {tableName} ORDER BY Score DESC LIMIT @offset, 1";
                break;
            default:
                return BadRequest("Invalid option.");
        }

        using (var command = _context.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = sql;
            command.Parameters.Add(new SqliteParameter("@amount", amount));
            if (option == GetEntryOptions.AroundMe)
            {
                command.Parameters.Add(new SqliteParameter("@playerScore", playerScore));
                command.Parameters.Add(new SqliteParameter("@halfAmount", amount / 2));
            }
            else if (option == GetEntryOptions.AtRank)
            {
                command.Parameters.Add(new SqliteParameter("@offset", amount - 1));
            }

            _context.Database.OpenConnection();
            using (var result = await command.ExecuteReaderAsync())
            {
                var entries = new List<AppLeaderboard>();
                while (result.Read())
                {
                    entries.Add(new AppLeaderboard
                    {
                        PlayerID = result.GetString(result.GetOrdinal("PlayerID")),
                        Score = result.GetInt32(result.GetOrdinal("Score"))
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
