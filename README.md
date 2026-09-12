# e-Kvarovi Županije

Web aplikacija za prijavu i obradu kvarova na objektima u nadležnosti županije.
Završni projekt tečaja Junior Developer .NET / Blazor.

Zaposlenici prijavljuju kvarove na svojim lokacijama, upravitelj ih pregledava,
određuje vrstu i prioritet te dodjeljuje izvršitelja. Izvršitelj evidentira
intervencije i utrošeni materijal, a upravitelj na kraju zatvara prijavu.

---

## Tehnologije

| Sloj | Tehnologija |
|---|---|
| Klijent | Blazor Web App (.NET), Interactive Server, MudBlazor |
| Poslužitelj | ASP.NET Core Web API, kontroleri |
| Baza | SQLite, Entity Framework Core (Code First) |
| Autentikacija | JWT Bearer, vlastite tablice korisnika i uloga |
| Dokumentacija API-ja | Swagger / OpenAPI |

## Struktura rješenja

```
eKvarovi/
├── eKvarovi.App/          Blazor klijent — stranice, dijalozi, servisi
├── eKvarovi.Api/          Web API — kontroleri, EF Core model, migracije, seed
├── eKvarovi.Shared/       DTO-ovi koje dijele klijent i API
└── eKvarovi.Api.http      Primjeri HTTP zahtjeva za testiranje
```

Entity modeli žive isključivo u `eKvarovi.Api/Models` i nikad ne izlaze iz API
projekta. `eKvarovi.Shared` sadrži samo DTO-ove, pa klijent strukturno nema
pristup entitetima baze. `eKvarovi.App` nikada ne koristi `DbContext` niti
izravno pristupa bazi — sva komunikacija ide preko HTTP-a.

---

## Pokretanje

### 1. Preduvjeti

- .NET SDK (verzija koja odgovara ciljanom frameworku projekta)
- Visual Studio 2022 ili noviji

### 2. Postavljanje tajnog ključa

Aplikacija koristi JWT i zahtijeva potpisni ključ. Ključ se **ne nalazi u
repozitoriju** nego se postavlja kroz User Secrets.

U Visual Studiju: desni klik na `eKvarovi.Api` → `Manage User Secrets`, pa
zalijepiti:

```json
{
  "Jwt": {
    "Key": "unesite-vlastiti-tajni-kljuc-od-najmanje-32-znaka"
  }
}
```

Alternativno, iz terminala u mapi `eKvarovi.Api`:

```bash
dotnet user-secrets set "Jwt:Key" "unesite-vlastiti-tajni-kljuc-od-najmanje-32-znaka"
```

Ključ mora imati najmanje 32 znaka. Bez njega se API neće pokrenuti i javit će
jasnu poruku o nedostajućoj konfiguraciji.

### 3. Pokretanje

Solution je postavljen na istovremeno pokretanje oba projekta
(`Configure Startup Projects` → `Multiple startup projects`, `eKvarovi.Api`
iznad `eKvarovi.App`). Dovoljno je pritisnuti F5.

Baza se **ne nalazi u repozitoriju**. Pri prvom pokretanju aplikacija sama:

1. primijeni sve migracije i stvori `ekvarovi.db`
2. napuni šifrarnike zadanim vrijednostima
3. unese demo lokacije, zaposlenike, materijale, prijave i korisničke račune

Seeder je idempotentan — ponovna pokretanja ne dupliciraju podatke.

Zadane adrese:

| Projekt | Adresa |
|---|---|
| Blazor klijent | https://localhost:7297 |
| Web API + Swagger | https://localhost:7110/swagger |

Ako se portovi razlikuju, uskladiti `ApiBaseUrl` u
`eKvarovi.App/appsettings.json` s HTTPS portom API projekta.

---

## Demo korisnici

Svi računi koriste lozinku **`Demo1234!`**

| Korisničko ime | Uloge | Povezani zaposlenik |
|---|---|---|
| `admin` | Administrator | — (tehnički račun) |
| `upravitelj` | Upravitelj, Prijavitelj | Petar Jurić |
| `izvrsitelj` | Izvršitelj, Prijavitelj | Ante Vuković |
| `prijavitelj` | Prijavitelj | Marina Kovač |

Račun `admin` namjerno nije povezan sa zaposlenikom — prikazuje da je
korisnički račun odvojen od poslovnog identiteta osobe. Zbog toga admin nema
vlastitih prijava ni naloga.

Računi `upravitelj` i `izvrsitelj` imaju po dvije uloge jer su i sami
zaposlenici koji mogu prijaviti kvar.

---

## Uloge i ovlasti

| Radnja | Admin | Upravitelj | Izvršitelj | Prijavitelj |
|---|:---:|:---:|:---:|:---:|
| Prijava kvara | ✓ | ✓ | ✓ | ✓ |
| Pregled svih prijava | ✓ | ✓ | ✓ | — |
| Određivanje vrste i prioriteta | ✓ | ✓ | — | — |
| Dodjela i prebacivanje naloga | ✓ | ✓ | — | — |
| Evidencija intervencija | ✓ | ✓ | ✓ (svojih) | — |
| Evidencija materijala | ✓ | ✓ | ✓ (svojih) | — |
| Zatvaranje prijave | ✓ | ✓ | — | — |
| Šifrarnici i zaposlenici | ✓ | — | — | — |
| Korisnički računi | ✓ | — | — | — |

Izvršitelj ne može mijenjati prioritet, dodjeljivati naloge niti uređivati
tuđe intervencije — takav pokušaj vraća `403 Forbidden`.

---

## Tokovi kroz aplikaciju

### Puni životni ciklus prijave

1. Prijavitelj otvara prijavu — status **Zaprimljeno**, bez vrste i prioriteta
2. Upravitelj pregledava, određuje vrstu i prioritet — status **Pregledano**
3. Upravitelj dodjeljuje izvršitelja — status **Dodijeljeno**
4. Izvršitelj otvara intervenciju i pokreće je — status **U radu**
5. Izvršitelj evidentira materijal i završava intervenciju — status **Riješeno**
6. Upravitelj zatvara prijavu — status **Zatvoreno**

### Ponovna dodjela s očuvanjem povijesti

1. Prijava je dodijeljena izvršitelju A
2. Upravitelj je prebacuje na izvršitelja B
3. Nalog izvršitelja A dobiva datum skidanja i ostaje u bazi
4. Nastaje novi nalog za izvršitelja B
5. Profil prijave prikazuje oba naloga u povijesti dodjela

### Neuspješna intervencija

1. Izvršitelj označava intervenciju neuspješnom uz obavezno objašnjenje
2. Intervencija ostaje u povijesti
3. Na istom nalogu moguće je otvoriti novu intervenciju
4. Prijava se ne može zatvoriti dok ne postoji uspješno završena intervencija

### Vlastiti pregled

1. Prijavitelj na `/myreports` vidi isključivo svoje prijave
2. Izvršitelj na `/myassignments` vidi isključivo svoje naloge
3. Oba prikaza koriste endpointe koji identitet čitaju iz JWT tokena — ID se
   nikada ne prosljeđuje iz sučelja

---

## Poslovna pravila koja provodi API

- Prijava se može otvoriti samo na aktivnoj lokaciji
- Vrsta i prioritet postavljaju se tek pri pregledu, ne pri otvaranju
- Prijava najvišeg prioriteta mora imati rok rješavanja
- Rok ne može biti raniji od datuma prijave
- Prijava može imati najviše jedan aktivan radni nalog
- Nalog se ne može skinuti dok postoji otvorena intervencija
- Intervencija sa statusom *Završena* mora imati početak, završetak i bilješku
- Intervencija sa statusom *Neuspješna* mora imati bilješku s razlogom
- Na jednom nalogu može postojati najviše jedna otvorena intervencija
- Količina utrošenog materijala mora biti veća od nule
- Prijava se zatvara tek nakon barem jedne uspješno završene intervencije
- Prijava s poviješću dodjela ne briše se fizički
- Lokacija, zaposlenik ili materijal s poviješću ne brišu se nego deaktiviraju
- U sustavu mora ostati barem jedan aktivan administrator

Pravilo o najviše jednoj aktivnoj dodjeli provodi se i na razini baze,
parcijalnim UNIQUE indeksom nad `FaultAssignments (FaultReportId)` uz uvjet
`UnassignedAt IS NULL`. Zaobilaženje aplikacijskog sloja stoga nije moguće.

---

## Model baze

Model je dokumentiran u datoteci `ekvarovi.dbml` i može se otvoriti na
[dbdiagram.io](https://dbdiagram.io).

Ukupno 19 tablica:

**Šifrarnici (7)** — `LocationTypes`, `FaultTypes`, `FaultPriorities`,
`FaultStatuses`, `InterventionStatuses`, `MaterialUnits`, `AttachmentPurposes`

**Poslovne tablice (8)** — `Locations`, `Employees`, `FaultReports`,
`FaultAssignments`, `Interventions`, `Materials`, `InterventionMaterials`,
`FaultAttachments`

**Korisnički sloj (3)** — `AppUsers`, `AppRoles`, `AppUserRoles`

**Povijest promjena (1)** — `FaultReportEvents`

### Ključne modelske odluke

**Povijest dodjela.** `FaultAssignments` bilježi svaku dodjelu kao zaseban
redak. Aktivna dodjela je ona kojoj je `UnassignedAt` prazan. Ponovna dodjela
ne mijenja postojeći redak nego mu upisuje datum skidanja i dodaje novi.

**Intervencije vise o nalogu, ne o prijavi.** Zbog toga se zna koji je
izvršitelj izveo koju intervenciju, a jedan nalog može imati više intervencija
kroz vrijeme.

**Jedna tablica za osobe.** `Employees` obuhvaća i prijavitelje i izvršitelje.
Tko što smije određuju uloge na povezanom korisničkom računu, a ne struktura
tablica. Isti zaposlenik tako može biti i prijavitelj i izvršitelj.

**Mjerna jedinica pripada materijalu**, ne pojedinom utrošku, pa se ista stavka
ne može jednom evidentirati u komadima a drugi put u litrama.

**Nullable vrsta i prioritet** na prijavi odražavaju stvarno početno stanje —
prijavitelj ih ne određuje.

---

## Sigurnost

- Lozinke se pohranjuju hashirane (`PasswordHasher<AppUser>`), nikad u čistom obliku
- JWT ključ se drži u User Secrets, izvan repozitorija
- Identitet korisnika čita se isključivo iz potpisanog tokena
- Endpointi `/mine` ne primaju ID iz zahtjeva
- Prijenos datoteka: bijeli popis formata (JPG, PNG, WEBP, PDF), ograničenje
  5 MB, provjera podudaranja ekstenzije i MIME tipa, spremanje pod generiranim
  imenom umjesto korisnički zadanog
- Poruka pri neuspjeloj prijavi ne otkriva postoji li korisničko ime

---

## Testiranje API-ja

Swagger je dostupan na `https://localhost:7110/swagger`.

Za zaštićene endpointe koristiti datoteku `eKvarovi.Api.http` u Visual Studiju:
pokrenuti zahtjev za prijavu, kopirati token u varijablu `@token`, pa pozivati
ostale zahtjeve.

---

## Napomene

- Baza (`*.db`) i priložene datoteke nisu dio repozitorija; nastaju pri pokretanju
- Mapa `eKvarovi.Api/wwwroot/uploads` prati se preko `.gitkeep`, sadržaj se ignorira
- Svi podaci u aplikaciji su izmišljeni i služe isključivo za demonstraciju
