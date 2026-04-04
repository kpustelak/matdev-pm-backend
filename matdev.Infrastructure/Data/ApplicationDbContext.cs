using matdev.Domain.Entities;
using matdev.Domain.Entities.Lab;
using Microsoft.EntityFrameworkCore;

namespace matdev.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<_Task> Tasks { get; set; }
        public DbSet<TaskAssigment> TaskAssigments { get; set; }
        public DbSet<TimeEntry> TimeEntries { get; set; }
        public DbSet<IssueType> IssueTypes { get; set; }
        public DbSet<Workpackage> Workpackages { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Priority> Priorities { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<LabOrder> LabOrders { get; set; }
        public DbSet<LabOrderStatus> LabOrderStatuses { get; set; }
        public DbSet<LabOrderAssigment> LabOrderAssigments { get; set; }
        public DbSet<TaskDependency> TaskDependencies { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Description).HasMaxLength(1000);
                entity.Property(p => p.Price).HasPrecision(18, 2);
            });

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserID);
                entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).HasMaxLength(200);
                entity.Property(u => u.PhoneNumber).HasMaxLength(50);
            });

            // Project
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(p => p.ProjectID);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Description).HasMaxLength(2000);
                entity.Property(p => p.CreatedAt).IsRequired();

                entity.HasOne(p => p.IssueType).WithMany().HasForeignKey(p => p.IssueTypeID).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(p => p.Workpackage).WithMany().HasForeignKey(p => p.WorkpackageID).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(p => p.Topic).WithMany().HasForeignKey(p => p.TopicID).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(p => p.ProjectStatus).WithMany().HasForeignKey(p => p.ProjectStatusID).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(p => p.Priority).WithMany().HasForeignKey(p => p.PriorityID).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(p => p.Responsible).WithMany().HasForeignKey(p => p.ResponsibleID).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(p => p.Support).WithMany().HasForeignKey(p => p.SupportID).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(p => p.CreatedBy).WithMany().HasForeignKey(p => p.CreatedByID).OnDelete(DeleteBehavior.SetNull);
            });

            // _Task
            modelBuilder.Entity<_Task>(entity =>
            {
                entity.HasKey(t => t.TaskID);
                entity.Property(t => t.Name).IsRequired().HasMaxLength(200);
                entity.Property(t => t.Description).HasMaxLength(2000);
                entity.Property(t => t.Progress).HasPrecision(5, 2);

                entity.HasOne(t => t.Project).WithMany(p => p.Tasks).HasForeignKey(t => t.ProjectID).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Status).WithMany().HasForeignKey(t => t.StatusID).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(t => t.Priority).WithMany().HasForeignKey(t => t.PriorityID).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(t => t.ParentTask).WithMany().HasForeignKey(t => t.ParentID).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(t => t.Requester).WithMany().HasForeignKey(t => t.RequesterID).OnDelete(DeleteBehavior.SetNull);
            });

            // TaskAssigment
            modelBuilder.Entity<TaskAssigment>(entity =>
            {
                entity.HasKey(a => a.TaskAssigmentID);
                entity.HasOne(a => a.User).WithMany(u => u.TaskAssigments).HasForeignKey(a => a.UserID).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(a => a.Task).WithMany(t => t.Assigments).HasForeignKey(a => a.TaskID).OnDelete(DeleteBehavior.Cascade);
            });

            // TimeEntry
            modelBuilder.Entity<TimeEntry>(entity =>
            {
                entity.HasKey(te => te.TimeEntryID);
                entity.Property(te => te.Hours).IsRequired();
                entity.Property(te => te.EntryDate).IsRequired();
                entity.HasOne(te => te.Task).WithMany(t => t.TimeEntries).HasForeignKey(te => te.TaskID).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(te => te.User).WithMany(u => u.TimeEntries).HasForeignKey(te => te.UserID).OnDelete(DeleteBehavior.Cascade);
            });

            // IssueType
            modelBuilder.Entity<IssueType>(entity =>
            {
                entity.HasKey(e => e.IssueTypeID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            });

            // Workpackage
            modelBuilder.Entity<Workpackage>(entity =>
            {
                entity.HasKey(e => e.WorkpackageID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            });

            // Topic
            modelBuilder.Entity<Topic>(entity =>
            {
                entity.HasKey(e => e.TopicID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            });

            // Priority
            modelBuilder.Entity<Priority>(entity =>
            {
                entity.HasKey(e => e.PriorityID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            // Status
            modelBuilder.Entity<Status>(entity =>
            {
                entity.HasKey(e => e.StatusID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            // LabOrder
            modelBuilder.Entity<LabOrder>(entity =>
            {
                entity.HasKey(l => l.LabOrderID);
                entity.Property(l => l.Description).HasMaxLength(2000);
                entity.Property(l => l.CreatedAt).IsRequired();
                entity.Property(l => l.SampleID).HasMaxLength(100);
            });

            // LabOrderStatus
            modelBuilder.Entity<LabOrderStatus>(entity =>
            {
                entity.HasKey(s => s.LabOrderStatusID);
                entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
            });

            // LabOrderAssigment
            modelBuilder.Entity<LabOrderAssigment>(entity =>
            {
                entity.HasKey(a => a.LabOrderAssigmentID);
                entity.HasOne(a => a.LabOrder).WithMany().HasForeignKey(a => a.LabOrderID).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(a => a.Project).WithMany(p => p.LabOrderAssigments).HasForeignKey(a => a.ProjectID).OnDelete(DeleteBehavior.Cascade);
            });

            // TaskDependency dodac relacje do Task i User
            modelBuilder.Entity<TaskDependency>(entity =>
            {
                entity.HasKey(d => d.TaskDependencyID);
                entity.Property(d => d.DependencyType).IsRequired();
                entity.Property(d => d.LagTime).HasDefaultValue(0);
            });

            // Seed data for Product
            modelBuilder.Entity<Product>().HasData(
                new Product("Test Product 1", "Description 1", 19.99m) { Id = 1 },
                new Product("Test Product 2", "Description 2", 29.99m) { Id = 2 }
            );
        }
    }
}
