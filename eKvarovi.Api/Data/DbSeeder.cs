using eKvarovi.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Data;

public static class DbSeeder
{
    public static void Seed(EKvaroviDbContext db)
    {
        db.Database.Migrate();

        SeedLookups(db);
        SeedRoles(db);
        SeedLocations(db);
        SeedEmployees(db);
        SeedMaterials(db);
        SeedDemoReports(db);
        SeedUsers(db);
    }

    private static void SeedUsers(EKvaroviDbContext db)
    {
        if (db.AppUsers.Any()) return;

        var hasher = new PasswordHasher<AppUser>();
        var now = DateTime.UtcNow;

        var users = new List<(string Username, string Email, int? EmployeeId, int[] RoleIds)>
    {
        ("admin",    "admin@zadarska-zupanija.hr",    null, new[] { 1 }),
        ("upravitelj","upravitelj@zadarska-zupanija.hr", 2,  new[] { 2, 4 }),
        ("izvrsitelj","izvrsitelj@zadarska-zupanija.hr", 5,  new[] { 3, 4 }),
        ("prijavitelj","prijavitelj@zadarska-zupanija.hr", 3, new[] { 4 })
    };

        foreach (var (username, email, employeeId, roleIds) in users)
        {
            var user = new AppUser
            {
                Username = username,
                Email = email,
                EmployeeId = employeeId,
                IsActive = true,
                CreatedAt = now
            };

            user.PasswordHash = hasher.HashPassword(user, "Demo1234!");

            db.AppUsers.Add(user);
            db.SaveChanges();

            foreach (var roleId in roleIds)
                db.AppUserRoles.Add(new AppUserRole { AppUserId = user.Id, AppRoleId = roleId });
        }

        db.SaveChanges();
    }

    private static void SeedLookups(EKvaroviDbContext db)
    {
        if (!db.LocationTypes.Any())
        {
            db.LocationTypes.AddRange(
                new LocationType { Id = 1, Name = "Upravna zgrada" },
                new LocationType { Id = 2, Name = "Škola" },
                new LocationType { Id = 3, Name = "Zdravstvena ustanova" },
                new LocationType { Id = 4, Name = "Skladište" });
        }

        if (!db.FaultTypes.Any())
        {
            db.FaultTypes.AddRange(
                new FaultType { Id = 1, Name = "Elektrika" },
                new FaultType { Id = 2, Name = "Voda" },
                new FaultType { Id = 3, Name = "Grijanje" },
                new FaultType { Id = 4, Name = "Mreža" },
                new FaultType { Id = 5, Name = "Građevinski radovi" },
                new FaultType { Id = 6, Name = "Ostalo" });
        }

        if (!db.FaultPriorities.Any())
        {
            db.FaultPriorities.AddRange(
                new FaultPriority { Id = 1, Name = "Nizak", Rank = 1 },
                new FaultPriority { Id = 2, Name = "Srednji", Rank = 2 },
                new FaultPriority { Id = 3, Name = "Visok", Rank = 3 },
                new FaultPriority { Id = 4, Name = "Kritičan", Rank = 4 });
        }

        if (!db.FaultStatuses.Any())
        {
            db.FaultStatuses.AddRange(
                new FaultStatus { Id = 1, Code = FaultStatusCodes.Received, Name = "Zaprimljeno", Rank = 1 },
                new FaultStatus { Id = 2, Code = FaultStatusCodes.Reviewed, Name = "Pregledano", Rank = 2 },
                new FaultStatus { Id = 3, Code = FaultStatusCodes.Assigned, Name = "Dodijeljeno", Rank = 3 },
                new FaultStatus { Id = 4, Code = FaultStatusCodes.InProgress, Name = "U radu", Rank = 4 },
                new FaultStatus { Id = 5, Code = FaultStatusCodes.Resolved, Name = "Riješeno", Rank = 5 },
                new FaultStatus { Id = 6, Code = FaultStatusCodes.Closed, Name = "Zatvoreno", Rank = 6 });
        }

        if (!db.InterventionStatuses.Any())
        {
            db.InterventionStatuses.AddRange(
                new InterventionStatus { Id = 1, Code = InterventionStatusCodes.Planned, Name = "Planirana" },
                new InterventionStatus { Id = 2, Code = InterventionStatusCodes.InProgress, Name = "U tijeku" },
                new InterventionStatus { Id = 3, Code = InterventionStatusCodes.Completed, Name = "Završena" },
                new InterventionStatus { Id = 4, Code = InterventionStatusCodes.Failed, Name = "Neuspješna" });
        }

        if (!db.MaterialUnits.Any())
        {
            db.MaterialUnits.AddRange(
                new MaterialUnit { Id = 1, Name = "Komad", Abbreviation = "kom" },
                new MaterialUnit { Id = 2, Name = "Metar", Abbreviation = "m" },
                new MaterialUnit { Id = 3, Name = "Litra", Abbreviation = "l" },
                new MaterialUnit { Id = 4, Name = "Kilogram", Abbreviation = "kg" },
                new MaterialUnit { Id = 5, Name = "Paket", Abbreviation = "pak" });
        }

        if (!db.AttachmentPurposes.Any())
        {
            db.AttachmentPurposes.AddRange(
                new AttachmentPurpose { Id = 1, Code = AttachmentPurposeCodes.PhotoBefore, Name = "Fotografija prije rada" },
                new AttachmentPurpose { Id = 2, Code = AttachmentPurposeCodes.PhotoAfter, Name = "Fotografija nakon rada" },
                new AttachmentPurpose { Id = 3, Code = AttachmentPurposeCodes.Document, Name = "Dokument" });
        }

        db.SaveChanges();
    }

    private static void SeedRoles(EKvaroviDbContext db)
    {
        if (db.AppRoles.Any()) return;

        db.AppRoles.AddRange(
            new AppRole { Id = 1, Name = RoleNames.Admin },
            new AppRole { Id = 2, Name = RoleNames.Manager },
            new AppRole { Id = 3, Name = RoleNames.Technician },
            new AppRole { Id = 4, Name = RoleNames.Reporter });

        db.SaveChanges();
    }

    private static void SeedLocations(EKvaroviDbContext db)
    {
        if (db.Locations.Any()) return;

        var now = DateTime.UtcNow;

        db.Locations.AddRange(
            new Location { Id = 1, Name = "Županijska palača", Address = "Trg Petra Zoranića 1", City = "Zadar", LocationTypeId = 1, IsActive = true, CreatedAt = now },
            new Location { Id = 2, Name = "OŠ Petra Preradovića", Address = "Ulica kralja Tomislava 14", City = "Zadar", LocationTypeId = 2, IsActive = true, CreatedAt = now },
            new Location { Id = 3, Name = "Dom zdravlja Benkovac", Address = "Šetalište kneza Branimira 3", City = "Benkovac", LocationTypeId = 3, IsActive = true, CreatedAt = now },
            new Location { Id = 4, Name = "Središnje skladište", Address = "Gaženička cesta 22", City = "Zadar", LocationTypeId = 4, IsActive = true, CreatedAt = now },
            new Location { Id = 5, Name = "Stara uprava (zatvoreno)", Address = "Obala kneza Trpimira 5", City = "Zadar", LocationTypeId = 1, IsActive = false, CreatedAt = now });

        db.SaveChanges();
    }

    private static void SeedEmployees(EKvaroviDbContext db)
    {
        if (db.Employees.Any()) return;

        var now = DateTime.UtcNow;

        db.Employees.AddRange(
            new Employee { Id = 1, FirstName = "Ivana", LastName = "Marić", Email = "ivana.maric@zadarska-zupanija.hr", Phone = "023111222", LocationId = 1, IsActive = true, CreatedAt = now },
            new Employee { Id = 2, FirstName = "Petar", LastName = "Jurić", Email = "petar.juric@zadarska-zupanija.hr", Phone = "023111223", LocationId = 1, IsActive = true, CreatedAt = now },
            new Employee { Id = 3, FirstName = "Marina", LastName = "Kovač", Email = "marina.kovac@zadarska-zupanija.hr", Phone = "023111224", LocationId = 2, IsActive = true, CreatedAt = now },
            new Employee { Id = 4, FirstName = "Josip", LastName = "Babić", Email = "josip.babic@zadarska-zupanija.hr", Phone = "023111225", LocationId = 3, IsActive = true, CreatedAt = now },
            new Employee { Id = 5, FirstName = "Ante", LastName = "Vuković", Email = "ante.vukovic@zadarska-zupanija.hr", Phone = "023111226", LocationId = 4, IsActive = true, CreatedAt = now },
            new Employee { Id = 6, FirstName = "Damir", LastName = "Perić", Email = "damir.peric@zadarska-zupanija.hr", Phone = "023111227", LocationId = 4, IsActive = true, CreatedAt = now },
            new Employee { Id = 7, FirstName = "Nikolina", LastName = "Šarić", Email = "nikolina.saric@zadarska-zupanija.hr", Phone = "023111228", LocationId = 4, IsActive = true, CreatedAt = now },
            new Employee { Id = 8, FirstName = "Tomislav", LastName = "Grgić", Email = "tomislav.grgic@zadarska-zupanija.hr", Phone = "023111229", LocationId = 1, IsActive = true, CreatedAt = now });

        db.SaveChanges();
    }

    private static void SeedMaterials(EKvaroviDbContext db)
    {
        if (db.Materials.Any()) return;

        db.Materials.AddRange(
            new Material { Id = 1, Name = "LED žarulja E27", UnitId = 1, IsActive = true },
            new Material { Id = 2, Name = "Prekidač jednopolni", UnitId = 1, IsActive = true },
            new Material { Id = 3, Name = "Instalacijski kabel 3x1.5", UnitId = 2, IsActive = true },
            new Material { Id = 4, Name = "UTP kabel Cat6", UnitId = 2, IsActive = true },
            new Material { Id = 5, Name = "RJ45 konektor", UnitId = 1, IsActive = true },
            new Material { Id = 6, Name = "Silikon sanitarni", UnitId = 1, IsActive = true },
            new Material { Id = 7, Name = "Brtva za slavinu", UnitId = 1, IsActive = true },
            new Material { Id = 8, Name = "Kvaka s rozetom", UnitId = 1, IsActive = true },
            new Material { Id = 9, Name = "Antifriz za grijanje", UnitId = 3, IsActive = true },
            new Material { Id = 10, Name = "Vijci i tiple set", UnitId = 5, IsActive = true });

        db.SaveChanges();
    }

    private static void SeedDemoReports(EKvaroviDbContext db)
    {
        if (db.FaultReports.Any()) return;

        var now = DateTime.UtcNow;

        db.FaultReports.AddRange(
            new FaultReport
            {
                Title = "Ne radi klima u uredu 204",
                Description = "Klima uređaj se pali, ali ne hladi. Čuje se glasno zujanje.",
                LocationId = 1,
                ReporterId = 1,
                StatusId = 1,
                CreatedAt = now.AddDays(-6)
            },
            new FaultReport
            {
                Title = "Curi slavina u čajnoj kuhinji",
                Description = "Slavina kaplje neprestano, ispod sudopera je mokro.",
                LocationId = 2,
                ReporterId = 3,
                StatusId = 1,
                CreatedAt = now.AddDays(-4)
            },
            new FaultReport
            {
                Title = "Nema mreže u zbornici",
                Description = "Mrežna utičnica ne daje vezu, testirano s dva laptopa.",
                LocationId = 2,
                ReporterId = 3,
                StatusId = 1,
                CreatedAt = now.AddDays(-3)
            },
            new FaultReport
            {
                Title = "Slomljena kvaka na ulaznim vratima",
                Description = "Kvaka se okreće u prazno, vrata se teško otvaraju izvana.",
                LocationId = 3,
                ReporterId = 4,
                StatusId = 1,
                CreatedAt = now.AddDays(-2)
            },
            new FaultReport
            {
                Title = "Pregorjela rasvjeta u hodniku",
                Description = "Tri stropne svjetiljke ne rade, hodnik je mračan.",
                LocationId = 1,
                ReporterId = 2,
                StatusId = 1,
                CreatedAt = now.AddDays(-1)
            });

        db.SaveChanges();
    }
}