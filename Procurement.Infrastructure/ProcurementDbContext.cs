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
    }
}
