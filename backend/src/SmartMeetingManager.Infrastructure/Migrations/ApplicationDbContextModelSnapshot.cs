using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SmartMeetingManager.Infrastructure.Data;

#nullable disable

namespace SmartMeetingManager.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.AgendaItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<TimeSpan?>("EstimatedDuration")
                        .HasColumnType("interval");

                    b.Property<bool>("IsCompleted")
                        .HasColumnType("boolean");

                    b.Property<Guid>("MeetingId")
                        .HasColumnType("uuid");

                    b.Property<int>("Order")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.HasKey("Id");

                    b.HasIndex("MeetingId");

                    b.ToTable("AgendaItems");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.Decision", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)");

                    b.Property<DateTime?>("ImplementedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsImplemented")
                        .HasColumnType("boolean");

                    b.Property<Guid?>("MadeById")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("MadeAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("MeetingId")
                        .HasColumnType("uuid");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.HasKey("Id");

                    b.HasIndex("MadeById");

                    b.HasIndex("MeetingId");

                    b.ToTable("Decisions");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.Integration", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("AccessToken")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("LastSyncAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uuid");

                    b.Property<string>("RefreshToken")
                        .HasColumnType("text");

                    b.Property<string>("Settings")
                        .HasColumnType("text");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("TokenExpiresAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("OrganizationId");

                    b.ToTable("Integrations");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.Meeting", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("interval");

                    b.Property<DateTime?>("EndedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("FinalAgenda")
                        .HasColumnType("text");

                    b.Property<string>("Location")
                        .HasColumnType("text");

                    b.Property<string>("MeetingUrl")
                        .HasColumnType("text");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("OrganizerId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ProjectId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("ScheduledAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("StartedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("SuggestedAgenda")
                        .HasColumnType("text");

                    b.Property<string>("Summary")
                        .HasColumnType("text");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("OrganizationId");

                    b.HasIndex("OrganizerId");

                    b.HasIndex("ProjectId");

                    b.ToTable("Meetings");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.MeetingParticipant", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("AttendedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("InvitedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("MeetingId")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("RespondedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("MeetingId");

                    b.HasIndex("UserId");

                    b.ToTable("MeetingParticipants");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.Organization", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("InviteCode")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("LogoUrl")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Organizations");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.OrganizationCustomRole", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("PermissionsJson")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)");

                    b.HasKey("Id");

                    b.HasIndex("OrganizationId");

                    b.ToTable("OrganizationCustomRoles");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.OrganizationMember", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("CustomRoleId")
                        .HasColumnType("uuid");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("JoinedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uuid");

                    b.Property<int>("Role")
                        .HasColumnType("integer");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("CustomRoleId");

                    b.HasIndex("OrganizationId");

                    b.HasIndex("UserId", "OrganizationId")
                        .IsUnique();

                    b.ToTable("OrganizationMembers");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.Project", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Color")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("OrganizationId");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.Task", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AssignedToId")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("CompletedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<DateTime?>("DueDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("MeetingId")
                        .HasColumnType("uuid");

                    b.Property<int>("Priority")
                        .HasColumnType("integer");

                    b.Property<Guid?>("ProjectId")
                        .HasColumnType("uuid");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("AssignedToId");

                    b.HasIndex("MeetingId");

                    b.HasIndex("ProjectId");

                    b.ToTable("Tasks");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.Transcript", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("AudioFilePath")
                        .HasColumnType("text");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("MeetingId")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("ProcessedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("VideoFilePath")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("MeetingId");

                    b.ToTable("Transcripts");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("AvatarUrl")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("EmailConfirmationToken")
                        .HasColumnType("text");

                    b.Property<int>("FailedLoginAttempts")
                        .HasColumnType("integer");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsSiteAdmin")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("LastLoginAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("LockoutEndAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("PasswordResetToken")
                        .HasColumnType("text");

                    b.Property<DateTime?>("PasswordResetTokenExpiry")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.RefreshToken", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedByIp")
                        .HasColumnType("text");

                    b.Property<DateTime>("ExpiresAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ReasonRevoked")
                        .HasColumnType("text");

                    b.Property<string>("ReplacedByToken")
                        .HasColumnType("text");

                    b.Property<DateTime?>("RevokedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("RevokedByIp")
                        .HasColumnType("text");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("Token")
                        .IsUnique();

                    b.HasIndex("UserId");

                    b.ToTable("RefreshTokens");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.Invite", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("AcceptedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<DateTime>("ExpiresAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("InvitedById")
                        .HasColumnType("uuid");

                    b.Property<string>("InviteCode")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("InvitePasswordHash")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<Guid?>("CustomRoleId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uuid");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CustomRoleId");

                    b.HasIndex("InviteCode")
                        .IsUnique();

                    b.HasIndex("InvitedById");

                    b.HasIndex("OrganizationId");

                    b.ToTable("Invites");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.MeetingFile", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Category")
                        .HasColumnType("integer");

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<long>("FileSize")
                        .HasColumnType("bigint");

                    b.Property<Guid>("MeetingId")
                        .HasColumnType("uuid");

                    b.Property<string>("OriginalFileName")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("StoragePath")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<Guid>("UploadedById")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("MeetingId");

                    b.HasIndex("UploadedById");

                    b.ToTable("MeetingFiles");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.TeamMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uuid");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasMaxLength(4000)
                        .HasColumnType("character varying(4000)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("OrganizationId", "CreatedAt");

                    b.HasIndex("UserId");

                    b.ToTable("TeamMessages");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.AgendaItem", b =>
                {
                    b.HasOne("SmartMeetingManager.Domain.Entities.Meeting", "Meeting")
                        .WithMany("AgendaItems")
                        .HasForeignKey("MeetingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Meeting");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.Decision", b =>
                {
                    b.HasOne("SmartMeetingManager.Domain.Entities.User", "MadeBy")
                        .WithMany()
                        .HasForeignKey("MadeById")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("SmartMeetingManager.Domain.Entities.Meeting", "Meeting")
                        .WithMany("Decisions")
                        .HasForeignKey("MeetingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MadeBy");

                    b.Navigation("Meeting");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.Integration", b =>
                {
                    b.HasOne("SmartMeetingManager.Domain.Entities.Organization", "Organization")
                        .WithMany()
                        .HasForeignKey("OrganizationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Organization");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.RefreshToken", b =>
                {
                    b.HasOne("SmartMeetingManager.Domain.Entities.User", "User")
                        .WithMany("RefreshTokens")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.Invite", b =>
                {
                    b.HasOne("SmartMeetingManager.Domain.Entities.OrganizationCustomRole", "CustomRole")
                        .WithMany()
                        .HasForeignKey("CustomRoleId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("SmartMeetingManager.Domain.Entities.Organization", "Organization")
                        .WithMany("Invites")
                        .HasForeignKey("OrganizationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SmartMeetingManager.Domain.Entities.User", "InvitedBy")
                        .WithMany()
                        .HasForeignKey("InvitedById")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("CustomRole");

                    b.Navigation("InvitedBy");

                    b.Navigation("Organization");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.MeetingFile", b =>
                {
                    b.HasOne("SmartMeetingManager.Domain.Entities.Meeting", "Meeting")
                        .WithMany("Files")
                        .HasForeignKey("MeetingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SmartMeetingManager.Domain.Entities.User", "UploadedBy")
                        .WithMany()
                        .HasForeignKey("UploadedById")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Meeting");

                    b.Navigation("UploadedBy");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.TeamMessage", b =>
                {
                    b.HasOne("SmartMeetingManager.Domain.Entities.Organization", "Organization")
                        .WithMany()
                        .HasForeignKey("OrganizationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SmartMeetingManager.Domain.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Organization");

                    b.Navigation("User");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.Meeting", b =>
                {
                    b.HasOne("SmartMeetingManager.Domain.Entities.Organization", "Organization")
                        .WithMany("Meetings")
                        .HasForeignKey("OrganizationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SmartMeetingManager.Domain.Entities.User", "Organizer")
                        .WithMany("MeetingsAsOrganizer")
                        .HasForeignKey("OrganizerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SmartMeetingManager.Domain.Entities.Project", "Project")
                        .WithMany("Meetings")
                        .HasForeignKey("ProjectId");

                    b.Navigation("Organization");

                    b.Navigation("Organizer");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.MeetingParticipant", b =>
                {
                    b.HasOne("SmartMeetingManager.Domain.Entities.Meeting", "Meeting")
                        .WithMany("Participants")
                        .HasForeignKey("MeetingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SmartMeetingManager.Domain.Entities.User", "User")
                        .WithMany("MeetingParticipants")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Meeting");

                    b.Navigation("User");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.OrganizationCustomRole", b =>
                {
                    b.HasOne("SmartMeetingManager.Domain.Entities.Organization", "Organization")
                        .WithMany("CustomRoles")
                        .HasForeignKey("OrganizationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Organization");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.OrganizationMember", b =>
                {
                    b.HasOne("SmartMeetingManager.Domain.Entities.Organization", "Organization")
                        .WithMany("Members")
                        .HasForeignKey("OrganizationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SmartMeetingManager.Domain.Entities.OrganizationCustomRole", "CustomRole")
                        .WithMany()
                        .HasForeignKey("CustomRoleId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("SmartMeetingManager.Domain.Entities.User", "User")
                        .WithMany("OrganizationMembers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CustomRole");

                    b.Navigation("Organization");

                    b.Navigation("User");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.Project", b =>
                {
                    b.HasOne("SmartMeetingManager.Domain.Entities.Organization", "Organization")
                        .WithMany("Projects")
                        .HasForeignKey("OrganizationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Organization");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.Task", b =>
                {
                    b.HasOne("SmartMeetingManager.Domain.Entities.User", "AssignedTo")
                        .WithMany("AssignedTasks")
                        .HasForeignKey("AssignedToId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SmartMeetingManager.Domain.Entities.Meeting", "Meeting")
                        .WithMany("Tasks")
                        .HasForeignKey("MeetingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SmartMeetingManager.Domain.Entities.Project", "Project")
                        .WithMany("Tasks")
                        .HasForeignKey("ProjectId");

                    b.Navigation("AssignedTo");

                    b.Navigation("Meeting");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.Transcript", b =>
                {
                    b.HasOne("SmartMeetingManager.Domain.Entities.Meeting", "Meeting")
                        .WithMany("Transcripts")
                        .HasForeignKey("MeetingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Meeting");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.Meeting", b =>
                {
                    b.Navigation("AgendaItems");

                    b.Navigation("Decisions");

                    b.Navigation("Files");

                    b.Navigation("Participants");

                    b.Navigation("Tasks");

                    b.Navigation("Transcripts");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.Organization", b =>
                {
                    b.Navigation("CustomRoles");

                    b.Navigation("Invites");

                    b.Navigation("Meetings");

                    b.Navigation("Members");

                    b.Navigation("Projects");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.Project", b =>
                {
                    b.Navigation("Meetings");

                    b.Navigation("Tasks");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.User", b =>
                {
                    b.Navigation("AssignedTasks");

                    b.Navigation("MeetingParticipants");

                    b.Navigation("MeetingsAsOrganizer");

                    b.Navigation("OrganizationMembers");

                    b.Navigation("RefreshTokens");
                });
#pragma warning restore 612, 618
        }
    }
}
