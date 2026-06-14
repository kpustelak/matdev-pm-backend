# Mock / demo data (Borg Warner lab theme)

Przy **pierwszym starcie** pustej bazy API ładuje dane demo automatycznie.

## Co jest w mock data

### Użytkownicy (6)
Kornel Kowalski (PM), Anna Nowak (lab), Jan Wiśniewski, Maria Kowalska, Tomasz Berg, Eva Schmidt — maile `@borgwarner.demo`.

### Projekty (2)
1. **BW-2026 Turbo Housing Validation** — główny projekt demo  
2. **BW-MAT Alloy Sample Program** — kolejka próbek labowych  

### W projekcie głównym
- **5 zadań** z datami (Gantt), zależności FS, przypisani użytkownicy, time entries  
- **Budżet** 85 000 PLN — alokacje, 4 wydatki (jeden powiązany z zadaniem)  
- **4 lab orders** — różne statusy, raporty test/final na ukończonym  
- **3 ostrzeżenia** (2 aktywne, 1 resolved)  
- **Zespół** na projekcie (assignments)  

### Lookupy
Issue types, topics, workpackages, priorytety, statusy zadań, kategorie budżetu i lab statusy (Created → Completed).

---

## Jak załadować / zresetować

API musi działać w **Development** (`docker compose up -d`).

```powershell
cd matdev-pm-backend

# Tylko jeśli baza nie ma projektów
powershell -File scripts/seed-demo.ps1

# Wyczyść projekty i załaduj mock od zera (prezentacja dla klienta)
powershell -File scripts/seed-demo.ps1 -Reset
```

Albo curl:

```powershell
Invoke-RestMethod -Method POST http://127.0.0.1:5196/api/dev/reset-demo
```

Po resecie wejdź w projekt **BW-2026 Turbo Housing Validation** (ID w `/projects`).

---

## Pliki w kodzie

- `matdev.Infrastructure/Data/Seeds/DemoSeedData.cs` — cała logika mocków  
- `matdev.API/Controllers/DevController.cs` — `POST /api/dev/seed-demo`, `POST /api/dev/reset-demo` (tylko Development)
