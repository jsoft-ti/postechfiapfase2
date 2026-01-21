using Domain.Entities;

public class CartItem
{
    public Guid Id { get; private set; }
    public Guid CartId { get; private set; }
    public Guid PlayerId { get; private set; }
    public Guid GalleryId { get; private set; }

    public Cart Cart { get; private set; } = null!;
    public GalleryGame Gallery { get; private set; } = null!;
    public Player Player { get; private set; } = null!;

    private CartItem() { }

    public CartItem(Guid playerId, Guid galleryId, Guid cartId)
    {
        if (playerId == Guid.Empty)
            throw new ArgumentException("PlayerId cannot be empty", nameof(playerId));
        
        if (galleryId == Guid.Empty)
            throw new ArgumentException("GameId cannot be empty", nameof(galleryId));
        
        if (cartId == Guid.Empty)
            throw new ArgumentException("CartId cannot be empty", nameof(cartId));

        Id = Guid.NewGuid();
        PlayerId = playerId;
        GalleryId = galleryId;
        CartId = cartId;
    }
}