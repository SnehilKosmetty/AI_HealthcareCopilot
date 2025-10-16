using Microsoft.EntityFrameworkCore;
using AIHealthcareCopilot.Shared.Models;

namespace AIHealthcareCopilot.PatientRecords.API.Data;

public class HealthcareDbContext : DbContext
{
    public HealthcareDbContext(DbContextOptions<HealthcareDbContext> options) : base(options)
    {
    }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<MedicalRecord> MedicalRecords { get; set; }
    public DbSet<AnalysisResult> AnalysisResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Patient entity
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Gender).IsRequired().HasMaxLength(10);
            entity.Property(e => e.MedicalRecordNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ContactInfo).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Index for faster searches
            entity.HasIndex(e => e.MedicalRecordNumber).IsUnique();
            entity.HasIndex(e => new { e.FirstName, e.LastName });
        });

        // Configure Doctor entity
        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Specialization).HasMaxLength(100);
            entity.Property(e => e.LicenseNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Hospital).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Ignore authentication fields (handled by Authentication API)
            entity.Ignore(e => e.PasswordHash);
            entity.Ignore(e => e.RefreshToken);
            entity.Ignore(e => e.RefreshTokenExpiry);
            
            // Indexes
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.LicenseNumber).IsUnique();
        });

        // Configure MedicalRecord entity
        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChiefComplaint).HasMaxLength(500);
            entity.Property(e => e.HistoryOfPresentIllness).HasColumnType("TEXT");
            entity.Property(e => e.PhysicalExamination).HasColumnType("TEXT");
            entity.Property(e => e.Assessment).HasColumnType("TEXT");
            entity.Property(e => e.Plan).HasColumnType("TEXT");
            entity.Property(e => e.Notes).HasColumnType("TEXT");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Foreign key relationships
            entity.HasOne(e => e.Patient)
                  .WithMany(p => p.MedicalRecords)
                  .HasForeignKey(e => e.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Doctor)
                  .WithMany(d => d.MedicalRecords)
                  .HasForeignKey(e => e.DoctorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure AnalysisResult entity
        modelBuilder.Entity<AnalysisResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AnalysisType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Result).HasColumnType("TEXT");
            entity.Property(e => e.Confidence).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Foreign key relationship
            entity.HasOne(e => e.MedicalRecord)
                  .WithMany(mr => mr.AnalysisResults)
                  .HasForeignKey(e => e.MedicalRecordId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}