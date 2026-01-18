using Microsoft.EntityFrameworkCore;
using SmartMeetingManager.Domain.Entities;

namespace SmartMeetingManager.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<OrganizationMember> OrganizationMembers { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Meeting> Meetings { get; set; }
    public DbSet<MeetingParticipant> MeetingParticipants { get; set; }
    public DbSet<AgendaItem> AgendaItems { get; set; }
    public DbSet<Decision> Decisions { get; set; }
    public DbSet<Task> Tasks { get; set; }
    public DbSet<Transcript> Transcripts { get; set; }
    public DbSet<Integration> Integrations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure entities
        ConfigureUser(modelBuilder);
        ConfigureMeeting(modelBuilder);
        ConfigureTask(modelBuilder);
        // Add more configurations as needed
    }

    private void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }

    private void ConfigureMeeting(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.HasOne(e => e.Organization).WithMany(o => o.Meetings).HasForeignKey(e => e.OrganizationId);
            entity.HasOne(e => e.Project).WithMany(p => p.Meetings).HasForeignKey(e => e.ProjectId);
            entity.HasOne(e => e.Organizer).WithMany(u => u.MeetingsAsOrganizer).HasForeignKey(e => e.OrganizerId);
        });

        modelBuilder.Entity<MeetingParticipant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Meeting).WithMany(m => m.Participants).HasForeignKey(e => e.MeetingId);
            entity.HasOne(e => e.User).WithMany(u => u.MeetingParticipants).HasForeignKey(e => e.UserId);
        });
    }

    private void ConfigureTask(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.HasOne(e => e.Meeting).WithMany(m => m.Tasks).HasForeignKey(e => e.MeetingId);
            entity.HasOne(e => e.Project).WithMany(p => p.Tasks).HasForeignKey(e => e.ProjectId);
            entity.HasOne(e => e.AssignedTo).WithMany(u => u.AssignedTasks).HasForeignKey(e => e.AssignedToId);
        });
    }
}