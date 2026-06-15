using Clinic_Manager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Manager.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {

    }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<Procedure> Procedures => Set<Procedure>();
    public DbSet<ProcedurePerformed> ProceduresPerformed => Set<ProcedurePerformed>();
    public DbSet<ClinicalNote> ClinicalNotes => Set<ClinicalNote>();
    public DbSet<PrescribedMedication> PrescribedMedications => Set<PrescribedMedication>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Patient>(entity =>
        {
            entity.HasQueryFilter(p => !p.IsDeleted);

            entity.HasIndex(p => p.Pesel);

            entity.HasIndex(p => p.LastName);
        });

        builder.Entity<Medication>(entity =>
        {
            entity.Property(m => m.UnitPrice).HasPrecision(18, 2);

            entity.HasIndex(m => m.Name);
        });

        builder.Entity<MedicalRecord>(entity =>
        {
            entity.HasOne(mr => mr.Patient)
                .WithMany()
                .HasForeignKey(mr => mr.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(mr => mr.PatientId);
        });

        builder.Entity<Visit>(entity =>
        {
            entity.HasOne(v => v.Patient)
                .WithMany()
                .HasForeignKey(v => v.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(v => v.Doctor)
                .WithMany()
                .HasForeignKey(v => v.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(v => v.PatientId);
            entity.HasIndex(v => v.DoctorId);
            entity.HasIndex(v => v.VisitDate);

            entity.HasIndex(v => new { v.DoctorId, v.VisitDate })
                .HasDatabaseName("IX_Visits_DoctorId_VisitDate");
        });

        builder.Entity<Procedure>(entity =>
        {
            entity.Property(p => p.Cost).HasPrecision(18, 2);
            entity.HasIndex(p => p.Name);
        });

        builder.Entity<ProcedurePerformed>(entity =>
        {
            entity.HasOne(pp => pp.Visit)
                .WithMany(v => v.ProceduresPerformed)
                .HasForeignKey(pp => pp.VisitId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pp => pp.Procedure)
                .WithMany()
                .HasForeignKey(pp => pp.ProcedureId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(pp => pp.VisitId);
            entity.HasIndex(pp => pp.ProcedureId);
        });

        builder.Entity<ClinicalNote>(entity =>
        {
            entity.HasOne(cn => cn.Visit)
                .WithMany(v => v.ClinicalNotes)
                .HasForeignKey(cn => cn.VisitId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(cn => cn.VisitId);
        });

        builder.Entity<PrescribedMedication>(entity =>
        {
            entity.HasOne(pm => pm.Visit)
                .WithMany(v => v.PrescribedMedications)
                .HasForeignKey(pm => pm.VisitId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pm => pm.Medication)
                .WithMany()
                .HasForeignKey(pm => pm.MedicationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(pm => pm.VisitId);
            entity.HasIndex(pm => pm.MedicationId);
        });
    }
}
