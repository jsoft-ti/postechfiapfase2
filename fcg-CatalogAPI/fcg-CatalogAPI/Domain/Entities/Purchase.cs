using Domain.Entities;

public class Purchase
{
    public Guid Id { get; private set; }
    public Guid PlayerId { get; private set; }
    public Guid GameId { get; private set; }
    public DateTime PurchaseDate { get; private set; }

    public Game Game { get; private set; } = null!;
    public Player Player { get; private set; } = null!;

    private Purchase() { }

    public Purchase(Guid playerId, Guid gameId)
    {
        Id = Guid.NewGuid();
        PlayerId = playerId;
        GameId = gameId;
        PurchaseDate = DateTime.UtcNow;
    }
}
