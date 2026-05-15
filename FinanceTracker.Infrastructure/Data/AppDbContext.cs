using FinanceTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Budget> Budgets => Set<Budget>();
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {}
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Tags)
                  .HasConversion(
                    v => string.Join(',', v), // Convert List<string> to a comma-separated string for storage
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() // Convert the comma-separated string back to List<string>
                  );
        });
    }

}
