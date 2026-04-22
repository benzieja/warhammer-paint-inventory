using System.Net.Http.Json;
using System.Text.Json.Serialization;

public class InventoryManager
{
    private readonly HttpClient _http;
    private const string TablePath = "/rest/v1/owned_paints";
    private List<Paint> _paints = [];

    public IReadOnlyList<Paint> Paints => _paints;

    public InventoryManager(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("Supabase");
    }

    public async Task InitializeAsync()
    {
        var masterList = PaintDatabase.BuildPaintList();

        try
        {
            var owned = await _http.GetFromJsonAsync<List<OwnedPaint>>($"{TablePath}?select=name");
            if (owned != null)
            {
                var ownedSet = new HashSet<string>(owned.Select(p => p.Name), StringComparer.OrdinalIgnoreCase);
                foreach (var paint in masterList)
                    paint.IsOwned = ownedSet.Contains(paint.Name);
            }
        }
        catch { }

        _paints = masterList;
    }

    public async Task SetOwnedAsync(string name, bool owned)
    {
        var paint = _paints.FirstOrDefault(p =>
            p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (paint == null) return;

        paint.IsOwned = owned;

        if (owned)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, TablePath);
            request.Headers.Add("Prefer", "resolution=merge-duplicates");
            request.Content = JsonContent.Create(new OwnedPaint(name));
            await _http.SendAsync(request);
        }
        else
        {
            await _http.DeleteAsync($"{TablePath}?name=eq.{Uri.EscapeDataString(name)}");
        }
    }

    public List<Paint> Search(string query) =>
        _paints.Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

    public List<Paint> GetByCategory(PaintCategory category) =>
        _paints.Where(p => p.Category == category).ToList();

    public List<Paint> GetByRack(int rack) =>
        _paints.Where(p => p.RackNumber == rack).ToList();

    public List<Paint> GetOwned() =>
        _paints.Where(p => p.IsOwned).ToList();

    public List<Paint> GetMissing() =>
        _paints.Where(p => !p.IsOwned).ToList();

    public (int Total, int Owned, int Missing) GetStats() =>
        (_paints.Count, _paints.Count(p => p.IsOwned), _paints.Count(p => !p.IsOwned));

    public Dictionary<PaintCategory, (int Total, int Owned)> GetStatsByCategory() =>
        Enum.GetValues<PaintCategory>()
            .ToDictionary(
                cat => cat,
                cat => (
                    _paints.Count(p => p.Category == cat),
                    _paints.Count(p => p.Category == cat && p.IsOwned)
                ));

    private record OwnedPaint([property: JsonPropertyName("name")] string Name);
}
