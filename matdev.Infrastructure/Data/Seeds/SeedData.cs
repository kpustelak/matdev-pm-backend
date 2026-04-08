using System;
using System.Linq;
using System.Threading.Tasks;
using matdev.Domain.Entities;
using matdev.Domain.Entities.BudgetEntities;
using matdev.Domain.Entities.LabEntities;
using matdev.Domain.Entities.LookupEntities;
using matdev.Domain.Entities.TaskEntities;
using Microsoft.EntityFrameworkCore;

namespace matdev.Infrastructure.Data.Seeds
{
    public static class SeedData
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));

            // Lookups
            if (!await db.IssueTypes.AnyAsync())
            {
                db.IssueTypes.AddRange(
                    new IssueType { Name = "Bug" },
                    new IssueType { Name = "Feature" }
                );
                await db.SaveChangesAsync();
            }

            if (!await db.Workpackages.AnyAsync())
            {
                db.Workpackages.AddRange(
                    new Workpackage { Name = "Core" },
                    new Workpackage { Name = "UI" }
                );
                await db.SaveChangesAsync();
            }

            if (!await db.Topics.AnyAsync())
            {
                db.Topics.AddRange(
                    new Topic { Name = "API" },
                    new Topic { Name = "Frontend" }
                );
                await db.SaveChangesAsync();
            }

            if (!await db.Priorities.AnyAsync())
            {
                db.Priorities.AddRange(
                    new Priority { Name = "Low" },
                    new Priority { Name = "High" }
                );
                await db.SaveChangesAsync();
            }

            if (!await db.Statuses.AnyAsync())
            {
                db.Statuses.AddRange(
                    new Status { Name = "Open" },
                    new Status { Name = "Closed" }
                );
                await db.SaveChangesAsync();
            }

            // Users
            if (!await db.Users.AnyAsync())
            {
                var u1 = new User { FirstName = "Alice", LastName = "Admin", Email = "alice@example.com", PhoneNumber = "+10000000001" };
                var u2 = new User { FirstName = "Bob", LastName = "Developer", Email = "bob@example.com", PhoneNumber = "+10000000002" };
                db.Users.AddRange(u1, u2);
                await db.SaveChangesAsync();
            }

            // Projects
            if (!await db.Projects.AnyAsync())
            {
                var issueType = await db.IssueTypes.FirstAsync();
                var wp = await db.Workpackages.FirstAsync();
                var topic = await db.Topics.FirstAsync();
                var status = await db.Statuses.FirstAsync();
                var priority = await db.Priorities.FirstAsync();
                var users = await db.Users.Take(2).ToListAsync();

                var projectA = new Project
                {
                    Name = "Project A",
                    Description = "Sample project A",
                    CreatedAt = DateTime.UtcNow,
                    IssueType = issueType,
                    Workpackage = wp,
                    Topic = topic,
                    ProjectStatus = status,
                    Priority = priority,
                    Responsible = users.ElementAtOrDefault(0),
                    Support = users.ElementAtOrDefault(1),
                    CreatedBy = users.ElementAtOrDefault(0)
                };

                var projectB = new Project
                {
                    Name = "Project B",
                    Description = "Sample project B",
                    CreatedAt = DateTime.UtcNow,
                    IssueType = issueType,
                    Workpackage = wp,
                    Topic = topic,
                    ProjectStatus = status,
                    Priority = priority,
                    Responsible = users.ElementAtOrDefault(1),
                    Support = users.ElementAtOrDefault(0),
                    CreatedBy = users.ElementAtOrDefault(1)
                };

                db.Projects.AddRange(projectA, projectB);
                await db.SaveChangesAsync();
            }

            // Tasks
            if (!await db.Tasks.AnyAsync())
            {
                var project = await db.Projects.FirstAsync();
                var status = await db.Statuses.FirstAsync();
                var priority = await db.Priorities.FirstAsync();
                var requester = await db.Users.FirstAsync();

                var t1 = new _Task
                {
                    Name = "Design API",
                    Description = "Design REST endpoints",
                    StartDate = DateTime.UtcNow.Date,
                    EndDate = DateTime.UtcNow.Date.AddDays(7),
                    Progress = 10m,
                    IsMilestone = false,
                    Project = project,
                    Status = status,
                    Priority = priority,
                    Requester = requester
                };

                var t2 = new _Task
                {
                    Name = "Implement UI",
                    Description = "Basic frontend",
                    StartDate = DateTime.UtcNow.Date,
                    EndDate = DateTime.UtcNow.Date.AddDays(14),
                    Progress = 0m,
                    IsMilestone = false,
                    Project = project,
                    Status = status,
                    Priority = priority,
                    Requester = requester
                };

                db.Tasks.AddRange(t1, t2);
                await db.SaveChangesAsync();

                // Assignment
                var usersList = await db.Users.Take(2).ToListAsync();
                db.TaskAssignments.Add(new TaskAssignment { Task = t1, User = usersList[0] });
                db.TaskAssignments.Add(new TaskAssignment { Task = t2, User = usersList[1] });
                await db.SaveChangesAsync();

                // Time entries
                db.TimeEntries.Add(new TimeEntry { Task = t1, User = usersList[0], Hours = 2.5f, EntryDate = DateTime.UtcNow.Date });
                db.TimeEntries.Add(new TimeEntry { Task = t2, User = usersList[1], Hours = 1.0f, EntryDate = DateTime.UtcNow.Date });
                await db.SaveChangesAsync();

                // Task dependency (t1 -> t2)
                db.TaskDependencies.Add(new TaskDependency { Predecessor = t1, Successor = t2, DependencyType = DependencyType.FinishToStart, LagTime = 0 });
                await db.SaveChangesAsync();
            }

            // Lab orders
            if (!await db.LabOrderStatuses.AnyAsync())
            {
                db.LabOrderStatuses.AddRange(
                    new LabOrderStatus { Name = "Pending" },
                    new LabOrderStatus { Name = "Completed" }
                );
                await db.SaveChangesAsync();
            }

            if (!await db.LabOrders.AnyAsync())
            {
                var project = await db.Projects.FirstAsync();
                var lo = new LabOrder { Description = "Sample lab order", CreatedAt = DateTime.UtcNow, SampleID = "SAMPLE-001" };
                db.LabOrders.Add(lo);
                await db.SaveChangesAsync();

                db.LabOrderAssignments.Add(new LabOrderAssignment { LabOrder = lo, Project = project });
                await db.SaveChangesAsync();
            }

            // Budget
            if (!await db.BudgetCategories.AnyAsync())
            {
                db.BudgetCategories.AddRange(
                    new BudgetCategory { Name = "Hardware", DefaultAlertThreshold = 1000m },
                    new BudgetCategory { Name = "Software", DefaultAlertThreshold = 500m }
                );
                await db.SaveChangesAsync();
            }

            if (!await db.BudgetPlans.AnyAsync())
            {
                var project = await db.Projects.FirstAsync();
                var cat = await db.BudgetCategories.FirstAsync();
                var plan = new BudgetPlan { Project = project, Name = "Initial Budget", Amount = 10000m, LastUpdated = DateTime.UtcNow };
                db.BudgetPlans.Add(plan);
                await db.SaveChangesAsync();

                db.BudgetExpenditures.Add(new BudgetExpenditure { BudgetPlan = plan, BudgetCategory = cat, Amount = 250m, TransactionDate = DateTime.UtcNow.Date, Description = "Purchase cables", Field = "Procurement" });
                await db.SaveChangesAsync();
            }
        }
    }
}
