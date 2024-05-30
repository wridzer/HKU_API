using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProjectContributor>()
            .HasKey(pc => new { pc.ProjectId, pc.Contributor });

        modelBuilder.Entity<ProjectContributor>()
            .HasOne(pc => pc.Project)
            .WithMany(p => p.Contributors)
            .HasForeignKey(pc => pc.ProjectId);
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<ApplicationUser> AppUsers { get; set; }
    public DbSet<ApplicationProject> AppProjects { get; set; }
    public DbSet<AppLeaderboardInfo> AppLeaderboardsInfo { get; set; }
    public DbSet<ProjectContributor> ProjectContributors { get; set; }
}
