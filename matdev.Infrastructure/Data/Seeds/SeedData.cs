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

            if (!await db.IssueTypes.AnyAsync())
            {
                db.IssueTypes.AddRange(
                    new IssueType { Name = "Bug" },
                    new IssueType { Name = "Feature" });
                await db.SaveChangesAsync();
            }

            if (!await db.Workpackages.AnyAsync())
            {
                db.Workpackages.AddRange(
                    new Workpackage { Name = "Core" },
                    new Workpackage { Name = "UI" });
                await db.SaveChangesAsync();
            }

            if (!await db.Topics.AnyAsync())
            {
                db.Topics.AddRange(
                    new Topic { Name = "API" },
                    new Topic { Name = "Frontend" });
                await db.SaveChangesAsync();
            }

            if (!await db.Priorities.AnyAsync())
            {
                db.Priorities.AddRange(
                    new Priority { Name = "Low" },
                    new Priority { Name = "High" });
                await db.SaveChangesAsync();
            }

            if (!await db.Statuses.AnyAsync())
            {
                db.Statuses.AddRange(
                    new Status { Name = "Open" },
                    new Status { Name = "Closed" });
                await db.SaveChangesAsync();
            }

            if (!await db.Statuses.AnyAsync(s => s.Name == "TODO"))
                db.Statuses.Add(new Status { Name = "TODO" });
            if (!await db.Statuses.AnyAsync(s => s.Name == "IN PROGRESS"))
                db.Statuses.Add(new Status { Name = "IN PROGRESS" });
            await db.SaveChangesAsync();

            if (!await db.Users.AnyAsync())
            {
                db.Users.AddRange(
                    new User { FirstName = "Alice", LastName = "Admin", Email = "alice@example.com", PhoneNumber = "+10000000001" },
                    new User { FirstName = "Bob", LastName = "Developer", Email = "bob@example.com", PhoneNumber = "+10000000002" });
                await db.SaveChangesAsync();
            }

            if (!await db.Projects.AnyAsync())
            {
                var issueType = await db.IssueTypes.FirstAsync();
                var wp = await db.Workpackages.FirstAsync();
                var topic = await db.Topics.FirstAsync();
                var status = await db.Statuses.FirstAsync(s => s.Name == "Open");
                var priority = await db.Priorities.FirstAsync();
                var users = await db.Users.Take(2).ToListAsync();
                var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

                var projectA = new Project
                {
                    Name = "Project A",
                    Description = "Sample project A",
                    CreatedAt = DateTime.UtcNow,
                    StartDate = today,
                    EndDate = today.AddDays(30),
                    IssueTypeID = issueType.IssueTypeID,
                    WorkpackageID = wp.WorkpackageID,
                    TopicID = topic.TopicID,
                    ProjectStatusID = status.StatusID,
                    PriorityID = priority.PriorityID,
                    ResponsibleID = users.ElementAtOrDefault(0)?.UserID,
                    SupportID = users.ElementAtOrDefault(1)?.UserID,
                    CreatedByID = users.ElementAtOrDefault(0)?.UserID
                };

                var projectB = new Project
                {
                    Name = "Project B",
                    Description = "Sample project B",
                    CreatedAt = DateTime.UtcNow,
                    StartDate = today.AddDays(1),
                    EndDate = today.AddDays(45),
                    IssueTypeID = issueType.IssueTypeID,
                    WorkpackageID = wp.WorkpackageID,
                    TopicID = topic.TopicID,
                    ProjectStatusID = status.StatusID,
                    PriorityID = priority.PriorityID,
                    ResponsibleID = users.ElementAtOrDefault(1)?.UserID,
                    SupportID = users.ElementAtOrDefault(0)?.UserID,
                    CreatedByID = users.ElementAtOrDefault(1)?.UserID
                };

                db.Projects.AddRange(projectA, projectB);
                await db.SaveChangesAsync();
            }

            if (!await db.TaskCategories.AnyAsync())
            {
                db.TaskCategories.AddRange(
                    new TaskCategory { Name = "Development" },
                    new TaskCategory { Name = "Testing" },
                    new TaskCategory { Name = "Documentation" });
                await db.SaveChangesAsync();
            }

            if (!await db.Tasks.AnyAsync())
            {
                var project = await db.Projects.FirstAsync();
                var statusTodo = await db.Statuses.FirstAsync(s => s.Name == "TODO");
                var statusProgress = await db.Statuses.FirstAsync(s => s.Name == "IN PROGRESS");
                var priority = await db.Priorities.FirstAsync();
                var requester = await db.Users.FirstAsync();
                var usersList = await db.Users.Take(2).ToListAsync();
                var catDev = await db.TaskCategories.FirstAsync(c => c.Name == "Development");
                var catTest = await db.TaskCategories.FirstAsync(c => c.Name == "Testing");
                var start = DateTime.UtcNow.Date;

                var t1 = new _Task
                {
                    Name = "Design API",
                    Description = "Design REST endpoints",
                    StartDate = start,
                    EndDate = start.AddDays(7),
                    Progress = 10m,
                    IsMilestone = false,
                    SortOrder = 1,
                    Project = project,
                    Status = statusTodo,
                    Priority = priority,
                    Requester = requester,
                    TaskCategory = catDev
                };

                var t2 = new _Task
                {
                    Name = "Implement UI",
                    Description = "Basic frontend",
                    StartDate = start,
                    EndDate = start.AddDays(14),
                    Progress = 0m,
                    IsMilestone = false,
                    SortOrder = 2,
                    Project = project,
                    Status = statusProgress,
                    Priority = priority,
                    Requester = requester,
                    TaskCategory = catTest
                };

                db.Tasks.AddRange(t1, t2);
                await db.SaveChangesAsync();

                db.Tasks.AddRange(
                    new _Task
                    {
                        Name = "OpenAPI draft",
                        Description = "Subtask for Design API",
                        StartDate = start,
                        EndDate = start.AddDays(3),
                        Progress = 0m,
                        IsMilestone = false,
                        SortOrder = 1,
                        ParentID = t1.TaskID,
                        ProjectID = project.ProjectID,
                        StatusID = statusTodo.StatusID,
                        PriorityID = priority.PriorityID,
                        RequesterID = requester.UserID,
                        TaskCategoryID = catDev.TaskCategoryID
                    },
                    new _Task
                    {
                        Name = "Review mockups",
                        Description = "Second subtask for Design API",
                        StartDate = start,
                        EndDate = start.AddDays(5),
                        Progress = 0m,
                        IsMilestone = false,
                        SortOrder = 2,
                        ParentID = t1.TaskID,
                        ProjectID = project.ProjectID,
                        StatusID = statusProgress.StatusID,
                        PriorityID = priority.PriorityID,
                        RequesterID = requester.UserID,
                        TaskCategoryID = catTest.TaskCategoryID
                    });
                await db.SaveChangesAsync();

                db.TaskAssignments.AddRange(
                    new TaskAssignment { Task = t1, User = usersList[0] },
                    new TaskAssignment { Task = t2, User = usersList[1] });
                await db.SaveChangesAsync();

                db.TimeEntries.AddRange(
                    new TimeEntry { Task = t1, User = usersList[0], Hours = 2.5f, EntryDate = DateTime.UtcNow.Date },
                    new TimeEntry { Task = t2, User = usersList[1], Hours = 1.0f, EntryDate = DateTime.UtcNow.Date });
                await db.SaveChangesAsync();

                db.TaskDependencies.Add(new TaskDependency
                {
                    Predecessor = t1,
                    Successor = t2,
                    DependencyType = DependencyType.FinishToStart,
                    LagTime = 0
                });
                await db.SaveChangesAsync();
            }

            if (!await db.LabOrderStatuses.AnyAsync())
            {
                db.LabOrderStatuses.AddRange(
                    new LabOrderStatus { Name = "Pending" },
                    new LabOrderStatus { Name = "Completed" });
                await db.SaveChangesAsync();
            }

            if (!await db.LabOrders.AnyAsync())
            {
                var project = await db.Projects.FirstAsync();
                var status = await db.LabOrderStatuses.FirstAsync();
                var lo = new LabOrder { Description = "Sample lab order", CreatedAt = DateTime.UtcNow, SampleID = "SAMPLE-001", Status = status };
                db.LabOrders.Add(lo);
                await db.SaveChangesAsync();

                db.LabOrderAssignments.Add(new LabOrderAssignment { LabOrder = lo, Project = project });
                await db.SaveChangesAsync();
            }

            if (!await db.BudgetCategories.AnyAsync())
            {
                db.BudgetCategories.AddRange(
                    new BudgetCategory { Name = "Hardware", DefaultAlertThreshold = 1000m },
                    new BudgetCategory { Name = "Software", DefaultAlertThreshold = 500m });
                await db.SaveChangesAsync();
            }

            if (!await db.BudgetPlans.AnyAsync())
            {
                var project = await db.Projects.FirstAsync();
                var cat = await db.BudgetCategories.FirstAsync();
                var plan = new BudgetPlan { Project = project, Name = "Initial Budget", Amount = 10000m, LastUpdated = DateTime.UtcNow };
                db.BudgetPlans.Add(plan);
                await db.SaveChangesAsync();

                db.BudgetExpenditures.Add(new BudgetExpenditure
                {
                    BudgetPlan = plan,
                    BudgetCategory = cat,
                    Amount = 250m,
                    TransactionDate = DateTime.UtcNow.Date,
                    Description = "Purchase cables",
                    Field = "Procurement"
                });
                await db.SaveChangesAsync();
            }
        }
    }
}
