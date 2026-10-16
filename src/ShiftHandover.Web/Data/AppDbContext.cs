using Microsoft.EntityFrameworkCore;
using ShiftHandover.Web.Domain;

namespace ShiftHandover.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<Issue> Issues => Set<Issue>();
    public DbSet<StatusChange> StatusChanges => Set<StatusChange>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Machine>(b =>
        {
            b.Property(m => m.Name).HasMaxLength(100).IsRequired();
            b.HasIndex(m => m.Name).IsUnique();
        });

        modelBuilder.Entity<Issue>(b =>
        {
            b.Property(i => i.Title).HasMaxLength(300).IsRequired();
            b.Property(i => i.ActionTaken).HasMaxLength(1000);
            b.Property(i => i.FollowUp).HasMaxLength(1000);

            // Enums are stored as text ("Open", "Toolmaker"...) so the database is readable
            b.Property(i => i.Status).HasConversion<string>().HasMaxLength(30);
            b.Property(i => i.Category).HasConversion<string>().HasMaxLength(30);
            b.Property(i => i.ResponsibleTeam).HasConversion<string>().HasMaxLength(30);

            b.HasOne(i => i.Machine)
                .WithMany(m => m.Issues)
                .HasForeignKey(i => i.MachineId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasMany(i => i.History)
                .WithOne()
                .HasForeignKey(s => s.IssueId)
                .OnDelete(DeleteBehavior.Cascade);

            // The handover board always filters on status, so index it
            b.HasIndex(i => i.Status);
        });

        modelBuilder.Entity<StatusChange>(b =>
        {
            b.Property(s => s.FromStatus).HasConversion<string>().HasMaxLength(30);
            b.Property(s => s.ToStatus).HasConversion<string>().HasMaxLength(30);
            b.Property(s => s.Comment).HasMaxLength(500);
        });
    }
}