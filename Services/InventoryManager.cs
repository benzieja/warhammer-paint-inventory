using Blazored.LocalStorage;

public class InventoryManager
{
    private readonly ILocalStorageService _localStorage;
    private const string OwnedKey = "paint-owned";
    private List<Paint> _paints = [];

    public IReadOnlyList<Paint> Paints => _paints;

    public InventoryManager(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task InitializeAsync()
    {
        var masterList = PaintDatabase.BuildPaintList();

        var ownedNames = await _localStorage.GetItemAsync<List<string>>(OwnedKey);
        if (ownedNames != null)
        {
            var ownedSet = new HashSet<string>(ownedNames, StringComparer.OrdinalIgnoreCase);
            foreach (var paint in masterList)
                paint.IsOwned = ownedSet.Contains(paint.Name);
        }

        _paints = masterList;
    }

    private async Task SaveAsync()
    {
        var owned = _paints.Where(p => p.IsOwned).Select(p => p.Name).ToList();
        await _localStorage.SetItemAsync(OwnedKey, owned);
    }

    public async Task SetOwnedAsync(string name, bool owned)
    {
        var paint = _paints.FirstOrDefault(p =>
            p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (paint != null)
        {
            paint.IsOwned = owned;
            await SaveAsync();
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
}
