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

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Organizations");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.OrganizationMember", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("JoinedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uuid");

                    b.Property<int>("Role")
                        .HasColumnType("integer");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

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

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Users");
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

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.OrganizationMember", b =>
                {
                    b.HasOne("SmartMeetingManager.Domain.Entities.Organization", "Organization")
                        .WithMany("Members")
                        .HasForeignKey("OrganizationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SmartMeetingManager.Domain.Entities.User", "User")
                        .WithMany("OrganizationMembers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

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

                    b.Navigation("Participants");

                    b.Navigation("Tasks");

                    b.Navigation("Transcripts");
                });

            modelBuilder.Entity("SmartMeetingManager.Domain.Entities.Organization", b =>
                {
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
                });
#pragma warning restore 612, 618
        }
    }
}
