using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
    public DbSet<OrganizationCustomRole> OrganizationCustomRoles { get; set; }
    public DbSet<OrganizationMember> OrganizationMembers { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Meeting> Meetings { get; set; }
    public DbSet<MeetingParticipant> MeetingParticipants { get; set; }
    public DbSet<AgendaItem> AgendaItems { get; set; }
    public DbSet<Decision> Decisions { get; set; }
    public DbSet<TaskEntity> Tasks { get; set; }
    public DbSet<Transcript> Transcripts { get; set; }
    public DbSet<Integration> Integrations { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Invite> Invites { get; set; }
    public DbSet<MeetingFile> MeetingFiles { get; set; }
    public DbSet<TeamMessage> TeamMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure entities
        ConfigureUser(modelBuilder);
        ConfigureOrganization(modelBuilder);
        ConfigureOrganizationCustomRole(modelBuilder);
        ConfigureProject(modelBuilder);
        ConfigureMeeting(modelBuilder);
        ConfigureAgendaItem(modelBuilder);
        ConfigureDecision(modelBuilder);
        ConfigureTask(modelBuilder);
        ConfigureTranscript(modelBuilder);
        ConfigureIntegration(modelBuilder);
        ConfigureRefreshToken(modelBuilder);
        ConfigureInvite(modelBuilder);
        ConfigureMeetingFile(modelBuilder);
        ConfigureTeamMessage(modelBuilder);
    }

    private void ConfigureTeamMessage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TeamMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(4000);
            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.OrganizationId, e.CreatedAt });
        });
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
            entity.HasOne(e => e.CustomRole)
                .WithMany()
                .HasForeignKey(e => e.CustomRoleId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.UserId, e.OrganizationId }).IsUnique();
        });
    }

    private void ConfigureOrganizationCustomRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrganizationCustomRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PermissionsJson).IsRequired().HasMaxLength(2000);
            entity.HasOne(e => e.Organization)
                .WithMany(o => o.CustomRoles)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
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
                .HasColumnType("text")
                .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, string>?>(
                    (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                    c => c != null ? c.GetHashCode() : 0,
                    c => c != null ? new Dictionary<string, string>(c) : null));
        });
    }

    private void ConfigureRefreshToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureInvite(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invite>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.InviteCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.InvitePasswordHash).HasMaxLength(500);
            entity.HasIndex(e => e.InviteCode).IsUnique();
            entity.HasOne(e => e.Organization)
                .WithMany(o => o.Invites)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.InvitedBy)
                .WithMany()
                .HasForeignKey(e => e.InvitedById)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.CustomRole)
                .WithMany()
                .HasForeignKey(e => e.CustomRoleId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureMeetingFile(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MeetingFile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.StoragePath).IsRequired().HasMaxLength(1000);
            entity.HasOne(e => e.Meeting)
                .WithMany(m => m.Files)
                .HasForeignKey(e => e.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.UploadedBy)
                .WithMany()
                .HasForeignKey(e => e.UploadedById)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}