using Domain.Entities;

namespace Domain.Service;


public class PurchaseDomainService : IPurchaseDomainService
{
    public Purchase RegisterPurchase(Player player, GalleryGame game)
    {
        if (player == null) throw new ArgumentException("Jogador inválido");
        if (game == null) throw new ArgumentException("Jogo inválido");

        // Regra: adiciona jogo à biblioteca do jogador
        player.AddGame(game);

        // Cria a compra
        return new Purchase(player.Id, game.Id);
    }
}
