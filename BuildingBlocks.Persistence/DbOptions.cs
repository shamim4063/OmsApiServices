namespace BuildingBlocks.Persistence;
public sealed class DbOptions
{
    public string ConnectionString { get; set; } = default!;
    public string Schema { get; set; } = "public";
}
