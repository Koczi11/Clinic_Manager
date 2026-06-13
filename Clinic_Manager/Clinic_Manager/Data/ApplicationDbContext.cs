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
    }
}
