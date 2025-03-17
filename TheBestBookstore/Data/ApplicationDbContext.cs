using Microsoft.EntityFrameworkCore;
using TheBestBookstore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace TheBestBookstore.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IServiceProvider serviceProvider) : base(options)
        {
            _serviceProvider = serviceProvider;
            ChangeTracker.LazyLoadingEnabled = true;
        }

        public DbSet<Book> Books { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderDetail> OrderDetails { get; set; } = null!;

        public override int SaveChanges()
        {
            try
            {
                var entries = ChangeTracker
                    .Entries()
                    .Where(e => e.State is EntityState.Added or EntityState.Modified);

                foreach (var entry in entries)
                {
                    switch (entry.Entity)
                    {
                        case Category category:
                            category.Books ??= new List<Book>();
                            break;
                        case Book book:
                            if (entry.State == EntityState.Added)
                            {
                                book.DateAdded = DateTime.Now;
                            }
                            // Validate required relationships
                            if (book.CategoryId > 0)
                            {
                                var categoryExists = Categories.Any(c => c.Id == book.CategoryId);
                                if (!categoryExists)
                                {
                                    throw new InvalidOperationException($"Cannot save book: Category with ID {book.CategoryId} does not exist.");
                                }
                            }
                            break;
                        case CartItem cartItem:
                            if (entry.State == EntityState.Added)
                            {
                                // Verify that the referenced Book exists
                                var book = Books.Find(cartItem.BookId);
                                if (book == null)
                                {
                                    throw new InvalidOperationException($"Cannot add CartItem: Book with ID {cartItem.BookId} does not exist.");
                                }
                            }
                            break;
                    }
                }

                return base.SaveChanges();
            }
            catch (Exception ex)
            {
                // Log detailed error information
                var logger = _serviceProvider.GetService<ILogger<ApplicationDbContext>>();
                if (logger != null)
                {
                    logger.LogError(ex, "Error occurred while saving entity changes.");
                }
                throw new DbUpdateException("Error occurred while saving entity changes. See the inner exception for details.", ex);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Book configurations
            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasIndex(b => b.ISBN).IsUnique();
                
                entity.HasOne(b => b.Category)
                    .WithMany(c => c.Books)
                    .HasForeignKey(b => b.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // CartItem configurations
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(c => c.Id);
                
                entity.HasOne(c => c.Book)
                    .WithMany()
                    .HasForeignKey(c => c.BookId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.Property(c => c.UnitPrice)
                    .HasColumnType("decimal(18,2)");
            });
            
            // OrderDetail configurations
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasOne(od => od.Order)
                    .WithMany(o => o.OrderDetails)
                    .HasForeignKey(od => od.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(od => od.Book)
                    .WithMany()
                    .HasForeignKey(od => od.BookId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.Book)
                .WithMany()
                .HasForeignKey(c => c.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
