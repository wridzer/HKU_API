using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Model configuratie
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<ApplicationUser> AppUsers { get; set; }
    public DbSet<ApplicationProject> AppProjects { get; set; }
    public DbSet<AppLeaderboardInfo> AppLeaderboardsInfo { get; set; }

    // Method to execute SQL Commands for dynamic tables
    public async Task ExecuteSqlCommand(string sql)
    {
        await Database.ExecuteSqlRawAsync(sql);
    }

}
