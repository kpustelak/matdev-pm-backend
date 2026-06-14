using matdev.Domain.Entities;
using matdev.Domain.Entities.BudgetEntities;
using matdev.Domain.Entities.LabEntities;
using matdev.Domain.Entities.LookupEntities;
using matdev.Domain.Entities.TaskEntities;
using matdev.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace matdev.Infrastructure.Data.Seeds;

/// <summary>
/// Realistic demo dataset for client presentations (materials lab / turbo validation theme).
/// </summary>
public static class DemoSeedData
{
    public const string DemoMainProjectName = "BW-2026 Turbo Housing Validation";

    public static async Task EnsureLookupsAsync(ApplicationDbContext db)
    {
        await EnsureNamedAsync<IssueType>(db, db.IssueTypes,
            "Validation", "Material Testing", "Engineering Change", "Bug", "Feature");

        await EnsureNamedAsync<Workpackage>(db, db.Workpackages,
            "WP-100 Turbo Development", "WP-200 Materials Lab", "WP-300 Quality", "Core", "UI");

        await EnsureNamedAsync<Topic>(db, db.Topics,
            "Powertrain", "Turbocharger", "Materials Lab", "API", "Frontend");

        await EnsureNamedAsync<Priority>(db, db.Priorities,
            "Low", "Medium", "High", "Critical");

        await EnsureNamedAsync<Status>(db, db.Statuses,
            "Open", "In Progress", "Closed", "TODO", "IN PROGRESS", "Completed");

        await EnsureNamedAsync<TaskCategory>(db, db.TaskCategories,
            "Lab Analysis", "Material Testing", "Procurement", "Documentation",
            "Development", "Testing");

        await EnsureNamedAsync<LabOrderStatus>(db, db.LabOrderStatuses,
            "Created", "In Progress", "Awaiting Results", "Completed");

        await EnsureNamedAsync<BudgetCategory>(db, db.BudgetCategories,
            "Lab consumables", "External testing", "Hardware", "Software", "Travel");

        foreach (var name in new[] { "Lab consumables", "External testing", "Hardware" })
        {
            var cat = await db.BudgetCategories.FirstAsync(c => c.Name == name);
            if (cat.DefaultAlertThreshold == 0)
                cat.DefaultAlertThreshold = name == "External testing" ? 15000m : 8000m;
        }

        await db.SaveChangesAsync();
    }

    public static async Task SeedDemoProjectsAsync(ApplicationDbContext db)
    {
        await EnsureLookupsAsync(db);

        if (await db.Projects.AnyAsync(p => p.Name == DemoMainProjectName))
            return;

        var users = await EnsureDemoUsersAsync(db);
        var today = DateTime.UtcNow.Date;
        var start = DateOnly.FromDateTime(today.AddDays(-21));

        var issueValidation = await db.IssueTypes.FirstAsync(i => i.Name == "Validation");
        var issueMatTest = await db.IssueTypes.FirstAsync(i => i.Name == "Material Testing");
        var wpTurbo = await db.Workpackages.FirstAsync(w => w.Name == "WP-100 Turbo Development");
        var wpLab = await db.Workpackages.FirstAsync(w => w.Name == "WP-200 Materials Lab");
        var topicTurbo = await db.Topics.FirstAsync(t => t.Name == "Turbocharger");
        var topicLab = await db.Topics.FirstAsync(t => t.Name == "Materials Lab");
        var stOpen = await db.Statuses.FirstAsync(s => s.Name == "Open");
        var stProgress = await db.Statuses.FirstAsync(s => s.Name == "In Progress");
        var priHigh = await db.Priorities.FirstAsync(p => p.Name == "High");
        var priMed = await db.Priorities.FirstAsync(p => p.Name == "Medium");
        var priCrit = await db.Priorities.FirstAsync(p => p.Name == "Critical");

        var pm = users[0];
        var labLead = users[1];
        var engineer = users[2];
        var qa = users[3];
        var tech = users[4];

        var project = new Project
        {
            Name = DemoMainProjectName,
            Description =
                "Validation program for next-gen turbo housing alloy. " +
                "Lab tensile/fatigue tests, thermal cycling, and supplier sample tracking.",
            CreatedAt = today.AddDays(-21),
            StartDate = start,
            EndDate = start.AddDays(90),
            IssueTypeID = issueValidation.IssueTypeID,
            WorkpackageID = wpTurbo.WorkpackageID,
            TopicID = topicTurbo.TopicID,
            ProjectStatusID = stProgress.StatusID,
            PriorityID = priCrit.PriorityID,
            ResponsibleID = pm.UserID,
            SupportID = labLead.UserID,
            CreatedByID = pm.UserID,
        };

        var projectLab = new Project
        {
            Name = "BW-MAT Alloy Sample Program",
            Description = "Central lab queue for incoming Borg Warner material samples and test reports.",
            CreatedAt = today.AddDays(-10),
            StartDate = DateOnly.FromDateTime(today.AddDays(-7)),
            EndDate = DateOnly.FromDateTime(today.AddDays(60)),
            IssueTypeID = issueMatTest.IssueTypeID,
            WorkpackageID = wpLab.WorkpackageID,
            TopicID = topicLab.TopicID,
            ProjectStatusID = stOpen.StatusID,
            PriorityID = priHigh.PriorityID,
            ResponsibleID = labLead.UserID,
            SupportID = engineer.UserID,
            CreatedByID = labLead.UserID,
        };

        db.Projects.AddRange(project, projectLab);
        await db.SaveChangesAsync();

        db.ProjectAssignments.AddRange(
            new ProjectAssignment { ProjectID = project.ProjectID, UserID = pm.UserID, IsResponsible = true },
            new ProjectAssignment { ProjectID = project.ProjectID, UserID = labLead.UserID, IsResponsible = false },
            new ProjectAssignment { ProjectID = project.ProjectID, UserID = engineer.UserID, IsResponsible = false },
            new ProjectAssignment { ProjectID = project.ProjectID, UserID = qa.UserID, IsResponsible = false },
            new ProjectAssignment { ProjectID = projectLab.ProjectID, UserID = labLead.UserID, IsResponsible = true },
            new ProjectAssignment { ProjectID = projectLab.ProjectID, UserID = tech.UserID, IsResponsible = false });

        await SeedTasksForProjectAsync(db, project, users, today);
        await SeedBudgetForProjectAsync(db, project, today);
        await SeedLabOrdersForProjectAsync(db, project, projectLab, today);
        await SeedRisksForProjectAsync(db, project, today);

        await db.SaveChangesAsync();
    }

    public static async Task ResetAndSeedAsync(ApplicationDbContext db)
    {
        await db.TaskDependencies.ExecuteDeleteAsync();
        await db.TimeEntries.ExecuteDeleteAsync();
        await db.TaskAssignments.ExecuteDeleteAsync();
        await db.Tasks.ExecuteDeleteAsync();
        await db.BudgetExpenditures.ExecuteDeleteAsync();
        await db.BudgetPlanLines.ExecuteDeleteAsync();
        await db.BudgetPlans.ExecuteDeleteAsync();
        await db.LabOrderAssignments.ExecuteDeleteAsync();
        await db.LabOrders.ExecuteDeleteAsync();
        await db.ProjectRisks.ExecuteDeleteAsync();
        await db.ProjectAssignments.ExecuteDeleteAsync();
        await db.Projects.ExecuteDeleteAsync();

        await SeedDemoProjectsAsync(db);
    }

    private static async Task<List<User>> EnsureDemoUsersAsync(ApplicationDbContext db)
    {
        var demoUsers = new[]
        {
            ("Kornel", "Kowalski", "kornel.kowalski@borgwarner.demo", "+48100000001"),
            ("Anna", "Nowak", "anna.nowak@borgwarner.demo", "+48100000002"),
            ("Jan", "Wiśniewski", "jan.wisniewski@borgwarner.demo", "+48100000003"),
            ("Maria", "Kowalska", "maria.kowalska@borgwarner.demo", "+48100000004"),
            ("Tomasz", "Berg", "tomasz.berg@borgwarner.demo", "+48100000005"),
            ("Eva", "Schmidt", "eva.schmidt@borgwarner.demo", "+49100000006"),
        };

        foreach (var (first, last, email, phone) in demoUsers)
        {
            if (!await db.Users.AnyAsync(u => u.Email == email))
            {
                db.Users.Add(new User
                {
                    FirstName = first,
                    LastName = last,
                    Email = email,
                    PhoneNumber = phone,
                });
            }
        }

        await db.SaveChangesAsync();

        var emails = demoUsers.Select(u => u.Item3).ToList();
        return await db.Users.Where(u => emails.Contains(u.Email)).OrderBy(u => u.UserID).ToListAsync();
    }

    private static async Task SeedTasksForProjectAsync(
        ApplicationDbContext db,
        Project project,
        List<User> users,
        DateTime today)
    {
        var stTodo = await db.Statuses.FirstAsync(s => s.Name == "TODO");
        var stProgress = await db.Statuses.FirstAsync(s => s.Name == "IN PROGRESS");
        var stDone = await db.Statuses.FirstAsync(s => s.Name == "Completed");
        var priHigh = await db.Priorities.FirstAsync(p => p.Name == "High");
        var priMed = await db.Priorities.FirstAsync(p => p.Name == "Medium");
        var catLab = await db.TaskCategories.FirstAsync(c => c.Name == "Lab Analysis");
        var catTest = await db.TaskCategories.FirstAsync(c => c.Name == "Material Testing");
        var catDoc = await db.TaskCategories.FirstAsync(c => c.Name == "Documentation");
        var pm = users[0];

        var tSpec = new _Task
        {
            Name = "Define test matrix",
            Description = "Tensile, fatigue, thermal cycle — acceptance criteria.",
            StartDate = today.AddDays(-18),
            EndDate = today.AddDays(-10),
            Progress = 100m,
            SortOrder = 1,
            ProjectID = project.ProjectID,
            StatusID = stDone.StatusID,
            PriorityID = priHigh.PriorityID,
            RequesterID = pm.UserID,
            TaskCategoryID = catDoc.TaskCategoryID,
            EstimatedCost = 1200m,
        };

        var tSamples = new _Task
        {
            Name = "Receive supplier samples",
            Description = "Log samples BW-TURBO-2026-A/B/C in lab tracker.",
            StartDate = today.AddDays(-14),
            EndDate = today.AddDays(-5),
            Progress = 100m,
            SortOrder = 2,
            ProjectID = project.ProjectID,
            StatusID = stDone.StatusID,
            PriorityID = priMed.PriorityID,
            RequesterID = pm.UserID,
            TaskCategoryID = catTest.TaskCategoryID,
            EstimatedCost = 800m,
        };

        var tTensile = new _Task
        {
            Name = "Tensile strength testing",
            Description = "Lab order SAMPLE-001 — cast housing alloy.",
            StartDate = today.AddDays(-7),
            EndDate = today.AddDays(7),
            Progress = 55m,
            SortOrder = 3,
            ProjectID = project.ProjectID,
            StatusID = stProgress.StatusID,
            PriorityID = priHigh.PriorityID,
            RequesterID = pm.UserID,
            TaskCategoryID = catLab.TaskCategoryID,
            EstimatedCost = 15000m,
        };

        var tFatigue = new _Task
        {
            Name = "High-cycle fatigue",
            Description = "Awaiting tensile results before HCF setup.",
            StartDate = today.AddDays(5),
            EndDate = today.AddDays(28),
            Progress = 0m,
            SortOrder = 4,
            ProjectID = project.ProjectID,
            StatusID = stTodo.StatusID,
            PriorityID = priHigh.PriorityID,
            RequesterID = pm.UserID,
            TaskCategoryID = catLab.TaskCategoryID,
            EstimatedCost = 22000m,
        };

        var tReport = new _Task
        {
            Name = "Validation report milestone",
            Description = "Consolidated report for BW engineering review.",
            StartDate = today.AddDays(25),
            EndDate = today.AddDays(35),
            Progress = 0m,
            IsMilestone = true,
            SortOrder = 5,
            ProjectID = project.ProjectID,
            StatusID = stTodo.StatusID,
            PriorityID = priMed.PriorityID,
            RequesterID = pm.UserID,
            TaskCategoryID = catDoc.TaskCategoryID,
            EstimatedCost = 3000m,
        };

        db.Tasks.AddRange(tSpec, tSamples, tTensile, tFatigue, tReport);
        await db.SaveChangesAsync();

        db.TaskDependencies.AddRange(
            new TaskDependency { PredecessorID = tSpec.TaskID, SuccessorID = tSamples.TaskID, DependencyType = DependencyType.FinishToStart },
            new TaskDependency { PredecessorID = tSamples.TaskID, SuccessorID = tTensile.TaskID, DependencyType = DependencyType.FinishToStart },
            new TaskDependency { PredecessorID = tTensile.TaskID, SuccessorID = tFatigue.TaskID, DependencyType = DependencyType.FinishToStart },
            new TaskDependency { PredecessorID = tFatigue.TaskID, SuccessorID = tReport.TaskID, DependencyType = DependencyType.FinishToStart });

        db.TaskAssignments.AddRange(
            new TaskAssignment { TaskID = tSpec.TaskID, UserID = users[0].UserID },
            new TaskAssignment { TaskID = tSamples.TaskID, UserID = users[4].UserID },
            new TaskAssignment { TaskID = tTensile.TaskID, UserID = users[1].UserID },
            new TaskAssignment { TaskID = tTensile.TaskID, UserID = users[2].UserID },
            new TaskAssignment { TaskID = tFatigue.TaskID, UserID = users[1].UserID },
            new TaskAssignment { TaskID = tReport.TaskID, UserID = users[3].UserID });

        db.TimeEntries.AddRange(
            new TimeEntry { TaskID = tSpec.TaskID, UserID = users[0].UserID, Hours = 6f, EntryDate = today.AddDays(-15) },
            new TimeEntry { TaskID = tTensile.TaskID, UserID = users[1].UserID, Hours = 12f, EntryDate = today.AddDays(-2) },
            new TimeEntry { TaskID = tTensile.TaskID, UserID = users[2].UserID, Hours = 8f, EntryDate = today.AddDays(-1) });
    }

    private static async Task SeedBudgetForProjectAsync(ApplicationDbContext db, Project project, DateTime today)
    {
        var catLab = await db.BudgetCategories.FirstAsync(c => c.Name == "Lab consumables");
        var catExt = await db.BudgetCategories.FirstAsync(c => c.Name == "External testing");
        var catHw = await db.BudgetCategories.FirstAsync(c => c.Name == "Hardware");

        var plan = new BudgetPlan
        {
            ProjectID = project.ProjectID,
            Name = "FY2026 Validation Budget",
            Amount = 85000m,
            LastUpdated = today,
        };
        db.BudgetPlans.Add(plan);
        await db.SaveChangesAsync();

        db.BudgetPlanLines.AddRange(
            new BudgetPlanLine { PlanID = plan.PlanID, CategoryID = catLab.CategoryID, AllocatedAmount = 12000m, AlertThresholdPercent = 80 },
            new BudgetPlanLine { PlanID = plan.PlanID, CategoryID = catExt.CategoryID, AllocatedAmount = 45000m, AlertThresholdPercent = 90 },
            new BudgetPlanLine { PlanID = plan.PlanID, CategoryID = catHw.CategoryID, AllocatedAmount = 18000m, AlertThresholdPercent = 75 });

        var tensileTask = await db.Tasks.FirstAsync(t => t.ProjectID == project.ProjectID && t.Name == "Tensile strength testing");

        db.BudgetExpenditures.AddRange(
            new BudgetExpenditure
            {
                BudgetPlanID = plan.PlanID,
                BudgetCategoryID = catLab.CategoryID,
                Amount = 2450m,
                TransactionDate = today.AddDays(-12),
                Description = "Specimen preparation consumables",
                Field = "Lab",
            },
            new BudgetExpenditure
            {
                BudgetPlanID = plan.PlanID,
                BudgetCategoryID = catExt.CategoryID,
                Amount = 8900m,
                TransactionDate = today.AddDays(-8),
                Description = "External SEM analysis",
                Field = "Supplier",
            },
            new BudgetExpenditure
            {
                BudgetPlanID = plan.PlanID,
                BudgetCategoryID = catExt.CategoryID,
                Amount = 6200m,
                TransactionDate = today.AddDays(-3),
                Description = "Tensile lab run — batch 1",
                Field = "Testing",
                TaskID = tensileTask.TaskID,
            },
            new BudgetExpenditure
            {
                BudgetPlanID = plan.PlanID,
                BudgetCategoryID = catHw.CategoryID,
                Amount = 3100m,
                TransactionDate = today.AddDays(-20),
                Description = "Extensometer calibration kit",
                Field = "Procurement",
            });

        await db.SaveChangesAsync();
    }

    private static async Task SeedLabOrdersForProjectAsync(
        ApplicationDbContext db,
        Project project,
        Project projectLab,
        DateTime today)
    {
        var stCreated = await db.LabOrderStatuses.FirstAsync(s => s.Name == "Created");
        var stProgress = await db.LabOrderStatuses.FirstAsync(s => s.Name == "In Progress");
        var stAwaiting = await db.LabOrderStatuses.FirstAsync(s => s.Name == "Awaiting Results");
        var stCompleted = await db.LabOrderStatuses.FirstAsync(s => s.Name == "Completed");

        var orders = new[]
        {
            new LabOrder
            {
                Description = "Tensile test — cast housing alloy A356",
                SampleID = "BW-TURBO-2026-001",
                CreatedAt = today.AddDays(-6),
                StatusID = stProgress.LabOrderStatusID,
                PlannedCompletionDate = today.AddDays(5),
                PredictedCompletionDate = today.AddDays(7),
            },
            new LabOrder
            {
                Description = "Hardness mapping — weld heat-affected zone",
                SampleID = "BW-TURBO-2026-002",
                CreatedAt = today.AddDays(-4),
                StatusID = stAwaiting.LabOrderStatusID,
                PlannedCompletionDate = today.AddDays(2),
                PredictedCompletionDate = today.AddDays(3),
                TestReportFileName = "hardness-map-v1.pdf",
                TestReportLink = "https://lab.borgwarner.demo/reports/hardness-map-v1.pdf",
            },
            new LabOrder
            {
                Description = "Thermal cycle — 500 cycles @ 850°C",
                SampleID = "BW-TURBO-2026-003",
                CreatedAt = today.AddDays(-2),
                StatusID = stCreated.LabOrderStatusID,
                PlannedCompletionDate = today.AddDays(21),
                PredictedCompletionDate = today.AddDays(24),
            },
            new LabOrder
            {
                Description = "Baseline material cert — supplier batch QC",
                SampleID = "BW-MAT-BASE-044",
                CreatedAt = today.AddDays(-30),
                StatusID = stCompleted.LabOrderStatusID,
                PlannedCompletionDate = today.AddDays(-10),
                PredictedCompletionDate = today.AddDays(-8),
                CompletionDate = today.AddDays(-9),
                TestReportFileName = "qc-baseline-test.pdf",
                TestReportLink = "https://lab.borgwarner.demo/reports/qc-baseline-test.pdf",
                FinalReportFileName = "BW-MAT-BASE-044-final.pdf",
                FinalReportLink = "https://lab.borgwarner.demo/reports/BW-MAT-BASE-044-final.pdf",
            },
        };

        db.LabOrders.AddRange(orders);
        await db.SaveChangesAsync();

        db.LabOrderAssignments.AddRange(
            new LabOrderAssignment { LabOrderID = orders[0].LabOrderID, ProjectID = project.ProjectID },
            new LabOrderAssignment { LabOrderID = orders[1].LabOrderID, ProjectID = project.ProjectID },
            new LabOrderAssignment { LabOrderID = orders[2].LabOrderID, ProjectID = project.ProjectID },
            new LabOrderAssignment { LabOrderID = orders[3].LabOrderID, ProjectID = projectLab.ProjectID });
    }

    private static async Task SeedRisksForProjectAsync(ApplicationDbContext db, Project project, DateTime today)
    {
        db.ProjectRisks.AddRange(
            new ProjectRisk
            {
                ProjectID = project.ProjectID,
                Severity = "High",
                Description = "Supplier sample BW-TURBO-2026-003 delayed — may slip fatigue window.",
                IsResolved = false,
                CreatedAt = today.AddDays(-1),
            },
            new ProjectRisk
            {
                ProjectID = project.ProjectID,
                Severity = "Medium",
                Description = "External SEM lab capacity booked until next week.",
                IsResolved = false,
                CreatedAt = today.AddDays(-3),
            },
            new ProjectRisk
            {
                ProjectID = project.ProjectID,
                Severity = "Low",
                Description = "Travel approval for BW review meeting — pending.",
                IsResolved = true,
                CreatedAt = today.AddDays(-10),
            });
    }

    private static async Task EnsureNamedAsync<T>(
        ApplicationDbContext db,
        DbSet<T> set,
        params string[] names) where T : class, new()
    {
        foreach (var name in names)
        {
            var exists = await set.AnyAsync(e => EF.Property<string>(e, "Name") == name);
            if (exists) continue;

            var entity = Activator.CreateInstance<T>();
            var prop = typeof(T).GetProperty("Name");
            if (prop != null && prop.CanWrite)
                prop.SetValue(entity, name);
            set.Add(entity);
        }

        await db.SaveChangesAsync();
    }
}
