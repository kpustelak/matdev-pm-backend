# Budżet + warningi — zadania dla backendu

Dokument dla osoby od API / EF / domeny.

| Plik | Po co |
|------|--------|
| **PROMPT_AGENT_BUDZET_BACKEND.md** | Gotowy tekst do wklejenia w agenta Cursor |
| `matdev-pm/FRONTEND_BUDGET_ZADANIA.md` | Spec frontendu (kontrakt) |
| `matdev-pm/BUDGET_PODZIAL_DLA_AGENTOW.md` | Instrukcja dla PM — kogo kiedy odpalać |

**Cel produktu:** osobna zakładka **Budget** w widoku projektu, pełny CRUD planu, alokacje po kategoriach, sensowne alerty (powiązane z istniejącymi **Warnings**), opcjonalnie wydatki / limity na poziomie **zadania**.

---

## Stan dziś (nie powielaj pracy)

| Jest | Brakuje |
|------|---------|
| `BudgetPlan`, `BudgetCategory`, `BudgetExpenditure` | `POST` utworzenia planu |
| `GET/PUT` planu, `POST/DELETE` wydatków | unikalność 1 aktywnego planu / projekt |
| `DefaultAlertThreshold` na kategorii | użycie progu w logice alertów |
| Auto-warning tylko przy `spent > plan` w `ProjectRiskService` | progi 80%/100%, alert per kategoria |
| `freeBudget` obcięte do 0 w DTO | `isOverBudget`, `overAmount`, `utilizationPercent` |

---

## Faza 1 — MVP zakładki (priorytet na start)

### 1.1 Utworzenie planu budżetowego

- [ ] `POST /api/project/{projectId}/budget`
  - Body: `{ "name": string, "amount": decimal, "currency": "PLN" }` (currency opcjonalnie, default PLN)
  - `201` + `GetProjectBudgetDTO`
  - `409` jeśli projekt ma już aktywny plan (po migracji unique)
- [ ] Migracja: `IX_BudgetPlans_ProjectID` → **UNIQUE** (jeden plan na projekt w Fazie 1; później wiele planów + flaga `IsActive`)
- [ ] Opcjonalnie: przy `POST /api/project` seeduj pusty plan albo zostaw bez planu — uzgodnij z frontendem (on obsłuży „Utwórz plan”)

### 1.2 Uczciwe DTO budżetu

Rozszerz `GetProjectBudgetDTO`:

```json
{
  "planId": 1,
  "planName": "Plan główny",
  "totalAmount": 10000,
  "totalSpent": 8500,
  "freeBudget": 1500,
  "utilizationPercent": 85.0,
  "isOverBudget": false,
  "overAmount": 0,
  "categories": [ ... ]
}
```

- [ ] Gdy `totalSpent > totalAmount`: `freeBudget` może być 0, ale **`overAmount` > 0** i **`isOverBudget: true`** (nie ukrywać overspendu)
- [ ] Wspólna metoda `BudgetCalculator.Snapshot(plan)` używana w:
  - `BudgetService.MapToDTO`
  - `ProjectService.MapToDto` (`budgetAmount` / `budgetSpent`)
  - `ProjectRiskService.ComputeAutoRisksAsync`

### 1.3 Alokacje per kategoria (linie planu)

Nowa encja **`BudgetPlanLine`**:

| Pole | Typ |
|------|-----|
| `LineID` | int PK |
| `PlanID` | FK |
| `CategoryID` | FK |
| `AllocatedAmount` | decimal(18,2) |
| `AlertThresholdPercent` | int? (np. 80 — alert przy 80% puli kategorii) |

- [ ] `GET /api/project/{projectId}/budget/lines`
- [ ] `PUT /api/project/{projectId}/budget/lines` — replace całej listy (walidacja: suma ≤ `plan.Amount` albo `plan.Amount` = suma — wybierz jedną regułę i opisz w Swagger)
- [ ] W `GetProjectBudgetDTO` per kategoria dodaj: `allocatedAmount`, `remainingInCategory`, `categoryUtilizationPercent`

### 1.4 Wydatki

- [ ] `PUT /api/project/{projectId}/budget/expenditures/{expenditureId}` — edycja
- [ ] Walidacja: `categoryId` istnieje, `amount > 0`, `transactionDate` sensowna
- [ ] Opcjonalnie w body wydatku: `taskId` (int?) — **Faza 2**, migracja nullable FK

### 1.5 Kategorie (słownik globalny)

- [ ] `GET` — już jest (`.../budget/categories`)
- [ ] `POST /api/budget/categories` — admin tworzy kategorię (`name`, `defaultAlertThreshold`)
- [ ] `PUT /api/budget/categories/{id}` — edycja progu domyślnego
- [ ] Nie kasuj kategorii z wydatkami (409 lub soft-delete)

### 1.6 Auto-warningi (istniejący `ProjectRiskService`)

Rozszerz `ComputeAutoRisksAsync` — **bez nowej tabeli Notification** na start:

| Reguła | Severity | `RiskId` (auto) |
|--------|----------|-----------------|
| Plan ≥ 80% wykorzystania | Medium | stały klucz np. `-100001` |
| Plan ≥ 100% (overspend) | High | `-100002` lub `int.MinValue` (jak dziś) |
| Kategoria ≥ `AlertThresholdPercent` linii | Medium | `-categoryId` |
| Kategoria ≥ 100% alokacji | High | `-categoryId - offset` |

- [ ] Użyj `DefaultAlertThreshold` jako fallback gdy brak linii planu dla kategorii (limit **kwotowy** absolutny)
- [ ] Stałe `ReferenceKey` w opisie lub osobne pole w DTO później — na razie wystarczy stabilny `RiskId` ujemny

### 1.7 Testy

- [ ] `BudgetServiceTests` — create plan, add expenditure, over budget DTO
- [ ] `ProjectRiskServiceTests` — progi 80/100, kategoria
- [ ] Aktualizuj seed: 2 kategorie z liniami planu

---

## Faza 2 — Budżet na zadaniu (nie głupie — rekomendowane)

**Idea:** zadanie nie musi mieć własnego „planu”, tylko **przypisane wydatki** i/lub **limit szacunkowy**.

### Model

```
Task
  + EstimatedCost? (decimal, opcjonalny budżet zadania)

BudgetExpenditure
  + TaskID? (nullable FK → Task)
```

- [ ] `GET /api/project/{projectId}/task-list` lub task view — zwróć `estimatedCost`, `taskSpent` (suma wydatków z `TaskID`)
- [ ] `POST` wydatku z `taskId` — wpis pojawia się w budżecie projektu **i** w panelu zadania
- [ ] Auto-warning: `taskSpent > estimatedCost` → auto risk Medium/High

**Reguła:** wydatki zawsze liczą się do planu projektu; zadanie to **wymiar analityczny** (slice), nie osobna kasa — chyba że później dodacie „sub-plan” (Faza 3).

---

## Faza 3 — Wiele planów / okresy (później)

- [ ] `BudgetPlan.IsActive`, `PlanType` (Master | Period | Revision), `ParentPlanID?`, `PeriodLabel?`
- [ ] Dokładnie jeden `IsActive = true` na projekt
- [ ] Wydatki tylko na aktywny plan
- [ ] Endpoint `GET .../budget/plans` — lista, `POST .../budget/plans/{id}/activate`

---

## Faza 4 — Powiadomienia (opcjonalnie)

- [ ] Tabela `Notification` + `BudgetAlertDismissal` (snooze)
- [ ] Po `AddExpenditure` / `UpdateBudgetPlan` wywołaj `IBudgetAlertEvaluator` (event), nie tylko GET risks
- [ ] `GET /api/project/{projectId}/notifications`

---

## Kontrakt API — podsumowanie dla frontu (Faza 1)

| Metoda | Ścieżka |
|--------|---------|
| GET | `/api/project/{id}/budget` |
| POST | `/api/project/{id}/budget` |
| PUT | `/api/project/{id}/budget` |
| GET | `/api/project/{id}/budget/lines` |
| PUT | `/api/project/{id}/budget/lines` |
| GET | `/api/project/{id}/budget/categories` |
| POST | `/api/project/{id}/budget/expenditures` |
| PUT | `/api/project/{id}/budget/expenditures/{expenditureId}` |
| DELETE | `/api/project/{id}/budget/expenditures/{expenditureId}` |
| GET | `/api/project/{id}/risks` | (już jest — auto budget warnings) |

Format odpowiedzi: `ResponseModel<T>` z `{ data, message }` — bez zmian.

---

## Kolejność PR (sugerowana)

1. `BudgetCalculator` + poprawione DTO + unique ProjectID  
2. `POST` create plan + testy  
3. `BudgetPlanLine` + PUT lines  
4. Rozszerzone auto-risks  
5. `TaskID` na wydatku + estimated cost (Faza 2)

---

## Pliki do ruszenia

- `matdev.Domain/Entities/BudgetEntities/*`
- `matdev.Infrastructure/Data/ApplicationDbContext.cs` + migracja
- `matdev.Infrastructure/Repositories/BudgetRepository.cs`
- `matdev.Application/Services/BudgetService.cs`
- `matdev.Application/Services/ProjectRiskService.cs`
- `matdev.Application/Services/ProjectService.cs`
- `matdev.API/Controllers/BudgetController.cs`
- `matdev.Application/DTOs/Budget/*`

Pytania do PM/frontu: czy `/budgets` globalna lista ma agregować wszystkie projekty — na backendzie wystarczy istniejące `GetAll` w repo + nowy endpoint `GET /api/budget/overview` (Faza 1.5 opcjonalnie).
