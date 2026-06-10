using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using representweb.Models;

namespace representweb.Data
{
    public static class DbInitializer
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            // Only seed if no products exist
            if (await context.Products.AnyAsync()) return;

            var productsPath = Path.Combine(AppContext.BaseDirectory, "Data", "products.json");
            if (!File.Exists(productsPath))
            {
                var fallbackPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "products.json"));
                if (File.Exists(fallbackPath))
                    productsPath = fallbackPath;
            }

            if (!File.Exists(productsPath))
                return;

            var json = await File.ReadAllTextAsync(productsPath);
            var products = JsonSerializer.Deserialize<List<Product>>(json, _jsonOptions);

            if (products is not null && products.Any())
            {
                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }
        }
    }
}
