﻿using Microsoft.Data.Sqlite;
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
        string tableName = $"\"Leaderboard_{leaderboardInfo.ID}\"";
        string sql = $"CREATE TABLE {tableName} (ID INTEGER PRIMARY KEY AUTOINCREMENT, PlayerID TEXT, Score INTEGER)";
        try
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                await command.ExecuteNonQueryAsync();
            }
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
		if (entry == null)
			return BadRequest("Entry cannot be null.");

		var leaderboardInfo = await _context.AppLeaderboardsInfo.FindAsync(leaderboardId);
		if (leaderboardInfo == null)
			return NotFound("Leaderboard not found.");

		var connection = _context.Database.GetDbConnection();
		if (connection == null)
			return StatusCode(500, "Database connection is not available.");

		try
		{
			await connection.OpenAsync();
			using (var command = connection.CreateCommand())
			{
				command.CommandText = $"SELECT Score FROM `Leaderboard_{leaderboardId}` WHERE PlayerID = @PlayerID";
				command.Parameters.Add(new SqliteParameter("@PlayerID", entry.PlayerID));
				var currentScoreObj = await command.ExecuteScalarAsync();

				int? currentScore = null;
				if (currentScoreObj != null && currentScoreObj != DBNull.Value)
				{
					currentScore = Convert.ToInt32(currentScoreObj);
				}

				bool shouldUpdate = currentScore == null ||
									(leaderboardInfo.SortMethod == ESortMethod.Descending && entry.Score > currentScore) ||
									(leaderboardInfo.SortMethod == ESortMethod.Ascending && entry.Score < currentScore);

				if (shouldUpdate)
				{
					if (currentScore == null)
					{
						command.CommandText = $"INSERT INTO `Leaderboard_{leaderboardId}` (PlayerID, Score) VALUES (@PlayerID, @Score)";
					}
					else
					{
						command.CommandText = $"UPDATE `Leaderboard_{leaderboardId}` SET Score = @Score WHERE PlayerID = @PlayerID";
					}
					command.Parameters.Clear();
					command.Parameters.Add(new SqliteParameter("@PlayerID", entry.PlayerID));
					command.Parameters.Add(new SqliteParameter("@Score", entry.Score));
					await command.ExecuteNonQueryAsync();
				}

				command.CommandText = $"SELECT COUNT(*) FROM `Leaderboard_{leaderboardId}` WHERE Score {(leaderboardInfo.SortMethod == ESortMethod.Descending ? ">" : "<")} @Score";
				command.Parameters.Clear();
				command.Parameters.Add(new SqliteParameter("@Score", entry.Score));
				var rankCount = await command.ExecuteScalarAsync();
				var rank = 1 + Convert.ToInt32(rankCount);

				return Ok(new { Message = "Entry added successfully.", Rank = rank });
			}
		}
		catch (Exception ex)
		{
			// Log the exception (if you have a logging framework)
			// _logger.LogError(ex, "An error occurred while adding an entry to the leaderboard.");

			return StatusCode(500, $"An error occurred: {ex.Message}");
		}
		finally
		{
			if (connection != null && connection.State == System.Data.ConnectionState.Open)
			{
				await connection.CloseAsync();
			}
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
    public struct LeaderboardEntry
    {
        public string PlayerID { get; set; }
        public int Score { get; set; }
        public int Rank { get; set; }
    }

    [HttpGet("entries/{leaderboardId}")]
    public async Task<IActionResult> GetEntries(string leaderboardId, [FromQuery] int amount = 10, [FromQuery] GetEntryOptions option = GetEntryOptions.Highest, [FromQuery] string playerId = null)
    {
        var leaderboardInfo = await _context.AppLeaderboardsInfo.FindAsync(leaderboardId);
        if (leaderboardInfo == null)
            return NotFound("Leaderboard not found.");

        string tableName = $"\"Leaderboard_{leaderboardId}\""; // Properly quote the table name
        string sql;
        int playerScore = 0;
        List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
        string orderDirection = leaderboardInfo.SortMethod == ESortMethod.Ascending ? "ASC" : "DESC";

        switch (option)
        {
            case GetEntryOptions.Highest:
                sql = $"SELECT PlayerID, Score FROM {tableName} ORDER BY Score {orderDirection} LIMIT @amount";
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.Add(new SqliteParameter("@amount", amount));
                    _context.Database.OpenConnection();
                    using (var result = await command.ExecuteReaderAsync())
                    {
                        int rank = 1;
                        while (result.Read())
                        {
                            entries.Add(new LeaderboardEntry
                            {
                                PlayerID = result.GetString(result.GetOrdinal("PlayerID")),
                                Score = result.GetInt32(result.GetOrdinal("Score")),
                                Rank = rank++
                            });
                        }
                    }
                }
                break;
            case GetEntryOptions.AroundMe:
                if (string.IsNullOrEmpty(playerId))
                    return BadRequest("Player ID is required for AroundMe option.");

                // Get player score
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
            (SELECT PlayerID, Score FROM {tableName} WHERE Score {GetComparisonOperator(leaderboardInfo.SortMethod, true)} @playerScore ORDER BY Score {GetOppositeOrderDirection(leaderboardInfo.SortMethod)} LIMIT @halfAmount)
            UNION
            (SELECT PlayerID, Score FROM {tableName} WHERE Score {GetComparisonOperator(leaderboardInfo.SortMethod, false)} @playerScore ORDER BY Score {orderDirection} LIMIT @halfAmount)
            ORDER BY Score {orderDirection}";
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.Add(new SqliteParameter("@playerScore", playerScore));
                    command.Parameters.Add(new SqliteParameter("@halfAmount", amount / 2));
                    _context.Database.OpenConnection();
                    using (var result = await command.ExecuteReaderAsync())
                    {
                        int rank = 1;
                        while (result.Read())
                        {
                            entries.Add(new LeaderboardEntry
                            {
                                PlayerID = result.GetString(result.GetOrdinal("PlayerID")),
                                Score = result.GetInt32(result.GetOrdinal("Score")),
                                Rank = rank++
                            });
                        }
                    }
                }
                break;
            case GetEntryOptions.AtRank:
                if (amount < 1)
                    return BadRequest("Amount must be at least 1 for AtRank option.");

                sql = $"SELECT PlayerID, Score FROM {tableName} ORDER BY Score {orderDirection} LIMIT @offset, 1";
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.Add(new SqliteParameter("@offset", amount - 1));
                    _context.Database.OpenConnection();
                    using (var result = await command.ExecuteReaderAsync())
                    {
                        int rank = 1;
                        while (result.Read())
                        {
                            entries.Add(new LeaderboardEntry
                            {
                                PlayerID = result.GetString(result.GetOrdinal("PlayerID")),
                                Score = result.GetInt32(result.GetOrdinal("Score")),
                                Rank = rank++
                            });
                        }
                    }
                }
                break;
            default:
                return BadRequest("Invalid option.");
        }

        return Ok(new { entries });
    }

    private string GetComparisonOperator(ESortMethod sortMethod, bool isUpper)
    {
        if (sortMethod == ESortMethod.Ascending)
            return isUpper ? ">" : "<=";
        else
            return isUpper ? "<" : ">=";
    }

    private string GetOppositeOrderDirection(ESortMethod sortMethod)
    {
        return sortMethod == ESortMethod.Ascending ? "DESC" : "ASC";
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
