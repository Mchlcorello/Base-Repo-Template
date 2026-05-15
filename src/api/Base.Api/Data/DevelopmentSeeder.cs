using Base.Api.Features.Items;
using Microsoft.EntityFrameworkCore;

namespace Base.Api.Data;

public static class DevelopmentSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Items.AnyAsync(cancellationToken))
        {
            return;
        }

        db.Items.Add(new Item { Id = Guid.NewGuid(), Name = "Hello, world." });
        await db.SaveChangesAsync(cancellationToken);
    }
}
