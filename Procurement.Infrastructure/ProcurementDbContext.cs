using Procurement.Domain;
using Microsoft.EntityFrameworkCore;

namespace Procurement.Infrastructure;

public sealed class ProcurementDbContext : DbContext
{
    private readonly string _schema;
    public ProcurementDbContext(DbContextOptions<ProcurementDbContext> options) : base(options)
    {
        _schema = (options.FindExtension<
            Microsoft.EntityFrameworkCore.Infrastructure.RelationalOptionsExtension>()?
            .MigrationsHistoryTableSchema) ?? "procurement";
    }

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SupplierProduct> SupplierProducts => Set<SupplierProduct>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(_schema);

        modelBuilder.Entity<Supplier>(b =>
        {
            b.ToTable("supplier");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(256).IsRequired();
            b.Property(x => x.Contact).HasColumnType("jsonb");
            b.Property(x => x.TaxId).HasMaxLength(64);
            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<SupplierProduct>(b =>
        {
            b.ToTable("supplier_product");
            b.HasKey(x => new { x.SupplierId, x.ProductId });
            b.Property(x => x.SupplierSku).HasMaxLength(128);
            b.Property(x => x.Price).HasColumnType("numeric(18,2)").IsRequired();
            b.Property(x => x.Currency).HasMaxLength(3).IsRequired();
            b.Property(x => x.LeadTimeDays);
            b.HasOne(x => x.Supplier)
                .WithMany()
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
