using KQAlumni.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace KQAlumni.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for KQ Alumni Association
/// Manages database connections and entity mappings
/// </summary>
public class AppDbContext : DbContext
{
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // ========================================
        // DbSets (Database Tables)
        // ========================================

        /// <summary>
        /// Alumni registrations table
        /// </summary>
        public DbSet<AlumniRegistration> AlumniRegistrations { get; set; } = null!;

        /// <summary>
        /// Admin users table (for HR dashboard access)
        /// </summary>
        public DbSet<AdminUser> AdminUsers { get; set; } = null!;

        /// <summary>
        /// Audit logs table (tracks all admin actions)
        /// </summary>
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        /// <summary>
        /// Email delivery logs table (tracks all email sending attempts)
        /// </summary>
        public DbSet<EmailLog> EmailLogs { get; set; } = null!;

        // ========================================
        // Model Configuration
        // ========================================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
                base.OnModelCreating(modelBuilder);

                // Configure AlumniRegistration entity
                modelBuilder.Entity<AlumniRegistration>(entity =>
                {
                        // ========================================
                        // TABLE NAME
                        // ========================================
                        entity.ToTable("AlumniRegistrations");

                        // ========================================
                        // PRIMARY KEY
                        // ========================================
                        entity.HasKey(e => e.Id);

                        // ========================================
                        // UNIQUE CONSTRAINTS
                        // ========================================

                        // Staff Number (unique, required)
                        entity.HasIndex(e => e.StaffNumber)
                    .IsUnique()
                    .HasDatabaseName("UQ_AlumniRegistrations_StaffNumber");

                        // Email (unique, required)
                        entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasDatabaseName("UQ_AlumniRegistrations_Email");

                        // Mobile Number (unique if provided, composite index on CountryCode + Number)
                        // Allows multiple NULL values (if mobile not provided)
                        entity.HasIndex(e => new { e.MobileCountryCode, e.MobileNumber })
                    .IsUnique()
                    .HasDatabaseName("UQ_AlumniRegistrations_Mobile")
                    .HasFilter("[MobileCountryCode] IS NOT NULL AND [MobileNumber] IS NOT NULL");

                        // LinkedIn Profile (unique if provided)
                        // Allows multiple NULL values (if LinkedIn not provided)
                        entity.HasIndex(e => e.LinkedInProfile)
                    .IsUnique()
                    .HasDatabaseName("UQ_AlumniRegistrations_LinkedIn")
                    .HasFilter("[LinkedInProfile] IS NOT NULL");

                        // ========================================
                        // REGULAR INDEXES (for query performance)
                        // ========================================

                        entity.HasIndex(e => e.RegistrationStatus)
                    .HasDatabaseName("IX_AlumniRegistrations_RegistrationStatus");

                        entity.HasIndex(e => e.CreatedAt)
                    .IsDescending()
                    .HasDatabaseName("IX_AlumniRegistrations_CreatedAt");

                        entity.HasIndex(e => e.ErpValidated)
                    .HasDatabaseName("IX_AlumniRegistrations_ErpValidated");

                        entity.HasIndex(e => e.RequiresManualReview)
                    .HasDatabaseName("IX_AlumniRegistrations_RequiresManualReview");

                        entity.HasIndex(e => e.ManuallyReviewed)
                    .HasDatabaseName("IX_AlumniRegistrations_ManuallyReviewed");

                        // ========================================
                        // COMPOSITE INDEXES (for query performance)
                        // ========================================

                        // Status + CreatedAt: For sorting registrations within a status
                        entity.HasIndex(e => new { e.RegistrationStatus, e.CreatedAt })
                    .IsDescending(false, true)
                    .HasDatabaseName("IX_AlumniRegistrations_Status_CreatedAt");

                        // Manual review filtering: For finding pending manual reviews
                        entity.HasIndex(e => new { e.RequiresManualReview, e.ManuallyReviewed, e.RegistrationStatus })
                    .HasDatabaseName("IX_AlumniRegistrations_ManualReview_Filter");

                        // Email verification + Status: For filtering by verification status
                        entity.HasIndex(e => new { e.ErpValidated, e.RegistrationStatus })
                    .HasDatabaseName("IX_AlumniRegistrations_Validated_Status");

                        // ========================================
                        // DEFAULT VALUES
                        // ========================================

                        entity.Property(e => e.Id)
                    .HasDefaultValueSql("NEWID()");

                        entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                        entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                        entity.Property(e => e.RegistrationStatus)
                    .HasDefaultValue("Pending"); // Changed from "Verified" to "Pending"

                        entity.Property(e => e.ConsentGiven)
                    .HasDefaultValue(false);

                        entity.Property(e => e.ErpValidated)
                    .HasDefaultValue(false);

                        // ========================================
                        // REQUIRED FIELDS
                        // ========================================

                        entity.Property(e => e.StaffNumber).IsRequired();
                        entity.Property(e => e.FullName).IsRequired();
                        entity.Property(e => e.Email).IsRequired();
                        entity.Property(e => e.CurrentCountry).IsRequired();
                        entity.Property(e => e.CurrentCountryCode).IsRequired();
                        entity.Property(e => e.CurrentCity).IsRequired();
                        entity.Property(e => e.QualificationsAttained).IsRequired();
                        entity.Property(e => e.EngagementPreferences).IsRequired();
                        entity.Property(e => e.ConsentGiven).IsRequired();
                        entity.Property(e => e.RegistrationStatus).IsRequired();

                        // ========================================
                        // CHECK CONSTRAINTS
                        // ========================================

                        // Consent must be true (user must explicitly consent)
                        entity.ToTable(t => t.HasCheckConstraint(
                    "CK_AlumniRegistrations_ConsentRequired",
                    "[ConsentGiven] = 1"
                ));
                });

                // ========================================
                // Configure AdminUser entity
                // ========================================
                modelBuilder.Entity<AdminUser>(entity =>
                {
                        entity.ToTable("AdminUsers");

                        entity.HasKey(e => e.Id);

                        // Username must be unique
                        entity.HasIndex(e => e.Username)
                            .IsUnique()
                            .HasDatabaseName("UQ_AdminUsers_Username");

                        // Email must be unique
                        entity.HasIndex(e => e.Email)
                            .IsUnique()
                            .HasDatabaseName("UQ_AdminUsers_Email");

                        // Index on IsActive for filtering active users
                        entity.HasIndex(e => e.IsActive)
                            .HasDatabaseName("IX_AdminUsers_IsActive");

                        // Index on Role for role-based queries
                        entity.HasIndex(e => e.Role)
                            .HasDatabaseName("IX_AdminUsers_Role");

                        // Index on LastLoginAt for tracking recent logins
                        entity.HasIndex(e => e.LastLoginAt)
                            .IsDescending()
                            .HasDatabaseName("IX_AdminUsers_LastLoginAt");

                        // Composite index: Username + IsActive for quick active user lookups
                        entity.HasIndex(e => new { e.Username, e.IsActive })
                            .HasDatabaseName("IX_AdminUsers_Username_IsActive");

                        // Composite index: Email + IsActive for email lookups
                        entity.HasIndex(e => new { e.Email, e.IsActive })
                            .HasDatabaseName("IX_AdminUsers_Email_IsActive");

                        // Required fields
                        entity.Property(e => e.Username).IsRequired();
                        entity.Property(e => e.Email).IsRequired();
                        entity.Property(e => e.PasswordHash).IsRequired();
                        entity.Property(e => e.Role).IsRequired();
                        entity.Property(e => e.FullName).IsRequired();

                        // Default values
                        entity.Property(e => e.IsActive).HasDefaultValue(true);
                        entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                        entity.Property(e => e.Role).HasDefaultValue("HROfficer");
                });

                // ========================================
                // Configure AuditLog entity
                // ========================================
                modelBuilder.Entity<AuditLog>(entity =>
                {
                        entity.ToTable("AuditLogs");

                        entity.HasKey(e => e.Id);

                        // Foreign key relationship to AlumniRegistration
                        entity.HasOne(e => e.Registration)
                            .WithMany(r => r.AuditLogs)
                            .HasForeignKey(e => e.RegistrationId)
                            .OnDelete(DeleteBehavior.Cascade);

                        // Foreign key relationship to AdminUser (nullable)
                        entity.HasOne(e => e.AdminUser)
                            .WithMany(u => u.AuditLogs)
                            .HasForeignKey(e => e.AdminUserId)
                            .OnDelete(DeleteBehavior.SetNull);

                        // Index on RegistrationId for filtering logs by registration
                        entity.HasIndex(e => e.RegistrationId)
                            .HasDatabaseName("IX_AuditLogs_RegistrationId");

                        // Index on AdminUserId for filtering logs by admin user
                        entity.HasIndex(e => e.AdminUserId)
                            .HasDatabaseName("IX_AuditLogs_AdminUserId");

                        // Index on Timestamp for chronological queries
                        entity.HasIndex(e => e.Timestamp)
                            .IsDescending()
                            .HasDatabaseName("IX_AuditLogs_Timestamp");

                        // Index on Action for filtering by action type
                        entity.HasIndex(e => e.Action)
                            .HasDatabaseName("IX_AuditLogs_Action");

                        // Index on IsAutomated for filtering automated vs manual actions
                        entity.HasIndex(e => e.IsAutomated)
                            .HasDatabaseName("IX_AuditLogs_IsAutomated");

                        // ========================================
                        // COMPOSITE INDEXES (for query performance)
                        // ========================================

                        // RegistrationId + Timestamp: For efficient audit log queries per registration
                        entity.HasIndex(e => new { e.RegistrationId, e.Timestamp })
                            .IsDescending(false, true)
                            .HasDatabaseName("IX_AuditLogs_Registration_Timestamp");

                        // AdminUserId + Timestamp: For tracking admin user activity
                        entity.HasIndex(e => new { e.AdminUserId, e.Timestamp })
                            .IsDescending(false, true)
                            .HasDatabaseName("IX_AuditLogs_AdminUser_Timestamp");

                        // Action + Timestamp: For filtering by action type with chronological order
                        entity.HasIndex(e => new { e.Action, e.Timestamp })
                            .IsDescending(false, true)
                            .HasDatabaseName("IX_AuditLogs_Action_Timestamp");

                        // Required fields
                        entity.Property(e => e.Action).IsRequired();
                        entity.Property(e => e.PerformedBy).IsRequired();

                        // Default values
                        entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");
                        entity.Property(e => e.IsAutomated).HasDefaultValue(false);
                });

                // ========================================
                // Configure EmailLog entity
                // ========================================
                modelBuilder.Entity<EmailLog>(entity =>
                {
                        entity.ToTable("EmailLogs");

                        entity.HasKey(e => e.Id);

                        // Foreign key relationship to AlumniRegistration (nullable)
                        entity.HasOne(e => e.Registration)
                            .WithMany()
                            .HasForeignKey(e => e.RegistrationId)
                            .OnDelete(DeleteBehavior.SetNull);

                        // Index on RegistrationId for filtering logs by registration
                        entity.HasIndex(e => e.RegistrationId)
                            .HasDatabaseName("IX_EmailLogs_RegistrationId");

                        // Index on ToEmail for filtering by recipient
                        entity.HasIndex(e => e.ToEmail)
                            .HasDatabaseName("IX_EmailLogs_ToEmail");

                        // Index on SentAt for chronological queries
                        entity.HasIndex(e => e.SentAt)
                            .IsDescending()
                            .HasDatabaseName("IX_EmailLogs_SentAt");

                        // Index on Status for filtering by delivery status
                        entity.HasIndex(e => e.Status)
                            .HasDatabaseName("IX_EmailLogs_Status");

                        // Index on EmailType for filtering by type
                        entity.HasIndex(e => e.EmailType)
                            .HasDatabaseName("IX_EmailLogs_EmailType");

                        // ========================================
                        // COMPOSITE INDEXES (for query performance)
                        // ========================================

                        // Status + SentAt: For efficient delivery status queries
                        entity.HasIndex(e => new { e.Status, e.SentAt })
                            .IsDescending(false, true)
                            .HasDatabaseName("IX_EmailLogs_Status_SentAt");

                        // EmailType + Status: For monitoring email type success rates
                        entity.HasIndex(e => new { e.EmailType, e.Status })
                            .HasDatabaseName("IX_EmailLogs_EmailType_Status");

                        // RegistrationId + SentAt: For tracking emails per registration
                        entity.HasIndex(e => new { e.RegistrationId, e.SentAt })
                            .IsDescending(false, true)
                            .HasDatabaseName("IX_EmailLogs_Registration_SentAt");

                        // Required fields
                        entity.Property(e => e.ToEmail).IsRequired();
                        entity.Property(e => e.Subject).IsRequired();
                        entity.Property(e => e.EmailType).IsRequired();
                        entity.Property(e => e.Status).IsRequired();

                        // Default values
                        entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
                        entity.Property(e => e.SentAt).HasDefaultValueSql("GETUTCDATE()");
                        entity.Property(e => e.RetryCount).HasDefaultValue(0);
                });
        }

        // ========================================
        // SaveChanges Override (Auto-update UpdatedAt)
        // ========================================

        public override int SaveChanges()
        {
                UpdateTimestamps();
                return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
                UpdateTimestamps();
                return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Automatically updates UpdatedAt timestamp for modified entities
        /// </summary>
        private void UpdateTimestamps()
        {
                var entries = ChangeTracker.Entries()
                    .Where(e => e.Entity is AlumniRegistration &&
                               (e.State == EntityState.Added || e.State == EntityState.Modified));

                foreach (var entry in entries)
                {
                        var entity = (AlumniRegistration)entry.Entity;
                        entity.UpdatedAt = DateTime.UtcNow;

                        if (entry.State == EntityState.Added && entity.CreatedAt == default)
                        {
                                entity.CreatedAt = DateTime.UtcNow;
                        }
                }
        }
}
