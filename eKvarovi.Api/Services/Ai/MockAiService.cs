using System.Text;
using eKvarovi.Api.Data;
using eKvarovi.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Services.Ai;

public class MockAiService : IAiService
{
    private readonly EKvaroviDbContext _db;

    public MockAiService(EKvaroviDbContext db)
    {
        _db = db;
    }

    public string ProviderName => "Mock";

    private static readonly Dictionary<string, string[]> TypeKeywords = new()
    {
        ["Elektrika"] = new[]
        {
            "struj", "elektr", "rasvjet", "žarul", "zarul", "utičnic", "uticnic",
            "prekidač", "prekidac", "osigurač", "osigurac", "iskri", "kabel", "svjetl"
        },
        ["Voda"] = new[]
        {
            "vod", "curi", "cijev", "slavin", "sudoper", "wc", "kanaliz",
            "poplav", "odvod", "bojler", "kaplje", "vlag"
        },
        ["Grijanje"] = new[]
        {
            "grijanj", "radijator", "hladno", "klima", "kotao", "termostat",
            "ne grije", "ne hladi", "ventilac"
        },
        ["Mreža"] = new[]
        {
            "mrež", "mrez", "internet", "wifi", "wi-fi", "router", "ruter",
            "switch", "lan", "utp", "nema vez", "printer"
        },
        ["Građevinski radovi"] = new[]
        {
            "zid", "strop", "pod", "vrat", "prozor", "krov", "pukotin",
            "žbuk", "zbuk", "brav", "kvak", "staklo", "pločic", "plocic"
        }
    };

    private static readonly string[] CriticalKeywords =
    {
        "hitno", "opasn", "poplav", "dim", "iskri", "gori", "požar", "pozar",
        "nema struje", "ozljed", "curi jako", "prijeti", "urušav", "urusav"
    };

    private static readonly string[] HighKeywords =
    {
        "ne radi", "prekinut", "onemogućen", "onemogucen", "blokiran",
        "cijeli kat", "svi korisnici", "zbornic", "ambulant", "ne može se",
        "ne moze se", "zaglavljen"
    };

    private static readonly string[] LowKeywords =
    {
        "povremeno", "ponekad", "kozmetič", "kozmetic", "nije hitno",
        "kad stignete", "sitn", "škripi", "skripi"
    };

    public async Task<AiSuggestionResultDto> SuggestReportAsync(
        string description,
        CancellationToken cancellationToken = default)
    {
        var text = description.ToLowerInvariant();
        var reasons = new List<string>();

        var faultTypeName = TypeKeywords
            .Select(kv => new
            {
                Name = kv.Key,
                Hits = kv.Value.Count(k => text.Contains(k))
            })
            .Where(x => x.Hits > 0)
            .OrderByDescending(x => x.Hits)
            .Select(x => x.Name)
            .FirstOrDefault() ?? "Ostalo";

        reasons.Add(faultTypeName == "Ostalo"
            ? "Vrsta kvara nije prepoznata iz opisa pa je predložena kategorija Ostalo."
            : $"Opis sadrži pojmove karakteristične za kategoriju {faultTypeName}.");

        string priorityName;

        if (CriticalKeywords.Any(k => text.Contains(k)))
        {
            priorityName = "Kritičan";
            reasons.Add("Opis upućuje na neposrednu opasnost ili prekid rada objekta.");
        }
        else if (HighKeywords.Any(k => text.Contains(k)))
        {
            priorityName = "Visok";
            reasons.Add("Opis upućuje na nefunkcionalnost koja ometa redovan rad.");
        }
        else if (LowKeywords.Any(k => text.Contains(k)))
        {
            priorityName = "Nizak";
            reasons.Add("Opis upućuje na manji nedostatak bez utjecaja na rad.");
        }
        else
        {
            priorityName = "Srednji";
            reasons.Add("Iz opisa nije vidljiva hitnost pa je predložen srednji prioritet.");
        }

        var faultType = await _db.FaultTypes
            .FirstOrDefaultAsync(t => t.Name == faultTypeName, cancellationToken);

        var priority = await _db.FaultPriorities
            .FirstOrDefaultAsync(p => p.Name == priorityName, cancellationToken);

        return new AiSuggestionResultDto
        {
            SuggestedTitle = BuildTitle(description),
            SuggestedFaultTypeId = faultType?.Id,
            SuggestedFaultTypeName = faultType?.Name,
            SuggestedPriorityId = priority?.Id,
            SuggestedPriorityName = priority?.Name,
            Reasoning = string.Join(" ", reasons),
            Provider = ProviderName,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<AiSummaryResultDto> SummarizeAssignmentAsync(
        int assignmentId,
        CancellationToken cancellationToken = default)
    {
        var assignment = await _db.FaultAssignments
            .Include(a => a.Technician)
            .Include(a => a.FaultReport)!
                .ThenInclude(r => r!.Location)
            .Include(a => a.FaultReport)!
                .ThenInclude(r => r!.Priority)
            .Include(a => a.FaultReport)!
                .ThenInclude(r => r!.Status)
            .Include(a => a.Interventions)
                .ThenInclude(i => i.Status)
            .Include(a => a.Interventions)
                .ThenInclude(i => i.Materials)
                    .ThenInclude(m => m.Material)
                        .ThenInclude(mat => mat!.Unit)
            .FirstOrDefaultAsync(a => a.Id == assignmentId, cancellationToken);

        if (assignment is null)
        {
            return new AiSummaryResultDto
            {
                Summary = "Traženi radni nalog ne postoji.",
                Provider = ProviderName,
                GeneratedAt = DateTime.UtcNow
            };
        }

        var report = assignment.FaultReport!;
        var sb = new StringBuilder();

        sb.Append($"Radni nalog za prijavu „{report.Title}” na lokaciji {report.Location!.Name}. ");
        sb.Append($"Izvršitelj je {assignment.Technician!.FirstName} {assignment.Technician!.LastName}, ");
        sb.Append($"nalog je otvoren {assignment.AssignedAt.ToLocalTime():dd.MM.yyyy.}");

        sb.Append(assignment.UnassignedAt is null
            ? " i još je aktivan. "
            : $" i zatvoren {assignment.UnassignedAt.Value.ToLocalTime():dd.MM.yyyy.}. ");

        if (report.Priority is not null)
            sb.Append($"Prioritet prijave je {report.Priority.Name.ToLowerInvariant()}, ");

        sb.Append($"trenutni status prijave je {report.Status!.Name.ToLowerInvariant()}. ");

        var interventions = assignment.Interventions
            .OrderBy(i => i.CreatedAt)
            .ToList();

        if (interventions.Count == 0)
        {
            sb.Append("Na nalogu još nije zabilježena nijedna intervencija.");
        }
        else
        {
            sb.Append($"Zabilježeno je {interventions.Count} ");
            sb.Append(interventions.Count == 1 ? "intervencija. " : "intervencija. ");

            var completed = interventions.Count(i => i.Status!.Code == InterventionStatusCodes.Completed);
            var failed = interventions.Count(i => i.Status!.Code == InterventionStatusCodes.Failed);
            var open = interventions.Count(i =>
                i.Status!.Code == InterventionStatusCodes.Planned ||
                i.Status!.Code == InterventionStatusCodes.InProgress);

            var parts = new List<string>();
            if (completed > 0) parts.Add($"uspješno završenih: {completed}");
            if (failed > 0) parts.Add($"neuspješnih: {failed}");
            if (open > 0) parts.Add($"još otvorenih: {open}");

            sb.Append($"Od toga {string.Join(", ", parts)}. ");

            var totalMinutes = interventions
                .Where(i => i.DurationMinutes.HasValue)
                .Sum(i => i.DurationMinutes!.Value);

            if (totalMinutes > 0)
            {
                var h = totalMinutes / 60;
                var m = totalMinutes % 60;
                sb.Append(h > 0
                    ? $"Ukupno utrošeno vrijeme je {h} h {m} min. "
                    : $"Ukupno utrošeno vrijeme je {m} min. ");
            }

            var materials = interventions
                .SelectMany(i => i.Materials)
                .GroupBy(m => new { m.Material!.Name, m.Material!.Unit!.Abbreviation })
                .Select(g => $"{g.Key.Name} ({g.Sum(x => x.Quantity):0.##} {g.Key.Abbreviation})")
                .ToList();

            sb.Append(materials.Count > 0
                ? $"Utrošeni materijal: {string.Join(", ", materials)}. "
                : "Utrošak materijala nije evidentiran. ");

            var lastNote = interventions
                .Where(i => !string.IsNullOrWhiteSpace(i.Note))
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => i.Note)
                .FirstOrDefault();

            if (lastNote is not null)
                sb.Append($"Posljednja bilješka izvršitelja: „{lastNote.Trim()}”");
        }

        return new AiSummaryResultDto
        {
            Summary = sb.ToString().Trim(),
            Provider = ProviderName,
            GeneratedAt = DateTime.UtcNow
        };
    }

    private static string BuildTitle(string description)
    {
        var clean = description.Trim().Replace('\n', ' ').Replace('\r', ' ');

        while (clean.Contains("  "))
            clean = clean.Replace("  ", " ");

        var end = clean.IndexOfAny(new[] { '.', '!', '?' });
        var candidate = end > 10 ? clean[..end] : clean;

        if (candidate.Length > 70)
        {
            var cut = candidate.LastIndexOf(' ', Math.Min(70, candidate.Length - 1));
            candidate = cut > 20 ? candidate[..cut] : candidate[..70];
        }

        candidate = candidate.Trim().TrimEnd(',', ';', '-');

        return candidate.Length > 0
            ? char.ToUpperInvariant(candidate[0]) + candidate[1..]
            : "Prijava kvara";
    }
}