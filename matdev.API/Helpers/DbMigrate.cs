using matdev.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace matdev.API.Helpers
{
    public static class DbMigrate
    {
        public static async Task MigrateDbAsync(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();
        }
    }
}
