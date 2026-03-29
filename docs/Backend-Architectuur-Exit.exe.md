# Exit.exe — Backend Architectuur

## 1. Overzicht

Exit.exe is een escape-room puzzelgame gebouwd als een **ASP.NET Core Web API** met een **React (Vite)** frontend. De backend is opgebouwd volgens **Clean Architecture** met vier lagen, gebruikt het **CQRS-patroon** via **MediatR**, en past **Dependency Injection / Inversion** toe als kern van de architectuur.

---

## 2. Projectstructuur (Clean Architecture)

Het project bestaat uit vier lagen die elk een eigen verantwoordelijkheid hebben. De afhankelijkheden lopen altijd **van buiten naar binnen** — de binnenste laag (Domain) kent niets van de buitenlagen.

```
┌──────────────────────────────────────────────────────┐
│  Exit.exe.AppHost            (.NET Aspire)            │
│  Orkestratielaag — start API + frontend samen op     │
├──────────────────────────────────────────────────────┤
│  Exit.exe.Web                (Presentatielaag)        │
│  Controllers, Program.cs, GlobalExceptionHandler     │
├──────────────────────────────────────────────────────┤
│  Exit.exe.Application        (Businesslogicalaag)     │
│  Commands, Queries, Validators, DTOs, Contracts      │
├──────────────────────────────────────────────────────┤
│  Exit.exe.Repository         (Data-accesslaag)        │
│  DbContexts, Repositories, EF Migrations, Identity  │
├──────────────────────────────────────────────────────┤
│  Exit.exe.Domain             (Domeinlaag)             │
│  Entiteiten: Puzzle, GameSession, SessionStatus      │
│  Geen dependencies — puur POCO-klassen               │
└──────────────────────────────────────────────────────┘
```

### Dependency-richting

- **Web** → Application → Domain
- **Repository** → Application → Domain

De **Application-laag** definieert interfaces (contracts). De **Repository-laag** implementeert ze. Dit is het principe van **Dependency Inversion** (het "D" uit SOLID): de businesslogica is niet afhankelijk van de database-implementatie, maar van abstracties.

---

## 3. Domeinlaag (Exit.exe.Domain)

De domeinlaag bevat alleen pure entiteiten zonder enige afhankelijkheid. Dit zijn Plain Old CLR Objects (POCO's).

### Puzzle

Vertegenwoordigt een raadsel in het spel.

- **Id** — Unieke identifier (GUID)
- **GameType** — Type spel, bijvoorbeeld `"hangman"`
- **Payload** — JSON-string met de speldata (woord, categorie, beschrijving, narratief)
- **CreatedAtUtc** — Aanmaakdatum
- **Sessions** — Navigatie-eigenschap naar gerelateerde speelsessies

### GameSession

Vertegenwoordigt één speelsessie van een gebruiker.

- **Id** — Unieke identifier
- **UserId** — Verwijzing naar de ASP.NET Identity-gebruiker
- **PuzzleId** — Foreign key naar de Puzzle
- **Status** — Huidige status (InProgress, Success, Failed)
- **GuessedLetters** — Komma-gescheiden reeks geraden letters, bijvoorbeeld `"A,E,K"`
- **AttemptsLeft** — Aantal resterende pogingen
- **HintsUsed** — Aantal gebruikte hints
- **Score** — Behaalde score (null als nog bezig)
- **StartedAtUtc / CompletedAtUtc** — Tijdstempels

### SessionStatus (Enum)

Drie mogelijke statussen: `InProgress`, `Success`, `Failed`.

---

## 4. Application-laag (Exit.exe.Application) — CQRS + MediatR

### 4.1 Wat is CQRS?

CQRS staat voor **Command Query Responsibility Segregation**. Het principe is dat je **lees-operaties** (Queries) en **schrijf-operaties** (Commands) scheidt in aparte klassen. Elke operatie heeft zijn eigen request-object en handler.

### 4.2 Wat is MediatR?

MediatR is een library die het **Mediator-patroon** implementeert. In plaats van dat een controller rechtstreeks een service aanroept, stuurt hij een request-object naar MediatR. MediatR zoekt automatisch de juiste handler en voert die uit.

**Voordelen:**
- Controllers bevatten geen businesslogica
- Handlers zijn los gekoppeld en testbaar
- Pipeline behaviors (zoals validatie) kunnen automatisch draaien vóór elke handler

### 4.3 De flow in het project

```
HTTP Request
    ↓
Controller
    ↓ maakt Command of Query aan
    ↓ sender.Send(command)
    ↓
MediatR
    ↓ zoekt de juiste Handler
    ↓ draait eerst ValidationBehavior (pipeline)
    ↓
Handler
    ↓ voert businesslogica uit
    ↓ praat met repositories via interfaces
    ↓
Response (DTO) terug naar Controller → HTTP Response
```

### 4.4 Overzicht van Commands en Queries

| Type | Klasse | Beschrijving |
|------|--------|--------------|
| Command | `StartSessionCommand` | Start een nieuw spel — maakt Puzzle (via AI of seed) + GameSession aan |
| Command | `SubmitGuessCommand` | Verwerkt een letter-gok — checkt of letter in woord zit, update status |
| Command | `RequestHintCommand` | Vraagt een hint aan via de AI-service |
| Query | `GetGamesQuery` | Haalt lijst van beschikbare speltypes op |
| Query | `GetSessionQuery` | Haalt details van één speelsessie op |
| Query | `GetSessionHistoryQuery` | Haalt de spelgeschiedenis van een gebruiker op (met paginatie) |
| Query | `GetStoriesQuery` | Haalt publieke verhalen op |
| Query | `GetProfileStatsQuery` | Haalt spelstatistieken van een gebruiker op |

### 4.5 Concreet voorbeeld: SubmitGuessCommand

1. Gebruiker stuurt POST `/api/sessions/{id}/guess` met body `{ "letter": "A" }`
2. `SessionsController` maakt `SubmitGuessCommand(sessionId, "A", userId)` aan
3. Controller stuurt naar MediatR: `sender.Send(command)`
4. `ValidationBehavior` draait eerst de `SubmitGuessCommandValidator`:
   - SessionId mag niet leeg zijn
   - Letter moet exact 1 karakter zijn (a-z)
5. `SubmitGuessCommandHandler` wordt uitgevoerd:
   - Haalt sessie + puzzle op via `ISessionRepository`
   - Controleert of het spel nog bezig is
   - Controleert of de letter al geraden is
   - Voegt letter toe aan `GuessedLetters`
   - Als letter niet in het woord zit: `AttemptsLeft--`
   - Checkt of het woord volledig geraden is → `Status = Success`
   - Checkt of pogingen op zijn → `Status = Failed`
   - Slaat op via `SaveChangesAsync`
6. Retourneert `GuessResultDto` met de gemaskeerde tekst, resterende pogingen, en status

### 4.6 Contracts (Interfaces)

De Application-laag definieert interfaces — zij weet niet hoe data wordt opgeslagen of hoe AI werkt:

| Interface | Beschrijving |
|-----------|-------------|
| `IPuzzleRepository` | Puzzles ophalen per gametype, nieuwe puzzles toevoegen |
| `ISessionRepository` | Sessies ophalen (met puzzle-data), geschiedenis opvragen, opslaan |
| `IAiService` | AI-gegenereerde puzzels en hints aanmaken |

### 4.7 Validatie (FluentValidation + Pipeline Behavior)

Elke Command/Query kan een bijbehorende validator hebben. De `ValidationBehavior` is een MediatR **pipeline behavior** die automatisch vóór elke handler draait:

1. MediatR ontvangt een request
2. `ValidationBehavior` zoekt alle geregistreerde validators voor dat request-type
3. Voert de validators uit
4. Als er fouten zijn: gooit een `ValidationException` (wordt 422 HTTP-response)
5. Als alles geldig is: gaat door naar de handler

---

## 5. Repository-laag (Exit.exe.Repository) — Entity Framework Core

### 5.1 Twee DbContexts, één database

Het project gebruikt twee aparte DbContexts die dezelfde SQL Server-database delen, maar met gescheiden migratie-histories:

| DbContext | Tabellen | Migratie-tabel | Doel |
|-----------|----------|----------------|------|
| `AuthDbContext` | ASP.NET Identity-tabellen (AspNetUsers, AspNetUserLogins, etc.) | `__AuthMigrationsHistory` | Authenticatie en gebruikersbeheer |
| `AppDbContext` | Puzzles, GameSessions | `__AppMigrationsHistory` | Speldata |

`AuthDbContext` erft van `IdentityDbContext<ApplicationUser>`, wat automatisch alle Identity-tabellen aanmaakt. `ApplicationUser` erft van `IdentityUser` en kan uitgebreid worden met extra velden.

### 5.2 Repository-implementaties

De repositories implementeren de interfaces uit de Application-laag:

**PuzzleRepository:**
- `GetByGameTypeAsync` — Haalt alle puzzles op voor een bepaald gametype
- `Add` — Voegt een nieuwe puzzle toe aan de context

**SessionRepository:**
- `GetWithPuzzleAsync` — Haalt een sessie op met bijbehorende puzzle-data (JOIN)
- `GetHistoryByUserAsync` — Geeft de spelgeschiedenis met paginatie (SKIP/TAKE)
- `GetAllByUserAsync` — Alle sessies van een gebruiker (voor statistieken)
- `Add` — Voegt een nieuwe sessie toe
- `SaveChangesAsync` — Slaat alle wijzigingen op naar de database (Unit of Work)

---

## 6. Web-laag (Exit.exe.Web) — Controllers en configuratie

### 6.1 Controllers (REST API)

De controllers doen bewust geen businesslogica. Ze doen alleen drie dingen: (1) de gebruikers-ID uit de claims halen, (2) een Command of Query aanmaken, (3) doorsturen naar MediatR.

| Controller | Route | Authenticatie vereist | Beschrijving |
|------------|-------|----------------------|-------------|
| `AuthController` | `/api/auth/*` | Nee (AllowAnonymous) | Google OAuth login, logout, `/me`-endpoint |
| `GamesController` | `/api/games` | Ja | Lijst beschikbare games |
| `SessionsController` | `/api/sessions` | Ja | Start spel, gok letter, vraag hint, geschiedenis |
| `StoriesController` | `/api/stories` | Nee | Publieke verhalen ophalen |
| `ProfileController` | `/api/profile/stats` | Ja | Spelstatistieken van de ingelogde gebruiker |

### 6.2 Voorbeeld: hoe een controller werkt

```csharp
[HttpPost("{id:guid}/guess")]
public async Task<ActionResult<GuessResultDto>> Guess(Guid id, [FromBody] GuessRequest request, CancellationToken ct)
{
    var userId = GetUserId();                                    // 1. Haal userId uit claims
    var command = new SubmitGuessCommand(id, request.Letter, userId);  // 2. Maak Command
    var result = await sender.Send(command, ct);                 // 3. Stuur naar MediatR
    return Ok(result);                                           // 4. Return resultaat
}
```

De controller weet niets van de database, de spelregels, of hoe een gok wordt verwerkt. Dat is de verantwoordelijkheid van de handler in de Application-laag.

### 6.3 GlobalExceptionHandler

Alle exceptions worden centraal afgevangen via de `IExceptionHandler`-interface. Dit zorgt ervoor dat de API altijd een consistente foutresponse geeft in ProblemDetails-formaat:

| Exception-type | HTTP-statuscode | Wanneer |
|----------------|----------------|---------|
| `ValidationException` | 422 Unprocessable Entity | FluentValidation faalt (ongeldige input) |
| `KeyNotFoundException` | 404 Not Found | Sessie of puzzle niet gevonden |
| `UnauthorizedAccessException` | 401 Unauthorized | Geen gebruikers-ID in de claims |
| `InvalidOperationException` | 400 Bad Request | Ongeldige actie (bv. sessie al afgelopen, letter al geraden) |
| Overige exceptions | 500 Internal Server Error | Onverwachte fouten |

---

## 7. Dependency Injection en Inversion of Control

### 7.1 Wat is Dependency Injection (DI)?

In plaats van dat een klasse zelf zijn afhankelijkheden aanmaakt, worden deze **van buitenaf geïnjecteerd** via de constructor. De ASP.NET Core DI-container beheert de levensduur en het aanmaken van objecten.

### 7.2 Wat is Dependency Inversion?

De businesslogica (Application-laag) is niet afhankelijk van de concrete implementatie (Repository-laag), maar van **abstracties** (interfaces). De Repository-laag implementeert die interfaces. Zo kan je de database-implementatie vervangen zonder de businesslogica te wijzigen.

### 7.3 Registraties in Program.cs

```csharp
// Repositories: interface → concrete implementatie
services.AddScoped<IPuzzleRepository, PuzzleRepository>();
services.AddScoped<ISessionRepository, SessionRepository>();

// MediatR: scant de Application-assembly voor alle handlers
services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(applicationAssembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// FluentValidation: scant voor alle validator-klassen
services.AddValidatorsFromAssembly(applicationAssembly);

// Entity Framework: DbContexts met SQL Server
services.AddDbContext<AuthDbContext>(opt => opt.UseSqlServer(conn, ...));
services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(conn, ...));

// ASP.NET Identity
services.AddIdentityCore<ApplicationUser>(...)
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddSignInManager();
```

### 7.4 Lifetimes

- **Scoped** (AddScoped): Eén instantie per HTTP-request. Alle code binnen dezelfde request deelt dezelfde DbContext. Dit is essentieel voor het Unit of Work-patroon — alle database-wijzigingen worden in één `SaveChangesAsync`-aanroep opgeslagen.
- **Singleton** (AddSingleton): Eén instantie voor de hele applicatie.
- **Transient** (AddTransient): Nieuwe instantie bij elke injectie.

### 7.5 Constructor injection in de praktijk

```csharp
// De handler vraagt om interfaces, niet om concrete klassen
public sealed class StartSessionCommandHandler(
    IPuzzleRepository puzzleRepository,    // ← interface
    ISessionRepository sessionRepository,  // ← interface
    IAiService aiService)                  // ← interface
    : IRequestHandler<StartSessionCommand, SessionDto>
```

De DI-container injecteert automatisch `PuzzleRepository`, `SessionRepository`, en `AiCoreService` als implementaties.

---

## 8. Authenticatie, Autorisatie en Beveiliging — Volledige Flow

### 8.1 Overzicht: drie partijen

De authenticatie-flow draait om drie partijen die samenwerken:

| Partij | URL (development) | Rol |
|--------|-------------------|-----|
| **Frontend (React/Vite)** | `http://localhost:5173` | De UI die de gebruiker ziet. Start de login, toont het profiel, stuurt API-requests. |
| **Backend (ASP.NET Core API)** | `https://localhost:7007` | Verwerkt de OAuth-flow, beheert cookies, beschermt endpoints. |
| **Google OAuth** | `accounts.google.com` | Externe identiteitsprovider — verifieert wie de gebruiker is. |

De frontend en API draaien op **verschillende origins** (ander poort, ander scheme). Dit heeft gevolgen voor cookies en CORS.

### 8.2 Google Cloud Console — Sleutels en configuratie

Voordat de OAuth-flow werkt, moet de applicatie geregistreerd zijn bij Google:

1. In **Google Cloud Console** → API & Services → Credentials wordt een **OAuth 2.0 Client** aangemaakt
2. Google geeft twee geheimen:
   - **Client ID** — publieke identificatie van de app (wordt meegestuurd naar Google)
   - **Client Secret** — geheim sleutel dat alleen de server kent (wordt NOOIT naar de browser gestuurd)
3. Er worden **Authorized redirect URIs** geconfigureerd, bv. `https://localhost:7007/signin-google`
4. Google stuurt gebruikers alleen terug naar deze geregistreerde URIs — dit voorkomt dat een aanvaller de flow kan kapen

In de backend worden deze sleutels opgeslagen via **User Secrets** (development) of environment variables (productie), nooit in broncode:

```csharp
var googleClientId     = config["Authentication:Google:ClientId"];
var googleClientSecret = config["Authentication:Google:ClientSecret"];

authBuilder.AddGoogle("Google", o =>
{
    o.ClientId     = googleClientId;
    o.ClientSecret = googleClientSecret;
    o.SignInScheme  = IdentityConstants.ExternalScheme;
    o.SaveTokens   = true;
});
```

### 8.3 De volledige login-flow stap voor stap

```
┌──────────────┐     ┌──────────────────┐     ┌─────────────────┐
│   Frontend   │     │   Backend API    │     │  Google OAuth    │
│  :5173       │     │  :7007           │     │  accounts.google │
└──────┬───────┘     └────────┬─────────┘     └────────┬────────┘
       │                      │                         │
  1.   │── navigeer naar ────→│                         │
       │  /api/auth/login/    │                         │
       │  google?returnUrl=   │                         │
       │  http://localhost:   │                         │
       │  5173/               │                         │
       │                      │                         │
  2.   │                      │── 302 Redirect ────────→│
       │                      │  naar Google met:       │
       │                      │  • client_id            │
       │                      │  • redirect_uri =       │
       │                      │    /signin-google       │
       │                      │  • scope = email,       │
       │                      │    profile              │
       │                      │  • state = (encrypted)  │
       │                      │                         │
       │                      │  + zet correlation      │
       │                      │    cookie               │
       │                      │                         │
  3.   │                      │                         │
       │←─────── Google login pagina ──────────────────→│
       │         gebruiker kiest account                │
       │         en geeft toestemming                   │
       │                      │                         │
  4.   │                      │←── 302 Redirect ────────│
       │                      │  naar /signin-google    │
       │                      │  met ?code=AUTH_CODE    │
       │                      │                         │
  5.   │                      │── POST (server-side) ──→│
       │                      │  wisselt AUTH_CODE      │
       │                      │  + client_secret        │
       │                      │  voor access_token      │
       │                      │                         │
  6.   │                      │←── access_token ────────│
       │                      │  + gebruikersinfo       │
       │                      │  (email, naam, id)      │
       │                      │                         │
  7.   │                      │ Server verwerkt:        │
       │                      │ • Zoek user in DB       │
       │                      │ • Bestaat niet? Maak    │
       │                      │   nieuw account aan     │
       │                      │ • Koppel Google-login   │
       │                      │ • Zet exitexe.auth      │
       │                      │   cookie                │
       │                      │                         │
  8.   │←── 302 Redirect ─────│                         │
       │  naar returnUrl      │                         │
       │  (http://localhost:  │                         │
       │   5173/)             │                         │
       │                      │                         │
  9.   │── fetch /api/auth/ ─→│                         │
       │   me (met cookie)    │                         │
       │                      │                         │
  10.  │←── { isAuth: true,  ─│                         │
       │     email: "..." }   │                         │
       │                      │                         │
```

### 8.4 Gedetailleerde uitleg per stap

**Stap 1 — Frontend start login:**
De gebruiker klikt op "Login met Google". De frontend navigeert de browser (volledige pagina-navigatie, geen fetch) naar de API:
```
https://localhost:7007/api/auth/login/google?returnUrl=http://localhost:5173/
```
De `returnUrl` vertelt de API waar de gebruiker na login naartoe moet.

**Stap 2 — API stuurt Challenge naar Google:**
De `AuthController.LoginGoogle()` methode maakt een Challenge aan. ASP.NET Core's Google-handler:
- Genereert een unieke `state`-parameter (bevat encrypted returnUrl + correlatie-ID)
- Zet een **correlation cookie** (`exitexe.external`) — dit voorkomt CSRF-aanvallen
- Redirect de browser naar `accounts.google.com/o/oauth2/v2/auth` met de `client_id`, `redirect_uri`, en `scope`

**Stap 3 — Gebruiker logt in bij Google:**
Google toont de login-pagina. De gebruiker kiest een account en geeft toestemming. Google verifieert de identiteit.

**Stap 4 — Google redirect terug naar de API:**
Google stuurt de browser terug naar `https://localhost:7007/signin-google?code=AUTH_CODE&state=...`. Dit is de **redirect URI** die in Google Cloud Console is geregistreerd. Google stuurt een eenmalige **authorization code** mee.

**Stap 5 & 6 — Server wisselt code voor tokens (server-to-server):**
De ASP.NET Google-handler maakt een directe HTTPS-call naar Google's token-endpoint:
- Stuurt de `authorization code` + `client_secret` (dit geheim verlaat nooit de server!)
- Google valideert en stuurt een `access_token` + gebruikersinformatie terug
- Dit gebeurt server-to-server — de browser ziet de `client_secret` nooit

**Stap 7 — Server verwerkt de login:**
```csharp
// Zoek bestaande externe login
var info = await signInManager.GetExternalLoginInfoAsync();

// Probeer in te loggen met bestaande koppeling
var result = await signInManager.ExternalLoginSignInAsync(
    info.LoginProvider, info.ProviderKey, isPersistent: false);

if (!result.Succeeded)
{
    // Nieuwe gebruiker: zoek op email of maak nieuw account
    var email = info.Principal.FindFirstValue(ClaimTypes.Email);
    var user = await userManager.FindByEmailAsync(email);

    if (user is null)
    {
        user = new ApplicationUser { UserName = email, Email = email };
        await userManager.CreateAsync(user);
    }

    // Koppel Google-login aan het account
    await userManager.AddLoginAsync(user, info);
    await signInManager.SignInAsync(user, isPersistent: false);
}

// Ruim de tijdelijke external cookie op
await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
// → De exitexe.auth cookie is nu gezet
```

**Stap 8 — Redirect naar frontend:**
De server stuurt een `302 Redirect` naar de `returnUrl` (bv. `http://localhost:5173/`). De browser navigeert naar de frontend.

**Stap 9 & 10 — Frontend checkt login-status:**
De React `AuthProvider` roept `fetch('/api/auth/me', { credentials: 'include' })` aan. De browser stuurt de `exitexe.auth`-cookie mee. De API leest de cookie, vindt de gebruiker, en retourneert de gebruikersinfo.

### 8.5 Cookies — Instellingen en beveiliging

| Cookie | Instelling | Waarde | Waarom |
|--------|-----------|--------|--------|
| `exitexe.auth` | `HttpOnly` | `true` | JavaScript kan de cookie niet lezen → beschermt tegen XSS |
| | `Secure` | `true` | Cookie wordt alleen verstuurd via HTTPS → beschermt tegen afluisteren |
| | `SameSite` | `None` | Nodig omdat frontend (`:5173`) en API (`:7007`) cross-origin zijn |
| | Naam | `exitexe.auth` | Eigen naam i.p.v. standaard `.AspNetCore.Identity.Application` |
| `exitexe.external` | `HttpOnly` | `true` | Zie boven |
| | `Secure` | `true` | Zie boven |
| | `SameSite` | `None` | Nodig voor de redirect vanuit Google (cross-site) |
| | `ExpireTimeSpan` | 5 min | Korte levensduur — alleen nodig tijdens OAuth-handshake |

### 8.6 CORS — Waarom en hoe

**Het probleem:** De frontend (`http://localhost:5173`) en de API (`https://localhost:7007`) draaien op verschillende origins. Browsers blokkeren standaard cross-origin requests (Same-Origin Policy).

**De oplossing:** CORS (Cross-Origin Resource Sharing) vertelt de browser welke origins de API mogen aanroepen.

**Hoe het werkt:**

1. De browser stuurt een **preflight request** (OPTIONS) voordat het echte request wordt verstuurd
2. De API antwoordt met CORS-headers:
   - `Access-Control-Allow-Origin: http://localhost:5173` (welke origin mag)
   - `Access-Control-Allow-Credentials: true` (cookies meesturen mag)
   - `Access-Control-Allow-Methods: GET, POST, PUT, DELETE` (welke methodes)
   - `Access-Control-Allow-Headers: Content-Type` (welke headers)
3. Als de headers kloppen, stuurt de browser het echte request

**Configuratie in Program.cs:**

```csharp
// Development: sta alle origins toe
services.AddCors(o => o.AddPolicy("Spa", p =>
    p.SetIsOriginAllowed(_ => true)    // elke origin toegestaan
     .AllowAnyHeader()                 // alle headers toegestaan
     .AllowAnyMethod()                 // GET, POST, PUT, DELETE
     .AllowCredentials()));            // cookies meesturen

// Productie: alleen geconfigureerde origins
services.AddCors(o => o.AddPolicy("Spa", p =>
    p.WithOrigins("https://exit-exe.com")  // alleen jouw domein
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));
```

**Belangrijk:** `UseCors("Spa")` staat in de middleware pipeline **vóór** `UseAuthentication()`. Als CORS na auth zou komen, worden preflight requests geblokkeerd voordat CORS-headers worden toegevoegd.

### 8.7 Beveiliging tegen open-redirect aanvallen

De `returnUrl`-parameter kan misbruikt worden: een aanvaller zou `?returnUrl=https://evil.com` kunnen meegeven om de gebruiker na login naar een phishing-site te sturen.

**Hoe wij dit voorkomen:**

```csharp
private string? ValidateReturnUrl(string? returnUrl)
{
    // Lege URL → geen override
    if (string.IsNullOrWhiteSpace(returnUrl))
        return null;

    // Lokale URL (/profile, /games) → altijd veilig
    if (Url.IsLocalUrl(returnUrl))
        return returnUrl;

    // Absolute URL → alleen als het scheme+host in de allowlist staat
    if (Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri) && IsAllowedOrigin(uri))
        return returnUrl;

    // Alles anders → weigeren
    return null;
}
```

De `IsAllowedOrigin`-methode vergelijkt met `Uri.Compare` op `SchemeAndServer`-niveau — dit voorkomt prefix-bypass aanvallen (bv. `https://localhost:5173.evil.com`).

De Scalar-redirect checkt ook dat de `Referer`-header van **dezelfde host en scheme** komt als het huidige request — een referer van `https://attacker.tld/scalar` wordt afgewezen.

### 8.8 De rol van ASP.NET Identity

ASP.NET Identity beheert het gebruikerssysteem:

| Component | Rol |
|-----------|-----|
| `ApplicationUser` | De gebruikersentiteit (erft van `IdentityUser`) |
| `UserManager<ApplicationUser>` | CRUD-operaties op gebruikers (aanmaken, zoeken, etc.) |
| `SignInManager<ApplicationUser>` | Login/logout, externe login-flow beheren |
| `AuthDbContext` | De database-context met Identity-tabellen (AspNetUsers, AspNetUserLogins, etc.) |

De `ExternalLoginSignInAsync`-methode zoekt in de `AspNetUserLogins`-tabel of er een koppeling bestaat tussen Google's `ProviderKey` (uniek Google-ID) en een lokaal account. Zo ja → direct ingelogd. Zo nee → nieuw account + koppeling.

### 8.9 Autorisatie op endpoints

| Attribuut | Effect | Voorbeeld |
|-----------|--------|-----------|
| `[Authorize]` | Vereist ingelogde gebruiker → 401 als niet ingelogd | `SessionsController`, `GamesController` |
| `[AllowAnonymous]` | Iedereen mag, ook zonder login | `AuthController.Me()`, `StoriesController` |

De `OnRedirectToLogin`-event voorkomt dat API-endpoints een HTML-loginpagina retourneren:

```csharp
options.Events.OnRedirectToLogin = ctx =>
{
    if (ctx.Request.Path.StartsWithSegments("/api"))
    {
        ctx.Response.StatusCode = 401;  // JSON-vriendelijk
        return Task.CompletedTask;
    }
    ctx.Response.Redirect(ctx.RedirectUri);  // Niet-API: gewoon redirecten
    return Task.CompletedTask;
};
```

### 8.10 Samenvatting beveiligingsmaatregelen

| Maatregel | Beschermt tegen |
|-----------|----------------|
| `client_secret` alleen server-side | Token-diefstal (secret verlaat nooit de browser) |
| Correlation cookie + state parameter | CSRF tijdens OAuth-flow |
| `HttpOnly` cookies | XSS (JavaScript kan cookies niet lezen) |
| `Secure` cookies | Man-in-the-middle (alleen via HTTPS) |
| `SameSite=None` + CORS allowlist | Gecontroleerde cross-origin toegang |
| `returnUrl` validatie + allowlist | Open-redirect aanvallen |
| `Uri.Compare` op SchemeAndServer | Prefix-bypass aanvallen |
| Referer host+scheme check | Spoofed referer redirects |
| Registered redirect URIs in Google Console | OAuth-flow kaping |

---

## 9. Middleware Pipeline

De volgorde van middleware in de pipeline is cruciaal. Elke request doorloopt deze stappen in volgorde:

```
Inkomende HTTP Request
    ↓
1. UseExceptionHandler()     → Vangt alle exceptions op (GlobalExceptionHandler)
    ↓
2. UseHttpsRedirection()     → Redirect HTTP naar HTTPS (alleen in productie)
    ↓
3. UseCors("Spa")            → Voegt CORS-headers toe (moet vóór auth!)
    ↓
4. UseAuthentication()       → Leest de cookie en stelt User-identity in
    ↓
5. UseAuthorization()        → Checkt [Authorize] / [AllowAnonymous] attributen
    ↓
6. MapControllers()          → Route naar de juiste controller-action
    ↓
HTTP Response terug
```

### CORS (Cross-Origin Resource Sharing)

Omdat de frontend (localhost:5173) en de API (localhost:7007) op verschillende origins draaien, moet CORS geconfigureerd worden. In development worden alle origins toegestaan. In productie worden alleen de geconfigureerde SPA-origins toegelaten.

---

## 10. .NET Aspire (AppHost)

Het project gebruikt .NET Aspire als orchestratielaag. De `AppHost` start de API en de frontend samen op:

```csharp
var api = builder.AddProject<Exit_exe_Web>("api")
    .WithEndpoint("https", e => { e.Port = 7007; e.IsProxied = false; });

var frontend = builder.AddViteApp("frontend", "../frontend")
    .WithReference(api)     // frontend kent de API-URL
    .WaitFor(api);          // start pas als API draait
```

**Voordelen van Aspire:**
- Eén klik om alles te starten
- Dashboard met logs, health-checks, en endpoints
- Service discovery (frontend weet automatisch waar de API is)

---

## 11. AI-integratie

Het project integreert met een AI-service voor het genereren van puzzels en hints:

| Interface | Implementatie | Beschrijving |
|-----------|--------------|-------------|
| `IAiService` | `AiCoreService` | Primaire AI-service voor puzzel- en hint-generatie |
| `IAiService` | `DisabledAiService` | Fallback wanneer geen AI geconfigureerd is — gebruikt seed-data |

Wanneer een nieuw spel wordt gestart (`StartSessionCommand`):
1. Probeer een puzzel te genereren via de AI-service
2. Als de AI beschikbaar is: maak een nieuwe Puzzle aan met AI-gegenereerde data
3. Als de AI niet beschikbaar is: kies een willekeurige bestaande puzzel uit de database (seed-data)

---

## 12. Testen

Het project bevat twee testprojecten:

| Project | Wat wordt getest |
|---------|-----------------|
| `Exit.exe.Domain.Tests` | Unit tests voor domeinlogica |
| `Exit.exe.Application.Tests` | Unit tests voor Commands, Queries en Handlers |

De handlers kunnen getest worden door de interfaces te mocken (dankzij Dependency Injection). Er is geen echte database nodig voor unit tests.

---

## 13. Samenvatting — Architectuurprincipes

| Principe | Toepassing in het project |
|----------|--------------------------|
| **Clean Architecture** | 4 lagen met duidelijke verantwoordelijkheden en afhankelijkheden van buiten naar binnen |
| **CQRS** | Scheiding tussen Commands (schrijven) en Queries (lezen) in de Application-laag |
| **Mediator-patroon (MediatR)** | Controllers sturen requests naar MediatR, die de juiste handler vindt |
| **Dependency Injection** | Alle afhankelijkheden worden via constructors geïnjecteerd door de DI-container |
| **Dependency Inversion (SOLID)** | Application definieert interfaces, Repository implementeert ze |
| **Repository-patroon** | Data-access is geabstraheerd achter interfaces |
| **Unit of Work** | Scoped DbContext — alle wijzigingen per request in één SaveChangesAsync |
| **Pipeline Behaviors** | ValidationBehavior draait automatisch vóór elke handler |
| **Cookie-based Auth** | ASP.NET Identity + Google OAuth, geen JWT |
| **Global Exception Handling** | Centraal via IExceptionHandler met ProblemDetails-responses |
| **REST API** | Resource-gebaseerde URL's, HTTP-verbs, [ApiController]-attributen |
| **.NET Aspire** | Orchestratie van API + frontend met service discovery |
