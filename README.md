# MatDev PM — backend

REST API aplikacji **MatDev PM** — projekty, zadania, budżety, zlecenia laboratoryjne, użytkownicy.

Frontend: repozytorium [`matdev-pm`](../matdev-pm) (Next.js + Electron).

---

## Stack

| Warstwa | Projekt | Opis |
|---------|---------|------|
| API | `matdev.API` | ASP.NET Core, Swagger |
| Aplikacja | `matdev.Application` | Serwisy, DTO, mapowania |
| Domena | `matdev.Domain` | Encje |
| Infrastruktura | `matdev.Infrastructure` | EF Core, PostgreSQL, repozytoria |
| Testy | `matdev.UnitTests` | xUnit |

**.NET 10** · **PostgreSQL 18** (Docker) · migracje EF Core

---

## Wymagania

- **Docker Desktop** (zalecane)
- **.NET SDK 10** — do lokalnego `dotnet run` i testów
- (Opcjonalnie) Visual Studio / Rider z workload Docker

---

## Szybki start (Docker)

```powershell
cd matdev-pm-backend
docker compose up -d --build
```

| Usługa | Port | Opis |
|--------|------|------|
| API | [http://127.0.0.1:5196](http://127.0.0.1:5196) | REST |
| Swagger | [http://127.0.0.1:5196/swagger](http://127.0.0.1:5196/swagger) | Dokumentacja |
| PostgreSQL | `localhost:5432` | baza `matdev`, user/hasło `postgres` |

Przy **pierwszym starcie** pustej bazy ładowane są dane demo (lookupy + przykładowe projekty). Szczegóły: **[MOCK_DATA.md](./MOCK_DATA.md)**.

Frontend (w drugim repo):

```powershell
cd ..\matdev-pm
copy .env.example .env.local
npm install
npm run dev:web
```

---

## Lokalny start bez Dockera (API)

Potrzebna działająca PostgreSQL z connection stringiem jak w `docker-compose.yml`.

```powershell
cd matdev-pm-backend
dotnet run --project matdev.API
```

Profil HTTP: [http://127.0.0.1:5196](http://127.0.0.1:5196) — patrz `matdev.API/Properties/launchSettings.json`.

---

## Testy

```powershell
cd matdev-pm-backend
dotnet test matdev-pm-backend.slnx
```

---

## Demo — seed / reset

Tylko środowisko **Development**.

```powershell
# Seed, jeśli brak projektów demo
powershell -File scripts/seed-demo.ps1

# Wyczyść projekty i załaduj od zera (prezentacja)
powershell -File scripts/seed-demo.ps1 -Reset
```

Albo HTTP:

```powershell
Invoke-RestMethod -Method POST http://127.0.0.1:5196/api/dev/seed-demo
Invoke-RestMethod -Method POST http://127.0.0.1:5196/api/dev/reset-demo
```

---

## Główne endpointy (prefiks `api/`)

| Obszar | Route |
|--------|--------|
| Projekty | `GET/POST /project`, `GET /project/{id}/view` |
| Zadania | `GET /project/{id}/task-list`, `GET /project/{id}/task/{taskId}/view` |
| Budżet | `GET/POST /project/{id}/budget` |
| Lab orders | `GET/POST /project/{id}/lab-orders`, upload raportów test/final |
| Ryzyka | `GET/POST /project/{id}/risks` |
| Użytkownicy | `GET/POST /user` |
| Lookupy | `/topic`, `/workpackage`, `/issuetype`, `/taskcategory` |
| Dev (Development) | `POST /dev/seed-demo`, `POST /dev/reset-demo` |

Pełna lista: Swagger po starcie API.

---

## Pliki lab (upload)

Raporty zleceń lab trafiają do katalogu uploads (w Dockerze: wolumen `lab_uploads`, ścieżka `/app/uploads`).

---

## Struktura rozwiązania

```
matdev.API/              — kontrolery, Program.cs, Dockerfile
matdev.Application/      — serwisy biznesowe
matdev.Domain/           — encje
matdev.Infrastructure/   — DbContext, migracje, repozytoria
matdev.UnitTests/        — testy jednostkowe
scripts/                 — seed-demo.ps1, api-smoke-test.ps1
docker-compose.yml       — API + PostgreSQL
```

---

## Rozwiązywanie problemów

| Problem | Rozwiązanie |
|---------|-------------|
| API nie startuje w Dockerze (Windows) | `docker compose build matdev.api --no-cache && docker compose up -d` |
| `Access denied` przy uploadzie | Rebuild obrazu API (entrypoint tworzy `/app/uploads`) |
| Port 5432 zajęty | Zatrzymaj inną PostgreSQL lub zmień mapowanie portów w compose |
| Brak dany demo | `scripts/seed-demo.ps1` lub `POST /api/dev/seed-demo` |

Smoke test API:

```powershell
powershell -File scripts/api-smoke-test.ps1
```

---

## Powiązane dokumenty

- [MOCK_DATA.md](./MOCK_DATA.md) — zawartość danych demo
- [matdev-pm/README.md](../matdev-pm/README.md) — frontend, Electron
- [matdev-pm/DESKTOP.md](../matdev-pm/DESKTOP.md) — instalator Windows
