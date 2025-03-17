using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheBestBookstore.Data;
using TheBestBookstore.Models;
using TheBestBookstore.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Extract connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Configure the database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(connectionString);
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
});

// Add HttpContextAccessor (removed duplicate)
builder.Services.AddHttpContextAccessor();

// Add Session support (removed duplicate)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register the cart service
builder.Services.AddScoped<ICartService, CartService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// First ensure database exists and seed it
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Ensure database exists before running any SQL
        context.Database.EnsureCreated();
        
        // Seed data manually
        await SeedDatabase(context);
        
        // NOW fix the duplicate ISBNs (AFTER database exists)
        try
        {
            // Create temp table
            context.Database.ExecuteSqlRaw(@"
                CREATE TEMPORARY TABLE IF NOT EXISTS temp_duplicates AS
                SELECT ISBN, ROW_NUMBER() OVER(PARTITION BY ISBN ORDER BY Id) as row_num
                FROM Books
                WHERE ISBN IN (
                    SELECT ISBN FROM Books GROUP BY ISBN HAVING COUNT(*) > 1
                );
            ");
            
            // Update duplicate ISBNs
            context.Database.ExecuteSqlRaw(@"
                UPDATE Books
                SET ISBN = Books.ISBN || '-DUP' || (
                    SELECT row_num 
                    FROM temp_duplicates 
                    WHERE temp_duplicates.ISBN = Books.ISBN
                )
                WHERE ISBN IN (
                    SELECT ISBN FROM Books GROUP BY ISBN HAVING COUNT(*) > 1
                ) AND Id NOT IN (
                    SELECT MIN(Id) FROM Books GROUP BY ISBN HAVING COUNT(*) > 1
                );
            ");
            
            // Drop temp table
            context.Database.ExecuteSqlRaw("DROP TABLE IF EXISTS temp_duplicates;");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred fixing duplicate ISBNs.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();

// Database seeding method
async Task SeedDatabase(ApplicationDbContext context)
{
    // Check if database already has data
    if (context.Books.Any() || context.Categories.Any())
    {
        return; // Database has already been seeded
    }

    // Create sample categories
    var categories = new List<Category>
    {
        new Category { Name = "Fiction" },
        new Category { Name = "Non-Fiction" },
        new Category { Name = "Science Fiction" },
        new Category { Name = "Mystery" },
        new Category { Name = "Biography" }
    };

    await context.Categories.AddRangeAsync(categories);
    await context.SaveChangesAsync();

    // Create sample books with unique ISBNs
    var books = new List<Book>
    {
        new Book { 
            Title = "The Great Gatsby", 
            Author = "F. Scott Fitzgerald", 
            CategoryId = categories[0].Id,
            Description = "A classic novel about the American Dream in the Jazz Age",
            Price = 12.99m,
            Published = new DateTime(1925, 4, 10),
            Pages = 180,
            ISBN = "9780743273565"  // Added ISBN
        },
        new Book { 
            Title = "1984", 
            Author = "George Orwell", 
            CategoryId = categories[2].Id,
            Description = "A dystopian novel set in a totalitarian society",
            Price = 10.99m,
            Published = new DateTime(1949, 6, 8),
            Pages = 328,
            ISBN = "9780451524935"  // Added ISBN
        },
        new Book { 
            Title = "To Kill a Mockingbird", 
            Author = "Harper Lee", 
            CategoryId = categories[0].Id,
            Description = "A novel about racial inequality and moral growth in the American South",
            Price = 15.99m,
            Published = new DateTime(1960, 7, 11),
            Pages = 281,
            ISBN = "9780061120084"  // Added ISBN
        },
        new Book { 
            Title = "A Brief History of Time", 
            Author = "Stephen Hawking", 
            CategoryId = categories[1].Id,
            Description = "A book on cosmology by the renowned physicist",
            Price = 20.99m,
            Published = new DateTime(1988, 4, 1),
            Pages = 256,
            ISBN = "9780553380163"  // Added ISBN
        },
        new Book { 
            Title = "The Da Vinci Code", 
            Author = "Dan Brown", 
            CategoryId = categories[3].Id,
            Description = "A mystery thriller novel combining conspiracies and art history",
            Price = 11.99m,
            Published = new DateTime(2003, 3, 18),
            Pages = 454,
            ISBN = "9780307474278"  // Added ISBN
        }
    };

    await context.Books.AddRangeAsync(books);
    await context.SaveChangesAsync();
}
