# Prompt dla agenta — BACKEND (budżet)

Skopiuj **cały blok** poniżej jako pierwszą wiadomość w nowym agencie Cursor z workspace **matdev-pm-backend**.

---

```
Jesteś agentem backendu w projekcie matdev-pm-backend (.NET, EF Core, PostgreSQL).

## Zadanie
Zaimplementuj Fazę 1 budżetu według pliku BUDGET_BACKEND_ZADANIA.md w katalogu głównym repozytorium.
Przeczytaj ten plik w całości przed kodowaniem.

## Cel biznesowy
Frontend buduje osobną zakładkę "Budget" na każdy projekt. Ty dostarczasz API:
- utworzenie planu budżetowego (dziś brak POST — tylko seed),
- uczciwe DTO (nie ukrywaj overspendu: isOverBudget, overAmount, utilizationPercent),
- podział planu po kategoriach (nowa encja BudgetPlanLine + GET/PUT lines),
- edycja wydatku (PUT),
- rozszerzone automatyczne warningi w ProjectRiskService (80%, 100% planu, progi kategorii),
- testy jednostkowe BudgetService + progi risks.

## Czego NIE robisz w tej iteracji
- Nie dotykasz repozytorium matdev-pm (frontend).
- Nie implementujesz Fazy 3 (wiele planów / wersje) ani Fazy 4 (tabela Notification) — tylko jeśli Faza 1 skończona wcześniej i PM każe.
- Faza 2 (taskId na wydatku, EstimatedCost) — osobny PR po Fazie 1; nie zaczynaj bez potwierdzenia.

## Wymagania techniczne
- Zachowaj ResponseModel<T> i konwencje kontrolerów jak w BudgetController.
- Jedna migracja EF dla BudgetPlanLine + UNIQUE na BudgetPlans.ProjectID.
- Wspólny helper/kalkulator budżetu używany w BudgetService, ProjectService, ProjectRiskService.
- DefaultAlertThreshold na BudgetCategory — użyj w alertach (dziś martwe pole).
- Po zmianach: dotnet build + dotnet test (wszystkie testy muszą przejść).

## Definition of done (Faza 1)
1. Wszystkie checkboxy Fazy 1 w BUDGET_BACKEND_ZADANIA.md odhaczone lub uzasadnione pominięcie.
2. Krótki komentarz na końcu: tabela endpointów + przykładowy JSON GetProjectBudgetDTO i PUT lines.
3. Nie commituj sekretów; nie zmieniaj git config.

## Kontekst istniejącego kodu
- BudgetService.MapToDTO obcina freeBudget do 0 — to napraw.
- ProjectRiskService już ma auto-warning tylko przy spent > plan.Amount.
- CreateProject nie tworzy planu — POST budget to załatwia.

Zacznij od przeczytania: BudgetService.cs, BudgetController.cs, ProjectRiskService.cs, encje w Domain/Entities/BudgetEntities.
```

---

## Druga wiadomość (opcjonalnie, po Fazie 1)

```
Faza 1 zaakceptowana. Zrób Fazę 2 z BUDGET_BACKEND_ZADANIA.md:
- TaskID nullable na BudgetExpenditure,
- EstimatedCost na Task,
- auto-risk gdy suma wydatków zadania > estimatedCost,
- rozszerz DTO task list / task view o taskSpent.
Wypisz zmiany kontraktu API dla frontendu.
```

---

## Trzecia wiadomość (gdy front zgłosi bugi integracji)

```
Front zgłasza: [wklej błąd / response].
Sprawdź tylko endpoint [X] i DTO — minimalny fix, bez refaktoru poza budżetem.
```

---

Szczegółowa lista zadań: **BUDGET_BACKEND_ZADANIA.md**
