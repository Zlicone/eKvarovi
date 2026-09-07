using eKvarovi.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Data;

public class EKvaroviDbContext : DbContext
{
    public EKvaroviDbContext(DbContextOptions<EKvaroviDbContext> options)
        : base(options)
    {
    }

    // Šifrarnici
    public DbSet<LocationType> LocationTypes => Set<LocationType>();
    public DbSet<FaultType> FaultTypes => Set<FaultType>();
    public DbSet<FaultPriority> FaultPriorities => Set<FaultPriority>();
    public DbSet<FaultStatus> FaultStatuses => Set<FaultStatus>();
    public DbSet<InterventionStatus> InterventionStatuses => Set<InterventionStatus>();
    public DbSet<MaterialUnit> MaterialUnits => Set<MaterialUnit>();
    public DbSet<AttachmentPurpose> AttachmentPurposes => Set<AttachmentPurpose>();

    // Poslovne tablice
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<FaultReport> FaultReports => Set<FaultReport>();
    public DbSet<FaultAssignment> FaultAssignments => Set<FaultAssignment>();
    public DbSet<Intervention> Interventions => Set<Intervention>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<InterventionMaterial> InterventionMaterials => Set<InterventionMaterial>();
    public DbSet<FaultAttachment> FaultAttachments => Set<FaultAttachment>();

    // Korisnici
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<AppRole> AppRoles => Set<AppRole>();
    public DbSet<AppUserRole> AppUserRoles => Set<AppUserRole>();

    // Bonus
    public DbSet<FaultReportEvent> FaultReportEvents => Set<FaultReportEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- ŠIFRARNICI ----------
        modelBuilder.Entity<LocationType>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<FaultType>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<FaultPriority>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
            e.HasIndex(x => x.Rank).IsUnique();
        });

        modelBuilder.Entity<FaultStatus>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(30).IsRequired();
            e.Property(x => x.Name).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<InterventionStatus>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(30).IsRequired();
            e.Property(x => x.Name).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<MaterialUnit>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(50).IsRequired();
            e.Property(x => x.Abbreviation).HasMaxLength(10).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<AttachmentPurpose>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(30).IsRequired();
            e.Property(x => x.Name).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
        });

        // ---------- LOKACIJE I ZAPOSLENICI ----------
        modelBuilder.Entity<Location>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Address).HasMaxLength(200).IsRequired();
            e.Property(x => x.City).HasMaxLength(100).IsRequired();

            e.HasOne(x => x.LocationType)
             .WithMany(t => t.Locations)
             .HasForeignKey(x => x.LocationTypeId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Employee>(e =>
        {
            e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(50);
            e.HasIndex(x => x.Email).IsUnique();

            e.HasOne(x => x.Location)
             .WithMany(l => l.Employees)
             .HasForeignKey(x => x.LocationId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- PRIJAVE ----------
        modelBuilder.Entity<FaultReport>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).IsRequired();

            e.HasOne(x => x.Location)
             .WithMany(l => l.FaultReports)
             .HasForeignKey(x => x.LocationId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Reporter)
             .WithMany(emp => emp.ReportedFaults)
             .HasForeignKey(x => x.ReporterId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.ClosedByEmployee)
             .WithMany()
             .HasForeignKey(x => x.ClosedByEmployeeId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.FaultType)
             .WithMany(t => t.FaultReports)
             .HasForeignKey(x => x.FaultTypeId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Priority)
             .WithMany(p => p.FaultReports)
             .HasForeignKey(x => x.PriorityId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Status)
             .WithMany(s => s.FaultReports)
             .HasForeignKey(x => x.StatusId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.StatusId);
            e.HasIndex(x => x.LocationId);
            e.HasIndex(x => x.CreatedAt);
        });

        // ---------- DODJELE ----------
        modelBuilder.Entity<FaultAssignment>(e =>
        {
            e.Property(x => x.Note).HasMaxLength(500);

            e.HasOne(x => x.FaultReport)
             .WithMany(r => r.Assignments)
             .HasForeignKey(x => x.FaultReportId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Technician)
             .WithMany(emp => emp.Assignments)
             .HasForeignKey(x => x.TechnicianId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.AssignedByEmployee)
             .WithMany()
             .HasForeignKey(x => x.AssignedByEmployeeId)
             .OnDelete(DeleteBehavior.Restrict);

            // NAJVIŠE JEDNA AKTIVNA DODJELA PO PRIJAVI
            e.HasIndex(x => x.FaultReportId)
             .IsUnique()
             .HasFilter("\"UnassignedAt\" IS NULL");

            e.HasIndex(x => x.TechnicianId);
        });

        // ---------- INTERVENCIJE ----------
        modelBuilder.Entity<Intervention>(e =>
        {
            e.HasOne(x => x.FaultAssignment)
             .WithMany(a => a.Interventions)
             .HasForeignKey(x => x.FaultAssignmentId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Status)
             .WithMany(s => s.Interventions)
             .HasForeignKey(x => x.StatusId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- MATERIJALI ----------
        modelBuilder.Entity<Material>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();

            e.HasOne(x => x.Unit)
             .WithMany(u => u.Materials)
             .HasForeignKey(x => x.UnitId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InterventionMaterial>(e =>
        {
            e.Property(x => x.Quantity).HasConversion<double>();

            e.HasOne(x => x.Intervention)
             .WithMany(i => i.Materials)
             .HasForeignKey(x => x.InterventionId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Material)
             .WithMany(m => m.InterventionMaterials)
             .HasForeignKey(x => x.MaterialId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.InterventionId, x.MaterialId }).IsUnique();
        });

        // ---------- PRIVITCI ----------
        modelBuilder.Entity<FaultAttachment>(e =>
        {
            e.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
            e.Property(x => x.StoredFileName).HasMaxLength(255).IsRequired();
            e.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.StoredFileName).IsUnique();

            e.HasOne(x => x.FaultReport)
             .WithMany(r => r.Attachments)
             .HasForeignKey(x => x.FaultReportId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Intervention)
             .WithMany(i => i.Attachments)
             .HasForeignKey(x => x.InterventionId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Purpose)
             .WithMany(p => p.Attachments)
             .HasForeignKey(x => x.PurposeId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- KORISNICI ----------
        modelBuilder.Entity<AppUser>(e =>
        {
            e.Property(x => x.Username).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();

            e.HasIndex(x => x.Username).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.EmployeeId).IsUnique();

            e.HasOne(x => x.Employee)
             .WithOne(emp => emp.AppUser)
             .HasForeignKey<AppUser>(x => x.EmployeeId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AppRole>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<AppUserRole>(e =>
        {
            e.HasKey(x => new { x.AppUserId, x.AppRoleId });

            e.HasOne(x => x.AppUser)
             .WithMany(u => u.UserRoles)
             .HasForeignKey(x => x.AppUserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.AppRole)
             .WithMany(r => r.UserRoles)
             .HasForeignKey(x => x.AppRoleId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- BONUS: VREMENSKA CRTA ----------
        modelBuilder.Entity<FaultReportEvent>(e =>
        {
            e.Property(x => x.EventType).HasMaxLength(50).IsRequired();
            e.Property(x => x.OldValue).HasMaxLength(200);
            e.Property(x => x.NewValue).HasMaxLength(200);

            e.HasOne(x => x.FaultReport)
             .WithMany(r => r.Events)
             .HasForeignKey(x => x.FaultReportId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.ChangedByAppUser)
             .WithMany()
             .HasForeignKey(x => x.ChangedByAppUserId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}