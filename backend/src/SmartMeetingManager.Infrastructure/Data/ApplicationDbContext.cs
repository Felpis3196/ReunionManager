using Microsoft.EntityFrameworkCore;
using SmartMeetingManager.Domain.Entities;
using TaskEntity = SmartMeetingManager.Domain.Entities.Task;

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
    public DbSet<TaskEntity> Tasks { get; set; }
    public DbSet<Transcript> Transcripts { get; set; }
    public DbSet<Integration> Integrations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure entities
        ConfigureUser(modelBuilder);
        ConfigureOrganization(modelBuilder);
        ConfigureProject(modelBuilder);
        ConfigureMeeting(modelBuilder);
        ConfigureAgendaItem(modelBuilder);
        ConfigureDecision(modelBuilder);
        ConfigureTask(modelBuilder);
        ConfigureTranscript(modelBuilder);
        ConfigureIntegration(modelBuilder);
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

    private void ConfigureOrganization(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
        });

        modelBuilder.Entity<OrganizationMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User).WithMany(u => u.OrganizationMembers).HasForeignKey(e => e.UserId);
            entity.HasOne(e => e.Organization).WithMany(o => o.Members).HasForeignKey(e => e.OrganizationId);
            entity.HasIndex(e => new { e.UserId, e.OrganizationId }).IsUnique();
        });
    }

    private void ConfigureProject(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.HasOne(e => e.Organization).WithMany(o => o.Projects).HasForeignKey(e => e.OrganizationId);
        });
    }

    private void ConfigureAgendaItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AgendaItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.HasOne(e => e.Meeting).WithMany(m => m.AgendaItems).HasForeignKey(e => e.MeetingId);
        });
    }

    private void ConfigureDecision(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Decision>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
            entity.HasOne(e => e.Meeting).WithMany(m => m.Decisions).HasForeignKey(e => e.MeetingId);
            entity.HasOne(e => e.MadeBy).WithMany().HasForeignKey(e => e.MadeById).OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureTask(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.HasOne(e => e.Meeting).WithMany(m => m.Tasks).HasForeignKey(e => e.MeetingId);
            entity.HasOne(e => e.Project).WithMany(p => p.Tasks).HasForeignKey(e => e.ProjectId);
            entity.HasOne(e => e.AssignedTo).WithMany(u => u.AssignedTasks).HasForeignKey(e => e.AssignedToId);
        });
    }

    private void ConfigureTranscript(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transcript>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.HasOne(e => e.Meeting).WithMany(m => m.Transcripts).HasForeignKey(e => e.MeetingId);
        });
    }

    private void ConfigureIntegration(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Integration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Organization).WithMany().HasForeignKey(e => e.OrganizationId);
            // Store settings as JSON
            entity.Property(e => e.Settings)
                .HasConversion(new JsonDictionaryConverter())
                .HasColumnType("text");
        });
    }
}