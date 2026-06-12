# Backend — naprawy (2026-06-03)

Audyt i poprawki analogicznie do frontendu (`matdev-pm`): build, testy jednostkowe, spójność z Dockerem i frontendem na `127.0.0.1:5196`.

## AutoMapper — `ApplicationMappingProfile.cs`

**Problem:** Mapowanie `CreateProjectDTO → Project` nie ignorowało `ProjectAssignments`. `AssertConfigurationIsValid()` w testach padało (`AutoMapperConfigurationException`, ~66 testów).

**Zmiana:** `.ForMember(d => d.ProjectAssignments, opt => opt.Ignore())` (jak przy `CreateUserDTO → User`).

---

## `ProjectService.GetByPhraseAsync`

**Problem:** Pusta lista przy braku wyników, podczas gdy inne serwisy (`UserService`, `TopicService`, …) rzucają `KeyNotFoundException`. Test `GetByPhraseAsync_WhenEmpty_ThrowsKeyNotFoundException` oczekiwał wyjątku.

**Zmiana:** `throw new KeyNotFoundException($"No projects found containing the phrase '{s}'.");` — API zwraca 404 przez `GlobalExceptionHandler`.

---

## Testy — `ProjectServiceTests.cs`

**Problem:** `ProjectService` wymaga `IBudgetRepository`; test przekazywał tylko `IProjectRepository` (`CS7036`).

**Zmiana:**
- `Mock<IBudgetRepository>` ze stubami: `GetBudgetPlanByProjectAsync` → `null`, `GetAllBudgetPlansAsync` → `[]`.
- SUT: `new ProjectService(_repository.Object, _budgetRepository.Object, TestMapperFactory.Create())`.

---

## Lista zadań — `GetProjectTaskListItemDTO` + `ProjectTaskListService`

**Problem:** Frontend oczekuje `description` w elemencie listy zadań; DTO nie zawierało pola (mapowanie w `matdev-project-map.ts` już je czyta).

**Zmiana:**
- Rekord DTO: pole `string? Description`.
- `ProjectTaskListService`: przekazywane `t.Description` przy budowaniu DTO.
- Test `GetTaskListPageAsync_ReturnsPagedDtos`: asercja `Description == "Task desc"`.

---

## `Program.cs`

- Komentarz: PostgreSQL zamiast mylącego „InMemory”.
- `UseHttpsRedirection()` tylko poza `Development` (dev HTTP na `5196` / Docker bez TLS).

---

## Docker i porty — `docker-compose.yml`, `override`, `launchSettings.json`

**Cel:** Ten sam adres co frontend (`MATDEV_API_BASE_URL=http://127.0.0.1:5196`).

| Środowisko | URL |
|------------|-----|
| `dotnet run --launch-profile http` | `http://127.0.0.1:5196` |
| Docker | host `5196` → kontener `8080`, dodatkowo `8080:8080` |

**Zmiany:**
- `ASPNETCORE_URLS=http://+:8080` (bez `HTTPS_PORTS` / 8081).
- Profil VS „Container (Dockerfile)”: `useSSL: false`.
- Usunięte sztywne `container_name` (mniej konfliktów przy wielu instancjach).

---

## Pliki pomocnicze

- **`matdev-pm-backend.slnx`** — pełna solution (API, warstwy, testy, `docker-compose.dcproj`). Build z katalogu repo:

```powershell
cd matdev-pm-backend
dotnet build matdev-pm-backend.slnx
dotnet test matdev.UnitTests/matdev.UnitTests.csproj
```

Nie używaj gołego `dotnet build` w katalogu z wieloma `.csproj` / `.dcproj` — MSB1011.

---

## Weryfikacja

```powershell
dotnet build matdev.API/matdev.API.csproj
dotnet test matdev.UnitTests/matdev.UnitTests.csproj
```

Oczekiwany wynik: **0 błędów**, **133/133 testów passed**.

API lokalnie: PostgreSQL (`ConnectionStrings:Database` w `appsettings.json`) lub `docker compose up` (`matdev.database` + `matdev.api`).

---

## Powiązanie z frontendem

- CORS w `Program.cs`: `http://localhost:3000`, `http://127.0.0.1:3000`.
- Health: `GET /api/health` (używane przez `BackendConnectionCard`).
