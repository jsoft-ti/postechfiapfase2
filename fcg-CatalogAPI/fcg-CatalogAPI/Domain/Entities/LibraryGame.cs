namespace Domain.Entities;

public class LibraryGame
{
    public Guid Id { get; private set; }
    public Guid GalleryId { get; private set; }
    public GalleryGame Gallery { get; private set; } = null!;
    public Guid PlayerId { get; private set; }
    public Player Player { get; private set; } = null!;
    public DateTime PurchaseDate { get; private set; }
    public decimal PurchasePrice { get; private set; }

    protected LibraryGame() { }

    public LibraryGame(Guid galleryId, Guid playerId, decimal purchasePrice)
    {
        if (purchasePrice < 0)
            throw new ArgumentException("Preço de compra inválido");

        Id = Guid.NewGuid();
        GalleryId = galleryId;
        PlayerId = playerId;
        PurchaseDate = DateTime.UtcNow;
        PurchasePrice = purchasePrice;
    }

    public string GetDescription()
        => $"{Gallery.GetDescription()} - Comprado em {PurchaseDate:d} por {PurchasePrice:C}";
}
