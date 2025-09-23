using InzSeeder.Core.Extensions;
using InzSeeder.Core.Models;
using InzSeeder.Samples.Web.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var isSeedMode = args.Contains("seedMode");
if (isSeedMode)
{
    const string seederConfigSectionName = "Seeder";
    builder.Services.AddOptions<SeederConfiguration>().BindConfiguration(seederConfigSectionName);
    var seedingSettings = builder.Configuration.GetSection(seederConfigSectionName).Get<SeederConfiguration>();
    builder.Services.AddInzSeeder(seedingSettings)
        .UseDbContext<ApplicationDbContext>()
        .RegisterEntitySeedersFromAssemblies(typeof(Program).Assembly)
        .RegisterEmbeddedSeedDataFromAssemblies(typeof(Program).Assembly);
}

var app = builder.Build();

if (isSeedMode)
{
    using var scope = app.Services.CreateScope();
    var appDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var isCreated = await appDbContext.Database.EnsureCreatedAsync();
    Console.WriteLine(isCreated ? "Database created successfully." : "Database already exists.");

    await scope.ServiceProvider.RunInzSeeder();
    await DisplaySeededData(appDbContext);

    return;
}

await app.RunAsync();
return;

async Task DisplaySeededData(ApplicationDbContext dbContext)
{
    Console.WriteLine("\n--- Seeded Data ---");

    var products = await dbContext.Products.ToListAsync();
    Console.WriteLine($"\nProducts ({products.Count} total):");
    foreach (var product in products.Take(5))
    {
        Console.WriteLine($"  - {product.Id}: {product.Name} (${product.Price})");
    }

    if (products.Count > 5) Console.WriteLine($"  ... and {products.Count - 5} more");

    var users = await dbContext.Users.ToListAsync();
    Console.WriteLine($"\nUsers ({users.Count} total):");
    foreach (var user in users.Take(5))
    {
        Console.WriteLine($"  - {user.Id}: {user.FirstName} {user.LastName} ({user.Email})");
    }

    if (users.Count > 5) Console.WriteLine($"  ... and {users.Count - 5} more");

    var categories = await dbContext.Categories.ToListAsync();
    Console.WriteLine($"\nCategories ({categories.Count} total):");
    foreach (var category in categories)
    {
        Console.WriteLine($"  - {category.Id}: {category.Name} (Slug: {category.Slug}, Active: {category.IsActive})");
    }

    var notificationTemplates = await dbContext.NotificationTemplates.ToListAsync();
    Console.WriteLine($"\nNotification Templates ({notificationTemplates.Count} total):");
    foreach (var template in notificationTemplates)
    {
        Console.WriteLine($"  - {template.Id}: {template.Name} (Key: {template.Key}, Method: {template.DeliveryMethod})");
    }

    var notificationTypes = await dbContext.NotificationTypes.ToListAsync();
    Console.WriteLine($"\nNotification Types ({notificationTypes.Count} total):");
    foreach (var type in notificationTypes)
    {
        Console.WriteLine($"  - {type.Id}: {type.Name} (Key: {type.Key}, Audience: {type.TargetAudience})");
    }
}