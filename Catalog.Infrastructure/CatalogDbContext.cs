using Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure;

public sealed class CatalogDbContext : DbContext
{
    private readonly string _schema;
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
        _schema = (options.FindExtension<
            Microsoft.EntityFrameworkCore.Infrastructure.RelationalOptionsExtension>()?
            .MigrationsHistoryTableSchema) ?? "catalog";
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(_schema);

        modelBuilder.Entity<Product>(b =>
        {
            b.ToTable("product");
            b.HasKey(x => x.Id);
            b.Property(x => x.Sku).HasMaxLength(64).IsRequired();
            b.HasIndex(x => x.Sku).IsUnique();
            b.Property(x => x.Name).HasMaxLength(256).IsRequired();
            b.HasMany(x => x.ProductCategories)
                .WithOne(pc => pc.Product)
                .HasForeignKey(pc => pc.ProductId);
        });

        modelBuilder.Entity<Category>(b =>
        {
            b.ToTable("category");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(128).IsRequired();
            b.Property(x => x.Description).HasMaxLength(512);
            b.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.ProductCategories)
                .WithOne(pc => pc.Category)
                .HasForeignKey(pc => pc.CategoryId);
        });

        modelBuilder.Entity<ProductCategory>(b =>
        {
            b.ToTable("product_category");
            b.HasKey(pc => new { pc.ProductId, pc.CategoryId });
        });
    }
}
