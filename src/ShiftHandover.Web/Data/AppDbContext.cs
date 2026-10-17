using Microsoft.EntityFrameworkCore;
using ShiftHandover.Web.Domain;

namespace ShiftHandover.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<Issue> Issues => Set<Issue>();
    public DbSet<StatusChange> StatusChanges => Set<StatusChange>();
    public DbSet<Tool> Tools => Set<Tool>();
    public DbSet<Operator> Operators => Set<Operator>();
    public DbSet<ShiftReport> ShiftReports => Set<ShiftReport>();

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

            b.HasOne(i => i.Tool)
                .WithMany()
                .HasForeignKey(i => i.ToolId);

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


             modelBuilder.Entity<Tool>(b =>
        {
            b.Property(t => t.Number).HasMaxLength(30).IsRequired();
            b.HasIndex(t => t.Number).IsUnique();

            // Optional: null means the tool is in the tool room
            b.HasOne(t => t.CurrentMachine)
                .WithMany()
                .HasForeignKey(t => t.CurrentMachineId);
        });

        modelBuilder.Entity<Operator>(b =>
        {
            b.Property(o => o.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<ShiftReport>(b =>
        {
            b.Property(r => r.Shift).HasConversion<string>().HasMaxLength(20);
            b.Property(r => r.MachineStatus).HasConversion<string>().HasMaxLength(30);
            b.Property(r => r.ToolStatus).HasConversion<string>().HasMaxLength(30);
            b.Property(r => r.QualityStatus).HasConversion<string>().HasMaxLength(30);
            b.Property(r => r.Comment).HasMaxLength(1000);

            b.HasOne(r => r.Machine).WithMany().HasForeignKey(r => r.MachineId).OnDelete(DeleteBehavior.Restrict);
            // Optional: a Tom machine has no tool
            b.HasOne(r => r.Tool).WithMany().HasForeignKey(r => r.ToolId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(r => r.Operator).WithMany().HasForeignKey(r => r.OperatorId).OnDelete(DeleteBehavior.Restrict);

            // Only one report per machine, date and shift
            b.HasIndex(r => new { r.MachineId, r.Date, r.Shift }).IsUnique();
        });
    }
}