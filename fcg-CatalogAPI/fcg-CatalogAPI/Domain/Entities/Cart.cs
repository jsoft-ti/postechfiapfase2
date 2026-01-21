namespace Domain.Entities;

public class Cart
{
    public Guid Id { get; private set; }
    public Guid PlayerId { get; private set; }
    private readonly List<CartItem> _items = new();

    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();
    public Player Player { get; private set; } = null!;

    private Cart() { }

    public Cart(Guid playerId)
    {
        if (playerId == Guid.Empty)
            throw new ArgumentException("PlayerId cannot be empty", nameof(playerId));

        Id = Guid.NewGuid();
        PlayerId = playerId;
    }

    public void AddItem(GalleryGame gallery)
    {
        if (gallery == null)
            throw new ArgumentNullException(nameof(gallery));

        if (gallery.Id == Guid.Empty)
            throw new ArgumentException("Game Id cannot be empty", nameof(gallery));

        if (_items.Any(i => i.GalleryId == gallery.Id)) 
            return;

        _items.Add(new CartItem(PlayerId, gallery.Id, Id));
    }

    public void RemoveItem(Guid gameId)
    {
        var item = _items.FirstOrDefault(i => i.GalleryId == gameId);
        if (item != null) _items.Remove(item);
    }

    public void Clear() => _items.Clear();
}