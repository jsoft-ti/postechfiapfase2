using Domain.ValuesObjects;

namespace Domain.Entities;

public class GalleryGame
{
    public Guid Id { get; private set; }
    public Guid GameId { get; private set; }
    public Game Game { get; private set; } = null!;
    public decimal Price { get; private set; }
    public Promotion Promotion { get; private set; } = Promotion.None;

    protected GalleryGame() { }

    public GalleryGame(Game game, decimal price)
    {
        if (price < 0)
            throw new ArgumentException("Preço inválido");

        Id = Guid.NewGuid();
        GameId = game.Id;
        Game = game;
        Price = price;
    }

    public decimal FinalPrice => Math.Max(0, Promotion.ApplyDiscount(Price));

    public void ApplyPromotion(Promotion promotion)
    {
        Promotion = promotion ?? Promotion.None;
    }

    public string GetDescription()
        => $"{Game.GetDescription()} - {FinalPrice:C}";
}
