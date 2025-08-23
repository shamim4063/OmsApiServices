using System.Net;
using System.Net.Http.Json;
using Procurement.Application.Catalog;

namespace Procurement.Infrastructure.Catalog;

public sealed class CatalogProductsClient(HttpClient http) : ICatalogProductsClient
{
    public async Task<IReadOnlyDictionary<Guid, ProductDto>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct)
    {
        var set = ids.Distinct().ToArray();
        if (set.Length == 0) return new Dictionary<Guid, ProductDto>();

        using var req = new HttpRequestMessage(HttpMethod.Post, "/v1/products/batch")
        {
            Content = JsonContent.Create(new { ids = set })
        };

        using var res = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
        if (res.StatusCode == HttpStatusCode.NotFound) return new Dictionary<Guid, ProductDto>();
        res.EnsureSuccessStatusCode();

        var list = await res.Content.ReadFromJsonAsync<List<ProductDto>>(cancellationToken: ct) ?? [];
        return list.ToDictionary(p => p.Id);
    }
}
