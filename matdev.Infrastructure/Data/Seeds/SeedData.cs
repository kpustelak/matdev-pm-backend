using matdev.Infrastructure.Data;

namespace matdev.Infrastructure.Data.Seeds;

public static class SeedData
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (db == null) throw new ArgumentNullException(nameof(db));

        await DemoSeedData.EnsureLookupsAsync(db);
        await DemoSeedData.SeedDemoProjectsAsync(db);
    }
}
