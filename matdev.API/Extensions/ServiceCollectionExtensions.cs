using matdev.Application.Interfaces;
using matdev.Application.Services;
using matdev.Domain.Interfaces;
using matdev.Infrastructure.Repositories;

namespace matdev.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddItemServices(this IServiceCollection services)
        {
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IIssueTypeService, IssueTypeService>();
            services.AddScoped<IIssueTypeRepository, IssueTypeRepository>();
            services.AddScoped<IWorkpackageService, WorkpackageService>();
            services.AddScoped<IWorkpackageRepository, WorkpackageRepository>();
            services.AddScoped<ITopicService, TopicService>();
            services.AddScoped<ITopicRepository, TopicRepository>();
            services.AddScoped<ITaskCategoryService, TaskCategoryService>();
            services.AddScoped<ITaskCategoryRepository, TaskCategoryRepository>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IProjectViewService, ProjectViewService>();
            services.AddScoped<IProjectTaskListService, ProjectTaskListService>();
            services.AddScoped<ITaskViewService, TaskViewService>();
            services.AddScoped<IProjectViewRepository, ProjectViewRepository>();
            return services;
        }
    }
}
